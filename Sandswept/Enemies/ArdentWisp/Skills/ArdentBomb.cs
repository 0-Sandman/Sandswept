using System;
using System.Collections;
using System.Linq;
using RoR2.CharacterAI;
using Sandswept.Survivors;
using Sandswept.Utils.Components;

namespace Sandswept.Enemies.ArdentWisp.States
{
    [ConfigSection("Enemies :: Ardent Wisp")]
    public class ArdentBomb : BaseSkillState
    {
        public static float damageCoefficient = 5f;
        public float duration = 1.6f;
        public Animator anim;
        public bool hasFired = false;
        public float leadTime => ArdentBombProjectile.blastTime;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Body", "Crystal Shot", "Generic.playbackRate", duration);

            // Main.ModLogger.LogError("starting crystal shot");

            anim = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (anim.GetFloat("crystalFire") > 0.5f && !hasFired)
            {
                hasFired = true;

                BaseAI ai = base.characterBody.master.GetComponent<BaseAI>();

                FireProjectileInfo info = new();
                info.position = base.transform.position;
                info.crit = base.RollCrit();
                info.rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection);
                info.owner = base.gameObject;
                info.damage = base.damageStat * 5f;
                info.projectilePrefab = ArdentWisp.ArdentBombProjectile;

                if (ai && ai.currentEnemy.gameObject)
                {
                    GameObject target = ai.currentEnemy.gameObject;
                    CharacterBody targetBody = target.GetComponent<CharacterBody>();

                    if (targetBody.characterMotor)
                    {
                        Vector3 vel = targetBody.characterMotor.velocity.Nullify(y: true);
                        Vector3 predicted = targetBody.footPosition + (vel * ArdentBombProjectile.blastTime);

                        info.rotation = Util.QuaternionSafeLookRotation((predicted - info.position).normalized);
                    }
                }

                ProjectileManager.instance.FireProjectile(info);
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
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

    public class ArdentBombSkill : SkillBase<ArdentBombSkill>
    {
        public override string Name => "fig";

        public override string Description => "<3 :3 :3 <3 UwU >w< >_< >_> OwO :3 <3";

        public override Type ActivationStateType => typeof(ArdentBomb);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 5f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}