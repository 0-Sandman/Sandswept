using System;
using R2API.Utils;
using RoR2.CharacterAI;
using System.Linq;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Enemies.CannonballJellyfish.States
{
    [ConfigSection("Enemies :: Cannonball Jellyfish")]
    public class JellyDash : BaseState
    {
        [ConfigField("Dash Damage Coefficient", "Decimal.", 4f)]
        public static float DamageCoefficient;

        public static float DashForce = 50f;

        //
        private float durationToAttack;

        private float durationToTelegraph;

        private float durationToPlayWarnSound;

        private OverlapAttack attack;
        private float baseDurationToAttack = 1.5f;
        private bool dashedAlready = false;
        private bool showedTelegraph = false;
        private bool playedWarnSound = false;
        private BaseAI ai;
        private Vector3 lockVector;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            durationToAttack = baseDurationToAttack / attackSpeedStat;
            durationToTelegraph = 0.75f * durationToAttack;
            durationToPlayWarnSound = 0.5f * durationToAttack;

            if (base.characterBody.master)
            {
                ai = base.characterBody.master.GetComponent<BaseAI>();
            }

            attack = new()
            {
                attacker = base.gameObject,
                damage = base.damageStat * DamageCoefficient,
                isCrit = base.RollCrit(),
                hitBoxGroup = base.FindHitBoxGroup("Impact"),
                procCoefficient = 1f,
                teamIndex = base.GetTeam(),
                attackerFiltering = AttackerFiltering.NeverHitSelf
            };

            FlipComponents();

        }

        public void SetDir()
        {
            // base.characterBody.SetAimTimer(0.02f);
            base.transform.forward = base.GetAimRay().direction;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= durationToPlayWarnSound && !playedWarnSound)
            {
                Util.PlayAttackSpeedSound("Play_clayboss_m2_shoot", gameObject, 1.5f);
                playedWarnSound = true;
            }

            if (fixedAge >= durationToTelegraph && !showedTelegraph)
            {
                // Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_windUp", gameObject, 1.5f);
                AkSoundEngine.PostEvent(Events.Play_jellyfish_detonate_pre, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_jellyfish_detonate_pre, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_vagrant_attack2_charge, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_vagrant_attack2_explode, base.gameObject);

                modelTransform = GetModelTransform();

                if (modelTransform)
                {
                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = 1f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = CannonballJellyfish.matTell;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                    showedTelegraph = true;
                }
            }

            if (!dashedAlready)
            {
                StartAimMode(0.2f);
            }

            if (fixedAge >= durationToAttack && !dashedAlready && base.isAuthority)
            {
                SetDir();

                AkSoundEngine.PostEvent(Events.Play_item_use_fireballDash_explode, base.gameObject);

                dashedAlready = true;

                BaseAI ai = base.characterBody.master.GetComponent<BaseAI>();

                if (ai && ai.currentEnemy.gameObject)
                {
                    float dist = Vector3.Distance(base.transform.position, ai.currentEnemy.gameObject.transform.position);
                    DashForce = dist * 2f;
                }

                PhysForceInfo info = new();
                info.force = base.inputBank.aimDirection * DashForce;
                info.massIsOne = true;
                info.disableAirControlUntilCollision = false;

                base.rigidbodyMotor.ApplyForceImpulse(in info);

                base.FindModelChild("Spray").GetComponent<ParticleSystem>().Play();

                lockVector = base.transform.forward;
                rigidbodyDirection.freezeXRotation = true;
                rigidbodyDirection.freezeYRotation = true;
                rigidbodyDirection.freezeZRotation = true;
            }

            if (dashedAlready && base.isAuthority)
            {
                if (attack.Fire())
                {
                    Util.PlaySound("Play_bison_headbutt_attack_hit", gameObject);
                }

                base.transform.forward = lockVector;
            }

            if (base.fixedAge >= durationToAttack + durationToTelegraph)
            {
                outer.SetNextStateToMain();
            }
        }

        public void FlipComponents()
        {
            VectorPID[] vPids = base.gameObject.GetComponents<VectorPID>();
            QuaternionPID[] qPids = base.gameObject.GetComponents<QuaternionPID>();

            vPids.ToList().ForEach(x => x.enabled = !x.enabled);
            qPids.ToList().ForEach(x => x.enabled = !x.enabled);

            base.rigidbodyMotor.rootMotion = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            FlipComponents();
            rigidbodyDirection.freezeXRotation = false;
            rigidbodyDirection.freezeYRotation = false;
            rigidbodyDirection.freezeZRotation = false;
            base.rigidbodyMotor.rootMotion = Vector3.zero;
            base.rigidbody.velocity = Vector3.zero;
            // Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_windDown", gameObject, 2f);
        }
    }
}