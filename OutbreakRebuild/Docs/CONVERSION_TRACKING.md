# Foundation 63 / conversion tracking

The encounter summary now shows **New conversions**: humans who have turned into zombies since starting the current scenario or loading its checkpoint. Starting zombies are excluded. Each actual conversion is counted once, including conversions inside multi-tick batches. Later movement and menu navigation do not add to or reset the count.

**Restart scenario** resets the count and restores the scenario's original actors. Loading a run begins counting from that loaded checkpoint; earlier conversions are not inferred from its zombie population. This inspection counter is not stored in the simulation save format and does not change infection, actor behavior or random streams.

Choose **Inspect Weapon Drop Paused**, then **Step 1 tick**. New conversions should increase from zero to one. Step further: it should stay at one. Restart should restore zero and the original infected human. Existing saves require no rules upgrade.

Automated UI checks cover starting zombie exclusion, staggered conversion deadlines, batched stepping, repeated rendering, further ticks, menu navigation, restart and checkpoint loading, including exact saved-state preservation. Native Unity appearance and desktop input require local verification.

Validation: 945 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
