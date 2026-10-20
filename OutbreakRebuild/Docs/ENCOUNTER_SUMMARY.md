# Foundation 62 / encounter summary

The encounter sidebar now shows humans remaining, infected humans, active zombies and defeated zombies. The infected count is a subset of remaining humans, so it is explicitly marked as included rather than added to the human total.

The summary distinguishes an ongoing outbreak, infected humans who may still turn, no remaining humans, no active zombies or infection, and no living actors. An empty authored encounter shows **No actors placed**. These are observations; the simulator continues normally and does not automatically pause or end a scenario.

Choose **Inspect Weapon Drop Paused** to see one infected human and no active zombies. Step one tick to see conversion update both counts. Restart restores the original summary. **Inspect Blunt Combat Paused**, then **Step 1 tick** and **Step 50 ticks**, shows a zombie move from active to defeated after real combat.

Counts are reconstructed directly from the current run, including after loading saves. This presentation update requires no save upgrade.

Automated UI checks cover mixed living and defeated populations, latent infection, conversion, real combat defeat, continued simulation after clearance, restart, save/load, empty encounters and defeated-only encounters. Native Unity appearance and desktop input require local verification.

Validation: 930 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
