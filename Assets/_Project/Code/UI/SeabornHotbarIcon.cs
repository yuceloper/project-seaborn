using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    // Small vector silhouettes: no font glyph or texture import dependency.
    [DisallowMultipleComponent]
    public sealed class SeabornHotbarIcon : MaskableGraphic
    {
        public int Kind { get; set; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (Kind)
            {
                case 0:
                    Disc(vh, 0.5f, 0.5f, 0.32f);
                    break;
                case 1:
                    Disc(vh, 0.22f, 0.28f, 0.18f);
                    Stroke(vh, new Vector2(0.25f, 0.3f), new Vector2(0.75f, 0.7f), 0.08f);
                    Disc(vh, 0.78f, 0.72f, 0.18f);
                    break;
                case 2:
                    Disc(vh, 0.28f, 0.3f, 0.16f);
                    Disc(vh, 0.72f, 0.3f, 0.16f);
                    Disc(vh, 0.5f, 0.7f, 0.16f);
                    break;
                case 3:
                case 4:
                    Stroke(vh, new Vector2(0.2f, 0.15f), new Vector2(0.73f, 0.75f), Kind == 4 ? 0.13f : 0.07f);
                    Polygon(vh, new Vector2(0.9f, 0.95f), new Vector2(0.48f, 0.78f), new Vector2(0.76f, 0.5f));
                    break;
                case 5:
                    Bottle(vh);
                    // Cross, drawn as negative-space-like dark inset.
                    Box(vh, 0.45f, 0.23f, 0.1f, 0.35f, new Color(0.12f, 0.17f, 0.18f, 1f));
                    Box(vh, 0.33f, 0.35f, 0.34f, 0.1f, new Color(0.12f, 0.17f, 0.18f, 1f));
                    break;
                case 6:
                    Polygon(vh, new Vector2(0.5f, 0.97f), new Vector2(0.23f, 0.64f), new Vector2(0.77f, 0.64f));
                    Box(vh, 0.25f, 0.18f, 0.5f, 0.4f, color);
                    Box(vh, 0.18f, 0.09f, 0.64f, 0.07f, color);
                    break;
                case 7:
                    Bottle(vh);
                    Box(vh, 0.27f, 0.31f, 0.46f, 0.11f, new Color(0.12f, 0.17f, 0.18f, 1f));
                    break;
                case 8:
                    Stroke(vh, new Vector2(0.1f, 0.7f), new Vector2(0.85f, 0.7f), 0.08f);
                    Stroke(vh, new Vector2(0.22f, 0.5f), new Vector2(0.72f, 0.5f), 0.08f);
                    Stroke(vh, new Vector2(0.1f, 0.3f), new Vector2(0.58f, 0.3f), 0.08f);
                    break;
                default:
                    Polygon(vh, new Vector2(0.15f, 0.87f), new Vector2(0.15f, 0.42f),
                        new Vector2(0.5f, 0.08f), new Vector2(0.85f, 0.42f), new Vector2(0.85f, 0.87f));
                    break;
            }
        }

        private void Bottle(VertexHelper vh)
        {
            Box(vh, 0.38f, 0.7f, 0.24f, 0.22f, color);
            Polygon(vh, new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.12f),
                new Vector2(0.75f, 0.6f), new Vector2(0.6f, 0.73f),
                new Vector2(0.4f, 0.73f), new Vector2(0.25f, 0.6f));
        }

        private void Disc(VertexHelper vh, float x, float y, float radius)
        {
            Vector2[] points = new Vector2[20];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = -i * Mathf.PI * 2f / points.Length;
                points[i] = new Vector2(x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius);
            }
            Polygon(vh, points);
        }

        private void Stroke(VertexHelper vh, Vector2 a, Vector2 b, float width)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
            Polygon(vh, a + normal, b + normal, b - normal, a - normal);
        }

        private void Box(VertexHelper vh, float x, float y, float w, float h, Color tint)
        {
            AddPolygon(vh, tint, new Vector2(x, y), new Vector2(x, y + h),
                new Vector2(x + w, y + h), new Vector2(x + w, y));
        }

        private void Polygon(VertexHelper vh, params Vector2[] points) => AddPolygon(vh, color, points);

        private void AddPolygon(VertexHelper vh, Color tint, params Vector2[] points)
        {
            Rect rect = rectTransform.rect;
            int first = vh.currentVertCount;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p = points[i];
                vh.AddVert(new Vector3(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height, 0f), tint, Vector2.zero);
            }
            for (int i = 1; i < points.Length - 1; i++) vh.AddTriangle(first, first + i, first + i + 1);
        }
    }
}
