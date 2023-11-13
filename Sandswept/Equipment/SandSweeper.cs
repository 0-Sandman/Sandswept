using static Sandswept.Main;

namespace Sandswept.Equipment
{
    [ConfigSection("Equipment: The Sand Sweeper")]
    public class SandSweeper : EquipmentBase
    {
        public override string EquipmentName => "The Sand Sweeper";

        public override string EquipmentLangTokenName => "SAND_SWEEPER";

        public override string EquipmentPickupDesc => "Sweeps sand around you.";

        public override string EquipmentFullDescription => $"Pushes all enemies within $sd{range}m$se around you back, $sudealing damage$se and $sustunning$se them based on the distance.".AutoFormat();

        public override string EquipmentLore => "<sprite name=\":joe_waiting:\"> #SANDSWEEP <sprite name=\":joe_cool:\">";

        public override GameObject EquipmentModel => Asset2s.LoadAsset<GameObject>("assets/sandswept/sandsweeper.fbx");

        public override Sprite EquipmentIcon => Asset2s.LoadAsset<Sprite>("assets/sandswept/sandsweepericon.png");
        public override float Cooldown => 24f;

        [ConfigField("Sweep Radius", "", 20f)]
        public static float range;
        [ConfigField("Sweep Force", "", 500f)]
        public static float force;
        [ConfigField("Minimum Sweep Damage", "Decimal.", 1.6f)]
        public static float minDamage;
        [ConfigField("Maximum Sweep Damage", "Decimal.", 8f)]
        public static float maxDamage;
        [ConfigField("Minimum Stun Duration", "", 2f)]
        public static float minStun;
        [ConfigField("Maximum Stun Duration", "", 8f)]
        public static float maxStun;
        [ConfigField("Proc Coefficient", "Decimal.", 1f)]
        public static float procco;

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
            EffectManager.SimpleSoundEffect(EntityStates.Croco.BaseLeap.landingSound.index, slot.characterBody.footPosition, transmit: true); // sandleep!
            EffectManager.SpawnEffect(Assets.GameObject.Bandit2SmokeBomb, new EffectData() { origin = slot.characterBody.footPosition, scale = range / 12f }, true);
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
                    procCoefficient = procco
                });
                body.healthComponent.GetComponent<SetStateOnHurt>()?.SetStun(Mathf.Lerp(maxStun, minStun, dist / range));
            }
            if (slot.characterBody?.characterMotor?.isGrounded ?? false)
                slot.characterBody.characterMotor.velocity = new Vector3(slot.characterBody.characterMotor.velocity.x, EntityStates.Bandit2.StealthMode.shortHopVelocity, slot.characterBody.characterMotor.velocity.z);
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