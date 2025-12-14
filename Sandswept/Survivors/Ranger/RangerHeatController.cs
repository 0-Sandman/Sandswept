using RoR2.UI;
using RoR2.HudOverlay;
using UnityEngine.UI;
using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.States.Special;
using System.Linq;

namespace Sandswept.Survivors.Ranger
{
    public class RangerHeatController : MonoBehaviour
    {
        public Animator anim;
        public CharacterBody cb;
        public HealthComponent hc;

        public GameObject overlayPrefab;
        public GameObject overlayInstance;

        public bool isInOverdrive = false;

        public static float maxHeat = 100f;

        public float currentHeat = 0f;

        public bool isInFullHeat = false;

        public float selfDamageAtMax = 0.04f;
        public float selfDamageInterval = 0.25f;
        public float selfDamageTimer;

        public float chargeLossTimer = 0f;
        public float chargeLossInterval = 3f;

        public int lastHealingReductionCount;

        public EntityStateMachine esm;

        public void Start()
        {
            overlayPrefab = Main.assets.LoadAsset<GameObject>("Assets/Sandswept/Base/Characters/Ranger/OverlayRanger.prefab");
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
            {
                chargeLossTimer += Time.fixedDeltaTime;
            }

            if (chargeLossTimer >= chargeLossInterval)
            {
                cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - 1));

                chargeLossTimer = 0f;
            }

            if (isInOverdrive)
            {
                selfDamageTimer += Time.fixedDeltaTime;

                if (selfDamageTimer >= selfDamageInterval)
                {
                    float self = Util.Remap(currentHeat, 0, maxHeat, 0f, selfDamageAtMax) * selfDamageInterval;
                    TakeDamage(self);
                    selfDamageTimer = 0f;

                    cb.SetBuffCount(HeatHealingReduction.instance.BuffDef.buffIndex, Mathf.Clamp(Mathf.RoundToInt(currentHeat / maxHeat * 2.5f * 100f), 0, 100));
                }
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
            chargeLossTimer = 0f;

            if (cb)
            {
                // cb.SetBuffCount(OverheatDamageBoost.instance.BuffDef.buffIndex, 0);

                // thoughts on this below
                // cb.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, 0);

                Invoke(nameof(RemoveHealingReduction), 2f);
            }

            currentHeat = 0f;

            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine && machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            EntityStateMachine machine2 = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            if (machine2)
            {
                machine2.SetState(new Idle());
            }

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
                damage = hc.fullCombinedHealth * timeInOverheat,
                crit = false,
                position = transform.position,
                damageColorIndex = DamageColorIndex.Fragile,
                damageType = DamageType.BypassArmor | DamageType.BypassBlock,
                force = Vector3.zero
            };

            info.AddModdedDamageType(Main.eclipseSelfDamage);

            if (NetworkServer.active)
            {
                cb.healthComponent.TakeDamage(info);
            }

