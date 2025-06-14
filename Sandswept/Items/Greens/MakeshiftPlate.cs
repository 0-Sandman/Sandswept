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

namespace Sandswept.Items.Greens
{
    [ConfigSection("Item: Makeshift Plate")]
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public static BuffDef MakeshiftPlateCount;

        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Gain plating on stage entry. Plating absorbs damage, but cannot be recovered.";

        public override string ItemFullDescription => $"Begin each stage with $sh{basePercentPlatingGain}%$se $ss(+{stackPercentPlatingGain}% per stack)$se plating. Plating acts as $shsecondary health$se, but cannot be recovered in any way. Taking damage with plating fires $sddebris shards$se at nearby enemies for $sd2x120%$se base damage.".AutoFormat();

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

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Damage };

        [ConfigField("Base Percent Plating Gain", "", 200f)]
        public static float basePercentPlatingGain;

        [ConfigField("Stack Percent Plating Gain", "", 200f)]
        public static float stackPercentPlatingGain;

        public override void Init()
        {
            base.Init();
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.Start += OnBodySpawn;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            IL.RoR2.UI.HealthBar.ApplyBars += UpdatePlatingUI;
            IL.RoR2.UI.HealthBar.UpdateHealthbar += UpdateHealthBar;
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

                int plating = Mathf.RoundToInt(self.maxHealth * platingMult);

                if (plating == 0)
                {
                    return;
                }

                PlatingManager pm = self.AddComponent<PlatingManager>();
                pm.CurrentPlating = plating;
                pm.MaxPlating = plating;
            }
        }

        public void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info)
        {
            if (self.body.GetComponent<PlatingManager>())
            {
                float plating = self.body.GetComponent<PlatingManager>().CurrentPlating;
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

                self.body.GetComponent<PlatingManager>().CurrentPlating -= toRemove;

                if (plating > 0 && Util.CheckRoll(100f * info.procCoefficient))
                {
                    SphereSearch search = new()
                    {
                        origin = self.transform.position,
                        radius = 50,
                        mask = LayerIndex.entityPrecise.mask
                    };
                    search.RefreshCandidates();
                    search.OrderCandidatesByDistance();
                    search.FilterCandidatesByDistinctHurtBoxEntities();
                    search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(self.body.teamComponent.teamIndex));

                    foreach (HurtBox box in search.GetHurtBoxes())
                    {
                        BulletAttack attack = new();
                        attack.damage = self.body.damage * 1.2f;
                        attack.bulletCount = 2;
                        attack.maxSpread = 2;
                        attack.damageColorIndex = DamageColorIndex.Item;
                        attack.origin = self.transform.position;
                        attack.aimVector = (box.transform.position - self.transform.position).normalized;
                        attack.procCoefficient = 0.2f;
                        attack.tracerEffectPrefab = Paths.GameObject.TracerToolbotNails;
                        attack.owner = self.gameObject;

                        attack.Fire();
                    }
                }
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

        public class PlatingManager : MonoBehaviour
        {
            public float CurrentPlating = 0;
            public float MaxPlating = 0;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        private void UpdatePlatingUI(ILContext il)
        {
            // NRE at IL_0007 in <UpdatePlatingUI>b__29_2
            ILCursor c = new(il);

            MethodReference handleBar = null;
            VariableDefinition allocator = null;
            int allocIndex = -1;

            c.TryGotoNext(x => x.MatchCallOrCallvirt(out handleBar) && handleBar.Name.StartsWith("<ApplyBars>g__HandleBar|"));
            c.TryGotoPrev(x => x.MatchLdloca(out allocIndex));
            allocator = il.Method.Body.Variables[allocIndex];

            VariableDefinition platingInfo = new(il.Import(typeof(HealthBar.BarInfo)));
            il.Method.Body.Variables.Add(platingInfo);

            c.Index = 0;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HealthBar, HealthBar.BarInfo>>((bar) =>
            {
                PlatingManager manager = bar.source.GetComponent<PlatingManager>();
                HealthBar.BarInfo info = new()
                {
                    enabled = manager,
                    color = Color.grey,
                    sprite = bar.style.echoDamageStyle.sprite,
                    imageType = bar.style.shieldBarStyle.imageType,
                    sizeDelta = bar.style.shieldBarStyle.sizeDelta * 1.3f,
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
