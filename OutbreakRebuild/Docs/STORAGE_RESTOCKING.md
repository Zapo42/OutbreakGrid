# Foundation 87: shelter restocking trips

Fed, calm shelter occupants can now collect food specifically to refill a storage box. The provisional reserve target is **six meals per box**, capped by its capacity. A box's capacity remains 30 items by default; reserve target and capacity are different limits. [Foundation 88](STORAGE_RESERVE_TARGETS.md) adds per-box configurable targets.

## Starting a trip

A human must be inside a shelter with a visible box below its reserve target, know a reachable shelf outside that room, have at least 60 energy, be uninfected and at least 50 health, and have hunger below 40. Visible danger, stress, unresolved freezing or recent warning/impact memories prevent departure. The human must have room to collect food; carried surplus is handled by existing sharing and storage behavior first.

A saved claim allows one active restocker per box. Other boxes can have their own restockers. The claim prevents duplicate assignments; it does not disclose zombie positions or unopened shelf contents.

## Collecting and returning

The restocker uses existing calm waits, doorway inspection and physical crossing. Opening a door and seeing a zombie cancels the exit attempt. Walking costs normal energy, and searching and pickup consume normal actions. Shelf choice uses personal shelf memory: confirmed food first, then known unsearched shelves. No undiscovered shelves or hidden stock quantities guide the trip.

After collecting, the human returns to the original room and deposits into the assigned box, keeping the existing personal reserve. Ordinary eating takes priority if hunger becomes significant. Collected items remain real carried inventory through interruptions and saves; nothing is teleported or created. Repeated trips stop starting when the six-meal reserve is met or no suitable known shelf remains.

Stress, observed danger, significant hunger, or energy falling to 40 ends optional collection and requests a return. If the home cannot be safely reached or admitted, the claim is released and normal survival decisions resume. Infection and conversion release claims. A 600-tick task deadline also releases abandoned claims. Existing hungry humans continue their personal food behavior independently.

This increment restocks shelter boxes from remembered, locally reachable shelves outside that room. It does not discover unknown supplies, transport between buildings' boxes, or assign arbitrary buildings as home bases.

## Inspect it

Choose **Inspect Restocking Paused**. The human starts fed with one meal, an empty shelter box and a remembered outdoor shelf containing twelve meals. Run or use tick batches to observe checking the exit, collecting, returning and stocking six meals over repeated trips. Select the human for **Restocking: Collecting / Returning**, or **Next storage box** for its current inventory and reserve target. The food overview counts active restocking trips. Restart restores original supplies and clears tasks.

## Compatibility and validation

Existing Foundation 86 runs retain local box transfers until **Enable restocking trips** is selected. Rules are now `encounter-prototype-69`; generation 68 is retained as `StorageVersion`. Optional actor fields persist task phase, box, home and timeout. Map schema stays unchanged. Older builds cannot resume an active restocking save.

Automated checks cover the complete round trip and six-meal target, food conservation, save/resume during collection and return, assignment eligibility, one claim per box, unopened stock privacy, hunger/stress/energy interruptions, empty stale shelves, infection/timeout cleanup, malformed task rejection, old rules, UI state, upgrade and restart. Existing room exploration and food behavior regression checks also pass.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
