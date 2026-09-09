using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipWaterContactVfx : MonoBehaviour
    {
        private const string ParticleShaderName =
            "Universal Render Pipeline/Particles/Unlit";

        [Header("Waterline")]
        [SerializeField]
        private float waterHeight;

        [SerializeField]
        private Vector3 bowLocalPosition =
            new Vector3(0f, -0.48f, 0.52f);

        [SerializeField]
        private Vector3 sternLocalPosition =
            new Vector3(0f, -0.46f, -0.52f);

        [SerializeField, Min(0.1f)]
        private float referenceSpeed = 5f;

        [Header("Emission")]
        [SerializeField, Min(0f)]
        private float maximumBowRate = 90f;

        [SerializeField, Min(0f)]
        private float maximumWakeRate = 68f;

        [SerializeField, Min(0f)]
        private float turnWakeBoost = 0.55f;

        private Rigidbody shipRigidbody;
        private ParticleSystem bowFoam;
        private ParticleSystem portWake;
        private ParticleSystem starboardWake;
        private Material particleMaterial;

        private Vector3 previousPosition;
        private Vector3 previousForward;
        private float smoothedSpeed;
        private float smoothedTurn;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            previousPosition = transform.position;
            previousForward = transform.forward;

            particleMaterial = CreateParticleMaterial();
            bowFoam = CreateBowFoam();
            portWake = CreateWake("Port Wake");
            starboardWake = CreateWake("Starboard Wake");
        }

        private void LateUpdate()
        {
            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 frameVelocity =
                (transform.position - previousPosition) / deltaTime;

            Vector3 velocity = shipRigidbody != null &&
                !shipRigidbody.isKinematic
                    ? shipRigidbody.linearVelocity
                    : frameVelocity;

            Vector3 planarVelocity =
                Vector3.ProjectOnPlane(velocity, Vector3.up);

            float targetSpeed = Mathf.Clamp01(
                planarVelocity.magnitude /
                Mathf.Max(0.1f, referenceSpeed)
            );

            float signedTurn = Vector3.SignedAngle(
                previousForward,
                transform.forward,
                Vector3.up
            ) / deltaTime;

            float targetTurn = Mathf.Clamp01(
                Mathf.Abs(signedTurn) / 70f
            );

            smoothedSpeed = Mathf.MoveTowards(
                smoothedSpeed,
                targetSpeed,
                deltaTime * 2.8f
            );
            smoothedTurn = Mathf.MoveTowards(
                smoothedTurn,
                targetTurn,
                deltaTime * 4f
            );

            PositionAtWaterline(
                bowFoam.transform,
                bowLocalPosition
            );
            PositionAtWaterline(
                portWake.transform,
                sternLocalPosition +
                Vector3.left * 0.24f
            );
            PositionAtWaterline(
                starboardWake.transform,
                sternLocalPosition +
                Vector3.right * 0.24f
            );

            float forwardMotion = Mathf.Clamp01(
                Mathf.Abs(
                    Vector3.Dot(
                        planarVelocity,
                        transform.forward
                    )
                ) / Mathf.Max(0.1f, referenceSpeed)
            );

            SetEmissionRate(
                bowFoam,
                maximumBowRate *
                smoothedSpeed *
                Mathf.Lerp(0.35f, 1f, forwardMotion)
            );

            float wakeIntensity = Mathf.Clamp01(
                smoothedSpeed +
                smoothedTurn * turnWakeBoost
            );

            float turnBalance = Mathf.Clamp(
                signedTurn / 70f,
                -1f,
                1f
            );

            SetEmissionRate(
                portWake,
                maximumWakeRate *
                wakeIntensity *
                (1f + Mathf.Max(0f, turnBalance) * 0.65f)
            );
            SetEmissionRate(
                starboardWake,
                maximumWakeRate *
                wakeIntensity *
                (1f + Mathf.Max(0f, -turnBalance) * 0.65f)
            );

            Vector3 wakeDirection =
                planarVelocity.sqrMagnitude > 0.04f
                    ? -planarVelocity.normalized
                    : -transform.forward;

            Quaternion wakeRotation = Quaternion.LookRotation(
                wakeDirection,
                Vector3.up
            );

            portWake.transform.rotation = wakeRotation;
            starboardWake.transform.rotation = wakeRotation;

            bowFoam.transform.rotation = Quaternion.LookRotation(
                (transform.forward + Vector3.up * 0.22f).normalized,
                Vector3.up
            );

            previousPosition = transform.position;
            previousForward = transform.forward;
        }

        private void PositionAtWaterline(
            Transform effectTransform,
            Vector3 normalizedLocalPosition)
        {
            Vector3 localPosition = new Vector3(
                normalizedLocalPosition.x,
                0f,
                normalizedLocalPosition.z
            );

            Vector3 worldPosition =
                transform.TransformPoint(localPosition);
            worldPosition.y = waterHeight + 0.065f;
            effectTransform.position = worldPosition;
        }

        private ParticleSystem CreateBowFoam()
        {
            ParticleSystem system = CreateSystem(
                "Bow Foam",
                new Color(0.72f, 0.9f, 0.92f, 0.72f),
                0.34f,
                0.9f,
                0.14f,
                0.38f,
                0.55f,
                1.45f,
                34f
            );

            ParticleSystem.ShapeModule shape = system.shape;
            shape.radius = 0.26f;
            return system;
        }

        private ParticleSystem CreateWake(string objectName)
        {
            ParticleSystem system = CreateSystem(
                objectName,
                new Color(0.62f, 0.84f, 0.86f, 0.55f),
                0.85f,
                2.1f,
                0.2f,
                0.55f,
                0.25f,
                0.8f,
                22f
            );

            ParticleSystem.ShapeModule shape = system.shape;
            shape.radius = 0.18f;

            ParticleSystem.SizeOverLifetimeModule size =
                system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.35f),
                    new Keyframe(0.25f, 1f),
                    new Keyframe(1f, 1.75f)
                )
            );

            return system;
        }

        private ParticleSystem CreateSystem(
            string objectName,
            Color color,
            float minimumLifetime,
            float maximumLifetime,
            float minimumSize,
            float maximumSize,
            float minimumSpeed,
            float maximumSpeed,
            float coneAngle)
        {
            GameObject effectObject = new GameObject(objectName);
            effectObject.transform.SetParent(transform, true);

            ParticleSystem system =
                effectObject.AddComponent<ParticleSystem>();

            system.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
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
            main.gravityModifier = -0.025f;
            main.maxParticles = 320;

            ParticleSystem.EmissionModule emission =
                system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = 0.1f;

            ParticleSystem.ColorOverLifetimeModule fade =
                system.colorOverLifetime;
            fade.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(color.a, 0.12f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            fade.color = gradient;

            ParticleSystemRenderer renderer =
                effectObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode =
                ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 2;

            if (particleMaterial != null)
            {
                renderer.sharedMaterial = particleMaterial;
            }

            system.Play();
            return system;
        }

        private Material CreateParticleMaterial()
        {
            Shader shader = Shader.Find(ParticleShaderName);

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = "Runtime Ocean Foam"
            };
            material.color = Color.white;
            return material;
        }

        private static void SetEmissionRate(
            ParticleSystem system,
            float rate)
        {
            ParticleSystem.EmissionModule emission =
                system.emission;
            emission.rateOverTime = Mathf.Max(0f, rate);
        }

        private void OnDestroy()
        {
            if (particleMaterial != null)
            {
                Destroy(particleMaterial);
            }
        }
    }

    internal sealed class ShipWaterContactVfxBootstrap :
        MonoBehaviour
    {
        private const float ScanInterval = 1f;
        private float nextScanTime;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBootstrap()
        {
            GameObject bootstrapObject =
                new GameObject("Ship Water Contact VFX Bootstrap");
            DontDestroyOnLoad(bootstrapObject);
            bootstrapObject.AddComponent<
                ShipWaterContactVfxBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + ScanInterval;

            ShipHealth[] ships = FindObjectsByType<ShipHealth>(
                FindObjectsSortMode.None
            );

            foreach (ShipHealth ship in ships)
            {
                if (ship.GetComponent<ShipWaterContactVfx>() ==
                    null &&
                    ship.GetComponent<Rigidbody>() != null)
                {
                    ship.gameObject.AddComponent<
                        ShipWaterContactVfx>();
                }
            }
        }
    }
}
