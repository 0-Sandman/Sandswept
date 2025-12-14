using Sandswept.Buffs;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class OverchargedProtection : SkillBase<OverchargedProtection>
    {
        public override string Name => "Overcharged Protection";
        public override string Description => "Hold up to " + Projectiles.DirectCurrent.maxCharge + " $rcCharge$ec. $rcCharge$ec increases $shhealth regeneration$se by up to $sh2.5 hp/s$se and $sharmor$se by up to $sh15$se. $suCharge decays over time$se.".AutoFormat();
        public override Type ActivationStateType => typeof(GenericCharacterMain);
        public override string ActivationMachineName => "Body";
        public override float Cooldown => 0f;
        public override Sprite Icon => Main.assets.LoadAsset<Sprite>("Overheat.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerPassiveDef>();
            var passive = (RangerPassiveDef)skillDef;

            passive.onAssigned += (slot) =>
            {
                var component = slot.AddComponent<RangerPassiveOverchargedProtection>();

                return new OverchargedProtectionInstanceData()
                {
                    self = component
                };
            };

            passive.onUnassigned += (slot) =>
            {
                if (slot.skillInstanceData != null)
                {
                    GameObject.Destroy((slot.skillInstanceData as OverchargedProtectionInstanceData).self);
                }
            };
        }

        public static float GetRegenForBody(CharacterBody body)
        {
            var levelScale = 0.125f * 0.2f * (body.level - 1);
            return (0.125f + levelScale) * body.GetBuffCount(Charge.instance.BuffDef);
        }

        public class OverchargedProtectionInstanceData : SkillDef.BaseSkillInstanceData
        {
            public RangerPassiveOverchargedProtection self;
        }

        public class RangerPassiveOverchargedProtection : MonoBehaviour
        {
            public CharacterBody body;
            public HealthComponent hc;
            public float regenAccumulator;
            public float chargeRegen;
            public static ModdedProcType OverchargeRegen = ProcTypeAPI.ReserveProcType();

            public void Start()
            {
                body = GetComponent<CharacterBody>();
                hc = GetComponent<HealthComponent>();

                RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
            }

            public void FixedUpdate()
            {
                if (hc)
                {
                    regenAccumulator += chargeRegen * Time.fixedDeltaTime;

                    if (regenAccumulator >= 1f)
                    {
                        ProcChainMask mask = new();
                        mask.AddModdedProc(OverchargeRegen);
                        hc.Heal(regenAccumulator, mask, false);

                        regenAccumulator = 0f;
                    }
                }
            }

            public void RecalculateStats(CharacterBody body, StatHookEventArgs args)
            {
                if (body == this.body)
                {
                    int count = body.GetBuffCount(Charge.instance.BuffDef);
                    float levelScale = 0.125f * 0.2f * (body.level - 1);
                    chargeRegen = (0.125f + levelScale) * count;
                    args.armorAdd += 0.75f * count;
                }
            }

            public void OnDestroy()
            {
                RecalculateStatsAPI.GetStatCoefficients -= RecalculateStats;
            }
        }
    }
}