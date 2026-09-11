using Seaborn.Combat;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Combat.Loot
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipwreckLootSource :
        MonoBehaviour
    {
        private ShipHealth health;
        private Transform player;
        private bool dropped;

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void Awake()
        {
            health = GetComponent<ShipHealth>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<ShipHealth>();
            }

            if (health != null)
            {
                health.Sunk += HandleSunk;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Sunk -= HandleSunk;
            }
        }

        private void HandleSunk()
        {
            if (dropped ||
                gameObject.name.Contains("Wreck Raider"))
            {
                return;
            }

            dropped = true;
            PrototypeShipwreckLootPickup.Create(
                transform.position,
                player
            );
        }
    }

    public sealed class PrototypeShipwreckLootPickup :
        MonoBehaviour
    {
        private const float InteractionRadius = 2.8f;
        private const float Lifetime = 120f;
        private const int SilverReward = 35;
        private const int AmmunitionReward = 6;

        private Transform player;
        private PrototypeHuntCargo cargo;
        private BroadsideController broadside;
        private LineRenderer ring;
        private float expiresAt;
        private float baseHeight;
        private bool collected;

        public static void Create(
            Vector3 position,
            Transform playerTransform)
        {
            GameObject pickup =
                new GameObject("Shipwreck Loot");
            pickup.transform.position =
                new Vector3(
                    position.x,
                    0.34f,
                    position.z
                );

            PrototypeShipwreckLootPickup component =
                pickup.AddComponent<
                    PrototypeShipwreckLootPickup>();
            component.Initialize(playerTransform);
        }

        private void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            expiresAt = Time.time + Lifetime;
            baseHeight = transform.position.y;
            ResolvePlayerComponents();
            BuildVisual();

            Debug.Log(
                "Düşman enkazı yüzeye çıktı: " +
                "35 güvencesiz silver, 6 standart gülle.",
                this
            );
        }

        private void Update()
        {
            Animate();

            if (Time.time >= expiresAt)
            {
                Debug.Log(
                    "Toplanmayan düşman enkazı battı.",
                    this
                );
                Destroy(gameObject);
                return;
            }

            if (player == null)
            {
                ResolvePlayer();
                return;
            }

            float distance = HorizontalDistance(
                transform.position,
                player.position
            );

            if (distance > InteractionRadius ||
                Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame)
            {
                return;
            }

            Collect();
        }

        private void Collect()
        {
            if (collected)
            {
                return;
            }

            ResolvePlayerComponents();

            if (cargo == null || broadside == null)
            {
                Debug.Log(
                    "Enkaz ganimeti için oyuncu bileşenleri bulunamadı.",
                    this
                );
                return;
            }

            collected = true;
            cargo.AddCatch(
                "Düşman gemisi enkazı",
                SilverReward
            );
            broadside.AddAmmunition(
                AmmunitionType.Standard,
                AmmunitionReward
            );

            PrototypeCombatVfx.PlayWaterSplash(
                transform.position
            );

            Debug.Log(
                "Enkaz toplandı: +35 güvencesiz silver, " +
                "+6 standart gülle.",
                this
            );
            Destroy(gameObject);
        }

        private void ResolvePlayer()
        {
            ManualBroadsideAimController controller =
                FindFirstObjectByType<
                    ManualBroadsideAimController>();

            if (controller == null)
            {
                return;
            }

            player = controller.transform;
            ResolvePlayerComponents();
        }

        private void ResolvePlayerComponents()
        {
            if (player == null)
            {
                return;
            }

            cargo =
                player.GetComponentInChildren<
                    PrototypeHuntCargo>();
            broadside =
                player.GetComponentInChildren<
                    BroadsideController>();
        }

        private void BuildVisual()
        {
            Material material =
                Resources.Load<Material>(
                    "PrototypeCombatParticle"
                );

            ring = gameObject.AddComponent<LineRenderer>();
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 40;
            ring.widthMultiplier = 0.07f;
            ring.startColor =
                new Color(0.92f, 0.72f, 0.3f, 0.82f);
            ring.endColor = ring.startColor;

            if (material != null)
            {
                ring.sharedMaterial = material;
            }

            for (int index = 0; index < 40; index++)
            {
                float angle =
                    index / 40f * Mathf.PI * 2f;
                ring.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) *
                            InteractionRadius,
                        0f,
                        Mathf.Sin(angle) *
                            InteractionRadius
                    )
                );
            }

            GameObject crate =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );
            crate.name = "Floating Salvage Crate";
            crate.transform.SetParent(transform, false);
            crate.transform.localPosition =
                new Vector3(0f, 0.22f, 0f);
            crate.transform.localRotation =
                Quaternion.Euler(8f, 24f, 5f);
            crate.transform.localScale =
                new Vector3(0.9f, 0.45f, 0.7f);

            Collider crateCollider =
                crate.GetComponent<Collider>();
            if (crateCollider != null)
            {
                Destroy(crateCollider);
            }

            Renderer crateRenderer =
                crate.GetComponent<Renderer>();
            if (crateRenderer != null)
            {
                if (material != null)
                {
                    crateRenderer.sharedMaterial = material;
                }

                MaterialPropertyBlock properties =
                    new MaterialPropertyBlock();
                crateRenderer.GetPropertyBlock(properties);
                Color crateColor =
                    new Color(0.43f, 0.24f, 0.09f, 1f);
                properties.SetColor(
                    "_BaseColor",
                    crateColor
                );
                properties.SetColor("_Color", crateColor);
                crateRenderer.SetPropertyBlock(properties);
            }
        }

        private void Animate()
        {
            Vector3 position = transform.position;
            position.y =
                baseHeight +
                Mathf.Sin(Time.time * 1.9f) * 0.1f;
            transform.position = position;

            if (ring != null)
            {
                float pulse =
                    1f +
                    Mathf.Sin(Time.time * 2.7f) * 0.04f;
                ring.transform.localScale =
                    Vector3.one * pulse;
            }
        }

        private void OnGUI()
        {
            if (player == null ||
                HorizontalDistance(
                    transform.position,
                    player.position
                ) > InteractionRadius)
            {
                return;
            }

            const float width = 330f;
            Rect prompt = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height - 128f,
                width,
                42f
            );

            Color previous = GUI.color;
            GUI.color =
                new Color(0.04f, 0.1f, 0.13f, 0.94f);
            GUI.Box(prompt, GUIContent.none);
            GUI.color = previous;

            GUIStyle style =
                new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;
            style.normal.textColor =
                new Color(0.92f, 0.76f, 0.38f);

            GUI.Label(
                prompt,
                "E — ENKAZ GANİMETİNİ TOPLA",
                style
            );
        }

        private static float HorizontalDistance(
            Vector3 first,
            Vector3 second)
        {
            Vector3 delta = first - second;
            delta.y = 0f;
            return delta.magnitude;
        }
    }

    internal sealed class PrototypeShipwreckLootBootstrap :
        MonoBehaviour
    {
        private float nextScanTime;
        private Transform player;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<
                    PrototypeShipwreckLootBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrap =
                new GameObject(
                    "Prototype Shipwreck Loot Bootstrap"
                );
            bootstrap.AddComponent<
                PrototypeShipwreckLootBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + 0.5f;

            if (player == null)
            {
                ManualBroadsideAimController controller =
                    FindFirstObjectByType<
                        ManualBroadsideAimController>();
                if (controller != null)
                {
                    player = controller.transform;
                }
            }

            EnemyShipController[] enemies =
                FindObjectsByType<EnemyShipController>(
                    FindObjectsSortMode.None
                );

            foreach (EnemyShipController enemy in enemies)
            {
                PrototypeShipwreckLootSource source =
                    enemy.GetComponent<
                        PrototypeShipwreckLootSource>();

                if (source == null)
                {
                    source = enemy.gameObject.AddComponent<
                        PrototypeShipwreckLootSource>();
                }

                source.Configure(player);
            }
        }
    }
}
