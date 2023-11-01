using RoR2.Projectile;
using Sandswept.Components;
using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.States.Ranger
{
    public class HeatSignature : BaseState
    {
        public static float damageCoefficient = 3f;
        public static float MinimumStateDuration = 1f;
        private RangerHeatManager heat;
        private Transform modelTransform;
        public static Material overlayMat1 = HeatSignatureVFX.dashMat1;
        public static Material overlayMat2 = HeatSignatureVFX.dashMat2;
        private bool hasLeftState = false;
        private static GameObject HeatSignatureTrailPrefab;
        private GameObject trailInstance;

        static HeatSignature() {
            HeatSignatureTrailPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.FireTrail, "RangerHeatTrail");
            DamageTrail trail = HeatSignatureTrailPrefab.GetComponent<DamageTrail>();
            trail.radius = 2f;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatManager>();

            heat.isUsingHeatSignature = true;

            if (characterBody && NetworkServer.active)
            {
                characterBody.AddBuff(Buffs.HeatSignatureBuff.instance.BuffDef);
            }

            modelTransform = GetModelTransform();

            trailInstance = GameObject.Instantiate(HeatSignatureTrailPrefab, base.transform);
            DamageTrail trail = trailInstance.GetComponent<DamageTrail>();
            trail.damagePerSecond = base.damageStat * 3f;
            trail.owner = base.gameObject;

            // spawn fire trail
            // I think it's hardcoded to do 150% damage though, and might not even work if you aren't blazing
            // hopoo games ! !

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
            }

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= MinimumStateDuration)
            {
                if (!hasLeftState && inputBank.skill3.justPressed)
                {
                    hasLeftState = true;

                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            heat.isUsingHeatSignature = false;

            if (characterBody && NetworkServer.active)
            {
                characterBody.RemoveBuff(Buffs.HeatSignatureBuff.instance.BuffDef);
            }
            if (modelTransform)
            {
                foreach (TemporaryOverlay overlay in modelTransform.GetComponents<TemporaryOverlay>())
                {
                    Object.Destroy(overlay);
                }
            }

            GameObject.Destroy(trailInstance);

            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
            Util.PlaySound("Play_fireballsOnHit_impact", gameObject);
        }
    }
}