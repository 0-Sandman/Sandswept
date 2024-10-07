using EntityStates.Bandit2;
using static Sandswept.Main;

namespace Sandswept.Equipment
{
    [ConfigSection("Equipment :: The Sand Sweeper")]
    public class SandSweeper : EquipmentBase
    {
        public override string EquipmentName => "The Sand Sweeper";

        public override string EquipmentLangTokenName => "SAND_SWEEPER";

        public override string EquipmentPickupDesc => "Sweeps sand around you.";

        public override string EquipmentFullDescription => $"Pushes all enemies within $sd{range}m$se, dealing up to $sd{d(maxDamage)} damage$se and $sustunning$se for up to $su{maxStun} seconds$se based on the distance.".AutoFormat();

        public override string EquipmentLore => "<style=cMono>//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//\r\n\r\n</style>\"Beginning recording. This is UESC field officer [Redacted], on the mission to retrieve UES document 4738, which was stolen by ex-UES technician Elijah Brettle (employee ID 38a5899f8dc76b). I am here with two other members of the strike team, [Redacted] and [Redacted], who will step in if I am in need of protection. We are currently stationed 1.2 kilometers away from the Hall of the Revered on Mars, where the target is seemingly taking refuge. We have brought in one of the Hall's employees for questioning.\"\r\n\r\n\"I'm a devotee, not an employee. They pay me with their hospitality, and I work out of reverence and the goodness of my heart.\"\r\n\r\n\"My apologies, ma'am. What is it you do for the Hall?\"\r\n\r\n\"I sweep the debris off the courtyards each morning, with this broom here. Makes sure they stay fittingly pristine.\"\r\n\r\n\"Anything else?\"\r\n\r\n\"I practice devotion in the same ways as every other devotee, but that is as far as my own duty extends.\"\r\n\r\n\"I see. Could you please state your name for the record?\"\r\n\r\n\"I don't think I will be doing that.\"\r\n\r\n\"We just have a few small questions for you. We're not here to upset anything. Your cooperation would be highly appreciated.\"\r\n\r\n\"How stupid do you think I am? You've brought me all the way here to your ship, which is conveniently hidden behind a rock outcropping. You've got two bodyguards behind you, ready to kill me if I try to act up. Try as you might to whisper, it was quite plain that you just said you're here to capture one of the Hall's devotees into that recording device.\"\r\n\r\n\"No need to be so hostile. If you would just work with us, we can both walk away happy. Now, if you please: is Elijah Brettle taking refuge in the Hall of the Revered?\"\r\n\r\n\"You know the answer to that already. And no, we won't be relinquishing him to you, no matter how much you or your big UESC bosses puff up your chests. Elijah came to us specifically to hide from you. Three different asylums he's had to flee from, because of you lot. Like we told him, we're the only ones who won't crumble under your corporate boot.\"\r\n\r\n\"Please calm down, we're not trying to make anyone upset here. Is there any way you could let us into the Hall to—\"\r\n\r\n\"No. No, there isn't. You're clearly ignorant of the past, but there's a reason the Hall has stayed upright this long. We've survived UESC before, and we've survived far worse, too. Leave this place at once. We will not stand by and let you harm one of our devotees.\"\r\n\r\n\"Ma'am, I don't think you understand the gravity of the situation. This fugitive has a document containing highly classified information. This is a matter of law, and it is not the place of the Hall of the Revered to interfere, with all due respect.\"\r\n\r\n\"'All due respect' would be leaving us alone. Let me off this ship this instant, or I will defend myself and devotees of the Hall you threaten.\"\r\n\r\n\"That's enough. [Redacted], shut the door. You WILL answer our questions; the UESC doesn't—\"\r\n\r\n<style=cMono>[9:23:08] Error: Recording malfunction in device 380a178c86f3209b. Pausing transcription.\r\n\r\n[9:26:52] Recording device 592ac439fda34e9c online. Resuming transcription with new device.\r\n\r\n</style>\"Damn UES corporate...\"\r\n\r\n[unintelligible]\r\n\r\n\"Good. Okay, you suits listening to this right now better understand me better than those muscles did. If you have even a shred of moral integrity, or even just a shred of brain matter, you won't come for us. A glance at the history books should tell you what happens when you threaten the Hall. I don't know which of you was dumb enough to send your little strike team here, but ensure this mistake is not repeated, or it will mean the end of your organization. We'll send your goons back to you in a few days.\r\n\r\n\"Oh, and, we're taking your ship.\"\r\n\r\n<style=cMono>[9:27:21] System shutdown. Ending transcription.</style>";

        public override GameObject EquipmentModel => prodAssets.LoadAsset<GameObject>("assets/sandswept/sandsweeper.fbx");

        public override Sprite EquipmentIcon => hifuSandswept.LoadAsset<Sprite>("texSandSweeper.png");
        public override float Cooldown => 30f;

        [ConfigField("Sweep Radius", "", 20f)]
        public static float range;

        [ConfigField("Sweep Force", "", 750f)]
        public static float force;

