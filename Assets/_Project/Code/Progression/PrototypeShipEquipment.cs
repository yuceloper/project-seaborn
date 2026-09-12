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

        public bool CanUseShipyard =>
            harborServices != null &&
            harborServices.CanUseServices;

        private Transform boundPlayer;
        private PrototypeSilverWallet wallet;
        private PrototypeHarborServices harborServices;
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

            int cost = GetUpgradeCost(track);
            if (wallet == null ||
                !wallet.TrySpendSilver(
                    cost,
                    UpgradeName(track)))
            {
                return ShipUpgradeResult.InsufficientSilver;
            }

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
                    $"+%{level * 8} hasar  •  " +
                    $"-%{level * 5} dolum",
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
            harborServices = player.GetComponentInChildren<
                PrototypeHarborServices>();
            health = player.GetComponentInChildren<ShipHealth>();
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponentInChildren<
                HarpoonHuntingController>();
            ApplyModifiers(false);
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
                1f + CannonLevel * 0.08f,
                1f - CannonLevel * 0.05f
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
