# Foundation 31: door movement noise

Human door opening and closing emits one quiet cue from the doorway. Zombies within four cardinal sound-path cells can hear it, using the same wall-blocking geometry as shelf searches. Only an actual state change emits a cue; repeatedly checking an already closed door does not renew hearing. Initial map setup and repairs do not create movement sounds. Door breaking retains its existing louder impact behavior.

A heard door movement lasts 25 ticks (five simulation seconds). The zombie receives a doorway location and source kind, without a human identity or live position. Sight and remembered visual pursuit still take priority; otherwise the newest search, movement, warning or impact guides investigation. Sound-only zombies cannot break closed doors. Hearing clears on arrival or exact expiry and persists through saves.

Choose **Inspect Door Noise Paused**. **Step 1 tick** closes the human's nearby door. Select zombie #2 and **Step 10 ticks**: it approaches the closed door without seeing the human or damaging the door. Its inspector identifies door movement rather than a shelf search.

New encounters use encounter-prototype-30. Earlier saves retain quiet door movement until **Enable door movement noise** (or an earlier-feature upgrade button) is clicked. Existing shelf-search hearing defaults to source kind zero, preserving older saved continuation.

These noises are simulation cues for AI hearing. Native Unity rendering and input require local testing; automated UI checks use narrow API doubles with actual production handlers.

Validation: 211 simulation cases / 35,098 assertions; 524 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
