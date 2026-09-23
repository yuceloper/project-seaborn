using System;
using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using Seaborn.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Seaborn.Harbor.UI
{
    // Views reference the authoritative cargo, ownership and loadout stores.
    // No copied item balances: existing saves remain the depot's source of truth.
    [DefaultExecutionOrder(-200)]
    public sealed class PrototypeHarborInventoryPanel : MonoBehaviour
    {
        public static PrototypeHarborInventoryPanel Instance { get; private set; }
        public static bool BlocksGameplayInput => Instance != null && Instance.isActiveAndEnabled &&
            (Instance.IsVisible || Instance.waitForMouseRelease ||
             Instance.lastBlockedFrame == Time.frameCount);
        private bool IsVisible => panel != null && panel.activeSelf;
        private bool portableOpen;
        private bool waitForMouseRelease;
        private int lastBlockedFrame = -1;
        private ShipHealth health;
        private Text footer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void EnsureCreated()
        {
            if (Instance != null) return;
            new GameObject("Ship Inventory").AddComponent<PrototypeHarborInventoryPanel>();
        }
        private static readonly Color Navy = new(0.025f, 0.075f, 0.105f, 0.98f);
        private static readonly Color Gold = new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream = new(0.91f, 0.88f, 0.76f, 1f);
        private GameObject panel;
        private RectTransform content;
        private ScrollRect scroll;
        private Text summary;
        private Text help;
        private Button depositAll;
        private readonly List<Action> refreshRows = new();
        private readonly List<Button> tabs = new();
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;
        private PrototypeRegionalLootInventory depot;
        private PrototypeEquipmentInventory equipment;
        private ShipLoadout loadout;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeShipConsumables consumables;
        private int view;
        private int rowCount;
        private float nextRefresh;
        private Font font;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 81;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            var root = Rect(transform, "Inventory", 0, 0, 940, 680);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, -40);
            root.gameObject.AddComponent<Image>().color = Navy;
            panel = root.gameObject;
            Label(root, "GEMİ ENVANTERİ VE LİMAN DEPOSU", 22, 20, 16, 770, 32);
            Button(root, "KAPAT", 812, 14, 108, 36, Close);
            string[] titles = { "GEMİ AMBARI", "LİMAN DEPOSU", "TAKILI DONANIM" };
            for (int i = 0; i < titles.Length; i++)
            {
                int tab = i;
                tabs.Add(Button(root, titles[i], 20 + i * 300, 60, 288, 40,
                    () => Select(tab)));
            }
            summary = Label(root, "", 15, 20, 114, 900, 30);
            help = Label(root, "", 13, 20, 148, 900, 42);
            var area = Rect(root, "Scroll", 20, 196, 900, 410);
            area.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.11f, 0.14f, 1);
            area.gameObject.AddComponent<RectMask2D>();
            scroll = area.gameObject.AddComponent<ScrollRect>();
            content = Rect(area, "Items", 0, 0, 900, 410);
            scroll.viewport = area;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            depositAll = Button(root, "TÜM GANİMETİ TESLİM ET", 610, 624, 310, 38,
                () => { if (cargo != null) cargo.TryDepositAll(wallet); Refresh(); });
            footer = Label(root, "Limana dönüşte ganimet otomatik depoya aktarılır.", 12,
                20, 632, 580, 26);
            Select(0);
            panel.SetActive(false);
        }

        private void Update()
        {
            // Run before weapon input; only component discovery is throttled.
            if (!IsVisible && (Mouse.current == null ||
                (!Mouse.current.leftButton.isPressed && !Mouse.current.rightButton.isPressed)))
                waitForMouseRelease = false;

            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + 0.2f;
                Resolve();
            }
            if (health == null || health.IsSunk)
            {
                if (IsVisible || portableOpen) Close();
                return;
            }
            bool docked = Seaborn.World.PrototypeExpeditionRegionDirector.IsHarborScene &&
                PrototypeHarborUiCoordinator.IsOpen;
            if (docked) portableOpen = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.iKey.wasPressedThisFrame)
                {
                    if (IsVisible) Close();
                    else if (docked) PrototypeHarborUiCoordinator.Instance.OpenInventory();
                    else { portableOpen = true; Select(0); }
                }
                else if (Keyboard.current.escapeKey.wasPressedThisFrame && IsVisible) Close();
            }
            bool visible = portableOpen || (docked &&
                PrototypeHarborUiCoordinator.IsSelected(PrototypeHarborTab.Inventory));
            if (IsVisible && !visible) GuardClosingInput();
            panel.SetActive(visible);
            if (!visible) return;
            // Sea scenes may not have a station UI/event system yet.
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var events = new GameObject("Inventory Event System",
                    typeof(UnityEngine.EventSystems.EventSystem));
                events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>()
                    .AssignDefaultActions();
            }
            waitForMouseRelease = true;
            tabs[1].interactable = docked;
            if (!docked && view == 1) Select(0);
            Refresh();
        }

        private void GuardClosingInput()
        {
            lastBlockedFrame = Time.frameCount;
            waitForMouseRelease = true;
        }

        private void Close()
        {
            portableOpen = false;
            if (PrototypeHarborUiCoordinator.IsOpen)
                PrototypeHarborUiCoordinator.Instance.CloseInventory();
            panel.SetActive(false);
            GuardClosingInput();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            portableOpen = false;
            panel.SetActive(false);
            GuardClosingInput();
            health = null;
            nextRefresh = 0;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void Resolve()
        {
            var player = FindFirstObjectByType<ManualBroadsideAimController>();
            if (player == null) { health = null; return; }
            health = player.GetComponentInChildren<ShipHealth>();
            cargo = player.GetComponentInChildren<PrototypeHuntCargo>();
            wallet = player.GetComponentInChildren<PrototypeSilverWallet>();
            depot = player.GetComponentInChildren<PrototypeRegionalLootInventory>();
            equipment = player.GetComponentInChildren<PrototypeEquipmentInventory>();
            loadout = player.GetComponentInChildren<ShipLoadout>();
            broadside = player.GetComponentInChildren<BroadsideController>();
            harpoons = player.GetComponentInChildren<HarpoonHuntingController>();
            consumables = player.GetComponentInChildren<PrototypeShipConsumables>();
        }

        private void Refresh()
        {
            if (summary == null) return;
            summary.text = view == 0
                ? $"GANİMET KAPASİTESİ  {cargo?.UsedCapacity ?? 0} / {cargo?.MaximumSilverValue ?? 0}"
                : view == 1 ? "GÜVENCEDE • Malzemeler ve yedek ekipman"
                : "GEMİDE TAKILI • Batınca korunur";
            help.text = view == 0
                ? "Ganimet batınca kaybolur. Her malzeme 1, ticari yük Silver değeri kadar yer kaplar.\nMühimmat ve sarflar ayrı gemi stoklarıdır; bu kapasiteyi kullanmaz."
                : view == 1 ? "Depodaki eşyalar batınca kaybolmaz. Tersane malzemeyi buradan kullanır.\nYedek top ve yelkenleri tersanenin DONANIM sekmesinden takabilirsin."
                : "Donanım değiştirmek için tersanenin DONANIM sekmesini kullan.\nTakılı ekipman depo adedine dahil edilmez.";
            depositAll.gameObject.SetActive(view == 0 && cargo != null && cargo.CanTransferToDepot);
            footer.text = portableOpen
                ? "I / Esc: Kapat • Deniz ve çatışma devam eder. Depo limanda açılır."
                : "Limana dönüşte ganimet otomatik depoya aktarılır. I / Esc: Kapat";
            depositAll.interactable = cargo != null && cargo.CanTransferToDepot &&
                cargo.HasCargo && wallet != null;
            foreach (var refresh in refreshRows) refresh();
        }

        private void Select(int selected)
        {
            view = selected;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                content.GetChild(i).gameObject.SetActive(false);
                Destroy(content.GetChild(i).gameObject);
            }
            refreshRows.Clear();
            rowCount = 0;
            for (int i = 0; i < tabs.Count; i++)
                tabs[i].GetComponent<Image>().color = i == view ? Gold : new Color(0.3f, 0.4f, 0.4f);
            if (view == 0) BuildHold();
            else if (view == 1) BuildDepot();
            else BuildLoadout();
            content.sizeDelta = new Vector2(900, Mathf.Max(410, rowCount * 72));
            scroll.verticalNormalizedPosition = 1;
            Refresh();
        }

        private void BuildHold()
        {
            Row("Ticari ganimet", "Teslimde cüzdana eklenir; batınca kaybolur.",
                () => $"{cargo?.UnsecuredSilverValue ?? 0} Silver", SeabornHudArt.Glyph(2));
            for (int i = 0; i < 4; i++)
            {
                var type = (RegionalMaterialType)i;
                Row(PrototypeRegionalLootInventory.DisplayName(type), MaterialUse(type),
                    () => $"×{cargo?.GetMaterial(type) ?? 0}", MaterialIcon(type),
                    () => { if (cargo != null) cargo.TryDepositMaterial(type, 1); Refresh(); },
                    () => cargo != null && cargo.CanTransferToDepot && cargo.GetMaterial(type) > 0);
            }
            string[] ammo = { "Standart gülle", "Zincirli gülle", "Saçma" };
            AmmunitionType[] types = { AmmunitionType.Standard, AmmunitionType.Chain, AmmunitionType.Grapeshot };
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                Row(ammo[i], "Gemi mühimmat stoğu", () => $"×{broadside?.GetAmmunitionStock(type) ?? 0}", SeabornHudArt.Icon(i));
            }
            Row("Hafif zıpkın", "Gemi av stoğu", () => $"×{harpoons?.LightHarpoonStock ?? 0}", SeabornHudArt.Icon(3));
            Row("Ağır zıpkın", "Gemi av stoğu", () => $"×{harpoons?.HeavyHarpoonStock ?? 0}", SeabornHudArt.Icon(4));
            Row("Tonik", "Gemi sarf stoğu", () => $"×{consumables?.TortugaTonics ?? 0}", SeabornHudArt.Icon(5));
            Row("Saklanma feneri", "Gemi sarf stoğu", () => $"×{consumables?.LightsOfTortuga ?? 0}", SeabornHudArt.Icon(6));
            Row("Rom", "Gemi sarf stoğu", () => $"×{consumables?.CorsairRum ?? 0}", SeabornHudArt.Icon(7));
            Row("Rüzgâr iksiri", "Gemi sarf stoğu", () => $"×{consumables?.GaleElixirs ?? 0}", SeabornHudArt.Icon(8));
            Row("Zırh", "Gemi sarf stoğu", () => $"×{consumables?.IronbarkBrews ?? 0}", SeabornHudArt.Icon(9));
        }

        private void BuildDepot()
        {
            for (int i = 0; i < 4; i++)
            {
                var type = (RegionalMaterialType)i;
                Row(PrototypeRegionalLootInventory.DisplayName(type), MaterialUse(type),
                    () => $"×{depot?.Get(type) ?? 0}", MaterialIcon(type));
            }
            Row("6 lb demir top", "Depodaki yedek toplar", () => $"×{equipment?.GetStoredCannons("iron_6lb") ?? 0}", SeabornHudArt.Icon(0));
            Row("12 lb demir top", "Depodaki yedek toplar", () => $"×{equipment?.GetStoredCannons("iron_12lb") ?? 0}", SeabornHudArt.Icon(0));
            Row("Yamalı yelken", "Depodaki yedek yelkenler", () => $"×{equipment?.GetStoredSails("patched_canvas") ?? 0}", SeabornHudArt.Glyph(3));
            Row("Rat yelkeni", "Depodaki yedek yelkenler", () => $"×{equipment?.GetStoredSails("rat_sails") ?? 0}", SeabornHudArt.Glyph(3));
        }

        private void BuildLoadout()
        {
            Row("6 lb demir top", "Takılı hafif toplar", () =>
                $"×{loadout?.CountCannons("iron_6lb") ?? 0}", SeabornHudArt.Icon(0));
            Row("12 lb demir top", "Takılı ağır toplar", () =>
                $"×{loadout?.CountCannons("iron_12lb") ?? 0}", SeabornHudArt.Icon(0));
            Row("Yelken", "Takılı yelken", () => loadout?.Sail?.displayName ?? "—", SeabornHudArt.Glyph(3));
            Row("Zıpkın", "Seçili av mühimmatı", () => harpoons?.SelectedHarpoon?.displayName ?? "—", SeabornHudArt.Icon(3));
        }

        private void Row(string title, string description, Func<string> amount, Sprite icon,
            Action transfer = null, Func<bool> canTransfer = null)
        {
            var row = Rect(content, title, 0, rowCount++ * 72, 900, 68);
            row.gameObject.AddComponent<Image>().color = new Color(0.055f, 0.14f, 0.17f, 1);
            var picture = Rect(row, "Icon", 12, 10, 46, 46).gameObject.AddComponent<Image>();
            picture.sprite = icon;
            picture.preserveAspect = true;
            picture.raycastTarget = false;
            Label(row, title, 15, 72, 8, 390, 23);
            Label(row, description, 12, 72, 34, 490, 28);
            var count = Label(row, "", 14, 570, 12, transfer == null ? 315 : 175, 44);
            count.alignment = TextAnchor.MiddleRight;
            Button button = transfer == null ? null : Button(row, "1 AKTAR", 760, 14, 125, 38, transfer);
            refreshRows.Add(() =>
            {
                count.text = amount();
                if (button != null) button.interactable = canTransfer != null && canTransfer();
            });
        }

        private static string MaterialUse(RegionalMaterialType type) => type switch
        {
            RegionalMaterialType.TideOil => "Zıpkın geliştirmelerinde kullanılır; avlardan çıkar.",
            RegionalMaterialType.StormjawScale => "İleri zıpkın geliştirmesi; Stormjaw avından çıkar.",
            RegionalMaterialType.CorsairIron => "Gövde ve top geliştirmeleri; korsan enkazlarından çıkar.",
            _ => "İleri geliştirmeler; ağır korsan ve nadir ganimet."
        };

        private static Sprite MaterialIcon(RegionalMaterialType type) => type switch
        {
            RegionalMaterialType.TideOil => SeabornHudArt.Icon(7),
            RegionalMaterialType.StormjawScale => SeabornHudArt.Icon(9),
            RegionalMaterialType.CorsairIron => SeabornHudArt.Icon(1),
            _ => SeabornHudArt.Glyph(2)
        };

        private Button Button(Transform parent, string title, float x, float y, float w, float h, Action action)
        {
            var rect = Rect(parent, title, x, y, w, h);
            rect.gameObject.AddComponent<Image>().color = Gold;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(() => action());
            var text = Label(rect, title, 13, 0, 0, w, h);
            text.color = Navy;
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private Text Label(Transform parent, string value, int size, float x, float y, float w, float h)
        {
            var text = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = Cream;
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
