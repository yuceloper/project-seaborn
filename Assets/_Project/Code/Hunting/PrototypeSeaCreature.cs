using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSeaCreature :
        MonoBehaviour,
        IHarpoonTarget
    {
        [SerializeField, Min(1f)]
        private float maximumHealth = 100f;

        [SerializeField, Min(0f)]
        private float cruiseSpeed = 0.8f;

        [SerializeField, Min(0f)]
        private float fleeSpeed = 2.15f;

        [SerializeField, Min(0f)]
        private float fleeDuration = 4f;

        [SerializeField, Min(0)]
        private int silverReward = 45;

        public event Action<float> HealthChanged;
        public event Action<GameObject> Harpooned;
        public event Action Harvested;

        public bool IsHarvested { get; private set; }
        public bool IsMovementExternallyControlled
        {
            get;
            set;
        }
        public float Health => health;
        public float HealthNormalized =>
            maximumHealth <= 0f
                ? 0f
                : health / maximumHealth;

        public void Configure(
            float newMaximumHealth,
            float newCruiseSpeed,
            float newFleeSpeed,
            float newFleeDuration,
            int newSilverReward,
            float visualScale,
            Color color)
        {
            maximumHealth = Mathf.Max(
                1f,
                newMaximumHealth
            );
            health = maximumHealth;
            cruiseSpeed = Mathf.Max(
                0f,
                newCruiseSpeed
            );
            fleeSpeed = Mathf.Max(
                0f,
                newFleeSpeed
            );
            fleeDuration = Mathf.Max(
                0f,
                newFleeDuration
            );
            silverReward = Mathf.Max(
                0,
                newSilverReward
            );
            transform.localScale =
                Vector3.one *
                Mathf.Max(0.1f, visualScale);

            if (material != null &&
                material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    color
                );
            }

            if (visualScale >= 1.5f)
            {
                CreatePart(
                    "Leviathan Crown",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.82f, 0.82f),
                    new Vector3(0.28f, 0.68f, 0.5f),
                    Quaternion.Euler(22f, 0f, 0f)
                );
                CreatePart(
                    "Leviathan Ridge",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.76f, -0.62f),
                    new Vector3(0.22f, 0.56f, 0.58f),
                    Quaternion.Euler(18f, 0f, 0f)
                );
            }
        }

        private float health;
        private float fleeUntil;
        private float movementPhase;
        private Vector3 homePosition;
        private Transform visualRoot;
        private Material material;

        private void Awake()
        {
            health = maximumHealth;
            homePosition = transform.position;
            movementPhase =
                transform.position.x * 0.17f +
                transform.position.z * 0.31f;
            BuildVisual();
        }

        private void Start()
        {
            // Select after the spawner adds the boss component and applies its 1.7 scale.
            bool isBoss = GetComponent<PrototypeLeviathanBehavior>() != null;
            if (!isBoss && (IsMovementExternallyControlled || transform.localScale.x >= 1.5f))
                return;
            GameObject prefab = Resources.Load<GameObject>(
                isBoss ? "SeabornStormjawVisual" : "SeabornWhaleVisual");
            if (prefab == null || visualRoot == null) return;
            foreach (Transform child in visualRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            GameObject instance = Instantiate(prefab, visualRoot, false);
            instance.name = isBoss ? "Meshy Stormjaw Visual" : "Meshy Whale Visual";
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                Destroy(collider);
            }
            // Keep the existing root target collider, movement, bobbing and sink flow.
        }

        private void Update()
        {
            if (IsHarvested ||
                IsMovementExternallyControlled)
            {
                return;
            }

            float woundedIntensity =
                1f - HealthNormalized;
            float speed =
                Time.time < fleeUntil
                    ? fleeSpeed *
                      Mathf.Lerp(
                          1f,
                          1.45f,
                          woundedIntensity
                      )
                    : cruiseSpeed;

            Vector3 toHome =
                homePosition - transform.position;
            toHome.y = 0f;

            if (toHome.sqrMagnitude > 64f)
            {
                Quaternion homeward =
                    Quaternion.LookRotation(
                        toHome.normalized,
                        Vector3.up
                    );
                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        homeward,
                        38f * Time.deltaTime
                    );
            }
            else
            {
                float turn =
                    Mathf.Sin(
                        Time.time * 0.42f +
                        movementPhase
                    ) * 16f;
                transform.Rotate(
                    0f,
                    turn * Time.deltaTime,
                    0f
                );
            }
            transform.position +=
                transform.forward *
                speed *
                Time.deltaTime;

            Vector3 position = transform.position;
            position.y = 0.74f;
            float mapLimit = Seaborn.World.PrototypeExpeditionRegionDirector.MapEdge - 8f;
            position.x = Mathf.Clamp(position.x, -mapLimit, mapLimit);
            position.z = Mathf.Clamp(position.z, -mapLimit, mapLimit);
            transform.position = position;

            if (visualRoot != null)
            {
                visualRoot.localPosition =
                    Vector3.up *
                    Mathf.Sin(
                        Time.time * 1.25f +
                        movementPhase
                    ) *
                    0.08f;
                visualRoot.localRotation =
                    Quaternion.Euler(
                        Mathf.Sin(
                            Time.time * 1.4f +
                            movementPhase
                        ) * 3f,
                        0f,
                        Mathf.Sin(
                            Time.time * 0.9f +
                            movementPhase
                        ) * 2f
                    );
            }
        }

        public void ApplyHarpoonHit(
            float damage,
            Vector3 hitPoint,
            GameObject hunter)
        {
            if (IsHarvested || damage <= 0f)
            {
                return;
            }

            health = Mathf.Max(0f, health - damage);
            HealthChanged?.Invoke(health);
            Harpooned?.Invoke(hunter);

            if (!IsMovementExternallyControlled &&
                hunter != null)
            {
                Vector3 away =
                    transform.position -
                    hunter.transform.position;
                away.y = 0f;

                if (away.sqrMagnitude > 0.01f)
                {
                    transform.rotation =
                        Quaternion.LookRotation(
                            away.normalized,
                            Vector3.up
                        );
                }

                fleeUntil =
                    Time.time + fleeDuration;
            }

            if (health <= 0f)
            {
                Harvest(hunter);
            }
        }

        private void Harvest(GameObject hunter)
        {
            IsHarvested = true;
            Harvested?.Invoke();

            if (hunter != null)
            {
                PrototypeHuntCargo cargo =
                    hunter.GetComponentInChildren<
                        PrototypeHuntCargo>();

                if (cargo != null)
                {
                    cargo.AddCatch(
                        name,
                        silverReward
                    );
                }

                Seaborn.Progression
                    .PrototypeRegionalLootInventory inventory =
                    Seaborn.Progression
                        .PrototypeRegionalLootInventory
                        .EnsureAttached(hunter.transform);
                inventory?.AwardHunt(name);
                Seaborn.Progression
                    .PrototypeDeckExtensionInventory
                    .EnsureAttached(hunter.transform)
                    ?.AwardLeviathan(name);
            }

            StartCoroutine(SinkAndDestroy());
        }

        private IEnumerator SinkAndDestroy()
        {
            Vector3 start = transform.position;
            Quaternion startRotation =
                transform.rotation;
            float elapsed = 0f;
            const float duration = 1.7f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress =
                    Mathf.Clamp01(elapsed / duration);
                transform.position =
                    start +
                    Vector3.down *
                    (progress * 1.8f);
                transform.rotation =
                    startRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        progress * 28f
                    );
                yield return null;
            }

            Destroy(gameObject);
        }

        private void BuildVisual()
        {
            GameObject visualObject =
                new GameObject("Tideback Visual");
            visualRoot = visualObject.transform;
            visualRoot.SetParent(transform, false);

            Material template =
                Resources.Load<Material>(
                    "PrototypeShipBlockout"
                );

            if (template != null)
            {
                material = new Material(template);
                material.name =
                    "Runtime Tideback Material";

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor(
                        "_BaseColor",
                        new Color(
                            0.17f,
                            0.3f,
                            0.33f,
                            1f
                        )
                    );
                }

                if (material.HasProperty("_TopLight"))
                {
                    material.SetFloat(
                        "_TopLight",
                        0.45f
                    );
                }
            }

            CreatePart(
                "Body",
                PrimitiveType.Capsule,
                new Vector3(0f, 0f, 0f),
                new Vector3(0.95f, 1.75f, 0.95f),
                Quaternion.Euler(90f, 0f, 0f)
            );
            CreatePart(
                "Head",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.02f, 1.55f),
                new Vector3(1.05f, 0.72f, 1.15f),
                Quaternion.identity
            );
            CreatePart(
                "Dorsal Fin",
                PrimitiveType.Cube,
                new Vector3(0f, 0.62f, -0.15f),
                new Vector3(0.16f, 0.48f, 0.72f),
                Quaternion.Euler(18f, 0f, 0f)
            );
            CreatePart(
                "Left Tail",
                PrimitiveType.Cube,
                new Vector3(-0.52f, 0f, -1.9f),
                new Vector3(0.92f, 0.13f, 0.48f),
                Quaternion.Euler(0f, 28f, 0f)
            );
            CreatePart(
                "Right Tail",
                PrimitiveType.Cube,
                new Vector3(0.52f, 0f, -1.9f),
                new Vector3(0.92f, 0.13f, 0.48f),
                Quaternion.Euler(0f, -28f, 0f)
            );

            CapsuleCollider targetCollider =
                gameObject.AddComponent<
                    CapsuleCollider>();
            targetCollider.direction = 2;
            targetCollider.radius = 0.9f;
            targetCollider.height = 4.2f;
            targetCollider.center =
                new Vector3(0f, 0f, -0.1f);
        }

        private void CreatePart(
            string partName,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation)
        {
            GameObject part =
                GameObject.CreatePrimitive(type);
            part.name = partName;
            part.transform.SetParent(
                visualRoot,
                false
            );
            part.transform.localPosition =
                localPosition;
            part.transform.localScale =
                localScale;
            part.transform.localRotation =
                localRotation;

            Collider collider =
                part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            if (material != null)
            {
                part.GetComponent<MeshRenderer>()
                    .sharedMaterial = material;
            }
        }

        private void OnDestroy()
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }

    public static class PrototypeSeaCreatureSpawner
    {
        public static void EnsureSpawned(
            Vector3 playerPosition)
        {
            if (SceneManager.GetActiveScene().name == "PrototypeHarbor") return;
            if (!Seaborn.World.PrototypePopulationDirector.EnsureCreated().TryInitializeHunts()) return;
            if (UnityEngine.Object.FindFirstObjectByType<
                    PrototypeSeaCreature>() != null)
            {
                return;
            }
            string scene =
                SceneManager.GetActiveScene().name;

            // Twenty independent hunt slots spread across a 5 x 4 sea grid.
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int index = row * 5 + column;
                    Vector3 position = new Vector3(
                        (row < 2 ? -1f : 1f) * (44f + (row % 2) * 12f),
                        0.74f,
                        -36f + column * 12f);
                    Spawn($"Tideback - {scene} {index + 1}",
                        position, (45f + index * 53f) % 360f);
                }
            }
            SpawnLeviathan(new Vector3(48f, 0.74f, 56f));
            Debug.Log($"{scene} av nüfusu: 20 Tideback, 1 Stormjaw.");
        }

        private static void SpawnLeviathan(Vector3 position)
        {
            SpawnLeviathanAt(position, position);
        }

        private static void SpawnLeviathanAt(
            Vector3 position, Vector3 home)
        {
            GameObject creature =
                new GameObject(
                    "Stormjaw Leviathan"
                );
            creature.transform.position = position;
            creature.transform.rotation =
                Quaternion.Euler(0f, 135f, 0f);

            PrototypeSeaCreature seaCreature =
                creature.AddComponent<
                    PrototypeSeaCreature>();
            seaCreature.Configure(
                340f,
                0.48f,
                1.35f,
                5f,
                240,
                1.7f,
                new Color(
                    0.12f,
                    0.16f,
                    0.27f,
                    1f
                )
            );
            creature.AddComponent<
                PrototypeLeviathanBehavior>();
            Seaborn.World.PrototypePopulationDirector.EnsureCreated()
                .RegisterHunt(seaCreature, home, 300f, next => SpawnLeviathanAt(next, home));
        }

        private static void Spawn(
            string creatureName,
            Vector3 position,
            float heading, Vector3? originalHome = null)
        {
            Vector3 home = originalHome ?? position;
            GameObject creature =
                new GameObject(creatureName);
            creature.transform.position = position;
            creature.transform.rotation =
                Quaternion.Euler(0f, heading, 0f);
            var target = creature.AddComponent<PrototypeSeaCreature>();
            Seaborn.World.PrototypePopulationDirector.EnsureCreated()
                .RegisterHunt(target, home, 60f, next => Spawn(creatureName, next, heading, home));
        }
    }
}
