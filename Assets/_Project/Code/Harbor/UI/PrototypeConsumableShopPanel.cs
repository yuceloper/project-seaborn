using System.Collections.Generic;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeConsumableShopPanel :
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

        private readonly Dictionary<
            ConsumableOffer,
            Button> buttons = new();

        private Transform player;
        private PrototypeConsumableShop shop;
        private PrototypeSilverWallet wallet;
        private PrototypeShipConsumables consumables;
        private GameObject panel;
        private Text balanceText;
        private Text stockText;
        private Text statusText;
        private float nextRefreshTime;
        private float statusExpiresAt;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeConsumableShopPanel existing =
                FindFirstObjectByType<
                    PrototypeConsumableShopPanel>();
            if (existing == null)
            {
                GameObject root =
                    new("Prototype Consumable Shop Panel");
                existing = root.AddComponent<
                    PrototypeConsumableShopPanel>();
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
                    PrototypeHarborStation.Trade);

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
            shop ??= player.GetComponent<
                PrototypeConsumableShop>();
            wallet ??= player.GetComponentInChildren<
                PrototypeSilverWallet>();
            consumables ??= player.GetComponentInChildren<
                PrototypeShipConsumables>();
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
                "Consumable Trade",
                Navy
            );
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition =
                new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(500f, 730f);

            GameObject accent =
                CreateBlock(rect, "Accent", Gold);
            RectTransform accentRect =
                accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.sizeDelta = new Vector2(0f, 4f);

            CreateText(
                rect,
                "TORTUGA TEDARİKÇİSİ",
                23,
                Gold,
                new Vector2(22f, -20f),
                new Vector2(456f, 32f),
                FontStyle.Bold
            );
            balanceText = CreateText(
                rect,
                "0 SILVER",
                14,
                Cream,
                new Vector2(22f, -58f),
                new Vector2(456f, 22f),
                FontStyle.Bold,
                TextAnchor.UpperRight
            );
            stockText = CreateText(
                rect,
                "TONIC x0  •  LIGHT x0",
                13,
                Cream,
                new Vector2(22f, -88f),
                new Vector2(456f, 22f),
                FontStyle.Bold
            );
            statusText = CreateText(
                rect,
                "Sefer için sarf malzemesi seç.",
                12,
                Muted,
                new Vector2(22f, -116f),
                new Vector2(456f, 24f)
            );

            AddOffer(
                rect,
                ConsumableOffer.TortugaTonic,
                "TORTUGA TONIC",
                "+250 gövde  •  15 sn bekleme",
                156f
            );
            AddOffer(
                rect,
                ConsumableOffer.TortugaTonicCrate,
                "TONIC SANDIĞI x3",
                "Üçlü ikmal  •  30 silver tasarruf",
                242f
            );
            AddOffer(
                rect,
                ConsumableOffer.LightOfTortuga,
                "LIGHT OF TORTUGA",
                "7 sn görünmezlik  •  ateşle bozulur",
                328f
            );
            AddOffer(
                rect,
                ConsumableOffer.CorsairRum,
                "CORSAIR RUM",
                "90 sn  •  +%10 top hasarı ve doldurma",
                414f
            );
            AddOffer(
                rect,
                ConsumableOffer.GaleElixir,
                "GALE ELIXIR",
                "90 sn  •  +%12 hız ve manevra",
                500f
            );
            AddOffer(
                rect,
                ConsumableOffer.IronbarkBrew,
                "IRONBARK BREW",
                "90 sn  •  -%15 alınan hasar",
                586f
            );

            CreateText(
                rect,
                "Satın alımlar kalıcı filona eklenir.  •  E ile ayrıl",
                12,
                Muted,
                new Vector2(22f, -692f),
                new Vector2(456f, 22f),
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );

            panel.SetActive(false);
        }

        private void AddOffer(
            RectTransform root,
            ConsumableOffer offer,
            string title,
            string detail,
            float y)
        {
            GameObject row =
                CreateBlock(root, title, NavyLight);
            RectTransform rowRect =
                row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(0f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.anchoredPosition =
                new Vector2(22f, -y);
            rowRect.sizeDelta = new Vector2(456f, 72f);

            CreateText(
                rowRect,
                title,
                13,
                Cream,
                new Vector2(12f, -9f),
                new Vector2(280f, 20f),
                FontStyle.Bold
            );
            CreateText(
                rowRect,
                detail,
                11,
                Muted,
                new Vector2(12f, -37f),
                new Vector2(280f, 20f)
            );

            GameObject buttonObject =
                CreateBlock(rowRect, "Buy", Gold);
            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition =
                new Vector2(310f, -13f);
            buttonRect.sizeDelta =
                new Vector2(132f, 46f);

            Button button =
                buttonObject.AddComponent<Button>();
            button.targetGraphic =
                buttonObject.GetComponent<Image>();
            ConsumableOffer captured = offer;
            button.onClick.AddListener(
                () => Purchase(captured)
            );
            Text label = CreateText(
                buttonRect,
                "SATIN AL",
                12,
                Navy,
                Vector2.zero,
                new Vector2(132f, 46f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            RectTransform labelRect =
                label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            buttons.Add(offer, button);
        }

        private void Refresh()
        {
            ResolveBindings();
            if (panel == null) return;

            int silver = wallet != null ? wallet.Silver : 0;
            balanceText.text = $"{silver} SILVER";
            stockText.text = consumables != null
                ? $"TONIC x{consumables.TortugaTonics}  •  " +
                  $"LIGHT x{consumables.LightsOfTortuga}"
                : "TONIC x0  •  LIGHT x0";

            foreach (KeyValuePair<
                     ConsumableOffer,
                     Button> item in buttons)
            {
                int price =
                    shop != null
                        ? shop.Price(item.Key)
                        : 0;
                item.Value.interactable =
                    shop != null && silver >= price;
                Text label =
                    item.Value.GetComponentInChildren<Text>();
                label.text = $"{price}  SATIN AL";
            }

            if (statusExpiresAt > 0f &&
                Time.unscaledTime >= statusExpiresAt)
            {
                statusExpiresAt = 0f;
                statusText.text =
                    "Sefer için sarf malzemesi seç.";
                statusText.color = Muted;
            }
        }

        private void Purchase(ConsumableOffer offer)
        {
            ConsumablePurchaseResult result =
                shop != null
                    ? shop.TryPurchase(offer)
                    : ConsumablePurchaseResult
                        .MissingInventory;

            statusText.text = result switch
            {
                ConsumablePurchaseResult.Completed =>
                    $"{PrototypeConsumableShop.OfferName(offer)} filoya eklendi.",
                ConsumablePurchaseResult
                    .InsufficientSilver =>
                    "Yeterli Silver yok.",
                ConsumablePurchaseResult.NotAtTradeDock =>
                    "Satın alma için Ticaret iskelesine yanaş.",
                _ => "Sarf malzemesi envanteri hazırlanıyor."
            };
            statusText.color =
                result == ConsumablePurchaseResult.Completed
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
