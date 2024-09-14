using HG;
using System.Linq;

namespace Sandswept.Elites
{
    internal class Motivating : EliteEquipmentBase<Motivating>
    {
        public override string EliteEquipmentName => "John Hopoo";

        public override string EliteAffixToken => "MOTIVATING";

        public override string EliteEquipmentPickupDesc => "Omg OwO <3 hiii :3 x3 hiiii heyyy :3 :3 :3 UwU meow mrrraow OwO";

        public override string EliteEquipmentFullDescription => "Omg OwO <3 hiii :3 x3 hiiii heyyy :3 :3 :3 UwU meow mrrraow OwO";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Motivating";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(255, 131, 20, 255));

        public override Sprite EliteEquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMotivatorAffix.png");

        public override Sprite EliteBuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMotivatorBuff2.png");

        public override Texture2D EliteRampTexture => Main.hifuSandswept.LoadAsset<Texture2D>("texRampMotivator.png");

        public override float DamageMultiplier => 2f;
        public override float HealthMultiplier => 4f;

        public static GameObject warbanner;

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override Color EliteBuffColor => Color.white; /*new Color32(200, 101, 105, 255);*/

        public override GameObject EliteCrownModel => Main.hifuSandswept.LoadAsset<GameObject>("eliteMotivatingCrown.prefab");

        public static ItemDisplayRule copiedBlazingIDRS = new();

        public static BuffDef wrbnnerBuff;
        public static BuffDef warcryBuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            wrbnnerBuff = ScriptableObject.CreateInstance<BuffDef>();
            wrbnnerBuff.isHidden = false;
            wrbnnerBuff.canStack = false;
            wrbnnerBuff.isCooldown = false;
            wrbnnerBuff.isDebuff = false;
            wrbnnerBuff.iconSprite = Paths.BuffDef.bdWarbanner.iconSprite;
            wrbnnerBuff.buffColor = new Color32(240, 35, 89, 255);

            warcryBuff = ScriptableObject.CreateInstance<BuffDef>();
            warcryBuff.isHidden = false;
            warcryBuff.canStack = false;
            warcryBuff.isCooldown = false;
            warcryBuff.isDebuff = false;
            warcryBuff.iconSprite = Paths.BuffDef.bdTeamWarCry.iconSprite;
            warcryBuff.buffColor = Paths.BuffDef.bdTeamWarCry.buffColor;

            ContentAddition.AddBuffDef(wrbnnerBuff);
            ContentAddition.AddBuffDef(warcryBuff);

            /*
            foreach (ItemDisplayRuleSet itemDisplayRuleSet in ItemDisplayRuleSet.instancesList)
            {
                var keyAssetRuleGroupArray = itemDisplayRuleSet.keyAssetRuleGroups;
                for (int i = 0; i < keyAssetRuleGroupArray.Length; i++)
                {
                    var index = keyAssetRuleGroupArray[i];
                    var keyAsset = index.keyAsset;

                    Main.ModLogger.LogError("==============");
                    Main.ModLogger.LogError(keyAsset);

                    var rules = index.displayRuleGroup.rules;
                    for (int j = 0; j < rules.Length; j++)
                    {
                        var rule = rules[j];

                        Main.ModLogger.LogError(rule);
                        Main.ModLogger.LogError(rule.childName);
                        Main.ModLogger.LogError(rule.followerPrefab);
                        Main.ModLogger.LogError(rule.limbMask);
                        Main.ModLogger.LogError(rule.localAngles);
                        Main.ModLogger.LogError(rule.localPos);
                        Main.ModLogger.LogError(rule.localScale);
                        Main.ModLogger.LogError(rule.ruleType);

                        if (keyAsset == Paths.EquipmentDef.EliteFireEquipment)
                        {
                            copiedBlazingIDRS.childName = rule.childName;
                            copiedBlazingIDRS.followerPrefab = rule.followerPrefab;
                            copiedBlazingIDRS.limbMask = rule.limbMask;
                            copiedBlazingIDRS.localAngles = rule.localAngles;
                            copiedBlazingIDRS.localPos = rule.localPos;
                            copiedBlazingIDRS.localScale = rule.localScale;
                            copiedBlazingIDRS.ruleType = rule.ruleType;
                            break;
                        }
                    }

                    Main.ModLogger.LogError("==============");
                }
            }
*/

