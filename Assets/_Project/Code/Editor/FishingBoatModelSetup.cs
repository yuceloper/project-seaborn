using System.IO;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    [InitializeOnLoad]
    public static class FishingBoatModelSetup
    {
        private const string Root = "Assets/_Project/Art/Ships/FishingBoat";
        private const string Model = Root + "/Models/SM_FishingBoat.fbx";
        private const string MaterialPath = Root + "/Materials/MAT_FishingBoat_URP.mat";
        private const string PrefabPath = "Assets/_Project/Resources/SeabornFishingBoatVisual.prefab";
        private static bool building;

        static FishingBoatModelSetup()
        {
            EditorApplication.delayCall += TryBuildMissing;
        }

        internal static void TryBuildMissing()
        {
            if (building || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryBuildMissing;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null &&
                InputsExist())
                Build();
        }

        private static bool InputsExist()
        {
            return File.Exists(Model) && File.Exists(TexturePath("BaseColor")) &&
                File.Exists(TexturePath("Normal")) && File.Exists(TexturePath("MetallicSmoothness"));
        }

        private static string TexturePath(string suffix) =>
            Root + "/Textures/T_FishingBoat_" + suffix + ".png";

        [MenuItem("Seaborn/Art/Build Fishing Boat")]
        public static void Build()
        {
            if (building || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!InputsExist())
            {
                Debug.LogWarning("Fishing boat files missing. Extract Seaborn_FishingBoat_2K.zip into the project root.");
                return;
            }
            building = true;
            GameObject root = null;
            try
            {
                var importer = AssetImporter.GetAtPath(Model) as ModelImporter;
                if (importer == null) throw new System.InvalidOperationException("FBX importer not ready.");
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importAnimation = false;
                importer.addCollider = false;
                importer.isReadable = false;
                importer.SaveAndReimport();

                ConfigureTexture("BaseColor", true, false);
                ConfigureTexture("Normal", false, true);
                ConfigureTexture("MetallicSmoothness", false, false);
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new System.InvalidOperationException("URP Lit shader missing.");
                Directory.CreateDirectory(Root + "/Materials");
                Directory.CreateDirectory("Assets/_Project/Resources");
                AssetDatabase.Refresh();
                Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, MaterialPath);
                }
                material.shader = shader;
                material.SetColor("_BaseColor", Color.white);
                material.SetTexture("_BaseMap", LoadTexture("BaseColor"));
                material.SetTexture("_BumpMap", LoadTexture("Normal"));
                material.SetTexture("_MetallicGlossMap", LoadTexture("MetallicSmoothness"));
                material.SetFloat("_BumpScale", 1f);
                material.SetFloat("_Metallic", 1f);
                material.SetFloat("_Smoothness", 1f);
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                material.SetFloat("_Cull", 0f); // Thin sails must be visible from both sides.
                material.EnableKeyword("_NORMALMAP");
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
                if (model == null) throw new System.InvalidOperationException("FBX did not import.");
                root = new GameObject("Seaborn Fishing Boat Visual");
                var visual = Object.Instantiate(model, root.transform);
                visual.name = "Fishing Boat Model";
                // Meshy source length is along X. The boat root travels along +Z.
                visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                    Object.DestroyImmediate(collider);
                foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
                {
                    var slots = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < slots.Length; i++) slots[i] = material;
                    renderer.sharedMaterials = slots;
                }
                Bounds bounds = BoundsOf(visual);
                float length = Mathf.Max(bounds.size.x, bounds.size.z);
                if (length <= 0.001f) throw new System.InvalidOperationException("Empty model bounds.");
                visual.transform.localScale *= 5.5f / length;
                bounds = BoundsOf(visual);
                float waterline = bounds.min.y + bounds.size.y * 0.12f;
                visual.transform.localPosition -= new Vector3(bounds.center.x, waterline, bounds.center.z);
                if (PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) == null)
                    throw new System.InvalidOperationException("Could not save fishing boat prefab.");
                AssetDatabase.SaveAssets();
                Debug.Log("Fishing boat ready: textured 2K URP prefab, length 5.5. Check bow direction and waterline in Play Mode.");
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                building = false;
            }
        }

        private static Texture2D LoadTexture(string suffix) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(suffix));

        private static void ConfigureTexture(string suffix, bool srgb, bool normal)
        {
            var importer = AssetImporter.GetAtPath(TexturePath(suffix)) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Texture importer not ready: " + suffix);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport();
        }

        private static Bounds BoundsOf(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new System.InvalidOperationException("Model has no renderer.");
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }

    internal sealed class FishingBoatAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
            {
                if (!path.StartsWith("Assets/_Project/Art/Ships/FishingBoat/")) continue;
                EditorApplication.delayCall += FishingBoatModelSetup.TryBuildMissing;
                break;
            }
        }
    }
}
