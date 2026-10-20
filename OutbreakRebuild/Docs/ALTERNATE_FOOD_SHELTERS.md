# Foundation 40: alternate food shelters

An uninfected human aborting a food trip still prefers its original shelter. If that room is full, unreachable by a guarded local route, personally rejected or visibly unsafe, the forager can choose another reachable room with admission space. Alternatives use the shortest guarded route, with stable room ordering to break ties. Planning retains the existing 16-cell route radius and existing discovered-room limits; hidden zombies are not revealed by this choice.

The chosen room becomes the committed return destination. Recovering stress during the walk does not restart the abandoned food search. If that destination becomes unavailable during the return, the human reassesses available rooms even after calming down. If none qualifies, ordinary survival behavior remains available. No room capacity is overridden. Infection keeps its escape priority.

Entry and door closure use ordinary actions. On arrival, recovery and future hunger-driven departures resume. An outgoing doorway crossing remains a departure. Shelf stock and inventory change only through the existing physical food actions. Existing home and shelter fields save the return commitment without a new actor schema or random draw.

## Try it

1. Choose **Inspect Alternate Food Shelter Paused**.
2. Click **Step 1 tick**, then **Next hungry human**. The original room on the left has seven occupants and no admission space. The stressed forager chooses the room on the right; its blue marker shows the shelter target.
3. Click **Step 50 ticks**. The human is inside the right room, its door is closed, and the outdoor shelf still contains six meals.

New encounters use encounter-prototype-35. Earlier saves retain their prior behavior until explicitly upgraded. Foundation 39 runs show **Enable alternate shelter returns**; earlier runs show their first missing feature's upgrade button. Upgrading keeps the run paused and preserves actors, clocks, inventory, terrain and random state. The original file stays unchanged until an explicit save.

Automated checks execute production simulation and UI handlers with narrow Unity API doubles. Native Unity rendering and input still require local testing.

Validation: 232 simulation cases / 35,175 assertions; 594 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
