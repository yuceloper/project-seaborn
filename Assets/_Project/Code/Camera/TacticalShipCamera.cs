using Seaborn.Combat;
using Seaborn.Harbor;
using Seaborn.Harbor.UI;
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
        [SerializeField, Min(0f)] private float speedDistanceBoost = 1.8f;
        [SerializeField, Min(0.1f)] private float speedForMaximumBoost = 10f;
        [SerializeField] private float zoomSmoothTime = 0.12f;

        [Header("Movement")]
        [SerializeField] private float positionSmoothTime = 0.18f;
        [SerializeField] private float lookAheadTime = 0.45f;
        [SerializeField] private float maximumLookAhead = 4f;

        private float shipyardBlend;
        private float inspectionDistance = 10f;
        private float nextInspectionMeasure;
        private UnityEngine.Camera viewCamera;
        public bool IsInspectingShip => shipyardBlend > 0.01f;

        private bool WantsShipyardView => target != null &&
            Seaborn.World.PrototypeExpeditionRegionDirector.IsHarborScene &&
            PrototypeHarborDockingDirector.Instance != null &&
            PrototypeHarborDockingDirector.Instance.IsDockedAt(PrototypeHarborStation.Shipyard) &&
            PrototypeHarborUiCoordinator.IsSelected(PrototypeHarborTab.Loadout);

        private Vector3 smoothedFocusPoint;
        private Vector3 focusVelocity;

        private float targetDistance;
        private float currentDistance;
        private float distanceVelocity;

        private void Awake()
        {
            viewCamera = GetComponent<UnityEngine.Camera>();
            targetDistance = defaultDistance;
            currentDistance = defaultDistance;
        }

        private void OnEnable()
        {
            zoomAction?.action.Enable();
        }

        private void OnDisable()
        {
            zoomAction?.action.Disable();
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
            if (target == null) return;
            UpdateZoom();
            UpdateFocusPoint();
            UpdateDistance();
            Vector3 sailingPosition = CalculateCameraPosition(smoothedFocusPoint, currentDistance);
            Quaternion sailingRotation = Quaternion.LookRotation(smoothedFocusPoint - sailingPosition, Vector3.up);
            bool inspecting = WantsShipyardView;
            shipyardBlend = Mathf.MoveTowards(shipyardBlend, inspecting ? 1f : 0f, Time.unscaledDeltaTime / 0.85f);
            if (shipyardBlend <= 0f)
            {
                transform.SetPositionAndRotation(sailingPosition, sailingRotation);
                return;
            }
            Quaternion inspectionRotation = Quaternion.Euler(72f, target.eulerAngles.y, 0f);
            if (inspecting && Time.unscaledTime >= nextInspectionMeasure)
            {
                nextInspectionMeasure = Time.unscaledTime + 0.5f;
                inspectionDistance = MeasureInspectionDistance(inspectionRotation);
            }
            float tangent = Mathf.Tan((viewCamera != null ? viewCamera.fieldOfView : 60f) * 0.5f * Mathf.Deg2Rad);
            float aspect = viewCamera != null ? viewCamera.aspect : (float)Screen.width / Mathf.Max(1, Screen.height);
            // Place the ship in the open left portion; the inventory occupies the right.
            float horizontalSpan = viewCamera != null && viewCamera.orthographic
                ? viewCamera.orthographicSize * aspect : inspectionDistance * tangent * aspect;
            Vector3 focus = target.position + Vector3.up * targetHeight +
                inspectionRotation * Vector3.right * horizontalSpan * 0.38f;
            Vector3 inspectionPosition = focus + inspectionRotation * Vector3.back * inspectionDistance;
            float blend = Mathf.SmoothStep(0f, 1f, shipyardBlend);
            transform.SetPositionAndRotation(Vector3.Lerp(sailingPosition, inspectionPosition, blend),
                Quaternion.Slerp(sailingRotation, inspectionRotation, blend));
        }

        private float MeasureInspectionDistance(Quaternion rotation)
        {
            Bounds bounds = new Bounds(target.position + Vector3.up, new Vector3(3f, 3f, 6f));
            foreach (var renderer in target.GetComponentsInChildren<Renderer>())
                if (renderer.enabled && (renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                    bounds.Encapsulate(renderer.bounds);
            Vector3 ext = bounds.extents;
            Quaternion inverse = Quaternion.Inverse(rotation);
            Vector3 projected = Vector3.zero;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = inverse * new Vector3(ext.x * x, ext.y * y, ext.z * z);
                        projected = Vector3.Max(projected, new Vector3(Mathf.Abs(corner.x), Mathf.Abs(corner.y), Mathf.Abs(corner.z)));
                    }
            float tangent = Mathf.Tan((viewCamera != null ? viewCamera.fieldOfView : 60f) * 0.5f * Mathf.Deg2Rad);
            float aspect = viewCamera != null ? viewCamera.aspect : 1.777f;
            return Mathf.Max(8f, Mathf.Max(projected.y / (tangent * 0.64f),
                projected.x / (tangent * aspect * 0.55f)) + projected.z);
        }

        private void UpdateZoom()
        {
            if (WantsShipyardView || shipyardBlend > 0f ||
                PrototypeHarborInventoryPanel.BlocksGameplayInput) return;
            float zoomInput = zoomAction != null ? zoomAction.action.ReadValue<float>() : 0f;

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
            float speedRatio = 0f;
            if (targetRigidbody != null)
            {
                Vector3 planarVelocity = Vector3.ProjectOnPlane(
                    targetRigidbody.linearVelocity,
                    Vector3.up
                );
                speedRatio = Mathf.Clamp01(
                    planarVelocity.magnitude /
                    speedForMaximumBoost
                );
            }

            float desiredDistance = Mathf.Clamp(
                targetDistance +
                speedDistanceBoost *
                speedRatio * speedRatio,
                minimumDistance,
                maximumDistance
            );
            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                desiredDistance,
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

            if (WantsShipyardView || aimController == null ||
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
