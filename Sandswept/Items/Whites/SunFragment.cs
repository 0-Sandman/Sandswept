﻿using BepInEx.Configuration;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Whites
{
    public class SunFragment : ItemBase<SunFragment>
    {
        public static DamageColorIndex SolarFlareColour = DamageColourHelper.RegisterDamageColor(new Color32(255, 150, 25, 255));

        public static DamageAPI.ModdedDamageType SolarFlareDamageType;

        public override string ItemName => "Sun Fragment";

        public override string ItemLangTokenName => "SUN_FRAGMENT";

        public override string ItemPickupDesc => "Create a blinding flash on hit that damages and stuns enemies";

        public override string ItemFullDescription => StringExtensions.AutoFormat("$su10%$se chance on hit to create a $sublinding flash$se in a $su10m$se $ss(+1m per stack)$se area, dealing $sd75%$se $ss(+50% per stack)$se TOTAL damage and $sustunning$se enemies for $su1s$se.\n\n$seThis effect deals a minimum of 150% (+150% per stack) damage$se.");

        public override string ItemLore => "Maybe less hell to code";

        public override string AchievementName => "A cycle, broken.";

        public override string AchievementDesc => "Destroy a child of the stars";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("SunFragmentIcon.png");

        public GameObject FragmentVFX;

        public static GameObject FragmentVFXSphere;

        public override void Init(ConfigFile config)
        {
            SolarFlareDamageType = DamageAPI.ReserveDamageType();

            FragmentVFX = Main.MainAssets.LoadAsset<GameObject>("FragmentFXRing.prefab");
            var component = FragmentVFX.AddComponent<EffectComponent>();
            component.applyScale = true;
            Main.EffectPrefabs.Add(FragmentVFX);

            FragmentVFXSphere = Main.MainAssets.LoadAsset<GameObject>("FragmentFXSphere.prefab");
            var Renderer = FragmentVFXSphere.GetComponent<ParticleSystemRenderer>();
            var val = FragmentVFXSphere.AddComponent<HGIntersectionController>();
            val.Renderer = Renderer;
            var val3 = val.Renderer.material;
            Material val4 = Object.Instantiate(val3);
            val4.SetColor("_TintColor", new Color32(255, 120, 0, 255));
            val4.SetTexture("_Cloud1Tex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Grandparent/texGrandparentDetailGDiffuse.png").WaitForCompletion());
            val4.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampParentTeleport.png").WaitForCompletion());
            val4.SetFloat("_IntersectionStrength", 0.95f);
            Renderer.material = val4;
            var component2 = FragmentVFXSphere.AddComponent<EffectComponent>();
            component2.applyScale = true;
            Main.EffectPrefabs.Add(FragmentVFXSphere);

            CreateLang();
            CreateUnlockLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += EnemyHit;
        }

        // what the shit :Trolley
        public void EnemyHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker || !victim)
            {
                return;
            }
            bool fromProc = damageInfo.HasModdedDamageType(SolarFlareDamageType);

            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
            EntityStateMachine stateMachine = victim.GetComponent<EntityStateMachine>();

            int stacks = GetCount(attackerBody);

            if (!fromProc && !damageInfo.rejected)
            {
                if (stacks > 0)
                {
                    if (Util.CheckRoll(10f * damageInfo.procCoefficient, attackerBody.master))
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = victimBody.corePosition,
                            rotation = Util.QuaternionSafeLookRotation(damageInfo.force != Vector3.zero ? damageInfo.force : Random.onUnitSphere),
                            scale = 9f + stacks
                        };
                        EffectData effectData2 = new EffectData
                        {
                            origin = victimBody.corePosition,
                            scale = 9f + stacks
                        };
                        EffectManager.SpawnEffect(FragmentVFX, effectData, true);
                        EffectManager.SpawnEffect(FragmentVFXSphere, effectData2, true);

                        float effectDamage = damageInfo.damage * (0.75f + 0.50f * (stacks - 1));

                        if (effectDamage < attackerBody.damage * (1.5f * stacks))
                        {
                            effectDamage = attackerBody.damage * (1.5f * stacks);
                        }
                        BlastAttack blastAttack = new BlastAttack
                        {
                            radius = 9f + stacks,
                            baseDamage = effectDamage,
                            procCoefficient = 0f,
                            crit = damageInfo.crit,
                            damageColorIndex = SolarFlareColour,
                            attackerFiltering = AttackerFiltering.NeverHitSelf,
                            falloffModel = BlastAttack.FalloffModel.None,
                            attacker = attackerBody.gameObject,
                            teamIndex = attackerBody.teamComponent.teamIndex,
                            position = damageInfo.position,
                        };
                        blastAttack.AddModdedDamageType(SolarFlareDamageType);
                        blastAttack.Fire();
                    }
                }
            }
            if (fromProc)
            {
                if (stateMachine)
                {
                    if (stateMachine.state is StunState)
                    {
                        StunState stunState = stateMachine.state as StunState;
                        if (stunState.timeRemaining < 1f)
                        {
                            stunState.ExtendStun(0.35f - stunState.timeRemaining);
                        }
                    }
                    else
                    {
                        StunState state = new StunState
                        {
                            stunDuration = 1f
                        };
                        stateMachine.SetInterruptState(state, InterruptPriority.Pain);
                    }
                }
            }
            orig(self, damageInfo, victim);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}