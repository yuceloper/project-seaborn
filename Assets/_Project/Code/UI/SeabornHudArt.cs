using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.UI
{
    // Rects are measured in source pixels, TOP-LEFT coordinates. Never rescale
    // these atlases in the importer without updating the measured bounds.
    public static class SeabornHudArt
    {
        private static readonly Dictionary<string, Sprite> cache = new();
        private static Texture2D maskTexture;
        public static Sprite Frame(int index)
        {
            RectInt[] bounds = {
                new(44,90,348,346), new(444,66,411,403),
                new(886,204,425,123), new(1340,204,422,120),
                new(58,498,324,317), new(466,470,381,376),
                new(886,628,427,69), new(1340,600,416,128)
            };
            return Slice("Frames", index, bounds[index]);
        }

        public static Sprite Icon(int index)
        {
            RectInt[] bounds = {
                new(56,87,280,274), new(435,56,305,309),
                new(833,80,313,297), new(1209,43,344,337),
                new(1608,36,337,346), new(108,430,180,318),
                new(496,407,196,341), new(865,427,249,322),
                new(1248,453,283,292), new(1645,424,271,327)
            };
            return Slice("Icons", index, bounds[index]);
        }

        public static Sprite Glyph(int index)
        {
            RectInt[] bounds = {
                new(94,104,358,352), new(587,103,376,353),
                new(1070,82,404,406), new(79,514,391,384),
                new(563,568,416,319), new(1067,512,421,413)
            };
            return Slice("Glyphs", index, bounds[index]);
        }

        private static Sprite Slice(string atlas, int index, RectInt bounds)
        {
            string key = atlas + index;
            if (cache.TryGetValue(key, out Sprite sprite) && sprite != null) return sprite;
            Texture2D texture = Resources.Load<Texture2D>("SeabornHud/" + atlas);
            if (texture == null)
            {
                Debug.LogError("Seaborn HUD atlas missing: " + atlas);
                return null;
            }
            Rect rect = new(bounds.x, texture.height - bounds.yMax, bounds.width, bounds.height);
            sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = "Seaborn " + key;
            cache[key] = sprite;
            return sprite;
        }

        public static Sprite CircleMask
        {
            get
            {
                if (cache.TryGetValue("Mask", out Sprite sprite) && sprite != null) return sprite;
                const int size = 128;
                maskTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                maskTexture.wrapMode = TextureWrapMode.Clamp;
                Color32[] pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * size * 0.5f);
                        byte alpha = (byte)(Mathf.Clamp01(size * 0.5f - distance) * 255f);
                        pixels[y * size + x] = new Color32(255,255,255,alpha);
                    }
                maskTexture.SetPixels32(pixels);
                maskTexture.Apply(false, true);
                sprite = Sprite.Create(maskTexture, new Rect(0,0,size,size), Vector2.one * 0.5f);
                cache["Mask"] = sprite;
                return sprite;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            foreach (Sprite sprite in cache.Values)
                if (sprite != null) Object.Destroy(sprite);
            cache.Clear();
            if (maskTexture != null) Object.Destroy(maskTexture);
            maskTexture = null;
        }
    }
}
