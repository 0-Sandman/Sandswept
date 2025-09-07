using System;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class DeltaBurnyTrail : MonoBehaviour
    {
        public CharacterBody owner;
        public float damagePerSecond;
        public int ticksPerSecond = 5;
        public float delay => 1f / ticksPerSecond;
        public float stopwatch = 0f;
        public float damage => damagePerSecond / ticksPerSecond;
        public OverlapAttack attack;

        public void Start()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            attack = new()
            {
                damage = damage,
                attacker = owner.gameObject,
                procCoefficient = 0,
                damageColorIndex = DamageColorIndex.Poison,
                hitBoxGroup = GetComponent<HitBoxGroup>(),
                isCrit = false
            };
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay)
                {
                    stopwatch = 0f;
                    attack.ResetIgnoredHealthComponents();
                    attack.Fire();
                }
            }
        }
    }
}