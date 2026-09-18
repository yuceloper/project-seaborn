# HUD refinement after in-game review

- Keep the authored nautical atlas; tint decorative frames toward quieter brass.
- Replace selected-slot bloom with a restrained bright frame, without changing selection semantics.
- Use the built-in sans-serif font for small labels and retain serif for headings.
- Increase hotbar key/count readability and move key labels to consistent top-left insets.
- Give notifications 30px horizontal padding, 82px height and 90px stack spacing; remove the detached accent stripe.
- Reduce compass footprint to 84%; change both map camera extent and marker projection to the same 60-unit half-extent.
- Preserve all inventory, combat and input bindings.

Validation: source changes inspected; camera extent and marker projection share one constant. This environment cannot run Unity. Check text fit, notification stacking, minimap markers at region boundaries, and harbor round trips in Editor and a player build. No claim of final visual approval.
