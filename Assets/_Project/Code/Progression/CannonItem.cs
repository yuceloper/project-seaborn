using System;
using UnityEngine;

namespace Seaborn.Progression
{
    // Identity and enhancement belong to the item, never to a hull or hardpoint.
    [Serializable]
    public sealed class CannonItem
    {
        public const int MaximumEnhancement = 10;
        [SerializeField] private string instanceId;
        [SerializeField] private string definitionId;
        [SerializeField] private int enhancement;
        public string InstanceId => instanceId;
        public string DefinitionId => definitionId;
        public int Enhancement => enhancement;
        public float DamageMultiplier => DamageAt(enhancement);
        public static float DamageAt(int level) => 1f + Mathf.Clamp(level, 0, MaximumEnhancement) * 0.08f;

        internal static CannonItem Create(string definition) => new CannonItem
        {
            instanceId = Guid.NewGuid().ToString("N"), definitionId = definition, enhancement = 0
        };
        internal CannonItem Copy() => new CannonItem
        {
            instanceId = instanceId, definitionId = definitionId?.ToLowerInvariant(),
            enhancement = Mathf.Clamp(enhancement, 0, MaximumEnhancement)
        };
        internal void Improve() => enhancement = Mathf.Min(MaximumEnhancement, enhancement + 1);
    }

    public readonly struct CannonUpgradeCost
    {
        public readonly int Silver, Iron, Charts, Scales;
        private CannonUpgradeCost(int silver, int iron, int charts, int scales)
        { Silver = silver; Iron = iron; Charts = charts; Scales = scales; }

        public static CannonUpgradeCost Next(CannonItem item)
        {
            if (item == null || item.Enhancement >= CannonItem.MaximumEnhancement) return default;
            int next = item.Enhancement + 1;
            int silverStep = item.DefinitionId == "iron_12lb" ? 60 : 40;
            return new CannonUpgradeCost(silverStep * next * next, next,
                Mathf.Max(0, next - 5), Mathf.Max(0, next - 8));
        }
    }
}
