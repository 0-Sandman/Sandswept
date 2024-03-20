using HarmonyLib;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Millenium")]
    public class Millenium : ItemBase<Millenium>
    {
        public override string ItemName => "Millenium";

        public override string ItemLangTokenName => "MILLENIUM";

        public override string ItemPickupDesc => "Create a tidal cataclysm on hit that grounds and collapses enemies. <style=cIsVoid>Corrupts all Sun Fragments</style>.";

        public override string ItemFullDescription => ("$su" + chance + "%$se chance on hit to create a $sdtidal cataclysm$se in a $su" + baseExplosionRadius + "m$se (+" + stackExplosionRadius + "m per stack) radius, $sdcollapsing$se and $sugrounding$se enemies for $sd400%$se base damage. <style=cIsVoid>Corrupts all Sun Fragments</style>.").AutoFormat();

        public override string ItemLore => "This voice.\r\nI hear ringing..\r\nIt asks.\r\nIt invades my mind.\r\nMy hearing, reversed..\r\nI'm falling\r\n\r\n[...]\r\n\r\nThis maze, reversed..\r\nCircled by a close fog\r\n\r\n[...]\r\n\r\nI've lost.";

        [ConfigField("Chance", "", 6f)]
        public static float chance;

        [ConfigField("Base Explosion Radius", "", 12f)]
        public static float baseExplosionRadius;

        [ConfigField("Stack Explosion Radius", "", 4f)]
        public static float stackExplosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.2f)]
        public static float explosionProcCoefficient;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static ProcType milleniumDoT = (ProcType)3298571298;
        public static ProcType milleniumAoE = (ProcType)3298571264;

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static DamageAPI.ModdedDamageType milleniumDamageType;

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            milleniumDamageType = DamageAPI.ReserveDamageType();

            vfx = PrefabAPI.InstantiateClone(Assets.GameObject.NullifierExplosion, "Millenium VFX", false);

            var trans = vfx.transform;

            var vacuumStars = trans.Find("Vacuum Stars");
            var vacuumStarsPSR = vacuumStars.GetComponent<ParticleSystemRenderer>();

            var newStarMat = Object.Instantiate(Assets.Material.matNullifierStarParticle);
            newStarMat.SetColor("_TintColor", new Color32(15, 49, 44, 255));

            var sphere = trans.Find("Sphere");
            var sphereMeshRenderer = sphere.GetComponent<MeshRenderer>();

            var newPortalMat = Object.Instantiate(Assets.Material.matNullifierGemPortal);
            newPortalMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium2.png"));
            newPortalMat.SetTexture("_EmissionTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium1.png"));

            sphereMeshRenderer.materials[0] = newPortalMat;

            ContentAddition.AddEffect(vfx);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
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

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attacker = damageInfo.attacker;
            if (!damageInfo.procChainMask.HasProc(milleniumDoT) && attacker && damageInfo.HasModdedDamageType(milleniumDamageType))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var inventory = attackerBody.inventory;
                    if (inventory)
                    {
                        var stack = inventory.GetItemCount(instance.ItemDef);
                        if (stack > 0)
                        {
                            DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                            DotController.InflictDot(self.gameObject, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f);

                            damageInfo.procChainMask.AddProc(milleniumDoT);
                        }
                    }
                }
            }
            orig(self, damageInfo);
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
                    /*
                    EffectData effectData = new()
                    {
                        origin = victimBody.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(damageInfo.force != Vector3.zero ? damageInfo.force : Random.onUnitSphere),
                        scale = baseExplosionRadius
                    };
                    EffectData effectData2 = new()
                    {
                        origin = victimBody.corePosition,
                        scale = baseExplosionRadius
                    };
                    EffectManager.SpawnEffect(milleniumVFX, effectData, true);
                    EffectManager.SpawnEffect(FragmentVFXSphere, effectData2, true);
                    */
                    // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

                    float mass;

                    if (victimBody.characterMotor) mass = victimBody.characterMotor.mass;
                    else if (victimBody.rigidbody) mass = victimBody.rigidbody.mass;
                    else mass = 1f;

                    var radius = baseExplosionRadius + stackExplosionRadius * (stack - 1);

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

                    blastAttack.AddModdedDamageType(milleniumDamageType);
                    blastAttack.Fire();

                    damageInfo.procChainMask.AddProc(milleniumAoE);

                    AkSoundEngine.PostEvent(Events.Play_voidRaid_snipe_impact, victimBody.gameObject);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}