using System;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class DeltaConstruct : EnemyBase<DeltaConstruct>
    {
        public static GameObject bolt;
        public static GameObject muzzleFlash;
        public static GameObject beam;
        public static Material matDeltaBeamStrong;
        public static GameObject DeltaBurnyTrail;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("DeltaConstructBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("DeltaConstructMaster.prefab");

            bolt = Paths.GameObject.MinorConstructProjectile;
            muzzleFlash = Paths.GameObject.MuzzleflashMinorConstruct;

            beam = Main.Assets.LoadAsset<GameObject>("DeltaBeam.prefab");
            matDeltaBeamStrong = Main.Assets.LoadAsset<Material>("matDeltaBeamStrong.mat");

            DeltaBurnyTrail = Main.Assets.LoadAsset<GameObject>("DeltaBurnyTrail.prefab");
            ContentAddition.AddNetworkedObject(DeltaBurnyTrail);
        }

        public override void PostCreation()
        {
            base.PostCreation();

            RegisterEnemy(prefab, prefabMaster);

            // prefab.GetComponent<ModelLocator>()._modelTransform.AddComponent<ShittyDebugComp>();
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;

            body.baseNameToken.Add("Delta Construct");
            body.portraitIcon = Main.hifuSandswept.LoadAsset<Texture>("texDeltaConstruct.png");

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, FireBoltsSkill.instance.skillDef);
            ReplaceSkill(loc.secondary, SkystrikeSkill.instance.skillDef);
        }

        public class ShittyDebugComp : MonoBehaviour
        {
            public Animator anim;

            public void Start()
            {
                anim = GetComponent<Animator>();
            }

            public void Update()
            {
                Debug.Log("yaw: " + anim.GetFloat("aimYawCycle"));
                Debug.Log("pitch: " + anim.GetFloat("aimPitchCycle"));
            }
        }
    }
}