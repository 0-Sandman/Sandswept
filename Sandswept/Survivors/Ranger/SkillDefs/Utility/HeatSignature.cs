namespace Sandswept.Survivors.Ranger.SkillDefs.Utility
{
    public class HeatSignature : SkillBase<HeatSignature>
    {
        public override string Name => "Heat Signature";

        public override string Description => "$suAgile$se. $sdStunning$se. $suSidestep$se a very short distance and deal $sd250% damage$se. Hitting enemies generates $rc3 Charge$ec.".AutoFormat();

        // should be 3 charge no matter how many you hit
        // also the overlapattack should stop as soon as you hit someone
        public override Type ActivationStateType => typeof(States.Utility.HeatSignature);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 3f;

        public override bool BeginCooldownOnSkillEnd => true;
        public override bool MustKeyPress => true;

        public override bool Agile => true;
        public override bool FullRestockOnAssign => true;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_hutl.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.Stun };

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerUtilityDef>();
        }
    }
}