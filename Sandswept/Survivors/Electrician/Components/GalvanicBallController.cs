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
        private bool hasStuck = false;
        private CharacterBody owner;
        private Rigidbody body;

        public void Start()
        {
            pDamage = GetComponent<ProjectileDamage>();
            controller = GetComponent<ProjectileController>();
            owner = controller.owner.GetComponent<CharacterBody>();
            body = GetComponent<Rigidbody>();

            GetComponent<ProjectileProximityBeamController>().attackInterval /= owner.attackSpeed;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!hasStuck && NetworkServer.active)
            {
                if (collision.collider)
                {
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

                    if (!hasBouncedEnemy) attack.Fire();

                    hasBouncedEnemy = true;

                    Util.PlaySound("Play_loader_R_shock", base.gameObject);
                    EffectManager.SpawnEffect(Electrician.staticSnareImpactVFX, new EffectData
                    {
                        origin = attack.position,
                        scale = attack.radius * 2f
                    }, true);

                    var rb = GetComponent<Rigidbody>();
                    
                    if (collision.collider.gameObject.layer == LayerIndex.world.intVal) {
                        rb.isKinematic = true;
                        rb.velocity = Vector3.zero;
                        base.transform.up = collision.contacts[0].normal;
                        base.GetComponentInChildren<ParticleSystem>().Play();
                        hasStuck = true;
                    }
                    else {
                        rb.useGravity = true;
                        rb.velocity = Vector3.zero;
                        // rb.velocity += Physics.gravity;
                    }
                }
            }
        }
    }
}