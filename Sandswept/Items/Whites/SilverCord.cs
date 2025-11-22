/*
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

        public override string ItemFullDescription => $"$suGain a Shock Drone.$se Your drones have a $sd{chance}%$se chance to $sushock$se up to $sd{baseMaxTargets}$se $ss(+{stackMaxTargets} per stack)$se targets for $sd{damage * 100f}% TOTAL damage$se.".AutoFormat();

        public override string ItemLore =>
        """
        Transcripted call from the Head Executive of UES and the Head Executive of HDTF Inc.

        "Mr. [Redacted] how the FUCK did you lose EVERY FUCKING DRONE WE SENT YOU?! EVERY. LAST. ONE. HOW?!"

        "Mr. [Redacted], please calm down, I assure you it's a very unfortunate series of events! I-"

        "I need a better explanation than 'Whoopsies can't do anything', how do you lose 10,000 of each drone, 20,000 boxes of spare parts, 5,000 companion drones we didn't even GIVE you, 800 new nuclear salvos, and FIVE TC-prototypes in the span of a god damn year. If you lose this next batch of electrician drones I will have your head as decoration in my penthouse, do you understand me [Redacted], You owe me so much money it's not funny"

        "Have you not heard of the Contact L-"

        "Contact my fucking ass- I want double the amount of credits those cost. We called it the "Silver Cord" package because your asses over at that over glorified pigeon carrier service are going to have to PRAY we don't slam you with lawsuits"
        """;
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

        [ConfigField("Damage", "Decimal.", 3f)]
        public static float damage;

        [ConfigField("Range", "", 28f)]
        public static float range;

        [ConfigField("Proc Coefficient", "", 0.2f)]
        public static float procCoefficient;

        public static LazyAddressable<GameObject> ShockEffect = new(() => Paths.GameObject.MageLightningBombExplosion);
        public static ModdedProcType SilverShock = ProcTypeAPI.ReserveProcType();
        public static ModdedDamageType StupidButNeccessary = DamageAPI.ReserveDamageType();

        public override void Init()
        {
            base.Init();
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

                    var stack = owner.inventory.GetItemCount(ItemDef);

                    if (Util.CheckRoll(chance))
                    {
                        report.damageInfo.procChainMask.AddModdedProc(SilverShock);

                        LightningOrb orb = new()
                        {
                            damageValue = report.damageInfo.damage * damage,
                            bouncesRemaining = baseMaxTargets + stackMaxTargets * (stack - 1),
                            attacker = report.attacker,
                            damageColorIndex = DamageColorIndex.Item,
                            isCrit = report.damageInfo.crit,
                            origin = report.damageInfo.position,
                            procCoefficient = procCoefficient,
                            range = range,
                            teamIndex = report.attackerTeamIndex,
                            lightningType = LightningOrb.LightningType.Loader,
                            procChainMask = report.damageInfo.procChainMask,
                            canBounceOnSameTarget = false,
                            bouncedObjects = new()
                        };
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
*/