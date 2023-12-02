using System;
using System.Collections;
using System.Linq;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Nuclear Salvo")]
    public class NuclearSalvo : ItemBase<NuclearSalvo>
    {
        public override string ItemName => "Nuclear Salvo";

        public override string ItemLangTokenName => "NUCLEAR_SALVO";

        public override string ItemPickupDesc => "Mechanical allies fire nuclear warheads periodically.";

        public override string ItemFullDescription => ("Every $sd" + interval + " seconds$se, all mechanical allies fire $sd" + baseMissileCount + "$se $ss(+" + stackMissileCount + " per stack)$se $sdnuclear missiles$se that deal $sd" + d(missileDamage) + "$se base damage each and $sdignite$se on hit.").AutoFormat();

        public override string ItemLore => "<style=cMono>//--AUTO-TRANSCRIPTION FROM LOADING BAY 4 OF THE UES [Redacted] --//</style>\r\n\r\n\"That's everything, right?\"\r\n\r\n\"Not quite. We're supposed to load those mean-looking missile salvos over there, too.\"\r\n\r\n\"Wait, what? Why would a shipping vessel like the Contact Light need these?\"\r\n\r\n\"I have no idea, but I've gotten word from high up that they need to be on board, and put in shipping chests. And when I say high up, I mean REALLY high up.\"\r\n\r\n\"There's so many, too. I have a bad feeling about this. The whole shipment has been fishy.\"\r\n\r\n\"The suits at the top of UES are always pulling secret stunts like this.  It's just part of the job. We've got to follow orders, or we're outta here.\"";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texNuclearSalvo.png");

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

        // for salvo display you can instantiate Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvo.prefab");

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

            missilePrefab = PrefabAPI.InstantiateClone(Assets.GameObject.MissileProjectile, "Nuclear Salvo Missile");

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

            var newImpact = PrefabAPI.InstantiateClone(Assets.GameObject.ImpVoidspikeExplosion, "Nuclear Salvo Explosion", false);
            var effectComponent = newImpact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_item_proc_missile_explo";

            var particles = newImpact.transform.GetChild(0);

            for (int i = 0; i < particles.childCount; i++)
            {
                var child = particles.transform.GetChild(i);
                child.localScale = Vector3.one * 1.5f;
            }

            var swipe = particles.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Assets.Material.matImpSwipe);
            newMat.SetTexture("_RemapTex", Assets.Texture2D.texRampAntler);
            newMat.SetFloat("_Boost", 6.8f);
            newMat.SetFloat("_AlphaBoost", 1.44f);
            newMat.SetColor("_TintColor", new Color32(1, 13, 0, 255));

            var newMat2 = Object.Instantiate(Assets.Material.matImpSwipe);
            newMat2.SetTexture("_RemapTex", Assets.Texture2D.texRampBeetleBreath);
            newMat2.SetColor("_TintColor", new Color32(80, 255, 54, 255));

            swipe.material = newMat;

            var dashRings = particles.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
            dashRings.material = newMat;

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

            var ghost = PrefabAPI.InstantiateClone(Assets.GameObject.MissileGhost, "Nuclear Salvo Missile Ghost", false);
            ghost.transform.localScale = Vector3.one * 2.5f;
            ghost.transform.GetChild(1).gameObject.SetActive(false);

            var pointLight = ghost.transform.GetChild(3).GetComponent<Light>();
            pointLight.color = new Color32(0, 255, 20, 255);
            pointLight.intensity = 1000f;

            var trail = ghost.transform.GetChild(0).GetComponent<TrailRenderer>();
            trail.widthMultiplier = 1f;
            trail.time = 1f;

            var newTrailMat = Object.Instantiate(Assets.Material.matMissileTrail);
            newTrailMat.SetColor("_TintColor", new Color32(20, 255, 0, 255));
            newTrailMat.SetFloat("_AlphaBoost", 0.66f);
            newTrailMat.SetTexture("_RemapTex", Assets.Texture2D.texRampBeetleQueen);

            trail.material = newTrailMat;

            var flare = ghost.transform.GetChild(1);
            flare.gameObject.SetActive(false);

            var missileModel = ghost.transform.GetChild(2);
            missileModel.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
            var meshRenderer = missileModel.GetComponent<MeshRenderer>();

            var atgMat = Object.Instantiate(Assets.Material.matMissile);
            // atgMat.SetColor("_Color", new Color32(224, 94, 94, 255));
            atgMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texNuclearSalvoMissile.png"));
            atgMat.EnableKeyword("DITHER");
            atgMat.EnableKeyword("FADECLOSE");
            meshRenderer.sharedMaterial = atgMat;

            missileProjectileController.ghostPrefab = ghost;

            var missileController = missilePrefab.GetComponent<MissileController>();
            missileController.maxSeekDistance = 10000f;
            missileController.turbulence = 0f;
            missileController.deathTimer = 30f;
            missileController.giveupTimer = 30f;
            missileController.delayTimer = 0f;
            missileController.maxVelocity = 40f;
            missileController.acceleration = 1.5f;

            PrefabAPI.RegisterNetworkPrefab(missilePrefab);
            ContentAddition.AddProjectile(missilePrefab);

            ContentAddition.AddNetworkedObject(SalvoPrefab);

            On.RoR2.CharacterBody.RecalculateStats += GiveItem;
            On.RoR2.CharacterBody.OnInventoryChanged += RecheckItems;
        }

        public void GiveItem(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && self.isPlayerControlled && self.inventory.GetItemCount(ItemDef) > 0)
            {
                // Main.ModLogger.LogError("has salvo");
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master).ToList();

                foreach (CharacterMaster cm in masters)
                {
                    // Main.ModLogger.LogError("iterating through all masters where owner is me >w< :fuwwy: OwO UwU <3 <3 :3 :3");
                    if (cm.inventory.GetItemCount(ItemDef) < self.inventory.GetItemCount(ItemDef))
                    {
                        // Main.ModLogger.LogError("giving salvo to drone");
                        cm.inventory.ResetItem(ItemDef);
                        cm.inventory.GiveItem(ItemDef, self.inventory.GetItemCount(ItemDef));
                        cm.GetBody().AddComponent<SalvoBehaviour>();
                    }
                }
            }
        }

        public void RecheckItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active && self.isPlayerControlled && self.inventory.GetItemCount(ItemDef) == 0)
            {
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master).ToList();

                foreach (CharacterMaster cm in masters)
                {
                    // Main.ModLogger.LogError("removing salvo from drone");
                    cm.inventory.RemoveItem(ItemDef, cm.inventory.GetItemCount(ItemDef));
                    var body = cm.GetBody();
                    if (body)
                    {
                        body.RemoveComponent<SalvoBehaviour>();
                    }
                }
            }
        }
    }

    [DisallowMultipleComponent]
    public class SalvoBehaviour : MonoBehaviour
    {
        public CharacterBody body;
        public float totalMissileDelay = 5f;
        public float stopwatch = 0f;

        public void Start()
        {
            // Main.ModLogger.LogError("salvo start");
            body = GetComponent<CharacterBody>();
            stopwatch = totalMissileDelay;
        }

        public void FixedUpdate()
        {
            stopwatch -= Time.fixedDeltaTime;

            var stack = body.inventory.GetItemCount(NuclearSalvo.instance.ItemDef);

            if (stopwatch <= 0)
            {
                stopwatch = totalMissileDelay;

                if ((body.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                    StartCoroutine(FireMissiles(stack));

                if (stack <= 0)
                {
                    Destroy(this);
                }
            }

            if (stack <= 0)
            {
                Destroy(this);
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