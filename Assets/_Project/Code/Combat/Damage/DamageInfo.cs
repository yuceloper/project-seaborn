using UnityEngine;

namespace Seaborn.Combat.Damage
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            Vector3 hitPoint,
            Vector3 hitDirection,
            GameObject instigator)
        {
            Amount = Mathf.Max(0f, amount);
            HitPoint = hitPoint;
            HitDirection = hitDirection.normalized;
            Instigator = instigator;
        }

        public float Amount { get; }

        public Vector3 HitPoint { get; }

        public Vector3 HitDirection { get; }

        public GameObject Instigator { get; }
    }
}
