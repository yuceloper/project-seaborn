using System;
using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class ShipSubsystemController :
        MonoBehaviour
    {
        private const float MaximumIntegrity = 100f;

        public event Action SubsystemsChanged;

        public float SailIntegrity { get; private set; } =
            MaximumIntegrity;
        public float CrewReadiness { get; private set; } =
            MaximumIntegrity;

        public float SailNormalized =>
            SailIntegrity / MaximumIntegrity;
        public float CrewNormalized =>
            CrewReadiness / MaximumIntegrity;
        public float MovementSpeedMultiplier =>
            Mathf.Lerp(0.55f, 1f, SailNormalized);
        public float TurnMultiplier =>
            Mathf.Lerp(0.7f, 1f, SailNormalized);
        public float MissingIntegrity =>
            MaximumIntegrity - SailIntegrity +
            MaximumIntegrity - CrewReadiness;

        private ShipMotor motor;
        private BroadsideController broadside;

        public static ShipSubsystemController EnsureAttached(
            Transform ship)
        {
            if (ship == null) return null;

            ShipSubsystemController controller =
                ship.GetComponent<
                    ShipSubsystemController>();

            if (controller == null)
            {
                controller = ship.gameObject.AddComponent<
                    ShipSubsystemController>();
            }

            controller.Bind();
            return controller;
        }

        private void Awake()
        {
            Bind();
            ApplyPenalties();
        }

        public void RegisterHit(DamageInfo damageInfo)
        {
            float sailDamage = 0f;
            float crewDamage = 0f;

            switch (damageInfo.AmmunitionType)
            {
                case AmmunitionType.Chain:
                    sailDamage =
                        damageInfo.Amount * 1.35f;
                    break;
                case AmmunitionType.Grapeshot:
                    crewDamage =
                        damageInfo.Amount * 0.75f;
                    break;
            }

            if (sailDamage <= 0f && crewDamage <= 0f)
            {
                return;
            }

            SailIntegrity = Mathf.Max(
                0f,
                SailIntegrity - sailDamage
            );
            CrewReadiness = Mathf.Max(
                0f,
                CrewReadiness - crewDamage
            );
            ApplyPenalties();
            SubsystemsChanged?.Invoke();
        }

        public void RestoreAll()
        {
            SailIntegrity = MaximumIntegrity;
            CrewReadiness = MaximumIntegrity;
            ApplyPenalties();
            SubsystemsChanged?.Invoke();
        }

        private void Bind()
        {
            Transform root = transform.root;
            if (motor == null)
            {
                motor = root.GetComponentInChildren<
                    ShipMotor>();
            }

            if (broadside == null)
            {
                broadside = root.GetComponentInChildren<
                    BroadsideController>();
            }
        }

        private void ApplyPenalties()
        {
            float sail = SailNormalized;
            motor?.SetDamagePerformance(
                MovementSpeedMultiplier,
                TurnMultiplier
            );

            float crew = CrewNormalized;
            broadside?.SetCrewReloadMultiplier(
                Mathf.Lerp(1.65f, 1f, crew)
            );
        }
    }
}
