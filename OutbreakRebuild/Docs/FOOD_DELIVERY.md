# Foundation 25: food delivery

A fed, uninfected shelter occupant carrying at least two meals can approach a visible hungry roommate who has no food. The donor keeps a reserve, chooses the hungriest reachable eligible person, and resolves equal need by stable actor identity. Adjacent sharing still happens first; delivery has priority over pantry storage.

Approach routes remain inside the assigned room, respect furniture and collision capacity, and use the bounded local pathfinder. Recipients must also be inside that shelter, visible and free of infection, recent warning/impact cues, visible zombies, freezing and active fear reactions. The donor rechecks its own hunger and safety each action. It can change recipients when their needs change; it has no knowledge of hidden humans.

Walking, giving one actual meal and eating are separate actions. Existing sharing transfers the food only after contact. When no one needs help, remaining surplus can go into the pantry. Saving mid-trip resumes without extra task state or random draws.

Choose **Inspect Food Delivery Paused**, then **Step 50 ticks**. Human #1 approaches hungry human #2, gives it a meal, and stores remaining surplus while keeping a reserve.

New encounters use encounter-prototype-24. Previous saves keep their behavior until explicitly upgraded with **Enable food delivery** (or the earlier-feature upgrade button). Native Unity visuals and input require local testing; automated UI checks use narrow API doubles with actual production handlers.

Validation: 193 simulation cases / 35,042 assertions; 489 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
