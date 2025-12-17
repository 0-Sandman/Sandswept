using System;
using System.Diagnostics;
using RoR2.UI;
using BarInfo = RoR2.UI.HealthBar.BarInfo;
using UnityEngine.UI;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Debug = UnityEngine.Debug;
using LookingGlass.ItemStatsNameSpace;
using R2API.Networking.Interfaces;
using R2API.Networking;
using R2API;
using static Sandswept.Items.Greens.MakeshiftPlate;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Item: Makeshift Plate")]
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public static BuffDef MakeshiftPlateCount;

        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Gain plating on stage entry. Plating absorbs damage, but cannot be recovered.";

        public override string ItemFullDescription => $"Begin each stage with $sh{basePercentPlatingGain}%$se $ss(+{stackPercentPlatingGain}% per stack)$se $shplating$se. $shPlating$se acts as $shsecondary health$se, but cannot be recovered in any way.".AutoFormat();

        public override string ItemLore =>
        """
        Order: WW2019 Artifact - #1485-43958
        Tracking Number: 599*****
        Estimated Delivery: 8/9/2027
        Shipping Method: Top Priority
        Shipping Address: 707th Penthouse Suite, Earth
        Shipping Details:

        A real, genuine plate of armor from the War of 2019. Somehow managed to survive what looks like gunshots, bombing runs, laser tech, and all sorts of other things. This thing would be a fine centerpiece to a collection I'd reckon. You paid a hefty price for this thing, hope it's worth it!
        """;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.mainAssets.LoadAsset<GameObject>("MakeshiftPlatePrefab.prefab");

        public override Sprite ItemIcon => Main.assets.LoadAsset<Sprite>("texIconPlate.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.Healing, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy]; // shouldnt be temporary because it mostly bypasses the downside of temporary items

        [ConfigField("Base Percent Plating Gain", "", 100f)]
        public static float basePercentPlatingGain;

        [ConfigField("Stack Percent Plating Gain", "", 100f)]
        public static float stackPercentPlatingGain;

        public static Sprite texPlatingBar => Main.sandsweptHIFU.LoadAsset<Sprite>("texPlatingBar.png");
        public static HealthBarAPI.BarOverlayIndex MakeshiftPlateOverlay;

        public static GameObject platingOverlayPrefab;

        public static GameObject fewParticles;
        public static GameObject lotsParticles;
        public static MethodInfo TakeDamageInfo;
        public static DamageAPI.ModdedDamageType BypassPlating = DamageAPI.ReserveDamageType();
        public override void Init()
        {
            base.Init();
            SetUpVFX();
        
            NetworkingAPI.RegisterMessageType<MakeshiftPlateAddSync>();

            MakeshiftPlateOverlay = HealthBarAPI.RegisterBarOverlay(new HealthBarAPI.BarOverlayInfo()
            {
                BarInfo = new HealthBar.BarInfo
                {
                    color = Color.white,
                    sprite = texPlatingBar,
                    imageType = Image.Type.Sliced,
                    sizeDelta = 25f,
                    normalizedXMax = 0f,
                    normalizedXMin = 0f
                },
                BodySpecific = true,
                ModifyBarInfo = (HealthBar bar, ref HealthBar.BarInfo info) =>
                {
                    PlatingManager manager = bar.source.GetComponent<PlatingManager>();
                    info.enabled = manager && manager.CurrentPlating > 0 && GetCount(bar.source.body) > 0;
                    if (manager)
                    {
                        info.normalizedXMin = 0f;
                        info.normalizedXMax = manager.CurrentPlating / manager.MaxPlating;
                    }
                },
                ModifyHealthValues = (HealthBar bar, ref float cur, ref float max) =>
                {
                    PlatingManager manager = bar.source.GetComponent<PlatingManager>();
                    if (manager && GetCount(bar.source.body) > 0)
                    {
                        cur += manager.CurrentPlating;
                    }
                }
            });
        }
        public override void Hooks()
        {
            TakeDamageInfo = typeof(MakeshiftPlate).GetMethod(nameof(TakeDamageHandler), BindingFlags.Public | BindingFlags.Static);
            On.RoR2.CharacterBody.Start += OnBodySpawn;
            IL.RoR2.HealthComponent.TakeDamageProcess += TakeDamage;
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += GiveItemPermanent;
            On.RoR2.Inventory.GiveItemChanneled += GiveItemChanneled;
            On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChanged;
        }

        private void GiveItemChanneled(On.RoR2.Inventory.orig_GiveItemChanneled orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            orig(self, itemIndex, countToAdd);
            HandleItemGain(self, itemIndex, countToAdd);
        }
        // give item channeled is likely it

        private void SetUpVFX()
        {
            platingOverlayPrefab = PrefabAPI.InstantiateClone(Paths.GameObject.BarrierEffect, "Plating Overlay", false);

            platingOverlayPrefab.AddComponent<UpdateAlphaBoostToPlating>();

            var meshHolder = platingOverlayPrefab.transform.Find("MeshHolder");
            meshHolder.GetComponent<Billboard>().enabled = false;
            meshHolder.localPosition = new Vector3(0f, 0.2f, 0f);

            var shieldMesh = meshHolder.Find("ShieldMesh");
            shieldMesh.AddComponent<AdjustShieldMeshLocalScale>();

            VFXUtils.AddLight(shieldMesh.gameObject, new Color32(85, 144, 164, 255), 0f, 0f);

            var meshFilter = shieldMesh.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = Addressables.LoadAssetAsync<GameObject>("a0ce346ce1a826e4a912f35f9ef705a0").WaitForCompletion().GetComponent<MeshFilter>().mesh; // 66 verts donut3Mesh from mdlVFXDonut3 from Assets/RoR2/Base/Common/VFX/Mesh_ Particle/

            var meshRenderer = shieldMesh.GetComponent<MeshRenderer>();

            var newMat = new Material(Paths.Material.matBarrier);
            newMat.SetColor("_TintColor", new Color32(0, 3, 3, 255));
            newMat.SetTexture("_MainTex", Paths.Texture2D.texCompExchangeGridE);
            newMat.SetTextureScale("_MainTex", Vector2.one * 2f);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampTritoneSmoothed);
            newMat.SetFloat("_InvFade", 0.237792f);
            newMat.SetFloat("_Boost", 20f);
            newMat.SetFloat("_AlphaBoost", 10f);
            newMat.SetFloat("_AlphaBias", 0.5379356f);

            meshRenderer.material = newMat;

            fewParticles = PrefabAPI.InstantiateClone(Paths.GameObject.Bandit2SmokeBomb, "Makeshift Plate On Hurt VFX", false);

            fewParticles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.OdpizdzijPierdoloneGownoKurwaCoZaJebanyKurwaSmiecToKurwaDodalPizdaKurwaJebanaKurwa(fewParticles);

            fewParticles.GetComponent<ShakeEmitter>().enabled = false;

            VFXUtils.RecolorMaterialsAndLights(fewParticles, new Color32(0, 70, 255, 255), new Color32(0, 181, 255, 255), true, true);

            var core = fewParticles.transform.Find("Core");

            core.Find("Point Light").GetComponent<Light>().range = 4f;

            core.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            core.Find("Dust, CenterSphere").gameObject.SetActive(false);
            core.Find("Dust, CenterTube").gameObject.SetActive(false);

            var sparks = core.Find("Sparks");

            sparks.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", Paths.Texture2D.texConstructMask);

            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 10;
            var sparksEmission = sparksPS.emission;
            var sparksBurst = new ParticleSystem.Burst(0f, 10, 10, 1, 0.01f)
            {
                probability = 1f
            };
            sparksEmission.SetBurst(0, sparksBurst);

            ContentAddition.AddEffect(fewParticles);

            lotsParticles = PrefabAPI.InstantiateClone(fewParticles, "Makeshift Plate On Break VFX", false);

            var core2 = lotsParticles.transform.Find("Core");

            var sparks2 = core2.Find("Sparks");

            var sparks2PS = sparks2.GetComponent<ParticleSystem>();
            var sparks2Main = sparks2PS.main;
            sparks2Main.maxParticles = 300;
            var sparks2Emission = sparks2PS.emission;
            var sparks2Burst = new ParticleSystem.Burst(0f, 300, 300, 1, 0.01f)
            {
                probability = 1f
            };
            sparks2Emission.SetBurst(0, sparks2Burst);

            ContentAddition.AddEffect(lotsParticles);
        }

        private void GiveItemPermanent(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            orig(self, itemIndex, countToAdd);
            HandleItemGain(self, itemIndex, countToAdd);
        }

        private void OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (self.GetComponent<PlatingManager>() && self.inventory.GetItemCountEffective(ItemDef) == 0 && !self.inventory.inventoryDisabled)
            {
                new MakeshiftPlateAddSync(self.gameObject, 0f, 0f, false).Send(NetworkDestination.Clients);
            }
        }

        private void HandleItemGain(Inventory self, ItemIndex itemIndex, int count)
        {
            if (itemIndex == ItemDef.itemIndex && self.TryGetComponent(out CharacterMaster characterMaster) && characterMaster.bodyInstanceObject)
            {
                PlatingManager platingManager = characterMaster.bodyInstanceObject.GetComponent<PlatingManager>();
                if (!platingManager)
                {
                    platingManager = characterMaster.AddComponent<PlatingManager>();
                }

                CharacterBody characterBody = characterMaster.bodyInstanceObject.GetComponent<CharacterBody>();

                float platingMult = stackPercentPlatingGain / 100f * count;

                // Main.ModLogger.LogError($"plating mult is {platingMult}");

                int plating = Mathf.RoundToInt((characterBody.maxHealth + characterBody.maxShield) * platingMult);

                // Main.ModLogger.LogError($"plating is {plating}");
                // Main.ModLogger.LogError($"platingManager.CurrentPlating BEFORE ADDING is {platingManager.CurrentPlating}");

                platingManager.CurrentPlating += plating;

                // Main.ModLogger.LogError($"adding to currentPlating");
                // Main.ModLogger.LogError($"platingManager.CurrentPlating AFTERRRRR ADDING is {platingManager.CurrentPlating}");

                if (platingManager.MaxPlating <= 0)
                {
                    // Main.ModLogger.LogError("platingManager.MaxPlating is less than or equal to 0");
                    platingManager.MaxPlating = plating;
                }

                platingManager.CurrentPlating = Mathf.Min(platingManager.CurrentPlating, platingManager.MaxPlating);

                Util.PlaySound("Play_loader_m1_shield", characterBody.gameObject);
                Util.PlaySound("Play_loader_m1_shield", characterBody.gameObject);

                platingManager.usedUpAllPlatingThisPickup = false;

                new MakeshiftPlateAddSync(characterBody.gameObject, platingManager.CurrentPlating, platingManager.MaxPlating, false).Send(NetworkDestination.Clients);
            }
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Plating: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Armor);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    (basePercentPlatingGain + stackPercentPlatingGain * (stack - 1)) / 100f
                };

                return values;
            };

            return itemStatsDef;
        }

        public void OnBodySpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (self.inventory)
            {
                float platingMult = (stackPercentPlatingGain / 100f) * self.inventory.GetItemCount(ItemDef);

                int plating = Mathf.RoundToInt((self.maxHealth + self.maxShield) * platingMult);

                if (plating == 0)
                {
                    return;
                }

                if (self.TryGetComponent<PlatingManager>(out var platingManager))
                {
                    Util.PlaySound("Play_loader_m1_shield", self.gameObject);
                    Util.PlaySound("Play_loader_m1_shield", self.gameObject);

                    platingManager.usedUpAllPlatingThisPickup = false;
                }

                new MakeshiftPlateAddSync(self.gameObject, plating, plating, false).Send(NetworkDestination.Clients);
            }
        }

        public void TakeDamage(ILContext il) {
            ILCursor c = new(il);
            int index = -1;
            c.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchStloc(out index),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcR4(out _)
            );
            c.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.PermanentCurse)));

            if (index != -1) {
                c.TryGotoNext(x => x.MatchLdfld(typeof(HealthComponent), nameof(HealthComponent.shield)));
                c.TryGotoPrev(MoveType.After, x => x.MatchLdloc(index));
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc, index);
                c.Emit(OpCodes.Call, TakeDamageInfo);
                c.Emit(OpCodes.Stloc, index);
                c.Emit(OpCodes.Ldloc, index);
            }
            else {
                Main.ModLogger.LogError("Failed to apply Makeshift Plate IL hook.");
            }
        }

        public static float TakeDamageHandler(HealthComponent self, DamageInfo info, float input)
        {
            if (!info.HasModdedDamageType(BypassPlating) && self.body && MakeshiftPlate.instance.GetCount(self.body) > 0 && self.body.TryGetComponent(out PlatingManager platingManager) && !self.body.inventory.inventoryDisabled)
            {
                float plating = platingManager.CurrentPlating;
                float toRemove = 0;

                if (plating > input)
                {
                    toRemove = input;
                    input = 0f;
                }
                else
                {
                    toRemove = input - plating;
                    input -= plating;
                }

                platingManager.CurrentPlating -= toRemove;
                platingManager.CurrentPlating = Mathf.Clamp(platingManager.CurrentPlating, 0, platingManager.MaxPlating);

                if (!platingManager.usedUpAllPlatingThisPickup)
                {
                    if (platingManager.CurrentPlating <= 0)
                    {
                        Util.PlaySound("Play_loader_m1_shield", self.gameObject);
                        Util.PlaySound("Play_loader_m1_shield", self.gameObject);
                        EffectManager.SpawnEffect(lotsParticles, new EffectData() { scale = MathHelpers.BestBestFitRadius(self.body), origin = self.body.corePosition }, true);
                        platingManager.usedUpAllPlatingThisPickup = true;
                    }
                    else
                    {
                        if (platingManager.canSpawnEffects)
                        {
                            Util.PlaySound("Play_item_proc_personal_shield_end", self.gameObject);
                            Util.PlaySound("Play_item_proc_personal_shield_end", self.gameObject);
                            EffectManager.SpawnEffect(fewParticles, new EffectData() { scale = MathHelpers.BestBestFitRadius(self.body), origin = self.body.corePosition }, true);
                            platingManager.canSpawnEffects = false;
                        }
                    }
                }

                new MakeshiftPlateAddSync(self.gameObject, platingManager.CurrentPlating, platingManager.MaxPlating, false).Send(NetworkDestination.Clients);
            }

            return input;
        }

        public void CreateBuff()
        {
            MakeshiftPlateCount = ScriptableObject.CreateInstance<BuffDef>();
            MakeshiftPlateCount.name = "Plated";
            MakeshiftPlateCount.buffColor = Color.white;
            MakeshiftPlateCount.canStack = true;
            MakeshiftPlateCount.isDebuff = false;
            MakeshiftPlateCount.iconSprite = Main.mainAssets.LoadAsset<Sprite>("MakeshiftPlateBuffIcon.png");
            ContentAddition.AddBuffDef(MakeshiftPlateCount);
        }

        public class PlatingManager : MonoBehaviour
        {
            public float CurrentPlating = 0;
            public float MaxPlating = 0;
            public CharacterBody body;
            public bool usedUpAllPlatingThisPickup = false;
            public bool canSpawnEffects = true;
            public float effectTimer = 0f;
            public float effectInterval = 0.33f;

            public void Start()
            {
                body = GetComponent<CharacterBody>();

                if (body)
                {
                    HealthBarAPI.AddOverlayToBody(body, MakeshiftPlateOverlay);
                }
            }

            public void FixedUpdate()
            {
                effectTimer += Time.fixedDeltaTime;
                if (effectTimer >= effectInterval)
                {
                    canSpawnEffects = true;
                    effectTimer = 0f;
                }
            }

            public void OnDestroy()
            {
                if (body)
                {
                    HealthBarAPI.RemoveOverlayFromBody(body, MakeshiftPlateOverlay);
                }
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
                    childName = "Chest",
                    localPos = new Vector3(0.01214F, 0.1681F, -0.27857F),
                    localAngles = new Vector3(16.31142F, 188.3025F, 3.37207F),
                    localScale = new Vector3(0.38187F, 0.35658F, 0.33242F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;

        }

        public class MakeshiftPlateAddSync : INetMessage
        {
            public GameObject target;
            public float plating;
            public float maxPlating;
            public bool remove;
            public void Deserialize(NetworkReader reader)
            {
                target = reader.ReadGameObject();
                plating = reader.ReadSingle();
                maxPlating = reader.ReadSingle();
                remove = reader.ReadBoolean();
            }

            public void OnReceived()
            {
                Process();
            }

            public MakeshiftPlateAddSync()
            {

            }

            public MakeshiftPlateAddSync(GameObject target, float plating, float maxPlating, bool remove)
            {
                this.target = target;
                this.plating = plating;
                this.maxPlating = maxPlating;
                this.remove = remove;

                Process();
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(target);
                writer.Write(plating);
                writer.Write(maxPlating);
                writer.Write(remove);
            }

            public void Process()
            {
                if (remove)
                {
                    target.RemoveComponent<PlatingManager>();
                }
                else
                {
                    PlatingManager manager = target.GetComponent<PlatingManager>();
                    if (!manager)
                    {
                        manager = target.AddComponent<PlatingManager>();
                    }
                    manager.MaxPlating = maxPlating;
                    manager.CurrentPlating = plating;
                }
            }
        }
    }

    public class UpdateAlphaBoostToPlating : MonoBehaviour
    {
        public float timer;
        public float interval = 0.2f;
        public MeshRenderer targetMeshRenderer;
        public Light targetLight;
        public TemporaryVisualEffect temporaryVisualEffect;
        public PlatingManager platingManager;
        public float alphaBoostMinimum = 0.5f;
        public float lightIntensityMinimum = 2f;

        public void Start()
        {
            var shieldMesh = transform.Find("MeshHolder/ShieldMesh");

            targetMeshRenderer = shieldMesh.GetComponent<MeshRenderer>();
            targetLight = shieldMesh.GetComponent<Light>();
            temporaryVisualEffect = GetComponent<TemporaryVisualEffect>();

            var modelPart = temporaryVisualEffect.parentTransform;
            if (modelPart)
            {
                var topmostModel = modelPart.root;
                var characterModel = topmostModel.GetComponent<CharacterModel>();
                if (characterModel)
                {
                    var body = characterModel.body;
                    if (body)
                    {
                        platingManager = body.GetComponent<PlatingManager>();
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer <= interval || platingManager == null)
            {
                return;
            }

            var materialPropertyBlock = new MaterialPropertyBlock();

            var platingPercent = platingManager.CurrentPlating / platingManager.MaxPlating;

            materialPropertyBlock.SetFloat("_AlphaBoost", Mathf.Max(alphaBoostMinimum, platingPercent * 7f));

            targetMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            targetLight.intensity = Mathf.Max(lightIntensityMinimum, platingPercent * 10f);

            timer = 0f;
        }
    }

    public class AdjustShieldMeshLocalScale : MonoBehaviour
    {
        public TemporaryVisualEffect temporaryVisualEffect;
        public void Start()
        {
            temporaryVisualEffect = transform.root.GetComponent<TemporaryVisualEffect>();
            var rootScale = temporaryVisualEffect.radius;
            transform.localScale = new Vector3(rootScale * 0.1373f, rootScale * 0.1373f, rootScale * 0.9615f);
            // magic numbers; I just did 0.25, 0.25, 1.75 for commando at his 1.82 bestFitRadius used for barrier and it looked fine so I multiplied the values to be the same here by default

            GetComponent<Light>().range = rootScale * 2.7472f;
            // same magic number explanation here, to get roughly 5f with mando, which looked nice in-game
        }
    }
}
