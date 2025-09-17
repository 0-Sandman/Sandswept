using System;
using RoR2.Navigation;

namespace Sandswept.Enemies.ThetaConstruct
{
    [ConfigSection("Enemies :: Theta Construct")]
    public class ThetaConstruct : EnemyBase<ThetaConstruct>
    {
        public static LazyIndex ThetaIndex = new("ThetaConstructBody");
        public static GameObject ThetaShieldEffect;

        [ConfigField("Director Credit Cost", "", 130)]
        public static int directorCreditCost;

        public override DirectorCardCategorySelection family => Paths.FamilyDirectorCardCategorySelection.dccsConstructFamily;
        public override MonsterCategory cat => MonsterCategory.BasicMonsters;

        public override void LoadPrefabs()
        {
            prefab = Main.assets.LoadAsset<GameObject>("ThetaConstructBody.prefab");
            prefabMaster = Main.assets.LoadAsset<GameObject>("ThetaConstructMaster.prefab");
            LanguageAPI.Add(prefab.GetComponent<CharacterBody>().baseNameToken.Replace("_NAME", "_LORE"),
            """
            <style=cMono>
            ========================================
            ====   MyBabel Machine Translator   ====
            ====     [Version 12.45.1.009 ]   ======
            ========================================
            Training... <100000000 cycles>
            Training... <100000000 cycles>
            Training... <51032988 cycles>
            Paused...
            Display partial result? Y/N
            Y
            ================================
            </style>
            Please, gather around children. Let me tell you the story of why I created you: the story of an attack on [CITY]

            Three lemurians are walking through the bustling city, the buildings with eccentric shapes all around them. Small pyramids, pillars, and spheres. There, two young hatchlings, clutching their dolls, accompanied by their mother, walking the streets. One of the hatchlings has been mortified after their bigger sibling told them a scary story.

            "Mother! Mother! W-what if the big scary red guys attack?!" the hatchling asks their mother, tears in their eyes. " 'Scary red things'? Oh! Do you mean the Imps? This city is a holy ground, sweetie, the imps can't hurt us here!"
            Suddenly, a massive portal appears in the market, a looming blackness accented by red. Imps pour out, the overlord watching as havoc is wreaked upon [CITY].

            But--! before any lives are claimed, yellow spheres surround lemurians, and a barrage of small orbs falls upon the invading imps. The sculptures that seemed mere decoration are constructs -- like you -- and come to [CITY]'s aid.

            The mother grabs her children and covers their eyes. One with her hands, the other with her tail. She shushes their cries, and ensuring they remain in the construct's bubble as they flee. Countless imp bodies are strewn about [CITY]. The portal to the [RED PLANE] is completely closed off.

            "F-follow me dearies, I-it'll be OK... T-this is a holy land. This is a holy land!"

            And indeed, it is not long before the constructs purge the city of imps, and the overlord's skull displayed as a centerpiece of the town. Good prevails over evil.
            That is why you, my children, must protect those below -- those innocents who cannot defend themselves.

            Go, and guard them with your souls.
            <style=cMono>
            ================================
            </style>
            """);
            ThetaShieldEffect = Main.assets.LoadAsset<GameObject>("ThetaShieldEffect.prefab");
            var meshRenderer = ThetaShieldEffect.GetComponent<MeshRenderer>();
            var outerMaterial = meshRenderer.materials[0];
            outerMaterial.SetColor("_TintColor", new Color32(34, 74, 46, 255));
            outerMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            outerMaterial.SetFloat("_InvFade", 0.1f);
            outerMaterial.SetFloat("_Boost", 1f);
            outerMaterial.SetFloat("_AlphaBoost", 1f);
            outerMaterial.SetFloat("_AlphaBias", 0f);

            var innerMaterial = meshRenderer.materials[1];
            innerMaterial.SetColor("_TintColor", new Color32(53, 178, 44, 255));
            innerMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            innerMaterial.SetFloat("_InvFade", 1f);
            innerMaterial.SetFloat("_Boost", 1f);
            innerMaterial.SetFloat("_AlphaBoost", 0f);
            innerMaterial.SetFloat("_AlphaBias", 0.397f);

            VFXUtils.AddLight(ThetaShieldEffect, new Color32(14, 255, 0, 255), 12f, 16f);

            // ThetaShieldEffect.GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matMinorConstructShield;
            PrefabAPI.RegisterNetworkPrefab(ThetaShieldEffect);
        }

        public override void PostCreation()
        {
            base.PostCreation();
            List<Stage> stages = new List<DirectorAPI.Stage> {
                Stage.SkyMeadow,
                Stage.SkyMeadowSimulacrum,
                DirectorAPI.Stage.SulfurPools,
                DirectorAPI.Stage.TreebornColony,
                DirectorAPI.Stage.GoldenDieback,
                DirectorAPI.Stage.ArtifactReliquary_AphelianSanctuary_Theme,
                DirectorAPI.Stage.AbandonedAqueduct,
                DirectorAPI.Stage.SirensCall,
            };

            RegisterEnemy(prefab, prefabMaster, stages, MonsterCategory.BasicMonsters);
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
            card.selectionWeight = 1;
            card.spawnCard = csc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            csc.directorCreditCost = directorCreditCost;
            csc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            csc.hullSize = HullClassification.Human;
            csc.nodeGraphType = MapNodeGroup.GraphType.Air;
            csc.sendOverNetwork = true;
            csc.prefab = prefabMaster;
            csc.name = "cscThetaConstruct";
        }

