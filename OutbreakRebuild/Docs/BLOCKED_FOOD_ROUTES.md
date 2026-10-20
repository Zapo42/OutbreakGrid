# Foundation 29: blocked food routes

When a hungry human cannot find a usable route to a chosen shelf, it remembers that last failed destination for 50 ticks (ten simulation seconds), clears its active search target and spends the action reassessing. On later opportunities it can choose another known shelf. A failed route never marks the shelf empty or changes its stock or search state.

The rejection is personal and applies only to food searching. Sight, warnings and food reports do not erase it. It expires exactly on the simulation clock, allowing a retry after a door opens or congestion clears. Only the last failed destination is retained; another failure replaces it. If no other supplies are available, ordinary shelter/survival behavior continues during the wait. Rejected room supplies do not keep hungry occupants waiting indefinitely inside a room.

Choose **Inspect Blocked Food Route Paused**. Human #1 remembers food beyond a solid partition. **Step 1 tick** records the failed route. **Step 50 ticks** shows it collect and eat from the reachable shelf instead. The inspector shows the rejected shelf and remaining retry time while that memory is active.

New encounters use encounter-prototype-28. Older saves retain prior behavior until **Enable food route memory** (or an earlier-feature upgrade button) is clicked. Rejection state is optional in saves, validates on restore and clears on conversion.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 205 simulation cases / 35,082 assertions; 514 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
