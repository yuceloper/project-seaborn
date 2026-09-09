using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BroadsideController))]
    [RequireComponent(typeof(ShipHealth))]
    public sealed class EnemyShipController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [SerializeField, Min(0f)]
        private float detectionRange = 22f;

        [Header("Navigation")]
        [SerializeField, Min(0.1f)]
        private float preferredRange = 7f;

        [SerializeField, Min(0.1f)]
        private float retreatDistance = 4.5f;

        [SerializeField, Min(0f)]
        private float forwardSpeed = 3f;

        [SerializeField, Min(0f)]
        private float broadsideSpeed = 1.5f;

        [SerializeField, Min(0f)]
        private float turnSpeed = 38f;

        [Header("Combat")]
        [SerializeField, Range(0f, 1f)]
        private float fireAlignment = 0.82f;

        [SerializeField, Min(0f)]
        private float fireRange = 10f;

        private Rigidbody shipRigidbody;
        private BroadsideController broadsideController;
        private ShipHealth shipHealth;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            broadsideController =
                GetComponent<BroadsideController>();
            shipHealth = GetComponent<ShipHealth>();
        }

        private void FixedUpdate()
        {
            if (target == null || shipHealth.IsSunk)
            {
                StopMoving();
                return;
            }

            Vector3 toTarget =
                target.position - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;

            if (distance > detectionRange ||
                distance <= Mathf.Epsilon)
            {
                StopMoving();
                return;
            }

            Vector3 targetDirection =
                toTarget / distance;

            if (distance < retreatDistance)
            {
                SteerAndMove(
                    -targetDirection,
                    forwardSpeed
                );
            }
            else if (distance > preferredRange)
            {
                SteerAndMove(
                    targetDirection,
                    forwardSpeed
                );
            }
            else
            {
                Vector3 tangent =
                    Vector3.Cross(
                        Vector3.up,
                        targetDirection
                    );

                if (Vector3.Dot(
                        tangent,
                        transform.forward) < 0f)
                {
                    tangent = -tangent;
                }

                SteerAndMove(
                    tangent,
                    broadsideSpeed
                );
            }

            TryFire(
                targetDirection,
                distance
            );
        }

        private void SteerAndMove(
            Vector3 desiredForward,
            float speed)
        {
            if (desiredForward.sqrMagnitude <= Mathf.Epsilon)
            {
                StopMoving();
                return;
            }

            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    desiredForward,
                    Vector3.up
                );

            Quaternion nextRotation =
                Quaternion.RotateTowards(
                    shipRigidbody.rotation,
                    desiredRotation,
                    turnSpeed * Time.fixedDeltaTime
                );

            shipRigidbody.MoveRotation(nextRotation);
            shipRigidbody.linearVelocity =
                nextRotation *
                Vector3.forward *
                speed;
        }

        private void TryFire(
            Vector3 targetDirection,
            float distance)
        {
            if (distance > fireRange)
            {
                return;
            }

            float sideAlignment =
                Vector3.Dot(
                    transform.right,
                    targetDirection
                );

            if (Mathf.Abs(sideAlignment) < fireAlignment)
            {
                return;
            }

            BroadsideSide side =
                sideAlignment >= 0f
                    ? BroadsideSide.Starboard
                    : BroadsideSide.Port;

            broadsideController.TryFire(side);
        }

        private void StopMoving()
        {
            shipRigidbody.linearVelocity = Vector3.zero;
            shipRigidbody.angularVelocity = Vector3.zero;
        }

        private void OnValidate()
        {
            preferredRange = Mathf.Max(
                preferredRange,
                retreatDistance + 0.1f
            );
            fireRange = Mathf.Max(
                fireRange,
                preferredRange
            );
            detectionRange = Mathf.Max(
                detectionRange,
                fireRange
            );
        }
    }
}
