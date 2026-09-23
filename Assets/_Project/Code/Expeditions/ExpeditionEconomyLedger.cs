using System;

namespace Seaborn.Expeditions
{
    // No Unity dependency: accounting is independent of UI and scene lifetime.
    public sealed class ExpeditionEconomyLedger
    {
        public int SilverEarned { get; private set; }
        public int SilverSecured { get; private set; }
        public int SilverLost { get; private set; }
        public decimal AmmunitionValue { get; private set; }
        public int StandardUsed { get; private set; }
        public int ChainUsed { get; private set; }
        public int GrapeshotUsed { get; private set; }
        public int HarpoonsUsed { get; private set; }

        public void Earn(int amount) => SilverEarned += Math.Max(0, amount);
        public void Secure(int amount) => SilverSecured += Math.Max(0, amount);
        public void Lose(int amount) => SilverLost += Math.Max(0, amount);

        // kind: 0 standard, 1 chain, 2 grapeshot, 3 harpoon.
        public void Consume(int kind, int count, int bundleCost, int bundleSize)
        {
            if (kind < 0 || kind > 3 || count <= 0) return;
            if (bundleSize <= 0 || bundleCost < 0)
                throw new ArgumentOutOfRangeException(nameof(bundleSize));
            AmmunitionValue += (decimal)count * bundleCost / bundleSize;
            switch (kind)
            {
                case 0: StandardUsed += count; break;
                case 1: ChainUsed += count; break;
                case 2: GrapeshotUsed += count; break;
                case 3: HarpoonsUsed += count; break;
            }
        }

        public decimal EstimatedNet(int departureRepairQuote, int returnRepairQuote)
        {
            return SilverEarned - AmmunitionValue -
                Math.Max(0, returnRepairQuote - departureRepairQuote);
        }
    }
}
