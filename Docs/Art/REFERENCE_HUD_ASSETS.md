# Reference HUD art pass

This supersedes the procedural HUD treatment in NAUTICAL_UI_SKIN_NOTES. Target is the user-approved nautical mockup: ship medallion, ivory health bar, parchment objective, currency capsules, circular compass map and ten illustrated floating hotbar slots. This is an implementation pass, not a verified pixel-identical Unity capture.

## Assets and provenance
Built-in imagegen produced transparent RGBA atlases from the approved mockup. No external image-generation credentials were used. Files are stored unmodified:
- Assets/_Project/Resources/SeabornHud/Frames.png — 1774 x 887; eight frames.
- Assets/_Project/Resources/SeabornHud/Icons.png — 1983 x 793; ten item illustrations.
- Assets/_Project/Resources/SeabornHud/Glyphs.png — 1536 x 1024; six utility glyphs.
- DejaVuSerif.ttf in the same directory; redistributed with Docs/Art/DejaVu-LICENSE.txt.

Generation briefs: transparent nautical game UI atlas matching the reference's antique brass, deep teal and ivory parchment; isolated boat medallion, compass rim, objective plaque, currency capsule, normal/selected item frames, health strip and location badge; no baked text. Item brief: cannonball, chain shot, scatter shot, two harpoons, tonic, concealment lantern, rum, wind swirl and shield, isolated on transparent background. Utility brief: silver/gold coins, cargo crate, sailboat, crew and compass rose; transparent-background correction applied to the utility sheet.

Generated source identifiers: Frames exec-631b391f-e6bb-4acd-b123-7fbe64b668e9; Icons exec-9968fc3f-4d91-48a1-a134-d2409c29031a; Glyphs exec-d507c223-b2c8-4e0c-bbcd-1da907f49394.
The repository's ship-texture LFS pattern does not cover these HUD PNGs; they are ordinary Git blobs.

## Integration
SeabornHudArt slices measured top-left pixel rectangles at runtime. Import at original dimensions: NPOT scaling off, max size 2048, mipmaps off, uncompressed RGBA. Do not resize the atlases without changing the rectangles.
PrototypeGameplayHud consumes these sprites directly. Game text and counts remain live. Existing key bindings and gameplay behavior remain unchanged. HUD graphics do not receive raycasts.
The circular map masks a 256px top-down render texture, refreshed about ten times per second, with shadows/post-processing disabled. Its camera is independent of the scaled Canvas and cleaned up with the HUD.
Canvas uses Expand at reference 1920 x 1080. No large background rectangle surrounds the hotbar. Legacy skin helpers remain available to other screens.

## Verification and next checks
PNG dimensions, transparency and slice bounds checked locally. No Unity executable is available in the authoring environment: compilation, player build, camera rendering and visual fidelity require an Editor run.
Check harbor -> ocean -> harbor, stock counts, selected ammunition, ship HP, circular minimap and 16:9 / 4:3 layouts. Compare an actual game capture against the approved reference before signing off visual parity.
