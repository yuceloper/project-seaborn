# Combat Balance Pass — September 2026

> Historical tuning record: the hull baselines and per-broadside cannon counts below are superseded by [Combat foundation repair](COMBAT_FOUNDATION_REPAIR.md). The follow-up repair tariff remains current.

## Goal

Move ship combat away from one-volley kills and create enough time for positioning, ammunition choice, subsystem pressure and reload timing to matter.

## Hull baseline

Enemy ships are normalized to a 2500 hull baseline before archetype multipliers:

- Skirmisher: 0.72x = 1800 hull
- Marauder: 0.90x = 2250 hull
- Gunship: 1.40x = 3500 hull

Player ship definitions now use their authored hull values instead of the old 0.05 prototype scale.

## Cannon baseline

Player cannon data is now used directly instead of the old hidden 0.25 damage scale.

- 6 lb Iron Cannon: 85 damage, 4.8 s reload
- 12 lb Iron Cannon: 125 damage, 6.2 s reload

With six 6 lb cannons, a perfect standard broadside is 510 raw hull damage before accuracy/spread effects. This gives the intended first-pass envelope of roughly 3–4 clean broadsides for a Skirmisher, 4–5 for a Marauder, and 6–8 for a Gunship.

## Ammunition identity

Standard Round Shot
- 1.00 hull damage
- 1.00 range
- 1.00 reload
- primary hull-killing ammunition

Chain Shot
- 0.50 hull damage
- 0.85 range
- 1.08 reload
- reduced spread penalty
- sail subsystem damage is intentionally moderated so one projectile cannot delete the rigging

Grapeshot
- 0.18 damage per pellet
- 3 pellets per cannon
- 0.58 range
- 0.92 reload
- 1.45 spread
- intended to pressure crew at close range, not kill hull efficiently

## Enemy pressure

Enemy broadsides retain their prototype base values but receive archetype-specific runtime equipment multipliers:

- Skirmisher: 2.4x damage, 1.8x reload duration
- Marauder: 2.8x damage, 1.9x reload duration
- Gunship: 3.2x damage, 2.0x reload duration

This keeps the fast/light enemies focused on subsystem harassment while making Gunships the meaningful hull threat.

## Reload HUD

Port and starboard reloads remain independent. A pair of circular reload dials is added around the bottom combat HUD:

- radial border fills clockwise during reload
- remaining seconds are shown in the medallion
- completed reload changes to HAZIR and receives a subtle pulse
- indicators hide while harbor modal UI is open

The existing BroadsideController reload timers remain the source of truth.

## Test checklist

1. Restart Play Mode so all runtime balance bootstraps are recreated.
2. Confirm current player hull reflects the full ship definition rather than the old ~5% prototype value.
3. Fight one Skirmisher, one Marauder and one Gunship with standard round shot.
4. Record approximate broadsides-to-sink for each target.
5. Verify chain shot damages sails noticeably but does not instantly erase them.
6. Verify grapeshot is poor at hull damage but degrades crew at close range.
7. Fire port and starboard independently and confirm their radial reload indicators progress independently.
8. Verify reload duration grows when crew readiness falls.

## Repair and subsystem follow-up

The first voyage report showed 80 Silver income, 21 ammunition replacement value
and 958 repair share, with hull at 4053/4960 and crew at zero.

New default harbor tariff:
- Hull: 0.025 Silver per missing HP (40 HP per Silver).
- Sail and crew: 0.10 Silver per missing integrity point.
- Round the combined quote upward once. An intact ship costs zero.
- New serialized field names intentionally discard the old 1 Silver/HP prototype
  setting. Existing saves and wallet balances are unchanged.
- With 907 missing hull HP and 100 missing crew points, the quote is 33 Silver.
  Assuming an intact departure, that example yields 80 - 21 - 33 = +26 estimated net.
  Previously frozen reports are not recalculated; start a new voyage.

Subsystem pressure:
- Grapeshot crew damage: min(3, pellet hull damage * 0.12).
- Chain sail damage: min(8, projectile hull damage * 0.20).
- Six 6 lb cannons, all 18 grapeshot pellets hitting: 33.05 crew points
  (previously 151.47); six 12 lb cannons: 48.60 points without bonuses.
- Six perfect 6 lb chain hits: 48 sail points. Standard shot does not damage
  these subsystems. Hull damage and existing movement/reload penalties are unchanged.
- These are first-pass tuning values, not a guarantee for every ship/loadout.
  Additional cannons and multiple attackers still increase salvo pressure.

Validation: independent formula arithmetic checked intact repair, crew-only repair,
1000 HP repair, screenshot scenario, grapeshot pressure and chain cap.
Unity compilation and gameplay have not been run in this environment.

Play Mode acceptance:
1. Restart Play Mode and begin a fresh voyage; check both report and actual harbor quote.
2. Repair once: debit the quoted Silver and restore hull, sails and crew.
   Attempt repair again: no charge. Insufficient funds must not restore anything.
3. Fight the grapeshot enemy and compare crew loss after one and several salvos.
4. Verify chain still slows targets progressively and ordinary shot preserves crew/sails.
5. Repeat the same route with the same loadout; record income, damage, repair and net.
