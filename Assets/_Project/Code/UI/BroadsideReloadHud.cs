using Seaborn.Combat;
using Seaborn.Harbor.UI;
using Seaborn.Hunting;
using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    [DisallowMultipleComponent]
    public sealed class BroadsideReloadHud : MonoBehaviour
    {
        private static readonly Color Gold = new(0.88f, 0.70f, 0.40f, 1f);
        private static readonly Color Cream = new(0.92f, 0.90f, 0.80f, 1f);
        private static readonly Color Muted = new(0.38f, 0.50f, 0.49f, 1f);
        private static readonly Color Dark = new(0.025f, 0.12f, 0.13f, 0.96f);
        private static readonly Color Track = new(0.20f, 0.30f, 0.29f, 0.92f);

        private BroadsideController broadside;
        private ReloadDial portDial;
        private ReloadDial starboardDial;
        private RectTransform combatRoot;
        private float nextBindTime;
        private Font font;

        private sealed class ReloadDial
        {
            public RectTransform Root;
            public Image Progress;
            public Text Time;
            public Text Label;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCreated()
        {
            BroadsideReloadHud existing = FindFirstObjectByType<BroadsideReloadHud>();
            if (existing != null) return;

            GameObject root = new("Broadside Reload HUD");
            root.AddComponent<BroadsideReloadHud>();
        }

        private void Awake()
        {
            font = Resources.Load<Font>("SeabornHud/DejaVuSerif");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            TryBind();
        }

        private void Update()
        {
            if (PrototypeHarborUiCoordinator.IsOpen)
            {
                SetVisible(false);
                return;
            }

            if ((broadside == null || combatRoot == null || portDial == null) &&
                Time.unscaledTime >= nextBindTime)
            {
                nextBindTime = Time.unscaledTime + 0.25f;
                TryBind();
            }

            SetVisible(combatRoot != null);
            RefreshDial(portDial, BroadsideSide.Port);
            RefreshDial(starboardDial, BroadsideSide.Starboard);
        }

        private void TryBind()
        {
            HarpoonHuntingController player = FindFirstObjectByType<HarpoonHuntingController>();
            if (player != null)
                broadside = player.transform.root.GetComponentInChildren<BroadsideController>();

            PrototypeGameplayHud hud = FindFirstObjectByType<PrototypeGameplayHud>();
            if (hud == null) return;

            Transform combat = hud.transform.Find("Combat");
            if (combat == null) return;

            RectTransform nextCombatRoot = combat as RectTransform;
            if (nextCombatRoot == null) return;

            if (combatRoot == nextCombatRoot && portDial != null) return;
            combatRoot = nextCombatRoot;

            RemoveOldDials();
            HideLegacyReloadBars();

            portDial = CreateDial("İSKELE", new Vector2(-62f, -65f));
            starboardDial = CreateDial("SANCAK", new Vector2(1042f, -65f));
        }

        private void HideLegacyReloadBars()
        {
            Transform portBar = combatRoot.Find("Port Background");
            Transform starboardBar = combatRoot.Find("Starboard Background");
            if (portBar != null) portBar.gameObject.SetActive(false);
            if (starboardBar != null) starboardBar.gameObject.SetActive(false);

            // The two legacy readiness texts were anonymous direct children.
            // Identify only those by their known layout coordinates so item labels
            // and hotbar text remain untouched.
            for (int i = 0; i < combatRoot.childCount; i++)
            {
                RectTransform child = combatRoot.GetChild(i) as RectTransform;
                if (child == null) continue;
                Text text = child.GetComponent<Text>();
                if (text == null) continue;

                Vector2 p = child.anchoredPosition;
                bool portLegacy = p.x >= 0f && p.x <= 20f && p.y > -12f && p.y < 8f;
                bool starboardLegacy = p.x >= 800f && p.x <= 840f && p.y > -12f && p.y < 8f;
                if (portLegacy || starboardLegacy)
                    child.gameObject.SetActive(false);
            }
        }

        private ReloadDial CreateDial(string label, Vector2 position)
        {
            GameObject root = new(label + " Radial Reload", typeof(RectTransform));
            root.transform.SetParent(combatRoot, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(94f, 112f);

            Image track = CreateImage(rect, "Reload Track", SeabornHudArt.CircleMask,
                new Vector2(47f, -43f), new Vector2(82f, 82f));
            track.color = Track;

            Image progress = CreateImage(rect, "Reload Progress", SeabornHudArt.CircleMask,
                new Vector2(47f, -43f), new Vector2(82f, 82f));
            progress.type = Image.Type.Filled;
            progress.fillMethod = Image.FillMethod.Radial360;
            progress.fillOrigin = 2;
            progress.fillClockwise = true;
            progress.fillAmount = 1f;
            progress.color = Gold;

            Image inner = CreateImage(rect, "Reload Inner", SeabornHudArt.CircleMask,
                new Vector2(47f, -43f), new Vector2(64f, 64f));
            inner.color = Dark;

            Image cannonball = CreateImage(rect, "Cannon Icon", SeabornHudArt.Icon(0),
                new Vector2(47f, -37f), new Vector2(34f, 34f));
            cannonball.preserveAspect = true;
            cannonball.color = Cream;

            Text time = CreateText(rect, "HAZIR", 11, Cream,
                new Vector2(47f, -61f), new Vector2(62f, 18f), FontStyle.Bold);
            Text state = CreateText(rect, label, 11, Cream,
                new Vector2(47f, -96f), new Vector2(94f, 18f), FontStyle.Normal);

            return new ReloadDial
            {
                Root = rect,
                Progress = progress,
                Time = time,
                Label = state
            };
        }

        private void RefreshDial(ReloadDial dial, BroadsideSide side)
        {
            if (dial == null) return;

            if (broadside == null)
            {
                dial.Progress.fillAmount = 0f;
                dial.Progress.color = Muted;
                dial.Time.text = "—";
                dial.Time.color = Muted;
                return;
            }

            float progress = broadside.GetReloadProgress(side);
            float remaining = broadside.GetCooldownRemaining(side);
            bool ready = progress >= 0.999f;

            dial.Progress.fillAmount = progress;
            if (ready)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
                dial.Progress.color = Color.Lerp(Gold, Cream, pulse * 0.30f);
                dial.Time.text = "HAZIR";
                dial.Time.color = Cream;
                dial.Root.localScale = Vector3.one * Mathf.Lerp(1f, 1.035f, pulse * 0.45f);
            }
            else
            {
                dial.Progress.color = Gold;
                dial.Time.text = $"{remaining:0.0}s";
                dial.Time.color = Gold;
                dial.Root.localScale = Vector3.one;
            }
        }

        private static Image CreateImage(RectTransform parent, string name, Sprite sprite,
            Vector2 position, Vector2 size)
        {
            GameObject item = new(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            Image image = item.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return image;
        }

        private Text CreateText(RectTransform parent, string value, int size,
            Color color, Vector2 position, Vector2 bounds, FontStyle style)
        {
            GameObject item = new("Text", typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            Text text = item.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position - bounds * 0.5f;
            rect.sizeDelta = bounds;
            return text;
        }

        private void SetVisible(bool visible)
        {
            if (portDial?.Root != null) portDial.Root.gameObject.SetActive(visible);
            if (starboardDial?.Root != null) starboardDial.Root.gameObject.SetActive(visible);
        }

        private void RemoveOldDials()
        {
            if (portDial?.Root != null) Destroy(portDial.Root.gameObject);
            if (starboardDial?.Root != null) Destroy(starboardDial.Root.gameObject);
            portDial = null;
            starboardDial = null;
        }
    }
}
