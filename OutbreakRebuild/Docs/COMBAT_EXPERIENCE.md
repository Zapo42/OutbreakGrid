# Foundation 47: blunt weapon experience

Successful bat hits now earn one practice hit. A human needs `3 + current blunt skill` successful hits to gain the next level: three hits from skill 0 to 1, four from 1 to 2, and so on. Progress resets on a level gain and stops at skill 10. The existing damage and accuracy formulas use the learned skill on subsequent swings: each level adds two damage and three percentage points to the accuracy calculation, subject to its existing 95% cap.

The hit that earns a level uses the previous skill for its accuracy and damage. Misses, collecting equipment, swinging during cooldown, unarmed shoves and inactive targets earn no practice. A fatal hit counts. Experience never consumes an extra action or random draw, changes attack speed, or overrides hunger, shelter, infection or flight decisions.

Partial practice is saved with the actor, so saving one hit before a level preserves the exact next gain. Skills and practice remain recorded after turning, but zombies cannot train or use their retained bats. Invalid progress, nonzero progress at maximum skill and progress under older rules are rejected during loading.

## Try it

1. Open the latest **OutbreakRebuild** project, then choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm the menu says **Foundation 47 / blunt weapon experience**.
3. Choose **Inspect Combat Experience Paused**. The selected human starts at skill 0 with **2/3 practice hits**.
4. **Step 1 tick** to collect the bat, then **Run**. Its first hit raises skill to 1. The inspector shows **0/4 hits** toward the next level and bat damage increases from **28** to **30**. The earning hit deals 28; the next hit deals 30.

Use the inspector to follow practice and skill in ordinary runs too. The faster swings, body hit flashes, nearby bat seeking and doorway movement from earlier builds remain active.

New runs use encounter-prototype-40. Foundation 46 saves retain encounter-prototype-39 without learning until **Enable combat experience** is clicked. Earlier melee saves offer their existing upgrade labels and upgrade to the current rules. Upgrading preserves actors, map, clocks and random streams and leaves the run paused. Save under a new name to preserve the original file.

Validation: 270 simulation/presentation cases / 317,652 assertions; 691 legacy-input UI assertions and 3 NewOnly assertions. Checks cover each level boundary, damage before and after learning, partial-progress save/resume, unchanged random consumption, older rules, misses, fatal hits, maximum skill, turning, malformed saves, inspector refresh and explicit upgrade. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles; native Unity appearance and input need local testing. No Windows executable was built here.
