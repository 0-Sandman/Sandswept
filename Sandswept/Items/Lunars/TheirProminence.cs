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

        public override string ItemLore => "\"Two brothers, standing at a well. <style=cIsVoid>Both young, both innocent.</style>\"\r\n\"A worm falls in. A new world is found. <style=cDeath>One betrayed.</style> <style=cIsUtility>One regretful.</style>\"\r\n\"Two brothers, toiling in the ambry. <style=cIsVoid>Both reverent, both powerful.</style>\"\r\n\"The [compounds] are discovered. Guardians created. <style=cIsVoid>Both amazed, both proud.</style>\"\r\n\"Two brothers, looking for a way out. <style=cIsVoid>Both hopeful. Both curious.</style>\"\r\n\"A society is found. <style=cDeath>One sympathetic.</style> <style=cIsUtility>One annoyed.</style>\"\r\n\"Two brothers, torn on ethics. <style=cDeath>One tyrannical.</style> <style=cIsUtility>One puritanical.</style>\"\r\n\"A teleporter is created. A choice is made. <style=cDeath>One regretful.</style> <style=cIsUtility>One betrayed.</style>\"\r\n\"Two brothers, separated by space. <style=cDeath>One enslaves.</style> <style=cIsUtility>One broods.</style>\"\r\n\"A shine appears in the sky. <style=cDeath>One enraged.</style> <style=cIsUtility>One hopeful.</style>\"\r\n\"Two brothers. Their times approach. <style=cDeath>One king.</style> <style=cIsUtility>One outcast.</style>\"\r\n\"A god is felled. Anarchy takes hold. <style=cDeath>One missing.</style> <style=cIsUtility>One forgotten.</style>\"\r\n\"Two brothers. <style=cIsVoid>Never... to meet again.</style>\"\r\n\r\n---------------------\r\n> Translated from a Lemurian Scribe found in the Temple of the Elders by UES personnel. Burn at leisure.";

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