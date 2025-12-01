using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.GammaConstruct
{
    [ConfigSection("Enemies :: Gamma Construct")]
    public class FireBeam : BaseSkillState
    {
        public Animator anim;
        public float duration = 6f;
        public bool firing = false;
        public BasicLaserBeam laser;
        private Transform modelTransform;

        [ConfigField("Single Laser Damage", "Decimal.", 6f)]
        public static float singleLaserDamage;

        public override void OnEnter()
        {
            base.OnEnter();

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 0.5f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = GammaConstruct.matTell;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }

            PlayAnimation("Body", "Deathray", "Generic.playbackRate", duration);

            Util.PlaySound("Play_majorConstruct_m1_laser_chargeShoot", base.gameObject);

            laser = new(base.characterBody, FindModelChild("MuzzleC"),
                new BasicLaserInfo
                {
                    OriginIsBase = true,
                    EndpointName = "End",
                    DamageCoefficient = singleLaserDamage,
                    FiringWidthMultiplier = 3.5f,
                    MaxRange = 190f,
                    FiringMaterial = GammaConstruct.matDeltaBeamStrong,
                    ChargeDelay = duration * 0.25f,
                    EffectPrefab = GammaConstruct.beam,
                    FiringMode = LaserFiringMode.TrackAim,
                    ImpactEffect = GammaConstruct.sigmaBlast,
                    TickRate = 30f
                }
            );

            anim = GetModelAnimator();
        }

        public override void Update()
        {
            base.Update();

            laser.UpdateVisual(Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            laser.Update(Time.fixedDeltaTime);

            base.StartAimMode(0.2f);

            if (anim.GetFloat("firing") >= 0.9 && !firing)
            {
                firing = true;
                laser.Fire();
                Util.PlaySound("Play_majorConstruct_m1_laser_loop", base.gameObject);
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

            Util.PlaySound("Stop_majorConstruct_m1_laser_loop", base.gameObject);
            Util.PlaySound("Play_majorConstruct_m1_laser_end", laser.effectInstance);
            laser.Stop();
        }
    }

    public class FireBeamSkill : SkillBase<FireBeamSkill>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(FireBeam);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 15f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}