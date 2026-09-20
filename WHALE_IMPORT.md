# Whale import

1. Pull feature/expedition-economy-report.
2. Stop Play Mode and extract Seaborn_Whale_2K.zip beside Assets in the project root.
3. Run Seaborn > Art > Build Whale after Unity finishes importing.
4. Enter a sea map. Normal Tideback hunts use the whale; Stormjaw keeps its existing visual.

Source: Meshy Gentle Giant 0920231524 FBX / 0920231521 GLB, 31058 triangles.
All shipped textures are 2048x2048. URP packed R=metallic, A=1-roughness.
Preserves FBX -90 X upright correction; no additional yaw because head is already +Z.
Length 4.2, centered on existing hunt body/collider. Existing root sea height and bobbing apply.
Start selects normal-hunt visuals after the spawner configures the boss in Awake's caller.
Missing prefab retains procedural fallback. Existing health, reward, harpoon collider,
fleeing, sinking, 60-second normal respawn and 300-second boss respawn are unchanged.
This is a static mesh: existing whole-body motion remains; no skeletal tail animation is added.

Offline checks: source orientation, dimensions, texture packing, integration paths.
Unity compilation and Play Mode could not be run here. Check head-first swimming, sea height,
harpoon hits, sinking and respawn; also check Stormjaw still has its separate visual.
Binary assets are separate from the code commit. Commit generated material, prefab and all
Unity .meta files after import, using the repository's Git LFS configuration.

```sh
git add Assets/_Project/Art/Creatures Assets/_Project/Art/Creatures.meta Assets/_Project/Resources/SeabornWhaleVisual.prefab Assets/_Project/Resources/SeabornWhaleVisual.prefab.meta
git commit -m "art: add textured whale and generated prefab"
git push
```
