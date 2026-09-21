# NPC pursuit and broadside recovery

## Changes

Fishing boats now move at 2.5 units/s (was 3.8), merchants at 1.8 (was 2.8). Civilian turning needs forward motion: nominal rates are 24/18 degrees/s, with authority from 12% at rest to 65% at full speed. Actual healthy maximum turns are therefore 15.6/11.7 degrees/s, instead of 55/35. Subsystem penalties still apply. Escape courses last three seconds unless a boundary requires an earlier change.

Warships hold position while their broadside has a firing solution. If the player leaves the arc for 0.9s, the ship unlocks yaw and realigns in place. Alignment follows the current target bearing and stops with a small angular margin. This does not reintroduce orbiting. Crossing the actual firing range for 1.5s resumes approach; the former one-unit non-firing idle band is removed. A rejected predicted shot discards its stale aim and starts preparation again.

## Unity checks still required

- Shoot a fishing boat and a merchant. From a healthy player ship at full ahead, verify they can be pursued, turn gradually and do not instantly mirror every steering change. A damaged player ship may still be slower.
- Drive either civilian toward each map boundary/corner. It must remain inside the map and choose a new course without spinning in place.
- Fight each pirate archetype. After it stops broadside-on, move toward its bow/stern while staying in range. After the short grace period it must turn in place and resume preparation/fire.
- Briefly cross the firing arc edge and return: it should retain its position rather than continuously chasing the bearing.
- Remain just outside actual firing range for over 1.5 seconds. The ship must approach again. Re-enter range and confirm the existing hold/fire cycle resumes.
- Move fast across the locked prediction immediately before a shot: a rejected salvo must not leave it stuck forever with a full preparation indicator.
- Disengage, conceal, sink and respawn an enemy: movement constraints and arc/escape timers must reset correctly.

No Unity Editor is available in the coding environment; these are acceptance checks, not claimed runtime results. Civilian ships remain unarmed and flee; warships realign to fight.
