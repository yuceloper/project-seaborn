# Fresh captain playtest

1. Stop Unity Play Mode. Wait for compilation to finish.
2. Choose **Seaborn > Testing > Reset Player Progress**.
3. Save or discard modified scenes if Unity asks. Cancel aborts before touching progress.
4. The tool backs up the existing progress file, writes a fresh captain save, and opens `Assets/_Project/Scenes/PrototypeHarbor.unity`.
5. Press Play. Check the starter ship, level, inventory and currency before sailing.

## Shared new-player profile

The same profile is used by the reset tool and by the runtime loader when no progress file exists. Existing saves continue to load normally until the reset is explicitly selected.

- Seaborn Sloop only, captain XP 0 / level 1, Silver 0, Gold 0.
- Six owned and installed iron 6 lb cannons; one patched canvas sail.
- Standard 120, chain 12, grapeshot 16; standard selected.
- Light harpoons 10, heavy 0; light selected.
- One tonic; no other consumables.
- No upgrades, captain skills, materials, spare advanced equipment or deck extensions.
- Empty cargo and fresh session contracts/expedition state come from the new Play session. Contracts are not persisted in the captain save.

This prevents serialized prototype/test stocks in a scene from masquerading as a new-player experience. It also prevents a reset test from starting with the first cannon upgrade's purchase money already supplied.

## Save safety

Only `project-seaborn-progress.json` under `Application.persistentDataPath` is replaced. An existing save is backed up beside it with `.before-reset-<UTC timestamp>-<unique id>.bak`. The Console prints the exact backup path. File replacement preserves the original if the atomic replacement fails. Unrelated preferences and files are not deleted.

To restore, stop Play Mode, keep a copy of the current save if wanted, and copy the desired backup over `project-seaborn-progress.json`. Restart Play from the harbor.

The menu is disabled while playing, entering Play Mode, or compiling. The old component context action now also refuses live resets, so autosave/OnApplicationQuit cannot recreate the previous captain after deletion. This Editor command does not modify an installed standalone build's separate save location. No save was reset remotely by adding this code; reset runs when you select the menu locally.

## Verification

C# syntax parsing passed; harbor scene path was verified in the repository. Unity compilation and execution were not available here.

Unity checks: reset an advanced captain, confirm a backup exists and the listed starter state loads; spend/fire, stop and resume to verify normal persistence; reset again; verify menu is unavailable in Play Mode; restore a backup and check the advanced captain returns. Also test deleting/moving the save while Unity is stopped: the next first launch must use this same new-player profile.
