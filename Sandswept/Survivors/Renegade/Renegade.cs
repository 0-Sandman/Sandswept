
using System;

namespace Sandswept.Survivors.Renegade
{
    public class Renegade : SurvivorBase<Renegade>
    {
        public override string Name => "Renegade";

        public override string Description => "mog";

        public override string Subtitle => "level 4 mog";

        public override string Outro => "sigma";

        public override string Failure => "the ever so helpful hive";
        public static DamageAPI.ModdedDamageType ShrapnelBullet = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType GravSlam = DamageAPI.ReserveDamageType();

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.LoaderBody, "RenegadeBody");
            Master = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.LoaderMonsterMaster, "RenegadeMaster");

            Body.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.Bandit2Crosshair;

            GameObject DisplayPrefab = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.LoaderDisplay, "RenegadeDisplay", false);

            ModelSkinController controller = DisplayPrefab.GetComponentInChildren<ModelSkinController>();
            if (controller)
            {
                GameObject.Destroy(controller);
            }

            controller = Body.GetComponentInChildren<ModelSkinController>();
            if (controller)
            {
                GameObject.Destroy(controller);
            }

            CharacterBody body = Body.GetComponent<CharacterBody>();
            body.baseNameToken = "SANDSWEPT_SURVIVOR_RENEGADE_NAME";
            body.subtitleNameToken = "SANDSWEPT_SURVIVOR_RENEGADE_SUBTITLE";
            body.portraitIcon = null;
            body.bodyColor = Color.red;

            CharacterMaster master = Master.GetComponent<CharacterMaster>();
            master.bodyPrefab = Body;

            SurvivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            (SurvivorDef as ScriptableObject).name = "sdRenegade";
            SurvivorDef.bodyPrefab = Body;
            SurvivorDef.descriptionToken = "SANDSWEPT_SURVIVOR_RENEGADE_DESCRIPTION";
            SurvivorDef.outroFlavorToken = "SANDSWEPT_SURVIVOR_RENEGADE_OUTRO";
            SurvivorDef.mainEndingEscapeFailureFlavorToken = "SANDSWEPT_SURVIVOR_RENEGADE_FAIL";
            SurvivorDef.displayPrefab = DisplayPrefab;
            SurvivorDef.displayNameToken = body.baseNameToken;
            SurvivorDef.desiredSortPosition = 20;
            // hidden hide hid h
            SurvivorDef.hidden = true;

            SwapMaterials(Body, Utils.Assets.Material.matVoidBubble, true);
            SwapMaterials(DisplayPrefab, Utils.Assets.Material.matVoidBubble, true);

            SerializableEntityStateType idle = new(typeof(Idle));

            SkillLocator locator = Body.GetComponent<SkillLocator>();

            On.RoR2.BulletAttack.FireSingle += HandleShrapnel;
            GlobalEventManager.onServerDamageDealt += HandleGravSlam;

            Body.AddComponent<RenegadeComboController>();

            ReplaceSkills(locator.primary, Skills.Revolver.instance.skillDef);
            ReplaceSkills(locator.secondary, Skills.Crush.instance.skillDef);
            ReplaceSkills(locator.utility, Skills.Charge.instance.skillDef);
            ReplaceSkills(locator.special, Skills.Slam.instance.skillDef);
        }

        private void HandleGravSlam(DamageReport report)
        {
            if (report.damageInfo.HasModdedDamageType(GravSlam))
            {
                if (!report.victimBody) return;
                CharacterBody victim = report.victimBody;

                PhysForceInfo upForce = new PhysForceInfo()
                {
                    massIsOne = false,
                    force = Vector3.upVector * 4000f
                };

                PhysForceInfo downForce = new PhysForceInfo()
                {
                    massIsOne = true,
                    force = Vector3.upVector * -40f
                };

                if (victim.TryGetComponent<CharacterMotor>(out var motor))
                {
                    if (motor.isGrounded)
                    {
                        motor.ApplyForceImpulse(in upForce);
                    }
                    else
                    {
                        motor.ApplyForceImpulse(in downForce);
                    }
                }
                else if (victim.TryGetComponent<RigidbodyMotor>(out var motor2))
                {
                    motor2.ApplyForceImpulse(in downForce);
                }
            }
        }

        private void HandleShrapnel(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, Vector3 normal, int muzzleIndex)
        {
            if (self.HasModdedDamageType(ShrapnelBullet))
            {
                self.weapon = null;
            }

            orig(self, normal, muzzleIndex);
        }
    }
}
