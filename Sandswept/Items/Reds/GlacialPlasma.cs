namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Glacial Plasma")]
    internal class GlacialPlasma : ItemBase<GlacialPlasma>
    {
        public override string ItemName => "Glacial Plasma";

        public override string ItemLangTokenName => "GLACIAL_PLASMA";

        public override string ItemPickupDesc => "Chance on hit to conjure a freezing javelin. Chance on hit to freeze stunned enemies.";

        public override string ItemFullDescription => ("$sd" + chance + "%$se chance on hit to conjure a $sdjavelin$se that deals $sd" + d(baseTotalDamage) + "$se TOTAL damage $ss(+" + stackTotalDamage + " per stack)$se and $sufreezes$se enemies. Your $sustuns$se have a $su" + stunToFreezeChance + "%$se chance to $sufreeze$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("BleedingWitnessHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBleedingWitness.png");

        [ConfigField("Chance", "", 8f)]
        public static float chance;

        [ConfigField("Base TOTAL Damage", "Decimal.", 4.5f)]
        public static float baseTotalDamage;

        [ConfigField("Stack TOTAL Damage", "Decimal.", 4.5f)]
        public static float stackTotalDamage;

        [ConfigField("Stunned Enemy Freeze Chance", "", 8f)]
        public static float stunToFreezeChance;

        public static GameObject javelinProjectile;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility };

        public override void Init(ConfigFile config)
        {
            javelinProjectile = PrefabAPI.InstantiateClone(Assets.GameObject.MageIceBolt, "Glacial Plasma Javelin", true);

            var projectileSimple = javelinProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 100f;
            projectileSimple.lifetime = 3f;
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 0f), new Keyframe(1f, 2f));

            PrefabAPI.RegisterNetworkPrefab(javelinProjectile);
            ContentAddition.AddProjectile(javelinProjectile);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack <= 0)
            {
                return;
            }

            var attackerMaster = attackerBody.master;
            if (!attackerMaster)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            if (victimBody.TryGetComponent<SetStateOnHurt>(out var setStateOnHurt))
            {
                if (Util.CheckRoll(stunToFreezeChance, attackerMaster) && setStateOnHurt.targetStateMachine.state is StunState)
                {
                    var stunState = setStateOnHurt.targetStateMachine.state as StunState;
                    setStateOnHurt.SetFrozen(stunState.timeRemaining);
                }
            }

            var inputBank = attackerBody.GetComponent<InputBankTest>();
            if (!inputBank)
            {
                return;
            }

            if (Util.CheckRoll(chance, attackerMaster))
            {
                var fpi = new FireProjectileInfo()
                {
                    crit = report.damageInfo.crit,
                    damage = Util.OnHitProcDamage(report.damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1)),
                    damageColorIndex = DamageColorIndex.Fragile,
                    force = 2000f,
                    procChainMask = default,
                    owner = attackerBody.gameObject,
                    position = attackerBody.corePosition + new Vector3(1.5f, 0f, 0f),
                    rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection),
                    projectilePrefab = javelinProjectile,
                    damageTypeOverride = DamageType.Freeze2s
                };

                Util.PlaySound("Play_mage_shift_wall_build", attackerBody.gameObject);

                ProjectileManager.instance.FireProjectile(fpi);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}