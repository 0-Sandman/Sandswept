using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using Sandswept.Items.Greens;
using System.Collections;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Millenium")]
    public class Millenium : ItemBase<Millenium>
    {
        public override string ItemName => "Millenium";

        public override string ItemLangTokenName => "MILLENIUM";

        public override string ItemPickupDesc => "Create a tidal cataclysm on hit that grounds and collapses enemies. $svCorrupts all Sun Fragments$se.".AutoFormat();

        public override string ItemFullDescription => $"$su{chance}%$se chance on hit to create a $sdtidal cataclysm$se in a $su{baseExplosionRadius}m$se $ss(+{stackExplosionRadius}m per stack)$se area, $sdcollapsing$se and $sugrounding$se enemies for $sd400%$se base damage. $svCorrupts all Sun Fragments$se.".AutoFormat();

        public override string ItemLore =>
        """
        Transcript from the last known log of the UES "Shimmer Mark"

        "Outside, it's so...so purple, that's the only way I can explain it, I can feel the fog from inside the ship...it's...It would be quite peaceful if I even knew where I was. It's making a cool breeze in the ship, like a nice summer's day at the beach...But, but it also hurts somehow...Not physically at least. Something inside of me just can't...take it. It's a searing pain, except not. My body feels like it should be on fire but...it's just a cool breeze...I've been wandering the ship recently, it's not the same ship. It's not. The same. Ship. The halls go on forever, twisting and turning l?ike a maze. Every time I turn around the halls change again and again, over and over again, left turns right forward goes backward. It's ma?ddenin?g. I was barely able to find the br?idge to record this. My body feels like its fa?lling constantly, wind brushing past my suit, I can tell the win?d is real, but I'm not falling. The wind, whist?ling into my e?ars, I can't think s?traig?ht with this n?oise, it's almost like a v?oi?ce. ?I can so?met?imes hear the? wind [WH?ISPE?RIN?G}, Whis?perin?g a tongue ?I dont kn?ow, but I st?ill un?derstand. Th?is [HEL?L] Is mad?denin?g. B?ut I must g?o dee?per. Ro?ckete?er of the [~2@#!@($*&%] signing off."
        """;

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Explosion Radius", "", 12f)]
        public static float baseExplosionRadius;

        [ConfigField("Stack Explosion Radius", "", 4f)]
        public static float stackExplosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.33f)]
        public static float explosionProcCoefficient;

        [ConfigField("Grounding Force", "", -25f)]
        public static float groundingForce;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("MilleniumHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMillenium.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary];

        public static GameObject vfx;

        public static ModdedProcType millenium = ProcTypeAPI.ReserveProcType();

        public override float modelPanelParametersMinDistance => 5f;
        public override float modelPanelParametersMaxDistance => 12f;

        public override ItemDef ItemToCorrupt => SunFragment.instance.ItemDef;

        public override void Init()
        {
            base.Init();

            SetUpMaterial();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Tidal Cataclysm Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Explosion Radius: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Meters);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(chance * procChance * 0.01f, luck),
                    baseExplosionRadius + stackExplosionRadius * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpMaterial()
        {
            var powerElixirGlassMat = new Material(Utils.Assets.Material.matHealingPotionGlass);
            powerElixirGlassMat.SetFloat("_Boost", 0.3f);
            powerElixirGlassMat.SetFloat("_RimPower", 1.567551f);
            powerElixirGlassMat.SetFloat("_RimStrength", 3.52f);
            powerElixirGlassMat.SetFloat("_AlphaBoost", 1f);
            powerElixirGlassMat.SetFloat("IntersectionStrength", 1f);
            powerElixirGlassMat.SetColor("_TintColor", new Color32(255, 44, 115, 255));

            var millenium = Main.hifuSandswept.LoadAsset<GameObject>("MilleniumHolder.prefab");
            var model = millenium.transform.GetChild(0);
            var hourglassMr = model.Find("Vert").GetComponent<MeshRenderer>();
            hourglassMr.material = powerElixirGlassMat;
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.NullifierExplosion, "Millenium VFX", false);

            var trans = vfx.transform;

            var newStarMat = new Material(Paths.Material.matNullifierStarParticle);
            newStarMat.SetColor("_TintColor", new Color32(15, 49, 44, 255));

            var sphere = trans.Find("Sphere");
            var sphereMeshRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one;

            var explosionSphere = trans.Find("ExplosionSphere").GetComponent<ParticleSystemRenderer>();

            var newExplosionMat = new Material(Addressables.LoadAssetAsync<Material>("RoR2/Base/Nullifier/matNullifierExplosionAreaIndicatorSoft.mat").WaitForCompletion());
            newExplosionMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampArcaneCircle.png").WaitForCompletion());
            newExplosionMat.SetColor("_TintColor", new Color32(72, 0, 255, 255));
            newExplosionMat.SetFloat("_RimPower", 1.569113f);
            newExplosionMat.SetFloat("_RimStrength", 2.149275f);
            newExplosionMat.SetFloat("_IntersectionStrength", 20f);

            explosionSphere.material = newExplosionMat;

            var vacuumRadial = trans.Find("Vacuum Radial").GetComponent<ParticleSystemRenderer>();

            var newVacuumMat = new Material(Paths.Material.matNullifierStarPortalEdge);
            newVacuumMat.SetColor("_TintColor", new Color32(3, 4, 255, 246));
            newVacuumMat.SetFloat("_Boost", 13f);
            newVacuumMat.SetFloat("_AlphaBias", 0.8564593f);
            newVacuumMat.SetTexture("_RemapTex", Paths.Texture2D.texRampCrosshair2);

            vacuumRadial.material = newVacuumMat;

            var newSphereMaterials = new Material[2];

            var newPortalMat = new Material(Paths.Material.matNullifierGemPortal);
            newPortalMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium2.png"));
            newPortalMat.SetTexture("_EmissionTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMillenium1.png"));

            sphereMeshRenderer.materials[0] = newPortalMat;

            var newSphereMat = new Material(Paths.Material.matGravsphereCore);
            newSphereMat.SetColor("_TintColor", new Color32(5, 0, 255, 255));
            newSphereMat.SetFloat("_InvFade", 0f);
            newSphereMat.SetFloat("_Boost", 1.090909f);
            newSphereMat.SetFloat("_AlphaBoost", 2.18f);
            newSphereMat.SetFloat("_AlphaBias", 0f);
            newSphereMat.SetTexture("_RemapTex", Paths.Texture2D.texRampHuntressAltColossusArrow);

            newSphereMaterials[0] = newPortalMat;
            newSphereMaterials[1] = newSphereMat;

            sphereMeshRenderer.materials = newSphereMaterials;

            ContentAddition.AddEffect(vfx);
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            if (damageInfo.procChainMask.HasModdedProc(millenium))
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack <= 0)
            {
                return;
            }

            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
            {
                return;
            }

            damageInfo.procChainMask.AddModdedProc(millenium);

            var radius = baseExplosionRadius + stackExplosionRadius * (stack - 1);

            EffectData effectData = new()
            {
                origin = victimBody.corePosition,
                rotation = Quaternion.identity,
                scale = baseExplosionRadius + Mathf.Sqrt(radius)
            };
            EffectManager.SpawnEffect(vfx, effectData, true);

            // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

            float mass;

            if (victimBody.characterMotor) mass = victimBody.characterMotor.mass;
            else if (victimBody.rigidbody) mass = victimBody.rigidbody.mass;
            else mass = 1f;

            BlastAttack blastAttack = new()
            {
                radius = radius,
                baseDamage = 0, // dont ask
                procCoefficient = explosionProcCoefficient,
                crit = damageInfo.crit,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = attackerBody.gameObject,
                teamIndex = attackerBody.teamComponent.teamIndex,
                position = damageInfo.position,
                damageType = DamageType.Silent,
                bonusForce = new Vector3(0f, groundingForce * mass, 0f),
                procChainMask = damageInfo.procChainMask
            };

            var result = blastAttack.Fire();

            var collapse = DotController.GetDotDef(DotController.DotIndex.Fracture);

            for (int i = 0; i < result.hitPoints.Length; i++)
            {
                var hitPoint = result.hitPoints[i];

                var hurtBox = hitPoint.hurtBox;
                if (!hurtBox)
                {
                    continue;
                }

                var hc = hurtBox.healthComponent;
                if (!hc)
                {
                    continue;
                }

                DotController.InflictDot(hitPoint.hurtBox.healthComponent.gameObject, attackerBody.gameObject, DotController.DotIndex.Fracture, collapse.interval, 1f, uint.MaxValue);
            }

            Util.PlaySound("Play_item_void_clover", victimBody.gameObject);
            Util.PlaySound("Play_voidDevastator_death_VO", victimBody.gameObject);
            Util.PlaySound("Play_voidDevastator_death_VO", victimBody.gameObject);
            Util.PlaySound("Play_clayboss_m2_explo", victimBody.gameObject);
            Util.PlaySound("Play_engi_M2_explo", victimBody.gameObject);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();

            var itemDisplay = SetUpFollowerIDRS(0.7f, 120f, true, -7f, true, -10f, true, -35f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(2f, 0.4f, -1.5f),
                localScale = new Vector3(0.125f, 0.125f, 0.125f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}