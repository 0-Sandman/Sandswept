/*using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class TarCauldron : ItemBase<TarCauldron>
    {
        public class TarToken : MonoBehaviour
        {
            public CharacterBody body;
        }

        public override string ItemName => "Tar Cauldron";

        public override string ItemLangTokenName => "TAR_CAULDRON";

        public override string ItemPickupDesc => "Debuffs last longer and apply tar";

        public override string ItemFullDescription => "Debuffs last <style=cIsUtility>25%</style> <style=cStack>(+15% per stack)</style> longer and have a <style=cIsUtility>15%</style> chance to apply <style=cArtifact>tar</style> to enemies.";

        public override string ItemLore => "this item is hell to code";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("CauldronPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("CauldronIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += DebuffApply;
            On.RoR2.CharacterBody.OnInventoryChanged += ComponentCheck;
        }

        private void ComponentCheck(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
        }

        private void DebuffApply(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}*/
