using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using Seaborn.Harbor.UI;
using Seaborn.Harbor;
using Seaborn.Recovery;
using Seaborn.Progression;
using Seaborn.Ship;
using Seaborn.World;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeGameplayHud : MonoBehaviour
    {
        private static readonly Color Navy = SeabornUiSkin.Teal;
        private static readonly Color NavyLight = SeabornUiSkin.TealRaised;
        private static readonly Color Gold = new(0.88f,0.70f,0.40f,1f);
        private static readonly Color Cream = SeabornUiSkin.Ivory;
        private static readonly Color Muted = new(0.66f, 0.76f, 0.73f, 1f);
        private static readonly Color Success = new(0.3f, 0.76f, 0.57f, 1f);
        private static readonly Color Danger = new(0.82f, 0.28f, 0.22f, 1f);

        private ShipHealth health;
        private ShipSubsystemController subsystems;
        private ShipMotor motor;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;
        private PrototypeGoldWallet goldWallet;
        private PrototypeShipConsumables consumables;
        private PrototypeFieldRepairController repairs;
        private PrototypeCaptainProgression captainProgression;
        private PrototypeExpeditionRegionDirector worldMap;

        private Text shipNameText;
        private Text hullText;
        private RectTransform hullFill;
        private Image hullFillImage;
        private Text sailText;
        private Text crewText;
        private RectTransform sailFill;
        private RectTransform crewFill;
        private Text expeditionStateText;
        private Text expeditionDetailText;
        private RectTransform pressureFill;
        private Text silverText;
        private Text goldText;
        private UnityEngine.Camera chartCamera;
        private RenderTexture chartTexture;
        private float nextChartFrame;
        private Text cargoText;
        private Text activeBuffText;
        private GameObject pressureTrack;
        private RectTransform resourceCard;
        private Text ammoNameText;
        private Text portText;
        private Text starboardText;
        private RectTransform portFill;
        private RectTransform starboardFill;
        private Text harpoonText;
        private RectTransform harpoonFill;
        private Text harborLockText;
        private GameObject targetPanel;
        private Text targetNameText;
        private Text targetStateText;
        private Text targetSailText;
        private Text targetCrewText;
        private RectTransform targetHullFill;
        private RectTransform targetSailFill;
        private RectTransform targetCrewFill;
        private Image targetHullImage;
        private Text minimapNameText;
        private Text minimapHintText;
        private RectTransform minimapArea;
        private RectTransform minimapShip;
        private GameObject minimapNorth;
        private GameObject minimapEast;
        private GameObject minimapSouth;
        private GameObject minimapWest;
        private float nextRefreshTime;
        private Transform boundPlayer;
        private RectTransform notificationRoot;
        private Font interfaceFont;
        private readonly List<Toast> toasts = new();
        private bool criticalHullWarningShown;
        private CanvasGroup hudGroup;
        private readonly HotbarSlot[] hotbar = new HotbarSlot[10];
        private sealed class HotbarSlot
        {
            public Image Background;
            public GameObject Selection;
            public Text Count;
            public Text Timer;
            public Image Icon;
        }

        private sealed class Toast
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public float CreatedAt;
            public float ExpiresAt;
        }

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;
            PrototypeGameplayHud hud = FindFirstObjectByType<PrototypeGameplayHud>();
            if (hud == null)
            {
                GameObject root = new("Prototype Gameplay HUD");
                hud = root.AddComponent<PrototypeGameplayHud>();
            }
            hud.Bind(player);
        }

        private void Awake()
        {
            BuildInterface();
        }

        private void Update()
        {
            if (chartCamera != null)
            {
                bool render = !PrototypeHarborUiCoordinator.IsOpen && Time.unscaledTime >= nextChartFrame;
                chartCamera.enabled = render;
                if (render) nextChartFrame = Time.unscaledTime + 0.1f;
            }
            bool harborInterfaceOpen =
                PrototypeHarborUiCoordinator.IsOpen;
            if (hudGroup != null)
            {
                hudGroup.alpha =
                    harborInterfaceOpen ? 0f : 1f;
                // Passive HUD must never intercept harbor or aiming input.
                hudGroup.interactable = false;
                hudGroup.blocksRaycasts = false;
            }

            if (harborInterfaceOpen)
            {
                return;
            }

            AnimateNotifications();
            if (Time.unscaledTime < nextRefreshTime) return;
            nextRefreshTime = Time.unscaledTime + 0.1f;
            Refresh();
        }

        private void Bind(Transform player)
        {
            if (boundPlayer == player && health != null && cargo != null) return;
            Unsubscribe();
            boundPlayer = player;
            health = player.GetComponentInChildren<ShipHealth>();
            subsystems = health != null
                ? health.GetComponent<ShipSubsystemController>()
                : null;
            motor = player.GetComponentInChildren<ShipMotor>();
            broadside = player.GetComponentInChildren<BroadsideController>();
            harpoons = player.GetComponentInChildren<HarpoonHuntingController>();
            cargo = player.GetComponentInChildren<PrototypeHuntCargo>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
            goldWallet = player.GetComponentInChildren<PrototypeGoldWallet>();
            consumables = player.GetComponentInChildren<PrototypeShipConsumables>();
            repairs = player.GetComponentInChildren<PrototypeFieldRepairController>();
            captainProgression =
                player.GetComponentInChildren<
                    PrototypeCaptainProgression>();
            worldMap =
                PrototypeExpeditionRegionDirector.Instance;
            if (health != null)
            {
                health.HealthChanged += HandleHealthChanged;
                health.Sunk += HandleSunk;
            }
            if (cargo != null)
            {
                cargo.CatchAdded += HandleCatchAdded;
                cargo.CargoSecured += HandleCargoSecured;
                cargo.CargoLost += HandleCargoLost;
            }
            if (captainProgression != null)
            {
                captainProgression.LevelChanged +=
                    HandleCaptainLevelChanged;
            }
            if (worldMap != null)
            {
                worldMap.TravelBlocked +=
                    HandleTravelBlocked;
            }
            Refresh();
        }

        private void BuildInterface()
        {
            hudGroup = gameObject.AddComponent<CanvasGroup>();
            hudGroup.interactable = false;
            hudGroup.blocksRaycasts = false;
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            interfaceFont = Resources.Load<Font>("SeabornHud/DejaVuSerif");
            if (interfaceFont == null) interfaceFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font font = interfaceFont;

            RectTransform ship = CreateCard("Ship", new(0,1), new(0,1), new(24,-18), new(535,138));
            Art(ship, "Ship Medallion", SeabornHudArt.Frame(0), new(0,0), new(138,138));
            shipNameText = CreateText(ship,font,"",22,Cream,FontStyle.Normal,new(150,-12),new(380,28));
            shipNameText.resizeTextForBestFit = true;
            shipNameText.resizeTextMinSize = 14;
            shipNameText.resizeTextMaxSize = 22;
            hullFill = CreateBar(ship,"Hull",new(148,-49),new(382,37),out hullFillImage);
            hullText = CreateText(ship,font,"",17,SeabornUiSkin.Ink,FontStyle.Bold,new(177,-56),new(328,25));
            sailText = CreateText(ship,font,"",13,Cream,FontStyle.Normal,new(180,-92),new(156,20));
            crewText = CreateText(ship,font,"",13,Cream,FontStyle.Normal,new(367,-92),new(159,20));
            Art(ship,"Sails",SeabornHudArt.Glyph(3),new(148,-88),new(27,27));
            Art(ship,"Crew",SeabornHudArt.Glyph(4),new(335,-88),new(27,27));
            sailFill = CreateBar(ship,"Sails",new(148,-120),new(179,7),out _);
            crewFill = CreateBar(ship,"Crew",new(335,-120),new(195,7),out _);

            RectTransform expedition = CreateCard("Expedition",new(0.5f,1),new(0.5f,1),new(0,-20),new(442,112));
            Art(expedition,"Quest Parchment",SeabornHudArt.Frame(2),Vector2.zero,new(442,112),false);
            Art(expedition,"Compass Rose",SeabornHudArt.Glyph(5),new(22,-28),new(57,57));
            expeditionStateText = CreateText(expedition,font,"",17,SeabornUiSkin.Ink,FontStyle.Bold,new(87,-17),new(333,25),TextAnchor.UpperCenter);
            expeditionStateText.resizeTextForBestFit = true;
            expeditionStateText.resizeTextMinSize = 11;
            expeditionStateText.resizeTextMaxSize = 17;
            expeditionDetailText = CreateText(expedition,font,"",12,SeabornUiSkin.InkMuted,FontStyle.Normal,new(87,-48),new(333,42),TextAnchor.UpperCenter);
            pressureFill = CreateBar(expedition,"Pressure",new(96,-95),new(312,5),out _);
            pressureTrack = pressureFill.parent.gameObject;

            RectTransform target = CreateCard("Target",new(0.5f,1),new(0.5f,1),new(0,-142),new(390,88));
            Art(target,"Target Capsule",SeabornHudArt.Frame(3),Vector2.zero,new(390,88),false);
            targetPanel = target.gameObject;
            targetNameText = CreateText(target,font,"",15,Cream,FontStyle.Bold,new(25,-14),new(230,21));
            targetStateText = CreateText(target,font,"",11,Gold,FontStyle.Normal,new(252,-17),new(112,20),TextAnchor.UpperRight);
            targetHullFill = CreateBar(target,"Target Hull",new(25,-40),new(340,9),out targetHullImage);
            targetSailText = CreateText(target,font,"",10,Cream,FontStyle.Normal,new(25,-53),new(150,16));
            targetCrewText = CreateText(target,font,"",10,Cream,FontStyle.Normal,new(212,-53),new(150,16),TextAnchor.UpperRight);
            targetSailFill = CreateBar(target,"Target Sail",new(25,-71),new(150,5),out _);
            targetCrewFill = CreateBar(target,"Target Crew",new(212,-71),new(150,5),out _);
            targetPanel.SetActive(false);

            RectTransform resources = CreateCard("Resources",new(1,1),new(1,1),new(-24,-24),new(388,138));
            resourceCard = resources;
            Art(resources,"Currency Capsule",SeabornHudArt.Frame(3),Vector2.zero,new(388,72),false);
            Art(resources,"Cargo Capsule",SeabornHudArt.Frame(3),new(178,-62),new(210,45),false);
            Art(resources,"Silver",SeabornHudArt.Glyph(0),new(18,-16),new(35,35));
            Art(resources,"Gold",SeabornHudArt.Glyph(1),new(220,-16),new(35,35));
            Art(resources,"Cargo",SeabornHudArt.Glyph(2),new(190,-71),new(24,24));
            silverText = CreateText(resources,font,"",16,Cream,FontStyle.Normal,new(60,-25),new(150,22));
            goldText = CreateText(resources,font,"",16,Cream,FontStyle.Normal,new(263,-25),new(113,22));
            cargoText = CreateText(resources,font,"",12,Cream,FontStyle.Normal,new(220,-72),new(151,27),TextAnchor.UpperRight);
            activeBuffText = CreateText(resources,font,"",11,Cream,FontStyle.Normal,new(0,-114),new(380,28),TextAnchor.UpperRight);

            RectTransform minimap = CreateCard("Navigation",new(0,0),new(0,0),new(20,28),new(335,360));
            Image circle = Art(minimap,"Round Map Mask",SeabornHudArt.CircleMask,new(49,-48),new(240,240));
            circle.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            minimapArea = circle.rectTransform;
            GameObject mapView = new GameObject("Live Chart",typeof(RectTransform),typeof(RawImage));
            mapView.transform.SetParent(minimapArea,false);
            RectTransform mapRect = mapView.GetComponent<RectTransform>();
            mapRect.anchorMin = Vector2.zero; mapRect.anchorMax = Vector2.one;
            mapRect.offsetMin = mapRect.offsetMax = Vector2.zero;
            RawImage mapImage = mapView.GetComponent<RawImage>();
            mapImage.raycastTarget = false;
            BuildMapCamera(mapImage);
            minimapNorth = CreateMapDot(minimapArea,"North",new(120,-12),Success,7);
            minimapEast = CreateMapDot(minimapArea,"East",new(228,-120),Gold,7);
            minimapSouth = CreateMapDot(minimapArea,"South",new(120,-228),Success,7);
            minimapWest = CreateMapDot(minimapArea,"West",new(12,-120),Danger,7);
            minimapShip = CreateMapDot(minimapArea,"Player",new(120,-120),Cream,9).GetComponent<RectTransform>();
            Art(minimap,"Compass Rim",SeabornHudArt.Frame(1),new(4,0),new(330,330));
            CreateText(minimap,font,"N",17,Cream,FontStyle.Normal,new(157,-37),new(24,24),TextAnchor.UpperCenter);
            CreateText(minimap,font,"S",17,Cream,FontStyle.Normal,new(157,-280),new(24,24),TextAnchor.UpperCenter);
            CreateText(minimap,font,"W",17,Cream,FontStyle.Normal,new(35,-154),new(26,24),TextAnchor.UpperCenter);
            CreateText(minimap,font,"E",17,Cream,FontStyle.Normal,new(280,-154),new(26,24),TextAnchor.UpperCenter);
            Art(minimap,"Location Parchment",SeabornHudArt.Frame(7),new(36,-300),new(268,65),false);
            minimapNameText = CreateText(minimap,font,"",14,SeabornUiSkin.Ink,FontStyle.Bold,new(51,-315),new(238,22),TextAnchor.UpperCenter);
            minimapHintText = CreateText(minimap,font,"",11,SeabornUiSkin.InkMuted,FontStyle.Normal,new(48,-339),new(242,20),TextAnchor.UpperCenter);

            // No full-width backing box. Art-backed cells float over the scene.
            RectTransform combat = CreateCard("Combat",new(0.5f,0),new(0.5f,0),new(0,24),new(980,157));
            portText = CreateText(combat,font,"",11,Cream,FontStyle.Normal,new(4,-1),new(160,18));
            portFill = CreateBar(combat,"Port",new(4,-21),new(150,5),out _);
            ammoNameText = CreateText(combat,font,"",12,Cream,FontStyle.Normal,new(200,-1),new(580,20),TextAnchor.UpperCenter);
            starboardText = CreateText(combat,font,"",11,Cream,FontStyle.Normal,new(816,-1),new(160,18),TextAnchor.UpperRight);
            starboardFill = CreateBar(combat,"Starboard",new(826,-21),new(150,5),out _);
            harpoonText = CreateText(combat,font,"",10,Muted,FontStyle.Normal,new(205,22),new(570,18),TextAnchor.UpperCenter);
            harpoonFill = CreateBar(combat,"Harpoon",new(427,4),new(126,4),out _);
            string[] keys = { "1", "2", "3", "SHIFT+1", "SHIFT+2", "4", "5", "6", "7", "8" };
            string[] names = { "STANDART\nGÜLLE", "ZİNCİRLİ\nGÜLLE", "SAÇMA", "HAFİF\nZIPKIN", "AĞIR\nZIPKIN", "TONİK", "SAKLANMA\nFENERİ", "ROM", "RÜZGÂR\nİKSİRİ", "ZIRH" };
            for (int i=0;i<hotbar.Length;i++) hotbar[i]=CreateHotbarSlot(combat,i,keys[i],names[i]);
            RectTransform helm = CreateCard("Helm",new(0.5f,0),new(0.5f,0),new(0,205),new(760,28));
            harborLockText = CreateText(helm,font,"",12,Cream,FontStyle.Normal,new(0,0),new(760,26),TextAnchor.UpperCenter);

            GameObject notifications = new GameObject("Notifications",typeof(RectTransform));
            notifications.transform.SetParent(transform,false);
            notificationRoot = notifications.GetComponent<RectTransform>();
            notificationRoot.anchorMin = notificationRoot.anchorMax = notificationRoot.pivot = new Vector2(1,1);
            notificationRoot.anchoredPosition = new Vector2(-24,-180);
            notificationRoot.sizeDelta = new Vector2(360,260);
        }

        private static Image Art(RectTransform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool preserve = true)
        {
            GameObject item = new GameObject(name,typeof(RectTransform),typeof(Image));
            item.transform.SetParent(parent,false);
            Image image = item.GetComponent<Image>();
            image.sprite = sprite; image.preserveAspect = preserve; image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return image;
        }

        private void BuildMapCamera(RawImage image)
        {
            chartTexture = new RenderTexture(256,256,16) { name = "Seaborn Live Minimap", antiAliasing = 1 };
            chartTexture.Create();
            image.texture = chartTexture;
            GameObject cameraObject = new GameObject("Seaborn Chart Camera");
            chartCamera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.transform.position = new Vector3(0,180,0);
            cameraObject.transform.rotation = Quaternion.Euler(90,0,0);
            chartCamera.orthographic = true;
            chartCamera.orthographicSize = 90f;
            chartCamera.nearClipPlane = 0.1f; chartCamera.farClipPlane = 400f;
            chartCamera.clearFlags = CameraClearFlags.SolidColor;
            chartCamera.backgroundColor = new Color(0.035f,0.16f,0.17f,1f);
            chartCamera.cullingMask = ~(1 << 5);
            chartCamera.allowHDR = false; chartCamera.allowMSAA = false;
            chartCamera.targetTexture = chartTexture;
            chartCamera.depth = -50;
            // Avoid postprocessing and shadow-map work for a tiny instrument.
            var data = cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;
            data.renderShadows = false;
        }

        private void Refresh()
        {
            RefreshShip();
            RefreshExpedition();
            RefreshResources();
            RefreshCombat();
            RefreshTarget();
            RefreshMinimap();
        }

        private void RefreshShip()
        {
            PrototypeShipRecoveryDirector recovery = PrototypeShipRecoveryDirector.Instance;
            bool reserve = recovery != null && recovery.State != PrototypeShipRecoveryState.MainShipActive;
            ShipProfileController profile =
                boundPlayer != null
                    ? boundPlayer.GetComponent<
                        ShipProfileController>()
                    : null;
            string shipLabel =
                profile?.Definition != null
                    ? profile.Definition.displayName
                        .ToUpperInvariant()
                    : "ANA GEMİ";
            string captainLabel =
                captainProgression != null
                    ? $"  •  SV. {captainProgression.Level}"
                    : "";
            shipNameText.text = reserve
                ? "YEDEK GEMİ" + captainLabel
                : shipLabel + captainLabel;

            float current = health != null ? health.CurrentHealth : 0f;
            float maximum = health != null ? health.MaximumHealth : 1f;
            float ratio = Mathf.Clamp01(current / Mathf.Max(1f, maximum));
            hullText.text = $"{current:0} / {maximum:0}";
            SetBar(hullFill, ratio);
            hullFillImage.color = ratio > 0.25f ? Color.white : new Color(1f,0.65f,0.50f,1f);

            float sail = subsystems != null
                ? subsystems.SailNormalized
                : 1f;
            float crew = subsystems != null
                ? subsystems.CrewNormalized
                : 1f;
            sailText.text = $"YELKEN  %{sail * 100f:0}";
            crewText.text = $"MÜRETTEBAT  %{crew * 100f:0}";
            sailText.color = sail < 0.4f ? Danger : Muted;
            crewText.color = crew < 0.4f ? Danger : Muted;
            SetBar(sailFill, sail);
            SetBar(crewFill, crew);
        }

        private void RefreshExpedition()
        {
            PrototypeExpeditionDirector director = PrototypeExpeditionDirector.Instance;
            pressureTrack.SetActive(director != null && director.IsActive);
            PrototypeExpeditionRegionDirector regions =
                PrototypeExpeditionRegionDirector.Instance;
            if (director == null)
            {
                bool harbor = regions != null &&
                    regions.CurrentRegionKind ==
                    PrototypeRegionKind.SafeHarbor;
                expeditionStateText.text = harbor
                    ? "GÜVENLİ LİMAN"
                    : "SEFER BEKLENİYOR";
                expeditionStateText.color = SeabornUiSkin.Ink;
                expeditionDetailText.text =
                    (harbor
                        ? "TİCARET VE HAZIRLIK MERKEZİ"
                        : "SİSTEMLER HAZIRLANIYOR") +
                    DailyContractLine();
                SetBar(pressureFill, 0f);
                return;
            }

            string regionName = regions != null
                ? regions.CurrentRegionName
                : "AÇIK DENİZ";
            string danger = regions != null
                ? regions.CurrentDangerLabel
                : "ORTA TEHLİKE";
            expeditionStateText.text =
                $"{StateLabel(director.State)}   •   {regionName}";
            expeditionStateText.color = SeabornUiSkin.Ink;
            int seconds = Mathf.FloorToInt(director.ElapsedTime);
            expeditionDetailText.text =
                (director.IsActive
                    ? $"{danger}   •   {seconds / 60:00}:{seconds % 60:00}   •   HEDEF {director.CurrentUnsecuredValue} / {director.RecommendedReturnValue}"
                    : director.State == PrototypeExpeditionState.AtHarbor
                        ? $"{danger}   •   SEFERE HAZIRLAN"
                        : $"{danger}   •   SÜRE {seconds / 60:00}:{seconds % 60:00}") +
                DailyContractLine();
            SetBar(pressureFill, director.PressureNormalized);
        }

        private static string DailyContractLine()
        {
            PrototypeContractBoard board =
                PrototypeContractBoard.Instance;
            PrototypeContractProgress active =
                board?.SelectedDailyContract;
            if (active == null)
            {
                return "";
            }

            return $"\nGÖREV  •  " +
                $"{active.Definition.Title.ToUpperInvariant()}  " +
                $"{active.Current}/{active.Definition.Target}";
        }

        private void RefreshResources()
        {
            silverText.text = $"SILVER  {wallet?.Silver ?? 0}";
            goldText.text = $"GOLD  {goldWallet?.Gold ?? 0}";
            int value = cargo != null ? cargo.UnsecuredSilverValue : 0;
            string capacity = cargo == null || cargo.MaximumSilverValue == int.MaxValue ? "" : $" / {cargo.MaximumSilverValue}";
            string active = "";
            if (consumables != null)
            {
                if (consumables.IsConcealed)
                    active +=
                        $"LIGHT {consumables.ConcealmentRemaining:0}s  ";
                if (consumables.CorsairRumRemaining > 0f)
                    active +=
                        $"RUM {consumables.CorsairRumRemaining:0}s  ";
                if (consumables.GaleElixirRemaining > 0f)
                    active +=
                        $"GALE {consumables.GaleElixirRemaining:0}s  ";
                if (consumables.IronbarkBrewRemaining > 0f)
                    active +=
                        $"IRON {consumables.IronbarkBrewRemaining:0}s";
            }
            cargoText.text = value > 0
                ? $"YÜK {value}{capacity}"
                : "AMBAR BOŞ";
            cargoText.color = value > 0 ? Gold : Muted;
            activeBuffText.text = active.Trim();
            resourceCard.sizeDelta = new Vector2(388f, 150f);

        }

        private void RefreshCombat()
        {
            if (harpoons == null && boundPlayer != null)
            {
                harpoons = boundPlayer.GetComponentInChildren<
                    HarpoonHuntingController>();
            }

            float port = broadside != null ? broadside.GetReloadProgress(BroadsideSide.Port) : 0f;
            float starboard = broadside != null ? broadside.GetReloadProgress(BroadsideSide.Starboard) : 0f;
            SetBar(portFill, port);
            SetBar(starboardFill, starboard);
            portText.text = port >= 0.999f ? "İSKELE HAZIR" : $"İSKELE  %{port * 100f:0}";
            starboardText.text = starboard >= 0.999f ? "SANCAK HAZIR" : $"SANCAK  %{starboard * 100f:0}";

            AmmunitionType selected = broadside != null ? broadside.SelectedAmmunition : AmmunitionType.Standard;
            ammoNameText.text = AmmoLabel(selected);


            float reload = harpoons != null ? harpoons.ReloadProgress : 0f;
            string harpoonName =
                harpoons?.SelectedHarpoon != null
                    ? $"{harpoons.SelectedHarpoon.weightKg:0} KG " +
                      (harpoons.SelectedHarpoon.weightKg >= 4f
                          ? "AĞIR"
                          : "HAFİF")
                    : "Zıpkın";
            harpoonText.text = $"ZIPKIN  •  {harpoonName}  •  SHIFT + SOL TIK";
            RefreshHotbar(selected);
            SetBar(harpoonFill, reload);
            bool locked =
                broadside != null &&
                broadside.IsBlockedBySafeHarbor;
            harborLockText.gameObject.SetActive(true);
            PrototypeHarborDockingDirector docking = PrototypeHarborDockingDirector.Instance;
            if (PrototypeExpeditionRegionDirector.IsHarborScene &&
                docking != null &&
                docking.NearbyStation != PrototypeHarborStation.None &&
                docking.DockedStation == PrototypeHarborStation.None)
            {
                harborLockText.text = $"E  •  {PrototypeHarborDockingDirector.StationLabel(docking.NearbyStation)} — YANAŞ";
                harborLockText.color = Gold;
            }
            else if (locked)
            {
                harborLockText.text =
                    "SİLAHLAR LİMANDA KİLİTLİ";
                harborLockText.color = Gold;
            }
            else if (repairs != null &&
                     repairs.IsRepairing)
            {
                harborLockText.text =
                    $"R  TAMİR EDİLİYOR  " +
                    $"%{repairs.CycleProgress * 100f:0}  •  " +
                    $"+{repairs.RepairAmount:0} / " +
                    $"{repairs.RepairInterval:0.#} sn";
                harborLockText.color = Success;
            }
            else if (repairs != null &&
                     repairs.LockRemaining > 0f)
            {
                harborLockText.text =
                    $"R  TAMİR KİLİTLİ  " +
                    $"{repairs.LockRemaining:0.0} sn";
                harborLockText.color = Danger;
            }
            else
            {
                string helm = motor != null
                    ? $"SEYİR {motor.SailingOrder} " +
                      $"{motor.SailingOrderLabel}  •  " +
                      $"DÜMEN {RudderLabel(motor.RudderAngleDegrees)}"
                    : "SEYİR DUR";
                harborLockText.text =
                    $"{helm}  •  R SAHA TAMİRİ";
                harborLockText.color = Muted;
            }
        }


        private HotbarSlot CreateHotbarSlot(RectTransform parent, int index, string key, string label)
        {
            GameObject root = new GameObject("Slot " + index,typeof(RectTransform));
            root.transform.SetParent(parent,false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = new Vector2(index * 98f,-31f);
            rect.sizeDelta = new Vector2(94,126);
            Image background = Art(rect,"Slot Frame",SeabornHudArt.Frame(4),Vector2.zero,new(94,94),false);
            Image glow = Art(rect,"Selected Frame",SeabornHudArt.Frame(5),new(-6,6),new(106,106),false);
            glow.gameObject.SetActive(false);
            CreateText(rect,interfaceFont,key,11,Cream,FontStyle.Normal,new(6,-7),new(82,17),TextAnchor.UpperCenter);
            Image icon = Art(rect,"Illustrated Item",SeabornHudArt.Icon(index),new(20,-26),new(54,51));
            Text count = CreateText(rect,interfaceFont,"0",12,Cream,FontStyle.Bold,new(44,-71),new(40,20),TextAnchor.UpperRight);
            Text timer = CreateText(rect,interfaceFont,"",12,Gold,FontStyle.Bold,new(6,-55),new(78,19),TextAnchor.UpperRight);
            CreateText(rect,interfaceFont,label,11,Cream,FontStyle.Normal,new(-1,-99),new(96,28),TextAnchor.UpperCenter);
            return new HotbarSlot { Background=background, Selection=glow.gameObject, Count=count, Timer=timer, Icon=icon };
        }

        private void RefreshHotbar(AmmunitionType selected)
        {
            SetSlot(0, broadside?.GetAmmunitionStock(AmmunitionType.Standard) ?? 0, broadside != null && selected == AmmunitionType.Standard);
            SetSlot(1, broadside?.GetAmmunitionStock(AmmunitionType.Chain) ?? 0, broadside != null && selected == AmmunitionType.Chain);
            SetSlot(2, broadside?.GetAmmunitionStock(AmmunitionType.Grapeshot) ?? 0, broadside != null && selected == AmmunitionType.Grapeshot);
            SetSlot(3, harpoons?.LightHarpoonStock ?? 0, harpoons != null && harpoons.SelectedHarpoonId == "light_2kg");
            SetSlot(4, harpoons?.HeavyHarpoonStock ?? 0, harpoons != null && harpoons.SelectedHarpoonId == "heavy_4kg");
            SetSlot(5, consumables?.TortugaTonics ?? 0, false, consumables?.TonicCooldownRemaining ?? 0f);
            SetSlot(6, consumables?.LightsOfTortuga ?? 0, false, consumables?.ConcealmentRemaining ?? 0f);
            SetSlot(7, consumables?.CorsairRum ?? 0, false, consumables?.CorsairRumRemaining ?? 0f);
            SetSlot(8, consumables?.GaleElixirs ?? 0, false, consumables?.GaleElixirRemaining ?? 0f);
            SetSlot(9, consumables?.IronbarkBrews ?? 0, false, consumables?.IronbarkBrewRemaining ?? 0f);
        }

        private void SetSlot(int index, int stock, bool selected, float seconds = 0f)
        {
            HotbarSlot slot = hotbar[index];
            if (slot == null) return;
            slot.Count.text = stock.ToString();
            slot.Count.color = stock > 0 ? Cream : Muted;
            slot.Icon.color = stock > 0 ? Color.white : new Color(0.5f,0.5f,0.5f,0.75f);
            slot.Selection.SetActive(selected);
            slot.Background.enabled = !selected;
            slot.Timer.text = seconds > 0f ? $"{Mathf.CeilToInt(seconds)}s" : "";
            slot.Timer.color = index == 5 ? Gold : Success;
        }

        private static string RudderLabel(
            float angle)
        {
            if (Mathf.Abs(angle) < 1f)
            {
                return "ORTA";
            }

            return angle < 0f
                ? $"İSKELE {Mathf.Abs(angle):0}°"
                : $"SANCAK {angle:0}°";
        }

        private void HandleTravelBlocked(
            string regionName,
            int requiredLevel)
        {
            ShowNotification(
                "BÖLGE KİLİTLİ",
                $"{regionName} için Kaptan SV. " +
                $"{requiredLevel} gerekir",
                Danger,
                4f
            );
        }

        private void HandleCaptainLevelChanged(int level)
        {
            ShowNotification(
                $"KAPTAN SEVİYESİ {level}",
                $"Harita tier sınırı: " +
                $"{captainProgression.HighestUnlockedMapTier}",
                Gold,
                4.5f
            );
        }

        private void HandleCatchAdded(string source, int value)
        {
            ShowNotification($"+{value} GÜVENCESİZ SILVER", source, Gold);
        }

        private void HandleCargoSecured(int value)
        {
            ShowNotification($"{value} SILVER GÜVENCEDE", "Liman teslimi tamamlandı", Success);
        }

        private void HandleCargoLost(int value)
        {
            ShowNotification($"{value} SILVER KAYBEDİLDİ", "Güvencesiz yük denizde kaldı", Danger, 4.5f);
        }

        private void HandleSunk()
        {
            ShowNotification("GEMİ BATTI", "Yedek gemi hazırlanıyor", Danger, 5f);
        }

        private void HandleHealthChanged(float current, float maximum)
        {
            float ratio = current / Mathf.Max(1f, maximum);
            if (ratio <= 0.3f && current > 0f && !criticalHullWarningShown)
            {
                criticalHullWarningShown = true;
                ShowNotification("KRİTİK GÖVDE HASARI", "Limana dönmeyi düşün", Danger, 4f);
            }
            else if (ratio > 0.4f)
            {
                criticalHullWarningShown = false;
            }
        }

        private void ShowNotification(string title, string detail, Color accent, float lifetime = 3.2f)
        {
            if (notificationRoot == null || interfaceFont == null) return;
            while (toasts.Count >= 4) RemoveToast(0);

            GameObject card = new("Gameplay Notification", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            card.transform.SetParent(notificationRoot, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(360f, 58f);
            card.GetComponent<Image>().sprite = SeabornHudArt.Frame(3);
            card.GetComponent<Image>().color = Color.white;
            card.GetComponent<Image>().raycastTarget = false;

            GameObject stripe = new("Accent", typeof(RectTransform), typeof(Image));
            stripe.transform.SetParent(card.transform, false);
            RectTransform stripeRect = stripe.GetComponent<RectTransform>();
            stripeRect.anchorMin = new Vector2(0f, 0f);
            stripeRect.anchorMax = new Vector2(0f, 1f);
            stripeRect.pivot = new Vector2(0f, 0.5f);
            stripeRect.sizeDelta = new Vector2(4f, 0f);
            stripeRect.anchoredPosition = Vector2.zero;
            stripe.GetComponent<Image>().color = accent;

            CreateText(rect, interfaceFont, title, 13, accent, FontStyle.Bold, new Vector2(14f, -8f), new Vector2(332f, 20f));
            CreateText(rect, interfaceFont, detail, 11, Cream, FontStyle.Normal, new Vector2(14f, -31f), new Vector2(332f, 18f));

            float now = Time.unscaledTime;
            toasts.Add(new Toast
            {
                Rect = rect,
                Group = card.GetComponent<CanvasGroup>(),
                CreatedAt = now,
                ExpiresAt = now + lifetime
            });
            LayoutNotifications();
        }

        private void AnimateNotifications()
        {
            float now = Time.unscaledTime;
            for (int index = toasts.Count - 1; index >= 0; index--)
            {
                Toast toast = toasts[index];
                if (now >= toast.ExpiresAt)
                {
                    RemoveToast(index);
                    continue;
                }

                float fadeIn = Mathf.InverseLerp(toast.CreatedAt, toast.CreatedAt + 0.18f, now);
                float fadeOut = Mathf.InverseLerp(toast.ExpiresAt, toast.ExpiresAt - 0.35f, now);
                toast.Group.alpha = Mathf.Min(fadeIn, fadeOut);
                float slide = Mathf.Lerp(24f, 0f, Mathf.SmoothStep(0f, 1f, fadeIn));
                Vector2 position = toast.Rect.anchoredPosition;
                position.x = slide;
                toast.Rect.anchoredPosition = position;
            }
        }

        private void LayoutNotifications()
        {
            for (int index = 0; index < toasts.Count; index++)
            {
                Vector2 position = toasts[index].Rect.anchoredPosition;
                position.y = -index * 66f;
                toasts[index].Rect.anchoredPosition = position;
            }
        }

        private void RemoveToast(int index)
        {
            if (index < 0 || index >= toasts.Count) return;
            Toast toast = toasts[index];
            toasts.RemoveAt(index);
            if (toast.Rect != null) Destroy(toast.Rect.gameObject);
            LayoutNotifications();
        }

        private void Unsubscribe()
        {
            if (health != null)
            {
                health.HealthChanged -= HandleHealthChanged;
                health.Sunk -= HandleSunk;
            }
            if (cargo != null)
            {
                cargo.CatchAdded -= HandleCatchAdded;
                cargo.CargoSecured -= HandleCargoSecured;
                cargo.CargoLost -= HandleCargoLost;
            }
            if (captainProgression != null)
            {
                captainProgression.LevelChanged -=
                    HandleCaptainLevelChanged;
            }
            if (worldMap != null)
            {
                worldMap.TravelBlocked -=
                    HandleTravelBlocked;
            }
        }

        private void OnDestroy()
        {
            if (chartCamera != null)
            {
                chartCamera.targetTexture = null;
                Destroy(chartCamera.gameObject);
            }
            if (chartTexture != null) { chartTexture.Release(); Destroy(chartTexture); }
            Unsubscribe();
        }

        private void RefreshMinimap()
        {
            PrototypeExpeditionRegionDirector map =
                PrototypeExpeditionRegionDirector.Instance;
            if (map == null ||
                minimapShip == null ||
                boundPlayer == null)
            {
                return;
            }

            minimapNameText.text =
                map.CurrentRegionName;
            Vector3 position = boundPlayer.position;
            float normalizedX = Mathf.Clamp(
                position.x / 90f,
                -1f,
                1f
            );
            float normalizedZ = Mathf.Clamp(
                position.z / 90f,
                -1f,
                1f
            );
            Vector2 mapPoint = Vector2.ClampMagnitude(new Vector2(normalizedX, normalizedZ), 0.95f);
            minimapShip.anchoredPosition = new Vector2(120f + mapPoint.x * 120f, -120f + mapPoint.y * 120f);
            minimapShip.localRotation = Quaternion.Euler(0f,0f,-boundPlayer.eulerAngles.y);

            PrototypeRegionKind kind =
                map.CurrentRegionKind;
            minimapNorth.SetActive(
                kind == PrototypeRegionKind.SafeHarbor
            );
            minimapWest.SetActive(
                kind ==
                PrototypeRegionKind.CentralWaters ||
                kind ==
                PrototypeRegionKind.EasternReach
            );
            minimapEast.SetActive(
                kind ==
                PrototypeRegionKind.CentralWaters ||
                kind ==
                PrototypeRegionKind.WesternReach
            );
            minimapSouth.SetActive(
                kind != PrototypeRegionKind.SafeHarbor
            );

            minimapHintText.text = kind switch
            {
                PrototypeRegionKind.SafeHarbor =>
                    "N  •  MERKEZ SULAR",
                PrototypeRegionKind.WesternReach =>
                    "E  •  MERKEZ   S  •  LİMAN",
                PrototypeRegionKind.EasternReach =>
                    "W  •  MERKEZ   S  •  LİMAN",
                _ =>
                    "W BATI • E DOĞU • S LİMAN"
            };
        }

        private void RefreshTarget()
        {
            if (boundPlayer == null || targetPanel == null)
            {
                return;
            }

            EnemyShipController[] enemies =
                FindObjectsByType<EnemyShipController>(
                    FindObjectsSortMode.None
                );
            EnemyShipController selected = null;
            float bestScore = float.MaxValue;

            foreach (EnemyShipController enemy in enemies)
            {
                if (enemy == null ||
                    !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                ShipHealth enemyHealth =
                    enemy.GetComponent<ShipHealth>();
                if (enemyHealth == null || enemyHealth.IsSunk)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    boundPlayer.position,
                    enemy.transform.position
                );
                float maximumDistance =
                    enemy.IsAggressive ? 30f : 14f;
                if (distance > maximumDistance)
                {
                    continue;
                }

                float score = distance -
                    (enemy.IsAggressive ? 100f : 0f);
                if (score < bestScore)
                {
                    bestScore = score;
                    selected = enemy;
                }
            }

            targetPanel.SetActive(selected != null);
            if (selected == null) return;

            ShipHealth health =
                selected.GetComponent<ShipHealth>();
            ShipSubsystemController systems =
                selected.GetComponent<
                    ShipSubsystemController>();

            float hull = Mathf.Clamp01(
                health.CurrentHealth /
                Mathf.Max(1f, health.MaximumHealth)
            );
            float sail = systems != null
                ? systems.SailNormalized
                : 1f;
            float crew = systems != null
                ? systems.CrewNormalized
                               : 1f;

            targetNameText.text =
                RoleLabel(selected.Archetype);
            targetStateText.text = selected.IsAggressive
                ? "ÇATIŞMADA"
                : "PASİF";
            targetStateText.color = selected.IsAggressive
                ? Danger
                : Muted;
            targetSailText.text =
                $"YELKEN  %{sail * 100f:0}";
            targetCrewText.text =
                $"MÜRETTEBAT  %{crew * 100f:0}";
            targetSailText.color =
                sail < 0.4f ? Danger : Muted;
            targetCrewText.color =
                crew < 0.4f ? Danger : Muted;
            targetHullImage.color =
                hull > 0.55f
                    ? Success
                    : hull > 0.25f
                        ? Gold
                        : Danger;
            SetBar(targetHullFill, hull);
            SetBar(targetSailFill, sail);
            SetBar(targetCrewFill, crew);
        }

        private static string RoleLabel(
            EnemyShipArchetype archetype)
        {
            return archetype switch
            {
                EnemyShipArchetype.Skirmisher =>
                    "AVCI",
                EnemyShipArchetype.Gunship =>
                    "TOPÇU",
                EnemyShipArchetype.Marauder =>
                    "YAĞMACI",
                _ => "DÜŞMAN"
            };
        }

        private RectTransform CreateCard(string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject root = new GameObject(name,typeof(RectTransform));
            root.transform.SetParent(transform,false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private static Text CreateText(RectTransform parent, Font font, string value, int size, Color color, FontStyle style, Vector2 position, Vector2 dimensions, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            GameObject item = new("Text", typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            Text text = item.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateMapDot(
            RectTransform parent,
            string dotName,
            Vector2 position,
            Color color,
            float size)
        {
            GameObject dot = new(
                dotName,
                typeof(RectTransform),
                typeof(Image)
            );
            dot.transform.SetParent(parent, false);
            RectTransform rect =
                dot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = Vector2.one * size;
            Image image = dot.GetComponent<Image>();
            image.color = color;
            image.sprite = SeabornHudArt.CircleMask;
            image.raycastTarget = false;
            if (dotName == "Player")
            {
                image.enabled = false;
                CreateText(rect, Resources.Load<Font>("SeabornHud/DejaVuSerif"),
                    "▲", 22, color, FontStyle.Normal,
                    new Vector2(-7f, 8f), new Vector2(24f, 24f),
                    TextAnchor.MiddleCenter);
            }
            return dot;
        }

        private static RectTransform CreateBar(RectTransform parent, string name, Vector2 position, Vector2 dimensions, out Image fillImage)
        {
            GameObject background = new(name + " Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            background.GetComponent<Image>().sprite = SeabornHudArt.Frame(6);
            background.GetComponent<Image>().color = new Color(0.25f,0.40f,0.40f,1f);
            background.GetComponent<Image>().raycastTarget = false;

            GameObject fill = new(name + " Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(background.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillImage = fill.GetComponent<Image>();
            fillImage.sprite = SeabornHudArt.Frame(6);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.color = Color.white;
            fillImage.raycastTarget = false;
            return fillRect;
        }

        private static void SetBar(RectTransform fill, float value)
        {
            if (fill == null) return;
            Image image = fill.GetComponent<Image>();
            if (image != null && image.type == Image.Type.Filled)
            {
                image.fillAmount = Mathf.Clamp01(value);
                return;
            }
            Vector2 maximum = fill.anchorMax;
            value = Mathf.Clamp01(value);
            fill.gameObject.SetActive(value > 0f);
            maximum.x = value;
            fill.anchorMax = maximum;
            fill.offsetMax = new Vector2(value <= 0f ? 0f : -2f, -2f);
        }

        private static string StateLabel(PrototypeExpeditionState state) => state switch
        {
            PrototypeExpeditionState.AtHarbor => "GÜVENLİ LİMAN",
            PrototypeExpeditionState.Underway => "SEFERDE",
            PrototypeExpeditionState.ReturnRecommended => "DÖNÜŞ ÖNERİLİYOR",
            PrototypeExpeditionState.Completed => "SEFER TAMAMLANDI",
            PrototypeExpeditionState.Failed => "SEFER BAŞARISIZ",
            _ => "SEFER"
        };

        private static string AmmoLabel(AmmunitionType type) => type switch
        {
            AmmunitionType.Chain => "ZİNCİRLİ GÜLLE",
            AmmunitionType.Grapeshot => "SAÇMA MÜHİMMATI",
            _ => "STANDART GÜLLE"
        };
    }

    internal sealed class GameplayHudBootstrap : MonoBehaviour
    {
        private float nextScanTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<GameplayHudBootstrap>() != null) return;
            new GameObject("Prototype Gameplay HUD Bootstrap").AddComponent<GameplayHudBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime) return;
            nextScanTime = Time.unscaledTime + 0.5f;
            ManualBroadsideAimController player = FindFirstObjectByType<ManualBroadsideAimController>();
            if (player != null) PrototypeGameplayHud.EnsureCreated(player.transform);
        }
    }
}
