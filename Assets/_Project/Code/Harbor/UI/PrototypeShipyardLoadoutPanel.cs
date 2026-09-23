using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Equipment;
using Seaborn.Progression;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipyardLoadoutPanel : MonoBehaviour
    {
        private static readonly Color Navy = new(0.025f, 0.075f, 0.105f, 0.98f);
        private static readonly Color Gold = new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream = new(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Muted = new(0.3f, 0.45f, 0.46f, 1f);
        private Transform player;
        private PrototypeEquipmentInventory inventory;
        private PrototypeSilverWallet wallet;
        private PrototypeRegionalLootInventory materials;
        private ShipLoadout loadout;
        private GameObject panel;
        private RectTransform slotContent;
        private readonly List<Button> slots = new();
        private Text summary, selected, comparison, status, sixDetail, heavyDetail, craftLabel;
        private Button sixBuy, heavyBuy, sixFit, heavyFit, remove, craft, ratBuy, ratFit, patchedFit;
        private Text ratLabel;
        private Font font;
        private int selectedSlot;
        private float nextRefresh, statusUntil;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;
            var view = FindFirstObjectByType<PrototypeShipyardLoadoutPanel>();
            if (view == null)
                view = new GameObject("Prototype Shipyard Loadout Panel").AddComponent<PrototypeShipyardLoadoutPanel>();
            view.player = player;
            view.Refresh();
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 81;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            var root = Rect(transform, "Modular Loadout", 0, 0, 1000, 650);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, -48);
            root.gameObject.AddComponent<Image>().color = Navy;
            panel = root.gameObject;
            Label(root, "TERSANE • TOP YUVALARI", 22, 24, 18, 600, 34);
            summary = Label(root, "", 14, 24, 60, 950, 46);
            Label(root, "İSKELE                 KIÇ → PRUVA                 SANCAK", 12, 24, 118, 420, 28);
            var viewport = Rect(root, "Hardpoint Scroll", 24, 150, 420, 374);
            viewport.gameObject.AddComponent<Image>().color = Navy;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            slotContent = Rect(viewport, "Hardpoints", 0, 0, 420, 374);
            scroll.viewport = viewport;
            scroll.content = slotContent;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            Label(root, "Bir yuva seç → depodaki topu tak.\nSökülen top depoya döner; kaybolmaz.\nDonanım aktif gemiyle taşınır; sığmayanlar depoya döner.", 12, 24, 540, 420, 76);

            selected = Label(root, "", 18, 470, 118, 506, 32);
            comparison = Label(root, "", 13, 470, 156, 506, 86);
            remove = Button(root, "SEÇİLİ TOPU SÖK", 470, 244, 506, 32, () => Fit(null));
            sixDetail = Label(root, "", 13, 470, 292, 290, 46);
            sixBuy = Button(root, "", 768, 292, 102, 42, () => Purchase("iron_6lb"));
            sixFit = Button(root, "TAK", 878, 292, 98, 42, () => Fit("iron_6lb"));
            heavyDetail = Label(root, "", 13, 470, 346, 290, 46);
            heavyBuy = Button(root, "", 768, 346, 102, 42, () => Purchase("iron_12lb"));
            heavyFit = Button(root, "TAK", 878, 346, 98, 42, () => Fit("iron_12lb"));
            craft = Button(root, "", 470, 402, 506, 44, Forge);
            craftLabel = craft.GetComponentInChildren<Text>();
            Label(root, "Korsan Demiri: korsan enkazından al, limana taşı.", 12, 470, 452, 506, 24);
            patchedFit = Button(root, "YAMALI YELKEN TAK", 470, 490, 245, 36, () => EquipSail("patched_canvas"));
            ratFit = Button(root, "RAT YELKENİ TAK", 727, 490, 249, 36, () => EquipSail("rat_sails"));
            ratBuy = Button(root, "", 470, 536, 506, 36, () => ShowPurchase(inventory.TryPurchaseSail("rat_sails")));
            ratLabel = ratBuy.GetComponentInChildren<Text>();
            status = Label(root, "", 13, 470, 588, 506, 46);
            panel.SetActive(false);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.15f;
            Refresh();
        }

        private void Refresh()
        {
            if (panel == null || player == null) { if (panel != null) panel.SetActive(false); return; }
            inventory = player.GetComponentInChildren<PrototypeEquipmentInventory>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
            materials = player.GetComponentInChildren<PrototypeRegionalLootInventory>();
            loadout = player.GetComponentInChildren<ShipLoadout>();
            bool visible = inventory != null && loadout != null && inventory.CanUseShipyard &&
                PrototypeHarborUiCoordinator.IsSelected(PrototypeHarborTab.Loadout);
            panel.SetActive(visible);
            if (!visible) return;
            if (slots.Count != loadout.CannonSlotCount) RebuildSlots();
            selectedSlot = Mathf.Clamp(selectedSlot, 0, Mathf.Max(0, slots.Count - 1));
            for (int i = 0; i < slots.Count; i++)
            {
                EquipmentCatalog.TryGetCannon(loadout.GetCannonAt(i), out var cannon);
                slots[i].GetComponentInChildren<Text>().text = $"{SlotName(i)}\n{(cannon != null ? cannon.caliberPounds + " lb" : "BOŞ")}";
                slots[i].GetComponent<Image>().color = i == selectedSlot ? Gold : Muted;
            }
            var battery = player.GetComponentInChildren<BroadsideController>();
            summary.text = $"{wallet?.Silver ?? 0} SILVER   •   {loadout.InstalledCannons}/{loadout.CannonSlotCount} top takılı\n" +
                $"Temel dolum: İskele {battery?.GetBaseBroadsideReload(BroadsideSide.Port) ?? 0:0.0} sn / Sancak {battery?.GetBaseBroadsideReload(BroadsideSide.Starboard) ?? 0:0.0} sn";
            selected.text = "SEÇİLİ YUVA • " + SlotName(selectedSlot);
            EquipmentCatalog.TryGetCannon(loadout.GetCannonAt(selectedSlot), out var current);
            EquipmentCatalog.TryGetCannon("iron_6lb", out var six);
            EquipmentCatalog.TryGetCannon("iron_12lb", out var heavy);
            if (six == null || heavy == null) return;
            comparison.text = (current == null ? "Bu yuva boş." : $"Takılı: {current.caliberPounds} lb • {current.damage:0} temel hasar • {current.reloadDuration:0.0} sn") +
                $"\n6 lb → 12 lb: +{heavy.damage - six.damage:0} hasar, +{heavy.reloadDuration - six.reloadDuration:0.0} sn dolum.\nBorda birlikte doldurulur; en yavaş takılı top süreyi belirler.";
            remove.interactable = current != null;
            CannonRow("iron_6lb", six, sixDetail, sixBuy, sixFit);
            CannonRow("iron_12lb", heavy, heavyDetail, heavyBuy, heavyFit);
            int iron = materials?.CorsairIron ?? 0;
            craftLabel.text = $"12 LB ÜRET • {PrototypeEquipmentInventory.HeavyForgeSilver} S + {iron}/{PrototypeEquipmentInventory.HeavyForgeIron} KORSAN DEMİRİ";
            craft.interactable = iron >= PrototypeEquipmentInventory.HeavyForgeIron &&
                (wallet?.Silver ?? 0) >= PrototypeEquipmentInventory.HeavyForgeSilver;
            patchedFit.interactable = inventory.GetOwnedSails("patched_canvas") > 0 && loadout.SailId != "patched_canvas";
            ratFit.interactable = inventory.GetOwnedSails("rat_sails") > 0 && loadout.SailId != "rat_sails";
            EquipmentCatalog.TryGetSail("rat_sails", out var sail);
            ratLabel.text = $"RAT YELKENİ SATIN AL • {sail?.silverPrice ?? 0} S • Sahip {inventory.GetOwnedSails("rat_sails")}";
            ratBuy.interactable = sail != null && (wallet?.Silver ?? 0) >= sail.silverPrice;
            if (Time.unscaledTime >= statusUntil)
                status.text = "Satın alınan veya üretilen top depoya gider. Seçili yuvaya ayrıca tak.";
        }

        private void CannonRow(string id, CannonDefinition definition, Text detail, Button buy, Button fit)
        {
            detail.text = $"{definition.caliberPounds} lb • {definition.damage:0} hasar • {definition.reloadDuration:0.0} sn\nTakılı {loadout.CountCannons(id)} • Depo {inventory.GetStoredCannons(id)}";
            buy.GetComponentInChildren<Text>().text = $"{definition.silverPrice} S\nSATIN AL";
            buy.interactable = (wallet?.Silver ?? 0) >= definition.silverPrice;
            fit.interactable = inventory.GetStoredCannons(id) > 0 && loadout.GetCannonAt(selectedSlot) != id;
        }

        private void RebuildSlots()
        {
            foreach (var button in slots) { button.gameObject.SetActive(false); Destroy(button.gameObject); }
            slots.Clear();
            for (int i = 0; i < loadout.CannonSlotCount; i++)
            {
                int slot = i;
                slots.Add(Button(slotContent, "", (i % 2) * 220, (i / 2) * 64, 200, 54,
                    () => { selectedSlot = slot; Refresh(); }));
            }
            slotContent.sizeDelta = new Vector2(420, Mathf.Max(374, ((slots.Count + 1) / 2) * 64));
        }

        private static string SlotName(int slot) => $"{(slot % 2 == 0 ? "İSKELE" : "SANCAK")} {slot / 2 + 1}";
        private void Fit(string id) => Show(loadout.TryEquipCannonAt(selectedSlot, id)
            ? (string.IsNullOrEmpty(id) ? "Top depoya kaldırıldı." : "Top seçili yuvaya takıldı.")
            : "İşlem yapılamadı; yuva ve depo stokunu kontrol et.");
        private void Purchase(string id) => ShowPurchase(inventory.TryPurchaseCannon(id));
        private void Forge() => ShowPurchase(inventory.TryForgeHeavyCannon());
        private void EquipSail(string id) => Show(inventory.TryEquipSail(id) ? "Yelken takıldı." : "Yelken takılamadı.");
        private void ShowPurchase(EquipmentPurchaseResult result) => Show(result switch
        {
            EquipmentPurchaseResult.Completed => "Donanım depoya eklendi; istediğin yuvaya takabilirsin.",
            EquipmentPurchaseResult.InsufficientSilver => "Yeterli Silver yok.",
            EquipmentPurchaseResult.InsufficientMaterials => "Depoda yeterli Korsan Demiri yok.",
            _ => "Bu işlem için tersaneye yanaş."
        });
        private void Show(string message) { status.text = message; statusUntil = Time.unscaledTime + 4; Refresh(); }

        private Button Button(Transform parent, string title, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(parent, title, x, y, w, h);
            rect.gameObject.AddComponent<Image>().color = Gold;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(action);
            var text = Label(rect, title, 12, 0, 0, w, h);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Navy;
            return button;
        }
        private Text Label(Transform parent, string value, int size, float x, float y, float w, float h)
        {
            var text = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = Cream;
            text.raycastTarget = false;
            return text;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }
    }
}
