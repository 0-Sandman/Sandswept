using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;

namespace Sandswept.Components
{
    public class RangerHeatManager : MonoBehaviour
    {
        public static float MaxHeat = 200f;
        public static float HeatDecayRate = 15f;
        public static float HeatSignatureHeatIncreaseRate = 40f;
        public static float HeatIncreaseRate = 25f;
        public static float OverheatThreshold = 100f;

        public float CurrentHeat = 0f;

        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;
        public bool isUsingHeatSignature = false;
        internal Animator anim;
        private CharacterBody cb;
        private HealthComponent hc;
        public GameObject overlayPrefab;
        internal GameObject overlayInstance;
        internal float SelfDamage = 0.0066f;
        internal float stopwatchSelfDamage = 0f;

        public void Start()
        {
            cb = GetComponent<CharacterBody>();
            hc = cb.healthComponent;
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
            if (IsOverheating && stopwatchSelfDamage >= 0.25f)
            {
                stopwatchSelfDamage = 0f;
                DamageInfo info = new()
                {
                    attacker = null,
                    procCoefficient = 0,
                    damage = hc.fullCombinedHealth * (SelfDamage + (0.000065f * CurrentHeat)),
                    crit = false,
                    position = transform.position,
                    damageColorIndex = DamageColorIndex.Fragile,
                    damageType = DamageType.BypassArmor | DamageType.BypassBlock
                };

                if (NetworkServer.active)
                {
                    cb.healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, gameObject);
            }

            // KillYourself(); no keep yourself safe :3 :3
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

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(0.6f, 0.6f, 1f);

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
                image.color = new Color32(255, (byte)Mathf.Lerp(200, 70, heatPercent), 0, (byte)Mathf.Lerp(0, 200, heatPercentScaled));
                backdropImage.color = new Color32(53, 53, 53, (byte)Mathf.Lerp(0, 200, heatPercentScaled));
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