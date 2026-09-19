using System;
using System.Collections;
using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Combat.Loot;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Seaborn.World
{
    // Scene-owned timers: unloading the map cancels its pending replacements.
    public sealed class PrototypePopulationDirector : MonoBehaviour
    {
        private readonly HashSet<ShipHealth> registeredShips = new();
        private bool huntsInitialized;
        public static PrototypePopulationDirector EnsureCreated()
        {
            var director = FindFirstObjectByType<PrototypePopulationDirector>();
            if (director != null) return director;
            return new GameObject("Map Population").AddComponent<PrototypePopulationDirector>();
        }

        public bool TryInitializeHunts()
        {
            if (huntsInitialized) return false;
            huntsInitialized = true;
            return true;
        }

        public void RegisterShip(EnemyShipController ship)
        {
            var health = ship.GetComponent<ShipHealth>();
            if (!registeredShips.Add(health)) return;
            var guns = ship.GetComponent<BroadsideController>();
            Vector3 home = ship.transform.position;
            int standard = guns.GetAmmunitionStock(AmmunitionType.Standard);
            int chain = guns.GetAmmunitionStock(AmmunitionType.Chain);
            int grapeshot = guns.GetAmmunitionStock(AmmunitionType.Grapeshot);
            health.Sunk += () =>
            {
                if (this != null && isActiveAndEnabled)
                    StartCoroutine(RespawnShip(ship, home, standard, chain, grapeshot));
            };
        }

        private IEnumerator RespawnShip(EnemyShipController ship, Vector3 home,
            int standard, int chain, int grapeshot)
        {
            yield return new WaitForSeconds(90f);
            Vector3 position;
            while (!TryFindPosition(home, out position))
            {
                if (ship == null) yield break;
                yield return new WaitForSeconds(5f);
            }
            if (ship == null || !ship.GetComponent<ShipHealth>().IsSunk) yield break;
            var sink = ship.GetComponent<ShipSinkController>();
            if (sink == null) yield break;
            // Reuse the same fleet slot, with all per-life state restored.
            ship.GetComponent<BroadsideController>().RestoreEnemyLife(standard, chain, grapeshot);
            ship.GetComponent<ShipSubsystemController>().RestoreAll();
            ship.GetComponent<PrototypeShipwreckLootSource>()?.ResetForRespawn();
            sink.RestoreAfterSinking(position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
            ship.SetPassive();
            ship.enabled = true;
        }

        public void RegisterHunt(PrototypeSeaCreature creature, Vector3 home,
            float delay, Action<Vector3> spawn)
        {
            creature.Harvested += () =>
            {
                if (this != null && isActiveAndEnabled)
                    StartCoroutine(RespawnHunt(home, delay, spawn));
            };
        }

        private IEnumerator RespawnHunt(Vector3 home, float delay, Action<Vector3> spawn)
        {
            yield return new WaitForSeconds(delay);
            Vector3 position;
            while (!TryFindPosition(home, out position))
                yield return new WaitForSeconds(5f);
            spawn(position);
        }

        private bool TryFindPosition(Vector3 home, out Vector3 position)
        {
            position = home;
            if (SceneManager.GetActiveScene().name == "PrototypeHarbor") return false;
            var player = FindFirstObjectByType<HarpoonHuntingController>();
            if (player == null) return false;
            float limit = PrototypeExpeditionRegionDirector.MapEdge - 12f;
            for (int attempt = 0; attempt < 24; attempt++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * 30f;
                Vector3 candidate = new Vector3(
                    Mathf.Clamp(home.x + offset.x, -limit, limit), home.y,
                    Mathf.Clamp(home.z + offset.y, -limit, limit));
                Vector3 delta = candidate - player.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude < 30f * 30f) continue;
                bool occupied = false;
                foreach (Collider collider in Physics.OverlapSphere(candidate + Vector3.up, 4f,
                    ~0, QueryTriggerInteraction.Ignore))
                {
                    // The ocean plane is a surface, not an obstacle.
                    if (collider.name == "Ocean") continue;
                    occupied = true;
                    break;
                }
                if (occupied) continue;
                position = candidate;
                return true;
            }
            return false;
        }
    }
}
