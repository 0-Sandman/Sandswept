using System.Collections;
using IL.RoR2.Achievements.Engi;
using LookingGlass.ItemStatsNameSpace;
using Sandswept.Items.Whites;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Drifting Perception")]
    public class DriftingPerception : ItemBase<DriftingPerception>
    {
        public override string ItemName => "Drifting Perception";

        public override string ItemLangTokenName => "DRIFTING_PERCEPTION";

        public override string ItemPickupDesc => "Cloak upon entering combat. Being cloaked increases your 'Critical Strike' chance and 'Critical Damage'. Recharges over time.";

        public override string ItemFullDescription => $"Upon entering combat, become $sucloaked$se for $su{cloakBuffDuration}s$se. While $sucloaked$se, increase $sdcritical chance$se by $sd{baseCritChanceGain}%$se and $sdcritical damage$se by $sd{baseCritDamageGain * 100f}%$se $ss(+{stackCritDamageGain * 100f}% per stack)$se. Recharges every $su{rechargeTime}$se seconds.".AutoFormat();

        public override string ItemLore =>
        """
        Here, my child. It is a gift for you. It will protect you from the invaders in what is to follow. I have given one to each of the others who will accompany us, as well. When it senses the need, it will shield you from the eyes of your foes, and empower you.

        No, no need to worry about that. It is a simple construction. Your own life, along with the lives of all my children, are at stake here. I would give far more to protect you.

        Is that so? Curiosity befits you; I am happy to indulge. I have constructed it with the compounds that drive reality: soul, mass, and blood. This construction uses only two of the four -- the first two. They are present in all things, but only I can sculpt them.

        Hm? Oh, so I did, that is my mistake. Yes, there are only three.

        These outer pieces -- they are mass. They hold the construction together, to support the centerpiece, which is infused with soul. Gold is naturally created when soul is imbued into mass; it was  one of the first things I ever discovered about the compounds. It does not have the soul necessary to be truly alive, but it will be able to sense your peril.

        Yes, it is pretty. Its form is incidental, however -- it was made to be functional. It is nearly impossible to create such things with intentionality; it is far more a process of experimentation. It takes whatever form it sees fit, which is why all of these constructions are shaped differently. Almost always, their chosen form is quite beautiful.

        There is no need to worry, they all serve the same function. I have tested them myself. There are some negligible inconsistencies, but they are inevitable when using the compounds.

        ...

        Yes, I suppose it would be convenient, but my influence on the outcome is minimal. It would not be right to force such powerful compounds into any kind of...design, as you say.
        """;
        public override float modelPanelParametersMinDistance => 7f;
        public override float modelPanelParametersMaxDistance => 15f;

        [ConfigField("Base Crit Chance Gain", "", 25f)]
        public static float baseCritChanceGain;

        [ConfigField("Base Crit Damage Gain", "Decimal.", 0.6f)]
        public static float baseCritDamageGain;

        [ConfigField("Stack Crit Damage Gain", "Decimal.", 0.6f)]
        public static float stackCritDamageGain;

        [ConfigField("Cloak Buff Duration", "", 6f)]
        public static float cloakBuffDuration;

        [ConfigField("Recharge Time", "", 25f)]
        public static float rechargeTime;

        public static BuffDef cooldown;
        public static BuffDef ready;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("DriftingPerceptionHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texDriftingPerception.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.MobilityRelated, ItemTag.CanBeTemporary]; // not sure if it should be technology or not.

        public override void Init()
        {
            base.Init();
            SetUpBuffs();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Crit Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    baseCritDamageGain + stackCritDamageGain * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpBuffs()
        {
            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.isHidden = false;
            cooldown.isDebuff = false;
            cooldown.canStack = false;
            cooldown.isCooldown = false;
            cooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f); // wolfo consistency
            cooldown.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffDriftingPerceptionCooldown.png");
            cooldown.name = "Drifting Perception - Cooldown";
            ContentAddition.AddBuffDef(cooldown);

            ready = ScriptableObject.CreateInstance<BuffDef>();
            ready.isHidden = false;
            ready.isDebuff = false;
            ready.canStack = false;
            ready.isCooldown = false;
            ready.buffColor = new Color32(198, 114, 48, 255);
            ready.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffDriftingPerceptionReady.png");
            ready.name = "Drifting Perception - Ready";
            ContentAddition.AddBuffDef(ready);
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            var stack = GetCount(sender);
            if (stack > 0 && sender.hasCloakBuff)
            {
                args.critAdd += baseCritChanceGain;
                args.critDamageMultAdd += baseCritDamageGain + stackCritDamageGain * (stack - 1);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<DriftingPerceptionController>(GetCount(body));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var itemDisplay = SetUpFollowerIDRS(0.3f, 25f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(1.5f, 2f, 1.4f),
                localScale = new Vector3(0.1f, 0.1f, 0.1f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class DriftingPerceptionController : CharacterBody.ItemBehavior
    {
        public HealthComponent hc;

        public void Start()
        {
            hc = body.healthComponent;
        }

        public void FixedUpdate()
        {
            if (!body.HasBuff(DriftingPerception.cooldown) && stack > 0)
            {
                if (!body.HasBuff(DriftingPerception.ready))
                {
                    body.AddBuff(DriftingPerception.ready);
                    return;
                }

                if (body.HasBuff(DriftingPerception.ready) && body.outOfCombatStopwatch <= 2f /*!body.outOfCombat*/)
                {
                    if (!body.hasCloakBuff)
                    {
                        body.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, DriftingPerception.cloakBuffDuration);
                        if (!body.HasBuff(RoR2Content.Buffs.CloakSpeed))
                        {
                            body.AddTimedBuffAuthority(RoR2Content.Buffs.CloakSpeed.buffIndex, DriftingPerception.cloakBuffDuration);
                        }

                        Util.PlaySound("Play_roboBall_attack2_mini_spawn", gameObject);
                        Util.PlaySound("Play_roboBall_attack2_mini_spawn", gameObject);
                        EffectManager.SimpleEffect(Paths.GameObject.SmokescreenEffect, body.corePosition, Quaternion.identity, true);

                        body.RemoveBuff(DriftingPerception.ready);
                        body.AddTimedBuffAuthority(DriftingPerception.cooldown.buffIndex, DriftingPerception.rechargeTime + DriftingPerception.cloakBuffDuration);
                    }
                }
            }
        }

        public void OnDisable()
        {
            if (body.HasBuff(DriftingPerception.ready))
            {
                body.RemoveBuff(DriftingPerception.ready);
            }
            if (body.HasBuff(DriftingPerception.cooldown))
            {
                body.RemoveBuff(DriftingPerception.cooldown);
            }
        }
    }
}