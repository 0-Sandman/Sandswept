using System.Diagnostics;
using RoR2.UI;
using BarInfo = RoR2.UI.HealthBar.BarInfo;
using UnityEngine.UI;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Sandswept.Items.Whites
{
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {

        public static BuffDef MakeshiftPlateCount;

        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Gain plating on stage entry.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Begin each stage with $sd1000$se $ss(+1000 per stack)$se plating. Plating acts as secondary health. Plating cannot be recovered in any way.");

        public override string ItemLore => "I hope ceremonial jar is coded soon :Yeah3D:";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("MakeshiftPlatePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("MakeshiftPlateIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            CreateBuff();
            Hooks();
        }

        public delegate int orig_GetActiveCount(ref HealthBar.BarInfoCollection self);

        public override void Hooks()
        {
            On.RoR2.CharacterBody.Start += OnBodySpawn;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.UI.HealthBar.ApplyBars += SeriouslyWhy;
            On.RoR2.UI.HealthBar.UpdateBarInfos += HopooWhatIsThisShitWhyGuh;
            On.RoR2.UI.HealthBar.Awake += GameMakerStudio2;

            Hook hook = new(
                typeof(HealthBar.BarInfoCollection).GetMethod(nameof(HealthBar.BarInfoCollection.GetActiveCount), (BindingFlags)(-1)),
                typeof(MakeshiftPlate).GetMethod(nameof(FuckingWhy), BindingFlags.Public | BindingFlags.Static)
            );
        }

        public void GameMakerStudio2(On.RoR2.UI.HealthBar.orig_Awake orig, HealthBar self) {
            orig(self);
            ButHeresTheHopoo hopoo = self.AddComponent<ButHeresTheHopoo>();
            hopoo.info.sprite = self.barInfoCollection.instantHealthbarInfo.sprite;
        }

        public static int FuckingWhy(orig_GetActiveCount orig, ref HealthBar.BarInfoCollection self) {
            return orig(ref self) + 1;
        } 

        public void SeriouslyWhy(On.RoR2.UI.HealthBar.orig_ApplyBars orig, HealthBar self) {
            orig(self);

            ButHeresTheHopoo guh = self.GetComponent<ButHeresTheHopoo>();

            HandleBar(ref guh.info);
            

            void HandleBar(ref BarInfo barInfo)
            {
                Image image = self.barAllocator.elements[self.barInfoCollection.GetActiveCount() - 1];

                if (barInfo.enabled)
                {
                    image.enabled = true;
                    image.type = barInfo.imageType;
                    image.sprite = barInfo.sprite;
                    image.color = barInfo.color;
                    SetRectPosition((RectTransform)image.transform, barInfo.normalizedXMin, barInfo.normalizedXMax, barInfo.sizeDelta);
                }
                else {
                    image.enabled = false;
                }
            }

            static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
            {
                rectTransform.anchorMin = new Vector2(xMin, 0f);
                rectTransform.anchorMax = new Vector2(xMax, 1f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
            }
        } 

        public void HopooWhatIsThisShitWhyGuh(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, HealthBar self) {
            ButHeresTheHopoo guh = self.GetComponent<ButHeresTheHopoo>();

            if (self.source.GetComponent<PlatingManager>()) {
                PlatingManager manager = self.source.GetComponent<PlatingManager>();
                ref BarInfo info = ref guh.info;

                info.enabled = manager.CurrentPlating > 0;

                info.normalizedXMin = 0f;
                info.normalizedXMax = manager.CurrentPlating == 0 ? 0 : (float)manager.CurrentPlating / (float)manager.MaxPlating;

                // UnityEngine.Debug.Log($"-----\nEnabled: {guh.enabled}\nXMax: {info.normalizedXMax}\n----");
            }
            else {
                guh.enabled = false;
            }

            orig(self);
        }

        public class ButHeresTheHopoo : MonoBehaviour {
            public BarInfo info;
            public Sprite sprite;

            public void Start() {
                info = new();
                info.enabled = false;
                info.color = Color.grey;
                info.imageType = Image.Type.Tiled;
                info.sprite = sprite;
            }
        }

        public void OnBodySpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) {
            orig(self);

            if (self.inventory) {
                int plating = self.inventory.GetItemCount(ItemDef) * 1000;

                if (plating == 0) {
                    return;
                }

                PlatingManager pm = self.AddComponent<PlatingManager>();
                pm.CurrentPlating = plating;
                pm.MaxPlating = plating;
            }
        }

        public void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info) {
            if (self.body.GetComponent<PlatingManager>()) {
                int plating = self.body.GetComponent<PlatingManager>().CurrentPlating;
                int toRemove = 0;
                if (plating > info.damage) {
                    toRemove = Mathf.RoundToInt(info.damage);
                    info.damage = 0;
                }
                else {
                    toRemove = Mathf.RoundToInt(info.damage) - plating;
                    info.damage -= plating;
                }

                self.body.GetComponent<PlatingManager>().CurrentPlating -= toRemove;
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
            MakeshiftPlateCount.iconSprite = Main.MainAssets.LoadAsset<Sprite>("MakeshiftPlateBuffIcon.png");
            ContentAddition.AddBuffDef(MakeshiftPlateCount);
        }

        public class PlatingManager : MonoBehaviour {
            public int CurrentPlating = 0;
            public int MaxPlating = 0;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}