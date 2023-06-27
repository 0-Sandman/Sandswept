using RoR2.UI;

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

        public override void Hooks()
        {
            On.RoR2.CharacterBody.Start += OnBodySpawn;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
        }

        public void OnBodySpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) {
            orig(self);

            if (self.inventory) {
                int plating = self.inventory.GetItemCount(ItemDef) * 1000;

                PlatingManager pm = self.AddComponent<PlatingManager>();
                pm.CurrentPlating = plating;
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
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}