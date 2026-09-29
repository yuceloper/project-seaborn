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

## Cove layout follow-up

- The rectangular land slab is replaced with a curved bank and gently raised inland surface.
- A short masonry quay joins the existing piers; all three station coordinates stay unchanged.
- Rock clusters form irregular shoulders and both breakwaters; the long cube/sphere breakwaters are removed.
- Loading platforms use one wide dock mesh each to remove interior pile rows. They meet the
  pier edges without the previous 0.7-unit overlap and share the same deck height.
  The authored pier module still has end posts; a dedicated platform/junction model can improve this further.
- A shadowless cool fill light improves shaded hull readability while the harbor exists.
- No land/rock colliders are introduced by this visual pass. Terrain navigation remains future work.
- Trade warehouse, office and crane are still blockouts pending dedicated models.

Validate in Unity: all docking snaps and ropes; shipyard camera; bank/platform joins; no visible
mesh underside; lighthouse base sits on rocks; daytime lighting; leave/re-enter harbor.
Syntax and numeric layout checks outside Unity do not replace this Play Mode check.

## Harbor office and trade warehouse asset pack

Extract `Seaborn_Harbor_Buildings_2K.zip` beside Assets, then run
**Seaborn > Art > Build Harbor Buildings** outside Play Mode. This is an additive pack;
the original rock, pier and workshop assets remain installed.

- Meshy Stonewatch Manor becomes HarborOffice: 10-unit width, ground pivot at shore height 1.1.
- Meshy Medieval Stable becomes TradeWarehouse: 9-unit width, ground pivot at deck height 1.235.
- Preserve the FBX authored -90 X / 100 scale transform. The final entrance direction needs
  visual confirmation in Unity; geometry inspection outside Unity confirms upright proportions.
- Their generated Resource prefabs replace the named blockout buildings. Old office door,
  tower, wings and trade awning/annex/roof additions are skipped when the corresponding model exists.
- The same validated 2K PBR importer and palette protection are used. No new colliders.
- The two building inputs can build independently of the original coastal group.
- Six packed textures pass full PNG decode and chunk verification. Archive CRC and C# syntax
  were checked; Unity compilation, import and Play Mode must be verified locally.

Run **Seaborn > Validation > Check Coastal Harbor Models** after both packs are installed.
Check entrances face the water, roofs have no old geometry, footprints stay on their platforms,
station labels remain visible and all three docking actions still work. Commit generated assets
and their metadata using the project's existing large-file workflow.

Reproduce the additive pack with `python Tools/prepare_harbor_buildings.py upload output`.

## Harbor navigation follow-up

The cove now has a closed static collision volume with vertical waterline faces, plus
frictionless solid footprints under piers/platforms, breakwaters and the lighthouse foundation.
It reuses ShipContactResponse sliding and contact steering; no collision damage is added.
Model prefabs remain collider-free; boundaries belong to the harbor scene root.
Office and warehouse instances rotate 180 degrees toward the water. Breakwater rocks are raised
from -0.95 to -0.25; the lighthouse gains a 10-unit rock base and roof height reduces to 7.15.
No model rebuild is required.

Numerical checks: closed manifold, nonzero face area, outward winding and station clearance
with a 1.2 half-width / 2.2 half-length envelope. Unity physics must be checked locally: glancing
contact, head-on steering and reverse, all docking snaps, upgraded hull sizes and harbor re-entry.
No automatic relocation is made for a ship already saved inside land.

## Ground surface pass

User confirmed docking works after the boundary pass. Ground work changes visuals only.
HarborGroundSurface maps a generated 512px albedo atlas over the existing shore: dark wet bank,
stone promenade, worn soil routes and patchy inland vegetation. Mipmaps and trilinear filtering
reduce distant shimmer. Grass and stones are two combined visual meshes without colliders;
a local random seed leaves gameplay random state unchanged. Generated materials, meshes and
texture are released with the harbor. No download or prefab build is required.

This is an initial procedural surface pass, not scanned PBR terrain. Inspect repetition, path scale,
shoreline transition and startup cost in Unity; compilation and rendering were not available here.
The crane replacement still needs a separate Meshy reference/model pass.

## Ground readability correction

