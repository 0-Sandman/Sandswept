using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Whites
{
    public class PocketPlutonium : ItemBase<PocketPlutonium>
    {
        public class ShieldedComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;

            public bool cachedIsShielded = false;
        }

        public class PlutoniumBehaviour : MonoBehaviour
        {
            public bool active = true;
            private GameObject PlutIndicator;
            public CharacterBody body;

            public float Timer;
            private float velocity;
            public float x;

            private bool indicatorEnabled
            {
                get
                {
                    return PlutIndicator;
                }
                set
                {
                    if (indicatorEnabled != value)
                    {
                        if (value)
                        {
                            PlutIndicator = Instantiate(PlutoniumZone, body.corePosition, Quaternion.identity);
                            PlutIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                        }
                        else
                        {
                            Destroy(PlutIndicator);
                            PlutIndicator = null;
                        }
                    }
                }
            }

            public void FixedUpdate()
            {
                if (!active || body.inventory.GetItemCount(instance.ItemDef) == 0)
                {
                    Destroy(this);
                    return;
                }

                if (NetworkServer.active)
                {
                    Timer += Time.fixedDeltaTime;
                    if (!(Timer >= 0.05))
                    {
                        return;
                    }

                    Timer = 0f;
                    TeamIndex teamIndex = body.teamComponent.teamIndex;
                    for (TeamIndex teamIndex2 = TeamIndex.Neutral; teamIndex2 < TeamIndex.Count; teamIndex2++)
                    {
                        if (teamIndex2 != teamIndex && teamIndex2 != 0)
                        {
                            foreach (TeamComponent teamMember in TeamComponent.GetTeamMembers(teamIndex2))
                            {
                                Vector3 val = teamMember.transform.position - body.corePosition;
                                if (val.sqrMagnitude <= 225f)
                                {
                                    InflictDotInfo inflictDotInfo = default;
                                    inflictDotInfo.victimObject = teamMember.gameObject;
                                    inflictDotInfo.attackerObject = body.gameObject;
                                    inflictDotInfo.totalDamage = body.damage;
                                    inflictDotInfo.dotIndex = IrradiatedIndex;
                                    inflictDotInfo.duration = 0f;
                                    inflictDotInfo.maxStacksFromAttacker = 1;
                                    inflictDotInfo.damageMultiplier = 1.25f + 0.75f * (body.inventory.GetItemCount(instance.ItemDef) - 1);
                                    InflictDotInfo dotInfo = inflictDotInfo;
                                    DotController.InflictDot(ref dotInfo);
                                }
                            }
                        }
                    }
                }
            }

            public void Update()
            {
                if (PlutIndicator && x != 30f)
                {
                    Transform zone = PlutIndicator.transform.Find("Radius, Spherical");
                    x = Mathf.SmoothDamp(zone.localScale.x, 30f, ref velocity, 0.2f);
                    zone.localScale = new Vector3(x, x, x);
                }
            }

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
            }

            private void Start()
            {
                indicatorEnabled = true;
            }

            private void OnDestroy()
            {
                indicatorEnabled = false;
            }
        }

        public static DamageColorIndex IrradiateDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public static DotController.DotDef IrradiatedDef;

        public static DotController.DotIndex IrradiatedIndex;

        public static BuffDef IrradiatedBuff;

        public static GameObject PlutoniumZone;
        public override string ItemName => "Pocket Plutonium";

        public override string ItemLangTokenName => "POCKET_PLUTONIUM";

        public override string ItemPickupDesc => "While shields are active, create an irradiating ring around you.";

        public override string ItemFullDescription => "Gain a $shshield$se equal to $sh3%$se of your maximum health. While shields are active, $sdirradiate$se all enemies within $sd15m$se for $sd125%$se $ss(+75% per stack)$se $sddamage per second$se.";

        public override string ItemLore => "<style=cStack>funny quirky funny funny funny quirky</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("PlutoniumPrefab.Prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("PlutoniumIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            CreateDot();
            CreatePrefab();
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCheck;
            GetStatCoefficients += GrantEffect;
        }

        public void CreatePrefab()
        {
            PlutoniumZone = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator").InstantiateClone("PlutoniumZone");
            Transform val = PlutoniumZone.transform.Find("Radius, Spherical");
            val.localScale = Vector3.one * 0f;
            HGIntersectionController hGIntersectionController = val.gameObject.AddComponent<HGIntersectionController>();
            hGIntersectionController.Renderer = val.GetComponent<MeshRenderer>();
            MeshRenderer val5 = (MeshRenderer)hGIntersectionController.Renderer;
            Material val3 = Addressables.LoadAssetAsync<Material>("d0eb35f70367cdc4882f3bb794b65f2b").WaitForCompletion();
            Material val4 = Object.Instantiate(val3);
            val4.SetColor("_TintColor", new Color32(95, 255, 0, 255));
            val4.SetTexture("_Cloud1Tex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/texCloudOrganic2.png").WaitForCompletion());
            val4.SetTextureScale("_Cloud1Tex", new Vector2(0.5f, 0.5f));
            val4.SetTexture("_Cloud2Tex", null);
            val4.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Engi/texUIEngiMissileLockedOn.png").WaitForCompletion());

            val4.SetFloat("_InvFade", 0.5f);
            val4.SetFloat("_Boost", 0.5f);
            val5.material = val4;
            hGIntersectionController.Material = val4;
            PlutoniumZone.RegisterNetworkPrefab();
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

        public void CreateDot()
        {
            IrradiatedDef = new DotController.DotDef
            {
                associatedBuff = IrradiatedBuff,
                damageCoefficient = 1f,
                damageColorIndex = IrradiateDamageColour,
                interval = 1f
            };
            IrradiatedIndex = DotAPI.RegisterDotDef(IrradiatedDef, null, null);
        }

        private void ShieldedCheck(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig.Invoke(self);
            ShieldedComponent shieldedCoreComponent = self.GetComponent<ShieldedComponent>();
            if (!shieldedCoreComponent)
            {
                shieldedCoreComponent = self.gameObject.AddComponent<ShieldedComponent>();
            }
            int count = GetCount(self);
            bool flag = self.healthComponent.shield > 0f;
            bool flag2 = false;
            if (shieldedCoreComponent.cachedInventoryCount != count)
            {
                flag2 = true;
                shieldedCoreComponent.cachedInventoryCount = count;
            }
            if (shieldedCoreComponent.cachedIsShielded != flag)
            {
                flag2 = true;
                shieldedCoreComponent.cachedIsShielded = flag;
            }
            if (flag2)
            {
                self.statsDirty = true;
            }
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent component = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += component.fullHealth * 0.03f;
            }
        }

        private void GrantEffect(CharacterBody sender, StatHookEventArgs args)
        {
            ShieldedComponent component = sender.GetComponent<ShieldedComponent>();
            PlutoniumBehaviour behaviourCheck = sender.GetComponent<PlutoniumBehaviour>();
            if ((bool)component && component.cachedIsShielded && component.cachedInventoryCount > 0 && !behaviourCheck)
            {
                PlutoniumBehaviour behaviour = sender.gameObject.AddComponent<PlutoniumBehaviour>();
            }
            if ((bool)component && component.cachedIsShielded == false && behaviourCheck)
            {
                behaviourCheck.active = false;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}