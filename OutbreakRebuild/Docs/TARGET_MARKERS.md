# Foundation 37: selected target markers

Selecting a body now outlines its active saved target cell on the map:

- Teal: a human's food destination.
- Blue: a human's shelter destination while seeking entry.
- Red: a zombie's last observed human location.
- Amber: a zombie's heard search, door movement, warning or impact source.

The inspector identifies the target and coordinates. Zombie markers follow the simulation's sight-memory and newest-sound priorities. They show remembered locations, without looking up a hidden human's live position. Human objectives may remain marked while the trip is temporarily paused for risk. Completed or expired targets disappear. Off-screen targets keep their inspector label and are culled from drawing.

**Hide target markers / Show target markers** toggles the outlines without advancing time or changing the run. The outlines leave map clicks and ordinary inspection working, and show a destination rather than a predicted path through walls.

Test: **Inspect Food Route Choices Paused → Step 1 tick → Next hungry human**. The chosen shelf at 3,7 has a teal outline. Hide/show markers, then **Step 50 ticks** to finish the trip and clear it. For sound, use **Inspect Door Noise Paused → Step 1 tick**, then select zombie #2 to see the amber door marker.

Existing saves need no upgrade; simulation rules remain encounter-prototype-32. Native Unity appearance and input require local testing. Automated UI checks exercise actual target labels, draw colors, toggling, unchanged simulation state and stale-marker cleanup using narrow Unity API doubles.

Validation: 571 legacy-input UI assertions and 3 NewOnly assertions; project configuration and whitespace checks passed. Simulation code is unchanged.
