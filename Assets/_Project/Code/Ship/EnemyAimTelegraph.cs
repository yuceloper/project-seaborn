using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(EnemyShipController))]
    public sealed class EnemyAimTelegraph : MonoBehaviour
    {
        [SerializeField]
        private Color preparingColor =
            new Color(1f, 0.65f, 0.1f, 0.35f);

        [SerializeField]
        private Color readyColor =
            new Color(1f, 0.12f, 0.08f, 0.9f);

        [SerializeField, Min(0.01f)]
        private float minimumWidth = 0.035f;

        [SerializeField, Min(0.01f)]
        private float maximumWidth = 0.11f;

        private EnemyShipController enemyController;
        private LineRenderer aimLine;
        private Material lineMaterial;

        private void Awake()
        {
            enemyController =
                GetComponent<EnemyShipController>();

            GameObject lineObject =
                new GameObject("Enemy Aim Telegraph");
            lineObject.transform.SetParent(transform, false);

            aimLine =
                lineObject.AddComponent<LineRenderer>();
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.shadowCastingMode =
                ShadowCastingMode.Off;
            aimLine.receiveShadows = false;
            aimLine.sortingOrder = 15;
            aimLine.enabled = false;

            Shader shader = Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            lineMaterial = new Material(shader);
            aimLine.material = lineMaterial;
        }

        private void LateUpdate()
        {
            bool visible =
                enemyController.IsPreparingShot;

            aimLine.enabled = visible;

            if (!visible)
            {
                return;
            }

            float preparation =
                enemyController.AimPreparation;

            Color color = Color.Lerp(
                preparingColor,
                readyColor,
                preparation
            );

            SetMaterialColor(color);

            float width = Mathf.Lerp(
                minimumWidth,
                maximumWidth,
                preparation
            );
            aimLine.startWidth = width;
            aimLine.endWidth = width * 0.6f;

            aimLine.SetPosition(
                0,
                transform.position +
                Vector3.up * 0.65f
            );
            aimLine.SetPosition(
                1,
                enemyController.PredictedAimPoint +
                Vector3.up * 0.08f
            );
        }

        private void SetMaterialColor(Color color)
        {
            if (lineMaterial.HasProperty("_BaseColor"))
            {
                lineMaterial.SetColor(
                    "_BaseColor",
                    color
                );
            }

            if (lineMaterial.HasProperty("_Color"))
            {
                lineMaterial.SetColor(
                    "_Color",
                    color
                );
            }
        }

        private void OnDestroy()
        {
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
            }
        }
    }
}
