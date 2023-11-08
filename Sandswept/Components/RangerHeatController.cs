using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;
using System.Diagnostics;
using Sandswept.Buffs;
using EntityStates.Bison;
using Sandswept.States.Ranger;

namespace Sandswept.Components
{
    public class RangerHeatManager : MonoBehaviour
    {
        public static float MaxHeat = 100f;
        public static float HeatDecayRate = 15f;
        public static float HeatSignatureHeatIncreaseRate = 60f;
        public static float HeatIncreaseRate = 35f;
        public static float OverheatThreshold = 35f;

        public float CurrentHeat = 0f;

        public bool IsOverheating => CurrentHeat > OverheatThreshold;
        public bool isFiring = false;
        public bool isUsingHeatSignature = false;
        internal Animator anim;
        private CharacterBody cb;
        private HealthComponent hc;
        public GameObject overlayPrefab;
        internal GameObject overlayInstance;
        internal float SelfDamage = 0.006f;
        internal float stopwatchSelfDamage = 0f;

        //
        internal bool isOverdrive = false;
        internal int overdriveChargeBuffer = 0;
        internal float chargeBufferStopwatch = 0f;
        internal static float chargeBufferDelay = 1f;
        internal float reduction => 1f - (Mathf.Clamp(0.15f * overdriveChargeBuffer, 0.1f, 0.9f));
        internal float stopwatchMaxHeat = 0f;
        internal float stunDelay = 1f;
        public bool isInStun = false;
        internal float stunStopwatch = 0f;

        internal EntityStateMachine esm;

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

        public void FixedUpdate() {
            if (isOverdrive) {
                CurrentHeat += (HeatIncreaseRate * Time.fixedDeltaTime) * reduction;
                CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat );

                chargeBufferStopwatch += Time.fixedDeltaTime;

                if (chargeBufferStopwatch >= chargeBufferDelay) {
                    overdriveChargeBuffer -= 1;
                    overdriveChargeBuffer = Mathf.Max(overdriveChargeBuffer, 0);
                    chargeBufferStopwatch = 0f;
                }

                if (CurrentHeat >= MaxHeat) {
                    stopwatchMaxHeat += Time.fixedDeltaTime;

                    if (stopwatchMaxHeat >= stunDelay) {
                        stopwatchMaxHeat = 0f;
                        ExitOverdrive();
                        stunStopwatch = 5f;
                        isInStun = true;
                        EffectManager.SpawnEffect(Assets.GameObject.ExplosionSolarFlare, new EffectData {
                            origin = base.transform.position,
                            scale = 0.5f
                        }, false);
                        AkSoundEngine.PostEvent(Events.Play_MULT_m2_secondary_explode, base.gameObject);
                    }
                }

                cb.SetBuffCount(Scorched.instance.BuffDef.buffIndex, Mathf.RoundToInt((CurrentHeat + 0.001f) / 10));
                cb.SetBuffCount(Charged.instance.BuffDef.buffIndex, overdriveChargeBuffer);
            }

            if (isInStun) {
                stunStopwatch -= Time.fixedDeltaTime;

                if (stunStopwatch <= 0f) {
                    isInStun = false;
                }
            }

            anim.SetFloat("combat", Mathf.Lerp(anim.GetFloat("combat"), cb.outOfCombat ? -1f : 1f, 3f * Time.fixedDeltaTime));
        }

        public void EnterOverdrive() {
            overdriveChargeBuffer = cb.GetBuffCount(Charged.instance.BuffDef);
            isOverdrive = true;
        }

        public void ExitOverdrive() {
            overdriveChargeBuffer = 0;
            isOverdrive = false;
            stopwatchMaxHeat = 0f;
            chargeBufferStopwatch = 0f;
            cb.SetBuffCount(Scorched.instance.BuffDef.buffIndex, 0);
            CurrentHeat = 0f;

            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            EntityStateMachine machine2 = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            machine2.SetState(new Idle());
        }

        public void ExitStun() {
            isInStun = false;
        }

        /*public void FixedUpdate()
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
                    damage = hc.fullCombinedHealth * (SelfDamage + (0.00006f * CurrentHeat)),
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
        }*/
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