            warbanner = PrefabAPI.InstantiateClone(Paths.GameObject.WarbannerWard, "Motivator Warbanner");
            var mdlWarbanner = warbanner.transform.GetChild(1);
            mdlWarbanner.transform.localPosition = Vector3.zero;
            mdlWarbanner.RemoveComponent<ObjectScaleCurve>();

            var buffWard = warbanner.GetComponent<BuffWard>();
            buffWard.buffDef = wrbnnerBuff;

            var cylinder = mdlWarbanner.GetChild(0).GetComponent<MeshRenderer>();
            var newMat = Object.Instantiate(Paths.Material.matWarbannerPole);
            newMat.SetColor("_TintColor", new Color32(160, 79, 60, 255));

            cylinder.material = newMat;

            var plane = mdlWarbanner.GetChild(1).GetComponent<SkinnedMeshRenderer>();
            var newMat2 = Object.Instantiate(Paths.Material.matWarbannerFlag);
            newMat2.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMotivatorWarbanner.png"));

            plane.material = newMat2;
            // plane.GetComponent<Cloth>().enabled = false;

            var indicator = warbanner.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();

            var newMat3 = Object.Instantiate(Paths.Material.matWarbannerSphereIndicator2);
            newMat3.SetColor("_TintColor", new Color32(255, 0, 28, 255));
            newMat3.SetFloat("_InvFade", 1.622365f);
            newMat3.SetFloat("_RimStrength", 0.2729147f);
            newMat3.SetFloat("_IntersectionStrength", 1.563318f);
            newMat3.SetTexture("_RemapTex", Paths.Texture2D.texRampBeamLightning);

            indicator.material = newMat3;

            PrefabAPI.RegisterNetworkPrefab(warbanner);

            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;
        }

        private System.Collections.IEnumerator BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            yield return orig();

