using Seaborn.Combat;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using Seaborn.Recovery;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeGameplayHud : MonoBehaviour
    {
        private static readonly Color Navy = new(0.025f, 0.075f, 0.105f, 0.96f);
        private static readonly Color NavyLight = new(0.06f, 0.15f, 0.18f, 0.94f);
        private static readonly Color Gold = new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream = new(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Muted = new(0.57f, 0.68f, 0.68f, 1f);
        private static readonly Color Success = new(0.3f, 0.76f, 0.57f, 1f);
        private static readonly Color Danger = new(0.82f, 0.28f, 0.22f, 1f);

        private ShipHealth health;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;

        private Text shipNameText;
        private Text hullText;
        private RectTransform hullFill;
        private Image hullFillImage;
        private Text expeditionStateText;
        private Text expeditionDetailText;
        private RectTransform pressureFill;
        private Text silverText;
        private Text cargoText;
        private Text ammoNameText;
        private Text ammoStockText;
        private Text portText;
        private Text starboardText;
        private RectTransform portFill;
        private RectTransform starboardFill;
        private Text harpoonText;
        private RectTransform harpoonFill;
        private Text harborLockText;
        private float nextRefreshTime;

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
            if (Time.unscaledTime < nextRefreshTime) return;
            nextRefreshTime = Time.unscaledTime + 0.1f;
            Refresh();
        }

        private void Bind(Transform player)
        {
            health = player.GetComponentInChildren<ShipHealth>();
            broadside = player.GetComponentInChildren<BroadsideController>();
            harpoons = player.GetComponentInChildren<HarpoonHuntingController>();
            cargo = player.GetComponentInChildren<PrototypeHuntCargo>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
            Refresh();
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform ship = CreateCard("Ship", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(330f, 98f));
            shipNameText = CreateText(ship, font, "ANA GEMİ", 15, Gold, FontStyle.Bold, new Vector2(16f, -11f), new Vector2(298f, 22f));
            hullText = CreateText(ship, font, "GÖVDE", 13, Cream, FontStyle.Normal, new Vector2(16f, -39f), new Vector2(298f, 22f));
            hullFill = CreateBar(ship, "Hull", new Vector2(16f, -70f), new Vector2(298f, 12f), out hullFillImage);

            RectTransform expedition = CreateCard("Expedition", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(500f, 88f));
            expeditionStateText = CreateText(expedition, font, "GÜVENLİ LİMAN", 16, Gold, FontStyle.Bold, new Vector2(16f, -10f), new Vector2(468f, 24f), TextAnchor.UpperCenter);
            expeditionDetailText = CreateText(expedition, font, "SEFERE HAZIRLAN", 13, Cream, FontStyle.Normal, new Vector2(16f, -38f), new Vector2(468f, 22f), TextAnchor.UpperCenter);
            pressureFill = CreateBar(expedition, "Pressure", new Vector2(16f, -69f), new Vector2(468f, 7f), out _);

            RectTransform resources = CreateCard("Resources", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(310f, 98f));
            silverText = CreateText(resources, font, "SILVER  0", 16, Gold, FontStyle.Bold, new Vector2(16f, -12f), new Vector2(278f, 24f), TextAnchor.UpperRight);
            cargoText = CreateText(resources, font, "GÜVENCESİZ YÜK  0", 13, Cream, FontStyle.Normal, new Vector2(16f, -45f), new Vector2(278f, 40f), TextAnchor.UpperRight);

            RectTransform combat = CreateCard("Combat", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(780f, 116f));
            portText = CreateText(combat, font, "İSKELE HAZIR", 12, Cream, FontStyle.Bold, new Vector2(18f, -14f), new Vector2(178f, 22f));
            portFill = CreateBar(combat, "Port", new Vector2(18f, -43f), new Vector2(178f, 10f), out _);
            ammoNameText = CreateText(combat, font, "STANDART GÜLLE", 15, Gold, FontStyle.Bold, new Vector2(214f, -11f), new Vector2(352f, 22f), TextAnchor.UpperCenter);
            ammoStockText = CreateText(combat, font, "1: 0   2: 0   3: 0", 12, Muted, FontStyle.Normal, new Vector2(214f, -39f), new Vector2(352f, 22f), TextAnchor.UpperCenter);
            harpoonText = CreateText(combat, font, "Zıpkın  0", 12, Cream, FontStyle.Bold, new Vector2(214f, -68f), new Vector2(352f, 20f), TextAnchor.UpperCenter);
            harpoonFill = CreateBar(combat, "Harpoon", new Vector2(292f, -94f), new Vector2(196f, 7f), out _);
            starboardText = CreateText(combat, font, "SANCAK HAZIR", 12, Cream, FontStyle.Bold, new Vector2(584f, -14f), new Vector2(178f, 22f), TextAnchor.UpperRight);
            starboardFill = CreateBar(combat, "Starboard", new Vector2(584f, -43f), new Vector2(178f, 10f), out _);
            harborLockText = CreateText(combat, font, "SİLAHLAR LİMANDA KİLİTLİ", 12, Gold, FontStyle.Bold, new Vector2(18f, -83f), new Vector2(744f, 20f), TextAnchor.UpperCenter);
        }

        private void Refresh()
        {
            RefreshShip();
            RefreshExpedition();
            RefreshResources();
            RefreshCombat();
        }

        private void RefreshShip()
        {
            PrototypeShipRecoveryDirector recovery = PrototypeShipRecoveryDirector.Instance;
            bool reserve = recovery != null && recovery.State != PrototypeShipRecoveryState.MainShipActive;
            shipNameText.text = reserve ? "YEDEK GEMİ" : "ANA GEMİ";

            float current = health != null ? health.CurrentHealth : 0f;
            float maximum = health != null ? health.MaximumHealth : 1f;
            float ratio = Mathf.Clamp01(current / Mathf.Max(1f, maximum));
            hullText.text = $"GÖVDE   {current:0} / {maximum:0}";
            SetBar(hullFill, ratio);
            hullFillImage.color = ratio > 0.55f ? Success : ratio > 0.25f ? Gold : Danger;
        }

        private void RefreshExpedition()
        {
            PrototypeExpeditionDirector director = PrototypeExpeditionDirector.Instance;
            if (director == null)
            {
                expeditionStateText.text = "SEFER BEKLENİYOR";
                expeditionDetailText.text = "SİSTEMLER HAZIRLANIYOR";
                SetBar(pressureFill, 0f);
                return;
            }

            expeditionStateText.text = StateLabel(director.State);
            int seconds = Mathf.FloorToInt(director.ElapsedTime);
            expeditionDetailText.text = director.IsActive
                ? $"{seconds / 60:00}:{seconds % 60:00}   •   HEDEF {director.CurrentUnsecuredValue} / {director.RecommendedReturnValue}"
                : director.State == PrototypeExpeditionState.AtHarbor ? "SEFERE HAZIRLAN" : $"SÜRE {seconds / 60:00}:{seconds % 60:00}";
            SetBar(pressureFill, director.PressureNormalized);
        }

        private void RefreshResources()
        {
            silverText.text = $"SILVER   {(wallet != null ? wallet.Silver : 0)}";
            int value = cargo != null ? cargo.UnsecuredSilverValue : 0;
            string capacity = cargo == null || cargo.MaximumSilverValue == int.MaxValue ? "SINIRSIZ" : cargo.MaximumSilverValue.ToString();
            cargoText.text = $"GÜVENCESİZ YÜK   {value} / {capacity}\nBATIŞTA KAYBEDİLİR";
        }

        private void RefreshCombat()
        {
            float port = broadside != null ? broadside.GetReloadProgress(BroadsideSide.Port) : 0f;
            float starboard = broadside != null ? broadside.GetReloadProgress(BroadsideSide.Starboard) : 0f;
            SetBar(portFill, port);
            SetBar(starboardFill, starboard);
            portText.text = port >= 0.999f ? "İSKELE HAZIR" : $"İSKELE  %{port * 100f:0}";
            starboardText.text = starboard >= 0.999f ? "SANCAK HAZIR" : $"SANCAK  %{starboard * 100f:0}";

            AmmunitionType selected = broadside != null ? broadside.SelectedAmmunition : AmmunitionType.Standard;
            ammoNameText.text = AmmoLabel(selected);
            ammoStockText.text = broadside == null ? "1: 0   2: 0   3: 0" : $"1: {broadside.GetAmmunitionStock(AmmunitionType.Standard)}   2: {broadside.GetAmmunitionStock(AmmunitionType.Chain)}   3: {broadside.GetAmmunitionStock(AmmunitionType.Grapeshot)}";

            float reload = harpoons != null ? harpoons.ReloadProgress : 0f;
            harpoonText.text = $"Zıpkın   {(harpoons != null ? harpoons.HarpoonStock : 0)}";
            SetBar(harpoonFill, reload);
            bool locked = broadside != null && broadside.IsBlockedBySafeHarbor;
            harborLockText.gameObject.SetActive(locked);
        }

        private RectTransform CreateCard(string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject card = new(name, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(transform, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            card.GetComponent<Image>().color = Navy;

            GameObject accent = new("Accent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(card.transform, false);
            RectTransform accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.sizeDelta = new Vector2(0f, 3f);
            accentRect.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = Gold;
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
            background.GetComponent<Image>().color = NavyLight;

            GameObject fill = new(name + " Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(background.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            fillImage = fill.GetComponent<Image>();
            fillImage.color = Gold;
            fillImage.raycastTarget = false;
            return fillRect;
        }

        private static void SetBar(RectTransform fill, float value)
        {
            if (fill == null) return;
            Vector2 maximum = fill.anchorMax;
            maximum.x = Mathf.Clamp01(value);
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
