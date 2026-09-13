using System;
using Seaborn.Combat;
using Seaborn.Hunting;
using Seaborn.Ship.Data;
using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class ShipProfileController : MonoBehaviour
    {
        [SerializeField] private string shipId = "starter_sloop";
        [SerializeField] private bool restoreHealthWhenApplied = true;

        public event Action<ShipDefinition> ProfileApplied;

        public string ShipId => shipId;
        public ShipDefinition Definition { get; private set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToPlayer()
        {
            HarpoonHuntingController player =
                UnityEngine.Object.FindFirstObjectByType<
                    HarpoonHuntingController>();

            if (player == null) return;

            ShipProfileController profile =
                player.GetComponent<ShipProfileController>();
            if (profile == null)
            {
                profile = player.gameObject.AddComponent<
                    ShipProfileController>();
            }

            profile.ApplyProfile();
        }

        private void Awake()
        {
            ApplyProfile();
        }

        [ContextMenu("Apply Ship Profile")]
        public void ApplyProfile()
        {
            if (!ShipCatalog.TryGet(shipId, out ShipDefinition definition))
            {
                Debug.LogError($"Unknown ship profile: {shipId}", this);
                return;
            }

            Definition = definition;

            ShipHealth health = GetComponentInChildren<ShipHealth>();
            health?.SetBaseMaximumHealth(
                definition.maximumHealth,
                restoreHealthWhenApplied
            );

            BroadsideController broadside =
                GetComponentInChildren<BroadsideController>();
            broadside?.SetShipConfiguration(
                definition.cannonSlots,
                definition.startingCannons,
                definition.cannonRange
            );

            ProfileApplied?.Invoke(definition);
        }

        public bool TrySelectProfile(string nextShipId, bool restoreHealth)
        {
            if (!ShipCatalog.TryGet(nextShipId, out _)) return false;

            shipId = nextShipId;
            restoreHealthWhenApplied = restoreHealth;
            ApplyProfile();
            return Definition != null;
        }
    }
}
