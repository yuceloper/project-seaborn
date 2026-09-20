# Duskwind import

1. Pull `feature/expedition-economy-report`.
2. Stop Play Mode. Extract `Seaborn_Duskwind_2K.zip` into the Unity project root (next to Assets).
3. Let Unity finish compiling/importing, then run **Seaborn > Art > Build Duskwind**.
4. Enter a sea map with a Marauder (raider). Other pirate archetypes keep their current models.

Source: Meshy Duskwind Corsair 0920230316 FBX / 0920230321 GLB, 30,434 triangles.
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
If fine adjustment is needed, edit the muzzle coordinates in DuskwindModelSetup.cs
and rebuild; prefab-only changes will be overwritten by the menu.

Binary art is supplied separately from the code commit. After successful import, commit
the art folder, generated material/prefab and their Unity .meta files using Git LFS:

```sh
git add Assets/_Project/Art/Ships/Duskwind Assets/_Project/Art/Ships/Duskwind.meta Assets/_Project/Resources/SeabornDuskwindVisual.prefab Assets/_Project/Resources/SeabornDuskwindVisual.prefab.meta
git commit -m "art: add textured Duskwind model and generated prefab"
git push
```

All Marauder archetypes now use this visual. Enemy names, population and combat statistics are unchanged. The five NPC archetypes now each have a Meshy visual integration; each corresponding art package and generated prefab must be installed.
