using System;
using Seaborn.Harbor;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class HarborPreparationChecks
    {
        [MenuItem("Seaborn/Validation/Check Harbor Preparation Quotes")]
        public static void Run()
        {
            var standard = new PreparationSupply(99, 120, 40, 20);
            Equal(40, standard.AddedStock, "partial packet rounds up");
            Equal(20, standard.Cost, "whole packet price");
            var empty = new PreparationSupply(0, 120, 40, 20);
            Equal(120, empty.AddedStock, "empty stock needs three packets");
            Equal(60, empty.Cost, "three packet price");
            var stocked = new PreparationSupply(139, 120, 40, 20);
            Equal(0, stocked.AddedStock, "surplus is not removed or topped up again");
            Equal(0, new PreparationSupply(120, 120, 40, 20).Cost, "exact target costs zero");
            var free = new PreparationSupply(0, 10, 10, 0);
            var chain = new PreparationSupply(9, 12, 12, 18);
            var grape = new PreparationSupply(13, 16, 16, 18);
            var light = new PreparationSupply(6, 10, 10, 15);
            var quote = new HarborPreparationQuote(13, standard, chain, grape, light, "light_2kg", "Light");
            Equal(84, quote.TotalCost, "repair plus all needed packets");
            Require(quote.HasWork, "damaged understocked ship has work");
            Require(quote.Matches(new HarborPreparationQuote(13, standard, chain, grape, light,
                "light_2kg", "Light")), "unchanged quote matches");
            Require(!quote.Matches(new HarborPreparationQuote(13, standard, chain, grape, light,
                "heavy_4kg", "Heavy")), "different selected harpoon invalidates quote");
            Require(!quote.Matches(new HarborPreparationQuote(14, standard, chain, grape, light,
                "light_2kg", "Light")), "repair change invalidates quote");
            Require(!quote.Matches(new HarborPreparationQuote(13, empty, chain, grape, light,
                "light_2kg", "Light")), "stock change invalidates quote");
            var ready = new HarborPreparationQuote(0, stocked, stocked, stocked, stocked, "light_2kg", "Light");
            Require(!ready.HasWork && ready.TotalCost == 0, "ready ship requires no purchase");
            Require(new HarborPreparationQuote(0, stocked, stocked, stocked, free,
                "light_2kg", "Light").HasWork, "free packets still represent work");
            bool rejected = false;
            try { _ = new PreparationSupply(0, 10, 0, 15); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            Require(rejected, "invalid bundle cannot divide by zero");
            Debug.Log("Harbor preparation quote checks passed. No player state or save was changed.");
        }

        private static void Equal(int expected, int actual, string scenario) => Require(expected == actual, scenario);
        private static void Require(bool valid, string scenario)
        {
            if (!valid) throw new Exception("Preparation check failed: " + scenario);
        }
    }
}
