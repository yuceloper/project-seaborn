using System;
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
            unsecuredSilverValue = 0;
            catchCount = 0;
            wallet.AddSilver(securedValue);
            CargoChanged?.Invoke();
            CargoSecured?.Invoke(securedValue);
            return securedValue;
        }

        public int LoseAllCargo()
        {
            int lostValue = unsecuredSilverValue;
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
