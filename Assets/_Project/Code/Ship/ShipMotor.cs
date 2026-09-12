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

        [Header("Steering")]
        [SerializeField, Min(0f)] private float turnSpeed = 45f;
        [SerializeField, Range(0f, 1f)] private float stationarySteering = 0.15f;

        [Header("Water Resistance")]
        [SerializeField, Min(0f)] private float lateralResistance = 2.5f;

        private Rigidbody shipRigidbody;
        private Vector2 moveInput;
        private float speedMultiplier = 1f;
        private float accelerationMultiplier = 1f;
        private float turnMultiplier = 1f;
        private float damageSpeedMultiplier = 1f;
        private float damageTurnMultiplier = 1f;

        public float SpeedMultiplier =>
            speedMultiplier * damageSpeedMultiplier;
        public float TurnMultiplier =>
            turnMultiplier * damageTurnMultiplier;

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
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
        }

        private void Update()
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            ApplyForwardMovement();
            ApplySteering();
            ApplyLateralResistance();
        }

        private void ApplyForwardMovement()
        {
            float requestedSpeed = moveInput.y >= 0f
                ? moveInput.y * maxForwardSpeed * SpeedMultiplier
                : moveInput.y * maxReverseSpeed * SpeedMultiplier;

            float currentForwardSpeed = Vector3.Dot(
                shipRigidbody.linearVelocity,
                transform.forward);

            float speedDifference = requestedSpeed - currentForwardSpeed;

            shipRigidbody.AddForce(
                transform.forward * speedDifference *
                acceleration * accelerationMultiplier,
                ForceMode.Acceleration);
        }

        private void ApplySteering()
        {
            float forwardSpeed = Mathf.Abs(Vector3.Dot(
                shipRigidbody.linearVelocity,
                transform.forward));

            float steeringAuthority = Mathf.Lerp(
                stationarySteering,
                1f,
                Mathf.Clamp01(
                    forwardSpeed /
                    (maxForwardSpeed * SpeedMultiplier)));

            float rotationAmount =
                moveInput.x *
                turnSpeed *
                TurnMultiplier *
                steeringAuthority *
                Time.fixedDeltaTime;

            Quaternion targetRotation =
                shipRigidbody.rotation *
                Quaternion.Euler(0f, rotationAmount, 0f);

            shipRigidbody.MoveRotation(targetRotation);
        }

        private void ApplyLateralResistance()
        {
            Vector3 lateralVelocity =
                Vector3.Project(
                    shipRigidbody.linearVelocity,
                    transform.right);

            shipRigidbody.AddForce(
                -lateralVelocity * lateralResistance,
                ForceMode.Acceleration);
        }
    }
}