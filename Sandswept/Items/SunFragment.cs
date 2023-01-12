using BepInEx.Configuration;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class SunFragment : ItemBase<SunFragment>
    {
        public static DamageColorIndex SolarFlareColour = DamageColourHelper.RegisterDamageColor(new Color32(255, 200, 40, 255));

        public static DamageAPI.ModdedDamageType SolarFlareDamageType;

        public override string ItemName => "Sun Fragment";

        public override string ItemLangTokenName => "SUN_FRAGMENT";

        public override string ItemPickupDesc => "Create a blinding flash on hit that damages and stuns enemies";

        public override string ItemFullDescription => "<style=cIsUtility>5%</style> chance on hit to create a <style=cIsUtility>blinding flash</style> in a <style=cIsUtility>10m</style> <style=cStack>(+1m per stack)</style> area, dealing <style=cIsDamage>20%</style> <style=cStack>(+20% per stack)</style> TOTAL damage and <style=cIsUtility>stunning</style> enemies for <style=cIsUtility>0.25s</style>, ";

        public override string ItemLore => "Maybe less hell to code";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("SunFragmentIcon.png");


        public override void Init(ConfigFile config)
        {
            SolarFlareDamageType = DamageAPI.ReserveDamageType();
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += EnemyHit;
        }

        public void EnemyHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker || !victim)
            {
                return;
            }
            bool fromProc = DamageAPI.HasModdedDamageType(damageInfo, SolarFlareDamageType);

            CharacterBody attackerbody = damageInfo.attacker.GetComponent<CharacterBody>();
            EntityStateMachine stateMachine = victim.GetComponent<EntityStateMachine>();

            int stacks = GetCount(attackerbody);

            if (!fromProc && !damageInfo.rejected)
            {
                if (stacks > 0)
                {
                    if (Util.CheckRoll(5f * damageInfo.procCoefficient, attackerbody.master))
                    {
                        BlastAttack blastAttack = new BlastAttack
                        {
                            radius = 9f + stacks,
                            baseDamage = damageInfo.damage * (0.20f * stacks),
                            procCoefficient = 0f,
                            crit = damageInfo.crit,
                            damageColorIndex = SolarFlareColour,
                            attackerFiltering = AttackerFiltering.NeverHitSelf,
                            falloffModel = BlastAttack.FalloffModel.Linear,
                            attacker = attackerbody.gameObject,
                            teamIndex = attackerbody.teamComponent.teamIndex,
                            position = damageInfo.position,
                        };
                        DamageAPI.AddModdedDamageType(blastAttack, SolarFlareDamageType);
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
                        if (stunState.timeRemaining < 0.35f)
                        {
                            stunState.ExtendStun(0.35f - stunState.timeRemaining);
                        }
                    }
                    else
                    {
                        StunState state = new StunState();
                        state.stunDuration = 0.35f;
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
