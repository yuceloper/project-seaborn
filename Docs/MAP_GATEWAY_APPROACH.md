# Confirmed map travel

Approaching a valid exit within 24 world units fades in animated mist and a
card naming the destination. Within 6 units, HARİTAYA GEÇ is immediately enabled
for an unlocked map. There is no countdown or automatic travel.

VAZGEÇ dismisses the card until leaving the approach or changing exits.
Confirmation rechecks destination, distance, health and captain unlock. Level
locks, entry placement and existing scene fade are preserved. Player bounds
remain ±82 on X/Z; controls stay live while deciding. Sinking and scene changes
clear the pending approach. Card clicks cannot fire cannons/harpoons.

Unity acceptance: check instant confirmation at each exit, locked destinations,
retreat, cancellation/re-entry, death, corners, repeated round trips and click
protection. Mist rendering and physics need a Unity playtest.