        [ConfigField("Minimum Sweep Damage", "Decimal.", 2f)]
        public static float minDamage;

        [ConfigField("Maximum Sweep Damage", "Decimal.", 5f)]
        public static float maxDamage;

        [ConfigField("Minimum Stun Duration", "", 2f)]
        public static float minStun;

        [ConfigField("Maximum Stun Duration", "", 6f)]
        public static float maxStun;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        public static readonly SphereSearch sphereSearch = new SphereSearch();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.characterBody == null) return false;
            EffectManager.SimpleSoundEffect(EntityStates.Croco.BaseLeap.landingSound.index, slot.characterBody.footPosition, transmit: true); // sandleep! BRUH LMFAO
            EffectManager.SpawnEffect(Paths.GameObject.Bandit2SmokeBomb, new EffectData() { origin = slot.characterBody.footPosition, scale = range / 12f }, true);
            sphereSearch.origin = slot.characterBody.corePosition;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = range;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(slot.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            List<HurtBox> dests = new();
            sphereSearch.GetHurtBoxes(dests);
            sphereSearch.ClearCandidates();
            foreach (HurtBox dest in dests)
            {
                CharacterBody body = dest?.healthComponent?.body;
                if (body?.characterMotor == null) continue;
                float dist = Vector3.Distance(slot.characterBody.corePosition, body.corePosition);
                Vector3 temp = body.corePosition - slot.characterBody.corePosition;
                temp.y = 0;
                body.healthComponent.TakeDamage(new DamageInfo()
                {
                    attacker = slot.gameObject,
                    inflictor = slot.gameObject,
                    damage = slot.characterBody.damage * Mathf.Lerp(maxDamage, minDamage, dist / range),
                    damageColorIndex = DamageColorIndex.Item,
                    force = ((range - dist) * Vector3.Normalize(temp) + (Vector3.up * Mathf.Lerp(10, 5, dist / range))) * force,
                    procCoefficient = procCoefficient
                });
                body.healthComponent.GetComponent<SetStateOnHurt>()?.SetStun(Mathf.Lerp(maxStun, minStun, dist / range));
            }
            slot.characterBody.characterMotor.velocity = Vector3.ProjectOnPlane(slot.characterBody.characterMotor.velocity, slot.gameObject.transform.up) + (slot.gameObject.transform.up * StealthMode.shortHopVelocity);
            return true;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(); // Yes i am Copying Recycler Manually, Again
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, -0.256f),
                    localAngles = new Vector3(0, 90, 348.7988f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.0183f, 0.1247f, 0.2044f),
                    localAngles = new Vector3(0, 90, 177.358f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Chest",
                    localPos = new Vector3(0, -0.087f, -0.204f),
                    localAngles = new Vector3(0, 90, 348.7988f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Hip",
                    localPos = new Vector3(-1.642f, 1.177f, 0),
                    localAngles = new Vector3(0, 90, 180),
                    localScale = new Vector3(0.6462f, 0.6462f, 0.6462f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0.2037f, 0.1072f, -0.0098f),
                    localAngles = new Vector3(52.5723f, 269.3417f, 269.0913f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Stomach",
                    localPos = new Vector3(-0.1005f, -0.076f, 0.1862f),
                    localAngles = new Vector3(0, 252.3484f, 0),
                    localScale = new Vector3(0.0551f, 0.0551f, 0.0551f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Stomach",
                    localPos = new Vector3(-0.1005f, -0.076f, 0.1862f),
                    localAngles = new Vector3(0, 252.3484f, 0),
                    localScale = new Vector3(0.0551f, 0.0551f, 0.0551f)
                }
            });
            rules.Add("mdlPaladin", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Chest",
                    localPos = new Vector3(-0.2011f, 0.098f, -0.3147f),
                    localAngles = new Vector3(0, 117.8714f, 0),
                    localScale = new Vector3(0.1008f, 0.1008f, 0.1008f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.201f, -0.378f),
                    localAngles = new Vector3(0, 90, 23.3119f),
                    localScale = new Vector3(0.1276f, 0.1276f, 0.1276f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.651f, 0.487f, -0.669f),
                    localAngles = new Vector3(0, 131.7342f, 0),
                    localScale = new Vector3(0.1333f, 0.1333f, 0.1333f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Chest",
                    localPos = new Vector3(0.2673f, 0.0405f, 0.1688f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.0579f, 0.0579f, 0.0579f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "Hip",
                    localPos = new Vector3(1.98f, 2.553f, 0.541f),
                    localAngles = new Vector3(34.9679f, 126.7749f, 162.9854f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "TopRail",
                    localPos = new Vector3(-0.001f, 0.497f, 0.059f),
                    localAngles = new Vector3(90, 270, 0),
                    localScale = new Vector3(0.045f, 0.045f, 0.045f)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]{
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "ForearmR",
                    localPos = new Vector3(0.1788f, 0.2191f, 0.1402f),
                    localAngles = new Vector3(55.8532f, 198.9949f, 166.9621f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }
    }
}