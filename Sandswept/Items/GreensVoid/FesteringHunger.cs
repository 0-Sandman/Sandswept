
// TODO: Implement DoT tick rate increase for nearby enemies (+20% per stack most likely, maybe nerf speed to +25% then too?)

using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using Sandswept.Items.Greens;
using System.Collections;
using UnityEngine.UIElements;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Festering Hunger")]
    public class FesteringHunger : ItemBase<FesteringHunger>
    {
        public override string ItemName => "Festering Hunger";

        public override string ItemLangTokenName => "FESTERING_HUNGER";

        public override string ItemPickupDesc => "Chance to decay enemies on hit. Moving near decaying enemies increases movement speed. $svCorrupts all Smouldering Documents$se.".AutoFormat();

        public override string ItemFullDescription => $"$sd{chance}%$se chance on hit to inflict $sddecay$se for $sd{DoTs.Decay.baseDamage * 100f}%$se base damage. Moving near $sddecaying$se enemies increases $sumovement speed$se by $su{baseMovementSpeedGain * 100f}%$se $ss(+{stackMovementSpeedGain * 100f}% per stack)$se for $su{movementSpeedBuffDuration}$se seconds. $svCorrupts all Smouldering Documents$se.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>
        Welcome to DataScraper (v3.1.53 - beta branch)
        $ Scraping memory...done.
        $ Resolving...done.
        $ Combing for relevant data...done.
        Complete!
        </style>

        <style=cMono>Day [null]</style>
        It's beautiful here, but terrifying. When we landed on the planet, we already knew what to expect on the surface from the screening. But this...the portal didn't show up in our scans, nor did this vast ecosystem. Ash opted to stay on the surface instead, a choice I thought foolish at first, but I'm now realizing may have been the better of the two.

        I'm not even certain where I am. My navigation and communication tools don't seem to be working, and the black sky holds no answers. The dizzying depth I observed when looking up makes me think I might be far underground. VERY far.

        <style=cMono>Day [null]</style>
        I found something. Not life -- still no signs of that, but...something. Sort of like a pool, I guess. I don't know what the blackness inside it is, but it seems liquid-ish. I'd take a sample, but I'm far more interested in just getting out of here alive at this point. I can come back later to investigate the wonders of this place. Ideally, someone else will. I'm sure my father can find some overeager xenobiologist.

        I don't know why I did it, but I put one of my knives in. I just sort of felt compelled to do it. When I brought it back out, it was...different. Now it looks like everything else, like it was made here. I felt a sharp pain in my hand when I did, though, as if it didn't want me to have it. My suit says there's no injuries, but the pain lingers.

        <style=cMono>Day -2</style>
        Finally found another portal. I'm out! It was on the other side of the planet, but my distress beacons work now, and the ship should be on its way. They told me over the radio that Ash returned a couple days ago and described the portal. They'd never heard of anything like it before.

        How I missed the sunlight, the air, the...finite-ness.

        I'm more convinced now than ever that the portal is invasive. There's nothing intelligent enough on this planet to have created something like that, and no signs of a past civilization, either. It's far outside the bounds of human technology. This discovery, whatever it is exactly, will shake things up for sure, though I want nothing to do with that place anymore.

        <style=cMono>Day -1</style>
        They're running tests on the portal. I've just been watching from far away. After what I described, nobody except the xenobiologists want to go in, and even they're hesitant. My father is enforcing strict regulations around the portal. Either way, they'll be making sure whoever follows in my footsteps is safer than I was.

        What was I thinking, just stepping in like that? At least it's over now...

        <style=cMono>Day 0</style>
        There's a hole in the glass container they used to store the knife. In the camera footage, it just sort of...appeared. Nothing was cut, the footage from that day is the length it should be. They didn't just tape it over, either -- you can see the glass breaking. But nothing caused it.

        They're calling it a pressure abnormality. They aren't worried. I don't fully share that, but they're probably right. After being in what we're now calling the void, I'm probably just paranoid.

        <style=cMono>Day 1</style>
        I woke up feeling a terrible empti?ness. Nothing has quenched it. It's made it hard to be cordial. Every time I talk to someone, I have to push down anger that wells up just from the time wasted on the words -- especially with the way they talk about the [void], as if it were some hellscape.

        They've never been there. Only I have. Cowards. They tell me I didn't say it was majestic before, that they're just repeating my wor?ds back to me, but I know my own memories.

        <style=cMono>Day 2</style>
        They all looked at me with surprise when I said I w?anted to go back. I calmed myself at first, trying to explai?n to them, but they wouldn't get out of the way. Just stood there, slack jawed. When I started yelling, they dragg?ed me away. Unbelievable.

        I'm in the med bay now. They're running sc?ans, but I can't let them. Some of my friends came in, talked with soft and concerned voices. They all b?lur together. Any one of them could have let me go, brought m?e to the portal, but they didn't. Some friends they are. I?'ll just have to get out myself.

        <style=cMono>Day 3</style>
        When they dis?covere?d I'd broken out, I made up something abou?t hating to be confined, and the doctors s?eemed to believe it. I suppressed the em??ptiness, told them I was fine, told them I never really wanted to go back anyw?ay. But it gets worse an?d worse, deeper and d?eeper, every hour.

        They let me go. I'm too valuab?le to the ship, apparently, a?nd my fa?ther wanted me out. I'm fr?ee.

        When I arr?ived breathless at the pods, though, I found onl?y empty space outside. The sh?ip had left during my confinement, l?eaving the planet and the portal behind.

        The captain won't listen. I had to b?ackped?al to avoid being sent to med bay again. I?f I kill him and take control of the ship, we can go back. He leaves me no choic?e.

        <style=cMono>Day 4</style>
        I l??ooked in the mirror t?oday. The s?kin below my eyes was darker than I've ev?er s?een before. I only then r??ealized I haven't sle?pt since the emptine?ss started. I n?ever feel tired. I can feel my body shaki??ng slightly now, tho?ugh. But I sti?ll fee?l energized.

        Toda??y will be the day I do it. I have a pr?ivate lunch scheduled with the h?im in the cockp?it, like we do ev?ery week. He l?ikes to keep up "father-d??aughter bonding time." It's the p?erfect opportun?ity.

        <style=cMono>Day 5</style>
        He's d?ead. All it took was some sob st?ory about how horrible the v??oid had been, and he came right up and embraced me without a seco?nd thought. Idio?t. I wasted no time rerouting the d?estination. They're at the door now, but th?ey won't get in. Father bragged ab?out the security of the cockpit. U?ES put a lot into that, after Contact L?ight.

        The warmth of his blood on m?y skin made the emptiness subside a bi?t. It's nothing compared to w?hat the [void] can offer, of cour?se. They won't follow me will?ingly after this, but maybe I can bring some of the c?rew in with me. They nee?d to see.

        <style=cMono>Day 8</style>
        Th?ey're trying to st?arve me out. I've been living o?ff his body. It's hard to e??at in the m?omen?t, but it ke?eps me alive. It make??s the emptin?ess a little bette?r, t?oo.

        We're on the p??lanet now, near the p?ortal. The terrain is rough, so the shi?p is com??promised, but the cockpit i?s fine. They're guardin?g it, t?hough, and they'll outl?ast me. I have t?o do so??mething.

        <style=cMono>Day 9</style>
        I co??uld f?eel the [reavers] ar??rivi??ng. Sc?rea?ms fr??om outside, fo??llowe?d b?y gunfir??e. The gunfire g??rad?ua?lly fad?ed, but the screa??ms didn't. Ev??entua??lly, t?he la?st bulle??t was sh?ot, and no?w only th??e sc?ream?s f?ill the silen??ce, al?ong with t?he [reavers'] son??gs.

        h??eir songs br?ought tear??s to my e??yes. They're e?ven more b?eautif?ul than the scre?ams. They s?ing of how the ship will be ta??ken and sequenc?ed. How the cr??ew will be [touched] to serve [the one beyond], and sent acro?ss the galaxy. Of my valor, and ho?w my essence will finally return to the [void]. The songs drive away the emptiness.

        I open the doors. The crew is disarmed, bound to the floor by the [reavers]. They gaze upon me in horror, but they don't matter. The [reavers'] gaze is the one that matters. I feel enormous pride. I've served the [void] well.

        One by one, the crew is [touched], then unbound. The effect takes hold for them faster than for me. The [infestors] have evolved. They amble away, into the portal, until only one is left. I wish to join them, but I know it isn't my place. I'll be there soon enough.

        At last, the [infestor] leaves me. As I collapse, the toll on my system throughout the past nine days finally paid, I watch it crawl onto the final crewmember.

        Lucidity returns, and the horror of what I've done dawns on me. The scent and sight of my father's blood alone makes me want to throw up. I can't even begin to think about the...other stuff. Already, my mind is blocking out the memories. In that moment, I want nothing less than to live with what I've done forever, sequenced into one of their cells.

        Another horror joins it as I recognize the last crewmember. Ash. They struggle against the reavers' bonds, but it's fruitless, and I'm powerless to help. I have to watch as they become touched, before following the others through the portal.

        They, who had been with me since the start of this. Choosing them as the one I watch lucidly feels like a message, one emphasized by the blackness that consumes the ship as one of the reavers detonates.

        No one escapes.
        """;

        [ConfigField("Chance", "", 8f)]
        public static float chance;

        [ConfigField("Base Movement Speed Gain", "Decimal.", 0.3f)]
        public static float baseMovementSpeedGain;

        [ConfigField("Stack Movement Speed Gain", "Decimal.", 0.3f)]
        public static float stackMovementSpeedGain;

        [ConfigField("Movement Speed Buff Duration", "", 3f)]
        public static float movementSpeedBuffDuration;

        [ConfigField("Movement Speed Buff Range", "", 16f)]
        public static float movementSpeedBuffRange;

        public static BuffDef movementSpeedBuff;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.assets.LoadAsset<GameObject>("DisplayFesteringHunger.prefab");

        public override Sprite ItemIcon => Main.assets.LoadAsset<Sprite>("texFesteringHunger.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary, ItemTag.MobilityRelated];

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public override ItemDef ItemToCorrupt => SmoulderingDocument.instance.ItemDef;

        public static GameObject vfx;

        public override void Init()
        {
            base.Init();
            /*
            if (!ItemBase.DefaultEnabledCallback(SmoulderingDocument.instance))
            {
                return;
            }
            */

            SetUpBuff();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Decay Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Movement Speed: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(chance * procChance * 0.01f, luck),
                    baseMovementSpeedGain + stackMovementSpeedGain * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpBuff()
        {
            movementSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            movementSpeedBuff.canStack = false;
            movementSpeedBuff.isCooldown = false;
            movementSpeedBuff.buffColor = new Color32(96, 56, 177, 255);
            movementSpeedBuff.iconSprite = Utils.Assets.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            movementSpeedBuff.isHidden = false;
            movementSpeedBuff.isDebuff = false;
            movementSpeedBuff.name = "Festering Hunger - Movement Speed";
            ContentAddition.AddBuffDef(movementSpeedBuff);
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            // On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private static void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = FesteringHunger.instance.ItemDef,
                itemDef1 = Greens.SmoulderingDocument.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);

            orig();
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<FesteringHungerController>(GetCount(body));
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(movementSpeedBuff))
            {
                var stack = GetCount(sender);
                args.moveSpeedMultAdd += baseMovementSpeedGain + stackMovementSpeedGain * (stack - 1);
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victim = report.victim;
            if (!victim)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0)
            {
                if (Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo inflictDotInfo = new()
                    {
                        attackerObject = attackerBody.gameObject,
                        victimObject = victim.gameObject,
                        totalDamage = null,
                        damageMultiplier = 1f,
                        dotIndex = DoTs.Decay.decayIndex,
                        maxStacksFromAttacker = uint.MaxValue,
                        duration = 3f
                    };

                    // Util.PlaySound("", attackerBody.gameObject);

                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();

            var itemDisplay = SetUpFollowerIDRS(0.1f, 1000f, true, -35f, true, 25f, true, -20f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(1f, -0.4f, 0.4f),
                localScale = new Vector3(0.2f, 0.2f, 0.2f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class FesteringHungerController : CharacterBody.ItemBehavior
    {
        public float checkInterval = 0.05f;
        public float timer;
        public float radiusSquared = FesteringHunger.movementSpeedBuffRange * FesteringHunger.movementSpeedBuffRange;
        public float distance = FesteringHunger.movementSpeedBuffRange;
        public TeamIndex ownerIndex;
        // public GameObject radiusIndicator;

        private void Start()
        {
            ownerIndex = body.teamComponent.teamIndex;
            // enableRadiusIndicator = false;
            // var radiusTrans = radiusIndicator.transform.GetChild(1);
            // radiusTrans.localScale = Vector3.one * FesteringHunger.attackSpeedBuffRange;
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer < checkInterval)
            {
                return;
            }

            // Main.WRBLogger.LogError("enabling razorwire indicator");

            // enableRadiusIndicator = true;

            for (TeamIndex firstIndex = TeamIndex.Neutral; firstIndex < TeamIndex.Count; firstIndex++)
            {
                if (firstIndex == ownerIndex || firstIndex <= TeamIndex.Neutral)
                {
                    continue;
                }

                foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(firstIndex))
                {
                    var enemyPosition = teamComponent.transform.position;
                    var corePosition = body.corePosition;
                    if ((enemyPosition - corePosition).sqrMagnitude <= radiusSquared)
                    {
                        TryGiveBuff(teamComponent);
                    }
                }
            }

            timer = 0f;
        }

        private void TryGiveBuff(TeamComponent teamComponent)
        {
            var victimBody = teamComponent.body;
            if (!victimBody)
            {
                return;
            }

            if (!victimBody.HasBuff(DoTs.Decay.decayBuff))
            {
                return;
            }

            if (body.HasBuff(FesteringHunger.movementSpeedBuff))
            {
                return;
            }

            body.AddTimedBuffAuthority(FesteringHunger.movementSpeedBuff.buffIndex, FesteringHunger.movementSpeedBuffDuration);
        }

        /*
        private bool enableRadiusIndicator
        {
            get
            {
                return radiusIndicator;
            }
            set
            {
                if (enableRadiusIndicator != value)
                {
                    if (value)
                    {
                        radiusIndicator = Instantiate(FesteringHunger.indicator, body.corePosition, Quaternion.identity);
                        radiusIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, null);
                    }
                    else
                    {
                        Object.Destroy(radiusIndicator);
                        radiusIndicator = null;
                    }
                }
            }
        }
        */

        private void OnDisable()
        {
            // enableRadiusIndicator = false;
        }
    }
}