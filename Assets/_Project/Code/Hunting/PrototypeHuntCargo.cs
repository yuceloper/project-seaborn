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

        public int UnsecuredSilverValue =>
            unsecuredSilverValue;
        public int CatchCount => catchCount;
        public bool HasCargo =>
            unsecuredSilverValue > 0;

        public void AddCatch(
            string creatureName,
            int silverValue)
        {
            if (silverValue <= 0)
            {
                return;
            }

            catchCount++;
            unsecuredSilverValue += silverValue;
            CargoChanged?.Invoke();

            Debug.Log(
                $"Av yükü alındı: {creatureName}, " +
                $"+{silverValue} güvencesiz silver " +
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
            return securedValue;
        }

        public int LoseAllCargo()
        {
            int lostValue = unsecuredSilverValue;
            unsecuredSilverValue = 0;
            catchCount = 0;
            CargoChanged?.Invoke();
            return lostValue;
        }
    }
}
