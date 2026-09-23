using System.Text;
using Seaborn.Expeditions;
using Seaborn.Harbor.UI;
using Seaborn.Progression;
using Seaborn.World;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeExpeditionReportPanel : MonoBehaviour
    {
        private PrototypeExpeditionReportTracker tracker;
        private GameObject canvasRoot;
        private GameObject panel;
        private GameObject reopen;
        private Text heading;
        private Text details;
        private Text net;
        private Text note;
        private Font font;
        private bool expanded;

        public static void EnsureAttached(PrototypeExpeditionReportTracker tracker)
        {
            if (tracker.GetComponent<PrototypeExpeditionReportPanel>() != null) return;
            var view = tracker.gameObject.AddComponent<PrototypeExpeditionReportPanel>();
            view.tracker = tracker;
            view.Build();
            tracker.ReportReady += view.ShowReport;
        }

        private void Update()
        {
            if (canvasRoot == null || tracker == null) return;
            bool visible = tracker.LastReport != null && !tracker.IsRecording &&
                PrototypeExpeditionRegionDirector.IsHarborScene &&
                !PrototypeHarborUiCoordinator.IsOpen;
            canvasRoot.SetActive(visible);
            panel.SetActive(expanded);
            reopen.SetActive(!expanded);
        }

        private void ShowReport()
        {
            ExpeditionReport report = tracker.LastReport;
            var ledger = report.Ledger;
            int seconds = Mathf.FloorToInt(report.Duration);
            heading.text = report.Sunk ? "SEFER KAYDI · GEMİ BATTI" : "SEFER TAMAMLANDI";
            StringBuilder text = new();
            text.AppendLine($"Denizde geçen süre                    {seconds / 60:00}:{seconds % 60:00}");
            text.AppendLine();
            text.AppendLine($"Teslim edilen yük                       {ledger.SilverSecured} Silver");
            text.AppendLine($"Diğer Silver ödülleri                  {Mathf.Max(0, ledger.SilverEarned - ledger.SilverSecured)} Silver");
            text.AppendLine($"Cüzdana giren toplam                 {ledger.SilverEarned} Silver");
            text.AppendLine($"Kaybedilen yük                           {ledger.SilverLost} Silver");
            text.AppendLine();
            text.AppendLine($"Gülle / zincir / saçma                   {ledger.StandardUsed} / {ledger.ChainUsed} / {ledger.GrapeshotUsed}");
            text.AppendLine($"Kullanılan zıpkın                          {ledger.HarpoonsUsed}");
            text.AppendLine($"Mühimmat ikmal değeri                {ledger.AmmunitionValue:0.0} Silver");
            text.AppendLine(report.Sunk ? "Onarım / kurtarma                       Hesaba dahil değil"
                : $"Seferin onarım payı                      {report.RepairCost} Silver");
            text.AppendLine();
            text.AppendLine("ELDE EDİLEN MALZEMELER");
            bool any = false;
            for (int i = 0; i < report.Materials.Length; i++)
            {
                if (report.Materials[i] <= 0) continue;
                any = true;
                text.AppendLine($"{PrototypeRegionalLootInventory.DisplayName((RegionalMaterialType)i)}  ×{report.Materials[i]}");
            }
            if (!any) text.AppendLine("Bu seferde malzeme kazanılmadı.");
            details.text = text.ToString();
            net.text = $"TAHMİNİ NET     {report.EstimatedNet:+0.0;-0.0;0.0} Silver";
            net.color = report.EstimatedNet >= 0 ? new Color(0.17f, 0.37f, 0.29f)
                : new Color(0.60f, 0.21f, 0.16f);
            note.text = "Mühimmat, kullanılan adetlerin birim ikmal değeridir; paket satın alma tutarı değildir. " +
                "Onarım payı, dönüş ve çıkış tekliflerinin pozitif farkıdır. " +
                "Sarf malzemeleri ve kurtarma giderleri hariçtir. Bu özet Silver harcamaz.";
            expanded = true;
        }

        private void Build()
        {
            font = Resources.Load<Font>("SeabornHud/DejaVuSerif");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvasRoot = new GameObject("Expedition Report Canvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(transform, false);
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 190;
            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            panel = new GameObject("Sefer Özeti", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasRoot.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(610f, 720f);
            SeabornUiSkin.Style(panel.GetComponent<Image>(), SeabornUiSurface.Parchment);
            panel.GetComponent<Image>().raycastTarget = true;
            heading = Label(panel.transform, new Vector2(28f, -24f), new Vector2(554f, 42f), 24, FontStyle.Bold);
            heading.color = SeabornUiSkin.Ink;
            details = Label(panel.transform, new Vector2(28f, -88f), new Vector2(554f, 420f), 18, FontStyle.Normal);
            net = Label(panel.transform, new Vector2(28f, -520f), new Vector2(554f, 42f), 25, FontStyle.Bold);
            note = Label(panel.transform, new Vector2(28f, -572f), new Vector2(554f, 86f), 14, FontStyle.Normal);
            Button close = MakeButton(panel.transform, "KAPAT", new Vector2(202f, -660f), new Vector2(206f, 38f));
            close.onClick.AddListener(() => expanded = false);

            Button open = MakeButton(canvasRoot.transform, "SON SEFER", new Vector2(-240f, -120f), new Vector2(208f, 42f));
            RectTransform openRect = open.GetComponent<RectTransform>();
            openRect.anchorMin = openRect.anchorMax = new Vector2(1f, 1f);
            reopen = open.gameObject;
            open.onClick.AddListener(() => expanded = true);
            canvasRoot.SetActive(false);
        }

        private Text Label(Transform parent, Vector2 position, Vector2 size, int fontSize, FontStyle style)
        {
            var item = new GameObject("Label", typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = item.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = SeabornUiSkin.Ink;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Button MakeButton(Transform parent, string title, Vector2 position, Vector2 size)
        {
            var item = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = item.GetComponent<Image>();
            SeabornUiSkin.Style(image, SeabornUiSurface.Enamel);
            image.raycastTarget = true;
            Button button = item.GetComponent<Button>();
            button.targetGraphic = image;
            Text label = Label(item.transform, Vector2.zero, size, 16, FontStyle.Bold);
            label.text = title;
            label.color = SeabornUiSkin.Ivory;
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private void OnDestroy()
        {
            if (tracker != null) tracker.ReportReady -= ShowReport;
            if (canvasRoot != null) Destroy(canvasRoot);
        }
    }
}
