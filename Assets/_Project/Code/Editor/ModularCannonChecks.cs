using System;
using Seaborn.Combat;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class ModularCannonChecks
    {
        [MenuItem("Seaborn/Validation/Check Modular Cannons")]
        public static void Run()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = new GameObject("Modular cannon validation") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var profile = root.AddComponent<ShipProfileController>();
                profile.ApplyProfile();
                var battery = root.AddComponent<BroadsideController>();
                var loadout = root.AddComponent<ShipLoadout>();
                loadout.Apply();
                var inventory = PrototypeEquipmentInventory.EnsureAttached(root.transform);
                inventory.Restore(6, 1, 1, 0);
                var assembler = root.AddComponent<PrototypeModularShipAssembler>();
                assembler.BuildHardpointsOnly(root.transform, null);
                loadout.Restore("iron_6lb", 6, "patched_canvas", "light_2kg");
                loadout.RestoreCannonSlots(null);
                Require(loadout.CountCannons("iron_6lb") == 6, "Migrate uniform save");
                Require(battery.GetBroadsideCannonCount(BroadsideSide.Port) == 3 &&
                    battery.GetBroadsideCannonCount(BroadsideSide.Starboard) == 3, "Starter battery 3+3");
                string[] mixed = { "iron_12lb", "iron_6lb", null, "iron_6lb", "iron_6lb", "iron_6lb" };
                loadout.RestoreCannonSlots(mixed);
                Require(loadout.InstalledCannons == 5 && inventory.GetStoredCannons("iron_6lb") == 2 &&
                    inventory.GetStoredCannons("iron_12lb") == 0, "Mixed battery ownership accounting");
                Require(battery.GetBroadsideCannonCount(BroadsideSide.Port) == 2 &&
                    battery.GetBroadsideCannonCount(BroadsideSide.Starboard) == 3, "Empty slot does not fire");
                Require(Mathf.Approximately(battery.GetBaseBroadsideReload(BroadsideSide.Port), 6.2f) &&
                    Mathf.Approximately(battery.GetBaseBroadsideReload(BroadsideSide.Starboard), 4.8f),
                    "Heavy gun only slows its own broadside");
                var lookup = typeof(BroadsideController).GetMethod("CannonAtMuzzle",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var portGun = (Seaborn.Equipment.CannonDefinition)lookup.Invoke(battery,
                    new object[] { assembler.PortHardpoints[0] });
                var starboardGun = (Seaborn.Equipment.CannonDefinition)lookup.Invoke(battery,
                    new object[] { assembler.StarboardHardpoints[0] });
                Require(portGun.damage == 125f && starboardGun.damage == 85f,
                    "Each muzzle resolves its own fitted damage profile");
                string[] snapshot = loadout.CaptureCannonSlots();
                snapshot[0] = null;
                Require(loadout.CountCannons("iron_12lb") == 1, "Save snapshots do not alias live state");
                profile.ApplyProfile();
                Require(loadout.CountCannons("iron_12lb") == 1 && battery.InstalledCannons == 5,
                    "Profile refresh preserves layout");
                loadout.RestoreCannonSlots(new[] { "iron_12lb", "iron_12lb", "invalid", null, null, null });
                Require(loadout.InstalledCannons == 1, "Reject unowned duplicate and invalid ID on restore");
                loadout.RestoreCannonSlots(new string[6]);
                Require(loadout.InstalledCannons == 0 && battery.GetBroadsideCannonCount(BroadsideSide.Port) == 0 &&
                    battery.GetBroadsideCannonCount(BroadsideSide.Starboard) == 0, "Fully empty battery is valid");
                Require(inventory.GetStoredCannons("iron_6lb") == 6 && inventory.GetStoredCannons("iron_12lb") == 1,
                    "Unequipped guns return to depot");
                assembler.RefreshHardpointCapacity(7);
                Require(assembler.PortHardpoints.Count >= 4 && assembler.StarboardHardpoints.Count >= 3,
                    "Extension creates real additional muzzle");
                assembler.RefreshHardpointCapacity(6);
                Require(!assembler.PortHardpoints[3].gameObject.activeInHierarchy,
                    "Removed extension disables unused muzzle");
                Debug.Log("Modular cannon checks passed. No save file changed. Validate live firing and shipyard UI in Play Mode.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static void Require(bool value, string scenario)
        {
            if (!value) throw new Exception("Modular cannon check failed: " + scenario);
        }
    }
}
