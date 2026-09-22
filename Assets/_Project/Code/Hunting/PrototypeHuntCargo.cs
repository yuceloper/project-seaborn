using System;
using Seaborn.Progression;
using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class PrototypeHuntCargo : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private int unsecuredSilverValue;

        [SerializeField, Min(0)]
        private int catchCount;

        public event Action CargoChanged;
        public event Action<string, int> CatchAdded;
        public event Action<int> CargoSecured;
        public event Action<int> CargoLost;
        public event Action<int> PirateCargoSecured;
        public int PirateWreckCount { get; private set; }
        public int UnsecuredCorsairIron { get; private set; }
        public int UnsecuredChartFragments { get; private set; }

        public bool TryAddWreck(int value, bool pirate, int iron = 0, int charts = 0)
        {
            if (value <= 0 || RemainingCapacity < value) return false;
            if (pirate) PirateWreckCount++;
            if (pirate)
            {
                UnsecuredCorsairIron += Mathf.Max(0, iron);
                UnsecuredChartFragments += Mathf.Max(0, charts);
            }
            AddCatch(pirate ? "Korsan enkazı" : "Sivil gemi enkazı", value);
            return true;
        }

        public int UnsecuredSilverValue =>
            unsecuredSilverValue;
        public int CatchCount => catchCount;
        public bool HasCargo =>
            unsecuredSilverValue > 0;
        public int MaximumSilverValue =>
            runtimeCapacity;
        public int RemainingCapacity =>
            runtimeCapacity == int.MaxValue
                ? int.MaxValue
                : Mathf.Max(
                    0,
                    runtimeCapacity -
                    unsecuredSilverValue
                );
        public bool IsFull => RemainingCapacity == 0;

        private int runtimeCapacity = int.MaxValue;

        private void Start()
        {
            // The hunting bootstrap can attach cargo after the ship profile has already applied.
            var profile = GetComponentInParent<Seaborn.Ship.ShipProfileController>();
            if (profile != null && profile.Definition != null)
                SetRuntimeCapacity(Mathf.Max(1, profile.Definition.cargoCapacity));
        }


        public void SetRuntimeCapacity(int capacity)
        {
            runtimeCapacity = Mathf.Max(1, capacity);
            CargoChanged?.Invoke();
        }

        public void ResetRuntimeCapacity()
        {
            runtimeCapacity = int.MaxValue;
            CargoChanged?.Invoke();
        }

        public void AddCatch(
            string creatureName,
            int silverValue)
        {
            if (silverValue <= 0)
            {
                return;
            }

            int acceptedValue =
                runtimeCapacity == int.MaxValue
                    ? silverValue
                    : Mathf.Min(
                        silverValue,
                        RemainingCapacity
                    );

            if (acceptedValue <= 0)
            {
                Debug.Log(
                    "Av ambarı dolu; yük alınamadı.",
                    this
                );
                return;
            }

            catchCount++;
            unsecuredSilverValue += acceptedValue;
            CargoChanged?.Invoke();
            CatchAdded?.Invoke(
                creatureName,
                acceptedValue
            );

            Debug.Log(
                $"Av yükü alındı: {creatureName}, " +
                $"+{acceptedValue} güvencesiz silver " +
                $"(Yük: {unsecuredSilverValue})",
                this
            );
        }

        public int SecureAtPort(
            PrototypeSilverWallet wallet)
        {
            if (wallet == null ||
                unsecuredSilverValue <= 0)
            {
                return 0;
            }

            int securedValue = unsecuredSilverValue;
            int pirateWrecks = PirateWreckCount;
            int iron = UnsecuredCorsairIron;
            int charts = UnsecuredChartFragments;
            UnsecuredCorsairIron = 0;
            UnsecuredChartFragments = 0;
            PirateWreckCount = 0;
            unsecuredSilverValue = 0;
            catchCount = 0;
            // Grant materials before wallet/quest/report notifications and autosaves.
            if (iron > 0 || charts > 0)
            {
                var inventory = GetComponentInParent<PrototypeRegionalLootInventory>();
                if (inventory == null)
                    inventory = PrototypeRegionalLootInventory.EnsureAttached(transform.root);
                inventory.Add(RegionalMaterialType.CorsairIron, iron, "Korsan yükü teslimi");
                inventory.Add(RegionalMaterialType.LostChartFragment, charts, "Korsan yükü teslimi");
            }
            wallet.AddSilver(securedValue);
            CargoChanged?.Invoke();
            // Quest credit must precede the expedition completion notification.
            if (pirateWrecks > 0) PirateCargoSecured?.Invoke(pirateWrecks);
            CargoSecured?.Invoke(securedValue);
            return securedValue;
        }

        public int LoseAllCargo()
        {
            int lostValue = unsecuredSilverValue;
            UnsecuredCorsairIron = 0;
            UnsecuredChartFragments = 0;
            PirateWreckCount = 0;
            unsecuredSilverValue = 0;
            catchCount = 0;
            CargoChanged?.Invoke();

            if (lostValue > 0)
            {
                CargoLost?.Invoke(lostValue);
            }

            return lostValue;
        }
    }
}
