using UnityEngine;

namespace Seaborn.Ship
{
    public sealed class PrototypeEnemyFleetDirector :
        MonoBehaviour
    {
        private bool initialized;

        public static void EnsureCreated()
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

            director.BuildFleet();
            Seaborn.UI.PrototypeEnemyNameplateOverlay
                .EnsureCreated();
        }

        private void BuildFleet()
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
                ConfigureExisting(ships);
                initialized = true;
                return;
            }

            Vector3 origin = template.transform.position;
            Quaternion rotation = template.transform.rotation;

            template.ConfigureArchetype(
                EnemyShipArchetype.Marauder
            );

            EnemyShipController skirmisher =
                Instantiate(
                    template.gameObject,
                    origin + new Vector3(12f, 0f, 8f),
                    rotation
                ).GetComponent<EnemyShipController>();
            skirmisher.ConfigureArchetype(
                EnemyShipArchetype.Skirmisher
            );

            EnemyShipController gunship =
                Instantiate(
                    template.gameObject,
                    origin + new Vector3(-13f, 0f, 10f),
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
            EnemyShipController[] ships)
        {
            ships[0].ConfigureArchetype(
                EnemyShipArchetype.Marauder);
            ships[1].ConfigureArchetype(
                EnemyShipArchetype.Skirmisher);
            ships[2].ConfigureArchetype(
                EnemyShipArchetype.Gunship);
        }
    }
}
