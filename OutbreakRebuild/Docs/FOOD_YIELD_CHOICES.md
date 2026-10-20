# Foundation 41: food yield choices

Hungry humans can make a small detour for more useful remembered food. Among reachable shelves with confirmed stock, a human compares walking distance with the number of meals it remembers being able to carry. Six meals and a thousand meals offer the same benefit to a human with three inventory slots. An available adjacent meal takes priority over walking to another shelf.

The prototype weights each walking step at three points and each additional portable meal at two points. Lower cost wins, then shorter routes, then stable coordinate order. With the three-meal inventory limit, three remembered meals can justify one extra walking step over a one-meal source. More distant stock does not justify a larger detour. The comparison also applies when selecting an alternative to a visibly busy shelf.

Reachability and guarded-route clearance still come first. Confirmed food still ranks above unopened shelves, which reveal no quantities. Hidden depletion does not alter a remembered amount until personally observed or reported through existing conversations. Unreachable or distant fallback targets keep their existing distance ordering. The new score does not change hunger/bravery risk tolerance, door actions, pickup limits or food sharing. Actual collection may differ from the estimate if stock changes or nearby hungry humans need a share.

A committed reachable trip stays committed; newly attractive stock does not make the human switch destinations mid-walk. Existing food targets and notebooks preserve the decision through saves. No new actor fields or random draws are introduced.

## Try it

1. Choose **Inspect Food Yield Choices Paused**.
2. Click **Step 1 tick**, then **Next hungry human**. The teal food target points to the shelf at **2,8**. It has six exposed meals, one extra walking step away compared with the one-meal shelf at **5,4**.
3. Click **Step 50 ticks**. The human has collected three meals and eaten one; two remain in inventory. The chosen shelf has three meals left, and the closer shelf still has one.

New encounters use encounter-prototype-36. Foundation 40 runs keep their previous choices until **Enable food yield choices** is clicked. Earlier runs retain their first missing feature's upgrade button. Upgrading keeps the run paused and preserves actors, clocks, terrain, inventory and random streams. The source save file changes only through an explicit save.

Automated checks execute production simulation and UI handlers with narrow Unity API doubles. Native Unity rendering and input still require local testing.

Validation: 237 simulation cases / 35,186 assertions; 605 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
