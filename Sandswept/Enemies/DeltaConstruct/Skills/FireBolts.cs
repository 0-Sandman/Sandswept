using System;
using System.Collections;
using Sandswept.Survivors;

namespace Sandswept.Enemies.DeltaConstruct
{
    [ConfigSection("Enemies :: Delta Construct")]
    public class FireBolts : BaseSkillState
    {
        public float damageCoeff = 2f;
        public float duration = 1.6f;
        public float initDelay;
        public float secondDelay;
        public Transform[] front;
        public Transform[] back;
        private Transform modelTransform;

        [ConfigField("Bolt Projectile Damage", "Decimal.", 2f)]
        public static float projectileDamage;

        public override void OnEnter()
        {
            base.OnEnter();

            damageCoeff = projectileDamage;

            duration /= base.attackSpeedStat;

            // base.characterMotor.walkSpeedPenaltyCoefficient = 0f;

            modelTransform = GetModelTransform();

            initDelay = duration * 0.3f;
            secondDelay = (duration - initDelay) * 0.3f;

            back = [FindModelChild("Muzzle1"), FindModelChild("Muzzle2")];
            front = [FindModelChild("Muzzle3"), FindModelChild("Muzzle4")];

            base.characterBody.StartCoroutine(HandleBolts());

            base.StartAimMode(initDelay);
        }

        public override void OnExit()
        {
            base.OnExit();

            // base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.StartAimMode(initDelay);

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public IEnumerator HandleBolts()
        {
            Util.PlaySound("Play_minorConstruct_attack_chargeUp", base.gameObject);
            Util.PlaySound("Play_minorConstruct_attack_chargeUp", base.gameObject);

            yield return new WaitForSeconds(0.15f);

            ShowTelegraph(initDelay);

            yield return new WaitForSeconds(initDelay - 0.15f);

            PlayAnimation("Gesture, Override", "Fire Cannons", "Generic.playbackRate", secondDelay);

            for (int i = 0; i < front.Length; i++)
            {
                FireBolt(front[i]);
            }

            yield return new WaitForSeconds(secondDelay);

            PlayAnimation("Gesture, Override", "Fire Cannons", "Generic.playbackRate", secondDelay);

            for (int i = 0; i < back.Length; i++)
            {
                FireBolt(back[i]);
            }
        }

        public void ShowTelegraph(float duration)
        {

            if (modelTransform)
            {
                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = duration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = DeltaConstruct.matTell;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }
        }

        public void FireBolt(Transform t)
        {
            Quaternion rot = Quaternion.LookRotation(t.up);

            if (Util.CharacterRaycast(base.gameObject, base.GetAimRay(), out RaycastHit hinfo, 400f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
            {
                rot = Util.QuaternionSafeLookRotation((hinfo.point - t.position).normalized);
            }

            FireProjectileInfo info = new();
            info.crit = base.RollCrit();
            info.damage = base.damageStat * damageCoeff;
            info.rotation = rot;
            info.position = t.position;
            info.owner = base.gameObject;
            info.projectilePrefab = DeltaConstruct.bolt;

            if (NetworkServer.active)
            {
                ProjectileManager.instance.FireProjectile(info);
            }

            EffectManager.SpawnEffect(DeltaConstruct.muzzleFlash, new EffectData
            {
                rotation = Util.QuaternionSafeLookRotation(t.up),
                origin = t.position
            }, false);

            Util.PlaySound("Play_minorConstruct_attack_shoot", base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class FireBoltsSkill : SkillBase<FireBoltsSkill>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(FireBolts);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 1.5f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}