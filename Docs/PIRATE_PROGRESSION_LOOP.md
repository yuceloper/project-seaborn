# Pirate salvage and shipyard progression

Pirate wrecks now carry guaranteed upgrade materials. Collect with E, then return
to port to bank them. The HUD marks carried materials as unsecured. Sinking loses
them with the silver cargo; only delivered materials enter the saved inventory.
Ordinary hunting material rewards keep their existing behavior.

| Wreck | Corsair iron | Chart fragments |
| --- | ---: | --- |
| Fishing boat / merchant | 0 | 0 |
| Skirmisher | 1 | 20% chance of 1 |
| Marauder | 2 | 20% chance of 1 |
| Gunship | 3 | 1 guaranteed |

Western Reach adds one iron to pirate wrecks. Charts roll once when a wreck is
created, never again when the player retries collection. Silver/ammunition
rewards are unchanged. A full hold rejects the entire wreck, including materials
and ammunition. Materials travel with the wreck's existing silver capacity cost.

## Progression

- First cannon upgrade: 180 Silver, no materials. Existing bonus remains +15%
  damage and 10% shorter reload; this gives a safe first improvement.
- Second cannon upgrade: 360 Silver and 2 iron. Two central-water Skirmishers
  supply the iron; other activities can supply the remaining Silver.
- Third cannon upgrade: 540 Silver, 3 iron and 1 chart. Gunships provide a
  guaranteed chart route, alongside the existing other chart sources.
- Bonuses are cumulative: cannon levels 1/2/3 give +15/23/31% damage and
  10/15/20% shorter reload. No damage rebalance was added in this change.

The shipyard lists current and next effects separately, owned/required resources
per track, and the missing resources for the next cannon upgrade. Completed
upgrades show the resulting bonus. Max-level rows have no next-level offer.

## Unity acceptance checks

1. Collect a central Skirmisher wreck: HUD shows one unsecured iron; inventory
   iron stays unchanged. Return to port: iron increases by one and the expedition
   report includes it. Re-enter delivery area: no duplicate reward.
2. Repeat, but sink before delivery: pending iron/charts clear, secured inventory
   stays unchanged, and no material gain appears in the return report.
3. With insufficient cargo space, press E repeatedly: no silver, materials or
   ammunition are awarded; the same wreck/reward remains available.
4. Collect fishing and merchant wrecks in each region: no iron/charts. Gunship
   salvage carries three iron and one chart (four iron in the west).
5. Save/relaunch after delivery: secured materials and upgrade levels persist.
   Unsecured cargo follows the existing session-only cargo behavior.
6. At cannon level zero, the panel shows 180 Silver and no materials. Buy it:
   Silver decreases once, damage/reload change immediately, HP is not healed.
7. At level one, 360 Silver but only one iron keeps the button disabled; two iron
   enables it. Buying consumes exactly 360 Silver and two iron. Test missing
   chart at level two, max level three, and each other upgrade track as well.
8. Inspect the expanded shipyard panel and cargo text at supported resolutions.

Static syntax/source review is possible here; compilation, UI rendering, save
reload and physics/combat behavior still require the Unity editor.

## Next playtest

- Compare the same Skirmisher with the starting cannons and the first upgrade.
- Check whether a guaranteed iron makes another pirate encounter worthwhile.
- Tune reward pacing after several voyages; story quests remain deferred.
