using System;
using Seaborn.Combat;
using Seaborn.Combat.Damage;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class ShipModuleChecks
    {
        [Serializable] private sealed class Snapshot
        { public ShipModuleItem[] items; public string sail; public string hull; }
        private static ShipModuleItem Item(string id, string definition, int level) => JsonUtility.FromJson<ShipModuleItem>(
            $"{{\"instanceId\":\"{id}\",\"definitionId\":\"{definition}\",\"enhancement\":{level}}}");

        [MenuItem("Seaborn/Validation/Check Sail And Hull Modules")]
        public static void Run()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = new GameObject("Sail and hull validation") { hideFlags = HideFlags.HideAndDontSave };
            root.SetActive(false); // No scene input or audio lifecycle on temporary components.
            try
            {
                var motor = root.AddComponent<ShipMotor>();
                var health = root.AddComponent<ShipHealth>();
                var systems = ShipSubsystemController.EnsureAttached(root.transform);
                var profile = root.AddComponent<ShipProfileController>(); profile.ApplyProfile();
                var loadout = root.AddComponent<ShipLoadout>(); loadout.Apply();
                var inventory = PrototypeEquipmentInventory.EnsureAttached(root.transform);
                inventory.Restore(6, 0, 2, 1); inventory.RestoreModules(null);
                loadout.Restore("iron_6lb", 6, "rat_sails", "light_2kg");
                loadout.RestoreModuleLayout(false, null, null);
                Require(inventory.Modules.Count == 4 && inventory.GetOwnedSails("patched_canvas") == 2 && inventory.GetOwnedSails("rat_sails") == 1,
                    "Legacy sail counts preserved, one neutral hull item migrated");
                Require(loadout.GetModule(ShipModuleSlot.Sail).DefinitionId == "rat_sails" &&
                    loadout.GetModule(ShipModuleSlot.Hull).DefinitionId == "timber_plating", "Legacy fitted sail and neutral hull selected");
                Require(Mathf.Approximately(motor.SpeedMultiplier, profile.Definition.speedMultiplier * 1.08f), "Legacy Rat sail speed unchanged");
                var upgraded = Item("storm3", "storm_canvas", 3);
                var spare = Item("storm0", "storm_canvas", 0);
                var hull = Item("heavy5", "reinforced_plating", 5);
                inventory.RestoreModules(new[] { upgraded, spare, hull });
                health.SetBaseMaximumHealth(2000, true);
                health.SetEquipmentHealthMultiplier(0.5f, false);
                health.SetEquipmentHealthMultiplier(1f, false);
                loadout.RestoreModuleLayout(true, "storm3", "heavy5");
                Require(Mathf.Approximately(health.CurrentHealth, 1000) && Mathf.Approximately(health.MaximumHealth, 2760),
                    "Hull fitting increases maximum without healing");
                float maximum = health.MaximumHealth; loadout.Apply(); loadout.Apply();
                Require(Mathf.Approximately(maximum, health.MaximumHealth) && Mathf.Approximately(health.CurrentHealth, 1000),
                    "Repeated Apply does not compound or heal");
                Require(Mathf.Approximately(motor.SpeedMultiplier, profile.Definition.speedMultiplier * upgraded.Stats.Speed * hull.Stats.Speed),
                    "Sail and hull movement effects combine");
                systems.RestoreAll();
                systems.RegisterHit(new DamageInfo(100, Vector3.zero, Vector3.forward, null, AmmunitionType.Chain));
                Require(Mathf.Approximately(systems.SailIntegrity, 100 - 4 / upgraded.Stats.SailDurability), "Durable sail reduces chain integrity damage");
                float damagedSail = systems.SailIntegrity;
                loadout.RestoreModuleLayout(true, "storm0", "heavy5");
                Require(Mathf.Approximately(systems.SailIntegrity, damagedSail), "Swapping sails does not repair integrity");
                Require(inventory.FindModule("storm3").Enhancement == 3 && inventory.FindModule("storm0").Enhancement == 0,
                    "Equal sail types keep independent levels");
                var snapshot = new Snapshot { items = inventory.CaptureModules(), sail = "storm3", hull = "heavy5" };
                var saved = JsonUtility.FromJson<Snapshot>(JsonUtility.ToJson(snapshot));
                inventory.RestoreModules(saved.items); loadout.RestoreModuleLayout(true, saved.sail, saved.hull);
                Require(loadout.GetModule(ShipModuleSlot.Sail).Enhancement == 3 && loadout.GetModule(ShipModuleSlot.Hull).Enhancement == 5,
                    "Identity and levels roundtrip through JSON");
                loadout.RestoreModuleLayout(true, "heavy5", "storm3");
                Require(loadout.GetModule(ShipModuleSlot.Sail) == null && loadout.GetModule(ShipModuleSlot.Hull) == null, "Wrong-category slots rejected");
                loadout.RestoreModuleLayout(true, null, null); loadout.Apply();
                Require(loadout.GetModule(ShipModuleSlot.Sail) == null && inventory.Modules.Count == 3 &&
                    Mathf.Approximately(motor.SpeedMultiplier, profile.Definition.speedMultiplier * 0.45f), "Empty slots persist, items remain owned, ship can limp home");
                inventory.RestoreModules(new[] { Item("clamp", "light_plating", 99), Item("clamp", "light_plating", 1), Item("bad", "unknown", 2) });
                Require(inventory.Modules.Count == 1 && inventory.Modules[0].Enhancement == 10, "Invalid/duplicate records rejected and levels clamped");
                var depot = root.AddComponent<PrototypeRegionalLootInventory>();
                var cost = new ShipModuleCost(100, 2, 3, 1, 1);
                depot.Restore(2, 1, 2, 1);
                Require(!depot.TrySpendModuleCost(cost) && depot.TideOil == 2 && depot.CorsairIron == 2 && depot.StormjawScales == 1,
                    "Missing material leaves all balances untouched");
                depot.Restore(2, 1, 3, 1);
                Require(depot.TrySpendModuleCost(cost) && depot.TideOil == 0 && depot.CorsairIron == 0 && depot.LostChartFragments == 0 && depot.StormjawScales == 0,
                    "Fully funded material recipe debits exactly once");
                Debug.Log("Sail/hull checks passed. No save file changed; verify UI, purchases/upgrades and movement in Play Mode.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static void Require(bool valid, string scenario)
        { if (!valid) throw new Exception("Ship module check failed: " + scenario); }
    }
}
