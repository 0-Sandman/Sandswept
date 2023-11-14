using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;
using System.Diagnostics;
using Sandswept.Buffs;
using EntityStates.Bison;
using Sandswept.Survivors.Ranger.States;

namespace Sandswept.Survivors.Ranger
{
    public class RangerHeatController : MonoBehaviour
    {
        public static float MaxHeat = 100f;
        public static float HeatDecayRate = 15f;
        public static float HeatSignatureHeatIncreaseRate = 60f;

        // public static float HeatIncreaseRate = 20f;
        public static float HeatIncreaseRate = 13f;

        public static float OverheatThreshold = 35f;

        public float CurrentHeat = 0f;

        public bool isFiring = false;
        internal Animator anim;
        private CharacterBody cb;
        private HealthComponent hc;
        public GameObject overlayPrefab = Main.Assets.LoadAsset<GameObject>("OverlayPrefab.prefab");
        internal GameObject overlayInstance;
        internal float SelfDamage = 0.006f;
        internal float stopwatchSelfDamage = 0f;

        //
        internal bool isOverdrive = false;

        internal int overdriveChargeBuffer = 0;
        internal float chargeLossTimer = 0f;
        internal static float chargeLossInterval = 3f;

        // internal float reduction => 1f - (Mathf.Clamp(0.15f * overdriveChargeBuffer, 0.15f, 0.6f));
        internal float stopwatchMaxHeat = 0f;

        public bool isOverheating = false;
        public float damageInterval = 0.2f;
        public float damageTimer;

        internal EntityStateMachine esm;

        public void Start()
        {
            cb = GetComponent<CharacterBody>();
            hc = cb.healthComponent;
            anim = GetComponent<CharacterDirection>().modelAnimator;
            OverlayCreationParams p = new();
            p.prefab = overlayPrefab;
            p.childLocatorEntry = "CrosshairExtras";

            OverlayController controller = HudOverlayManager.AddOverlay(gameObject, p);
            controller.onInstanceAdded += (c, i) =>
            {
                overlayInstance = i;
                overlayInstance.GetComponent<RangerCrosshairManager>().target = this;
            };
        }

        public void FixedUpdate()
        {
            chargeLossTimer += Time.fixedDeltaTime;

            if (chargeLossTimer >= chargeLossInterval)
            {
                var buffCount = cb.GetBuffCount(Buffs.Charge.instance.BuffDef);

                cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - 1));

                chargeLossTimer = 0f;
            }

            if (isOverdrive)
            {
                CurrentHeat += HeatIncreaseRate * Time.fixedDeltaTime;
                CurrentHeat = Mathf.Clamp(CurrentHeat, 0, MaxHeat);

                if (CurrentHeat >= MaxHeat)
                {
                    stopwatchMaxHeat += Time.fixedDeltaTime;
                    damageTimer += Time.fixedDeltaTime;

                    if (damageTimer >= damageInterval && stopwatchMaxHeat > 1f)
                    {
                        TakeDamage(stopwatchMaxHeat * 0.4f);
                        damageTimer = 0f;
                    }
                }

                cb.SetBuffCount(Scorched.instance.BuffDef.buffIndex, Mathf.RoundToInt((CurrentHeat + 0.001f) / 10));
            }

            anim.SetFloat("combat", Mathf.Lerp(anim.GetFloat("combat"), cb.outOfCombat ? -1f : 1f, 3f * Time.fixedDeltaTime));
        }

        public void EnterOverdrive()
        {
            overdriveChargeBuffer = cb.GetBuffCount(Buffs.Charge.instance.BuffDef);
            isOverdrive = true;
        }

        public void ExitOverdrive()
        {
            overdriveChargeBuffer = 0;
            isOverdrive = false;
            stopwatchMaxHeat = 0f;
            chargeLossTimer = 0f;
            cb.SetBuffCount(Scorched.instance.BuffDef.buffIndex, 0);
            cb.SetBuffCount(OverheatingDamageBoost.instance.BuffDef.buffIndex, 0);
            cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, 0);

            CurrentHeat = 0f;

            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            EntityStateMachine machine2 = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            if (machine2)
                machine2.SetState(new Idle());
        }

        public void TakeDamage(float timeInOverheat)
        {
            DamageInfo info = new()
            {
                attacker = null,
                procCoefficient = 0,
                damage = hc.fullCombinedHealth * 0.013f * timeInOverheat,
                crit = false,
                position = transform.position,
                damageColorIndex = DamageColorIndex.Fragile,
                damageType = DamageType.BypassArmor | DamageType.BypassBlock
            };

            info.AddModdedDamageType(Main.HeatSelfDamage);

            if (NetworkServer.active)
            {
                cb.AddBuff(OverheatingDamageBoost.instance.BuffDef);
                cb.healthComponent.TakeDamage(info);
            }

            AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, gameObject);
        }
    }

    public class RangerCrosshairManager : MonoBehaviour
    {
        public ImageFillController ifc;
        public RangerHeatController target;
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

            var heatPercent = target.CurrentHeat / RangerHeatController.MaxHeat;

            colorUpdateTimer += Time.fixedDeltaTime;
            if (colorUpdateTimer >= colorUpdateInterval)
            {
                var heatPercentScaled = Mathf.Clamp01(target.CurrentHeat * 3f / RangerHeatController.MaxHeat);
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