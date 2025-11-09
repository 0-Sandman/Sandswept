using System;
using System.Diagnostics;
using RoR2.UI;
using BarInfo = RoR2.UI.HealthBar.BarInfo;
using UnityEngine.UI;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Debug = UnityEngine.Debug;
using LookingGlass.ItemStatsNameSpace;
using R2API.Networking.Interfaces;
using R2API.Networking;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Item: Makeshift Plate")]
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public static BuffDef MakeshiftPlateCount;

        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Gain plating on stage entry. Plating absorbs damage, but cannot be recovered.";

        public override string ItemFullDescription => $"Begin each stage with $sh{basePercentPlatingGain}%$se $ss(+{stackPercentPlatingGain}% per stack)$se plating. Plating acts as $shsecondary health$se, but cannot be recovered in any way.".AutoFormat();

        public override string ItemLore =>
        """
        Order: WW2019 Artifact - #1485-43958
        Tracking Number: 599*****
        Estimated Delivery: 8/9/2027
        Shipping Method: Top Priority
        Shipping Address: 707th Penthouse Suite, Earth
        Shipping Details:

        A real, genuine plate of armor from the War of 2019, Somehow managed to survive what looks like gunshots, bombing runs, laser tech, and all sorts of other things. This thing would be a fine centerpiece to a collection I'd reckon. You paid a hefty price for this thing, hope it's worth it!
        """;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.mainAssets.LoadAsset<GameObject>("MakeshiftPlatePrefab.prefab");

        public override Sprite ItemIcon => Main.assets.LoadAsset<Sprite>("texIconPlate.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.Healing, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy];

        [ConfigField("Base Percent Plating Gain", "", 150f)]
        public static float basePercentPlatingGain;

        [ConfigField("Stack Percent Plating Gain", "", 150f)]
        public static float stackPercentPlatingGain;

        public static Sprite texPlatingBar => Main.sandsweptHIFU.LoadAsset<Sprite>("texPlatingBar.png");

        public override void Init()
        {
            base.Init();

            NetworkingAPI.RegisterMessageType<MakeshiftPlateAddSync>();
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.Start += OnBodySpawn;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            IL.RoR2.UI.HealthBar.ApplyBars += UpdatePlatingUI;
            IL.RoR2.UI.HealthBar.UpdateHealthbar += UpdateHealthBar;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += GiveItem;
            On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (self.GetComponent<PlatingManager>() && self.inventory.GetItemCount(ItemDef) == 0)
            {
                new MakeshiftPlateAddSync(self.gameObject, 0f, 0f, true).Send(NetworkDestination.Clients);
            }
        }

        private void GiveItem(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);

            if (itemIndex == ItemDef.itemIndex && self.TryGetComponent<CharacterMaster>(out CharacterMaster cm) && cm.bodyInstanceObject)
            {
                PlatingManager manager = cm.bodyInstanceObject.GetComponent<PlatingManager>();
                if (!manager)
                {
                    manager = cm.bodyInstanceObject.AddComponent<PlatingManager>();
                }

                CharacterBody cb = cm.bodyInstanceObject.GetComponent<CharacterBody>();

                float platingMult = (stackPercentPlatingGain / 100f) * count;
                int plating = Mathf.RoundToInt((cb.maxHealth + cb.maxShield) * platingMult);

                manager.CurrentPlating += plating;
                if (manager.MaxPlating == 0)
                {
                    manager.MaxPlating = plating;
                }
                manager.CurrentPlating = Mathf.Min(manager.CurrentPlating, manager.MaxPlating);

                new MakeshiftPlateAddSync(cb.gameObject, manager.CurrentPlating, manager.MaxPlating, false).Send(NetworkDestination.Clients);
            }
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Plating: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Armor);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    (basePercentPlatingGain + stackPercentPlatingGain * (stack - 1)) / 100f
                };

                return values;
            };

            return itemStatsDef;
        }

        private void UpdateHealthBar(ILContext il)
        {
            ILCursor c = new(il);

            FieldReference cur = null;

            c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(4),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out _)
            );

            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<float, HealthBar, float>>((orig, self) =>
            {
                if (self.source && self.source.TryGetComponent<PlatingManager>(out var p))
                {
                    return orig + p.MaxPlating;
                }

                return orig;
            });

            c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdloc(4),
                x => x.MatchStfld(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdloc(3),
                x => x.MatchStfld(out cur)
            );

            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 3);
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<float, HealthBar, float>>((orig, self) =>
            {
                if (self.source && self.source.TryGetComponent<PlatingManager>(out var p))
                {
                    return orig + p.CurrentPlating;
                }

                return orig;
            });
            c.Emit(OpCodes.Stfld, cur);
        }

        public void OnBodySpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (self.inventory)
            {
                float platingMult = (stackPercentPlatingGain / 100f) * self.inventory.GetItemCount(ItemDef);

                int plating = Mathf.RoundToInt((self.maxHealth + self.maxShield) * platingMult);

                if (plating == 0)
                {
                    return;
                }

                new MakeshiftPlateAddSync(self.gameObject, plating, plating, false).Send(NetworkDestination.Clients);
            }
        }

        public void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info)
        {
            if (self.body && self.body.TryGetComponent<PlatingManager>(out PlatingManager platingManager))
            {
                float plating = platingManager.CurrentPlating;
                float toRemove = 0;

                if (plating > info.damage)
                {
                    toRemove = info.damage;
                    info.damage = 0;
                }
                else
                {
                    toRemove = info.damage - plating;
                    info.damage -= plating;
                }

                platingManager.CurrentPlating -= toRemove;
                platingManager.CurrentPlating = Mathf.Clamp(platingManager.CurrentPlating, 0, platingManager.MaxPlating);

                new MakeshiftPlateAddSync(self.gameObject, platingManager.CurrentPlating, platingManager.MaxPlating, false).Send(NetworkDestination.Clients);
            }

            orig(self, info);
        }

        public void CreateBuff()
        {
            MakeshiftPlateCount = ScriptableObject.CreateInstance<BuffDef>();
            MakeshiftPlateCount.name = "Plated";
            MakeshiftPlateCount.buffColor = Color.white;
            MakeshiftPlateCount.canStack = true;
            MakeshiftPlateCount.isDebuff = false;
            MakeshiftPlateCount.iconSprite = Main.mainAssets.LoadAsset<Sprite>("MakeshiftPlateBuffIcon.png");
            ContentAddition.AddBuffDef(MakeshiftPlateCount);
        }
        /*
        public class PlatingManager : MonoBehaviour
        {
            public float CurrentPlating = 0;
            public float MaxPlating = 0;
        }
        */

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            var itemDisplay = SetUpIDRS();

            ItemDisplayRuleDict i = new();

            #region Sandswept Survivors
            /*
            i.Add("RangerBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                    localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                    localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );
            */

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01214F, 0.1681F, -0.27857F),
                    localAngles = new Vector3(16.31142F, 188.3025F, 3.37207F),
                    localScale = new Vector3(0.38187F, 0.35658F, 0.33242F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;

        }

        public class MakeshiftPlateAddSync : INetMessage
        {
            public GameObject target;
            public float plating;
            public float maxPlating;
            public bool remove;
            public void Deserialize(NetworkReader reader)
            {
                target = reader.ReadGameObject();
                plating = reader.ReadSingle();
                maxPlating = reader.ReadSingle();
                remove = reader.ReadBoolean();
            }

            public void OnReceived()
            {
                Process();
            }

            public MakeshiftPlateAddSync()
            {

            }

            public MakeshiftPlateAddSync(GameObject target, float plating, float maxPlating, bool remove)
            {
                this.target = target;
                this.plating = plating;
                this.maxPlating = maxPlating;
                this.remove = remove;

                Process();
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(target);
                writer.Write(plating);
                writer.Write(maxPlating);
                writer.Write(remove);
            }

            public void Process()
            {
                if (remove)
                {
                    target.RemoveComponent<PlatingManager>();
                }
                else
                {
                    PlatingManager manager = target.GetComponent<PlatingManager>();
                    if (!manager)
                    {
                        manager = target.AddComponent<PlatingManager>();
                    }
                    manager.MaxPlating = maxPlating;
                    manager.CurrentPlating = plating;
                }
            }
        }

        private void UpdatePlatingUI(ILContext il)
        {
            // NRE at IL_0007 in <UpdatePlatingUI>b__29_2
            ILCursor c = new(il);

            MethodReference handleBar = null;
            VariableDefinition allocator = null;
            int allocIndex = -1;

            c.TryGotoNext(x => x.MatchCallOrCallvirt(out handleBar) && handleBar != null && handleBar.Name != null && handleBar.Name.StartsWith("<ApplyBars>g__HandleBar|"));
            c.TryGotoPrev(x => x.MatchLdloca(out allocIndex));
            allocator = il.Method.Body.Variables[allocIndex];

            VariableDefinition platingInfo = new(il.Import(typeof(HealthBar.BarInfo)));
            il.Method.Body.Variables.Add(platingInfo);

            c.Index = 0;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HealthBar, HealthBar.BarInfo>>((bar) =>
            {
                PlatingManager manager = bar.source ? bar.source.GetComponent<PlatingManager>() : null;
                HealthBar.BarInfo info = new()
                {
                    enabled = manager && manager.CurrentPlating > 0,
                    color = Color.white,
                    sprite = texPlatingBar,
                    imageType = bar.style.barrierBarStyle.imageType,
                    sizeDelta = 25f,
                    normalizedXMax = 0f,
                    normalizedXMin = 0f
                };

                if (info.enabled)
                {
                    float hp = manager.CurrentPlating / manager.MaxPlating;
                    float max = 1f;

                    info.normalizedXMin = 0f;
                    info.normalizedXMax = hp;
                }

                return info;
            });
            c.Emit(OpCodes.Stloc, platingInfo);

            c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<HealthBar.BarInfoCollection>(nameof(HealthBar.BarInfoCollection.GetActiveCount)));
            c.Emit(OpCodes.Ldloca, platingInfo);
            c.EmitDelegate((int count, in HealthBar.BarInfo info) =>
            {
                if (info.enabled)
                {
                    count++;
                }

                return count;
            });

            c.TryGotoNext(MoveType.Before, x => x.MatchRet());
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloca, platingInfo);
            c.Emit(OpCodes.Ldloca, allocator);
            c.Emit(OpCodes.Call, handleBar);
        }
    }
}
