using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Ship.Data
{
    [Serializable]
    public sealed class ShipDefinition
    {
        public string id;
        public string displayName;
        [Min(0)] public int basePrice;
        [Min(1)] public int cannonSlots = 1;
        [Min(1)] public int startingCannons = 1;
        [Min(0.1f)] public float cannonRange = 10f;
        [Min(0.1f)] public float speedMultiplier = 1f;
        [Min(1f)] public float maximumHealth = 100f;
        [Min(0)] public int sailSlots = 1;
        [Min(0f)] public float repairAmount;
        [Min(0.1f)] public float repairInterval = 5f;
        [Min(0f)] public float defense;
        [Min(0.1f)] public float maneuverMultiplier = 1f;
        [Min(0)] public int cargoCapacity;
        [Min(0)] public int specialSlots;
        [Min(0)] public int deckExtensionLimit;
    }

    [Serializable]
    internal sealed class ShipCatalogDocument
    {
        public ShipDefinition[] ships;
    }

    public static class ShipCatalog
    {
        private const string ResourcePath = "Definitions/ships";
        private static readonly Dictionary<string, ShipDefinition> Definitions =
            new Dictionary<string, ShipDefinition>(StringComparer.OrdinalIgnoreCase);
        private static bool isLoaded;

        public static IReadOnlyCollection<ShipDefinition> All
        {
            get
            {
                EnsureLoaded();
                return Definitions.Values;
            }
        }

        public static bool TryGet(string shipId, out ShipDefinition definition)
        {
            EnsureLoaded();
            return Definitions.TryGetValue(shipId ?? string.Empty, out definition);
        }

        public static ShipDefinition GetRequired(string shipId)
        {
            if (TryGet(shipId, out ShipDefinition definition))
            {
                return definition;
            }

            throw new InvalidOperationException(
                $"Ship definition '{shipId}' was not found in Resources/{ResourcePath}.json.");
        }

        private static void EnsureLoaded()
        {
            if (isLoaded) return;
            isLoaded = true;

            TextAsset source = Resources.Load<TextAsset>(ResourcePath);
            if (source == null)
            {
                Debug.LogError($"Ship catalogue is missing: Resources/{ResourcePath}.json");
                return;
            }

            ShipCatalogDocument document = JsonUtility.FromJson<ShipCatalogDocument>(source.text);
            if (document?.ships == null)
            {
                Debug.LogError("Ship catalogue contains no ship definitions.");
                return;
            }

            foreach (ShipDefinition ship in document.ships)
            {
                if (ship == null || string.IsNullOrWhiteSpace(ship.id))
                {
                    Debug.LogWarning("Ignored a ship definition without an id.");
                    continue;
                }

                if (Definitions.ContainsKey(ship.id))
                {
                    Debug.LogError($"Duplicate ship id in catalogue: {ship.id}");
                    continue;
                }

                ship.cannonSlots = Mathf.Max(1, ship.cannonSlots);
                ship.startingCannons = Mathf.Clamp(ship.startingCannons, 1, ship.cannonSlots);
                ship.cannonRange = Mathf.Max(0.1f, ship.cannonRange);
                ship.maximumHealth = Mathf.Max(1f, ship.maximumHealth);
                Definitions.Add(ship.id, ship);
            }
        }

#if UNITY_EDITOR
        public static void ReloadForEditor()
        {
            Definitions.Clear();
            isLoaded = false;
        }
#endif
    }
}