Corrected the terrain blend masks to normalize sampled distances with InverseLerp before SmoothStep. The previous use treated distance thresholds as interpolation endpoints, suppressing the intended paths and paving. Ground albedo is now 1024 square, with wider pale stone paths, two building plazas and an irregular gravel-to-wet-bank transition. Breakwater visuals vary in height, width and angle; tested docking and collision boundaries stay unchanged. Trees and a replacement crane await Meshy models made from separate reference images. Unity visual verification remains required.

## Coastal pine and shipyard crane

Install `Seaborn_Harbor_Props_2K.zip` at the project root, then run **Seaborn > Art > Build Harbor Props** outside Play Mode. This separate group builds `SeabornCoastalPineVisual` and `SeabornHarborCraneVisual`; existing coast and building groups remain independent. Missing prefabs can also be generated on the next editor domain reload when both source assets exist.

Ten pine instances form sparse inland groups with varied uniform scales and headings. The pine prefab has width 7 and a ground-level pivot. The crane has width 5.2, a ground-level pivot, and is placed on the shipyard deck with 180-degree yaw so its hook faces the pier. It replaces all three primitive crane parts only when its prefab is available. Model colliders are removed; existing navigation and docking boundaries remain authoritative.

Reproduce the asset ZIP with `python Tools/prepare_harbor_props.py INPUT_DIRECTORY OUTPUT_DIRECTORY`. Supplied FBX geometry and authored -90 X rotation are preserved. External albedo/normal maps are 2K; 4K source metallic/roughness maps are resized and packed into R=metallic, A=smoothness at 2K. Original FBX files may still embed higher-resolution source textures. Both source archives passed CRC checks and all output PNGs were fully decoded from the final ZIP. Geometry axes/bounds were inspected. C# syntax checks passed; Unity compilation, appearance and performance still need local validation.

## Outer shore closure and layout correction

Added a sloping visual bank along both side edges and the rear, ending at Y=-2.5 below water. The existing shore solid and all docking positions remain unchanged. Cove-shoulder and rear rocks now have frictionless vertical box footprints (85% of renderer X/Z bounds); these are simplified contacts, not exact rock meshes. Rear outcrops are smaller and moved to the back edge; pine groups are moved inward to reduce overlap. Stone paths and plazas are narrower and darker, and paving fades before the rear edge. No asset archive or prefab rebuild is needed.

Validated skirt triangle winding/nondegeneracy numerically. Unity compilation, sliding around rocks and final visual placement require Play Mode testing.

## Textured outer bank

The bank now shares the ground material and world-aligned UV mapping, with the texture domain expanded to include the submerged edges. Gravel blends across the shared top edge into darker wet soil down the bank. Side segments are subdivided at roughly two-unit intervals and the submerged foot varies smoothly to break the straight waterline. Removed the procedural pyramid stones; sparse grass remains. Existing navigation colliders and docking positions are unchanged.

C# syntax, bank winding, nondegenerate triangles and UV-domain bounds checked. Unity rendering and collision behavior still require local Play Mode verification. No ZIP or prefab rebuild required.

## Subtle outer shoreline foam

Added one narrow transparent ribbon at the outer bank water intersection (local Y=0.04). A small generated seamless alpha texture produces broken patches; slow UV drift and opacity changes suggest gentle wash. The ribbon sits toward the water, casts no shadows, and uses no colliders or particles. Runtime mesh, texture and material are cleaned up with the harbor. The inner quay and docking positions are unchanged. No archive or prefab rebuild is needed.

Unity rendering remains unverified: check visibility against the ocean material, depth ordering and intensity in Play Mode. This is a cosmetic first pass, not simulated breaking waves.

## Outer bank contact alignment

The outer bank now derives static frictionless vertical contact strips directly from each visual triangle's intersection with local water level Y=0.04. The prior inland shore solid is retained as a backstop; the new strips stop the hull at the sloping bank's visible waterline. Strips are 0.3 units thick, extend from Y=-4 to Y=4, and overlap slightly at their ends. No collider is placed on the sloped surface, so collision response should remain horizontal. No docking stations, inner quay, player controls or saves are changed.

C# syntax and numerical intersection coverage checked. Local Play Mode verification is still required: approach the rear bank head-on, steer away while touching it, and check the corners. A saved ship already inside the newly blocked strip is not automatically relocated. No asset download or prefab rebuild required.
