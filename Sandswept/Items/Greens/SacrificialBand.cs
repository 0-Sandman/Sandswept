namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Sacrificial Band")]
    public class SacrificialBand : ItemBase<SacrificialBand>
    {
        public override string ItemName => "Sacrificial Band";

        public override string ItemLangTokenName => "SACRIFICIAL_BAND";

        public override string ItemPickupDesc => "High damage hits also make enemies bleed. Recharges over time.";

        public override string ItemFullDescription => $"Hits that deal $sdmore than {percentDamageThreshold * 100f}% damage$se also inflict $sd{baseBleedCount}$se $ss(+{stackBleedCount} per stack)$se $sdbleeds$se on enemies for each $sd{damageScalar * 100f}%$se of $sddamage dealt$se. Recharges every $su{cooldown}$se seconds.".AutoFormat();

        public override string ItemLore =>
        """
        "When we draw our final breaths,
        When N'kuhana's grasp entwines us,
        May our patience and our solace
        Clear the clouds of deathly silence.
        Will you live with me?"

        - The Syzygy of Io and Europa
        """;
        public override float modelPanelParametersMinDistance => 4f;
        public override float modelPanelParametersMaxDistance => 10f;

        [ConfigField("Base Bleed Count", "", 1)]
        public static float baseBleedCount;

        [ConfigField("Stack Bleed Count", "", 1)]
        public static float stackBleedCount;

        [ConfigField("Per Skill Damage Scalar", "Decimal.", 1.1f)]
        public static float damageScalar;

        [ConfigField("Percent Damage Threshold", "Decimal.", 4f)]
        public static float percentDamageThreshold;

        [ConfigField("Cooldown", "", 10f)]
        public static float cooldown;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("SacrificialBandHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSacrificialBand.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CanBeTemporary };

        public static BuffDef readyBuff;
        public static BuffDef cooldownBuff;

        public static GameObject vfx;
        public static Material bleedingMat;

        public override void Init()
        {
            base.Init();
            SetUpBuffs();
            SetUpVFX();
        }

        public void SetUpBuffs()
        {
            readyBuff = ScriptableObject.CreateInstance<BuffDef>();
            readyBuff.isDebuff = false;
            readyBuff.canStack = false;
            readyBuff.isHidden = false;
            readyBuff.isCooldown = false;
            readyBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandReady.png");
            readyBuff.buffColor = new Color32(160, 0, 5, 255);
            readyBuff.name = "Sacrificial Band Ready";

            ContentAddition.AddBuffDef(readyBuff);

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.canStack = true;
            cooldownBuff.isDebuff = false;
            cooldownBuff.isHidden = false;
            cooldownBuff.isCooldown = true;
            cooldownBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandCooldown.png");
            cooldownBuff.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldownBuff.name = "Sacrificial Band Cooldown";

            ContentAddition.AddBuffDef(cooldownBuff);
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.ElementalRingVoidImplodeEffect, "Sacrificial Band VFX", false);
            var effectComponent = vfx.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_gravekeeper_attack2_impact";
            effectComponent.applyScale = true;
            var trans = vfx.transform;

            var reb = new Color32(205, 0, 10, 255);

            VFXUtils.RecolorMaterialsAndLights(vfx, reb, reb, true);
            VFXUtils.MultiplyScale(vfx, 2.5f);

            var pointLight = trans.Find("Point Light");
            var light = pointLight.GetComponent<Light>();
            light.intensity = 150f;
            light.range = 50f;
            var lightIntensityCurve = pointLight.GetComponent<LightIntensityCurve>();
            lightIntensityCurve.timeMax = 0.5f;

            VFXUtils.MultiplyDuration(vfx, 1.5f);

            ContentAddition.AddEffect(vfx);

            bleedingMat = new Material(Paths.Material.matBlighted);
            bleedingMat.SetColor("_TintColor", new Color32(193, 2, 0, 209));
            bleedingMat.SetTexture("_MainTex", null);
            bleedingMat.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);
            bleedingMat.SetFloat("_Boost", 1f);
            bleedingMat.SetFloat("_AlphaBoost", 7.711387f);
            bleedingMat.SetFloat("_AlphaBias", 1f);
            bleedingMat.SetFloat("_FresnelPower", 0.4284072f);
            bleedingMat.name = "matBleeding";
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var inventory = body.inventory;
            if (!inventory)
            {
                return;
            }
            body.AddItemBehavior<SacrificialBandController>(inventory.GetItemCount(instance.ItemDef));
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool triggered = false;
            CharacterBody attackerBody = null;
            var attacker = damageInfo.attacker;
            if (attacker && victim)
            {
                attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if (attackerBody && victimBody)
                {
                    var stack = GetCount(attackerBody);
                    if (stack > 0)
                    {
                        var skillDamage = damageInfo.damage / attackerBody.damage;

                        if (attackerBody.HasBuff(readyBuff) && skillDamage >= percentDamageThreshold)
                        {
                            triggered = true;

                            var realerDamageScalar = 1f / damageScalar;
                            var scaledSkillDamage = skillDamage * realerDamageScalar;
                            var stackScalar = baseBleedCount + stackBleedCount * (stack - 1);
                            var roundedSkillDamage = Mathf.RoundToInt(scaledSkillDamage) * stackScalar;

                            for (int i = 0; i < roundedSkillDamage; i++)
                            {
                                DotController.InflictDot(victim, attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f, uint.MaxValue);
                            }

                            for (int i = 0; i < 2; i++)
                            {
                                Util.PlaySound("Play_vulture_attack1_impact", victim);
                                Util.PlaySound("Play_imp_impact", victim);
                                Util.PlaySound("Play_gravekeeper_attack2_impact", victim);
                                Util.PlaySound("Play_lunar_wisp_impact", victim);
                            }

                            Util.PlaySound("Play_item_proc_dagger_impact", attacker);
                            Util.PlaySound("Play_vulture_attack1_impact", attacker);
                            Util.PlaySound("Play_vulture_attack1_impact", attacker);
                            Util.PlaySound("Play_vulture_death_land_thud", attacker);

                            EffectManager.SpawnEffect(vfx, new EffectData() { origin = victimBody.corePosition, rotation = Quaternion.identity }, true);

                            var modelLocator = victimBody.modelLocator;
                            if (modelLocator)
                            {
                                var modelTransform = modelLocator.modelTransform;
                                if (modelTransform)
                                {
                                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                                    temporaryOverlay.duration = 3f;
                                    temporaryOverlay.animateShaderAlpha = true;
                                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                                    temporaryOverlay.destroyComponentOnEnd = true;
                                    temporaryOverlay.originalMaterial = bleedingMat;
                                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                                }
                            }
                        }
                    }
                }
            }

            orig(self, damageInfo, victim);

            if (triggered && attackerBody.HasBuff(readyBuff))
            {
                attackerBody.RemoveBuff(readyBuff);
                for (int j = 1; j <= cooldown; j++)
                {
                    attackerBody.AddTimedBuff(cooldownBuff, j);
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
                    childName = "Head",
                    localPos = new Vector3(0.00006F, -0.73108F, -0.00924F),
                    localAngles = new Vector3(85.04073F, 197.8464F, 22.78797F),
                    localScale = new Vector3(0.19069F, 0.17992F, 0.17806F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;
        }
    }

    public class SacrificialBandController : CharacterBody.ItemBehavior
    {
        public bool shouldRun = false;

        public void Start()
        {
            if (!body.HasBuff(SacrificialBand.readyBuff) && stack > 0)
            {
                body.AddBuff(SacrificialBand.readyBuff);
                shouldRun = true;
            }
        }

        public void FixedUpdate()
        {
            if (!body || !shouldRun)
            {
                return;
            }

            if (!body.HasBuff(SacrificialBand.cooldownBuff) && !body.HasBuff(SacrificialBand.readyBuff))
            {
                body.AddBuff(SacrificialBand.readyBuff);
            }
        }

        public void OnDestroy()
        {
            if (body.HasBuff(SacrificialBand.readyBuff))
            {
                body.RemoveBuff(SacrificialBand.readyBuff);
            }
            if (body.HasBuff(SacrificialBand.cooldownBuff))
            {
                body.RemoveBuff(SacrificialBand.cooldownBuff);
            }
        }
    }
}