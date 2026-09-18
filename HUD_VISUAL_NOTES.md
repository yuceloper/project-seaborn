# HUD visual pass
Builds on feature/ocean-surface-polish; sea, accepted wake and ship movement are unchanged.

## Direction and scope
Gameplay HUD first: charcoal panels, restrained brass accents, readable off-white labels.
This is a layout/type/color pass, not final illustrated UI art.
Harbor station menus are not reskinned yet; harbor environment is the next separate art task.

## Changes
- Short brass marks replace full-width gold stripes.
- Ship/cargo/expedition cards become more compact.
- Empty cargo uses a quiet label; risk instructions appear when carrying unsecured cargo.
- Empty buff area collapses; supply counts move to the weapons panel with meaningful names.
- Helm/repair status sits in its own strip above weapons.
- Pressure track appears only during active expedition.
- Missing daily quest no longer creates a permanent prompt; selected task progress remains.
- Minimap, target state, active effects, selected ammunition, reloads and notifications retained.
- Passive HUD explicitly blocks no raycasts and is non-interactable.
- Zero-length bars hide rather than drawing negative-width fills.
- Height-based canvas scaling; top/bottom card extents checked for 4:3, 16:9 and ultrawide.
  Portrait is outside current PC prototype scope.

## Verification
Source checks: no duplicate methods within HUD class, initialized references, balanced structure.
Layout rectangle checks passed at virtual widths 1440, 1920 and 2560 (1080 high).
Unity compilation, actual font wrapping, Editor/Windows appearance not run here.

## Acceptance
- Restart Play Mode after branch switch.
- Compare 1920x1080, 1280x720 and 4:3. Check long ship names and large ammo counts.
- Check target, notifications, pressure and selected task while at sea.
- Check all 1–8 controls still perform existing actions (labels are not new buttons).
- Dock/undock, open station tabs, return from sea: HUD hides/restores without stealing clicks.
- Verify repair/concealment/buffs/countdowns and empty/nonempty cargo.
- Ship/water/foam should look identical to accepted ocean branch.

## Next
After this layout is accepted, create a cohesive wood/stone harbor corner.
Then share typography and component styling with harbor station menus.
