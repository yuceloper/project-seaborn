# Stormjaw import

1. Pull feature/expedition-economy-report.
2. Stop Play Mode; extract Seaborn_Stormjaw_2K.zip beside Assets in the project root.
3. Run Seaborn > Art > Build Stormjaw after importing completes.
4. Enter a sea map and find Stormjaw Leviathan.

Source: Meshy Stoneback Leviathan 0920233233 FBX / 0920233239 GLB; 30923 triangles.
Textures: 2048x2048, URP packed metallic R / inverse roughness A.
Head already faces +Z; preserve FBX upright correction without additional yaw.
Prefab length 4.2; existing boss root scale 1.7 gives length 7.14 in-game.
Centered on existing collider, using existing sea height. No extra root scaling.
Start selects boss prefab after spawner configuration. Removes old primitive body,
crown and ridge only when the replacement prefab is available. Missing prefab
retains the existing boss visual. Normal hunts still use SeabornWhaleVisual.
Existing health, loot, charge, retreat, sinking and 300-second respawn are unchanged.
Static mesh: no skeletal tail animation included.

Checked offline: mesh orientation, texture packing, ZIP CRC and source-byte equality.
Unity compilation/Play Mode unavailable here. Verify head-first movement, waterline,
charge/retreat, harpoon hits, sinking and boss respawn in Unity.
Commit the art folder, generated material/prefab and all Unity .meta files after import:

```sh
git add Assets/_Project/Art/Creatures/Stormjaw Assets/_Project/Art/Creatures/Stormjaw.meta Assets/_Project/Resources/SeabornStormjawVisual.prefab Assets/_Project/Resources/SeabornStormjawVisual.prefab.meta
git commit -m "art: add Stormjaw model and generated prefab"
git push
```
