using System.Threading;
using System.Linq;
using System;
using RoR2.UI;
using TMPro;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine.UI;
using Sandswept2.States.Ranger;

namespace Sandswept2.Components
{
    public class RangerHeatManager : MonoBehaviour
    {
        public static float MaxHeat = 200f;
        public static float HeatDecayRate = 25f;
        public static float HeatSignatureHeatIncreaseRate = 40f;
        public static float HeatIncreaseRate = 30f;
        public static float OverheatThreshold = 100f;

        public float CurrentHeat = 0f;

        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;
        public bool isUsingHeatSignature = false;
        internal Animator anim;
        private CharacterBody cb;
        public GameObject overlayPrefab;
        internal GameObject overlayInstance;
        internal float SelfDamage = 0.04f;
        internal float stopwatchSelfDamage = 0f;

        public void Start()
        {
            cb = GetComponent<CharacterBody>();
            anim = GetComponent<CharacterDirection>().modelAnimator;
            OverlayCreationParams p = new();
            p.prefab = overlayPrefab;
            p.childLocatorEntry = "CrosshairExtras";

            OverlayController controller = HudOverlayManager.AddOverlay(base.gameObject, p);
            controller.onInstanceAdded += (c, i) =>
            {
                overlayInstance = i;
                overlayInstance.GetComponent<RangerCrosshairManager>().target = this;
            };
        }

        public void FixedUpdate()
        {
            if (isFiring && CurrentHeat < MaxHeat)
            {
                CurrentHeat += HeatIncreaseRate * Time.fixedDeltaTime;
            }

            if (isUsingHeatSignature && CurrentHeat < MaxHeat)
            {
                CurrentHeat += HeatSignatureHeatIncreaseRate * Time.fixedDeltaTime;
            }

            if (!isFiring && CurrentHeat > 0)
            {
                CurrentHeat -= HeatDecayRate * Time.fixedDeltaTime;
            }

            CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat);

            stopwatchSelfDamage += Time.fixedDeltaTime;
            if (IsOverheating && stopwatchSelfDamage >= 0.2f)
            {
                stopwatchSelfDamage = 0f;
                DamageInfo info = new()
                {
                    attacker = null,
                    procCoefficient = 0,
                    damage = (cb.damage * (SelfDamage + (0.0006f * CurrentHeat))),
                    crit = false,
                    position = transform.position,
                    damageColorIndex = DamageColorIndex.Fragile,
                    damageType = DamageType.BypassArmor | DamageType.BypassBlock | DamageType.Silent
                };

                if (NetworkServer.active)
                {
                    cb.healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, gameObject);
            }

            KillYourself();
        }

        public void KillYourself()
        {
            anim.SetBool("isFiring", cb.inputBank.skill1.down);
        }
    }

    public class RangerCrosshairManager : MonoBehaviour
    {
        public ImageFillController ifc;
        public RangerHeatManager target;
        public HudElement element;
        public Image image;
        public Image backdropImage;
        public RawImage actualCrosshair;
        public Animator animator;
        public Transform heatMeterBackdrop;
        public Transform heatMeter;
        public static RuntimeAnimatorController runtimeAnimatorController;
        public float colorUpdateInterval = 0.05f;
        public float colorUpdateTimer;

        public void Start()
        {
            actualCrosshair = GetComponent<RawImage>();
            actualCrosshair.enabled = false;

            heatMeter = transform.GetChild(0).GetChild(1);
            heatMeterBackdrop = transform.GetChild(0).GetChild(0);

            image = heatMeter.GetComponent<Image>();
            image.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatMeter.png");

            backdropImage = heatMeterBackdrop.GetComponent<Image>();
            backdropImage.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatMeterOutline.png");

            element = GetComponent<HudElement>();
            ifc = GetComponentInChildren<ImageFillController>();
        }

        public void FixedUpdate()
        {
            if (!target)
            {
                return;
            }

            var heatPercent = target.CurrentHeat / RangerHeatManager.MaxHeat;

            colorUpdateTimer += Time.fixedDeltaTime;
            if (colorUpdateTimer >= colorUpdateInterval)
            {
                var heatPercentScaled = Mathf.Clamp01(target.CurrentHeat * 3f / RangerHeatManager.MaxHeat);
                image.color = new Color32(255, (byte)Mathf.Lerp(200, 70, heatPercent), 0, (byte)Mathf.Lerp(0, 255, heatPercentScaled));
                backdropImage.color = new Color32(53, 53, 53, (byte)Mathf.Lerp(0, 255, heatPercentScaled));
                colorUpdateTimer = 0f;
            }

            ifc.SetTValue(heatPercent);
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