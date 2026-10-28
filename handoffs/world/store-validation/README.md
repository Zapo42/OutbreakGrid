# NPC store-validation package

Download `OutbreakGrid-store-validation-efa3634.zip` and extract it. It contains the NPC handoff, four paused saves, inventory/assignment/route manifest, exact reference sources and geometry validation evidence. The same saves are directly available under `cases/` here.

Read `NPC_HANDOFF.md`. Load a `.obsave`, rather than its map-only counterpart, to retain assigned homes, staffed stores, shifts and inventories. Source baseline World19e18e7; fixtureefa3634; NPC0aa8ec34 plus the prior World schema14 reader. The ZIP includes the pinned source snapshot; do not overwrite newer NPC code with it.

**NPC behavioral validation has not run.** `DIAGNOSTIC_RESULTS.md` remains pending. The453 fixture assertions cover geometry, assignments and serialization, not NPC restocking, shopping, deliveries, daily survival or mid-action continuation. Known NPC137 LegacyUI failure is documented separately.

Both implementation commits are ancestors of this branch. Keep test runs and resulting saves local. Publication of this handoff does not merge the implementation into main.
