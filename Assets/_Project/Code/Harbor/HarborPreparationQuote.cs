using System;

namespace Seaborn.Harbor
{
    public readonly struct PreparationSupply
    {
        public readonly int AddedStock;
        public readonly int Cost;

        public PreparationSupply(int stock, int target, int bundleSize, int bundlePrice)
        {
            if (stock < 0 || target < 0 || bundleSize <= 0 || bundlePrice < 0)
                throw new ArgumentOutOfRangeException(nameof(bundleSize));
            long missing = Math.Max(0L, (long)target - stock);
            long bundles = (missing + bundleSize - 1) / bundleSize;
            AddedStock = checked((int)(bundles * bundleSize));
            Cost = checked((int)(bundles * bundlePrice));
        }
    }

    public sealed class HarborPreparationQuote
    {
        public const int StandardTarget = 120;
        public const int ChainTarget = 12;
        public const int GrapeshotTarget = 16;
        public const int HarpoonTarget = 10;
        public readonly int RepairCost;
        public readonly PreparationSupply Standard, Chain, Grapeshot, Harpoon;
        public readonly string HarpoonId, HarpoonName;
        public int TotalCost => checked(RepairCost + Standard.Cost + Chain.Cost + Grapeshot.Cost + Harpoon.Cost);
        public bool HasWork => RepairCost > 0 || Standard.AddedStock > 0 || Chain.AddedStock > 0 ||
            Grapeshot.AddedStock > 0 || Harpoon.AddedStock > 0;

        public HarborPreparationQuote(int repairCost, PreparationSupply standard,
            PreparationSupply chain, PreparationSupply grapeshot, PreparationSupply harpoon,
            string harpoonId, string harpoonName)
        {
            RepairCost = repairCost;
            Standard = standard;
            Chain = chain;
            Grapeshot = grapeshot;
            Harpoon = harpoon;
            HarpoonId = harpoonId;
            HarpoonName = harpoonName;
        }

        public bool Matches(HarborPreparationQuote other) => other != null &&
            RepairCost == other.RepairCost && HarpoonId == other.HarpoonId &&
            Same(Standard, other.Standard) && Same(Chain, other.Chain) &&
            Same(Grapeshot, other.Grapeshot) && Same(Harpoon, other.Harpoon);

        private static bool Same(PreparationSupply a, PreparationSupply b) =>
            a.AddedStock == b.AddedStock && a.Cost == b.Cost;
    }
}
