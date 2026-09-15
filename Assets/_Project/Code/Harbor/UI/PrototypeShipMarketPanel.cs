using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipMarketPanel : MonoBehaviour
    {
        private static readonly Color Navy =
            new Color(0.025f, 0.075f, 0.105f, 0.97f);
        private static readonly Color NavyLight =
            new Color(0.06f, 0.15f, 0.18f, 0.96f);
        private static readonly Color Gold =
            new Color(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream =
            new Color(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Muted =
            new Color(0.57f, 0.68f, 0.68f, 1f);
        private static readonly Color Success =
            new Color(0.3f, 0.76f, 0.57f, 1f);
        private static readonly Color Danger =
            new Color(0.9f, 0.43f, 0.34f, 1f);

        private sealed class ShipRow
        {
            public string Id;
            public Text Title;
            public Text Stats;
            public Button Action;
            public Text ActionLabel;
        }

        private Transform player;
        private PrototypeFleetInventory fleet;
        private PrototypeSilverWallet wallet;
        private PrototypeCaptainProgression progression;
        private GameObject panel;
        private Text silverText;
        private Text statusText;
        private ShipRow starter;
        private ShipRow rat;
        private ShipRow dreadwake;
        private float nextRefreshTime;
        private float statusExpiresAt;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeShipMarketPanel view =
                FindFirstObjectByType<
                    PrototypeShipMarketPanel>();
            if (view == null)
            {
                GameObject root =
                    new GameObject("Prototype Ship Market Panel");
                view = root.AddComponent<
                    PrototypeShipMarketPanel>();
            }

            view.Bind(player);
        }

        private void Awake()
        {
            BuildInterface();
        }

        private void Bind(Transform target)
        {
            player = target;
            ResolveBindings();
            Refresh();
        }

        private void ResolveBindings()
        {
            if (player == null) return;
            fleet = player.GetComponentInChildren<
                PrototypeFleetInventory>();
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            progression = player.GetComponentInChildren<
                PrototypeCaptainProgression>();
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
                "Ship Market",
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
                new Vector2(-936f, 24f);
            rect.sizeDelta = new Vector2(450f, 462f);
            panel.GetComponent<Image>().color = Navy;

            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            CreateAccent(rect);

            CreateText(
                rect, font, "GEMİ PAZARI",
                20, Gold, FontStyle.Bold,
                new Vector2(20f, -18f),
                new Vector2(250f, 30f)
            );
            silverText = CreateText(
                rect, font, "0 SILVER",
                15, Cream, FontStyle.Bold,
                new Vector2(310f, -20f),
                new Vector2(120f, 26f),
                TextAnchor.UpperRight
            );
            statusText = CreateText(
                rect, font,
                "Filondaki gemiler kalıcıdır.",
                11, Muted, FontStyle.Normal,
                new Vector2(20f, -55f),
                new Vector2(410f, 28f)
            );

            starter = CreateShipRow(
                rect, font, "starter_sloop", -96f);
            rat = CreateShipRow(
                rect, font, "rat_sails", -210f);
            dreadwake = CreateShipRow(
                rect, font, "dreadwake", -324f);
        }

        private ShipRow CreateShipRow(
            RectTransform parent,
            Font font,
            string shipId,
            float y)
        {
            ShipCatalog.TryGet(
                shipId,
                out ShipDefinition definition);

            GameObject block = new GameObject(
                shipId,
                typeof(RectTransform),
                typeof(Image)
            );
            block.transform.SetParent(parent, false);
            RectTransform rect =
                block.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, y);
            rect.sizeDelta = new Vector2(410f, 100f);
            block.GetComponent<Image>().color = NavyLight;

            ShipRow row = new ShipRow();
            row.Id = shipId;
            row.Title = CreateText(
                rect, font,
                definition?.displayName ?? shipId,
                14, Cream, FontStyle.Bold,
                new Vector2(14f, -10f),
                new Vector2(240f, 22f)
            );
            row.Stats = CreateText(
                rect, font, "",
                10, Muted, FontStyle.Normal,
                new Vector2(14f, -38f),
                new Vector2(270f, 48f)
            );

            row.Action = CreateButton(
                rect, font, "SEÇ",
                new Vector2(288f, -27f),
                new Vector2(108f, 48f),
                () => RunAction(shipId),
                out row.ActionLabel
            );
            return row;
        }

        private void Refresh()
        {
            if (panel == null) return;
            ResolveBindings();

            bool visible =
                fleet != null &&
                fleet.CanUseShipyard &&
                PrototypeHarborUiCoordinator.IsSelected(
                    PrototypeHarborTab.ShipMarket);
            panel.SetActive(visible);
            if (!visible) return;

            int silver = wallet != null ? wallet.Silver : 0;
            silverText.text = $"{silver} SILVER";
            RefreshRow(starter, silver);
            RefreshRow(rat, silver);
            RefreshRow(dreadwake, silver);

            if (Time.unscaledTime >= statusExpiresAt)
            {
                statusText.text =
                    "Filondaki gemiler kalıcıdır.";
                statusText.color = Muted;
            }
        }

        private void RefreshRow(
            ShipRow row,
            int silver)
        {
            if (!ShipCatalog.TryGet(
                    row.Id,
                    out ShipDefinition definition))
            {
                row.Action.interactable = false;
                return;
            }

            bool owned = fleet.Owns(row.Id);
            bool active =
                fleet.ActiveShipId == row.Id;

            row.Stats.text =
                $"Top {definition.cannonSlots}  •  " +
                $"Menzil {definition.cannonRange:0.#}  •  " +
                $"Hız {definition.speedMultiplier:0.##}\n" +
                $"Gövde {definition.maximumHealth:0}  •  " +
                $"Lisans SV. {definition.requiredCaptainLevel}";

            if (active)
            {
                row.ActionLabel.text = "AKTİF";
                row.Action.interactable = false;
            }
            else if (owned)
            {
                row.ActionLabel.text = "AKTİF ET";
                row.Action.interactable = true;
            }
            else
            {
                bool levelUnlocked =
                    progression != null &&
                    progression.MeetsLevel(
                        definition.requiredCaptainLevel);
                row.ActionLabel.text = levelUnlocked
                    ? $"{definition.basePrice} S"
                    : $"SV. {definition.requiredCaptainLevel}";
                row.Action.interactable =
                    levelUnlocked &&
                    silver >= definition.basePrice;
            }
        }

        private void RunAction(string shipId)
        {
            ShipMarketResult result = fleet.Owns(shipId)
                ? fleet.TryActivate(shipId)
                : fleet.TryPurchase(shipId);

            switch (result)
            {
                case ShipMarketResult.Completed:
                    ShowStatus(
                        fleet.ActiveShipId == shipId
                            ? "Aktif gemi değiştirildi."
                            : "Gemi filoya eklendi.",
                        Success
                    );
                    break;
                case ShipMarketResult.InsufficientSilver:
                    ShowStatus("Yeterli silver yok.", Danger);
                    break;
                case ShipMarketResult.LevelLocked:
                    ShowStatus(
                        "Kaptan seviyesi bu gemi lisansı için yetersiz.",
                        Danger
                    );
                    break;
                case ShipMarketResult.AlreadyOwned:
                    ShowStatus("Bu gemi zaten filonda.", Muted);
                    break;
                default:
                    ShowStatus(
                        "Bu işlem şu anda yapılamıyor.",
                        Muted
                    );
                    break;
            }
        }

        private void ShowStatus(string message, Color color)
        {
            statusText.text = message;
            statusText.color = color;
            statusExpiresAt = Time.unscaledTime + 3f;
            Refresh();
        }

        private static Button CreateButton(
            RectTransform parent,
            Font font,
            string label,
            Vector2 position,
            Vector2 size,
            UnityEngine.Events.UnityAction action,
            out Text labelText)
        {
            GameObject item = new GameObject(
                label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            item.transform.SetParent(parent, false);
            RectTransform rect =
                item.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            item.GetComponent<Image>().color = Gold;

            Button button = item.GetComponent<Button>();
            button.targetGraphic = item.GetComponent<Image>();
            button.onClick.AddListener(action);

            labelText = CreateText(
                rect, font, label,
                11, Navy, FontStyle.Bold,
                Vector2.zero, size,
                TextAnchor.MiddleCenter
            );
            RectTransform textRect =
                labelText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static void CreateAccent(
            RectTransform parent)
        {
            GameObject accent = new GameObject(
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
            TextAnchor alignment = TextAnchor.UpperLeft)
        {
            GameObject item = new GameObject(
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
