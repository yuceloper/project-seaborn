using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipMotor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;

        [Header("Speed")]
        [SerializeField, Min(0f)] private float maxForwardSpeed = 10f;
        [SerializeField, Min(0f)] private float maxReverseSpeed = 3f;
        [SerializeField, Min(0f)] private float acceleration = 2.5f;
        [SerializeField, Min(0f)] private float deceleration = 1.8f;
        [SerializeField, Range(0.05f, 0.6f)]
        private float slowAheadRatio = 0.3f;
        [SerializeField, Range(0.4f, 0.85f)]
        private float halfAheadRatio = 0.65f;

        [Header("Steering")]
        [SerializeField, Min(0f)] private float turnSpeed = 45f;
        [SerializeField, Range(0f, 1f)] private float stationarySteering = 0.04f;
        [SerializeField, Min(0.1f)] private float rudderResponse = 1.8f;
        [SerializeField, Min(0.1f)] private float rudderReturnSpeed = 2.4f;
        [SerializeField, Range(0.4f, 1f)]
        private float fullSpeedTurnRetention = 0.72f;
        [SerializeField, Range(10f, 45f)]
        private float maximumRudderAngle = 35f;

        [Header("Water Resistance")]
        [SerializeField, Min(0f)] private float lateralResistance = 2.5f;

        [Header("Collision Stability")]
        [SerializeField, Min(0f)] private float angularRecovery = 12f;
        [SerializeField, Min(0f)] private float collisionRecoveryDuration = 0.65f;
        [SerializeField, Range(0f, 1f)] private float collisionVelocityRetention = 0.45f;
        [SerializeField, Min(0f)] private float maximumImpactDrift = 3f;

        private Rigidbody shipRigidbody;
        private Vector2 moveInput;
        private float previousThrottleAxis;
        private float rudder;
        private int sailingOrder;
        private float speedMultiplier = 1f;
        private float accelerationMultiplier = 1f;
        private float turnMultiplier = 1f;
        private float damageSpeedMultiplier = 1f;
        private float damageTurnMultiplier = 1f;
        private float skillSpeedMultiplier = 1f;
        private float skillTurnMultiplier = 1f;
        private float consumableSpeedMultiplier = 1f;
        private float consumableTurnMultiplier = 1f;
        private float repairSpeedMultiplier = 1f;
        private float repairTurnMultiplier = 1f;

        private float collisionRecoveryUntil;

        public int SailingOrder => sailingOrder;
        public float RudderNormalized => rudder;
        public float RudderAngleDegrees =>
            rudder * maximumRudderAngle;
        public float CurrentForwardSpeed =>
            shipRigidbody != null
                ? Vector3.Dot(
                    shipRigidbody.linearVelocity,
                    transform.forward)
                : 0f;
        public float TargetForwardSpeed =>
            GetTargetForwardSpeed();
        public string SailingOrderLabel => sailingOrder switch
        {
            -1 => "TORNİSTAN",
            0 => "DUR",
            1 => "AĞIR YOL",
            2 => "YARIM YOL",
            3 => "TAM YOL",
            _ => "DUR"
        };

        public float SpeedMultiplier =>
            speedMultiplier *
            damageSpeedMultiplier *
            skillSpeedMultiplier *
            consumableSpeedMultiplier *
            repairSpeedMultiplier;
        public float TurnMultiplier =>
            turnMultiplier *
            damageTurnMultiplier *
            skillTurnMultiplier *
            consumableTurnMultiplier *
            repairTurnMultiplier;

        public void RefreshInputBindings()
        {
            if (moveAction == null ||
                moveAction.action == null)
            {
                return;
            }

            InputActionAsset actions =
                moveAction.action.actionMap?.asset;
            if (actions != null)
            {
                actions.Enable();
            }
            else
            {
                moveAction.action.Enable();
            }
        }

        public void SetRuntimePerformance(
            float speed,
            float accelerationRate,
            float turning)
        {
            speedMultiplier = Mathf.Max(0.1f, speed);
            accelerationMultiplier =
                Mathf.Max(0.1f, accelerationRate);
            turnMultiplier = Mathf.Max(0.1f, turning);
        }

        public void SetSkillPerformance(
            float speed,
            float turning)
        {
            skillSpeedMultiplier =
                Mathf.Max(0.1f, speed);
            skillTurnMultiplier =
                Mathf.Max(0.1f, turning);
        }

        public void SetConsumablePerformance(
            float speed,
            float turning)
        {
            consumableSpeedMultiplier =
                Mathf.Max(0.1f, speed);
            consumableTurnMultiplier =
                Mathf.Max(0.1f, turning);
        }

        public void SetRepairPerformance(
            float speed,
            float turning)
        {
            repairSpeedMultiplier =
                Mathf.Clamp(speed, 0.1f, 1f);
            repairTurnMultiplier =
                Mathf.Clamp(turning, 0.1f, 1f);
        }

        public void SetDamagePerformance(
            float speed,
            float turning)
        {
            damageSpeedMultiplier =
                Mathf.Clamp(speed, 0.25f, 1f);
            damageTurnMultiplier =
                Mathf.Clamp(turning, 0.4f, 1f);
        }

        public void ResetRuntimePerformance()
        {
            speedMultiplier = 1f;
            accelerationMultiplier = 1f;
            turnMultiplier = 1f;
        }

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            shipRigidbody.constraints |=
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null &&
                moveAction.action != null)
            {
                moveAction.action.Disable();
            }

            sailingOrder = 0;
            previousThrottleAxis = 0f;
            rudder = 0f;
            moveInput = Vector2.zero;
        }

        private void Update()
        {
            if (moveAction == null ||
                moveAction.action == null)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput =
                moveAction.action.ReadValue<Vector2>();
            UpdateSailingOrder(moveInput.y);
        }

        private void UpdateSailingOrder(
            float throttleAxis)
        {
            const float threshold = 0.5f;

            if (throttleAxis >= threshold &&
                previousThrottleAxis < threshold)
            {
                sailingOrder = Mathf.Min(
                    3,
                    sailingOrder + 1
                );
            }
            else if (throttleAxis <= -threshold &&
                     previousThrottleAxis > -threshold)
            {
                sailingOrder = Mathf.Max(
                    -1,
                    sailingOrder - 1
                );
            }

            previousThrottleAxis = throttleAxis;
        }

        private void FixedUpdate()
        {
            ApplyCollisionStability();
            ApplyForwardMovement();
            ApplySteering();
            ApplyLateralResistance();
            ClampPlanarVelocity();
        }

        private void ApplyForwardMovement()
        {
            float requestedSpeed =
                GetTargetForwardSpeed();
            float currentForwardSpeed =
                CurrentForwardSpeed;

            bool slowing =
                Mathf.Abs(requestedSpeed) <
                    Mathf.Abs(currentForwardSpeed) ||
                Mathf.Sign(requestedSpeed) !=
                    Mathf.Sign(currentForwardSpeed);
            float response = slowing
                ? deceleration
                : acceleration;
            response *= accelerationMultiplier;

            float nextForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                requestedSpeed,
                response * Time.fixedDeltaTime
            );
            float velocityChange =
                nextForwardSpeed - currentForwardSpeed;

            shipRigidbody.AddForce(
                transform.forward * velocityChange,
                ForceMode.VelocityChange
            );
        }

        private float GetTargetForwardSpeed()
        {
            float ratio = sailingOrder switch
            {
                -1 => -maxReverseSpeed /
                    Mathf.Max(0.01f, maxForwardSpeed),
                1 => slowAheadRatio,
                2 => halfAheadRatio,
                3 => 1f,
                _ => 0f
            };

            return ratio *
                maxForwardSpeed *
                SpeedMultiplier;
        }

        private void ApplySteering()
        {
            float targetRudder =
                Mathf.Abs(moveInput.x) > 0.08f
                    ? Mathf.Clamp(moveInput.x, -1f, 1f)
                    : 0f;
            float rudderRate =
                Mathf.Abs(targetRudder) > 0.01f
                    ? rudderResponse
                    : rudderReturnSpeed;
            rudder = Mathf.MoveTowards(
                rudder,
                targetRudder,
                rudderRate * Time.fixedDeltaTime
            );

            float forwardSpeed =
                Mathf.Abs(CurrentForwardSpeed);
            float normalizedSpeed = Mathf.Clamp01(
                forwardSpeed /
                Mathf.Max(
                    0.01f,
                    maxForwardSpeed * SpeedMultiplier)
            );
            float waterFlowAuthority = Mathf.Lerp(
                stationarySteering,
                1f,
                normalizedSpeed
            );
            float highSpeedPenalty = Mathf.Lerp(
                1f,
                fullSpeedTurnRetention,
                Mathf.InverseLerp(
                    0.65f,
                    1f,
                    normalizedSpeed)
            );
            float reverseDirection =
                CurrentForwardSpeed < -0.1f
                    ? -1f
                    : 1f;

            float rotationAmount =
                rudder *
                reverseDirection *
                turnSpeed *
                TurnMultiplier *
                waterFlowAuthority *
                highSpeedPenalty *
                Time.fixedDeltaTime;

            Quaternion targetRotation =
                shipRigidbody.rotation *
                Quaternion.Euler(
                    0f,
                    rotationAmount,
                    0f
                );

            shipRigidbody.MoveRotation(targetRotation);
        }

        private void ApplyLateralResistance()
        {
            Vector3 lateralVelocity =
                Vector3.Project(
                    shipRigidbody.linearVelocity,
                    transform.right);

            float recoveryBoost =
                Time.time < collisionRecoveryUntil ? 2.5f : 1f;

            shipRigidbody.AddForce(
                -lateralVelocity *
                lateralResistance *
                recoveryBoost,
                ForceMode.Acceleration);
        }

        private void ApplyCollisionStability()
        {
            Vector3 angularVelocity =
                shipRigidbody.angularVelocity;

            angularVelocity.x = 0f;
            angularVelocity.z = 0f;
            angularVelocity.y = Mathf.MoveTowards(
                angularVelocity.y,
                0f,
                angularRecovery * Time.fixedDeltaTime
            );

            if (Time.time < collisionRecoveryUntil)
            {
                angularVelocity.y *= 0.2f;
            }

            shipRigidbody.angularVelocity = angularVelocity;
        }

        private void ClampPlanarVelocity()
        {
            Vector3 velocity = shipRigidbody.linearVelocity;
            Vector3 planarVelocity =
                Vector3.ProjectOnPlane(velocity, Vector3.up);

            float maximumPlanarSpeed =
                maxForwardSpeed * SpeedMultiplier +
                maximumImpactDrift;

            if (planarVelocity.sqrMagnitude >
                maximumPlanarSpeed * maximumPlanarSpeed)
            {
                planarVelocity = planarVelocity.normalized *
                    maximumPlanarSpeed;
                shipRigidbody.linearVelocity =
                    planarVelocity +
                    Vector3.up * velocity.y;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contactCount <= 0) return;

            collisionRecoveryUntil =
                Time.time + collisionRecoveryDuration;

            Vector3 velocity = shipRigidbody.linearVelocity;
            Vector3 planarVelocity =
                Vector3.ProjectOnPlane(velocity, Vector3.up) *
                collisionVelocityRetention;

            shipRigidbody.linearVelocity =
                planarVelocity + Vector3.up * velocity.y;
            shipRigidbody.angularVelocity = Vector3.zero;
        }
    }
}