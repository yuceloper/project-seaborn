using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Combat
{
    public static class PrototypeCombatVfx
    {
        private const string ParticleShaderName =
            "Universal Render Pipeline/Particles/Unlit";

        public static void PlayMuzzleBurst(
            Vector3 position,
            Vector3 direction)
        {
            CreateBurst(
                "Muzzle Burst",
                position,
                direction,
                new Color(1f, 0.48f, 0.08f, 1f),
                14,
                0.12f,
                0.28f,
                1.5f,
                3.2f,
                0.08f,
                0.2f,
                0f
            );

            CreateBurst(
                "Muzzle Smoke",
                position,
                direction,
                new Color(0.55f, 0.56f, 0.58f, 0.75f),
                9,
                0.45f,
                0.8f,
                0.35f,
                1.1f,
                0.16f,
                0.38f,
                -0.08f
            );
        }

        public static void PlayHullImpact(
            Vector3 position,
            Vector3 surfaceDirection)
        {
            CreateBurst(
                "Hull Impact",
                position,
                surfaceDirection,
                new Color(1f, 0.72f, 0.2f, 1f),
                18,
                0.18f,
                0.45f,
                1.8f,
                4.2f,
                0.04f,
                0.12f,
                0.35f
            );

            CreateBurst(
                "Impact Smoke",
                position,
                Vector3.up,
                new Color(0.22f, 0.2f, 0.18f, 0.75f),
                7,
                0.45f,
                0.9f,
                0.25f,
                0.8f,
                0.15f,
                0.32f,
                -0.12f
            );
        }

        public static void PlayWaterSplash(Vector3 position)
        {
            CreateBurst(
                "Water Splash",
                position,
                Vector3.up,
                new Color(0.72f, 0.9f, 1f, 0.9f),
                22,
                0.35f,
                0.75f,
                1.2f,
                3.6f,
                0.05f,
                0.16f,
                0.8f
            );
        }

        public static void PlaySinkingSmoke(Vector3 position)
        {
            CreateBurst(
                "Sinking Smoke",
                position,
                Vector3.up,
                new Color(0.15f, 0.16f, 0.18f, 0.8f),
                28,
                0.8f,
                1.5f,
                0.25f,
                1.25f,
                0.2f,
                0.5f,
                -0.18f
            );
        }

        private static void CreateBurst(
            string objectName,
            Vector3 position,
            Vector3 direction,
            Color color,
            int particleCount,
            float minimumLifetime,
            float maximumLifetime,
            float minimumSpeed,
            float maximumSpeed,
            float minimumSize,
            float maximumSize,
            float gravityModifier)
        {
            Vector3 safeDirection =
                direction.sqrMagnitude > Mathf.Epsilon
                    ? direction.normalized
                    : Vector3.up;

            GameObject effectObject =
                new GameObject(objectName);

            effectObject.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(safeDirection)
            );

            ParticleSystem particleSystem =
                effectObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main =
                particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.1f;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;
            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    minimumLifetime,
                    maximumLifetime
                );
            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    minimumSpeed,
                    maximumSpeed
                );
            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    minimumSize,
                    maximumSize
                );
            main.startColor = color;
            main.gravityModifier = gravityModifier;

            ParticleSystem.EmissionModule emission =
                particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape =
                particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.08f;

            ParticleSystem.ColorOverLifetimeModule
                colorOverLifetime =
                    particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient alphaGradient = new Gradient();
            alphaGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = alphaGradient;

            ParticleSystemRenderer particleRenderer =
                effectObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode =
                ParticleSystemRenderMode.Billboard;
            particleRenderer.shadowCastingMode =
                ShadowCastingMode.Off;
            particleRenderer.receiveShadows = false;

            Shader particleShader =
                Shader.Find(ParticleShaderName);

            if (particleShader == null)
            {
                particleShader = Shader.Find("Sprites/Default");
            }

            Material particleMaterial = null;

            if (particleShader != null)
            {
                particleMaterial = new Material(particleShader);
                particleMaterial.color = color;
                particleRenderer.material = particleMaterial;
            }

            particleSystem.Emit(particleCount);
            particleSystem.Play();

            float destroyDelay = maximumLifetime + 0.5f;
            Object.Destroy(effectObject, destroyDelay);

            if (particleMaterial != null)
            {
                Object.Destroy(
                    particleMaterial,
                    destroyDelay
                );
            }
        }
    }
}
