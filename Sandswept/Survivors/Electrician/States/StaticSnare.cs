using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class StaticSnare : BaseSkillState
    {
        public float duration = 0.5f;
        public bool tossedOut = false;
        public bool FUCKINGEXPLODE = true;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;

            PlayAnimation("Gesture, Override", "Throw", "Generic.playbackRate", duration);

            characterBody.SetSpreadBloom(12f, true);

            if (base.isAuthority && !TripwireController.ControllerMap.ContainsKey(base.gameObject))
            {
                tossedOut = true;
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.StaticSnare, 1f, base.characterBody);
                ProjectileManager.instance.FireProjectile(info);

                Util.PlaySound("Play_MULT_m2_throw", base.gameObject);

                FUCKINGEXPLODE = false;
            }

            Util.PlaySound("Play_mage_m1_impact_lightning", gameObject);

            base.skillLocator.utility.DeductStock(1);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!base.inputBank.skill3.down) {
                if (!tossedOut)
                {
                    if (TripwireController.ControllerMap.ContainsKey(base.gameObject))
                    {
                        TripwireController controller = TripwireController.ControllerMap[base.gameObject];
                        tossedOut = !controller.StartZip();
                        FUCKINGEXPLODE = false;
                    }
                }
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (tossedOut)
            {
                float cd = base.skillLocator.utility.baseRechargeInterval * 0.75f;
                base.skillLocator.utility.RunRecharge(cd);
            }
            
            if (FUCKINGEXPLODE) {
                if (TripwireController.ControllerMap.ContainsKey(base.gameObject))
                {
                    TripwireController controller = TripwireController.ControllerMap[base.gameObject];
                    controller.KABOOM();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}