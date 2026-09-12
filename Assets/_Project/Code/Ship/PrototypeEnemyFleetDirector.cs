using UnityEngine;

namespace Seaborn.Ship
{
    public sealed class PrototypeEnemyFleetDirector :
        MonoBehaviour
    {
        private bool initialized;

        public static void EnsureCreated(
            Transform player)
        {
            PrototypeEnemyFleetDirector director =
                FindFirstObjectByType<
                    PrototypeEnemyFleetDirector>();

            if (director == null)
            {
                GameObject root = new(
                    "Prototype Enemy Fleet Director");
                director = root.AddComponent<
                    PrototypeEnemyFleetDirector>();
            }

            if (player != null)
            {
                director.BuildFleet(player.position);
            }
            Seaborn.UI.PrototypeEnemyNameplateOverlay
                .EnsureCreated();
        }

        private void BuildFleet(Vector3 playerPosition)
        {
            if (initialized) return;

            EnemyShipController[] ships =
                FindObjectsByType<EnemyShipController>(
                    FindObjectsSortMode.None
                );

            if (ships.Length == 0) return;

            EnemyShipController template = ships[0];
            if (ships.Length >= 3)
            {
                ConfigureExisting(
                    ships,
                    playerPosition
                );
                initialized = true;
                return;
            }

            Vector3 origin = playerPosition +
                new Vector3(-18f, 0f, 22f);
            origin.y = template.transform.position.y;
            Quaternion rotation = template.transform.rotation;
            template.transform.position = origin;

            template.ConfigureArchetype(
                EnemyShipArchetype.Marauder
            );

            EnemyShipController skirmisher =
                Instantiate(
                    template.gameObject,
                    origin + new Vector3(6f, 0f, -4f),
                    rotation
                ).GetComponent<EnemyShipController>();
            skirmisher.ConfigureArchetype(
                EnemyShipArchetype.Skirmisher
            );

            EnemyShipController gunship =
                Instantiate(
                    template.gameObject,
                    origin + new Vector3(-6f, 0f, 3f),
                    rotation
                ).GetComponent<EnemyShipController>();
            gunship.ConfigureArchetype(
                EnemyShipArchetype.Gunship
            );

            initialized = true;
            Debug.Log(
                "Düşman filosu hazır: Saltfang, " +
                "Razorwind ve Ironwake.",
                this
            );
        }

        private static void ConfigureExisting(
            EnemyShipController[] ships,
            Vector3 playerPosition)
        {
            Vector3 center = playerPosition +
                new Vector3(-18f, 0f, 22f);
            Vector3 first = center;
            Vector3 second = center +
                new Vector3(6f, 0f, -4f);
            Vector3 third = center +
                new Vector3(-6f, 0f, 3f);
            first.y = ships[0].transform.position.y;
            second.y = ships[1].transform.position.y;
            third.y = ships[2].transform.position.y;
            ships[0].transform.position = first;
            ships[1].transform.position = second;
            ships[2].transform.position = third;

            ships[0].ConfigureArchetype(
                EnemyShipArchetype.Marauder);
            ships[1].ConfigureArchetype(
                EnemyShipArchetype.Skirmisher);
            ships[2].ConfigureArchetype(
                EnemyShipArchetype.Gunship);
        }
    }
}
