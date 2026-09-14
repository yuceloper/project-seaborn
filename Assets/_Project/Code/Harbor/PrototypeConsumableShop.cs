using System;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Harbor
{
    public enum ConsumableOffer
    {
        TortugaTonic,
        TortugaTonicCrate,
        LightOfTortuga
    }

    public enum ConsumablePurchaseResult
    {
        Completed,
        NotAtTradeDock,
        InsufficientSilver,
        MissingInventory
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeConsumableShop : MonoBehaviour
    {
        public event Action ShopChanged;

        private PrototypeSilverWallet wallet;
        private PrototypeShipConsumables consumables;

        public static PrototypeConsumableShop EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeConsumableShop shop =
                player.GetComponent<
                    PrototypeConsumableShop>();
            if (shop == null)
            {
                shop = player.gameObject.AddComponent<
                    PrototypeConsumableShop>();
            }
            shop.Bind(player);
            return shop;
        }

        public int Price(ConsumableOffer offer)
        {
            return offer switch
            {
                ConsumableOffer.TortugaTonic => 90,
                ConsumableOffer.TortugaTonicCrate => 240,
                ConsumableOffer.LightOfTortuga => 650,
                _ => 0
            };
        }

        public ConsumablePurchaseResult TryPurchase(
            ConsumableOffer offer)
        {
            PrototypeHarborDockingDirector docking =
                PrototypeHarborDockingDirector.Instance;
            if (docking == null ||
                !docking.IsDockedAt(
                    PrototypeHarborStation.Trade))
            {
                return ConsumablePurchaseResult
                    .NotAtTradeDock;
            }

            if (wallet == null || consumables == null)
            {
                return ConsumablePurchaseResult
                    .MissingInventory;
            }

            int price = Price(offer);
            if (!wallet.TrySpendSilver(
                    price,
                    OfferName(offer)))
            {
                return ConsumablePurchaseResult
                    .InsufficientSilver;
            }

            switch (offer)
            {
                case ConsumableOffer.TortugaTonic:
                    consumables.AddTortugaTonics(1);
                    break;
                case ConsumableOffer.TortugaTonicCrate:
                    consumables.AddTortugaTonics(3);
                    break;
                case ConsumableOffer.LightOfTortuga:
                    consumables.AddLightsOfTortuga(1);
                    break;
            }

            ShopChanged?.Invoke();
            Debug.Log(
                $"{OfferName(offer)} satın alındı.",
                this
            );
            return ConsumablePurchaseResult.Completed;
        }

        public static string OfferName(
            ConsumableOffer offer)
        {
            return offer switch
            {
                ConsumableOffer.TortugaTonic =>
                    "Tortuga Tonic",
                ConsumableOffer.TortugaTonicCrate =>
                    "Tortuga Tonic Sandığı",
                ConsumableOffer.LightOfTortuga =>
                    "Light of Tortuga",
                _ => "Sarf Malzemesi"
            };
        }

        private void Bind(Transform player)
        {
            wallet = player.GetComponentInChildren<
                PrototypeSilverWallet>();
            consumables = player.GetComponentInChildren<
                PrototypeShipConsumables>();
        }
    }
}
