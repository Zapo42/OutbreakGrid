# Foundation 38: guarded food routes

Food-route planning now excludes cells within two grid steps in either axis of observed zombie threats, matching the existing immediate-contact cutoff for collecting or eating food. This applies to route comparisons, chosen approaches, search alternatives and remembered-route waypoints. Shelter entry, ordinary retreat, fighting, delivery and pantry behavior keep their existing routing rules.

The route planner uses current visible threats and the existing personal avoidance memory when no zombie is visible. Hidden zombie positions do not create a new danger map. Existing collision restrictions remain in effect. A committed food destination is reconsidered if its route becomes unsafe; partial work cannot override immediate danger. If every known route fails, ordinary blocked-route memory and survival behavior apply without changing shelf stock or marking it empty.

Choose **Inspect Guarded Food Paused → Step 1 tick → Next hungry human**. The near shelf at 6,3 is guarded; the teal marker instead identifies 3,8. Continue stepping to watch the active zombie approach: survival may interrupt the supply trip. This fixture demonstrates route choice, without guaranteeing a completed meal while a zombie is actively pursuing.

New encounters use encounter-prototype-33. Earlier saves retain their previous clearance until **Enable guarded food routes** (or an earlier-feature upgrade button) is clicked. No new actor state or stock schema is introduced.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 223 simulation cases / 35,149 assertions; 576 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
