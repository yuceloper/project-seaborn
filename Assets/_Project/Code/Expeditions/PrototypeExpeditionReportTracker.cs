using System;
using Seaborn.Combat;
using Seaborn.Harbor;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using Seaborn.World;
using UnityEngine;

namespace Seaborn.Expeditions
{
    public sealed class ExpeditionReport
    {
        public ExpeditionEconomyLedger Ledger { get; internal set; }
        public float Duration { get; internal set; }
        public bool Sunk { get; internal set; }
        public int DepartureRepairQuote { get; internal set; }
        public int ReturnRepairQuote { get; internal set; }
        public int[] Materials { get; internal set; }
        public int RepairCost => Mathf.Max(0, ReturnRepairQuote - DepartureRepairQuote);
        public decimal EstimatedNet => Ledger.EstimatedNet(DepartureRepairQuote, ReturnRepairQuote);
    }

    // Attached to the persistent player, not the scene-owned expedition director.
    [DisallowMultipleComponent]
    public sealed class PrototypeExpeditionReportTracker : MonoBehaviour
    {
        public event Action ReportReady;
        public ExpeditionReport LastReport { get; private set; }
        public bool IsRecording => ledger != null;

        private ExpeditionEconomyLedger ledger;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;
        private PrototypeHarborServices services;
        private PrototypeRegionalLootInventory materials;
        private ShipHealth health;
        private bool wasHarbor;
        private bool initialized;
        private bool sinkPending;
        private bool awaitHarborAfterSinking;
        private float elapsed;
        private int departureRepairQuote;
        private int harborFrames;
        private readonly int[] startingMaterials = new int[4];

        public static void EnsureAttached(Transform player)
        {
            if (player == null) return;
            PrototypeExpeditionReportTracker tracker =
                player.GetComponent<PrototypeExpeditionReportTracker>();
            if (tracker == null)
                tracker = player.gameObject.AddComponent<PrototypeExpeditionReportTracker>();
            tracker.Bind();
            tracker.ObserveMap();
            Seaborn.UI.PrototypeExpeditionReportPanel.EnsureAttached(tracker);
        }

        private void Bind()
        {
            if (initialized) return;
            broadside = GetComponentInChildren<BroadsideController>();
            harpoons = GetComponentInChildren<HarpoonHuntingController>();
            cargo = GetComponentInChildren<PrototypeHuntCargo>();
            wallet = GetComponentInChildren<PrototypeSilverWallet>();
            materials = GetComponentInChildren<PrototypeRegionalLootInventory>();
            health = GetComponentInChildren<ShipHealth>();
            PrototypeHarborServices.EnsureAttached(transform);
            services = GetComponent<PrototypeHarborServices>();
            if (broadside != null) broadside.AmmunitionConsumed += OnAmmunition;
            if (harpoons != null) harpoons.HarpoonFired += OnHarpoon;
            if (cargo != null)
            {
                cargo.CargoSecured += OnSecured;
                cargo.CargoLost += OnLost;
            }
            if (wallet != null) wallet.SilverAdded += OnSilver;
            if (health != null) health.Sunk += OnSunk;
            wasHarbor = PrototypeExpeditionRegionDirector.IsHarborScene;
            initialized = true;
            if (!wasHarbor) Begin(); // Supports directly playing a sea scene in the Editor.
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            // Use scaled gameplay time, excluding scene loads and paused time.
            if (ledger != null && !sinkPending &&
                !PrototypeExpeditionRegionDirector.IsHarborScene)
                elapsed += Time.deltaTime;
            if (sinkPending)
            {
                Finish(true);
                sinkPending = false;
                awaitHarborAfterSinking = true;
            }
            ObserveMap();
            // Delivery and contract callbacks run in Update. Freeze only afterwards,
            // including an empty return and every credit made during delivery.
            if (ledger != null && PrototypeExpeditionRegionDirector.IsHarborScene)
            {
                // Scene bootstraps can create the delivery zone after Update has
                // begun. Give it an Update and wait for actual cargo settlement.
                harborFrames++;
                if (harborFrames >= 2 && (cargo == null || !cargo.HasCargo))
                    Finish(false);
            }
        }

        private void ObserveMap()
        {
            bool harbor = PrototypeExpeditionRegionDirector.IsHarborScene;
            if (harbor) awaitHarborAfterSinking = false;
            if (wasHarbor && !harbor && ledger == null && !awaitHarborAfterSinking)
                Begin();
            wasHarbor = harbor;
        }

        private void Begin()
        {
            ledger = new ExpeditionEconomyLedger();
            elapsed = 0f;
            harborFrames = 0;
            departureRepairQuote = services != null ? services.RepairCost : 0;
            for (int i = 0; i < startingMaterials.Length; i++)
                startingMaterials[i] = materials != null ? materials.Get((RegionalMaterialType)i) : 0;
        }

        private void OnAmmunition(AmmunitionType type, int count)
        {
            if (ledger == null || sinkPending || services == null) return;
            int kind = type == AmmunitionType.Chain ? 1 :
                type == AmmunitionType.Grapeshot ? 2 : 0;
            PrototypeHarborServiceType service = kind == 1
                ? PrototypeHarborServiceType.ChainAmmunition
                : kind == 2 ? PrototypeHarborServiceType.GrapeshotAmmunition
                : PrototypeHarborServiceType.StandardAmmunition;
            ledger.Consume(kind, count, services.GetServiceCost(service),
                Math.Max(1, services.GetBundleSize(service)));
        }

        private void OnHarpoon()
        {
            if (ledger == null || sinkPending || harpoons.SelectedHarpoon == null) return;
            var definition = harpoons.SelectedHarpoon;
            ledger.Consume(3, 1, definition.silverPricePerBundle, Math.Max(1, definition.bundleSize));
        }

        private void OnSilver(int value) => ledger?.Earn(value);
        private void OnSecured(int value) => ledger?.Secure(value);
        private void OnLost(int value) => ledger?.Lose(value);
        private void OnSunk()
        {
            // Defer freeze until all Sunk/CargoLost subscribers have run.
            if (ledger != null) sinkPending = true;
        }

        private void Finish(bool sunk)
        {
            if (ledger == null) return;
            int[] gained = new int[4];
            for (int i = 0; i < gained.Length; i++)
                gained[i] = materials != null
                    ? Mathf.Max(0, materials.Get((RegionalMaterialType)i) - startingMaterials[i]) : 0;
            LastReport = new ExpeditionReport
            {
                Ledger = ledger,
                Duration = elapsed,
                Sunk = sunk,
                DepartureRepairQuote = departureRepairQuote,
                // A sunk ship has a separate recovery flow, not a normal repair quote.
                ReturnRepairQuote = sunk ? departureRepairQuote : services.RepairCost,
                Materials = gained
            };
            ledger = null;
            ReportReady?.Invoke();
        }

        private void OnDestroy()
        {
            if (broadside != null) broadside.AmmunitionConsumed -= OnAmmunition;
            if (harpoons != null) harpoons.HarpoonFired -= OnHarpoon;
            if (cargo != null)
            {
                cargo.CargoSecured -= OnSecured;
                cargo.CargoLost -= OnLost;
            }
            if (wallet != null) wallet.SilverAdded -= OnSilver;
            if (health != null) health.Sunk -= OnSunk;
        }
    }
}
