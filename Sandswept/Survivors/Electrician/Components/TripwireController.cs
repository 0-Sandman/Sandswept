using System;
using R2API.Networking.Interfaces;

namespace Sandswept.Survivors.Electrician
{
    public class TripwireController : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Transform explo;
        public float interval;
        public VehicleSeat seat;
        public int hitRate;
        public static Dictionary<GameObject, TripwireController> ControllerMap = new();
        public float stopwatch;
        public Transform head;
        public BulletAttack attack;
        public ProjectileDamage pDamage;
        public ProjectileController controller;
        public float stopwatchBeam;
        public float delay;
        public float stopwatch2 = 0f;
        public float initDelay = 2f;
        public bool init = false;
        public BlastAttack blast;
        public GameObject effect;
        public bool isInVehicleMode = false;
        public float speed = 190f;
        public CharacterBody body;
        public Vector3 startPosition;
        public GameObject lightningEffect;
        public Animator pylonAnim;
        public SkinnedMeshRenderer mesh;
        public Material mat1;
        public Material mat2;
        public SkinDef skin1;
        public SkinDef skin2;
        public Mesh mesh1;
        public Mesh mesh2;
        public bool hasDetonated = false;
        public Transform modelTransform;
        public GameObject lightningVFX;
        public GameObject indicatorPrefab;
        public GameObject indicatorInstance;

