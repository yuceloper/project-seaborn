using UnityEngine;
using UnityEngine.SceneManagement;

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
                director.BuildFleet();
            }

            Seaborn.UI.PrototypeEnemyNameplateOverlay
                .EnsureCreated();
        }

        private void BuildFleet()
        {
            if (initialized) return;

            EnemyShipController[] existing =
                FindObjectsByType<EnemyShipController>(
                    FindObjectsSortMode.None
                );
            if (existing.Length == 0) return;

            string scene =
                SceneManager.GetActiveScene().name;
            EnemyShipArchetype[] archetypes =
                ProfileArchetypes(scene);
            Vector3[] positions =
                ProfilePositions(scene);

            EnemyShipController template = existing[0];
            Quaternion rotation =
                template.transform.rotation;

            for (int i = 0;
                 i < archetypes.Length;
                 i++)
            {
                EnemyShipController ship;
                if (i < existing.Length)
                {
                    ship = existing[i];
                }
                else
                {
                    ship = Instantiate(
                        template.gameObject,
                        positions[i],
                        rotation
                    ).GetComponent<
                        EnemyShipController>();
                }

                Vector3 position = positions[i];
                position.y = ship.transform.position.y;
                ship.transform.position = position;
                ship.ConfigureArchetype(archetypes[i]);
                ship.name =
                    $"{ship.name} #{i + 1}";
            }

            for (int i = archetypes.Length;
                 i < existing.Length;
                 i++)
            {
                existing[i].gameObject.SetActive(false);
                Destroy(existing[i].gameObject);
            }

            var population = Seaborn.World.PrototypePopulationDirector.EnsureCreated();
            foreach (var ship in FindObjectsByType<EnemyShipController>(FindObjectsSortMode.None))
            {
                if (ship.gameObject.activeInHierarchy) population.RegisterShip(ship);
            }
            initialized = true;
            Debug.Log(
                $"{MapLabel(scene)} filosu hazır: " +
                $"{archetypes.Length} düşman gemisi.",
                this
            );
        }

        private static EnemyShipArchetype[]
            ProfileArchetypes(string scene)
        {
            if (scene == "PrototypeWesternReach")
            {
                return new[]
                {
                    EnemyShipArchetype.Marauder,
                    EnemyShipArchetype.Gunship,
                    EnemyShipArchetype.Skirmisher,
                    EnemyShipArchetype.Gunship,
                    EnemyShipArchetype.Marauder
                };
            }

            if (scene == "PrototypeEasternReach")
            {
                return new[]
                {
                    EnemyShipArchetype.Skirmisher,
                    EnemyShipArchetype.Skirmisher,
                    EnemyShipArchetype.Marauder,
                    EnemyShipArchetype.Skirmisher,
                    EnemyShipArchetype.Marauder
                };
            }

            return new[]
            {
                EnemyShipArchetype.Skirmisher,
                EnemyShipArchetype.Marauder,
                EnemyShipArchetype.Skirmisher,
                EnemyShipArchetype.Marauder,
                EnemyShipArchetype.Gunship
            };
        }

        private static Vector3[]
            ProfilePositions(string scene)
        {
            return new[]
            {
                new Vector3(-38f, 0.5f, -24f),
                new Vector3(34f, 0.5f, -22f),
                new Vector3(-32f, 0.5f, 24f),
                new Vector3(36f, 0.5f, 26f),
                new Vector3(0f, 0.5f, 44f)
            };
        }

        private static string MapLabel(string scene)
        {
            return scene switch
            {
                "PrototypeWesternReach" =>
                    "Batı Sınırı",
                "PrototypeEasternReach" =>
                    "Doğu Avları",
                _ => "Merkez Sular"
            };
        }
    }
}
