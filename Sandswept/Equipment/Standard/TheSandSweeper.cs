using EntityStates.Bandit2;
using static Sandswept.Main;

namespace Sandswept.Equipment.Standard
{
    [ConfigSection("Equipment :: The Sand Sweeper")]
    public class TheSandSweeper : EquipmentBase<TheSandSweeper>
    {
        public override string EquipmentName => "The Sand Sweeper";

        public override string EquipmentLangTokenName => "THE_SAND_SWEEPER";

        public override string EquipmentPickupDesc => "Sweeps sand around you.";

        public override string EquipmentFullDescription => $"Pushes all enemies within $sd{range}m$se, dealing up to $sd{maxDamage * 100f}% damage$se and $sustunning$se for up to $su{maxStun}$se seconds based on the distance.".AutoFormat();

        public override string EquipmentLore =>
        """
        <style=cMono>
        //--AUTO-TRANSCRIPTION FROM UES [Redacted] --//
        </style>
        "Beginning recording. This is UESC field officer [Redacted], on the mission to retrieve UES document 4738, which was stolen by ex-UES technician Elijah Brettle (employee ID 38a5899f8dc76b). I am here with two other members of the strike team, [Redacted] and [Redacted], who will step in if I am in need of protection. We are currently stationed 1.2 kilometers away from the Hall of the Revered on Mars, where the target is seemingly taking refuge. We have brought in one of the Hall's employees for questioning."

        "I'm a devotee, not an employee. They pay me with their hospitality, and I work out of reverence and the goodness of my heart."

        "My apologies, ma'am. What is it you do for the Hall?"

        "I sweep the debris off the courtyards each morning, with this broom here. Make sure they stay fittingly pristine."

        "Anything else?"

        "I practice devotion in the same ways as every other devotee, but that is as far as my own duty extends."

        "I see. Could you please state your name for the record?"

        "I don't think I will be doing that."

        "We just have a few small questions for you. We're not here to upset anything. Your cooperation would be highly appreciated."

        "How stupid do you think I am? You've brought me all the way here to your ship, which is conveniently hidden behind a rock outcropping. You've got two bodyguards behind you, ready to kill me if I try to act up. Try as you might to whisper, it was quite plain that you just said you're here to capture one of the Hall's devotees into that recording device."

        "No need to be so hostile. If you would just work with us, we can both walk away happy. Now, if you please: is Elijah Brettle taking refuge in the Hall of the Revered?"

        "You know the answer to that already. And no, we won't be relinquishing him to you, no matter how much you or your big UESC bosses puff up your chests. Elijah came to us specifically to hide from you. Three different asylums he's had to flee from, because of you lot. Like we told him, we're the only ones who won't crumble under your corporate boot."

        "Please calm down, we're not trying to make anyone upset here. Is there any way you could let us into the Hall to--"

        "No. No, there isn't. You're clearly ignorant of the past, but there's a reason the Hall has stayed upright this long. We've survived UESC before, and we've survived far worse, too. Leave this place at once. We will not stand by and let you harm one of our devotees."

        "Ma'am, I don't think you understand the gravity of the situation. This fugitive has a document containing highly classified information. This is a matter of law, and it is not the place of the Hall of the Revered to interfere, with all due respect."

        "'All due respect' would be leaving us alone. Let me off this ship this instant, or I will defend myself and devotees of the Hall you threaten."

        "That's enough. [Redacted], shut the door. You WILL answer our questions; the UESC doesn't--"
        <style=cMono>
        [9:23:08] Error: Recording malfunction in device 380a178c86f3209b. Pausing transcription.

        [9:26:52] Recording device 592ac439fda34e9c online. Resuming transcription with new device.
        </style>
        "Damn UES corporate..."

        [unintelligible]

        "Good. Okay, you suits listening to this right now better understand me better than those muscles did. If you have even a shred of moral integrity, or even just a shred of brain matter, you won't come for us. A glance at the history books should tell you what happens when you threaten the Hall. I don't know which of you was dumb enough to send your little strike team here, but ensure this mistake is not repeated, or it will mean the end of your organization. We'll send your goons back to you in a few days.

        "Oh, and, we're taking your ship."
        <style=cMono>
        [9:27:21] System shutdown. Ending transcription.
        </style>
        """;

        public override GameObject EquipmentModel => prodAssets.LoadAsset<GameObject>("assets/sandswept/sandsweeper.fbx");

        public override Sprite EquipmentIcon => hifuSandswept.LoadAsset<Sprite>("texSandSweeper.png");
        public override float Cooldown => 25f;

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

