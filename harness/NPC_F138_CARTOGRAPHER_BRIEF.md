# NPC-F138 — message for the lead cartographer

Please provide a small representative city map/save with assigned homes and food boxes, staffed stores with shelves/crates/phones, a distribution center with couriers, connected pedestrian routes, short and longer trips, and one identifiable park.

Return the local map/save path, seed, building/NPC assignments, intended routes and known blocked areas. State the source revision and rules generation. Do not silently upgrade or regenerate an existing map. Coordinate shared-file ownership first: cartographer owns map/world authoring, NPC agent owns behavior diagnostics/tests. Keep all work local; lead designer handles GitHub.

The NPC runner will monitor work attendance, shopping and physical supply transfers, optional leisure, return home, hunger/energy, repeated doorway crossings and prolonged task inactivity. These are diagnostic signals, not automatic proof of bugs: legitimate rest, work, supply exhaustion and queues must be distinguished. Optional leisure is not mandatory for every actor/day.

Pending: user relay and cartographer response. No direct agent contact is available. Existing controlled maps and generated-city smoke runs are the interim baseline; acceptance of a cartographer-supplied integrated map is not yet complete.


## NPC-F139 investigation update for relay

The original generated day was reproduced exactly. The remaining24meals were20at the distribution center and2in each clerk's home; stores were empty. Controlled copies identify hunger blocking the explicit off-duty commute and return travel preempting consumption of carried food. Do not assume these are disconnected roads, and do not silently edit the baseline to hide them.

The78-meal starting budget is also below the minimum80needed to keep all10NPCs below max hunger for24hours before recovery costs. When preparing your map, label whether it intentionally tests scarcity or a well-supplied daily routine. Provide intended shifts, routes and finite stock totals; preserve the original scarce baseline as a comparison. NPC priority repair is a separate increment, not a request for you to alter NPC code. The user has already authorized running your package when it arrives.
