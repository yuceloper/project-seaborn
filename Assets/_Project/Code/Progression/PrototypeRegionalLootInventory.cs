using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Seaborn.Progression
{
    public enum RegionalMaterialType
    {
        TideOil,
        StormjawScale,
        CorsairIron,
        LostChartFragment
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeRegionalLootInventory :
        MonoBehaviour
    {
        public event Action MaterialsChanged;

        public int TideOil { get; private set; }
        public int StormjawScales { get; private set; }
        public int CorsairIron { get; private set; }
        public int LostChartFragments { get; private set; }

        private string lastDropMessage;
        private float lastDropExpiresAt;

        public static PrototypeRegionalLootInventory
            EnsureAttached(Transform player)
        {
            if (player == null)
            {
                return null;
            }

            PrototypeRegionalLootInventory inventory =
                player.GetComponent<
                    PrototypeRegionalLootInventory>();
            if (inventory == null)
            {
                inventory = player.gameObject.AddComponent<
                    PrototypeRegionalLootInventory>();
            }
            return inventory;
        }

        public int Get(RegionalMaterialType type)
        {
            return type switch
            {
                RegionalMaterialType.TideOil =>
                    TideOil,
                RegionalMaterialType.StormjawScale =>
                    StormjawScales,
                RegionalMaterialType.CorsairIron =>
                    CorsairIron,
                RegionalMaterialType.LostChartFragment =>
                    LostChartFragments,
                _ => 0
            };
        }

        public void Add(
            RegionalMaterialType type,
            int amount,
            string source)
        {
            if (amount <= 0)
            {
                return;
            }

            switch (type)
            {
                case RegionalMaterialType.TideOil:
                    TideOil += amount;
                    break;
                case RegionalMaterialType.StormjawScale:
                    StormjawScales += amount;
                    break;
                case RegionalMaterialType.CorsairIron:
                    CorsairIron += amount;
                    break;
                case RegionalMaterialType.LostChartFragment:
                    LostChartFragments += amount;
                    break;
            }

            lastDropMessage =
                $"{source}: +{amount} {DisplayName(type)}";
            lastDropExpiresAt =
                Time.unscaledTime + 3.5f;
            MaterialsChanged?.Invoke();
            Debug.Log(lastDropMessage, this);
        }

        public void AwardHunt(string creatureName)
        {
            string scene =
                SceneManager.GetActiveScene().name;
            bool stormjaw =
                creatureName.Contains(
                    "Stormjaw",
                    StringComparison.OrdinalIgnoreCase);

            if (stormjaw)
            {
                Add(
                    RegionalMaterialType.StormjawScale,
                    2,
                    "Nadir av ganimeti"
                );
                Add(
                    RegionalMaterialType
                        .LostChartFragment,
                    1,
                    "Stormjaw hazinesi"
                );
                return;
            }

            float oilChance =
                scene == "PrototypeEasternReach"
                    ? 1f
                    : scene == "PrototypeOcean"
                        ? 0.45f
                        : 0.2f;
            if (UnityEngine.Random.value <= oilChance)
            {
                Add(
                    RegionalMaterialType.TideOil,
                    1,
                    "Av ganimeti"
                );
            }

            if (scene == "PrototypeEasternReach" &&
                UnityEngine.Random.value <= 0.14f)
            {
                Add(
                    RegionalMaterialType
                        .LostChartFragment,
                    1,
                    "Nadir av ganimeti"
                );
            }
        }

        public void AwardShipwreck(
            string sceneName)
        {
            if (sceneName ==
                "PrototypeWesternReach")
            {
                Add(
                    RegionalMaterialType.CorsairIron,
                    1,
                    "Batı Sınırı enkazı"
                );
                if (UnityEngine.Random.value <= 0.2f)
                {
                    Add(
                        RegionalMaterialType
                            .LostChartFragment,
                        1,
                        "Nadir korsan ganimeti"
                    );
                }
                return;
            }

            if (sceneName == "PrototypeOcean" &&
                UnityEngine.Random.value <= 0.3f)
            {
                Add(
                    RegionalMaterialType.CorsairIron,
                    1,
                    "Merkez Sular enkazı"
                );
            }
        }

        public bool CanAfford(
            RegionalMaterialType type,
            int amount)
        {
            return amount <= 0 || Get(type) >= amount;
        }

        public bool TrySpend(
            RegionalMaterialType type,
            int amount)
        {
            if (!CanAfford(type, amount))
            {
                return false;
            }
            if (amount <= 0)
            {
                return true;
            }

            switch (type)
            {
                case RegionalMaterialType.TideOil:
                    TideOil -= amount;
                    break;
                case RegionalMaterialType.StormjawScale:
                    StormjawScales -= amount;
                    break;
                case RegionalMaterialType.CorsairIron:
                    CorsairIron -= amount;
                    break;
                case RegionalMaterialType.LostChartFragment:
                    LostChartFragments -= amount;
                    break;
            }

            MaterialsChanged?.Invoke();
            return true;
        }

        public void Restore(
            int tideOil,
            int stormjawScales,
            int corsairIron,
            int chartFragments)
        {
            TideOil = Mathf.Max(0, tideOil);
            StormjawScales =
                Mathf.Max(0, stormjawScales);
            CorsairIron =
                Mathf.Max(0, corsairIron);
            LostChartFragments =
                Mathf.Max(0, chartFragments);
            MaterialsChanged?.Invoke();
        }

        public static string DisplayName(
            RegionalMaterialType type)
        {
            return type switch
            {
                RegionalMaterialType.TideOil =>
                    "Tide Yağı",
                RegionalMaterialType.StormjawScale =>
                    "Stormjaw Pulu",
                RegionalMaterialType.CorsairIron =>
                    "Korsan Demiri",
                RegionalMaterialType.LostChartFragment =>
                    "Kayıp Harita Parçası",
                _ => "Malzeme"
            };
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(lastDropMessage) ||
                Time.unscaledTime >= lastDropExpiresAt)
            {
                return;
            }

            const float width = 430f;
            Rect rect = new(
                (Screen.width - width) * 0.5f,
                86f,
                width,
                38f
            );
            Color previous = GUI.color;
            GUI.color =
                new Color(0.025f, 0.075f, 0.105f, 0.94f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = previous;

            GUIStyle style =
                new(GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 14
                };
            style.normal.textColor =
                new Color(0.91f, 0.78f, 0.46f);
            GUI.Label(rect, lastDropMessage, style);
        }
    }
}
