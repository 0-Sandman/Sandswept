using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items
{
    public class Plutonium : ItemBase<Plutonium>
    {
        public class ShieldedComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;

            public bool cachedIsShielded = false;
        }

        public class PlutoniumBehaviour : MonoBehaviour
        {
            public bool active = true;
            public CharacterBody body;
            public float Timer;
            private GameObject PlutIndicator;

            private bool indicatorEnabled
            {
                get
                {
                    return PlutIndicator;
                }
                set
                {
                    if (indicatorEnabled != value)
                    {
                        if (value)
                        {
                            PlutIndicator = Instantiate(PlutoniumZone, body.corePosition, Quaternion.identity);
                            PlutIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                        }
                        else
                        {
                            Destroy(PlutIndicator);
                            PlutIndicator = null;
                        }
                    }
                }
            }

            public void FixedUpdate()
            {

                if (!active)
                {
                    Destroy(this);
                }

                if (NetworkServer.active)
                {
                    Timer += Time.fixedDeltaTime;
                    if (!(Timer >= 0.05))
                    {
                        return;
                    }
                    Timer = 0f;
                    TeamIndex teamIndex = body.teamComponent.teamIndex;
                    for (TeamIndex teamIndex2 = TeamIndex.Neutral; teamIndex2 < TeamIndex.Count; teamIndex2++)
                    {
                        if (teamIndex2 != teamIndex && teamIndex2 != 0)
                        {
                            foreach (TeamComponent teamMember in TeamComponent.GetTeamMembers(teamIndex2))
                            {
                                Vector3 val = teamMember.transform.position - body.corePosition;
                                if (val.sqrMagnitude <= 300f)
                                {
                                    InflictDotInfo inflictDotInfo = default;
                                    inflictDotInfo.victimObject = teamMember.gameObject;
                                    inflictDotInfo.attackerObject = body.gameObject;
                                    inflictDotInfo.totalDamage = body.damage;
                                    inflictDotInfo.dotIndex = IrradiatedIndex;
                                    inflictDotInfo.duration = 0.1f;
                                    inflictDotInfo.maxStacksFromAttacker = 1;
                                    inflictDotInfo.damageMultiplier = 0.75f + (0.5f * body.inventory.GetItemCount(instance.ItemDef));
                                    InflictDotInfo dotInfo = inflictDotInfo;
                                    DotController.InflictDot(ref dotInfo);
                                }
                            }
                        }
                    }
                }
            }

            private void Start()
            {
                indicatorEnabled = true;
                On.RoR2.CharacterBody.OnInventoryChanged += InventoryCheck;
            }

            private void OnDestroy()
            {
                indicatorEnabled = false;
                On.RoR2.CharacterBody.OnInventoryChanged -= InventoryCheck;
            }

            public void InventoryCheck(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
            {
                orig(self);
                int stack = body.inventory.GetItemCount(instance.ItemDef);
                if (stack == 0)
                {
                    Destroy(this);
                }
            }
        }
        public static DamageColorIndex IrradiateDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public static DotController.DotDef IrradiatedDef;

        public static DotController.DotIndex IrradiatedIndex;

        public static BuffDef IrradiatedBuff;

        public static GameObject PlutoniumZone;
        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "Create an irradiating ring around you when you have active shield";

        public override string ItemFullDescription => "Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>3%</style> of your maximum health. While shields are active create a <style=cIsUtility>15m</style> radius that <style=cIsHealing>Irradiates</style> enemies for <style=cIsDamage>75%</style> <style=cStack>(+50% per stack)</style> damage.";

        public override string ItemLore => "<style=cStack>funny quirky funny funny funny quirky</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("PlutoniumPrefab.Prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("PlutoniumIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuff();
            CreateDot();
            CreateItem();
            CreatePrefab();
            Hooks();
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCheck;
            GetStatCoefficients += GrantEffect;
        }

        public void CreatePrefab()
        {
            PlutoniumZone = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator"), "PlutoniumZone");
            Transform val = PlutoniumZone.transform.Find("Radius, Spherical");
            val.localScale = Vector3.one * 15f * 2f;
            HGIntersectionController hGIntersectionController = val.gameObject.AddComponent<HGIntersectionController>();
            hGIntersectionController.Renderer = val.GetComponent<MeshRenderer>();
            MeshRenderer val5 = (MeshRenderer)hGIntersectionController.Renderer;
            Material val3 = Addressables.LoadAssetAsync<Material>("d0eb35f70367cdc4882f3bb794b65f2b").WaitForCompletion();
            Material val4 = Object.Instantiate(val3);
            val4.SetColor("_TintColor", new Color32(95, 255, 0, 255));
            val4.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("385005992afbfce4089807386adc07b0").WaitForCompletion());
            val4.SetFloat("_Boost", 3);
            val5.material = val4;
            hGIntersectionController.Material = val4;
            PrefabAPI.RegisterNetworkPrefab(PlutoniumZone);
        }

        public void CreateBuff()
        {
            IrradiatedBuff = ScriptableObject.CreateInstance<BuffDef>();
            IrradiatedBuff.name = "Irradiated";
            IrradiatedBuff.buffColor = new Color32(95, 255, 0, 255);
            IrradiatedBuff.canStack = false;
            IrradiatedBuff.isDebuff = true;
            IrradiatedBuff.iconSprite = Main.MainAssets.LoadAsset<Sprite>("IrradiatedIcon.png");
            ContentAddition.AddBuffDef(IrradiatedBuff);
        }

        public void CreateDot()
        {
            IrradiatedDef = new DotController.DotDef
            {
                associatedBuff = IrradiatedBuff,
                damageCoefficient = 1f,
                damageColorIndex = IrradiateDamageColour,
                interval = 1f
            };
            IrradiatedIndex = DotAPI.RegisterDotDef(IrradiatedDef, null, null);
        }

        private void ShieldedCheck(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig.Invoke(self);
            ShieldedComponent shieldedCoreComponent = self.GetComponent<ShieldedComponent>();
            if (!shieldedCoreComponent)
            {
                shieldedCoreComponent = self.gameObject.AddComponent<ShieldedComponent>();
            }
            int count = GetCount(self);
            bool flag = self.healthComponent.shield > 0f;
            bool flag2 = false;
            if (shieldedCoreComponent.cachedInventoryCount != count)
            {
                flag2 = true;
                shieldedCoreComponent.cachedInventoryCount = count;
            }
            if (shieldedCoreComponent.cachedIsShielded != flag)
            {
                flag2 = true;
                shieldedCoreComponent.cachedIsShielded = flag;
            }
            if (flag2)
            {
                self.statsDirty = true;
            }
        }
        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent component = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += component.fullHealth * 0.03f;
            }
        }
        private void GrantEffect(CharacterBody sender, StatHookEventArgs args)
        {
            ShieldedComponent component = sender.GetComponent<ShieldedComponent>();
            PlutoniumBehaviour behaviourCheck = sender.GetComponent<PlutoniumBehaviour>();
            if ((bool)component && component.cachedIsShielded && component.cachedInventoryCount > 0 && !behaviourCheck)
            {
                PlutoniumBehaviour behaviour = sender.gameObject.AddComponent<PlutoniumBehaviour>();
                behaviour.body = sender;
            }
            if ((bool)component && component.cachedIsShielded == false && behaviourCheck)
            {
                behaviourCheck.active = false;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

    }
}
