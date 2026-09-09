using Seaborn.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Camera
{
    public sealed class TacticalShipCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody targetRigidbody;

        [Header("View")]
        [SerializeField, Range(25f, 80f)] private float pitch = 55f;
        [SerializeField, Range(-180f, 180f)] private float yaw = 45f;
        [SerializeField] private float targetHeight = 1f;

        [Header("Aim Focus")]
        [SerializeField]
        private ManualBroadsideAimController aimController;

        [SerializeField, Range(0f, 1f)]
        private float aimFocusWeight = 0.35f;

        [SerializeField, Min(0f)]
        private float maximumAimFocusOffset = 4f;

        [Header("Distance")]
        [SerializeField] private InputActionReference zoomAction;
        [SerializeField] private float defaultDistance = 20f;
        [SerializeField] private float minimumDistance = 11f;
        [SerializeField] private float maximumDistance = 28f;
        [SerializeField] private float zoomStep = 2.5f;
        [SerializeField] private float zoomSmoothTime = 0.12f;

        [Header("Movement")]
        [SerializeField] private float positionSmoothTime = 0.18f;
        [SerializeField] private float lookAheadTime = 0.45f;
        [SerializeField] private float maximumLookAhead = 4f;

        private Vector3 smoothedFocusPoint;
        private Vector3 focusVelocity;

        private float targetDistance;
        private float currentDistance;
        private float distanceVelocity;

        private void Awake()
        {
            targetDistance = defaultDistance;
            currentDistance = defaultDistance;
        }

        private void OnEnable()
        {
            zoomAction.action.Enable();
        }

        private void OnDisable()
        {
            zoomAction.action.Disable();
        }

        private void Start()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }

            smoothedFocusPoint = CalculateDesiredFocusPoint();
            transform.position = CalculateCameraPosition(
                smoothedFocusPoint,
                currentDistance
            );

            LookAtFocusPoint();
        }

        private void LateUpdate()
        {
            UpdateZoom();
            UpdateFocusPoint();
            UpdateDistance();

            transform.position = CalculateCameraPosition(
                smoothedFocusPoint,
                currentDistance
            );

            LookAtFocusPoint();
        }

        private void UpdateZoom()
        {
            float zoomInput = zoomAction.action.ReadValue<float>();

            if (Mathf.Abs(zoomInput) < 0.01f)
            {
                return;
            }

            targetDistance -= Mathf.Sign(zoomInput) * zoomStep;
            targetDistance = Mathf.Clamp(
                targetDistance,
                minimumDistance,
                maximumDistance
            );
        }

        private void UpdateFocusPoint()
        {
            Vector3 desiredFocusPoint = CalculateDesiredFocusPoint();

            smoothedFocusPoint = Vector3.SmoothDamp(
                smoothedFocusPoint,
                desiredFocusPoint,
                ref focusVelocity,
                positionSmoothTime
            );
        }

        private void UpdateDistance()
        {
            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                targetDistance,
                ref distanceVelocity,
                zoomSmoothTime
            );
        }

        private Vector3 CalculateDesiredFocusPoint()
        {
            Vector3 focusPoint =
                target.position +
                Vector3.up * targetHeight;

            if (targetRigidbody != null)
            {
                Vector3 lookAhead =
                    targetRigidbody.linearVelocity *
                    lookAheadTime;

                lookAhead.y = 0f;
                lookAhead = Vector3.ClampMagnitude(
                    lookAhead,
                    maximumLookAhead
                );

                focusPoint += lookAhead;
            }

            if (aimController == null ||
                !aimController.IsAiming)
            {
                return focusPoint;
            }

            Vector3 aimOffset =
                aimController.CurrentAimPoint -
                target.position;
            aimOffset.y = 0f;
            aimOffset = Vector3.ClampMagnitude(
                aimOffset,
                maximumAimFocusOffset /
                Mathf.Max(0.01f, aimFocusWeight)
            );

            return focusPoint +
                aimOffset * aimFocusWeight;
        }

        private Vector3 CalculateCameraPosition(
            Vector3 focusPoint,
            float cameraDistance)
        {
            Quaternion viewRotation =
                Quaternion.Euler(pitch, yaw, 0f);

            Vector3 offset =
                viewRotation *
                Vector3.back *
                cameraDistance;

            return focusPoint + offset;
        }

        private void LookAtFocusPoint()
        {
            Vector3 direction =
                smoothedFocusPoint -
                transform.position;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                direction,
                Vector3.up
            );
        }
    }
}