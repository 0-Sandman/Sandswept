using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.GammaConstruct {
    public class FireTwinBeam : BaseSkillState {
        public Animator anim;
        public float duration = 2.5f;
        public bool firing = false;
        public BasicLaserBeam laser;
        public BasicLaserBeam laser2;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Body", "Sweep", "Generic.playbackRate", duration);
            
            laser = new(base.characterBody, FindModelChild("MuzzleL"),
                new BasicLaserInfo {
                    OriginIsBase = true,
                    EndpointName = "End",
                    DamageCoefficient = 4f,
                    FiringWidthMultiplier = 2f,
                    MaxRange = 190f,
                    FiringMaterial = GammaConstruct.matDeltaBeamStrong,
                    ChargeDelay = duration * 0.3f,
                    EffectPrefab = GammaConstruct.beam,
                    FiringMode = LaserFiringMode.Straight,
                    ImpactEffect = Paths.GameObject.ExplosionMinorConstruct
                }
            );

            laser2 = new(base.characterBody, FindModelChild("MuzzleR"),
                new BasicLaserInfo {
                    OriginIsBase = true,
                    EndpointName = "End",
                    DamageCoefficient = 4f,
                    FiringWidthMultiplier = 2f,
                    MaxRange = 190f,
                    FiringMaterial = GammaConstruct.matDeltaBeamStrong,
                    ChargeDelay = duration * 0.3f,
                    EffectPrefab = GammaConstruct.beam,
                    FiringMode = LaserFiringMode.Straight,
                    ImpactEffect = Paths.GameObject.ExplosionMinorConstruct
                }
            );

            anim = GetModelAnimator();

            base.StartAimMode(0.05f);
            // base.rigidbodyDirection.enabled = false;

            AkSoundEngine.PostEvent(Events.Play_majorConstruct_m1_laser_chargeShoot, base.gameObject);
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

            if (anim.GetFloat("firing") >= 0.9 && !firing) {
                firing = true;
                laser.Fire();
                laser2.Fire();

                AkSoundEngine.PostEvent(Events.Play_majorConstruct_m1_laser_loop, laser.effectInstance);
                AkSoundEngine.PostEvent(Events.Play_majorConstruct_m1_laser_loop, laser2.effectInstance);
            }

            if (anim.GetFloat("firing") <= 0.2f && firing) {
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

            AkSoundEngine.PostEvent(Events.Stop_majorConstruct_m1_laser_loop, laser.effectInstance);
            AkSoundEngine.PostEvent(Events.Stop_majorConstruct_m1_laser_loop, laser2.effectInstance);
            AkSoundEngine.PostEvent(Events.Play_majorConstruct_m1_laser_end, laser.effectInstance);
            AkSoundEngine.PostEvent(Events.Play_majorConstruct_m1_laser_end, laser2.effectInstance);

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