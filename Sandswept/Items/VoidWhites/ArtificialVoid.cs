/*
using EntityStates.Toolbot;
using HarmonyLib;

namespace Sandswept.Items.VoidWhites
{
    [ConfigSection("Items :: Artificial Void")]
    public class ArtificialVoid : ItemBase<ArtificialVoid>
    {
        public override string ItemName => "Artificial Void";

        public override string ItemLangTokenName => "ARTIFICIAL_VOID";

        public override string ItemPickupDesc => "Killing a champion grants permanent shield. $svCorrupts all Topaz Brooches$se.";

        public override string ItemFullDescription => ("Upon killing a $sdchampion$se, gain $sh" + baseShieldGain + "$se $ss(+" + stackShieldGain + " per stack)$se $shpermanent shield$se. $svCorrupts all Topaz Brooches$se.").AutoFormat();

        public override string ItemLore => "";

        [ConfigField("Base Shield Gain", "", 8)]
        public static int baseShieldGain;

        [ConfigField("Stack Shield Gain", "", 8)]
        public static int stackShieldGain;

        public static BuffDef shields;

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            shields = ScriptableObject.CreateInstance<BuffDef>();
            shields.isHidden = false;
            shields.isDebuff = true;
            shields.buffColor = Color.blue;
            shields.isCooldown = false;
            shields.canStack = false;
            shields.name = "Artificial Void Shield - 1% Per";

            ContentAddition.AddBuffDef(shields);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster master)
        {
            if (master.teamIndex == TeamIndex.Player && master.GetComponent<ArtificialVoidController>() == null)
            {
                master.AddComponent<ArtificialVoidController>();
            }
        }

        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            var stack = GetCount(body);
            if (stack > 0)
            {
                Main.ModLogger.LogError("stack above 0");
                var increase = baseShieldGain + stackShieldGain * (stack - 1);

                if (self.TryGetComponent<ArtificialVoidController>(out var artificialVoidController))
                {
                    body.SetBuffCount(shields.buffIndex, artificialVoidController.championKills * increase);
                }

                if (!artificialVoidController)
                {
                    Main.ModLogger.LogError("no av component found a A   a ");
                }
            }
            else
            {
                body.SetBuffCount(shields.buffIndex, 0);
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            if (!victimBody.isChampion)
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var attackerMaster = attackerBody.master;
            if (!attackerMaster)
            {
                return;
            }

            if (attackerMaster.TryGetComponent<ArtificialVoidController>(out var artificialVoidController))
            {
                Main.ModLogger.LogError("adding to champion kills");
                artificialVoidController.championKills++;
            }

            var stack = GetCount(attackerBody);
            if (stack <= 0)
            {
                Main.ModLogger.LogError("setting shields buffs to 0");
                attackerBody.SetBuffCount(shields.buffIndex, 0);
                return;
            }

            var increase = baseShieldGain + stackShieldGain * (stack - 1);

            if (artificialVoidController)
            {
                Util.PlaySound("Play_bandit2_R_alt_kill", attackerBody.gameObject);
                attackerBody.SetBuffCount(shields.buffIndex, artificialVoidController.championKills * increase);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            args.baseShieldAdd += sender.GetBuffCount(shields);
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = instance.ItemDef,
                itemDef1 = RoR2Content.Items.BarrierOnKill
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public class ArtificialVoidController : MonoBehaviour
        {
            public int championKills = 0;
        }
    }
}
*/