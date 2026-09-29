# Individual sail and hull modules

## Player flow

The shipyard now has three equipment categories: cannon hardpoints, one sail slot and one
hull-plating slot. Click YELKEN or GÖVDE over the inspection ship, or use YELKEN / GÖVDE in
the card. Choose an owned item, compare its multipliers and fit/remove it. Each slot has its
own inventory and supply page. +0–+10 enhancement belongs to that item, survives removal,
ship changes and saves, and does not upgrade other copies. No rudder or harpoon slots added.

Production ship meshes are unchanged: these are selectable functional equipment slots, not
new sail cloth meshes or sectional hull assets. Mast-by-mast sail plans and physical fore/mid/aft
construction remain future art/system work.

## Initial catalogue (+0)

Multipliers apply to the hull's base performance before skills, damage and consumable effects.

| Sail | Speed | Acceleration | Turning | Sail durability | Purchase |
|---|---:|---:|---:|---:|---|
| Patched Canvas | 1.00 | 1.00 | 1.00 | 1.00 | 180 Silver |
| Rat Sails | 1.08 | 1.08 | 1.04 | 1.00 | 1800 Silver |
| Swift Linen | 1.02 | 1.16 | 1.02 | 0.90 | 1000 Silver + 3 Tide Oil |
| Storm Canvas | 0.96 | 0.96 | 1.00 | 1.35 | 1400 Silver + 5 Tide Oil |

| Plating | Maximum hull HP | Speed | Acceleration | Turning | Purchase |
|---|---:|---:|---:|---:|---|
| Timber | 1.00 | 1.00 | 1.00 | 1.00 | 450 Silver |
| Light | 0.95 | 1.04 | 1.08 | 1.06 | 1100 Silver + 4 Corsair Iron |
| Reinforced | 1.20 | 0.94 | 0.90 | 0.94 | 1500 Silver + 8 Corsair Iron |

An empty sail slot gives speed 0.45, acceleration 0.50, turning 0.75, durability 0.70: the
ship can still return to port. Empty hull plating is neutral (all multipliers 1).

## Enhancement

Guaranteed success; no breakage/failure roll. Buying always grants a new +0 item.
At level `n`, sail speed is its base × (1 + .01n), acceleration × (1 + .015n), and durability
× (1 + .03n). Turning is unchanged. Plating maximum HP is its base × (1 + .03n); its movement
tradeoffs are unchanged. These are linear bonuses from each definition's base, not compounded.

Cost to reach destination level `n`:
- Sail: 45 × n² Silver and n Tide Oil.
- Hull: 55 × n² Silver and (n + 1) Corsair Iron.
- Both: max(0, n − 5) Lost Chart Fragments and max(0, n − 8) Stormjaw Scales.

Only secured depot materials are used. The entire material recipe is validated before debit;
insufficient funds/materials or stale level quotes leave the item unchanged. +10 cannot be
charged again. Prices and progression rates are initial tuning, not playtest-validated balance.

## Actual effects and persistence

ShipLoadout composes ship definition × sail × plating movement. Plating HP uses a separate
ShipHealth multiplier, preserving old global equipment/skill bonuses. Applying, fitting or
upgrading plating NEVER heals: current HP is capped if necessary, not increased. Removing and
replacing a piece therefore cannot generate health. Increased capacity can be repaired normally.
Sail durability divides chain-shot integrity damage; fitting/upgrading does not restore integrity.

Old save counts migrate to unique +0 sail items, preserving all copies and the selected sail.
One neutral Timber +0 plating is introduced for both migrated and new captains, leaving old HP
unchanged. Existing general hull upgrades remain intact. New saves store cloned module records,
stable fitted item IDs, and an explicit layout flag so deliberately empty slots stay empty.
Unknown definitions, duplicate IDs and wrong-slot assignments are rejected; levels clamp to 0–10.
The active loadout follows the selected ship; separate per-hull loadout presets are not added.

## Validation

Local: C# syntax parsing and integration review only. Unity compilation and Play Mode remain
unavailable in the editing environment. Editor check added (not executed here):
`Seaborn > Validation > Check Sail And Hull Modules`.

The check covers legacy counts/selected sail, neutral migration, independent levels, movement
composition, HP without healing/compounding, chain durability, no integrity repair on swap,
JSON roundtrip, wrong-category IDs, empty slots, clamping and atomic material debit. It only
uses temporary inactive objects, not the player's save file.

Play Mode acceptance:
1. Existing save: all cannon levels remain, sail count/selection retained, Timber +0 fitted,
   previous hull bonuses unchanged. Fresh reset starts with Patched Canvas +0 and Timber +0.
2. Click each world slot and each inventory/supply tab; verify the right category, costs,
   stock, comparison and fitted status at 16:9 and a smaller window.
3. Buy two equal sails. Upgrade only one, remove/refit it, change ship and restart; identities,
   levels and depot totals must remain distinct/correct.
4. Damaged hull: fit/upgrade/remove/reapply each plating. Check maximum HP and repair quote,
   but never gain free current HP. Repeat with existing skill/global hull upgrades.
5. Compare acceleration/speed and handling with Light vs Reinforced. Compare chain-shot sail
   integrity damage with Patched vs Storm. No integrity reset from changing equipment.
6. Test exact/insufficient costs, stale quotes, repeated clicks, +10 and both empty slots.
7. Re-run cannon checks and verify cannon damage, camera restore, inventory and harbor input.
