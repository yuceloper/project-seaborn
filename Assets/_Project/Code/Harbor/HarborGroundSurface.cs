using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Harbor
{
    // Owns only visual resources. The verified docking and shoreline colliders are untouched.
    public sealed class HarborGroundSurface : MonoBehaviour
    {
        private readonly List<Material> materials = new();
        private readonly List<Mesh> meshes = new();
        private Texture2D ground;
        private const int Resolution = 1024;
        private static float Front(float x) => -16f + 28f * Mathf.Pow(Mathf.Abs(x) / 40f, 2.2f);
        private static float PathDistance(float x, float z) => Mathf.Min(
            Mathf.Abs(z + 26f), Mathf.Min(Mathf.Abs(x - 8.5f), Mathf.Abs(x + 8.5f)));

        // Unity SmoothStep interpolates endpoints; normalize the sampled value first.
        private static float Mask(float low, float high, float value) =>
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(low, high, value));

        private static float Plaza(float x, float z, float cx, float cz, float rx, float rz) =>
            1f - Mask(.8f, 1f, Mathf.Max(Mathf.Abs(x - cx) / rx, Mathf.Abs(z - cz) / rz));

        public void Build(Mesh mesh, MeshRenderer renderer)
        {
            var uv = new Vector2[mesh.vertexCount];
            var vertices = mesh.vertices;
            for (int i = 0; i < uv.Length; i++)
                uv[i] = new Vector2((vertices[i].x + 40f) / 80f, (vertices[i].z + 43f) / 57f);
            mesh.uv = uv;
            ground = new Texture2D(Resolution, Resolution, TextureFormat.RGB24, true)
            { name = "Harbor ground stone soil grass", wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            var pixels = new Color32[Resolution * Resolution];
            for (int y = 0; y < Resolution; y++)
                for (int x = 0; x < Resolution; x++)
                {
                    float wx = x / (Resolution - 1f) * 80f - 40f;
                    float wz = y / (Resolution - 1f) * 57f - 43f;
                    float grain = Mathf.PerlinNoise(wx * 5f + 300f, wz * 5f + 300f);
                    float patches = Mathf.PerlinNoise(wx * .22f + 150f, wz * .22f + 150f);
                    float depth = Front(wx) - wz;
                    Color soil = Color.Lerp(new Color(.24f, .21f, .15f), new Color(.43f, .37f, .25f), grain);
                    float path = 1f - Mask(.8f, 1.65f, PathDistance(wx, wz) + (patches - .5f) * .5f);
                    float vegetation = Mask(3f, 8f, depth) * (1f - path) *
                        Mask(.32f, .7f, patches);
                    Color color = Color.Lerp(soil, new Color(.25f, .29f, .16f) * (.8f + grain * .4f), vegetation);
                    color = Color.Lerp(color, new Color(.45f, .39f, .29f) * (.85f + grain * .3f), path * .75f);
                    // Narrow irregular stone promenade inside the bank; worn seams stay readable from above.
                    float promenade = (1f - Mask(2.5f, 3.4f, depth)) * Mask(.6f, 1.8f, depth);
                    float plazas = Mathf.Max(Plaza(wx, wz, 0f, -20f, 5.5f, 4f),
                        Plaza(wx, wz, 16f, -12f, 4.5f, 4.5f));
                    float inlandFade = Mask(-42f, -37f, wz);
                    float paving = Mathf.Max(promenade, Mathf.Max(path, plazas)) * inlandFade;
                    float row = Mathf.Floor(wz / .7f);
                    float sx = Mathf.Repeat(wx / 1.1f + Mathf.Repeat(row, 2f) * .5f, 1f);
                    float sz = Mathf.Repeat(wz / .7f, 1f);
                    float edge = Mathf.Min(Mathf.Min(sx, 1f - sx), Mathf.Min(sz, 1f - sz));
                    Color stone = Color.Lerp(new Color(.20f, .21f, .19f),
                        new Color(.52f, .49f, .41f) * (.8f + grain * .35f), Mask(.025f, .10f, edge));
                    color = Color.Lerp(color, stone, paving);
                    float gravel = 1f - Mask(.8f, 2f, depth + (patches - .5f) * 1.1f);
                    color = Color.Lerp(color, new Color(.48f, .43f, .33f) * (.7f + grain * .6f), gravel);
                    float wet = 1f - Mask(-.6f, .65f, depth);
                    color = Color.Lerp(color, new Color(.12f, .17f, .15f) * (.8f + grain * .4f), wet);
                    pixels[y * Resolution + x] = color;
                }
            ground.SetPixels32(pixels);
            ground.Apply(true, true);
            var material = MakeMaterial("Harbor Ground", Color.white);
            material.SetTexture("_BaseMap", ground);
            renderer.SetPropertyBlock(null);
            renderer.sharedMaterial = material;
            BuildDetails();
        }

        private static float Height(float x, float z)
        {
            // Match the shore's two-unit tessellation rather than floating above its inland slope.
            float left = Mathf.Clamp(Mathf.Floor((x + 40f) / 2f) * 2f - 40f, -40f, 38f);
            float rear = Mathf.Lerp(3.2f + Mathf.Sin(left * .16f) * .5f,
                3.2f + Mathf.Sin((left + 2f) * .16f) * .5f, (x - left) / 2f);
            return Mathf.Lerp(1.1f, rear, Mathf.Clamp01((-z - 28f) / 15f));
        }

        private void BuildDetails()
        {
            var random = new System.Random(927);
            var grass = new List<Vector3>();
            var stones = new List<Vector3>();
            for (int i = 0; i < 380; i++)
            {
                float x = (float)random.NextDouble() * 72f - 36f;
                float z = -24f - (float)random.NextDouble() * 17f;
                if (PathDistance(x, z) < 2f || Front(x) - z < 4f) continue;
                if (Mathf.Abs(x) < 7f && z > -25f) continue;
                var p = new Vector3(x, Height(x, z) + .02f, z);
                float h = .18f + (float)random.NextDouble() * .25f;
                if (i % 4 == 0)
                {
                    float r = h * 1.2f;
                    Vector3 a = p + new Vector3(-r, 0, -r), b = p + new Vector3(r, 0, -r);
                    Vector3 c = p + new Vector3(r, 0, r), d = p + new Vector3(-r, 0, r), tip = p + Vector3.up * h;
                    stones.AddRange(new[] { a, tip, b, b, tip, c, c, tip, d, d, tip, a });
                }
                else
                {
                    grass.AddRange(new[] { p - Vector3.right * .16f, p + Vector3.up * h, p + Vector3.right * .16f,
                        p - Vector3.forward * .16f, p + Vector3.up * h * .85f, p + Vector3.forward * .16f });
                }
            }
            DetailMesh("Sparse Coastal Grass", grass, new Color(.28f, .32f, .17f), true);
            DetailMesh("Small Ground Stones", stones, new Color(.39f, .38f, .32f), false);
        }

        private void DetailMesh(string label, List<Vector3> vertices, Color color, bool doubleSided)
        {
            var triangles = new List<int>();
            for (int i = 0; i < vertices.Count; i += 3)
            {
                triangles.AddRange(new[] { i, i + 1, i + 2 });
            }
            var mesh = new Mesh { name = label };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            meshes.Add(mesh);
            var part = new GameObject(label); part.transform.SetParent(transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var material = MakeMaterial(label, color);
            if (doubleSided) material.SetFloat("_Cull", 0f);
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private Material MakeMaterial(string label, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = label };
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .12f);
            materials.Add(material); return material;
        }

        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
            foreach (var mesh in meshes) if (mesh != null) Destroy(mesh);
            if (ground != null) Destroy(ground);
        }
    }
}
