using System;
using System.Collections;
using UnityEngine;

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
        public event Action Harvested;

        public bool IsHarvested { get; private set; }
        public float Health => health;
        public float HealthNormalized =>
            maximumHealth <= 0f
                ? 0f
                : health / maximumHealth;

        private float health;
        private float fleeUntil;
        private float movementPhase;
        private Transform visualRoot;
        private Material material;

        private void Awake()
        {
            health = maximumHealth;
            movementPhase =
                transform.position.x * 0.17f +
                transform.position.z * 0.31f;
            BuildVisual();
        }

        private void Update()
        {
            if (IsHarvested)
            {
                return;
            }

            float speed =
                Time.time < fleeUntil
                    ? fleeSpeed
                    : cruiseSpeed;

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
            transform.position +=
                transform.forward *
                speed *
                Time.deltaTime;

            Vector3 position = transform.position;
            position.y = 0.74f;
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

            if (hunter != null)
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
            }

            fleeUntil = Time.time + fleeDuration;

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
                PrototypeSilverWallet wallet =
                    hunter.GetComponentInChildren<
                        PrototypeSilverWallet>();

                if (wallet != null)
                {
                    wallet.AddSilver(silverReward);
                }
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
        private static bool hasSpawned;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            hasSpawned = false;
        }

        public static void EnsureSpawned(
            Vector3 playerPosition)
        {
            if (hasSpawned)
            {
                return;
            }

            hasSpawned = true;
            Spawn(
                "Tideback - North",
                playerPosition +
                new Vector3(7f, 0.74f, 8f),
                210f
            );
            Spawn(
                "Tideback - East",
                playerPosition +
                new Vector3(11f, 0.74f, -3f),
                285f
            );
            Spawn(
                "Tideback - West",
                playerPosition +
                new Vector3(-9f, 0.74f, 2f),
                75f
            );
        }

        private static void Spawn(
            string creatureName,
            Vector3 position,
            float heading)
        {
            GameObject creature =
                new GameObject(creatureName);
            creature.transform.position = position;
            creature.transform.rotation =
                Quaternion.Euler(0f, heading, 0f);
            creature.AddComponent<
                PrototypeSeaCreature>();
        }
    }
}
