using System;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician.States
{
    public class SignalOverloadCharge : BaseSkillState
    {
        public float baseDuration = 1.2f;

        public override void OnEnter()
        {
            base.OnEnter();

            baseDuration /= base.attackSpeedStat;

            FindModelChild("Tethers").gameObject.SetActive(true);

            PlayAnimation("Gesture, Override", "StartOverload", "Generic.playbackRate", baseDuration * 2f);

            // base.characterMotor.walkSpeedPenaltyCoefficient = 0.1f;
            Util.PlaySound("Play_ui_obj_nullWard_charge_loop", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
            // base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            Util.PlaySound("Stop_ui_obj_nullWard_charge_loop", gameObject);

            FindModelChild("Tethers").gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= baseDuration)
            {
                outer.SetNextState(new SignalOverloadDischarge());
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
        public Animator animator;
        public GameObject effect;
        public LineRenderer lr;
        public GameObject beamEffect;
        public Transform head;
        public Vector3 pos;
        public Transform origin;
        public Transform end;
        public CameraTargetParams.CameraParamsOverrideHandle handle;
        public GameObject signalIndicator;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "OverloadLoop", "Generic.playbackRate", 0.5f);
            animator = GetModelAnimator();
            animator.SetBool("discharging", true);

            effect = Paths.GameObject.LoaderGroundSlam;

            Util.PlaySound("Play_roboBall_attack2_mini_active_loop", gameObject);
            Util.PlaySound("Play_ui_obj_nullWard_charge_loop", gameObject);
            Util.PlaySound("Play_captain_m1_shotgun_charge_loop", gameObject);

            head = FindModelChild("Head");

            FindModelChild("Tethers").gameObject.SetActive(true);

            beamEffect = GameObject.Instantiate(Main.Assets.LoadAsset<GameObject>("ElectricianChargeBeam.prefab"), head.position, head.rotation);
            origin = beamEffect.GetComponent<ChildLocator>().FindChild("Start");
            end = beamEffect.GetComponent<ChildLocator>().FindChild("End");

            lr = beamEffect.GetComponent<LineRenderer>();

            handle = base.cameraTargetParams.AddParamsOverride(new() {
                cameraParamsData = Paths.CharacterCameraParams.ccpToolbot.data
            }, 0.3f);

            signalIndicator = GameObject.Instantiate(Electrician.SignalOverloadIndicator, new Vector3(0, -4000, 0), Quaternion.identity);
            signalIndicator.transform.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
        }

        public override void Update() {
            beamEffect.transform.position = head.transform.position;
            origin.transform.position = head.transform.position;
            end.transform.position = pos;
            signalIndicator.transform.position = pos;

            characterBody.SetSpreadBloom(12f, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Util.CharacterRaycast(base.gameObject, base.GetAimRay(), out RaycastHit info, 300f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore)) {
                pos = info.point;
            }
            else {
                pos = base.GetAimRay().GetPoint(100f);
            }

            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                stopwatch = delay;
                Util.PlaySound("Play_item_proc_armorReduction_hit", gameObject);
                Util.PlaySound("Play_mage_m1_cast_lightning", gameObject);

                HandleBlastAuthority(pos);
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            
            animator.SetBool("discharging", false);
            Util.PlaySound("Stop_roboBall_attack2_mini_active_loop", gameObject);
            Util.PlaySound("Stop_ui_obj_nullWard_charge_loop", gameObject);
            Util.PlaySound("Stop_captain_m1_shotgun_charge_loop", gameObject);

            if (beamEffect) {
                Destroy(beamEffect);
            }

            if (signalIndicator) {
                Destroy(signalIndicator);
            }

            FindModelChild("Tethers").gameObject.SetActive(false);

            cameraTargetParams.RemoveParamsOverride(handle, 0.3f);

            base.skillLocator.special.DeductStock(1);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public void HandleBlastAuthority(Vector3 position)
        {
            SphereSearch search = new();
            search.radius = radius;
            search.mask = LayerIndex.entityPrecise.mask;
            search.origin = position;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(base.GetTeam()));

            BlastAttack attack = new();
            attack.radius = radius * 0.10f;
            attack.baseDamage = base.damageStat * coeff * 1.8f;
            attack.damageType = DamageType.Stun1s | DamageType.Shock5s;
            attack.crit = base.RollCrit();
            attack.attacker = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.falloffModel = BlastAttack.FalloffModel.None;
            attack.procCoefficient = 0.8f;
            attack.position = position;

            attack.Fire();

            EffectManager.SpawnEffect(effect, new EffectData {
                origin = attack.position,
                scale = attack.radius * 2f
            }, true);

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                if (Vector3.Distance(box.transform.position, attack.position) < attack.radius * 1.5f) {
                    continue;
                }

                LightningOrb orb = new();
                orb.attacker = base.gameObject;
                orb.damageValue = base.damageStat * coeff;
                orb.bouncesRemaining = 0;
                orb.isCrit = base.RollCrit();
                orb.lightningType = LightningOrb.LightningType.Loader;
                orb.origin = position;
                orb.procCoefficient = 1f;
                orb.target = box;
                orb.teamIndex = base.GetTeam();
                orb.AddModdedDamageType(Electrician.Grounding);

                if (box.healthComponent)
                {
                    CharacterMotor motor = box.healthComponent.GetComponent<CharacterMotor>();
                    RigidbodyMotor motor2 = box.healthComponent.GetComponent<RigidbodyMotor>();

                    if (motor)
                    {
                        motor.Motor.ForceUnground();
                        motor.velocity += (position - motor.transform.position).normalized * ((14.5f) * delay);
                    }

                    if (motor2)
                    {
                        PhysForceInfo info = new();
                        info.massIsOne = true;
                        info.force = (position - motor2.transform.position).normalized * (2.5f * delay);
                        motor2.ApplyForceImpulse(in info);
                    }
                }

                OrbManager.instance.AddOrb(orb);
            }
        }
    }

    public class SignalOverloadFire : BaseSkillState
    {
        public float recoilDuration = 0.8f;
        public float effectMultiplier = 1f;
        private float damageCoeff = 8f;
        private float radius = 50f;

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

            if (NetworkServer.active) {
                base.characterBody.AddTimedBuff(Buffs.ShieldSpeed.instance.BuffDef, 5f);
            }

            if (base.isAuthority)
            {
                HandleBlastAuthority();
            }

            EffectManager.SpawnEffect(Electrician.staticSnareImpactVFX, new EffectData
            {
                origin = base.transform.position,
                scale = radius * 2f
            }, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= recoilDuration)
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
            SphereSearch search = new();
            search.radius = radius;
            search.mask = LayerIndex.entityPrecise.mask;
            search.origin = base.transform.position;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(base.GetTeam()));

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                LightningOrb orb = new();
                orb.attacker = base.gameObject;
                orb.damageValue = base.damageStat * damageCoeff;
                orb.bouncesRemaining = 0;
                orb.isCrit = base.RollCrit();
                orb.lightningType = LightningOrb.LightningType.Loader;
                orb.origin = base.transform.position;
                orb.procCoefficient = 0f;
                orb.target = box;
                orb.teamIndex = base.GetTeam();
                orb.damageType = DamageType.Shock5s;
                orb.AddModdedDamageType(Electrician.Grounding);

                OrbManager.instance.AddOrb(orb);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}