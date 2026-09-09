using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Combat
{
    public sealed class BroadsideController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private InputActionReference firePortAction;

        [SerializeField]
        private InputActionReference fireStarboardAction;

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

        private void OnEnable()
        {
            firePortAction?.action.Enable();
            fireStarboardAction?.action.Enable();
        }

        private void OnDisable()
        {
            firePortAction?.action.Disable();
            fireStarboardAction?.action.Disable();
        }

        private void Update()
        {
            if (firePortAction != null &&
                firePortAction.action.WasPressedThisFrame())
            {
                TryFirePort();
            }

            if (fireStarboardAction != null &&
                fireStarboardAction.action.WasPressedThisFrame())
            {
                TryFireStarboard();
            }
        }

        private void TryFirePort()
        {
            TryFire(BroadsideSide.Port);
        }

        private void TryFireStarboard()
        {
            TryFire(BroadsideSide.Starboard);
        }

        public bool TryFire(BroadsideSide side)
        {
            bool isPort = side == BroadsideSide.Port;
            float nextFireTime =
                isPort
                    ? nextPortFireTime
                    : nextStarboardFireTime;

            if (Time.time < nextFireTime)
            {
                return false;
            }

            Transform[] muzzles =
                isPort
                    ? portMuzzles
                    : starboardMuzzles;

            if (cannonballPrefab == null ||
                muzzles == null ||
                muzzles.Length == 0)
            {
                return false;
            }

            float updatedFireTime =
                Time.time + broadsideCooldown;

            if (isPort)
            {
                nextPortFireTime = updatedFireTime;
            }
            else
            {
                nextStarboardFireTime = updatedFireTime;
            }

            StartCoroutine(
                FireBroadside(
                    muzzles,
                    isPort
                        ? -transform.right
                        : transform.right
                )
            );

            return true;
        }

        private IEnumerator FireBroadside(
            Transform[] muzzles,
            Vector3 direction)
        {
            if (cannonballPrefab == null || muzzles == null)
            {
                yield break;
            }

            foreach (Transform muzzle in muzzles)
            {
                if (muzzle == null)
                {
                    continue;
                }

                CannonballProjectile projectile =
                    Instantiate(
                        cannonballPrefab,
                        muzzle.position,
                        Quaternion.LookRotation(direction)
                    );

                PrototypeCombatVfx.PlayMuzzleBurst(
                    muzzle.position,
                    direction
                );

                PrototypeCameraShake.Request(
                    0.06f,
                    0.08f
                );

                projectile.Launch(
                    transform,
                    direction,
                    projectileRange,
                    projectileFlightDuration,
                    projectileArcHeight
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
