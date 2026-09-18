# Hotbar and docking readability

## Implemented
- Ten passive hotbar slots: 1/2/3 ammunition, Shift+1/Shift+2 harpoons, 4–8 consumables.
- Code-drawn vector silhouettes, short names, stock counts and selection underlines.
- Selected cannon ammunition and selected harpoon remain visible even with zero stock.
- Tonic cooldown and active concealment/rum/gale/ironbark durations appear in their slots.
- Existing keyboard controls retained; slots do not accept mouse clicks or block aiming.
- Compact nearest-station ring: 1.3 visual radius, proximity fade from 14 units,
  hidden when docked or harbor menus are open. Actual approach radius remains 6.
- E docking prompt now uses the scaled HUD helm line instead of a separate IMGUI box.
- Delivery ring/beacon visible only with unsecured cargo nearby and no harbor menu.
  Delivery radius and reward logic remain unchanged.

## Integration and tests
Based on feature/harbor-art-pass. Restart Play Mode after pulling.
Source checks passed: removed old stock text references, balanced delimiters,
unchanged docking selection/snap/range method bodies, slots contained by the HUD.
No Unity installation/compiler is available remotely; compilation, rendering,
controller integration and Windows build verification remain pending.

Manual acceptance:
1. Switch 1/2/3; selected underline and counts follow, including stock zero.
2. Shift+1 and Shift+2 select harpoons; confirm selection and launch consume the correct stock.
3. Use 4–8; stock and timer update; tonic timer means cooldown, other timers mean active duration.
4. Travel out/back; approach each station, press E, close its menu and depart.
5. Carry cargo back; confirm normal delivery and no idle ring/beacon afterward.
6. Check 1920x1080, 2560x1440 and 1440x1080; test mouse aiming through passive HUD.
7. Check icon legibility and selection contrast in the Windows build.

## Follow-up
- Mouse-click activation needs a coordinated gameplay-input block, not just UI buttons.
- Replace placeholder silhouettes with authored icon art after the layout is validated.
- Ship lighting remains a separate task; no lighting changes in this pass.
