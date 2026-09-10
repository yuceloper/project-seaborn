using UnityEngine;

namespace Seaborn.Combat.Damage
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            Vector3 hitPoint,
            Vector3 hitDirection,
            GameObject instigator,
            AmmunitionType ammunitionType =
                AmmunitionType.Standard)
        {
            Amount = Mathf.Max(0f, amount);
            HitPoint = hitPoint;
            HitDirection = hitDirection.normalized;
            Instigator = instigator;
            AmmunitionType = ammunitionType;
        }

        public float Amount { get; }

        public Vector3 HitPoint { get; }

        public Vector3 HitDirection { get; }

        public GameObject Instigator { get; }

        public AmmunitionType AmmunitionType { get; }
    }
}
