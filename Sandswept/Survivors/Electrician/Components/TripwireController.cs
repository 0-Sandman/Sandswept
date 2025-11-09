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
        public float passiveExplosionRadius = 13f;
        public float passiveExplosionDamage = 2f;
        public float passiveExplosionInterval = 2f;
        public float ejectExplosionRadius = 18f;
        public float ejectExplosionDamage = 6f;
        public float ejectExplosionDelay;
        public float ejectExplosionTimer = 0f;
        public float lineRadius = 1.5f;
        public float lineDamage = 2f;
        public float baseSpeedMultiplier = 5f;
        public float accelerationCoefficient = 4f;
        public float decelerationCoefficient = 12f;

        public void OnInteract(Interactor interactor)
        {
            blast.position = explo.position;
            blast.radius = ejectExplosionRadius; // stop fucking using *= :sob:
            blast.baseDamage = ejectExplosionDamage; // stop fucking using *= :sob:
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
                damage = pDamage.damage * lineDamage / hitRate,
                radius = lineRadius,
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
                radius = passiveExplosionRadius,
                attacker = attack.owner,
                crit = pDamage.crit,
                losType = BlastAttack.LoSType.None,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = DamageType.Shock5s,
                teamIndex = controller.teamFilter.teamIndex,
                procCoefficient = 1f,
                baseDamage = pDamage.damage * passiveExplosionDamage
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
                "VOLT_SKIN_COVENANT_NAME" => VFX.GalvanicBolt.impactCovenant,
                _ => VFX.GalvanicBolt.impactDefault
            };

            lightningVFX = skinNameToken switch
            {
                "VOLT_SKIN_COVENANT_NAME" => VFX.StaticSnare.lightningVFXCovenant,
                _ => VFX.StaticSnare.lightningVFXDefault
            };

            indicatorPrefab = skinNameToken switch
            {
                "VOLT_SKIN_COVENANT_NAME" => VFX.StaticSnare.staticSnareIndicatorCovenant,
                _ => VFX.StaticSnare.staticSnareIndicatorDefault
            };

            ControllerMap.Add(controller.owner, this);
            body = controller.owner.GetComponent<CharacterBody>();

            delay = 1f / hitRate / body.attackSpeed;
            interval = passiveExplosionInterval / body.attackSpeed;
            // normally, the interval is 1f

            var constantBodySpeed = body.isSprinting ? body.moveSpeed / body.sprintingSpeedMultiplier : body.moveSpeed;

            speed = constantBodySpeed * baseSpeedMultiplier; // 7f * 27f = 189f, but I'm slowing it down initially to accelerate it later for a cool effect ! !

            lightningEffect = GameObject.Instantiate(lightningVFX, seat.seatPosition);
            lightningEffect.transform.localPosition = Vector3.zero;
            lightningEffect.SetActive(false);

            // the code below swaps out VOL-T's Utility skill mesh and material based on the skin selected
            // reversed ifs for other skin fix
            if (skin == skin2)
            {
                mesh.sharedMesh = mesh2;
                mesh.sharedMaterial = mat2;
            }
            else
            {
                mesh.sharedMesh = mesh1;
                mesh.sharedMaterial = mat1;
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
            attack.radius = lineRadius; // stop fucking using *= :sob:
            attack.damage = pDamage.damage * ejectExplosionRadius; // radius like passive explosion, but damage like the ejection explosion, something in between for ease of use and not having to go through it?
            attack.damageType |= DamageType.Shock5s;

            attack.Fire();

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = blast.position,
                scale = blast.radius
            }, true);

            Util.PlaySound("Play_elec_pylon_blast", base.gameObject);
            Util.PlaySound("Play_voidRaid_snipe_impact", gameObject);

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

            if (lineRenderer && explo && seat && head)
            {
                lineRenderer.enabled = init && head;

                lineRenderer.SetPosition(1, explo.position);
                lineRenderer.SetPosition(0, isInVehicleMode ? seat.seatPosition.position : head.transform.position);
            }

            if (isInVehicleMode)
            {
                if (!body)
                {
                    return;
                }

                startPosition = Vector3.MoveTowards(startPosition, base.transform.position, speed * Time.fixedDeltaTime);
                seat.seatPosition.position = startPosition;
                // seat.UpdatePassengerPosition();

                var distance = Vector3.Distance(startPosition, base.transform.position);

                speed += speed * accelerationCoefficient * Time.fixedDeltaTime; // acceelerateeeeeeee

                if (NetworkServer.active)
                {
                    if (!hasDetonated && distance < 0.5f)
                    {
                        ejectExplosionTimer += Time.fixedDeltaTime;
                        if (ejectExplosionTimer >= ejectExplosionDelay)
                        {
                            blast.position = explo.position;
                            blast.radius = ejectExplosionRadius; // stop fucking using *= :sob:
                            blast.baseDamage = pDamage.damage * ejectExplosionDamage;
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

                            Util.PlaySound("Play_voidRaid_snipe_impact", gameObject);
                            Util.PlaySound("Stop_loader_m2_travel_loop", body.gameObject);

                            seat.EjectPassenger();
                            GameObject.Destroy(this.gameObject);
                        }
                    }
                    else
                    {
                        ejectExplosionDelay = 0.05f + (Mathf.Sqrt(speed) * 0.003f);
                    }
                }
            }

            if (!NetworkServer.active || isInVehicleMode)
            {
                return;
            }

            if (init && body && attack != null)
            {
                stopwatchBeam += Time.fixedDeltaTime;

                if (stopwatchBeam >= delay)
                {
                    stopwatchBeam = 0f;

                    delay = 1f / hitRate / body.attackSpeed;

                    attack.origin = explo.position;
                    attack.aimVector = (head.position - explo.position).normalized;
                    attack.maxDistance = Vector3.Distance(explo.position, head.position);

                    attack.Fire();
                }
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= interval && init && blast != null)
            {
                stopwatch = 0f;

                interval = passiveExplosionInterval / body.attackSpeed;

                blast.position = explo.position;
                blast.Fire();

                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = blast.position,
                    scale = blast.radius
                }, true);

                pylonAnim.Play("Pulse", pylonAnim.GetLayerIndex("Base"));

                Util.PlaySound("Play_elec_pylon_blast", base.gameObject);
                Util.PlaySound("Play_voidRaid_snipe_impact", gameObject);
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
            /*
            if (controller.owner)
            {
                Util.PlaySound("Play_bison_charge_attack_collide", controller.owner);
            }
            */

            if (indicatorInstance)
            {
                Object.Destroy(indicatorInstance);
                indicatorInstance = null;
            }
        }
    }
}