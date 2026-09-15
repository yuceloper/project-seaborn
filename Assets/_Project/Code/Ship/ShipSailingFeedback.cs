using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipSailingFeedback : MonoBehaviour
    {
        [Header("Wake")]
        [SerializeField, Min(0f)] private float wakeStartSpeed = 0.7f;
        [SerializeField, Min(0.1f)] private float fullWakeSpeed = 8f;
        [SerializeField, Min(0f)] private float sternOffset = 2.35f;
        [SerializeField, Min(0f)] private float wakeHalfWidth = 0.58f;
        [SerializeField, Min(0f)] private float waterlineOffset = 0.035f;
        [SerializeField, Range(0.05f, 0.8f)] private float maximumWakeWidth = 0.24f;
        [SerializeField, Range(0.2f, 3f)] private float wakeLifetime = 1.25f;

        private Rigidbody shipRigidbody;
        private TrailRenderer portWake;
        private TrailRenderer starboardWake;
        private Material wakeMaterial;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            BuildWake();
        }

        private void LateUpdate()
        {
            if (shipRigidbody == null)
            {
                SetWake(false, 0f);
                return;
            }

            float forwardSpeed = Mathf.Max(
                0f,
                Vector3.Dot(
                    shipRigidbody.linearVelocity,
                    transform.forward
                )
            );
            float strength = Mathf.InverseLerp(
                wakeStartSpeed,
                fullWakeSpeed,
                forwardSpeed
            );
            SetWake(strength > 0.01f, strength);
        }

        private void BuildWake()
        {
            Shader shader = Shader.Find(
                "Universal Render Pipeline/Unlit"
            );
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            wakeMaterial = new Material(shader)
            {
                name = "Runtime Ship Wake"
            };
            ConfigureTransparentMaterial(wakeMaterial);

            portWake = CreateWakeTrail(
                "Port Wake",
                new Vector3(
                    -wakeHalfWidth,
                    waterlineOffset,
                    -sternOffset
                )
            );
            starboardWake = CreateWakeTrail(
                "Starboard Wake",
                new Vector3(
                    wakeHalfWidth,
                    waterlineOffset,
                    -sternOffset
                )
            );
        }

        private TrailRenderer CreateWakeTrail(
            string trailName,
            Vector3 localPosition)
        {
            GameObject trailObject = new GameObject(trailName);
            trailObject.transform.SetParent(transform, false);
            trailObject.transform.localPosition = localPosition;

            TrailRenderer trail =
                trailObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = wakeMaterial;
            trail.time = wakeLifetime;
            trail.minVertexDistance = 0.12f;
            trail.widthCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.58f, 0.55f),
                new Keyframe(1f, 0f)
            );
            trail.colorGradient = CreateWakeGradient();
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.generateLightingData = false;
            trail.emitting = false;
            return trail;
        }

        private void SetWake(bool emitting, float strength)
        {
            float width = maximumWakeWidth *
                Mathf.SmoothStep(0f, 1f, strength);
            UpdateTrail(portWake, emitting, width);
            UpdateTrail(starboardWake, emitting, width);
        }

        private static void UpdateTrail(
            TrailRenderer trail,
            bool emitting,
            float width)
        {
            if (trail == null) return;
            trail.widthMultiplier = width;
            trail.emitting = emitting;
        }

        private static Gradient CreateWakeGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.82f, 0.95f, 0.93f), 0f),
                    new GradientColorKey(
                        new Color(0.54f, 0.82f, 0.82f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.58f, 0f),
                    new GradientAlphaKey(0.28f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            return gradient;
        }

        private static void ConfigureTransparentMaterial(
            Material material)
        {
            if (material == null) return;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword(
                    "_SURFACE_TYPE_TRANSPARENT"
                );
            }

            material.renderQueue = 3000;
        }

        private void OnDisable()
        {
            SetWake(false, 0f);
            if (portWake != null) portWake.Clear();
            if (starboardWake != null) starboardWake.Clear();
        }

        private void OnDestroy()
        {
            if (wakeMaterial != null)
            {
                Destroy(wakeMaterial);
            }
        }
    }
}
