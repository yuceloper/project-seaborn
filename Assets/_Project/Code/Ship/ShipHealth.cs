using System;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Ship
{
    public sealed class ShipHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)]
        private float maximumHealth = 100f;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> Damaged;
        public event Action Sunk;

        public float CurrentHealth { get; private set; }

        public float MaximumHealth => maximumHealth;

        public bool IsSunk { get; private set; }

        private void Awake()
        {
            CurrentHealth = maximumHealth;
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (IsSunk || damageInfo.Amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(
                0f,
                CurrentHealth - damageInfo.Amount
            );

            Damaged?.Invoke(damageInfo);
            HealthChanged?.Invoke(
                CurrentHealth,
                maximumHealth
            );

            if (CurrentHealth > 0f)
            {
                return;
            }

            IsSunk = true;
            Sunk?.Invoke();
        }

        [ContextMenu("Reset Health")]
        public void RestoreToFullHealth()
        {
            CurrentHealth = maximumHealth;
            IsSunk = false;

            HealthChanged?.Invoke(
                CurrentHealth,
                maximumHealth
            );
        }

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
        }
    }
}
