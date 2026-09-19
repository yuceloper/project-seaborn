using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipVisual : MonoBehaviour
    {
        private const string ShaderName =
            "Seaborn/Ship Blockout";
        private const string MaterialResourceName =
            "PrototypeShipBlockout";
        private const string ProductionVisualResourceName =
            "SeabornSloopVisual";
        private const float ProductionVisualLength = 6.5f;
        private const float ProductionWaterlineRatio = 0.12f;

        private readonly List<Material> materials =
            new List<Material>();
        private readonly List<Mesh> meshes =
            new List<Mesh>();

        [Header("Visual Buoyancy")]
        [SerializeField, Min(0f)]
        private float heaveAmplitude = 0.055f;

        [SerializeField, Min(0.01f)]
        private float heaveFrequency = 0.18f;

        [SerializeField, Min(0f)]
        private float rollAmplitude = 1.35f;

        [SerializeField, Min(0f)]
        private float pitchAmplitude = 0.7f;

        [Header("Sailing Response")]
        [SerializeField, Range(0f, 8f)]
        private float maximumTurnLean = 2.8f;

        [SerializeField, Min(0.1f)]
        private float turnLeanResponse = 2.5f;

        [SerializeField, Range(0f, 1f)]
        private float speedHeaveReduction = 0.35f;

        private Transform visualRoot;
        private Transform motionRoot;
        private Renderer[] originalRenderers;
        private Material hullMaterial;
        private Material sailMaterial;
        private Material accentMaterial;
        private ShipMotor shipMotor;
        private float motionPhase;
        private float turnLean;

        private void Awake()
        {
            shipMotor = GetComponent<ShipMotor>();

            if (GetComponent<ShipSailingFeedback>() == null)
            {
                gameObject.AddComponent<ShipSailingFeedback>();
            }

            float roleOffset =
                name.IndexOf(
                    "Enemy",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0
                    ? 1.7f
                    : 0.25f;

            motionPhase = Mathf.Repeat(
                transform.position.x * 0.173f +
                transform.position.z * 0.319f +
                roleOffset,
                Mathf.PI * 2f
            );

            originalRenderers =
                GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in originalRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }

            bool isEnemy =
                name.IndexOf(
                    "Enemy",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;

            Transform staleVisual =
                transform.Find("Ship Visual");
            if (staleVisual != null)
            {
                Destroy(staleVisual.gameObject);
            }

            BuildVisual(isEnemy);
        }

        public void ApplyEnemyPalette(
            Color hull,
            Color sail,
            Color accent)
        {
            SetMaterialColor(hullMaterial, hull);
            SetMaterialColor(sailMaterial, sail);
            SetMaterialColor(accentMaterial, accent);
        }

        private static void SetMaterialColor(
            Material material,
            Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void LateUpdate()
        {
            if (motionRoot == null)
            {
                return;
            }

            float cycle =
                Time.time *
                heaveFrequency *
                Mathf.PI *
                2f +
                motionPhase;

            float speedRatio = 0f;
            float desiredTurnLean = 0f;
            if (shipMotor != null)
            {
                speedRatio = Mathf.Clamp01(
                    Mathf.Abs(shipMotor.CurrentForwardSpeed) / 8f
                );
                desiredTurnLean =
                    -shipMotor.RudderNormalized *
                    maximumTurnLean *
                    speedRatio;
            }

            turnLean = Mathf.Lerp(
                turnLean,
                desiredTurnLean,
                1f - Mathf.Exp(
                    -turnLeanResponse * Time.deltaTime
                )
            );

            float effectiveHeaveAmplitude =
                heaveAmplitude *
                Mathf.Lerp(
                    1f,
                    1f - speedHeaveReduction,
                    speedRatio
                );
            float heave =
                Mathf.Sin(cycle) * effectiveHeaveAmplitude +
                Mathf.Sin(cycle * 1.73f + 0.8f) *
                effectiveHeaveAmplitude *
                0.28f;

            float roll =
                Mathf.Sin(cycle * 0.82f + 1.25f) *
                rollAmplitude;

            float pitch =
                Mathf.Sin(cycle * 1.21f - 0.45f) *
                pitchAmplitude;

            motionRoot.localPosition =
                Vector3.up * heave;
            motionRoot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    roll + turnLean
                );
        }

        private void BuildVisual(bool isEnemy)
        {
            GameObject rootObject =
                new GameObject("Ship Visual");
            visualRoot = rootObject.transform;
            visualRoot.SetParent(transform, false);

            Vector3 scale = transform.lossyScale;
            visualRoot.localScale = new Vector3(
                SafeInverse(scale.x),
                SafeInverse(scale.y),
                SafeInverse(scale.z)
            );

            GameObject motionObject =
                new GameObject("Visual Buoyancy");
            motionRoot = motionObject.transform;
            motionRoot.SetParent(visualRoot, false);

            if (!isEnemy && TryBuildProductionVisual())
            {
                Material productionCannonMaterial =
                    CreateMaterial(
                        new Color(0.055f, 0.05f, 0.045f, 1f),
                        0.12f
                    );

                PrototypeModularShipAssembler productionAssembler =
                    GetComponent<PrototypeModularShipAssembler>();
                if (productionAssembler == null)
                {
                    productionAssembler = gameObject.AddComponent<
                        PrototypeModularShipAssembler>();
                }

                productionAssembler.BuildHardpointsOnly(
                    motionRoot,
                    productionCannonMaterial
                );
                return;
            }

            hullMaterial = CreateMaterial(
                isEnemy
                    ? new Color(0.24f, 0.105f, 0.075f, 1f)
                    : new Color(0.29f, 0.17f, 0.095f, 1f),
                0.2f
            );
            Material deckMaterial = CreateMaterial(
                new Color(0.47f, 0.33f, 0.19f, 1f),
                0.32f
            );
            sailMaterial = CreateMaterial(
                isEnemy
                    ? new Color(0.42f, 0.18f, 0.15f, 1f)
                    : new Color(0.72f, 0.68f, 0.55f, 1f),
                0.38f
            );
            Material darkMaterial = CreateMaterial(
                new Color(0.055f, 0.05f, 0.045f, 1f),
                0.12f
            );
            accentMaterial = CreateMaterial(
                isEnemy
                    ? new Color(0.58f, 0.08f, 0.055f, 1f)
                    : new Color(0.08f, 0.28f, 0.36f, 1f),
                0.42f
            );

            PrototypeModularShipAssembler assembler =
                GetComponent<
                    PrototypeModularShipAssembler>();
            if (assembler == null)
            {
                assembler = gameObject.AddComponent<
                    PrototypeModularShipAssembler>();
            }
            assembler.Build(
                motionRoot,
                hullMaterial,
                deckMaterial,
                darkMaterial
            );
            CreatePrimitivePart(
                "Main Mast",
                PrimitiveType.Cylinder,
                new Vector3(0f, 1.58f, 0.12f),
                new Vector3(0.11f, 1.42f, 0.11f),
                Quaternion.identity,
                darkMaterial
            );
            CreatePrimitivePart(
                "Bowsprit",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.58f, 2.78f),
                new Vector3(0.065f, 0.92f, 0.065f),
                Quaternion.Euler(90f, 0f, 0f),
                darkMaterial
            );

            CreateSail(
                "Main Sail",
                new[]
                {
                    new Vector3(-0.05f, 2.85f, 0.08f),
                    new Vector3(-0.05f, 0.72f, 0.08f),
                    new Vector3(-0.92f, 0.92f, 0.08f)
                },
                sailMaterial
            );
            CreateSail(
                "Fore Sail",
                new[]
                {
                    new Vector3(0.05f, 2.42f, 0.58f),
                    new Vector3(0.05f, 0.82f, 0.58f),
                    new Vector3(0.72f, 1.02f, 0.58f)
                },
                sailMaterial
            );

            CreatePrimitivePart(
                "Mast Flag",
                PrimitiveType.Cube,
                new Vector3(0.34f, 2.75f, 0.12f),
                new Vector3(0.62f, 0.32f, 0.035f),
                Quaternion.identity,
                accentMaterial
            );
        }


        private bool TryBuildProductionVisual()
        {
            GameObject template =
                Resources.Load<GameObject>(
                    ProductionVisualResourceName
                );
            if (template == null)
            {
                return false;
            }

            GameObject instance = Instantiate(
                template,
                motionRoot,
                false
            );
            instance.name = "Seaborn Sloop Production Visual";
            // Preserve the authored textured material colors; no runtime warm tint.

            foreach (Collider visualCollider in
                     instance.GetComponentsInChildren<Collider>(true))
            {
                visualCollider.enabled = false;
                Destroy(visualCollider);
            }

            Bounds bounds = CalculateBoundsInRoot(
                instance,
                motionRoot
            );
            float sourceLength = Mathf.Max(
                bounds.size.x,
                bounds.size.z
            );
            if (sourceLength <= 0.001f)
            {
                Destroy(instance);
                return false;
            }

            float fitScale =
                ProductionVisualLength / sourceLength;
            instance.transform.localScale =
                Vector3.one * fitScale;

            bounds = CalculateBoundsInRoot(
                instance,
                motionRoot
            );
            float waterlineY =
                bounds.min.y +
                bounds.size.y *
                ProductionWaterlineRatio;

            Vector3 correction = new Vector3(
                -bounds.center.x,
                -waterlineY,
                -bounds.center.z
            );
            instance.transform.localPosition += correction;
            return true;
        }


        private static Bounds CalculateBoundsInRoot(
            GameObject target,
            Transform root)
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(
                    Vector3.zero,
                    Vector3.zero
                );
            }

            bool initialized = false;
            Bounds result = default;

            foreach (Renderer renderer in renderers)
            {
                Bounds worldBounds = renderer.bounds;
                Vector3 center = worldBounds.center;
                Vector3 extents = worldBounds.extents;

                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = center + Vector3.Scale(
                        extents,
                        new Vector3(x, y, z)
                    );
                    Vector3 local =
                        root.InverseTransformPoint(corner);

                    if (!initialized)
                    {
                        result = new Bounds(
                            local,
                            Vector3.zero
                        );
                        initialized = true;
                    }
                    else
                    {
                        result.Encapsulate(local);
                    }
                }
            }

            return result;
        }

        private void CreateHull(Material material)
        {
            Mesh mesh = BuildHullMesh();
            meshes.Add(mesh);

            GameObject hull = new GameObject("Hull");
            hull.transform.SetParent(motionRoot, false);

            MeshFilter filter =
                hull.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer =
                hull.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static Mesh BuildHullMesh()
        {
            float[] zPositions =
                { -2.45f, -1.55f, 0f, 1.45f, 2.55f };
            float[] halfWidths =
                { 0.68f, 1.02f, 1.08f, 0.78f, 0.06f };

            Vector3[] vertices =
                new Vector3[zPositions.Length * 4];

            for (int index = 0;
                 index < zPositions.Length;
                 index++)
            {
                float width = halfWidths[index];
                int start = index * 4;

                vertices[start] =
                    new Vector3(-width, 0.25f, zPositions[index]);
                vertices[start + 1] =
                    new Vector3(width, 0.25f, zPositions[index]);
                vertices[start + 2] =
                    new Vector3(
                        -width * 0.56f,
                        -0.48f,
                        zPositions[index]
                    );
                vertices[start + 3] =
                    new Vector3(
                        width * 0.56f,
                        -0.48f,
                        zPositions[index]
                    );
            }

            List<int> triangles = new List<int>();

            for (int section = 0;
                 section < zPositions.Length - 1;
                 section++)
            {
                int current = section * 4;
                int next = (section + 1) * 4;

                AddQuad(
                    triangles,
                    current,
                    next,
                    next + 1,
                    current + 1
                );
                AddQuad(
                    triangles,
                    current,
                    current + 2,
                    next + 2,
                    next
                );
                AddQuad(
                    triangles,
                    current + 1,
                    next + 1,
                    next + 3,
                    current + 3
                );
                AddQuad(
                    triangles,
                    current + 2,
                    current + 3,
                    next + 3,
                    next + 2
                );
            }

            AddQuad(triangles, 0, 1, 3, 2);

            int bow = (zPositions.Length - 1) * 4;
            AddQuad(
                triangles,
                bow,
                bow + 2,
                bow + 3,
                bow + 1
            );

            Mesh mesh = new Mesh
            {
                name = "Prototype Ship Hull"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(
            List<int> triangles,
            int a,
            int b,
            int c,
            int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private void CreateCannons(Material material)
        {
            float[] positions = { -0.82f, 0f, 0.82f };

            foreach (float zPosition in positions)
            {
                CreatePrimitivePart(
                    "Port Cannon",
                    PrimitiveType.Cylinder,
                    new Vector3(-1.04f, 0.37f, zPosition),
                    new Vector3(0.09f, 0.28f, 0.09f),
                    Quaternion.Euler(0f, 0f, 90f),
                    material
                );
                CreatePrimitivePart(
                    "Starboard Cannon",
                    PrimitiveType.Cylinder,
                    new Vector3(1.04f, 0.37f, zPosition),
                    new Vector3(0.09f, 0.28f, 0.09f),
                    Quaternion.Euler(0f, 0f, 90f),
                    material
                );
            }
        }

        private void CreateSail(
            string sailName,
            Vector3[] vertices,
            Material material)
        {
            Mesh mesh = new Mesh
            {
                name = sailName + " Mesh"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(
                new[] { 0, 1, 2, 2, 1, 0 },
                0
            );
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshes.Add(mesh);

            GameObject sail = new GameObject(sailName);
            sail.transform.SetParent(motionRoot, false);

            MeshFilter filter =
                sail.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer =
                sail.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private void CreatePrimitivePart(
            string partName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject part =
                GameObject.CreatePrimitive(primitiveType);
            part.name = partName;
            part.transform.SetParent(motionRoot, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            Collider partCollider =
                part.GetComponent<Collider>();

            if (partCollider != null)
            {
                partCollider.enabled = false;
                Destroy(partCollider);
            }

            MeshRenderer renderer =
                part.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private Material CreateMaterial(
            Color color,
            float topLight)
        {
            Material template =
                Resources.Load<Material>(
                    MaterialResourceName
                );

            Material material;

            if (template != null)
            {
                material = new Material(template);
            }
            else
            {
                Shader shader = Shader.Find(ShaderName);

                if (shader == null)
                {
                    shader = Shader.Find(
                        "Universal Render Pipeline/Lit"
                    );
                }

                material = new Material(shader);
            }

            material.name = "Runtime Ship Blockout";

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_TopLight"))
            {
                material.SetFloat("_TopLight", topLight);
            }

            materials.Add(material);
            return material;
        }

        private static float SafeInverse(float value)
        {
            return Mathf.Abs(value) > 0.0001f
                ? 1f / value
                : 1f;
        }

        private void OnDestroy()
        {
            if (originalRenderers != null)
            {
                foreach (Renderer renderer in originalRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
            }

            foreach (Mesh mesh in meshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }

            foreach (Material material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }

    internal sealed class PrototypeShipVisualBootstrap :
        MonoBehaviour
    {
        private const float ScanInterval = 1f;
        private float nextScanTime;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBootstrap()
        {
            GameObject bootstrapObject =
                new GameObject("Prototype Ship Visual Bootstrap");
            DontDestroyOnLoad(bootstrapObject);
            bootstrapObject.AddComponent<
                PrototypeShipVisualBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + ScanInterval;

            ShipHealth[] ships = FindObjectsByType<ShipHealth>(
                FindObjectsSortMode.None
            );

            foreach (ShipHealth ship in ships)
            {
                if (ship.GetComponent<PrototypeShipVisual>() ==
                    null)
                {
                    ship.gameObject.AddComponent<
                        PrototypeShipVisual>();
                }

                if (ship.GetComponent<ShipSailingFeedback>() ==
                    null)
                {
                    ship.gameObject.AddComponent<
                        ShipSailingFeedback>();
                }
            }
        }
    }
}
