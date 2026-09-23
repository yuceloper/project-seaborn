using System;
using Seaborn.Combat;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class CannonEnhancementChecks
    {
        [Serializable]
        private sealed class Snapshot
        {
            public CannonItem[] items;
            public string[] slots;
        }

        [MenuItem("Seaborn/Validation/Check Cannon Enhancements")]
        public static void Run()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = new GameObject("Cannon item validation") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var profile = root.AddComponent<ShipProfileController>();
                profile.ApplyProfile();
                var battery = root.AddComponent<BroadsideController>();
                var loadout = root.AddComponent<ShipLoadout>();
                loadout.Apply();
                var inventory = PrototypeEquipmentInventory.EnsureAttached(root.transform);
                inventory.Restore(6, 2, 1, 0);
                loadout.Restore("iron_6lb", 6, "patched_canvas", "light_2kg");
                loadout.RestoreCannonSlots(null);
                Require(inventory.Cannons.Count == 8, "Legacy counts migrate without granting extras");
                var ids = new System.Collections.Generic.HashSet<string>();
                foreach (var item in inventory.Cannons)
                    Require(item.Enhancement == 0 && ids.Add(item.InstanceId), "Unique +0 legacy items");
                var plusThree = JsonUtility.FromJson<CannonItem>("{\"instanceId\":\"upgraded\",\"definitionId\":\"iron_6lb\",\"enhancement\":3}");
                var plain = JsonUtility.FromJson<CannonItem>("{\"instanceId\":\"plain\",\"definitionId\":\"iron_6lb\",\"enhancement\":0}");
                inventory.RestoreCannonItems(new[] { plusThree, plain });
                loadout.RestoreCannonSlots(null, new[] { "upgraded", "plain", null, null, null, null });
                Require(loadout.GetCannonItemAt(0).Enhancement == 3 && loadout.GetCannonItemAt(1).Enhancement == 0,
                    "Same type retains independent enhancement levels");
                var assembler = root.AddComponent<PrototypeModularShipAssembler>();
                assembler.BuildHardpointsOnly(root.transform, null);
                var damageMethod = typeof(BroadsideController).GetMethod("CannonDamageAtMuzzle",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                float portDamage = (float)damageMethod.Invoke(battery, new object[] { assembler.PortHardpoints[0] });
                float starboardDamage = (float)damageMethod.Invoke(battery, new object[] { assembler.StarboardHardpoints[0] });
                Require(Mathf.Approximately(portDamage, 105.4f) && Mathf.Approximately(starboardDamage, 85f),
                    "+3 damage affects only its own muzzle");
                var snapshot = new Snapshot { items = inventory.CaptureCannonItems(), slots = loadout.CaptureCannonItemIds() };
                var roundtrip = JsonUtility.FromJson<Snapshot>(JsonUtility.ToJson(snapshot));
                inventory.RestoreCannonItems(roundtrip.items);
                loadout.RestoreCannonSlots(null, roundtrip.slots);
                Require(loadout.GetCannonItemId(0) == "upgraded" && loadout.GetCannonItemAt(0).Enhancement == 3,
                    "JSON roundtrip preserves identity, layout and level");
                loadout.RestoreCannonSlots(null, new[] { null, "plain", "upgraded", null, null, null });
                Require(loadout.GetCannonItemAt(2).Enhancement == 3 && inventory.Cannons.Count == 2,
                    "Moving item does not clone or reset it");
                loadout.RestoreCannonSlots(null, new[] { "upgraded", "upgraded", "missing", null, null, null });
                Require(loadout.InstalledCannons == 1, "Duplicate and unknown fitted item IDs rejected");
                loadout.RestoreCannonSlots(null, new string[6]);
                Require(inventory.FindSpareCannon("iron_6lb").Enhancement == 3 && inventory.GetStoredCannons("iron_6lb") == 2,
                    "Unfitted enhanced item is stored intact");
                var max = JsonUtility.FromJson<CannonItem>("{\"instanceId\":\"max\",\"definitionId\":\"iron_12lb\",\"enhancement\":999}");
                inventory.RestoreCannonItems(new[] { max, max });
                Require(inventory.Cannons.Count == 1 && inventory.Cannons[0].Enhancement == 10,
                    "Clamp enhancement and reject duplicate owned IDs");
                Require(Mathf.Approximately(CannonItem.DamageAt(0), 1f) && Mathf.Approximately(CannonItem.DamageAt(10), 1.8f),
                    "Enhancement damage endpoints");
                var depot = root.AddComponent<PrototypeRegionalLootInventory>();
                var cost = CannonUpgradeCost.Next(plusThree); // +3 -> +4, 640 Silver + 4 Iron.
                Require(cost.Silver == 640 && cost.Iron == 4 && cost.Charts == 0 && cost.Scales == 0, "Recipe tier");
                depot.Restore(0, 2, 3, 5);
                Require(!depot.TrySpendCannonUpgrade(cost) && depot.CorsairIron == 3 && depot.LostChartFragments == 5,
                    "Unfunded recipe never partially debits materials");
                depot.Restore(0, 2, 4, 5);
                Require(depot.TrySpendCannonUpgrade(cost) && depot.CorsairIron == 0 && depot.LostChartFragments == 5,
                    "Exact material balance succeeds once");
                Debug.Log("Cannon enhancement checks passed. Temporary objects only; no save file changed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void Require(bool valid, string scenario)
        {
            if (!valid) throw new Exception("Cannon enhancement check failed: " + scenario);
        }
    }
}
