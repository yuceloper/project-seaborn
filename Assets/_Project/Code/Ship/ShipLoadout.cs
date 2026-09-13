using System;
using Seaborn.Combat;
using Seaborn.Equipment;
using Seaborn.Hunting;
using Seaborn.Ship.Data;
using UnityEngine;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class ShipLoadout : MonoBehaviour
    {
        private const float PrototypeCannonDamageScale = 0.25f;

        [SerializeField] private string cannonId = "iron_6lb";
        [SerializeField, Min(1)] private int installedCannons = 6;
        [SerializeField] private string sailId = "patched_canvas";
        [SerializeField] private string harpoonId = "light_2kg";

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

            loadout.Apply();
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
            if (ship == null || count > ship.cannonSlots)
            {
                return false;
            }

            cannonId = nextCannonId;
            installedCannons = count;
            Apply();
            return true;
        }

        public bool TryEquipSail(string nextSailId)
        {
            if (!EquipmentCatalog.TryGetSail(
                    nextSailId,
                    out _))
            {
                return false;
            }

            sailId = nextSailId;
            Apply();
            return true;
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

            if (EquipmentCatalog.TryGetSail(
                    savedSailId,
                    out _))
            {
                sailId = savedSailId;
            }

            if (EquipmentCatalog.TryGetHarpoon(
                    savedHarpoonId,
                    out _))
            {
                harpoonId = savedHarpoonId;
            }

            installedCannons = Mathf.Max(
                1,
                savedCannonCount
            );
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

            if (!EquipmentCatalog.TryGetSail(
                    sailId,
                    out SailDefinition sail))
            {
                Debug.LogError(
                    $"Unknown sail definition: {sailId}",
                    this
                );
                return;
            }

            Cannon = cannon;
            Sail = sail;
            installedCannons = Mathf.Clamp(
                installedCannons,
                1,
                ship.cannonSlots
            );

            BroadsideController broadside =
                GetComponentInChildren<
                    BroadsideController>();
            broadside?.SetCannonLoadout(
                ship.cannonSlots,
                installedCannons,
                Mathf.Min(ship.cannonRange, cannon.range),
                cannon.reloadDuration,
                cannon.damage *
                    PrototypeCannonDamageScale
            );

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
