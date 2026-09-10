using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class CannonballProjectile : MonoBehaviour
    {
        private const int MaximumSweepHits = 8;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField, Min(0.01f)] private float collisionRadius = 0.2f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private readonly RaycastHit[] sweepHits = new RaycastHit[MaximumSweepHits];
        private Rigidbody projectileRigidbody;
        private Transform ownerRoot;
        private AmmunitionType ammunitionType;
        private float damageMultiplier = 1f;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private float flightDuration;
        private float arcHeight;
        private float elapsedTime;
        private bool isFlying;

        public AmmunitionType AmmunitionType => ammunitionType;

        private void Awake()
        {
            projectileRigidbody = GetComponent<Rigidbody>();
            projectileRigidbody.useGravity = false;
            projectileRigidbody.isKinematic = true;
            projectileRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            projectileRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        public void Configure(AmmunitionType type, float newDamageMultiplier, float projectileScale)
        {
            ammunitionType = type;
            damageMultiplier = Mathf.Max(0f, newDamageMultiplier);
            transform.localScale *= Mathf.Max(0.1f, projectileScale);
        }

        public void Launch(Transform projectileOwner, Vector3 direction, float range, float duration, float height)
        {
            LaunchAt(
                projectileOwner,
                transform.position + direction.normalized * Mathf.Max(0f, range),
                duration,
                height
            );
        }

        public void LaunchAt(Transform projectileOwner, Vector3 targetPosition, float duration, float height)
        {
            ownerRoot = projectileOwner != null ? projectileOwner.root : null;
            startPosition = transform.position;
            endPosition = targetPosition;
            flightDuration = Mathf.Max(0.1f, duration);
            arcHeight = Mathf.Max(0f, height);
            elapsedTime = 0f;
            isFlying = true;
        }

        private void FixedUpdate()
        {
            if (!isFlying) return;

            elapsedTime += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / flightDuration);
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, endPosition, progress);
            float verticalOffset = 4f * arcHeight * progress * (1f - progress);
            Vector3 nextPosition = horizontalPosition + Vector3.up * verticalOffset;

            if (TryHitDamageable(projectileRigidbody.position, nextPosition)) return;

            projectileRigidbody.MovePosition(nextPosition);
            if (progress < 1f) return;

            PrototypeCombatVfx.PlayWaterSplash(nextPosition);
            PrototypeCameraShake.Request(0.035f, 0.06f);
            StopAndDestroy();
        }

        private bool TryHitDamageable(Vector3 currentPosition, Vector3 nextPosition)
        {
            Vector3 displacement = nextPosition - currentPosition;
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon) return false;

            int hitCount = Physics.SphereCastNonAlloc(
                currentPosition,
                collisionRadius,
                displacement / distance,
                sweepHits,
                distance,
                hitLayers,
                QueryTriggerInteraction.Collide
            );

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = sweepHits[index];
                if (ownerRoot != null && hit.collider.transform.root == ownerRoot) continue;

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsSunk) continue;

                damageable.ApplyDamage(new DamageInfo(
                    damage * damageMultiplier,
                    hit.point,
                    displacement,
                    ownerRoot != null ? ownerRoot.gameObject : null,
                    ammunitionType
                ));

                PrototypeCombatVfx.PlayHullImpact(hit.point, -displacement.normalized);
                PrototypeCameraShake.Request(0.12f, 0.12f);
                transform.position = hit.point;
                StopAndDestroy();
                return true;
            }

            return false;
        }

        private void StopAndDestroy()
        {
            isFlying = false;
            Destroy(gameObject);
        }
    }
}
