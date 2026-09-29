# Hull speed limits and sail handling

Player ShipMotor now limits forward orders using current / maximum hull HP:

| Hull | Maximum order |
| --- | --- |
| 50% or above | 3 |
| 20% to below 50% | 2 |
| Below 20% | 1 |

Orders are clamped in Update and before physics movement, even without input.
Existing deceleration lowers speed; the collision velocity clamp is unchanged.
Stop and reverse remain available. Repair unlocks orders without automatically
increasing the selected order. Held throttle still requires a fresh press.

Chain sail damage is now min(4, hull hit amount * 0.10), half the previous
min(8, amount * 0.20). For the player, sail damage no longer reduces top speed:
at zero sail integrity it reduces acceleration by 20% and turning by 15%.
Braking is unaffected by sail integrity. NPC sail speed reduction is retained
so chain ammunition remains useful during pursuit; their turning floor is now
85% as well. Crew damage and reload penalties are unchanged.

The HUD shows the hull speed cap above the helm, including during field repairs.

## Unity acceptance checks

- With a 2000 HP starter ship, verify max order 3 at 1000 HP, 2 at 999 HP,
  2 at 400 HP, and 1 at 399 HP.
- Cross each threshold at full speed without pressing movement keys: the order
  falls immediately while forward speed decreases smoothly.
- Verify stop/reverse still work at low hull; repeated forward presses cannot
  exceed the cap. Test both keyboard and the configured input action.
- Repair above each threshold: cap text updates/disappears, current order stays
  unchanged, and a fresh forward press can select the unlocked order.
- At full hull and zero sails, verify full speed remains reachable, acceleration
  and turning are mildly reduced, braking remains normal, and sail restoration
  removes the handling penalties.
- Check cap text above the helm at the normal game resolution and during repair.

Static C# parsing and threshold arithmetic can be checked outside Unity;
compilation, actual physics feel, and HUD layout require a Unity playtest.