        public override void Modify()
        {
            base.Modify();

            body.baseNameToken.Add("Theta Construct");
            body.portraitIcon = Main.sandsweptHIFU.LoadAsset<Texture2D>("texThetaConstruct.png");

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, CastShieldSkill.instance.skillDef);

            prefab.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(DeathState));
            EntityStateMachine.FindByCustomName(prefab, "Body").initialStateType = new(typeof(SpawnState));
        }

        public override void SetUpIDRS()
        {
            AddDisplayRule(Paths.EquipmentDef.EliteFireEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.36739F, -0.03842F, 0.04068F),
                localAngles = new Vector3(15.63205F, 249.5763F, 226.0409F),
                localScale = new Vector3(0.75968F, 0.75968F, 0.75968F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.35483F, 0.00219F, 0F),
                localAngles = new Vector3(10.07104F, 110.0094F, 184.1571F),
                localScale = new Vector3(0.16054F, 0.16054F, 0.16054F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.68181F, 1.12509F, -0.00002F),
                localAngles = new Vector3(280.623F, 90.99331F, 181.8165F),
                localScale = new Vector3(1.84967F, 1.84967F, 1.84967F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.00157F, 0.22454F, -0.19173F),
                localAngles = new Vector3(274.1475F, 184.8704F, 355.4697F),
                localScale = new Vector3(0.30225F, 0.30225F, 0.30225F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.15667F, -0.03144F, -0.00803F),
                localAngles = new Vector3(0.34901F, 279.1591F, 179.7042F),
                localScale = new Vector3(0.38139F, 0.38139F, 0.38139F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.00821F, 1.33318F, -0.00002F),
                localAngles = new Vector3(90F, 0F, 0F),
                localScale = new Vector3(1.55002F, 1.54702F, 1.55002F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.10183F, -0.2142F, -0.81232F),
                localAngles = new Vector3(353.1712F, 278.42F, 90.78405F),
                localScale = new Vector3(0.79894F, 0.79894F, 0.79894F),
                limbMask = LimbFlags.None,
                followerPrefab = Paths.GameObject.DisplayEliteHorn
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.97177F, 0.35453F, -0.33704F),
                localAngles = new Vector3(36.10262F, 176.0289F, 49.29921F),
                localScale = new Vector3(0.07252F, 0.07252F, 0.07252F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadSpike,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.00169F, 0.49371F, 0.12895F),
                localAngles = new Vector3(63.14748F, 275.4161F, 187.0335F),
                localScale = new Vector3(3.80658F, 3.80658F, 3.80658F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.00276F, 1.10564F, 0.02518F),
                localAngles = new Vector3(359.7713F, 40.45325F, 269.915F),
                localScale = new Vector3(1.03379F, 1.03379F, 1.03379F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.11136F, 0.17698F, -0.00342F),
                localAngles = new Vector3(0.64661F, 258.1166F, 4.47902F),
                localScale = new Vector3(4.29187F, 4.29187F, 4.29187F),
                followerPrefab = Elites.Motivating.Crown,
                limbMask = LimbFlags.None
            });

            CollapseIDRS();
        }
    }

    public class ThetaShieldController : MonoBehaviour
    {
        public GenericOwnership targetHolder;
        public GenericOwnership ownerHolder;
        internal GameObject owner;
        internal GameObject target;
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

            Util.PlaySound("Play_loader_R_active_loop", gameObject);

            if (owner)
            {
                ownerBody = owner.GetComponent<CharacterBody>();

                GetComponent<LineBetweenTransforms>()._transformNodes[1] = ownerBody.transform;

                var loc2 = ownerBody.GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>();

                p1 = loc2.FindChild("PrismL");
                p2 = loc2.FindChild("PrismR");
                p3 = loc2.FindChild("PrismT");

                Util.PlaySound("Play_loader_R_active_loop", owner);
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

                    targetBody.SetBuffCount(Buffs.ThetaBoost.instance.BuffDef.buffIndex, targetBody.GetBuffCount(Buffs.ThetaBoost.instance.BuffDef) + 1);
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

                void Handle(ref Transform p, ref Transform t, ref Vector3 c, ref Vector3 r)
                {
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

                targetBody.SetBuffCount(Buffs.ThetaBoost.instance.BuffDef.buffIndex, targetBody.GetBuffCount(Buffs.ThetaBoost.instance.BuffDef) - 1);
            }

            if (owner)
            {
                Util.PlaySound("Stop_loader_R_active_loop", owner);
            }

            Util.PlaySound("Stop_loader_R_active_loop", gameObject);
        }
    }
}