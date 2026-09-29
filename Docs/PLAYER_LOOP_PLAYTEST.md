# Combat readability and first expedition loop

## Changes

- Ship impacts aggregate actual hull damage into short-lived numbers. Recent damage lingers in the target health bar; hulls below 35% emit intermittent smoke. Cosmetic effects never move the physics root.
- Aiming labels explain range, firing arc, empty ammunition, safe harbor, reload and selected broadside. The target panel follows the aim point while aiming; inactive consumable icons fade slightly.
- Enemy telegraphs start after reload, cancel when the firing solution is lost, and lock aim for the last 0.4 seconds. Enemies still hold position after aligning.
- Harbor Office offers **Korsan Avı**: sink a pirate, collect its wreck with E, deliver to harbor. Civilian wrecks do not count. Delivery pays 160 Silver plus the wreck's existing cargo value. No kill-only bounty.
- Wreck collection requires enough room for the entire cargo reward; failed collection grants neither ammunition nor materials and leaves the wreck available. Nearby wrecks show a distance label.
- First cannon upgrade costs 180 Silver without materials: +15% damage, -10% reload duration. Later levels add 8 percentage points damage and 5 points reload reduction and retain material requirements. Existing upgrade levels use the same new curve.
- Contract board and expedition director travel with the player. Harbor protection uses the harbor scene rather than coordinates shared across different maps. Empty returns reopen preparation without counting as successful voyages; a recovered failed expedition can restart.
- Population remains five ships, twenty normal hunts and one boss. Fishers patrol the south, the merchant travels between two local waypoints, pirates patrol around northern homes, hunts occupy two coastal bands and the boss begins northeast. Respawns retain original home areas across generations. Ordinary hunts also clamp to map bounds.
- Sun/post-processing warmth is reduced to preserve imported material colors. Harbor annexes have pitched roofs in place of flat decorative caps.

## Verification performed here

All 27 inspected C# files parse with the C# tree-sitter grammar. Changed source baselines were compared with branch commit `696a13d88b00733abe89e4d42e0d5fd5c5225db8`; differences were only trailing newlines. Whitespace diff check produced no warnings. Reviewed cargo event ordering, reward guards, broadside API compatibility and scene lifecycle.

No Unity Editor or Unity compiler is available in this environment. Syntax parsing is not compilation, and the following gameplay checks remain to be run in Unity.

## Unity acceptance route

1. Start in PrototypeHarbor. Accept Korsan Avı at Harbor Office. Sail to Central Waters: selected contract must survive; weapons must unlock in the sea, including at coordinates matching the former harbor position.
2. Aim at both sides of a pirate. Check range/arc/reload labels, only the selected broadside firing, grouped damage numbers and delayed target health. Watch one enemy reload: preparation must be absent during reload, then visible before firing. Move after its aim locks and confirm the salvo can miss.
3. Sink the pirate. Killing alone must not pay the bounty. Approach its wreck and press E. Confirm cargo and a return instruction; return south and deliver. Wreck value plus exactly one 160 Silver reward should be paid. Moving around harbor must not pay again.
4. Repeat with insufficient cargo space. Collection must leave the wreck and ammunition/material totals unchanged. Free space and collect once. Sink after collecting a pirate wreck: cargo loss must remove quest cargo; returning without it must not pay the bounty.
5. A fishing boat or merchant wreck must not complete Korsan Avı. Repeat a new pirate expedition to confirm progress resets per departure.
6. Buy the first cannon upgrade with 180 Silver and no materials; compare damage/reload before and after. Later upgrades still require materials. Net voyage profit depends on ammunition and damage taken; the reward is not a guarantee of profit under every outcome.
7. Cross sea-region boundaries and return. Contract progress must survive; there must be one contract board and one expedition director. Return empty, and separately sink/recover: both routes must permit preparing another expedition without successful-voyage reward farming.
8. Watch respawns (ships 90s, hunts 60s, boss 300s). Replacement can be delayed while the player is within 30 units of its spawn area. Repeated hunt respawns must not drift progressively across the map.
9. Inspect 1920x1080 and a smaller game view: contract panel, aiming label, damage text and target health remain readable. Verify wood/blue paint under regional lighting and annex roofs in harbor. Measure performance during repeated grapeshot impacts; transient particle systems/materials must expire.

Quest state survives scene travel within a play session. This change does not add contract save/load across application restarts or skeletal animation to imported meshes.
