using System.Linq;
using System;
using RoR2.UI;
using TMPro;

namespace Sandswept.Components {
    public class RangerHeatManager : MonoBehaviour {
        public static float MaxHeat = 200f;
        public static float HeatDecayRate = 20f;
        public static float OverheatThreshold = 100f;

        public float CurrentHeat = 0f;
        
        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;

        public void Start() {
            CharacterBody cb = GetComponent<CharacterBody>();
        }

        public void FixedUpdate() {
            if (isFiring && CurrentHeat < MaxHeat) {
                CurrentHeat += HeatDecayRate * Time.fixedDeltaTime;
            }
            else if (!isFiring && CurrentHeat > 0) {
                CurrentHeat -= HeatDecayRate * Time.fixedDeltaTime;
            }

            CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat);
        }
    }

    public class RangerCrosshairManager : MonoBehaviour {
        public ImageFillController ifc;
        public RangerHeatManager target;
        public HudElement element;

        public void Start() {
            target = element._targetBodyObject.GetComponent<RangerHeatManager>();
        }

        public void FixedUpdate() {
            ifc.SetTValue(target.CurrentHeat / RangerHeatManager.MaxHeat);
        }
    }
}