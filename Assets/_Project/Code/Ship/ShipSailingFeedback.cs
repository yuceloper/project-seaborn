using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipSailingFeedback : MonoBehaviour
    {
        [Header("Wake")]
        [SerializeField, Min(0f)]
        private float wakeStartSpeed = 0.25f;

        [SerializeField, Min(0.1f)]
        private float fullWakeSpeed = 8f;

        [SerializeField, Min(0f)]
        private float sternOffset = 2.35f;

        [SerializeField, Min(0f)]
        private float wakeHalfWidth = 0.48f;

        [SerializeField, Min(0f)]
        private float waterlineOffset = 0.12f;

        [SerializeField, Range(0.08f, 0.6f)]
        private float minimumEmissionInterval = 0.12f;

        [SerializeField, Range(0.1f, 1f)]
        private float maximumEmissionInterval = 0.34f;

        private Rigidbody shipRigidbody;
        private float nextWakeTime;
        private bool emitPortNext;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            if (shipRigidbody == null)
            {
                return;
            }

            float forwardSpeed = Mathf.Max(
                0f,
                Vector3.Dot(
                    shipRigidbody.linearVelocity,
                    transform.forward
                )
            );
            if (forwardSpeed < wakeStartSpeed)
            {
                nextWakeTime = Time.time;
                return;
            }

            float strength = Mathf.InverseLerp(
                wakeStartSpeed,
                fullWakeSpeed,
                forwardSpeed
            );
            if (Time.time < nextWakeTime)
            {
                return;
            }

            float interval = Mathf.Lerp(
                maximumEmissionInterval,
                minimumEmissionInterval,
                Mathf.SmoothStep(0f, 1f, strength)
            );
            nextWakeTime = Time.time + interval;

            Vector3 sternCenter =
                transform.position -
                transform.forward * sternOffset +
                Vector3.up * waterlineOffset;
            Vector3 sideOffset =
                transform.right * wakeHalfWidth;
            Vector3 emissionPosition =
                sternCenter +
                (emitPortNext ? -sideOffset : sideOffset);
            emitPortNext = !emitPortNext;

            PrototypeCombatVfx.PlayShipWake(
                emissionPosition,
                -transform.forward,
                strength
            );
        }
    }
}
