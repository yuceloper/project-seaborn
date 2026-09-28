using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Harbor
{
    // One visual ribbon along the outer bank; no particles, colliders or moving geometry.
    public sealed class HarborShoreFoam : MonoBehaviour
    {
        private Mesh ribbon;
        private Material material;
        private Texture2D texture;

        public void Build(List<Vector3> bank)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            float distance = 0f;
            Vector3 previous = Vector3.zero;
            bool connected = false;
            for (int i = 0; i < bank.Count; i += 2)
            {
                Vector3 top = bank[i], foot = bank[i + 1];
                if (top.y <= .04f || foot.y >= .04f) { connected = false; continue; }
                Vector3 point = Vector3.Lerp(top, foot, (top.y - .04f) / (top.y - foot.y));
                Vector3 outward = foot - top; outward.y = 0f; outward.Normalize();
                if (connected) distance += Vector3.Distance(previous, point);
                float width = .24f + .16f * Mathf.PerlinNoise(point.x * .2f + 100f, point.z * .2f + 100f);
                int v = vertices.Count;
                vertices.Add(point + outward * .03f);
                vertices.Add(point + outward * width);
                uv.Add(new Vector2(distance / 5f, 0f)); uv.Add(new Vector2(distance / 5f, 1f));
                if (connected) triangles.AddRange(new[] { v - 2, v, v - 1, v, v + 1, v - 1 });
                previous = point; connected = true;
            }
            if (triangles.Count == 0) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return;
            ribbon = new Mesh { name = "Outer bank foam ribbon" };
            ribbon.SetVertices(vertices); ribbon.SetUVs(0, uv); ribbon.SetTriangles(triangles, 0);
            ribbon.RecalculateBounds();
            texture = new Texture2D(128, 32, TextureFormat.RGBA32, true)
            { name = "Broken shoreline foam", wrapModeU = TextureWrapMode.Repeat,
                wrapModeV = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[128 * 32];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 128; x++)
                {
                    float u = x / 128f, v = y / 31f;
                    // Circular noise coordinates keep the repeated texture seamless along the bank.
                    float angle = u * Mathf.PI * 2f;
                    float noise = Mathf.PerlinNoise(10f + Mathf.Cos(angle) * 2f, 10f + Mathf.Sin(angle) * 2f + v);
                    float grain = Mathf.PerlinNoise(30f + Mathf.Cos(angle) * 12f, 30f + Mathf.Sin(angle) * 12f + v * 5f);
                    float edge = Mathf.Sin(v * Mathf.PI);
                    float patches = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.42f, .7f, noise));
                    pixels[y * 128 + x] = new Color(.76f, .84f, .78f, edge * patches * grain);
                }
            texture.SetPixels32(pixels); texture.Apply(true, true);
            material = new Material(shader) { name = "Harbor subtle foam", renderQueue = 3010 };
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f); material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, .5f));
            gameObject.AddComponent<MeshFilter>().sharedMesh = ribbon;
            var renderer = gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void Update()
        {
            if (material == null) return;
            float time = Time.time;
            material.SetTextureOffset("_BaseMap", new Vector2(Mathf.Repeat(time * .012f, 1f), 0f));
            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, .42f + .12f * Mathf.Sin(time * .7f)));
        }

        private void OnDestroy()
        {
            if (ribbon != null) Destroy(ribbon);
            if (material != null) Destroy(material);
            if (texture != null) Destroy(texture);
        }
    }
}
