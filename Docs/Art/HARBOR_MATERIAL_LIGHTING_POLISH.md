# Harbor Material & Lighting Polish

This pass addresses the accepted harbor capture after the silhouette pass.

## Goals
- separate civic, trade and shipyard structures through distinct palettes
- push roofs toward dark slate/teal instead of flat brown
- give windows a cool reflective read
- warm stone/quay surfaces slightly without touching ocean rendering
- add three low-cost warm light pools at the harbor office, trade loading face and shipyard work area

## Scope
Presentation only. No docking positions, triggers, safe-zone rules, combat, controls, economy or HUD behavior are changed.

## Validation
Run the harbor scene from a fresh Play Mode session. Confirm the three main building groups are readable as separate materials from the tactical camera, roofs no longer merge into wall color, and the warm pools are subtle rather than visibly emissive. Windows should read cooler than plaster/timber. Check both Editor Game view and Windows build before merge.
