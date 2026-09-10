using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class PrototypeHarborDeliveryZone :
        MonoBehaviour
    {
        [SerializeField, Min(1f)]
        private float deliveryRadius = 3.2f;

        private Transform player;
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;
        private Material markerMaterial;

        public float DeliveryRadius => deliveryRadius;

        public static void EnsureCreated(
            Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            PrototypeHarborDeliveryZone existing =
                FindFirstObjectByType<
                    PrototypeHarborDeliveryZone>();

            if (existing != null)
            {
                existing.SetPlayer(playerTransform);
                return;
            }

            GameObject zoneObject =
                new GameObject(
                    "Prototype Harbor Delivery Zone"
                );
            Vector3 position =
                playerTransform.position;
            position.y = 0.76f;
            zoneObject.transform.position = position;

            PrototypeHarborDeliveryZone zone =
                zoneObject.AddComponent<
                    PrototypeHarborDeliveryZone>();
            zone.SetPlayer(playerTransform);
        }

        private void Awake()
        {
            BuildMarker();
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            if (cargo == null)
            {
                cargo =
                    player.GetComponentInChildren<
                        PrototypeHuntCargo>();
            }

            if (wallet == null)
            {
                wallet =
                    player.GetComponentInChildren<
                        PrototypeSilverWallet>();
            }

            if (cargo == null ||
                wallet == null ||
                !cargo.HasCargo)
            {
                return;
            }

            Vector3 offset =
                player.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude >
                deliveryRadius * deliveryRadius)
            {
                return;
            }

            int secured =
                cargo.SecureAtPort(wallet);

            if (secured > 0)
            {
                Debug.Log(
                    $"Liman teslimi: {secured} silver " +
                    "güvenceye alındı.",
                    this
                );
            }
        }

        private void SetPlayer(
            Transform playerTransform)
        {
            player = playerTransform;
            cargo =
                player.GetComponentInChildren<
                    PrototypeHuntCargo>();
            wallet =
                player.GetComponentInChildren<
                    PrototypeSilverWallet>();
        }

        private void BuildMarker()
        {
            GameObject buoy =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );
            buoy.name = "Harbor Beacon";
            buoy.transform.SetParent(
                transform,
                false
            );
            buoy.transform.localPosition =
                new Vector3(0f, 0.35f, 0f);
            buoy.transform.localScale =
                new Vector3(0.34f, 0.55f, 0.34f);

            Collider buoyCollider =
                buoy.GetComponent<Collider>();
            if (buoyCollider != null)
            {
                Destroy(buoyCollider);
            }

            Material template =
                Resources.Load<Material>(
                    "PrototypeShipBlockout"
                );

            if (template != null)
            {
                markerMaterial = new Material(template);
                markerMaterial.name =
                    "Runtime Harbor Marker";

                if (markerMaterial.HasProperty(
                        "_BaseColor"))
                {
                    markerMaterial.SetColor(
                        "_BaseColor",
                        new Color(
                            0.72f,
                            0.48f,
                            0.12f,
                            1f
                        )
                    );
                }

                buoy.GetComponent<MeshRenderer>()
                    .sharedMaterial = markerMaterial;
            }

            LineRenderer ring =
                gameObject.AddComponent<
                    LineRenderer>();
            const int segmentCount = 48;
            ring.positionCount = segmentCount;
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.widthMultiplier = 0.065f;
            ring.numCapVertices = 2;
            ring.startColor =
                new Color(
                    0.95f,
                    0.68f,
                    0.2f,
                    0.8f
                );
            ring.endColor = ring.startColor;

            Material lineMaterial =
                Resources.Load<Material>(
                    "PrototypeCombatParticle"
                );
            if (lineMaterial != null)
            {
                ring.sharedMaterial = lineMaterial;
            }

            for (
                int index = 0;
                index < segmentCount;
                index++)
            {
                float angle =
                    index /
                    (float)segmentCount *
                    Mathf.PI *
                    2f;
                ring.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) *
                        deliveryRadius,
                        0.04f,
                        Mathf.Sin(angle) *
                        deliveryRadius
                    )
                );
            }
        }

        private void OnDestroy()
        {
            if (markerMaterial != null)
            {
                Destroy(markerMaterial);
            }
        }
    }
}
