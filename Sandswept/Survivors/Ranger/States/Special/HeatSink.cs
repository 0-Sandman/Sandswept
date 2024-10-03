using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Special
{
    public class HeatSink : BaseState
    {
        public static float damageCoefficient = 5f;
        public static float BaseDuration = 0.5f;
        private float duration;
        private RangerHeatController heat;
        private Transform modelTransform;
        private Material overlayMat1;
        private Material overlayMat2;
        private GameObject explosion1;
        private GameObject explosion2;

        private TemporaryOverlayInstance tempOverlayInstance1;
        private TemporaryOverlayInstance tempOverlayInstance2;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatController>();

            duration = BaseDuration / attackSpeedStat;

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat1 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSinkVFX.explodeMat1Major,
                    "SKINDEF_RENEGADE" => HeatSinkVFX.explodeMat1Renegade,
                    "SKINDEF_MILEZERO" => HeatSinkVFX.explodeMat1MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSinkVFX.explodeMat1Sandswept,
                    _ => HeatSinkVFX.explodeMat1Default
                };

                overlayMat2 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSinkVFX.explodeMat2Major,
                    "SKINDEF_RENEGADE" => HeatSinkVFX.explodeMat2Renegade,
                    "SKINDEF_MILEZERO" => HeatSinkVFX.explodeMat2MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSinkVFX.explodeMat2Sandswept,
                    _ => HeatSinkVFX.explodeMat2Default
                };

                explosion1 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSinkVFX.explosion1Major,
                    "SKINDEF_RENEGADE" => HeatSinkVFX.explosion1Renegade,
                    "SKINDEF_MILEZERO" => HeatSinkVFX.explosion1MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSinkVFX.explosion1Sandswept,
                    _ => HeatSinkVFX.explosion1Default
                };

                explosion2 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => HeatSinkVFX.explosion2Major,
                    "SKINDEF_RENEGADE" => HeatSinkVFX.explosion2Renegade,
                    "SKINDEF_MILEZERO" => HeatSinkVFX.explosion2MileZero,
                    "SKINDEF_SANDSWEPT" => HeatSinkVFX.explosion2Sandswept,
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

            FireNova();

            if (characterBody)
            {
                characterBody.AddTimedBuffAuthority(HeatAttackSpeedBoost.instance.BuffDef.buffIndex, 0.05f * Mathf.Pow(heat.currentHeat / 1.51991108f, 1.1f));
            }

            heat.currentHeat = 0f;
        }

        public void FireNova()
        {
            // FEAR

            EffectManager.SpawnEffect(explosion2, new EffectData
            {
                origin = transform.position,
                scale = 16f,
                rotation = Quaternion.identity
            }, true);

            EffectManager.SpawnEffect(explosion1, new EffectData
            {
                origin = transform.position,
                scale = 16f,
                rotation = Quaternion.identity
            }, true);

            Util.PlaySound("Play_magmaWorm_death_small_explos", gameObject);
            Util.PlaySound("Play_item_proc_igniteOnKill", gameObject);
            Util.PlaySound("Play_clayboss_m2_explo", gameObject);
            Util.PlaySound("Play_roboBall_death_small_explo", gameObject);
            Util.PlaySound("Play_voidRaid_m1_explode", gameObject);
            Util.PlaySound("Play_captain_shift_impact", gameObject);

            PlayAnimation("FullBody, Override", "Twirl");

            if (isAuthority)
                new BlastAttack()
                {
                    attacker = gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = damageStat * Util.Remap(heat.currentHeat, 0f, 100f, 3f, 9f),
                    baseForce = 1500f,
                    bonusForce = Vector3.zero,
                    canRejectForce = true,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Fragile,
                    damageType = DamageType.IgniteOnHit,
                    inflictor = gameObject,
                    radius = 16f,
                    position = transform.position,
                    procChainMask = default,
                    procCoefficient = 1f,
                    teamIndex = characterBody.teamComponent.teamIndex,
                    losType = BlastAttack.LoSType.None,
                    falloffModel = BlastAttack.FalloffModel.None,
                }.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextState(new OverdriveExitHeatSink());
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