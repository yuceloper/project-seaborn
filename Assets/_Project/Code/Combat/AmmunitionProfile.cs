using Seaborn.Equipment;
using UnityEngine;

namespace Seaborn.Combat
{
    public readonly struct AmmunitionProfile
    {
        public AmmunitionProfile(
            float damageMultiplier,
            float rangeMultiplier,
            float spreadMultiplier,
            float reloadMultiplier,
            int projectilesPerCannon,
            float projectileScale)
        {
            DamageMultiplier = damageMultiplier;
            RangeMultiplier = rangeMultiplier;
            SpreadMultiplier = spreadMultiplier;
            ReloadMultiplier = reloadMultiplier;
            ProjectilesPerCannon = projectilesPerCannon;
            ProjectileScale = projectileScale;
        }

        public float DamageMultiplier { get; }
        public float RangeMultiplier { get; }
        public float SpreadMultiplier { get; }
        public float ReloadMultiplier { get; }
        public int ProjectilesPerCannon { get; }
        public float ProjectileScale { get; }

        public static AmmunitionProfile Get(AmmunitionType type)
        {
            string catalogueId = type.ToString().ToLowerInvariant();
            if (EquipmentCatalog.TryGetAmmunition(
                    catalogueId,
                    out AmmunitionDefinition definition))
            {
                return new AmmunitionProfile(
                    definition.damageMultiplier,
                    definition.rangeMultiplier,
                    definition.spreadMultiplier,
                    definition.reloadMultiplier,
                    Mathf.Max(1, definition.projectilesPerCannon),
                    definition.projectileScale
                );
            }

            switch (type)
            {
                case AmmunitionType.Chain:
                    return new AmmunitionProfile(
                        0.5f,
                        0.85f,
                        1.1f,
                        1.08f,
                        1,
                        1.08f
                    );

                case AmmunitionType.Grapeshot:
                    return new AmmunitionProfile(
                        0.18f,
                        0.58f,
                        1.45f,
                        0.92f,
                        3,
                        0.62f
                    );

                default:
                    return new AmmunitionProfile(
                        1f,
                        1f,
                        1f,
                        1f,
                        1,
                        1f
                    );
            }
        }
    }
}
