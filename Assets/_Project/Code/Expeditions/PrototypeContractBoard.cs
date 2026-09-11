using System;
using System.Collections.Generic;
using Seaborn.Hunting;
using UnityEngine;

namespace Seaborn.Expeditions
{
    public enum PrototypeContractCadence
    {
        Expedition,
        Daily,
        Monthly
    }

    public enum PrototypeContractObjective
    {
        HuntTideback,
        HuntStormjaw,
        HuntRareCreature,
        SecureSilver,
        CompleteExpedition
    }

    public sealed class PrototypeContractDefinition
    {
        public PrototypeContractDefinition(
            string id,
            string title,
            string description,
            PrototypeContractCadence cadence,
            PrototypeContractObjective objective,
            int target,
            int silverReward)
        {
            Id = id;
            Title = title;
            Description = description;
            Cadence = cadence;
            Objective = objective;
            Target = Mathf.Max(1, target);
            SilverReward = Mathf.Max(0, silverReward);
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public PrototypeContractCadence Cadence { get; }
        public PrototypeContractObjective Objective { get; }
        public int Target { get; }
        public int SilverReward { get; }
    }

    public sealed class PrototypeContractProgress
    {
        public PrototypeContractProgress(
            PrototypeContractDefinition definition)
        {
            Definition = definition;
        }

        public PrototypeContractDefinition Definition
        {
            get;
        }

        public int Current { get; private set; }
        public bool IsComplete =>
            Current >= Definition.Target;
        public bool RewardClaimed { get; private set; }

        internal bool Add(int amount)
        {
            if (amount <= 0 || IsComplete)
            {
                return false;
            }

            Current = Mathf.Min(
                Definition.Target,
                Current + amount
            );
            return true;
        }

        internal void MarkClaimed()
        {
            RewardClaimed = true;
        }

