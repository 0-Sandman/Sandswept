using System;
using System.Linq;

namespace Sandswept.Items.Greens {
    public class NuclearSalvo : ItemBase<NuclearSalvo>
    {
        public override string ItemName => "Nucler Salvo";

        public override string ItemLangTokenName => "SS_SALVO";

        public override string ItemPickupDesc => "Mechanical allies fire nuclear warheads periodically.";

        public override string ItemFullDescription => "Every $sd10 seconds$se $ss(-25% per stack)$se, mechanical allies fire $sunuclear missiles$se for $sd3x100%$se, igniting hit targets.";

        public override string ItemLore => "N/A";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;
        public GameObject SalvoPrefab;
        public GameObject SalvoMissile;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            base.Hooks();

            SalvoPrefab = Main.Assets.LoadAsset<GameObject>("SalvoBehaviour.prefab");
            SalvoMissile = Main.Assets.LoadAsset<GameObject>("Missile.prefab");
            On.RoR2.Inventory.GiveItem_ItemDef_int += GiveItem;
            On.RoR2.CharacterBody.OnInventoryChanged += RecheckItems;
        }

        public void GiveItem(On.RoR2.Inventory.orig_GiveItem_ItemDef_int orig, Inventory self, ItemDef def, int count) {
            orig(self, def, count);
            if (NetworkServer.active && def == ItemDef && self.GetComponent<PlayerCharacterMasterController>()) {
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.GetComponent<CharacterMaster>()).ToList();

                foreach (CharacterMaster cm in masters) {
                    if (cm.inventory.GetItemCount(def) == 0) {
                        cm.inventory.GiveItem(def);
                        GameObject attachment = GameObject.Instantiate(SalvoPrefab);
                        attachment.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(cm.GetBodyObject());
                    }
                }
            }
        }

        public void RecheckItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);

            if (NetworkServer.active && self.isPlayerControlled && self.inventory.GetItemCount(ItemDef) == 0) {
                List<CharacterMaster> masters = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master).ToList();

                foreach (CharacterMaster cm in masters) {
                    cm.inventory.RemoveItem(ItemDef);
                }
            }
        }
    }

    public class SalvoBehaviour : MonoBehaviour {
        public NetworkedBodyAttachment attachment;
        public int MissileCount;
        public GameObject MissilePrefab;
        public float MissileDamageCoefficient;
        public float BaseMissileDelay;
        private float MissileDelay => BaseMissileDelay * Mathf.Pow(0.75f, attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef));
        private float stopwatch = 0f;

        public void Start() {
            stopwatch = BaseMissileDelay;
        }

        public void FixedUpdate() {
            stopwatch -= Time.fixedDeltaTime;

            if (stopwatch <= 0) {
                stopwatch = MissileDelay;

                for (int i = 0; i < MissileCount; i++) {
                    FireProjectileInfo info = new();
                    info.crit = false;
                    info.damage = attachment.attachedBody.damage * MissileDamageCoefficient;
                    info.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(attachment.attachedBody.inputBank.aimDirection, -3f, 3f, 1f, 1f));
                    info.position = attachment.attachedBody.transform.position;
                    info.owner = attachment.attachedBody.gameObject;
                    info.projectilePrefab = MissilePrefab;
                    
                    ProjectileManager.instance.FireProjectile(info);
                }

                if (attachment.attachedBody.inventory.GetItemCount(NuclearSalvo.instance.ItemDef) == 0) {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}