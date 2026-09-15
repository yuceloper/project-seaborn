using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeExtendedDeckPanel :
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

        private Transform player;
        private PrototypeDeckExtensionInventory inventory;
        private ShipProfileController profile;
        private GameObject panel;
        private Text shipText;
        private Text inventoryText;
        private Text statusText;
        private Button installButton;
        private float nextRefreshTime;
        private float statusExpiresAt;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeExtendedDeckPanel existing =
                FindFirstObjectByType<
                    PrototypeExtendedDeckPanel>();
            if (existing == null)
            {
                GameObject root =
                    new("Prototype Extended Deck Panel");
                existing = root.AddComponent<
                    PrototypeExtendedDeckPanel>();
            }
            existing.Bind(player);
        }

        private void Awake()
        {
            BuildInterface();
        }

        private void Bind(Transform value)
        {
            player = value;
            ResolveBindings();
            Refresh();
        }

        private void Update()
        {
            PrototypeHarborDockingDirector docking =
                PrototypeHarborDockingDirector.Instance;
            bool visible =
                docking != null &&
                docking.IsDockedAt(
                    PrototypeHarborStation.Shipyard) &&
                PrototypeHarborUiCoordinator.IsSelected(
                    PrototypeHarborTab.ExtendedDeck);

            if (panel.activeSelf != visible)
                panel.SetActive(visible);
            if (!visible ||
                Time.unscaledTime < nextRefreshTime)
                return;

            nextRefreshTime = Time.unscaledTime + 0.15f;
            Refresh();
        }

        private void ResolveBindings()
        {
            if (player == null) return;
            inventory ??= player.GetComponentInChildren<
                PrototypeDeckExtensionInventory>();
            profile ??= player.GetComponentInChildren<
                ShipProfileController>();
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 82;

            CanvasScaler scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            panel = CreateBlock(
                transform,
                "Extended Deck",
                Navy
            );
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition =
                new Vector2(24f, 250f);
            rect.sizeDelta = new Vector2(430f, 240f);

            CreateText(
                rect,
                "EXTENDED DECK",
                20,
                Gold,
                new Vector2(20f, -18f),
                new Vector2(390f, 28f),
                FontStyle.Bold
            );
            inventoryText = CreateText(
                rect,
                "ENVANTER  x0",
                13,
                Cream,
                new Vector2(20f, -54f),
                new Vector2(390f, 22f),
                FontStyle.Bold
            );
            shipText = CreateText(
                rect,
                "GEMİ  0/0",
                13,
                Cream,
                new Vector2(20f, -84f),
                new Vector2(390f, 42f),
                FontStyle.Bold
            );
            CreateText(
                rect,
                "Her kurulum: +1 top yuvası  •  +250 gövde",
                12,
                Muted,
                new Vector2(20f, -126f),
                new Vector2(390f, 22f)
            );
            statusText = CreateText(
                rect,
                "Nadir güverteyi aktif gemiye kalıcı olarak kur.",
                11,
                Muted,
                new Vector2(20f, -152f),
                new Vector2(390f, 22f)
            );

            GameObject buttonObject =
                CreateBlock(rect, "Install", Gold);
            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition =
                new Vector2(20f, -184f);
            buttonRect.sizeDelta =
                new Vector2(390f, 38f);
            installButton =
                buttonObject.AddComponent<Button>();
            installButton.targetGraphic =
                buttonObject.GetComponent<Image>();
            installButton.onClick.AddListener(Install);

            Text label = CreateText(
                buttonRect,
                "AKTİF GEMİYE KUR",
                12,
                Navy,
                Vector2.zero,
                new Vector2(390f, 38f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            RectTransform labelRect =
                label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            panel.SetActive(false);
        }

        private void Refresh()
        {
            ResolveBindings();
            if (panel == null) return;

            int available =
                inventory != null
                    ? inventory.AvailableExtensions
                    : 0;
            int installed =
                inventory != null
                    ? inventory.ActiveInstalled
                    : 0;
            int limit =
                inventory != null
                    ? inventory.ActiveLimit
                    : 0;
            string ship =
                profile?.Definition != null
                    ? profile.Definition.displayName
                    : "Gemi";

            inventoryText.text =
                $"ENVANTER  x{available}";
            shipText.text =
                $"{ship.ToUpperInvariant()}  •  " +
                $"KURULU {installed}/{limit}";
            installButton.interactable =
                inventory != null &&
                available > 0 &&
                installed < limit;

            if (statusExpiresAt > 0f &&
                Time.unscaledTime >= statusExpiresAt)
            {
                statusExpiresAt = 0f;
                statusText.text =
                    "Nadir güverteyi aktif gemiye kalıcı olarak kur.";
                statusText.color = Muted;
            }
        }

        private void Install()
        {
            DeckExtensionInstallResult result =
                inventory != null
                    ? inventory.TryInstallOnActiveShip()
                    : DeckExtensionInstallResult
                        .ProfileUnavailable;

            statusText.text = result switch
            {
                DeckExtensionInstallResult.Completed =>
                    "Extended Deck kuruldu.",
                DeckExtensionInstallResult.NoExtension =>
                    "Envanterde Extended Deck yok.",
                DeckExtensionInstallResult.MaximumInstalled =>
                    "Bu gemi genişletme sınırına ulaştı.",
                DeckExtensionInstallResult.NotAtShipyard =>
                    "Kurulum için Tersane'ye yanaş.",
                _ => "Gemi profili hazırlanıyor."
            };
            statusText.color =
                result == DeckExtensionInstallResult.Completed
                    ? Success
                    : Gold;
            statusExpiresAt = Time.unscaledTime + 3f;
            Refresh();
        }

        private static GameObject CreateBlock(
            Transform parent,
            string objectName,
            Color color)
        {
            GameObject result = new(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );
            result.transform.SetParent(parent, false);
            result.GetComponent<Image>().color = color;
            return result;
        }

        private static Text CreateText(
            Transform parent,
            string value,
            int fontSize,
            Color color,
            Vector2 position,
            Vector2 size,
            FontStyle style = FontStyle.Normal,
            TextAnchor alignment = TextAnchor.UpperLeft)
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
            rect.sizeDelta = size;

            Text text = item.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow =
                HorizontalWrapMode.Wrap;
            text.verticalOverflow =
                VerticalWrapMode.Overflow;
            return text;
        }
    }
}
