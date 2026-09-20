using System.IO;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    [InitializeOnLoad]
    public static class DuskwindModelSetup
    {
        private const string Root = "Assets/_Project/Art/Ships/Duskwind";
        private const string Model = Root + "/Models/SM_Duskwind.fbx";
        private const string MaterialPath = Root + "/Materials/MAT_Duskwind_URP.mat";
        private const string PrefabPath = "Assets/_Project/Resources/SeabornDuskwindVisual.prefab";
        private static bool building;

        static DuskwindModelSetup()
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
            Root + "/Textures/T_Duskwind_" + suffix + ".png";

        [MenuItem("Seaborn/Art/Build Duskwind")]
        public static void Build()
        {
            if (building || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!InputsExist())
            {
                Debug.LogWarning("Duskwind files missing. Extract Seaborn_Duskwind_2K.zip into the project root.");
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
                root = new GameObject("Seaborn Duskwind Visual");
                var visual = Object.Instantiate(model, root.transform);
                visual.name = "Duskwind Model";
                // Preserve the FBX importer's axis correction (the Meshy model has an authored -90 X rotation).
                // Turn the Duskwind bow toward ship-local +Z while preserving the upright correction.
                visual.transform.localRotation = Quaternion.Euler(0f, 270f, 0f) *
                    visual.transform.localRotation;
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
                visual.transform.localScale *= 6.5f / length;
                bounds = BoundsOf(visual);
                float waterline = bounds.min.y + bounds.size.y * 0.12f;
                visual.transform.localPosition -= new Vector3(bounds.center.x, waterline, bounds.center.z);
                BuildMuzzles(root.transform);
                if (PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) == null)
                    throw new System.InvalidOperationException("Could not save Duskwind prefab.");
                AssetDatabase.SaveAssets();
                Debug.Log("Duskwind ready: textured 2K URP prefab, length 6.5. Check bow direction and waterline in Play Mode.");
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                building = false;
            }
        }

        // Measured from the supplied Duskwind mesh, after heading, 6.5 length
        // and 12% waterline correction. Keep these under the visual buoyancy root.
        private static void BuildMuzzles(Transform root)
        {
            Vector3[] port = {
                new Vector3(-0.959f, 0.292f, -0.011f),
                new Vector3(-0.985f, 0.291f, -0.538f),
                new Vector3(-0.957f, 0.312f, -1.029f)
            };
            Vector3[] starboard = {
                new Vector3(0.958f, 0.295f, -0.014f),
                new Vector3(0.985f, 0.284f, -0.528f),
                new Vector3(0.962f, 0.308f, -1.019f)
            };
            for (int i = 0; i < 3; i++)
            {
                CreateMuzzle(root, "Port Muzzle " + i, port[i], Vector3.left);
                CreateMuzzle(root, "Starboard Muzzle " + i, starboard[i], Vector3.right);
            }
        }

        private static void CreateMuzzle(Transform root, string label, Vector3 position, Vector3 direction)
        {
            var muzzle = new GameObject(label).transform;
            muzzle.SetParent(root, false);
            muzzle.localPosition = position;
            muzzle.localRotation = Quaternion.LookRotation(direction, Vector3.up);
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

    internal sealed class DuskwindAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
            {
                if (!path.StartsWith("Assets/_Project/Art/Ships/Duskwind/")) continue;
                EditorApplication.delayCall += DuskwindModelSetup.TryBuildMissing;
                break;
            }
        }
    }
}



