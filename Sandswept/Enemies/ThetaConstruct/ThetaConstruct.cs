using System;

namespace Sandswept.Enemies.ThetaConstruct {
    public class ThetaConstruct : EnemyBase<ThetaConstruct>
    {
        public static GameObject ThetaShieldEffect;
        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("ThetaConstructBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("ThetaConstructMaster.prefab");

            ThetaShieldEffect = Main.Assets.LoadAsset<GameObject>("ThetaShieldEffect.prefab");
            ThetaShieldEffect.GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matMinorConstructShield;
        }

        public override void PostCreation()
        {
            base.PostCreation();
            RegisterEnemy(prefab, prefabMaster, null);
        }

        public override void Modify()
        {
            base.Modify();

            body.baseNameToken.Add("Theta Construct");

            SkillLocator loc = body.GetComponent<SkillLocator>();
            
            ReplaceSkill(loc.primary, CastShieldSkill.instance.skillDef);
        }
    }

    public class ThetaShieldController : MonoBehaviour
    {
        public GenericOwnership targetHolder;
        public GenericOwnership ownerHolder;
        private GameObject owner;
        private GameObject target;
        private CharacterBody ownerBody;
        private CharacterBody targetBody;
        private Animator anim;
        private Transform p1, p1t, p2, p2t, p3, p3t;
        private Vector3 p1c, p2c, p3c = Vector3.zero;
        private Vector3 p1r, p2r, p3r = Vector3.zero;
        private List<BuffIndex> activeEliteBuffs = new();
        public void Start()
        {
            owner = ownerHolder.ownerObject;
            target = targetHolder.ownerObject;

            var loc = GetComponent<ChildLocator>();

            p1t = loc.FindChild("PrismL");
            p2t = loc.FindChild("PrismR");
            p3t = loc.FindChild("PrismT");

            if (owner)
            {
                ownerBody = owner.GetComponent<CharacterBody>();

                GetComponent<LineBetweenTransforms>()._transformNodes[1] = ownerBody.transform;

                var loc2 = ownerBody.GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>();

                p1 = loc2.FindChild("PrismL");
                p2 = loc2.FindChild("PrismR");
                p3 = loc2.FindChild("PrismT");
            }

            if (target)
            {
                targetBody = target.GetComponent<CharacterBody>();
                base.transform.localScale *= targetBody.bestFitRadius * 1.5f;

                if (NetworkServer.active)
                {
                    if (ownerBody && ownerBody.isElite)
                    {
                        for (int i = 0; i < ownerBody.activeBuffsList.Length; i++)
                        {
                            var buff = ownerBody.activeBuffsList[i];

                            BuffDef bd = BuffCatalog.GetBuffDef(buff);

                            if (bd.isElite && !targetBody.HasBuff(buff))
                            {
                                activeEliteBuffs.Add(buff);
                                targetBody.SetBuffCount(buff, 1);
                            }
                        }
                    }

                    targetBody.SetBuffCount(Buffs.ThetaBoost.instance.BuffDef.buffIndex, 1);
                }

            }

            if (!owner || !target || !ownerBody.healthComponent.alive || !targetBody.healthComponent.alive)
            {
                Destroy(base.gameObject);
            }
        }

        public void LateUpdate()
        {
            if (owner)
            {
                Handle(ref p1, ref p1t, ref p1c, ref p1r);
                Handle(ref p2, ref p2t, ref p2c, ref p2r);
                Handle(ref p3, ref p3t, ref p3c, ref p3r);

                void Handle(ref Transform p, ref Transform t, ref Vector3 c, ref Vector3 r) {
                    if (c == Vector3.zero) c = p.position;
                    p.position = c;
                    p.position = Vector3.MoveTowards(p.position, t.position, 90f * Time.fixedDeltaTime);
                    c = p.position;
                    if (r == Vector3.zero) r = p.up;
                    p.up = r;
                    p.up = Vector3.RotateTowards(p.up, t.up, 40f * Time.fixedDeltaTime, 1f);
                    r = p.up;
                }
            }
        }

        public void FixedUpdate()
        {
            if (target)
            {
                base.transform.position = Vector3.MoveTowards(base.transform.position, targetBody.corePosition, 90f * Time.fixedDeltaTime);
            }

            if (!owner || !target)
            {
                Destroy(base.gameObject);
            }
        }

        public void OnDestroy()
        {
            if (targetBody)
            {
                if (activeEliteBuffs.Count > 0)
                {
                    foreach (BuffIndex buff in activeEliteBuffs)
                    {
                        targetBody.SetBuffCount(buff, 0);
                    }
                }

                targetBody.SetBuffCount(Buffs.ThetaBoost.instance.BuffDef.buffIndex, 0);
            }
        }
    }
}