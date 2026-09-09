using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Atmosphere
{
    public sealed class PrototypeCoastalSilhouettes :
        MonoBehaviour
    {
        private const string ShaderName =
            "Seaborn/Coastal Silhouette";
        private const int RingSegments = 14;

        private readonly List<Mesh> generatedMeshes =
            new List<Mesh>();

        private Material coastMaterial;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateForPrototypeOcean()
        {
            if (GameObject.Find("Ocean") == null ||
                FindFirstObjectByType<
                    PrototypeCoastalSilhouettes>() != null)
            {
                return;
            }

            GameObject root =
                new GameObject("Prototype Coastal Silhouettes");
            root.AddComponent<PrototypeCoastalSilhouettes>();
        }

        private void Awake()
        {
            coastMaterial = CreateMaterial();

            CreateIsland(
                "Northwest Coast",
                new Vector3(-43f, 0.72f, 34f),
                new Vector2(8.2f, 4.8f),
                2.1f,
                104
            );
            CreateIsland(
                "Northeast Rocks",
                new Vector3(44f, 0.72f, 32f),
                new Vector2(6.5f, 4.2f),
                1.7f,
                211
            );
            CreateIsland(
                "Southwest Shelf",
                new Vector3(-46f, 0.72f, -37f),
                new Vector2(9.5f, 4.6f),
                1.5f,
                307
            );
            CreateIsland(
                "Southeast Coast",
                new Vector3(43f, 0.72f, -43f),
                new Vector2(8.8f, 5.8f),
                2.25f,
                419
            );
        }

        private void CreateIsland(
            string islandName,
            Vector3 position,
            Vector2 radius,
            float height,
            int seed)
        {
            GameObject island = new GameObject(islandName);
            island.transform.SetParent(transform, false);
            island.transform.position = position;
            island.transform.rotation = Quaternion.Euler(
                0f,
                seed % 71,
                0f
            );

            Mesh mesh = BuildIslandMesh(
                islandName,
                radius,
                height,
                seed
            );
            generatedMeshes.Add(mesh);

            MeshFilter filter =
                island.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer =
                island.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = coastMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static Mesh BuildIslandMesh(
            string meshName,
            Vector2 radius,
            float height,
            int seed)
        {
            System.Random random = new System.Random(seed);
            int ringVertexCount = RingSegments * 3;
            Vector3[] vertices =
                new Vector3[ringVertexCount + 1];

            for (int index = 0;
                 index < RingSegments;
                 index++)
            {
                float angle =
                    index * Mathf.PI * 2f / RingSegments;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);

                float outerNoise = Mathf.Lerp(
                    0.78f,
                    1.18f,
                    (float)random.NextDouble()
                );
                float middleNoise = Mathf.Lerp(
                    0.86f,
                    1.08f,
                    (float)random.NextDouble()
                );
                float innerNoise = Mathf.Lerp(
                    0.82f,
                    1.14f,
                    (float)random.NextDouble()
                );

                vertices[index] = new Vector3(
                    cosine * radius.x * outerNoise,
                    0.015f,
                    sine * radius.y * outerNoise
                );

                vertices[RingSegments + index] =
                    new Vector3(
                        cosine *
                        radius.x *
                        0.62f *
                        middleNoise,
                        height *
                        Mathf.Lerp(
                            0.13f,
                            0.27f,
                            (float)random.NextDouble()
                        ),
                        sine *
                        radius.y *
                        0.62f *
                        middleNoise
                    );

                vertices[RingSegments * 2 + index] =
                    new Vector3(
                        cosine *
                        radius.x *
                        0.28f *
                        innerNoise,
                        height *
                        Mathf.Lerp(
                            0.42f,
                            0.72f,
                            (float)random.NextDouble()
                        ),
                        sine *
                        radius.y *
                        0.28f *
                        innerNoise
                    );
            }

            vertices[ringVertexCount] = new Vector3(
                radius.x * 0.04f,
                height,
                -radius.y * 0.06f
            );

            List<int> triangles =
                new List<int>(RingSegments * 15);

            ConnectRings(
                triangles,
                0,
                RingSegments
            );
            ConnectRings(
                triangles,
                RingSegments,
                RingSegments * 2
            );

            int peakIndex = ringVertexCount;

            for (int index = 0;
                 index < RingSegments;
                 index++)
            {
                int next = (index + 1) % RingSegments;
                triangles.Add(
                    RingSegments * 2 + index
                );
                triangles.Add(peakIndex);
                triangles.Add(
                    RingSegments * 2 + next
                );
            }

            Mesh mesh = new Mesh
            {
                name = meshName + " Mesh"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ConnectRings(
            List<int> triangles,
            int outerStart,
            int innerStart)
        {
            for (int index = 0;
                 index < RingSegments;
                 index++)
            {
                int next = (index + 1) % RingSegments;

                int outer = outerStart + index;
                int outerNext = outerStart + next;
                int inner = innerStart + index;
                int innerNext = innerStart + next;

                triangles.Add(outer);
                triangles.Add(inner);
                triangles.Add(outerNext);

                triangles.Add(outerNext);
                triangles.Add(inner);
                triangles.Add(innerNext);
            }
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find(ShaderName);

            if (shader == null)
            {
                shader = Shader.Find(
                    "Universal Render Pipeline/Lit"
                );
            }

            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = "Runtime Coastal Silhouette"
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    new Color(0.035f, 0.075f, 0.08f, 1f)
                );
            }

            return material;
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in generatedMeshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }

            if (coastMaterial != null)
            {
                Destroy(coastMaterial);
            }
        }
    }
}
