# Sailing feel pass — 2026-09-23

## Scope

First implementation package from playtest feedback: non-blocking wildlife, ship contact recovery,
visible ocean edges, and less text in the sailing HUD. No economy or save format changes.

## Changes

- Hunt capsule colliders are triggers. HarpoonProjectile already sweeps with QueryTriggerInteraction.Collide,
  so they remain harpoon targets. Regular wildlife briefly dips its visual and turns away on ship overlap.
  The target stays at its original height. Externally controlled boss attacks keep their own movement/damage.
- ShipContactResponse attaches to player and enemy navigation bodies. Non-trigger hull colliders have zero
  friction/bounce. After navigation, recent horizontal contacts remove inward velocity while preserving
  tangential/vertical travel; ship contact receives a capped 0.65 m/s outward velocity, not a stacking impulse.
  Floor contacts do not participate. Contact memory expires after 1.5 fixed steps.
- Player contact steering has a 40% authority floor; the previous contact steering multiplier, all-axis
  planar slowdown and boosted lateral braking are removed. Water resistance still applies outside contact.
- Enemy combat hold still issues no travel orders and realigns as before. X/Z physics freezes are removed,
  permitting contact displacement; after separation it holds the new position instead of snapping back.
- Regional atmosphere builds an eight-triangle visual ring around the original Ocean renderer. The ring
  shares its current regional material, adds no collider, and extends beyond the camera's far clip distance
  plus the map edge. The world-space water shader keeps the same wave scale. Playable bounds stay ±82.
- Gateway mist is lighter with a clear center. The smaller destination card replaces the regular region
  heading while visible. The target panel shifts down to avoid overlap. There is still explicit confirmation,
  no countdown; level gates and input guards remain.
- HUD defaults to icons/counts, selected ammo, readiness dials and short sailing order. Item usage appears
  when hovering hotbar cells. F1 toggles names/help/details; I opens the existing inventory. Material breakdown
  appears over resources or with F1. Important pirate-wreck return guidance remains visible.
- Hull speed-cap changes produce short notifications; the persistent cap explanation is in F1 details.
  The actual sailing order/max remains visible. Docking, active repair and harbor weapon lock are retained.

## Verification status

C# syntax parsing and source/diff checks are performed outside Unity. This is not a Unity compilation,
physics simulation, or visual approval. No Unity executable or C# compiler is available in the editing environment.

Added menu: **Seaborn > Validation > Check Sailing Feel**. Edit Mode checks contact velocity invariants;
Play Mode additionally checks actual ship colliders/constraints, hunt targets and ocean extension.
This menu has not been executed in the editing environment. It does not change saves or award resources.

## Playtest acceptance

1. Sail straight through a normal hunt target at slow/full speed; no stop or collision shove. It briefly
   dips/turns away and remains harpoonable. Kill it, wait for respawn and repeat. Confirm Stormjaw attacks
   still deal their existing scripted damage.
2. Meet a stationary combat ship bow-on. Hold A/D or reverse: the player can disengage. Check glancing
   contact, two moving civilians and a crowded pair; no accelerating rebound, jitter lock, or pass-through.
   Enemy combat hold must not resume circling merely because it was displaced. Check shore/pier collisions too.
3. At maximum zoom visit all edges/corners in harbor, central, west and east maps. Water must cover the view;
   no grey triangular gap, seam, changed wave size or expanded traversable bounds. Repeat after map travel.
4. Approach, cancel and confirm each gateway. One top heading at a time, target panel below it, mist fades
   after departure. Verify blocked-map messages and no click-through shot on confirmation/cancel.
5. At 1280×720, 1920×1080 and an ultrawide resolution: check hotbar hover, F1 on/off, Shift-harpoon aiming,
   resource hover, damage-cap notification, repair, docking and returning from the shipyard. Dials and
   inventory shortcuts remain usable. No text should cover the target or transition buttons.

## Next design work (not implemented here)

- Deliberate bow ramming: speed/angle/mass, reciprocal damage, one impact per separation cycle.
- Coastal harbor and useful geography: coves, islands, routes, choke points.
- Progressively harder regions with their own safe ports and distinctive rewards.
- Sail cosmetics separate from equipment stats/upgrade level, including flags and patterns;
  assets need isolated sail material/UV support.
- Personal home port, depot/drydock progression and missions for reserve ships; complement active sailing.
