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
            public int selectedAmmunition;
            public int standardStock;
            public int chainStock;
            public int grapeshotStock;
            public int harpoonStock;
            public int lightHarpoonStock;
            public int heavyHarpoonStock;
            public string selectedHarpoonId;
            public string cannonId;
            public int installedCannons;
            public string sailId;
            public int iron6LbCannons;
            public int iron12LbCannons;
            public int patchedCanvasSails;
            public int ratSails;
            public int hullLevel;
            public int cannonLevel;
            public int harpoonLevel;
            public int tideOil;
            public int stormjawScales;
            public int corsairIron;
            public int lostChartFragments;
        }

        private PrototypeSilverWallet wallet;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private PrototypeShipEquipment equipment;
        private PrototypeRegionalLootInventory materials;
        private ShipLoadout loadout;
        private PrototypeEquipmentInventory inventory;
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

        private string SavePath =>
            Path.Combine(Application.persistentDataPath, SaveFileName);

        private void Bind(Transform player)
        {
            if (wallet == null)
            {
                wallet = player.GetComponentInChildren<
                    PrototypeSilverWallet>();
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

            if (!loaded && wallet != null &&
                broadside != null && harpoons != null &&
                equipment != null && materials != null &&
                loadout != null && inventory != null)
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
            if (!File.Exists(SavePath))
            {
                Debug.Log(
                    "İlk kaptan kaydı hazırlanacak.",
                    this
                );
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
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

                inventory.Restore(
                    data.iron6LbCannons,
                    data.iron12LbCannons,
                    data.patchedCanvasSails,
                    data.ratSails
                );
                loadout.Restore(
                    data.cannonId,
                    data.installedCannons,
                    data.sailId,
                    data.selectedHarpoonId
                );
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

        [ContextMenu("Delete Prototype Progress")]
        private void DeletePrototypeProgress()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                Debug.Log(
                    "Prototip kaptan kaydı silindi.",
                    this
                );
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Kaptan kaydı silinemedi: {exception.Message}",
                    this
                );
            }
        }
    }
}
