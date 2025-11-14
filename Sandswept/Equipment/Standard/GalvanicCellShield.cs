namespace Sandswept.Equipment.Standard
{
    [ConfigSection("Equipment :: Galvanic Cell Shield")]
    public class GalvanicCellShield : EquipmentBase<GalvanicCellShield>
    {
        public override string EquipmentName => "Galvanic Cell Shield";

        public override string EquipmentLangTokenName => "GALVANIC_CELL_SHIELD";

        public override string EquipmentPickupDesc => "Evoke a shield that parries the next attack. Upon successfully parrying, shock and damage your attacker and nearby enemies.";

        public override string EquipmentFullDescription => $"Evoke a $shshield$se that $shparries$se the next attack. Upon successfully parrying, $sushock$se and $sddamage$se your attacker and nearby enemies for $sd{baseDamage * 100f}% damage$se.".AutoFormat();

        public override string EquipmentLore =>
        """
        <style=cMono>
        Welcome to DataScraper (v3.1.53 - beta branch)
        $ Scraping memory...done.
        $ Resolving...done.
        $ Combing for relevant data...done.
        Complete!
        </style>

        The following is the script for an advertisement by Gell n Vowell™ for the Galvanic Shield, presented in an executive meeting on 4/17/2055 before the company's dissolution by the Mercurian government.

        [Scene starts; Burglar 1 attacks Helpless woman on the street]
        <b>Burglar 1: GRAH!
        Helpless Woman: AHHHH!!</b>
        [Explosion sfx followed by outsourced lightning vfx]
        <b>Narrator: Has this ever happened to you? You're just walking down the street, in some back alley place...like Mercury, and then BAM. You're being robbed, burglarized even? This is the product for you.</b>
        [More outsourced lightning sfx and vfx, fading into an ominous orb]
        <b>Narrator: Introducing...the Galvanized Shield© Max Pro 2™! An all new way to defend yourself from nefarious ne'er do wells. Packed with 100,000(kV), your attacker will be atomized the second they lay a finger on you!</b>
        [Scene two starts. Burglar 2 walks up to Helpless woman]
        <b>Burglar 2: GR-huh-Wait! NO! N-</b> [Zapping noise followed by sand falling]
        <b>Helpless Woman: Take that- oh-OH MY GOD! DIRECTOR! DIRECTOR!</b>
        [Noises of the woman panicking are quietly played in the background while the narrator explains the product. The image of Galvanic Shield is shown.]
        <b>Narrator: The Max Pro 2's design is much more ergonomic...</b> [Awkward silence] <b>The Sphere itself is made purely out of a new material that siphons energy from the atmosphere, into the palm of your hands. We've dubbed this technology..."Statictanium". Statictanium is the next step in military grade self defense products. We took apart a Mercurian Fighter Ship and compacted the materials into Statictanium! Finally, The Max Pro 2 requires the special Max Pro 2 Glove to properly work. This thing's so lethal we accidentally caused a governmental coup on Mercury!</b>
        [Fades into a white screen with text. Text TBD. Currently its "Add Text Here" 5 times descending down the middle]
        <b>Narrator: Very. Very. Very. Very. Very limited time. We're currently being sued by the Mercurian Military, and the rest of the solar system for this Ground-breaking Breakthrough in Self Defense Technology because THEY don't want you to have it. BUY NOW!</b>
        [Screen fades to black]
        """;
        public override GameObject EquipmentModel => Main.assets.LoadAsset<GameObject>("PickupCellShield.prefab");
        public override float Cooldown => 20f;
        public override Sprite EquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texGalvanicCellShield.png");

        [ConfigField("Activation Length", "", 0.5f)]
        public static float activationTime;

        [ConfigField("Base Damage", "Decimal.", 20f)]
        public static float baseDamage;

        [ConfigField("Enemy Hit Damage", "Decimal.", 2f)]
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
                childName = "Head",
                localPos = new Vector3(1, -1, -0.9f),
                localScale = new Vector3(0.5f, 0.5f, 0.5f),
                followerPrefab = Main.assets.LoadAsset<GameObject>("DisplayCellShield.prefab"),
                limbMask = LimbFlags.None,
                followerPrefabAddress = new("useless"),
            });
        }

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public void SetUpVFX()
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
        }

        public static void PulseShieldForBody(CharacterBody body)
        {
            if (!body)
            {
                return;
            }

            if (!body.equipmentSlot)
            {
                return;
            }

            var display = body.equipmentSlot.FindActiveEquipmentDisplay();
            if (!display || !display.GetComponent<ItemFollower>())
            {
                return;
            }

            var followerInstance = display.GetComponent<ItemFollower>().followerInstance;
            if (!followerInstance)
            {
                return;
            }

            display = followerInstance.transform;

            var childLocator = display.GetComponent<ChildLocator>();
            if (!childLocator)
            {
                return;
            }

            Transform model = childLocator.FindChild("model");

            if (!model)
            {
                return;
            }

            var animator = model.GetComponent<Animator>();
            if (!animator)
            {
                return;
            }

            int layer = animator.GetLayerIndex("Base");
            animator.Play("Pulse", layer);

            var parent = model.parent;
            if (!parent)
            {
                return;
            }

            foreach (ParticleSystem system in parent.GetComponentsInChildren<ParticleSystem>())
            {
                system.Play();
            }
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<GalvanicCellShieldController>(body.inventory.GetEquipment(body.inventory.activeEquipmentSlot).equipmentDef == EquipmentDef ? 1 : 0);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.body.HasBuff(Buffs.ParryBuff.instance.BuffDef) && damageInfo.damage > 0f && damageInfo.procCoefficient > 0)
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
        private float timer = 0;
        public DamageInfo damageInfo;
        public bool activated = false;
        private bool hasFired = false;
        private float projectileDeletionRadius;
        private bool someThingThatIsSupposedToRunOnce = false;

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
                if (!someThingThatIsSupposedToRunOnce)
                {
                    CleanBuffsServer();
                    if (!body.HasBuff(Buffs.ParryBuff.instance.BuffDef))
                    {
                        body.AddBuff(Buffs.ParryBuff.instance.BuffDef);
                    }
                    someThingThatIsSupposedToRunOnce = true;
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
            someThingThatIsSupposedToRunOnce = false;
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
                // EffectManager.SpawnEffect(effectPrefab, effectData, true);
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

                var retaliateInfo = new DamageInfo
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
                };

                if (body.teamComponent && body.teamComponent.teamIndex != TeamIndex.Player)
                {
                    retaliateInfo.damageType |= DamageType.NonLethal;
                }

                attackerHc.TakeDamage(retaliateInfo);
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