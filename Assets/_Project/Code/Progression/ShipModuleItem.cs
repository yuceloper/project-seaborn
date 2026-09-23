using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum ShipModuleSlot { Sail, Hull }

    [Serializable]
    public sealed class ShipModuleItem
    {
        public const int MaximumEnhancement = 10;
        [SerializeField] private string instanceId;
        [SerializeField] private string definitionId;
        [SerializeField] private int enhancement;
        public string InstanceId => instanceId;
        public string DefinitionId => definitionId;
        public int Enhancement => enhancement;
        public ShipModuleDefinition Definition => ShipModuleCatalog.Find(definitionId);
        public ShipModuleStats Stats => Definition.StatsAt(enhancement);
        internal static ShipModuleItem Create(string id) => new ShipModuleItem
            { instanceId = Guid.NewGuid().ToString("N"), definitionId = id, enhancement = 0 };
        internal ShipModuleItem Copy() => new ShipModuleItem
            { instanceId = instanceId, definitionId = definitionId?.ToLowerInvariant(), enhancement = Mathf.Clamp(enhancement, 0, 10) };
        internal void Improve() => enhancement = Mathf.Min(10, enhancement + 1);
    }

    public readonly struct ShipModuleStats
    {
        public readonly float Speed, Acceleration, Turning, HullHealth, SailDurability;
        public ShipModuleStats(float speed = 1, float acceleration = 1, float turning = 1, float hullHealth = 1, float sailDurability = 1)
        { Speed = speed; Acceleration = acceleration; Turning = turning; HullHealth = hullHealth; SailDurability = sailDurability; }
        public static ShipModuleStats Empty(ShipModuleSlot slot) => slot == ShipModuleSlot.Sail
            ? new ShipModuleStats(0.45f, 0.5f, 0.75f, 1, 0.7f) : new ShipModuleStats(1, 1, 1, 1, 1);
    }

    public readonly struct ShipModuleCost
    {
        public readonly int Silver, Oil, Iron, Charts, Scales;
        public ShipModuleCost(int silver, int oil = 0, int iron = 0, int charts = 0, int scales = 0)
        { Silver = silver; Oil = oil; Iron = iron; Charts = charts; Scales = scales; }
        public static ShipModuleCost Next(ShipModuleItem item)
        {
            if (item == null || item.Enhancement >= 10) return default;
            int n = item.Enhancement + 1;
            bool sail = item.Definition.Slot == ShipModuleSlot.Sail;
            return new ShipModuleCost((sail ? 45 : 55) * n * n, sail ? n : 0, sail ? 0 : n + 1,
                Mathf.Max(0, n - 5), Mathf.Max(0, n - 8));
        }
    }

    public sealed class ShipModuleDefinition
    {
        public readonly string Id, Name, Role;
        public readonly ShipModuleSlot Slot;
        public readonly ShipModuleCost PurchaseCost;
        private readonly ShipModuleStats stats;
        public ShipModuleDefinition(string id, string name, string role, ShipModuleSlot slot, ShipModuleCost price, ShipModuleStats stats)
        { Id = id; Name = name; Role = role; Slot = slot; PurchaseCost = price; this.stats = stats; }
        public ShipModuleStats StatsAt(int level)
        {
            int n = Mathf.Clamp(level, 0, 10);
            return Slot == ShipModuleSlot.Sail
                ? new ShipModuleStats(stats.Speed * (1 + n * 0.01f), stats.Acceleration * (1 + n * 0.015f),
                    stats.Turning, 1, stats.SailDurability * (1 + n * 0.03f))
                : new ShipModuleStats(stats.Speed, stats.Acceleration, stats.Turning, stats.HullHealth * (1 + n * 0.03f), 1);
        }
    }

    public static class ShipModuleCatalog
    {
        private static readonly ShipModuleDefinition[] entries =
        {
            new("patched_canvas", "Yamalı Yelken", "Dengeli başlangıç yelkeni", ShipModuleSlot.Sail,
                new ShipModuleCost(180), new ShipModuleStats(1, 1, 1, 1, 1)),
            new("rat_sails", "Rat Yelkeni", "Seyir hızı", ShipModuleSlot.Sail,
                new ShipModuleCost(1800), new ShipModuleStats(1.08f, 1.08f, 1.04f, 1, 1)),
            new("swift_linen", "Çevik Keten Yelken", "Hızlı hızlanma, daha narin kumaş", ShipModuleSlot.Sail,
                new ShipModuleCost(1000, oil: 3), new ShipModuleStats(1.02f, 1.16f, 1.02f, 1, 0.9f)),
            new("storm_canvas", "Fırtına Yelkeni", "Dayanıklı kumaş, daha yavaş seyir", ShipModuleSlot.Sail,
                new ShipModuleCost(1400, oil: 5), new ShipModuleStats(0.96f, 0.96f, 1, 1, 1.35f)),
            new("timber_plating", "Ahşap Kaplama", "Dengeli başlangıç kaplaması", ShipModuleSlot.Hull,
                new ShipModuleCost(450), new ShipModuleStats(1, 1, 1, 1, 1)),
            new("light_plating", "Hafif Kaplama", "Hareket avantajı, daha az koruma", ShipModuleSlot.Hull,
                new ShipModuleCost(1100, iron: 4), new ShipModuleStats(1.04f, 1.08f, 1.06f, 0.95f, 1)),
            new("reinforced_plating", "Ağır Kaplama", "Daha fazla gövde canı, daha ağır hareket", ShipModuleSlot.Hull,
                new ShipModuleCost(1500, iron: 8), new ShipModuleStats(0.94f, 0.90f, 0.94f, 1.2f, 1))
        };
        public static IReadOnlyList<ShipModuleDefinition> All { get; } = Array.AsReadOnly(entries);
        public static ShipModuleDefinition Find(string id)
        {
            foreach (var entry in entries)
                if (string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)) return entry;
            return null;
        }
    }
}
