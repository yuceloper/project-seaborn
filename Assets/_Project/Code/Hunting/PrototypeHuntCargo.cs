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
        private readonly int[] carriedMaterials = new int[4];
        public int UnsecuredCorsairIron => GetMaterial(RegionalMaterialType.CorsairIron);
        public int UnsecuredChartFragments => GetMaterial(RegionalMaterialType.LostChartFragment);
        public int MaterialCount => carriedMaterials[0] + carriedMaterials[1] +
            carriedMaterials[2] + carriedMaterials[3];
        public int UsedCapacity => unsecuredSilverValue + MaterialCount;

        public int GetMaterial(RegionalMaterialType type)
        {
            int index = (int)type;
            return index >= 0 && index < carriedMaterials.Length ? carriedMaterials[index] : 0;
        }

        public bool TryAddMaterial(RegionalMaterialType type, int amount)
        {
            int index = (int)type;
            if (index < 0 || index >= carriedMaterials.Length || amount <= 0 ||
                RemainingCapacity < amount) return false;
            carriedMaterials[index] += amount;
            CargoChanged?.Invoke();
            return true;
        }

        // Manual transfers are available only at a docked harbor station.
        public bool CanTransferToDepot =>
            Seaborn.World.PrototypeExpeditionRegionDirector.IsHarborScene &&
            Seaborn.Harbor.PrototypeHarborDockingDirector.Instance != null &&
            Seaborn.Harbor.PrototypeHarborDockingDirector.Instance.DockedStation !=
                Seaborn.Harbor.PrototypeHarborStation.None;

        public bool TryDepositMaterial(RegionalMaterialType type, int amount)
        {
            if (!CanTransferToDepot || amount <= 0 || GetMaterial(type) < amount) return false;
            var depot = ResolveDepot();
            carriedMaterials[(int)type] -= amount;
            depot.Add(type, amount, "Depoya aktarıldı");
            CargoChanged?.Invoke();
            return true;
        }

        public bool TryDepositAll(PrototypeSilverWallet wallet)
        {
            if (!CanTransferToDepot || !HasCargo || wallet == null) return false;
            SecureAtPort(wallet);
            return true;
        }

        private PrototypeRegionalLootInventory ResolveDepot()
        {
            return GetComponentInParent<PrototypeRegionalLootInventory>() ??
                PrototypeRegionalLootInventory.EnsureAttached(transform.root);
        }

        public bool TryAddWreck(int value, bool pirate, int iron = 0, int charts = 0)
        {
            iron = pirate ? Mathf.Max(0, iron) : 0;
            charts = pirate ? Mathf.Max(0, charts) : 0;
            if (value <= 0 || (long)value + iron + charts > RemainingCapacity) return false;
            if (pirate) PirateWreckCount++;
            if (pirate)
            {
                carriedMaterials[(int)RegionalMaterialType.CorsairIron] += iron;
                carriedMaterials[(int)RegionalMaterialType.LostChartFragment] += charts;
            }
            AddCatch(pirate ? "Korsan enkazı" : "Sivil gemi enkazı", value);
            return true;
        }

        public int UnsecuredSilverValue =>
            unsecuredSilverValue;
        public int CatchCount => catchCount;
        public bool HasCargo =>
            unsecuredSilverValue > 0 || MaterialCount > 0;
        public int MaximumSilverValue =>
            runtimeCapacity;
        public int RemainingCapacity =>
            runtimeCapacity == int.MaxValue
                ? int.MaxValue
                : Mathf.Max(
                    0,
                    runtimeCapacity -
                    UsedCapacity
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
                !HasCargo)
            {
                return 0;
            }

            int securedValue = unsecuredSilverValue;
            int pirateWrecks = PirateWreckCount;
            int[] delivered = (int[])carriedMaterials.Clone();
            Array.Clear(carriedMaterials, 0, carriedMaterials.Length);
            PirateWreckCount = 0;
            unsecuredSilverValue = 0;
            catchCount = 0;
            // Empty the hold before callbacks so repeated delivery cannot duplicate loot.
            var depot = ResolveDepot();
            for (int i = 0; i < delivered.Length; i++)
                depot.Add((RegionalMaterialType)i, delivered[i], "Liman deposuna teslim");
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
            Array.Clear(carriedMaterials, 0, carriedMaterials.Length);
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
