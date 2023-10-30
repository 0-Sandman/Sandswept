using System;
using System.Linq;

namespace Sandswept.Items.Greens
{
    public class NuclearSalvo : ItemBase<NuclearSalvo>
    {
        public override string ItemName => "Nuclear Salvo";

        public override string ItemLangTokenName => "NUCLEAR_SALVO";

        public override string ItemPickupDesc => "Mechanical allies fire nuclear warheads periodically.";

        public override string ItemFullDescription => "Every $sd10 seconds$se $ss(-25% per stack)$se, all mechanical allies fire $sunuclear missiles$se that deal $sd3x100%$se base damage and $sdignite$se on hit.".AutoFormat();

        public override string ItemLore => "TBD";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/NuclearSalvoHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texNuclearSalvo.png");
        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

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
        public int MissileCount;
        public GameObject MissilePrefab;
        public float MissileDamageCoefficient = 3f;
        public float BaseMissileDelay;
        private float MissileDelay => BaseMissileDelay * Mathf.Pow(0.75f, attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) - 1);
        private float stopwatch = 0f;

        public void Start()
        {
            stopwatch = BaseMissileDelay;
        }

        public void FixedUpdate()
        {
            stopwatch -= Time.fixedDeltaTime;

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
                        projectilePrefab = MissilePrefab
                    };

                    Debug.Log(attachment.attachedBody);

                    ProjectileManager.instance.FireProjectile(info);
                }

                if (attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) <= 0)
                {
                    Destroy(gameObject);
                }
            }

            if (attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}