            foreach (CharacterBody allBodyPrefabBodyBodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                CharacterModel componentInChildren = allBodyPrefabBodyBodyComponent.GetComponentInChildren<CharacterModel>();
                if ((bool)componentInChildren && componentInChildren.itemDisplayRuleSet != null)
                {
                    DisplayRuleGroup equipmentDisplayRuleGroup = componentInChildren.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixRed.equipmentIndex);
                    if (!equipmentDisplayRuleGroup.Equals(DisplayRuleGroup.empty))
                    {
                        string bodyName = BodyCatalog.GetBodyName(allBodyPrefabBodyBodyComponent.bodyIndex);
                        ItemDisplayRule[] rules = equipmentDisplayRuleGroup.rules;
                        for (int i = 0; i < rules.Length; i++)
                        {
                            ItemDisplayRule itemDisplayRule = rules[i];
                            AddDisplayRule(bodyName, itemDisplayRule.childName, itemDisplayRule.localPos, itemDisplayRule.localAngles, itemDisplayRule.localScale);
                        }
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(wrbnnerBuff))
            {
                args.moveSpeedMultAdd += 0.25f;
                args.baseAttackSpeedAdd += 0.25f;
            }
            if (sender.HasBuff(warcryBuff))
            {
                args.baseAttackSpeedAdd += 0.25f;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var info = report.damageInfo;
            if (!NetworkServer.active)
            {
                return;
            }

            if (!info.attacker)
            {
                return;
            }

            var motivatorController = info.attacker.GetComponent<MotivatorController>();
            if (!motivatorController)
            {
                return;
            }
            if (Util.CheckRoll(100f * info.procCoefficient))
            {
                if (Util.HasEffectiveAuthority(info.attacker))
                {
                    motivatorController.Proc();
                }
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var sfp = body.GetComponent<MotivatorController>();
            if (body.HasBuff(Instance.EliteBuffDef))
            {
                if (sfp == null)
                {
                    body.gameObject.AddComponent<MotivatorController>();
                }
            }
            else if (sfp != null)
            {
                body.gameObject.RemoveComponent<MotivatorController>();
            }
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public static float GetOffset(GameObject prefab)
        {
            var capsuleCollider = prefab.GetComponent<CapsuleCollider>();
            if (capsuleCollider)
            {
                return capsuleCollider.height * 0.5f + capsuleCollider.center.y;
            }
            return 0f;
        }
    }

    public class MotivatorController : MonoBehaviour
    {
        public GameObject warbannerPrefab = Motivating.warbanner;
        public GameObject warbannerInstance;
        public float warbannerRadius = 20f;
        public float onHitRadius = 20f;
        public CharacterBody body;
        public Transform modelTransform;
        public HealthComponent healthComponent;
        public TeamIndex team;
        public static readonly SphereSearch sphereSearch = new();
        public static readonly List<HurtBox> hurtBoxBuffer = new();
        public static List<MotivatorController> motivatorControllers = new();
        public GameObject warbannerOffset = new("Motivator Warbanner Offset");
        public Transform mdlWarbanner;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            healthComponent = body?.healthComponent;
            modelTransform = body?.modelLocator?.modelTransform;
            if (body)
            {
                warbannerRadius += body.radius;
                onHitRadius += body.radius;
            }
            if (modelTransform)
            {
                warbannerOffset.transform.parent = modelTransform;
                warbannerOffset.transform.localPosition = new Vector3(0f, Motivating.GetOffset(body.gameObject), 0f);
                warbannerOffset.transform.eulerAngles = Vector3.zero;

                warbannerInstance = Instantiate(warbannerPrefab, modelTransform.position, Quaternion.identity);
                warbannerInstance.transform.parent = warbannerOffset.transform;

                warbannerInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                warbannerInstance.GetComponent<BuffWard>().Networkradius = warbannerRadius;

                NetworkServer.Spawn(warbannerInstance);

                mdlWarbanner = warbannerInstance.transform.GetChild(1);
                if (body)
                {
                    mdlWarbanner.localScale = Vector3.one * body.radius * 0.3f;
                    if (body.isPlayerControlled)
                        mdlWarbanner.gameObject.SetActive(false);
                }
            }

            /*
            Main.ModLogger.LogError("motivator warbanner is " + Motivator.warbanner);
            Main.ModLogger.LogError("warbanner instance is " + warbannerInstance);
            Main.ModLogger.LogError("warbanner instance networked body attaachment is " + warbannerInstance.GetComponent<NetworkedBodyAttachment>());
            * none null
            * */

            /*
            var networkedBodyAttachment = warbannerInstance.GetComponent<NetworkedBodyAttachment>();
            networkedBodyAttachment.AttachToGameObjectAndSpawn(warbannerOffset);

            warbannerInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(warbannerOffset);
            */

            motivatorControllers.Add(this);
        }

        public void FixedUpdate()
        {
            if (warbannerOffset && modelTransform)
            {
                warbannerOffset.transform.eulerAngles = modelTransform.eulerAngles;
            }

            if (warbannerInstance)
            {
                warbannerInstance.transform.localPosition = Vector3.zero;
                warbannerInstance.transform.localEulerAngles = Vector3.zero;
            }
        }

        public void Proc()
        {
            if (!body)
            {
                return;
            }

            if (!healthComponent)
            {
                return;
            }

            if (!healthComponent.alive)
            {
                return;
            }

            Vector3 corePosition = body.corePosition;
            sphereSearch.origin = corePosition;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = onHitRadius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.GetHurtBoxes(hurtBoxBuffer);
            sphereSearch.ClearCandidates();

            for (int i = 0; i < hurtBoxBuffer.Count; i++)
            {
                var hurtBox = hurtBoxBuffer[i];
                if (hurtBox.healthComponent)
                {
                    var targetBody = hurtBox.healthComponent.body;
                    if (targetBody && !targetBody.HasBuff(Motivating.Instance.EliteBuffDef) && targetBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                    {
                        targetBody.AddTimedBuff(Motivating.warcryBuff, 4f);
                    }
                }
            }

            hurtBoxBuffer.Clear();

            // Someone Else make vfx later and uncomment
            /*
            EffectManager.SpawnEffect(buffVFX, new EffectData
            {
                origin = corePosition,
                scale = radius
            }, true);
            */
        }

        public void OnDestroy()
        {
            NetworkServer.Destroy(warbannerInstance);
            motivatorControllers.Remove(this);
        }
    }
}