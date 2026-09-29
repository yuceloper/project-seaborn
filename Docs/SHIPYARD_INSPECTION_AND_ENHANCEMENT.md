# Shipyard inspection and individual cannon enhancements

## Player flow

Dock at the shipyard with E. DONANIM opens by default. The tactical camera blends toward
an elevated close-up over 0.85 seconds, framing the ship to the left of the inventory card.
The inspection distance accounts for the current model's renderer bounds. Other harbor
tabs and undocking blend back to the existing sailing view; the user's zoom target is preserved.

Port/starboard handles are connected to the actual runtime muzzle transforms. Labels are
spaced into two rails for readability; their leader endpoints follow the ship's visual motion.
Select a handle, then an individual cannon in the right-hand list, inspect the damage comparison,
and press TAK. The list includes spares and the current slot's cannon; cannons fitted elsewhere
must be removed first. There is no duplicate mounting. SÖK returns the same item to the depot.
TEDARİK / YELKEN retains cannon purchases, heavy-cannon forging and sail equipment.

The full-screen harbor veil is transparent only in the loadout view. Weapon and consumable
input is blocked while harbor UI is open. The ship remains docked under the existing docking controller.

## Item ownership and saves

`CannonItem` has a stable instance ID, catalogue definition ID and enhancement level 0–10.
ShipLoadout stores item IDs in ordered port/starboard hardpoints. Enhancement belongs to the
item: moving it, removing it, changing hulls, buying another cannon or leaving the scene must
not reset or duplicate that level. Spare and fitted inventory views show each cannon's level.

SaveData adds `cannonItems` and `cannonItemIds`, keeping version 1 and legacy count/type fields.
Legacy counts become the same number of unique +0 items; the prior mixed type layout is mapped
to those items. Current saves restore item ownership first, then fitted IDs. Unknown/duplicate
IDs are rejected and saved enhancement values are clamped to 0–10. Snapshot arrays and items are
copied so autosave detects upgrades even if the slot layout does not change. Existing captain,
wallet, material, ship and global-upgrade progression is retained.

Current-hull equipment still follows the active ship. Excess slots on a smaller hull return to
the shared depot; separate per-hull presets are not introduced here.

## Initial +0–+10 tuning

Each + level adds 8% of the cannon's catalogue base damage (linear, not compounded).
Examples: 6 lb +0 = 85, +3 = 105.4, +10 = 153. 12 lb +10 = 225.
Only that cannon's projectile gets the multiplier. Ammo, crew, consumable, skill and legacy
ship-wide modifiers still apply afterwards. Reload and range are unchanged by enhancement.
The UI compares base enhanced damage, before those global modifiers.

For destination level `n` (1 to 10):

- Silver: `40 × n²` for 6 lb, `60 × n²` for 12 lb.
- Corsair Iron: `n`.
- Lost Chart Fragments: `max(0, n − 5)`.
- Stormjaw Scales: `max(0, n − 8)`.

Upgrade is guaranteed, with no destruction or failure roll. Only secured depot materials can
be spent. All material balances are checked/debited as one recipe. Insufficient resources or
an outdated selected level produce no upgrade; +10 cannot be charged again. Upgrade re-entry
is guarded. Buying/forging creates a separate +0 item rather than copying an existing level.

These are initial grind costs, not measured final pacing. Rare resources gate +6 onward and
boss scales gate +9/+10. The prior shared broadside reload tradeoff between 6 lb and 12 lb remains.

## Verification

Local environment: C# syntax parsing and source integration review only. Unity compilation,
validation menu execution and live camera/UI/combat checks are still pending.

Unity editor checks:
- `Seaborn > Validation > Check Modular Cannons`
- `Seaborn > Validation > Check Cannon Enhancements`

The new check uses temporary objects without touching the save file. It covers legacy item
migration, unique IDs, independent levels, muzzle damage, JSON roundtrip, moving items,
duplicate/missing IDs, enhancement clamping, depot identity and atomic material recipes.

Play Mode checks:
1. Dock/undock and switch tabs during/after the animation. Confirm old zoom is restored and no
   camera fight, stale veil or input capture remains. Inspect different hull sizes and 16:9/4:3 windows.
2. Click port and starboard handles, including extension slots. Check every leader ends at the
   right physical muzzle and does not overlap the inventory card or another clickable handle.
3. Equip two 6 lb cannons. Upgrade only one; compare shots and the untouched cannon.
4. Remove the upgraded item, refit on the other side, switch to a smaller hull, and reload the
   save. Verify identity, level, ownership and depot totals throughout.
5. Check insufficient Silver, one missing material, exact funds, rapid repeat clicks, stale
   quotes and +10. A failed request must not consume resources or create/change equipment.
6. Verify purchases and forging grant +0 items; existing material/ship upgrades still work.
7. While the panel is open, mouse clicks and consumable hotkeys must not fire/use equipment.

Sail and hull equipment now use the same inspection screen. See SAIL_AND_HULL_MODULES.md.
