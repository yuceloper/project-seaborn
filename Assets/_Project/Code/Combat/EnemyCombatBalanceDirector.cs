using System.Collections.Generic;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyCombatBalanceDirector : MonoBehaviour
    {
        // Three real cannons per side on the prototype Sloop (six total).
        // Retain the original 4/5/7 broadside pacing without phantom shots.
        private const float EnemyBaseHull = 1250f;

        private readonly Dictionary<EnemyShipController, EnemyShipArchetype> tuned = new();
        private float nextScanTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCreated()
        {
            EnemyCombatBalanceDirector existing =
                FindFirstObjectByType<EnemyCombatBalanceDirector>();
            if (existing != null)
            {
                DontDestroyOnLoad(existing.gameObject);
                return;
            }

            GameObject root = new("Enemy Combat Balance Director");
            root.AddComponent<EnemyCombatBalanceDirector>();
            DontDestroyOnLoad(root);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime) return;
            nextScanTime = Time.unscaledTime + 0.25f;

            PruneDestroyedEnemies();

            EnemyShipController[] enemies =
                FindObjectsByType<EnemyShipController>(FindObjectsSortMode.None);

            foreach (EnemyShipController enemy in enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

                if (tuned.TryGetValue(enemy, out EnemyShipArchetype applied) &&
                    applied == enemy.Archetype)
                {
                    continue;
                }

                // Scene/prefab instances still serialize the old prototype hull.
                // Force the authored combat baseline every time a newly loaded
                // enemy is discovered, then preserve its archetype multiplier.
                ShipHealth health = enemy.GetComponent<ShipHealth>();
                health?.SetBaseMaximumHealth(EnemyBaseHull, true);

                BroadsideController broadside =
                    enemy.GetComponent<BroadsideController>();
                if (broadside == null) continue;

                switch (enemy.Archetype)
                {
                    case EnemyShipArchetype.Skirmisher:
                        broadside.SetEquipmentModifiers(2.4f, 1.8f);
                        break;
                    case EnemyShipArchetype.Gunship:
                        broadside.SetEquipmentModifiers(3.2f, 2.0f);
                        break;
                    default:
                        broadside.SetEquipmentModifiers(2.8f, 1.9f);
                        break;
                }

                tuned[enemy] = enemy.Archetype;
            }
        }

        private void PruneDestroyedEnemies()
        {
            if (tuned.Count == 0) return;

            List<EnemyShipController> stale = null;
            foreach (EnemyShipController enemy in tuned.Keys)
            {
                if (enemy != null) continue;
                stale ??= new List<EnemyShipController>();
                stale.Add(enemy);
            }

            if (stale == null) return;
            foreach (EnemyShipController enemy in stale)
            {
                tuned.Remove(enemy);
            }
        }
    }
}

