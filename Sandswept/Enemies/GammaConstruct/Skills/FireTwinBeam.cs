using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.GammaConstruct
{
    [ConfigSection("Enemies :: Gamma Construct")]
    public class FireTwinBeam : BaseSkillState
    {
        public Animator anim;
        public float duration = 3.5f;
        public bool firing = false;
        public BasicLaserBeam laser;
        public BasicLaserBeam laser2;

        [ConfigField("Twin Laser Damage", "Decimal.", 20f)]
        public static float twinLaserDamage;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Body", "Sweep", "Generic.playbackRate", duration);

            laser = new(base.characterBody, FindModelChild("MuzzleL"),
                new BasicLaserInfo
                {
                    OriginIsBase = true,
                    EndpointName = "End",
                    DamageCoefficient = twinLaserDamage,
                    FiringWidthMultiplier = 3.5f,
                    MaxRange = 190f,
                    FiringMaterial = GammaConstruct.matDeltaBeamStrong,
                    ChargeDelay = duration * 0.3f,
                    EffectPrefab = GammaConstruct.beam,
                    FiringMode = LaserFiringMode.Straight,
                    ImpactEffect = GammaConstruct.sigmaBlast,
                    TickRate = 20f
                }
            );

            laser2 = new(base.characterBody, FindModelChild("MuzzleR"),
                new BasicLaserInfo
                {
                    OriginIsBase = true,
                    EndpointName = "End",
                    DamageCoefficient = twinLaserDamage,
                    FiringWidthMultiplier = 3.5f,
                    MaxRange = 190f,
                    FiringMaterial = GammaConstruct.matDeltaBeamStrong,
                    ChargeDelay = duration * 0.3f,
                    EffectPrefab = GammaConstruct.beam,
                    FiringMode = LaserFiringMode.Straight,
                    ImpactEffect = GammaConstruct.sigmaBlast,
                    TickRate = 20f
                }
            );

            anim = GetModelAnimator();

            base.StartAimMode(0.05f);
            // base.rigidbodyDirection.enabled = false;

            Util.PlaySound("Play_majorConstruct_m1_laser_chargeShoot", base.gameObject);
        }

        public override void Update()
        {
            base.Update();

            laser.UpdateVisual(Time.deltaTime);
            laser2.UpdateVisual(Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            laser.Update(Time.fixedDeltaTime);
            laser2.Update(Time.fixedDeltaTime);

            if (anim.GetFloat("firing") >= 0.9 && !firing)
            {
                firing = true;
                laser.Fire();
                laser2.Fire();

                Util.PlaySound("Play_majorConstruct_m1_laser_loop", laser.effectInstance);
                Util.PlaySound("Play_majorConstruct_m1_laser_loop", laser2.effectInstance);
            }

            if (anim.GetFloat("firing") <= 0.2f && firing)
            {
                firing = false;
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public override void OnExit()
        {
            base.OnExit();

            // base.rigidbodyDirection.enabled = true;

            Util.PlaySound("Stop_majorConstruct_m1_laser_loop", laser.effectInstance);
            Util.PlaySound("Stop_majorConstruct_m1_laser_loop", laser2.effectInstance);
            Util.PlaySound("Play_majorConstruct_m1_laser_end", laser.effectInstance);
            Util.PlaySound("Play_majorConstruct_m1_laser_end", laser2.effectInstance);

            laser.Stop();
            laser2.Stop();
        }
    }

    public class FireTwinBeamSkill : SkillBase<FireTwinBeamSkill>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(FireTwinBeam);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 8f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}