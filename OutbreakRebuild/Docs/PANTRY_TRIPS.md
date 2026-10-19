# Foundation 24: pantry trips

Fed, uninfected shelter occupants carrying spare meals can walk to a shelf and deposit supplies, keeping one meal. Directly helping an adjacent hungry person still takes priority. Nearby shelves are preferred by reachable path length, with stable cell order resolving ties.

Only shelves currently visible or in the human's personal notebook are considered. Routes stay on interior cells of the assigned room, respect furniture, collision capacity and corners, and use the existing bounded local pathfinder. They cannot go outside or open doors to reach a pantry. Each action rechecks hunger, visible threats, warnings, impacts and fear before continuing. No extra task state or random draws are introduced.

Choose **Inspect Pantry Trip Paused**, select human #1 and step **50 ticks**. It walks from the far end of the room to the empty shelf, deposits two meals and retains one. Saving mid-trip resumes deterministically.

New encounters use encounter-prototype-23. Existing Foundation 23 saves keep adjacent-only storage until **Enable pantry trips** is clicked. Older feature generations also remain unchanged until explicitly upgraded.

Native Unity visuals and input still need local testing; automated checks compile and exercise actual simulation and UI code with narrow Unity API doubles.

Validation: 191 simulation cases / 34,989 assertions; 485 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
