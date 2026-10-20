# Foundation 46: quick swings and nearby weapon seeking

Bat swings now sweep and return in two ticks (0.4 seconds at normal speed), down from five. The tip trail and gold spark are removed. A successful hit instead flashes the struck zombie pale red for 1.5 ticks (0.3 seconds). The flash stays on the moving body during knockback, also appears on the fatal hit, and leaves the selection border visible. Misses animate without a flash. Damage, accuracy and the ten-tick attack cooldown are unchanged.

Flashes and swing poses follow simulation time, freeze while paused, and reset on loading rather than replaying attacks. Hit notifications include the actual target identity; they remain transient and do not change saves or consume randomness. The previous doorway movement fixes remain active.

## Next combat step

Calm, unarmed, uninfected humans can approach a visible nearby bat. Food and shelter decisions run first. Seeking stops when a zombie is visible, stress is nonzero, the human is fleeing/frozen, recent warnings or door impacts remain active, or a shelter is assigned. Nearby pickup retains its existing safety rules and consumes its own action.

Candidates must be visible within six cells in each axis. The human chooses the shortest available route to a cardinal pickup neighbor, with a maximum of six movement steps; coordinate order resolves ties. Routes respect occupancy, solid corners and doorway movement, and cannot open closed doors to collect equipment. The human rechecks visible equipment each opportunity, so another human taking it or losing sight cancels the approach. There is no hidden-item knowledge or remembered weapon destination in this milestone. One ground bat still equips only one human.

## Try it

1. Download the latest repository ZIP and open its **OutbreakRebuild** folder in Unity Hub.
2. Choose **Outbreak > Rebuild > Open and Run Foundation**. The menu should show **Foundation 46 / quick swings and weapon seeking**.
3. Choose **Inspect Blunt Combat Paused → Run** to watch the faster swings and hit flashes. Use **0.5x** if useful. Large paused tick batches skip intermediate animation frames.
4. Return to **Main Menu**, choose **Inspect Weapon Seeking Paused**, then **Run**. The human walks to the visible bat and collects it. **Step 50 ticks** also verifies collection while paused.

New runs use encounter-prototype-39. Foundation 44/45 saves retain encounter-prototype-38 and their previous behavior until **Enable weapon seeking** is clicked. The upgrade keeps the run paused and preserves actors, terrain, clocks and random streams. Save under a new name to preserve the older file. Visual changes apply to existing melee runs without a behavior upgrade.

Validation: 266 simulation/presentation cases / 317,617 assertions; 676 legacy-input UI assertions and 3 NewOnly assertions. Coverage includes finite pickup after approach, exact continuation after saving mid-approach, guarded seeking, closed-door occlusion, older-rule preservation and explicit upgrade, fast swing timing, misses, fatal-hit flashes, flash expiration and load reset, plus the existing doorway regression. Configuration and whitespace checks passed.

The UI harness runs production handlers with narrow Unity API doubles. Native Unity appearance and input need local testing; no Windows executable was built here.
