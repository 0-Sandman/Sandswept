using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;
using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.States;

namespace Sandswept.Survivors.Ranger
{
    public class RangerHeatController : MonoBehaviour
    {
        public bool isFiring = false;
        public Animator anim;
        private CharacterBody cb;
        private HealthComponent hc;

        public GameObject overlayPrefab;
        public GameObject overlayInstance;

        public bool isInOverdrive = false;

        public static float maxHeat = 100f;

        public float heatGainRate = 13f;

        public float currentHeat = 0f;

        public float fullHeatTimer = 0f;
        public bool isInFullHeat = false;

        public float selfDamage = 0.006f;
        public float selfDamageInterval = 0.2f;
        public float selfDamageTimer;

        public float chargeLossTimer = 0f;
        public float chargeLossInterval = 6f;

        public int lastHealingReductionCount;

        internal EntityStateMachine esm;

        public void Start()
        {
            overlayPrefab = Main.Assets.LoadAsset<GameObject>("Assets/Sandswept/Base/Characters/Ranger/OverlayRanger.prefab");
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
            var buffCount = cb.GetBuffCount(Buffs.Charge.instance.BuffDef);

            if (buffCount > 0)
                chargeLossTimer += Time.fixedDeltaTime;

            if (chargeLossTimer >= chargeLossInterval)
            {
                cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - 1));

                chargeLossTimer = 0f;
            }

            if (isInOverdrive)
            {
                currentHeat += heatGainRate * Time.fixedDeltaTime;
                currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);

                if (currentHeat >= maxHeat)
                {
                    fullHeatTimer += Time.fixedDeltaTime;
                    selfDamageTimer += Time.fixedDeltaTime;

                    if (selfDamageTimer >= selfDamageInterval && fullHeatTimer > 1f)
                    {
                        TakeDamage(fullHeatTimer * 0.4f);
                        selfDamageTimer = 0f;
                    }
                }

                var reductionCount = cb.GetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex);

                if (lastHealingReductionCount > reductionCount)
                    lastHealingReductionCount = reductionCount;
                cb.SetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex, Mathf.RoundToInt((currentHeat + 0.001f) / 10));
            }

            anim.SetFloat("combat", Mathf.Lerp(anim.GetFloat("combat"), cb.outOfCombat ? -1f : 1f, 3f * Time.fixedDeltaTime));
        }

        public void EnterOverdrive()
        {
            isInOverdrive = true;
        }

        public void ExitOverdrive()
        {
            isInOverdrive = false;
            fullHeatTimer = 0f;
            chargeLossTimer = 0f;
            cb.SetBuffCount(OverheatDamageBoost.instance.BuffDef.buffIndex, 0);
            cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, 0);

            currentHeat = 0f;
            Invoke(nameof(RemoveHealingReduction), 2f);

            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            EntityStateMachine machine2 = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            if (machine2)
                machine2.SetState(new Idle());
        }

        public void RemoveHealingReduction()
        {
            cb.SetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex, 0);
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

            var guh = 1f + (2f / 3f);

            var toAddFromCharge = Convert.ToInt32(cb.GetBuffCount(Charge.instance.BuffDef) / guh); // 6 at max charge

            var toAdd = 6 + toAddFromCharge;

            if (NetworkServer.active)
            {
                for (int i = 0; i < toAdd; i++)
                    cb.AddBuff(OverheatDamageBoost.instance.BuffDef);

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

            var heatPercent = target.currentHeat / RangerHeatController.maxHeat;

            colorUpdateTimer += Time.fixedDeltaTime;
            if (colorUpdateTimer >= colorUpdateInterval)
            {
                var heatPercentScaled = Mathf.Clamp01(target.currentHeat * 3f / RangerHeatController.maxHeat);
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