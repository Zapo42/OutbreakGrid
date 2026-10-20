# Foundation 64 / outbreak history

The encounter sidebar now includes a stacked population history graph. Blue shows living humans, including infected humans; olive shows active zombies. Defeated zombies are excluded. The graph's scale adapts to the largest combined population in its displayed history, and labels show the tick range and actor scale.

The graph retains up to 120 samples, roughly two minutes of recent simulation time. It samples each simulation second and refreshes the newest sample between those boundaries. The initial population is retained until it ages out of the rolling window. Sampling follows actual simulation ticks, including manual batches, rather than render frames.

Choose **Inspect Weapon Drop Paused**, then **Step 1 tick**. The graph should show the initial human followed by an active zombie. Step further to extend the history. **Restart scenario** starts a fresh graph. Loading a saved run starts at its checkpoint; earlier history is not reconstructed or stored in the save. Menu navigation preserves the current graph.

This is a presentation update and requires no rules upgrade. Automated UI checks cover initial populations, real conversion, both graph colors, newest-sample updates, unchanged simulation state while rendering, bounded rolling history, chronological wraparound, restart and loaded checkpoints. Native Unity appearance and desktop input require local verification.

Validation: 1,084 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
