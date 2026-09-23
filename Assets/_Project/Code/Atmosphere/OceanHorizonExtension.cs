using UnityEngine;
using UnityEngine.Rendering;
using Seaborn.World;

namespace Seaborn.Atmosphere
{
    // A visual-only ring around the authored ocean: no larger collider or playable map.
    [DisallowMultipleComponent]
    public sealed class OceanHorizonExtension : MonoBehaviour
    {
        private Mesh mesh;
        private MeshRenderer ringRenderer;

        public static void Ensure(Renderer ocean, UnityEngine.Camera camera)
        {
            if (ocean == null) return;
            var extension = ocean.GetComponent<OceanHorizonExtension>();
            if (extension == null) extension = ocean.gameObject.AddComponent<OceanHorizonExtension>();
            extension.Rebuild(ocean, camera);
        }

        private void Rebuild(Renderer ocean, UnityEngine.Camera camera)
        {
            if (ringRenderer == null)
            {
                var ring = new GameObject("Distant Ocean", typeof(MeshFilter), typeof(MeshRenderer));
                ring.layer = ocean.gameObject.layer;
                ring.transform.SetParent(transform, false);
                ringRenderer = ring.GetComponent<MeshRenderer>();
                ringRenderer.shadowCastingMode = ShadowCastingMode.Off;
                ringRenderer.receiveShadows = true;
                ringRenderer.lightProbeUsage = LightProbeUsage.Off;
                ringRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
            // Ocean waves and foam use world coordinates, so the ring matches without stretching.
            ringRenderer.sharedMaterial = ocean.sharedMaterial;
            Bounds bounds = ocean.bounds;
            float extent = Mathf.Max(1000f, (camera != null ? camera.farClipPlane : 1000f) * 1.2f)
                + PrototypeExpeditionRegionDirector.MapEdge;
            float y = bounds.center.y;
            Vector3[] vertices =
            {
                new(bounds.min.x, y, bounds.min.z), new(bounds.min.x, y, bounds.max.z),
                new(bounds.max.x, y, bounds.max.z), new(bounds.max.x, y, bounds.min.z),
                new(bounds.min.x - extent, y, bounds.min.z - extent),
                new(bounds.min.x - extent, y, bounds.max.z + extent),
                new(bounds.max.x + extent, y, bounds.max.z + extent),
                new(bounds.max.x + extent, y, bounds.min.z - extent)
            };
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = ringRenderer.transform.InverseTransformPoint(vertices[i]);
            int[] triangles = new int[24];
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4, t = i * 6;
                triangles[t] = i; triangles[t + 1] = i + 4; triangles[t + 2] = next + 4;
                triangles[t + 3] = i; triangles[t + 4] = next + 4; triangles[t + 5] = next;
            }
            if (mesh != null) Destroy(mesh);
            mesh = new Mesh { name = "Ocean horizon ring", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ringRenderer.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }
    }
}
