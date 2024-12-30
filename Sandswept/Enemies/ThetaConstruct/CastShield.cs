using System;
using RoR2.CharacterAI;
using Sandswept.Survivors;

namespace Sandswept.Enemies.ThetaConstruct
{
    public class CastShield : BaseSkillState
    {
        public GameObject shieldInstance;
        public float duration = 15f;
        public GameObject effect;
        public bool doYuriBlast = false;
        private Vector3 yuriDir;
        private GameObject buddy;
        public override void OnEnter()
        {
            base.OnEnter();

            buddy = base.characterBody.master.GetComponent<BaseAI>().buddy._gameObject;

            if (!buddy)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (buddy.GetComponent<CharacterBody>() && buddy.GetComponent<CharacterBody>().bodyIndex == ThetaConstruct.ThetaIndex) {
                doYuriBlast = true;
                yuriDir = (buddy.transform.position - base.transform.position).normalized * -1f;

                EffectManager.SpawnEffect(Paths.GameObject.MajorConstructSpawnEffect, new EffectData {
                    origin = base.transform.position,
                    scale = base.characterBody.bestFitRadius * 2f
                }, false);

                AkSoundEngine.PostEvent(Events.Play_majorConstruct_spawn_rumble, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_vagrant_R_explode, base.gameObject);
            }

            if (base.isAuthority) {
                shieldInstance = GameObject.Instantiate(ThetaConstruct.ThetaShieldEffect, buddy.transform.position, Quaternion.identity);
                ThetaShieldController shieldController = shieldInstance.GetComponent<ThetaShieldController>();
                shieldController.targetHolder.ownerObject = buddy;
                shieldController.ownerHolder.ownerObject = base.gameObject;

                buddy.gameObject.GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.Immune, 15f);

                base.characterBody.master.GetComponent<BaseAI>().leader.gameObject = buddy;

                NetworkServer.Spawn(shieldInstance);
            }
        }

        public void DoYuriBlast() {
            doYuriBlast = false;

            EffectManager.SpawnEffect(Paths.GameObject.ExplosionMinorConstruct, new EffectData {
                    origin = base.transform.position,
                    scale = base.characterBody.bestFitRadius * 3.5f
            }, true);
            
            PhysForceInfo info = default;
            info.force = yuriDir * 90f;
            info.massIsOne = true;

            base.rigidbodyMotor.ApplyForceImpulse(in info);

            AkSoundEngine.PostEvent(Events.Play_majorConstruct_death, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_majorConstruct_R_pulse, base.gameObject);

            EffectManager.SpawnEffect(Paths.GameObject.ExplosionMinorConstruct, new EffectData {
                origin = shieldInstance.transform.position,
                scale = base.characterBody.bestFitRadius * 3.5f
            }, true);

            outer.SetNextStateToMain();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (doYuriBlast) {
                base.rigidbody.velocity = Vector3.zero;
            }

            if (!base.isAuthority) return;

            if (base.fixedAge >= 2.5f && doYuriBlast) {
                DoYuriBlast();
            }

            if (base.fixedAge >= duration || !shieldInstance) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.isAuthority && shieldInstance)
            {
                NetworkServer.DestroyObject(shieldInstance);
            }

            if (base.isAuthority && buddy) {
                buddy.gameObject.GetComponent<CharacterBody>().RemoveBuff(RoR2Content.Buffs.Immune);
            }
        }
    }

    public class CastShieldSkill : SkillBase<CastShieldSkill>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(CastShield);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 10f;

        public override Sprite Icon => null;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}