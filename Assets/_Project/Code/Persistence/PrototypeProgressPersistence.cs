using System;
using System.IO;
using Seaborn.Combat;
using Seaborn.Hunting;
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
        }

        private PrototypeSilverWallet wallet;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
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

            if (!loaded && wallet != null &&
                broadside != null && harpoons != null)
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
                harpoons.RestoreHarpoonStock(
                    data.harpoonStock
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
                    ? harpoons.HarpoonStock
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
                    second.harpoonStock;
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
