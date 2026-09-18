# Combat Balance Pass — September 2026

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
