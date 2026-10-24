# Foundation 94 — assigned couriers and houses

Humans can now have a persistent assigned home house, separate from the house receiving their current delivery. A courier has one source box and up to eight distinct destination boxes. Each box belongs to the existing discovered room/house that contains it. Source and destinations may share a house; a house may contain several destinations. These remain independent inventories.

## Set up a courier

1. Select a living, uninfected human inside a house. Click **Assign current house**.
2. Click **Edit courier assignment**. Select a storage box, then click **Use as courier source**. The source must be inside a discovered house.
3. Select another indoor storage box and click **Add courier destination**. Repeat for each destination, in the order you want them served. Selecting an assigned box offers **Remove courier destination** instead.
4. Click **Finish courier editing** to return to the human inspector, then run or step the simulation.

While editing a courier, **Assign this box's house** sets their home to that box's house. It does not change their source or destination list. Editing pauses the simulation. **Clear courier route** removes the source and destinations while retaining their assigned home. Route and house edits are refused during an active collection/delivery; finish that task before editing.

If loading an older save, click **Enable courier assignments** first. The upgrade retains existing actors, inventories, energy, clocks and random state. Courier and house assignments, destination order, route cursor and active trips persist in saves. Restart restores the scenario checkpoint; assignments authored before its first tick are included in that checkpoint. Loading another world clears the UI's selected courier editor.

## Delivery behavior

- Couriers collect actual food from their assigned source box and carry it to one destination per load. They then return to the source for the next destination. This build supports multiple destinations across successive trips; splitting a single load among several stops is a future extension.
- Destinations rotate in assignment order, skipping remembered stocked/full boxes, reserve-zero boxes, claimed destinations, unsafe houses and destinations with no usable route. They aim to replenish the existing reserve targets. Other residents keep their automatic restocking behavior.
- Assigned addresses provide locations, not live stock knowledge. Unknown destination contents must be inspected; known observations retain their normal age and expiry. The player shortage overview is not fed into courier decisions.
- Donor reserve targets and **Block donations** still apply. Collected cargo stays with a courier if donations are blocked afterward. Inventories remain food-only, finite and capacity-limited. Couriers keep their personal meal reserve and may eat carried food when hungry.
- Couriers use ordinary movement, energy costs, rest, door opening, cautious departure and door closing. Work starts when fed, safe and sufficiently rested. Danger and infection take priority. Death or conversion clears human assignments.
- The permanent home is not overwritten by a delivery stop. Couriers return to their assigned house when no assigned delivery can start; they may rest in visited houses. A human with only a home assignment also returns there when safe and free of a restocking task, and can still perform ordinary shelf restocking in their home. Immediate survival and normal hunger behavior remain active.

Assigned humans use a bounded routing radius of 64 cells instead of the ordinary 16-cell shelter query. This supports separated near and far houses without teleporting or giving every human a world-sized path search. Routes requiring a larger search window or much longer detours remain outside this prototype's routing support. Assigned tasks have a longer saved timeout (3600 ticks) to accommodate travel, door checks and resting.

## Ready-made scene and validation

Open **Inspect Assigned Courier Paused**. The courier starts in the source house with a stocked source at **(4, 3)** and destinations at **(13, 3)** and **(42, 3)**. Run or step: the near box receives two meals, then the far box receives two meals, and the courier returns to its assigned house. Select the human to inspect their assignments. Each destination starts with a two-meal target; source food starts at 30 with its usual six-meal reserve.

Automated checks cover physical near/far deliveries and return home, exact saved continuation, separate boxes in one house, same-house transfers, hidden-stock privacy, defensive copies, editing restrictions, donation controls, malformed/legacy saves, conversion cleanup, and the UI assignment/upgrade/restart paths. Native Unity testing remains paused after Foundation 82; `foundation-82-unity-checkpoint` remains unchanged.

Foundation 95 adds [household rosters and resident reassignment](HOUSEHOLD_MANAGEMENT.md) while preserving courier source/destination routes.

Foundation 97 adds optional [daily courier work/rest schedules](COURIER_SCHEDULES.md), preserving active trips at shift end.
