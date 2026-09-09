using UnityEngine;

namespace Seaborn.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class CannonballProjectile : MonoBehaviour
    {
        private Rigidbody projectileRigidbody;
        private Transform ownerRoot;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private float flightDuration;
        private float arcHeight;
        private float elapsedTime;
        private bool isFlying;

        private void Awake()
        {
            projectileRigidbody = GetComponent<Rigidbody>();
            projectileRigidbody.useGravity = false;
            projectileRigidbody.isKinematic = true;
            projectileRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            projectileRigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;
        }

        public void Launch(
            Transform projectileOwner,
            Vector3 direction,
            float range,
            float duration,
            float height)
        {
            ownerRoot = projectileOwner != null
                ? projectileOwner.root
                : null;

            startPosition = transform.position;
            endPosition =
                startPosition +
                direction.normalized * Mathf.Max(0f, range);

            flightDuration = Mathf.Max(0.1f, duration);
            arcHeight = Mathf.Max(0f, height);

            elapsedTime = 0f;
            isFlying = true;
        }

        private void FixedUpdate()
        {
            if (!isFlying)
            {
                return;
            }

            elapsedTime += Time.fixedDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / flightDuration
            );

            Vector3 horizontalPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                progress
            );

            float verticalOffset =
                4f *
                arcHeight *
                progress *
                (1f - progress);

            Vector3 nextPosition =
                horizontalPosition +
                Vector3.up * verticalOffset;

            projectileRigidbody.MovePosition(nextPosition);

            if (progress >= 1f)
            {
                isFlying = false;
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ownerRoot != null &&
                other.transform.root == ownerRoot)
            {
                return;
            }

            isFlying = false;
            Destroy(gameObject);
        }
    }
}
