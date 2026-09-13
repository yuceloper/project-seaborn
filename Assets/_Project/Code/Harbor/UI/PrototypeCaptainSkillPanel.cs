using System.Collections.Generic;
using Seaborn.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.Harbor.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCaptainSkillPanel : MonoBehaviour
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

        private readonly Dictionary<CaptainSkill, SkillRow> rows =
            new();

        private Transform player;
        private PrototypeCaptainProgression progression;
        private PrototypeCaptainSkills skills;
        private GameObject panel;
        private Text summaryText;
        private Text statusText;
        private float nextRefreshTime;
        private float statusExpiresAt;

        private sealed class SkillRow
        {
            public Text Description;
            public Text Rank;
            public Button Spend;
        }

        public static void EnsureCreated(Transform player)
        {
            if (player == null) return;

            PrototypeCaptainSkillPanel existing =
                FindFirstObjectByType<PrototypeCaptainSkillPanel>();
            if (existing == null)
            {
                GameObject root =
                    new("Prototype Captain Skill Panel");
                existing =
                    root.AddComponent<PrototypeCaptainSkillPanel>();
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
            bool visible =
                skills != null &&
                skills.CanSpendAtHarborOffice;

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

            progression ??= player.GetComponentInChildren<
                PrototypeCaptainProgression>();
            skills ??= player.GetComponentInChildren<
                PrototypeCaptainSkills>();
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 83;

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
                "Captain Skills Panel",
                Navy
            );
            RectTransform rect =
                panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition =
                new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(500f, 680f);

            GameObject accent = CreateBlock(
                rect,
                "Accent",
                Gold
            );
            RectTransform accentRect =
                accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.sizeDelta = new Vector2(0f, 4f);

            CreateText(
                rect,
                "KAPTAN YETENEKLERİ",
                23,
                Gold,
                new Vector2(22f, -20f),
                new Vector2(456f, 32f),
                FontStyle.Bold
            );
            summaryText = CreateText(
                rect,
                "SV.1  •  0 YETENEK PUANI",
                14,
                Cream,
                new Vector2(22f, -58f),
                new Vector2(456f, 24f),
                FontStyle.Bold
            );
            statusText = CreateText(
                rect,
                "Her iki seviyede bir yetenek puanı kazanılır.",
                13,
                Muted,
                new Vector2(22f, -86f),
                new Vector2(456f, 38f)
            );

            AddBranch(
                rect,
                "TOPÇULUK",
                132f,
                CaptainSkill.CannonMastery,
                "Top Ustalığı",
                CaptainSkill.Rangefinding,
                "Menzil Hesabı"
            );
            AddBranch(
                rect,
                "ZIPKINCILIK",
                302f,
                CaptainSkill.HarpoonMastery,
                "Zıpkın Ustalığı",
                CaptainSkill.HarpoonRigging,
                "Hızlı Donanım"
            );
            AddBranch(
                rect,
                "GEMİ",
                472f,
                CaptainSkill.ReinforcedHull,
                "Güçlendirilmiş Omurga",
                CaptainSkill.FineSails,
                "İnce Yelkenler"
            );

            CreateText(
                rect,
                "Puanlar kalıcıdır. Yeniden dağıtım daha sonra eklenecek.  •  E ile ayrıl",
                12,
                Muted,
                new Vector2(22f, -646f),
                new Vector2(456f, 22f),
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );

            panel.SetActive(false);
        }

        private void AddBranch(
            RectTransform root,
            string title,
            float y,
            CaptainSkill first,
            string firstName,
            CaptainSkill second,
            string secondName)
        {
            CreateText(
                root,
                title,
                13,
                Gold,
                new Vector2(22f, -y),
                new Vector2(456f, 22f),
                FontStyle.Bold
            );
            AddSkillRow(root, first, firstName, y + 28f);
            AddSkillRow(root, second, secondName, y + 92f);
        }

        private void AddSkillRow(
            RectTransform root,
            CaptainSkill skill,
            string title,
            float y)
        {
            GameObject row = CreateBlock(
                root,
                title,
                NavyLight
            );
            RectTransform rowRect =
                row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(0f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.anchoredPosition =
                new Vector2(22f, -y);
            rowRect.sizeDelta = new Vector2(456f, 56f);

            CreateText(
                rowRect,
                title,
                13,
                Cream,
                new Vector2(12f, -7f),
                new Vector2(230f, 20f),
                FontStyle.Bold
            );
            Text description = CreateText(
                rowRect,
                "",
                11,
                Muted,
                new Vector2(12f, -29f),
                new Vector2(300f, 19f)
            );
            Text rank = CreateText(
                rowRect,
                "0/0",
                12,
                Cream,
                new Vector2(315f, -18f),
                new Vector2(55f, 22f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );

            GameObject buttonObject = CreateBlock(
                rowRect,
                "Spend",
                Gold
            );
            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition =
                new Vector2(386f, -10f);
            buttonRect.sizeDelta = new Vector2(56f, 36f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic =
                buttonObject.GetComponent<Image>();
            CaptainSkill captured = skill;
            button.onClick.AddListener(
                () => Spend(captured)
            );
            Text label = CreateText(
                buttonRect,
                "+1",
                14,
                Navy,
                Vector2.zero,
                new Vector2(56f, 36f),
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            RectTransform labelRect =
                label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            rows.Add(
                skill,
                new SkillRow
                {
                    Description = description,
                    Rank = rank,
                    Spend = button
                }
            );
        }

        private void Refresh()
        {
            ResolveBindings();
            if (panel == null || skills == null)
                return;

            int level =
                progression != null ? progression.Level : 1;
            summaryText.text =
                $"SV.{level}  •  {skills.AvailablePoints} " +
                "YETENEK PUANI";

            foreach (KeyValuePair<CaptainSkill, SkillRow> pair
                     in rows)
            {
                CaptainSkill skill = pair.Key;
                SkillRow row = pair.Value;
                int rank = skills.GetRank(skill);
                int maximum = skills.GetMaximumRank(skill);
                bool prerequisite =
                    skills.MeetsPrerequisite(skill);

                row.Rank.text = $"{rank}/{maximum}";
                row.Description.text =
                    prerequisite
                        ? skills.EffectDescription(skill)
                        : PrerequisiteText(skill);
                row.Description.color =
                    prerequisite ? Muted : Gold;
                row.Spend.interactable =
                    skills.AvailablePoints > 0 &&
                    prerequisite &&
                    rank < maximum;
            }

            if (statusExpiresAt > 0f &&
                Time.unscaledTime >= statusExpiresAt)
            {
                statusExpiresAt = 0f;
                statusText.text =
                    skills.AvailablePoints > 0
                        ? "Bir dal seç ve kaptanını uzmanlaştır."
                        : "Yeni puan için kaptan seviyeni yükselt.";
                statusText.color = Muted;
            }
        }

        private void Spend(CaptainSkill skill)
        {
            if (skills == null) return;

            SkillSpendResult result = skills.TrySpend(skill);
            statusText.text = result switch
            {
                SkillSpendResult.Completed =>
                    "Yetenek geliştirildi.",
                SkillSpendResult.NoSkillPoints =>
                    "Kullanılabilir yetenek puanın yok.",
                SkillSpendResult.MaximumRank =>
                    "Bu yetenek azami seviyede.",
                SkillSpendResult.PrerequisiteMissing =>
                    "Önce dalın ilk yeteneğini aç.",
                _ => "Yetenekler Liman İdaresi'nde geliştirilir."
            };
            statusText.color =
                result == SkillSpendResult.Completed
                    ? Success
                    : Gold;
            statusExpiresAt = Time.unscaledTime + 3f;
            Refresh();
        }

        private static string PrerequisiteText(
            CaptainSkill skill)
        {
            return skill switch
            {
                CaptainSkill.Rangefinding =>
                    "GEREKİR: Top Ustalığı 1",
                CaptainSkill.HarpoonRigging =>
                    "GEREKİR: Zıpkın Ustalığı 1",
                CaptainSkill.FineSails =>
                    "GEREKİR: Güçlendirilmiş Omurga 1",
                _ => ""
            };
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
