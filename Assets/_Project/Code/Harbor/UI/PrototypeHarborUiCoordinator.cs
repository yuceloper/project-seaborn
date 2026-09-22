using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    public enum PrototypeHarborTab
    {
        ShipMarket,
        Loadout,
        Upgrades,
        ExtendedDeck,
        TradePreparation,
        Consumables,
        Contracts,
        CaptainSkills,
        Inventory
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeHarborUiCoordinator :
        MonoBehaviour
    {
        private static readonly Color Navy =
            new(0.018f, 0.052f, 0.072f, 0.96f);
        private static readonly Color NavyLight =
            new(0.055f, 0.14f, 0.17f, 0.98f);
        private static readonly Color Gold =
            new(0.86f, 0.68f, 0.3f, 1f);
        private static readonly Color Cream =
            new(0.91f, 0.88f, 0.76f, 1f);
        private static readonly Color Success =
            new(0.3f, 0.76f, 0.57f, 1f);

        public static PrototypeHarborUiCoordinator
            Instance { get; private set; }

        public static bool IsOpen =>
            Instance != null &&
            Instance.shell != null &&
            Instance.shell.activeSelf;

        public static bool IsSelected(PrototypeHarborTab tab)
        {
            return Instance == null ||
                Instance.selectedTab == tab;
        }

        private readonly List<Button> buttons = new();
        private readonly List<PrototypeHarborTab>
            buttonTabs = new();
        private GameObject shell;
        private RectTransform tabRoot;
        private Text stationTitle;
        private PrototypeHarborStation activeStation;
        private PrototypeHarborTab selectedTab;

        public static void EnsureCreated()
        {
            PrototypeHarborUiCoordinator existing =
                FindFirstObjectByType<
                    PrototypeHarborUiCoordinator>();
            if (existing != null) return;

            new GameObject("Prototype Harbor UI")
                .AddComponent<PrototypeHarborUiCoordinator>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildInterface();
            PrototypeHarborInventoryPanel.EnsureCreated();
            shell.SetActive(false);
        }

        private void Update()
        {
            PrototypeHarborDockingDirector docking =
                PrototypeHarborDockingDirector.Instance;
            PrototypeHarborStation station =
                docking != null
                    ? docking.DockedStation
                    : PrototypeHarborStation.None;

            if (station == PrototypeHarborStation.None)
            {
                if (shell.activeSelf) shell.SetActive(false);
                activeStation = station;
                return;
            }

            if (!shell.activeSelf) shell.SetActive(true);
            if (activeStation != station)
            {
                activeStation = station;
                selectedTab = DefaultTab(station);
                RebuildTabs();
            }

            RefreshButtons();
        }

        private void Select(PrototypeHarborTab tab)
        {
            selectedTab = tab;
            RefreshButtons();
        }

        public void OpenInventory() => Select(PrototypeHarborTab.Inventory);

        public void CloseInventory()
        {
            if (selectedTab == PrototypeHarborTab.Inventory)
                Select(DefaultTab(activeStation));
        }

        private void RebuildTabs()
        {
            for (int i = tabRoot.childCount - 1; i >= 0; i--)
                Destroy(tabRoot.GetChild(i).gameObject);
            buttons.Clear();
            buttonTabs.Clear();

            stationTitle.text = activeStation switch
            {
                PrototypeHarborStation.Shipyard =>
                    "TERSANE",
                PrototypeHarborStation.Trade =>
                    "TİCARET VE İKMAL",
                PrototypeHarborStation.HarborOffice =>
                    "LİMAN İDARESİ",
                _ => "GÜVENLİ LİMAN"
            };

            if (activeStation ==
                PrototypeHarborStation.Shipyard)
            {
                AddTab("GEMİLER", PrototypeHarborTab.ShipMarket);
                AddTab("DONANIM", PrototypeHarborTab.Loadout);
                AddTab("GELİŞTİR", PrototypeHarborTab.Upgrades);
                AddTab("GÜVERTE", PrototypeHarborTab.ExtendedDeck);
                AddTab("AMBAR / DEPO", PrototypeHarborTab.Inventory);
            }
            else if (activeStation ==
                     PrototypeHarborStation.Trade)
            {
                AddTab("HAZIRLIK",
                    PrototypeHarborTab.TradePreparation);
                AddTab("TEDARİKÇİ",
                    PrototypeHarborTab.Consumables);
                AddTab("AMBAR / DEPO", PrototypeHarborTab.Inventory);
            }
            else if (activeStation ==
                     PrototypeHarborStation.HarborOffice)
            {
                AddTab("GÖREVLER",
                    PrototypeHarborTab.Contracts);
                AddTab("YETENEKLER",
                    PrototypeHarborTab.CaptainSkills);
            }
        }

        private void AddTab(
            string label,
            PrototypeHarborTab tab)
        {
            GameObject item = new(
                label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            item.transform.SetParent(tabRoot, false);
            RectTransform rect =
                item.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(156f, 44f);

            Button button = item.GetComponent<Button>();
            button.onClick.AddListener(() => Select(tab));

            Text text = CreateText(
                item.transform,
                label,
                13,
                Cream,
                FontStyle.Bold
            );
            text.alignment = TextAnchor.MiddleCenter;

            buttons.Add(button);
            buttonTabs.Add(tab);
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                bool selected = buttonTabs[i] == selectedTab;
                Image image = buttons[i].GetComponent<Image>();
                image.color = selected ? Gold : NavyLight;
                Text label =
                    buttons[i].GetComponentInChildren<Text>();
                label.color = selected ? Navy : Cream;
            }
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            CanvasScaler scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            shell = new GameObject(
                "Harbor Shell",
                typeof(RectTransform)
            );
            shell.transform.SetParent(transform, false);
            RectTransform shellRect =
                shell.GetComponent<RectTransform>();
            shellRect.anchorMin = Vector2.zero;
            shellRect.anchorMax = Vector2.one;
            shellRect.offsetMin = Vector2.zero;
            shellRect.offsetMax = Vector2.zero;

            GameObject veil = new(
                "Veil",
                typeof(RectTransform),
                typeof(Image)
            );
            veil.transform.SetParent(shell.transform, false);
            RectTransform veilRect =
                veil.GetComponent<RectTransform>();
            veilRect.anchorMin = Vector2.zero;
            veilRect.anchorMax = Vector2.one;
            veilRect.offsetMin = Vector2.zero;
            veilRect.offsetMax = Vector2.zero;
            veil.GetComponent<Image>().color =
                new Color(0.005f, 0.02f, 0.028f, 0.58f);

            GameObject header = new(
                "Header",
                typeof(RectTransform),
                typeof(Image)
            );
            header.transform.SetParent(shell.transform, false);
            RectTransform headerRect =
                header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -20f);
            headerRect.sizeDelta = new Vector2(900f, 112f);
            header.GetComponent<Image>().color = Navy;

            stationTitle = CreateText(
                header.transform,
                "GÜVENLİ LİMAN",
                20,
                Success,
                FontStyle.Bold
            );
            RectTransform titleRect =
                stationTitle.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);
            titleRect.sizeDelta = new Vector2(0f, 28f);
            stationTitle.alignment = TextAnchor.MiddleCenter;

            GameObject tabs = new(
                "Tabs",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup)
            );
            tabs.transform.SetParent(header.transform, false);
            tabRoot = tabs.GetComponent<RectTransform>();
            tabRoot.anchorMin = new Vector2(0.5f, 0f);
            tabRoot.anchorMax = new Vector2(0.5f, 0f);
            tabRoot.pivot = new Vector2(0.5f, 0f);
            tabRoot.anchoredPosition = new Vector2(0f, 12f);
            tabRoot.sizeDelta = new Vector2(820f, 44f);

            HorizontalLayoutGroup layout =
                tabs.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static PrototypeHarborTab DefaultTab(
            PrototypeHarborStation station)
        {
            return station switch
            {
                PrototypeHarborStation.Shipyard =>
                    PrototypeHarborTab.ShipMarket,
                PrototypeHarborStation.Trade =>
                    PrototypeHarborTab.TradePreparation,
                PrototypeHarborStation.HarborOffice =>
                    PrototypeHarborTab.Contracts,
                _ => PrototypeHarborTab.Contracts
            };
        }

        private static Text CreateText(
            Transform parent,
            string value,
            int size,
            Color color,
            FontStyle style)
        {
            GameObject item = new(
                "Text",
                typeof(RectTransform),
                typeof(Text)
            );
            item.transform.SetParent(parent, false);
            RectTransform rect =
                item.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = item.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
