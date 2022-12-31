using BepInEx.Configuration;
using R2API;
using RoR2;
using Sandswept.Utils.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.RecalculateStatsAPI;
using RoR2.Projectile;

namespace Sandswept.Items
{
    public class Plutonium : ItemBase<Plutonium>
    {
        public class PlutoniumBehaviour : CharacterBody.ItemBehavior
        {
            public float Timer;
            public void FixedUpdate()
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
                            if (val.sqrMagnitude <= 500f)
                            {
                                InflictDotInfo inflictDotInfo = default(InflictDotInfo);
                                inflictDotInfo.victimObject = teamMember.gameObject;
                                inflictDotInfo.attackerObject = body.gameObject;
                                inflictDotInfo.totalDamage = body.damage * 0.25f;
                                inflictDotInfo.dotIndex = IrradiatedIndex;
                                inflictDotInfo.damageMultiplier = 1f;
                                InflictDotInfo dotInfo = inflictDotInfo;
                                DotController.InflictDot(ref dotInfo);
                            }
                        }
                    }
                }
            }
        }
        public static DotController.DotDef IrradiatedDef;

        public static DotController.DotIndex IrradiatedIndex;

        public static BuffDef IrradiatedBuff;

        public static GameObject PlutoniumZone;
        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "Create an irradiating ring around you when you have active shield";

        public override string ItemFullDescription => "I dont want to write this rn";

        public override string ItemLore => "<style=cIsStack>funny quirky funny funny funny quirky</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuff();
            CreateDot();
            CreateItem();
            PlutoniumZone = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab").WaitForCompletion(), "PlutoniumZone");
            GameObject.Destroy(PlutoniumZone.GetComponent<ProjectileDotZone>());
            GameObject.Destroy(PlutoniumZone.transform.GetChild(0).gameObject);
            Transform val = PlutoniumZone.transform.Find("Radius, Spherical");
            val.localScale = Vector3.one * 25f * 2f;
            MaterialControllerComponents.HGIntersectionController hGIntersectionController = val.gameObject.AddComponent<MaterialControllerComponents.HGIntersectionController>();
            hGIntersectionController.Renderer = val.GetComponent<MeshRenderer>();
            MeshRenderer val2 = (MeshRenderer)hGIntersectionController.Renderer;
            Material val3 = PlutoniumZone.
        }

        public void CreateBuff()
        {
            IrradiatedBuff = ScriptableObject.CreateInstance<BuffDef>();
            IrradiatedBuff.name = "Irradiated";
            IrradiatedBuff.buffColor = new Color(175f, 255f, 30f, byte.MaxValue);
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
                damageColorIndex = DamageColorIndex.Heal,
                interval = 0.5f
            };
            IrradiatedIndex = DotAPI.RegisterDotDef(IrradiatedDef, null, null);
        }
        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent component = ((Component)sender).GetComponent<HealthComponent>();
                args.baseShieldAdd += component.fullHealth * 0.03f;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

    }
}
