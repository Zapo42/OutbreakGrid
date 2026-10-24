# Foundation 111 — pedestrian street routes

Choose **Inspect Pedestrian Crossing Paused**, then Run or step 100 ticks. The selected off-duty courier leaves the distribution center, walks along the sidewalk to the crosswalk below the direct route, crosses both lanes and returns to the assigned home. Restart restores the initial paused setup. The two generated-city previews also use the new preference in new runs.

Safe, unconstrained walking plans on maps containing streets now favor sidewalks and crosswalks. Base movement costs remain 10 cardinal / 14 diagonal; undeveloped grass adds 8 per step, and an ordinary road tile adds 60. Diagonal steps across a road/crosswalk boundary add 20; cutting diagonally past a road corner adds another 60. Indoor floors and doors retain normal costs. These are planning weights, not energy charges or movement-speed changes.

This is a preference, not a crossing prohibition. A nearby crossing can justify a modest detour, but a missing, blocked or very distant crossing can lead to ordinary road crossing. Disconnected sidewalks and building entrances remain reachable through grass. Routine trips, shopping and commutes use the preference wherever they already request shelter routes. Random idle movement is not a sidewalk patrol system.

Threat-aware and confined-room routing retain existing behavior. Fleeing humans are free to take shortcuts under the existing danger rules. Walls, doors, corner clearance, occupancy, distribution access, finite inventory, real travel energy and resting remain active.

Assigned residents retain the bounded 256-cell extended search and 16,384-discovered-cell budget. Unassigned humans retain their 16-cell local range. The safe street preference also applies to short route queries, preventing the last few steps from reverting to an unweighted road shortcut. Safe assigned walkers also retain direct route scoring within the final 16 cells, including maps without streets. This removes the previous switch to local breadth-first routing, whose equal-length tie choices could produce an unnecessary sideways arc near the destination. No vehicles, traffic rules or road signals are introduced.

New runs use **encounter-prototype-89**. Saves through rules 88 retain their earlier route choices until explicitly upgraded with **Enable pedestrian street routes**, or the applicable earlier feature upgrade. No saved fields were added; route queries do not alter NPC knowledge, tick state or random streams.

The previous Foundation 110 long-courier acceptance trial remains pinned to rules 88 to preserve its original travel-timing scope. New checks cover the current physical crossing commute, save/load, short assigned/unassigned routes, unavailable crossings, threat-aware compatibility and prior rules. Native Unity 6000.6.3f1 rendering, layout and performance remain untested, per the pause after Foundation 82.

Next city milestones remain generated building lots with usable entrances and linked homes/workplaces. Future item categories are **supplies**, **medical**, **ammo**; this build keeps existing food-only storage and does not add weapon storage.

The reported Generated City pre-doorway deviation was reproduced with an approach from (60,14) to the store interior at (74,10). Rules 88 first arc outward to row 18 before returning to the door; current rules approach row 10 directly. A regression check covers this specific doorway as well as a straight final approach on a map without streets.
