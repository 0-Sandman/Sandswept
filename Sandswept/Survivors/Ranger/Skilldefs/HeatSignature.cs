﻿using Sandswept.Survivors;
using Sandswept.Survivors.Ranger.States;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class HeatSignature : SkillBase<HeatSignature>
    {
        public override string Name => "Heat Signature";

        public override string Description => "$suAgile$se. $sdStunning$se. $suSidestep$se a very short distance and deal $sd250% damage$se. Hitting enemies generates $r1 Charge$ec.".AutoFormat();

        // should be 3 charge no matter how many you hit
        // also the overlapattack should stop as soon as you hit someone
        public override Type ActivationStateType => typeof(States.HeatSignature);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 3f;

        public override bool BeginCooldownOnSkillEnd => true;
        public override bool MustKeyPress => true;

        public override bool Agile => true;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatSignature.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.Stun };
    }
}