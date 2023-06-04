using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Whites
{
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "'Critical Strikes' give temporary barrier.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Gain $sd5%$se $ss(+5% per stack)$se $sdcritical chance$se. $sdCritical Strikes$se give a $shtemporary barrier$se for $sh10$se $ss(+5 per stack)$se $shhealth$se.");

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("AmberKnifePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("AmberKnifeIcon.png");

        public override void Init(ConfigFile config)
        {
            var component = ItemModel.transform.Find("AmberKnife").Find("Knife").gameObject;
            var renderer = component.GetComponent<MeshRenderer>();
            var controller = component.AddComponent<HGStandardController>();
            controller.Renderer = renderer;
            controller.Material = renderer.materials[0];
            var material = controller.Material;
            material.SetTexture("_EmTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Grandparent/texGrandparentDetailGDiffuse.png").WaitForCompletion());
            material.SetFloat("_EmPower", 10f);
            material.SetColor("_EmColor", new Color32(50, 0, 0, 255));
            material.SetTexture("_FresnelRamp", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampTwotone.jpg").WaitForCompletion());
            material.SetFloat("_FresnelPower", 13f);
            material.SetFloat("_FresnelBoost", 2.5f);
            material.EnableKeyword("FRESNEL_EMISSION");
            renderer.material = material;
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            GetStatCoefficients += GiveCrit;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;
            if (!damageInfo.crit)
            {
                return;
            }

            if (damageInfo.procCoefficient <= 0)
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            int stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                attackerBody.healthComponent.AddBarrier(5f + (5f * stacks));
            }
        }

        public void GiveCrit(CharacterBody sender, StatHookEventArgs args)
        {
            int stacks = GetCount(sender);
            if (stacks > 0)
            {
                args.critAdd += 5f * stacks;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}