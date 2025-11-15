using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Special
{
    public class HeatSink : BaseState
    {
        public static float damageCoefficient = 3f;
        public static float baseDuration = 0.5f;
        private float duration;
        private float finalDamageCoefficient;
        private RangerHeatController heat;
        private Transform modelTransform;
        private Material overlayMat1;
        private Material overlayMat2;
        private GameObject explosion1;
        private GameObject explosion2;

        private TemporaryOverlayInstance tempOverlayInstance1;
        private TemporaryOverlayInstance tempOverlayInstance2;
        private float secondaryBlastMultiplier = 0.75f;
        private float secondaryBlastRadiusMult = 0.75f;
        private float intervalBetweenBlasts;
        private int totalBlasts;
        private float multiAttackStopwatch = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            if (characterMotor)
            {
                SmallHop(characterMotor, 20f);
            }

            int stock = base.skillLocator.special.maxStock;
            base.skillLocator.special.DeductStock(base.skillLocator.special.maxStock);

            heat = GetComponent<RangerHeatController>();

            finalDamageCoefficient = damageCoefficient + (heat.currentHeat / 16.6666f); // makes damage go from 300% to 900% at 100% heat, 1500% at 200% heat, etc

            duration = baseDuration / attackSpeedStat;

            totalBlasts = stock - 1;

            intervalBetweenBlasts = duration / totalBlasts;

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat1 = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => HeatSinkVFX.explodeMat1Major,
                    "RANGER_SKIN_RENEGADE_NAME" => HeatSinkVFX.explodeMat1Renegade,
                    "RANGER_SKIN_MILEZERO_NAME" => HeatSinkVFX.explodeMat1MileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => HeatSinkVFX.explodeMat1Sandswept,
                    _ => HeatSinkVFX.explodeMat1Default
                };

                overlayMat2 = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => HeatSinkVFX.explodeMat2Major,
                    "RANGER_SKIN_RENEGADE_NAME" => HeatSinkVFX.explodeMat2Renegade,
                    "RANGER_SKIN_MILEZERO_NAME" => HeatSinkVFX.explodeMat2MileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => HeatSinkVFX.explodeMat2Sandswept,
                    _ => HeatSinkVFX.explodeMat2Default
                };

                explosion1 = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => HeatSinkVFX.explosion1Major,
                    "RANGER_SKIN_RENEGADE_NAME" => HeatSinkVFX.explosion1Renegade,
                    "RANGER_SKIN_MILEZERO_NAME" => HeatSinkVFX.explosion1MileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => HeatSinkVFX.explosion1Sandswept,
                    _ => HeatSinkVFX.explosion1Default
                };

                explosion2 = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => HeatSinkVFX.explosion2Major,
                    "RANGER_SKIN_RENEGADE_NAME" => HeatSinkVFX.explosion2Renegade,
                    "RANGER_SKIN_MILEZERO_NAME" => HeatSinkVFX.explosion2MileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => HeatSinkVFX.explosion2Sandswept,
                    _ => HeatSinkVFX.explosion2Default
                };

                var characterModel = modelTransform.GetComponent<CharacterModel>();

                tempOverlayInstance1 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                tempOverlayInstance1.duration = 9999f;
                tempOverlayInstance1.animateShaderAlpha = true;
                tempOverlayInstance1.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                tempOverlayInstance1.destroyComponentOnEnd = true;
                tempOverlayInstance1.originalMaterial = overlayMat1;
                tempOverlayInstance1.inspectorCharacterModel = characterModel;

                tempOverlayInstance2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                tempOverlayInstance2.duration = 9999f;
                tempOverlayInstance2.animateShaderAlpha = true;
                tempOverlayInstance2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                tempOverlayInstance2.destroyComponentOnEnd = true;
                tempOverlayInstance2.originalMaterial = overlayMat2;
                tempOverlayInstance2.inspectorCharacterModel = characterModel;
            }

            FireNova(false);

            /*
            if (characterBody)
            {
                characterBody.AddTimedBuffAuthority(HeatAttackSpeedBoost.instance.BuffDef.buffIndex, 0.05f * Mathf.Pow(heat.currentHeat / 1.51991108f, 1.1f));
            }
            */

            heat.currentHeat = 0f;
        }

        public void FireNova(bool secondary = false)
        {
            // FEAR

            float radius = 16f;
            if (secondary) radius *= secondaryBlastRadiusMult;

            EffectManager.SpawnEffect(explosion2, new EffectData
            {
                origin = transform.position,
                scale = radius,
                rotation = Quaternion.identity
            }, true);

            EffectManager.SpawnEffect(explosion1, new EffectData
            {
                origin = transform.position,
                scale = radius,
                rotation = Quaternion.identity
            }, true);

            Util.PlaySound("Play_magmaWorm_death_small_explos", gameObject);
            Util.PlaySound("Play_item_proc_igniteOnKill", gameObject);
            Util.PlaySound("Play_clayboss_m2_explo", gameObject);
            Util.PlaySound("Play_roboBall_death_small_explo", gameObject);
            Util.PlaySound("Play_voidRaid_m1_explode", gameObject);
            Util.PlaySound("Play_captain_shift_impact", gameObject);

            if (!secondary) PlayAnimation("FullBody, Override", "Twirl");

            if (isAuthority)
            {
                var attack = new BlastAttack()
                {
                    attacker = gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = damageStat * finalDamageCoefficient,
                    baseForce = 4000f,
                    bonusForce = Vector3.zero,
                    canRejectForce = true,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Fragile,
                    damageType = DamageType.IgniteOnHit,
                    inflictor = gameObject,
                    radius = radius,
                    position = transform.position,
                    procChainMask = default,
                    procCoefficient = 1f,
                    teamIndex = characterBody.teamComponent.teamIndex,
                    losType = BlastAttack.LoSType.None,
                    falloffModel = BlastAttack.FalloffModel.None,
                };

                if (secondary)
                {
                    attack.baseDamage *= secondaryBlastMultiplier;
                }

                attack.damageType.damageSource = DamageSource.Special;

                attack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (totalBlasts > 0)
            {
                multiAttackStopwatch += Time.fixedDeltaTime;

                if (multiAttackStopwatch >= intervalBetweenBlasts)
                {
                    multiAttackStopwatch = 0f;
                    FireNova(true);
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

            if (modelTransform)
            {
                TemporaryOverlayManager.RemoveOverlay(tempOverlayInstance1.managerIndex);
                TemporaryOverlayManager.RemoveOverlay(tempOverlayInstance2.managerIndex);
            }
        }
    }
}