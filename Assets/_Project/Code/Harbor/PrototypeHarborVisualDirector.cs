using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Harbor
{
    [DisallowMultipleComponent]
    public sealed class PrototypeHarborVisualDirector :
        MonoBehaviour
    {
        private static readonly Color Stone =
            new(0.24f, 0.27f, 0.26f, 1f);
        private static readonly Color StoneLight =
            new(0.37f, 0.39f, 0.35f, 1f);
        private static readonly Color Timber =
            new(0.31f, 0.18f, 0.08f, 1f);
        private static readonly Color TimberLight =
            new(0.52f, 0.32f, 0.13f, 1f);
        private static readonly Color Plaster =
            new(0.68f, 0.63f, 0.49f, 1f);
        private static readonly Color Roof =
            new(0.34f, 0.12f, 0.08f, 1f);
        private static readonly Color Lantern =
            new(1f, 0.63f, 0.22f, 1f);

        private readonly List<Transform> labels = new();
        private Material sharedMaterial;
        private Transform lighthouseBeam;
        private bool initialized;

        public static void EnsureCreated(Vector3 origin)
        {
            PrototypeHarborVisualDirector director =
                FindFirstObjectByType<
                    PrototypeHarborVisualDirector>();

            if (director == null)
            {
                GameObject root = new(
                    "Prototype Seaborn Harbor");
                director = root.AddComponent<
                    PrototypeHarborVisualDirector>();
            }

            director.Build(origin);
        }

        private void Build(Vector3 origin)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            transform.position =
                new Vector3(origin.x, 0f, origin.z);

            BuildShore();
            BuildWestShipyard();
            BuildEastMarket();
            BuildHarborOffice();
            BuildBreakwaters();
            BuildLighthouse();
            BuildNavigationLane();
        }

        private void BuildShore()
        {
            CreatePart(
                "Southern Stone Shore",
                PrimitiveType.Cube,
                new Vector3(0f, 0.35f, -23f),
                new Vector3(48f, 1.5f, 14f),
                Stone
            );
            CreatePart(
                "Harbor Wall",
                PrimitiveType.Cube,
                new Vector3(0f, 1.05f, -15.8f),
                new Vector3(44f, 1.2f, 1.2f),
                StoneLight
            );

            for (int x = -18; x <= 18; x += 6)
            {
                CreatePart(
                    $"Wall Post {x}",
                    PrimitiveType.Cube,
                    new Vector3(x, 1.65f, -15.3f),
                    new Vector3(1.1f, 2.2f, 1.1f),
                    Stone
                );
            }
        }

        private void BuildWestShipyard()
        {
            CreateDock(
                "West Shipyard Pier",
                new Vector3(-8.5f, 0.92f, -4f),
                new Vector3(3.4f, 0.45f, 22f)
            );
            CreateDock(
                "Shipyard Platform",
                new Vector3(-14.5f, 0.94f, -8f),
                new Vector3(10f, 0.5f, 8f)
            );

            CreateBuilding(
                "Shipwright Workshop",
                new Vector3(-16f, 2.2f, -10.5f),
                new Vector3(8f, 3.4f, 5.5f),
                TimberLight
            );

            CreatePart(
                "Crane Mast",
                PrimitiveType.Cylinder,
                new Vector3(-12.3f, 4.2f, -4.2f),
                new Vector3(0.45f, 3.4f, 0.45f),
                Timber
            );
            CreatePart(
                "Crane Arm",
                PrimitiveType.Cube,
                new Vector3(-10f, 6.9f, -4.2f),
                new Vector3(5.2f, 0.35f, 0.42f),
                Timber
            );
            CreatePart(
                "Crane Rope",
                PrimitiveType.Cylinder,
                new Vector3(-8f, 5.15f, -4.2f),
                new Vector3(0.08f, 1.7f, 0.08f),
                new Color(0.12f, 0.09f, 0.05f, 1f)
            );

            CreateLabel(
                "TERSANE",
                new Vector3(-14.5f, 4.7f, -7.5f),
                Lantern
            );
            AddLantern(
                new Vector3(-9.2f, 2.8f, 2.8f)
            );
        }

        private void BuildEastMarket()
        {
            CreateDock(
                "East Trade Pier",
                new Vector3(8.5f, 0.92f, -3f),
                new Vector3(3.4f, 0.45f, 20f)
            );
            CreateDock(
                "Market Platform",
                new Vector3(14.5f, 0.94f, -8f),
                new Vector3(10f, 0.5f, 8f)
            );

            CreateBuilding(
                "Trade Warehouse",
                new Vector3(16f, 2.35f, -10f),
                new Vector3(9f, 3.8f, 6f),
                Plaster
            );

            for (int i = 0; i < 4; i++)
            {
                CreatePart(
                    $"Trade Crate {i + 1}",
                    PrimitiveType.Cube,
                    new Vector3(
                        11.5f + (i % 2) * 1.4f,
                        1.45f,
                        -5.7f - (i / 2) * 1.3f
                    ),
                    new Vector3(1.1f, 1f, 1.1f),
                    TimberLight,
                    Quaternion.Euler(
                        0f,
                        i * 13f,
                        0f
                    )
                );
            }

            CreateLabel(
                "TİCARET",
                new Vector3(14.5f, 4.9f, -7.5f),
                Lantern
            );
            AddLantern(
                new Vector3(9.2f, 2.8f, 2.8f)
            );
        }

        private void BuildHarborOffice()
        {
            CreateBuilding(
                "Harbor Office",
                new Vector3(0f, 2.6f, -20f),
                new Vector3(10f, 4.2f, 5f),
                new Color(0.57f, 0.54f, 0.43f, 1f)
            );
            CreatePart(
                "Office Door",
                PrimitiveType.Cube,
                new Vector3(0f, 1.75f, -17.42f),
                new Vector3(1.6f, 2.5f, 0.18f),
                Timber
            );
            CreateLabel(
                "LİMAN İDARESİ",
                new Vector3(0f, 5.35f, -17.2f),
                new Color(0.91f, 0.78f, 0.46f, 1f)
            );
        }

        private void BuildBreakwaters()
        {
            CreatePart(
                "West Breakwater",
                PrimitiveType.Cube,
                new Vector3(-20f, 0.65f, 15f),
                new Vector3(4f, 1.4f, 25f),
                Stone
            );
            CreatePart(
                "East Breakwater",
                PrimitiveType.Cube,
                new Vector3(20f, 0.65f, 15f),
                new Vector3(4f, 1.4f, 25f),
                Stone
            );

            for (int side = -1; side <= 1; side += 2)
            {
                for (int z = 5; z <= 25; z += 5)
                {
                    CreatePart(
                        $"Breakwater Stone {side} {z}",
                        PrimitiveType.Sphere,
                        new Vector3(
                            side * (20f + (z % 2) * 0.7f),
                            1.1f,
                            z
                        ),
                        new Vector3(3.4f, 1.5f, 3.1f),
                        StoneLight,
                        Quaternion.Euler(
                            0f,
                            z * 7f,
                            0f
                        )
                    );
                }
            }
        }

        private void BuildLighthouse()
        {
            CreatePart(
                "Lighthouse Base",
                PrimitiveType.Cylinder,
                new Vector3(20f, 2.2f, 27f),
                new Vector3(3.2f, 2f, 3.2f),
                StoneLight
            );
            CreatePart(
                "Lighthouse Tower",
                PrimitiveType.Cylinder,
                new Vector3(20f, 6.8f, 27f),
                new Vector3(2.1f, 4.5f, 2.1f),
                Plaster
            );
            CreatePart(
                "Lighthouse Lantern",
                PrimitiveType.Sphere,
                new Vector3(20f, 11.5f, 27f),
                new Vector3(1.6f, 1.2f, 1.6f),
                Lantern
            );
            CreatePart(
                "Lighthouse Roof",
                PrimitiveType.Cylinder,
                new Vector3(20f, 12.45f, 27f),
                new Vector3(2.3f, 0.45f, 2.3f),
                Roof
            );

            GameObject beam = new(
                "Lighthouse Beam");
            beam.transform.SetParent(transform, false);
            beam.transform.localPosition =
                new Vector3(20f, 11.5f, 27f);
            Light light = beam.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color =
                new Color(1f, 0.72f, 0.38f);
            light.intensity = 650f;
            light.range = 38f;
            light.spotAngle = 24f;
            beam.transform.rotation =
                Quaternion.Euler(8f, 0f, 0f);
            lighthouseBeam = beam.transform;
        }

        private void BuildNavigationLane()
        {
            for (int index = 0; index < 5; index++)
            {
                float z = 10f + index * 11f;
                CreateBuoy(
                    new Vector3(-5.5f, 0.95f, z),
                    new Color(0.74f, 0.2f, 0.16f, 1f)
                );
                CreateBuoy(
                    new Vector3(5.5f, 0.95f, z),
                    new Color(0.22f, 0.68f, 0.48f, 1f)
                );
            }

            CreateLabel(
                "AÇIK DENİZ",
                new Vector3(0f, 2.4f, 58f),
                new Color(0.48f, 0.78f, 0.86f, 1f)
            );
        }

        private void CreateDock(
            string objectName,
            Vector3 position,
            Vector3 scale)
        {
            CreatePart(
                objectName,
                PrimitiveType.Cube,
                position,
                scale,
                TimberLight
            );

            float halfLength = scale.z * 0.5f;
            for (float z = -halfLength + 1f;
                 z <= halfLength - 1f;
                 z += 3f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    CreatePart(
                        $"{objectName} Post",
                        PrimitiveType.Cylinder,
                        position +
                        new Vector3(
                            side * scale.x * 0.42f,
                            -0.35f,
                            z
                        ),
                        new Vector3(0.28f, 1.25f, 0.28f),
                        Timber
                    );
                }
            }
        }

        private void CreateBuilding(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Color wallColor)
        {
            CreatePart(
                objectName,
                PrimitiveType.Cube,
                position,
                scale,
                wallColor
            );
            CreatePart(
                $"{objectName} Roof",
                PrimitiveType.Cube,
                position +
                Vector3.up * (scale.y * 0.56f),
                new Vector3(
                    scale.x * 1.08f,
                    0.7f,
                    scale.z * 1.08f
                ),
                Roof,
                Quaternion.Euler(0f, 0f, 3f)
            );
        }

        private void CreateBuoy(
            Vector3 position,
            Color color)
        {
            CreatePart(
                "Navigation Buoy",
                PrimitiveType.Cylinder,
                position,
                new Vector3(0.42f, 0.65f, 0.42f),
                color
            );
            CreatePart(
                "Navigation Buoy Light",
                PrimitiveType.Sphere,
                position + Vector3.up * 0.9f,
                Vector3.one * 0.28f,
                Lantern
            );
        }

        private void AddLantern(Vector3 position)
        {
            CreatePart(
                "Lantern Post",
                PrimitiveType.Cylinder,
                position,
                new Vector3(0.16f, 1.5f, 0.16f),
                Timber
            );
            CreatePart(
                "Lantern",
                PrimitiveType.Sphere,
                position + Vector3.up * 1.65f,
                Vector3.one * 0.35f,
                Lantern
            );

            GameObject lightObject =
                new GameObject("Harbor Lantern Light");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition =
                position + Vector3.up * 1.65f;
            Light light =
                lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Lantern;
            light.intensity = 45f;
            light.range = 9f;
        }

        private void CreateLabel(
            string value,
            Vector3 position,
            Color color)
        {
            GameObject label =
                new GameObject($"{value} Label");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = position;

            TextMesh text = label.AddComponent<TextMesh>();
            text.text = value;
            text.fontSize = 48;
            text.characterSize = 0.1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            labels.Add(label.transform);
        }

        private GameObject CreatePart(
            string objectName,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Color color,
            Quaternion? rotation = null)
        {
            GameObject part =
                GameObject.CreatePrimitive(primitive);
            part.name = objectName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation =
                rotation ?? Quaternion.identity;
            part.transform.localScale = scale;

            Collider collider =
                part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer =
                part.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = ResolveMaterial();
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }

                MaterialPropertyBlock properties =
                    new MaterialPropertyBlock();
                renderer.GetPropertyBlock(properties);
                properties.SetColor(
                    "_BaseColor",
                    color
                );
                properties.SetColor("_Color", color);
                renderer.SetPropertyBlock(properties);
            }

            return part;
        }

        private Material ResolveMaterial()
        {
            if (sharedMaterial != null)
            {
                return sharedMaterial;
            }

            Shader shader = Shader.Find(
                "Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader != null)
            {
                sharedMaterial = new Material(shader)
                {
                    name =
                        "Prototype Harbor Shared Material"
                };
            }
            return sharedMaterial;
        }

        private void OnDestroy()
        {
            if (sharedMaterial != null)
            {
                Destroy(sharedMaterial);
            }
        }

        private void Update()
        {
            if (lighthouseBeam != null)
            {
                lighthouseBeam.Rotate(
                    0f,
                    18f * Time.deltaTime,
                    0f,
                    Space.World
                );
            }

            UnityEngine.Camera camera =
                UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            for (int i = 0; i < labels.Count; i++)
            {
                if (labels[i] != null)
                {
                    labels[i].rotation =
                        camera.transform.rotation;
                }
            }
        }
    }
}
