using HarmonyLib;
using System.Collections;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Millenium")]
    public class Millenium : ItemBase<Millenium>
    {
        public override string ItemName => "Millenium";

        public override string ItemLangTokenName => "MILLENIUM";

        public override string ItemPickupDesc => "Create a tidal cataclysm on hit that grounds and collapses enemies. $svCorrupts all Sun Fragments$se.".AutoFormat();

        public override string ItemFullDescription => ("$su" + chance + "%$se chance on hit to create a $sdtidal cataclysm$se in a $su" + baseExplosionRadius + "m$se $ss(+" + stackExplosionRadius + "m per stack)$se area, $sdcollapsing$se and $sugrounding$se enemies for $sd400%$se base damage. $svCorrupts all Sun Fragments$se.").AutoFormat();

        public override string ItemLore => "This voice.\r\nI hear ringing..\r\nIt asks . . .\r\nIt invades my mind.\r\nMy hearing has become reversed..\r\nI'm falling\r\n\r\n[...]\r\n\r\nThis maze, reversed..\r\nCircled by a close fog\r\n\r\n[...]\r\n\r\nI've lost.\r\n[...]\r\n\r\n[...]\r\nVast. Infinite. Purgatory.\r\n\r\nMy voice, audible by all.\r\nUnprocessed, yet, incomprehensible.";

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Explosion Radius", "", 12f)]
        public static float baseExplosionRadius;

        [ConfigField("Stack Explosion Radius", "", 4f)]
        public static float stackExplosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.33f)]
        public static float explosionProcCoefficient;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("MilleniumHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMillenium.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            var powerElixirGlassMat = new Material(Utils.Assets.Material.matHealingPotionGlass);
            powerElixirGlassMat.SetFloat("_Boost", 0.3f);
            powerElixirGlassMat.SetFloat("_RimPower", 1.567551f);
            powerElixirGlassMat.SetFloat("_RimStrength", 3.52f);
            powerElixirGlassMat.SetFloat("_AlphaBoost", 1f);
            powerElixirGlassMat.SetFloat("IntersectionStrength", 1f);
            powerElixirGlassMat.SetColor("_TintColor", new Color32(255, 44, 115, 255));

            var millenium = Main.hifuSandswept.LoadAsset<GameObject>("MilleniumHolder.prefab");
            var model = millenium.transform.GetChild(0);
            var hourglassMr = model.Find("Vert").GetComponent<MeshRenderer>();
            hourglassMr.material = powerElixirGlassMat;

            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.NullifierExplosion, "Millenium VFX", false);

            var trans = vfx.transform;

            var newStarMat = new Material(Paths.Material.matNullifierStarParticle);
            newStarMat.SetColor("_TintColor", new Color32(15, 49, 44, 255));

            var sphere = trans.Find("Sphere");
            var sphereMeshRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one;

            var explosionSphere = trans.Find("ExplosionSphere").GetComponent<ParticleSystemRenderer>();

            var newExplosionMat = new Material(Addressables.LoadAssetAsync<Material>("RoR2/Base/Nullifier/matNullifierExplosionAreaIndicatorSoft.mat").WaitForCompletion());
            newExplosionMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampArcaneCircle.png").WaitForCompletion());
            newExplosionMat.SetColor("_TintColor", new Color32(72, 0, 255, 255));
            newExplosionMat.SetFloat("_RimPower", 1.569113f);
            newExplosionMat.SetFloat("_RimStrength", 2.149275f);
            newExplosionMat.SetFloat("_IntersectionStrength", 20f);

            explosionSphere.material = newExplosionMat;

            var vacuumRadial = trans.Find("Vacuum Radial").GetComponent<ParticleSystemRenderer>();

            var newVacuumMat = new Material(Paths.Material.matNullifierStarPortalEdge);
            newVacuumMat.SetColor("_TintColor", new Color32(3, 4, 255, 246));
            newVacuumMat.SetFloat("_Boost", 13f);
            newVacuumMat.SetFloat("_AlphaBias", 0.8564593f);
            newVacuumMat.SetTexture("_RemapTex", Paths.Texture2D.texRampCrosshair2);

            vacuumRadial.material = newVacuumMat;

            var newSphereMaterials = new Material[2];

            var newPortalMat = new Material(Paths.Material.matNullifierGemPortal);
            newPortalMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium2.png"));
            newPortalMat.SetTexture("_EmissionTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium1.png"));

            sphereMeshRenderer.materials[0] = newPortalMat;

            var newSphereMat = new Material(Paths.Material.matGravsphereCore);
            newSphereMat.SetColor("_TintColor", new Color32(5, 0, 255, 255));
            newSphereMat.SetFloat("_InvFade", 0f);
            newSphereMat.SetFloat("_Boost", 1.090909f);
            newSphereMat.SetFloat("_AlphaBoost", 2.18f);
            newSphereMat.SetFloat("_AlphaBias", 0f);
            newSphereMat.SetTexture("_RemapTex", Paths.Texture2D.texRampHuntressAltColossusArrow);

            newSphereMaterials[0] = newPortalMat;
            newSphereMaterials[1] = newSphereMat;

            sphereMeshRenderer.materials = newSphereMaterials;

            ContentAddition.AddEffect(vfx);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            if (report.damageInfo.procChainMask.HasProc(ProcType.AACannon))
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack <= 0)
            {
                return;
            }

            if (!Util.CheckRoll(chance * report.damageInfo.procCoefficient, attackerBody.master))
            {
                return;
            }

            report.damageInfo.procChainMask.AddProc(ProcType.AACannon);

            var radius = baseExplosionRadius + stackExplosionRadius * (stack - 1);

            EffectData effectData = new()
            {
                origin = victimBody.corePosition,
                rotation = Quaternion.identity,
                scale = radius * 1.3f
            };
            EffectManager.SpawnEffect(vfx, effectData, true);

            // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

            float mass;

            if (victimBody.characterMotor) mass = victimBody.characterMotor.mass;
            else if (victimBody.rigidbody) mass = victimBody.rigidbody.mass;
            else mass = 1f;

            BlastAttack blastAttack = new()
            {
                radius = radius,
                baseDamage = Mathf.Epsilon, // dont ask
                procCoefficient = explosionProcCoefficient,
                crit = report.damageInfo.crit,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = attackerBody.gameObject,
                teamIndex = attackerBody.teamComponent.teamIndex,
                position = report.damageInfo.position,
                damageType = DamageType.Silent,
                bonusForce = new Vector3(0f, -25f * mass, 0f)
            };

            var result = blastAttack.Fire();

            var collapse = DotController.GetDotDef(DotController.DotIndex.Fracture);

            for (int i = 0; i < result.hitPoints.Length; i++)
            {
                var hitPoint = result.hitPoints[i];

                var hurtBox = hitPoint.hurtBox;
                if (!hurtBox)
                {
                    continue;
                }

                var hc = hurtBox.healthComponent;
                if (!hc)
                {
                    continue;
                }

                DotController.InflictDot(hitPoint.hurtBox.healthComponent.gameObject, attackerBody.gameObject, DotController.DotIndex.Fracture, collapse.interval, 1f, uint.MaxValue);
            }

            Util.PlaySound("Play_item_void_clover", victimBody.gameObject);
            Util.PlaySound("Play_voidDevastator_death_VO", victimBody.gameObject);
            Util.PlaySound("Play_voidDevastator_death_VO", victimBody.gameObject);
            Util.PlaySound("Play_clayboss_m2_explo", victimBody.gameObject);
            Util.PlaySound("Play_engi_M2_explo", victimBody.gameObject);
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = instance.ItemDef,
                itemDef1 = Greens.SunFragment.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}