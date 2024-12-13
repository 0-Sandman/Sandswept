namespace Sandswept.Equipment.Standard
{
    [ConfigSection("Equipment :: Galvanic Cell Shield")]
    public class GalvanicCellShield : EquipmentBase
    {
        public override string EquipmentName => "Galvanic Cell Shield";

        public override string EquipmentLangTokenName => "GALVANIC_CELL_SHIELD";

        public override string EquipmentPickupDesc => "Evoke a shield that parries the next attack. Upon successfully parrying, shock and damage your attacker and nearby enemies.";

        public override string EquipmentFullDescription => ("Evoke a $shshield$se that $shparries$se the next attack. Upon successfully parrying, $sushock$se and $sddamage$se your attacker and nearby enemies for $sd" + d(baseDamage) + " damage$se.").AutoFormat();

        // this description is fucked, idk how to unfuck it due to how complex the item is
        public override string EquipmentLore => "TBD";

        public override GameObject EquipmentModel => Main.Assets.LoadAsset<GameObject>("PickupCellShield.prefab");
        public override float Cooldown => 20f;
        public override Sprite EquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texGalvanicCellShield.png");

        [ConfigField("Activation Length", "", 0.5f)]
        public static float activationTime;

        [ConfigField("Base Damage", "Decimal.", 20f)]
        public static float baseDamage;

        [ConfigField("Hit Damage", "Decimal.", 2f)]
        public static float hitDamage;

        [ConfigField("Blast Field Radius", "", 12f)]
        public static float radius;

        [ConfigField("Projectile Graze Radius", "", 2.5f)]
        public static float grazeRadius;

        public static GameObject vfx;
        public static BlastAttack blastAttack;
        public static GameObject ShieldEffect;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Base",
                localPos = new Vector3(1, -1, -0.9f),
                localScale = new Vector3(0.5f, 0.5f, 0.5f),
                followerPrefab = Main.Assets.LoadAsset<GameObject>("DisplayCellShield.prefab"),
                limbMask = LimbFlags.None
            });
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();

            CreateEquipment();
            Hooks();
        }

        public static void PulseShieldForBody(CharacterBody body)
        {
            if (!body.equipmentSlot) return;
            Transform display = body.equipmentSlot.FindActiveEquipmentDisplay();
            if (!display.GetComponent<ItemFollower>())
            {
                return;
            }

            display = display.GetComponent<ItemFollower>().followerInstance.transform;

            ChildLocator loc = display.GetComponent<ChildLocator>();
            if (!loc) return;

            Transform model = loc.FindChild("model");

            if (!model) return;

            Animator anim = model.GetComponent<Animator>();
            int layer = anim.GetLayerIndex("Base");
            anim.Play("Pulse", layer);

            foreach (ParticleSystem system in model.parent.GetComponentsInChildren<ParticleSystem>())
            {
                system.Play();
            }
        }

        public override void Hooks()
        {
            vfx = Paths.GameObject.EngiShield.InstantiateClone("Parry VFX", false);
            vfx.RemoveComponent<TemporaryVisualEffect>();
            foreach (ObjectScaleCurve item in vfx.GetComponents<ObjectScaleCurve>())
            {
                Object.Destroy(item);
            }
            vfx.RemoveComponent<Billboard>();
            vfx.AddComponent<NetworkIdentity>();
            var component = vfx.AddComponent<EffectComponent>();
            // component.applyScale = true;
            component.parentToReferencedTransform = true;
            component.positionAtReferencedTransform = true;

            //var scale = vfx.AddComponent<ObjectScaleCurve>();
            //scale.overallCurve = Main.dgoslingAssets.LoadAsset<AnimationCurveAsset>("ACAparryVFXScale").value;
            //scale.useOverallCurveOnly = true;
            //scale.timeMax = 1;
            //scale.resetOnAwake = false;
            //scale.enabled = false;
            vfx.GetComponent<DestroyOnTimer>().enabled = true;
            vfx.RegisterNetworkPrefab();
            Main.EffectPrefabs.Add(vfx);
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;

            // EffectManager.SpawnEffect()
            // ShieldEffect.GetComponent<ObjectScaleCurve>().timeMax = 0.1f;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<GalvanicCellShieldController>(body.inventory.GetEquipment(body.inventory.activeEquipmentSlot).equipmentDef == EquipmentDef ? 1 : 0);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.body.HasBuff(Buffs.ParryBuff.instance.BuffDef) && damageInfo.damage > 0f)
            {
                HandleParryBuffsServer(self.body, damageInfo);
                return;
            }
            orig(self, damageInfo);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.characterBody == null)
            {
                return false;
            }

            var galvanicCellShieldController = slot.characterBody.GetComponent<GalvanicCellShieldController>();

            PulseShieldForBody(slot.characterBody);

            if (galvanicCellShieldController)
            {
                galvanicCellShieldController.activated = true;
            }

            EffectManager.SpawnEffect(Paths.GameObject.LoaderGroundSlam, new EffectData
            {
                origin = slot.characterBody.corePosition,
                scale = slot.characterBody.bestFitRadius * 2f
            }, true);

            return true;
        }

        public static void HandleParryBuffsServer(CharacterBody body, DamageInfo damageInfo)
        {
            if (body.HasBuff(Buffs.ParryBuff.instance.BuffDef))
            {
                body.RemoveBuff(Buffs.ParryBuff.instance.BuffDef);
            }

            if (!body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef))
            {
                body.AddBuff(Buffs.ParryActivatedBuff.instance.BuffDef);
            }

            body.AddTimedBuff(Paths.BuffDef.bdImmune, 0.5f);
            body.GetComponent<GalvanicCellShieldController>().damageInfo = damageInfo;
            return;
        }
    }

    public class GalvanicCellShieldController : CharacterBody.ItemBehavior
    {
        private GameObject effectPrefab;
        private float timer = 0;
        public DamageInfo damageInfo;
        public bool activated = false;
        private bool hasFired = false;
        private float projectileDeletionRadius;

        public void Start()
        {
            //  if (NetworkServer.active)
            //   {
            //        CleanBuffsServer();
            //        if (!body.HasBuff(Buffs.ParryBuff.instance.BuffDef)) body.AddBuff(Buffs.ParryBuff.instance.BuffDef);
            //    }
            projectileDeletionRadius = GalvanicCellShield.grazeRadius + body.radius;
            // On.RoR2.ObjectScaleCurve.Reset += ObjectScaleCurve_Reset;
            // On.RoR2.EffectManager.SpawnEffect_GameObject_EffectData_bool += EffectManager_SpawnEffect_GameObject_EffectData_bool;
        }

        private void CleanBuffsServer()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef))
            {
                body.RemoveBuff(Buffs.ParryActivatedBuff.instance.BuffDef);
            }

            if (body.HasBuff(Buffs.ParryBuff.instance.BuffDef))
            {
                body.RemoveBuff(Buffs.ParryBuff.instance.BuffDef);
            }
        }

        private void FixedUpdate()
        {
            if (activated)
            {
                if (!effectPrefab)
                {
                    effectPrefab = GalvanicCellShield.vfx;
                    var component = effectPrefab.AddComponent<ObjectScaleCurve>();
                    component.baseScale = Vector3.one * GalvanicCellShield.radius;
                    component.overallCurve = Main.dgoslingAssets.LoadAsset<AnimationCurveAsset>("ACAparryVFXScale").value;
                    component.useOverallCurveOnly = true;
                    component.timeMax = 1f;
                    component.resetOnAwake = true;
                    component.enabled = true;
                    CleanBuffsServer();
                    if (!body.HasBuff(Buffs.ParryBuff.instance.BuffDef))
                    {
                        body.AddBuff(Buffs.ParryBuff.instance.BuffDef);
                    }
                }
                if (body.HasBuff(Buffs.ParryBuff.instance.BuffDef) || body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef))
                {
                    timer += Time.fixedDeltaTime;
                    if (damageInfo != null && !hasFired)
                    {
                        DoAttack();
                    }
                }

                if (timer >= GalvanicCellShield.activationTime)
                {
                    Reset();
                }
            }
        }

        private void Reset()
        {
            damageInfo = null;
            activated = false;
            hasFired = false;
            timer = 0f;
            effectPrefab = null;
            CleanBuffsServer();
        }

        private void DoAttack()
        {
            if (!NetworkServer.active || !body)
            {
                return;
            }

            bool parry = body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef);
            if (!parry)
            {
                return;
            }

            DamageType damageType = DamageType.Shock5s;

            float damageCoefficient = GalvanicCellShield.baseDamage * body.damage + damageInfo.damage * GalvanicCellShield.hitDamage;
            float radius = GalvanicCellShield.radius;
            if (parry)
            {
                hasFired = true;

                //DeleteProjectilesServer(projectileDeletionRadius);
                Util.CleanseBody(body, false, false, false, false, false, true);
                EffectData effectData = new()
                {
                    origin = body.modelLocator.modelBaseTransform.position,
                    scale = radius,
                    rootObject = body.gameObject
                };
                EffectManager.SpawnEffect(effectPrefab, effectData, true);
                // effectPrefab.GetComponent<ObjectScaleCurve>().enabled = true;
                BlastAttack.Result result;
                result = new BlastAttack
                {
                    //impactEffect = EffectCatalog.FindEffectIndexFromPrefab()
                    attacker = body.gameObject,
                    inflictor = body.gameObject,
                    teamIndex = TeamComponent.GetObjectTeam(body.gameObject),
                    baseDamage = 0f,
                    baseForce = 0f,
                    position = body.corePosition,
                    radius = radius,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageType = damageType,
                    attackerFiltering = AttackerFiltering.NeverHitSelf
                }.Fire();

                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (!attackerBody)
                {
                    return;
                }

                var attackerHc = attackerBody.healthComponent;
                if (!attackerHc)
                {
                    return;
                }

                attackerHc.TakeDamage(new DamageInfo
                {
                    attacker = body.gameObject,
                    damage = damageCoefficient,
                    damageType = damageType,
                    crit = Util.CheckRoll(body.master.luck, body.master),
                    position = body.corePosition,
                    force = Vector3.zero,
                    damageColorIndex = DamageColorIndex.Default,
                    procCoefficient = 0f,
                    procChainMask = default
                });
                Reset();
            }
        }

        private void DeleteProjectilesServer(float rad)
        {
            List<ProjectileController> projectileControllers = new List<ProjectileController>();

            Collider[] array = Physics.OverlapSphere(body.corePosition, rad, LayerIndex.projectile.mask);
            for (int i = 0; i < array.Length; i++)
            {
                var projectileController = array[i].GetComponentInParent<ProjectileController>();
                if (projectileController && !projectileController.cannotBeDeleted && projectileController.owner != body.gameObject && !(projectileController.teamFilter && projectileController.teamFilter.teamIndex == TeamComponent.GetObjectTeam(body.gameObject)))
                {
                    bool cannotDelete = false;
                    var projectileSimple = projectileController.gameObject.GetComponent<ProjectileSimple>();
                    var projectileCharacterController = projectileController.gameObject.GetComponent<ProjectileCharacterController>();

                    if ((!projectileSimple || projectileSimple.desiredForwardSpeed == 0) && !projectileCharacterController)
                    {
                        cannotDelete = true;
                    }

                    if (!cannotDelete && !projectileControllers.Contains(projectileController))
                    {
                        projectileControllers.Add(projectileController);
                    }
                }
            }

            int projectilesDeleted = projectileControllers.Count;
            for (int i = 0; i < projectilesDeleted; i++)
            {
                GameObject toDelete = projectileControllers[i].gameObject;
                if (toDelete)
                {
                    Destroy(toDelete);
                    // shouldnt it be NetworkServer.Destroy()?
                }
            }
        }
    }
}