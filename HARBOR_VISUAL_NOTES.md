# Harbor visual pass

## Direction
The harbor should read as a weathered working port: desaturated timber,
stone quays, pale plaster and slate roofs. Brass/warm light is an accent.
This is a procedural environment art pass, not final production harbor art.

## Implemented
- Segmented wooden pier surfaces with real seams and deterministic tone variation.
- Two staggered masonry courses and a coping strip along the quay.
- Pitched roofs, closed gable ends, ridge caps, timber facade posts/beams,
  inset dark windows and stone building footings.
- Reduced pier lantern intensity from 45 to 3 and range from 9 to 6.
- Rough nonmetal harbor material; runtime roof meshes cleaned up on destruction.
- Visual primitive colliders disabled immediately before removal.

## Integration
Built by PrototypeHarborVisualDirector on scene creation; stop and restart
Play to rebuild. No scene setup or manual Inspector assignments required.
Based on feature/hud-visual-polish, including that HUD and previous sea/wake work.
Station triggers, navigation exits, primary building/pier positions and gameplay
logic are unchanged. No changes to shop panels in this pass.

## Verification
Source check confirms station-area build functions and navigation layout remain
unchanged. Roof winding and slope math reviewed. Unity compilation and visual
validation have not been run in the remote environment.

Play/build acceptance:
- Inspect the harbor from the normal tactical camera and near each station.
- Check roof ends for gaps and both roof slopes for correct shading.
- Confirm board seams are readable without distracting shimmer at normal zoom.
- Dock at shipyard, trade and administration; depart, travel and return.
- Compare lantern brightness against ship and sea in a Windows build.
- Profile the harbor before adding more detail: this pass adds 143 individual
  board renderers and 45 masonry blocks. Consolidate static geometry or use
  authored assets/LODs before expanding this approach to the full town.

## Next
- Apply the HUD's restrained hierarchy to harbor shop/service panels.
- Replace procedural structures progressively with authored modular harbor assets.
- Adjust text labels after judging the new skyline in gameplay.
