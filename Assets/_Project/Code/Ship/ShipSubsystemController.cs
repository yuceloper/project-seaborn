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
                    // Integrity uses a 100-point scale independently of hull HP.
                    // Limit a single chain impact while retaining salvo pressure.
                    sailDamage =
                        Mathf.Min(8f, damageInfo.Amount * 0.20f);
                    break;
                case AmmunitionType.Grapeshot:
                    // Each cannon fires three pellets. Count their combined pressure:
                    // six 6 lb cannons cause ~33 readiness loss on perfect hits.
                    // A stronger individual pellet cannot remove more than 3 points.
                    crewDamage =
                        Mathf.Min(3f, damageInfo.Amount * 0.12f);
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
