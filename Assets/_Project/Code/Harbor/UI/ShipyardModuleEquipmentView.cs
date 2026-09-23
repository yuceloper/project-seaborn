using System;
using System.Collections.Generic;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    public sealed class ShipyardModuleEquipmentView : MonoBehaviour
    {
        private static readonly Color Navy = new(0.025f, 0.075f, 0.105f, 1);
        private static readonly Color Gold = new(0.86f, 0.68f, 0.3f, 1);
        private static readonly Color Cream = new(0.91f, 0.88f, 0.76f, 1);
        private static readonly Color Muted = new(0.55f, 0.68f, 0.69f, 1);
        private Transform player;
        private PrototypeEquipmentInventory inventory;
        private ShipLoadout loadout;
        private RectTransform content, market;
        private GameObject equipmentPage, marketPage;
        private readonly List<Button> rows = new();
        private readonly List<string> shownIds = new();
        private readonly List<Button> offers = new();
        private readonly List<ShipModuleDefinition> definitions = new();
        private Button fit, remove, upgrade;
        private Text equipped, comparison, recipe, status;
        private Font font;
        private ShipModuleSlot slot;
        private string selectedId;
        private int quotedLevel;
        private float nextRefresh, statusUntil;
        private bool built;

        public void Bind(Transform value) { player = value; }
        public void SelectSlot(ShipModuleSlot value)
        {
            slot = value; selectedId = null;
            if (!built) return;
            equipmentPage.SetActive(true); marketPage.SetActive(false);
            BuildMarket(); Refresh();
        }
        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Button(transform, "YELKEN", 20, 0, 250, 34, () => SelectSlot(ShipModuleSlot.Sail));
            Button(transform, "GÖVDE KAPLAMASI", 280, 0, 250, 34, () => SelectSlot(ShipModuleSlot.Hull));
            Button(transform, "ENVANTER", 20, 44, 250, 32, () => { equipmentPage.SetActive(true); marketPage.SetActive(false); Refresh(); });
            Button(transform, "TEDARİK", 280, 44, 250, 32, () => { equipmentPage.SetActive(false); marketPage.SetActive(true); Refresh(); });
            var area = Rect(transform, "Equipment", 0, 88, 550, 450); equipmentPage = area.gameObject;
            equipped = Label(area, "", 14, 20, 0, 510, 44);
            var viewport = Rect(area, "Inventory", 20, 48, 510, 126);
            viewport.gameObject.AddComponent<Image>().color = Navy;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            content = Rect(viewport, "Items", 0, 0, 510, 126);
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            comparison = Label(area, "", 13, 20, 184, 510, 84);
            fit = Button(area, "SEÇİLİ PARÇAYI TAK", 20, 274, 250, 34, Equip);
            remove = Button(area, "TAKILI PARÇAYI SÖK", 280, 274, 250, 34, Unequip);
            recipe = Label(area, "", 13, 20, 320, 510, 82);
            upgrade = Button(area, "+ GELİŞTİR", 20, 416, 510, 40, Upgrade);
            market = Rect(transform, "Market", 0, 88, 550, 460); marketPage = market.gameObject;
            status = Label(transform, "", 12, 20, 556, 510, 34);
            built = true;
            SelectSlot(ShipModuleSlot.Sail);
        }
        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.15f; Refresh();
        }
        private void Resolve()
        {
            if (player == null) return;
            inventory = player.GetComponent<PrototypeEquipmentInventory>();
            loadout = player.GetComponent<ShipLoadout>();
        }
        private void Refresh()
        {
            Resolve();
            if (inventory == null || loadout == null) return;
            var installed = loadout.GetModule(slot);
            if (inventory.FindModule(selectedId)?.Definition.Slot != slot) selectedId = installed?.InstanceId;
            var selected = inventory.FindModule(selectedId);
            equipped.text = $"{(slot == ShipModuleSlot.Sail ? "YELKEN" : "GÖVDE KAPLAMASI")} • Takılı: {Name(installed)}\n" +
                (selected == null ? "Envanterden bir parça seç; yeni parçalar TEDARİK sekmesinde." : $"Seçili: {Name(selected)} • {selected.Definition.Role}");
            RefreshRows();
            var current = loadout.GetModuleStats(slot);
            var next = selected?.Stats ?? current;
            comparison.text = Compare(current, next, slot);
            fit.interactable = selected != null && !loadout.IsModuleInstalled(selected.InstanceId);
            remove.interactable = installed != null;
            quotedLevel = selected?.Enhancement ?? 0;
            bool max = selected != null && selected.Enhancement >= 10;
            var cost = ShipModuleCost.Next(selected);
            var depot = player.GetComponent<PrototypeRegionalLootInventory>();
            string stock = $"Depo: Yağ {depot?.TideOil ?? 0}, Demir {depot?.CorsairIron ?? 0}, Harita {depot?.LostChartFragments ?? 0}, Pul {depot?.StormjawScales ?? 0}";
            recipe.text = selected == null ? "Geliştirmek için bir parça seç." : max ? "+10 • Maksimum seviye" :
                $"+{selected.Enhancement} → +{selected.Enhancement + 1} • Başarı %100\n{Cost(cost)}\n{stock}\n" +
                (slot == ShipModuleSlot.Sail ? "+ başına temel hız %1, hızlanma %1,5 ve kumaş dayanıklılığı %3 artar."
                    : "+ başına kaplamanın temel gövde canı çarpanı %3 artar. Hareket bedeli değişmez.");
            upgrade.interactable = selected != null && !max && inventory.CanAffordModule(cost);
            upgrade.GetComponentInChildren<Text>().text = max ? "+10 • TAMAMLANDI" : "+ GELİŞTİR";
            for (int i = 0; i < offers.Count; i++) offers[i].interactable = inventory.CanAffordModule(definitions[i].PurchaseCost);
            if (Time.unscaledTime >= statusUntil)
                status.text = slot == ShipModuleSlot.Hull ? "Kaplama değiştirmek can doldurmaz. Hasarlı gemini onarabilirsin."
                    : "Yelkensiz gemi düşük hızla seyredebilir. Parça değişimi yelken hasarını onarmaz.";
        }
        private void RefreshRows()
        {
            var ids = new List<string>();
            foreach (var item in inventory.Modules) if (item.Definition.Slot == slot) ids.Add(item.InstanceId);
            bool changed = ids.Count != shownIds.Count;
            if (!changed) for (int i = 0; i < ids.Count; i++) if (ids[i] != shownIds[i]) { changed = true; break; }
            if (changed)
            {
                foreach (var row in rows) { row.gameObject.SetActive(false); Destroy(row.gameObject); }
                rows.Clear(); shownIds.Clear(); shownIds.AddRange(ids);
                for (int i = 0; i < ids.Count; i++)
                {
                    string id = ids[i]; rows.Add(Button(content, "", 4, i * 42, 502, 38, () => { selectedId = id; Refresh(); }));
                }
                content.sizeDelta = new Vector2(510, Mathf.Max(126, ids.Count * 42));
            }
            for (int i = 0; i < rows.Count; i++)
            {
                var item = inventory.FindModule(shownIds[i]);
                rows[i].GetComponentInChildren<Text>().text = Name(item) + (loadout.IsModuleInstalled(item.InstanceId) ? " • TAKILI" : " • DEPODA");
                rows[i].GetComponent<Image>().color = item.InstanceId == selectedId ? Gold : Muted;
            }
        }
        private void BuildMarket()
        {
            for (int i = market.childCount - 1; i >= 0; i--) { market.GetChild(i).gameObject.SetActive(false); Destroy(market.GetChild(i).gameObject); }
            offers.Clear(); definitions.Clear();
            int row = 0;
            foreach (var definition in ShipModuleCatalog.All)
            {
                if (definition.Slot != slot) continue;
                var value = definition;
                var stats = definition.StatsAt(0);
                Label(market, $"{definition.Name} +0 • {definition.Role}\n" + Summary(stats, slot), 12, 20, row * 108, 510, 50);
                offers.Add(Button(market, "SATIN AL • " + Cost(definition.PurchaseCost), 20, row * 108 + 54, 510, 40,
                    () => Result(inventory.TryPurchaseModule(value.Id), "Parça +0 olarak depoya eklendi.")));
                definitions.Add(definition); row++;
            }
        }
        private static string Name(ShipModuleItem item) => item == null ? "BOŞ" : $"{item.Definition.Name} +{item.Enhancement}";
        private static string Cost(ShipModuleCost c)
        {
            string value = $"{c.Silver} Silver";
            if (c.Oil > 0) value += $" • {c.Oil} Yağ";
            if (c.Iron > 0) value += $" • {c.Iron} Demir";
            if (c.Charts > 0) value += $" • {c.Charts} Harita";
            if (c.Scales > 0) value += $" • {c.Scales} Pul";
            return value;
        }
        private static string Summary(ShipModuleStats s, ShipModuleSlot slot) => slot == ShipModuleSlot.Sail
            ? $"Hız ×{s.Speed:0.00} • Hızlanma ×{s.Acceleration:0.00} • Kumaş ×{s.SailDurability:0.00}"
            : $"Gövde ×{s.HullHealth:0.00} • Hız ×{s.Speed:0.00} • Hızlanma ×{s.Acceleration:0.00} • Manevra ×{s.Turning:0.00}";
        private static string Compare(ShipModuleStats a, ShipModuleStats b, ShipModuleSlot slot) =>
            $"Hız ×{a.Speed:0.00} → ×{b.Speed:0.00}  •  Hızlanma ×{a.Acceleration:0.00} → ×{b.Acceleration:0.00}\n" +
            $"Manevra ×{a.Turning:0.00} → ×{b.Turning:0.00}\n" + (slot == ShipModuleSlot.Sail
                ? $"Yelken dayanıklılığı ×{a.SailDurability:0.00} → ×{b.SailDurability:0.00}"
                : $"Azami gövde canı ×{a.HullHealth:0.00} → ×{b.HullHealth:0.00}");
        private void Equip() => Show(loadout.TryEquipModule(slot, selectedId) ? "Parça takıldı; seviyesi korundu." : "Parça takılamadı.");
        private void Unequip() => Show(loadout.TryEquipModule(slot, null) ? "Parça seviyesiyle depoya döndü." : "Parça sökülemedi.");
        private void Upgrade() => Result(inventory.TryUpgradeModule(selectedId, quotedLevel), "Parça geliştirildi.");
        private void Result(EquipmentPurchaseResult result, string success) => Show(result switch
        {
            EquipmentPurchaseResult.Completed => success,
            EquipmentPurchaseResult.InsufficientSilver => "Yeterli Silver yok.",
            EquipmentPurchaseResult.InsufficientMaterials => "Depoda gerekli malzemeler eksik.",
            EquipmentPurchaseResult.MaximumEnhancement => "Parça zaten +10.",
            EquipmentPurchaseResult.StaleSelection => "Seviye değişti; maliyet yenilendi.",
            _ => "Bu işlem için tersaneye yanaş."
        });
        private void Show(string text) { status.text = text; statusUntil = Time.unscaledTime + 4; Refresh(); }
        private Button Button(Transform parent, string title, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(parent, title, x, y, w, h); rect.gameObject.AddComponent<Image>().color = Gold;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>(); button.onClick.AddListener(action);
            var label = Label(rect, title, 12, 0, 0, w, h); label.alignment = TextAnchor.MiddleCenter; label.color = Navy;
            return button;
        }
        private Text Label(Transform parent, string text, int size, float x, float y, float w, float h)
        {
            var label = Rect(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = size; label.color = Cream; label.raycastTarget = false; return label;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
    }
}
