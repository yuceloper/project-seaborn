using Seaborn.Equipment;
using Seaborn.Combat;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipyardLoadoutPanel : MonoBehaviour
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

        private sealed class MarketRow
        {
            public Text Detail;
            public Button Buy;
            public Text BuyLabel;
            public Button Equip;
            public Text EquipLabel;
        }

        private Transform player;
        private PrototypeEquipmentInventory inventory;
        private PrototypeSilverWallet wallet;
        private ShipLoadout loadout;
        private GameObject panel;
        private Text silverText;
        private Text currentText;
        private Text statusText;
        private MarketRow sixLb;
        private MarketRow twelveLb;
        private MarketRow patchedSail;
        private MarketRow ratSail;
        private float nextRefreshTime;
        private float statusExpiresAt;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeShipyardLoadoutPanel view =
                FindFirstObjectByType<
                    PrototypeShipyardLoadoutPanel>();
            if (view == null)
            {
                GameObject root =
                    new GameObject("Prototype Shipyard Loadout Panel");
                view = root.AddComponent<
                    PrototypeShipyardLoadoutPanel>();
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

            inventory = player.GetComponentInChildren<
                PrototypeEquipmentInventory>();
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            loadout = player.GetComponentInChildren<
                ShipLoadout>();
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
                "Loadout Panel",
                typeof(RectTransform),
                typeof(Image)
            );
            panel.transform.SetParent(transform, false);
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition =
                new Vector2(0f, -48f);
            rect.sizeDelta = new Vector2(450f, 462f);
            panel.GetComponent<Image>().color = Navy;

            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            CreateAccent(rect);

            CreateText(
                rect, font, "DONANIM VE LOADOUT",
                20, Gold, FontStyle.Bold,
                new Vector2(20f, -18f),
                new Vector2(270f, 30f)
            );
            silverText = CreateText(
                rect, font, "0 SILVER",
                15, Cream, FontStyle.Bold,
                new Vector2(310f, -20f),
                new Vector2(120f, 26f),
                TextAnchor.UpperRight
            );
            currentText = CreateText(
                rect, font, "",
                12, Cream, FontStyle.Normal,
                new Vector2(20f, -54f),
                new Vector2(410f, 42f)
            );
            statusText = CreateText(
                rect, font,
                "Topları adet adet satın al; sahip olduğun grubu tak.",
                11, Muted, FontStyle.Normal,
                new Vector2(20f, -94f),
                new Vector2(410f, 30f)
            );

            sixLb = CreateRow(
                rect, font, "6 LB IRON CANNON",
                -132f, true,
                () => PurchaseCannon("iron_6lb"),
                () => EquipCannons("iron_6lb")
            );
            twelveLb = CreateRow(
                rect, font, "12 LB IRON CANNON",
                -210f, true,
                () => PurchaseCannon("iron_12lb"),
                () => EquipCannons("iron_12lb")
            );
            patchedSail = CreateRow(
                rect, font, "PATCHED CANVAS",
                -288f, false, null,
                () => EquipSail("patched_canvas")
            );
            ratSail = CreateRow(
                rect, font, "RAT SAILS",
                -366f, true,
                () => PurchaseSail("rat_sails"),
                () => EquipSail("rat_sails")
            );
        }

        private MarketRow CreateRow(
            RectTransform parent,
            Font font,
            string title,
            float y,
            bool canBuy,
            UnityEngine.Events.UnityAction buyAction,
            UnityEngine.Events.UnityAction equipAction)
        {
            GameObject block = new GameObject(
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
            rect.anchoredPosition = new Vector2(20f, y);
            rect.sizeDelta = new Vector2(410f, 66f);
            block.GetComponent<Image>().color = NavyLight;

            CreateText(
                rect, font, title,
                12, Cream, FontStyle.Bold,
                new Vector2(12f, -8f),
                new Vector2(185f, 20f)
            );

            MarketRow row = new MarketRow();
            row.Detail = CreateText(
                rect, font, "",
                10, Muted, FontStyle.Normal,
                new Vector2(12f, -34f),
                new Vector2(185f, 20f)
            );

            if (canBuy)
            {
                row.Buy = CreateButton(
                    rect, font, "SATIN AL",
                    new Vector2(202f, -13f),
                    new Vector2(94f, 40f),
                    buyAction,
                    out row.BuyLabel
                );
            }

            row.Equip = CreateButton(
                rect, font, "TAK",
                new Vector2(304f, -13f),
                new Vector2(92f, 40f),
                equipAction,
                out row.EquipLabel
            );
            return row;
        }

        private void Refresh()
        {
            if (panel == null) return;
            ResolveBindings();

            bool visible =
                inventory != null &&
                inventory.CanUseShipyard &&
                PrototypeHarborUiCoordinator.IsSelected(
                    PrototypeHarborTab.Loadout);
            panel.SetActive(visible);
            if (!visible) return;

            int silver = wallet != null ? wallet.Silver : 0;
            silverText.text = $"{silver} SILVER";

            ShipProfileController shipProfile =
                player != null
                    ? player.GetComponent<ShipProfileController>()
                    : null;
            int cannonCapacity =
                shipProfile?.Definition != null
                    ? shipProfile.EffectiveCannonSlots
                    : 0;
            int storedCannons =
                loadout != null && inventory != null
                    ? Mathf.Max(
                        0,
                        inventory.GetOwnedCannons(
                            loadout.CannonId) -
                        loadout.InstalledCannons)
                    : 0;

            BroadsideController battery = player != null
                ? player.GetComponentInChildren<BroadsideController>() : null;
            int portCount = battery != null ? battery.GetBroadsideCannonCount(BroadsideSide.Port) : 0;
            int starboardCount = battery != null ? battery.GetBroadsideCannonCount(BroadsideSide.Starboard) : 0;
            currentText.text = loadout != null
                ? $"Kurulu: {loadout.Cannon?.displayName ?? loadout.CannonId}" +
                  $"  {loadout.InstalledCannons}/{cannonCapacity}" +
                  $"  •  Depoda {storedCannons}\n" +
                  $"Aktif borda: İskele {portCount} / Sancak {starboardCount}"
                : "Loadout hazırlanıyor";

            RefreshCannonRow(
                sixLb,
                "iron_6lb",
                silver
            );
            RefreshCannonRow(
                twelveLb,
                "iron_12lb",
                silver
            );
            RefreshSailRow(
                patchedSail,
                "patched_canvas",
                silver
            );
            RefreshSailRow(
                ratSail,
                "rat_sails",
                silver
            );

            if (Time.unscaledTime >= statusExpiresAt)
            {
                statusText.text =
                    "Topları adet adet satın al; sahip olduğun grubu tak.";
                statusText.color = Muted;
            }
        }

        private void RefreshCannonRow(
            MarketRow row,
            string id,
            int silver)
        {
            EquipmentCatalog.TryGetCannon(
                id,
                out CannonDefinition definition);
            int owned = inventory.GetOwnedCannons(id);
            bool equipped =
                loadout != null &&
                loadout.CannonId == id;

            ShipProfileController profile =
                player != null
                    ? player.GetComponent<ShipProfileController>()
                    : null;
            int capacity = profile?.Definition != null
                ? profile.Definition.cannonSlots
                : 0;
            int installed = equipped && loadout != null
                ? loadout.InstalledCannons
                : 0;
            int stored = Mathf.Max(0, owned - installed);

            row.Detail.text = equipped
                ? $"Sahip {owned} • Kurulu {installed}/{capacity} • Depo {stored}"
                : $"Sahip {owned}  •  {definition.damage:0} hasar";
            if (row.Buy != null)
            {
                row.BuyLabel.text =
                    $"{definition.silverPrice} S";
                row.Buy.interactable =
                    silver >= definition.silverPrice;
            }

            int targetInstalled =
                Mathf.Min(owned, capacity);
            bool canCompleteBattery =
                equipped &&
                installed < targetInstalled;

            row.EquipLabel.text = equipped
                ? canCompleteBattery
                    ? $"TAMAMLA {targetInstalled}/{capacity}"
                    : $"TAKILI {installed}/{capacity}"
                : "TAK";
            row.Equip.interactable =
                owned > 0 &&
                (!equipped || canCompleteBattery);
        }

        private void RefreshSailRow(
            MarketRow row,
            string id,
            int silver)
        {
            EquipmentCatalog.TryGetSail(
                id,
                out SailDefinition definition);
            int owned = inventory.GetOwnedSails(id);
            bool equipped =
                loadout != null &&
                loadout.SailId == id;

            row.Detail.text =
                $"Sahip: {owned}  •  " +
                $"Hız x{definition.speedMultiplier:0.00}";
            if (row.Buy != null)
            {
                row.BuyLabel.text =
                    $"{definition.silverPrice} S";
                row.Buy.interactable =
                    silver >= definition.silverPrice;
            }

            row.EquipLabel.text =
                equipped ? "TAKILI" : "TAK";
            row.Equip.interactable =
                owned > 0 && !equipped;
        }

        private void PurchaseCannon(string id)
        {
            EquipmentPurchaseResult result =
                inventory.TryPurchaseCannon(id);
            ShowPurchaseResult(result);
        }

        private void PurchaseSail(string id)
        {
            EquipmentPurchaseResult result =
                inventory.TryPurchaseSail(id);
            ShowPurchaseResult(result);
        }

        private void EquipCannons(string id)
        {
            bool success = inventory.TryEquipCannons(id);
            ShowStatus(
                success
                    ? "Top bataryası gemiye takıldı."
                    : "Bu top grubunu takamazsın.",
                success ? Success : Danger
            );
        }

        private void EquipSail(string id)
        {
            bool success = inventory.TryEquipSail(id);
            ShowStatus(
                success
                    ? "Yelken gemiye takıldı."
                    : "Bu yelkene sahip değilsin.",
                success ? Success : Danger
            );
        }

        private void ShowPurchaseResult(
            EquipmentPurchaseResult result)
        {
            switch (result)
            {
                case EquipmentPurchaseResult.Completed:
                    ShowStatus("Satın alma tamamlandı.", Success);
                    break;
                case EquipmentPurchaseResult.InsufficientSilver:
                    ShowStatus("Yeterli silver yok.", Danger);
                    break;
                default:
                    ShowStatus(
                        "Bu işlem tersanede yapılamıyor.",
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
            if (action != null)
            {
                button.onClick.AddListener(action);
            }

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