        public void OnInteract(Interactor interactor)
        {
            blast.position = explo.position;
            blast.radius = 9f; // stop fucking using *= :sob:
            blast.baseDamage *= 2f; // stop fucking using *= :sob:
            blast.damageType.damageSource = DamageSource.Utility;
            blast.Fire();

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = blast.position,
                scale = blast.radius * 2f
            }, true);

            seat.EjectPassenger();
            GameObject.Destroy(this.gameObject);
        }

        public void OnCollisionEnter(Collision col)
        {
            if (col.collider && col.collider.GetComponent<GalvanicBallController>())
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                base.GetComponent<ProjectileSimple>().updateAfterFiring = false;
                rb.velocity = (base.transform.position - body.transform.position).normalized * 150f;

                if (indicatorInstance)
                {
                    indicatorInstance.GetComponent<PositionIndicator>().insideViewObject.transform.GetChild(0).GetComponent<ObjectScaleCurve>().Reset(); // lmao
                }
            }
        }

        public void Start()
        {
            controller = GetComponent<ProjectileController>();
            pDamage = GetComponent<ProjectileDamage>();

            seat.passengerAssignmentCooldown = float.MaxValue;

            attack = new()
            {
                damage = pDamage.damage * 3f / hitRate,
                radius = 0.8f,
                isCrit = pDamage.crit,
                owner = controller.owner,
                procCoefficient = 0.4f,
                stopperMask = LayerIndex.noCollision.mask,
                falloffModel = BulletAttack.FalloffModel.None,
                damageType = DamageType.Generic,
            };

            attack.damageType.damageSource = DamageSource.Utility;

            blast = new()
            {
                radius = 5f,
                attacker = attack.owner,
                crit = pDamage.crit,
                losType = BlastAttack.LoSType.None,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = DamageType.Shock5s,
                teamIndex = controller.teamFilter.teamIndex,
                procCoefficient = 1f,
                baseDamage = pDamage.damage * 3f
            };

            blast.damageType.damageSource = DamageSource.Utility;

            ModelLocator loc = attack.owner.GetComponent<ModelLocator>();
            modelTransform = loc.modelTransform;
            head = modelTransform.GetComponent<ChildLocator>().FindChild("Head");

            var modelSkinController = modelTransform.GetComponent<ModelSkinController>();

            var skin = modelSkinController.skins[modelSkinController.currentSkinIndex];
            var skinNameToken = skin.nameToken;

            effect = skinNameToken switch
            {
                "SKIN_ELEC_MASTERY" => VFX.GalvanicBolt.impactCovenant,
                _ => VFX.GalvanicBolt.impactDefault
            };

            lightningVFX = skinNameToken switch
            {
                "SKIN_ELEC_MASTERY" => VFX.StaticSnare.lightningVFXCovenant,
                _ => VFX.StaticSnare.lightningVFXDefault
            };

            indicatorPrefab = skinNameToken switch
            {
                "SKIN_ELEC_MASTERY" => VFX.StaticSnare.staticSnareIndicatorCovenant,
                _ => VFX.StaticSnare.staticSnareIndicatorDefault
            };

            ControllerMap.Add(controller.owner, this);
            body = controller.owner.GetComponent<CharacterBody>();

            delay = 1f / hitRate / body.attackSpeed;

            var constantBodySpeed = body.isSprinting ? body.moveSpeed / body.sprintingSpeedMultiplier : body.moveSpeed;

            speed = constantBodySpeed * 5f; // 7f * 27f = 189f, but I'm slowing it down initially to accelerate it later for a cool effect ! !

            lightningEffect = GameObject.Instantiate(lightningVFX, seat.seatPosition);
            lightningEffect.transform.localPosition = Vector3.zero;
            lightningEffect.SetActive(false);

            // the code below swaps out VOL-T's Utility skill mesh and material based on the skin selected
            if (skin == skin1)
            {
                mesh.sharedMesh = mesh1;
                mesh.sharedMaterial = mat1;
            }
            else
            {
                mesh.sharedMesh = mesh2;
                mesh.sharedMaterial = mat2;
            }

            indicatorInstance = UnityEngine.Object.Instantiate(indicatorPrefab, base.transform);
            indicatorInstance.GetComponent<PositionIndicator>().targetTransform = transform;
        }

        public void KABOOM()
        {
            // also wtf is this? this is the bulletattack being used and fired here for some reason ? ?
            // ok nvm this is just extremely misleading, this happens when CANCELLING the utility
            attack.origin = explo.position;
            attack.aimVector = (head.position - explo.position).normalized;
            attack.maxDistance = Vector3.Distance(explo.position, head.position);
            attack.radius = 0.8f; // stop fucking using *= :sob:
            attack.damage = pDamage.damage * 8f; // radius like passive explosion, but damage like the ejection explosion, something in between for ease of use and not having to go through it?
            attack.damageType |= DamageType.Shock5s;

            attack.Fire();

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = blast.position,
                scale = blast.radius
            }, true);

            Util.PlaySound("Play_elec_pylon_blast", base.gameObject);

            GameObject.Destroy(base.gameObject);
        }

        public bool StartZip()
        {
            if (!head)
            {
                return false;
            }

            seat.AssignPassenger(body.gameObject);
            startPosition = body.transform.position;
            isInVehicleMode = true;

            head.gameObject.SetActive(false);

            lightningEffect.SetActive(true);

            new CallNetworkedMethod(base.gameObject, "StartZipClient").Send(R2API.Networking.NetworkDestination.Clients);

            return true;
        }

        public void StartZipClient()
        {
            head.gameObject.SetActive(false);

            lightningEffect.SetActive(true);

            isInVehicleMode = true;
        }

        public void RestoreHeadClient()
        {
            if (head)
            {
                head.gameObject.SetActive(true);
            }
        }

        public void FixedUpdate()
        {
            stopwatch2 += Time.fixedDeltaTime;

            if (!init)
            {
                initDelay -= Time.fixedDeltaTime;

                if (initDelay <= 0f)
                {
                    init = true;
                }
            }

            lineRenderer.enabled = init && head;

            lineRenderer.SetPosition(1, explo.position);
            lineRenderer.SetPosition(0, isInVehicleMode ? seat.seatPosition.position : head.transform.position);

            if (isInVehicleMode)
            {
                if (!body)
                {
                    return;
                }

                speed += speed * 4f * Time.fixedDeltaTime; // acceelerateeeeeeee

                startPosition = Vector3.MoveTowards(startPosition, base.transform.position, speed * Time.fixedDeltaTime);
                seat.seatPosition.position = startPosition;
                // seat.UpdatePassengerPosition();

                if (Vector3.Distance(startPosition, base.transform.position) < 0.5f && !hasDetonated && NetworkServer.active)
                {
                    blast.position = explo.position;
                    blast.radius = 9f; // stop fucking using *= :sob:
                    blast.baseDamage = pDamage.damage * 8f;
                    blast.Fire();

                    hasDetonated = true;

                    isInVehicleMode = false;

                    EffectManager.SpawnEffect(effect, new EffectData
                    {
                        origin = blast.position,
                        scale = blast.radius
                    }, true);

                    if (head)
                    {
                        new CallNetworkedMethod(base.gameObject, "RestoreHeadClient").Send(R2API.Networking.NetworkDestination.Clients);
                        head.gameObject.SetActive(true);
                    }

                    seat.EjectPassenger();
                    GameObject.Destroy(this.gameObject);
                }
            }

            if (!NetworkServer.active || isInVehicleMode)
            {
                return;
            }

            if (init)
            {
                stopwatchBeam += Time.fixedDeltaTime;

                if (stopwatchBeam >= delay)
                {
                    stopwatchBeam = 0f;

                    attack.origin = explo.position;
                    attack.aimVector = (head.position - explo.position).normalized;
                    attack.maxDistance = Vector3.Distance(explo.position, head.position);

                    attack.Fire();
                }
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= interval && init)
            {
                stopwatch = 0f;

                blast.position = explo.position;
                blast.Fire();

                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = blast.position,
                    scale = blast.radius
                }, true);

                pylonAnim.Play("Pulse", pylonAnim.GetLayerIndex("Base"));

                Util.PlaySound("Play_elec_pylon_blast", base.gameObject);
            }
        }

        public void OnDisable()
        {
            if (controller.owner && ControllerMap.ContainsKey(controller.owner))
            {
                ControllerMap.Remove(controller.owner);
            }

            if (head)
            {
                head.gameObject.SetActive(true);
            }
        }

        public void OnDestroy()
        {
            if (indicatorInstance)
            {
                Object.Destroy(indicatorInstance);
                indicatorInstance = null;
            }
        }
    }
}