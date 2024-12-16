using System;
using RoR2.CharacterAI;
using Sandswept.Survivors;

namespace Sandswept.Enemies.ThetaConstruct {
    public class CastShield : BaseSkillState {
        public GameObject shieldInstance;
        public float duration = 15f;
        public GameObject effect;
        public override void OnEnter()
        {
            base.OnEnter();

            GameObject buddy = base.characterBody.master.GetComponent<BaseAI>().buddy._gameObject;

            if (!buddy) {
                outer.SetNextStateToMain();
                return;
            }

            if (base.isAuthority) {
                shieldInstance = GameObject.Instantiate(ThetaConstruct.ThetaShieldEffect, buddy.transform.position, Quaternion.identity);
                ThetaShieldController shieldController = shieldInstance.GetComponent<ThetaShieldController>();
                shieldController.targetHolder.ownerObject = buddy;
                shieldController.ownerHolder.ownerObject = base.gameObject;

                base.characterBody.master.GetComponent<BaseAI>().leader.gameObject = buddy;

                NetworkServer.Spawn(shieldInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!base.isAuthority) return;

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

            if (base.isAuthority && shieldInstance) {
                NetworkServer.DestroyObject(shieldInstance);
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