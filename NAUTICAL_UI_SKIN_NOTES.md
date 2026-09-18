# Nautical UI skin

## Visual direction
Project Seaborn's HUD uses compact maritime instruments rather than generic
transparent debug panels. Deep teal enamel carries tactical information,
weathered ivory parchment is reserved for objectives and route context, and
aged brass marks selection or urgency. Pure black panels and neon accents are
outside the visual language.

## Runtime assets
SeabornUiSkin creates six deterministic, scalable UI sprites at runtime:
- enamel information card
- parchment objective card
- normal and selected hotbar slots
- chart/minimap surface
- compact progress track

The sprites include chamfered transparent corners, layered rims, restrained
grain and 9-slice borders. They are generated once per play session and require
no inspector references or external texture imports. This keeps the prototype
portable while defining an art target for later authored PSD/PNG assets.

## Applied surfaces
- Ship, resources, target, helm, combat and notification cards: teal enamel.
- Expedition card: ivory parchment with dark ink typography.
- Minimap chart: raised chart surface.
- Ten hotbar cells: framed slots with a distinct selected variant.
- Progress tracks: unified inset maritime surface.
- Existing code-drawn gameplay icons remain and follow the new ivory palette.

## Verification
Source checks passed for balanced delimiters, all required skin bindings,
ten icon kinds and removal of the legacy black panel color.
Unity compilation, Play Mode and Windows build rendering were not available
in the remote environment.

Acceptance:
1. Restart Play Mode so runtime sprites are rebuilt.
2. Inspect 1920x1080, 2560x1440 and 1440x1080.
3. Verify parchment text contrast in harbor and expedition maps.
4. Confirm all cards preserve their chamfered corners while dynamically resizing.
5. Switch ammunition and harpoons; selected slot must change its full frame.
6. Trigger notifications, buffs, cargo and target cards.
7. Verify mouse aiming passes through every HUD surface.
8. Compare Editor and Windows build; inspect texture filtering and thin borders.

## Later production pass
Once layout and palette are accepted, replace generated sprites with authored
texture assets using the same 9-slice contract. This avoids committing to an
expensive texture pipeline before the UI structure settles.
