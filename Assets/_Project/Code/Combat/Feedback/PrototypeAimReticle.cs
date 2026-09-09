using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Combat
{
    [RequireComponent(typeof(ManualBroadsideAimController))]
    public sealed class PrototypeAimReticle : MonoBehaviour
    {
        private const int SegmentCount = 48;

        [SerializeField, Min(0.01f)]
        private float lineWidth = 0.055f;

        [SerializeField]
        private Color invalidColor =
            new Color(1f, 0.2f, 0.15f, 0.95f);

        [SerializeField]
        private Color preparingColor =
            new Color(1f, 0.72f, 0.12f, 0.95f);

        [SerializeField]
        private Color readyColor =
            new Color(0.2f, 1f, 0.42f, 0.95f);

        [SerializeField]
        private Color cooldownColor =
            new Color(0.2f, 0.65f, 1f, 0.95f);

        private ManualBroadsideAimController aimController;
        private LineRenderer desiredRing;
        private LineRenderer currentRing;
        private LineRenderer sideIndicator;

        private Material desiredMaterial;
        private Material currentMaterial;
        private Material sideMaterial;

        private void Awake()
        {
            aimController =
                GetComponent<ManualBroadsideAimController>();

            desiredRing = CreateRing(
                "Desired Aim",
                out desiredMaterial
            );
            currentRing = CreateRing(
                "Current Aim",
                out currentMaterial
            );
            sideIndicator = CreateRing(
                "Selected Broadside",
                out sideMaterial
            );
        }

        private void LateUpdate()
        {
            bool visible = aimController.IsAiming;

            desiredRing.enabled = visible;
            currentRing.enabled = visible;
            sideIndicator.enabled = visible;

            if (!visible)
            {
                return;
            }

            bool valid =
                aimController.IsInRange &&
                aimController.IsInsideFiringArc;

            Color stateColor;

            if (!valid)
            {
                stateColor = invalidColor;
            }
            else if (aimController.IsCoolingDown)
            {
                stateColor = cooldownColor;
            }
            else
            {
                stateColor = Color.Lerp(
                    preparingColor,
                    readyColor,
                    aimController.AimReadiness
                );
            }

            SetMaterialColor(
                desiredMaterial,
                stateColor
            );
            SetMaterialColor(
                currentMaterial,
                stateColor
            );
            SetMaterialColor(
                sideMaterial,
                stateColor
            );

            float spreadRadius = Mathf.Lerp(
                1.8f,
                0.3f,
                aimController.AimReadiness
            );

            if (aimController.IsCoolingDown)
            {
                spreadRadius +=
                    0.08f *
                    Mathf.Sin(Time.time * 10f);
            }

            DrawRing(
                desiredRing,
                aimController.DesiredAimPoint,
                spreadRadius
            );
            DrawRing(
                currentRing,
                aimController.CurrentAimPoint,
                0.22f
            );

            Vector3 sideDirection =
                aimController.SelectedBroadside ==
                BroadsideSide.Starboard
                    ? transform.right
                    : -transform.right;

            Vector3 sidePosition =
                transform.position +
                sideDirection * 1.55f;
            sidePosition.y =
                aimController.CurrentAimPoint.y;

            float sideRadius =
                aimController.IsCoolingDown
                    ? Mathf.Lerp(
                        0.12f,
                        0.34f,
                        aimController.CooldownProgress
                    )
                    : 0.34f;

            DrawRing(
                sideIndicator,
                sidePosition,
                sideRadius
            );
        }

        private LineRenderer CreateRing(
            string objectName,
            out Material material)
        {
            GameObject ringObject =
                new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer =
                ringObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.positionCount = SegmentCount;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.shadowCastingMode =
                ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sortingOrder = 20;

            Shader shader = Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            material = new Material(shader);
            lineRenderer.material = material;
            lineRenderer.enabled = false;

            return lineRenderer;
        }

        private static void SetMaterialColor(
            Material material,
            Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void DrawRing(
            LineRenderer lineRenderer,
            Vector3 center,
            float radius)
        {
            center.y += 0.06f;

            for (int index = 0;
                 index < SegmentCount;
                 index++)
            {
                float angle =
                    index /
                    (float)SegmentCount *
                    Mathf.PI *
                    2f;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                lineRenderer.SetPosition(
                    index,
                    center + offset
                );
            }
        }

        private void OnDestroy()
        {
            DestroyMaterial(desiredMaterial);
            DestroyMaterial(currentMaterial);
            DestroyMaterial(sideMaterial);
        }

        private static void DestroyMaterial(
            Material material)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
