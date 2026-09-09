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
                ? moveInput.y * maxForwardSpeed
                : moveInput.y * maxReverseSpeed;

            float currentForwardSpeed = Vector3.Dot(
                shipRigidbody.linearVelocity,
                transform.forward);

            float speedDifference = requestedSpeed - currentForwardSpeed;

            shipRigidbody.AddForce(
                transform.forward * speedDifference * acceleration,
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
                Mathf.Clamp01(forwardSpeed / maxForwardSpeed));

            float rotationAmount =
                moveInput.x *
                turnSpeed *
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