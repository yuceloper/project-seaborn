# One-click expedition preparation

At the trade station, HAZIRLIK now quotes full hull/subsystem repair and the
packets needed to reach 120 standard, 12 chain, 16 grapeshot and 10 of the selected
harpoon. It displays actual added amounts, per-line prices, total price, missing
Silver or the balance after purchase. Whole catalog packets are purchased, so
stock can exceed a target. Surplus is never removed and creates no further cost.
The selected harpoon name is explicit; the other harpoon stock is untouched.
Supplies/consumables are not included in this action.

ONAR VE İKMAL ET validates the displayed quote again, requires protected harbor
trade docking, checks all dependencies, and debits the complete total once before
applying repairs and stock additions. Insufficient Silver changes nothing. A
changed quote returns a refresh result without spending; a ready ship cannot be
charged again. Reentrant service purchases are rejected during preparation.
The existing individual repair/restock buttons remain available. This action
does not launch an expedition or require a contract.

## Checks

Seaborn > Validation > Check Harbor Preparation Quotes tests the real pure quote
calculator: whole-packet rounding, empty/exact/above-target stocks, total costs,
free bundles, invalid packet sizes and stale repair/stock/harpoon quotes.
This command does not read or write saves.

Unity integration playtest:

1. With stocks 99/9/13, six light harpoons and a 13 Silver repair quote, expect
   additions 40/12/16/10 and a total of 84 Silver at current catalog prices.
2. With 83 Silver the button is disabled; calling the service also rejects it.
   Health, subsystems, all stocks and wallet must stay unchanged.
3. With 84 Silver, buy once: wallet becomes zero, hull/subsystems are restored,
   stocks become 139/21/29/16. Repeated purchase is a no-op.
4. Change the selected harpoon, stock or repair cost between quote and purchase:
   reject the stale quote and refresh without charging.
5. Test while undocked, outside harbor and at the shipyard: reject the combined
   action. Return to trade docking and verify it becomes available again.
6. Test healthy/full-stock ships, subsystem-only damage, and surplus stocks.
7. Save/relaunch after preparation and check normal existing state persistence.
8. Inspect the breakdown and button at the normal game resolution; confirm no
   text overlaps the existing individual service rows or departure hint.

Static C# syntax/source review completed outside Unity; Unity compilation,
execution of the validation command and rendered UI are still pending.
