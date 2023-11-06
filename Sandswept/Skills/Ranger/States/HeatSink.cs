using RoR2.Projectile;
using Sandswept.Components;
using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.States.Ranger
{
    public class HeatSink : BaseState
    {
        public static float damageCoefficient = 5f;
        public static float BaseDuration = 0.5f;
        private float duration;
        private RangerHeatManager heat;
        private Transform modelTransform;
        public static Material overlayMat1 = HeatSinkVFX.dashMat1;
        public static Material overlayMat2 = HeatSinkVFX.dashMat2;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatManager>();

            duration = BaseDuration / attackSpeedStat;

            FireNova();

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.9f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat1;
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());

                var temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 1f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = overlayMat2;
                temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }
            /*
            DamageInfo info = new()
            {
                attacker = null,
                procCoefficient = 0,
                damage = healthComponent.fullCombinedHealth * 0.2f,
                crit = false,
                position = transform.position,
                damageColorIndex = DamageColorIndex.Fragile,
                damageType = DamageType.BypassArmor | DamageType.BypassBlock | DamageType.NonLethal
            };

            if (NetworkServer.active)
            {
                healthComponent.TakeDamage(info);
            }
            */
            heat.CurrentHeat = 0f;

            PlayAnimation("Body", "Twirl");
        }

        public void FireNova()
        {
            EffectManager.SpawnEffect(Assets.GameObject.MolotovClusterIgniteExplosionVFX, new EffectData
            {
                origin = transform.position,
                scale = 16f,
                rotation = Quaternion.identity
            }, true);

            Util.PlaySound("Play_magmaWorm_death_small_explos", gameObject);
            Util.PlaySound("Play_item_proc_igniteOnKill", gameObject);
            Util.PlaySound("Play_clayboss_m2_explo", gameObject);

            if (isAuthority)
                new BlastAttack()
                {
                    attacker = gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = damageStat * Util.Remap(heat.CurrentHeat, 0f, 100f, 3f, 9f),
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
            heat.isUsingHeatSignature = false;

            if (modelTransform)
            {
                foreach (TemporaryOverlay overlay in modelTransform.GetComponents<TemporaryOverlay>())
                {
                    Object.Destroy(overlay);
                }
            }
        }
    }
}