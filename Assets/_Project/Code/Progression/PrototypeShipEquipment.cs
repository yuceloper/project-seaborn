using System;
using Seaborn.Combat;
using Seaborn.Harbor;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum ShipUpgradeTrack
    {
        Hull,
        Cannons,
        HarpoonGear
    }

    public enum ShipUpgradeResult
    {
        Completed,
        NotAtHarbor,
        MaximumLevel,
        InsufficientSilver,
        InsufficientMaterials,
        Unavailable
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeShipEquipment : MonoBehaviour
    {
        public const int MaximumLevel = 3;

        public event Action EquipmentChanged;

        public int HullLevel { get; private set; }
        public int CannonLevel { get; private set; }
        public int HarpoonLevel { get; private set; }

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

        private Transform boundPlayer;
        private PrototypeSilverWallet wallet;
        private PrototypeRegionalLootInventory materials;
        private ShipHealth health;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;

        public static void EnsureAttached(Transform player)
        {
            if (player == null) return;

            PrototypeShipEquipment equipment =
                player.GetComponent<PrototypeShipEquipment>();

            if (equipment == null)
            {
                equipment = player.gameObject.AddComponent<
                    PrototypeShipEquipment>();
            }

            equipment.Bind(player);
        }

        public int GetLevel(ShipUpgradeTrack track)
        {
            return track switch
            {
                ShipUpgradeTrack.Hull => HullLevel,
                ShipUpgradeTrack.Cannons => CannonLevel,
                ShipUpgradeTrack.HarpoonGear =>
                    HarpoonLevel,
                _ => 0
            };
        }

        public int GetUpgradeCost(ShipUpgradeTrack track)
        {
            int level = GetLevel(track);
            if (level >= MaximumLevel) return 0;

            int baseCost = track switch
            {
                ShipUpgradeTrack.Hull => 140,
                ShipUpgradeTrack.Cannons => 180,
                ShipUpgradeTrack.HarpoonGear => 100,
                _ => 0
            };

            return baseCost * (level + 1);
        }

        public ShipUpgradeResult TryUpgrade(
            ShipUpgradeTrack track)
        {
            if (!CanUseShipyard)
            {
                return ShipUpgradeResult.NotAtHarbor;
            }

            int level = GetLevel(track);
            if (level >= MaximumLevel)
            {
                return ShipUpgradeResult.MaximumLevel;
            }

            if (!CanAffordUpgradeMaterials(track))
            {
                return ShipUpgradeResult
                    .InsufficientMaterials;
            }

            int cost = GetUpgradeCost(track);
            if (wallet == null ||
                !wallet.TrySpendSilver(
                    cost,
                    UpgradeName(track)))
            {
                return ShipUpgradeResult.InsufficientSilver;
            }

            SpendUpgradeMaterials(track);
            SetLevel(track, level + 1);
            ApplyModifiers(true);
            EquipmentChanged?.Invoke();
            return ShipUpgradeResult.Completed;
        }

        public void RestoreLevels(
            int hull,
            int cannons,
            int harpoon)
        {
            HullLevel = Mathf.Clamp(
                hull, 0, MaximumLevel);
            CannonLevel = Mathf.Clamp(
                cannons, 0, MaximumLevel);
            HarpoonLevel = Mathf.Clamp(
                harpoon, 0, MaximumLevel);
            ApplyModifiers(true);
            EquipmentChanged?.Invoke();
        }

        public string EffectDescription(
            ShipUpgradeTrack track)
        {
            int level = GetLevel(track);
            return track switch
            {
                ShipUpgradeTrack.Hull =>
                    $"+%{level * 12} azami gövde",
                ShipUpgradeTrack.Cannons =>
                    $"+%{(level == 0 ? 0 : 15 + (level - 1) * 8)} hasar  •  " +
                    $"-%{(level == 0 ? 0 : 10 + (level - 1) * 5)} dolum",
                ShipUpgradeTrack.HarpoonGear =>
                    $"+%{level * 10} hasar  •  " +
                    $"-%{level * 5} dolum",
                _ => ""
            };
        }

        private void Bind(Transform player)
        {
            if (boundPlayer == player &&
                wallet != null &&
                health != null &&
                broadside != null &&
                harpoons != null)
            {
                return;
            }

            boundPlayer = player;
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            materials = player.GetComponentInChildren<
                PrototypeRegionalLootInventory>();
            health = player.GetComponentInChildren<ShipHealth>();
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponentInChildren<
                HarpoonHuntingController>();
            ApplyModifiers(false);
        }

        public bool CanAffordUpgradeMaterials(
            ShipUpgradeTrack track)
        {
            if (track == ShipUpgradeTrack.Cannons && CannonLevel == 0) return true;
            if (materials == null)
            {
                return false;
            }

            int level = GetLevel(track);
            if (level >= MaximumLevel)
            {
                return true;
            }

            RegionalMaterialType primary =
                PrimaryMaterial(track);
            int primaryCost = level + 1;
            if (!materials.CanAfford(
                    primary,
                    primaryCost))
            {
                return false;
            }

            if (level < 2)
            {
                return true;
            }

            RegionalMaterialType rare =
                track == ShipUpgradeTrack.HarpoonGear
                    ? RegionalMaterialType.StormjawScale
                    : RegionalMaterialType
                        .LostChartFragment;
            return materials.CanAfford(rare, 1);
        }

        public string UpgradeRequirementDescription(
            ShipUpgradeTrack track)
        {
            if (track == ShipUpgradeTrack.Cannons && CannonLevel == 0)
                return "İlk top geliştirmesi: malzeme gerekmez";
            int level = GetLevel(track);
            if (level >= MaximumLevel)
            {
                return "Azami seviye";
            }

            RegionalMaterialType primary =
                PrimaryMaterial(track);
            string result =
                $"{level + 1} " +
                $"{PrototypeRegionalLootInventory.DisplayName(primary)}";

            if (level >= 2)
            {
                RegionalMaterialType rare =
                    track == ShipUpgradeTrack.HarpoonGear
                        ? RegionalMaterialType.StormjawScale
                        : RegionalMaterialType
                            .LostChartFragment;
                result +=
                    $" + 1 " +
                    PrototypeRegionalLootInventory
                        .DisplayName(rare);
            }

            return result;
        }

        private void SpendUpgradeMaterials(
            ShipUpgradeTrack track)
        {
            if (track == ShipUpgradeTrack.Cannons && CannonLevel == 0) return;
            int level = GetLevel(track);
            RegionalMaterialType primary =
                PrimaryMaterial(track);
            materials.TrySpend(primary, level + 1);

            if (level < 2)
            {
                return;
            }

            RegionalMaterialType rare =
                track == ShipUpgradeTrack.HarpoonGear
                    ? RegionalMaterialType.StormjawScale
                    : RegionalMaterialType
                        .LostChartFragment;
            materials.TrySpend(rare, 1);
        }

        private static RegionalMaterialType
            PrimaryMaterial(ShipUpgradeTrack track)
        {
            return track == ShipUpgradeTrack.HarpoonGear
                ? RegionalMaterialType.TideOil
                : RegionalMaterialType.CorsairIron;
        }

        private void SetLevel(
            ShipUpgradeTrack track,
            int level)
        {
            switch (track)
            {
                case ShipUpgradeTrack.Hull:
                    HullLevel = level;
                    break;
                case ShipUpgradeTrack.Cannons:
                    CannonLevel = level;
                    break;
                case ShipUpgradeTrack.HarpoonGear:
                    HarpoonLevel = level;
                    break;
            }
        }

        private void ApplyModifiers(bool restoreHull)
        {
            health?.SetEquipmentHealthMultiplier(
                1f + HullLevel * 0.12f,
                restoreHull
            );
            broadside?.SetEquipmentModifiers(
                1f + (CannonLevel == 0 ? 0f : 0.15f + (CannonLevel - 1) * 0.08f),
                1f - (CannonLevel == 0 ? 0f : 0.10f + (CannonLevel - 1) * 0.05f)
            );
            harpoons?.SetEquipmentModifiers(
                1f + HarpoonLevel * 0.1f,
                1f - HarpoonLevel * 0.05f
            );
        }

        private static string UpgradeName(
            ShipUpgradeTrack track)
        {
            return track switch
            {
                ShipUpgradeTrack.Hull =>
                    "Gövde güçlendirmesi",
                ShipUpgradeTrack.Cannons =>
                    "Top takımı geliştirmesi",
                ShipUpgradeTrack.HarpoonGear =>
                    "Zıpkın donanımı geliştirmesi",
                _ => "Tersane geliştirmesi"
            };
        }
    }
}
