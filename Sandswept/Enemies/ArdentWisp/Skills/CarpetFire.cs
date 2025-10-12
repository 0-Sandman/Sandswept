using System;
using System.Collections;
using System.Linq;
using RoR2.CharacterAI;
using Sandswept.Survivors;
using Sandswept.Utils.Components;

namespace Sandswept.Enemies.ArdentWisp.States
{
    [ConfigSection("Enemies :: Ardent Wisp")]
    public class CarpetFireStart : BaseSkillState
    {
        public float duration = 0.6f;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Body", "Carpet Fire, Start", "Generic.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.StartAimMode(0.2f);

            if (base.fixedAge >= duration)
            {
                outer.SetNextState(new CarpetFire());
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }

    [ConfigSection("Enemies :: Ardent Wisp")]
    public class CarpetFire : BaseSkillState
    {
        public static int shotsPerVolley = 3;
        public static int totalVolleys = 12;
        public static float duration = 5f;
        public static float warningTime = 1f;
        public static float blastRadius = 5.5f;
        public static float attackRadius = 18f;
        public static float damageCoefficient = 2.5f;
        public static GameObject explosion => ArdentWisp.ArdentExplosion;
        public float delay;
        public float stopwatch;
        public BaseAI ai;
        public int totalFired = 0;
        public override void OnEnter()
        {
            base.OnEnter();

            delay = duration / (float)totalVolleys;

            GetModelAnimator().SetBool("isRaining", true);

            ai = base.characterBody.master.GetComponent<BaseAI>();

            ChildLocator loc = GetModelChildLocator();
            loc.StartParticles("HandFireL");
            loc.StartParticles("HandFireR");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.StartAimMode(0.2f);

            if (!base.isAuthority)
            {
                return;
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch > delay && totalFired < totalVolleys)
            {
                stopwatch = 0f;

                if (!ai.currentEnemy.gameObject)
                {
                    return;
                }

                ChildLocator loc = base.GetModelChildLocator();

                int[] usedNums = new int[3];
                List<Vector3> struckPoints = new();
                for (int i = 0; i < 3f; i++)
                {
                    int counter = 0;
                ret:
                    Vector3 point = ai.currentEnemy.gameObject.transform.position + (Random.onUnitSphere * attackRadius);
                    point.y = ai.currentEnemy.gameObject.transform.position.y + 15f;

                    Vector3? grounded = MiscUtils.GroundPoint(point);

                    if (!grounded.HasValue || (grounded.HasValue && !struckPoints.All(x => Vector3.Distance(grounded.Value, x) > blastRadius * 4f)))
                    {
                        counter++;
                        if (counter > 15)
                        {
                            continue;
                        }
                        goto ret;
                    }

                    int index = 0;
                    while (true)
                    {
                        index = Random.Range(0, 7);

                        if (usedNums[0] != index && usedNums[1] != index && usedNums[2] != index)
                        {
                            usedNums[i] = index;
                            break;
                        }
                    }

                    byte id = GetComboNumber();

                    EffectManager.SpawnEffect(ArdentWisp.ArdentChargeLine, new EffectData
                    {
                        origin = grounded.Value,
                        genericFloat = warningTime,
                        scale = blastRadius * 2f,
                        modelChildIndex = (short)index,
                        rootObject = base.gameObject,
                        genericUInt = id
                    }, true);

                    base.characterBody.StartCoroutine(CarpetFireAuthority(warningTime, grounded.Value));

                    FireProjectileInfo info = new();
                    info.position = loc.FindChild(index).transform.position;
                    info.rotation = Quaternion.identity;
                    info.damage = base.damageStat * damageCoefficient;
                    info.crit = base.RollCrit();
                    info.owner = base.gameObject;
                    info.comboNumber = id;
                    info.projectilePrefab = ArdentWisp.ArdentFireball;

                    ProjectileManager.instance.FireProjectile(info);
                }

                totalFired++;
            }

            if (base.fixedAge >= duration) {
                // recoil state is so we cant chain from this immediately into a rock blast
                outer.SetNextState(new CarpetFireRecoil());
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            ChildLocator loc = GetModelChildLocator();
            loc.StopParticles("HandFireL");
            loc.StopParticles("HandFireR");

            GetModelAnimator().SetBool("isRaining", false);
        }

        public byte GetComboNumber()
        {
            for (int i = 0; i < 50; i++)
            {
                byte val = (byte)Random.Range(byte.MinValue, byte.MaxValue + 1);

                if (!ArdentFlareCharge.BZMap.ContainsKey(val))
                {
                    return val;
                }
            }

            return 0;
        }

        public IEnumerator CarpetFireAuthority(float delay, Vector3 target)
        {
            yield return new WaitForSeconds(delay);

            BlastAttack attack = new();
            attack.position = target;
            attack.radius = blastRadius;
            attack.attacker = base.gameObject;
            attack.baseDamage = base.damageStat * damageCoefficient;
            attack.crit = base.RollCrit();
            attack.procCoefficient = 1f;
            attack.teamIndex = base.GetTeam();
            attack.falloffModel = BlastAttack.FalloffModel.None;

            attack.Fire();

            EffectManager.SpawnEffect(explosion, new EffectData
            {
                origin = target,
                scale = blastRadius,
            }, true);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
    public class CarpetFireRecoil : BaseSkillState {
        public float duration = 2f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }
    }
    public class CarpetFireSkill : SkillBase<CarpetFireSkill>
    {
        public override string Name => "omg hiii";

        public override string Description => "<3 :3 :3 <3 UwU >w< >_< >_> OwO :3 <3";

        public override Type ActivationStateType => typeof(CarpetFireStart);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 15f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}