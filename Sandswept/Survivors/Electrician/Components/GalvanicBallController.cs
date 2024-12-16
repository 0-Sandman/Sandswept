using System;

namespace Sandswept.Survivors.Electrician
{
    public class GalvanicBallController : MonoBehaviour
    {
        public float radius = 13f;
        public float damage = 1f;
        private bool hasBouncedEnemy = false;
        private ProjectileDamage pDamage;
        private ProjectileController controller;
        private CharacterBody owner;
        private Rigidbody body;
        private ProjectileStickOnImpact stick;

        public void Start()
        {
            pDamage = GetComponent<ProjectileDamage>();
            controller = GetComponent<ProjectileController>();
            owner = controller.owner.GetComponent<CharacterBody>();
            body = GetComponent<Rigidbody>();
            stick = GetComponent<ProjectileStickOnImpact>();

            stick.stickEvent.AddListener(UnstickAndDrop);

            GetComponent<ProjectileProximityBeamController>().attackInterval /= owner.attackSpeed;
        }

        public void UnstickAndDrop()
        {
            if (stick.stuckTransform && stick.stuckTransform.parent.GetComponent<TripwireController>())
            {
                stick.Detach();
                stick.rigidbody.velocity = Vector3.zero;
                stick.rigidbody.useGravity = true;
                stick.enabled = false;
                base.transform.parent = null;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!hasBouncedEnemy && NetworkServer.active)
            {
                if (collision.collider)
                {
                    hasBouncedEnemy = true;

                    BlastAttack attack = new()
                    {
                        radius = radius,
                        attacker = owner.gameObject,
                        position = collision.contacts[0].point,
                        crit = pDamage.crit,
                        losType = BlastAttack.LoSType.None,
                        falloffModel = BlastAttack.FalloffModel.None,
                        damageType = pDamage.damageType,
                        teamIndex = owner.teamComponent.teamIndex,
                        procCoefficient = 1f,
                        baseDamage = pDamage.damage * damage
                    };

                    attack.Fire();

                    Util.PlaySound("Play_loader_R_shock", base.gameObject);
                    EffectManager.SpawnEffect(Electrician.staticSnareImpactVFX, new EffectData
                    {
                        origin = attack.position,
                        scale = attack.radius * 2f
                    }, true);

                    var rb = GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.velocity = Vector3.zero;
                    // rb.velocity += Physics.gravity;
                }
            }
        }
    }
}