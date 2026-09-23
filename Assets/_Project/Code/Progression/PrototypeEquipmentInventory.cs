using System;
using System.Collections.Generic;
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
        InsufficientMaterials,
        MaximumEnhancement,
        StaleSelection
    }

    [DisallowMultipleComponent]
    public sealed partial class PrototypeEquipmentInventory : MonoBehaviour
    {
        [SerializeField, Min(0)] private int iron6LbCannons = 6;
        [SerializeField, Min(0)] private int iron12LbCannons;
        [SerializeField, Min(0)] private int patchedCanvasSails = 1;
        [SerializeField, Min(0)] private int ratSails;

        [SerializeField] private List<CannonItem> cannonItems = new();
        [SerializeField] private bool itemsInitialized;
        private bool upgrading;
        public IReadOnlyList<CannonItem> Cannons { get { EnsureItems(); return cannonItems; } }

        private void EnsureItems()
        {
            if (itemsInitialized) return;
            itemsInitialized = true;
            cannonItems = new List<CannonItem>();
            for (int i = 0; i < iron6LbCannons; i++) cannonItems.Add(CannonItem.Create("iron_6lb"));
            for (int i = 0; i < iron12LbCannons; i++) cannonItems.Add(CannonItem.Create("iron_12lb"));
        }

        public CannonItem FindCannon(string instanceId)
        {
            EnsureItems();
            if (string.IsNullOrEmpty(instanceId)) return null;
            return cannonItems.Find(item => item.InstanceId == instanceId);
        }

        public CannonItem FindSpareCannon(string definitionId)
        {
            EnsureItems();
            if (loadout == null) loadout = GetComponent<ShipLoadout>();
            return cannonItems.Find(item => string.Equals(item.DefinitionId, definitionId,
                StringComparison.OrdinalIgnoreCase) && (loadout == null || !loadout.IsCannonInstalled(item.InstanceId)));
        }

        private void AddCannon(string id)
        {
            EnsureItems();
            cannonItems.Add(CannonItem.Create(id));
            SyncLegacyCounts();
        }

        private void SyncLegacyCounts()
        {
            iron6LbCannons = cannonItems.FindAll(x => x.DefinitionId == "iron_6lb").Count;
            iron12LbCannons = cannonItems.FindAll(x => x.DefinitionId == "iron_12lb").Count;
        }

        public CannonItem[] CaptureCannonItems()
        {
            EnsureItems();
            return cannonItems.ConvertAll(item => item.Copy()).ToArray();
        }

        public void RestoreCannonItems(CannonItem[] saved)
        {
            EnsureItems();
            if (saved == null) return; // Counts-only save: keep the +0 migration.
            cannonItems.Clear();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in saved)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.InstanceId) ||
                    !ids.Add(item.InstanceId) || !EquipmentCatalog.TryGetCannon(item.DefinitionId, out _)) continue;
                cannonItems.Add(item.Copy());
            }
            SyncLegacyCounts();
            InventoryChanged?.Invoke();
        }

        public EquipmentPurchaseResult TryUpgradeCannon(string instanceId, int expectedLevel)
        {
            if (!CanUseShipyard || upgrading) return EquipmentPurchaseResult.NotAtShipyard;
            var item = FindCannon(instanceId);
            if (item == null) return EquipmentPurchaseResult.UnknownItem;
            if (item.Enhancement != expectedLevel) return EquipmentPurchaseResult.StaleSelection;
            if (item.Enhancement >= CannonItem.MaximumEnhancement) return EquipmentPurchaseResult.MaximumEnhancement;
            var cost = CannonUpgradeCost.Next(item);
            var depot = GetComponent<PrototypeRegionalLootInventory>();
            if (depot == null || !depot.CanAfford(RegionalMaterialType.CorsairIron, cost.Iron) ||
                !depot.CanAfford(RegionalMaterialType.LostChartFragment, cost.Charts) ||
                !depot.CanAfford(RegionalMaterialType.StormjawScale, cost.Scales))
                return EquipmentPurchaseResult.InsufficientMaterials;
            if (wallet == null || wallet.Silver < cost.Silver) return EquipmentPurchaseResult.InsufficientSilver;
            upgrading = true;
            try
            {
                if (!wallet.TrySpendSilver(cost.Silver, "Top geliştirme")) return EquipmentPurchaseResult.InsufficientSilver;
                if (!depot.TrySpendCannonUpgrade(cost))
                {
                    wallet.RestoreSilver(wallet.Silver + cost.Silver);
                    return EquipmentPurchaseResult.InsufficientMaterials;
                }
                item.Improve();
                loadout?.Apply();
                InventoryChanged?.Invoke();
                return EquipmentPurchaseResult.Completed;
            }
            finally { upgrading = false; }
        }

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
            inventory.loadout?.Apply();
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

        public int GetOwnedSails(string sailId) => CountModules(sailId);

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

            AddCannon(definition.id);

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
                AddCannon("iron_12lb");
                InventoryChanged?.Invoke();
                return EquipmentPurchaseResult.Completed;
            }
            finally { forging = false; }
        }

        public EquipmentPurchaseResult TryPurchaseSail(string sailId)
        {
            var definition = ShipModuleCatalog.Find(sailId);
            return definition == null || definition.Slot != ShipModuleSlot.Sail
                ? EquipmentPurchaseResult.UnknownItem : TryPurchaseModule(sailId);
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

            itemsInitialized = false;
            EnsureItems();
            modulesInitialized = false;
            EnsureModules();
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
