# First expedition economy baseline

This pass budgets the starter Sloop against Central Waters pirates. It does not replace the temporary contract system or balance endgame ship purchases.

## Authored changes

| Item | Before | Now |
|---|---:|---:|
| Skirmisher wreck cargo | 80 | 110 Silver |
| Marauder wreck cargo | 110 | 145 Silver |
| Gunship wreck cargo | 180 | 210 Silver |
| Merchant wreck cargo | 100 | 65 Silver |
| Starter Sloop cargo capacity | 180 | 320 Silver value |
| Rat Sails cargo capacity | 280 | 480 Silver value |

Capacity values above are catalogue values: they were not reliably bound to runtime cargo. Ship profile application now updates cargo capacity, and late-attached cargo reads the active profile at Start. Existing cargo is never truncated if it exceeds the new capacity; further pickup waits for delivery.

Fishing cargo remains 20. Existing west x1.5 / east x0.9 regional modifiers remain. A western gunship wreck is 315, so even a starter with an empty hold can collect one; mixed full holds still require delivery before pickup. Unarmed merchant rewards are below armed pirate rewards.

Ammunition bundles now read the equipment catalogue for both quantity and price; serialized fields remain only as a missing-catalogue fallback. Standard ammunition remains 40 for 20 Silver. Hull repairs remain 0.025 Silver/HP and subsystem repairs 0.1 Silver/integrity point, rounded up together. The first cannon upgrade stays 180 Silver with no materials.

Buying an upgrade no longer repairs existing hull damage for free. A hull upgrade increases maximum HP while retaining current HP; the harbor repair quote includes the remaining missing HP. Save restoration behavior is unchanged.

## Budget assumptions

Six installed 6 lb cannons, three actual muzzles on one broadside, 85 damage per standard round, no captain/equipment/consumable boosts. Enemy hulls are 900 / 1125 / 1750. Baseline hit fraction is 65%; round to whole three-cannon salvos. Per fight repair assumes 25% of the 2000 HP starter hull lost plus 40 total missing subsystem points: ceil(500 x 0.025 + 40 x 0.1) = 17 Silver.

| Target | Salvos | Consumed ammo value | Repair | Delivered cargo | Estimated net |
|---|---:|---:|---:|---:|---:|
| Skirmisher | 6 | 9 | 17 | 110 | 84 |
| Marauder | 7 | 10.5 | 17 | 145 | 117.5 |
| Gunship | 11 | 16.5 | 17 | 210 | 176.5 |

No contract bounty, recovered rounds, or material value is credited. No consumable, field-repair, sinking or rescue costs are included. Damage received is an explicit assumption, not a prediction from enemy AI. Ammunition cost is consumed-unit value; actual wallet outlay comes in bundles. Rewards remain cargo and are lost if the player sinks before delivery.

Two skirmishers yield about 168 net; a skirmisher and marauder about 201.5. Thus the first cannon upgrade targets two to three baseline victories without relying on the repeatable bounty. The existing 160 Silver bounty can accelerate this. Later upgrades still require their material drops.

## Repeatable check

Run `python Tools/check_expedition_economy.py` from the repository root. It reads live catalogue and C# constants, calculates clean/baseline/costly scenarios, and fails if baseline profit falls below 50, planned two-wreck cargo exceeds capacity, or a western single wreck exceeds starter capacity. This is a budget regression check, not a combat simulation.

## Unity validation still required

1. Fresh starter, standard rounds, no active buffs. Fight each pirate; record hits, rounds used, duration, damage and repair quote. Compare actuals with assumptions before further tuning.
2. Pick up a skirmisher and marauder wreck: cargo should be 255/320. Deliver once, confirm 255 cargo Silver plus only independently earned quest rewards.
3. Confirm the shop shows catalogue quantities/prices and the expedition report uses the same per-unit values.
4. Damage the player, buy a cannon or harpoon upgrade: current hull must not jump to full. Buy a hull upgrade: maximum rises, current hull remains unchanged, repairs still cost Silver.
5. Existing saved Sloop/Rat Sails should receive the new capacity when their ship definition is applied. Check a 315-value western gunship wreck with an empty starter hold.

Executed here: the Python budget checks and C# syntax parsing. Unity compilation and gameplay are not available in this environment. Keep the hit/damage assumptions provisional until measured in play.
