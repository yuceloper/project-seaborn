using System.Collections.Generic;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyCombatBalanceDirector : MonoBehaviour
    {
        private const float EnemyBaseHull = 2500f;

        private readonly Dictionary<EnemyShipController, EnemyShipArchetype> tuned = new();
        private float nextScanTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCreated()
        {
            EnemyCombatBalanceDirector existing =
                FindFirstObjectByType<EnemyCombatBalanceDirector>();
            if (existing != null) return;

            GameObject root = new("Enemy Combat Balance Director");
            root.AddComponent<EnemyCombatBalanceDirector>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime) return;
            nextScanTime = Time.unscaledTime + 0.5f;

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
    }
}
