using LookingGlass.ItemStatsNameSpace;
using ThreeEyedGames;
using UnityEngine.XR;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Pocket Plutonium")]
    public class PocketPlutonium : ItemBase<PocketPlutonium>
    {
        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "Create a nuclear pool after losing all shield.";

        public override string ItemFullDescription => $"Gain a $shshield$se equal to $sh{basePercentShieldGain * 100f}%$se of your maximum health. Upon losing all $shshield$se, create a $sdnuclear pool$se in a $sd{poolRadius}m$se area that deals $sd{poolBaseDamage * 100f}%$se $ss(+{poolStackDamage * 100f}% per stack)$se base damage, plus an additional $sd{poolBasePercentShieldDamage * 100f}%$se $ss(+{poolStackPercentShieldDamage * 100f}% per stack)$se of $shshield$se.".AutoFormat();

        public override string ItemLore =>
        """
        Order: Plutonium Rod
        Tracking Number: 946*****
        Estimated Delivery: 07/04/2056
        Shipping Method: Priority
        Shipping Address: Magers Military Center, Venus, Box 47261
        Shipping Details:

        Plutonium might sound dangerous, but I can assure you, in a standard-issue suit, it's harmless. It's made of the same material that's used in our ships' shields, as well as all shield technology -- there's even trace amounts of it in those smaller civilian generators that have been so trendy recently.

        Ideally, your shields stay up for the whole operation. With thirteen of those generators, you'll be safe from anything short of a nuclear blast. If your shield does somehow break, though -- and that's a big if -- this nasty stuff will be released right in the face of the culprit and give you a chance to escape. If it happens, though, don't linger. You'll probably be impervious to it, but there's no reason to test your luck.
        """;
        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("PocketPlutoniumHolder.Prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texPocketPlutonium.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.BrotherBlacklist, ItemTag.AIBlacklist, ItemTag.CanBeTemporary, ItemTag.Technology];
        public override float modelPanelParametersMinDistance => 7f;
        public override float modelPanelParametersMaxDistance => 15f;

        public static GameObject indicator;

        public static GameObject poolPrefab;

        [ConfigField("Base Percent Shield Gain", "Decimal.", 0.1f)]
        public static float basePercentShieldGain;

        [ConfigField("Pool Radius", "", 20f)]
        public static float poolRadius;

        [ConfigField("Pool Base Damage", "Decimal.", 7.5f)]
        public static float poolBaseDamage;

        [ConfigField("Pool Stack Damage", "Decimal.", 7.5f)]
        public static float poolStackDamage;

        [ConfigField("Pool Base Percent Shield Damage", "Decimal.", 2.5f)]
        public static float poolBasePercentShieldDamage;

        [ConfigField("Pool Stack Percent Shield Damage", "Decimal.", 2.5f)]
        public static float poolStackPercentShieldDamage;

        public static BuffDef pocketPlutoniumRecharge;

        public static GameObject pocketPlutoniumPoolProcVFX;

        public static GameObject pocketPlutoniumConstantVFX;

        public override void Init()
        {
            base.Init();
            SetUpBuffs();
            SetUpProjectiles();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Pool Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Pool Shield Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    poolBaseDamage + poolStackDamage * (stack - 1),
                    poolBasePercentShieldDamage + poolStackPercentShieldDamage * (stack - 1),
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpBuffs()
        {
            pocketPlutoniumRecharge = ScriptableObject.CreateInstance<BuffDef>();
            pocketPlutoniumRecharge.isDebuff = false;
            pocketPlutoniumRecharge.canStack = false;
            pocketPlutoniumRecharge.buffColor = new Color32(115, 204, 71, 255);
            pocketPlutoniumRecharge.isHidden = true;
            pocketPlutoniumRecharge.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texProtogen3.png");
            pocketPlutoniumRecharge.name = "Pocket Plutonium Cooldown";

            ContentAddition.AddBuffDef(pocketPlutoniumRecharge);
        }

        public void SetUpProjectiles()
        {
            pocketPlutoniumPoolProcVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovSingleIgniteExplosionVFXVariant, "Pocket Plutonium Proc VFX", false);

            var vfx = pocketPlutoniumPoolProcVFX.GetComponent<ParticleSystem>().main;
            var guh = vfx.startColor;
            guh.color = new Color32(90, 224, 52, 255);
            vfx.startLifetime = 1.5f;

            var trans = pocketPlutoniumPoolProcVFX.transform;

            var omni = trans.GetChild(0).GetComponent<ParticleSystemRenderer>();
            var omni2 = omni.GetComponent<ParticleSystem>().main.startLifetime;
            omni2.mode = ParticleSystemCurveMode.Constant;
            omni2.constant = 0.8f;
            var newMat = Object.Instantiate(Paths.Material.matOmniHitspark3Gasoline);
            newMat.SetColor("_TintColor", new Color32(46, 116, 28, 255));

            omni.material = newMat;

            var light = trans.GetChild(1).GetComponent<Light>();
            light.range = poolRadius;
            light.intensity = 40f;
            light.color = new Color32(88, 255, 0, 255);

            var lightIntensityCurve = light.GetComponent<LightIntensityCurve>();
            lightIntensityCurve.timeMax = 0.8f;

            var flames = trans.GetChild(2).GetComponent<ParticleSystemRenderer>();
            var flames2 = flames.GetComponent<ParticleSystem>().main.startLifetime;
            flames2.mode = ParticleSystemCurveMode.Constant;
            flames2.constant = 0.8f;

            var newMat2 = Object.Instantiate(Paths.Material.matOmniExplosion1Generic);
            newMat2.SetColor("_TintColor", new Color32(46, 116, 28, 255));

            flames.material = newMat2;

            var flash = trans.GetChild(3).GetComponent<ParticleSystemRenderer>();
            var flash2 = flash.GetComponent<ParticleSystem>().main.startLifetime;
            flash2.constant = 0.2f;
            var newMat3 = Object.Instantiate(Paths.Material.matTracerBright);
            newMat3.SetColor("_TintColor", new Color32(46, 116, 28, 255));

            flash.material = newMat3;

            var destroyOnTimer = pocketPlutoniumPoolProcVFX.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 5f;

            ContentAddition.AddEffect(pocketPlutoniumPoolProcVFX);

            pocketPlutoniumConstantVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovProjectileDotZone, "Pocket Plutonium Pool VFX", false);

            var networkIdentity = pocketPlutoniumConstantVFX.GetComponent<NetworkIdentity>();
            networkIdentity.Reset();
            networkIdentity.enabled = false;

            pocketPlutoniumConstantVFX.GetComponent<TeamFilter>().enabled = false;
            pocketPlutoniumConstantVFX.GetComponent<ProjectileController>().enabled = false;
            pocketPlutoniumConstantVFX.GetComponent<HitBoxGroup>().enabled = false;
            pocketPlutoniumConstantVFX.GetComponent<ProjectileDotZone>().enabled = false;
            pocketPlutoniumConstantVFX.GetComponent<ProjectileDamage>().enabled = false;

            var destroyOnTimer2 = pocketPlutoniumConstantVFX.AddComponent<DestroyOnTimer>();
            destroyOnTimer2.duration = 5f;

            var effectComponent = pocketPlutoniumConstantVFX.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            pocketPlutoniumConstantVFX.AddComponent<VFXAttributes>();

            var fx = pocketPlutoniumConstantVFX.transform.Find("FX");
            var hitbox = fx.Find("Hitbox");
            hitbox.gameObject.SetActive(false);
            hitbox.GetComponent<HitBox>().enabled = false;

            foreach (AnimateShaderAlpha animateShaderAlpha in fx.GetComponents<AnimateShaderAlpha>())
            {
                animateShaderAlpha.timeMax = 5f;
            }

            var pointLight = fx.Find("Point Light").GetComponent<Light>();
            pointLight.intensity = 10f;
            pointLight.color = new Color32(36, 255, 0, 255);
            pointLight.range = 70f;

            var flickerLight = pointLight.GetComponent<FlickerLight>();
            flickerLight.enabled = false;

            var scaledOnImpact = fx.Find("ScaledOnImpact");
            var teamIndicator = scaledOnImpact.Find("TeamAreaIndicator, GroundOnly");
            teamIndicator.gameObject.SetActive(false);

            var decal = scaledOnImpact.Find("Decal").GetComponent<Decal>();
            // decal Fade keeps increasing, and once it reaches 2, it completely disappears
            // idk what causes it to increase - like where

            var newMat4 = Object.Instantiate(Paths.Material.matMolotovDecal);
            newMat4.SetColor("_Color", new Color32(17, 121, 0, 255));
            // newMat4.SetTexture("_RemapTex", Paths.Texture2D.texRampBeetleQueen2); // breaks it for some reason???????

            decal.Material = newMat4;

            var fireBillboard = scaledOnImpact.Find("Fire, Billboard").GetComponent<ParticleSystemRenderer>();

            var fireMaterial = new Material(Paths.Material.matFirePillarParticle);
            fireMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritoneHShrine);
            fireMaterial.SetFloat("_Boost", 5f);
            fireMaterial.SetFloat("_AlphaBoost", 5f);
            fireMaterial.SetFloat("_AlphaBias", 0.07f);
            fireMaterial.SetColor("_TintColor", new Color32(25, 255, 0, 255));

            fireBillboard.material = fireMaterial;

            ContentAddition.AddEffect(pocketPlutoniumConstantVFX);

            poolPrefab = PrefabAPI.InstantiateClone(Paths.GameObject.HuntressArrowRain, "Pocket Plutonium Pool");
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

            var fx2 = poolPrefab.transform.GetChild(0);

            var radiusIndicator = fx2.GetChild(0).GetComponent<MeshRenderer>();

            radiusIndicator.material = Main.hifuSandswept.LoadAsset<Material>("matPocketPlutoniumPool.mat");

            var hitbox1 = fx2.GetChild(3);

            hitbox1.transform.localPosition = Vector3.zero;
            hitbox1.transform.localScale = new Vector3(0.9145522f, 0.1f, 0.9145525f);

            var hitbox2 = fx2.GetChild(4);
            hitbox2.transform.localScale = new Vector3(0.914552f, 0.1f, 0.9145527f);

            hitbox2.transform.localPosition = Vector3.zero;

            var arrowsFalling = fx2.GetChild(1);
            arrowsFalling.gameObject.SetActive(false);

            var impaledArrow = fx2.GetChild(5);
            impaledArrow.gameObject.SetActive(false);

            PrefabAPI.RegisterNetworkPrefab(poolPrefab);
        }

        public override void Hooks()
        {
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

                        EffectManager.SpawnEffect(pocketPlutoniumPoolProcVFX, new EffectData { scale = poolRadius, rotation = Quaternion.identity, origin = raycast.point }, true);
                        EffectManager.SpawnEffect(pocketPlutoniumConstantVFX, new EffectData { scale = poolRadius / 4f, rotation = Quaternion.identity, origin = raycast.point }, true);
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
            indicator = Paths.GameObject.NearbyDamageBonusIndicator.InstantiateClone("Pocket Plutonium Visual", true);
            var radiusTrans = indicator.transform.Find("Radius, Spherical");
            radiusTrans.localScale = new Vector3(poolRadius * 2f, poolRadius * 2f, poolRadius * 2f);

            var razorMat = Object.Instantiate(Paths.Material.matNearbyDamageBonusRangeIndicator);
            var cloudTexture = Paths.Texture2D.texCloudWaterRipples;
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

            var itemDisplay = SetUpIDRS();

            ItemDisplayRuleDict i = new();

            #region Sandswept Survivors
            /*
            i.Add("RangerBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                    localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                    localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );
            */

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.0115F, -0.41669F, -0.00077F),
                    localAngles = new Vector3(351.2338F, 359.9024F, 180.1094F),
                    localScale = new Vector3(0.15481F, 0.14456F, 0.13524F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;

        }
    }
}