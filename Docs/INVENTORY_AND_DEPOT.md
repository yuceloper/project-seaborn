# Inventory and harbor depot, first version

Open **AMBAR / DEPO** at the shipyard or trade station. The panel has three
scrollable views, with item icons, amounts and descriptions. At sea or while
undocked, press **I** to inspect the hold and fitted equipment. The depot tab is
locked until docked. **I**, **Esc**, or **KAPAT** closes the window. At a dock,
I opens the station's inventory tab; closing returns to the station's default tab.

The three views are:

- **Gemi ambarı:** unsecured sale cargo and all four material types, then the
  existing ship ammunition and consumable stocks. Material rows can deposit one
  item, and the bottom button delivers all loot. Transfers require a docked harbor
  station; sea loot cannot be banked through these controls.
- **Liman deposu:** secured materials and spare cannons/sails. Fitted equipment
  is subtracted from owned totals; this is a view of existing ownership, not a
  second equipment inventory. Use the shipyard's DONANIM tab to fit spares.
- **Takılı donanım:** the actual current cannon set, sail and selected harpoon.

Returning to the harbor delivery zone still deposits automatically. Silver sale
cargo goes to the wallet; material stacks go to the depot. The shipyard reads
and spends those same depot balances. This version supports hold-to-depot
deposits; materials do not need to be withdrawn for crafting. Sea HUD continues
to show capacity and unsecured materials, plus the I shortcut.

## Ownership and loss

Existing saved regional-material fields remain authoritative secured balances.
No reset or save migration is needed. Existing owned equipment, loadout and
ammunition/supply saves remain unchanged. Secured items survive sinking.

All four material types now travel in cargo until delivery, including oil,
Stormjaw scales and hunt chart fragments (previously credited immediately).
Shipwreck rolls are unchanged. The unused old direct shipwreck material award
method was removed. Sinking clears all carried materials. Unsecured cargo is
still session-only, like the existing silver cargo; quitting does not bank it.
Leviathan deck-extension unlocks remain progression rewards, not material cargo.

Capacity now counts sale value plus one unit per material. Wreck collection is
all-or-nothing including its materials. Hunt materials are offered before the
sale cargo so valuable drops get first use of remaining capacity; drops that do
not fit are rejected with feedback. Ammunition/supplies use their existing
separate stocks and are not charged to this loot capacity. Their existing loss
rules are unchanged. Capacity reductions never delete existing contents.

## Validation

Run **Seaborn > Validation > Check Inventory And Depot** outside Play Mode.
It uses temporary unsaved objects and the actual cargo/depot methods to check
full-wreck rejection, exact capacity, material-only delivery, repeated delivery,
civilian filtering, all-material sinking, preserved banked balances, bad input,
and capacity reduction without deletion. It never reads or writes player saves.

Also playtest:

1. Open an existing save: depot quantities match previous secured balances.
2. Collect a pirate wreck and hunt materials: only the hold/HUD changes at sea.
3. Return to harbor: hold empties, depot and expedition report show the gains;
   entering the delivery area again gives no second credit.
4. Sink before delivery: carried items disappear, previous depot remains intact.
5. Buy/equip another cannon or sail: depot displays only the unfitted remainder;
   equipped view follows the active loadout. Switching back returns the old set
   to the spare count. Reopen the game to verify these existing saves persist.
6. Upgrade at the shipyard: the depot count decreases by the recipe exactly once.
7. Inspect all three views and scroll to supplies at normal and narrow aspect
   ratios. Verify other station panels disappear when inventory is selected.

The implementation received static syntax/API/source review here. Unity compile,
the editor validation command and rendered layout still require a Unity run.

Next: playtest storage clarity and capacity pressure before adding warehouse
expansion, item-specific artwork, withdrawals or ammunition loss rebalance.

## Sea inventory interaction checks

- Opening the window does not pause time, enemy AI, ship movement, reload timers
  or active consumable expiry. The footer explains that the sea stays live.
- Cannon/harpoon aim is cancelled while open; ammo selection and consumable
  hotkeys are ignored. Scrolling the list does not zoom the camera, whose normal
  follow motion continues. Close-frame input stays blocked and held mouse buttons
  must be released before weapon input resumes.
- The persistent singleton is reused by harbor UI and closes on scene changes or
  sinking. If a sea scene lacks a UI event system, a scene-owned one is created
  when the panel opens. Harbor loadout/cargo bindings refresh after transitions.

Manual Unity checks (not run in the tool environment):

1. Start directly in the ocean, press I, scroll all the way to supplies and switch
   to fitted equipment. Depot is disabled; neither transfer path is usable.
2. Press I together with fire/Shift-fire: no projectile or ammunition use. While
   open try both mouse buttons, Shift+1/2 and 1–8: no weapon/consumable action.
3. Keep right/left mouse held while closing with I/Esc; release, then press again
   to re-aim/fire normally. Click KAPAT while aiming: the close click cannot fire.
4. Scroll over the list: camera distance stays fixed; moving ship remains followed.
5. Dock at trade/shipyard: open the same inventory via tab or I, browse the depot,
   then close. Confirm the normal station tab returns without duplicate panels.
6. Travel harbor → ocean → harbor repeatedly, including with the window open;
   it closes on each scene transition and can be reopened for the current ship.
7. Sink with the window open: it closes and does not remain over the recovery UI.
