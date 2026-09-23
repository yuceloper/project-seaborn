using System;
using Seaborn.Combat;
using Seaborn.Equipment;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship.Data;
using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class ShipLoadout : MonoBehaviour
    {
        [SerializeField] private string cannonId = "iron_6lb";
        [SerializeField, Min(0)] private int installedCannons = 6;
        [SerializeField] private string sailId = "patched_canvas";
        [SerializeField] private string harpoonId = "light_2kg";

        [SerializeField] private string sailItemId;
        [SerializeField] private string hullItemId;
        [SerializeField] private bool moduleLayoutInitialized;
        public string GetModuleId(ShipModuleSlot slot) => slot == ShipModuleSlot.Sail ? sailItemId : hullItemId;
        public ShipModuleItem GetModule(ShipModuleSlot slot) => GetComponent<PrototypeEquipmentInventory>()?.FindModule(GetModuleId(slot));
        public bool IsModuleInstalled(string id) => !string.IsNullOrEmpty(id) && (id == sailItemId || id == hullItemId);
        public ShipModuleStats GetModuleStats(ShipModuleSlot slot) => GetModule(slot)?.Stats ?? ShipModuleStats.Empty(slot);

        public bool TryEquipModule(ShipModuleSlot slot, string itemId)
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null || !inventory.CanUseShipyard || !Enum.IsDefined(typeof(ShipModuleSlot), slot)) return false;
            var item = inventory.FindModule(itemId);
            if (!string.IsNullOrEmpty(itemId) && (item == null || item.Definition.Slot != slot || IsModuleInstalled(itemId))) return false;
            if (slot == ShipModuleSlot.Sail) sailItemId = item?.InstanceId;
            else hullItemId = item?.InstanceId;
            moduleLayoutInitialized = true;
            Apply(); return true;
        }

        public void RestoreModuleLayout(bool hasLayout, string sail, string hull)
        {
            moduleLayoutInitialized = hasLayout;
            sailItemId = sail; hullItemId = hull;
            Apply();
        }

        private void ResolveModules()
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null) return;
            if (!moduleLayoutInitialized)
            {
                sailItemId = hullItemId = null;
                foreach (var item in inventory.Modules)
                {
                    if (item.DefinitionId == sailId && sailItemId == null) sailItemId = item.InstanceId;
                    if (item.DefinitionId == "timber_plating" && hullItemId == null) hullItemId = item.InstanceId;
                }
                moduleLayoutInitialized = true;
            }
            if (inventory.FindModule(sailItemId)?.Definition.Slot != ShipModuleSlot.Sail) sailItemId = null;
            if (inventory.FindModule(hullItemId)?.Definition.Slot != ShipModuleSlot.Hull) hullItemId = null;
            sailId = GetModule(ShipModuleSlot.Sail)?.DefinitionId ?? string.Empty;
        }

        // Stable interleaved hardpoints: port 1, starboard 1, port 2, ...
        [SerializeField] private string[] cannonSlots;
        [SerializeField] private string[] cannonItemIds;
        public string GetCannonItemId(int slot) => cannonItemIds != null && slot >= 0 && slot < cannonItemIds.Length
            ? cannonItemIds[slot] : null;
        public CannonItem GetCannonItemAt(int slot) => GetComponent<PrototypeEquipmentInventory>()?.FindCannon(GetCannonItemId(slot));
        public bool IsCannonInstalled(string id) => !string.IsNullOrEmpty(id) && cannonItemIds != null && Array.IndexOf(cannonItemIds, id) >= 0;
        public string[] CaptureCannonItemIds() => cannonItemIds == null ? null : (string[])cannonItemIds.Clone();
        public int CannonSlotCount => cannonSlots?.Length ?? 0;
        public string GetCannonAt(int slot) => slot >= 0 && slot < CannonSlotCount
            ? cannonSlots[slot] : null;
        public string[] CaptureCannonSlots() => cannonSlots == null ? null : (string[])cannonSlots.Clone();
        public int CountCannons(string id)
        {
            int count = 0;
            if (cannonSlots != null)
                foreach (string fitted in cannonSlots)
                    if (!string.IsNullOrEmpty(fitted) && string.Equals(fitted, id,
                        StringComparison.OrdinalIgnoreCase)) count++;
            return count;
        }

        public bool TryEquipCannonAt(int slot, string definitionId)
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            var spare = inventory?.FindSpareCannon(definitionId);
            return string.IsNullOrEmpty(definitionId) ? TryEquipCannonItemAt(slot, null)
                : spare != null && TryEquipCannonItemAt(slot, spare.InstanceId);
        }

        public bool TryEquipCannonItemAt(int slot, string instanceId)
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null || !inventory.CanUseShipyard || slot < 0 || slot >= CannonSlotCount) return false;
            var item = inventory.FindCannon(instanceId);
            if (!string.IsNullOrEmpty(instanceId) && (item == null || IsCannonInstalled(instanceId))) return false;
            if (cannonItemIds == null) cannonItemIds = new string[CannonSlotCount];
            cannonItemIds[slot] = item?.InstanceId;
            cannonSlots[slot] = item?.DefinitionId;
            Apply();
            return true;
        }

        public void RestoreCannonSlots(string[] saved, string[] itemIds = null)
        {
            int capacity = CannonSlotCount;
            if (saved != null && saved.Length > 0)
            {
                cannonSlots = new string[capacity];
                Array.Copy(saved, cannonSlots, Mathf.Min(saved.Length, capacity));
            }
            cannonItemIds = null;
            if (itemIds != null && itemIds.Length > 0)
            {
                cannonItemIds = new string[capacity];
                cannonSlots = new string[capacity];
                var inventory = GetComponent<PrototypeEquipmentInventory>();
                var used = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < Mathf.Min(itemIds.Length, capacity); i++)
                {
                    var item = inventory?.FindCannon(itemIds[i]);
                    if (item == null || !used.Add(item.InstanceId)) continue;
                    cannonItemIds[i] = item.InstanceId;
                    cannonSlots[i] = item.DefinitionId;
                }
            }
            Apply();
        }

        private void ResolveCannonItems()
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null) return; // Bootstrap before ownership is attached.
            bool migrate = cannonItemIds == null || cannonItemIds.Length == 0;
            if (migrate) cannonItemIds = new string[CannonSlotCount];
            else if (cannonItemIds.Length != CannonSlotCount) Array.Resize(ref cannonItemIds, CannonSlotCount);
            var used = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < CannonSlotCount; i++)
            {
                CannonItem item = null;
                if (migrate && !string.IsNullOrEmpty(cannonSlots[i]))
                {
                    foreach (var candidate in inventory.Cannons)
                        if (string.Equals(candidate.DefinitionId, cannonSlots[i], StringComparison.OrdinalIgnoreCase) &&
                            !used.Contains(candidate.InstanceId)) { item = candidate; break; }
                }
                else item = inventory.FindCannon(cannonItemIds[i]);
                if (item != null && used.Add(item.InstanceId))
                { cannonItemIds[i] = item.InstanceId; cannonSlots[i] = item.DefinitionId; }
                else { cannonItemIds[i] = null; cannonSlots[i] = null; }
            }
        }

        public event Action LoadoutChanged;

        public string CannonId => cannonId;
        public int InstalledCannons => installedCannons;
        public string SailId => sailId;
        public string HarpoonId => harpoonId;

        public CannonDefinition Cannon { get; private set; }
        public SailDefinition Sail { get; private set; }

        public static ShipLoadout EnsureAttached(Transform player)
        {
            if (player == null) return null;

            ShipLoadout loadout =
                player.GetComponent<ShipLoadout>();
            if (loadout == null)
            {
                loadout =
                    player.gameObject.AddComponent<ShipLoadout>();
            }

            return loadout;
        }

        private void Awake()
        {
            Apply();
        }

        public bool TryEquipCannons(
            string nextCannonId,
            int count)
        {
            if (!EquipmentCatalog.TryGetCannon(
                    nextCannonId,
                    out _) ||
                count <= 0)
            {
                return false;
            }

            ShipDefinition ship = GetShipDefinition();
            ShipProfileController profile =
                GetComponent<ShipProfileController>();
            int capacity = profile != null
                ? profile.EffectiveCannonSlots
                : ship != null ? ship.cannonSlots : 0;
            if (ship == null || count > capacity)
            {
                return false;
            }

            cannonId = nextCannonId;
            installedCannons = count;
            cannonSlots = null;
            cannonItemIds = null;
            Apply();
            return true;
        }

        public bool TryEquipSail(string nextSailId)
        {
            var inventory = GetComponent<PrototypeEquipmentInventory>();
            if (inventory == null) return false;
            foreach (var item in inventory.Modules)
                if (item.DefinitionId == nextSailId && !IsModuleInstalled(item.InstanceId))
                    return TryEquipModule(ShipModuleSlot.Sail, item.InstanceId);
            return false;
        }

        public bool TryEquipHarpoon(string nextHarpoonId)
        {
            HarpoonHuntingController harpoons =
                GetComponentInChildren<
                    HarpoonHuntingController>();
            if (harpoons == null ||
                !harpoons.TrySelectHarpoon(nextHarpoonId))
            {
                return false;
            }

            harpoonId = nextHarpoonId;
            LoadoutChanged?.Invoke();
            return true;
        }

        public void Restore(
            string savedCannonId,
            int savedCannonCount,
            string savedSailId,
            string savedHarpoonId)
        {
            if (EquipmentCatalog.TryGetCannon(
                    savedCannonId,
                    out _))
            {
                cannonId = savedCannonId;
            }

            if (ShipModuleCatalog.Find(savedSailId)?.Slot == ShipModuleSlot.Sail)
            {
                sailId = savedSailId;
            }

            if (EquipmentCatalog.TryGetHarpoon(
                    savedHarpoonId,
                    out _))
            {
                harpoonId = savedHarpoonId;
            }

            if (savedCannonCount > 0)
            {
                installedCannons = savedCannonCount;
            }

            moduleLayoutInitialized = false;
            cannonSlots = null;
            cannonItemIds = null;
            Apply();
        }

        [ContextMenu("Apply Loadout")]
        public void Apply()
        {
            ShipDefinition ship = GetShipDefinition();
            if (ship == null) return;

            if (!EquipmentCatalog.TryGetCannon(
                    cannonId,
                    out CannonDefinition cannon))
            {
                Debug.LogError(
                    $"Unknown cannon definition: {cannonId}",
                    this
                );
                return;
            }

            ResolveModules();
            var sailStats = GetModuleStats(ShipModuleSlot.Sail);
            var hullStats = GetModuleStats(ShipModuleSlot.Hull);
            var sailItem = GetModule(ShipModuleSlot.Sail);
            Cannon = cannon;
            Sail = new SailDefinition
            {
                id = sailItem?.DefinitionId ?? "bare_rig",
                displayName = sailItem == null ? "Yelken yok" : sailItem.Definition.Name + " +" + sailItem.Enhancement,
                speedMultiplier = sailStats.Speed, maneuverMultiplier = sailStats.Turning,
                silverPrice = sailItem?.Definition.PurchaseCost.Silver ?? 0
            };
            ShipProfileController profile =
                GetComponent<ShipProfileController>();
            int cannonCapacity = profile != null
                ? profile.EffectiveCannonSlots
                : ship.cannonSlots;
            cannonCapacity = Mathf.Max(1, cannonCapacity);
            if (cannonSlots == null)
            {
                cannonSlots = new string[cannonCapacity];
                for (int i = 0; i < Mathf.Min(installedCannons, cannonCapacity); i++)
                    cannonSlots[i] = cannonId;
            }
            else if (cannonSlots.Length != cannonCapacity)
                Array.Resize(ref cannonSlots, cannonCapacity);
            ResolveCannonItems();
            installedCannons = 0;
            foreach (string id in cannonSlots)
                if (EquipmentCatalog.TryGetCannon(id, out var fitted))
                {
                    if (installedCannons == 0) { cannonId = fitted.id; Cannon = fitted; }
                    installedCannons++;
                }

            GetComponent<PrototypeModularShipAssembler>()?.RefreshHardpointCapacity(cannonCapacity);
            int[] levels = new int[CannonSlotCount];
            for (int i = 0; i < levels.Length; i++) levels[i] = GetCannonItemAt(i)?.Enhancement ?? 0;
            GetComponentInChildren<BroadsideController>()?.SetModularLoadout(cannonSlots, ship.cannonRange, levels);

            ShipMotor motor =
                GetComponentInChildren<ShipMotor>();
            motor?.SetRuntimePerformance(
                ship.speedMultiplier * sailStats.Speed * hullStats.Speed,
                sailStats.Acceleration * hullStats.Acceleration,
                ship.maneuverMultiplier * sailStats.Turning * hullStats.Turning
            );
            GetComponentInChildren<ShipHealth>()?.SetHullModuleHealthMultiplier(hullStats.HullHealth);
            GetComponentInChildren<ShipSubsystemController>()?.SetSailDurability(sailStats.SailDurability);

            HarpoonHuntingController harpoons =
                GetComponentInChildren<
                    HarpoonHuntingController>();
            harpoons?.TrySelectHarpoon(harpoonId);

            LoadoutChanged?.Invoke();
        }

        private ShipDefinition GetShipDefinition()
        {
            ShipProfileController profile =
                GetComponent<ShipProfileController>();
            if (profile == null)
            {
                profile =
                    gameObject.AddComponent<
                        ShipProfileController>();
            }

            if (profile.Definition == null)
            {
                profile.ApplyProfile();
            }

            return profile.Definition;
        }
    }
}