            Util.PlaySound("Play_item_proc_igniteOnKill", gameObject);
        }
    }

    public class RangerChargeDisplay : MonoBehaviour
    {
        public RawImage[] ChargePips;
        public Color32 ChargeColor;
        public Color32 fullyChargedColor;
        public CharacterBody body;
        public HudElement hud;
        public bool ranColorSwap = false;

        public void Start()
        {
            hud = GetComponent<HudElement>();

            ChargePips[0].transform.localScale *= 1.5f;
            ChargePips[1].transform.localScale *= 1.5f;
            ChargePips[2].transform.localScale *= 1.5f;
        }

        public void Update()
        {
            if (!body && hud && hud.targetBodyObject)
            {
                body = hud.targetBodyObject.GetComponent<CharacterBody>();
            }

            if (!body)
            {
                return;
            }

            if (!ranColorSwap)
            {
                Transform modelTransform = null;
                if (body.modelLocator)
                {
                    modelTransform = body.modelLocator.modelTransform;
                }

                if (modelTransform)
                {
                    var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[body.skinIndex].nameToken;

                    ChargeColor = skinNameToken switch
                    {
                        "RANGER_SKIN_MAJOR_NAME" => new Color32(61, 105, 198, 140),
                        "RANGER_SKIN_RENEGADE_NAME" => new Color32(196, 62, 174, 140),
                        "RANGER_SKIN_MILEZERO_NAME" => new Color32(181, 30, 30, 140),
                        "RANGER_SKIN_SANDSWEPT_NAME" => new Color32(196, 136, 62, 140),
                        _ => new Color32(78, 182, 142, 140),
                    };
                    fullyChargedColor = skinNameToken switch
                    {
                        "RANGER_SKIN_MAJOR_NAME" => new Color32(61, 105, 198, 255),
                        "RANGER_SKIN_RENEGADE_NAME" => new Color32(196, 62, 174, 255),
                        "RANGER_SKIN_MILEZERO_NAME" => new Color32(181, 30, 30, 255),
                        "RANGER_SKIN_SANDSWEPT_NAME" => new Color32(196, 136, 62, 255),
                        _ => new Color32(78, 182, 142, 255),
                    };
                }
                ranColorSwap = true;
            }

            int charge = body.GetBuffCount(Buffs.Charge.instance.BuffDef);

            ChargePips[0].color = new Color32(0, 0, 0, 0);
            ChargePips[1].color = new Color32(0, 0, 0, 0);
            ChargePips[2].color = new Color32(0, 0, 0, 0);
            ChargePips[3].color = Color.white;

            if (charge >= 20)
            {
                ChargePips[3].color = fullyChargedColor;
            }

            UpdateColor(ChargePips[0]);
            UpdateColor(ChargePips[1]);
            UpdateColor(ChargePips[2]);

            void UpdateColor(RawImage image)
            {
                if (charge == 0)
                {
                    return;
                }

                if (charge >= 6)
                {
                    image.color = ChargeColor;
                    charge -= 6;
                    return;
                }

                float mult = (float)charge / 6f;
                image.color = new Color32((byte)(ChargeColor.r * mult), (byte)(ChargeColor.g * mult), (byte)(ChargeColor.b * mult), (byte)(ChargeColor.a * mult));
                charge -= charge;
            }
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
        public Stack<Image> heatSprites = new();
        public GameObject heatGaugeRef;
        public float smoothingTime = 0.2f;
        public float targetPercent;
        public float heatPercent;

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
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillClockwise = true;
            image.RemoveComponent<ImageFillController>();

            heatGaugeRef = image.gameObject;

            heatSprites.Push(image);

            backdropImage = heatMeterBackdrop.GetComponent<Image>();
            backdropImage.sprite = Main.hifuSandswept.LoadAsset<Sprite>("texHeatMeterOutline.png");

            element = GetComponent<HudElement>();

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
                    "RANGER_SKIN_MAJOR_NAME" => new Color32(0, 130, 224, 255),
                    "RANGER_SKIN_RENEGADE_NAME" => new Color32(244, 95, 197, 255),
                    "RANGER_SKIN_MILEZERO_NAME" => new Color32(153, 0, 23, 255),
                    "RANGER_SKIN_SANDSWEPT_NAME" => new Color32(214, 159, 79, 255),
                    _ => new Color32(255, 200, 0, 255),
                };
                inHeatColor = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => new Color32(60, 0, 244, 255),
                    "RANGER_SKIN_RENEGADE_NAME" => new Color32(114, 39, 244, 255),
                    "RANGER_SKIN_MILEZERO_NAME" => new Color32(226, 0, 33, 255),
                    "RANGER_SKIN_SANDSWEPT_NAME" => new Color32(220, 220, 220, 255),
                    _ => new Color32(255, 70, 0, 255),
                };
            }
        }

        public void FixedUpdate()
        {
            if (!target)
            {
                return;
            }

            if (!target.isInOverdrive)
            {
                targetPercent = 0f;
                heatPercent = 0f;
                image.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                image.transform.parent.gameObject.SetActive(true);
            }

            targetPercent = target.currentHeat / RangerHeatController.maxHeat;
            heatPercent = Mathf.MoveTowards(heatPercent, targetPercent, (Mathf.Abs(targetPercent - heatPercent) / smoothingTime) * Time.fixedDeltaTime);
            // Main.ModLogger.LogError("target.currentHeat: " + target.currentHeat + ", RangerHeatController.maxHeat: " + RangerHeatController.maxHeat + ", heatPercent: " + heatPercent);
            // all seems correct with these values, the previous heat value is shown when first entering overdrive and not doing anything probably because it instantiates first and THEN pops?

            colorUpdateTimer += Time.fixedDeltaTime;
            if (colorUpdateTimer >= colorUpdateInterval)
            {
                // image.color = Color32.Lerp(lowHeatColor, inHeatColor, heatPercent);
                backdropImage.color = new Color32(53, 53, 53, (byte)Mathf.Lerp(60, 190, heatPercent));
                colorUpdateTimer = 0f;
            }

            int iterations = (int)Mathf.Ceil(heatPercent);
            if (iterations > heatSprites.Count)
            {
                for (int i = heatSprites.Count; i < iterations; i++)
                {
                    GameObject newGauge = GameObject.Instantiate(heatGaugeRef, image.transform.parent);
                    heatSprites.Push(newGauge.GetComponent<Image>());
                }
            }
            else if (heatSprites.Count > iterations && heatSprites.Peek() != image)
            {
                for (int i = iterations; i < heatSprites.Count; i++)
                {
                    Image gauge = heatSprites.Pop();
                    GameObject.Destroy(gauge.gameObject);
                }
            }

            /* for (int i = iterations - 1; i >= 0; i--)
            {
                float fillAmount = i > 0 ? 1f : heatPercent;
                Image image = heatSprites.ElementAt(i);
                image.fillAmount = Mathf.Clamp01(fillAmount);
                image.color = Color.Lerp(lowHeatColor, inHeatColor, (heatPercent - i) / 3f);
            } */

            for (int i = 0; i < heatSprites.Count; i++)
            {
                Image image = heatSprites.ElementAt(i);
                float fillAmount = heatPercent - i;
                if (i != Mathf.FloorToInt(heatPercent)) fillAmount = 1f;
                image.fillAmount = Mathf.Clamp01(fillAmount);
                image.color = Color.Lerp(lowHeatColor, inHeatColor, i / 3f);
            }
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