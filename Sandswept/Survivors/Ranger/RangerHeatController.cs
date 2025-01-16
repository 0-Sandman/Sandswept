using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;
using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.States.Special;

namespace Sandswept.Survivors.Ranger
{
    public class RangerHeatController : MonoBehaviour
    {
        public Animator anim;
        public CharacterBody cb;
        private HealthComponent hc;

        public GameObject overlayPrefab;
        public GameObject overlayInstance;

        public bool isInOverdrive = false;

        public static float maxHeat = 100f;

        public float heatGainRate = 10f;

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
                heatGainRate += Time.fixedDeltaTime;
                currentHeat += heatGainRate * Time.fixedDeltaTime;
                currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);
                if (currentHeat >= maxHeat)
                {
                    fullHeatTimer += Time.fixedDeltaTime;
                    selfDamageTimer += Time.fixedDeltaTime;

                    if (fullHeatTimer >= 1f)
                    {
                        cb.SetBuffCount(OverheatDamageBoost.instance.BuffDef.buffIndex, (10 + cb.GetBuffCount(Charge.instance.BuffDef)) * (int)fullHeatTimer);

                        if (selfDamageTimer >= selfDamageInterval)
                        {
                            TakeDamage(fullHeatTimer * 0.4f);
                            selfDamageTimer = 0f;
                        }
                    }
                }
                else
                {
                    fullHeatTimer = 0f;
                    cb.SetBuffCount(OverheatDamageBoost.instance.BuffDef.buffIndex, 0);
                }

                var reductionCount = cb.GetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex);

                if (lastHealingReductionCount > reductionCount)
                {
                    lastHealingReductionCount = reductionCount;
                }

                cb.SetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex, Mathf.RoundToInt((currentHeat + 0.001f) / 10));
            }

            // anim.SetFloat("combat", Mathf.Lerp(anim.GetFloat("combat"), cb.outOfCombat ? -1f : 1f, 3f * Time.fixedDeltaTime));
            anim.SetFloat("combat", 1);

            // Debug.Log(anim.GetFloat("aimYawCycle"));
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
                damageType = DamageType.BypassArmor | DamageType.BypassBlock,
                force = Vector3.zero
            };

            info.AddModdedDamageType(Main.HeatSelfDamage);

            if (NetworkServer.active)
            {
                cb.healthComponent.TakeDamage(info);
            }

            Util.PlaySound("Play_item_proc_igniteOnKill", gameObject);
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
        public CharacterBody body;
        public Color32 lowHeatColor;
        public Color32 inHeatColor;

        public void Start()
        {
            actualCrosshair = GetComponent<RawImage>();
            actualCrosshair.enabled = false;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(0.6f, 0.6f, 1f);

            heatMeter = transform.GetChild(0).GetChild(1);
            heatMeterBackdrop = transform.GetChild(0).GetChild(0);

            image = heatMeter.GetComponent<Image>();
            image.sprite = Main.hifuSandswept.LoadAsset<Sprite>("texHeatMeter.png");

            backdropImage = heatMeterBackdrop.GetComponent<Image>();
            backdropImage.sprite = Main.hifuSandswept.LoadAsset<Sprite>("texHeatMeterOutline.png");

            element = GetComponent<HudElement>();
            ifc = GetComponentInChildren<ImageFillController>();

            body = target.cb;

            Transform modelTransform = null;
            if (body.modelLocator)
            {
                modelTransform = body.modelLocator.modelTransform;
            }

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[body.skinIndex].nameToken;

                lowHeatColor = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => new Color32(0, 130, 224, 140),
                    "SKINDEF_RENEGADE" => new Color32(244, 95, 197, 140),
                    "SKINDEF_MILEZERO" => new Color32(153, 0, 23, 140),
                    "SKINDEF_SANDSWEPT" => new Color32(214, 159, 79, 140),
                    _ => new Color32(255, 200, 0, 140),
                };
                inHeatColor = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => new Color32(60, 0, 244, 180),
                    "SKINDEF_RENEGADE" => new Color32(114, 39, 244, 180),
                    "SKINDEF_MILEZERO" => new Color32(226, 0, 33, 180),
                    "SKINDEF_SANDSWEPT" => new Color32(220, 220, 220, 180),
                    _ => new Color32(255, 70, 0, 180),
                };
            }
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
                image.color = Color32.Lerp(lowHeatColor, inHeatColor, heatPercent);
                backdropImage.color = new Color32(53, 53, 53, (byte)Mathf.Lerp(0, 190, heatPercent));
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