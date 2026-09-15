using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipSailingFeedback : MonoBehaviour
    {
        private const string ParticleMaterialResourceName =
            "PrototypeCombatParticle";
        private const string ParticleShaderName =
            "Seaborn/Combat Particle";

        [Header("Wake")]
        [SerializeField, Min(0f)]
        private float wakeStartSpeed = 0.55f;

        [SerializeField, Min(0.1f)]
        private float fullWakeSpeed = 8f;

        [SerializeField, Min(0f)]
        private float sternOffset = 2.35f;

        [SerializeField, Min(0f)]
        private float wakeHalfWidth = 0.58f;

        [SerializeField, Min(0f)]
        private float waterlineOffset = 0.18f;

        [SerializeField, Range(1f, 30f)]
        private float maximumEmissionRate = 14f;

        private Rigidbody shipRigidbody;
        private ParticleSystem portWake;
        private ParticleSystem starboardWake;
        private Material wakeMaterial;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            BuildWake();
        }

        private void LateUpdate()
        {
            UpdateWakeAnchorPositions();

            if (shipRigidbody == null)
            {
                SetWakeStrength(0f);
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
            SetWakeStrength(strength);
        }

        private void BuildWake()
        {
            Material template = Resources.Load<Material>(
                ParticleMaterialResourceName
            );
            if (template != null)
            {
                wakeMaterial = new Material(template)
                {
                    name = "Runtime Ship Wake"
                };
            }
            else
            {
                Shader shader = Shader.Find(
                    ParticleShaderName
                );
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                }

                if (shader != null)
                {
                    wakeMaterial = new Material(shader)
                    {
                        name = "Runtime Ship Wake"
                    };
                }
            }

            portWake = CreateWakeEmitter("Port Wake");
            starboardWake = CreateWakeEmitter(
                "Starboard Wake"
            );
            UpdateWakeAnchorPositions();
        }

        private ParticleSystem CreateWakeEmitter(
            string emitterName)
        {
            GameObject emitterObject =
                new GameObject(emitterName);
            emitterObject.transform.SetParent(transform, false);

            ParticleSystem particles =
                emitterObject.AddComponent<ParticleSystem>();
            particles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;
            main.startLifetime =
                new ParticleSystem.MinMaxCurve(0.55f, 0.95f);
            main.startSpeed =
                new ParticleSystem.MinMaxCurve(0.03f, 0.16f);
            main.startSize =
                new ParticleSystem.MinMaxCurve(0.13f, 0.28f);
            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    new Color(0.72f, 0.92f, 0.94f, 0.68f),
                    new Color(0.42f, 0.74f, 0.78f, 0.42f)
                );
            main.gravityModifier = -0.015f;

            ParticleSystem.EmissionModule emission =
                particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape =
                particles.shape;
            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;
            shape.radius = 0.07f;
            shape.radiusThickness = 1f;

            ParticleSystem.ColorOverLifetimeModule color =
                particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(
                        new Color(0.62f, 0.84f, 0.86f),
                        1f
                    )
                },
                new[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0.35f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            color.color = gradient;

            ParticleSystem.SizeOverLifetimeModule size =
                particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.45f),
                    new Keyframe(0.35f, 1f),
                    new Keyframe(1f, 1.35f)
                )
            );

            ParticleSystemRenderer renderer =
                emitterObject.GetComponent<
                    ParticleSystemRenderer>();
            renderer.renderMode =
                ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode =
                ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 25;
            if (wakeMaterial != null)
            {
                renderer.sharedMaterial = wakeMaterial;
            }

            particles.Play();
            return particles;
        }

        private void UpdateWakeAnchorPositions()
        {
            Vector3 sternCenter =
                transform.position -
                transform.forward * sternOffset +
                Vector3.up * waterlineOffset;
            Vector3 sideOffset =
                transform.right * wakeHalfWidth;

            if (portWake != null)
            {
                portWake.transform.position =
                    sternCenter - sideOffset;
            }

            if (starboardWake != null)
            {
                starboardWake.transform.position =
                    sternCenter + sideOffset;
            }
        }

        private void SetWakeStrength(float strength)
        {
            float rate = maximumEmissionRate *
                Mathf.SmoothStep(0f, 1f, strength);
            SetEmissionRate(portWake, rate);
            SetEmissionRate(starboardWake, rate);
        }

        private static void SetEmissionRate(
            ParticleSystem particles,
            float rate)
        {
            if (particles == null) return;
            ParticleSystem.EmissionModule emission =
                particles.emission;
            emission.rateOverTime = rate;
        }

        private void OnDisable()
        {
            StopAndClear(portWake);
            StopAndClear(starboardWake);
        }

        private void OnEnable()
        {
            if (portWake != null) portWake.Play();
            if (starboardWake != null) starboardWake.Play();
        }

        private static void StopAndClear(
            ParticleSystem particles)
        {
            if (particles == null) return;
            particles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
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
