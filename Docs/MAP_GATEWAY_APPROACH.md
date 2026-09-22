# Fog, countdown and confirmed map travel

The previous automatic edge travel is replaced by an approach flow for every
existing route: harbor ↔ central, central ↔ west/east, and west/east → harbor.
Existing captain-level locks, entry positions, scene loading and fade remain.

- Within 24 world units of a valid exit, a gently animated screen-space mist
  fades in with proximity. It does not modify scene fog or regional lighting.
  A top-center card names the destination.
- Within 6 units, an unlocked route counts down 10 scaled gameplay seconds.
  Moving out of that band resets the timer. Moving to another exit resets it too.
- At zero, HARİTAYA GEÇ becomes available. No scene load occurs without clicking
  it. Player location, health, destination and map unlock are rechecked at click.
- VAZGEÇ dismisses the card and cancels the countdown until
  leaving the fog approach or approaching a different exit.
- Locked routes show the required captain level and cannot start the timer.
- Movement/combat remain live. Player position is bounded at ±82 on X/Z while
  awaiting confirmation; only outward velocity is removed, so turning/reversing
  back into the map stays possible.
- Clicking the card cannot fire cannons/harpoons through it. The confirmation
  remains a mouse/UI action, avoiding conflicts with the E salvage/docking key.
- Scene changes, missing player and sinking clear the pending countdown/card.
  Fog fades away; persistent UI does not accumulate on successive trips.

## Unity playtest

1. Harbor north: fog appears beyond Z=58, timer starts at Z=76. Stay for ten
   seconds: confirm unlocks but scene remains unchanged until clicked.
2. Return to Z<76 before or after zero: timer resets. Approach again for a new
   full countdown. Cancel, leave Z<58, then approach to re-arm.
3. Repeat central south, west and east; repeat west eastward and east westward
   return routes. Arrival at X=±68 or Z=-70 must not start another countdown.
4. Test a locked region: required level appears, no timer or confirm. Test a
   level becoming unlocked during approach and one revoked before confirmation.
5. Try each map corner: closest valid route wins, south wins exact ties; moving
   between destination bands restarts the countdown.
6. Drive outward during waiting: no auto-travel and no leaving map bounds.
   Reverse/turn away to cancel. Sink while waiting: no post-death transition.
7. Click confirm/cancel while armed or holding Shift: no cannon/harpoon fired by
   that click. Pause gameplay: countdown stops. Resume without an instant jump.
8. Check mist readability and top-card layout at normal game resolutions; test
   multiple round trips for duplicate canvases/event systems.

Static syntax/source review is performed outside Unity. Actual rendering,
physics, level loading and input checks require a Unity playtest.
