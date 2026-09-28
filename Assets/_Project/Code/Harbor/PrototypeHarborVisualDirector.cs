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
            new(0.19f, 0.17f, 0.14f, 1f);
        private static readonly Color TimberLight =
            new(0.39f, 0.34f, 0.26f, 1f);
        private static readonly Color Plaster =
            new(0.56f, 0.55f, 0.48f, 1f);
        private static readonly Color Roof =
            new(0.20f, 0.25f, 0.27f, 1f);
        private static readonly Color Lantern =
            new(1f, 0.63f, 0.22f, 1f);

        private readonly List<Transform> labels = new();
        private Material sharedMaterial;
        private PhysicsMaterial boundaryMaterial;
        private readonly List<Mesh> generatedMeshes = new();
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
            BuildCoastalRim();
            BuildCoastalTrees();
            // Soft harbor-only fill keeps the player's shaded hull readable.
            var fillObject = new GameObject("Harbor Sky Fill");
            fillObject.transform.SetParent(transform, false);
            fillObject.transform.localRotation = Quaternion.Euler(48f, -35f, 0f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.74f, 0.84f, 1f);
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;
        }

        private static float CoastFront(float x) =>
            -16f + 28f * Mathf.Pow(Mathf.Abs(x) / 40f, 2.2f);

        private void BuildCoastalTrees()
        {
            // Inland groups keep the quay, paths and station silhouettes clear.
            Vector2[] positions = {
                new Vector2(-30, -31), new Vector2(-25, -35), new Vector2(-32, -39),
                new Vector2(-18, -38), new Vector2(-14, -33),
                new Vector2(16, -34), new Vector2(22, -38), new Vector2(29, -32),
                new Vector2(33, -38), new Vector2(3, -37)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                var p = positions[i];
                float height = Mathf.Lerp(1.1f, 3.2f + Mathf.Sin(p.x * .16f) * .5f,
                    Mathf.Clamp01((-p.y - 28f) / 15f));
                float scale = .78f + (i % 4) * .12f;
                MeshyCoastAssets.Place(transform, "CoastalPine", "Coastal Pine " + (i + 1),
                    new Vector3(p.x, height - .12f, p.y), Vector3.one * scale, i * 137.5f);
            }
        }

        private void BuildCoastalRim()
        {
            // Offset clusters follow the outer shoulders rather than lining the quay like a fence.
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                {
                    float x = side * (25f + i * 3.3f);
                    float z = CoastFront(x) - 3.5f + Mathf.Sin(i * 2.1f + side) * 1.4f;
                    MeshyCoastAssets.Rock(transform, "Cove Shoulder " + side + " " + i,
                        new Vector3(x, -0.65f - (i % 2) * 0.4f, z),
                        9f + (i % 3) * 1.6f, i * 67f + side * 19f);
                }
            for (int i = 0; i < 5; i++)
                MeshyCoastAssets.Rock(transform, "Inland Outcrop " + i,
                    new Vector3(-30f + i * 15f, 0.4f, -36f - (i % 2) * 3f),
                    14f + (i % 2) * 4f, i * 73f);
        }

        private void BuildShore()
        {
            // Four rows form a submerged lip, a sloping bank, the usable shore and inland rise.
            const int columns = 41;
            var vertices = new Vector3[columns * 4];
            var triangles = new List<int>();
            for (int i = 0; i < columns; i++)
            {
                float x = -40f + i * 2f;
                float front = CoastFront(x);
                vertices[i * 4] = new Vector3(x, -0.8f, front + 1.2f);
                vertices[i * 4 + 1] = new Vector3(x, 1.1f, front);
                vertices[i * 4 + 2] = new Vector3(x, 1.1f, -28f);
                vertices[i * 4 + 3] = new Vector3(x, 3.2f + Mathf.Sin(x * 0.16f) * 0.5f, -43f);
                if (i == columns - 1) continue;
                for (int row = 0; row < 3; row++)
                {
                    int v = i * 4 + row;
                    triangles.AddRange(new[] { v, v + 4, v + 1, v + 1, v + 4, v + 5 });
                }
            }
            var mesh = new Mesh { name = "Cove Shore Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            var shore = new GameObject("Cove Shore");
            shore.transform.SetParent(transform, false);
            shore.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = shore.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = ResolveMaterial();
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", new Color(0.32f, 0.31f, 0.24f));
            renderer.SetPropertyBlock(block);
            BuildShoreBoundary(mesh, columns);
            shore.AddComponent<HarborGroundSurface>().Build(mesh, renderer);

            // The working quay remains short and straight between the two existing piers.
            CreatePart("Harbor Wall", PrimitiveType.Cube,
                new Vector3(0f, 0.65f, -15.5f), new Vector3(21f, 1.3f, 0.8f), Stone);
            CreatePart("Quay Coping", PrimitiveType.Cube,
                new Vector3(0f, 1.34f, -15.5f), new Vector3(21.2f, 0.16f, 0.95f), StoneLight);
            for (int i = 0; i < 14; i++)
                CreatePart("Quay Masonry " + i, PrimitiveType.Cube,
                    new Vector3(-9.75f + i * 1.5f, 0.75f, -15.05f),
                    new Vector3(1.44f, 1.05f, 0.12f), StoneLight);
        }

        private PhysicsMaterial BoundaryMaterial()
        {
            if (boundaryMaterial == null)
                boundaryMaterial = new PhysicsMaterial("Harbor sliding boundary")
                {
                    staticFriction = 0f, dynamicFriction = 0f, bounciness = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
            return boundaryMaterial;
        }

        private void BuildShoreBoundary(Mesh surface, int columns)
        {
            // A closed vertical solid follows the visible bank. Sloped terrain colliders
            // would lift the hull onto land; vertical faces preserve water-level movement.
            var source = surface.vertices;
            int count = source.Length;
            var vertices = new Vector3[count * 2];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(source[i].x, 3f, source[i].z);
                vertices[i + count] = new Vector3(source[i].x, -4f, source[i].z);
            }
            var triangles = new List<int>(surface.triangles);
            var top = surface.triangles;
            for (int i = 0; i < top.Length; i += 3)
                triangles.AddRange(new[] { top[i] + count, top[i + 2] + count, top[i + 1] + count });
            var rim = new List<int>();
            for (int i = 0; i < columns; i++) rim.Add(i * 4);
            for (int row = 1; row < 4; row++) rim.Add((columns - 1) * 4 + row);
            for (int i = columns - 2; i >= 0; i--) rim.Add(i * 4 + 3);
            rim.Add(2); rim.Add(1);
            for (int i = 0; i < rim.Count; i++)
            {
                int a = rim[i], b = rim[(i + 1) % rim.Count];
                triangles.AddRange(new[] { a, a + count, b, b, a + count, b + count });
            }
            var mesh = new Mesh { name = "Harbor Shore Collision" };
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            var root = new GameObject("Harbor Shore Boundary");
            root.transform.SetParent(transform, false);
            var collider = root.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.sharedMaterial = BoundaryMaterial();
        }

        private void AddSolidBoundary(string name, Vector3 position, Vector3 size)
        {
            var root = new GameObject(name + " Boundary");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = position;
            var collider = root.AddComponent<BoxCollider>();
            collider.size = size;
            collider.sharedMaterial = BoundaryMaterial();
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
                new Vector3(-14.85f, 0.895f, -8f),
                new Vector3(9.3f, 0.5f, 8f)
            );

            CreateBuilding(
                "Shipwright Workshop",
                new Vector3(-16f, 2.2f, -10.5f),
                new Vector3(8f, 3.4f, 5.5f),
                TimberLight
            );

            if (!MeshyCoastAssets.Place(transform, "HarborCrane", "Shipyard Timber Crane",
                new Vector3(-11.7f, 1.235f, -5.6f), Vector3.one, 180f))
            {
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
            }

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
                new Vector3(14.85f, 0.895f, -8f),
                new Vector3(9.3f, 0.5f, 8f)
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
            if (!MeshyCoastAssets.HasOffice) CreatePart(
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
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 6; i++)
                {
                    float z = 4f + i * 4.5f;
                    float x = side * (24f - i * 0.8f);
                    // Keep the tested solid boundary in place; vary only the visible rock cluster.
                    var position = new Vector3(x + Mathf.Sin(i * 2.3f + side) * .35f,
                        -.45f + Mathf.Sin(i * 1.7f + side) * .3f, z + Mathf.Cos(i * 2.1f) * .3f);
                    AddSolidBoundary("Breakwater " + side + " " + i,
                        new Vector3(x, 0f, z), new Vector3(4.2f, 5f, 4.6f));
                    if (!MeshyCoastAssets.Rock(transform, "Rock Breakwater " + side + " " + i,
                        position, 7f + Mathf.Sin(i * 1.9f + side) * .8f, i * 79f + side * 31f))
                        CreatePart("Breakwater Rock", PrimitiveType.Sphere,
                            position + Vector3.up * 0.8f, new Vector3(5f, 2.3f, 5.8f), Stone);
                }
        }

        private void BuildLighthouse()
        {
            MeshyCoastAssets.Rock(transform, "Lighthouse Rock Foundation",
                new Vector3(20f, -0.15f, 27f), 10f, 31f);
            AddSolidBoundary("Lighthouse Foundation", new Vector3(20f, 0f, 27f), new Vector3(6f, 6f, 6f));
            CreatePart(
                "Lighthouse Base",
                PrimitiveType.Cylinder,
                new Vector3(20f, 2.1f, 27f),
                new Vector3(3.5f, 0.7f, 3.5f),
                StoneLight
            );
            CreatePart(
                "Lighthouse Tower",
                PrimitiveType.Cylinder,
                new Vector3(20f, 4.3f, 27f),
                new Vector3(2.5f, 1.8f, 2.5f),
                Plaster
            );
            CreatePart(
                "Lighthouse Lantern",
                PrimitiveType.Sphere,
                new Vector3(20f, 6.5f, 27f),
                new Vector3(1.6f, 1.2f, 1.6f),
                Lantern
            );
            CreatePart(
                "Lighthouse Roof",
                PrimitiveType.Cylinder,
                new Vector3(20f, 7.15f, 27f),
                new Vector3(2.9f, 0.25f, 2.9f),
                Roof
            );

            GameObject beam = new(
                "Lighthouse Beam");
            beam.transform.SetParent(transform, false);
            beam.transform.localPosition =
                new Vector3(20f, 6.5f, 27f);
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
            AddSolidBoundary(objectName, new Vector3(position.x, 0f, position.z),
                new Vector3(scale.x, 4f, scale.z));
            if (MeshyCoastAssets.Dock(transform, objectName,
                position + Vector3.up * (scale.y * 0.5f + 0.09f), new Vector2(scale.x, scale.z))) return;
            // The dark support stays under the boards, making real seams
            // without transparent surfaces or overlapping coplanar faces.
            CreatePart(objectName, PrimitiveType.Cube, position,
                scale, Timber);
            int rows = Mathf.CeilToInt(scale.z / 0.65f);
            int columns = Mathf.CeilToInt(scale.x / 3.5f);
            float depth = scale.z / rows;
            float width = scale.x / columns;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    float shade = 0.90f + ((row * 7 + column * 3) % 9) * 0.025f;
                    Color boardColor = new Color(
                        TimberLight.r * shade, TimberLight.g * shade,
                        TimberLight.b * shade, 1f);
                    CreatePart($"{objectName} Board {row} {column}",
                        PrimitiveType.Cube,
                        position + new Vector3(
                            -scale.x * 0.5f + (column + 0.5f) * width,
                            scale.y * 0.5f + 0.045f,
                            -scale.z * 0.5f + (row + 0.5f) * depth),
                        new Vector3(width - 0.035f, 0.09f, depth - 0.035f),
                        boardColor);
                }
            }

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
            if (objectName == "Trade Warehouse" && MeshyCoastAssets.Place(transform,
                "TradeWarehouse", objectName, new Vector3(position.x, 1.235f, position.z), Vector3.one, 180f)) return;
            if (objectName == "Harbor Office" && MeshyCoastAssets.Place(transform,
                "HarborOffice", objectName, new Vector3(position.x, 1.1f, position.z), Vector3.one, 180f)) return;
            if (objectName == "Shipwright Workshop" && MeshyCoastAssets.Place(transform,
                "ShipwrightWorkshop", objectName, new Vector3(position.x, 1.25f, position.z), Vector3.one)) return;
            CreatePart(
                objectName,
                PrimitiveType.Cube,
                position,
                scale,
                wallColor
            );
            float eave = position.y + scale.y * 0.5f;
            float halfDepth = scale.z * 0.54f;
            float rise = scale.z * 0.26f;
            float slope = Mathf.Atan2(rise, halfDepth) * Mathf.Rad2Deg;
            float roofLength = Mathf.Sqrt(halfDepth * halfDepth + rise * rise);
            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart($"{objectName} Roof {side}", PrimitiveType.Cube,
                    new Vector3(position.x, eave + rise * 0.5f,
                        position.z + side * halfDepth * 0.5f),
                    new Vector3(scale.x * 1.08f, 0.16f, roofLength),
                    Roof, Quaternion.Euler(side * slope, 0f, 0f));
            }
            CreateGables(objectName, position, scale, rise, wallColor);
            CreatePart($"{objectName} Ridge", PrimitiveType.Cube,
                new Vector3(position.x, eave + rise, position.z),
                new Vector3(scale.x * 1.1f, 0.2f, 0.22f), Roof);

            // Facades face the water (+Z); details remain inside each plot.
            float front = position.z + scale.z * 0.5f + 0.06f;
            for (int index = -1; index <= 1; index++)
            {
                float x = position.x + index * scale.x * 0.46f;
                CreatePart($"{objectName} Upright {index}", PrimitiveType.Cube,
                    new Vector3(x, position.y, front),
                    new Vector3(0.18f, scale.y, 0.18f), Timber);
            }
            CreatePart($"{objectName} Front Beam", PrimitiveType.Cube,
                new Vector3(position.x, eave - 0.12f, front),
                new Vector3(scale.x, 0.24f, 0.2f), Timber);
            CreatePart($"{objectName} Stone Footing", PrimitiveType.Cube,
                position + Vector3.down * (scale.y * 0.5f - 0.2f),
                new Vector3(scale.x + 0.12f, 0.4f, scale.z + 0.12f), StoneLight);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 window = new Vector3(
                    position.x + side * scale.x * 0.26f,
                    position.y + 0.25f, front + 0.06f);
                CreatePart($"{objectName} Window Frame {side}", PrimitiveType.Cube,
                    window, new Vector3(1.2f, 1.25f, 0.16f), Timber);
                CreatePart($"{objectName} Window {side}", PrimitiveType.Cube,
                    window + Vector3.forward * 0.09f,
                    new Vector3(0.94f, 1f, 0.04f),
                    new Color(0.12f, 0.19f, 0.20f, 1f));
                CreatePart($"{objectName} Window Mullion {side}", PrimitiveType.Cube,
                    window + Vector3.forward * 0.13f,
                    new Vector3(0.07f, 1f, 0.05f), TimberLight);
            }
        }

        private void CreateGables(string objectName, Vector3 position,
            Vector3 scale, float rise, Color color)
        {
            // Separate vertices keep the end-wall normals flat.
            Vector3[] vertices = new Vector3[6];
            for (int end = 0; end < 2; end++)
            {
                float x = (end == 0 ? -1f : 1f) * scale.x * 0.5f;
                vertices[end * 3] = new Vector3(x, 0f, -scale.z * 0.5f);
                vertices[end * 3 + 1] = new Vector3(x, rise, 0f);
                vertices[end * 3 + 2] = new Vector3(x, 0f, scale.z * 0.5f);
            }
            Mesh mesh = new Mesh { name = $"{objectName} Gables" };
            mesh.vertices = vertices;
            mesh.triangles = new[] { 0, 2, 1, 3, 4, 5 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            GameObject part = new GameObject(mesh.name);
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position + Vector3.up * scale.y * 0.5f;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = ResolveMaterial();
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
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
            light.intensity = 3f;
            light.range = 6f;
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
                collider.enabled = false;
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
            if (sharedMaterial != null)
            {
                sharedMaterial.SetFloat("_Smoothness", 0.16f);
                sharedMaterial.SetFloat("_Metallic", 0f);
            }
            return sharedMaterial;
        }

        private void OnDestroy()
        {
            if (boundaryMaterial != null) Destroy(boundaryMaterial);
            foreach (Mesh mesh in generatedMeshes)
            {
                if (mesh != null) Destroy(mesh);
            }
            generatedMeshes.Clear();
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
