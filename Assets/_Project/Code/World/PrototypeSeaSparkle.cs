using Seaborn.Combat.Damage;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Seaborn.World
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSeaSparkle : MonoBehaviour
    {
        private const float CollectRange = 3.2f;
        private const float CollectDuration = 3f;
        private const float MovementTolerance = 0.75f;

        private Transform player;
        private ShipHealth health;
        private PrototypeHuntCargo cargo;
        private PrototypeGoldWallet gold;
        private PrototypeRegionalLootInventory materials;
        private Vector3 basePosition;
        private Vector3 channelStart;
        private float progress;
        private float interruptedUntil;
        private bool collected;
        private string rewardMessage;
        private Material visualMaterial;

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            health = player.GetComponentInChildren<ShipHealth>();
            cargo = player.GetComponentInChildren<PrototypeHuntCargo>();
            gold = PrototypeGoldWallet.EnsureAttached(player);
            materials = PrototypeRegionalLootInventory
                .EnsureAttached(player);
            basePosition = transform.position;

            if (health != null)
                health.Damaged += HandleDamaged;

            BuildVisual();
        }

        private void Update()
        {
            if (collected || player == null) return;

            float time = Time.unscaledTime;
            transform.position = basePosition +
                Vector3.up * (0.28f + Mathf.Sin(time * 2.2f) * 0.2f);
            transform.Rotate(
                0f,
                42f * Time.unscaledDeltaTime,
                0f,
                Space.World
            );

            float distance = HorizontalDistance(
                player.position,
                transform.position
            );
            bool holding =
                Keyboard.current != null &&
                Keyboard.current.eKey.isPressed;

            if (distance > CollectRange ||
                !holding ||
                time < interruptedUntil)
            {
                progress = 0f;
                return;
            }

            if (progress <= 0f)
                channelStart = player.position;

            if (HorizontalDistance(
                    player.position,
                    channelStart) >
                MovementTolerance)
            {
                progress = 0f;
                interruptedUntil = time + 0.45f;
                return;
            }

            progress += Time.unscaledDeltaTime;
            if (progress >= CollectDuration)
                Collect();
        }

        private void Collect()
        {
            collected = true;
            progress = CollectDuration;

            float roll = Random.value;
            if (roll < 0.58f && cargo != null)
            {
                int amount = Random.Range(18, 41);
                cargo.AddCatch("Deniz Pırıltısı", amount);
                rewardMessage =
                    $"+{amount} güvencesiz silver";
            }
            else if (roll < 0.78f && health != null)
            {
                float amount = Mathf.Max(
                    10f,
                    health.MaximumHealth * 0.12f
                );
                float restored =
                    health.RestoreHealth(amount);
                if (restored > 0.1f)
                {
                    rewardMessage =
                        $"+{Mathf.RoundToInt(restored)} gövde";
                }
                else
                {
                    const int fallback = 18;
                    cargo?.AddCatch(
                        "Deniz Pırıltısı",
                        fallback
                    );
                    rewardMessage =
                        $"+{fallback} güvencesiz silver";
                }
            }
            else if (roll < 0.98f && materials != null)
            {
                RegionalMaterialType type =
                    SelectRegionalMaterial();
                materials.Add(type, 1, "Deniz Pırıltısı");
                rewardMessage =
                    $"+1 {PrototypeRegionalLootInventory.DisplayName(type)}";
            }
            else
            {
                gold?.AddGold(1, "Nadir Deniz Pırıltısı");
                rewardMessage = "+1 GOLD";
            }

            SetVisuals(false);
            Debug.Log(
                $"Deniz pırıltısı toplandı: {rewardMessage}.",
                this
            );
            Destroy(gameObject, 3.5f);
        }

        private RegionalMaterialType SelectRegionalMaterial()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "PrototypeEasternReach")
            {
                return Random.value < 0.9f
                    ? RegionalMaterialType.TideOil
                    : RegionalMaterialType.LostChartFragment;
            }
            if (scene == "PrototypeWesternReach")
            {
                return Random.value < 0.9f
                    ? RegionalMaterialType.CorsairIron
                    : RegionalMaterialType.LostChartFragment;
            }
            return Random.value < 0.55f
                ? RegionalMaterialType.TideOil
                : RegionalMaterialType.CorsairIron;
        }

        private void HandleDamaged(DamageInfo info)
        {
            if (progress <= 0f) return;

            progress = 0f;
            interruptedUntil = Time.unscaledTime + 1f;
        }

        private void BuildVisual()
        {
            visualMaterial = new Material(
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Sprites/Default")
            );
            visualMaterial.color =
                new Color(0.3f, 0.9f, 1f, 0.88f);

            LineRenderer ring =
                gameObject.AddComponent<LineRenderer>();
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 32;
            ring.widthMultiplier = 0.08f;
            ring.sharedMaterial = visualMaterial;
            ring.startColor =
                new Color(0.45f, 0.95f, 1f, 0.75f);
            ring.endColor = ring.startColor;
            ring.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle =
                    i / (float)ring.positionCount *
                    Mathf.PI * 2f;
                ring.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * 0.75f,
                        0f,
                        Mathf.Sin(angle) * 0.75f
                    )
                );
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject mote =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Sphere);
                mote.name = "Sea Sparkle";
                mote.transform.SetParent(transform, false);
                float angle = i * Mathf.PI * 0.5f;
                mote.transform.localPosition =
                    new Vector3(
                        Mathf.Cos(angle) * 0.48f,
                        0.18f + (i % 2) * 0.2f,
                        Mathf.Sin(angle) * 0.48f
                    );
                mote.transform.localScale =
                    Vector3.one * (i == 0 ? 0.18f : 0.12f);
                Destroy(mote.GetComponent<Collider>());
                Renderer renderer =
                    mote.GetComponent<Renderer>();
                renderer.sharedMaterial = visualMaterial;
                renderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            Light glow = gameObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.25f, 0.85f, 1f);
            glow.range = 3.5f;
            glow.intensity = 1.4f;
            glow.shadows = LightShadows.None;
        }

        private void SetVisuals(bool visible)
        {
            Renderer[] renderers =
                GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = visible;

            Light glow = GetComponent<Light>();
            if (glow != null) glow.enabled = visible;
        }

        private void OnGUI()
        {
            if (player == null) return;

            if (collected &&
                !string.IsNullOrEmpty(rewardMessage))
            {
                DrawMessage(
                    rewardMessage,
                    new Color(0.91f, 0.78f, 0.46f)
                );
                return;
            }

            if (collected ||
                HorizontalDistance(
                    player.position,
                    transform.position) > CollectRange)
                return;

            float ratio = Mathf.Clamp01(
                progress / CollectDuration);
            string label = interruptedUntil >
                Time.unscaledTime
                ? "TOPLAMA KESİLDİ"
                : progress > 0f
                    ? $"TOPLANIYOR  %{Mathf.RoundToInt(ratio * 100f)}"
                    : "E BASILI TUT  •  DENİZ PIRILTISI";
            DrawMessage(
                label,
                interruptedUntil > Time.unscaledTime
                    ? new Color(0.92f, 0.4f, 0.3f)
                    : new Color(0.5f, 0.92f, 1f)
            );
        }

        private static void DrawMessage(
            string label,
            Color color)
        {
            const float width = 390f;
            Rect rect = new(
                (Screen.width - width) * 0.5f,
                Screen.height - 178f,
                width,
                38f
            );
            GUI.Box(rect, GUIContent.none);
            GUIStyle style =
                new(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 14
                };
            style.normal.textColor = color;
            GUI.Label(rect, label, style);
        }

        private static float HorizontalDistance(
            Vector3 first,
            Vector3 second)
        {
            Vector3 offset = first - second;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
            if (visualMaterial != null)
                Destroy(visualMaterial);
        }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeSeaSparkleDirector :
        MonoBehaviour
    {
        private static readonly Vector3[] Positions =
        {
            new(-52f, 0.2f, -34f),
            new(-28f, 0.2f, 46f),
            new(18f, 0.2f, 58f),
            new(49f, 0.2f, 31f),
            new(57f, 0.2f, -42f),
            new(8f, 0.2f, -54f),
            new(-43f, 0.2f, 12f),
            new(34f, 0.2f, -8f),
            new(-6f, 0.2f, 25f)
        };

        public static void EnsureCreated(Transform player)
        {
            if (player == null ||
                PrototypeExpeditionRegionDirector.IsHarborScene)
                return;

            PrototypeSeaSparkleDirector existing =
                FindFirstObjectByType<
                    PrototypeSeaSparkleDirector>();
            if (existing != null) return;

            GameObject root =
                new("Prototype Sea Sparkles");
            PrototypeSeaSparkleDirector director =
                root.AddComponent<
                    PrototypeSeaSparkleDirector>();
            director.Spawn(player);
        }

        private void Spawn(Transform player)
        {
            string scene = SceneManager.GetActiveScene().name;
            int count = scene == "PrototypeOcean" ? 6 : 8;
            int offset = scene == "PrototypeEasternReach"
                ? 1
                : scene == "PrototypeWesternReach"
                    ? 2
                    : 0;

            for (int i = 0; i < count; i++)
            {
                Vector3 position =
                    Positions[(i + offset) % Positions.Length];
                GameObject sparkle =
                    new($"Deniz Pırıltısı {i + 1}");
                sparkle.transform.SetParent(transform, false);
                sparkle.transform.position = position;
                sparkle.AddComponent<PrototypeSeaSparkle>()
                    .Initialize(player);
            }
        }
    }
}
