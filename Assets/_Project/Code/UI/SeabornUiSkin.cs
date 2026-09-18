using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    public enum SeabornUiSurface
    {
        Enamel,
        Parchment,
        Slot,
        SlotSelected,
        Chart
    }

    // Runtime-authored sprites keep the prototype self-contained while giving
    // every HUD surface one consistent, scalable maritime material language.
    public static class SeabornUiSkin
    {
        public static readonly Color Ivory = new(0.91f, 0.88f, 0.80f, 1f);
        public static readonly Color Ink = new(0.10f, 0.16f, 0.16f, 1f);
        public static readonly Color InkMuted = new(0.27f, 0.34f, 0.33f, 1f);
        public static readonly Color Brass = new(0.70f, 0.52f, 0.27f, 1f);
        public static readonly Color Teal = new(0.055f, 0.20f, 0.21f, 0.98f);
        public static readonly Color TealRaised = new(0.075f, 0.27f, 0.27f, 0.98f);

        private static Sprite enamel;
        private static Sprite parchment;
        private static Sprite slot;
        private static Sprite slotSelected;
        private static Sprite chart;
        private static Sprite track;

        public static Sprite Surface(SeabornUiSurface kind)
        {
            EnsureCreated();
            return kind switch
            {
                SeabornUiSurface.Parchment => parchment,
                SeabornUiSurface.Slot => slot,
                SeabornUiSurface.SlotSelected => slotSelected,
                SeabornUiSurface.Chart => chart,
                _ => enamel
            };
        }

        public static Sprite Track
        {
            get
            {
                EnsureCreated();
                return track;
            }
        }

        public static void Style(Image image, SeabornUiSurface surface)
        {
            if (image == null) return;
            image.sprite = Surface(surface);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private static void EnsureCreated()
        {
            if (enamel != null) return;
            enamel = Build("HUD Enamel", Teal, TealRaised, Brass, 7, false);
            parchment = Build("HUD Parchment",
                new Color(0.82f, 0.77f, 0.65f, 1f),
                new Color(0.94f, 0.90f, 0.80f, 1f),
                new Color(0.48f, 0.36f, 0.21f, 1f), 7, true);
            slot = Build("HUD Slot",
                new Color(0.045f, 0.15f, 0.16f, 0.99f),
                new Color(0.075f, 0.25f, 0.25f, 0.99f),
                new Color(0.35f, 0.40f, 0.35f, 1f), 5, false);
            slotSelected = Build("HUD Selected Slot",
                new Color(0.09f, 0.23f, 0.22f, 1f),
                new Color(0.14f, 0.32f, 0.29f, 1f),
                new Color(0.92f, 0.70f, 0.31f, 1f), 5, false);
            chart = Build("HUD Chart",
                new Color(0.035f, 0.18f, 0.20f, 0.96f),
                new Color(0.06f, 0.29f, 0.30f, 0.96f),
                Brass, 12, false);
            track = Build("HUD Track",
                new Color(0.04f, 0.12f, 0.13f, 1f),
                new Color(0.08f, 0.25f, 0.25f, 1f),
                new Color(0.36f, 0.39f, 0.34f, 1f), 3, false);
        }

        private static Sprite Build(string name, Color outer, Color inner,
            Color edge, int chamfer, bool paper)
        {
            const int size = 64;
            const int border = 6;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int nearestX = Mathf.Min(x, size - 1 - x);
                    int nearestY = Mathf.Min(y, size - 1 - y);
                    bool cutCorner = nearestX + nearestY < chamfer;
                    if (cutCorner)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    int edgeDistance = Mathf.Min(nearestX, nearestY);
                    Color value;
                    if (edgeDistance <= 1)
                        value = new Color(0.025f, 0.06f, 0.065f, 0.95f);
                    else if (edgeDistance <= 3)
                        value = edge;
                    else if (edgeDistance < border)
                        value = outer;
                    else
                    {
                        float vertical = y / (float)(size - 1);
                        value = Color.Lerp(outer, inner,
                            paper ? 0.55f + vertical * 0.35f : 0.28f + vertical * 0.38f);
                        float grain = Hash(x, y) * (paper ? 0.035f : 0.018f);
                        value = new Color(
                            Mathf.Clamp01(value.r + grain),
                            Mathf.Clamp01(value.g + grain),
                            Mathf.Clamp01(value.b + grain), value.a);
                    }
                    pixels[y * size + x] = value;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(12f, 12f, 12f, 12f));
            sprite.name = name;
            return sprite;
        }

        private static float Hash(int x, int y)
        {
            uint value = (uint)(x * 374761393 + y * 668265263);
            value = (value ^ (value >> 13)) * 1274126177u;
            return ((value ^ (value >> 16)) & 255u) / 255f - 0.5f;
        }
    }
}
