using System;
using System.Linq;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Nuclear Salvo")]
    public class NuclearSalvo : ItemBase<NuclearSalvo>
    {
        public override string ItemName => "Nuclear Salvo";

        public override string ItemLangTokenName => "NUCLEAR_SALVO";

        public override string ItemPickupDesc => "Mechanical allies fire nuclear warheads periodically.";

        public override string ItemFullDescription => ("Every $sd" + baseInterval + " seconds$se $ss(-" + d(stackIntervalReduction) + " per stack)$se, all mechanical allies fire $sdnuclear missiles$se that deal $sd" + missileCount + "x" + d(missileDamage) + "$se base damage and $sdignite$se on hit.").AutoFormat();

        public override string ItemLore => "TBD";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texNuclearSalvo.png");

        public override bool AIBlacklisted => true;

        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

        [ConfigField("Base Interval", "", 10f)]
        public static float baseInterval;

        [ConfigField("Stack Interval Reduction", "Decimal.", 0.25f)]
        public static float stackIntervalReduction;

        [ConfigField("Missile Count", "", 3)]
        public static int missileCount;

        [ConfigField("Missile Damage", "Decimal.", 1f)]
        public static float missileDamage;

        // for salvo display you can instantiate Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvo.prefab");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            base.Hooks();

            SalvoPrefab = Main.Assets.LoadAsset<GameObject>("SalvoBehaviour.prefab");
            SalvoMissile = Main.Assets.LoadAsset<GameObject>("Missile.prefab");
            ContentAddition.AddProjectile(SalvoMissile);
            ContentAddition.AddNetworkedObject(SalvoPrefab);
            On.RoR2.CharacterBody.RecalculateStats += GiveItem;
            On.RoR2.CharacterBody.OnInventoryChanged += RecheckItems;
        }

        public void GiveItem(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && self.isPlayerControlled && self.inventory.GetItemCount(ItemDef) > 0)
            {
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master).ToList();

                foreach (CharacterMaster cm in masters)
                {
                    if (cm.inventory.GetItemCount(ItemDef) < self.inventory.GetItemCount(ItemDef))
                    {
                        Debug.Log("giving salvo to drone");
                        cm.inventory.ResetItem(ItemDef);
                        cm.inventory.GiveItem(ItemDef, self.inventory.GetItemCount(ItemDef));
                        GameObject attachment = Object.Instantiate(SalvoPrefab);
                        attachment.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(cm.GetBodyObject());
                    }
                }
            }
        }

        public void RecheckItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active && self.isPlayerControlled && self.inventory.GetItemCount(ItemDef) == 0)
            {
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master).ToList();

                foreach (CharacterMaster cm in masters)
                {
                    cm.inventory.RemoveItem(ItemDef, cm.inventory.GetItemCount(ItemDef));
                }
            }
        }
    }

    public class SalvoBehaviour : MonoBehaviour
    {
        public NetworkedBodyAttachment attachment;
        public int MissileCount = NuclearSalvo.missileCount;
        public GameObject MissilePrefab;
        public float MissileDamageCoefficient = NuclearSalvo.missileDamage;
        public float BaseMissileDelay;
        private float MissileDelay => BaseMissileDelay * Mathf.Pow(1f - NuclearSalvo.stackIntervalReduction, attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) - 1);
        private float stopwatch = 0f;

        public void Start()
        {
            stopwatch = BaseMissileDelay;
        }

        public void FixedUpdate()
        {
            stopwatch -= Time.fixedDeltaTime;

            var stack = attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef);

            if (stopwatch <= 0)
            {
                stopwatch = MissileDelay;

                for (int i = 0; i < MissileCount; i++)
                {
                    Debug.Log("firing salvo missile");
                    FireProjectileInfo info = new()
                    {
                        crit = false,
                        damage = attachment.attachedBody.damage * MissileDamageCoefficient,
                        rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(attachment.attachedBody.inputBank.aimDirection, -10f, 10f, 1f, 1f)),
                        position = attachment.attachedBody.transform.position,
                        owner = attachment.attachedBody.gameObject,
                        projectilePrefab = MissilePrefab,
                        damageTypeOverride = DamageType.IgniteOnHit
                    };

                    Debug.Log(attachment.attachedBody);

                    ProjectileManager.instance.FireProjectile(info);
                }

                if (stack <= 0)
                {
                    Destroy(gameObject);
                }
            }

            if (stack <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}