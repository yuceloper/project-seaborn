using Seaborn.Progression;
using Seaborn.Hunting;
using Seaborn.Harbor;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipyardUpgradePanel :
        MonoBehaviour
    {
        private static readonly Color Navy =
            new(0.025f, 0.075f, 0.105f, 0.97f);
        private static readonly Color NavyLight =
            new(0.06f, 0.15f, 0.18f, 0.96f);
        private static readonly Color Gold =
            new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream =
            new(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Muted =
            new(0.57f, 0.68f, 0.68f, 1f);
        private static readonly Color Success =
            new(0.3f, 0.76f, 0.57f, 1f);
        private static readonly Color Danger =
            new(0.9f, 0.43f, 0.34f, 1f);

        private PrototypeShipEquipment equipment;
        private PrototypeSilverWallet wallet;
        private GameObject panel;
        private Text silverText;
        private Text statusText;
        private UpgradeRow hull;
        private UpgradeRow cannons;
        private UpgradeRow harpoon;
        private float nextRefreshTime;
        private float statusExpiresAt;

        private sealed class UpgradeRow
        {
            public Text Title;
            public Text Effect;
            public Button Button;
            public Text ButtonText;
        }

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeShipyardUpgradePanel panel =
                FindFirstObjectByType<
                    PrototypeShipyardUpgradePanel>();

            if (panel == null)
            {
                GameObject root = new(
                    "Prototype Shipyard Upgrade Panel");
                panel = root.AddComponent<
                    PrototypeShipyardUpgradePanel>();
            }

            panel.Bind(player);
        }

        private void Awake()
        {
            BuildInterface();
        }

        private void Bind(Transform player)
        {
            equipment = player.GetComponentInChildren<
                PrototypeShipEquipment>();
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime) return;
            nextRefreshTime = Time.unscaledTime + 0.15f;
            Refresh();
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 81;

            CanvasScaler scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            panel = new GameObject(
                "Shipyard Panel",
                typeof(RectTransform),
                typeof(Image)
            );
            panel.transform.SetParent(transform, false);
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition =
                new Vector2(-24f, 24f);
            rect.sizeDelta = new Vector2(430f, 366f);
            panel.GetComponent<Image>().color = Navy;

            CreateAccent(rect);
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

            CreateText(
                rect, font, "TERSANE GELİŞTİRMELERİ",
                20, Gold, FontStyle.Bold,
                new Vector2(20f, -18f),
                new Vector2(280f, 30f)
            );
            silverText = CreateText(
                rect, font, "0 SILVER",
                15, Cream, FontStyle.Bold,
                new Vector2(304f, -20f),
                new Vector2(106f, 26f),
                TextAnchor.UpperRight
            );
            statusText = CreateText(
                rect, font,
                "Kalıcı gemi donanımını geliştir.",
                12, Muted, FontStyle.Normal,
                new Vector2(20f, -52f),
                new Vector2(390f, 22f)
            );

            hull = CreateRow(
                rect, font, "GÜÇLENDİRİLMİŞ GÖVDE",
                ShipUpgradeTrack.Hull, -88f
            );
            cannons = CreateRow(
                rect, font, "TOP TAKIMI",
                ShipUpgradeTrack.Cannons, -176f
            );
            harpoon = CreateRow(
                rect, font, "ZIPKIN DONANIMI",
                ShipUpgradeTrack.HarpoonGear, -264f
            );
        }

        private UpgradeRow CreateRow(
            RectTransform parent,
            Font font,
            string title,
            ShipUpgradeTrack track,
            float y)
        {
            GameObject block = new(
                title,
                typeof(RectTransform),
                typeof(Image)
            );
            block.transform.SetParent(parent, false);
            RectTransform rect =
                block.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition =
                new Vector2(20f, y);
            rect.sizeDelta = new Vector2(390f, 76f);
            block.GetComponent<Image>().color = NavyLight;

            UpgradeRow row = new();
            row.Title = CreateText(
                rect, font, title,
                13, Cream, FontStyle.Bold,
                new Vector2(14f, -9f),
                new Vector2(230f, 21f)
            );
            row.Effect = CreateText(
                rect, font, "",
                11, Muted, FontStyle.Normal,
                new Vector2(14f, -37f),
                new Vector2(235f, 21f)
            );

            GameObject buttonObject = new(
                "Upgrade",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            buttonObject.transform.SetParent(rect, false);
            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition =
                new Vector2(256f, -14f);
            buttonRect.sizeDelta =
                new Vector2(120f, 48f);
            Image background =
                buttonObject.GetComponent<Image>();
            background.color = Gold;
            row.Button =
                buttonObject.GetComponent<Button>();
            row.Button.targetGraphic = background;
            row.Button.onClick.AddListener(
                () => Upgrade(track)
            );
            row.ButtonText = CreateText(
                buttonRect, font, "YÜKSELT",
                12, Navy, FontStyle.Bold,
                Vector2.zero,
                new Vector2(120f, 48f),
                TextAnchor.MiddleCenter
            );
            RectTransform textRect =
                row.ButtonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return row;
        }

        private void Refresh()
        {
            if (panel == null) return;

            PrototypeHarborDockingDirector docking =
                PrototypeHarborDockingDirector.Instance;
            bool visible =
                equipment != null &&
                equipment.CanUseShipyard &&
                docking != null &&
                docking.IsDockedAt(
                    PrototypeHarborStation.Shipyard);
            panel.SetActive(visible);
            if (!visible) return;

            silverText.text =
                $"{(wallet != null ? wallet.Silver : 0)} SILVER";

            RefreshRow(
                hull, ShipUpgradeTrack.Hull);
            RefreshRow(
                cannons, ShipUpgradeTrack.Cannons);
            RefreshRow(
                harpoon, ShipUpgradeTrack.HarpoonGear);

            if (Time.unscaledTime >= statusExpiresAt)
            {
                statusText.text =
                    "Kalıcı gemi donanımını geliştir.";
                statusText.color = Muted;
            }
        }

        private void RefreshRow(
            UpgradeRow row,
            ShipUpgradeTrack track)
        {
            int level = equipment.GetLevel(track);
            bool maximum =
                level >= PrototypeShipEquipment.MaximumLevel;
            int cost = equipment.GetUpgradeCost(track);
            row.Title.text =
                $"{TrackName(track)}   SV. {level}/" +
                $"{PrototypeShipEquipment.MaximumLevel}";
            row.Effect.text =
                equipment.EffectDescription(track);
            row.ButtonText.text = maximum
                ? "AZAMİ"
                : $"{cost} SILVER";
            row.Button.interactable =
                !maximum &&
                wallet != null &&
                wallet.Silver >= cost;
        }

        private void Upgrade(ShipUpgradeTrack track)
        {
            ShipUpgradeResult result =
                equipment != null
                    ? equipment.TryUpgrade(track)
                    : ShipUpgradeResult.Unavailable;

            switch (result)
            {
                case ShipUpgradeResult.Completed:
                    ShowStatus(
                        $"{TrackName(track)} geliştirildi.",
                        Success
                    );
                    break;
                case ShipUpgradeResult.InsufficientSilver:
                    ShowStatus(
                        "Yeterli silver yok.",
                        Danger
                    );
                    break;
                case ShipUpgradeResult.MaximumLevel:
                    ShowStatus(
                        "Bu donanım azami seviyede.",
                        Muted
                    );
                    break;
                default:
                    ShowStatus(
                        "Tersane yalnızca güvenli limanda kullanılabilir.",
                        Muted
                    );
                    break;
            }

            Refresh();
        }

        private void ShowStatus(
            string message,
            Color color)
        {
            statusText.text = message;
            statusText.color = color;
            statusExpiresAt =
                Time.unscaledTime + 3f;
        }

        private static string TrackName(
            ShipUpgradeTrack track)
        {
            return track switch
            {
                ShipUpgradeTrack.Hull =>
                    "GÜÇLENDİRİLMİŞ GÖVDE",
                ShipUpgradeTrack.Cannons =>
                    "TOP TAKIMI",
                ShipUpgradeTrack.HarpoonGear =>
                    "ZIPKIN DONANIMI",
                _ => "DONANIM"
            };
        }

        private static void CreateAccent(
            RectTransform parent)
        {
            GameObject accent = new(
                "Accent",
                typeof(RectTransform),
                typeof(Image)
            );
            accent.transform.SetParent(parent, false);
            RectTransform rect =
                accent.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 3f);
            rect.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = Gold;
        }

        private static Text CreateText(
            RectTransform parent,
            Font font,
            string value,
            int size,
            Color color,
            FontStyle style,
            Vector2 position,
            Vector2 dimensions,
            TextAnchor alignment =
                TextAnchor.UpperLeft)
        {
            GameObject item = new(
                "Text",
                typeof(RectTransform),
                typeof(Text)
            );
            item.transform.SetParent(parent, false);
            RectTransform rect =
                item.GetComponent<RectTransform>();
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
    }
}
