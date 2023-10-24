namespace Sandswept.Items.Greens
{
    public class PocketPlutonium : ItemBase<PocketPlutonium>
    {
        // I see you copied noop's wool code trolley

        public static DamageColorIndex IrradiateDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public static DotController.DotDef IrradiatedDef;

        public static DotController.DotIndex IrradiatedIndex;

        public static BuffDef IrradiatedBuff;

        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "While shields are active, create an irradiating ring around you.";

        public override string ItemFullDescription => "Gain a $shshield$se equal to $sh10%$se of your maximum health. While shields are active, $sdirradiate$se all enemies within $sd13m$se for $sd350%$se $ss(+200% per stack)$se $sddamage per second$se.".AutoFormat();

        public override string ItemLore => "<style=cStack>funny quirky funny funny funny quirky</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/PocketPlutoniumHolder.Prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("PlutoniumIcon.png");

        public static GameObject indicator;

        public static float radius = 13f;

        public static float baseDamage = 3.5f;
        public static float damagePerStack = 2f;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            CreateBuff();
            CreatePrefab();
            Hooks();
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        public void CreatePrefab()
        {
            indicator = Assets.GameObject.NearbyDamageBonusIndicator.InstantiateClone("Pocket Plutonium Visual", true);
            var radiusTrans = indicator.transform.Find("Radius, Spherical");
            radiusTrans.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);

            var razorMat = Object.Instantiate(Assets.Material.matNearbyDamageBonusRangeIndicator);
            var cloudTexture = Assets.Texture2D.texCloudWaterRipples;
            razorMat.SetTexture("_MainTex", cloudTexture);
            razorMat.SetTexture("_Cloud1Tex", cloudTexture);
            razorMat.SetColor("_TintColor", new Color32(175, 255, 30, 150));

            radiusTrans.GetComponent<MeshRenderer>().material = razorMat;

            indicator.RegisterNetworkPrefab();
        }

        public void CreateBuff()
        {
            IrradiatedBuff = ScriptableObject.CreateInstance<BuffDef>();
            IrradiatedBuff.name = "Irradiated";
            IrradiatedBuff.buffColor = new Color32(95, 255, 0, 255);
            IrradiatedBuff.canStack = false;
            IrradiatedBuff.isDebuff = true;
            IrradiatedBuff.iconSprite = Main.MainAssets.LoadAsset<Sprite>("IrradiatedIcon.png");
            ContentAddition.AddBuffDef(IrradiatedBuff);
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender && GetCount(sender) > 0)
            {
                var healthComponent = sender.healthComponent;
                args.baseShieldAdd += healthComponent.fullHealth * 0.1f;
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody characterBody)
        {
            if (NetworkServer.active)
                characterBody.AddItemBehavior<PocketPlutoniumController>(characterBody.inventory.GetItemCount(instance.ItemDef));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public class PocketPlutoniumController : CharacterBody.ItemBehavior
        {
            public float damageInterval = 0.2f;
            public float damage;
            public float timer;
            public float radiusSquared = radius * radius;
            public float distance = radius;
            public TeamIndex ownerIndex;
            public GameObject radiusIndicator;

            private void Start()
            {
                ownerIndex = body.teamComponent.teamIndex;
                enableRadiusIndicator = true;
                var radiusTrans = radiusIndicator.transform.GetChild(1);
                radiusTrans.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
                if (stack > 0)
                {
                    damage = (baseDamage + damagePerStack * (stack - 1)) * damageInterval;
                }
                else damage = 0;
            }

            private void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer < damageInterval || body.healthComponent.shield <= 0f) // blinks for some reason??
                {
                    enableRadiusIndicator = false;
                    return;
                }

                enableRadiusIndicator = true;

                for (TeamIndex firstIndex = TeamIndex.Neutral; firstIndex < TeamIndex.Count; firstIndex++)
                {
                    if (firstIndex == ownerIndex || firstIndex <= TeamIndex.Neutral)
                    {
                        continue;
                    }

                    foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(firstIndex))
                    {
                        var enemyPosition = teamComponent.transform.position;
                        var corePosition = body.corePosition;
                        if ((enemyPosition - corePosition).sqrMagnitude <= radiusSquared)
                        {
                            Damage(teamComponent);
                        }
                    }
                }

                timer = 0f;
            }

            private void Damage(TeamComponent teamComponent)
            {
                var victimBody = teamComponent.body;
                if (!victimBody)
                {
                    return;
                }

                var victimHealthComponent = victimBody.healthComponent;
                if (!victimHealthComponent)
                {
                    return;
                }

                if (victimHealthComponent)
                {
                    var info = new DamageInfo()
                    {
                        attacker = gameObject,
                        crit = false,
                        damage = damage * body.damage,
                        force = Vector3.zero,
                        procCoefficient = 0f,
                        damageType = DamageType.Generic,
                        position = victimBody.corePosition,
                        inflictor = gameObject
                    };
                    victimBody.AddTimedBuffAuthority(IrradiatedBuff.buffIndex, 0.2f);
                    victimHealthComponent.TakeDamage(info);
                }
            }

            public bool enableRadiusIndicator
            {
                get
                {
                    return radiusIndicator;
                }
                set
                {
                    if (enableRadiusIndicator != value)
                    {
                        if (value)
                        {
                            radiusIndicator = Instantiate(indicator, body.corePosition, Quaternion.identity);
                            radiusIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, null);
                        }
                        else
                        {
                            Destroy(radiusIndicator);
                            radiusIndicator = null;
                        }
                    }
                }
            }

            private void OnDisable()
            {
                enableRadiusIndicator = false;
            }
        }
    }
}