using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    public sealed class PrototypeEnemyNameplateOverlay :
        MonoBehaviour
    {
        private const float ScanInterval = 0.5f;
        private const float MaximumDistance = 38f;

        private readonly Dictionary<
            EnemyShipController,
            Text> labels = new();
        private readonly List<
            EnemyShipController> removals = new();

        private UnityEngine.Camera worldCamera;
        private Transform player;
        private Font font;
        private float nextScanTime;

        public static void EnsureCreated()
        {
            if (FindFirstObjectByType<
                    PrototypeEnemyNameplateOverlay>() != null)
            {
                return;
            }

            GameObject root = new(
                "Prototype Enemy Nameplates");
            root.AddComponent<
                PrototypeEnemyNameplateOverlay>();
        }

        private void Awake()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 55;

            CanvasScaler scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            worldCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null)
            {
                worldCamera = UnityEngine.Camera.main;
            }

            if (player == null)
            {
                ManualBroadsideAimController controller =
                    FindFirstObjectByType<
                        ManualBroadsideAimController>();
                player = controller != null
                    ? controller.transform
                    : null;
            }

            if (Time.unscaledTime >= nextScanTime)
            {
                nextScanTime =
                    Time.unscaledTime + ScanInterval;
                ScanEnemies();
            }

            PositionLabels();
        }

        private void ScanEnemies()
        {
            EnemyShipController[] enemies =
                FindObjectsByType<EnemyShipController>(
                    FindObjectsSortMode.None
                );

            foreach (EnemyShipController enemy in enemies)
            {
                if (enemy != null &&
                    !labels.ContainsKey(enemy))
                {
                    labels.Add(
                        enemy,
                        CreateLabel(enemy)
                    );
                }
            }
        }

        private Text CreateLabel(
            EnemyShipController enemy)
        {
            GameObject labelObject = new(
                enemy.name + " Nameplate",
                typeof(RectTransform),
                typeof(Text),
                typeof(Outline)
            );
            labelObject.transform.SetParent(
                transform,
                false
            );

            RectTransform rect =
                labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240f, 24f);

            Text text = labelObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 12;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = ArchetypeColor(enemy.Archetype);
            text.raycastTarget = false;

            Outline outline =
                labelObject.GetComponent<Outline>();
            outline.effectColor =
                new Color(0.01f, 0.025f, 0.03f, 0.95f);
            outline.effectDistance =
                new Vector2(1.2f, -1.2f);
            outline.useGraphicAlpha = true;
            return text;
        }

        private void PositionLabels()
        {
            if (worldCamera == null) return;

            removals.Clear();
            foreach (KeyValuePair<
                         EnemyShipController,
                         Text> pair in labels)
            {
                EnemyShipController enemy = pair.Key;
                Text label = pair.Value;

                if (enemy == null || label == null)
                {
                    removals.Add(enemy);
                    continue;
                }

                ShipHealth health =
                    enemy.GetComponent<ShipHealth>();
                float distance = player != null
                    ? Vector3.Distance(
                        player.position,
                        enemy.transform.position
                    )
                    : 0f;
                Vector3 screen = worldCamera
                    .WorldToScreenPoint(
                        enemy.transform.position
                    );

                bool visible =
                    enemy.gameObject.activeInHierarchy &&
                    health != null &&
                    !health.IsSunk &&
                    screen.z > 0f &&
                    (player == null ||
                        distance <= MaximumDistance);

                label.gameObject.SetActive(visible);
                if (!visible) continue;

                label.text = enemy.name.ToUpperInvariant();
                label.color =
                    ArchetypeColor(enemy.Archetype);
                label.rectTransform.position =
                    new Vector3(
                        screen.x,
                        screen.y - 30f,
                        0f
                    );
            }

            foreach (EnemyShipController enemy in removals)
            {
                if (labels.TryGetValue(
                        enemy,
                        out Text label) &&
                    label != null)
                {
                    Destroy(label.gameObject);
                }

                labels.Remove(enemy);
            }
        }

        private static Color ArchetypeColor(
            EnemyShipArchetype archetype)
        {
            return archetype switch
            {
                EnemyShipArchetype.Skirmisher =>
                    new Color(0.45f, 0.88f, 0.9f),
                EnemyShipArchetype.Gunship =>
                    new Color(0.94f, 0.7f, 0.3f),
                EnemyShipArchetype.Marauder =>
                    new Color(0.94f, 0.42f, 0.34f),
                _ => new Color(0.9f, 0.86f, 0.74f)
            };
        }
    }
}
