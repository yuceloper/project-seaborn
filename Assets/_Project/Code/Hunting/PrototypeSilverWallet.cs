using System;
using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSilverWallet : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private int silver;

        public event Action<int> SilverChanged;

        public int Silver => silver;

        private void Awake()
        {
            if (GetComponent<PrototypeHuntCargo>() == null)
            {
                gameObject.AddComponent<PrototypeHuntCargo>();
            }
        }

        public void AddSilver(int amount)
        {
            AddSilver(amount, "Av tamamlandı");
        }

        public void AddSilver(
            int amount,
            string reason)
        {
            if (amount <= 0)
            {
                return;
            }

            silver += amount;
            SilverChanged?.Invoke(silver);
            Debug.Log(
                $"{reason}: +{amount} silver " +
                $"(Toplam: {silver})",
                this
            );
        }
    }
}
