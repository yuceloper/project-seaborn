using System;
using Seaborn.Harbor;
using Seaborn.Hunting;
using Seaborn.Ship;
using Seaborn.Ship.Data;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum ShipMarketResult
    {
        Completed,
        NotAtShipyard,
        AlreadyOwned,
        NotOwned,
        ActiveShip,
        UnknownShip,
        InsufficientSilver,
        LevelLocked
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeFleetInventory : MonoBehaviour
    {
        [SerializeField] private bool ownsStarterSloop = true;
        [SerializeField] private bool ownsRatSails;
        [SerializeField] private bool ownsDreadwake;
        [SerializeField] private string activeShipId =
            "starter_sloop";

        public event Action FleetChanged;

        public string ActiveShipId => activeShipId;
        public bool OwnsStarterSloop => ownsStarterSloop;
        public bool OwnsRatSails => ownsRatSails;
        public bool OwnsDreadwake => ownsDreadwake;

        public static PrototypeFleetInventory EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeFleetInventory fleet =
                player.GetComponent<PrototypeFleetInventory>();
            if (fleet == null)
            {
                fleet = player.gameObject.AddComponent<
                    PrototypeFleetInventory>();
            }

            fleet.Bind(player);
            return fleet;
        }

        public bool CanUseShipyard
        {
            get
            {
                PrototypeHarborDockingDirector docking =
                    PrototypeHarborDockingDirector.Instance;
                return Seaborn.World
                        .PrototypeExpeditionRegionDirector
                        .IsHarborScene &&
                    docking != null &&
                    docking.IsDockedAt(
                        PrototypeHarborStation.Shipyard);
            }
        }

        public bool Owns(string shipId)
        {
            if (string.Equals(
                    shipId,
                    "starter_sloop",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ownsStarterSloop;
            }

            if (string.Equals(
                    shipId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ownsRatSails;
            }

            return string.Equals(
                    shipId,
                    "dreadwake",
                    StringComparison.OrdinalIgnoreCase) &&
                ownsDreadwake;
        }

        public ShipMarketResult TryPurchase(string shipId)
        {
            if (!CanUseShipyard)
                return ShipMarketResult.NotAtShipyard;
            if (!ShipCatalog.TryGet(
                    shipId,
                    out ShipDefinition definition))
            {
                return ShipMarketResult.UnknownShip;
            }
            if (Owns(shipId))
                return ShipMarketResult.AlreadyOwned;
            if (progression == null ||
                !progression.MeetsLevel(
                    definition.requiredCaptainLevel))
            {
                return ShipMarketResult.LevelLocked;
            }
            if (wallet == null ||
                !wallet.TrySpendSilver(
                    definition.basePrice,
                    definition.displayName))
            {
                return ShipMarketResult.InsufficientSilver;
            }

            SetOwned(shipId, true);
            FleetChanged?.Invoke();
            return ShipMarketResult.Completed;
        }

        public ShipMarketResult TryActivate(string shipId)
        {
            if (!CanUseShipyard)
                return ShipMarketResult.NotAtShipyard;
            if (!ShipCatalog.TryGet(shipId, out _))
                return ShipMarketResult.UnknownShip;
            if (!Owns(shipId))
                return ShipMarketResult.NotOwned;
            if (string.Equals(
                    activeShipId,
                    shipId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return ShipMarketResult.ActiveShip;
            }

            if (!ApplyActiveShip(shipId, true))
                return ShipMarketResult.UnknownShip;

            activeShipId = shipId;
            FleetChanged?.Invoke();
            return ShipMarketResult.Completed;
        }

        public void Restore(
            bool starter,
            bool rat,
            bool dreadwake,
            string savedActiveShipId)
        {
            ownsStarterSloop = true;
            ownsRatSails = rat;
            ownsDreadwake = dreadwake;

            string requested =
                !string.IsNullOrWhiteSpace(savedActiveShipId) &&
                Owns(savedActiveShipId)
                    ? savedActiveShipId
                    : "starter_sloop";

            activeShipId = requested;
            ApplyActiveShip(activeShipId, true);
            FleetChanged?.Invoke();
        }

        private PrototypeSilverWallet wallet;
        private PrototypeCaptainProgression progression;
        private ShipProfileController profile;
        private ShipLoadout loadout;
        private PrototypeEquipmentInventory equipmentInventory;

        private void Bind(Transform player)
        {
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            progression = player.GetComponentInChildren<
                PrototypeCaptainProgression>();
            profile = player.GetComponent<
                ShipProfileController>();
            loadout = player.GetComponentInChildren<
                ShipLoadout>();
            equipmentInventory =
                player.GetComponentInChildren<
                    PrototypeEquipmentInventory>();

            if (profile != null &&
                !string.Equals(
                    profile.ShipId,
                    activeShipId,
                    StringComparison.OrdinalIgnoreCase))
            {
                ApplyActiveShip(activeShipId, false);
            }
        }

        private bool ApplyActiveShip(
            string shipId,
            bool restoreHealth)
        {
            if (profile == null)
            {
                profile = GetComponent<
                    ShipProfileController>();
            }
            if (loadout == null)
            {
                loadout = GetComponentInChildren<
                    ShipLoadout>();
            }

            if (profile == null ||
                !profile.TrySelectProfile(
                    shipId,
                    restoreHealth))
            {
                return false;
            }

            loadout?.Apply();

            if (equipmentInventory != null &&
                loadout != null &&
                equipmentInventory.CanUseShipyard)
            {
                equipmentInventory.TryEquipCannons(
                    loadout.CannonId
                );
            }

            return true;
        }

        private void SetOwned(string shipId, bool value)
        {
            if (string.Equals(
                    shipId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
            {
                ownsRatSails = value;
            }
            else if (string.Equals(
                         shipId,
                         "dreadwake",
                         StringComparison.OrdinalIgnoreCase))
            {
                ownsDreadwake = value;
            }
            else
            {
                ownsStarterSloop = true;
            }
        }
    }
}
