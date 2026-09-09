using UnityEngine;
using UnityEngine.Rendering;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipWaterContactVfx : MonoBehaviour
    {
        private const string TrailShaderName =
            "Universal Render Pipeline/Particles/Unlit";

        [Header("Waterline")]
        [SerializeField]
        private float waterHeight;

        [SerializeField]
        private Vector3 sternLocalPosition =
            new Vector3(0f, 0f, -0.52f);

        [SerializeField, Min(0.1f)]
        private float referenceSpeed = 5f;

        [Header("Wake")]
        [SerializeField, Min(0.01f)]
        private float wakeLifetime = 1.55f;

        [SerializeField, Min(0.01f)]
        private float maximumWakeWidth = 0.32f;

        [SerializeField, Min(0f)]
        private float minimumVisibleSpeed = 0.08f;

        [SerializeField, Min(0f)]
        private float turnWakeBoost = 0.45f;

        private Rigidbody shipRigidbody;
        private TrailRenderer portWake;
        private TrailRenderer starboardWake;
        private Material wakeMaterial;

        private Vector3 previousPosition;
        private Vector3 previousForward;
        private float smoothedSpeed;
        private float smoothedTurn;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            previousPosition = transform.position;
            previousForward = transform.forward;

            wakeMaterial = CreateWakeMaterial();
            portWake = CreateWake("Port Wake");
            starboardWake = CreateWake("Starboard Wake");
        }

        private void LateUpdate()
        {
            float deltaTime = Mathf.Max(
                Time.deltaTime,
                0.0001f
            );

            Vector3 frameVelocity =
                (transform.position - previousPosition) /
                deltaTime;

            Vector3 velocity =
                shipRigidbody != null &&
                !shipRigidbody.isKinematic
                    ? shipRigidbody.linearVelocity
                    : frameVelocity;

            Vector3 planarVelocity =
                Vector3.ProjectOnPlane(
                    velocity,
                    Vector3.up
f);
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
(
                Mathf.Abs(signedTurn) / 70f
            );

            smoothedSpeed = Mathf.MoveTowards(
                smoothedSpeed,
                targetSpeed,
                deltaTime * 2.4f
            );
            smoothedTurn = Mathf.MoveTowards(
                smoothedTurn,
                targetTurn,
                deltaTime * 3.5f
            );

            PositionWake(portWake.transform, -0.23f);
            PositionWake(starboardWake.transform, 0.23f);

            bool shouldEmit =
                smoothedSpeed > minimumVisibleSpeed;

            portWake.emitting = shouldEmit;
            starboardWake.emitting = shouldEmit;

            float baseWidth = maximumWakeWidth *
                Mathf.Lerp(0.38f, 1f, smoothedSpeed);

            float turnBalance = Mathf.Clamp(
                signedTurn / 70f,
                -1f,
                1f
            );

            portWake.widthMultiplier =
                baseWidth *
                (1f +
                 Mathf.Max(0f, turnBalance) *
                 turnWakeBoost);

            starboardWake.widthMultiplier =
                baseWidth *
                (1f +
                 Mathf.Max(0f, -turnBalance) *
                 turnWakeBoost);

            previousPosition = transform.position;
            previousForward = transform.forward;
        }

        private void PositionWake(
            Transform wakeTransform,
            float horizontalOffset)
        {
            Vector3 localPosition =
                sternLocalPosition +
                Vector3.right * horizontalOffset;

            Vector3 worldPosition =
                transform.TransformPoint(localPosition);
            worldPosition.y = waterHeight + 0.075f;
            wakeTransform.position = worldPosition;
        }

        private TrailRenderer CreateWake(string objectName)
        {
            GameObject wakeObject = new GameObject(objectName);
            wakeObject.transform.SetParent(transform, true);

            TrailRenderer trail =
                wakeObject.AddComponent<TrailRenderer>();

            trail.emitting = false;
            trail.time = wakeLifetime;
            trail.minVertexDistance = 0.08f;
            trail.autodestruct = false;
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            trail.numCornerVertices = 3;
            trail.numCapVertices = 3;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.sortingOrder = 2;

            trail.widthCurve = new AnimationCurve(
                new Keyframe(0f, 0.08f),
                new Keyframe(0.22f, 1f),
                new Keyframe(1f, 0.18f)
            );

            Gradient color = new Gradient();
            color.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.68f, 0.88f, 0.9f),
                        0f
                    ),
                    new GradientColorKey(
                        new Color(0.38f, 0.68f, 0.72f),
                        1f
                    )
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.62f, 0.12f),
                    new GradientAlphaKey(0.34f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = color;

            if (wakeMaterial != null)
            {
                trail.sharedMaterial = wakeMaterial;
            }

            trail.Clear();
            return trail;
        }

        private Material CreateWakeMaterial()
        {
            Shader shader = Shader.Find(TrailShaderName);

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
                name = "Runtime Ocean Wake"
            };
            material.color = Color.white;
            return material;
        }

        private void OnDisable()
        {
            if (portWake != null)
            {
                portWake.emitting = false;
                portWake.Clear();
            }

            if (starboardWake != null)
            {
                starboardWake.emitting = false;
                starboardWake.Clear();
            }
        }

        private void OnDestroy()
        {
            if (wakeMaterial != null)
            {
                Destroy(wakeMaterial);
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
