using RoR2.Orbs;
using System.Reflection;
using static R2API.DamageAPI;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Silver Cord")]
    internal class SilverCord : ItemBase<SilverCord>
    {
        public override string ItemName => "Silver Cord";

        public override string ItemLangTokenName => "SILVER_CORD";

        public override string ItemPickupDesc => "Gain a shock drone. Drone attacks have a chance to shock nearby targets.";

        public override string ItemFullDescription => $"$suGain a Shock Drone.$se Your drones have a $sd{chance}%$se chance to $sushock$se up to $sd{baseMaxTargets}$se $ss(+{stackMaxTargets} per stack)$se targets for $sd{baseDamageCoefficient * 100f} TOTAL damage$se.".AutoFormat();

        public override string ItemLore => "the silver cord is a king gizzard and the lizard wizard reference. you can make it related to that or just do whatever I guess";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("FracturedTimepieceHolder.prefab");
        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist };

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Max Targets", "", 2)]
        public static int baseMaxTargets;

        [ConfigField("Stack Max Targets", "", 1)]
        public static int stackMaxTargets;

        [ConfigField("Base Damage Coefficient", "", 3f)]
        public static float baseDamageCoefficient;

        public static LazyAddressable<GameObject> ShockEffect = new(() => Paths.GameObject.MageLightningBombExplosion);
        public static ModdedProcType SilverShock = ProcTypeAPI.ReserveProcType();
        public static ModdedDamageType StupidButNeccessary = DamageAPI.ReserveDamageType();

        public override void Init(ConfigFile config)
        {
            base.Init(config);
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += HandleShockAttacks;
        }

        private void HandleShockAttacks(DamageReport report)
        {
            if (report.damageInfo.HasModdedDamageType(StupidButNeccessary))
            { // orbs dont accept an impact effect so we bullshit this with a damage type
                EffectManager.SpawnEffect(ShockEffect, new EffectData
                {
                    origin = report.damageInfo.position,
                    scale = 1.5f
                }, true);
            }

            if (!report.victimBody || !report.attackerBody) return;

            if (report.damageInfo.procChainMask.HasModdedProc(SilverShock)) return;

            if (report.attackerBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical) && !report.attackerBody.isPlayerControlled)
            {
                if (report.attackerBody.master && report.attackerBody.master.minionOwnership)
                {
                    CharacterMaster owner = report.attackerBody.master.minionOwnership.ownerMaster;

                    if (!owner) return;

                    int c = owner.inventory.GetItemCount(ItemDef);

                    if (Util.CheckRoll(chance))
                    {
                        report.damageInfo.procChainMask.AddModdedProc(SilverShock);

                        LightningOrb orb = new();
                        orb.damageValue = report.damageInfo.damage * baseDamageCoefficient;
                        orb.bouncesRemaining = baseMaxTargets + (stackMaxTargets * (c - 1));
                        orb.attacker = report.attacker;
                        orb.damageColorIndex = DamageColorIndex.Item;
                        orb.isCrit = report.damageInfo.crit;
                        orb.origin = report.damageInfo.position;
                        orb.procCoefficient = 0.2f;
                        orb.range = 28;
                        orb.teamIndex = report.attackerTeamIndex;
                        orb.lightningType = LightningOrb.LightningType.Loader;
                        orb.procChainMask = report.damageInfo.procChainMask;
                        orb.canBounceOnSameTarget = false;
                        orb.bouncedObjects = new();
                        orb.AddModdedDamageType(StupidButNeccessary);
                        orb.target = orb.PickNextTarget(report.damageInfo.position);

                        if (orb.target)
                        {
                            OrbManager.instance.AddOrb(orb);
                        }
                    }
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}