using System;
using Seaborn.Combat;
using Seaborn.Equipment;
using Seaborn.Expeditions;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Harbor
{
    public enum PrototypeHarborServiceType
    {
        Repair,
        StandardAmmunition,
        ChainAmmunition,
        GrapeshotAmmunition,
        Harpoons,
        Preparation
    }

    public enum PrototypeHarborServiceResult
    {
        Completed,
        NotAtHarbor,
        Unavailable,
        NothingToDo,
        InsufficientSilver,
        QuoteChanged
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeHarborServices :
        MonoBehaviour
    {
        [Header("Repair")]
        // New field names intentionally replace the serialized prototype 1 Silver/HP tariff.
        [SerializeField, Min(0.001f)]
        private float hullRepairSilverPerPoint = 0.025f;
        [SerializeField, Min(0.001f)]
        private float subsystemRepairSilverPerPoint = 0.1f;

        [Header("Supply bundles")]
        [SerializeField, Min(1)]
        private int standardBundleSize = 40;
        [SerializeField, Min(0)]
        private int standardBundleCost = 20;
        [SerializeField, Min(1)]
        private int chainBundleSize = 12;
        [SerializeField, Min(0)]
        private int chainBundleCost = 18;
        [SerializeField, Min(1)]
        private int grapeshotBundleSize = 16;
        [SerializeField, Min(0)]
        private int grapeshotBundleCost = 18;
        [SerializeField, Min(1)]
        private int harpoonBundleSize = 10;
        [SerializeField, Min(0)]
        private int harpoonBundleCost = 15;

        public event Action ServiceStateChanged;
        public event Action<
            PrototypeHarborServiceType,
            int> ServiceCompleted;

        public bool CanUseServices =>
            protection != null &&
            protection.IsProtected;

        public int RepairCost
        {
            get
            {
                if (shipHealth == null)
                {
                    return 0;
                }

                float missingHealth = Mathf.Max(
                    0f,
                    shipHealth.MaximumHealth -
                    shipHealth.CurrentHealth
                );
                float subsystemDamage =
                    subsystems != null
                        ? subsystems.MissingIntegrity * Mathf.Max(0.001f, subsystemRepairSilverPerPoint)
                        : 0f;
                return Mathf.CeilToInt(
                    missingHealth * Mathf.Max(0.001f, hullRepairSilverPerPoint) +
                    subsystemDamage
                );
            }
        }

        public int Silver =>
            wallet != null ? wallet.Silver : 0;

        private PrototypeSafeHarborProtection protection;
        private PrototypeSilverWallet wallet;
        private ShipHealth shipHealth;
        private ShipSubsystemController subsystems;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private bool preparing;

        public bool CanPrepare => CanUseServices &&
            Seaborn.World.PrototypeExpeditionRegionDirector.IsHarborScene &&
            PrototypeHarborDockingDirector.Instance != null &&
            PrototypeHarborDockingDirector.Instance.IsDockedAt(PrototypeHarborStation.Trade);

        public HarborPreparationQuote GetPreparationQuote()
        {
            // Components may be attached by later runtime bootstraps.
            if (wallet == null) wallet = GetComponent<PrototypeSilverWallet>();
            if (shipHealth == null) shipHealth = GetComponentInChildren<ShipHealth>();
            if (subsystems == null && shipHealth != null)
                subsystems = shipHealth.GetComponent<ShipSubsystemController>();
            if (broadside == null) broadside = GetComponentInChildren<BroadsideController>();
            if (harpoons == null) harpoons = GetComponent<HarpoonHuntingController>();
            if (wallet == null || shipHealth == null || shipHealth.IsSunk || broadside == null ||
                harpoons == null || harpoons.SelectedHarpoon == null) return null;
            return new HarborPreparationQuote(RepairCost,
                QuoteSupply(PrototypeHarborServiceType.StandardAmmunition,
                    broadside.GetAmmunitionStock(AmmunitionType.Standard), HarborPreparationQuote.StandardTarget),
                QuoteSupply(PrototypeHarborServiceType.ChainAmmunition,
                    broadside.GetAmmunitionStock(AmmunitionType.Chain), HarborPreparationQuote.ChainTarget),
                QuoteSupply(PrototypeHarborServiceType.GrapeshotAmmunition,
                    broadside.GetAmmunitionStock(AmmunitionType.Grapeshot), HarborPreparationQuote.GrapeshotTarget),
                QuoteSupply(PrototypeHarborServiceType.Harpoons,
                    harpoons.HarpoonStock, HarborPreparationQuote.HarpoonTarget),
                harpoons.SelectedHarpoonId, harpoons.SelectedHarpoon.displayName);
        }

        private PreparationSupply QuoteSupply(PrototypeHarborServiceType type, int stock, int target)
        {
            return new PreparationSupply(Mathf.Max(0, stock), target,
                Mathf.Max(1, GetBundleSize(type)), Mathf.Max(0, GetServiceCost(type)));
        }

        public PrototypeHarborServiceResult TryPrepare(HarborPreparationQuote displayedQuote)
        {
            if (preparing) return PrototypeHarborServiceResult.Unavailable;
            if (!CanPrepare) return PrototypeHarborServiceResult.NotAtHarbor;
            var quote = GetPreparationQuote();
            if (quote == null) return PrototypeHarborServiceResult.Unavailable;
            if (!quote.Matches(displayedQuote)) return PrototypeHarborServiceResult.QuoteChanged;
            if (!quote.HasWork) return PrototypeHarborServiceResult.NothingToDo;
            preparing = true;
            try
            {
                // One debit after all validation; never buy just the affordable subset.
                if (!TryPay(quote.TotalCost, "Sefer hazırlığı"))
                    return PrototypeHarborServiceResult.InsufficientSilver;
                if (quote.RepairCost > 0)
                {
                    shipHealth.RestoreToFullHealth();
                    subsystems?.RestoreAll();
                }
                broadside.AddAmmunition(AmmunitionType.Standard, quote.Standard.AddedStock);
                broadside.AddAmmunition(AmmunitionType.Chain, quote.Chain.AddedStock);
                broadside.AddAmmunition(AmmunitionType.Grapeshot, quote.Grapeshot.AddedStock);
                harpoons.AddHarpoons(quote.Harpoon.AddedStock);
                Complete(PrototypeHarborServiceType.Preparation, quote.TotalCost);
                return PrototypeHarborServiceResult.Completed;
            }
            finally { preparing = false; }
        }

        public static void EnsureAttached(Transform player)
        {
            if (player == null)
            {
                return;
            }

            PrototypeHarborServices services =
                player.GetComponent<
                    PrototypeHarborServices>();

            if (services == null)
            {
                services = player.gameObject.AddComponent<
                    PrototypeHarborServices>();
            }

            services.Bind(player);
        }

        public int GetBundleSize(
            PrototypeHarborServiceType serviceType)
        {
            if (TryGetAmmunitionDefinition(serviceType, out AmmunitionDefinition definition))
                return definition.bundleSize;

            switch (serviceType)
            {
                case PrototypeHarborServiceType
                    .StandardAmmunition:
                    return standardBundleSize;
                case PrototypeHarborServiceType
                    .ChainAmmunition:
                    return chainBundleSize;
                case PrototypeHarborServiceType
                    .GrapeshotAmmunition:
                    return grapeshotBundleSize;
                case PrototypeHarborServiceType.Harpoons:
                    return harpoons?.SelectedHarpoon != null
                        ? harpoons.SelectedHarpoon.bundleSize
                        : harpoonBundleSize;
                default:
                    return 0;
            }
        }

        public int GetServiceCost(
            PrototypeHarborServiceType serviceType)
        {
            if (TryGetAmmunitionDefinition(serviceType, out AmmunitionDefinition definition))
                return definition.silverPricePerBundle;

            switch (serviceType)
            {
                case PrototypeHarborServiceType.Repair:
                    return RepairCost;
                case PrototypeHarborServiceType
                    .StandardAmmunition:
                    return standardBundleCost;
                case PrototypeHarborServiceType
                    .ChainAmmunition:
                    return chainBundleCost;
                case PrototypeHarborServiceType
                    .GrapeshotAmmunition:
                    return grapeshotBundleCost;
                case PrototypeHarborServiceType.Harpoons:
                    return harpoons?.SelectedHarpoon != null
                        ? harpoons.SelectedHarpoon
                            .silverPricePerBundle
                        : harpoonBundleCost;
                default:
                    return 0;
            }
        }

        private static bool TryGetAmmunitionDefinition(
            PrototypeHarborServiceType serviceType, out AmmunitionDefinition definition)
        {
            string id = serviceType switch
            {
                PrototypeHarborServiceType.StandardAmmunition => "standard",
                PrototypeHarborServiceType.ChainAmmunition => "chain",
                PrototypeHarborServiceType.GrapeshotAmmunition => "grapeshot",
                _ => null
            };
            definition = null;
            return id != null && EquipmentCatalog.TryGetAmmunition(id, out definition);
        }

        public PrototypeHarborServiceResult TryRepair()
        {
            PrototypeHarborServiceResult validation =
                ValidateService(shipHealth != null);

            if (validation !=
                PrototypeHarborServiceResult.Completed)
            {
                return validation;
            }

            int cost = RepairCost;
            if (cost <= 0)
            {
                return PrototypeHarborServiceResult
                    .NothingToDo;
            }

            if (!TryPay(cost, "Gemi onarımı"))
            {
                return PrototypeHarborServiceResult
                    .InsufficientSilver;
            }

            shipHealth.RestoreToFullHealth();
            subsystems?.RestoreAll();
            Complete(
                PrototypeHarborServiceType.Repair,
                cost
            );
            return PrototypeHarborServiceResult.Completed;
        }

        public PrototypeHarborServiceResult
            TryRestockAmmunition(
                AmmunitionType ammunitionType)
        {
            PrototypeHarborServiceResult validation =
                ValidateService(broadside != null);

            if (validation !=
                PrototypeHarborServiceResult.Completed)
            {
                return validation;
            }

            PrototypeHarborServiceType serviceType =
                ToServiceType(ammunitionType);
            int cost = GetServiceCost(serviceType);

            if (!TryPay(cost, "Top mühimmatı ikmali"))
            {
                return PrototypeHarborServiceResult
                    .InsufficientSilver;
            }

            broadside.AddAmmunition(
                ammunitionType,
                GetBundleSize(serviceType)
            );
            Complete(serviceType, cost);
            return PrototypeHarborServiceResult.Completed;
        }

        public PrototypeHarborServiceResult
            TryRestockHarpoons()
        {
            PrototypeHarborServiceResult validation =
                ValidateService(harpoons != null);

            if (validation !=
                PrototypeHarborServiceResult.Completed)
            {
                return validation;
            }

            int cost = GetServiceCost(
                PrototypeHarborServiceType.Harpoons
            );

            if (!TryPay(cost, "Zıpkın ikmali"))
            {
                return PrototypeHarborServiceResult
                    .InsufficientSilver;
            }

            harpoons.AddHarpoons(
                GetBundleSize(
                    PrototypeHarborServiceType.Harpoons
                )
            );
            Complete(
                PrototypeHarborServiceType.Harpoons,
                cost
            );
            return PrototypeHarborServiceResult.Completed;
        }

        [ContextMenu("Prototype/Repair Ship")]
        private void RepairFromInspector()
        {
            Debug.Log(
                $"Onarım sonucu: {TryRepair()}",
                this
            );
        }

        [ContextMenu("Prototype/Restock Standard")]
        private void RestockStandardFromInspector()
        {
            Debug.Log(
                $"Standart ikmal sonucu: " +
                $"{TryRestockAmmunition(AmmunitionType.Standard)}",
                this
            );
        }

        [ContextMenu("Prototype/Restock Chain")]
        private void RestockChainFromInspector()
        {
            Debug.Log(
                $"Zincirli ikmal sonucu: " +
                $"{TryRestockAmmunition(AmmunitionType.Chain)}",
                this
            );
        }

        [ContextMenu("Prototype/Restock Grapeshot")]
        private void RestockGrapeshotFromInspector()
        {
            Debug.Log(
                $"Saçma ikmal sonucu: " +
                $"{TryRestockAmmunition(AmmunitionType.Grapeshot)}",
                this
            );
        }

        [ContextMenu("Prototype/Restock Harpoons")]
        private void RestockHarpoonsFromInspector()
        {
            Debug.Log(
                $"Zıpkın ikmal sonucu: {TryRestockHarpoons()}",
                this
            );
        }

        private PrototypeHarborServiceResult ValidateService(
            bool dependencyAvailable)
        {
            if (preparing) return PrototypeHarborServiceResult.Unavailable;
            if (!CanUseServices)
            {
                return PrototypeHarborServiceResult
                    .NotAtHarbor;
            }

            return dependencyAvailable && wallet != null
                ? PrototypeHarborServiceResult.Completed
                : PrototypeHarborServiceResult.Unavailable;
        }

        private bool TryPay(int cost, string reason)
        {
            return cost == 0 ||
                wallet.TrySpendSilver(cost, reason);
        }

        private void Complete(
            PrototypeHarborServiceType serviceType,
            int cost)
        {
            ServiceCompleted?.Invoke(serviceType, cost);
            ServiceStateChanged?.Invoke();

            Debug.Log(
                $"Liman servisi tamamlandı: {serviceType}, " +
                $"{cost} silver.",
                this
            );
        }

        private void Bind(Transform player)
        {
            if (protection != null &&
                protection.transform == player && wallet != null && shipHealth != null &&
                subsystems != null && broadside != null && harpoons != null)
            {
                return;
            }

            Unsubscribe();

            protection = player.GetComponent<
                PrototypeSafeHarborProtection>();
            wallet = player.GetComponent<
                PrototypeSilverWallet>();
            shipHealth = player.GetComponentInChildren<
                ShipHealth>();
            subsystems = shipHealth != null
                ? shipHealth.GetComponent<
                    ShipSubsystemController>()
                : null;
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponent<
                HarpoonHuntingController>();

            Subscribe();
            ServiceStateChanged?.Invoke();
        }

        private void Subscribe()
        {
            if (protection != null)
            {
                protection.ProtectionChanged +=
                    HandleProtectionChanged;
            }

            if (wallet != null)
            {
                wallet.SilverChanged += HandleSilverChanged;
            }

            if (shipHealth != null)
            {
                shipHealth.HealthChanged +=
                    HandleHealthChanged;
            }

            if (broadside != null)
            {
                broadside.AmmunitionStateChanged +=
                    HandleStateChanged;
            }

            if (harpoons != null)
            {
                harpoons.HuntingStateChanged +=
                    HandleStateChanged;
            }
        }

        private void Unsubscribe()
        {
            if (protection != null)
            {
                protection.ProtectionChanged -=
                    HandleProtectionChanged;
            }

            if (wallet != null)
            {
                wallet.SilverChanged -= HandleSilverChanged;
            }

            if (shipHealth != null)
            {
                shipHealth.HealthChanged -=
                    HandleHealthChanged;
            }

            if (broadside != null)
            {
                broadside.AmmunitionStateChanged -=
                    HandleStateChanged;
            }

            if (harpoons != null)
            {
                harpoons.HuntingStateChanged -=
                    HandleStateChanged;
            }
        }

        private void HandleProtectionChanged(bool isProtected)
        {
            ServiceStateChanged?.Invoke();
        }

        private void HandleSilverChanged(int value)
        {
            ServiceStateChanged?.Invoke();
        }

        private void HandleHealthChanged(
            float current,
            float maximum)
        {
            ServiceStateChanged?.Invoke();
        }

        private void HandleStateChanged()
        {
            ServiceStateChanged?.Invoke();
        }

        private static PrototypeHarborServiceType
            ToServiceType(AmmunitionType ammunitionType)
        {
            switch (ammunitionType)
            {
                case AmmunitionType.Chain:
                    return PrototypeHarborServiceType
                        .ChainAmmunition;
                case AmmunitionType.Grapeshot:
                    return PrototypeHarborServiceType
                        .GrapeshotAmmunition;
                default:
                    return PrototypeHarborServiceType
                        .StandardAmmunition;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
