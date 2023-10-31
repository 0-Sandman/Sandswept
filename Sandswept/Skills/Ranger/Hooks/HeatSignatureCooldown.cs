using Sandswept.States.Ranger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Skills.Ranger.Hooks
{
    public static class HeatSignatureCooldown
    {
        public static void Init()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var body = self.body;
            if (body)
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    var entityStateMachine = EntityStateMachine.FindByCustomName(self.gameObject, "Overdrive");
                    if (entityStateMachine)
                    {
                        var currentStateType = entityStateMachine.state.GetType();
                        var isInOverdrive = currentStateType == typeof(OverdriveEnter) || currentStateType == typeof(OverdriveFire) || currentStateType == typeof(OverdriveExit);
                        if (isInOverdrive && skillLocator.utility)
                        {
                            skillLocator.utility.rechargeStopwatch = Mathf.Max(0f, skillLocator.utility.rechargeStopwatch - 2f);
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}