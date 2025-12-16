using RoR2.Orbs;
using Sandswept.Items.Greens;

namespace Sandswept.Survivors.Electrician.States
{
    public class SignalOverloadCharge : BaseSkillState
    {
        public float baseDuration = 1.8f;
        public float shieldDrained = 0f;
        public float stopwatch = 0f;
        public float delay = 0.7f / 10;
        public float drainAmount;
        public float baseMax = 0.4f;
        public float radiusMultiplierParam = 1f;
        public float damageMultiplierParam = 1f;

        public override void OnEnter()
        {
            base.OnEnter();

            // baseDuration /= attackSpeedStat;

            delay = baseDuration / 10f;

            FindModelChild("Tethers").gameObject.SetActive(true);

            PlayAnimation("Gesture, Override", "StartOverload", "Generic.playbackRate", baseDuration * 2f);
            PlayAnimation("ChestSpin", "Winding", "Generic.playbackRate", baseDuration * 2f);

            // base.characterMotor.walkSpeedPenaltyCoefficient = 0.1f;
            // Util.PlaySound("Play_ui_obj_nullWard_charge_loop", gameObject);

            Util.PlaySound("Play_elec_r_wind", gameObject);

            drainAmount = healthComponent.fullShield * delay;
        }

        public override void OnExit()
        {
            base.OnExit();
            // base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            // Util.PlaySound("Stop_ui_obj_nullWard_charge_loop", gameObject);

            FindModelChild("Tethers").gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= baseDuration)
            {
                if (base.healthComponent.shield > 0 && shieldDrained <= base.healthComponent.shield * 0.8f)
                {
                    float shieldToDrain = base.healthComponent.shield;

                    if (shieldToDrain > healthComponent.shield)
                    {
                        shieldToDrain = healthComponent.shield;
                    }

                    if (shieldToDrain > 0f)
                    {
                        shieldDrained += shieldToDrain;

                        if (NetworkServer.active)
                        {
                            var damageInfo = new DamageInfo
                            {
                                position = transform.position,
                                damage = shieldToDrain,
                                procCoefficient = 0f,
                                damageType = DamageType.NonLethal | DamageType.BypassArmor | DamageType.BypassBlock,
                                damageColorIndex = DamageColorIndex.Luminous,
                                attacker = null
                            };

                            damageInfo.AddModdedDamageType(Main.eclipseSelfDamage);
                            damageInfo.AddModdedDamageType(MakeshiftPlate.BypassPlating);

                            healthComponent.TakeDamage(damageInfo);
                        }
                    }
                }

                var percent = healthComponent.fullHealth * baseMax;

                radiusMultiplierParam = Util.Remap(shieldDrained, 0f, percent, 1f, 1.4f);
                damageMultiplierParam = Util.Remap(shieldDrained, 0f, percent, 1f, 1.611f);

                outer.SetNextState(new SignalOverloadDischarge());
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= delay)
            {
                stopwatch = 0f;
                float shieldToDrain = drainAmount;

                if (shieldToDrain > healthComponent.shield)
                {
                    shieldToDrain = healthComponent.shield;
                }

                if (shieldToDrain > 0f)
                {
                    shieldDrained += shieldToDrain;

                    if (NetworkServer.active)
                    {
                        var damageInfo = new DamageInfo()
                        {
                            position = transform.position,
                            damage = shieldToDrain,
                            procCoefficient = 0f,
                            damageType = DamageType.NonLethal | DamageType.BypassArmor | DamageType.BypassBlock,
                            damageColorIndex = DamageColorIndex.Luminous,
                            attacker = null
                        };

                        damageInfo.AddModdedDamageType(Main.eclipseSelfDamage);
                        damageInfo.AddModdedDamageType(MakeshiftPlate.BypassPlating);

                        healthComponent.TakeDamage(damageInfo);

                    }
                }
            }
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);

