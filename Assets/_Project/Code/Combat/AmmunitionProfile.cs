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
            switch (type)
            {
                case AmmunitionType.Chain:
                    return new AmmunitionProfile(
                        0.68f,
                        0.82f,
                        1.2f,
                        1.18f,
                        1,
                        1.08f
                    );

                case AmmunitionType.Grapeshot:
                    return new AmmunitionProfile(
                        0.28f,
                        0.56f,
                        1.65f,
                        0.88f,
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
