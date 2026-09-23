using System;
using Seaborn.Equipment;
using Seaborn.Harbor;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum EquipmentPurchaseResult
    {
        Completed,
        NotAtShipyard,
        UnknownItem,
        InsufficientSilver,
        InsufficientMaterials
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeEquipmentInventory : MonoBehaviour
    {
        [SerializeField, Min(0)] private int iron6LbCannons = 6;
        [SerializeField, Min(0)] private int iron12LbCannons;
        [SerializeField, Min(0)] private int patchedCanvasSails = 1;
        [SerializeField, Min(0)] private int ratSails;

        public event Action InventoryChanged;

        public int Iron6LbCannons => iron6LbCannons;
        public int Iron12LbCannons => iron12LbCannons;
        public int PatchedCanvasSails => patchedCanvasSails;
        public int RatSails => ratSails;

        // Ownership already includes fitted items. The depot shows only the spare count.
        public int GetStoredCannons(string id)
        {
            if (loadout == null) loadout = GetComponentInChildren<ShipLoadout>();
            int fitted = loadout != null ? loadout.CountCannons(id) : 0;
            return Mathf.Max(0, GetOwnedCannons(id) - fitted);
        }

        public int GetStoredSails(string id)
        {
            if (loadout == null) loadout = GetComponentInChildren<ShipLoadout>();
            int fitted = loadout != null && string.Equals(loadout.SailId, id,
                StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            return Mathf.Max(0, GetOwnedSails(id) - fitted);
        }

        public static PrototypeEquipmentInventory EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeEquipmentInventory inventory =
                player.GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null)
            {
                inventory = player.gameObject.AddComponent<
                    PrototypeEquipmentInventory>();
            }

            inventory.Bind(player);
            return inventory;
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

        public int GetOwnedCannons(string cannonId)
        {
            if (string.Equals(
                    cannonId,
                    "iron_12lb",
                    StringComparison.OrdinalIgnoreCase))
            {
                return iron12LbCannons;
            }

            return string.Equals(
                    cannonId,
                    "iron_6lb",
                    StringComparison.OrdinalIgnoreCase)
                ? iron6LbCannons
                : 0;
        }

        public int GetOwnedSails(string sailId)
        {
            if (string.Equals(
                    sailId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ratSails;
            }

            return string.Equals(
                    sailId,
                    "patched_canvas",
                    StringComparison.OrdinalIgnoreCase)
                ? patchedCanvasSails
                : 0;
        }

        public EquipmentPurchaseResult TryPurchaseCannon(
            string cannonId)
        {
            if (!CanUseShipyard)
                return EquipmentPurchaseResult.NotAtShipyard;

            if (!EquipmentCatalog.TryGetCannon(
                    cannonId,
                    out CannonDefinition definition))
            {
                return EquipmentPurchaseResult.UnknownItem;
            }

            if (wallet == null ||
                !wallet.TrySpendSilver(
                    definition.silverPrice,
                    definition.displayName))
            {
                return EquipmentPurchaseResult
                    .InsufficientSilver;
            }

            if (string.Equals(
                    cannonId,
                    "iron_12lb",
                    StringComparison.OrdinalIgnoreCase))
            {
                iron12LbCannons++;
            }
            else
            {
                iron6LbCannons++;
            }

            InventoryChanged?.Invoke();
            return EquipmentPurchaseResult.Completed;
        }

        public const int HeavyForgeSilver = 360;
        public const int HeavyForgeIron = 4;
        private bool forging;

        public EquipmentPurchaseResult TryForgeHeavyCannon()
        {
            if (!CanUseShipyard || forging) return EquipmentPurchaseResult.NotAtShipyard;
            var depot = GetComponent<PrototypeRegionalLootInventory>();
            if (depot == null || !depot.CanAfford(RegionalMaterialType.CorsairIron, HeavyForgeIron))
                return EquipmentPurchaseResult.InsufficientMaterials;
            if (wallet == null || wallet.Silver < HeavyForgeSilver)
                return EquipmentPurchaseResult.InsufficientSilver;
            forging = true;
            try
            {
                if (!wallet.TrySpendSilver(HeavyForgeSilver, "12 lb top üretimi"))
                    return EquipmentPurchaseResult.InsufficientSilver;
                if (!depot.TrySpend(RegionalMaterialType.CorsairIron, HeavyForgeIron))
                {
                    // Refund without recording expedition income.
                    wallet.RestoreSilver(wallet.Silver + HeavyForgeSilver);
                    return EquipmentPurchaseResult.InsufficientMaterials;
                }
                iron12LbCannons++;
                InventoryChanged?.Invoke();
                return EquipmentPurchaseResult.Completed;
            }
            finally { forging = false; }
        }

        public EquipmentPurchaseResult TryPurchaseSail(
            string sailId)
        {
            if (!CanUseShipyard)
                return EquipmentPurchaseResult.NotAtShipyard;

            if (!EquipmentCatalog.TryGetSail(
                    sailId,
                    out SailDefinition definition))
            {
                return EquipmentPurchaseResult.UnknownItem;
            }

            if (wallet == null ||
                !wallet.TrySpendSilver(
                    definition.silverPrice,
                    definition.displayName))
            {
                return EquipmentPurchaseResult
                    .InsufficientSilver;
            }

            if (string.Equals(
                    sailId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
            {
                ratSails++;
            }
            else
            {
                patchedCanvasSails++;
            }

            InventoryChanged?.Invoke();
            return EquipmentPurchaseResult.Completed;
        }

        public bool TryEquipCannons(string cannonId)
        {
            int owned = GetOwnedCannons(cannonId);
            if (!CanUseShipyard || owned <= 0 || loadout == null)
                return false;

            ShipProfileController profile =
                GetComponent<ShipProfileController>();
            int capacity = profile?.Definition != null
                ? profile.EffectiveCannonSlots
                : owned;

            return loadout.TryEquipCannons(
                cannonId,
                Mathf.Min(owned, capacity)
            );
        }

        public bool TryEquipSail(string sailId)
        {
            return CanUseShipyard &&
                GetOwnedSails(sailId) > 0 &&
                loadout != null &&
                loadout.TryEquipSail(sailId);
        }

        public void Restore(
            int sixLb,
            int twelveLb,
            int patched,
            int rat)
        {
            iron6LbCannons = Mathf.Max(0, sixLb);
            iron12LbCannons = Mathf.Max(0, twelveLb);
            patchedCanvasSails = Mathf.Max(0, patched);
            ratSails = Mathf.Max(0, rat);

            // Legacy saves begin with the starter loadout.
            if (iron6LbCannons == 0 &&
                iron12LbCannons == 0)
            {
                iron6LbCannons = 6;
            }

            if (patchedCanvasSails == 0 &&
                ratSails == 0)
            {
                patchedCanvasSails = 1;
            }

            InventoryChanged?.Invoke();
        }

        private PrototypeSilverWallet wallet;
        private ShipLoadout loadout;

        private void Bind(Transform player)
        {
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            loadout = player.GetComponentInChildren<
                ShipLoadout>();
        }
    }
}
