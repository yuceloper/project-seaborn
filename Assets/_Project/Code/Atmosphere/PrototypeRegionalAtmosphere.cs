using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Atmosphere
{
    [DisallowMultipleComponent]
    public sealed class PrototypeRegionalAtmosphere :
        MonoBehaviour
    {
        private Material runtimeOceanMaterial;

        public static void Apply(
            string sceneName,
            UnityEngine.Camera activeCamera)
        {
            GameObject ocean = GameObject.Find("Ocean");
            if (ocean == null)
            {
                return;
            }

            PrototypeRegionalAtmosphere atmosphere =
                ocean.GetComponent<
                    PrototypeRegionalAtmosphere>();
            if (atmosphere == null)
            {
                atmosphere = ocean.AddComponent<
                    PrototypeRegionalAtmosphere>();
            }

            atmosphere.ApplyProfile(
                ProfileFor(sceneName),
                activeCamera
            );
            PrototypeAtmospherePostProcessing
                .EnsureForActiveScene();
        }

        private void ApplyProfile(
            AtmosphereProfile profile,
            UnityEngine.Camera activeCamera)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogStartDistance =
                profile.FogStart;
            RenderSettings.fogEndDistance =
                profile.FogEnd;
            RenderSettings.ambientMode =
                AmbientMode.Trilight;
            RenderSettings.ambientSkyColor =
                Color.Lerp(profile.AmbientSky, new Color(0.40f, 0.44f, 0.48f), 0.45f);
            RenderSettings.ambientEquatorColor =
                Color.Lerp(profile.AmbientEquator, new Color(0.26f, 0.30f, 0.33f), 0.4f);
            RenderSettings.ambientGroundColor =
                profile.AmbientGround;
            RenderSettings.reflectionIntensity =
                profile.ReflectionIntensity;

            if (activeCamera == null)
            {
                activeCamera = UnityEngine.Camera.main;
            }
            if (activeCamera != null)
            {
                activeCamera.backgroundColor =
                    profile.FogColor;
            }

            Light sun = FindSun();
            if (sun != null)
            {
                sun.color = Color.Lerp(profile.SunColor, Color.white, 0.72f);
                sun.intensity = profile.SunIntensity;
                sun.transform.rotation =
                    Quaternion.Euler(profile.SunRotation);
                RenderSettings.sun = sun;
            }

            Renderer oceanRenderer =
                GetComponent<Renderer>();
            if (oceanRenderer == null)
            {
                oceanRenderer =
                    GetComponentInChildren<Renderer>();
            }
            if (oceanRenderer == null ||
                oceanRenderer.sharedMaterial == null)
            {
                return;
            }

            if (runtimeOceanMaterial != null)
            {
                Destroy(runtimeOceanMaterial);
            }

            runtimeOceanMaterial = new Material(
                oceanRenderer.sharedMaterial)
            {
                name =
                    $"Runtime Ocean - {profile.Label}"
            };
            oceanRenderer.sharedMaterial =
                runtimeOceanMaterial;

            SetColor("_DeepColor", profile.DeepColor);
            SetColor(
                "_ShallowColor",
                profile.ShallowColor
            );
            SetColor(
                "_HorizonColor",
                profile.HorizonColor
            );
            SetColor(
                "_SunGlintColor",
                profile.GlintColor
            );
            SetFloat(
                "_SunGlintStrength",
                profile.GlintStrength * 0.78f
            );
            SetFloat(
                "_WaveAmplitudeA",
                profile.LargeWave
            );
            SetFloat(
                "_WaveAmplitudeB",
                profile.SmallWave
            );
            SetFloat("_WaveSpeedA", profile.WaveSpeedA);
            SetFloat("_WaveSpeedB", profile.WaveSpeedB);

            OceanHorizonExtension.Ensure(oceanRenderer, activeCamera);

            // Calm harbor -> open sea -> rough western waters.
            float roughness = Mathf.InverseLerp(0.12f, 0.22f, profile.LargeWave);
            SetFloat("_RippleStrength", Mathf.Lerp(0.12f, 0.22f, roughness));
            SetFloat("_RippleScale", 1.15f);
            SetFloat("_SkyReflection", Mathf.Lerp(0.38f, 0.28f, roughness));
            SetFloat("_WhitecapStrength", Mathf.Lerp(0.008f, 0.045f, roughness));
            SetFloat("_SunGlintPower", 48f);
            SetFloat("_FresnelPower", 4.5f);


            Debug.Log(
                $"Bölgesel atmosfer: {profile.Label}.",
                this
            );
        }

        private static Light FindSun()
        {
            if (RenderSettings.sun != null)
            {
                return RenderSettings.sun;
            }

            Light[] lights = FindObjectsByType<Light>(
                FindObjectsSortMode.None
            );
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type ==
                    LightType.Directional)
                {
                    return lights[i];
                }
            }

            return null;
        }

        private void SetColor(string property, Color value)
        {
            if (runtimeOceanMaterial.HasProperty(property))
            {
                runtimeOceanMaterial.SetColor(
                    property,
                    value
                );
            }
        }

        private void SetFloat(string property, float value)
        {
            if (runtimeOceanMaterial.HasProperty(property))
            {
                runtimeOceanMaterial.SetFloat(
                    property,
                    value
                );
            }
        }

        private static AtmosphereProfile ProfileFor(
            string sceneName)
        {
            return sceneName switch
            {
                "PrototypeHarbor" =>
                    AtmosphereProfile.Harbor(),
                "PrototypeWesternReach" =>
                    AtmosphereProfile.West(),
                "PrototypeEasternReach" =>
                    AtmosphereProfile.East(),
                _ => AtmosphereProfile.Central()
            };
        }

        private void OnDestroy()
        {
            if (runtimeOceanMaterial != null)
            {
                Destroy(runtimeOceanMaterial);
            }
        }

        private readonly struct AtmosphereProfile
        {
            public readonly string Label;
            public readonly Color DeepColor;
            public readonly Color ShallowColor;
            public readonly Color HorizonColor;
            public readonly Color GlintColor;
            public readonly float GlintStrength;
            public readonly float LargeWave;
            public readonly float SmallWave;
            public readonly float WaveSpeedA;
            public readonly float WaveSpeedB;
            public readonly Color FogColor;
            public readonly float FogStart;
            public readonly float FogEnd;
            public readonly Color AmbientSky;
            public readonly Color AmbientEquator;
            public readonly Color AmbientGround;
            public readonly float ReflectionIntensity;
            public readonly Color SunColor;
            public readonly float SunIntensity;
            public readonly Vector3 SunRotation;

            private AtmosphereProfile(
                string label,
                Color deepColor,
                Color shallowColor,
                Color horizonColor,
                Color glintColor,
                float glintStrength,
                float largeWave,
                float smallWave,
                float waveSpeedA,
                float waveSpeedB,
                Color fogColor,
                float fogStart,
                float fogEnd,
                Color ambientSky,
                Color ambientEquator,
                Color ambientGround,
                float reflectionIntensity,
                Color sunColor,
                float sunIntensity,
                Vector3 sunRotation)
            {
                Label = label;
                DeepColor = deepColor;
                ShallowColor = shallowColor;
                HorizonColor = horizonColor;
                GlintColor = glintColor;
                GlintStrength = glintStrength;
                LargeWave = largeWave;
                SmallWave = smallWave;
                WaveSpeedA = waveSpeedA;
                WaveSpeedB = waveSpeedB;
                FogColor = fogColor;
                FogStart = fogStart;
                FogEnd = fogEnd;
                AmbientSky = ambientSky;
                AmbientEquator = ambientEquator;
                AmbientGround = ambientGround;
                ReflectionIntensity = reflectionIntensity;
                SunColor = sunColor;
                SunIntensity = sunIntensity;
                SunRotation = sunRotation;
            }

            public static AtmosphereProfile Harbor() =>
                new(
                    "Seaborn Limanı",
                    new Color(0.018f, 0.105f, 0.16f),
                    new Color(0.055f, 0.29f, 0.32f),
                    new Color(0.28f, 0.43f, 0.5f),
                    new Color(1f, 0.68f, 0.38f),
                    0.32f,
                    0.12f,
                    0.045f,
                    0.48f,
                    0.82f,
                    new Color(0.34f, 0.39f, 0.4f),
                    34f,
                    96f,
                    new Color(0.42f, 0.34f, 0.3f),
                    new Color(0.22f, 0.25f, 0.25f),
                    new Color(0.06f, 0.075f, 0.075f),
                    0.72f,
                    new Color(1f, 0.76f, 0.55f),
                    1.3f,
                    new Vector3(42f, -32f, 0f)
                );

            public static AtmosphereProfile Central() =>
                new(
                    "Merkez Sular",
                    new Color(0.014f, 0.075f, 0.15f),
                    new Color(0.04f, 0.24f, 0.30f),
                    new Color(0.26f, 0.43f, 0.56f),
                    new Color(1f, 0.72f, 0.42f),
                    0.28f,
                    0.18f,
                    0.06f,
                    0.7f,
                    1.15f,
                    new Color(0.29f, 0.37f, 0.41f),
                    24f,
                    82f,
                    new Color(0.34f, 0.29f, 0.28f),
                    new Color(0.19f, 0.23f, 0.25f),
                    new Color(0.055f, 0.075f, 0.08f),
                    0.65f,
                    new Color(1f, 0.77f, 0.58f),
                    1.35f,
                    new Vector3(48f, -28f, 0f)
                );

            public static AtmosphereProfile East() =>
                new(
                    "Doğu Avları",
                    new Color(0.012f, 0.115f, 0.20f),
                    new Color(0.05f, 0.35f, 0.38f),
                    new Color(0.28f, 0.49f, 0.57f),
                    new Color(1f, 0.81f, 0.5f),
                    0.36f,
                    0.14f,
                    0.05f,
                    0.58f,
                    0.96f,
                    new Color(0.36f, 0.48f, 0.5f),
                    38f,
                    108f,
                    new Color(0.4f, 0.38f, 0.33f),
                    new Color(0.22f, 0.3f, 0.3f),
                    new Color(0.055f, 0.09f, 0.09f),
                    0.78f,
                    new Color(1f, 0.84f, 0.64f),
                    1.48f,
                    new Vector3(40f, -48f, 0f)
                );

            public static AtmosphereProfile West() =>
                new(
                    "Batı Sınırı",
                    new Color(0.012f, 0.055f, 0.10f),
                    new Color(0.04f, 0.16f, 0.23f),
                    new Color(0.20f, 0.30f, 0.40f),
                    new Color(0.74f, 0.66f, 0.54f),
                    0.16f,
                    0.22f,
                    0.075f,
                    0.82f,
                    1.3f,
                    new Color(0.19f, 0.27f, 0.32f),
                    18f,
                    66f,
                    new Color(0.2f, 0.23f, 0.27f),
                    new Color(0.12f, 0.17f, 0.2f),
                    new Color(0.025f, 0.045f, 0.06f),
                    0.48f,
                    new Color(0.76f, 0.78f, 0.82f),
                    0.95f,
                    new Vector3(55f, -18f, 0f)
                );
        }
    }
}
