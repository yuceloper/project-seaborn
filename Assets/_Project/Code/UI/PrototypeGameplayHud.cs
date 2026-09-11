using System.Collections.Generic;
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
        private Transform boundPlayer;
        private RectTransform notificationRoot;
        private Font interfaceFont;
        private readonly List<Toast> toasts = new();
        private bool criticalHullWarningShown;

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
            broadside = player.GetComponentInChildren<BroadsideController>();
            harpoons = player.GetComponentInChildren<HarpoonHuntingController>();
            cargo = player.GetComponentInChildren<PrototypeHuntCargo>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
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
            interfaceFont = font;

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

            GameObject notifications = new("Notifications", typeof(RectTransform));
            notifications.transform.SetParent(transform, false);
            notificationRoot = notifications.GetComponent<RectTransform>();
            notificationRoot.anchorMin = new Vector2(1f, 1f);
            notificationRoot.anchorMax = new Vector2(1f, 1f);
            notificationRoot.pivot = new Vector2(1f, 1f);
            notificationRoot.anchoredPosition = new Vector2(-24f, -138f);
            notificationRoot.sizeDelta = new Vector2(360f, 260f);
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
            card.GetComponent<Image>().color = Navy;

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
        }

        private void OnDestroy()
        {
            Unsubscribe();
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
