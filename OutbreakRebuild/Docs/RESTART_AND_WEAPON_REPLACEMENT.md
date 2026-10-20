# Foundation 53: restart scenario and replace worn bats

**Restart scenario** is now in the simulation toolbar beside **Main Menu**. One click restores the scenario's starting state and pauses it. It resets playback to 1x, clears tick debt and old visual feedback, and resets the camera and selection. There is no replacement confirmation for this explicit restart action. Save a run first if you want to preserve its current progress. Existing save files are not changed.

Restart uses the actual starting snapshot: the same map, bodies, seed/random states, food, equipment and condition. Editing the setup form later does not change the active scenario's restart. Rules upgrades enabled during the run remain enabled after restarting.

Manual bodies and supplies placed before the first tick become part of the starting setup. Later edits are removed by restart. For a loaded run, the starting snapshot is its loaded checkpoint, so restart returns to that saved tick rather than inventing an unknown original scenario. Diagnostic scenarios that intentionally start at a later tick likewise return to their initial diagnostic state. The old **New movement test** control remains available in the inspector, leaving room for restart in the toolbar.

## Next combat step: proactive replacement

Calm humans can replace a bat with three or fewer successful hits remaining when a better bat is visible at their feet or in a cardinal neighboring cell. The replacement must offer at least three additional hits. They choose the best condition first, then nearer cells and stable coordinate order. This milestone does not add longer weapon-seeking trips for already armed humans.

A swap consumes one normal action. The human picks up exactly one bat and drops its used bat at its actual cell. No equipment is repaired, destroyed or duplicated. Mixed ground piles preserve every member's condition, and the human retains skill, practice and inventory. The new bat uses the existing pickup animation and the inspector briefly says **Bat replaced**.

Replacement yields to food and shelter actions. It does not happen during visible danger, infection, flight, freezing, stress, recent warnings/door impacts or swing recovery. Closed doors block collection. A bat exchanged while standing in an open doorway remains on that doorway with its condition intact.

## Try it

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm **Foundation 53 / restart and weapon replacement** appears on the menu.
3. Start any configured encounter, advance some ticks, then click **Restart scenario** beside **Main Menu**. The same starting encounter returns, paused. Click **Run** to replay it.
4. Choose **Inspect Weapon Replacement Paused**. The human starts with **2/12 hits remaining** and a fresh bat at its feet.
5. **Step 1 tick**. The inspector shows **12/12 hits remaining** and **Bat replaced**. The used bat stays on the ground with two hits left. **Restart scenario** restores the same original pair of bats for another test.

Restart works with older rules too. New simulations use encounter-prototype-46. Existing Foundation 52 runs retain encounter-prototype-45 without proactive swaps until **Enable weapon replacement** is clicked. The upgrade preserves saved state and leaves the run paused; save under a new name to preserve the older file. Restart remembers the enabled generation. No new save fields or map schema changes are required for this milestone.

Validation: 298 simulation/presentation cases / 320,823 assertions; 815 legacy-input UI assertions and 3 NewOnly assertions. Checks cover exact configured restarts, repeated deterministic replay, changed setup fields, loaded checkpoints, feedback reset, preserved upgrades, pre-tick manual setup and removal of later edits, finite swaps, mixed-pile condition, safe eligibility, food priority, closed doors, doorway drops, older rules and inspector refresh. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles; native Unity appearance and input require local testing. No Windows executable was built here.
