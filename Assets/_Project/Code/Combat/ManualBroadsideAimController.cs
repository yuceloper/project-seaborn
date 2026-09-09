using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Combat
{
    [RequireComponent(typeof(BroadsideController))]
    public sealed class ManualBroadsideAimController :
        MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private InputActionReference aimAction;

        [SerializeField]
        private InputActionReference fireAction;

        [Header("References")]
        [SerializeField]
        private UnityEngine.Camera aimCamera;

        [SerializeField]
        private BroadsideController broadsideController;

        [Header("Aim Geometry")]
        [SerializeField]
        private float aimPlaneHeight = 0.75f;

        [SerializeField, Min(0f)]
        private float minimumRange = 2f;

        [SerializeField, Range(1f, 89f)]
        private float firingHalfAngle = 48f;

        [Header("Preparation")]
        [SerializeField, Min(0.1f)]
        private float fullPreparationTime = 1f;

        [SerializeField, Min(0.1f)]
        private float aimFollowSpeed = 8f;

        [SerializeField, Min(0.1f)]
        private float maximumTrackingError = 3f;

        public event Action AimStateChanged;
        public event Action AimUpdated;

        public bool IsAiming { get; private set; }

        public Vector3 DesiredAimPoint { get; private set; }

        public Vector3 CurrentAimPoint { get; private set; }

        public float AimReadiness { get; private set; }

        public BroadsideSide SelectedBroadside
        {
            get;
            private set;
        }

        public bool IsInRange { get; private set; }

        public bool IsInsideFiringArc { get; private set; }

        public float CooldownRemaining =>
            broadsideController != null
                ? broadsideController.GetCooldownRemaining(
                    SelectedBroadside)
                : 0f;

        public bool IsCoolingDown =>
            CooldownRemaining > 0f;

        public float CooldownProgress
        {
            get
            {
                if (broadsideController == null ||
                    broadsideController.CooldownDuration <= 0f)
                {
                    return 1f;
                }

                return 1f - Mathf.Clamp01(
                    CooldownRemaining /
                    broadsideController.CooldownDuration
                );
            }
        }

        public bool CanFire =>
            IsAiming &&
            IsInRange &&
            IsInsideFiringArc &&
            !IsCoolingDown;

        private float preparation;
        private bool hasCurrentAimPoint;

        private void Reset()
        {
            broadsideController =
                GetComponent<BroadsideController>();
            aimCamera = UnityEngine.Camera.main;
        }

        private void Awake()
        {
            if (broadsideController == null)
            {
                broadsideController =
                    GetComponent<BroadsideController>();
            }

            if (aimCamera == null)
            {
                aimCamera = UnityEngine.Camera.main;
            }
        }

        private void OnEnable()
        {
            aimAction?.action.Enable();
            fireAction?.action.Enable();
        }

        private void OnDisable()
        {
            aimAction?.action.Disable();
            fireAction?.action.Disable();
            SetAiming(false);
        }

        private void Update()
        {
            bool wantsToAim =
                aimAction != null &&
                aimAction.action.IsPressed();

            SetAiming(wantsToAim);

            if (!IsAiming)
            {
                return;
            }

            UpdateAim();

            if (fireAction != null &&
                fireAction.action.WasPressedThisFrame())
            {
                Fire();
            }
        }

        private void SetAiming(bool value)
        {
            if (IsAiming == value)
            {
                return;
            }

            IsAiming = value;
            preparation = 0f;
            AimReadiness = 0f;

            if (!value)
            {
                hasCurrentAimPoint = false;
                IsInRange = false;
                IsInsideFiringArc = false;
            }

            AimStateChanged?.Invoke();
        }

        private void UpdateAim()
        {
            if (!TryGetPointerPoint(out Vector3 pointerPoint))
            {
                IsInRange = false;
                IsInsideFiringArc = false;
                AimReadiness = 0f;
                AimUpdated?.Invoke();
                return;
            }

            Vector3 fromShip =
                pointerPoint - transform.position;
            fromShip.y = 0f;

            float rawDistance = fromShip.magnitude;

            if (rawDistance <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 direction = fromShip / rawDistance;
            SelectedBroadside =
                Vector3.Dot(transform.right, direction) >= 0f
                    ? BroadsideSide.Starboard
                    : BroadsideSide.Port;

            float maximumRange =
                broadsideController.MaximumRange;

            IsInRange =
                rawDistance >= minimumRange &&
                rawDistance <= maximumRange;

            float clampedDistance = Mathf.Clamp(
                rawDistance,
                minimumRange,
                maximumRange
            );

            DesiredAimPoint =
                transform.position +
                direction * clampedDistance;
            DesiredAimPoint =
                new Vector3(
                    DesiredAimPoint.x,
                    aimPlaneHeight,
                    DesiredAimPoint.z
                );

            Vector3 broadsideDirection =
                SelectedBroadside ==
                BroadsideSide.Starboard
                    ? transform.right
                    : -transform.right;

            float minimumAlignment =
                Mathf.Cos(
                    firingHalfAngle *
                    Mathf.Deg2Rad
                );

            IsInsideFiringArc =
                Vector3.Dot(
                    broadsideDirection,
                    direction
                ) >= minimumAlignment;

            if (!hasCurrentAimPoint)
            {
                CurrentAimPoint = transform.position;
                CurrentAimPoint =
                    new Vector3(
                        CurrentAimPoint.x,
                        aimPlaneHeight,
                        CurrentAimPoint.z
                    );
                hasCurrentAimPoint = true;
            }

            CurrentAimPoint = Vector3.MoveTowards(
                CurrentAimPoint,
                DesiredAimPoint,
                aimFollowSpeed * Time.deltaTime
            );

            if (IsInRange && IsInsideFiringArc)
            {
                preparation = Mathf.Clamp01(
                    preparation +
                    Time.deltaTime /
                    fullPreparationTime
                );
            }
            else
            {
                preparation = 0f;
            }

            float trackingError = Vector3.Distance(
                CurrentAimPoint,
                DesiredAimPoint
            );
            float trackingQuality = 1f - Mathf.Clamp01(
                trackingError / maximumTrackingError
            );

            AimReadiness =
                preparation * trackingQuality;

            AimUpdated?.Invoke();
        }

        private bool TryGetPointerPoint(
            out Vector3 pointerPoint)
        {
            pointerPoint = default;

            if (aimCamera == null ||
                Mouse.current == null)
            {
                return false;
            }

            Vector2 screenPosition =
                Mouse.current.position.ReadValue();
            Ray pointerRay =
                aimCamera.ScreenPointToRay(screenPosition);

            Plane aimPlane = new Plane(
                Vector3.up,
                new Vector3(
                    0f,
                    aimPlaneHeight,
                    0f
                )
            );

            if (!aimPlane.Raycast(
                    pointerRay,
                    out float enter))
            {
                return false;
            }

            pointerPoint = pointerRay.GetPoint(enter);
            return true;
        }

        private void Fire()
        {
            if (!CanFire)
            {
                return;
            }

            if (broadsideController.TryFireAt(
                    SelectedBroadside,
                    CurrentAimPoint,
                    AimReadiness))
            {
                preparation = 0f;
                AimReadiness = 0f;
            }
        }
    }
}