        public static GameObject vfx;

        public static Material overlayMat;

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.Bandit2SmokeBomb, "The Sand Sweeper VFX", false);

            vfx.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.OdpizdzijPierdoloneGownoKurwaCoZaJebanyKurwaSmiecToKurwaDodalPizdaKurwaJebanaKurwa(vfx);

            var lightColor = new Color32(239, 181, 79, 255);
            var sandColor = new Color32(113, 84, 32, 255);
            VFXUtils.RecolorMaterialsAndLights(vfx, sandColor, lightColor, true, true);
            VFXUtils.AddLight(vfx, lightColor, 15f, range, 1f);

            var transform = vfx.transform.Find("Core");
            transform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            transform.localPosition = Vector3.zero;

            var sparks = transform.Find("Sparks");
            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 400;
            var sparksEmission = sparksPS.emission;
            var burst = new ParticleSystem.Burst(0f, 400, 400, 1, 0.01f);
            burst.probability = 1f;
            sparksEmission.SetBurst(0, burst);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();
            sparksPSR.material.SetTexture("_MainTex", Paths.Texture2D.texGlowPaintMask);

            ContentAddition.AddEffect(vfx);

            VFXUtils.MultiplyDuration(vfx, 2.5f);

            overlayMat = new Material(Paths.Material.matHuntressFlashBright);

            overlayMat.SetColor("_TintColor", Color.white);
            overlayMat.SetTexture("_MainTex", null);
            overlayMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampDirectCurrentSandswept.png"));
            overlayMat.SetFloat("_InvFade", 0f);
            overlayMat.SetFloat("_Boost", 1f);
            overlayMat.SetFloat("_AlphaBoost", 1.06f);
            overlayMat.SetFloat("_AlphaBias", 0f);
            overlayMat.SetInt("_SrcBlend", 7);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.characterBody == null)
            {
                return false;
            }

            Util.PlaySound("Play_bison_charge_attack_end_skid", slot.gameObject);
            EffectManager.SimpleSoundEffect(EntityStates.Croco.BaseLeap.landingSound.index, slot.characterBody.footPosition, transmit: true); // sandleep! BRUH LMFAO
            for (int i = 0; i < 4; i++)
            {
                Util.PlaySound("Play_Player_footstep", slot.gameObject);
                Util.PlaySound("Play_beetle_guard_impact", slot.gameObject);
            }
            EffectManager.SpawnEffect(vfx, new EffectData() { scale = range, origin = slot.characterBody.footPosition }, true);

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
                var healthComponent = dest.healthComponent;
                if (!healthComponent)
                {
                    continue;
                }

                var body = healthComponent.body;
                if (!body)
                {
                    continue;
                }

                var distanceToTarget = Vector3.Distance(slot.characterBody.corePosition, body.corePosition);

                var heightIgnoredDistance = body.corePosition - slot.characterBody.corePosition;
                heightIgnoredDistance.y = 0;

                var damageCoeff = Mathf.Lerp(maxDamage, minDamage, distanceToTarget / range);

                var damageInfo = new DamageInfo()
                {
                    attacker = slot.gameObject,
                    inflictor = slot.gameObject,
                    damage = slot.characterBody.damage * damageCoeff,
                    damageColorIndex = DamageColorIndex.Item,
                    force = ((range - distanceToTarget) * Vector3.Normalize(heightIgnoredDistance) + Vector3.up * Mathf.Lerp(10, 5, distanceToTarget / range)) * force,
                    procCoefficient = procCoefficient
                };

                if (slot.characterBody.teamComponent && slot.characterBody.teamComponent.teamIndex != TeamIndex.Player)
                {
                    damageInfo.damageType = DamageType.NonLethal;
                }

                body.healthComponent.TakeDamage(damageInfo);

                if (body.TryGetComponent<SetStateOnHurt>(out var setStateOnHurt))
                {
                    setStateOnHurt.SetStun(Mathf.Lerp(maxStun, minStun, distanceToTarget / range));
                }

                var modelLocator = body.modelLocator;
                if (!modelLocator)
                {
                    continue;
                }

                var modelTransform = modelLocator.modelTransform;
                if (!modelTransform)
                {
                    continue;
                }

                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = damageCoeff * 2f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }
            if (slot.characterBody.characterMotor)
            {
                slot.characterBody.characterMotor.velocity = Vector3.ProjectOnPlane(slot.characterBody.characterMotor.velocity, slot.gameObject.transform.up) + slot.gameObject.transform.up * StealthMode.shortHopVelocity;
            }

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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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
                    followerPrefabAddress = new("useless"),
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