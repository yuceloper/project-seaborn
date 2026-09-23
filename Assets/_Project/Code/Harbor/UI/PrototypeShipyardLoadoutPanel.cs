using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Equipment;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    // Overlay handles follow real muzzle transforms; the world/ship remains visible.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PrototypeShipyardLoadoutPanel : MonoBehaviour
    {
        private static readonly Color Navy = new(0.025f, 0.075f, 0.105f, 0.98f);
        private static readonly Color Gold = new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream = new(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Muted = new(0.55f, 0.68f, 0.69f, 1f);
        private Transform player;
        private PrototypeEquipmentInventory inventory;
        private PrototypeSilverWallet wallet;
        private PrototypeRegionalLootInventory materials;
        private ShipLoadout loadout;
        private PrototypeModularShipAssembler assembler;
        private RectTransform itemContent, pinRoot;
        private GameObject view, cannonPage, supplyPage;
        private Text title, detail, recipe, status, supplyText;
        private Button fit, remove, upgrade, sixBuy, heavyBuy, forge, patchedFit, ratFit, ratBuy;
        private readonly List<Button> pins = new();
        private readonly List<RectTransform> leaders = new();
        private readonly List<RectTransform> dots = new();
        private readonly List<Button> itemButtons = new();
        private readonly List<string> shownItems = new();
        private Font font;
        private int selectedSlot = -1, quotedLevel;
        private string previewId;
        private float nextRefresh, statusUntil;
        private bool wasVisible;
        private string shownHull;

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;
            var panel = FindFirstObjectByType<PrototypeShipyardLoadoutPanel>();
            if (panel == null)
                panel = new GameObject("Prototype Shipyard Loadout Panel").AddComponent<PrototypeShipyardLoadoutPanel>();
            panel.player = player;
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
            var root = Rect(transform, "Shipyard Inspection", 0, 0, 0, 0);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            view = root.gameObject;
            pinRoot = Rect(root, "World Hardpoints", 0, 0, 0, 0);
            pinRoot.anchorMin = Vector2.zero; pinRoot.anchorMax = Vector2.one;
            pinRoot.offsetMin = pinRoot.offsetMax = Vector2.zero;
            pinRoot.pivot = new Vector2(0.5f, 0.5f);

            var card = Rect(root, "Inventory Card", 0, 0, 550, 760);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(1, 0.5f);
            card.anchoredPosition = new Vector2(-24, -30);
            card.gameObject.AddComponent<Image>().color = Navy;
            title = Label(card, "GEMİ ÜZERİNDEN BİR YUVA SEÇ", 19, 20, 16, 510, 32);
            Button(card, "TOP ENVANTERİ", 20, 58, 250, 36, () => SetPage(false));
            Button(card, "TEDARİK / YELKEN", 280, 58, 250, 36, () => SetPage(true));
            var guns = Rect(card, "Cannons", 0, 108, 550, 590);
            cannonPage = guns.gameObject;
            detail = Label(guns, "", 14, 20, 0, 510, 96);
            var viewport = Rect(guns, "Owned Cannons", 20, 108, 510, 228);
            viewport.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.12f, 0.15f, 1);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            itemContent = Rect(viewport, "Items", 0, 0, 510, 228);
            scroll.viewport = viewport; scroll.content = itemContent; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            fit = Button(guns, "SEÇİLİ TOPU TAK", 20, 348, 250, 38, Equip);
            remove = Button(guns, "YUVADAKİ TOPU SÖK", 280, 348, 250, 38, Unequip);
            recipe = Label(guns, "", 14, 20, 406, 510, 110);
            upgrade = Button(guns, "+ GELİŞTİR", 20, 532, 510, 44, Upgrade);
            var supplies = Rect(card, "Supplies", 0, 108, 550, 590);
            supplyPage = supplies.gameObject;
            supplyText = Label(supplies, "", 14, 20, 0, 510, 130);
            sixBuy = Button(supplies, "", 20, 150, 250, 44, () => Purchase("iron_6lb"));
            heavyBuy = Button(supplies, "", 280, 150, 250, 44, () => Purchase("iron_12lb"));
            forge = Button(supplies, "", 20, 210, 510, 48, Forge);
            Label(supplies, "Korsan Demiri: korsan enkazından alıp limana taşı.\nAlınan ve üretilen toplar +0 olarak depoya eklenir.", 13, 20, 274, 510, 62);
            patchedFit = Button(supplies, "YAMALI YELKEN TAK", 20, 368, 250, 40, () => EquipSail("patched_canvas"));
            ratFit = Button(supplies, "RAT YELKENİ TAK", 280, 368, 250, 40, () => EquipSail("rat_sails"));
            ratBuy = Button(supplies, "", 20, 428, 510, 44, () => Result(inventory.TryPurchaseSail("rat_sails")));
            status = Label(card, "", 13, 20, 706, 510, 44);
            var hint = Rect(root, "Hint", 0, 0, 580, 56);
            hint.anchorMin = hint.anchorMax = hint.pivot = new Vector2(0.32f, 0f);
            hint.anchoredPosition = new Vector2(0, 205);
            hint.gameObject.AddComponent<Image>().color = Navy;
            Label(hint, "GEMİDEKİ YUVAYA TIKLA • E: Tersaneden ayrıl\nTopun + seviyesi sökülünce ve gemi değişince korunur.", 13, 16, 8, 548, 42);
            SetPage(false);
            view.SetActive(false);
        }

        private void Update()
        {
            if (player == null) { view.SetActive(false); return; }
            var docking = PrototypeHarborDockingDirector.Instance;
            bool visible = docking != null && docking.IsDockedAt(PrototypeHarborStation.Shipyard) &&
                Seaborn.World.PrototypeExpeditionRegionDirector.IsHarborScene &&
                PrototypeHarborUiCoordinator.IsSelected(PrototypeHarborTab.Loadout);
            view.SetActive(visible);
            if (!visible) { wasVisible = false; return; }
            inventory = player.GetComponentInChildren<PrototypeEquipmentInventory>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
            materials = player.GetComponentInChildren<PrototypeRegionalLootInventory>();
            loadout = player.GetComponentInChildren<ShipLoadout>();
            assembler = player.GetComponent<PrototypeModularShipAssembler>();
            string hull = player.GetComponent<ShipProfileController>()?.ShipId;
            if (!wasVisible || shownHull != hull)
            {
                selectedSlot = -1; previewId = null; shownHull = hull;
                SetPage(false); nextRefresh = 0;
            }
            wasVisible = true;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.15f;
            Refresh();
        }

        private void LateUpdate()
        {
            if (!view.activeSelf || assembler == null || loadout == null) return;
            var camera = UnityEngine.Camera.main;
            if (camera == null) return;
            int portCount = (loadout.CannonSlotCount + 1) / 2;
            int starboardCount = loadout.CannonSlotCount / 2;
            Vector3 centerScreen = camera.WorldToScreenPoint(player.position + Vector3.up * 0.4f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pinRoot, centerScreen, null, out var center);
            for (int i = 0; i < pins.Count; i++)
            {
                bool port = i % 2 == 0;
                var rail = port ? assembler.PortHardpoints : assembler.StarboardHardpoints;
                int index = i / 2;
                Transform muzzle = index < rail.Count ? rail[index] : null;
                Vector3 screen = muzzle != null ? camera.WorldToScreenPoint(muzzle.position) : Vector3.back;
                bool shown = muzzle != null && muzzle.gameObject.activeInHierarchy && screen.z > 0;
                pins[i].gameObject.SetActive(shown); leaders[i].gameObject.SetActive(shown); dots[i].gameObject.SetActive(shown);
                if (!shown) continue;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(pinRoot, screen, null, out var anchor);
                // Spread labels into two readable rails; leaders retain the exact world anchor.
                var labelPoint = new Vector2(anchor.x + (port ? -88 : 88),
                    center.y + (index - ((port ? portCount : starboardCount) - 1) * 0.5f) * 46f);
                pins[i].GetComponent<RectTransform>().anchoredPosition = labelPoint;
                dots[i].anchoredPosition = anchor;
                Vector2 delta = labelPoint - anchor;
                leaders[i].anchoredPosition = anchor;
                leaders[i].sizeDelta = new Vector2(delta.magnitude, 2);
                leaders[i].localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }
        }

        private void Refresh()
        {
            if (inventory == null || loadout == null) return;
            if (pins.Count != loadout.CannonSlotCount) RebuildPins();
            if (selectedSlot >= loadout.CannonSlotCount) { selectedSlot = -1; previewId = null; }
            for (int i = 0; i < pins.Count; i++)
            {
                var item = loadout.GetCannonItemAt(i);
                pins[i].GetComponentInChildren<Text>().text = $"{(i % 2 == 0 ? "İ" : "S")}{i / 2 + 1}  {(item == null ? "BOŞ" : "+" + item.Enhancement)}";
                pins[i].GetComponent<Image>().color = i == selectedSlot ? Gold : Muted;
                leaders[i].GetComponent<Image>().color = dots[i].GetComponent<Image>().color = i == selectedSlot ? Gold : Cream;
            }
            title.text = selectedSlot < 0 ? "GEMİ ÜZERİNDEN BİR YUVA SEÇ" : SlotName(selectedSlot);
            var installed = loadout.GetCannonItemAt(selectedSlot);
            if (inventory.FindCannon(previewId) == null ||
                (loadout.IsCannonInstalled(previewId) && previewId != installed?.InstanceId)) previewId = installed?.InstanceId;
            var preview = inventory.FindCannon(previewId);
            detail.text = selectedSlot < 0 ? "İskele veya sancaktaki işaretlerden birini seç.\nO yuvaya uygun, depodaki toplar burada listelenir."
                : $"Yuvada: {ItemName(installed)}\nSeçili: {ItemName(preview)}\n" +
                  (preview == null ? "Depodan bir top seç veya TEDARİK sekmesini aç."
                    : $"Temel hasar: {Damage(installed):0.0} → {Damage(preview):0.0}  •  Temel dolum: {Reload(preview):0.0} sn");
            RefreshItems(installed);
            fit.interactable = selectedSlot >= 0 && preview != null && !loadout.IsCannonInstalled(preview.InstanceId);
            remove.interactable = installed != null;
            quotedLevel = preview?.Enhancement ?? 0;
            var cost = CannonUpgradeCost.Next(preview);
            bool max = preview != null && preview.Enhancement >= CannonItem.MaximumEnhancement;
            recipe.text = preview == null ? "Geliştirmek için listeden bir top seç." : max
                ? $"{ItemName(preview)} • MAKSİMUM SEVİYE\nHasar: {Damage(preview):0.0}"
                : $"+{preview.Enhancement} → +{preview.Enhancement + 1}  •  Hasar {Damage(preview):0.0} → {NextDamage(preview):0.0}\n" +
                  $"Silver {wallet?.Silver ?? 0}/{cost.Silver}  •  Korsan Demiri {materials?.CorsairIron ?? 0}/{cost.Iron}\n" +
                  $"Harita Parçası {materials?.LostChartFragments ?? 0}/{cost.Charts}  •  Stormjaw Pulu {materials?.StormjawScales ?? 0}/{cost.Scales}\n" +
                  "Başarı %100 • Her + seviye temel hasara %8 ekler.";
            upgrade.interactable = preview != null && !max && Affordable(cost);
            upgrade.GetComponentInChildren<Text>().text = max ? "+10 • TAMAMLANDI" : "+ GELİŞTİR";
            RefreshSupply();
            if (Time.unscaledTime >= statusUntil)
                status.text = "Sökülen top depoya döner. Geliştirme yalnız seçtiğin topu etkiler.";
        }

        private void RefreshItems(CannonItem installed)
        {
            var wanted = new List<string>();
            if (selectedSlot >= 0)
                foreach (var item in inventory.Cannons)
                    if (!loadout.IsCannonInstalled(item.InstanceId) || item.InstanceId == installed?.InstanceId)
                        wanted.Add(item.InstanceId);
            bool changed = wanted.Count != shownItems.Count;
            if (!changed) for (int i = 0; i < wanted.Count; i++) if (wanted[i] != shownItems[i]) { changed = true; break; }
            if (changed)
            {
                foreach (var button in itemButtons) { button.gameObject.SetActive(false); Destroy(button.gameObject); }
                itemButtons.Clear(); shownItems.Clear(); shownItems.AddRange(wanted);
                for (int i = 0; i < wanted.Count; i++)
                {
                    string id = wanted[i];
                    itemButtons.Add(Button(itemContent, "", 4, i * 56, 502, 50, () => { previewId = id; Refresh(); }));
                }
                itemContent.sizeDelta = new Vector2(510, Mathf.Max(228, wanted.Count * 56));
            }
            for (int i = 0; i < shownItems.Count; i++)
            {
                var item = inventory.FindCannon(shownItems[i]);
                itemButtons[i].GetComponentInChildren<Text>().text = $"{ItemName(item)}  •  {Damage(item):0.0} hasar  •  " +
                    (item?.InstanceId == installed?.InstanceId ? "YUVADA" : "DEPODA");
                itemButtons[i].GetComponent<Image>().color = shownItems[i] == previewId ? Gold : Muted;
            }
        }

        private void RefreshSupply()
        {
            EquipmentCatalog.TryGetCannon("iron_6lb", out var six);
            EquipmentCatalog.TryGetCannon("iron_12lb", out var heavy);
            EquipmentCatalog.TryGetSail("rat_sails", out var sail);
            int silver = wallet?.Silver ?? 0;
            supplyText.text = $"{silver} SILVER\n6 lb: {six?.damage ?? 0:0} hasar / {six?.reloadDuration ?? 0:0.0} sn\n" +
                $"12 lb: {heavy?.damage ?? 0:0} hasar / {heavy?.reloadDuration ?? 0:0.0} sn\n" +
                "Bir bordanın dolum süresini en yavaş takılı top belirler.";
            sixBuy.GetComponentInChildren<Text>().text = $"6 LB SATIN AL • {six?.silverPrice ?? 0} S";
            heavyBuy.GetComponentInChildren<Text>().text = $"12 LB SATIN AL • {heavy?.silverPrice ?? 0} S";
            sixBuy.interactable = six != null && silver >= six.silverPrice;
            heavyBuy.interactable = heavy != null && silver >= heavy.silverPrice;
            forge.GetComponentInChildren<Text>().text = $"12 LB ÜRET • {PrototypeEquipmentInventory.HeavyForgeSilver} S + " +
                $"{materials?.CorsairIron ?? 0}/{PrototypeEquipmentInventory.HeavyForgeIron} KORSAN DEMİRİ";
            forge.interactable = silver >= PrototypeEquipmentInventory.HeavyForgeSilver &&
                (materials?.CorsairIron ?? 0) >= PrototypeEquipmentInventory.HeavyForgeIron;
            patchedFit.interactable = inventory.GetOwnedSails("patched_canvas") > 0 && loadout.SailId != "patched_canvas";
            ratFit.interactable = inventory.GetOwnedSails("rat_sails") > 0 && loadout.SailId != "rat_sails";
            ratBuy.GetComponentInChildren<Text>().text = $"RAT YELKENİ SATIN AL • {sail?.silverPrice ?? 0} S";
            ratBuy.interactable = sail != null && silver >= sail.silverPrice;
        }

        private void RebuildPins()
        {
            for (int i = pinRoot.childCount - 1; i >= 0; i--) { pinRoot.GetChild(i).gameObject.SetActive(false); Destroy(pinRoot.GetChild(i).gameObject); }
            pins.Clear(); leaders.Clear(); dots.Clear();
            for (int i = 0; i < loadout.CannonSlotCount; i++)
            {
                int slot = i;
                var line = Rect(pinRoot, "Leader", 0, 0, 0, 2);
                line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.5f); line.pivot = new Vector2(0, 0.5f);
                line.gameObject.AddComponent<Image>().raycastTarget = false;
                leaders.Add(line);
                var dot = Rect(pinRoot, "Mount", 0, 0, 8, 8);
                dot.anchorMin = dot.anchorMax = dot.pivot = new Vector2(0.5f, 0.5f);
                dot.gameObject.AddComponent<Image>().raycastTarget = false; dots.Add(dot);
                var pin = Button(pinRoot, "", 0, 0, 84, 34, () => SelectSlot(slot));
                var rect = pin.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                pins.Add(pin);
            }
        }

        private void SelectSlot(int slot)
        {
            selectedSlot = slot; previewId = loadout.GetCannonItemId(slot);
            SetPage(false); Refresh();
        }
        private void SetPage(bool supply) { cannonPage.SetActive(!supply); supplyPage.SetActive(supply); }
        private bool Affordable(CannonUpgradeCost cost) => materials != null && wallet != null && wallet.Silver >= cost.Silver &&
            materials.CorsairIron >= cost.Iron && materials.LostChartFragments >= cost.Charts && materials.StormjawScales >= cost.Scales;
        private static string SlotName(int slot) => $"{(slot % 2 == 0 ? "İSKELE" : "SANCAK")} • TOP YUVASI {slot / 2 + 1}";
        private static string ItemName(CannonItem item) => item == null ? "BOŞ" :
            $"{(EquipmentCatalog.TryGetCannon(item.DefinitionId, out var definition) ? definition.caliberPounds + " lb top" : item.DefinitionId)} +{item.Enhancement}";
        private static float Damage(CannonItem item) => item != null && EquipmentCatalog.TryGetCannon(item.DefinitionId, out var definition)
            ? definition.damage * item.DamageMultiplier : 0;
        private static float NextDamage(CannonItem item) => item != null && EquipmentCatalog.TryGetCannon(item.DefinitionId, out var definition)
            ? definition.damage * CannonItem.DamageAt(item.Enhancement + 1) : 0;
        private static float Reload(CannonItem item) => item != null && EquipmentCatalog.TryGetCannon(item.DefinitionId, out var definition)
            ? definition.reloadDuration : 0;
        private void Equip() => Show(loadout.TryEquipCannonItemAt(selectedSlot, previewId) ? "Top seçili yuvaya takıldı." : "Top takılamadı; seçimini yenile.");
        private void Unequip() => Show(loadout.TryEquipCannonItemAt(selectedSlot, null) ? "Top seviyesiyle birlikte depoya döndü." : "Top sökülemedi.");
        private void Upgrade() => Result(inventory.TryUpgradeCannon(previewId, quotedLevel), "Top geliştirildi.");
        private void Purchase(string id) => Result(inventory.TryPurchaseCannon(id));
        private void Forge() => Result(inventory.TryForgeHeavyCannon());
        private void EquipSail(string id) => Show(inventory.TryEquipSail(id) ? "Yelken takıldı." : "Yelken takılamadı.");
        private void Result(EquipmentPurchaseResult result, string success = "Donanım depoya eklendi.") => Show(result switch
        {
            EquipmentPurchaseResult.Completed => success,
            EquipmentPurchaseResult.InsufficientSilver => "Yeterli Silver yok.",
            EquipmentPurchaseResult.InsufficientMaterials => "Depoda gerekli malzemeler eksik.",
            EquipmentPurchaseResult.MaximumEnhancement => "Bu top +10 seviyesinde.",
            EquipmentPurchaseResult.StaleSelection => "Topun seviyesi değişti; maliyeti yeniledim.",
            _ => "İşlem yapılamadı; tersane ve donanım seçimini kontrol et."
        });
        private void Show(string message) { status.text = message; statusUntil = Time.unscaledTime + 4; Refresh(); }
        private Button Button(Transform parent, string value, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(parent, value, x, y, w, h);
            rect.gameObject.AddComponent<Image>().color = Gold;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(action);
            var text = Label(rect, value, 12, 0, 0, w, h); text.alignment = TextAnchor.MiddleCenter; text.color = Navy;
            return button;
        }
        private Text Label(Transform parent, string value, int size, float x, float y, float w, float h)
        {
            var text = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = Cream; text.raycastTarget = false;
            return text;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
            return rect;
        }
    }
}
