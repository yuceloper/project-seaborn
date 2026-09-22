using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Seaborn.World
{
    public sealed class MapGatewayApproachView : MonoBehaviour
    {
        private static MapGatewayApproachView instance;
        private static int closedFrame = -1;
        public static bool BlocksPointerInput => closedFrame == Time.frameCount ||
            (instance != null && instance.card != null && instance.card.gameObject.activeInHierarchy &&
             Mouse.current != null && RectTransformUtility.RectangleContainsScreenPoint(
                 instance.card, Mouse.current.position.ReadValue()));

        private RectTransform card;
        private Text mapName;
        private Text status;
        private Button confirm;
        private RawImage mist;
        private Texture2D mistTexture;
        private float targetFog;
        private float currentFog;

        public void Configure(Action onConfirm, Action onCancel)
        {
            instance = this;
            var fogCanvas = Canvas("Gateway Mist", 60);
            mistTexture = new Texture2D(256, 128, TextureFormat.RGBA32, false);
            mistTexture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[256 * 128];
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 256; x++)
                {
                    float u = x / 255f, v = y / 127f;
                    float edge = Mathf.Clamp01(Mathf.Abs(u - 0.5f) * 1.4f + Mathf.Abs(v - 0.5f));
                    float cloud = Mathf.PerlinNoise(u * 7f, v * 5f) * 0.65f +
                        Mathf.PerlinNoise(u * 19f + 12f, v * 13f) * 0.35f;
                    pixels[y * 256 + x] = new Color(0.73f, 0.82f, 0.84f,
                        (0.06f + edge * 0.62f) * cloud);
                }
            mistTexture.SetPixels(pixels);
            mistTexture.Apply(false, true);
            var mistRect = Rect(fogCanvas.transform, "Soft sea mist", Vector2.zero, Vector2.zero);
            mistRect.anchorMin = Vector2.zero; mistRect.anchorMax = Vector2.one;
            mistRect.offsetMin = mistRect.offsetMax = Vector2.zero;
            mist = mistRect.gameObject.AddComponent<RawImage>();
            mist.texture = mistTexture;
            mist.color = new Color(1, 1, 1, 0);
            mist.raycastTarget = false;

            var ui = Canvas("Gateway Approach", 120);
            ui.gameObject.AddComponent<GraphicRaycaster>();
            card = Rect(ui.transform, "Destination", new Vector2(0, -22), new Vector2(600, 154));
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 1);
            card.pivot = new Vector2(0.5f, 1);
            card.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.075f, 0.105f, 0.97f);
            mapName = Label(card, "", new Vector2(15, -12), new Vector2(570, 32), 23);
            status = Label(card, "", new Vector2(15, -48), new Vector2(570, 30), 16);
            confirm = MakeButton(card, "HARİTAYA GEÇ", new Vector2(52, -94), () =>
            {
                closedFrame = Time.frameCount;
                onConfirm();
            });
            MakeButton(card, "VAZGEÇ", new Vector2(322, -94), () =>
            {
                closedFrame = Time.frameCount;
                onCancel();
            });
            card.gameObject.SetActive(false);
        }

        public void Show(string destination, string message, float fog, bool ready, bool showCard)
        {
            targetFog = Mathf.Clamp01(fog);
            mapName.text = destination;
            status.text = message;
            confirm.interactable = ready;
            card.gameObject.SetActive(showCard);
            if (showCard && UnityEngine.EventSystems.EventSystem.current == null)
            {
                var events = new GameObject("Gateway Event System", typeof(UnityEngine.EventSystems.EventSystem));
                events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        public void HideCard()
        {
            closedFrame = Time.frameCount;
            if (card != null) card.gameObject.SetActive(false);
        }

        public void Hide()
        {
            targetFog = 0f;
            // Only consume a close frame when the card was actually visible.
            if (card != null && card.gameObject.activeSelf) HideCard();
        }

        private void LateUpdate()
        {
            if (mist == null) return;
            currentFog = Mathf.MoveTowards(currentFog, targetFog, Time.unscaledDeltaTime * 1.2f);
            mist.color = new Color(1, 1, 1, currentFog);
            mist.uvRect = new UnityEngine.Rect(Mathf.Sin(Time.unscaledTime * 0.12f) * 0.025f,
                Mathf.Cos(Time.unscaledTime * 0.09f) * 0.02f, 1, 1);
        }

        private Canvas Canvas(string name, int order)
        {
            var root = new GameObject(name);
            root.transform.SetParent(transform, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Text Label(Transform parent, string value, Vector2 position, Vector2 size, int fontSize)
        {
            var text = Rect(parent, "Label", position, size).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.color = new Color(0.92f, 0.78f, 0.46f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string title, Vector2 position, Action action)
        {
            var rect = Rect(parent, title, position, new Vector2(226, 42));
            rect.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.3f, 0.32f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(() => action());
            Label(rect, title, Vector2.zero, new Vector2(226, 42), 15);
            return button;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (mistTexture != null) Destroy(mistTexture);
        }
    }
}
