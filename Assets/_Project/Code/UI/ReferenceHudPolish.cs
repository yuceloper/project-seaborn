using UnityEngine;
using UnityEngine.UI;

namespace Seaborn.UI
{
    [DisallowMultipleComponent]
    internal sealed class ReferenceHudPolish : MonoBehaviour
    {
        private RectTransform navigation;
        private RectTransform combat;
        private RectTransform helm;
        private Text helmText;
        private bool applied;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<ReferenceHudPolishBootstrap>() != null) return;
            new GameObject("Reference HUD Polish Bootstrap")
                .AddComponent<ReferenceHudPolishBootstrap>();
        }

        private void Start()
        {
            ApplyLayout();
        }

        private void LateUpdate()
        {
            if (!applied)
            {
                ApplyLayout();
            }

        }

        private void ApplyLayout()
        {
            navigation = transform.Find("Navigation") as RectTransform;
            combat = transform.Find("Combat") as RectTransform;
            helm = transform.Find("Helm") as RectTransform;
            if (navigation == null || combat == null || helm == null) return;

            navigation.localScale = Vector3.one * 0.78f;
            navigation.anchoredPosition = new Vector2(18f, 22f);

            combat.anchoredPosition = new Vector2(0f, 32f);

            for (int index = 0; index < 10; index++)
            {
                RectTransform slot = combat.Find("Slot " + index) as RectTransform;
                if (slot == null) continue;

                slot.localScale = Vector3.one * 0.94f;
                slot.anchoredPosition = new Vector2(18f + index * 94f, -31f);

                Text[] texts = slot.GetComponentsInChildren<Text>(true);
                foreach (Text text in texts)
                {
                    RectTransform rect = text.rectTransform;
                    float y = rect.anchoredPosition.y;
                    if (y <= -95f)
                    {
                        text.fontSize = 12;
                        text.lineSpacing = 0.84f;
                        Color color = text.color;
                        color.a = 0.88f;
                        text.color = color;
                        rect.anchoredPosition = new Vector2(3f, -98f);
                        rect.sizeDelta = new Vector2(88f, 28f);
                    }
                    else if (y > -20f)
                    {
                        text.fontSize = 12;
                        Color color = text.color;
                        color.a = 0.86f;
                        text.color = color;
                    }
                    else if (y <= -65f)
                    {
                        text.fontSize = 13;
                    }
                    else
                    {
                        text.fontSize = Mathf.Max(text.fontSize, 12);
                    }
                }
            }

            // Reduce the visual weight of the secondary readiness copy above the hotbar.
            foreach (Text text in combat.GetComponentsInChildren<Text>(true))
            {
                if (text.transform.parent != combat) continue;
                text.fontSize = Mathf.Max(text.fontSize, 12);
                Color color = text.color;
                color.a = 0.84f;
                text.color = color;
            }

            helm.anchoredPosition = new Vector2(0f, 188f);
            helmText = helm.GetComponentInChildren<Text>(true);
            if (helmText != null)
            {
                helmText.fontSize = 14;
            }

            applied = true;
        }
    }

    internal sealed class ReferenceHudPolishBootstrap : MonoBehaviour
    {
        private float nextScanTime;

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime) return;
            nextScanTime = Time.unscaledTime + 0.25f;

            PrototypeGameplayHud hud = FindFirstObjectByType<PrototypeGameplayHud>();
            if (hud == null) return;

            if (hud.GetComponent<ReferenceHudPolish>() == null)
            {
                hud.gameObject.AddComponent<ReferenceHudPolish>();
            }

            Destroy(gameObject);
        }
    }
}
