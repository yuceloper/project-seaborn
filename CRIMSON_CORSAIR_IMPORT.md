# CrimsonCorsair import

1. Pull `feature/expedition-economy-report`.
2. Stop Play Mode. Extract `Seaborn_CrimsonCorsair_2K.zip` into the Unity project root (next to Assets).
3. Let Unity finish compiling/importing, then run **Seaborn > Art > Build Crimson Corsair**.
4. Enter a sea map with a Gunship (heavy gunship). Other pirate archetypes keep their current models.

Source: Meshy Crimson Corsair 0920210425 FBX / 0920210422 GLB, 30,712 triangles.
All shipped textures are 2048x2048. URP packed map R=metallic, A=1-roughness.
The builder preserves imported -90 X correction and applies 270 Y heading;
length 6.5, waterline 12% of bounds height. Rebuilding reproduces these settings.
Three muzzle empties per side are measured from the supplied mesh barrel ends,
in corrected prefab space. They follow visual buoyancy and bind to BroadsideController.
Missing/incomplete prefab falls back to the existing procedural pirate.

Validation: source geometry, FBX axis metadata, texture dimensions/channel packing
and code integration checked offline. Unity compilation and Play Mode are not available
in the preparation environment. In Unity check upright bow-first movement, sea level,
port/starboard shots originating at barrels, and respawn after sinking.
If fine adjustment is needed, edit the muzzle coordinates in CrimsonCorsairModelSetup.cs
and rebuild; prefab-only changes will be overwritten by the menu.

Binary art is supplied separately from the code commit. After successful import, commit
the art folder, generated material/prefab and their Unity .meta files using Git LFS:

```sh
git add Assets/_Project/Art/Ships/CrimsonCorsair Assets/_Project/Art/Ships/CrimsonCorsair.meta Assets/_Project/Resources/SeabornCrimsonCorsairVisual.prefab Assets/_Project/Resources/SeabornCrimsonCorsairVisual.prefab.meta
git commit -m "art: add textured CrimsonCorsair model and generated prefab"
git push
```

The in-game enemy names and combat statistics are unchanged; all Gunship archetypes use this visual. Marauder keeps its existing visual.
