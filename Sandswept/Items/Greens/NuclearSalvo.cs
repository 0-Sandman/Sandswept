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

        public override string ItemFullDescription => ("Every $sd" + interval + " seconds$se, all mechanical allies fire $sd" + baseMissileCount + "$se $ss(+" + stackMissileCount + " per stack)$se $sdnuclear missiles$se that deal $sd" + d(missileDamage) + "$se base damage each and $sdignite$se on hit.").AutoFormat();

        public override string ItemLore => "<style=cMono>//--AUTO-TRANSCRIPTION FROM LOADING BAY 4 OF THE UES [Redacted] --//\r\n\r\n\"</style>That's everything, right?\"\r\n\r\n\"Not quite. We're supposed to load those mean-looking missile salvos over there, too.\"\r\n\r\n\"Wait, what? Why would a shipping vessel like the Contact Light need these?\"\r\n\r\n\"I have no idea, but I've gotten word from high up that they need to be on board, in shipping chests. And when I say high up, I mean REALLY high up.\"\r\n\r\n\"There's so many. I have a bad feeling about this...especially after everything else going on with this shipment.\"\r\n\r\n\"Like it or not, it's part of the job. The suits at the top of UES are always pulling secret stunts like this. If we don't follow orders, we're outta here.\"";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texNuclearSalvo.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.BrotherBlacklist, ItemTag.AIBlacklist };

        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

        [ConfigField("Interval", "", 5f)]
        public static float interval;

        [ConfigField("Base Missile Count", "", 2)]
        public static int baseMissileCount;

        [ConfigField("Stack Missile Count", "", 2)]
        public static int stackMissileCount;

        [ConfigField("Missile Damage", "Decimal.", 1f)]
        public static float missileDamage;

        [ConfigField("Missile Proc Coefficient", "", 0.33f)]
        public static float missileProcCoefficient;

        [ConfigField("Missile AoE", "", 9f)]
        public static float missileAoE;

        public static GameObject missilePrefab;

        // for salvo display you can instantiate Main.hifuSandswept.LoadAsset<GameObject>("NuclearSalvoHolder.prefab");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            base.Hooks();

            SalvoPrefab = Main.Assets.LoadAsset<GameObject>("SalvoBehaviour.prefab");
            // Main.ModLogger.LogError(SalvoPrefab);
            /*
            SalvoMissile = Main.Assets.LoadAsset<GameObject>("Missile.prefab");
            ContentAddition.AddProjectile(SalvoMissile);
            */

            missilePrefab = PrefabAPI.InstantiateClone(Paths.GameObject.MissileProjectile, "Nuclear Salvo Missile");

            var missileProjectileController = missilePrefab.GetComponent<ProjectileController>();
            missileProjectileController.procCoefficient = missileProcCoefficient;

            // var projectileSingleTargetImpact = missilePrefab.GetComponent<ProjectileSingleTargetImpact>();
            missilePrefab.RemoveComponent<ProjectileSingleTargetImpact>();

            var projectileImpactExplosion = missilePrefab.AddComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastProcCoefficient = missileProcCoefficient;
            projectileImpactExplosion.blastAttackerFiltering = AttackerFiltering.NeverHitSelf;
            projectileImpactExplosion.blastDamageCoefficient = missileDamage;
            projectileImpactExplosion.blastRadius = missileAoE;
            projectileImpactExplosion.destroyOnEnemy = true;
            projectileImpactExplosion.destroyOnWorld = true;
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.lifetime = 30f;
            projectileImpactExplosion.impactOnWorld = true;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.applyDot = false;

            var newImpact = PrefabAPI.InstantiateClone(Paths.GameObject.ImpVoidspikeExplosion, "Nuclear Salvo Explosion", false);
            var effectComponent = newImpact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_item_proc_missile_explo";

            var particles = newImpact.transform.GetChild(0);

            for (int i = 0; i < particles.childCount; i++)
            {
                var child = particles.transform.GetChild(i);
                child.localScale = Vector3.one * 1.5f;
            }

            var swipe = particles.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matImpSwipe);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampArchWisp);
            newMat.SetFloat("_Boost", 9f);
            newMat.SetFloat("_AlphaBoost", 1.44f);
            newMat.SetFloat("_AlphaBias", 0.6f);
            newMat.SetColor("_TintColor", new Color32(1, 13, 0, 255));

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
            light.range = missileAoE;
            light.intensity = 60f;
            var lightIntensityCurve = light.GetComponent<LightIntensityCurve>();
            lightIntensityCurve.timeMax = 0.6f;

            var dash = particles.transform.GetChild(5).GetComponent<ParticleSystemRenderer>();
            dash.material = newMat;

            ContentAddition.AddEffect(newImpact);

            // projectileSingleTargetImpact.impactEffect = newImpact;
            projectileImpactExplosion.impactEffect = newImpact;

            var ghost = PrefabAPI.InstantiateClone(Paths.GameObject.MissileGhost, "Nuclear Salvo Missile Ghost", false);
            ghost.transform.localScale = Vector3.one * 2.5f;
            ghost.transform.GetChild(1).gameObject.SetActive(false);

            var pointLight = ghost.transform.GetChild(3).GetComponent<Light>();
            pointLight.color = new Color32(0, 255, 20, 255);
            pointLight.intensity = 1000f;

            var trail = ghost.transform.GetChild(0).GetComponent<TrailRenderer>();
            trail.widthMultiplier = 1f;
            trail.time = 1f;

            var newTrailMat = Object.Instantiate(Paths.Material.matMissileTrail);
            newTrailMat.SetColor("_TintColor", new Color32(20, 255, 0, 255));
            newTrailMat.SetFloat("_AlphaBoost", 0.66f);
            newTrailMat.SetTexture("_RemapTex", Paths.Texture2D.texRampBeetleQueen);

            trail.material = newTrailMat;

            var flare = ghost.transform.GetChild(1);
            flare.gameObject.SetActive(false);

            var missileModel = ghost.transform.GetChild(2);
            missileModel.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
            var meshRenderer = missileModel.GetComponent<MeshRenderer>();

            var atgMat = Object.Instantiate(Paths.Material.matMissile);
            // atgMat.SetColor("_Color", new Color32(224, 94, 94, 255));
            atgMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texNuclearSalvoMissile.png"));
            atgMat.EnableKeyword("DITHER");
            atgMat.EnableKeyword("FADECLOSE");
            meshRenderer.sharedMaterial = atgMat;

            missileProjectileController.ghostPrefab = ghost;

            var missileController = missilePrefab.GetComponent<MissileController>();
            missileController.maxSeekDistance = 60f;
            missileController.turbulence = 0f;
            missileController.deathTimer = 30f;
            missileController.giveupTimer = 30f;
            missileController.delayTimer = 0f;
            missileController.maxVelocity = 40f;
            missileController.acceleration = 1.5f;

            PrefabAPI.RegisterNetworkPrefab(missilePrefab);
            ContentAddition.AddProjectile(missilePrefab);

            ContentAddition.AddNetworkedObject(SalvoPrefab);

            // On.RoR2.CharacterBody.RecalculateStats += GiveItem;
            // On.RoR2.CharacterBody.OnInventoryChanged += RecheckItems;
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
            // Main.ModLogger.LogError("oninventorychagned: giving itembehavior");
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
            body.onInventoryChanged += Body_onInventoryChanged;
        }

        public void OnEnable()
        {
            if (NetworkServer.active)
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
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                var hurtBoxes = sphereSearch.GetHurtBoxes();
                for (int i = 0; i < hurtBoxes.Length; i++)
                {
                    var hurtBox = hurtBoxes[i];

                    var hc = hurtBox.healthComponent;
                    if (!hc)
                    {
                        continue;
                    }

                    var enemyBody = hc.body;
                    if (!enemyBody)
                    {
                        continue;
                    }

                    if (!enemyBody.teamComponent)
                    {
                        continue;
                    }

                    if (enemyBody.teamComponent.teamIndex == TeamIndex.Player)
                    {
                        continue;
                    }

                    shouldFire = true;
                }

                enemyCheckTimer = 0f;
            }

            if (stopwatch <= 0 && body && shouldFire)
            {
                stopwatch = totalMissileDelay;

                StartCoroutine(FireMissiles(stack));
            }
        }

        public IEnumerator FireMissiles(int stack)
        {
            // Main.ModLogger.LogError("salvo fire missiles ran");
            var count = NuclearSalvo.baseMissileCount + NuclearSalvo.stackMissileCount * (stack - 1);
            for (int i = 0; i < count; i++)
            {
                // Debug.Log("firing salvo missile");
                FireProjectileInfo info = new()
                {
                    crit = false,
                    damage = body.damage * NuclearSalvo.missileDamage,
                    rotation = Quaternion.identity,/*Util.QuaternionSafeLookRotation(Util.ApplySpread(attachment.attachedBody.inputBank.aimDirection, -10f, 10f, 1f, 1f)),*/
                    position = body.transform.position + new Vector3(0f, 2f, 0f),
                    owner = body.gameObject,
                    projectilePrefab = NuclearSalvo.missilePrefab,
                    damageTypeOverride = DamageType.IgniteOnHit
                };

                // Debug.Log(attachment.attachedBody);
                if (Util.HasEffectiveAuthority(gameObject))
                    ProjectileManager.instance.FireProjectile(info);

                yield return new WaitForSeconds(1f / count);
            }
            yield return null;
        }
    }
}