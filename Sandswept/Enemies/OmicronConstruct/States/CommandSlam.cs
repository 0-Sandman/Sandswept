using System;
using RoR2.CharacterAI;

namespace Sandswept.Enemies.OmicronConstruct.States {
    public class CommandSlam : BaseSkillState {
        public override void OnEnter()
        {
            base.OnEnter();

            BaseAI ai = base.characterBody.master.GetComponent<BaseAI>();

            if (!ai || !ai.currentEnemy.gameObject || !base.isAuthority) {
                outer.SetNextStateToMain();
                return;
            }

            GetComponent<OmicronController>().CommandSlam(ai.currentEnemy.gameObject);

            outer.SetNextStateToMain();
        }
    }
}