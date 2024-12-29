using System;
using R2API.Networking.Interfaces;

namespace Sandswept.Survivors.Electrician.States
{
    public class StaticSnare : BaseSkillState
    {
        public float duration = 0.3f;
        public bool tossedOut = false;
        public bool FUCKINGEXPLODE = true;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= attackSpeedStat;

            PlayAnimation("Gesture, Override", "Throw", "Generic.playbackRate", duration);

            characterBody.SetSpreadBloom(12f, true);

            if (isAuthority && !TripwireController.ControllerMap.ContainsKey(gameObject))
            {
                tossedOut = true;
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.StaticSnare, 1f, characterBody, DamageTypeCombo.GenericUtility);
                ProjectileManager.instance.FireProjectile(info);

                Util.PlaySound("Play_MULT_m2_throw", gameObject);

                FUCKINGEXPLODE = false;
            }

            Util.PlaySound("Play_mage_m1_impact_lightning", gameObject);

            skillLocator.utility.DeductStock(1);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!inputBank.skill3.down && isAuthority)
            {
                if (!tossedOut)
                {
                    if (TripwireController.ControllerMap.ContainsKey(gameObject))
                    {
                        TripwireController controller = TripwireController.ControllerMap[gameObject];
                        new CallNetworkedMethod(controller.gameObject, "StartZip").Send(R2API.Networking.NetworkDestination.Server);
                        FUCKINGEXPLODE = false;
                    }
                }
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (tossedOut)
            {
                float cd = skillLocator.utility.baseRechargeInterval * 0.75f;
                skillLocator.utility.RunRecharge(cd);
            }

            if (FUCKINGEXPLODE && base.isAuthority)
            {
                if (TripwireController.ControllerMap.ContainsKey(gameObject))
                {
                    TripwireController controller = TripwireController.ControllerMap[gameObject];
                    new CallNetworkedMethod(controller.gameObject, "KABOOM").Send(R2API.Networking.NetworkDestination.Server);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}