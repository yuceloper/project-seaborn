using System;
using Seaborn.Expeditions;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class ExpeditionEconomyChecks
    {
        [MenuItem("Seaborn/Validation/Check Expedition Accounting")]
        public static void Run()
        {
            var empty = new ExpeditionEconomyLedger();
            Equal(0m, empty.EstimatedNet(0, 0), "empty return");
            var trip = new ExpeditionEconomyLedger();
            trip.Earn(100);
            trip.Secure(100); // Classification only; no second credit.
            trip.Earn(25);
            trip.Consume(0, 6, 20, 40);
            trip.Consume(0, 2, 20, 40); // Partial broadside.
            trip.Consume(1, 3, 18, 12);
            trip.Consume(2, 4, 18, 16); // Four shells, not twelve pellets.
            trip.Consume(3, 1, 15, 10);
            trip.Consume(3, 1, 60, 10); // Price follows selected harpoon.
            Equal(125m, trip.SilverEarned, "credit counted once");
            Equal(100m, trip.SilverSecured, "cargo classification");
            Equal(8m, trip.StandardUsed, "partial salvo");
            Equal(4m, trip.GrapeshotUsed, "shells versus pellets");
            Equal(20.5m, trip.AmmunitionValue, "per-unit replacement value");
            Equal(74.5m, trip.EstimatedNet(50, 80), "pre-existing damage excluded");
            Equal(104.5m, trip.EstimatedNet(80, 50), "repair cannot create income");
            var sunk = new ExpeditionEconomyLedger();
            sunk.Consume(0, 6, 20, 40);
            sunk.Lose(90);
            Equal(90m, sunk.SilverLost, "lost cargo classification");
            Equal(-3m, sunk.EstimatedNet(0, 0), "lost cargo not debited twice");
            sunk.Earn(-10);
            sunk.Consume(0, 0, 20, 40);
            Equal(-3m, sunk.EstimatedNet(0, 0), "invalid amounts ignored");
            var fractional = new ExpeditionEconomyLedger();
            for (int i = 0; i < 8; i++) fractional.Consume(2, 1, 18, 16);
            Equal(9m, fractional.AmmunitionValue, "no per-shot rounding drift");
            bool rejected = false;
            try { fractional.Consume(0, 1, 20, 0); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            if (!rejected) throw new Exception("Zero bundle size was accepted.");
            Debug.Log("Seaborn expedition accounting: all checks passed.");
        }

        private static void Equal(decimal expected, decimal actual, string scenario)
        {
            if (expected != actual)
                throw new Exception($"{scenario}: expected {expected}, got {actual}");
        }
    }
}
