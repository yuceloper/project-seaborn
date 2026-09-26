using System;
using UnityEditor;
using UnityEngine;
using Seaborn.Harbor;

namespace Seaborn.Editor
{
    public static class CoastalHarborModelChecks
    {
        [MenuItem("Seaborn/Validation/Check Coastal Harbor Models")]
        public static void Check()
        {
            foreach (string name in new[] { "CoastalRock", "DockModule", "ShipwrightWorkshop" })
            {
                var prefab = Resources.Load<GameObject>("Seaborn" + name + "Visual");
                Require(prefab != null, "Missing " + name + "; build the coastal models first.");
                Require(prefab.GetComponent<MeshyEnvironmentVisual>() != null, name + ": palette protection missing.");
                Require(prefab.GetComponentsInChildren<Collider>(true).Length == 0, name + ": unexpected navigation collider.");
                var instance = UnityEngine.Object.Instantiate(prefab);
                try
                {
                    instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    instance.transform.localScale = Vector3.one;
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    Require(renderers.Length > 0, name + ": empty prefab.");
                    Bounds bounds = renderers[0].bounds;
                    foreach (var renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                        foreach (var material in renderer.sharedMaterials)
                        {
                            Require(material != null && material.shader.name == "Universal Render Pipeline/Lit", name + ": wrong shader.");
                            foreach (string property in new[] { "_BaseMap", "_BumpMap", "_MetallicGlossMap" })
                            {
                                var texture = material.GetTexture(property);
                                Require(texture != null && texture.width == 2048 && texture.height == 2048,
                                    name + ": missing/non-2K " + property);
                            }
                            Require(material.GetColor("_BaseColor") == Color.white, name + ": unwanted material tint.");
                        }
                    }
                    Require(Mathf.Abs(bounds.center.x) < 0.02f && Mathf.Abs(bounds.center.z) < 0.02f,
                        name + ": off-center pivot.");
                    if (name == "DockModule")
                    {
                        Require(Mathf.Abs(bounds.size.x - 3.48f) < 0.03f && Mathf.Abs(bounds.size.z - 8f) < 0.03f,
                            "Dock footprint must be 3.48 x 8.");
                        Require(Mathf.Abs(bounds.min.y + bounds.size.y * 0.82f) < 0.03f, "Dock deck pivot is incorrect.");
                    }
                    else
                    {
                        Require(Mathf.Abs(bounds.min.y) < 0.03f, name + ": ground pivot is incorrect.");
                        Require(Mathf.Abs(bounds.size.x - (name == "CoastalRock" ? 12f : 8f)) < 0.03f,
                            name + ": wrong scale.");
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(instance); }
            }
            Debug.Log("Coastal harbor model checks passed. Still verify appearance, pier joins, docking and scene travel in Play Mode.");
        }
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
