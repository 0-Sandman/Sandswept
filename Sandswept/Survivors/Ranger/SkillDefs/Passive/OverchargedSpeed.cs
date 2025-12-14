using R2API.Networking;
using Sandswept.Buffs;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class OverchargedSpeed : SkillBase<OverchargedSpeed>
    {
        public override string Name => "Overcharged Speed";
        public override string Description => "Hold up to " + Projectiles.DirectCurrent.maxCharge + " $rcCharge$ec. $rcCharge$ec increases $sumovement speed$se by up to $su25%$se. Consume $rc5$ec Charge to $sudouble jump$se. $suCharge decays over time$se.".AutoFormat();
        public override Type ActivationStateType => typeof(GenericCharacterMain);
        public override string ActivationMachineName => "Body";
        public override float Cooldown => 0f;
        public override Sprite Icon => Main.assets.LoadAsset<Sprite>("Dash.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerPassiveDef>();
            var passive = (RangerPassiveDef)skillDef;

            passive.onAssigned += (slot) =>
            {
                var component = slot.AddComponent<RangerPassiveOverchargedSpeed>();

                return new OverchargedSpeedInstanceData()
                {
                    self = component
                };
            };

            passive.onUnassigned += (slot) =>
            {
                if (slot.skillInstanceData != null)
                {
                    GameObject.Destroy((slot.skillInstanceData as OverchargedSpeedInstanceData).self);
                }
            };
        }

        public class OverchargedSpeedInstanceData : SkillDef.BaseSkillInstanceData
        {
            public RangerPassiveOverchargedSpeed self;
        }

        public class RangerPassiveOverchargedSpeed : MonoBehaviour
        {
            public CharacterBody body;

            public void Start()
            {
                body = GetComponent<CharacterBody>();

                RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
                On.EntityStates.GenericCharacterMain.ProcessJump_bool += ProcessJump;
            }

            public void RecalculateStats(CharacterBody body, StatHookEventArgs args)
            {
                if (body == this.body)
                {
                    args.moveSpeedMultAdd += 0.0125f * body.GetBuffCount(Charge.instance.BuffDef);
                }
            }

            private void ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump_bool orig, EntityStates.GenericCharacterMain self, bool ignoreRequirements)
            {
                if (!self.characterBody || self.characterBody != body)
                {
                    orig(self, ignoreRequirements);
                    return;
                }

                int count = self.characterBody.GetBuffCount(Charge.instance.BuffDef);

                if (self.jumpInputReceived && count >= 5 && self.characterMotor && self.characterMotor.jumpCount >= self.characterBody.maxJumpCount && !ignoreRequirements)
                {
                    self.characterBody.SetBuffCountSynced(Charge.instance.BuffDef.buffIndex, count - 5);
                    ignoreRequirements = true;
                }

                orig(self, ignoreRequirements);
            }

            public void OnDestroy()
            {
                RecalculateStatsAPI.GetStatCoefficients -= RecalculateStats;
                On.EntityStates.GenericCharacterMain.ProcessJump_bool -= ProcessJump;
            }
        }
    }
}