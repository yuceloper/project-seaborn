# Coastal harbor models

The supplied Meshy rock, wooden dock and barn exports replace selected prototype visuals.
This is an art-integration pass, not a new terrain/navigation system or complete harbor rebuild.

## Installation

1. Pull `feature/expedition-economy-report` and exit Play Mode.
2. Extract **Seaborn_Coastal_Harbor_2K.zip** into the project root, beside `Assets`.
   The archive already contains `Assets/_Project/Art/Environment/SeabornCoast`; avoid `Assets/Assets`.
3. Wait for imports, then run **Seaborn > Art > Build Coastal Harbor Models**.
   Missing prefabs are also built automatically once all inputs are present.
4. Run **Seaborn > Validation > Check Coastal Harbor Models** and enter `PrototypeHarbor` Play Mode.
5. Commit the source models/textures and generated materials, Resources prefabs and `.meta` files
   from Unity using the project's existing large-file workflow. The remote code commit alone does not include binary assets.

Missing models retain the previous blockout. Re-running the build menu recreates these three generated
prefabs/materials; make separate variants if manually editing them.

## Normalization and materials

- Preserve the supplied FBXs' authored -90° X / 100 scale transform; do not reset imported roots.
- Rock: 12-unit width, ground pivot. Workshop: 8-unit width, ground pivot, entrance toward +Z.
- Dock: long axis rotated from X to Z, 8-unit length, 3.48-unit width. The pivot follows the measured
  deck surface (82% of total height from the lowest pile); support piles extend below it.
- Dock cells fill the existing pier/platform footprints. Footprint scaling affects X/Z only.
- External textures are 2048×2048: sRGB base color, normal map, linear metallic/smoothness.
  Packed texture R is metallic; A is `1 - roughness`. Embedded original FBX data is retained;
  automatic FBX material import is disabled, so only the assigned prepared maps are used.
- Authored visuals have a marker preventing HarborMaterialLightingPolish from tinting their PBR materials.
- No mesh collider is generated. Existing docking triggers, stations, navigation and save data are untouched.

## Placement

- Both long piers and their loading platforms use the dock module instead of procedural planks.
- Shipwright Workshop uses the barn model; the old workshop roof/awning detail pass is skipped.
  Its label is raised above the new roof. Trade warehouse, office, crane and lighthouse remain blockouts.
- Eleven rock instances create shoulders and a southern rim around the existing quay.
- Four existing offshore silhouette locations use the same rock model at different scales/headings.
  The coastal controller is recreated after scene travel; it must not duplicate within a scene.
- These rocks are visual-only, like the old silhouettes. Solid terrain and NPC obstacle avoidance
  must be designed together in a later geography pass, not added as invisible blockers now.

## Verification

Input archive CRC and output archive CRC, texture sizes, source file hashes and C# syntax can be checked
outside Unity. The previous local inspection rendered all three supplied GLBs upright and textured;
their FBX transforms were also inspected. The editing environment cannot run Unity compilation or Play Mode.
The included Unity validation menu checks generated prefab presence, dimensions/pivots, material maps and no colliders.

Playtest: fresh start and harbor re-entry, all three docking stations, shipyard zoom, pier joins/waterline,
workshop entrance direction, no old roof clipping, no unwanted brown tint, models included in a Windows build.

## Reproduce the asset archive

With Pillow installed, run `python Tools/prepare_coastal_harbor.py INPUT_DIRECTORY OUTPUT_DIRECTORY`.
It reads the three original FBX ZIPs without changing them and writes the normalized-name asset package.
Geometry is retained; generated `.meta` GUIDs are deterministic for this asset pack.

## Texture import repair

The first distributed ZIP contained truncated CoastalRock Normal and MetallicSmoothness PNGs.
Extract the corrected package over Assets and run the build menu again; paths and GUIDs are unchanged.
PNG encoding now uses an in-memory buffer followed by an atomic file replacement. Packaging checks
PNG chunk checksums and fully decodes all nine images from the final ZIP. The Unity builder decodes
all sources before modifying materials/prefabs and rejects missing imported maps instead of reporting
success. After a failed automatic build, repair the files and run the build menu manually.

Imported textures may be reduced by platform Max Size overrides. The builder checks 2K on
the decoded source, accepts valid reduced imported maps, and reports their actual dimensions.
Texture settings are saved before a forced synchronous import. A missing Texture2D is reported
separately from a source decoding failure, with the active build target for diagnosis.

For GUID-only texture metadata, setup explicitly initializes TextureImporterSettings including
Texture2D shape and automatic default platform format. Generated coastal textures have their
Standalone override cleared. If import still returns no Texture2D, one uncompressed retry is made
through Unity's importer (including normal-map conversion); successful recovery logs increased
texture memory use. A final failure reports actual main asset type, shape, GUID and build target.
This repair preserves PNG contents and GUIDs; it does not delete project-wide import caches.
