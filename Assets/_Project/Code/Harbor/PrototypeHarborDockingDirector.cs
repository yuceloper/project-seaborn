using System;
using System.Collections;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Harbor
{
    public enum PrototypeHarborStation
    {
        None,
        Shipyard,
        Trade,
        HarborOffice
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeHarborDockingDirector :
        MonoBehaviour
    {
        private const float ApproachRadius = 6f;
        private const float LeaveRadius = 8f;

        public static PrototypeHarborDockingDirector
            Instance { get; private set; }

        public event Action DockingChanged;
        public PrototypeHarborStation NearbyStation
        {
            get;
            private set;
        }
        public PrototypeHarborStation DockedStation
        {
            get;
            private set;
        }

        private Transform player;
        private Rigidbody playerBody;
        private ShipMotor shipMotor;
        private Vector3 origin;
        private Material markerMaterial;
        private bool initialized;
        private bool isDocking;
        private LineRenderer forwardRope;
        private LineRenderer aftRope;

        public bool IsDockedAt(
            PrototypeHarborStation station)
        {
            return DockedStation == station;
        }

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeHarborDockingDirector director =
                FindFirstObjectByType<
                    PrototypeHarborDockingDirector>();
            if (director == null)
            {
                GameObject root =
                    new("Prototype Harbor Docking");
                director = root.AddComponent<
                    PrototypeHarborDockingDirector>();
            }
            director.Initialize(player);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Initialize(Transform value)
        {
            player = value;
            playerBody = player.GetComponent<Rigidbody>();
            shipMotor = player.GetComponent<ShipMotor>();
            if (initialized) return;

            initialized = true;
            origin = new Vector3(
                player.position.x,
                0f,
                player.position.z
            );
            CreateMarker(
                PrototypeHarborStation.Shipyard,
                new Color(0.9f, 0.57f, 0.2f, 0.82f)
            );
            CreateMarker(
                PrototypeHarborStation.Trade,
                new Color(0.3f, 0.76f, 0.57f, 0.82f)
            );
            CreateMarker(
                PrototypeHarborStation.HarborOffice,
                new Color(0.42f, 0.68f, 0.86f, 0.82f)
            );
        }

        private void Update()
        {
            if (player == null) return;

            if (DockedStation !=
                PrototypeHarborStation.None)
            {
                StopShip();
                UpdateMooringLines();
            }

            PrototypeHarborStation nearest =
                FindNearbyStation();

            if (DockedStation !=
                    PrototypeHarborStation.None &&
                HorizontalDistance(
                    player.position,
                    StationPosition(DockedStation)
                ) > LeaveRadius)
            {
                SetDocked(PrototypeHarborStation.None);
            }

            if (NearbyStation != nearest)
            {
                NearbyStation = nearest;
                DockingChanged?.Invoke();
            }

            if (isDocking ||
                NearbyStation ==
                    PrototypeHarborStation.None ||
                Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame)
            {
                return;
            }

            SetDocked(
                DockedStation == NearbyStation
                    ? PrototypeHarborStation.None
                    : NearbyStation
            );
        }

        private PrototypeHarborStation
            FindNearbyStation()
        {
            PrototypeHarborStation result =
                PrototypeHarborStation.None;
            float best = ApproachRadius;
            PrototypeHarborStation[] stations =
            {
                PrototypeHarborStation.Shipyard,
                PrototypeHarborStation.Trade,
                PrototypeHarborStation.HarborOffice
            };

            for (int i = 0; i < stations.Length; i++)
            {
                float distance = HorizontalDistance(
                    player.position,
                    StationPosition(stations[i])
                );
                if (distance <= best)
                {
                    best = distance;
                    result = stations[i];
                }
            }
            return result;
        }

        private void SetDocked(
            PrototypeHarborStation station)
        {
            if (DockedStation == station) return;

            if (station ==
                PrototypeHarborStation.None)
            {
                DockedStation = station;
                ReleaseMooringLines();
                SetMotorEnabled(true);
                DockingChanged?.Invoke();
                Debug.Log(
                    "Liman istasyonundan ayrıldın.",
                    this
                );
                return;
            }

            DockedStation = station;
            SetMotorEnabled(false);
            StopShip();
            StartCoroutine(SnapToStation(station));
            DockingChanged?.Invoke();
            Debug.Log(
                $"{StationLabel(station)} " +
                "istasyonuna yanaşılıyor.",
                this
            );
        }

        private IEnumerator SnapToStation(
            PrototypeHarborStation station)
        {
            isDocking = true;
            Vector3 startPosition = player.position;
            Quaternion startRotation = player.rotation;
            Vector3 targetPosition =
                StationPosition(station);
            targetPosition.y = startPosition.y;
            Quaternion targetRotation =
                StationRotation(station);

            const float duration = 0.7f;
            float elapsed = 0f;
            while (elapsed < duration &&
                   DockedStation == station)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(elapsed / duration)
                );
                player.SetPositionAndRotation(
                    Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        progress
                    ),
                    Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        progress
                    )
                );
                StopShip();
                yield return null;
            }

            if (DockedStation == station)
            {
                player.SetPositionAndRotation(
                    targetPosition,
                    targetRotation
                );
                StopShip();
                CreateMooringLines();
                Debug.Log(
                    $"{StationLabel(station)} " +
                    "istasyonuna yanaşıldı.",
                    this
                );
            }
            isDocking = false;
        }

        private void StopShip()
        {
            if (playerBody == null) return;
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
        }

        private void SetMotorEnabled(bool enabled)
        {
            if (shipMotor == null) return;
            shipMotor.enabled = enabled;
            if (enabled)
            {
                shipMotor.RefreshInputBindings();
            }
        }

        private static Quaternion StationRotation(
            PrototypeHarborStation station)
        {
            return station ==
                PrototypeHarborStation.HarborOffice
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;
        }

        private void CreateMooringLines()
        {
            ReleaseMooringLines();
            forwardRope = CreateRope(
                "Forward Mooring Line");
            aftRope = CreateRope(
                "Aft Mooring Line");
            UpdateMooringLines();
        }

        private LineRenderer CreateRope(
            string ropeName)
        {
            GameObject rope = new(ropeName);
            rope.transform.SetParent(transform, false);
            LineRenderer line =
                rope.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier = 0.055f;
            line.startColor =
                new Color(0.18f, 0.12f, 0.06f, 1f);
            line.endColor = line.startColor;
            line.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;
            line.receiveShadows = false;
            if (markerMaterial != null)
            {
                line.sharedMaterial = markerMaterial;
            }
            return line;
        }

        private void UpdateMooringLines()
        {
            if (player == null ||
                forwardRope == null ||
                aftRope == null)
            {
                return;
            }

            Vector3 right = player.right;
            Vector3 forward = player.forward;
            float dockSide =
                DockedStation ==
                    PrototypeHarborStation.Shipyard
                    ? -1f
                    : DockedStation ==
                        PrototypeHarborStation.Trade
                        ? 1f
                        : 0f;

            Vector3 forwardShip =
                player.position +
                forward * 1.6f +
                right * dockSide * 1.05f +
                Vector3.up * 0.35f;
            Vector3 aftShip =
                player.position -
                forward * 1.6f +
                right * dockSide * 1.05f +
                Vector3.up * 0.35f;

            Vector3 forwardDock;
            Vector3 aftDock;
            if (DockedStation ==
                PrototypeHarborStation.HarborOffice)
            {
                forwardDock =
                    player.position +
                    new Vector3(-2f, 0.3f, -3.5f);
                aftDock =
                    player.position +
                    new Vector3(2f, 0.3f, -3.5f);
            }
            else
            {
                forwardDock =
                    forwardShip +
                    right * dockSide * 2.7f;
                aftDock =
                    aftShip +
                    right * dockSide * 2.7f;
            }

            forwardRope.SetPosition(0, forwardShip);
            forwardRope.SetPosition(1, forwardDock);
            aftRope.SetPosition(0, aftShip);
            aftRope.SetPosition(1, aftDock);
        }

        private void ReleaseMooringLines()
        {
            if (forwardRope != null)
            {
                Destroy(forwardRope.gameObject);
                forwardRope = null;
            }
            if (aftRope != null)
            {
                Destroy(aftRope.gameObject);
                aftRope = null;
            }
        }

        private Vector3 StationPosition(
            PrototypeHarborStation station)
        {
            Vector3 offset = station switch
            {
                PrototypeHarborStation.Shipyard =>
                    new Vector3(-5.2f, 0.82f, -4f),
                PrototypeHarborStation.Trade =>
                    new Vector3(5.2f, 0.82f, -3f),
                PrototypeHarborStation.HarborOffice =>
                    new Vector3(0f, 0.82f, -11.8f),
                _ => Vector3.zero
            };
            return origin + offset;
        }

        private void CreateMarker(
            PrototypeHarborStation station,
            Color color)
        {
            GameObject marker = new(
                $"{StationLabel(station)} Yanaşma Alanı");
            marker.transform.SetParent(transform, false);
            marker.transform.position =
                StationPosition(station);

            LineRenderer ring =
                marker.AddComponent<LineRenderer>();
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 40;
            ring.widthMultiplier = 0.09f;
            ring.startColor = color;
            ring.endColor = color;
            ring.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;
            ring.receiveShadows = false;

            if (markerMaterial == null)
            {
                Shader shader =
                    Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    markerMaterial =
                        new Material(shader);
                }
            }
            if (markerMaterial != null)
            {
                ring.sharedMaterial = markerMaterial;
            }

            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i / 40f *
                    Mathf.PI * 2f;
                ring.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * 2.4f,
                        0f,
                        Mathf.Sin(angle) * 2.4f
                    )
                );
            }
        }

        private void OnGUI()
        {
            if (NearbyStation ==
                PrototypeHarborStation.None) return;

            const float width = 430f;
            Rect prompt = new(
                (Screen.width - width) * 0.5f,
                Screen.height - 182f,
                width,
                46f
            );
            Color previous = GUI.color;
            GUI.color =
                new Color(0.025f, 0.075f, 0.105f, 0.95f);
            GUI.Box(prompt, GUIContent.none);
            GUI.color = previous;

            GUIStyle style =
                new(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 14
                };
            style.normal.textColor =
                new Color(0.91f, 0.78f, 0.46f);

            string action =
                DockedStation == NearbyStation
                    ? "AYRIL"
                    : "YANAŞ";
            GUI.Label(
                prompt,
                $"E — {StationLabel(NearbyStation)} " +
                action,
                style
            );
        }

        private static string StationLabel(
            PrototypeHarborStation station)
        {
            return station switch
            {
                PrototypeHarborStation.Shipyard =>
                    "TERSANE",
                PrototypeHarborStation.Trade =>
                    "TİCARET VE İKMAL",
                PrototypeHarborStation.HarborOffice =>
                    "LİMAN İDARESİ",
                _ => "LİMAN"
            };
        }

        private static float HorizontalDistance(
            Vector3 first,
            Vector3 second)
        {
            Vector3 offset = first - second;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void OnDestroy()
        {
            ReleaseMooringLines();
            SetMotorEnabled(true);
            if (Instance == this) Instance = null;
            if (markerMaterial != null)
            {
                Destroy(markerMaterial);
            }
        }
    }
}
