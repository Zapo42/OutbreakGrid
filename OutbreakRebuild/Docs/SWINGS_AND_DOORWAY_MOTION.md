# Foundation 45: visible bat actions and continuous doorway movement

Bat pickups now lift the ground item into the hand over three simulation ticks. Equipped bats follow movement facing. Each actual swing produces a five-tick sweep and return, a short tip trail, and a gold contact spark for a hit. Misses still animate their swings without a hit spark. These are procedural debug visuals; directional character and weapon sprite animation remains future work.

The animation is driven by transient per-tick pickup and swing notifications from the existing simulation actions. Swing notifications retain the actual pre-knockback contact location and hit/miss result. Reading or drawing notifications does not advance the random stream or change saved state. Presentation retains only one current weapon action per actor, and loading resets it rather than replaying an old attack.

## Doorway fix

Previously, any door opening, closing or breaking reset every actor's interpolated movement. That made an entering human jump across its doorway and could also snap an unrelated zombie halfway through a step. Terrain changes now refresh the terrain drawing while preserving safe movement segments.

A door that has logically closed stays visually open underneath a body that is still interpolating through it. Once the rendered body clears the doorway, the closed appearance becomes visible. Damage tint is preserved. This visual hold does not reopen the door in the simulation or alter its collision timing. Paused/manual inspection still uses exact committed body positions.

## Try it

- Choose **Inspect Blunt Combat Paused**, optionally select **0.5x**, then click **Run**. Watch the pickup and the three swings. Large paused tick batches jump to the final state, so they do not play intermediate animation frames.
- Choose **Inspect Food Trip Retreat Paused → Run** to watch the human enter and the door close after its rendered body clears.
- Choose **Inspect Door Breach Paused → Run** to watch zombie movement through a broken doorway.

Foundation 44 saves need no behavior upgrade. Saved rules remain encounter-prototype-38. Damage, accuracy, attack cadence and door actions continue through the existing simulation rules; the new notifications and animation poses are not saved.

Validation: 264 simulation/presentation cases / 317,605 assertions; 662 legacy-input UI assertions and 3 NewOnly assertions. The runtime UI checks reproduce a closing door with a human entering and a zombie already mid-step, verify continuous movement, confirm temporary visual door opening and subsequent closure, and check that bat-tip drawing changes with its swing trail and hit spark. Project configuration and whitespace checks passed.

These checks execute production drawing handlers with narrow Unity API doubles. Actual appearance, native input and Unity rendering still require local testing.
