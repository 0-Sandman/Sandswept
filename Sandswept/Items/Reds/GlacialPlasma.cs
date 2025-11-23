using LookingGlass.ItemStatsNameSpace;
using Rewired.ComponentControls.Effects;
using UnityEngine;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Glacial Plasma")]
    internal class GlacialPlasma : ItemBase<GlacialPlasma>
    {
        public override string ItemName => "Glacial Plasma";

        public override string ItemLangTokenName => "GLACIAL_PLASMA";

        public override string ItemPickupDesc => "Activating your primary also conjures a freezing javelin. Chance on hit to freeze stunned enemies.";

        public override string ItemFullDescription => $"Activating your $suPrimary skill$se also conjures a $sdpiercing javelin$se that deals $sd{baseDamage * 100f}%$se $ss(+{stackDamage * 100f}% per stack)$se damage and $sufreezes$se enemies. Recharges over $su10$se seconds. Your $sustuns$se have a $su{stunToFreezeChance}%$se chance to $sufreeze$se for $su{stunToFreezeDuration}$se seconds.".AutoFormat();

        public override string ItemLore =>
        """
        "Drink it in."

        "It's quite something. I've never seen anything like it...this is a thoroughly unparalleled discovery."

        "I'd like to think so. The potential of such an object is nearly boundless, even just in engineering, and it does far more. Temperatures previously restricted to isolated labs, now applicable to any object with just a smear of this stuff."

        "How'd you discover it? And how did you synthesize such a bizarre thing so quickly?"

        "Well, um..."

        "Hm? Oh, no, I know that look on your face. What'd you do?"

        "So, you remember that High Court ambassador?"

        "You didn't."

        "I may have."

        "We're dead meat, you know. We're [REDACTED] dead. You don't mess with the High Court, you just don't. You know that."

        "I beg to differ. I've had it for over a month now--"

        "WHAT?"

        "--for over a month now, and nobody's come a-knocking. I think it's high time we gave this sample to someone else. Let them deal with the consequences, and we'll make it out with a nice chunk of change."

        "So what, we just lie to them about its origins? What happens when the High Court starts knocking on their door? Even if the Court doesn't kill us, our buyer will."

        "We don't have to lie. Have you seen the things this stuff can do? For the UES, or one of its would-be competitors, this is too valuable to pass up."
        """;

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.assets.LoadAsset<GameObject>("PickupGlacialPlasma.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texGlacialPlasma.png");

        [ConfigField("Base Damage", "Decimal.", 20f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 20f)]
        public static float stackDamage;

        [ConfigField("Cooldown", "", 10f)]
        public static float cooldown;

        [ConfigField("Stunned Enemy Freeze Chance", "", 6f)]
        public static float stunToFreezeChance;

        [ConfigField("Stun To Freeze Duration", "", 3f)]
        public static float stunToFreezeDuration;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        public static GameObject javelinProjectile;

        public static BuffDef javelinReady;
        public static BuffDef javelinCooldown;
        public static GameObject SpawnEffect;

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary];

        public override void Init()
        {
            base.Init();
            SetUpVFX();
            SetUpBuffs();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Javelin Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Stun to Freeze Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    baseDamage + stackDamage * (stack - 1),
                    LookingGlass.Utils.CalculateChanceWithLuck(stunToFreezeChance * procChance * 0.01f, luck),
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            javelinProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.MageIceBombProjectile, "Glacial Plasma Javelin", true);

            var projectileSimple = javelinProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 400f;
            projectileSimple.lifetime = 3f;
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.04f, 0f), new Keyframe(0.9f, 3f), new Keyframe(1f, 0f));

            var projectileController = javelinProjectile.GetComponent<ProjectileController>();
            projectileController.procCoefficient = procCoefficient;

            var ghost = Main.assets.LoadAsset<GameObject>("GlacialSpearGhost.prefab");
            ghost.transform.localScale = Vector3.one * 4f;

            var mesh = ghost.transform.Find("GlacialPlasma").GetComponent<MeshFilter>();

            var rot = mesh.AddComponent<RotateAroundAxis>();
            rot.enabled = true;
            rot.speed = RotateAroundAxis.Speed.Fast;
            rot.fastRotationSpeed = 300f;
            rot.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;

            /*
            var rot2 = mesh.AddComponent<RotateAroundAxis>();
            rot2.enabled = true;
            rot2.speed = RotateAroundAxis.Speed.Fast;
            rot2.fastRotationSpeed = 200f;
            rot2.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;

            var rot3 = mesh.AddComponent<RotateAroundAxis>();
            rot3.enabled = true;
            rot3.speed = RotateAroundAxis.Speed.Fast;
            rot3.fastRotationSpeed = 200f;
            rot3.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
            */

            var randomRot2 = mesh.AddComponent<SetRandomRotation>();
            randomRot2.setRandomXRotation = true;
            randomRot2.setRandomYRotation = false;
            randomRot2.setRandomZRotation = false;

            var objectScaleCurve = ghost.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.useOverallCurveOnly = true;
            objectScaleCurve.timeMax = 3f;
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(1f, 3f));

            var hitbox = javelinProjectile.transform.Find("Hitbox");
            hitbox.localScale = new Vector3(6f, 6f, 20f);
            hitbox.localPosition = new Vector3(0f, 0f, -4f);
            javelinProjectile.GetComponent<ProjectileController>().ghostPrefab = ghost;

            var newImpact = PrefabAPI.InstantiateClone(Paths.GameObject.MageIceExplosion, "Glacial Plasma Explosion VFX", false);

            VFXUtils.MultiplyScale(newImpact, 2.5f);
            VFXUtils.RecolorMaterialsAndLights(newImpact, new Color32(0, 188, 255, 255), new Color32(0, 188, 255, 255), true);

            newImpact.transform.Find("Point Light").GetComponent<Light>().range = 15f;

            ContentAddition.AddEffect(newImpact);

            projectileSimple.lifetimeExpiredEffect = newImpact;
            var projectileSingleTargetImpact = javelinProjectile.GetComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact.impactEffect = newImpact;

            PrefabAPI.RegisterNetworkPrefab(javelinProjectile);
            ContentAddition.AddProjectile(javelinProjectile);

            SpawnEffect = Main.assets.LoadAsset<GameObject>("GlacialCastEffect.prefab");
            ContentAddition.AddEffect(SpawnEffect);
        }

        public void SetUpBuffs()
        {
            javelinReady = ScriptableObject.CreateInstance<BuffDef>();
            javelinReady.isCooldown = false;
            javelinReady.canStack = false;
            javelinReady.isDebuff = false;
            javelinReady.isHidden = false;
            javelinReady.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffGlacialPlasmaReady.png");
            javelinReady.buffColor = new Color32(0, 208, 252, 255);
            javelinReady.name = "Glacial Plasma Ready";

            ContentAddition.AddBuffDef(javelinReady);

            javelinCooldown = ScriptableObject.CreateInstance<BuffDef>();
            javelinCooldown.isCooldown = false;
            javelinCooldown.canStack = false;
            javelinCooldown.isDebuff = false;
            javelinCooldown.isHidden = false;
            javelinCooldown.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffGlacialPlasmaCooldown.png");
            javelinCooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            javelinCooldown.name = "Glacial Plasma Cooldown";

            ContentAddition.AddBuffDef(javelinCooldown);
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
                if (Util.CheckRoll(stunToFreezeChance, attackerMaster) && (setStateOnHurt.targetStateMachine.state is StunState || setStateOnHurt.targetStateMachine.state is ShockState))
                {
                    // var stunState = setStateOnHurt.targetStateMachine.state as StunState;
                    setStateOnHurt.SetFrozen(3f);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();

            var itemDisplay = SetUpFollowerIDRS(0.36f, 66f, false, 0f, true, 20f, false, 0f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(1f, 0.5f, 0.5f),
                localScale = new Vector3(1f, 1f, 1f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }

        public class GlacialPlasmaController : CharacterBody.ItemBehavior
        {
            public InputBankTest inputBank;
            public SkillLocator skillLocator;

            public void Start()
            {
                inputBank = body.inputBank;
                skillLocator = body.skillLocator;
                body.onSkillActivatedServer += OnSkillActivated;
            }

            public void OnSkillActivated(GenericSkill skill)
            {
                if (skill != skillLocator.primary)
                {
                    return;
                }

                if (body.HasBuff(javelinCooldown))
                {
                    return;
                }

                FireJavelin();
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

                EffectManager.SpawnEffect(SpawnEffect, new EffectData
                {
                    scale = 2f,
                    origin = fpi.position,
                    rotation = fpi.rotation
                }, false);

                // if (Util.HasEffectiveAuthority(gameObject))
                {
                    ProjectileManager.instance.FireProjectile(fpi);
                }

                Util.PlaySound("Play_mage_shift_wall_build", gameObject);

                body.AddTimedBuffAuthority(javelinCooldown.buffIndex, cooldown);
            }

            public void OnDestroy()
            {
                body.onSkillActivatedServer -= OnSkillActivated;
            }
        }
    }
}