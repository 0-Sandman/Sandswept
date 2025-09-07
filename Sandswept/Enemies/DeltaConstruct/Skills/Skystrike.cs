using System;
using Rewired.Demos;
using Sandswept.Survivors;
using Sandswept.Utils.Components;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class SkystrikeIntro : BaseSkillState
    {
        public float duration = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.characterMotor.walkSpeedPenaltyCoefficient = 0f;

            base.gameObject.layer = LayerIndex.noCollision.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            GetModelAnimator().SetBool("isAerial", true);

            PlayAnimation("Body", "Leap", "Generic.playbackRate", duration);

            GetModelAnimator().SetLayerWeight(GetModelAnimator().GetLayerIndex("AimYaw"), 0f);
            GetModelAnimator().SetLayerWeight(GetModelAnimator().GetLayerIndex("AimPitch"), 0f);

            base.characterMotor.ApplyForce(Vector3.up * base.characterMotor.mass * 40f, true, true);

            Util.PlaySound("Play_moonBrother_phaseJump_kneel", base.gameObject);
            // Util.PlaySound("Play_moonBrother_phaseJump_jumpAway", base.gameObject);
            Util.PlaySound("Play_majorConstruct_shift_raise", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration)
            {
                outer.SetNextState(new SkystrikeTransform());
            }
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }

    [ConfigSection("Enemies :: Delta Construct")]
    public class SkystrikeFire : BaseSkillState
    {
        public float duration = 1.5f;
        public float delay = 1f / 20f;
        public float damageCoeff = 12f / 10f;
        public float stopwatch = 0f;
        public SkystrikeLaserInfo[] beams;
        public float speed = 40f;
        public Vector3 guh;

        [ConfigField("Laser Configuration Speed", "In m/s", 40f)]
        public static float laserSpeed;

        [ConfigField("Laser Configuration Damage", "Decimal.", 1.2f)]
        public static float laserDamage;

        [ConfigField("Fire Trail Damage Per Second", "Decimal.", 4f)]
        public static float fireTrailDPS;

        public override void OnEnter()
        {
            base.OnEnter();

            speed = laserSpeed;
            damageCoeff = laserDamage;

            for (int i = 0; i < beams.Length; i++)
            {
                if (!beams[i].effect) continue;

                beams[i].effect.GetComponent<ChildLocator>().FindChild("Sparks").gameObject.SetActive(true);
                beams[i].rend.widthMultiplier = 2f;
                beams[i].rend.material = DeltaConstruct.matDeltaBeamStrong;
                //Util.PlaySound("Play_majorConstruct_m1_laser_loop", beams[i].lineHandle.gameObject);
            }
            Util.PlaySound("Play_majorConstruct_m1_laser_loop", gameObject);
            // bro is so loud
            // maybe play sound at endpoints + gameobject instead?
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            base.characterMotor.velocity = Vector3.zero;
            // base.characterDirection.forward = guh;

            bool recalc = false;

            if (stopwatch >= delay)
            {
                stopwatch = 0f;
                recalc = true;
            }

            for (int i = 0; i < beams.Length; i++)
            {
                SkystrikeLaserInfo beam = beams[i];
                if (beam.direction == Vector3.zero)
                {
                    continue;
                }

                beam.endpoint = beam.endpoint + (beam.direction * (speed * Time.fixedDeltaTime));

                if (recalc)
                {
                    var grounded = beam.endpoint.GroundPointWithNormal();
                    beam.endpoint = grounded.Item1.HasValue ? grounded.Item1.Value : beam.endpoint;

                    if (NetworkServer.active)
                    {
                        GetBulletAttack(beam).Fire();
                    }

                    if (NetworkServer.active)
                    {
                        GameObject trail = GameObject.Instantiate(DeltaConstruct.DeltaBurnyTrail, beam.endpoint, Quaternion.identity);
                        // trail.transform.up = grounded.Item2.Value;
                        trail.GetComponent<DeltaBurnyTrail>().damagePerSecond = base.damageStat * fireTrailDPS;
                        trail.GetComponent<DeltaBurnyTrail>().owner = base.characterBody;
                    }
                }

                beam.lineHandle.position = Vector3.MoveTowards(beam.lineHandle.position, beam.endpoint, 90f * Time.fixedDeltaTime);
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;

            base.characterDirection.enabled = true;

            for (int i = 0; i < beams.Length; i++)
            {
                // Util.PlaySound("Stop_majorConstruct_m1_laser_loop", beams[i].lineHandle.gameObject);
                // Util.PlaySound("Play_majorConstruct_m1_laser_end", beams[i].lineHandle.gameObject);
                GameObject.Destroy(beams[i].effect);
            }

            Util.PlaySound("Stop_majorConstruct_m1_laser_loop", gameObject);
            Util.PlaySound("Play_majorConstruct_m1_laser_end", gameObject);

            GetModelAnimator().SetBool("isAerial", false);
            PlayAnimation("Body", "Aerial To Ground", "Generic.playbackRate", duration);
            GetModelAnimator().SetLayerWeight(GetModelAnimator().GetLayerIndex("AimYaw"), 1f);
            GetModelAnimator().SetLayerWeight(GetModelAnimator().GetLayerIndex("AimPitch"), 1f);
        }

        public BulletAttack GetBulletAttack(SkystrikeLaserInfo info)
        {
            BulletAttack attack = new()
            {
                radius = 1.2f,
                damage = base.damageStat * damageCoeff,
                origin = info.muzzle.position,
                aimVector = (info.endpoint - info.muzzle.position).normalized,
                procCoefficient = 0.1f,
                owner = base.gameObject,
                falloffModel = BulletAttack.FalloffModel.None,
                isCrit = base.RollCrit(),
                stopperMask = LayerIndex.world.mask
            };

            return attack;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    public class SkystrikeWindup : BaseSkillState
    {
        public float duration = 3f;
        public SkystrikeLaserInfo[] skystrikeBeams;
        public Vector3 guh;
        public bool wasKnockedOutOfState = true;

        public override void OnEnter()
        {
            base.OnEnter();

            skystrikeBeams = new SkystrikeLaserInfo[8];

            Vector3[] points = GetSpiralPointSet(base.transform.position, 50f, 10f);
            int guh = points.Length / 9;

            for (int i = 0; i < skystrikeBeams.Length; i++)
            {
                SkystrikeLaserInfo info = new();
                info.muzzle = FindModelChild("Muzzle" + (i + 1));
                info.endpoint = GetEndpoint(points[guh * (i + 1)]);
                info.direction = Random.onUnitSphere.normalized;
                info.direction = new(info.direction.x, 0f, info.direction.z);
                info.effect = GameObject.Instantiate(DeltaConstruct.beam, info.muzzle);
                info.lineHandle = info.effect.GetComponent<ChildLocator>().FindChild("End");
                info.rend = info.effect.GetComponent<DetachLineRendererAndFade>().line;

                if (info.endpoint == Vector3.zero)
                {
                    info.effect.gameObject.SetActive(false);
                    info.direction = Vector3.zero;
                }

                info.lineHandle.position = info.endpoint;

                skystrikeBeams[i] = info;
            }

            Util.PlaySound("Play_majorConstruct_m1_laser_chargeShoot", base.gameObject);
            Util.PlaySound("Play_majorConstruct_m1_laser_chargeShoot", base.gameObject);
            // Util.PlaySound("Play_majorConstruct_m1_laser_chargeShoot", base.gameObject);
        }

        public Vector3 GetEndpoint(Vector3 inp)
        {
            Vector3? val = MiscUtils.GroundPoint(inp);

            return (val.HasValue ? val.Value : Vector3.zero);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float mult = Mathf.Clamp01(1f - (base.fixedAge / duration));

            for (int i = 0; i < skystrikeBeams.Length; i++)
            {
                skystrikeBeams[i].rend.widthMultiplier = mult;
            }

            // characterDirection.forward = guh;

            if (base.fixedAge >= duration)
            {
                wasKnockedOutOfState = false;
                outer.SetNextState(new SkystrikeFire());
            }

            if (base.characterMotor.velocity.y < 0)
            {
                base.characterMotor.velocity.y = 0;
            }

            base.characterMotor.velocity = new(0, base.characterMotor.velocity.y, 0);
        }

        public static Vector3[] GetSpiralPointSet(Vector3 origin, float scalar, float initialRadius, int loops = 5)
        {
            Vector3[] points = new Vector3[360 * loops];

            float radius = initialRadius;
            float radialStep = (scalar - initialRadius) / (loops * 360f);

            for (int i = 0; i < 360 * loops; i++)
            {
                int j = i > 360 ? i - 360 : i;

                float rad = j * 2 * Mathf.PI / 360f;
                float vert = Mathf.Sin(rad);
                float horiz = Mathf.Cos(rad);

                Vector3 dir = new(horiz, 0, vert);

                Vector3 point = origin + dir * radius;
                radius += radialStep;

                points[i] = point;
            }

            return points;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!wasKnockedOutOfState) return;

            for (int i = 0; i < skystrikeBeams.Length; i++)
            {
                // Util.PlaySound("Stop_majorConstruct_m1_laser_loop", skystrikeBeams[i].lineHandle.gameObject);
                // Util.PlaySound("Play_majorConstruct_m1_laser_end", skystrikeBeams[i].lineHandle.gameObject);
                GameObject.Destroy(skystrikeBeams[i].effect);
            }

            Util.PlaySound("Stop_majorConstruct_m1_laser_loop", gameObject);
            Util.PlaySound("Play_majorConstruct_m1_laser_end", gameObject);
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);

            if (nextState is SkystrikeFire skystrikeFire)
            {
                skystrikeFire.beams = skystrikeBeams;
                skystrikeFire.guh = guh;
                // why would you name a variable that...
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    public class SkystrikeTransform : BaseSkillState
    {
        public float duration = 1f;
        public Vector3 dir;

        public override void OnEnter()
        {
            base.OnEnter();

            dir = base.characterDirection.forward;

            base.characterDirection.enabled = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // characterDirection.forward = dir;

            if (base.fixedAge >= duration)
            {
                outer.SetNextState(new SkystrikeWindup());
            }

            if (base.characterMotor.velocity.y < 0)
            {
                base.characterMotor.velocity.y = 0;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    public class SkystrikeLaserInfo
    {
        public Transform muzzle;
        public Vector3 direction;
        public Vector3 endpoint;
        public GameObject effect;
        public Transform lineHandle;
        public LineRenderer rend;
    }

    public class SkystrikeSkill : SkillBase<SkystrikeSkill>
    {
        public override string Name => "omg hiii";

        public override string Description => "<3 :3 :3 <3 UwU >w< >_< >_> OwO :3 <3";

        public override Type ActivationStateType => typeof(SkystrikeIntro);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 20f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}