        internal void Reset()
        {
            Current = 0;
            RewardClaimed = false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeContractBoard :
        MonoBehaviour
    {
        private const int DailyMainReward = 35;
        private const int MonthlyMainReward = 200;

        public static PrototypeContractBoard Instance
        {
            get;
            private set;
        }

        public event Action ContractsChanged;
        public event Action<PrototypeContractProgress>
            ContractCompleted;
        public event Action<PrototypeContractProgress>
            ContractRewardClaimed;

        public IReadOnlyList<PrototypeContractProgress>
            ExpeditionContracts => expeditionContracts;
        public IReadOnlyList<PrototypeContractProgress>
            DailyContracts => dailyContracts;
        public IReadOnlyList<PrototypeContractProgress>
            MonthlyContracts => monthlyContracts;

        public PrototypeContractProgress
            SelectedExpeditionContract
        {
            get;
            private set;
        }

        public int DailyCompletedCount =>
            CountCompleted(dailyContracts);
        public int MonthlyCompletedCount =>
            CountCompleted(monthlyContracts);
        public bool DailyMainRewardClaimed =>
            dailyMainRewardClaimed;
        public bool MonthlyMainRewardClaimed =>
            monthlyMainRewardClaimed;
        public string DailyPeriodKey => dailyPeriodKey;
        public string MonthlyPeriodKey => monthlyPeriodKey;

        private readonly List<PrototypeContractProgress>
            expeditionContracts = new();
        private readonly List<PrototypeContractProgress>
            dailyContracts = new();
        private readonly List<PrototypeContractProgress>
            monthlyContracts = new();

        private Transform player;
        private PrototypeHuntCargo cargo;
        private PrototypeSilverWallet wallet;
        private PrototypeExpeditionDirector director;
        private string dailyPeriodKey;
        private string monthlyPeriodKey;
        private float nextPeriodCheck;
        private bool dailyMainRewardClaimed;
        private bool monthlyMainRewardClaimed;

        public static void EnsureCreated(
            Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            PrototypeContractBoard board =
                FindFirstObjectByType<
                    PrototypeContractBoard>();

            if (board == null)
            {
                GameObject boardObject =
                    new GameObject(
                        "Prototype Contract Board"
                    );
                board = boardObject.AddComponent<
                    PrototypeContractBoard>();
            }

            board.BindPlayer(playerTransform);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildCatalog();
            RefreshPeriods(true);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextPeriodCheck)
            {
                return;
            }

            nextPeriodCheck =
                Time.unscaledTime + 30f;
            RefreshPeriods(false);
        }

        public bool SelectExpeditionContract(
            string contractId)
        {
            if (director == null ||
                (director.State !=
                    PrototypeExpeditionState.AtHarbor &&
                 director.State !=
                    PrototypeExpeditionState.Completed))
            {
                return false;
            }

            foreach (PrototypeContractProgress contract
                     in expeditionContracts)
            {
                if (contract.Definition.Id != contractId)
                {
                    continue;
                }

                SelectedExpeditionContract = contract;
                contract.Reset();
                ContractsChanged?.Invoke();

                Debug.Log(
                    $"Sefer kontratı seçildi: " +
                    $"{contract.Definition.Title}",
                    this
                );
                return true;
            }

            return false;
        }

        private void BindPlayer(
            Transform playerTransform)
        {
            if (player == playerTransform &&
                cargo != null &&
                wallet != null &&
                director != null)
            {
                return;
            }

            Unsubscribe();
            player = playerTransform;
            cargo =
                player.GetComponentInChildren<
                    PrototypeHuntCargo>();
            wallet =
                player.GetComponentInChildren<
                    PrototypeSilverWallet>();
            director =
                PrototypeExpeditionDirector.Instance;

            if (cargo != null)
            {
                cargo.CatchAdded += HandleCatchAdded;
                cargo.CargoSecured += HandleCargoSecured;
            }

            if (director != null)
            {
                director.StateChanged +=
                    HandleExpeditionStateChanged;
            }

            ContractsChanged?.Invoke();
        }

        private void HandleCatchAdded(
            string creatureName,
            int silverValue)
        {
            if (creatureName.Contains(
                    "Stormjaw",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddProgress(
                    PrototypeContractObjective
                        .HuntStormjaw,
                    1
                );
                AddProgress(
                    PrototypeContractObjective
                        .HuntRareCreature,
                    1
                );
            }
            else if (creatureName.Contains(
                         "Tideback",
                         StringComparison.OrdinalIgnoreCase))
            {
                AddProgress(
                    PrototypeContractObjective
                        .HuntTideback,
                    1
                );
            }
        }

        private void HandleCargoSecured(int value)
        {
            AddProgress(
                PrototypeContractObjective.SecureSilver,
                value
            );

            if (director != null &&
                director.State ==
                    PrototypeExpeditionState.Completed)
            {
                ClaimExpeditionReward();
                ClaimReadyRecurringRewards();
            }
        }

        private void HandleExpeditionStateChanged(
            PrototypeExpeditionState state)
        {
            if (state ==
                PrototypeExpeditionState.Underway)
            {
                SelectedExpeditionContract?.Reset();
                ContractsChanged?.Invoke();

                if (SelectedExpeditionContract != null)
                {
                    Debug.Log(
                        $"Aktif sefer kontratı: " +
                        $"{SelectedExpeditionContract.Definition.Title} " +
                        $"({SelectedExpeditionContract.Definition.Description})",
                        this
                    );
                }

                return;
            }

            if (state !=
                PrototypeExpeditionState.Completed)
            {
                return;
            }

            AddProgress(
                PrototypeContractObjective
                    .CompleteExpedition,
                1
            );
            ClaimExpeditionReward();
            ClaimReadyRecurringRewards();
        }

        private void AddProgress(
            PrototypeContractObjective objective,
            int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (director != null &&
                (director.IsActive ||
                 director.State ==
                    PrototypeExpeditionState.Completed) &&
                SelectedExpeditionContract != null &&
                SelectedExpeditionContract.Definition
                    .Objective == objective)
            {
                Advance(
                    SelectedExpeditionContract,
                    amount
                );
            }

            AdvanceMatching(
                dailyContracts,
                objective,
                amount
            );
            AdvanceMatching(
                monthlyContracts,
                objective,
                amount
            );
            ContractsChanged?.Invoke();
        }

        private void AdvanceMatching(
            List<PrototypeContractProgress> contracts,
            PrototypeContractObjective objective,
            int amount)
        {
            foreach (PrototypeContractProgress contract
                     in contracts)
            {
                if (contract.Definition.Objective ==
                    objective)
                {
                    Advance(contract, amount);
                }
            }
        }

        private void Advance(
            PrototypeContractProgress contract,
            int amount)
        {
            bool wasComplete = contract.IsComplete;

            if (!contract.Add(amount))
            {
                return;
            }

            if (!wasComplete && contract.IsComplete)
            {
                ContractCompleted?.Invoke(contract);
                Debug.Log(
                    $"Kontrat hedefi tamamlandı: " +
                    $"{contract.Definition.Title}",
                    this
                );
            }
        }

        private void ClaimExpeditionReward()
        {
            PrototypeContractProgress contract =
                SelectedExpeditionContract;

            if (contract == null ||
                !contract.IsComplete ||
                contract.RewardClaimed)
            {
                return;
            }

            ClaimReward(contract);
        }

        private void ClaimReadyRecurringRewards()
        {
            ClaimReady(dailyContracts);
            ClaimReady(monthlyContracts);

            if (!dailyMainRewardClaimed &&
                DailyCompletedCount >= 2)
            {
                dailyMainRewardClaimed = true;
                wallet?.AddSilver(
                    DailyMainReward,
                    "Günlük kontrat ana ödülü"
                );
            }

            if (!monthlyMainRewardClaimed &&
                MonthlyCompletedCount >= 2)
            {
                monthlyMainRewardClaimed = true;
                wallet?.AddSilver(
                    MonthlyMainReward,
                    "Aylık seyir defteri ana ödülü"
                );
            }

            ContractsChanged?.Invoke();
        }

        private void ClaimReady(
            List<PrototypeContractProgress> contracts)
        {
            foreach (PrototypeContractProgress contract
                     in contracts)
            {
                if (contract.IsComplete &&
                    !contract.RewardClaimed)
                {
                    ClaimReward(contract);
                }
            }
        }

        private void ClaimReward(
            PrototypeContractProgress contract)
        {
            if (wallet == null)
            {
                return;
            }

            contract.MarkClaimed();
            wallet.AddSilver(
                contract.Definition.SilverReward,
                $"Kontrat ödülü — " +
                $"{contract.Definition.Title}"
            );
            ContractRewardClaimed?.Invoke(contract);
        }

        private void RefreshPeriods(bool force)
        {
            DateTime utcNow = DateTime.UtcNow;
            string newDaily =
                utcNow.ToString("yyyy-MM-dd");
            string newMonthly =
                utcNow.ToString("yyyy-MM");

            if (force || dailyPeriodKey != newDaily)
            {
                dailyPeriodKey = newDaily;
                ResetContracts(dailyContracts);
                dailyMainRewardClaimed = false;
            }

            if (force || monthlyPeriodKey != newMonthly)
            {
                monthlyPeriodKey = newMonthly;
                ResetContracts(monthlyContracts);
                monthlyMainRewardClaimed = false;
            }

            ContractsChanged?.Invoke();
        }

        private void BuildCatalog()
        {
            expeditionContracts.Add(
                Create(
                    "coastal-hunt",
                    "Kıyı Avı",
                    "2 Tideback avla ve limana dön.",
                    PrototypeContractCadence.Expedition,
                    PrototypeContractObjective.HuntTideback,
                    2,
                    35
                )
            );
            expeditionContracts.Add(
                Create(
                    "dangerous-hunt",
                    "Tehlikeli Av",
                    "Stormjaw avla ve limana dön.",
                    PrototypeContractCadence.Expedition,
                    PrototypeContractObjective.HuntStormjaw,
                    1,
                    100
                )
            );
            expeditionContracts.Add(
                Create(
                    "free-sail",
                    "Serbest Sefer",
                    "En az 90 silver yükle limana dön.",
                    PrototypeContractCadence.Expedition,
                    PrototypeContractObjective.SecureSilver,
                    90,
                    25
                )
            );

            dailyContracts.Add(
                Create(
                    "daily-secure-90",
                    "Güvenli Dönüş",
                    "Toplam 90 silver güvenceye al.",
                    PrototypeContractCadence.Daily,
                    PrototypeContractObjective.SecureSilver,
                    90,
                    10
                )
            );
            dailyContracts.Add(
                Create(
                    "daily-tidebacks",
                    "Kıyı Avcısı",
                    "2 Tideback avla.",
                    PrototypeContractCadence.Daily,
                    PrototypeContractObjective.HuntTideback,
                    2,
                    10
                )
            );
            dailyContracts.Add(
                Create(
                    "daily-voyages",
                    "Sağ Salim",
                    "2 başarılı sefer tamamla.",
                    PrototypeContractCadence.Daily,
                    PrototypeContractObjective.CompleteExpedition,
                    2,
                    15
                )
            );

            monthlyContracts.Add(
                Create(
                    "monthly-secure-1500",
                    "Dolu Ambarlar",
                    "Toplam 1.500 silver güvenceye al.",
                    PrototypeContractCadence.Monthly,
                    PrototypeContractObjective.SecureSilver,
                    1500,
                    100
                )
            );
            monthlyContracts.Add(
                Create(
                    "monthly-voyages",
                    "Tecrübeli Kaptan",
                    "12 başarılı sefer tamamla.",
                    PrototypeContractCadence.Monthly,
                    PrototypeContractObjective.CompleteExpedition,
                    12,
                    100
                )
            );
            monthlyContracts.Add(
                Create(
                    "monthly-rare-hunts",
                    "Derinliklerin Avcısı",
                    "3 nadir deniz canlısı avla.",
                    PrototypeContractCadence.Monthly,
                    PrototypeContractObjective.HuntRareCreature,
                    3,
                    120
                )
            );

            SelectedExpeditionContract =
                expeditionContracts[2];
        }

        private static PrototypeContractProgress Create(
            string id,
            string title,
            string description,
            PrototypeContractCadence cadence,
            PrototypeContractObjective objective,
            int target,
            int silverReward)
        {
            return new PrototypeContractProgress(
                new PrototypeContractDefinition(
                    id,
                    title,
                    description,
                    cadence,
                    objective,
                    target,
                    silverReward
                )
            );
        }

        private static int CountCompleted(
            List<PrototypeContractProgress> contracts)
        {
            int count = 0;

            foreach (PrototypeContractProgress contract
                     in contracts)
            {
                if (contract.IsComplete)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ResetContracts(
            List<PrototypeContractProgress> contracts)
        {
            foreach (PrototypeContractProgress contract
                     in contracts)
            {
                contract.Reset();
            }
        }

        private void Unsubscribe()
        {
            if (cargo != null)
            {
                cargo.CatchAdded -= HandleCatchAdded;
                cargo.CargoSecured -= HandleCargoSecured;
            }

            if (director != null)
            {
                director.StateChanged -=
                    HandleExpeditionStateChanged;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
