using System;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician.States
{
    public class SignalOverloadCharge : BaseSkillState
    {
        public float baseDuration = 1.8f;
        public float shieldDrained = 0f;
        public float stopwatch = 0f;
        public float delay = 0.7f / 10;
        public float drainAmount;
        public float baseMax = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();

            baseDuration /= attackSpeedStat;

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
                if (base.healthComponent.shield > 0)
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

                            healthComponent.TakeDamage(damageInfo);
                        }
                    }
                }

                outer.SetNextState(new SignalOverloadDischarge(Util.Remap(shieldDrained, 0f, healthComponent.fullHealth * baseMax, 0.6f, 1f)));
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

                        healthComponent.TakeDamage(damageInfo);

                    }
                }
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
        public float totalDamageCoef = 16f;
        public int totalHits = 10;
        public float delay => duration / totalHits;
        public float coeff => totalDamageCoef / totalHits;
        public float radius = 30f;
        public float stopwatch = 0f;
        public float multiplier = 1f;
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

        public SignalOverloadDischarge(float mult)
        {
            multiplier = Mathf.Min(mult, 1.5f);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            totalDamageCoef *= multiplier;
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
                    "SKIN_ELEC_MASTERY" => VFX.SignalOverload.indicatorCovenant,
                    _ => VFX.SignalOverload.indicatorDefault
                };

                effect = skinNameToken switch
                {
                    "SKIN_ELEC_MASTERY" => VFX.SignalOverload.impactCovenant,
                    _ => VFX.SignalOverload.impactDefault
                };

                beamVFX = skinNameToken switch
                {
                    "SKIN_ELEC_MASTERY" => VFX.SignalOverload.beamCovenant,
                    _ => VFX.SignalOverload.beamDefault
                };

            }

            signalIndicatorInstance = Object.Instantiate(signalIndicator, new Vector3(0, -4000, 0), Quaternion.identity);
            signalIndicatorInstance.transform.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);

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
                radius = radius * 0.10f,
                baseDamage = damageStat * coeff * 1.8f,
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
                    procCoefficient = 1f - Util.Remap(Vector3.Distance(base.transform.position, box.transform.position), 0f, 60f, 0f, 0.8f),
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

    public class SignalOverloadFire : BaseSkillState
    {
        public float recoilDuration = 0.8f;
        public float effectMultiplier = 1f;
        public float damageCoeff = 2.8f;
        public float radius = 50f;
        public Transform modelTransform;
        public Material overlayMat;

        public SignalOverloadFire(float modifier)
        {
            // effectMultiplier = modifier;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            recoilDuration *= effectMultiplier;
            damageCoeff *= effectMultiplier;
            radius *= effectMultiplier;

            PlayAnimation("Fullbody, Override", "Discharge", "Generic.playbackRate", recoilDuration);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat = skinNameToken switch
                {
                    "SKIN_ELEC_MASTERY" => VFX.SignalOverload.matShieldBreakCovenant,
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