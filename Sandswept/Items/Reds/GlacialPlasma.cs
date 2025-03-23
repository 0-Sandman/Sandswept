namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Glacial Plasma")]
    internal class GlacialPlasma : ItemBase<GlacialPlasma>
    {
        public override string ItemName => "Glacial Plasma";

        public override string ItemLangTokenName => "GLACIAL_PLASMA";

        public override string ItemPickupDesc => "Activating your primary also conjures a freezing javelin. Chance on hit to freeze stunned enemies.";

        public override string ItemFullDescription => ("Activating your $suPrimary skill$se also conjures a $sdpiercing javelin$se that deals $sd" + d(baseDamage) + "$se $ss(+" + d(stackDamage) + " per stack)$se damage and $sufreezes$se enemies. Recharges over $su10$se seconds. Your $sustuns$se have a $su" + stunToFreezeChance + "%$se chance to $sufreeze$se for $su3$se seconds.").AutoFormat();

        public override string ItemLore => "tbd but might be cool if the lore had the phrase 'everything will freeze' somewhere in there";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupGlacialPlasma.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBleedingWitness.png");

        [ConfigField("Base Damage", "Decimal.", 20f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 20f)]
        public static float stackDamage;

        [ConfigField("Cooldown", "", 10f)]
        public static float cooldown;

        [ConfigField("Stunned Enemy Freeze Chance", "", 8f)]
        public static float stunToFreezeChance;

        public static GameObject javelinProjectile;

        public static BuffDef javelinReady;
        public static BuffDef javelinCooldown;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility };

        public override void Init(ConfigFile config)
        {
            javelinProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.MageIceBombProjectile, "Glacial Plasma Javelin", true);

            var projectileSimple = javelinProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 400f;
            projectileSimple.lifetime = 3f;
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.05f, 0f), new Keyframe(1f, 3f));

            PrefabAPI.RegisterNetworkPrefab(javelinProjectile);
            ContentAddition.AddProjectile(javelinProjectile);

            javelinReady = ScriptableObject.CreateInstance<BuffDef>();
            javelinReady.isCooldown = false;
            javelinReady.canStack = false;
            javelinReady.isDebuff = false;
            javelinReady.isHidden = false;
            javelinReady.iconSprite = Paths.BuffDef.bdBleeding.iconSprite;
            javelinReady.buffColor = Color.cyan;

            ContentAddition.AddBuffDef(javelinReady);

            javelinCooldown = ScriptableObject.CreateInstance<BuffDef>();
            javelinCooldown.isCooldown = false;
            javelinCooldown.canStack = false;
            javelinCooldown.isDebuff = false;
            javelinCooldown.isHidden = false;
            javelinCooldown.iconSprite = Paths.BuffDef.bdBugWings.iconSprite;
            javelinCooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);

            ContentAddition.AddBuffDef(javelinCooldown);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<GlacialPlasmaController>(GetCount(body));
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
                    // var stunState = setStateOnHurt.targetStateMachine.state as StunState;
                    setStateOnHurt.SetFrozen(3f);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public class GlacialPlasmaController : CharacterBody.ItemBehavior
        {
            public InputBankTest inputBank;
            public SkillLocator skillLocator;

            public void Start()
            {
                inputBank = body.inputBank;
                skillLocator = body.skillLocator;
            }

            public void FixedUpdate()
            {
                if (!body.HasBuff(javelinCooldown) && !body.HasBuff(javelinReady))
                {
                    body.AddBuff(javelinReady);
                }

                if (inputBank.skill1.down && body.HasBuff(javelinReady))
                {
                    body.RemoveBuff(javelinReady);
                    body.AddTimedBuff(javelinCooldown, cooldown);
                    FireJavelin();
                }
            }

            public void FireJavelin()
            {
                var damage = baseDamage + stackDamage * (stack - 1);

                var fpi = new FireProjectileInfo()
                {
                    crit = body.RollCrit(),
                    damage = body.damage * damage,
                    damageColorIndex = DamageColorIndex.Fragile,
                    force = 2000f,
                    procChainMask = default,
                    owner = gameObject,
                    position = body.corePosition + new Vector3(1.5f, 0f, 0f),
                    rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection),
                    projectilePrefab = javelinProjectile,
                    damageTypeOverride = DamageType.Freeze2s
                };

                if (Util.HasEffectiveAuthority(gameObject))
                {
                    ProjectileManager.instance.FireProjectile(fpi);
                }

                Util.PlaySound("Play_mage_shift_wall_build", gameObject);
            }
        }
    }
}