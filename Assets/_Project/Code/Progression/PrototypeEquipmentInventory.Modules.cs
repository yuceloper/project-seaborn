using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seaborn.Progression
{
    public sealed partial class PrototypeEquipmentInventory
    {
        [SerializeField] private List<ShipModuleItem> moduleItems = new();
        [SerializeField] private bool modulesInitialized;
        private bool moduleTransaction;
        public IReadOnlyList<ShipModuleItem> Modules { get { EnsureModules(); return moduleItems; } }

        private void EnsureModules()
        {
            if (modulesInitialized) return;
            modulesInitialized = true;
            moduleItems = new List<ShipModuleItem>();
            for (int i = 0; i < patchedCanvasSails; i++) moduleItems.Add(ShipModuleItem.Create("patched_canvas"));
            for (int i = 0; i < ratSails; i++) moduleItems.Add(ShipModuleItem.Create("rat_sails"));
            // Neutral +0 slot for old captains and new captains; no change in legacy hull HP.
            moduleItems.Add(ShipModuleItem.Create("timber_plating"));
        }
        public ShipModuleItem FindModule(string id)
        { EnsureModules(); return moduleItems.Find(x => x.InstanceId == id); }
        public int CountModules(string definitionId)
        { EnsureModules(); return moduleItems.FindAll(x => string.Equals(x.DefinitionId, definitionId, StringComparison.OrdinalIgnoreCase)).Count; }
        private void SyncSailCounts()
        {
            patchedCanvasSails = moduleItems.FindAll(x => x.DefinitionId == "patched_canvas").Count;
            ratSails = moduleItems.FindAll(x => x.DefinitionId == "rat_sails").Count;
        }
        public ShipModuleItem[] CaptureModules()
        { EnsureModules(); return moduleItems.ConvertAll(x => x.Copy()).ToArray(); }
        public void RestoreModules(ShipModuleItem[] saved)
        {
            EnsureModules();
            if (saved == null) return;
            moduleItems.Clear();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in saved)
                if (item != null && !string.IsNullOrWhiteSpace(item.InstanceId) && item.Definition != null && ids.Add(item.InstanceId))
                    moduleItems.Add(item.Copy());
            SyncSailCounts();
            InventoryChanged?.Invoke();
        }
        public bool CanAffordModule(ShipModuleCost cost)
        {
            var depot = GetComponent<PrototypeRegionalLootInventory>();
            return wallet != null && wallet.Silver >= cost.Silver && depot != null && depot.CanAffordModule(cost);
        }
        private EquipmentPurchaseResult SpendModuleCost(ShipModuleCost cost)
        {
            var depot = GetComponent<PrototypeRegionalLootInventory>();
            if (depot == null || !depot.CanAffordModule(cost)) return EquipmentPurchaseResult.InsufficientMaterials;
            if (wallet == null || wallet.Silver < cost.Silver) return EquipmentPurchaseResult.InsufficientSilver;
            if (!wallet.TrySpendSilver(cost.Silver, "Gemi donanımı")) return EquipmentPurchaseResult.InsufficientSilver;
            if (depot.TrySpendModuleCost(cost)) return EquipmentPurchaseResult.Completed;
            wallet.RestoreSilver(wallet.Silver + cost.Silver);
            return EquipmentPurchaseResult.InsufficientMaterials;
        }
        public EquipmentPurchaseResult TryPurchaseModule(string definitionId)
        {
            if (!CanUseShipyard || moduleTransaction) return EquipmentPurchaseResult.NotAtShipyard;
            var definition = ShipModuleCatalog.Find(definitionId);
            if (definition == null) return EquipmentPurchaseResult.UnknownItem;
            moduleTransaction = true;
            try
            {
                var result = SpendModuleCost(definition.PurchaseCost);
                if (result != EquipmentPurchaseResult.Completed) return result;
                EnsureModules(); moduleItems.Add(ShipModuleItem.Create(definition.Id)); SyncSailCounts();
                InventoryChanged?.Invoke(); return EquipmentPurchaseResult.Completed;
            }
            finally { moduleTransaction = false; }
        }
        public EquipmentPurchaseResult TryUpgradeModule(string instanceId, int expectedLevel)
        {
            if (!CanUseShipyard || moduleTransaction) return EquipmentPurchaseResult.NotAtShipyard;
            var item = FindModule(instanceId);
            if (item == null) return EquipmentPurchaseResult.UnknownItem;
            if (item.Enhancement != expectedLevel) return EquipmentPurchaseResult.StaleSelection;
            if (item.Enhancement >= 10) return EquipmentPurchaseResult.MaximumEnhancement;
            moduleTransaction = true;
            try
            {
                var result = SpendModuleCost(ShipModuleCost.Next(item));
                if (result != EquipmentPurchaseResult.Completed) return result;
                item.Improve(); loadout?.Apply(); InventoryChanged?.Invoke();
                return EquipmentPurchaseResult.Completed;
            }
            finally { moduleTransaction = false; }
        }
    }
}
