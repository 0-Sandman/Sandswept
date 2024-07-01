using HarmonyLib;
using System.Collections;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Millenium")]
    public class Millenium : ItemBase<Millenium>
    {
        public override string ItemName => "Millenium";

        public override string ItemLangTokenName => "MILLENIUM";

        public override string ItemPickupDesc => "Create a tidal cataclysm on hit that grounds and collapses enemies. $svCorrupts all Sun Fragments$se.";

        public override string ItemFullDescription => ("$su" + chance + "%$se chance on hit to create a $sdtidal cataclysm$se in a $su" + baseExplosionRadius + "m$se $ss(+" + stackExplosionRadius + "m per stack)$se area, $sdcollapsing$se and $sugrounding$se enemies for $sd400%$se base damage. $svCorrupts all Sun Fragments$se.").AutoFormat();

        public override string ItemLore => "This voice.\r\nI hear ringing..\r\nIt asks.\r\nIt invades my mind.\r\nMy hearing, reversed..\r\nI'm falling\r\n\r\n[...]\r\n\r\n\r\n\r\nThis maze, reversed..\r\nCircled by a close fog\r\n\r\n[...]\r\n\r\n\r\n\r\nI've lost.";

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Explosion Radius", "", 12f)]
        public static float baseExplosionRadius;

        [ConfigField("Stack Explosion Radius", "", 4f)]
        public static float stackExplosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.33f)]
        public static float explosionProcCoefficient;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static ProcType milleniumAoE = (ProcType)3298571264;

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static DamageAPI.ModdedDamageType milleniumDamageType;

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            milleniumDamageType = DamageAPI.ReserveDamageType();

            vfx = PrefabAPI.InstantiateClone(Assets.GameObject.NullifierExplosion, "Millenium VFX", false);

            var trans = vfx.transform;

            var newStarMat = Object.Instantiate(Assets.Material.matNullifierStarParticle);
            newStarMat.SetColor("_TintColor", new Color32(15, 49, 44, 255));

            var sphere = trans.Find("Sphere");
            var sphereMeshRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one;

            var explosionSphere = trans.Find("ExplosionSphere").GetComponent<ParticleSystemRenderer>();

            var newExplosionMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Nullifier/matNullifierExplosionAreaIndicatorSoft.mat").WaitForCompletion());
            newExplosionMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampArcaneCircle.png").WaitForCompletion());
            newExplosionMat.SetColor("_TintColor", new Color32(72, 0, 255, 255));
            newExplosionMat.SetFloat("_RimPower", 1.569113f);
            newExplosionMat.SetFloat("_RimStrength", 2.149275f);
            newExplosionMat.SetFloat("_IntersectionStrength", 20f);

            explosionSphere.material = newExplosionMat;

            var vacuumRadial = trans.Find("Vacuum Radial").GetComponent<ParticleSystemRenderer>();

            var newVacuumMat = Object.Instantiate(Assets.Material.matNullifierStarPortalEdge);
            newVacuumMat.SetColor("_TintColor", new Color32(3, 4, 255, 246));
            newVacuumMat.SetFloat("_Boost", 13f);
            newVacuumMat.SetFloat("_AlphaBias", 0.9590086f);
            newVacuumMat.SetTexture("_RemapTex", Assets.Texture2D.texRampCrosshair2);

            vacuumRadial.material = newVacuumMat;

            var newPortalMat = Object.Instantiate(Assets.Material.matNullifierGemPortal);
            newPortalMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium2.png"));
            newPortalMat.SetTexture("_EmissionTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium1.png"));

            sphereMeshRenderer.materials[0] = newPortalMat;

            var newSphereMat = Object.Instantiate(Assets.Material.matGravsphereCore);
            newSphereMat.SetColor("_TintColor", new Color32(5, 0, 255, 255));
            newSphereMat.SetFloat("_InvFade", 0f);
            newSphereMat.SetFloat("_Boost", 20f);
            newSphereMat.SetFloat("_AlphaBoost", 0.148027f);
            newSphereMat.SetFloat("_AlphaBias", 1f);

            sphereMeshRenderer.materials[1] = newSphereMat;

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

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef1 = instance.ItemDef,
                itemDef2 = Greens.SunFragment.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            if (damageInfo.procChainMask.HasProc(milleniumAoE))
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0)
            {
                if (Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
                {
                    var radius = baseExplosionRadius + stackExplosionRadius * (stack - 1);

                    EffectData effectData = new()
                    {
                        origin = victimBody.corePosition,
                        rotation = Quaternion.identity,
                        scale = radius * 3f
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
                        crit = damageInfo.crit,
                        damageColorIndex = milleniumColor,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = attackerBody.gameObject,
                        teamIndex = attackerBody.teamComponent.teamIndex,
                        position = damageInfo.position,
                        damageType = DamageType.Silent | DamageType.BypassArmor | DamageType.BypassBlock, // I said dont ask
                        bonusForce = new Vector3(0f, -25f * mass, 0f)
                    };

                    var result = blastAttack.Fire();

                    damageInfo.procChainMask.AddProc(milleniumAoE);

                    attackerBody.StartCoroutine(AddCollapse(damageInfo, result));

                    AkSoundEngine.PostEvent(Events.Play_voidRaid_snipe_impact, victimBody.gameObject);
                }
            }
        }

        public IEnumerator AddCollapse(DamageInfo damageInfo, BlastAttack.Result result)
        {
            foreach (BlastAttack.HitPoint hitPoint in result.hitPoints)
            {
                if (hitPoint.hurtBox && hitPoint.hurtBox.healthComponent)
                {
                    yield return new WaitForSeconds(1.5f / result.hitCount);
                    var collapse = DotController.GetDotDef(DotController.DotIndex.Fracture);
                    DotController.InflictDot(hitPoint.hurtBox.healthComponent.gameObject, damageInfo.attacker, DotController.DotIndex.Fracture, collapse.interval, 1f);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}