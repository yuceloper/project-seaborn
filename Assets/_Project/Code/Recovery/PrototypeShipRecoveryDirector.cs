using System;
using System.Collections;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Recovery
{
    public enum PrototypeShipRecoveryState
    {
        MainShipActive,
        AwaitingReserve,
        ReserveShipActive,
        MainShipRecovered
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeShipRecoveryDirector :
        MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float reserveLaunchDelay = 3.2f;
        [SerializeField, Min(1f)]
        private float salvageRadius = 3.6f;
        [SerializeField, Min(0.1f)]
        private float salvageDuration = 2.5f;
        [SerializeField, Min(0)]
        private int restorationCost = 60;

        public static PrototypeShipRecoveryDirector Instance
        {
            get;
            private set;
        }

        public event Action RecoveryChanged;

        public PrototypeShipRecoveryState State
        {
            get;
            private set;
        } = PrototypeShipRecoveryState.MainShipActive;

        public bool IsRecoveryActive =>
            State != PrototypeShipRecoveryState.MainShipActive;
        public Vector3 WreckPosition => wreckPosition;
        public float SalvageProgress =>
            Mathf.Clamp01(salvageProgress / salvageDuration);
        public int RestorationCost => restorationCost;

        private Transform player;
        private ShipHealth health;
        private ShipSinkController sinkController;
        private PrototypeSilverWallet wallet;
        private PrototypeExpeditionDirector expedition;
        private Vector3 harborPosition;
        private Quaternion harborRotation;
        private Vector3 wreckPosition;
        private GameObject wreckMarker;
        private LineRenderer wreckRing;
        private float salvageProgress;
        private float bobOrigin;
        private bool respawning;

        public static void EnsureCreated(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            PrototypeShipRecoveryDirector director =
                FindFirstObjectByType<
                    PrototypeShipRecoveryDirector>();

            if (director == null)
            {
                GameObject directorObject =
                    new GameObject(
                        "Prototype Ship Recovery Director"
                    );
                director = directorObject.AddComponent<
                    PrototypeShipRecoveryDirector>();
            }

            director.Bind(playerTransform);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            AnimateWreckMarker();

            if (State ==
                PrototypeShipRecoveryState.ReserveShipActive)
            {
                UpdateSalvage();
                return;
            }

            if (State ==
                PrototypeShipRecoveryState.MainShipRecovered)
            {
                UpdateRestoration();
            }
        }

        private void Bind(Transform playerTransform)
        {
            if (player == playerTransform &&
                health != null &&
                sinkController != null)
            {
                return;
            }

            Unsubscribe();

            player = playerTransform;
            health = player.GetComponentInChildren<ShipHealth>();
            sinkController =
                player.GetComponentInChildren<
                    ShipSinkController>();
            wallet =
                player.GetComponentInChildren<
                    PrototypeSilverWallet>();
            expedition =
                PrototypeExpeditionDirector.Instance;
            harborPosition = expedition != null
                ? expedition.HarborPosition
                : player.position;
            harborRotation = player.rotation;

            if (health != null)
            {
                health.Sunk += HandleShipSunk;
            }
        }

        private void HandleShipSunk()
        {
            if (respawning)
            {
                return;
            }

            bool mainShipWasActive =
                State ==
                PrototypeShipRecoveryState.MainShipActive;

            if (mainShipWasActive)
            {
                wreckPosition = player.position;
                CreateWreckMarker();
            }

            State =
                PrototypeShipRecoveryState.AwaitingReserve;
            salvageProgress = 0f;
            respawning = true;
            RecoveryChanged?.Invoke();

            Debug.Log(
                mainShipWasActive
                    ? "Ana gemi battı. Yedek gemi hazırlanıyor."
                    : "Yedek gemi battı. Enkaz hedefi korunuyor.",
                this
            );

            StartCoroutine(LaunchReserve(
                mainShipWasActive
            ));
        }

        private IEnumerator LaunchReserve(
            bool createdRecovery)
        {
            yield return new WaitForSeconds(
                reserveLaunchDelay
            );

            if (sinkController == null)
            {
                respawning = false;
                yield break;
            }

            sinkController.RestoreAfterSinking(
                harborPosition,
                harborRotation
            );

            State =
                createdRecovery || wreckMarker != null
                    ? PrototypeShipRecoveryState
                        .ReserveShipActive
                    : PrototypeShipRecoveryState
                        .MainShipActive;
            respawning = false;
            RecoveryChanged?.Invoke();

            Debug.Log(
                "Yedek gemi limanda hazır. " +
                "Enkaz bölgesine dön.",
                this
            );
        }

        private void UpdateSalvage()
        {
            if (player == null || wreckMarker == null)
            {
                return;
            }

            float distance = HorizontalDistance(
                player.position,
                wreckPosition
            );

            bool holdingInteract =
                distance <= salvageRadius &&
                Keyboard.current != null &&
                Keyboard.current.eKey.isPressed;

            salvageProgress = holdingInteract
                ? salvageProgress + Time.deltaTime
                : Mathf.Max(
                    0f,
                    salvageProgress -
                    Time.deltaTime * 1.5f
                );

            if (salvageProgress < salvageDuration)
            {
                return;
            }

            salvageProgress = salvageDuration;
            Destroy(wreckMarker);
            wreckMarker = null;
            wreckRing = null;
            State =
                PrototypeShipRecoveryState.MainShipRecovered;
            RecoveryChanged?.Invoke();

            Debug.Log(
                "Ana gemi enkazdan çıkarıldı. " +
                "Onarım için limana dön.",
                this
            );
        }

        private void UpdateRestoration()
        {
            if (!IsPlayerAtHarbor() ||
                Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame)
            {
                return;
            }

            if (wallet == null ||
                !wallet.TrySpendSilver(
                    restorationCost,
                    "Ana gemiyi hizmete alma"))
            {
                Debug.Log(
                    $"Ana gemi için {restorationCost} silver gerekli.",
                    this
                );
                return;
            }

            State =
                PrototypeShipRecoveryState.MainShipActive;
            salvageProgress = 0f;
            RecoveryChanged?.Invoke();

            Debug.Log(
                "Ana gemi onarıldı ve yeniden hizmette.",
                this
            );
        }

        private bool IsPlayerAtHarbor()
        {
            return expedition != null
                ? expedition.IsPlayerAtHarbor
                : HorizontalDistance(
                    player.position,
                    harborPosition
                ) <= 4.8f;
        }

        private void CreateWreckMarker()
        {
            if (wreckMarker != null)
            {
                Destroy(wreckMarker);
            }

            wreckMarker =
                new GameObject("Main Ship Wreck Target");
            wreckMarker.transform.position =
                new Vector3(
                    wreckPosition.x,
                    0.34f,
                    wreckPosition.z
                );
            bobOrigin = wreckMarker.transform.position.y;

            wreckRing =
                wreckMarker.AddComponent<LineRenderer>();
            wreckRing.loop = true;
            wreckRing.useWorldSpace = false;
            wreckRing.positionCount = 48;
            wreckRing.widthMultiplier = 0.09f;
            wreckRing.startColor =
                new Color(0.88f, 0.57f, 0.2f, 0.9f);
            wreckRing.endColor = wreckRing.startColor;

            Material material =
                Resources.Load<Material>(
                    "PrototypeCombatParticle"
                );
            if (material != null)
            {
                wreckRing.sharedMaterial = material;
            }

            for (int index = 0; index < 48; index++)
            {
                float angle =
                    index / 48f * Mathf.PI * 2f;
                wreckRing.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) * salvageRadius,
                        0f,
                        Mathf.Sin(angle) * salvageRadius
                    )
                );
            }

            GameObject buoy = GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );
            buoy.name = "Recovery Buoy";
            buoy.transform.SetParent(
                wreckMarker.transform,
                false
            );
            buoy.transform.localPosition =
                new Vector3(0f, 0.42f, 0f);
            buoy.transform.localScale =
                new Vector3(0.48f, 0.75f, 0.48f);

            Collider buoyCollider =
                buoy.GetComponent<Collider>();
            if (buoyCollider != null)
            {
                Destroy(buoyCollider);
            }

            Renderer buoyRenderer =
                buoy.GetComponent<Renderer>();
            if (material != null && buoyRenderer != null)
            {
                buoyRenderer.sharedMaterial = material;
            }
        }

        private void AnimateWreckMarker()
        {
            if (wreckMarker == null)
            {
                return;
            }

            Vector3 position =
                wreckMarker.transform.position;
            position.y =
                bobOrigin +
                Mathf.Sin(Time.time * 1.7f) * 0.12f;
            wreckMarker.transform.position = position;

            if (wreckRing != null)
            {
                float pulse =
                    1f + Mathf.Sin(Time.time * 2.4f) * 0.05f;
                wreckRing.transform.localScale =
                    Vector3.one * pulse;
            }
        }

        private void OnGUI()
        {
            if (!IsRecoveryActive)
            {
                return;
            }

            const float width = 430f;
            const float height = 104f;
            Rect box = new Rect(
                (Screen.width - width) * 0.5f,
                26f,
                width,
                height
            );

            Color previousColor = GUI.color;
            GUI.color =
                new Color(0.04f, 0.1f, 0.13f, 0.96f);
            GUI.Box(box, GUIContent.none);
            GUI.color = previousColor;

            GUIStyle title = new GUIStyle(
                GUI.skin.label
            );
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.fontSize = 17;
            title.normal.textColor =
                new Color(0.9f, 0.7f, 0.32f);

            GUIStyle body = new GUIStyle(
                GUI.skin.label
            );
            body.alignment = TextAnchor.MiddleCenter;
            body.fontSize = 14;
            body.normal.textColor =
                new Color(0.9f, 0.88f, 0.78f);

            GUI.Label(
                new Rect(box.x, box.y + 8f, width, 25f),
                StateTitle(),
                title
            );
            GUI.Label(
                new Rect(box.x + 16f, box.y + 35f,
                    width - 32f, 42f),
                StateInstruction(),
                body
            );

            if (State ==
                PrototypeShipRecoveryState.ReserveShipActive &&
                SalvageProgress > 0f)
            {
                Rect progress = new Rect(
                    box.x + 44f,
                    box.y + 82f,
                    width - 88f,
                    10f
                );
                GUI.Box(progress, GUIContent.none);
                GUI.color =
                    new Color(0.9f, 0.62f, 0.25f);
                GUI.Box(
                    new Rect(
                        progress.x,
                        progress.y,
                        progress.width *
                            SalvageProgress,
                        progress.height
                    ),
                    GUIContent.none
                );
                GUI.color = previousColor;
            }
        }

        private string StateTitle()
        {
            switch (State)
            {
                case PrototypeShipRecoveryState
                    .AwaitingReserve:
                    return "YEDEK GEMİ HAZIRLANIYOR";
                case PrototypeShipRecoveryState
                    .ReserveShipActive:
                    return "ANA GEMİ ENKAZI";
                case PrototypeShipRecoveryState
                    .MainShipRecovered:
                    return "ANA GEMİ KURTARILDI";
                default:
                    return "";
            }
        }

        private string StateInstruction()
        {
            switch (State)
            {
                case PrototypeShipRecoveryState
                    .AwaitingReserve:
                    return "Birazdan güvenli limanda " +
                        "yedek gemiye geçeceksin.";
                case PrototypeShipRecoveryState
                    .ReserveShipActive:
                    if (player == null)
                    {
                        return "Enkaz bölgesine dön.";
                    }

                    float distance = HorizontalDistance(
                        player.position,
                        wreckPosition
                    );
                    return distance <= salvageRadius
                        ? "Ana gemiyi çıkarmak için E basılı tut."
                        : $"Enkaza uzaklık: {distance:0.0}";
                case PrototypeShipRecoveryState
                    .MainShipRecovered:
                    return IsPlayerAtHarbor()
                        ? $"Ana gemiyi hizmete almak için " +
                            $"E — {restorationCost} silver"
                        : "Ana gemiyi onarmak için limana dön.";
                default:
                    return "";
            }
        }

        private static float HorizontalDistance(
            Vector3 first,
            Vector3 second)
        {
            Vector3 delta = first - second;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void Unsubscribe()
        {
            if (health != null)
            {
                health.Sunk -= HandleShipSunk;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (wreckMarker != null)
            {
                Destroy(wreckMarker);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
