# Modular progression — playable first slice

Goal: outings earn specific ship parts. A captain chooses a build and works toward it,
with secured materials and persistent equipment ownership. Story quests remain separate.

## Implemented: individual cannon hardpoints

- Shipyard DONANIM now opens a close-up with selectable port/starboard mounts. Select a mount,
  inspect individual owned guns and their + levels, then fit, remove or enhance one item.
- Ownership includes mounted guns. Removal releases that gun to the depot, with no sale,
  destruction or duplicate grant. Buying/forging adds one spare; mounting is a separate action.
- Mixed batteries are real combat loadouts. Every occupied slot fires from its own muzzle
  using its gun's damage and range, subject to the existing hull range limit and modifiers.
  Empty slots neither fire nor consume ammunition. NPC uniform batteries remain supported.
- Port and starboard still reload as separate batteries. A battery takes the longest base
  reload of its mounted guns; the other side is unaffected. All existing reload modifiers apply.
- 6 lb: 85 base damage / 4.8 s. 12 lb: 125 / 6.2 s. These are different firing profiles,
  not an unconditional DPS upgrade: a mixed battery can gain burst damage but lose sustained
  damage because of shared reload. The shipyard explicitly displays that tradeoff.
- Save data stores cloned item records and an ordered item-ID layout. Legacy count/type saves
  migrate to unique +0 items; invalid/unowned excess entries are rejected. Empty batteries survive save/reload.
- Current equipment follows the active ship. Moving to a smaller hull releases excess guns
  to the depot. Larger hulls/extensions add empty slots, never free cannons. This is NOT yet
  a separate saved build for every owned ship.
- Runtime muzzle rails expand with actual slot capacity, including Extended Deck.

## First repeatable crafting target

12 lb cannon has two acquisition paths:

| Path | Cost | Result |
|---|---|---|
| Purchase | Existing 900 Silver | One spare 12 lb cannon |
| Forge | 360 Silver + 4 secured Corsair Iron | One spare 12 lb cannon |

Iron must be carried home from pirate wrecks. The recipe uses the secured depot only.
Insufficient resources produce no item and charge nothing. Crafting is guaranteed;
there is no failure roll or destruction. Existing bought equipment and legacy upgrades remain.
These initial recipe numbers are tuning values, not validated pacing measurements.

## Subsequent ship design stages

1. Camera close-up, world-anchored mount selection, individual inventory, comparison and
   +0–+10 item enhancement are now implemented; see SHIPYARD_INSPECTION_AND_ENHANCEMENT.md.
   Replacing the physical cannon mesh with a ghost/item preview is still a later art step.
2. Fore/mid/aft sections with repeatable midship modules: gun deck, cargo hold, reinforced
   keel and hunting/crew deck. Fit benefits against mass, acceleration, turning and target size.
   Existing production models are monolithic: sectional assets/connectors are needed for
   genuine hull-length changes. Do not substitute stretched meshes or invisible bonuses.
3. Mast slots and sail plans, cloth/rigging quality; meaningful speed, response and sail durability.
4. A broader crafting catalogue and item progression after the first build is playtested.
   Do not erase legacy investments or impose item-loss RNG as a substitute for content.

## Validation

Added `Seaborn > Validation > Check Modular Cannons` (temporary objects, no save writes):
legacy migration, mixed ownership, empty slots, distinct side reloads, per-muzzle definitions,
cloned snapshots, profile refresh, invalid/unowned items, full removal and extension muzzles.
This menu requires Unity and has NOT been run in the editing environment.

Play Mode acceptance:
- Mount one 12 lb on port: only that muzzle deals 125 base damage; starboard remains 6 lb.
- Remove a middle gun: only the other two fire; ammo debit equals fired guns.
- Save/restart with mixed guns, a gap and then an entirely empty ship; depot totals stay correct.
- Change hulls, add a deck extension, leave/re-enter harbor: slot order, stock and actual shots agree.
- Forge with 359/360 Silver and 3/4 iron: only the fully funded case succeeds; exactly one item is added.
- Inspect UI at 16:9 and a smaller window; scroll all hardpoints on larger hulls.
- Test shared-reload burst vs sustained damage before adjusting cannon stats or recipe pacing.

Local checks here: C# syntax parsing and source integration review only. No Unity build or
live combat/UI test was available.
