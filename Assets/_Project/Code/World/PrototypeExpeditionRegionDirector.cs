using System.Collections;
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
        private const float Edge = 82f;

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

        private static EntrySide pendingEntry;
        private Transform player;
        private UnityEngine.Camera persistentCamera;
        private Material gatewayMaterial;
        private CanvasGroup transitionGroup;
        private Text transitionText;
        private bool transitioning;

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
            if (transitioning || player == null)
            {
                return;
            }

            Vector3 position = player.position;
            string scene = SceneManager
                .GetActiveScene().name;

            if (scene == HarborScene)
            {
                if (position.z >= Edge)
                {
                    TravelTo(
                        CentralScene,
                        EntrySide.Harbor
                    );
                }
                return;
            }

            if (position.z <= -Edge)
            {
                TravelTo(
                    HarborScene,
                    EntrySide.Harbor
                );
                return;
            }

            if (scene == CentralScene)
            {
                if (position.x <= -Edge)
                {
                    TravelTo(
                        WestScene,
                        EntrySide.West
                    );
                }
                else if (position.x >= Edge)
                {
                    TravelTo(
                        EastScene,
                        EntrySide.East
                    );
                }
            }
            else if (scene == WestScene &&
                     position.x >= Edge)
            {
                TravelTo(
                    CentralScene,
                    EntrySide.West
                );
            }
            else if (scene == EastScene &&
                     position.x <= -Edge)
            {
                TravelTo(
                    CentralScene,
                    EntrySide.East
                );
            }
        }

        private void TravelTo(
            string sceneName,
            EntrySide entrySide)
        {
            if (transitioning)
            {
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

        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
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
            PreserveRuntimeUi();

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

        private void PreserveRuntimeUi()
        {
            Canvas[] canvases =
                FindObjectsByType<Canvas>(
                    FindObjectsSortMode.None
                );
            for (int i = 0; i < canvases.Length; i++)
            {
                DontDestroyOnLoad(
                    canvases[i].transform.root.gameObject
                );
            }
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
