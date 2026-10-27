# NPC-F138 — message for the lead cartographer

Please provide a small representative city map/save with assigned homes and food boxes, staffed stores with shelves/crates/phones, a distribution center with couriers, connected pedestrian routes, short and longer trips, and one identifiable park.

Return the local map/save path, seed, building/NPC assignments, intended routes and known blocked areas. State the source revision and rules generation. Do not silently upgrade or regenerate an existing map. Coordinate shared-file ownership first: cartographer owns map/world authoring, NPC agent owns behavior diagnostics/tests. Keep all work local; lead designer handles GitHub.

The NPC runner will monitor work attendance, shopping and physical supply transfers, optional leisure, return home, hunger/energy, repeated doorway crossings and prolonged task inactivity. These are diagnostic signals, not automatic proof of bugs: legitimate rest, work, supply exhaustion and queues must be distinguished. Optional leisure is not mandatory for every actor/day.

Pending: user relay and cartographer response. No direct agent contact is available. Existing controlled maps and generated-city smoke runs are the interim baseline; acceptance of a cartographer-supplied integrated map is not yet complete.
