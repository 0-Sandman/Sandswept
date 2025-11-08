using LookingGlass.ItemStatsNameSpace;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sandswept.Items.Lunars
{
    [ConfigSection("Items :: Their Prominence")]
    public class TheirProminence : ItemBase<TheirProminence>
    {
        public override string ItemName => "Their Prominence";

        public override string ItemLangTokenName => "THEIR_PROMINENCE";

        public override string ItemPickupDesc => "Using a Shrine has a chance to invite the challenge of the Mountain. $lcTeleporters summon Lunar Fissures periodically.$ec".AutoFormat();

        public override string ItemFullDescription => $"Using a Shrine has a $su{baseChance * 100f}%$se $ss(+{stackChance * 100f}% per stack)$se chance to invite the $suchallenge of the Mountain$se. Teleporters summon $sr{baseLunarFissureCount}$se $ss(+{stackLunarFissureCount} per challenge of the Mountain)$se $srlunar fissures$se.".AutoFormat();

        public override string ItemLore => "\"Two brothers, standing at a well. <style=cIsVoid>Both young, both innocent.</style>\"\r\n\"A worm falls in. A new world is found. <style=cDeath>One betrayed.</style> <style=cIsUtility>One regretful.</style>\"\r\n\"Two brothers, toiling in the ambry. <style=cIsVoid>Both reverent, both powerful.</style>\"\r\n\"The [compounds] are discovered. Guardians created. <style=cIsVoid>Both amazed, both proud.</style>\"\r\n\"Two brothers, looking for a way out. <style=cIsVoid>Both hopeful. Both curious.</style>\"\r\n\"A society is found. <style=cDeath>One sympathetic.</style> <style=cIsUtility>One annoyed.</style>\"\r\n\"Two brothers, torn on ethics. <style=cDeath>One tyrannical.</style> <style=cIsUtility>One puritanical.</style>\"\r\n\"A teleporter is created. A choice is made. <style=cDeath>One regretful.</style> <style=cIsUtility>One betrayed.</style>\"\r\n\"Two brothers, separated by space. <style=cDeath>One enslaves.</style> <style=cIsUtility>One broods.</style>\"\r\n\"A shine appears in the sky. <style=cDeath>One enraged.</style> <style=cIsUtility>One hopeful.</style>\"\r\n\"Two brothers. Their times approach. <style=cDeath>One king.</style> <style=cIsUtility>One outcast.</style>\"\r\n\"A god is felled. Anarchy takes hold. <style=cDeath>One missing.</style> <style=cIsUtility>One forgotten.</style>\"\r\n\"Two brothers. <style=cIsVoid>Never... to meet again.</style>\"\r\n\r\n---------------------\r\n> Translated from a Lemurian Scribe found in the Temple of the Elders by UES personnel. Burn at leisure.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTheirProminence.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist];

        [ConfigField("Base Chance", "Decimal.", 0.35f)]
        public static float baseChance;

        [ConfigField("Stack Chance", "Decimal.", 0.15f)]
        public static float stackChance;

        [ConfigField("Base Lunar Fissure Count", "", 1)]
        public static int baseLunarFissureCount;

        [ConfigField("Stack Lunar Fissure Count", "Stack is actually Mountain Shrine Stacks", 1)]
        public static int stackLunarFissureCount;

        [ConfigField("Count stacks as global?", "This makes everyone able to proc Their Prominence if any player has it, and stacks add to a global counter instead.", true)]
        public static bool countStacksAsGlobal;

        public static int itemCount;

        public static GameObject vfx;

        public static Color32 darkBlue = new(0, 2, 255, 255);

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Challenge of the Mountain Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    MathHelpers.InverseHyperbolicScaling(baseChance, stackChance, 1f, stack)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineChanceDollUseEffect, "Their Prominence VFX", false);

            var transform = vfx.transform;

            var coloredLightShafts = transform.Find("ColoredLightShafts").GetComponent<ParticleSystemRenderer>();

            var newColoredLightShaftMat = new Material(Paths.Material.matClayBossLightshaft);
            newColoredLightShaftMat.SetFloat("_AlphaBoost", 4f);
            newColoredLightShaftMat.SetFloat("_AlphaBias", 0.1f);

            coloredLightShafts.material = newColoredLightShaftMat;

            transform.Find("ColoredLightShaftsBalance").GetComponent<ParticleSystemRenderer>().material = newColoredLightShaftMat;

            var coloredDustBalance = transform.Find("ColoredDustBalance").GetComponent<ParticleSystemRenderer>();

            var newColoredDustBalanceMat = new Material(Paths.Material.matChanceShrineDollEffect);
            newColoredDustBalanceMat.SetColor("_TintColor", darkBlue);
            newColoredDustBalanceMat.SetTexture("_RemapTex", Paths.Texture2D.texRampAreaIndicator);
            newColoredDustBalanceMat.SetFloat("_Boost", 12f);
            newColoredDustBalanceMat.SetTexture("_MainTex", Paths.Texture2D.texShrineBossSymbol);

            VFXUtils.MultiplyScale(vfx, 3f);
            VFXUtils.MultiplyDuration(vfx, 1.5f);
            VFXUtils.AddLight(vfx, darkBlue, 100f, 20f, 2f);

            coloredDustBalance.material = newColoredDustBalanceMat;

            ContentAddition.AddEffect(vfx);
        }

        public override void Hooks()
        {
            if (countStacksAsGlobal)
            {
                CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            }

            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;

            On.RoR2.TeleporterInteraction.IdleToChargingState.OnEnter += OnTPBegin;
        }

        private void OnTPBegin(On.RoR2.TeleporterInteraction.IdleToChargingState.orig_OnEnter orig, TeleporterInteraction.IdleToChargingState self)
        {
            orig(self);
            int stacks = GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, true);

            if (stacks > 0 && !self.GetComponent<TheirProminenceController>())
            {
                self.gameObject.AddComponent<TheirProminenceController>();
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, true);
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactor.TryGetComponent<CharacterBody>(out var interactorBody))
            {
                var stack = GetCount(interactorBody);
                if (countStacksAsGlobal)
                {
                    stack = itemCount;
                }
                if (stack <= 0)
                {
                    return;
                }

                if (interactableObject.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
                {
                    if (!purchaseInteraction.isShrine)
                    {
                        return;
                    }

                    var chance = MathHelpers.InverseHyperbolicScaling(baseChance, stackChance, 1f, stack) * 100f;

                    if (!Util.CheckRoll(chance))
                    {
                        return;
                    }

                    var teleporterInteraction = TeleporterInteraction.instance;
                    if (!teleporterInteraction)
                    {
                        return;
                    }

                    teleporterInteraction.AddShrineStack();
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = interactorBody,
                        baseToken = "SHRINE_BOSS_USE_MESSAGE"
                    });

                    Util.PlaySound("Play_UI_monsterLogDrop", interactableObject);
                    Util.PlaySound("Play_UI_monsterLogDrop", interactableObject);

                    EffectManager.SpawnEffect(vfx, new EffectData
                    {
                        origin = interactableObject.transform.position,
                        rotation = Quaternion.identity,
                        scale = 3f,
                        color = darkBlue
                    }, true);
                    /*
                    EffectManager.SpawnEffect(ShrineChanceBehavior.effectPrefabShrineRewardJackpotVFX, new EffectData
                    {
                        origin = base.transform.position,
                        rotation = Quaternion.identity,
                        scale = 1f,
                        color = new Color(0.7372549f, 0.90588236f, 0.94509804f)
                    }, true);
                    */
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
            
            var itemDisplay = SetUpFollowerIDRS(0.5f, 120f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1.5f, 0.5f, -1f),
                localScale = new Vector3(0.25f, 0.25f, 0.25f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class ForceMonster : MonoBehaviour
    {
        public void Start()
        {
            GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
        }
    }

    public class TheirProminenceController : MonoBehaviour
    {
        public static LazyAddressable<GameObject> fissureL = new(() => Paths.GameObject.BrotherUltLineProjectileRotateLeft);
        public static LazyAddressable<GameObject> fissureR = new(() => Paths.GameObject.BrotherUltLineProjectileRotateRight);
        [SerializeField]
        public float minSecondsBetweenPulses = 1f; // lepton lily has 1s

        public HoldoutZoneController holdoutZone;

        public TeamIndex teamIndex;

        public float previousPulseFraction;

        public int pulseCount;

        public float secondsUntilPulseAvailable;
        private static bool modifiedTheSigma = false;

        public void Start()
        {
            holdoutZone = GetComponentInParent<HoldoutZoneController>();
            previousPulseFraction = 0f;

            if (!modifiedTheSigma)
            {
                modifiedTheSigma = true;
                fissureL.Asset.AddComponent<ForceMonster>();
                fissureR.Asset.AddComponent<ForceMonster>();
            }
        }

        private float GetCurrentTeleporterChargeFraction()
        {
            return holdoutZone.charge;
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!holdoutZone || holdoutZone.charge >= 1f)
            {
                return;
            }

            if (secondsUntilPulseAvailable > 0f)
            {
                secondsUntilPulseAvailable -= Time.fixedDeltaTime;
                return;
            }

            pulseCount = CalculatePulseCount();

            float nextPulseFraction = CalculateNextPulseFraction(pulseCount, previousPulseFraction);
            float currentTeleporterChargeFraction = GetCurrentTeleporterChargeFraction();
            if (nextPulseFraction < currentTeleporterChargeFraction)
            {
                Pulse();
                previousPulseFraction = nextPulseFraction;
                secondsUntilPulseAvailable = minSecondsBetweenPulses;
            }
        }

        private static int CalculatePulseCount()
        {
            var baseCount = TheirProminence.baseLunarFissureCount;
            var stackCount = TheirProminence.stackLunarFissureCount;
            if (!TeleporterInteraction.instance)
            {
                return baseCount;
            }

            return baseCount + (stackCount * TeleporterInteraction.instance.shrineBonusStacks);
        }

        private static float CalculateNextPulseFraction(int pulseCount, float previousPulseFraction)
        {
            float num = 1f / (float)(pulseCount + 1);
            for (int i = 1; i <= pulseCount; i++)
            {
                float num2 = (float)i * num;
                if (!(num2 <= previousPulseFraction))
                {
                    return num2;
                }
            }
            return 1f;
        }

        protected void Pulse()
        {
            float num = 360f / 8;
            Vector3 norm = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
            Vector3 pos = base.transform.position;
            GameObject prefab = Random.value <= 0.5f ? fissureL : fissureR;

            for (int i = 0; i < 8; i++)
            {
                Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * norm;

                FireProjectileInfo info = new();
                info.owner = null;
                info.damage = (5f + (1.4f * Run.instance.ambientLevel)) * 5f;
                info.position = pos;
                info.rotation = Util.QuaternionSafeLookRotation(forward);
                info.projectilePrefab = prefab;

                ProjectileManager.instance.FireProjectile(info);
            }
        }
    }
}