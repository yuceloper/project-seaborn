# Harbor environment polish

This pass follows the accepted reference HUD and targets the remaining prototype-looking harbor architecture visible in the latest in-game capture.

## Scope
- Harbor Office: adds lower side wings, a central watch tower/cupola, front posts, inset windows and entrance steps.
- Trade Warehouse: adds a loading canopy, three loading bays, posts, an east annex and roof vents.
- Shipwright Workshop: adds a work awning, braces, a side shed, barrels and lumber stacks.
- Quay: adds low bollards to tie the masonry wall to the timber piers.
- World labels: reduces the oversized billboard treatment and repositions TERSANE, TİCARET and LİMAN İDARESİ over small architectural sign boards.

The pass is deliberately additive. It does not alter docking station positions, trigger volumes, harbor navigation, combat safe-zone behavior, ship controls or the established HUD.

## Runtime implementation
`HarborEnvironmentPolish` attaches after `PrototypeHarborVisualDirector` has produced the procedural harbor. It waits for the three primary buildings before applying details and uses collider-free primitives with a shared URP Lit material. The original harbor director remains the authority for the base footprint and label billboard orientation.

## Acceptance checks
1. Restart Play Mode so the procedural harbor rebuilds from scratch.
2. From the default tactical camera, the Harbor Office should read as a civic building with a distinct central silhouette rather than one dark slab.
3. The Trade Warehouse should visibly communicate loading/commerce through its canopy and bays.
4. The Shipyard should read as a working yard with structural and prop clutter.
5. `LİMAN İDARESİ`, `TİCARET`, and `TERSANE` should remain readable without dominating the screen.
6. Verify docking at all harbor stations and confirm no new visual primitive blocks ship movement; generated colliders are explicitly disabled and removed.
7. Check the Windows build as well as the Editor because the earlier prototype showed Editor/build visual differences.

## Follow-up
If this composition is accepted, the next environment step should be actual authored harbor assets/materials rather than continuing to increase procedural primitive count. Profile renderers before scaling this technique further.
