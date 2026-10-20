# Foundation 57 / first weapon priority

When an unarmed human and a human replacing a worn bat have equal route lengths to the same visible supply, the replacement seeker now favors an alternative supply. Equally close peers with the same need still use stable human ID ordering. A closer peer retains the existing distance preference.

This remains a target preference rather than a reservation. A contested bat stays usable when it is the only option, and a pile with enough useful bats can serve both humans. Actual pickups and exchanges still require their own action. Food, shelter and danger retain priority.

Choose **Inspect First Weapon Priority Paused**, then **Run** or **Step 50 ticks**. The armed human should head toward the lower supply while the unarmed human collects the nearer shared target. One used bat should remain on the ground afterward. **Restart scenario** repeats the setup. Foundation 56 saves retain their original tie-break until **Enable first weapon priority** is selected.

Automated checks cover the mixed-need tie, finite equipment, exact save continuation, previous rules, same-need ordering, only-supply fallback, sufficient stock, menu controls, upgrade and restart. Native Unity rendering and Windows builds require local verification.

Validation: 310 core cases / 320,868 assertions; 855 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
