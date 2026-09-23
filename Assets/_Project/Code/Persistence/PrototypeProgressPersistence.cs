using System;
using System.IO;
using Seaborn.Combat;
using Seaborn.Hunting;
using Seaborn.Progression;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Persistence
{
    [DisallowMultipleComponent]
    public sealed class PrototypeProgressPersistence : MonoBehaviour
    {
        private const int CurrentVersion = 1;
        private const float ScanInterval = 0.5f;
        private const string SaveFileName = "project-seaborn-progress.json";

        [Serializable]
        private sealed class SaveData
        {
            public int version = CurrentVersion;
            public int silver;
            public int gold;
            public int tortugaTonics = 3;
            public int lightsOfTortuga = 1;
            public int corsairRum = 2;
            public int galeElixirs = 2;
            public int ironbarkBrews = 2;
            public int availableDeckExtensions = 1;
            public int starterDeckExtensions;
            public int ratDeckExtensions;
            public int dreadwakeDeckExtensions;
            public int selectedAmmunition;
            public int standardStock;
            public int chainStock;
            public int grapeshotStock;
            public int harpoonStock;
            public int lightHarpoonStock;
            public int heavyHarpoonStock;
            public string selectedHarpoonId;
            public string cannonId;
            public string[] cannonSlots;
            public string[] cannonItemIds;
            public CannonItem[] cannonItems;
            public int installedCannons;
            public string sailId;
            public int iron6LbCannons;
            public int iron12LbCannons;
            public int patchedCanvasSails;
            public int ratSails;
            public bool ownsStarterSloop;
            public bool ownsRatSailsShip;
            public bool ownsDreadwake;
            public string activeShipId;
            public int captainExperience;
            public int cannonMasteryRank;
            public int rangefindingRank;
            public int harpoonMasteryRank;
            public int harpoonRiggingRank;
            public int reinforcedHullRank;
            public int fineSailsRank;
            public int hullLevel;
            public int cannonLevel;
            public int harpoonLevel;
            public int tideOil;
            public int stormjawScales;
            public int corsairIron;
            public int lostChartFragments;
        }

        private PrototypeSilverWallet wallet;
        private PrototypeGoldWallet goldWallet;
        private PrototypeShipConsumables consumables;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeShipEquipment equipment;
        private PrototypeRegionalLootInventory materials;
        private ShipLoadout loadout;
        private PrototypeEquipmentInventory inventory;
        private PrototypeFleetInventory fleet;
        private PrototypeCaptainProgression captainProgression;
        private PrototypeCaptainSkills captainSkills;
        private PrototypeDeckExtensionInventory deckExtensions;
        private SaveData lastSnapshot;
        private float nextScanTime;
        private bool loaded;

        public static void EnsureAttached(Transform player)
        {
            if (player == null) return;

            PrototypeProgressPersistence persistence =
                player.GetComponent<PrototypeProgressPersistence>();

            if (persistence == null)
            {
                persistence = player.gameObject.AddComponent<
                    PrototypeProgressPersistence>();
            }

            persistence.Bind(player);
        }

        public static string SavePath =>
            Path.Combine(Application.persistentDataPath, SaveFileName);

        private void Bind(Transform player)
        {
            if (wallet == null)
            {
                wallet = player.GetComponentInChildren<
                    PrototypeSilverWallet>();
            }

            if (goldWallet == null)
            {
                goldWallet = player.GetComponentInChildren<
                    PrototypeGoldWallet>();
            }

            if (consumables == null)
            {
                consumables = player.GetComponentInChildren<
                    PrototypeShipConsumables>();
            }

            if (broadside == null)
            {
                broadside = player.GetComponentInChildren<
                    BroadsideController>();
            }

            if (harpoons == null)
            {
                harpoons = player.GetComponentInChildren<
                    HarpoonHuntingController>();
            }

            if (equipment == null)
            {
                equipment = player.GetComponentInChildren<
                    PrototypeShipEquipment>();
            }

            if (materials == null)
            {
                materials = player.GetComponentInChildren<
                    PrototypeRegionalLootInventory>();
            }

            if (loadout == null)
            {
                loadout = player.GetComponentInChildren<
                    ShipLoadout>();
            }

            if (inventory == null)
            {
                inventory = player.GetComponentInChildren<
                    PrototypeEquipmentInventory>();
            }

            if (fleet == null)
            {
                fleet = player.GetComponentInChildren<
                    PrototypeFleetInventory>();
            }

            if (captainProgression == null)
            {
                captainProgression =
                    player.GetComponentInChildren<
                        PrototypeCaptainProgression>();
            }

            if (captainSkills == null)
            {
                captainSkills =
                    player.GetComponentInChildren<
                        PrototypeCaptainSkills>();
            }

            if (deckExtensions == null)
            {
                deckExtensions =
                    player.GetComponentInChildren<
                        PrototypeDeckExtensionInventory>();
            }

            if (!loaded && wallet != null &&
                goldWallet != null &&
                consumables != null &&
                broadside != null && harpoons != null &&
                equipment != null && materials != null &&
                loadout != null && inventory != null &&
                fleet != null &&
                captainProgression != null &&
                captainSkills != null &&
                deckExtensions != null)
            {
                Load();
                loaded = true;
                lastSnapshot = Capture();
            }
        }

        private void Update()
        {
            if (!loaded || Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + ScanInterval;
            SaveData current = Capture();
            if (!Matches(current, lastSnapshot))
            {
                Write(current);
                lastSnapshot = current;
            }
        }

        private void Load()
        {
            try
            {
                SaveData data = File.Exists(SavePath)
                    ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath))
                    : CreateNewCaptain();
                if (data == null || data.version != CurrentVersion)
                {
                    Debug.LogWarning(
                        "Kaptan kaydı sürümü uyumsuz; varsayılan değerler kullanılıyor.",
                        this
                    );
                    return;
                }

                AmmunitionType selected =
                    Enum.IsDefined(
                        typeof(AmmunitionType),
                        data.selectedAmmunition)
                        ? (AmmunitionType)data.selectedAmmunition
                        : AmmunitionType.Standard;

                wallet.RestoreSilver(data.silver);
                goldWallet.RestoreGold(data.gold);
                consumables.RestoreStocks(
                    data.tortugaTonics,
                    data.lightsOfTortuga,
                    data.corsairRum,
                    data.galeElixirs,
                    data.ironbarkBrews
                );
                broadside.RestorePersistentState(
                    selected,
                    data.standardStock,
                    data.chainStock,
                    data.grapeshotStock
                );
                bool legacyHarpoonStock =
                    data.lightHarpoonStock == 0 &&
                    data.heavyHarpoonStock == 0 &&
                    data.harpoonStock > 0;
                if (legacyHarpoonStock)
                {
                    harpoons.RestoreHarpoonStock(
                        data.harpoonStock
                    );
                }
                else
                {
                    harpoons.RestoreHarpoonStocks(
                        data.lightHarpoonStock,
                        data.heavyHarpoonStock,
                        data.selectedHarpoonId
                    );
                }

                captainProgression.RestoreExperience(
                    data.captainExperience
                );
                captainSkills.Restore(
                    data.cannonMasteryRank,
                    data.rangefindingRank,
                    data.harpoonMasteryRank,
                    data.harpoonRiggingRank,
                    data.reinforcedHullRank,
                    data.fineSailsRank
                );
                deckExtensions.Restore(
                    data.availableDeckExtensions,
                    data.starterDeckExtensions,
                    data.ratDeckExtensions,
                    data.dreadwakeDeckExtensions
                );
                fleet.Restore(
                    data.ownsStarterSloop,
                    data.ownsRatSailsShip,
                    data.ownsDreadwake,
                    data.activeShipId
                );
                inventory.Restore(
                    data.iron6LbCannons,
                    data.iron12LbCannons,
                    data.patchedCanvasSails,
                    data.ratSails
                );
                inventory.RestoreCannonItems(data.cannonItems);
                loadout.Restore(
                    data.cannonId,
                    data.installedCannons,
                    data.sailId,
                    data.selectedHarpoonId
                );
                loadout.RestoreCannonSlots(data.cannonSlots, data.cannonItemIds);
                equipment.RestoreLevels(
                    data.hullLevel,
                    data.cannonLevel,
                    data.harpoonLevel
                );
                materials.Restore(
                    data.tideOil,
                    data.stormjawScales,
                    data.corsairIron,
                    data.lostChartFragments
                );

                Debug.Log(
                    $"Kaptan kaydı yüklendi: {data.silver} silver.",
                    this
                );
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Kaptan kaydı okunamadı: {exception.Message}",
                    this
                );
            }
        }

        private SaveData Capture()
        {
            return new SaveData
            {
                silver = wallet != null ? wallet.Silver : 0,
                gold = goldWallet != null ? goldWallet.Gold : 0,
                tortugaTonics = consumables != null
                    ? consumables.TortugaTonics
                    : 0,
                lightsOfTortuga = consumables != null
                    ? consumables.LightsOfTortuga
                    : 0,
                corsairRum = consumables != null
                    ? consumables.CorsairRum
                    : 0,
                galeElixirs = consumables != null
                    ? consumables.GaleElixirs
                    : 0,
                ironbarkBrews = consumables != null
                    ? consumables.IronbarkBrews
                    : 0,
                selectedAmmunition = broadside != null
                    ? (int)broadside.SelectedAmmunition
                    : (int)AmmunitionType.Standard,
                standardStock = GetStock(
                    AmmunitionType.Standard),
                chainStock = GetStock(
                    AmmunitionType.Chain),
                grapeshotStock = GetStock(
                    AmmunitionType.Grapeshot),
                harpoonStock = harpoons != null
                    ? harpoons.TotalHarpoonStock
                    : 0,
                lightHarpoonStock = harpoons != null
                    ? harpoons.LightHarpoonStock
                    : 0,
                heavyHarpoonStock = harpoons != null
                    ? harpoons.HeavyHarpoonStock
                    : 0,
                selectedHarpoonId = harpoons != null
                    ? harpoons.SelectedHarpoonId
                    : "light_2kg",
                cannonItems = inventory != null ? inventory.CaptureCannonItems() : null,
                cannonItemIds = loadout != null ? loadout.CaptureCannonItemIds() : null,
                cannonSlots = loadout != null ? loadout.CaptureCannonSlots() : null,
                cannonId = loadout != null
                    ? loadout.CannonId
                    : "iron_6lb",
                installedCannons = loadout != null
                    ? loadout.InstalledCannons
                    : 6,
                sailId = loadout != null
                    ? loadout.SailId
                    : "patched_canvas",
                iron6LbCannons = inventory != null
                    ? inventory.Iron6LbCannons
                    : 6,
                iron12LbCannons = inventory != null
                    ? inventory.Iron12LbCannons
                    : 0,
                patchedCanvasSails = inventory != null
                    ? inventory.PatchedCanvasSails
                    : 1,
                ratSails = inventory != null
                    ? inventory.RatSails
                    : 0,
                ownsStarterSloop = fleet == null ||
                    fleet.OwnsStarterSloop,
                ownsRatSailsShip = fleet != null &&
                    fleet.OwnsRatSails,
                ownsDreadwake = fleet != null &&
                    fleet.OwnsDreadwake,
                activeShipId = fleet != null
                    ? fleet.ActiveShipId
                    : "starter_sloop",
                availableDeckExtensions =
                    deckExtensions != null
                        ? deckExtensions.AvailableExtensions
                        : 0,
                starterDeckExtensions =
                    deckExtensions != null
                        ? deckExtensions.StarterSloopExtensions
                        : 0,
                ratDeckExtensions =
                    deckExtensions != null
                        ? deckExtensions.RatSailsExtensions
                        : 0,
                dreadwakeDeckExtensions =
                    deckExtensions != null
                        ? deckExtensions.DreadwakeExtensions
                        : 0,
                captainExperience =
                    captainProgression != null
                        ? captainProgression.TotalExperience
                        : 0,
                cannonMasteryRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.CannonMastery)
                    : 0,
                rangefindingRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.Rangefinding)
                    : 0,
                harpoonMasteryRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.HarpoonMastery)
                    : 0,
                harpoonRiggingRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.HarpoonRigging)
                    : 0,
                reinforcedHullRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.ReinforcedHull)
                    : 0,
                fineSailsRank = captainSkills != null
                    ? captainSkills.GetRank(
                        CaptainSkill.FineSails)
                    : 0,
                hullLevel = equipment != null
                    ? equipment.HullLevel
                    : 0,
                cannonLevel = equipment != null
                    ? equipment.CannonLevel
                    : 0,
                harpoonLevel = equipment != null
                    ? equipment.HarpoonLevel
                    : 0,
                tideOil = materials != null
                    ? materials.TideOil
                    : 0,
                stormjawScales = materials != null
                    ? materials.StormjawScales
                    : 0,
                corsairIron = materials != null
                    ? materials.CorsairIron
                    : 0,
                lostChartFragments = materials != null
                    ? materials.LostChartFragments
                    : 0
            };
        }

        private int GetStock(AmmunitionType ammunition)
        {
            return broadside != null
                ? broadside.GetAmmunitionStock(ammunition)
                : 0;
        }

        private void Write(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string temporaryPath = SavePath + ".tmp";
                File.WriteAllText(temporaryPath, json);

                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                File.Move(temporaryPath, SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Kaptan kaydı yazılamadı: {exception.Message}",
                    this
                );
            }
        }

        private static bool Matches(
            SaveData first,
            SaveData second)
        {
            return first != null &&
                second != null &&
                first.silver == second.silver &&
                first.gold == second.gold &&
                first.tortugaTonics ==
                    second.tortugaTonics &&
                first.lightsOfTortuga ==
                    second.lightsOfTortuga &&
                first.corsairRum == second.corsairRum &&
                first.galeElixirs == second.galeElixirs &&
                first.ironbarkBrews ==
                    second.ironbarkBrews &&
                first.selectedAmmunition ==
                    second.selectedAmmunition &&
                first.standardStock ==
                    second.standardStock &&
                first.chainStock ==
                    second.chainStock &&
                first.grapeshotStock ==
                    second.grapeshotStock &&
                first.harpoonStock ==
                    second.harpoonStock &&
                first.lightHarpoonStock ==
                    second.lightHarpoonStock &&
                first.heavyHarpoonStock ==
                    second.heavyHarpoonStock &&
                string.Equals(
                    first.selectedHarpoonId,
                    second.selectedHarpoonId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    first.cannonId,
                    second.cannonId,
                    StringComparison.Ordinal) &&
                SameSlots(first.cannonSlots, second.cannonSlots) &&
                SameSlots(first.cannonItemIds, second.cannonItemIds) &&
                SameCannonItems(first.cannonItems, second.cannonItems) &&
                first.installedCannons ==
                    second.installedCannons &&
                string.Equals(
                    first.sailId,
                    second.sailId,
                    StringComparison.Ordinal) &&
                first.iron6LbCannons ==
                    second.iron6LbCannons &&
                first.iron12LbCannons ==
                    second.iron12LbCannons &&
                first.patchedCanvasSails ==
                    second.patchedCanvasSails &&
                first.ratSails == second.ratSails &&
                first.ownsStarterSloop ==
                    second.ownsStarterSloop &&
                first.ownsRatSailsShip ==
                    second.ownsRatSailsShip &&
                first.ownsDreadwake ==
                    second.ownsDreadwake &&
                string.Equals(
                    first.activeShipId,
                    second.activeShipId,
                    StringComparison.Ordinal) &&
                first.availableDeckExtensions ==
                    second.availableDeckExtensions &&
                first.starterDeckExtensions ==
                    second.starterDeckExtensions &&
                first.ratDeckExtensions ==
                    second.ratDeckExtensions &&
                first.dreadwakeDeckExtensions ==
                    second.dreadwakeDeckExtensions &&
                first.captainExperience ==
                    second.captainExperience &&
                first.cannonMasteryRank ==
                    second.cannonMasteryRank &&
                first.rangefindingRank ==
                    second.rangefindingRank &&
                first.harpoonMasteryRank ==
                    second.harpoonMasteryRank &&
                first.harpoonRiggingRank ==
                    second.harpoonRiggingRank &&
                first.reinforcedHullRank ==
                    second.reinforcedHullRank &&
                first.fineSailsRank ==
                    second.fineSailsRank &&
                first.hullLevel == second.hullLevel &&
                first.cannonLevel == second.cannonLevel &&
                first.harpoonLevel == second.harpoonLevel &&
                first.tideOil == second.tideOil &&
                first.stormjawScales ==
                    second.stormjawScales &&
                first.corsairIron == second.corsairIron &&
                first.lostChartFragments ==
                    second.lostChartFragments;
        }

        private static bool SameCannonItems(CannonItem[] first, CannonItem[] second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (first == null || second == null || first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++)
                if (first[i] == null || second[i] == null || first[i].InstanceId != second[i].InstanceId ||
                    first[i].DefinitionId != second[i].DefinitionId || first[i].Enhancement != second[i].Enhancement) return false;
            return true;
        }

        private static bool SameSlots(string[] first, string[] second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (first == null || second == null || first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++)
                if (!string.Equals(first[i], second[i], StringComparison.Ordinal)) return false;
            return true;
        }

        private void SaveNow()
        {
            if (!loaded) return;
            SaveData current = Capture();
            Write(current);
            lastSnapshot = current;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        // One authoritative first-session profile: never inherit serialized cheat stocks.
        private static SaveData CreateNewCaptain()
        {
            return new SaveData
            {
                silver = 0,
                gold = 0,
                ownsStarterSloop = true,
                activeShipId = "starter_sloop",
                cannonId = "iron_6lb",
                installedCannons = 6,
                iron6LbCannons = 6,
                sailId = "patched_canvas",
                patchedCanvasSails = 1,
                selectedHarpoonId = "light_2kg",
                selectedAmmunition = (int)AmmunitionType.Standard,
                standardStock = 120,
                chainStock = 12,
                grapeshotStock = 16,
                lightHarpoonStock = 10,
                heavyHarpoonStock = 0,
                tortugaTonics = 1,
                lightsOfTortuga = 0,
                corsairRum = 0,
                galeElixirs = 0,
                ironbarkBrews = 0,
                availableDeckExtensions = 0
            };
        }

        // Reset on disk only when no live captain can autosave stale state over it.
        // Returns the backup path, or null when there was no previous save.
        public static string ResetSavedProgressForNewCaptain()
        {
            if (Application.isPlaying)
                throw new InvalidOperationException("Önce Play Mode'u durdur; sonra ilerlemeyi sıfırla.");

            Directory.CreateDirectory(Application.persistentDataPath);
            string temporaryPath = SavePath + ".reset.tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(CreateNewCaptain(), true));
            string backup = null;
            try
            {
                if (File.Exists(SavePath))
                {
                    backup = SavePath + ".before-reset-" +
                        DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" +
                        Guid.NewGuid().ToString("N") + ".bak";
                    File.Replace(temporaryPath, SavePath, backup);
                }
                else File.Move(temporaryPath, SavePath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
            return backup;
        }

        [ContextMenu("Reset Prototype Progress (Stop Play Mode First)")]
        private void DeletePrototypeProgress()
        {
            try
            {
                string backup = ResetSavedProgressForNewCaptain();
                Debug.Log("Yeni kaptan kaydı hazır. Liman sahnesinden Play başlat. Yedek: " +
                    (backup ?? "Önceki kayıt yok"), this);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"İlerleme sıfırlanamadı: {exception.Message}", this);
            }
        }
    }
}
