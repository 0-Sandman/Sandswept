using Sandswept.Buffs;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class OverchargedSpeed : SkillBase<OverchargedSpeed>
    {
        public override string Name => "Overcharged Speed";
        public override string Description => "Hold up to " + Projectiles.DirectCurrent.maxCharge + " $rcCharge$ec. Each $rcCharge$ec increases $sumovement speed$se by $su2.5%$se and $sharmor$se by $sh1.5$se. Consume 3 charges to $sudouble jump$se. $rcCharge decays over time$ec.".AutoFormat();
        public override Type ActivationStateType => typeof(GenericCharacterMain);
        public override string ActivationMachineName => "Body";
        public override float Cooldown => 0f;
        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Dash.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerPassiveDef>();
            var passive = (RangerPassiveDef)skillDef;
            passive.onHook += () =>
            {
                GetStatCoefficients += Charged_GetStatCoefficients;
                On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            };
            passive.onUnhook += () =>
            {
                GetStatCoefficients -= Charged_GetStatCoefficients;
                On.EntityStates.GenericCharacterMain.ProcessJump -= GenericCharacterMain_ProcessJump;
            };
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (body)
            {
                args.moveSpeedMultAdd += 0.025f * body.GetBuffCount(Charge.instance.BuffDef);
            }
        }

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            if (!self.hasCharacterMotor || !self.jumpInputReceived)
            {
                orig(self);
                return;
            }
            var buffCount = self.characterBody.GetBuffCount(Charge.instance.BuffDef);
            if (self.characterMotor.jumpCount == self.characterBody.maxJumpCount && buffCount >= 3)
            {
                var jumpCount = self.characterBody.maxJumpCount;

                self.characterBody.SetBuffCount(Charge.instance.BuffDef.buffIndex, self.characterBody.GetBuffCount(Charge.instance.BuffDef) - 3);
                self.characterBody.maxJumpCount += 1;

                orig(self);

                self.characterBody.maxJumpCount = jumpCount;
            }
            else orig(self);
        }
    }
}