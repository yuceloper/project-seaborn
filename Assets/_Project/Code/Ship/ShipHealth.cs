using System;
using Seaborn.Combat.Damage;
using Seaborn.Expeditions;
using UnityEngine;

namespace Seaborn.Ship
{
    public sealed class ShipHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)]
        private float maximumHealth = 2500f;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> Damaged;
        public event Action Sunk;

        public float CurrentHealth { get; private set; }

        public float MaximumHealth =>
            maximumHealth *
            maximumHealthMultiplier *
            equipmentHealthMultiplier *
            skillHealthMultiplier * hullModuleHealthMultiplier;

        public float MaximumHealthMultiplier =>
            maximumHealthMultiplier;

        private float hullModuleHealthMultiplier = 1f;
        private float maximumHealthMultiplier = 1f;
        private float equipmentHealthMultiplier = 1f;
        private float skillHealthMultiplier = 1f;
        private float consumableDamageTakenMultiplier = 1f;
        private ShipSubsystemController subsystems;

        public bool IsSunk { get; private set; }

        private void Awake()
        {
            subsystems =
                ShipSubsystemController.EnsureAttached(
                    transform
                );
            CurrentHealth = MaximumHealth;
            if (GetComponent<ShipCombatFeedback>() == null)
                gameObject.AddComponent<ShipCombatFeedback>();
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
                CurrentHealth - damageInfo.Amount *
                consumableDamageTakenMultiplier
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

        public void SetHullModuleHealthMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(0.1f, multiplier);
            if (Mathf.Approximately(hullModuleHealthMultiplier, multiplier)) return;
            hullModuleHealthMultiplier = multiplier;
            // Equipment swaps/upgrades must not heal or resurrect a ship.
            CurrentHealth = Mathf.Min(CurrentHealth, MaximumHealth);
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
        }

        public void SetConsumableDamageTakenMultiplier(
            float multiplier)
        {
            consumableDamageTakenMultiplier =
                Mathf.Clamp(multiplier, 0.1f, 1f);
        }

        public void SetBaseMaximumHealth(
            float value,
            bool restoreToFull)
        {
            maximumHealth = Mathf.Max(1f, value);
            CurrentHealth = restoreToFull
                ? MaximumHealth
                : Mathf.Min(CurrentHealth, MaximumHealth);
            IsSunk = CurrentHealth <= 0f;
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
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

        public void SetSkillHealthMultiplier(
            float multiplier,
            bool restoreToFull)
        {
            skillHealthMultiplier =
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

        public float RestoreHealth(float amount)
        {
            if (IsSunk || amount <= 0f)
            {
                return 0f;
            }

            float before = CurrentHealth;
            CurrentHealth = Mathf.Min(
                MaximumHealth,
                CurrentHealth + amount
            );
            float restored = CurrentHealth - before;
            if (restored > 0f)
            {
                HealthChanged?.Invoke(
                    CurrentHealth,
                    MaximumHealth
                );
            }
            return restored;
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
