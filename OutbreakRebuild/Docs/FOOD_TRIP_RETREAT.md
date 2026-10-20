# Foundation 39: interrupted food trips

Hungry uninfected humans on a food trip can return to their original shelter when perceived risk exceeds their hunger/bravery tolerance. The home must still exist, have admission space, have no personally known internal danger, not be under a remembered rejection, and have a route using the guarded-food clearance rules.

Starting the return clears the active food target, partial search and departure phase, without changing shelf stock or inventory. The existing home and shelter fields preserve the committed return through saves; falling stress during the walk does not restart the abandoned search. Doors still open, cross and close through ordinary actions. After reaching home, normal recovery and future hunger-driven departures resume. If home or its route becomes unsafe, ordinary shelter and survival behavior remains available. Infection keeps its existing escape priority.

Choose **Inspect Food Trip Retreat Paused → Step 1 tick → Next hungry human**. The stressed forager abandons the nearby shelf; the blue marker identifies its original room at 2,2. **Step 50 ticks** shows it inside with the door closed and the outdoor shelf untouched. The fixture uses lingering stress rather than a currently visible pursuer, so the return itself can be inspected clearly.

New encounters use encounter-prototype-34. Earlier saves retain their prior response until **Enable food trip retreat** (or an earlier-feature upgrade button) is clicked. No new saved actor fields or food schema are introduced.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 227 simulation cases / 35,162 assertions; 583 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
