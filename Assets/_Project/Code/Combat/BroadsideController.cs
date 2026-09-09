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
            if (Time.time < nextPortFireTime)
            {
                return;
            }

            nextPortFireTime =
                Time.time + broadsideCooldown;

            StartCoroutine(
                FireBroadside(
                    portMuzzles,
                    -transform.right
                )
            );
        }

        private void TryFireStarboard()
        {
            if (Time.time < nextStarboardFireTime)
            {
                return;
            }

            nextStarboardFireTime =
                Time.time + broadsideCooldown;

            StartCoroutine(
                FireBroadside(
                    starboardMuzzles,
                    transform.right
                )
            );
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
