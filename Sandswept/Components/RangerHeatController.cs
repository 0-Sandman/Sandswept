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
        public static float HeatDecayRate = 25f;
        public static float HeatIncreaseRate = 30f;
        public static float OverheatThreshold = 100f;

        public float CurrentHeat = 0f;

        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;

        public void FixedUpdate()
        {
            if (isFiring && CurrentHeat < MaxHeat)
            {
                CurrentHeat += HeatIncreaseRate * Time.fixedDeltaTime;
            }
            else if (!isFiring && CurrentHeat > 0)
            {
                CurrentHeat -= HeatDecayRate * Time.fixedDeltaTime;
            }

            CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat);
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

    public class FUCKINGKILLYOURSELF : MonoBehaviour
    {
        public void FixedUpdate()
        {
            if (transform.localPosition.y != -1)
            {
                transform.transform.localPosition = new(transform.localPosition.x, -1, transform.localPosition.z);
            }
        }
    }
}