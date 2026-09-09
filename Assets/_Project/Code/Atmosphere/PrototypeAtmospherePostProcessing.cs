using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Seaborn.Atmosphere
{
    [DisallowMultipleComponent]
    public sealed class PrototypeAtmospherePostProcessing :
        MonoBehaviour
    {
        [Header("Color")]
        [SerializeField, Range(-1f, 1f)]
        private float postExposure = -0.04f;

        [SerializeField, Range(-100f, 100f)]
        private float contrast = 7f;

        [SerializeField, Range(-100f, 100f)]
        private float saturation = -4f;

        [SerializeField]
        private Color colorFilter =
            new Color(1f, 0.97f, 0.92f, 1f);

        [Header("Bloom")]
        [SerializeField, Min(0f)]
        private float bloomThreshold = 1.15f;

        [SerializeField, Range(0f, 1f)]
        private float bloomIntensity = 0.09f;

        [SerializeField, Range(0f, 1f)]
        private float bloomScatter = 0.52f;

        [Header("Framing")]
        [SerializeField, Range(0f, 1f)]
        private float vignetteIntensity = 0.115f;

        [SerializeField, Range(0.01f, 1f)]
        private float vignetteSmoothness = 0.44f;

        private VolumeProfile runtimeProfile;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateForPrototypeOcean()
        {
            if (GameObject.Find("Ocean") == null ||
                FindFirstObjectByType<
                    PrototypeAtmospherePostProcessing>() != null)
            {
                return;
            }

            GameObject volumeObject =
                new GameObject(
                    "Prototype Atmosphere Post Processing"
                );
            volumeObject.AddComponent<
                PrototypeAtmospherePostProcessing>();
        }

        private void Awake()
        {
            runtimeProfile =
                ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeProfile.name =
                "Runtime Prototype Atmosphere Profile";

            ColorAdjustments color =
                runtimeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(postExposure);
            color.contrast.Override(contrast);
            color.saturation.Override(saturation);
            color.colorFilter.Override(colorFilter);

            WhiteBalance whiteBalance =
                runtimeProfile.Add<WhiteBalance>(true);
            whiteBalance.temperature.Override(3f);
            whiteBalance.tint.Override(-1f);

            Bloom bloom =
                runtimeProfile.Add<Bloom>(true);
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(bloomScatter);
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
                vignetteIntensity
            );
            vignette.smoothness.Override(
                vignetteSmoothness
            );
            vignette.rounded.Override(false);

            Volume volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 20f;
            volume.weight = 1f;
            volume.sharedProfile = runtimeProfile;
        }

        private void OnDestroy()
        {
            if (runtimeProfile != null)
            {
                Destroy(runtimeProfile);
            }
        }
    }
}
