using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Seaborn.Ship
{
    // Flat, world-space surface ribbon; no spray, gravity or camera-facing particles.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipSailingFeedback : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float wakeStartSpeed = 0.25f;
        [SerializeField, Min(0.1f)] private float fullWakeSpeed = 8f;
        [SerializeField, Min(0f)] private float sternOffset = 2.35f;
        [SerializeField, Min(0.05f)] private float wakeHalfWidth = 0.48f;
        [Header("Surface ribbon")]
        [SerializeField] private Transform waterSurface;
        [SerializeField] private float fallbackWaterHeight = 0.7f;
        [SerializeField, Min(0.005f)] private float surfaceClearance = 0.035f;
        [SerializeField, Range(1f, 8f)] private float lifetime = 3.8f;
        [SerializeField, Min(0f)] private float spreadingSpeed = 0.24f;
        [SerializeField, Min(0.05f)] private float sampleDistance = 0.22f;

        private const int MaxSamples = 192;
        private struct Sample
        {
            public Vector3 position, right;
            public float born, strength, distance;
            public bool startsRun;
        }

        private readonly List<Sample> samples = new List<Sample>(MaxSamples);
        private readonly List<Vector3> vertices = new List<Vector3>(MaxSamples * 2);
        private readonly List<Vector2> uvs = new List<Vector2>(MaxSamples * 2);
        private readonly List<Color> colors = new List<Color>(MaxSamples * 2);
        private readonly List<int> triangles = new List<int>((MaxSamples - 1) * 6);
        private Rigidbody body;
        private Mesh mesh;
        private Material material;
        private GameObject ribbon;
        private float travelled;
        private bool startsRun = true;
        private Vector3 previousPosition;
        private Scene ownerScene;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            Shader shader = Resources.Load<Shader>("SeabornSurfaceWake");
            if (shader == null || !shader.isSupported)
            {
                Debug.LogError("Surface wake shader missing or unsupported: SeabornSurfaceWake.", this);
                enabled = false;
                return;
            }

            material = new Material(shader) { name = "Runtime Surface Foam" };
            mesh = new Mesh { name = "World Space Stern Wake" };
            mesh.MarkDynamic();
            // Identity transform keeps recorded positions independent of ship scale/roll.
            ribbon = new GameObject("Ship Surface Wake");
            ribbon.layer = gameObject.layer;
            ownerScene = gameObject.scene;
            SceneManager.MoveGameObjectToScene(ribbon, ownerScene);
            ribbon.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            previousPosition = transform.position;
            ResolveSurface();
        }

        private void ResolveSurface()
        {
            if (waterSurface != null && waterSurface.gameObject.scene == ownerScene) return;
            waterSurface = null;
            foreach (GameObject root in ownerScene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name != "Ocean") continue;
                    waterSurface = candidate;
                    return;
                }
            }
            Debug.LogWarning("Wake: Ocean surface not found; using configured fallback water height.", this);
        }

        private void LateUpdate()
        {
            if (mesh == null || ribbon == null) return;

            if (gameObject.scene != ownerScene)
            {
                ownerScene = gameObject.scene;
                SceneManager.MoveGameObjectToScene(ribbon, ownerScene);
                samples.Clear();
                startsRun = true;
                ResolveSurface();
            }

            Vector3 step = transform.position - previousPosition;
            previousPosition = transform.position;
            // Never draw a strip across map travel / respawn.
            if (step.sqrMagnitude > 100f)
            {
                samples.Clear();
                startsRun = true;
            }

            float now = Time.time;
            while (samples.Count > 0 && now - samples[0].born > lifetime)
                samples.RemoveAt(0);

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 stern = transform.position - forward * sternOffset;
            float speed = Vector3.Dot(body.linearVelocity, forward);
            bool moving = body.gameObject.activeInHierarchy && speed > wakeStartSpeed;
            float strength = Mathf.Lerp(0.28f, 1f,
                Mathf.InverseLerp(wakeStartSpeed, Mathf.Max(wakeStartSpeed + 0.01f, fullWakeSpeed), speed));

            if (moving)
            {
                float distance = samples.Count == 0 ? 0f :
                    Vector3.ProjectOnPlane(stern - samples[samples.Count - 1].position, Vector3.up).magnitude;
                if (samples.Count == 0 || startsRun || distance >= sampleDistance)
                {
                    travelled += startsRun ? 0f : distance;
                    if (samples.Count >= MaxSamples) samples.RemoveAt(0);
                    samples.Add(new Sample
                    {
                        position = stern, right = right, born = now,
                        strength = strength, distance = travelled, startsRun = startsRun
                    });
                    startsRun = false;
                }
            }
            else startsRun = true;

            BuildRibbon(now, stern, right, strength, moving);
        }

        private void BuildRibbon(float now, Vector3 stern, Vector3 right, float strength, bool moving)
        {
            vertices.Clear(); uvs.Clear(); colors.Clear(); triangles.Clear();
            float height = (waterSurface != null ? waterSurface.position.y : fallbackWaterHeight) + surfaceClearance;
            for (int i = 0; i < samples.Count; i++)
                AddRow(samples[i], now, height, i == 0 || samples[i].startsRun);

            // Live head eliminates visible gaps between the stern and sampled history.
            if (moving && samples.Count > 0)
            {
                Sample last = samples[samples.Count - 1];
                AddRow(new Sample
                {
                    position = stern, right = right, born = now,
                    strength = strength,
                    distance = last.distance + Vector3.Distance(stern, last.position)
                }, now, height, false);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void AddRow(Sample sample, float now, float height, bool breakBefore)
        {
            float age = Mathf.Max(0f, now - sample.born);
            float life = Mathf.Clamp01(age / Mathf.Max(0.1f, lifetime));
            float spreadTime = Mathf.Max(0.1f, lifetime * 0.9f);
            float spread = spreadingSpeed * spreadTime * (1f - Mathf.Exp(-age / spreadTime));
            float width = wakeHalfWidth * Mathf.Lerp(0.8f, 1.2f, sample.strength) +
                spread * Mathf.Lerp(0.5f, 1f, sample.strength);
            Vector3 center = sample.position;
            center.y = height;
            int index = vertices.Count;
            vertices.Add(center - sample.right * width);
            vertices.Add(center + sample.right * width);
            uvs.Add(new Vector2(0f, sample.distance));
            uvs.Add(new Vector2(1f, sample.distance));
            float alpha = Mathf.Pow(1f - life, 1.6f) * sample.strength;
            if (breakBefore) alpha = 0f;
            // R encodes normalized foam age; A remains opacity.
            Color color = new Color(life, 1f, 1f, alpha);
            colors.Add(color); colors.Add(color);
            if (index < 2 || breakBefore) return;
            triangles.Add(index - 2); triangles.Add(index); triangles.Add(index - 1);
            triangles.Add(index - 1); triangles.Add(index); triangles.Add(index + 1);
        }

        private void OnDisable()
        {
            samples.Clear();
            startsRun = true;
            if (mesh != null) mesh.Clear();
        }

        private void OnDestroy()
        {
            if (ribbon != null) Destroy(ribbon);
            if (mesh != null) Destroy(mesh);
            if (material != null) Destroy(material);
        }
    }
}
