using System;
using Seaborn.Hunting;
using Seaborn.Progression;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class InventoryDepotChecks
    {
        [MenuItem("Seaborn/Validation/Check Inventory And Depot")]
        public static void Run()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = new GameObject("Inventory validation") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var wallet = root.AddComponent<PrototypeSilverWallet>();
                var cargo = root.GetComponent<PrototypeHuntCargo>();
                if (cargo == null) cargo = root.AddComponent<PrototypeHuntCargo>();
                var depot = root.AddComponent<PrototypeRegionalLootInventory>();
                depot.Restore(7, 3, 5, 2); // Legacy secured balances.
                cargo.SetRuntimeCapacity(112);
                Require(!cargo.TryAddWreck(110, true, 2, 1), "Reject full wreck atomically");
                Require(!cargo.HasCargo && cargo.PirateWreckCount == 0, "Rejected wreck leaves no cargo");
                Require(cargo.TryAddWreck(110, true, 1, 1), "Exact capacity wreck fits");
                Require(cargo.IsFull && cargo.UsedCapacity == 112, "Materials occupy capacity");
                Require(!cargo.TryAddMaterial(RegionalMaterialType.TideOil, 1), "Full hold refuses material");
                Require(depot.CorsairIron == 5, "Pickup does not bank material");
                int notifications = 0;
                cargo.CargoSecured += _ => notifications++;
                cargo.SecureAtPort(wallet);
                Require(wallet.Silver == 110 && depot.CorsairIron == 6 &&
                    depot.LostChartFragments == 3, "Delivery credits cargo once");
                Require(!cargo.HasCargo && cargo.PirateWreckCount == 0, "Delivery clears hold");
                cargo.SecureAtPort(wallet);
                Require(wallet.Silver == 110 && depot.CorsairIron == 6 && notifications == 1,
                    "Repeated delivery is inert");
                Require(cargo.TryAddMaterial(RegionalMaterialType.TideOil, 2), "Material-only cargo accepted");
                Require(cargo.HasCargo, "Material-only cargo triggers delivery");
                cargo.SecureAtPort(wallet);
                Require(depot.TideOil == 9 && wallet.Silver == 110, "Material-only deposit preserves wallet");
                Require(cargo.TryAddWreck(20, false, 5, 5), "Civilian cargo accepted");
                Require(cargo.MaterialCount == 0 && cargo.PirateWreckCount == 0, "Civilian cannot award pirate material");
                for (int i = 0; i < 4; i++)
                    Require(cargo.TryAddMaterial((RegionalMaterialType)i, 1), "Load each material type");
                cargo.LoseAllCargo();
                Require(!cargo.HasCargo && cargo.MaterialCount == 0, "Sinking clears all carried types");
                Require(depot.TideOil == 9 && depot.StormjawScales == 3 && depot.CorsairIron == 6 &&
                    depot.LostChartFragments == 3, "Sinking preserves legacy and newly banked materials");
                Require(!cargo.TryAddMaterial((RegionalMaterialType)99, 1) &&
                    !cargo.TryAddMaterial(RegionalMaterialType.TideOil, -1), "Reject invalid material input");
                cargo.TryAddMaterial(RegionalMaterialType.TideOil, 2);
                cargo.SetRuntimeCapacity(1);
                Require(cargo.GetMaterial(RegionalMaterialType.TideOil) == 2 && cargo.RemainingCapacity == 0,
                    "Capacity reduction never destroys carried items");
                Debug.Log("Inventory/depot checks passed; temporary objects only, no save file changed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void Require(bool valid, string scenario)
        {
            if (!valid) throw new Exception("Inventory check failed: " + scenario);
        }
    }
}
