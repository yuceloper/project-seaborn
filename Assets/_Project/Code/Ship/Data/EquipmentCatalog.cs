using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Equipment
{
    [Serializable]
    public sealed class CannonDefinition
    {
        public string id;
        public string displayName;
        [Min(1)] public int caliberPounds;
        [Min(0f)] public float damage;
        [Min(0.1f)] public float reloadDuration;
        [Min(0.1f)] public float range;
        [Range(0f, 1f)] public float accuracy = 1f;
        [Min(0)] public int silverPrice;
    }

    [Serializable]
    public sealed class AmmunitionDefinition
    {
        public string id;
        public string displayName;
        [Min(0f)] public float damageMultiplier = 1f;
        [Min(0.1f)] public float rangeMultiplier = 1f;
        [Min(0.1f)] public float spreadMultiplier = 1f;
        [Min(0.1f)] public float reloadMultiplier = 1f;
        [Min(1)] public int projectilesPerCannon = 1;
        [Min(0.1f)] public float projectileScale = 1f;
        [Min(0)] public int silverPricePerBundle;
        [Min(1)] public int bundleSize = 1;
    }

    [Serializable]
    public sealed class HarpoonDefinition
    {
        public string id;
        public string displayName;
        [Min(0.1f)] public float weightKg;
        [Min(0f)] public float damage;
        [Min(0.1f)] public float range;
        [Min(0.1f)] public float reloadDuration;
        [Min(0.1f)] public float flightDuration;
        [Min(0f)] public float arcHeight;
        [Min(0)] public int silverPricePerBundle;
        [Min(1)] public int bundleSize = 1;
    }

    [Serializable]
    public sealed class SailDefinition
    {
        public string id;
        public string displayName;
        [Min(0.1f)] public float speedMultiplier = 1f;
        [Min(0.1f)] public float maneuverMultiplier = 1f;
        [Min(0)] public int silverPrice;
    }

    [Serializable]
    public sealed class ConsumableDefinition
    {
        public string id;
        public string displayName;
        public string effectType;
        public string cooldownGroup;
        [Min(0f)] public float magnitude;
        [Min(0f)] public float duration;
        [Min(0f)] public float cooldown;
        [Min(0)] public int silverPrice;
        [Min(0)] public int goldPrice;
    }

    [Serializable]
    internal sealed class EquipmentCatalogDocument
    {
        public CannonDefinition[] cannons;
        public AmmunitionDefinition[] ammunition;
        public HarpoonDefinition[] harpoons;
        public SailDefinition[] sails;
        public ConsumableDefinition[] consumables;
    }

    public static class EquipmentCatalog
    {
        private const string ResourcePath = "Definitions/equipment";
        private static readonly Dictionary<string, CannonDefinition> Cannons =
            NewIndex<CannonDefinition>();
        private static readonly Dictionary<string, AmmunitionDefinition> Ammunition =
            NewIndex<AmmunitionDefinition>();
        private static readonly Dictionary<string, HarpoonDefinition> Harpoons =
            NewIndex<HarpoonDefinition>();
        private static readonly Dictionary<string, SailDefinition> Sails =
            NewIndex<SailDefinition>();
        private static readonly Dictionary<string, ConsumableDefinition> Consumables =
            NewIndex<ConsumableDefinition>();
        private static bool isLoaded;

        public static bool TryGetCannon(string id, out CannonDefinition value)
        {
            EnsureLoaded();
            return Cannons.TryGetValue(id ?? string.Empty, out value);
        }

        public static bool TryGetAmmunition(string id, out AmmunitionDefinition value)
        {
            EnsureLoaded();
            return Ammunition.TryGetValue(id ?? string.Empty, out value);
        }

        public static bool TryGetHarpoon(string id, out HarpoonDefinition value)
        {
            EnsureLoaded();
            return Harpoons.TryGetValue(id ?? string.Empty, out value);
        }

        public static bool TryGetSail(string id, out SailDefinition value)
        {
            EnsureLoaded();
            return Sails.TryGetValue(id ?? string.Empty, out value);
        }

        public static bool TryGetConsumable(string id, out ConsumableDefinition value)
        {
            EnsureLoaded();
            return Consumables.TryGetValue(id ?? string.Empty, out value);
        }

        private static Dictionary<string, T> NewIndex<T>()
        {
            return new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        private static void EnsureLoaded()
        {
            if (isLoaded) return;
            isLoaded = true;

            TextAsset source = Resources.Load<TextAsset>(ResourcePath);
            if (source == null)
            {
                Debug.LogError($"Equipment catalogue is missing: Resources/{ResourcePath}.json");
                return;
            }

            EquipmentCatalogDocument document =
                JsonUtility.FromJson<EquipmentCatalogDocument>(source.text);
            if (document == null)
            {
                Debug.LogError("Equipment catalogue JSON could not be parsed.");
                return;
            }

            Index(document.cannons, Cannons, item => item.id, "cannon");
            Index(document.ammunition, Ammunition, item => item.id, "ammunition");
            Index(document.harpoons, Harpoons, item => item.id, "harpoon");
            Index(document.sails, Sails, item => item.id, "sail");
            Index(document.consumables, Consumables, item => item.id, "consumable");
        }

        private static void Index<T>(
            T[] items,
            Dictionary<string, T> destination,
            Func<T, string> getId,
            string category)
        {
            if (items == null) return;

            foreach (T item in items)
            {
                if (ReferenceEquals(item, null)) continue;
                string id = getId(item);
                if (string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning($"Ignored {category} definition without an id.");
                    continue;
                }

                if (destination.ContainsKey(id))
                {
                    Debug.LogError($"Duplicate {category} id: {id}");
                    continue;
                }

                destination.Add(id, item);
            }
        }
    }
}
