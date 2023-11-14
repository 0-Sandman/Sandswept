using RoR2.Projectile;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States
{
    public class HeatSignature : BaseState
    {
        public static float Duration = 0.2f;
        public static float BuffDuration = 1f;
        public static float SpeedCoefficient = 6f;
        private RangerHeatController heat;
        private Vector3 stepVector;
        private Transform modelTransform;
        public static Material overlayMat1 = HeatSignatureVFX.dashMat1;
        public static Material overlayMat2 = HeatSignatureVFX.dashMat2;
        private OverlapAttack attack;

        public override void OnEnter()
        {
            base.OnEnter();

            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                    characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            heat = GetComponent<RangerHeatController>();

            modelTransform = GetModelTransform();

            HitBoxGroup hitBoxGroup = null;

            if (modelTransform)
            {
                var overlay1 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                overlay1.duration = 999f;
                overlay1.animateShaderAlpha = true;
                overlay1.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay1.destroyComponentOnEnd = true;
                overlay1.originalMaterial = overlayMat1;
                overlay1.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                var overlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                overlay2.duration = 999f;
                overlay2.animateShaderAlpha = true;
                overlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay2.destroyComponentOnEnd = true;
                overlay2.originalMaterial = overlayMat2;
                overlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (x) => x.groupName == "GaySex");
            }

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlayAttackSpeedSound("Play_commando_shift", gameObject, 1.2f);

            stepVector = inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector;

            PlayAnimation("Body", "Twirl");

            attack = new()
            {
                attacker = gameObject,
                inflictor = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                pushAwayForce = 500f,
                damage = damageStat * 3.5f,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Stun1s,
                procCoefficient = 1f,
                isCrit = RollCrit(),
                procChainMask = default,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                impactSound = Assets.NetworkSoundEventDef.nsePulverizeBuildupBuffApplied.index,
                forceVector = Vector3.zero,
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = ExhaustVFX.impactPrefab
            };

            attack.AddModdedDamageType(Projectiles.DirectCurrent.chargeOnHitDash);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += stepVector * (moveSpeedStat * SpeedCoefficient * Time.fixedDeltaTime);
            }

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }

            attack.Fire(null);

            if (fixedAge >= Duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                    characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (modelTransform)
            {
                foreach (TemporaryOverlay overlay in modelTransform.GetComponents<TemporaryOverlay>())
                {
                    Object.Destroy(overlay);
                }
            }

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
        }
    }
}