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

        public override string ItemFullDescription => ("Every $sd" + baseInterval + " seconds$se $ss(-" + d(stackIntervalReduction) + " per stack)$se, all mechanical allies fire $sdnuclear missiles$se that deal $sd" + missileCount + "x" + d(missileDamage) + "$se base damage and $sdignite$se on hit.").AutoFormat();

        public override string ItemLore => "//--AUTO-TRANSCRIPTION FROM LOADING BAY 4 OF THE UES [Redacted] --//\r\n\r\n\"That's everything, right?\"\r\n\r\n\"Not quite. We're supposed to load those mean-looking missile salvos over there, too.\"\r\n\r\n\"Wait, what? What could they possibly be needed for?\"\r\n\r\n\"I have no idea, but I've gotten word from high up that they need to be on board, and put in shipping chests. And when I say high up, I mean REALLY high up.\"\r\n\r\n\"There's so many, too. I have a bad feeling about this. The whole shipment has been fishy.\"\r\n\r\n\"The suits at the top of UES are always pulling secret stunts like this. It's just part of the job. We've got to follow orders, or we're outta here.\"";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texNuclearSalvo.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.BrotherBlacklist, ItemTag.AIBlacklist };

        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

        [ConfigField("Base Interval", "", 5f)]
        public static float baseInterval;

        [ConfigField("Stack Interval Reduction", "Decimal.", 0.25f)]
        public static float stackIntervalReduction;

        [ConfigField("Missile Count", "", 2)]
        public static int missileCount;

        [ConfigField("Missile Damage", "Decimal.", 1f)]
        public static float missileDamage;

        [ConfigField("Missile Proc Coefficient", "", 0.33f)]
        public static float missileProcCoefficient;

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
            Main.ModLogger.LogError(SalvoPrefab);
            /*
            SalvoMissile = Main.Assets.LoadAsset<GameObject>("Missile.prefab");
            ContentAddition.AddProjectile(SalvoMissile);
            */

            missilePrefab = PrefabAPI.InstantiateClone(Assets.GameObject.MissileProjectile, "Nuclear Salvo Missile");

            var missileProjectileController = missilePrefab.GetComponent<ProjectileController>();
            missileProjectileController.procCoefficient = missileProcCoefficient;

            var projectileSingleTargetImpact = missilePrefab.GetComponent<ProjectileSingleTargetImpact>();

            var newImpact = PrefabAPI.InstantiateClone(Assets.GameObject.MissileExplosionVFX, "Nuclear Salvo Explosion", false);

            var particles = newImpact.transform.GetChild(0);

            for (int i = 0; i < particles.childCount; i++)
            {
                var child = particles.transform.GetChild(i);
                child.localScale = Vector3.one * 4f;
            }

            var flames = particles.transform.GetChild(1);
            var flamesColor = flames.GetComponent<ParticleSystem>().colorOverLifetime;

            var gradient = new Gradient();

            var colors = new GradientColorKey[3];
            colors[0] = new GradientColorKey(Color.white, 0f);
            colors[1] = new GradientColorKey(new Color32(87, 255, 77, 255), 0.132f);
            colors[2] = new GradientColorKey(new Color32(177, 193, 0, 255), 0.362f);

            var alphas = new GradientAlphaKey[3];
            alphas[0] = new GradientAlphaKey(0f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 0.074f);
            alphas[2] = new GradientAlphaKey(0f, 0f);

            gradient.SetKeys(colors, alphas);

            flamesColor.color = gradient;

            var flamesPSR = flames.GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Assets.Material.matGenericFire);
            newMat.SetColor("_TintColor", new Color32(236, 255, 105, 255));

            flamesPSR.material = newMat;

            var flash = particles.transform.GetChild(2);
            var flashColor = flash.GetComponent<ParticleSystem>().colorOverLifetime;

            var gradient2 = new Gradient();

            var colors2 = new GradientColorKey[2];
            colors[0] = new GradientColorKey(new Color32(215, 255, 214, 255), 0f);
            colors[1] = new GradientColorKey(new Color32(69, 161, 0, 255), 1f);

            var alphas2 = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(0.25490196078f, 0.238f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            gradient2.SetKeys(colors2, alphas2);

            flashColor.color = gradient2;

            ContentAddition.AddEffect(newImpact);

            projectileSingleTargetImpact.impactEffect = newImpact;

            var ghost = PrefabAPI.InstantiateClone(Assets.GameObject.MissileGhost, "Nuclear Salvo Missile Ghost", false);
            ghost.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
            ghost.transform.GetChild(1).gameObject.SetActive(false);

            var pointLight = ghost.transform.GetChild(3).GetComponent<Light>();
            pointLight.color = new Color32(118, 255, 25, 255);

            var trail = ghost.transform.GetChild(0).GetComponent<TrailRenderer>();

            var newTrailMat = Object.Instantiate(Assets.Material.matMissileTrail);
            newTrailMat.SetColor("_TintColor", Color.black);
            newTrailMat.SetFloat("_AlphaBoost", 0.66f);

            trail.material = newTrailMat;

            var missileModel = ghost.transform.GetChild(2);
            missileModel.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
            var meshRenderer = missileModel.GetComponent<MeshRenderer>();

            var atgMat = Object.Instantiate(Assets.Material.matMissile);
            // atgMat.SetColor("_Color", new Color32(224, 94, 94, 255));
            atgMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texNuclearSalvoMissile.png"));
            atgMat.EnableKeyword("DITHER");
            atgMat.EnableKeyword("FADECLOSE");
            meshRenderer.sharedMaterial = atgMat;

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
                    cm.GetBody().RemoveComponent<SalvoBehaviour>();
                }
            }
        }
    }

    public class SalvoBehaviour : MonoBehaviour
    {
        public CharacterBody body;
        public float totalMissileDelay => NuclearSalvo.baseInterval * Mathf.Pow(1f - NuclearSalvo.stackIntervalReduction, body.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) - 1);
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

                StartCoroutine(FireMissiles());

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

        public IEnumerator FireMissiles()
        {
            // Main.ModLogger.LogError("salvo fire missiles ran");
            for (int i = 0; i < NuclearSalvo.missileCount; i++)
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

                yield return new WaitForSeconds(0.25f);
            }
            yield return null;
        }
    }
}