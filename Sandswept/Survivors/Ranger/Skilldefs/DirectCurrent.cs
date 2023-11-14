using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Ranger.States;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class DirectCurrent : SkillBase<DirectCurrent>
    {
        public override string Name => "Direct Current";

        public override string Description => "Fire an arcing electric current for $sd250% damage$se. Hitting enemies generates $rc1 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.DirectCurrent);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
    }
}