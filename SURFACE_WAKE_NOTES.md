# Surface wake acceptance pass

## Goal
A continuous water-attached stern wake, not smoke, floating bubbles or speed lines.
The existing ship visual lean is unchanged.

## Evidence and root cause
PrototypeOcean and PrototypeHarbor serialize Ocean at Y=0.7 and PlayerShip at Y=0.5.
Earlier ship-root-relative offsets of 0.12 placed the wake below the serialized water plane.
Raising/lifting particles made them visible but produced detached puffs in the supplied gameplay video.

## Implementation
- World-space mesh ribbon tracks historical stern positions and headings.
- Width grows with sample age; opacity decays over 3.8 seconds.
- Procedural multiscale noise breaks up a soft center and two stronger shoulders.
- Ocean transform supplies water height; clearance is 0.035 world units.
- Missing Ocean logs a warning and uses explicit fallback height (0.7).
- Dedicated shader is loaded from Resources, so no scene material setup is required.
- Forward motion emits; stopping/reversing stops new samples while old foam fades.
- Map travel/teleport clears history; disabling/destruction clears/releases runtime resources.
- Bounded history: 192 samples plus one live head (386 vertices).
- Existing combat splash and leviathan effects are unchanged.

## Limitations
This targets the current flat-water prototype. It does not sample displaced wave heights.
Source-level checks passed; Unity compilation, Editor rendering and standalone rendering still need user testing.
This is a first surface-foam pass, not a claim of final photorealistic water.

## Acceptance checks
1. Pull feature/sloop-sailing-feedback, exit Play Mode and restart.
2. At rest: no new wake; old wake fades without floating upward.
3. Half/full ahead: narrow stern origin, widening broken foam on the surface.
4. Turn both directions: previous path stays in place and curves behind the ship.
5. Return to harbor / change map: no line bridging maps.
6. Zoom in/out: stays flat on water, no camera-facing billboards.
7. Check Console for shader errors or missing Ocean warnings.
8. Repeat in Windows build; inspect water height, shore occlusion and transparency.
9. If origin is wrong, inspect sternOffset against model before changing foam intensity.

## API references checked
- [Unity Mesh.SetVertices](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Mesh.SetVertices.html)
- [URP shader position transforms](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/use-built-in-shader-methods-transformations.html)
