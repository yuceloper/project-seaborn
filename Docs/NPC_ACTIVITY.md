# NPC activity and ammunition fallback

The target panel now reports actual controller activity: sailing, fleeing (civilian), searching, approaching, aligning, aiming, reloading with remaining seconds, preparation, or empty ammunition. Civilian escape and ammunition exhaustion use amber instead of the combat red.

Warships keep their selected ammunition while stock remains. Once empty, they switch to an available standard, chain, or grapeshot stock, in that order. Preparation resets when switching. Selection does not grant ammunition or reset either broadside's reload timer. The new ammunition's effective range is used in the same navigation tick.

With no ammunition of any type, a warship releases its position lock and follows the existing bounded escape logic. After losing the target it resumes patrol instead of chasing the last seen position. If supplied again while engaged, it can resume approach. Respawn/passive reset clears withdrawal state.

Validation here: both changed C# files parse; inspected BroadsideController.TrySelectAmmunition to verify stock and reload timers are preserved. No Unity runtime validation was possible.

Unity acceptance:
- Attack a civilian: panel says KAÇIYOR, not fighting/aiming.
- Fight a pirate and move across its broadside: panel alternates approach, alignment, aim and reload states in line with its movement; reload countdown belongs to the side facing the player.
- Use the Inspector to exhaust only its selected stock: another stocked type is selected, existing reload finishes normally, and no ammunition is created.
- Exhaust all three types while the ship holds position: it unlocks movement, retreats inside map bounds, and shows CEPHANE YOK. Disengage: it should patrol again.
- Sink/respawn it: restored stock permits fighting and no withdrawal flag persists.
