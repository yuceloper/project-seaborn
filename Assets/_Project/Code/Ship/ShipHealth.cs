using System;
using Seaborn.Combat.Damage;
using Seaborn.Expeditions;
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

        public float MaximumHealth =>
            maximumHealth *
            maximumHealthMultiplier *
            equipmentHealthMultiplier;

        public float MaximumHealthMultiplier =>
            maximumHealthMultiplier;

        private float maximumHealthMultiplier = 1f;
        private float equipmentHealthMultiplier = 1f;
        private ShipSubsystemController subsystems;

        public bool IsSunk { get; private set; }

        private void Awake()
        {
            subsystems =
                ShipSubsystemController.EnsureAttached(
                    transform
                );
            CurrentHealth = MaximumHealth;
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (IsSunk ||
                damageInfo.Amount <= 0f ||
                PrototypeSafeHarborProtection.IsDamageProtected(
                    this
                ))
            {
                return;
            }

            subsystems?.RegisterHit(damageInfo);

            CurrentHealth = Mathf.Max(
                0f,
                CurrentHealth - damageInfo.Amount
            );

            Damaged?.Invoke(damageInfo);
            HealthChanged?.Invoke(
                CurrentHealth,
                MaximumHealth
            );

            if (CurrentHealth > 0f)
            {
                return;
            }

            IsSunk = true;
            Sunk?.Invoke();
        }

        public void SetRuntimeMaximumHealthMultiplier(
            float multiplier,
            bool restoreToFull)
        {
            maximumHealthMultiplier =
                Mathf.Max(0.1f, multiplier);
            CurrentHealth = restoreToFull
                ? MaximumHealth
                : Mathf.Min(CurrentHealth, MaximumHealth);
            IsSunk = CurrentHealth <= 0f;
            HealthChanged?.Invoke(
                CurrentHealth,
                MaximumHealth
            );
        }

        public void SetEquipmentHealthMultiplier(
            float multiplier,
            bool restoreToFull)
        {
            equipmentHealthMultiplier =
                Mathf.Max(0.1f, multiplier);
            CurrentHealth = restoreToFull
                ? MaximumHealth
                : Mathf.Min(CurrentHealth, MaximumHealth);
            IsSunk = CurrentHealth <= 0f;
            HealthChanged?.Invoke(
                CurrentHealth,
                MaximumHealth
            );
        }

        public void ResetRuntimeMaximumHealth(
            bool restoreToFull)
        {
            SetRuntimeMaximumHealthMultiplier(
                1f,
                restoreToFull
            );
        }

        [ContextMenu("Reset Health")]
        public void RestoreToFullHealth()
        {
            CurrentHealth = MaximumHealth;
            IsSunk = false;

            HealthChanged?.Invoke(
                CurrentHealth,
                MaximumHealth
            );
        }

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
        }
    }
}
