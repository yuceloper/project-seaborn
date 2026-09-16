# Ocean surface polish — reflection and wake refinement

Base: feature/sloop-sailing-feedback (accepted surface foam). Wake fading/shading refined in this follow-up; no ship handling changes.

## Direction
Deep blue open water, turquoise harbor/east, darker western waters.
Four directional swells plus three independently rotated noise-gradient ripple layers.
Fine ripples are filtered at subpixel scales; highlights broaden with normal variation.
Subtle procedural sky tint/reflection and sparse crest breakup.
Main-light shadow attenuation supports ship/pier grounding.
Water geometry, collider height and render queue remain opaque/unchanged.

## Explicit limitations
This is an art-directed surface shader, not an ocean simulation.
There is no measured bathymetry, underwater refraction, shoreline foam, reflected ship image,
SSR, planar reflection camera or displaced wave geometry.
Sky reflection is a procedural approximation.
Performance and appearance are not verified in Unity; source/property checks only.

## Integration
Existing shader GUID/name and material references preserved.
Regional profiles update new surface properties at runtime; scene settings cannot silently
restore the old broad-band palette.
Shader is already referenced by OceanPrototype.mat; no new resource or manual wiring needed.
No change to global post-processing, camera, ship light settings, water height or wake anchoring.

## Acceptance
- Exit Play Mode, switch to feature/ocean-surface-polish, restart.
- Compare harbor and central water at identical zoom and ship position.
- Ocean should have moving fine structure without looking like lava, stripes or glitter.
- Ship silhouette, reticle, loot and wake remain clearly visible.
- Near/far zoom and turns should not generate crawling pixels or flashing glints.
- Verify west roughness and east turquoise palette after map travel.
- Confirm material/shader has no pink output or Console errors in Editor AND Windows build.
- Compare GPU frame time before/after at the same resolution; this shader does more per-pixel work.
- If too busy: lower RippleStrength, then WhitecapStrength in regional profile.
- If highlights overwhelm the target: lower GlintStrength, not water/wake height.

## Follow-up, only after visual acceptance
Investigate depth-based shore transitions and reusable water normal textures.
Do not add vertex displacement without updating the accepted wake and waterline contract.

## API reference
[Unity 6.6 custom shader shadows](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/use-built-in-shader-methods-shadows.html)

## Gameplay follow-up
Video 01.40.19.18 showed orange reflection grids and dense repeating whitecaps.
- Quintic noise gradients replace periodic fine-wave trains; independent rotations break stripe intersections.
- Softer, weaker highlights with reduced double-orange tinting.
- Whitecap strength: 0.008–0.045 by region, stricter crest thresholds and irregular patch gating.
- Wake spread settles gradually; aged shoulders dissolve unevenly.
- C# vertex color R encodes foam age; A remains opacity. Wake shader and controller update together.
- Filter distant fine foam detail to reduce shimmer.

Validation: analytic noise gradients checked against finite differences at 200 locations;
101 wake ages checked for finite, monotonic bounded spreading.
Material/profile property names checked against shader declarations.
Unity compilation, visual quality and GPU timings are still unverified here.

Acceptance: half/full ahead, left/right turns, stop and observe tail fading, far zoom,
map transitions, Windows build. Compare highlights at the same sun angle.

## Bow wash pass
User accepted the reflection / wake refinement and requested continuation.
- Two short curved ribbons at the bow, on the same Ocean water plane.
- Forward-speed driven, with smooth buildup/fade and a restrained helm-side bias.
- Uses the existing surface foam shader; no particle spray or extra material/draw call.
- Stern history and accepted ocean shading remain unchanged.
- Bow endpoints fade to zero; bow strips do not connect to stern strip or each other.
- 36 extra vertices and 32 extra triangles; maximum combined capacity 422 vertices.
- Bow offset/length/width and enable switch are exposed in ShipSailingFeedback.
- Geometric defaults target the current 6.5-unit fitted prototype hull, not arbitrary imported ships.

Acceptance: check port/starboard foam is outside the hull at half/full ahead,
turn both ways, stop and reverse (no new forward bow wash), switch maps,
compare near/far zoom. Console/Editor/Windows visual testing remains pending.