            if (nextState is SignalOverloadDischarge signalOverloadDischarge)
            {
                signalOverloadDischarge.multiplier = Mathf.Min(radiusMultiplierParam, 2f); // second value is the max "overshoot" value (consuming above 40% shield, for synergy with psg, trans, etc)
                signalOverloadDischarge.damageMultiplier = Mathf.Min(damageMultiplierParam, 2.4165f); // 1.611 * 1.5 => 2.4165, so up to lots more damage :smirk: | second value is the max "overshoot" value (consuming above 40% shield, for synergy with psg, trans, etc)
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class SignalOverloadDischarge : BaseSkillState
    {
        public float duration = 3f;
        public float totalDamageCoef = 9.9f; // used to be 16f and scale down to 0.6x with no shield drained, and stay 16x with full shield - now it's 1x to 1.6x of this value depending on shield drained
        public int totalHits = 10;
        public float delay;
        public float coeff;
        public float radius = 18f;
        public float stopwatch = 0f;
        public float multiplier = 1f; // radius multiplier
        public float damageMultiplier = 1f;
        public Animator animator;
        public GameObject effect;
        public LineRenderer lr;
        public GameObject beamEffect;
        public Transform head;
        public Vector3 pos;
        public Transform origin;
        public Transform end;
        public CameraTargetParams.CameraParamsOverrideHandle handle;
        public Transform modelTransform;
        public GameObject signalIndicatorInstance;
        public GameObject signalIndicator;
        public GameObject beamVFX;

        public override void OnEnter()
        {
            base.OnEnter();

            totalHits = Mathf.FloorToInt(10 * attackSpeedStat);
            delay = duration / totalHits;
            totalDamageCoef *= damageMultiplier;
            coeff = totalDamageCoef / 10f;

            radius *= multiplier;

            PlayAnimation("Gesture, Override", "OverloadLoop", "Generic.playbackRate", 0.5f);
            animator = GetModelAnimator();
            animator.SetBool("discharging", true);

            // Util.PlaySound("Play_roboBall_attack2_mini_active_loop", gameObject);
            // Util.PlaySound("Play_ui_obj_nullWard_charge_loop", gameObject);
            // Util.PlaySound("Play_captain_m1_shotgun_charge_loop", gameObject);

            Util.PlaySound("Play_elec_r_loop", gameObject);

            head = FindModelChild("Head");

            FindModelChild("Tethers").gameObject.SetActive(true);

            handle = cameraTargetParams.AddParamsOverride(new()
            {
                cameraParamsData = Paths.CharacterCameraParams.ccpToolbot.data
            }, 0.3f);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                signalIndicator = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.SignalOverload.indicatorCovenant,
                    _ => VFX.SignalOverload.indicatorDefault
                };

                effect = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.SignalOverload.impactCovenant,
                    _ => VFX.SignalOverload.impactDefault
                };

                beamVFX = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.SignalOverload.beamCovenant,
                    _ => VFX.SignalOverload.beamDefault
                };

            }

            signalIndicatorInstance = Object.Instantiate(signalIndicator, new Vector3(0, -4000, 0), Quaternion.identity);
            signalIndicatorInstance.transform.localScale = Vector3.one * radius * 2f;

            beamEffect = Object.Instantiate(beamVFX, head.position, head.rotation);
            origin = beamEffect.GetComponent<ChildLocator>().FindChild("Start");
            end = beamEffect.GetComponent<ChildLocator>().FindChild("End");

            lr = beamEffect.GetComponent<LineRenderer>();
        }

        public override void Update()
        {
            beamEffect.transform.position = head.transform.position;
            origin.transform.position = head.transform.position;
            end.transform.position = pos;
            signalIndicatorInstance.transform.position = pos;

            characterBody.SetSpreadBloom(12f, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Util.CharacterRaycast(gameObject, GetAimRay(), out RaycastHit info, 300f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
            {
                pos = info.point;
            }
            else
            {
                pos = GetAimRay().GetPoint(100f);
            }

            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                stopwatch = delay;
                Util.PlaySound("Play_item_proc_armorReduction_hit", gameObject);
                Util.PlaySound("Play_jellyfish_detonate", end.gameObject);

                HandleBlastAuthority(pos);
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            animator.SetBool("discharging", false);
            // Util.PlaySound("Stop_roboBall_attack2_mini_active_loop", gameObject);
            // Util.PlaySound("Stop_ui_obj_nullWard_charge_loop", gameObject);
            // Util.PlaySound("Stop_captain_m1_shotgun_charge_loop", gameObject);

            Util.PlaySound("Stop_elec_r_loop", gameObject);

            if (beamEffect)
            {
                Destroy(beamEffect);
            }

            if (signalIndicatorInstance)
            {
                Destroy(signalIndicatorInstance);
            }

            FindModelChild("Tethers").gameObject.SetActive(false);

            cameraTargetParams.RemoveParamsOverride(handle, 0.3f);

            skillLocator.special.DeductStock(1);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public void HandleBlastAuthority(Vector3 position)
        {
            SphereSearch search = new()
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = position
            };
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(GetTeam()));

            BlastAttack attack = new()
            {
                radius = radius * 0.125f, // used to be radius * 0.1f - 1.8m to 4.5m aoe, now it is 2.25m to 4.5m
                baseDamage = damageStat * coeff * 1.5f, // used to be 1.8f, but with the damage coeff changes I had to balance it back
                damageType = DamageType.Stun1s | DamageType.Shock5s,
                crit = RollCrit(),
                attacker = gameObject,
                teamIndex = GetTeam(),
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = 1f,
                position = position
            };

            attack.damageType.damageSource = DamageSource.Special;

            var res = attack.Fire();
            List<HurtBox> boxesHit = new();
            foreach (var point in res.hitPoints)
            {
                if (point.hurtBox) boxesHit.Add(point.hurtBox);
            }

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = attack.position,
                scale = attack.radius * 3f
            }, true);

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                if (boxesHit.Contains(box))
                {
                    continue;
                }

                var boxHealthComponent = box.healthComponent;
                if (!boxHealthComponent)
                {
                    continue;
                }

                var enemyBody = boxHealthComponent.body;
                if (!enemyBody)
                {
                    continue;
                }

                VoltLightningOrb orb = new()
                {
                    attacker = gameObject,
                    damageValue = damageStat * coeff,
                    isCrit = RollCrit(),
                    origin = position,
                    procCoefficient = 1f - Util.Remap(Vector3.Distance(base.transform.position, box.transform.position), 0f, 42.66f, 0f, 0.8f), // used to be 60f at a maximum radius of, 45m - 75%/133%, so now it is 42.66f to 32m max radius, still 75%/133%
                    target = box,
                    teamIndex = GetTeam(),
                    attackerBody = characterBody,
                    victimBody = enemyBody
                };

                orb.AddModdedDamageType(Electrician.Grounding);

                CharacterMotor motor = box.healthComponent.GetComponent<CharacterMotor>();
                RigidbodyMotor motor2 = box.healthComponent.GetComponent<RigidbodyMotor>();

                if (motor)
                {
                    motor.Motor.ForceUnground();
                    motor.velocity += (position - motor.transform.position).normalized * (14.5f * delay);
                }

                if (motor2)
                {
                    PhysForceInfo info = new();
                    info.massIsOne = true;
                    info.force = (position - motor2.transform.position).normalized * (2.5f * delay);
                    motor2.ApplyForceImpulse(in info);
                }

                OrbManager.instance.AddOrb(orb);
            }
        }
    }

    // please rename this and put it in its own class, it's so confusing and unnecessarily here
    // also maybe gradually rename all files related electrician to vol-t? pls.. 1:1 naming is so readable and clearcut and peak and based
    public class SignalOverloadFire : BaseSkillState
    {
        public float recoilDuration = 0.8f;
        public float damageCoeff = 2.5f;
        public float radius = 36f;
        public Transform modelTransform;
        public Material overlayMat;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Fullbody, Override", "Discharge", "Generic.playbackRate", recoilDuration);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.SignalOverload.matShieldBreakCovenant,
                    _ => VFX.SignalOverload.matShieldBreakDefault
                };

                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 8f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(Buffs.ShieldSpeed.instance.BuffDef, 7f);
            }

            if (isAuthority)
            {
                HandleBlastAuthority();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= recoilDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public void HandleBlastAuthority()
        {
            SphereSearch search = new()
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = transform.position
            };
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(GetTeam()));

            Util.PlaySound("Play_loader_R_variant_slam", base.gameObject);

            var hurtBoxes = search.GetHurtBoxes();

            foreach (HurtBox box in hurtBoxes)
            {
                var boxHealthComponent = box.healthComponent;
                if (!boxHealthComponent)
                {
                    continue;
                }

                var enemyBody = boxHealthComponent.body;
                if (!enemyBody)
                {
                    continue;
                }

                VoltLightningOrb orb = new()
                {
                    attacker = gameObject,
                    damageValue = damageStat * damageCoeff,
                    bouncesRemaining = 0,
                    isCrit = RollCrit(),
                    lightningType = LightningOrb.LightningType.Loader,
                    origin = transform.position,
                    procCoefficient = 0f,
                    target = box,
                    teamIndex = GetTeam(),
                    damageType = DamageType.Shock5s,
                    damageColorIndex = DamageColorIndex.Default,
                    attackerBody = characterBody,
                    victimBody = enemyBody
                };

                VoltLightningOrb orb2 = new()
                {
                    attacker = gameObject,
                    damageValue = damageStat * damageCoeff,
                    isCrit = orb.isCrit,
                    origin = orb.origin,
                    procCoefficient = 0f,
                    target = box,
                    teamIndex = GetTeam(),
                    damageType = DamageType.Shock5s,
                    damageColorIndex = DamageColorIndex.Default,
                    attackerBody = characterBody,
                    victimBody = enemyBody
                };

                orb.damageType.damageSource = DamageSource.NoneSpecified;

                orb.AddModdedDamageType(Electrician.Grounding);

                OrbManager.instance.AddOrb(orb);
                OrbManager.instance.AddOrb(orb2);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}