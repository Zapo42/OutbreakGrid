# Foundation 26: shelf search noise

Searching a shelf emits one quiet noise from the searcher's cell, whether the shelf contains food or is empty. Nearby zombies can hear it up to four cardinal path cells away. Existing sound propagation applies: walls block sound, while doors, windows and fences transmit it. Carrying, eating, sharing and stocking shelves remain quiet.

A zombie remembers the source cell for 25 ticks (five simulation seconds). Sight and remembered visual pursuit take priority. Otherwise it investigates the newest heard search, warning or door impact, preferring a search on an exact deadline tie. It forgets a search on arrival or at the exact expiry tick and resumes normal behavior. Hearing supplies no human identity, live location or ability to attack a closed door.

Choose **Inspect Search Noise Paused**. Step **1 tick**, select zombie #2, then step **10 ticks**. The zombie hears a shelf search beyond the closed partition and approaches the door. Its inspector identifies the search location and remaining hearing time. The door stays closed and undamaged unless actual human sight and pursuit later justify an attack.

New encounters use encounter-prototype-25. Earlier saves retain quiet searches until explicitly upgraded with **Enable search noise** (or their earlier-feature upgrade button). Search hearing is persisted, validated and resumed exactly. Fields use optional zero defaults to preserve earlier save serialization.

Automated checks run production simulation and UI code. Native Unity rendering and input still require local testing.

Validation: 196 simulation cases / 35,052 assertions; 495 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
