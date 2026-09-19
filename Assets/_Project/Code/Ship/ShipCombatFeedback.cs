using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(ShipHealth))]
    public sealed class ShipCombatFeedback : MonoBehaviour
    {
        private ShipHealth shipHealth;
        private bool isPlayer;
        private float nextHitFeedback;

        private void Awake()
        {
            shipHealth = GetComponent<ShipHealth>();
            isPlayer = GetComponent<ManualBroadsideAimController>() != null;
        }

        private void OnEnable()
        {
            shipHealth.Damaged += HandleDamaged;
            shipHealth.Sunk += HandleSunk;
        }

        private void OnDisable()
        {
            shipHealth.Damaged -= HandleDamaged;
            shipHealth.Sunk -= HandleSunk;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            // Never move the physics root for cosmetic hit feedback.
            // Hull impact particles are emitted by the projectile; a pellet burst
            // produces at most one small player-camera pulse per 0.2 seconds.
            if (!isPlayer || Time.time < nextHitFeedback) return;
            nextHitFeedback = Time.time + 0.2f;
            PrototypeCameraShake.Request(0.025f, 0.08f);
        }

        private void HandleSunk()
        {
            PrototypeCombatVfx.PlaySinkingSmoke(transform.position + Vector3.up * 0.4f);
        }
    }
}
