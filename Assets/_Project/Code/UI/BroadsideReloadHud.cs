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
        private static readonly Color Muted = new(0.44f, 0.57f, 0.55f, 1f);
        private static readonly Color Dark = new(0.025f, 0.12f, 0.13f, 0.94f);

        private BroadsideController broadside;
        private CanvasGroup group;
        private ReloadDial portDial;
        private ReloadDial starboardDial;
        private float nextBindTime;
        private Texture2D ringTexture;
        private Sprite ringSprite;
        private Font font;

        private sealed class ReloadDial
        {
            public Image Progress;
            public Text Time;
            public Text State;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCreated()
        {
            BroadsideReloadHud existing =
                FindFirstObjectByType<BroadsideReloadHud>();
            if (existing != null) return;

            GameObject root = new("Broadside Reload HUD");
            root.AddComponent<BroadsideReloadHud>();
        }

        private void Awake()
        {
            BuildInterface();
            TryBind();
        }

        private void Update()
        {
            bool harborUiOpen = PrototypeHarborUiCoordinator.IsOpen;
            if (group != null)
            {
                group.alpha = harborUiOpen ? 0f : 1f;
            }

            if (harborUiOpen) return;

            if (broadside == null && Time.unscaledTime >= nextBindTime)
            {
                nextBindTime = Time.unscaledTime + 0.5f;
                TryBind();
            }

            RefreshDial(portDial, BroadsideSide.Port);
            RefreshDial(starboardDial, BroadsideSide.Starboard);
        }

        private void TryBind()
        {
            HarpoonHuntingController player =
                FindFirstObjectByType<HarpoonHuntingController>();
            if (player == null) return;

            broadside = player.transform.root.GetComponentInChildren<BroadsideController>();
        }

        private void BuildInterface()
        {
            group = gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 61;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            font = Resources.Load<Font>("SeabornHud/DejaVuSerif");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            ringSprite = CreateRingSprite();
            portDial = CreateDial("İSKELE", new Vector2(-552f, 82f));
            starboardDial = CreateDial("SANCAK", new Vector2(552f, 82f));
        }

        private ReloadDial CreateDial(string label, Vector2 position)
        {
            GameObject root = new(label + " Reload", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(92f, 104f);

            Image backdrop = CreateImage(rect, "Ring Backdrop", ringSprite,
                new Vector2(0f, 42f), new Vector2(72f, 72f));
            backdrop.color = new Color(0.18f, 0.24f, 0.22f, 0.72f);

            Image progress = CreateImage(rect, "Reload Progress", ringSprite,
                new Vector2(0f, 42f), new Vector2(72f, 72f));
            progress.type = Image.Type.Filled;
            progress.fillMethod = Image.FillMethod.Radial360;
            progress.fillOrigin = 2;
            progress.fillClockwise = true;
            progress.fillAmount = 1f;
            progress.color = Gold;

            Image center = CreateImage(rect, "Medallion", SeabornHudArt.CircleMask,
                new Vector2(0f, 42f), new Vector2(54f, 54f));
            center.color = Dark;

            Image cannon = CreateImage(rect, "Cannon", SeabornHudArt.Icon(0),
                new Vector2(0f, 47f), new Vector2(31f, 31f));
            cannon.preserveAspect = true;
            cannon.color = Cream;

            Text time = CreateText(rect, "", 11, Gold,
                new Vector2(0f, 25f), new Vector2(58f, 18f), FontStyle.Bold);
            Text state = CreateText(rect, label, 11, Cream,
                new Vector2(0f, 2f), new Vector2(90f, 18f), FontStyle.Normal);

            return new ReloadDial
            {
                Progress = progress,
                Time = time,
                State = state
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
                return;
            }

            float progress = broadside.GetReloadProgress(side);
            float remaining = broadside.GetCooldownRemaining(side);
            bool ready = progress >= 0.999f;

            dial.Progress.fillAmount = progress;
            if (ready)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
                dial.Progress.color = Color.Lerp(Gold, Cream, pulse * 0.28f);
                dial.Time.text = "HAZIR";
                dial.Time.color = Cream;
            }
            else
            {
                dial.Progress.color = Gold;
                dial.Time.text = remaining >= 1f
                    ? $"{remaining:0.0}s"
                    : $"{remaining:0.0}s";
                dial.Time.color = Gold;
            }
        }

        private Image CreateImage(RectTransform parent, string name, Sprite sprite,
            Vector2 position, Vector2 size)
        {
            GameObject item = new(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            Image image = item.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
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
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = bounds;
            return text;
        }

        private Sprite CreateRingSprite()
        {
            const int size = 128;
            const float outer = 62f;
            const float inner = 52f;
            ringTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Broadside Reload Ring",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels = new Color32[size * size];
            Vector2 center = Vector2.one * size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x + 0.5f, y + 0.5f), center);
                    float outerAlpha = Mathf.Clamp01(outer - distance + 1f);
                    float innerAlpha = Mathf.Clamp01(distance - inner + 1f);
                    byte alpha = (byte)(outerAlpha * innerAlpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            ringTexture.SetPixels32(pixels);
            ringTexture.Apply(false, true);
            Sprite sprite = Sprite.Create(
                ringTexture,
                new Rect(0f, 0f, size, size),
                Vector2.one * 0.5f,
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "Broadside Reload Ring";
            return sprite;
        }

        private void OnDestroy()
        {
            if (ringSprite != null) Destroy(ringSprite);
            if (ringTexture != null) Destroy(ringTexture);
        }
    }
}
