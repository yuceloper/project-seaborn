# Ocean surface polish — first pass

Base: feature/sloop-sailing-feedback (accepted surface foam). No wake or ship handling edits.

## Direction
Deep blue open water, turquoise harbor/east, darker western waters.
Eight directional normal waves replace two broad sine bands.
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
No change to global post-processing, camera, ship light settings or accepted wake.

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
