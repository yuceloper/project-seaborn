using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Seaborn.Atmosphere
{
    [DisallowMultipleComponent]
    public sealed class PrototypeAtmospherePostProcessing :
        MonoBehaviour
    {
        private VolumeProfile runtimeProfile;
        private Volume runtimeVolume;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateForPrototypeOcean()
        {
            EnsureForActiveScene();
        }

        public static void EnsureForActiveScene()
        {
            if (GameObject.Find("Ocean") == null)
            {
                return;
            }

            PrototypeAtmospherePostProcessing instance =
                FindFirstObjectByType<
                    PrototypeAtmospherePostProcessing>();

            if (instance == null)
            {
                GameObject volumeObject =
                    new GameObject(
                        "Prototype Atmosphere Post Processing"
                    );
                instance = volumeObject.AddComponent<
                    PrototypeAtmospherePostProcessing>();
            }
            else
            {
                instance.RebuildProfile();
            }
        }

        private void Awake()
        {
            RebuildProfile();
        }

        private void RebuildProfile()
        {
            if (runtimeVolume != null)
            {
                Destroy(runtimeVolume);
            }
            if (runtimeProfile != null)
            {
                Destroy(runtimeProfile);
            }

            PostProfile profile = PostProfile.ForScene(
                SceneManager.GetActiveScene().name
            );

            runtimeProfile =
                ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeProfile.name =
                $"Runtime Post - {profile.Label}";

            ColorAdjustments color =
                runtimeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(
                profile.Exposure);
            color.contrast.Override(profile.Contrast);
            color.saturation.Override(
                profile.Saturation);
            color.colorFilter.Override(
                Color.Lerp(profile.ColorFilter, Color.white, 0.8f));

            WhiteBalance whiteBalance =
                runtimeProfile.Add<WhiteBalance>(true);
            whiteBalance.temperature.Override(
                profile.Temperature * 0.2f);
            whiteBalance.tint.Override(profile.Tint);

            Bloom bloom =
                runtimeProfile.Add<Bloom>(true);
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(
                profile.BloomIntensity);
            bloom.scatter.Override(0.52f);
            bloom.highQualityFiltering.Override(false);
            bloom.dirtIntensity.Override(0f);

            Vignette vignette =
                runtimeProfile.Add<Vignette>(true);
            vignette.color.Override(
                new Color(0.01f, 0.025f, 0.03f, 1f)
            );
            vignette.center.Override(
                new Vector2(0.5f, 0.5f)
            );
            vignette.intensity.Override(
                profile.Vignette);
            vignette.smoothness.Override(0.44f);
            vignette.rounded.Override(false);

            runtimeVolume =
                gameObject.AddComponent<Volume>();
            runtimeVolume.isGlobal = true;
            runtimeVolume.priority = 20f;
            runtimeVolume.weight = 1f;
            runtimeVolume.sharedProfile = runtimeProfile;
        }

        private void OnDestroy()
        {
            if (runtimeProfile != null)
            {
                Destroy(runtimeProfile);
            }
        }

        private readonly struct PostProfile
        {
            public readonly string Label;
            public readonly float Exposure;
            public readonly float Contrast;
            public readonly float Saturation;
            public readonly Color ColorFilter;
            public readonly float Temperature;
            public readonly float Tint;
            public readonly float BloomIntensity;
            public readonly float Vignette;

            private PostProfile(
                string label,
                float exposure,
                float contrast,
                float saturation,
                Color colorFilter,
                float temperature,
                float tint,
                float bloomIntensity,
                float vignette)
            {
                Label = label;
                Exposure = exposure;
                Contrast = contrast;
                Saturation = saturation;
                ColorFilter = colorFilter;
                Temperature = temperature;
                Tint = tint;
                BloomIntensity = bloomIntensity;
                Vignette = vignette;
            }

            public static PostProfile ForScene(
                string sceneName)
            {
                return sceneName switch
                {
                    "PrototypeHarbor" => new(
                        "Seaborn Limanı",
                        0.01f,
                        6f,
                        -2f,
                        new Color(1f, 0.97f, 0.91f),
                        5f,
                        -1f,
                        0.08f,
                        0.09f
                    ),
                    "PrototypeEasternReach" => new(
                        "Doğu Avları",
                        0.08f,
                        5f,
                        3f,
                        new Color(0.94f, 1f, 0.98f),
                        1f,
                        -3f,
                        0.1f,
                        0.075f
                    ),
                    "PrototypeWesternReach" => new(
                        "Batı Sınırı",
                        -0.16f,
                        10f,
                        -13f,
                        new Color(0.86f, 0.93f, 1f),
                        -8f,
                        -2f,
                        0.055f,
                        0.14f
                    ),
                    _ => new(
                        "Merkez Sular",
                        -0.04f,
                        7f,
                        -4f,
                        new Color(1f, 0.97f, 0.92f),
                        3f,
                        -1f,
                        0.09f,
                        0.115f
                    )
                };
            }
        }
    }
}
