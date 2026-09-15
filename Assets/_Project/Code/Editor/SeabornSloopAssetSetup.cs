using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.EditorTools
{
    [InitializeOnLoad]
    internal static class SeabornSloopAssetSetup
    {
        private const string Root =
            "Assets/_Project/Art/Ships/SeabornSloop";
        private const string ModelPath =
            Root + "/Models/SM_SeabornSloop.fbx";
        private const string BaseColorPath =
            Root + "/Textures/T_SeabornSloop_BaseColor.png";
        private const string NormalPath =
            Root + "/Textures/T_SeabornSloop_Normal.png";
        private const string MetallicPath =
            Root + "/Textures/T_SeabornSloop_Metallic.png";
        private const string RoughnessPath =
            Root + "/Textures/T_SeabornSloop_Roughness.png";
        private const string MaskPath =
            Root + "/Textures/T_SeabornSloop_MaskMap.png";
        private const string MaterialFolder =
            Root + "/Materials";
        private const string MaterialPath =
            MaterialFolder + "/MAT_SeabornSloop_URP.mat";
        private const string ResourceFolder =
            "Assets/_Project/Resources";
        private const string PrefabPath =
            ResourceFolder + "/SeabornSloopVisual.prefab";

        private static bool isBuilding;
        private static bool isScheduled;

        static SeabornSloopAssetSetup()
        {
            ScheduleEnsure();
        }

        [MenuItem("Seaborn/Art/Rebuild Seaborn Sloop Assets")]
        private static void Rebuild()
        {
            Build(true);
        }

        internal static void ScheduleEnsure()
        {
            if (isScheduled) return;
            isScheduled = true;
            EditorApplication.delayCall += () =>
            {
                isScheduled = false;
                if (!isBuilding)
                {
                    Build(false);
                }
            };
        }

        private static void Build(bool force)
        {
            if (isBuilding || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) == null ||
                AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorPath) == null ||
                AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath) == null ||
                AssetDatabase.LoadAssetAtPath<Texture2D>(MetallicPath) == null ||
                AssetDatabase.LoadAssetAtPath<Texture2D>(RoughnessPath) == null)
            {
                return;
            }

            if (!force &&
                AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath) != null &&
                AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                return;
            }

            isBuilding = true;
            try
            {
                EnsureFolder(MaterialFolder);
                EnsureFolder(ResourceFolder);
                ConfigureSourceImports();

                if (force ||
                    AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath) == null)
                {
                    CreateMaskMap();
                }

                Material material = CreateOrUpdateMaterial();
                CreateOrUpdatePrefab(material);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log(
                    "Seaborn Sloop assetleri hazır: PBR materyal, mask map ve runtime prefab oluşturuldu."
                );
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                isBuilding = false;
            }
        }

        private static void ConfigureSourceImports()
        {
            ConfigureTexture(BaseColorPath, true, false);
            ConfigureTexture(NormalPath, false, true);
            ConfigureTexture(MetallicPath, false, false);
            ConfigureTexture(RoughnessPath, false, false);

            ModelImporter importer =
                AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.materialImportMode !=
                    ModelImporterMaterialImportMode.None)
                {
                    importer.materialImportMode =
                        ModelImporterMaterialImportMode.None;
                    dirty = true;
                }

                if (importer.importCameras)
                {
                    importer.importCameras = false;
                    dirty = true;
                }

                if (importer.importLights)
                {
                    importer.importLights = false;
                    dirty = true;
                }

                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static void ConfigureTexture(
            string path,
            bool srgb,
            bool normalMap)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            TextureImporterType targetType =
                normalMap
                    ? TextureImporterType.NormalMap
                    : TextureImporterType.Default;

            bool dirty =
                importer.sRGBTexture != srgb ||
                importer.textureType != targetType;

            importer.sRGBTexture = srgb;
            importer.textureType = targetType;
            importer.mipmapEnabled = true;
            importer.alphaIsTransparency = false;

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }

        private static void CreateMaskMap()
        {
            Texture2D metallic =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    MetallicPath
                );
            Texture2D roughness =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    RoughnessPath
                );

            Texture2D metallicCopy =
                ReadTexture(metallic);
            Texture2D roughnessCopy =
                ReadTexture(roughness);

            int width = Mathf.Min(
                metallicCopy.width,
                roughnessCopy.width
            );
            int height = Mathf.Min(
                metallicCopy.height,
                roughnessCopy.height
            );

            Color32[] metallicPixels =
                metallicCopy.GetPixels32();
            Color32[] roughnessPixels =
                roughnessCopy.GetPixels32();
            Color32[] maskPixels =
                new Color32[width * height];

            for (int index = 0;
                 index < maskPixels.Length;
                 index++)
            {
                byte metal = metallicPixels[index].r;
                byte smoothness =
                    (byte)(255 - roughnessPixels[index].r);
                maskPixels[index] = new Color32(
                    metal,
                    255,
                    255,
                    smoothness
                );
            }

            Texture2D mask = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                true,
                true
            );
            mask.name = "T_SeabornSloop_MaskMap";
            mask.SetPixels32(maskPixels);
            mask.Apply(true, false);

            File.WriteAllBytes(
                Path.GetFullPath(MaskPath),
                mask.EncodeToPNG()
            );

            UnityEngine.Object.DestroyImmediate(mask);
            UnityEngine.Object.DestroyImmediate(metallicCopy);
            UnityEngine.Object.DestroyImmediate(roughnessCopy);

            AssetDatabase.ImportAsset(
                MaskPath,
                ImportAssetOptions.ForceUpdate
            );
            ConfigureTexture(MaskPath, false, false);
        }

        private static Texture2D ReadTexture(Texture2D source)
        {
            RenderTexture temporary =
                RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear
                );
            Graphics.Blit(source, temporary);

            RenderTexture previous =
                RenderTexture.active;
            RenderTexture.active = temporary;

            Texture2D copy = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false,
                true
            );
            copy.ReadPixels(
                new Rect(
                    0f,
                    0f,
                    source.width,
                    source.height
                ),
                0,
                0
            );
            copy.Apply(false, false);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return copy;
        }

        private static Material CreateOrUpdateMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    MaterialPath
                );
            Shader shader = Shader.Find(
                "Universal Render Pipeline/Lit"
            );
            if (shader == null)
            {
                throw new InvalidOperationException(
                    "URP Lit shader bulunamadı."
                );
            }

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "MAT_SeabornSloop_URP"
                };
                AssetDatabase.CreateAsset(
                    material,
                    MaterialPath
                );
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture(
                "_BaseMap",
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    BaseColorPath
                )
            );
            material.SetTexture(
                "_BumpMap",
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    NormalPath
                )
            );
            material.SetTexture(
                "_MetallicGlossMap",
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    MaskPath
                )
            );
            material.SetFloat("_BumpScale", 1f);
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat(
                "_SmoothnessTextureChannel",
                0f
            );
            material.SetFloat("_Cull", 0f);
            material.SetColor("_BaseColor", Color.white);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword(
                "_METALLICSPECGLOSSMAP"
            );
            material.doubleSidedGI = true;
            material.enableInstancing = true;

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateOrUpdatePrefab(
            Material material)
        {
            GameObject model =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    ModelPath
                );
            if (model == null)
            {
                throw new InvalidOperationException(
                    "Seaborn Sloop FBX bulunamadı."
                );
            }

            GameObject root =
                new GameObject("SeabornSloopVisual");
            GameObject instance =
                PrefabUtility.InstantiatePrefab(model)
                    as GameObject;

            if (instance == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                throw new InvalidOperationException(
                    "Seaborn Sloop FBX örneklenemedi."
                );
            }

            instance.name = "Model";
            instance.transform.SetParent(
                root.transform,
                false
            );
            instance.transform.localPosition =
                Vector3.zero;
            instance.transform.localRotation =
                Quaternion.identity;
            instance.transform.localScale =
                Vector3.one;

            foreach (Collider collider in
                     root.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            foreach (Renderer renderer in
                     root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] assigned =
                    new Material[
                        renderer.sharedMaterials.Length
                    ];
                for (int index = 0;
                     index < assigned.Length;
                     index++)
                {
                    assigned[index] = material;
                }

                renderer.sharedMaterials = assigned;
                renderer.shadowCastingMode =
                    ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            PrefabUtility.SaveAsPrefabAsset(
                root,
                PrefabPath
            );
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int index = 1;
                 index < parts.Length;
                 index++)
            {
                string next =
                    current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[index]
                    );
                }

                current = next;
            }
        }
    }

    internal sealed class SeabornSloopAssetPostprocessor :
        AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (path.StartsWith(
                        "Assets/_Project/Art/Ships/SeabornSloop",
                        StringComparison.Ordinal))
                {
                    SeabornSloopAssetSetup.ScheduleEnsure();
                    return;
                }
            }
        }
    }
}
