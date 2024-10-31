using System;

namespace Sandswept.Survivors.Electrician {
    public class TripwireController : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Transform explo;
        public float interval;
        public VehicleSeat seat;
        public int hitRate;
        public static Dictionary<GameObject, TripwireController> ControllerMap = new();
        private float stopwatch;
        private Transform head;
        private BulletAttack attack;
        private ProjectileDamage pDamage;
        private ProjectileController controller;
        private float stopwatchBeam;
        private float delay;
        private float stopwatch2 = 0f;
        private float initDelay = 2f;
        private bool init = false;
        private BlastAttack blast;
        private GameObject effect;
        private bool isInVehicleMode = false;
        private float speed = 190f;
        private CharacterBody body;
        private Vector3 startPosition;
        public GameObject lightningEffect;

        public void OnInteract(Interactor interactor) {
            blast.position = explo.position;
            blast.radius *= 2f;
            blast.baseDamage *= 2f;
            blast.Fire();

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = blast.position,
                scale = blast.radius * 2
            }, true);

            seat.EjectPassenger();
            GameObject.Destroy(this.gameObject);
        }

        public void OnCollisionEnter(Collision col) {
            if (col.collider && col.collider.GetComponent<GalvanicBallController>()) {
                Rigidbody rb = GetComponent<Rigidbody>();
                base.GetComponent<ProjectileSimple>().updateAfterFiring = false;
                rb.velocity = col.contacts[0].normal * 130f;
            }
        }

        public void Start()
        {
            controller = GetComponent<ProjectileController>();
            pDamage = GetComponent<ProjectileDamage>();

            delay = 1f / hitRate;

            attack = new()
            {
                damage = pDamage.damage * 2f * delay,
                radius = lineRenderer.startWidth,
                isCrit = pDamage.crit,
                owner = controller.owner,
                procCoefficient = 0.4f,
                stopperMask = LayerIndex.noCollision.mask,
                falloffModel = BulletAttack.FalloffModel.None
            };

            blast = new()
            {
                radius = 3f,
                attacker = attack.owner,
                crit = pDamage.crit,
                losType = BlastAttack.LoSType.None,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = DamageType.Shock5s,
                teamIndex = controller.teamFilter.teamIndex,
                procCoefficient = 1f,
                baseDamage = pDamage.damage * 3f
            };
        

            ModelLocator loc = attack.owner.GetComponent<ModelLocator>();
            head = loc.modelTransform.GetComponent<ChildLocator>().FindChild("Head");

            effect = Electrician.staticSnareImpactVFX;

            ControllerMap.Add(controller.owner, this);
            body = controller.owner.GetComponent<CharacterBody>();

            lightningEffect = GameObject.Instantiate(Electrician.LightningZipEffect, seat.seatPosition);
            lightningEffect.transform.localPosition = Vector3.zero;
            lightningEffect.SetActive(false);
        }

        public bool StartZip() {
            if (!head) {
                return false;
            }

            seat.AssignPassenger(body.gameObject);
            startPosition = body.transform.position;
            isInVehicleMode = true;

            attack.origin = explo.position;
            attack.aimVector = (head.position - explo.position).normalized;
            attack.maxDistance = Vector3.Distance(explo.position, head.position);
            attack.radius *= 3f;
            attack.damage = pDamage.damage * 8f;
            attack.damageType |= DamageType.Shock5s;

            attack.Fire();

            head.gameObject.SetActive(false);

            lightningEffect.SetActive(true);

            return true;
        }

        public void FixedUpdate()
        {
            stopwatch2 += Time.fixedDeltaTime;
            
            if (!init) {
                initDelay -= Time.fixedDeltaTime;

                if (initDelay <= 0f) {
                    init = true;
                }
            }

            lineRenderer.enabled = init && head;

            
            lineRenderer.SetPosition(0, explo.position);
            lineRenderer.SetPosition(1, isInVehicleMode ? seat.seatPosition.position : head.transform.position);

            if (isInVehicleMode) {
                if (!body || !body.hasAuthority) {
                    return;
                }

                startPosition = Vector3.MoveTowards(startPosition, base.transform.position, speed * Time.fixedDeltaTime);
                seat.seatPosition.position = startPosition;
                seat.UpdatePassengerPosition();

                if (Vector3.Distance(startPosition, base.transform.position) < 0.5f) {
                    blast.position = explo.position;
                    blast.radius *= 2f;
                    blast.baseDamage *= 2f;
                    blast.Fire();

                    EffectManager.SpawnEffect(effect, new EffectData
                    {
                        origin = blast.position,
                        scale = blast.radius * 2
                    }, true);

                    if (head) {
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
                    scale = blast.radius * 2
                }, true);
            }
        }

        public void OnDisable()
        {
            if (controller.owner && ControllerMap.ContainsKey(controller.owner)) {
                ControllerMap.Remove(controller.owner);
            }

            if (head) {
                head.gameObject.SetActive(true);
            }
        }
    }
}