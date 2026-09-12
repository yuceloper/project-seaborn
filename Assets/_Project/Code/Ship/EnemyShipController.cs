using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Ship
{
    public enum EnemyShipArchetype
    {
        Skirmisher,
        Gunship,
        Marauder
    }

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

        [SerializeField, Min(0f)]
        private float initialReactionDelay = 2f;

        [SerializeField, Min(0.1f)]
        private float aimPreparationTime = 1.1f;

        [SerializeField, Range(0f, 1f)]
        private float movementPrediction = 0.65f;

        public EnemyShipArchetype Archetype
        {
            get;
            private set;
        } = EnemyShipArchetype.Marauder;

        public float AimPreparation =>
            Mathf.Clamp01(
                aimPreparation /
                Mathf.Max(0.1f, aimPreparationTime)
            );

        public bool IsPreparingShot =>
            aimPreparation > 0f;

        public Vector3 PredictedAimPoint
        {
            get;
            private set;
        }

        private Rigidbody shipRigidbody;
        private BroadsideController broadsideController;
        private ShipHealth shipHealth;
        private ShipSubsystemController subsystems;
        private float engagementStartTime;
        private float aimPreparation;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            broadsideController =
                GetComponent<BroadsideController>();
            shipHealth = GetComponent<ShipHealth>();
            subsystems =
                ShipSubsystemController.EnsureAttached(
                    transform
                );
            engagementStartTime = Time.time;
        }

        public void ConfigureArchetype(
            EnemyShipArchetype archetype)
        {
            Archetype = archetype;

            switch (archetype)
            {
                case EnemyShipArchetype.Skirmisher:
                    name = "Razorwind Skirmisher";
                    detectionRange = 25f;
                    preferredRange = 6.2f;
                    retreatDistance = 3.6f;
                    forwardSpeed = 4.5f;
                    broadsideSpeed = 2.5f;
                    turnSpeed = 62f;
                    fireRange = 8.5f;
                    fireAlignment = 0.76f;
                    aimPreparationTime = 0.78f;
                    movementPrediction = 0.9f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        0.72f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Chain);
                    ApplyPalette(
                        new Color(0.12f, 0.26f, 0.3f),
                        new Color(0.28f, 0.58f, 0.62f),
                        new Color(0.18f, 0.72f, 0.78f));
                    break;

                case EnemyShipArchetype.Gunship:
                    name = "Ironwake Gunship";
                    detectionRange = 27f;
                    preferredRange = 9f;
                    retreatDistance = 6f;
                    forwardSpeed = 2.2f;
                    broadsideSpeed = 1.05f;
                    turnSpeed = 27f;
                    fireRange = 12f;
                    fireAlignment = 0.86f;
                    aimPreparationTime = 1.55f;
                    movementPrediction = 0.52f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        1.4f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Standard);
                    ApplyPalette(
                        new Color(0.19f, 0.12f, 0.1f),
                        new Color(0.52f, 0.38f, 0.25f),
                        new Color(0.82f, 0.5f, 0.16f));
                    break;

                default:
                    name = "Saltfang Marauder";
                    detectionRange = 22f;
                    preferredRange = 5.5f;
                    retreatDistance = 3.5f;
                    forwardSpeed = 3.5f;
                    broadsideSpeed = 1.8f;
                    turnSpeed = 44f;
                    fireRange = 7.2f;
                    fireAlignment = 0.74f;
                    aimPreparationTime = 0.62f;
                    movementPrediction = 0.72f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        0.9f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Grapeshot);
                    ApplyPalette(
                        new Color(0.28f, 0.08f, 0.07f),
                        new Color(0.52f, 0.15f, 0.12f),
                        new Color(0.78f, 0.12f, 0.08f));
                    break;
            }

            engagementStartTime = Time.time;
        }

        private void ApplyPalette(
            Color hull,
            Color sail,
            Color accent)
        {
            PrototypeShipVisual visual =
                GetComponent<PrototypeShipVisual>();
            visual?.ApplyEnemyPalette(
                hull,
                sail,
                accent
            );
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
                aimPreparation = 0f;
                StopMoving();
                return;
            }

            Vector3 targetDirection =
                toTarget / distance;

            Navigate(
                targetDirection,
                distance
            );
            PrepareAndFire(
                targetDirection,
                distance
            );
        }

        private void Navigate(
            Vector3 targetDirection,
            float distance)
        {
            if (distance < retreatDistance)
            {
                SteerAndMove(
                    -targetDirection,
                    forwardSpeed
                );
                return;
            }

            if (distance > preferredRange)
            {
                SteerAndMove(
                    targetDirection,
                    forwardSpeed
                );
                return;
            }

            Vector3 tangent = Vector3.Cross(
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

        private void PrepareAndFire(
            Vector3 targetDirection,
            float distance)
        {
            float sideAlignment = Vector3.Dot(
                transform.right,
                targetDirection
            );

            bool hasFiringSolution =
                distance <= fireRange &&
                Mathf.Abs(sideAlignment) >= fireAlignment &&
                Time.time - engagementStartTime >=
                initialReactionDelay;

            if (!hasFiringSolution)
            {
                aimPreparation = Mathf.Max(
                    0f,
                    aimPreparation -
                    Time.fixedDeltaTime
                );
                return;
            }

            PredictedAimPoint = target.position;

            Rigidbody trackedTargetRigidbody =
                target.GetComponent<Rigidbody>();

            if (trackedTargetRigidbody != null)
            {
                PredictedAimPoint +=
                    trackedTargetRigidbody.linearVelocity *
                    movementPrediction;
            }

            PredictedAimPoint =
                new Vector3(
                    PredictedAimPoint.x,
                    transform.position.y + 0.25f,
                    PredictedAimPoint.z
                );

            aimPreparation += Time.fixedDeltaTime;

            if (aimPreparation < aimPreparationTime)
            {
                return;
            }

            BroadsideSide side =
                sideAlignment >= 0f
                    ? BroadsideSide.Starboard
                    : BroadsideSide.Port;

            Vector3 predictedTarget =
                target.position;

            Rigidbody targetRigidbody =
                target.GetComponent<Rigidbody>();

            if (targetRigidbody != null)
            {
                predictedTarget +=
                    targetRigidbody.linearVelocity *
                    movementPrediction;
            }

            predictedTarget.y =
                transform.position.y + 0.25f;
            PredictedAimPoint = predictedTarget;

            if (broadsideController.TryFireAt(
                    side,
                    predictedTarget,
                    0.82f))
            {
                aimPreparation = 0f;
            }
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
                    turnSpeed *
                    (subsystems != null
                        ? subsystems.TurnMultiplier
                        : 1f) *
                    Time.fixedDeltaTime
                );

            shipRigidbody.MoveRotation(nextRotation);
            shipRigidbody.linearVelocity =
                nextRotation *
                Vector3.forward *
                speed *
                (subsystems != null
                    ? subsystems.MovementSpeedMultiplier
                    : 1f);
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
