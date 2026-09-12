using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Seaborn.Harbor
{
    [DisallowMultipleComponent]
    public sealed class PrototypeHarborPreparationPanel :
        MonoBehaviour
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

        private Transform player;
        private PrototypeHarborServices services;
        private PrototypeSafeHarborProtection protection;
        private PrototypeContractBoard contracts;
        private ShipHealth health;
        private ShipSubsystemController subsystems;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;

        private GameObject panel;
        private Font font;
        private Text silverText;
        private Text statusText;
        private Text healthText;
        private Text standardText;
        private Text chainText;
        private Text grapeshotText;
        private Text harpoonText;
        private Text recurringText;
        private Text readinessText;
        private Button repairButton;
        private Button standardButton;
        private Button chainButton;
        private Button grapeshotButton;
        private Button harpoonButton;
        private readonly Dictionary<string, Image>
            contractBackgrounds = new();
        private float nextRefreshTime;
        private float statusExpiresAt;

        public static void EnsureCreated(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            PrototypeHarborPreparationPanel existing =
                FindFirstObjectByType<
                    PrototypeHarborPreparationPanel>();

            if (existing == null)
            {
                GameObject canvasObject =
                    new GameObject(
                        "Prototype Harbor Preparation UI"
                    );
                existing = canvasObject.AddComponent<
                    PrototypeHarborPreparationPanel>();
            }

            existing.Bind(playerTransform);
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            EnsureEventSystem();
            BuildInterface();
        }

        private void Update()
        {
            bool shouldShow =
                protection != null &&
                protection.IsProtected;

            if (panel.activeSelf != shouldShow)
            {
                panel.SetActive(shouldShow);
            }

            if (!shouldShow ||
                Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + 0.15f;
            Refresh();

            if (statusExpiresAt > 0f &&
                Time.unscaledTime >= statusExpiresAt)
            {
                statusExpiresAt = 0f;
                statusText.text =
                    "Kontratını seç, gemini hazırla.";
                statusText.color = Muted;
            }
        }

        private void Bind(Transform playerTransform)
        {
            player = playerTransform;
            services = player.GetComponent<
                PrototypeHarborServices>();
            protection = player.GetComponent<
                PrototypeSafeHarborProtection>();
            health = player.GetComponentInChildren<
                ShipHealth>();
            subsystems = health != null
                ? health.GetComponent<
                    ShipSubsystemController>()
                : null;
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponent<
                HarpoonHuntingController>();
            contracts = PrototypeContractBoard.Instance;
            Refresh();
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

            panel = CreateBlock(
                transform,
                "Harbor Panel",
                Navy,
                new Vector2(24f, -24f),
                new Vector2(456f, 1010f)
            );

            CreateAccent(panel.transform);

            Text title = CreateText(
                panel.transform,
                "Title",
                "GÜVENLİ LİMAN",
                28,
                Gold,
                new Vector2(24f, -24f),
                new Vector2(280f, 38f),
                FontStyle.Bold
            );
            title.letterSpacingCompat(1f);

            silverText = CreateText(
                panel.transform,
                "Silver",
                "0 SILVER",
                20,
                Cream,
                new Vector2(286f, -27f),
                new Vector2(142f, 32f),
                FontStyle.Bold,
                TextAnchor.MiddleRight
            );

            statusText = CreateText(
                panel.transform,
                "Status",
                "Kontratını seç, gemini hazırla.",
                14,
                Muted,
                new Vector2(24f, -68f),
                new Vector2(404f, 28f)
            );

            CreateRule(panel.transform, -106f);
            CreateSectionLabel(
                panel.transform,
                "SEFER KONTRATI",
                -122f
            );

            CreateContractButtons();
            CreateRule(panel.transform, -274f);

            recurringText = CreateText(
                panel.transform,
                "Recurring",
                "Günlük 0/3   •   Aylık 0/3",
                13,
                Muted,
                new Vector2(24f, -290f),
                new Vector2(404f, 24f),
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );

            CreateSectionLabel(
                panel.transform,
                "GEMİ HAZIRLIĞI",
                -330f
            );

            healthText = CreateText(
                panel.transform,
                "Health",
                "Gövde",
                16,
                Cream,
                new Vector2(24f, -366f),
                new Vector2(245f, 40f),
                FontStyle.Bold,
                TextAnchor.MiddleLeft
            );
            repairButton = CreateActionButton(
                panel.transform,
                "ONAR",
                new Vector2(286f, -366f),
                () => RunService(
                    services != null
                        ? services.TryRepair()
                        : PrototypeHarborServiceResult
                            .Unavailable
                )
            );

            standardText = CreateSupplyRow(
                "Standart gülle",
                -424f,
                out standardButton,
                () => Restock(AmmunitionType.Standard)
            );
            chainText = CreateSupplyRow(
                "Zincirli gülle",
                -482f,
                out chainButton,
                () => Restock(AmmunitionType.Chain)
            );
            grapeshotText = CreateSupplyRow(
                "Saçma",
                -540f,
                out grapeshotButton,
                () => Restock(AmmunitionType.Grapeshot)
            );
            harpoonText = CreateSupplyRow(
                "Av zıpkını",
                -598f,
                out harpoonButton,
                () => RunService(
                    services != null
                        ? services.TryRestockHarpoons()
                        : PrototypeHarborServiceResult
                            .Unavailable
                )
            );

            CreateRule(panel.transform, -662f);
            CreateSectionLabel(
                panel.transform,
                "SEFERE HAZIRLIK",
                -680f
            );

            readinessText = CreateText(
                panel.transform,
                "Readiness",
                "",
                15,
                Cream,
                new Vector2(24f, -718f),
                new Vector2(404f, 145f)
            );

            Text hint = CreateText(
                panel.transform,
                "Departure Hint",
                "Liman halkasından ayrıldığında sefer başlar",
                13,
                Gold,
                new Vector2(24f, -885f),
                new Vector2(404f, 44f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            hint.horizontalOverflow =
                HorizontalWrapMode.Wrap;
        }

        private void CreateContractButtons()
        {
            if (contracts == null)
            {
                contracts = PrototypeContractBoard.Instance;
            }

            if (contracts == null)
            {
                return;
            }

            int index = 0;
            foreach (PrototypeContractProgress contract
                     in contracts.ExpeditionContracts)
            {
                PrototypeContractProgress captured = contract;
                float x = 24f + index * 136f;
                GameObject buttonObject = CreateBlock(
                    panel.transform,
                    "Contract " +
                    contract.Definition.Id,
                    NavyLight,
                    new Vector2(x, -158f),
                    new Vector2(128f, 100f)
                );

                Button button =
                    buttonObject.AddComponent<Button>();
                button.targetGraphic =
                    buttonObject.GetComponent<Image>();
                button.onClick.AddListener(
                    () => SelectContract(captured)
                );

                Text text = CreateText(
                    buttonObject.transform,
                    "Label",
                    ContractLabel(contract),
                    12,
                    Cream,
                    new Vector2(8f, -8f),
                    new Vector2(112f, 84f),
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter
                );
                text.horizontalOverflow =
                    HorizontalWrapMode.Wrap;
                text.verticalOverflow =
                    VerticalWrapMode.Truncate;

                contractBackgrounds[
                    contract.Definition.Id
                ] = buttonObject.GetComponent<Image>();
                index++;
            }
        }

        private Text CreateSupplyRow(
            string label,
            float y,
            out Button button,
            UnityEngine.Events.UnityAction action)
        {
            Text valueText = CreateText(
                panel.transform,
                label,
                label,
                15,
                Cream,
                new Vector2(24f, y),
                new Vector2(245f, 40f),
                FontStyle.Normal,
                TextAnchor.MiddleLeft
            );
            button = CreateActionButton(
                panel.transform,
                "İKMAL",
                new Vector2(286f, y),
                action
            );
            return valueText;
        }

        private Button CreateActionButton(
            Transform parent,
            string label,
            Vector2 position,
            UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateBlock(
                parent,
                label,
                Gold,
                position,
                new Vector2(142f, 40f)
            );
            Button button =
                buttonObject.AddComponent<Button>();
            button.targetGraphic =
                buttonObject.GetComponent<Image>();
            button.onClick.AddListener(action);

            Text text = CreateText(
                buttonObject.transform,
                "Label",
                label,
                13,
                Navy,
                Vector2.zero,
                new Vector2(142f, 40f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            RectTransform rect =
                text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        private void Refresh()
        {
            if (services == null && player != null)
            {
                Bind(player);
                return;
            }

            silverText.text =
                $"{(services != null ? services.Silver : 0)} SILVER";

            if (health != null)
            {
                healthText.text =
                    $"Gövde  {health.CurrentHealth:0}/" +
                    $"{health.MaximumHealth:0}   •   " +
                    $"{(services != null ? services.RepairCost : 0)} silver";
            }

            UpdateSupplyText(
                standardText,
                "Standart gülle",
                AmmunitionType.Standard,
                PrototypeHarborServiceType.StandardAmmunition
            );
            UpdateSupplyText(
                chainText,
                "Zincirli gülle",
                AmmunitionType.Chain,
                PrototypeHarborServiceType.ChainAmmunition
            );
            UpdateSupplyText(
                grapeshotText,
                "Saçma",
                AmmunitionType.Grapeshot,
                PrototypeHarborServiceType.GrapeshotAmmunition
            );

            if (harpoonText != null)
            {
                harpoonText.text =
                    $"Av zıpkını  {harpoons?.HarpoonStock ?? 0}   •   " +
                    $"{Cost(PrototypeHarborServiceType.Harpoons)} silver";
            }

            if (contracts != null)
            {
                recurringText.text =
                    $"Günlük {contracts.DailyCompletedCount}/3" +
                    "   •   " +
                    $"Aylık {contracts.MonthlyCompletedCount}/3";

                foreach (PrototypeContractProgress contract
                         in contracts.ExpeditionContracts)
                {
                    bool selected =
                        contracts.SelectedExpeditionContract ==
                        contract;
                    if (contractBackgrounds.TryGetValue(
                            contract.Definition.Id,
                            out Image background))
                    {
                        background.color =
                            selected ? Gold : NavyLight;
                        Text label =
                            background.GetComponentInChildren<Text>();
                        label.color = selected ? Navy : Cream;
                        label.text = ContractLabel(contract);
                    }
                }
            }

            int silver = services != null
                ? services.Silver
                : 0;
            bool selectedContract =
                contracts?.SelectedExpeditionContract != null;
            bool repaired =
                health != null &&
                health.CurrentHealth >=
                    health.MaximumHealth - 0.01f &&
                (subsystems == null ||
                    subsystems.MissingIntegrity <= 0.01f);
            bool supplied =
                broadside != null &&
                broadside.GetAmmunitionStock(
                    AmmunitionType.Standard) >= 12 &&
                harpoons != null &&
                harpoons.HarpoonStock >= 3;

            readinessText.text =
                Check(selectedContract) +
                " Sefer kontratı seçildi\n" +
                Check(repaired) +
                " Gemi sefere dayanıklı\n" +
                Check(supplied) +
                " Temel mühimmat hazır\n\n" +
                $"Kasa: {silver} silver";

            bool usable =
                services != null &&
                services.CanUseServices;
            repairButton.interactable =
                usable &&
                services.RepairCost > 0 &&
                silver >= services.RepairCost;
            SetSupplyInteractable(
                standardButton,
                PrototypeHarborServiceType.StandardAmmunition,
                usable,
                silver
            );
            SetSupplyInteractable(
                chainButton,
                PrototypeHarborServiceType.ChainAmmunition,
                usable,
                silver
            );
            SetSupplyInteractable(
                grapeshotButton,
                PrototypeHarborServiceType.GrapeshotAmmunition,
                usable,
                silver
            );
            SetSupplyInteractable(
                harpoonButton,
                PrototypeHarborServiceType.Harpoons,
                usable,
                silver
            );
        }

        private void UpdateSupplyText(
            Text target,
            string label,
            AmmunitionType ammunition,
            PrototypeHarborServiceType serviceType)
        {
            if (target == null)
            {
                return;
            }

            int stock = broadside != null
                ? broadside.GetAmmunitionStock(ammunition)
                : 0;
            target.text =
                $"{label}  {stock}   •   {Cost(serviceType)} silver";
        }

        private void SetSupplyInteractable(
            Button button,
            PrototypeHarborServiceType type,
            bool usable,
            int silver)
        {
            button.interactable =
                usable && silver >= Cost(type);
        }

        private int Cost(PrototypeHarborServiceType type)
        {
            return services != null
                ? services.GetServiceCost(type)
                : 0;
        }

        private void Restock(AmmunitionType type)
        {
            RunService(
                services != null
                    ? services.TryRestockAmmunition(type)
                    : PrototypeHarborServiceResult.Unavailable
            );
        }

        private void RunService(
            PrototypeHarborServiceResult result)
        {
            switch (result)
            {
                case PrototypeHarborServiceResult.Completed:
                    ShowStatus(
                        "Hazırlık tamamlandı.",
                        Success
                    );
                    break;
                case PrototypeHarborServiceResult
                    .InsufficientSilver:
                    ShowStatus(
                        "Yeterli silver yok.",
                        new Color(0.9f, 0.43f, 0.34f)
                    );
                    break;
                case PrototypeHarborServiceResult.NothingToDo:
                    ShowStatus(
                        "Gemi zaten tam durumda.",
                        Muted
                    );
                    break;
                default:
                    ShowStatus(
                        "Bu işlem şu anda kullanılamıyor.",
                        Muted
                    );
                    break;
            }

            Refresh();
        }

        private void SelectContract(
            PrototypeContractProgress contract)
        {
            bool selected =
                contracts != null &&
                contracts.SelectExpeditionContract(
                    contract.Definition.Id
                );

            ShowStatus(
                selected
                    ? $"{contract.Definition.Title} kontratı seçildi."
                    : "Kontrat yalnızca limanda seçilebilir.",
                selected ? Success : Muted
            );
            Refresh();
        }

        private void ShowStatus(string message, Color color)
        {
            statusText.text = message;
            statusText.color = color;
            statusExpiresAt = Time.unscaledTime + 3f;
        }

        private static string ContractLabel(
            PrototypeContractProgress contract)
        {
            return
                $"{contract.Definition.Title}\n" +
                $"{contract.Definition.Description}\n" +
                $"+{contract.Definition.SilverReward}";
        }

        private static string Check(bool value)
        {
            return value ? "✓" : "○";
        }

        private void CreateSectionLabel(
            Transform parent,
            string label,
            float y)
        {
            CreateText(
                parent,
                label,
                label,
                13,
                Gold,
                new Vector2(24f, y),
                new Vector2(404f, 24f),
                FontStyle.Bold
            );
        }

        private void CreateAccent(Transform parent)
        {
            GameObject accent = CreateBlock(
                parent,
                "Gold Accent",
                Gold,
                Vector2.zero,
                new Vector2(6f, 1010f)
            );
            RectTransform rect =
                accent.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
        }

        private void CreateRule(Transform parent, float y)
        {
            CreateBlock(
                parent,
                "Rule",
                new Color(0.86f, 0.68f, 0.3f, 0.22f),
                new Vector2(24f, y),
                new Vector2(404f, 1f)
            );
        }

        private GameObject CreateBlock(
            Transform parent,
            string objectName,
            Color color,
            Vector2 position,
            Vector2 size)
        {
            GameObject result = new GameObject(objectName);
            result.transform.SetParent(parent, false);
            Image image = result.AddComponent<Image>();
            image.color = color;

            RectTransform rect =
                result.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return result;
        }

        private Text CreateText(
            Transform parent,
            string objectName,
            string value,
            int fontSize,
            Color color,
            Vector2 position,
            Vector2 size,
            FontStyle style = FontStyle.Normal,
            TextAnchor alignment = TextAnchor.UpperLeft)
        {
            GameObject textObject =
                new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
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

            RectTransform rect =
                textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return text;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem =
                new GameObject("Prototype Event System");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }

    internal static class PrototypeTextCompatibility
    {
        public static void letterSpacingCompat(
            this Text text,
            float spacing)
        {
            // Legacy UI Text has no letter spacing setting.
            // Kept as a named no-op for the prototype style API.
        }
    }
}
