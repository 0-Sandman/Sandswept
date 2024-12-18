using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Their Prominence")]
    public class TheirProminence : ItemBase<TheirProminence>
    {
        public override string ItemName => "Their Prominence";

        public override string ItemLangTokenName => "THEIR_PROMINENCE";

        public override string ItemPickupDesc => "Using a Shrine has a chance to invite the challenge of the Mountain.";

        public override string ItemFullDescription => ("Using a shrine has a $su" + d(baseChance) + "$se $ss(+" + d(stackChance) + " per stack)$se chance to invite the $suchallenge of the Mountain$se.").AutoFormat();

        public override string ItemLore => "\"Two brothers, standing at a well. Both young, both innocent\"\r\n\"A worm falls in. A new world is found. One betrayed. One regretful.\"\r\n\"Two brothers, toiling in the ambry. Both reverent, both powerful\"\r\n\"The [compounds] are discovered. Guardians created. Both amazed, both proud\"\r\n\"Two brothers, looking for a way out. Both hopeful. Both curious\"\r\n\"A society is found. One sympathetic. One annoyed.\"\r\n\"Two brothers, torn on ethics. One tyrannical. One puritanical.\" \r\n\"A teleporter is created. A choice is made. One regretful. One betrayed.\"\r\n\"Two brothers, separated by space. One enslaves. One broods.\"\r\n\"A shine appears in the sky. One enraged. One hopeful.\"\r\n\"Two brothers. Their times approach. One king. One outcast.\"\r\n\"A god is felled. Anarchy takes hold. One missing. One forgotten.\"\r\n\"Two brothers. Never… to meet again.\"\r\n\r\nTranslated from a Lemurian Scribe found in the Temple of the Elders by UES personnel. Burn at leisure.\r\n";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTheirProminence.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Chance", "Decimal.", 0.35f)]
        public static float baseChance;

        [ConfigField("Stack Chance", "Decimal.", 0.15f)]
        public static float stackChance;

        public override void Hooks()
        {
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactor.TryGetComponent<CharacterBody>(out var interactorBody))
            {
                var stack = GetCount(interactorBody);
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

                    EffectManager.SpawnEffect(Paths.GameObject.ShrineChanceDollUseEffect, new EffectData
                    {
                        origin = interactableObject.transform.position,
                        rotation = Quaternion.identity,
                        scale = 1f,
                        color = new Color(0.7372549f, 0.90588236f, 0.94509804f)
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
        }
    }
}