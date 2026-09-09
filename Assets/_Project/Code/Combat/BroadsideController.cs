using System.Collections;
using UnityEngine;

namespace Seaborn.Combat
{
    public sealed class BroadsideController : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField]
        private CannonballProjectile cannonballPrefab;

        [SerializeField, Min(1f)]
        private float projectileRange = 9f;

        [SerializeField, Min(0.1f)]
        private float projectileFlightDuration = 1.25f;

        [SerializeField, Min(0f)]
        private float projectileArcHeight = 2f;

        [Header("Muzzles")]
        [SerializeField]
        private Transform[] portMuzzles;

        [SerializeField]
        private Transform[] starboardMuzzles;

        [Header("Timing")]
        [SerializeField, Min(0f)]
        private float delayBetweenCannons = 0.18f;

        [SerializeField, Min(0f)]
        private float broadsideCooldown = 2.5f;

        private float nextPortFireTime;
        private float nextStarboardFireTime;

        public float MaximumRange => projectileRange;

        public bool TryFire(BroadsideSide side)
        {
            Vector3 direction =
                side == BroadsideSide.Port
                    ? -transform.right
                    : transform.right;

            Vector3 targetPoint =
                transform.position +
                direction * projectileRange;

            return TryFireAt(
                side,
                targetPoint,
                1f
            );
        }

        public bool TryFireAt(
            BroadsideSide side,
            Vector3 targetPoint,
            float accuracy)
        {
            if (!CanFire(side))
            {
                return false;
            }

            Transform[] muzzles = GetMuzzles(side);

            if (cannonballPrefab == null ||
                muzzles == null ||
                muzzles.Length == 0)
            {
                return false;
            }

            SetCooldown(side);

            StartCoroutine(
                FireBroadsideAt(
                    muzzles,
                    targetPoint,
                    Mathf.Clamp01(accuracy)
                )
            );

            return true;
        }

        public float GetCooldownRemaining(
            BroadsideSide side)
        {
            float nextFireTime =
                side == BroadsideSide.Port
                    ? nextPortFireTime
                    : nextStarboardFireTime;

            return Mathf.Max(
                0f,
                nextFireTime - Time.time
            );
        }

        private bool CanFire(BroadsideSide side)
        {
            return GetCooldownRemaining(side) <= 0f;
        }

        private Transform[] GetMuzzles(
            BroadsideSide side)
        {
            return side == BroadsideSide.Port
                ? portMuzzles
                : starboardMuzzles;
        }

        private void SetCooldown(BroadsideSide side)
        {
            float nextFireTime =
                Time.time + broadsideCooldown;

            if (side == BroadsideSide.Port)
            {
                nextPortFireTime = nextFireTime;
            }
            else
            {
                nextStarboardFireTime = nextFireTime;
            }
        }

        private IEnumerator FireBroadsideAt(
            Transform[] muzzles,
            Vector3 targetPoint,
            float accuracy)
        {
            float spreadRadius = Mathf.Lerp(
                1.8f,
                0.12f,
                accuracy
            );

            foreach (Transform muzzle in muzzles)
            {
                if (muzzle == null)
                {
                    continue;
                }

                Vector2 randomOffset =
                    Random.insideUnitCircle *
                    spreadRadius;

                Vector3 scatteredTarget =
                    targetPoint +
                    new Vector3(
                        randomOffset.x,
                        0f,
                        randomOffset.y
                    );

                Vector3 toTarget =
                    scatteredTarget - muzzle.position;
                toTarget.y = 0f;

                float distance = Mathf.Clamp(
                    toTarget.magnitude,
                    0.1f,
                    projectileRange
                );

                Vector3 clampedTarget =
                    muzzle.position +
                    toTarget.normalized * distance;
                clampedTarget.y = targetPoint.y;

                float distanceRatio = Mathf.Clamp01(
                    distance / projectileRange
                );
                float duration =
                    projectileFlightDuration *
                    Mathf.Lerp(
                        0.4f,
                        1f,
                        distanceRatio
                    );
                float height =
                    projectileArcHeight *
                    Mathf.Lerp(
                        0.35f,
                        1f,
                        distanceRatio
                    );

                CannonballProjectile projectile =
                    Instantiate(
                        cannonballPrefab,
                        muzzle.position,
                        Quaternion.LookRotation(
                            toTarget.normalized,
                            Vector3.up
                        )
                    );

                PrototypeCombatVfx.PlayMuzzleBurst(
                    muzzle.position,
                    toTarget.normalized
                );

                PrototypeCameraShake.Request(
                    0.06f,
                    0.08f
                );

                projectile.LaunchAt(
                    transform,
                    clampedTarget,
                    duration,
                    height
                );

                if (delayBetweenCannons > 0f)
                {
                    yield return new WaitForSeconds(
                        delayBetweenCannons
                    );
                }
            }
        }
    }
}
