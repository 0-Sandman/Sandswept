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

        public override string ItemFullDescription => ($"Activating your $suSecondary skill$se also swings $sdThe Endless Pierce$se for $sd{baseDamage * 100f}%$se base damage and grants $sh{basePlatingGain}$se $ss(+{stackPlatingGain} per stack)$se plating. $sdThe Endless Pierce$se renews over $sd{cooldown}$se seconds. $svCorrupts all Amber Knives$se.").AutoFormat();

        public override string ItemLore => "badass lore for a long cutlass that got corrupted and became sentient, choosing its owners based on strength of will (maybe a mercenary got captured?) :trollshrug:";

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