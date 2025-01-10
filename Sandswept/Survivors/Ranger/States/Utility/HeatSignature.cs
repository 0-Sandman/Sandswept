using RoR2.Projectile;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Utility
{
    public class HeatSignature : BaseState
    {
        public float duration = 0.2f;
        public static float BuffDuration = 1f;
        public static float SpeedCoefficient = 6f;
        private Vector3 stepVector;
        private Transform modelTransform;
        private Material overlayMat1;
        private Material overlayMat2;
        private OverlapAttack attack;

        public override void OnEnter()
        {
            base.OnEnter();

            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                {
                    characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
            }

            modelTransform = GetModelTransform();

            HitBoxGroup hitBoxGroup = null;

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat1 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSignatureVFX.heatDashMat1Major,
                    "SKINDEF_RENEGADE" => HeatSignatureVFX.heatDashMat1Renegade,
                    "SKINDEF_MILEZERO" => HeatSignatureVFX.heatDashMat1MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSignatureVFX.heatDashMat1Sandswept,
                    _ => HeatSignatureVFX.heatDashMat1Default
                };

                overlayMat2 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSignatureVFX.heatDashMat2Major,
                    "SKINDEF_RENEGADE" => HeatSignatureVFX.heatDashMat2Renegade,
                    "SKINDEF_MILEZERO" => HeatSignatureVFX.heatDashMat2MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSignatureVFX.heatDashMat2Sandswept,
                    _ => HeatSignatureVFX.heatDashMat2Default
                };

                var overlay1 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                overlay1.duration = 0.4f;
                overlay1.animateShaderAlpha = true;
                overlay1.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay1.destroyComponentOnEnd = true;
                overlay1.originalMaterial = overlayMat1;
                overlay1.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                var overlay2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                overlay2.duration = 0.5f;
                overlay2.animateShaderAlpha = true;
                overlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay2.destroyComponentOnEnd = true;
                overlay2.originalMaterial = overlayMat2;
                overlay2.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (x) => x.groupName == "GaySex");
            }

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlayAttackSpeedSound("Play_commando_shift", gameObject, 1.2f);

            stepVector = inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector;

            PlayAnimation("FullBody, Override", "Twirl");

            attack = new()
            {
                attacker = gameObject,
                inflictor = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                pushAwayForce = 1500f,
                damage = damageStat * 2.5f,
                damageColorIndex = DamageColorIndex.Fragile,
                damageType = DamageType.Stun1s,
                procCoefficient = 1f,
                isCrit = RollCrit(),
                procChainMask = default,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                impactSound = Paths.NetworkSoundEventDef.nsePulverizeBuildupBuffApplied.index,
                forceVector = Vector3.zero,
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = ExhaustVFX.impactPrefabDefault
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

            if (isAuthority)
            {
                if (attack.Fire(null))
                {
                    duration += 0.2f;
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

            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                {
                    characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
            }

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
        }
    }
}