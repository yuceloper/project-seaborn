using System;
using System.Collections;
using Seaborn.Progression;
using Seaborn.Atmosphere;
using Seaborn.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Seaborn.World
{
    public enum PrototypeRegionKind
    {
        SafeHarbor,
        CentralWaters,
        WesternReach,
        EasternReach
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeExpeditionRegionDirector :
        MonoBehaviour
    {
        private enum EntrySide
        {
            None,
            Harbor,
            West,
            East
        }

        private const string HarborScene =
            "PrototypeHarbor";
        private const string CentralScene =
            "PrototypeOcean";
        private const string WestScene =
            "PrototypeWesternReach";
        private const string EastScene =
            "PrototypeEasternReach";
        public const float MapEdge = 82f;
        private const float Edge = MapEdge;

        public static PrototypeExpeditionRegionDirector
            Instance { get; private set; }

        public static bool IsHarborScene =>
            SceneManager.GetActiveScene().name ==
            HarborScene;

        public string CurrentRegionName { get; private set; } =
            "AÇIK DENİZ";

        public string CurrentDangerLabel { get; private set; } =
            "ORTA TEHLİKE";

        public Color CurrentDangerColor { get; private set; } =
            new(0.86f, 0.68f, 0.3f, 1f);

        public PrototypeRegionKind CurrentRegionKind
        {
            get;
            private set;
        } = PrototypeRegionKind.CentralWaters;

        public event Action<string, int> TravelBlocked;

        private static EntrySide pendingEntry;
        private Transform player;
        private UnityEngine.Camera persistentCamera;
        private Material gatewayMaterial;
        private CanvasGroup transitionGroup;
        private Text transitionText;
        private bool transitioning;
        private MapGatewayApproachView approachView;
        private string approachedScene;
        private EntrySide approachedSide;
        private string cancelledGate;
        private const float FogDistance = 24f;
        private const float ConfirmationDistance = 6f;
        private float nextBlockedNoticeTime;

        public static void EnsureCreated(Transform player)
        {
            if (player == null)
            {
                return;
            }

            PrototypeExpeditionRegionDirector director =
                FindFirstObjectByType<
                    PrototypeExpeditionRegionDirector>();

            if (director == null)
            {
                GameObject root = new(
                    "Prototype World Map Director");
                director = root.AddComponent<
                    PrototypeExpeditionRegionDirector>();
            }

            director.Bind(player);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildTransitionOverlay();
            approachView = gameObject.AddComponent<MapGatewayApproachView>();
            approachView.Configure(ConfirmApproach, CancelApproach);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ConfigureActiveMap();
        }

        private void Bind(Transform playerTransform)
        {
            player = playerTransform;
            if (player != null)
            {
                DontDestroyOnLoad(player.root.gameObject);
            }

            if (persistentCamera == null)
            {
                persistentCamera = UnityEngine.Camera.main;
                if (persistentCamera != null)
                {
                    DontDestroyOnLoad(
                        persistentCamera.gameObject);
                }
            }
        }

        private void Update()
        {
            var health = player != null ? player.GetComponent<Seaborn.Ship.ShipHealth>() : null;
            if (transitioning || player == null || health == null || health.IsSunk)
            {
                ResetApproach();
                return;
            }

            string scene = SceneManager.GetActiveScene().name;
            FindApproach(scene, player.position, out string destination, out EntrySide side, out float distance);
            if (destination == null || distance > FogDistance)
            {
                ResetApproach();
                return;
            }
            if (approachedScene != destination)
            {
                cancelledGate = null;
            }
            approachedScene = destination;
            approachedSide = side;
            bool inside = distance <= ConfirmationDistance;
            int requiredTier = RequiredTier(destination);
            var progression = player.GetComponent<PrototypeCaptainProgression>();
            bool unlocked = (progression != null ? progression.HighestUnlockedMapTier : 1) >= requiredTier;

            float fog = 1f - Mathf.Clamp01(distance / FogDistance);
            string status = !unlocked ? $"KAPTAN SV. {RequiredLevelForTier(requiredTier)} GEREKİR"
                : !inside ? "GEÇİŞ ALANINA YAKLAŞ"
                : "GEÇİŞ HAZIR • ONAYINI BEKLİYOR";
            approachView.Show(GetMapDisplayName(destination), status, fog,
                inside && unlocked,
                cancelledGate == null);
        }

        private static void FindApproach(string scene, Vector3 position,
            out string destination, out EntrySide side, out float distance)
        {
            destination = null; side = EntrySide.None; distance = float.PositiveInfinity;
            if (scene == HarborScene)
            {
                destination = CentralScene; side = EntrySide.Harbor; distance = Edge - position.z;
                return;
            }
            if (scene != CentralScene && scene != WestScene && scene != EastScene) return;
            destination = HarborScene; side = EntrySide.Harbor; distance = Edge + position.z;
            if ((scene == CentralScene || scene == EastScene) && Edge + position.x < distance)
            {
                destination = scene == CentralScene ? WestScene : CentralScene;
                side = scene == CentralScene ? EntrySide.West : EntrySide.East;
                distance = Edge + position.x;
            }
            if ((scene == CentralScene || scene == WestScene) && Edge - position.x < distance)
            {
                destination = scene == CentralScene ? EastScene : CentralScene;
                side = scene == CentralScene ? EntrySide.East : EntrySide.West;
                distance = Edge - position.x;
            }
        }

        private void LateUpdate()
        {
            if (transitioning || player == null) return;
            // Leave controls live; constrain position and only outward drift at map bounds.
            Vector3 position = player.position;
            Vector3 bounded = position;
            bounded.x = Mathf.Clamp(position.x, -Edge, Edge);
            bounded.z = Mathf.Clamp(position.z, -Edge, Edge);
            if (bounded == position) return;
            var body = player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = bounded;
                if (body.isKinematic) return;
                Vector3 velocity = body.linearVelocity;
                if (position.x != bounded.x && Mathf.Sign(velocity.x) == Mathf.Sign(position.x)) velocity.x = 0f;
                if (position.z != bounded.z && Mathf.Sign(velocity.z) == Mathf.Sign(position.z)) velocity.z = 0f;
                body.linearVelocity = velocity;
            }
            else player.position = bounded;
        }

        private void ConfirmApproach()
        {
            if (transitioning || player == null || cancelledGate != null) return;
            var health = player.GetComponent<Seaborn.Ship.ShipHealth>();
            if (health == null || health.IsSunk) return;
            FindApproach(SceneManager.GetActiveScene().name, player.position,
                out string destination, out EntrySide side, out float distance);
            if (destination != approachedScene || side != approachedSide || distance > ConfirmationDistance) return;
            if (!Application.CanStreamedLevelBeLoaded(destination))
            {
                approachView.Show(GetMapDisplayName(destination), "HARİTA YÜKLENEMİYOR", 1f, false, true);
                Debug.LogWarning($"Build listesinde harita bulunamadı: {destination}", this);
                CancelApproach();
                return;
            }
            TravelTo(destination, side);
            ResetApproach();
        }

        private void CancelApproach()
        {
            cancelledGate = approachedScene;
            approachView.HideCard();
        }

        private void ResetApproach()
        {
            approachedScene = null;
            approachedSide = EntrySide.None;
            cancelledGate = null;
            if (approachView != null) approachView.Hide();
        }

        private void TravelTo(
            string sceneName,
            EntrySide entrySide)
        {
            if (transitioning)
            {
                return;
            }

            int requiredTier = RequiredTier(sceneName);
            PrototypeCaptainProgression progression =
                player != null
                    ? player.GetComponent<
                        PrototypeCaptainProgression>()
                    : null;
            int unlockedTier = progression != null
                ? progression.HighestUnlockedMapTier
                : 1;

            if (unlockedTier < requiredTier)
            {
                BlockLockedTravel(
                    sceneName,
                    entrySide,
                    requiredTier
                );
                return;
            }

            transitioning = true;
            pendingEntry = entrySide;

            if (player != null)
            {
                DontDestroyOnLoad(player.root.gameObject);
            }
            if (persistentCamera != null)
            {
                DontDestroyOnLoad(
                    persistentCamera.gameObject);
            }

            Debug.Log(
                $"Harita geçişi: " +
                $"{SceneManager.GetActiveScene().name} " +
                $"→ {sceneName}.",
                this
            );
            StartCoroutine(
                TravelRoutine(sceneName)
            );
        }

        private void BlockLockedTravel(
            string sceneName,
            EntrySide entrySide,
            int requiredTier)
        {
            StopPlayerMotion();

            if (player != null)
            {
                Vector3 position = player.position;
                if (entrySide == EntrySide.West)
                {
                    position.x = -Edge + 4f;
                }
                else if (entrySide == EntrySide.East)
                {
                    position.x = Edge - 4f;
                }
                else if (position.z >= Edge)
                {
                    position.z = Edge - 4f;
                }
                else if (position.z <= -Edge)
                {
                    position.z = -Edge + 4f;
                }

                player.position = position;
            }

            int requiredLevel =
                RequiredLevelForTier(requiredTier);
            if (Time.unscaledTime >= nextBlockedNoticeTime)
            {
                nextBlockedNoticeTime =
                    Time.unscaledTime + 1f;
                TravelBlocked?.Invoke(
                    GetMapDisplayName(sceneName),
                    requiredLevel
                );
                Debug.Log(
                    $"Bölge kilitli: " +
                    $"{GetMapDisplayName(sceneName)}, " +
                    $"Kaptan SV. {requiredLevel} gerekir.",
                    this
                );
            }
        }

        private static int RequiredTier(string sceneName)
        {
            if (sceneName == EastScene) return 2;
            if (sceneName == WestScene) return 3;
            return 1;
        }

        private static int RequiredLevelForTier(int tier)
        {
            return 1 + (Mathf.Max(1, tier) - 1) * 4;
        }

        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            ResetApproach();
            RemoveDuplicatePlayer();
            RemoveDuplicateCamera();
            ConfigureActiveMap();
            PlacePlayerAtEntry();
            StartCoroutine(RestorePlayerInput());
        }

        private IEnumerator TravelRoutine(
            string sceneName)
        {
            Seaborn.Ship.ShipMotor motor =
                player != null
                    ? player.GetComponent<
                        Seaborn.Ship.ShipMotor>()
                    : null;
            if (motor != null)
            {
                motor.enabled = false;
            }

            StopPlayerMotion();
            if (transitionText != null)
            {
                transitionText.text =
                    GetMapDisplayName(sceneName);
            }

            yield return FadeTo(1f, 0.28f);

            // Scene-owned canvases must be destroyed with their
            // scene. Preserving every Canvas left invisible
            // raycasters and duplicate EventSystems after travel.
            // Runtime HUDs are rebuilt and rebound by the bootstrap.
            AsyncOperation load =
                SceneManager.LoadSceneAsync(sceneName);
            while (load != null && !load.isDone)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.18f);
            yield return FadeTo(0f, 0.35f);
            transitioning = false;
        }

        private IEnumerator FadeTo(
            float target,
            float duration)
        {
            if (transitionGroup == null)
            {
                yield break;
            }

            float start = transitionGroup.alpha;
            float elapsed = 0f;
            transitionGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transitionGroup.alpha = Mathf.Lerp(
                    start,
                    target,
                    Mathf.Clamp01(elapsed / duration)
                );
                yield return null;
            }

            transitionGroup.alpha = target;
            transitionGroup.blocksRaycasts =
                target > 0.01f;
        }

        private IEnumerator RestorePlayerInput()
        {
            // Duplicate scene objects disable the shared
            // InputActionAsset when they are destroyed.
            // Wait until their OnDisable calls have finished.
            yield return null;

            if (player == null)
            {
                yield break;
            }

            Seaborn.Ship.ShipMotor motor =
                player.GetComponent<
                    Seaborn.Ship.ShipMotor>();
            if (motor != null)
            {
                motor.enabled = true;
                motor.RefreshInputBindings();
            }

            Rigidbody body =
                player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = false;
                body.WakeUp();
            }
        }

        private void StopPlayerMotion()
        {
            if (player == null)
            {
                return;
            }

            Rigidbody body =
                player.GetComponent<Rigidbody>();
            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private void RemoveDuplicatePlayer()
        {
            ManualBroadsideAimController[] players =
                FindObjectsByType<
                    ManualBroadsideAimController>(
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < players.Length; i++)
            {
                Transform candidate =
                    players[i].transform;
                if (candidate == player)
                {
                    continue;
                }

                candidate.gameObject.SetActive(false);
                Destroy(candidate.gameObject);
            }
        }

        private void RemoveDuplicateCamera()
        {
            UnityEngine.Camera[] cameras =
                FindObjectsByType<UnityEngine.Camera>(
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < cameras.Length; i++)
            {
                UnityEngine.Camera candidate = cameras[i];
                if (candidate == persistentCamera)
                {
                    continue;
                }

                candidate.gameObject.SetActive(false);
                Destroy(candidate.gameObject);
            }
        }

        private void PlacePlayerAtEntry()
        {
            if (player == null)
            {
                ManualBroadsideAimController controller =
                    FindFirstObjectByType<
                        ManualBroadsideAimController>();
                if (controller != null)
                {
                    Bind(controller.transform);
                }
            }

            if (player == null)
            {
                return;
            }

            string scene = SceneManager
                .GetActiveScene().name;
            Vector3 position = player.position;
            position.y = 0.5f;
            Quaternion rotation =
                Quaternion.identity;

            if (scene == HarborScene)
            {
                position.x = 0f;
                position.z = 0f;
            }
            else if (pendingEntry == EntrySide.Harbor)
            {
                position.x = 0f;
                position.z = -70f;
            }
            else if (pendingEntry == EntrySide.West)
            {
                bool enteringWest =
                    scene == WestScene;
                position.x =
                    enteringWest ? 68f : -68f;
                position.z = 0f;
                rotation = Quaternion.Euler(
                    0f,
                    enteringWest ? -90f : 90f,
                    0f
                );
            }
            else if (pendingEntry == EntrySide.East)
            {
                bool enteringEast =
                    scene == EastScene;
                position.x =
                    enteringEast ? -68f : 68f;
                position.z = 0f;
                rotation = Quaternion.Euler(
                    0f,
                    enteringEast ? 90f : -90f,
                    0f
                );
            }

            player.SetPositionAndRotation(
                position,
                rotation
            );
            StopPlayerMotion();
            pendingEntry = EntrySide.None;
        }

        private void BuildTransitionOverlay()
        {
            GameObject overlay = new(
                "Prototype Map Transition");
            DontDestroyOnLoad(overlay);

            Canvas canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            overlay.AddComponent<CanvasScaler>();
            overlay.AddComponent<GraphicRaycaster>();
            transitionGroup =
                overlay.AddComponent<CanvasGroup>();
            transitionGroup.alpha = 0f;
            transitionGroup.blocksRaycasts = false;

            GameObject background = new(
                "Background",
                typeof(RectTransform),
                typeof(Image)
            );
            background.transform.SetParent(
                overlay.transform,
                false
            );
            RectTransform backgroundRect =
                background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color =
                new Color(0.01f, 0.035f, 0.05f, 1f);

            GameObject title = new(
                "Map Name",
                typeof(RectTransform),
                typeof(Text)
            );
            title.transform.SetParent(
                background.transform,
                false
            );
            RectTransform titleRect =
                title.GetComponent<RectTransform>();
            titleRect.anchorMin =
                new Vector2(0.2f, 0.42f);
            titleRect.anchorMax =
                new Vector2(0.8f, 0.58f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            transitionText = title.GetComponent<Text>();
            transitionText.font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");
            transitionText.fontSize = 28;
            transitionText.fontStyle =
                FontStyle.Bold;
            transitionText.alignment =
                TextAnchor.MiddleCenter;
            transitionText.color =
                new Color(0.91f, 0.78f, 0.46f, 1f);
            transitionText.text = "SEABORN";
        }

        private static string GetMapDisplayName(
            string sceneName)
        {
            return sceneName switch
            {
                HarborScene => "SEABORN LİMANI",
                WestScene => "BATI SINIRI",
                EastScene => "DOĞU AVLARI",
                _ => "MERKEZ SULAR"
            };
        }

        private void ConfigureActiveMap()
        {
            ClearGateways();
            string scene = SceneManager
                .GetActiveScene().name;

            PrototypeRegionalAtmosphere.Apply(
                scene,
                persistentCamera
            );

            if (scene == HarborScene)
            {
                CurrentRegionKind =
                    PrototypeRegionKind.SafeHarbor;
                CurrentRegionName = "SEABORN LİMANI";
                CurrentDangerLabel = "GÜVENLİ BÖLGE";
                CurrentDangerColor =
                    new Color(0.3f, 0.76f, 0.57f, 1f);
                RemoveHarborThreats();
                CreateGateway(
                    "SEFER DENİZİ",
                    new Vector3(0f, 0.76f, 74f),
                    new Color(0.36f, 0.72f, 0.82f, 0.9f)
                );
                return;
            }

            if (scene == WestScene)
            {
                CurrentRegionKind =
                    PrototypeRegionKind.WesternReach;
                CurrentRegionName = "BATI SINIRI";
                CurrentDangerLabel = "YÜKSEK TEHLİKE";
                CurrentDangerColor =
                    new Color(0.9f, 0.48f, 0.2f, 1f);
                CreateGateway(
                    "MERKEZ SULAR",
                    new Vector3(74f, 0.76f, 0f),
                    CurrentDangerColor
                );
            }
            else if (scene == EastScene)
            {
                CurrentRegionKind =
                    PrototypeRegionKind.EasternReach;
                CurrentRegionName = "DOĞU AVLARI";
                CurrentDangerLabel = "ORTA TEHLİKE";
                CurrentDangerColor =
                    new Color(0.36f, 0.72f, 0.82f, 1f);
                CreateGateway(
                    "MERKEZ SULAR",
                    new Vector3(-74f, 0.76f, 0f),
                    CurrentDangerColor
                );
            }
            else
            {
                CurrentRegionKind =
                    PrototypeRegionKind.CentralWaters;
                CurrentRegionName = "MERKEZ SULAR";
                CurrentDangerLabel = "ORTA TEHLİKE";
                CurrentDangerColor =
                    new Color(0.86f, 0.68f, 0.3f, 1f);
                CreateGateway(
                    "BATI SINIRI",
                    new Vector3(-74f, 0.76f, 0f),
                    new Color(0.9f, 0.48f, 0.2f, 0.9f)
                );
                CreateGateway(
                    "DOĞU AVLARI",
                    new Vector3(74f, 0.76f, 0f),
                    new Color(0.36f, 0.72f, 0.82f, 0.9f)
                );
            }

            CreateGateway(
                "SEABORN LİMANI",
                new Vector3(0f, 0.76f, -74f),
                new Color(0.3f, 0.76f, 0.57f, 0.9f)
            );
        }

        private void RemoveHarborThreats()
        {
            Seaborn.Ship.EnemyShipController[] enemies =
                FindObjectsByType<
                    Seaborn.Ship.EnemyShipController>(
                    FindObjectsSortMode.None
                );
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].gameObject.SetActive(false);
                Destroy(enemies[i].gameObject);
            }

            Seaborn.Hunting.PrototypeSeaCreature[] creatures =
                FindObjectsByType<
                    Seaborn.Hunting.PrototypeSeaCreature>(
                    FindObjectsSortMode.None
                );
            for (int i = 0; i < creatures.Length; i++)
            {
                creatures[i].gameObject.SetActive(false);
                Destroy(creatures[i].gameObject);
            }
        }

        private void CreateGateway(
            string label,
            Vector3 position,
            Color color)
        {
            GameObject gateway = new(
                $"Harita Geçidi - {label}");
            gateway.transform.SetParent(transform, false);
            gateway.transform.position = position;

            LineRenderer line =
                gateway.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.widthMultiplier = 0.12f;
            line.startColor = color;
            line.endColor = color;
            line.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;
            line.receiveShadows = false;

            if (gatewayMaterial == null)
            {
                Shader shader = Shader.Find(
                    "Sprites/Default");
                if (shader != null)
                {
                    gatewayMaterial = new Material(shader)
                    {
                        name =
                            "Prototype Map Gateway Material"
                    };
                }
            }
            if (gatewayMaterial != null)
            {
                line.sharedMaterial = gatewayMaterial;
            }

            const float radius = 4.5f;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle =
                    (float)i / line.positionCount *
                    Mathf.PI * 2f;
                line.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius
                    )
                );
            }

            GameObject labelObject =
                new GameObject("Gateway Label");
            labelObject.transform.SetParent(
                gateway.transform,
                false
            );
            labelObject.transform.localPosition =
                new Vector3(0f, 1.1f, 0f);
            labelObject.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            TextMesh text =
                labelObject.AddComponent<TextMesh>();
            text.text = label;
            text.fontSize = 42;
            text.characterSize = 0.12f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
        }

        private void ClearGateways()
        {
            for (int i = transform.childCount - 1;
                 i >= 0;
                 i--)
            {
                Transform child =
                    transform.GetChild(i);
                if (child.name.StartsWith(
                        "Harita Geçidi"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -=
                HandleSceneLoaded;
            Instance = null;
            if (gatewayMaterial != null)
            {
                Destroy(gatewayMaterial);
            }
            if (transitionGroup != null)
            {
                Destroy(
                    transitionGroup.gameObject);
            }
        }
    }
}
