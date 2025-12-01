using LookingGlass.ItemStatsNameSpace;
using MonoMod.Cil;
using RoR2.Orbs;
using System;
using System.Collections;
using System.Linq;
using static RoR2.MasterSummon;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Nuclear Salvo")]
    public class NuclearSalvo : ItemBase<NuclearSalvo>
    {
        public override string ItemName => "Nuclear Salvo";

        public override string ItemLangTokenName => "NUCLEAR_SALVO";

        public override string ItemPickupDesc => "Mechanical allies fire nuclear warheads periodically.";

        public override string ItemFullDescription => $"Every $sd{interval} seconds$se, all mechanical allies fire $sd{baseMissileCount}$se $ss(+{stackMissileCount} per stack)$se $sdnuclear missiles$se that deal $sd{missileDamage * 100f}%$se base damage each in a $sd{missileExplosionRadius}m$se radius.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>
        //--AUTO-TRANSCRIPTION FROM LOADING BAY 4 OF THE UES [Redacted] --//
        </style>
        "That's everything, right?"

        "Not quite. We're supposed to load those mean-looking missile salvos over there, too."

        "Wait, what? Why would a shipping vessel like the Contact Light need these?"

        "I have no idea, but I've gotten word from high up that they need to be on board, in shipping chests. And when I say high up, I mean REALLY high up."

        "There's so many. I have a bad feeling about this...especially after everything else going on with this shipment."

        "Like it or not, it's part of the job. The suits at the top of UES are always pulling secret stunts like this. If we don't follow orders, we're outta here."
        """;
        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texNuclearSalvo3.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.BrotherBlacklist, ItemTag.AIBlacklist, ItemTag.CanBeTemporary, ItemTag.Technology];

        public override float modelPanelParametersMinDistance => 7f;
        public override float modelPanelParametersMaxDistance => 15f;

        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

        [ConfigField("Interval", "", 5f)]
        public static float interval;

        [ConfigField("Base Missile Count", "", 2)]
        public static int baseMissileCount;

        [ConfigField("Stack Missile Count", "", 2)]
        public static int stackMissileCount;

        [ConfigField("Missile Damage", "Decimal.", 0.5f)]
        public static float missileDamage;

        [ConfigField("Missile Proc Coefficient", "", 0.33f)]
        public static float missileProcCoefficient;

        [ConfigField("Missile Explosion Radius", "", 16f)]
        public static float missileExplosionRadius;

        public static List<string> stageBlacklist = ["bazaar", "computationalexchange"];

        // uncomment for aoe

        public static GameObject orbEffect;

        // for salvo display you can instantiate Main.hifuSandswept.LoadAsset<GameObject>("NuclearSalvoHolder.prefab");

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Missile Count: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.descriptions.Add("Maximum Missile Count: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    baseMissileCount + stackMissileCount * (stack - 1),
                    60
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            SalvoPrefab = Main.assets.LoadAsset<GameObject>("SalvoBehaviour.prefab");
            ContentAddition.AddNetworkedObject(SalvoPrefab);

            orbEffect = PrefabAPI.InstantiateClone(Paths.GameObject.MicroMissileOrbEffect, "Nuclear Salvo Orb", false);
            var trans = orbEffect.transform;
            var ghost = trans.Find("MissileGhost");

            ghost.transform.localScale = Vector3.one * 3.5f;
            ghost.transform.GetChild(1).gameObject.SetActive(false);

            var pointLight = ghost.Find("Point Light").GetComponent<Light>();
            pointLight.color = new Color32(0, 255, 20, 255);
            pointLight.intensity = 1000f;

            var trail = ghost.Find("Trail").GetComponent<TrailRenderer>();
            trail.widthMultiplier = 1f;
            trail.time = 1f;

            var newTrailMat = Object.Instantiate(Paths.Material.matMissileTrail);
            newTrailMat.SetColor("_TintColor", new Color32(20, 255, 0, 255));
            newTrailMat.SetFloat("_AlphaBoost", 0.66f);
            newTrailMat.SetTexture("_RemapTex", Paths.Texture2D.texRampBeetleQueen);

            trail.material = newTrailMat;

            var flare = ghost.Find("Flare");
            flare.gameObject.SetActive(false);

            var missileModel = ghost.Find("missile VFX");
            missileModel.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
            var meshRenderer = missileModel.GetComponent<MeshRenderer>();

            var atgMat = Object.Instantiate(Paths.Material.matMissile);
            // atgMat.SetColor("_Color", new Color32(224, 94, 94, 255));
            atgMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texNuclearSalvoMissile.png"));
            atgMat.EnableKeyword("DITHER");
            atgMat.EnableKeyword("FADECLOSE");
            meshRenderer.sharedMaterial = atgMat;

            ContentAddition.AddEffect(orbEffect);

            var newImpact = PrefabAPI.InstantiateClone(Paths.GameObject.ImpVoidspikeExplosion, "Nuclear Salvo Orb Explosion", false);
            var effectComponent = newImpact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_item_proc_missile_explo";

            var particles = newImpact.transform.GetChild(0);

            for (int i = 0; i < particles.childCount; i++)
            {
                var child = particles.transform.GetChild(i);
                child.localScale = Vector3.one * 0.13f * missileExplosionRadius;
            }

            var swipe = particles.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matImpSwipe);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampArchWisp);
            newMat.SetFloat("_Boost", 9f);
            newMat.SetFloat("_AlphaBoost", 1.44f);
            newMat.SetFloat("_AlphaBias", 0.3457627f);
            newMat.SetColor("_TintColor", new Color32(1, 13, 0, 255));

            VFXUtils.MultiplyDuration(swipe.gameObject, 1.5f);

            var newMat2 = Object.Instantiate(Paths.Material.matImpSwipe);
            newMat2.SetTexture("_RemapTex", Paths.Texture2D.texRampBeetleBreath);
            newMat2.SetColor("_TintColor", new Color32(80, 255, 54, 255));

            swipe.material = newMat;

            var dashRings = particles.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
            dashRings.material = newMat;
            var dashringsPS = dashRings.GetComponent<ParticleSystem>().emission;
            var burst = dashringsPS.GetBurst(0);
            var burstCount = burst.count;
            burstCount.constant = 6;

            var flashRed = particles.transform.GetChild(3).GetComponent<ParticleSystem>().main.startColor;
            flashRed.color = new Color32(77, 255, 0, 255);

            var light = particles.transform.GetChild(4).GetComponent<Light>();
            light.color = new Color32(109, 255, 74, 255);
            light.range = missileExplosionRadius;
            light.intensity = 35f;
            var lightIntensityCurve = light.GetComponent<LightIntensityCurve>();
            lightIntensityCurve.timeMax = 0.6f;

            var dash = particles.transform.GetChild(5).GetComponent<ParticleSystemRenderer>();
            dash.material = newMat;

            ContentAddition.AddEffect(newImpact);

            orbEffect.GetComponent<OrbEffect>().endEffect = newImpact;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var itemDisplay = SetUpIDRS();

            ItemDisplayRuleDict i = new();

            #region Sandswept Survivors
            /*
            i.Add("RangerBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                    localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                    localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );
            */

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "MuzzleCannon",
                    localPos = new Vector3(-0.21825F, 0.01201F, -0.50626F),
                    localAngles = new Vector3(0.70196F, 352.6952F, 269.1494F),
                    localScale = new Vector3(0.20667F, 0.19298F, 0.19298F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;
        }

        public override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (!body.isPlayerControlled)
            {
                return;
            }

            var inventory = body.inventory;
            if (!inventory || body.GetComponent<SalvoPlayerController>())
            {
                return;
            }

            body.gameObject.AddComponent<SalvoPlayerController>();
        }
    }

    public class SalvoPlayerController : MonoBehaviour
    {
        public CharacterMaster master;
        public CharacterBody body;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            master = body.master;
            if (RoR2.Stage.instance && !NuclearSalvo.stageBlacklist.Contains(RoR2.Stage.instance.sceneDef.cachedName))
            {
                body.onInventoryChanged += Body_onInventoryChanged;
            }
        }

        public void OnEnable()
        {
            if (NetworkServer.active && RoR2.Stage.instance && !NuclearSalvo.stageBlacklist.Contains(RoR2.Stage.instance.sceneDef.cachedName))
            {
                // Main.ModLogger.LogError("subscribinbingign to master summon");
                onServerMasterSummonGlobal += MasterSummon_onServerMasterSummonGlobal;
            }
        }

        private void Body_onInventoryChanged()
        {
            var members = CharacterMaster.readOnlyInstancesList.Where(member => member.minionOwnership.ownerMaster == master).ToList();
            // Main.ModLogger.LogError("members is " + members);
            // Main.ModLogger.LogError("member count is " + members.Count);
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var npcMaster = member.GetComponent<CharacterMaster>();
                if (!npcMaster)
                {
                    // Main.ModLogger.LogError("couldnt get member master");
                    continue;
                }

                var npcInventory = npcMaster.inventory;
                if (!npcInventory)
                {
                    continue;
                }

                var npcBody = npcMaster.GetBody();
                if (!npcBody)
                {
                    // Main.ModLogger.LogError("couldnt get member body");
                    continue;
                }

                TryGiveItemInternal(npcMaster, npcBody, npcInventory);
            }
        }

        private void MasterSummon_onServerMasterSummonGlobal(MasterSummonReport masterSummonReport)
        {
            if (!master)
            {
                return;
            }

            if (master != masterSummonReport.leaderMasterInstance)
            {
                return;
            }

            TryGiveItem(masterSummonReport.summonMasterInstance);
        }

        public void TryGiveItem(CharacterMaster npcMaster)
        {
            if (!npcMaster)
            {
                return;
            }

            var npcInventory = npcMaster.inventory;
            if (!npcInventory)
            {
                return;
            }

            var npcBody = npcMaster.GetBody();

            TryGiveItemInternal(npcMaster, npcBody, npcInventory);
        }

        public void TryGiveItemInternal(CharacterMaster npcMaster, CharacterBody npcBody, Inventory npcInventory)
        {
            if (npcBody && (npcBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
            {
                // Main.ModLogger.LogError("member is mechanical");
                var salvo = npcMaster.GetComponent<SalvoBehaviour>();
                if (salvo == null)
                {
                    npcMaster.AddComponent<SalvoBehaviour>();
                }

                var playerItemCount = body.inventory.GetItemCount(NuclearSalvo.instance.ItemDef);
                var npcItemCount = npcInventory.GetItemCount(NuclearSalvo.instance.ItemDef);

                npcInventory.GiveItem(NuclearSalvo.instance.ItemDef, playerItemCount - npcItemCount);
            }
        }

        public void OnDisable()
        {
            if (NetworkServer.active)
            {
                onServerMasterSummonGlobal -= MasterSummon_onServerMasterSummonGlobal;
            }
            body.onInventoryChanged -= Body_onInventoryChanged;
        }
    }

    [DisallowMultipleComponent]
    public class SalvoBehaviour : MonoBehaviour
    {
        public CharacterBody body;
        public CharacterMaster master;
        public float totalMissileDelay = 5f;
        public float enemyCheckInterval = 0.25f;
        public float enemyCheckTimer = 0f;
        public float stopwatch = 0f;
        public bool shouldFire = false;

        public void Start()
        {
            // Main.ModLogger.LogError("salvo start");
            master = GetComponent<CharacterMaster>();
            body = master.GetBody();
            stopwatch = totalMissileDelay;
        }

        public void FixedUpdate()
        {
            if (!master || !master.inventory)
            {
                return;
            }

            stopwatch -= Time.fixedDeltaTime;
            enemyCheckTimer += Time.fixedDeltaTime;

            var stack = master.inventory.GetItemCount(NuclearSalvo.instance.ItemDef);

            if (stack <= 0)
            {
                Destroy(this);
            }

            if (enemyCheckTimer >= enemyCheckInterval)
            {
                shouldFire = false;

                if (!body)
                {
                    body = master.GetBody();
                    return;
                }

                var sphereSearch = new SphereSearch()
                {
                    mask = LayerIndex.entityPrecise.mask,
                    origin = body.corePosition,
                    radius = 75f,
                    queryTriggerInteraction = QueryTriggerInteraction.Ignore
                };

                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.AllExcept(TeamIndex.Player));
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();

                var hurtBoxes = sphereSearch.GetHurtBoxes();
                if (hurtBoxes.Length <= 0)
                {
                    enemyCheckTimer = 0f;
                    return;
                }
                var randomTarget = hurtBoxes[UnityEngine.Random.RandomRangeInt(0, hurtBoxes.Length)];

                var enemyHealthComponent = randomTarget.healthComponent;
                if (!enemyHealthComponent)
                {
                    return;
                }

                var enemyBody = enemyHealthComponent.body;
                if (!enemyBody)
                {
                    return;
                }

                if (stopwatch <= 0)
                {
                    stopwatch = totalMissileDelay;

                    StartCoroutine(FireMissiles(enemyBody, stack));
                }
                // adds up to +- interval random variance cause im lazy and sleepy

                enemyCheckTimer = 0f;
            }
        }

        public IEnumerator FireMissiles(CharacterBody enemyBody, int stack)
        {
            var missileCount = Mathf.Min(60, NuclearSalvo.baseMissileCount + NuclearSalvo.stackMissileCount * (stack - 1));
            // var missileCount = NuclearSalvo.baseMissileCount + NuclearSalvo.stackMissileCount * (stack - 1);
            for (int i = 0; i < missileCount; i++)
            {
                var nuclearSalvoOrb = new NuclearSalvoOrb
                {
                    origin = body.corePosition,
                    target = Util.FindBodyMainHurtBox(enemyBody),
                    attacker = gameObject,
                    isCrit = body.RollCrit(),
                    damageType = DamageType.IgniteOnHit,
                    damageValue = body.damage * NuclearSalvo.missileDamage,
                    // comment above and uncomment below, and the blastattack code for aoe
                    // damageValue = 0,
                    procChainMask = default,
                    speed = 45f,
                    teamIndex = TeamComponent.GetObjectTeam(body.gameObject),
                    procCoefficient = NuclearSalvo.missileProcCoefficient,
                    damageColorIndex = DamageColorIndex.Poison
                };

                if (Util.HasEffectiveAuthority(gameObject))
                {
                    OrbManager.instance.AddOrb(nuclearSalvoOrb);
                }

                // yield return new WaitForSeconds(Mathf.Min(1f / 60f, 1f / missileCount));
                yield return new WaitForSeconds(1f / missileCount);
            }
            yield return null;
        }
    }

    public class NuclearSalvoOrb : GenericDamageOrb
    {
        // uncomment these for aoe, too
        public CharacterMaster master;
        public CharacterBody body;
        public override void Begin()
        {
            base.Begin();
            // uncomment these for aoe, too
            master = attacker.GetComponent<CharacterMaster>();
            body = master.GetBody();
            speed = 45f;
            // duration = Mathf.Min(1f / 60f, duration);

            duration = Time.fixedDeltaTime + (distanceToTarget / speed);
            var effectData = new EffectData
            {
                scale = scale,
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(NuclearSalvo.orbEffect, effectData, true);

        }
        // code below mimicks the aoe behavior of the original, but is much laggier

        /*
        public override GameObject GetOrbEffect()
        {
            return NuclearSalvo.orbEffect;
        }
        */

        public override void OnArrival()
        {
            base.OnArrival();
            if (!body)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            BlastAttack blastAttack = new();

            blastAttack.baseDamage = body.damage * NuclearSalvo.missileDamage;
            blastAttack.inflictor = attacker;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            blastAttack.baseForce = 0f;
            blastAttack.bonusForce = Vector3.zero;
            blastAttack.damageColorIndex = damageColorIndex;
            blastAttack.damageType = DamageType.IgniteOnHit;
            blastAttack.crit = isCrit;
            blastAttack.procCoefficient = procCoefficient;
            blastAttack.teamIndex = teamIndex;
            blastAttack.radius = NuclearSalvo.missileExplosionRadius;
            blastAttack.procChainMask = default;
            blastAttack.position = target.transform.position;
            blastAttack.attacker = attacker;

            blastAttack.Fire();

        }

    }
}