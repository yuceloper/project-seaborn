using System.Collections.Generic;
using Seaborn.Hunting;
using Seaborn.Progression;
using UnityEngine;

namespace Seaborn.Harbor.UI
{
    internal static class ShipyardUpgradeGuidance
    {
        public static string Describe(int silver, int oil, int iron, int charts, int scales,
            int ownedSilver, PrototypeRegionalLootInventory depot, PrototypeHuntCargo cargo)
        {
            var missing = new List<string>();
            int money = Mathf.Max(0, silver - ownedSilver);
            if (money > 0) missing.Add($"{money} Silver");
            string source = null;
            string delivery = null;
            Add(RegionalMaterialType.TideOil, oil, "Yağ", "Yağ: Doğu Suları'nda normal av; ganimeti limana getir.");
            Add(RegionalMaterialType.CorsairIron, iron, "Demir", "Demir: Korsan batığını E ile topla, limana getir.");
            Add(RegionalMaterialType.LostChartFragment, charts, "Harita", "Harita: Gunship batığı veya Stormjaw avı; limana getir.");
            Add(RegionalMaterialType.StormjawScale, scales, "Pul", "Pul: Stormjaw avla; ganimeti limana getir.");
            if (missing.Count == 0) return "HAZIR • Kaynaklar tamam. Geliştirebilirsin.";
            return "EKSİK: " + string.Join(" • ", missing) + "\n" +
                (delivery ?? source ?? "Silver: Sefer yükünü limana teslim et.");

            void Add(RegionalMaterialType type, int required, string name, string hint)
            {
                int deficit = Mathf.Max(0, required - (depot?.Get(type) ?? 0));
                if (deficit == 0) return;
                missing.Add($"{deficit} {name}");
                if (source == null) source = hint;
                int carried = cargo?.GetMaterial(type) ?? 0;
                if (carried > 0 && delivery == null)
                    delivery = $"Ambarda {carried} {name} var; önce limana teslim et.";
            }
        }
    }
}
