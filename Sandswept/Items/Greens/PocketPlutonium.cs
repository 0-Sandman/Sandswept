using UnityEngine.XR;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Pocket Plutonium")]
    public class PocketPlutonium : ItemBase<PocketPlutonium>
    {
        // I see you copied noop's wool code trolley

        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "Create a nuclear pool after losing shields.";

        public override string ItemFullDescription => ("Gain a $shshield$se equal to $sh" + d(basePercentShieldGain) + "$se of your maximum health. Upon losing all $shshield$se, create a nuclear pool in a $sd" + poolRadius + "m$se area that deals $sd" + d(poolBaseDamage) + "$se $ss(+" + d(poolStackDamage) + "per stack)$se base damage, plus an additional $sd" + d(poolBasePercentShieldDamage) + "$se $ss(+" + d(poolStackPercentShieldDamage) + " per stack)$se of $shshields$se.").AutoFormat();

        public override string ItemLore => "you have no idea how many times I reworked this item";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/PocketPlutoniumHolder.Prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texPocketPlutonium.png");

        public override bool AIBlacklisted => true;

        public static GameObject indicator;

        public static GameObject poolPrefab;

        [ConfigField("Base Percent Shield Gain", "Decimal.", 0.1f)]
        public static float basePercentShieldGain;

        [ConfigField("Pool Radius", "", 16f)]
        public static float poolRadius;

        [ConfigField("Pool Base Damage", "Decimal.", 10f)]
        public static float poolBaseDamage;

        [ConfigField("Pool Stack Damage", "Decimal.", 5f)]
        public static float poolStackDamage;

        [ConfigField("Pool Base Percent Shield Damage", "Decimal.", 3f)]
        public static float poolBasePercentShieldDamage;

        [ConfigField("Pool Stack Percent Shield Damage", "Decimal.", 2f)]
        public static float poolStackPercentShieldDamage;

        public static BuffDef pocketPlutoniumRecharge;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            // CreatePrefab();
            Hooks();
        }

        public override void Hooks()
        {
            pocketPlutoniumRecharge = ScriptableObject.CreateInstance<BuffDef>();
            pocketPlutoniumRecharge.isDebuff = false;
            pocketPlutoniumRecharge.canStack = false;
            pocketPlutoniumRecharge.buffColor = new Color32(115, 204, 71, 255);
            pocketPlutoniumRecharge.isHidden = true;
            pocketPlutoniumRecharge.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texProtogen3.png");

            ContentAddition.AddBuffDef(pocketPlutoniumRecharge);

            poolPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.HuntressArrowRain, "Pocket Plutonium Pool");
            poolPrefab.transform.localScale = Vector3.one * (poolRadius * 2f);
            var projectileDamage = poolPrefab.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var projectileDotZone = poolPrefab.GetComponent<ProjectileDotZone>();
            projectileDotZone.damageCoefficient = 0.25f;
            projectileDotZone.resetFrequency = 4f;
            projectileDotZone.lifetime = 5f;
            // hits 4x per sec for 25% of the damage, that is multiplied by 0.2 (down in the component) so does its full damage in 5s aka the entire lifetime

            // hitbox x,z scale of 0.9145525 = 7.5m radius
            // so 16m = (16 / 7.5) * 0.9145525
            // prefab has a scale of 15,15,15 = 7.5m radius, so 32,32,32 = 16m radius hopefully

            foreach (AkEvent akEvent in poolPrefab.GetComponents<AkEvent>())
            {
                Object.Destroy(akEvent);
            }

            var fx = poolPrefab.transform.GetChild(0);

            var radiusIndicator = fx.GetChild(0).GetComponent<MeshRenderer>();

            radiusIndicator.material = Main.hifuSandswept.LoadAsset<Material>("Assets/Sandswept/matPocketPlutoniumPool.mat");

            var hitbox1 = fx.GetChild(3);

            hitbox1.transform.localPosition = Vector3.zero;
            hitbox1.transform.localScale = new Vector3(0.9145522f, 0.1f, 0.9145525f);

            var hitbox2 = fx.GetChild(4);
            hitbox2.transform.localScale = new Vector3(0.914552f, 0.1f, 0.9145527f);

            hitbox2.transform.localPosition = Vector3.zero;

            var arrowsFalling = fx.GetChild(1);
            arrowsFalling.gameObject.SetActive(false);

            var impaledArrow = fx.GetChild(5);
            impaledArrow.gameObject.SetActive(false);

            PrefabAPI.RegisterNetworkPrefab(poolPrefab);

            GetStatCoefficients += GrantBaseShield;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var hasShieldPre = HasShield(self);
            var body = self.body;
            var stack = GetCount(body);
            orig(self, damageInfo);
            var what = hasShieldPre && !HasShield(self) && self.alive;
            if (what && body)
            {
                if (stack > 0 && !body.HasBuff(pocketPlutoniumRecharge))
                {
                    if (Physics.Raycast(body.transform.position, Vector3.down, out var raycast, 500f, LayerIndex.world.mask))
                    {
                        var damageFromBase = body.damage * (poolBaseDamage + poolStackDamage * (stack - 1));
                        var damageFromShields = body.maxShield * (poolBasePercentShieldDamage + poolStackPercentShieldDamage * (stack - 1));
                        var damage = (damageFromBase + damageFromShields) * 0.2f;
                        // Main.ModLogger.LogError("damage from base is " + damageFromBase);
                        // Main.ModLogger.LogError("damage from shields is " + damageFromShields);
                        // Main.ModLogger.LogError("damage is " + damage);
                        // Main.ModLogger.LogError("FULL FINAL FUCKING, in 5s should be " + damage * 5f);
                        ProjectileManager.instance.FireProjectile(poolPrefab, raycast.point, Quaternion.identity, self.gameObject, damage, 0f, body.RollCrit(), DamageColorIndex.Poison, null, -1f);

                        Util.PlaySound("Play_item_use_molotov_impact_big", self.gameObject);
                    }
                    self.body.AddTimedBuffAuthority(pocketPlutoniumRecharge.buffIndex, 5f);
                }
            }
        }

        private static bool HasShield(HealthComponent hc)
        {
            return hc.shield > 1f;
        }

        public void CreatePrefab()
        {
            indicator = Assets.GameObject.NearbyDamageBonusIndicator.InstantiateClone("Pocket Plutonium Visual", true);
            var radiusTrans = indicator.transform.Find("Radius, Spherical");
            radiusTrans.localScale = new Vector3(poolRadius * 2f, poolRadius * 2f, poolRadius * 2f);

            var razorMat = Object.Instantiate(Assets.Material.matNearbyDamageBonusRangeIndicator);
            var cloudTexture = Assets.Texture2D.texCloudWaterRipples;
            razorMat.SetTexture("_MainTex", cloudTexture);
            razorMat.SetTexture("_Cloud1Tex", cloudTexture);
            razorMat.SetColor("_TintColor", new Color32(175, 255, 30, 150));

            radiusTrans.GetComponent<MeshRenderer>().material = razorMat;

            PrefabAPI.RegisterNetworkPrefab(indicator);
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender && GetCount(sender) > 0)
            {
                var healthComponent = sender.healthComponent;
                if (healthComponent)
                    args.baseShieldAdd += healthComponent.fullHealth * basePercentShieldGain;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}