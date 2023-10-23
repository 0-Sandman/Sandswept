using System.Threading;
using System.Linq;
using System;
using RoR2.UI;
using TMPro;

namespace Sandswept.Components
{
    public class RangerHeatManager : MonoBehaviour
    {
        public static float MaxHeat = 200f;
        public static float HeatDecayRate = 20f;
        public static float OverheatThreshold = 100f;

        public float CurrentHeat = 0f;

        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;
        internal Animator anim;
        CharacterBody cb;

        public void Start()
        {
            cb = GetComponent<CharacterBody>();
            anim = GetComponent<CharacterDirection>().modelAnimator;
        }

        public void FixedUpdate()
        {
            if (isFiring && CurrentHeat < MaxHeat)
            {
                CurrentHeat += HeatDecayRate * Time.fixedDeltaTime;
            }
            else if (!isFiring && CurrentHeat > 0)
            {
                CurrentHeat -= HeatDecayRate * Time.fixedDeltaTime;
            }

            CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat);

            KillYourself();
        }

        public void KillYourself() {
            anim.SetBool("isFiring", cb.inputBank.skill1.down);
        }
    }

    public class RangerCrosshairManager : MonoBehaviour
    {
        public ImageFillController ifc;
        public RangerHeatManager target;
        public HudElement element;

        public void Start()
        {
            element = GetComponent<HudElement>();
            ifc = GetComponentInChildren<ImageFillController>();
            target = element._targetBodyObject.GetComponent<RangerHeatManager>();
        }

        public void FixedUpdate()
        {
            ifc.SetTValue(target.CurrentHeat / RangerHeatManager.MaxHeat);
        }
    }

    public class FUCKINGKILLYOURSELF : MonoBehaviour {
        public void FixedUpdate() {
            if (base.transform.localPosition.y != -1) {
                base.transform.transform.localPosition = new(base.transform.localPosition.x, -1, base.transform.localPosition.z);
            }
        }
    }
}