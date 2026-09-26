using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Seaborn.Harbor;

namespace Seaborn.Editor
{
    [InitializeOnLoad]
    public static class CoastalHarborModelSetup
    {
        private const string Root = "Assets/_Project/Art/Environment/SeabornCoast";
        private static readonly string[] Names = { "CoastalRock", "DockModule", "ShipwrightWorkshop" };
        private static bool building;
        private static bool automaticBuildAttempted;
        static CoastalHarborModelSetup() => EditorApplication.delayCall += TryBuildMissing;
        private static string Model(string name) => $"{Root}/{name}/Models/SM_{name}.fbx";
        private static string Texture(string name, string kind) => $"{Root}/{name}/Textures/T_{name}_{kind}.png";
        private static string Prefab(string name) => $"Assets/_Project/Resources/Seaborn{name}Visual.prefab";
        private static bool InputsExist(string name) => File.Exists(Model(name)) &&
            File.Exists(Texture(name, "BaseColor")) && File.Exists(Texture(name, "Normal")) &&
            File.Exists(Texture(name, "MetallicSmoothness"));

        internal static void TryBuildMissing()
        {
            if (building || automaticBuildAttempted || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryBuildMissing;
                return;
            }
            foreach (string name in Names) if (!InputsExist(name)) return;
            foreach (string name in Names)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name)) == null) { automaticBuildAttempted = true; Build(); return; }
        }

        [MenuItem("Seaborn/Art/Build Coastal Harbor Models")]
        public static void Build()
        {
            if (building || EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (string name in Names)
                if (!InputsExist(name))
                {
                    Debug.LogWarning("Missing " + name + ". Extract Seaborn_Coastal_Harbor_2K.zip into the project root, beside Assets.");
                    return;
                }
            building = true;
            try
            {
                // Decode every source before changing any generated material or prefab.
                foreach (string name in Names)
                    foreach (string kind in new[] { "BaseColor", "Normal", "MetallicSmoothness" })
                        ValidateSourceTexture(Texture(name, kind));
                foreach (string name in Names) BuildOne(name);
                AssetDatabase.SaveAssets();
                Debug.Log("Coastal harbor models ready. Enter Play Mode; docking positions are unchanged.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Coastal harbor build stopped: " + exception.Message +
                    "\nSee the specific cause above. Source decoding and Unity asset import are checked separately.");
            }
            finally { building = false; }
        }

        private static void BuildOne(string name)
        {
            var importer = AssetImporter.GetAtPath(Model(name)) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("FBX importer missing: " + name);
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = false;
            importer.addCollider = false;
            importer.isReadable = false;
            importer.SaveAndReimport();
            foreach (string kind in new[] { "BaseColor", "Normal", "MetallicSmoothness" })
            {
                ImportTexture(name, kind);
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            string materialPath = $"{Root}/{name}/Materials/MAT_{name}_URP.mat";
            Directory.CreateDirectory(Path.GetDirectoryName(materialPath));
            Directory.CreateDirectory("Assets/_Project/Resources");
            AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath); }
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", RequireTexture(name, "BaseColor"));
            material.SetTexture("_BumpMap", RequireTexture(name, "Normal"));
            material.SetTexture("_MetallicGlossMap", RequireTexture(name, "MetallicSmoothness"));
            material.SetFloat("_BumpScale", 1);
            material.SetFloat("_Metallic", 1);
            material.SetFloat("_Smoothness", 1);
            material.SetFloat("_SmoothnessTextureChannel", 0);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            var root = new GameObject("Seaborn " + name + " Visual");
            try
            {
                root.AddComponent<MeshyEnvironmentVisual>();
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model(name));
                if (model == null) throw new InvalidOperationException("Model did not import: " + name);
                var visual = UnityEngine.Object.Instantiate(model, root.transform);
                // Preserve the Meshy FBX's authored -90 X / 100 scale correction.
                if (name == "DockModule") visual.transform.localRotation = Quaternion.Euler(0, 90, 0) * visual.transform.localRotation;
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
                {
                    var slots = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < slots.Length; i++) slots[i] = material;
                    renderer.sharedMaterials = slots;
                    renderer.SetPropertyBlock(null);
                }
                Bounds bounds = BoundsOf(visual);
                float span = name == "DockModule" ? bounds.size.z : bounds.size.x;
                if (span < 0.001f || bounds.size.y > span) throw new InvalidOperationException("Unexpected model axes: " + name);
                visual.transform.localScale *= (name == "CoastalRock" ? 12f : 8f) / span;
                bounds = BoundsOf(visual);
                float pivotY = name == "DockModule" ? bounds.min.y + bounds.size.y * 0.82f : bounds.min.y;
                visual.transform.localPosition -= new Vector3(bounds.center.x, pivotY, bounds.center.z);
                // Exact footprint makes repeated pier modules meet without cumulative gaps.
                if (name == "DockModule")
                {
                    var holder = new GameObject("Dock footprint").transform;
                    holder.SetParent(root.transform, false);
                    visual.transform.SetParent(holder, false);
                    holder.localScale = new Vector3(3.48f / bounds.size.x, 1, 1);
                }
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab(name)) == null) throw new InvalidOperationException("Prefab save failed: " + name);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void ImportTexture(string name, string kind)
        {
            string path = Texture(name, kind);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing texture importer: " + path);

            // The asset pack carries GUID-only metadata. Explicitly initialize all relevant
            // settings rather than relying on missing serialized fields to have valid defaults.
            var settings = new TextureImporterSettings();
            settings.ApplyTextureType(kind == "Normal" ? TextureImporterType.NormalMap : TextureImporterType.Default);
            settings.textureShape = TextureImporterShape.Texture2D;
            settings.sRGBTexture = kind == "BaseColor";
            settings.alphaSource = TextureImporterAlphaSource.FromInput;
            settings.mipmapEnabled = true;
            settings.readable = false;
            settings.convertToNormalMap = false;
            settings.filterMode = FilterMode.Bilinear;
            settings.wrapMode = TextureWrapMode.Repeat;
            importer.SetTextureSettings(settings);
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = false;
            var platform = importer.GetDefaultPlatformTextureSettings();
            platform.maxTextureSize = 2048;
            platform.format = TextureImporterFormat.Automatic;
            platform.textureCompression = TextureImporterCompression.Compressed;
            platform.crunchedCompression = false;
            importer.SetPlatformTextureSettings(platform);
            importer.ClearPlatformTextureSettings("Standalone");
            ReimportTexture(path);

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                // One bounded retry isolates platform compression failures while still using
                // Unity's normal-map importer. Preserve the source and its GUID throughout.
                importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Texture importer disappeared: " + path);
                platform = importer.GetDefaultPlatformTextureSettings();
                platform.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(platform);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                ReimportTexture(path);
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null)
                    Debug.LogWarning("Coastal texture recovered with uncompressed import (higher texture memory): " + path);
            }
            var imported = RequireTexture(name, kind);
            if (imported.width != 2048 || imported.height != 2048)
                Debug.LogWarning($"Coastal texture imported at {imported.width}x{imported.height}: {path}. Source is verified 2K.");
        }

        private static void ReimportTexture(string path)
        {
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        internal static void ValidateSourceTexture(string path)
        {
            var probe = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(probe, File.ReadAllBytes(path), false) ||
                    probe.width != 2048 || probe.height != 2048)
                    throw new InvalidDataException("Unreadable or non-2K texture: " + path);
            }
            catch (Exception exception)
            {
                throw new InvalidDataException("Source texture failed: " + path + " — " + exception.Message +
                    ". Replace this source file from the corrected asset archive.", exception);
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); }
        }

        private static Texture2D RequireTexture(string name, string kind)
        {
            string path = Texture(name, kind);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var main = AssetDatabase.LoadMainAssetAtPath(path);
                string actualType = main == null ? "null" : main.GetType().FullName;
                string shape = importer == null ? "no importer" : importer.textureShape.ToString();
                throw new InvalidDataException($"Texture import failed after settings repair and uncompressed retry: {path}. " +
                    $"Source decode passed; main asset={actualType}; shape={shape}; GUID={AssetDatabase.AssetPathToGUID(path)}; " +
                    $"target={EditorUserBuildSettings.activeBuildTarget}. Include this PNG's .meta file and preceding importer errors when reporting.");
            }
            if (texture.width <= 0 || texture.height <= 0)
                throw new InvalidDataException($"Invalid imported texture size {texture.width}x{texture.height}: {path}");
            return texture;
        }

        private static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Model has no renderers.");
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }

    internal sealed class CoastalHarborAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
                if (path.StartsWith("Assets/_Project/Art/Environment/SeabornCoast/", StringComparison.Ordinal))
                { EditorApplication.delayCall += CoastalHarborModelSetup.TryBuildMissing; break; }
        }
    }
}
