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

            if (!inputBank.skill3.down)
            {
                if (!tossedOut)
                {
                    if (TripwireController.ControllerMap.ContainsKey(gameObject))
                    {
                        TripwireController controller = TripwireController.ControllerMap[gameObject];
                        tossedOut = !controller.StartZip();
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

            if (FUCKINGEXPLODE)
            {
                if (TripwireController.ControllerMap.ContainsKey(gameObject))
                {
                    TripwireController controller = TripwireController.ControllerMap[gameObject];
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