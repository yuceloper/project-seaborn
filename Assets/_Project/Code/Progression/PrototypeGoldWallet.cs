using System;
using UnityEngine;

namespace Seaborn.Progression
{
    [DisallowMultipleComponent]
    public sealed class PrototypeGoldWallet : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private int gold;

        public event Action<int> GoldChanged;

        public int Gold => gold;

        public static PrototypeGoldWallet EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeGoldWallet wallet =
                player.GetComponent<PrototypeGoldWallet>();
            if (wallet == null)
            {
                wallet = player.gameObject.AddComponent<
                    PrototypeGoldWallet>();
            }
            return wallet;
        }

        public void AddGold(int amount, string reason)
        {
            if (amount <= 0) return;

            gold += amount;
            GoldChanged?.Invoke(gold);
            Debug.Log(
                $"{reason}: +{amount} gold (Toplam: {gold})",
                this
            );
        }

        public void RestoreGold(int amount)
        {
            gold = Mathf.Max(0, amount);
            GoldChanged?.Invoke(gold);
        }
    }
}
