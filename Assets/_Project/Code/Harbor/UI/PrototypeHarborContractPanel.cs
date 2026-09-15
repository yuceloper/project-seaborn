using System.Collections.Generic;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeHarborContractPanel :
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

        private readonly Dictionary<string, DailyRow>
            dailyRows = new();
        private readonly List<Text> monthlyRows = new();

        private Transform player;
        private PrototypeContractBoard contracts;
        private PrototypeSilverWallet wallet;
        private GameObject panel;
        private RectTransform dailyRoot;
        private RectTransform monthlyRoot;
        private Text silverText;
        private Text statusText;
        private Text dailySummary;
        private Text monthlySummary;
        private float nextRefreshTime;
        private float statusExpiresAt;

        private sealed class DailyRow
        {
            public Image Background;
            public Button Button;
            public Text Label;
        }

        public static void EnsureCreated(Transform player)
        {
            if (player == null)
            {
                return;
            }

            PrototypeHarborContractPanel existing =
                FindFirstObjectByType<
                    PrototypeHarborContractPanel>();
            if (existing == null)
            {
                GameObject root = new(
                    "Prototype Harbor Contract Panel");
                existing = root.AddComponent<
                    PrototypeHarborContractPanel>();
            }

            existing.Bind(player);
        }

        private void Awake()
        {
            BuildInterface();
        }

        private void Bind(Transform playerTransform)
        {
            player = playerTransform;
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
                    PrototypeHarborStation.HarborOffice) &&
                PrototypeHarborUiCoordinator.IsSelected(
                    PrototypeHarborTab.Contracts);

            if (panel.activeSelf != visible)
            {
                panel.SetActive(visible);
            }

            if (!visible ||
                Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime =
                Time.unscaledTime + 0.15f;
            Refresh();
        }

        private void ResolveBindings()
        {
            if (contracts == null)
            {
                contracts =
                    PrototypeContractBoard.Instance;
            }
            if (wallet == null && player != null)
            {
                wallet = player.GetComponentInChildren<
                    PrototypeSilverWallet>();
            }

            if (contracts != null &&
                dailyRows.Count == 0)
            {
                BuildContractRows();
            }
        }

        private void BuildInterface()
        {
            Canvas canvas =
                gameObject.AddComponent<Canvas>();
            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 82;

            CanvasScaler scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            panel = CreateBlock(
                transform,
                "Harbor Office Panel",
                Navy
            );
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition =
                new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(500f, 680f);

            GameObject accent = CreateBlock(
                rect,
                "Accent",
                Gold
            );
            RectTransform accentRect =
                accent.GetComponent<RectTransform>();
            accentRect.anchorMin =
                new Vector2(0f, 1f);
            accentRect.anchorMax =
                new Vector2(1f, 1f);
            accentRect.pivot =
                new Vector2(0.5f, 1f);
            accentRect.anchoredPosition =
                Vector2.zero;
            accentRect.sizeDelta =
                new Vector2(0f, 4f);

            CreateText(
                rect,
                "LİMAN İDARESİ",
                23,
                Gold,
                new Vector2(22f, -20f),
                new Vector2(290f, 32f),
                FontStyle.Bold
            );
            silverText = CreateText(
                rect,
                "0 SILVER",
                15,
                Cream,
                new Vector2(330f, -23f),
                new Vector2(146f, 26f),
                FontStyle.Bold,
                TextAnchor.UpperRight
            );
            statusText = CreateText(
                rect,
                "Bir günlük görev seç.",
                13,
                Muted,
                new Vector2(22f, -56f),
                new Vector2(454f, 24f)
            );

            dailySummary = CreateText(
                rect,
                "GÜNLÜK KONTRATLAR  0/3",
                14,
                Gold,
                new Vector2(22f, -96f),
                new Vector2(454f, 24f),
                FontStyle.Bold
            );

            GameObject daily = CreateBlock(
                rect,
                "Daily Contracts",
                new Color(0f, 0f, 0f, 0f)
            );
            dailyRoot =
                daily.GetComponent<RectTransform>();
            dailyRoot.anchorMin =
                new Vector2(0f, 1f);
            dailyRoot.anchorMax =
                new Vector2(0f, 1f);
            dailyRoot.pivot =
                new Vector2(0f, 1f);
            dailyRoot.anchoredPosition =
                new Vector2(22f, -128f);
            dailyRoot.sizeDelta =
                new Vector2(454f, 270f);

            monthlySummary = CreateText(
                rect,
                "AYLIK SEYİR DEFTERİ  0/3",
                14,
                Gold,
                new Vector2(22f, -420f),
                new Vector2(454f, 24f),
                FontStyle.Bold
            );

            GameObject monthly = CreateBlock(
                rect,
                "Monthly Contracts",
                new Color(0f, 0f, 0f, 0f)
            );
            monthlyRoot =
                monthly.GetComponent<RectTransform>();
            monthlyRoot.anchorMin =
                new Vector2(0f, 1f);
            monthlyRoot.anchorMax =
                new Vector2(0f, 1f);
            monthlyRoot.pivot =
                new Vector2(0f, 1f);
            monthlyRoot.anchoredPosition =
                new Vector2(22f, -452f);
            monthlyRoot.sizeDelta =
                new Vector2(454f, 170f);

            CreateText(
                rect,
                "Günlük görev tamamlanınca diğerini seçebilirsin.  •  E ile ayrıl",
                12,
                Muted,
                new Vector2(22f, -642f),
                new Vector2(454f, 22f),
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );

            panel.SetActive(false);
        }

        private void BuildContractRows()
        {
            int index = 0;
            foreach (PrototypeContractProgress contract
                     in contracts.DailyContracts)
            {
                PrototypeContractProgress captured =
                    contract;
                GameObject row = CreateBlock(
                    dailyRoot,
                    contract.Definition.Id,
                    NavyLight
                );
                RectTransform rect =
                    row.GetComponent<RectTransform>();
                rect.anchorMin =
                    new Vector2(0f, 1f);
                rect.anchorMax =
                    new Vector2(0f, 1f);
                rect.pivot =
                    new Vector2(0f, 1f);
                rect.anchoredPosition =
                    new Vector2(0f, index * -86f);
                rect.sizeDelta =
                    new Vector2(454f, 76f);

                Text label = CreateText(
                    rect,
                    "",
                    12,
                    Cream,
                    new Vector2(12f, -9f),
                    new Vector2(310f, 58f),
                    FontStyle.Bold
                );

                GameObject buttonObject = CreateBlock(
                    rect,
                    "Select",
                    Gold
                );
                RectTransform buttonRect =
                    buttonObject.GetComponent<
                        RectTransform>();
                buttonRect.anchorMin =
                    new Vector2(0f, 1f);
                buttonRect.anchorMax =
                    new Vector2(0f, 1f);
                buttonRect.pivot =
                    new Vector2(0f, 1f);
                buttonRect.anchoredPosition =
                    new Vector2(326f, -14f);
                buttonRect.sizeDelta =
                    new Vector2(114f, 46f);

                Button button =
                    buttonObject.AddComponent<Button>();
                button.targetGraphic =
                    buttonObject.GetComponent<Image>();
                button.onClick.AddListener(
                    () => SelectDaily(captured)
                );
                Text buttonText = CreateText(
                    buttonRect,
                    "SEÇ",
                    12,
                    Navy,
                    Vector2.zero,
                    new Vector2(114f, 46f),
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter
                );
                RectTransform buttonLabel =
                    buttonText.GetComponent<
                        RectTransform>();
                buttonLabel.anchorMin = Vector2.zero;
                buttonLabel.anchorMax = Vector2.one;
                buttonLabel.offsetMin = Vector2.zero;
                buttonLabel.offsetMax = Vector2.zero;

                dailyRows.Add(
                    contract.Definition.Id,
                    new DailyRow
                    {
                        Background =
                            row.GetComponent<Image>(),
                        Button = button,
                        Label = label
                    }
                );
                index++;
            }

            index = 0;
            foreach (PrototypeContractProgress contract
                     in contracts.MonthlyContracts)
            {
                Text row = CreateText(
                    monthlyRoot,
                    "",
                    12,
                    Cream,
                    new Vector2(8f, index * -48f),
                    new Vector2(438f, 42f),
                    FontStyle.Normal
                );
                monthlyRows.Add(row);
                index++;
            }
        }

        private void Refresh()
        {
            if (panel == null)
            {
                return;
            }

            ResolveBindings();
            silverText.text =
                $"{(wallet != null ? wallet.Silver : 0)} SILVER";

            if (contracts == null)
            {
                statusText.text =
                    "Kontrat panosu hazırlanıyor...";
                return;
            }

            dailySummary.text =
                $"GÜNLÜK KONTRATLAR  " +
                $"{contracts.DailyCompletedCount}/3";
            monthlySummary.text =
                $"AYLIK SEYİR DEFTERİ  " +
                $"{contracts.MonthlyCompletedCount}/3";

            foreach (PrototypeContractProgress contract
                     in contracts.DailyContracts)
            {
                if (!dailyRows.TryGetValue(
                        contract.Definition.Id,
                        out DailyRow row))
                {
                    continue;
                }

                bool selected =
                    contracts.SelectedDailyContract ==
                    contract;
                row.Background.color =
                    selected
                        ? new Color(
                            0.12f, 0.25f, 0.24f, 0.98f)
                        : NavyLight;
                string state = contract.RewardClaimed
                    ? "TAMAMLANDI  •  ÖDÜL ALINDI  •  "
                    : selected
                        ? "AKTİF  •  "
                        : "";
                row.Label.text =
                    state +
                    $"{contract.Definition.Title}\n" +
                    $"{contract.Current}/" +
                    $"{contract.Definition.Target}  •  " +
                    $"+{contract.Definition.SilverReward} silver";
                row.Label.color =
                    contract.IsComplete
                        ? Success
                        : Cream;
                bool anotherActive =
                    contracts.SelectedDailyContract != null &&
                    contracts.SelectedDailyContract != contract;
                row.Button.interactable =
                    !selected &&
                    !contract.RewardClaimed &&
                    !contract.IsComplete &&
                    !anotherActive;
                Text buttonText =
                    row.Button.GetComponentInChildren<Text>();
                buttonText.text =
                    contract.RewardClaimed
                        ? "ALINDI"
                        : selected
                            ? "AKTİF"
                            : "SEÇ";
            }

            for (int i = 0;
                 i < contracts.MonthlyContracts.Count &&
                 i < monthlyRows.Count;
                 i++)
            {
                PrototypeContractProgress contract =
                    contracts.MonthlyContracts[i];
                monthlyRows[i].text =
                    $"{contract.Definition.Title}  •  " +
                    $"{contract.Current}/" +
                    $"{contract.Definition.Target}  •  " +
                    $"+{contract.Definition.SilverReward}";
                monthlyRows[i].color =
                    contract.IsComplete
                        ? Success
                        : Cream;
            }

            if (statusExpiresAt > 0f &&
                Time.unscaledTime >= statusExpiresAt)
            {
                statusExpiresAt = 0f;
                statusText.text =
                    "Bir günlük görev seç.";
                statusText.color = Muted;
            }
        }

        private void SelectDaily(
            PrototypeContractProgress contract)
        {
            bool selected =
                contracts != null &&
                contracts.SelectDailyContract(
                    contract.Definition.Id);

            statusText.text =
                selected
                    ? $"{contract.Definition.Title} aktif."
                    : "Görev yalnızca limanda seçilebilir.";
            statusText.color =
                selected ? Success : Muted;
            statusExpiresAt =
                Time.unscaledTime + 3f;
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
            rect.sizeDelta = size;

            Text text = item.GetComponent<Text>();
            text.font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");
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
