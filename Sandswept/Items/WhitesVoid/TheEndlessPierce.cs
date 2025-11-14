/*
using HarmonyLib;
using Sandswept.Items.Greens;
using Sandswept.Items.Whites;
using System.Collections;
using UnityEngine.UIElements;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace Sandswept.Items.VoidWhites
{
    [ConfigSection("Items :: The Endless Pierce")]
    public class TheEndlessPierce : ItemBase<TheEndlessPierce>
    {
        public override string ItemName => "The Endless Pierce";

        public override string ItemLangTokenName => "THE_ENDLESS_PIERCE";

        public override string ItemPickupDesc => "Activating your secondary skill also swings The Endless Pierce. Recharges over time. $svCorrupts all Amber Knives$se.".AutoFormat();

        public override string ItemFullDescription => $"Activating your $suSecondary skill$se also swings $sdThe Endless Pierce$se for $sd{baseDamage * 100f}%$se base damage and grants $sh{basePlatingGain}$se $ss(+{stackPlatingGain} per stack)$se plating. $sdThe Endless Pierce$se renews over $sd{cooldown}$se seconds. $svCorrupts all Amber Knives$se.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>//--AUTO-TRANSCRIPTION FROM ROOM 211 OF HALL OF THE REVERED --//</style>

        "New relic came in today. We can't get stuff shipped through UES anymore, so we had a courier pick it up. Be careful with this one, it's dangerous."

        "How so? Seems pretty unassuming."

        "We got it from a ruptured cell. This was the only thing from it that made it back to our plane. Examinations suggest it was there for trillions of years, by far the oldest sample the void's ever given us. It didn't come out unscathed, either."

        "By the heavens, trillions? ...wait, what do you mean, it didn't come out unscathed? I thought the cells encoded their contents perfectly."

        "As did everyone else. This...item exists as a counterpoint. Kind of hard to see on the surface, but there's all kinds of life forms attached to it. They're like nothing we've ever seen. I guess it was there long enough for an entire ecosystem to evolve around it."

        "How? I was under the impression the cells don't store the actual objects, just compressed data or something."

        "That's the question on everyone's mind. Seems like the voidborne organisms found a way to adapt to an environment of even that complexity. I suppose there's a lot of stuff to feed off in the cells, after all. Artifacts from other cells are being investigated, but no similar phenomena have been found -- then again, this is a very special case."

        "What's this ecosystem do, then? Is it useful?"

        "So far, nothing. Examinations have proven the organisms have complex structures and capabilities, but so far they haven't used any of them. They just sit there, like borderline-invisible barnacles. They haven't dulled its edge at all, either."

        "So what, it's just an ordinary sword with some fancy ornaments to appeal to the scientists? What do you want me to do with it?"

        "Well, you're the most competent with weapons here, obviously. We thought you could, y'know, swing it around a bit. See if it does anything interesting."

        "Okay. I'll take it to the tr-- whoa!"

        "Heavens, what's it doing? Drop it already! It could be dangerous!"

        "Sorry, I just...I don't know, something came over me. Seems like it's settled down, though."

        "Yeah. Well, I guess we figured out what those organisms do. Seems like they're active now. We should take it in for examination.

        ...

        "That means give it to me."

        "Right, sorry."

        "Thanks. Wait, what? Where...?"

        "Here, give it back to me for a minute."

        "Okay, sure. Are y-- Whoa! How're you making it do that?"

        "I'm not doing anything. I guess it just likes me more than you and your sterile science pals."

        "If you say so. Can you take it into the lab, then?"

        "Sure, I guess. This thing wants to be a weapon, though, not a microscope slide. I can tell. Should be a great addition to my arsenal for Project Bulwark."
        """;
        
        [ConfigField("Base Damage", "Decimal.", 3f)]
        public static float baseDamage;

        [ConfigField("Base Plating Gain", "", 8f)]
        public static float basePlatingGain;

        [ConfigField("Stack Plating Gain", "", 8f)]
        public static float stackPlatingGain;

        [ConfigField("Cooldown", "", 3f)]
        public static float cooldown;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        public static BuffDef cooldownBuff;

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("DisplayFesteringHunger.prefab");

        public override Sprite ItemIcon => Main.Assets.LoadAsset<Sprite>("texFesteringHunger.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.Healing };

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            if (!ItemBase.DefaultEnabledCallback(AmberKnife.instance))
            {
                return;
            }

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.canStack = false;
            cooldownBuff.isCooldown = true;
            cooldownBuff.buffColor = new Color32(96, 56, 177, 255);
            cooldownBuff.iconSprite = Utils.Assets.BuffDef.bdCrocoRegen.iconSprite;
            cooldownBuff.isHidden = false;
            cooldownBuff.isDebuff = false;
            cooldownBuff.name = "Endless Pierce - Cooldown";
            ContentAddition.AddBuffDef(cooldownBuff);

            vfx = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Assassin2/AssassinSlash.prefab").WaitForCompletion(), "The Endless Pierce VFX", false);

            var effectComponent = vfx.GetComponent<EffectComponent>();
            effectComponent.applyScale = true;

            var swingTrail = vfx.transform.GetChild(0);
            var swingTrailPS = swingTrail.GetComponent<ParticleSystem>();
            var rotationOverLifetime = swingTrailPS.rotationOverLifetime;
            rotationOverLifetime.zMultiplier = 1.1f;

            var swingTrailMat = swingTrail.GetComponent<ParticleSystemRenderer>();

            var newSwingMat = new Material(Paths.Material.matMercSwipe2);
            newSwingMat.SetColor("_TintColor", new Color32(96, 56, 177, 255));
            newSwingMat.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);
            newSwingMat.SetFloat("_Boost", 7f);
            swingTrailMat.material = newSwingMat;

            var swingDistortion = vfx.transform.GetChild(1).GetComponent<ParticleSystem>();
            var rotationOverLifetime2 = swingDistortion.rotationOverLifetime;
            rotationOverLifetime2.zMultiplier = 1.13f;

            ContentAddition.AddEffect(vfx);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private static void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = instance.ItemDef,
                itemDef1 = Whites.AmberKnife.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);

            orig();
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<TheEndlessPierceController>(GetCount(body));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class TheEndlessPierceController : CharacterBody.ItemBehavior
    {
        public SkillLocator skillLocator;
        public float damage = 3f;
        public float cooldown = 3f;
        public float platingGain = 0;

        // public BoxCollider boxCollider;
        public OverlapAttack overlapAttack;

        public ModelLocator modelLocator;
        public Transform modelTransform;
        public GameObject swordObject;
        public HitBoxGroup hitBoxGroup;
        public HitBox hitBox;

        public void Start()
        {
            modelLocator = GetComponent<ModelLocator>();
            modelTransform = modelLocator?.modelTransform;
            if (modelTransform && swordObject == null)
            {
                swordObject = new("The Endless Pierce Swing")
                {
                    layer = LayerIndex.defaultLayer.intVal
                };

                swordObject.transform.localScale = new Vector3(20f, 10f, 20f);

                hitBox = swordObject.AddComponent<HitBox>();
                hitBoxGroup = swordObject.AddComponent<HitBoxGroup>();
                hitBoxGroup.groupName = "TheEndlessPierce";
                hitBoxGroup.hitBoxes = new HitBox[] { hitBox };
            }
            damage = TheEndlessPierce.baseDamage;
            cooldown = TheEndlessPierce.cooldown;
            platingGain = TheEndlessPierce.basePlatingGain + TheEndlessPierce.stackPlatingGain * (stack - 1);
            skillLocator = GetComponent<SkillLocator>();
            body.onSkillActivatedServer += Body_onSkillActivatedServer;
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            var body = skill.GetComponent<CharacterBody>();
            if (!body)
            {
                return;
            }
            if (skill != skillLocator.secondary)
            {
                return;
            }

            if (body.HasBuff(TheEndlessPierce.cooldownBuff))
            {
                return;
            }

            StartCoroutine(FireProjectile());
        }

        public IEnumerator FireProjectile()
        {
            overlapAttack = new()
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                damage = body.damage * damage,
                forceVector = Vector3.zero,
                pushAwayForce = 0,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                impactSound = Paths.NetworkSoundEventDef.nseMercAssaulterImpact.index,
                procCoefficient = TheEndlessPierce.procCoefficient,
                isCrit = body.RollCrit()
            };

            if (swordObject && body.inputBank)
            {
                swordObject.transform.forward = body.inputBank.aimDirection;
                swordObject.transform.position = modelTransform.position;
                overlapAttack.hitBoxGroup = swordObject.GetComponent<HitBoxGroup>();

                Util.PlaySound("Play_bandit2_m2_slash", gameObject);

                EffectData data = new() { scale = 1.66f, origin = body.corePosition, rotation = Util.QuaternionSafeLookRotation(new Vector3(body.inputBank.aimDirection.x, 0f, body.inputBank.aimDirection.z)) };
                EffectManager.SpawnEffect(TheEndlessPierce.vfx, data, true);

                if (overlapAttack.Fire())
                {
                    // add plating hereee
                }

                body.AddTimedBuffAuthority(TheEndlessPierce.cooldownBuff.buffIndex, TheEndlessPierce.cooldown);
            }

            yield return null;
        }

        public void OnDestroy()
        {
            body.onSkillActivatedServer -= Body_onSkillActivatedServer;
        }
    }
}
*/