# Foundation 52: recovery footwork

Armed humans who choose Fight can now step back while their bat swing is recovering. They do not spend the recovery period charging with an unavailable attack. Once the cooldown expires, normal contact strikes and existing courage decisions resume.

A recovering fighter considers one normal neighboring step when a visible zombie is within two cells. The step must improve its minimum clearance from visible zombies without moving closer to any of them. Clearance uses the same square-cell range as bite contact, with a second distance check to avoid approaching another threat diagonally. Direct retreats win equal-clearance ties, reducing unnecessary zigzags. Hidden zombie positions do not influence the choice.

Movement respects occupied-cell limits, furniture, walls and solid corners. Doorway crossings remain cardinal. The human does not open a closed door for a combat backstep; if there is no safe destination, it holds position. With more distance already available, it waits for its swing to recover rather than charging. Footwork consumes the ordinary movement opportunity, adds no extra action or random draw, and does not reset cooldown, consume durability or grant practice.

Food, shelter, infected escape, freezing and flight retain their existing decision priority. Unarmed shoves retain their behavior. Recovery footwork applies to a healthy armed fighter with a pending swing cooldown, rather than replacing those other reactions. It does not guarantee protection from multiple zombies or from a missed swing before the next movement opportunity.

## Try it

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm **Foundation 52 / combat footwork** appears on the menu.
3. Choose **Inspect Combat Footwork Paused**, then **Run**. The first strike damages and staggers the zombie, but a wall blocks its knockback. The human steps back on its next action while recovering, then attacks normally when ready.
4. Alternatively, **Step 10 ticks**: the human should be at cell **2,2**, uninfected, with its bat at **11/12 hits remaining** and **1 tick** left on swing cooldown. The zombie has 62 health internally. **Step 1 tick** resumes the attack, leaving the zombie at 24 health and the bat at 10/12 hits.

This controlled scene contrasts the old stationary recovery, which leaves the human in contact for a bite. Ordinary encounters use the same safe-step checks.

New runs use encounter-prototype-45. Existing Foundation 51 saves retain encounter-prototype-44 until **Enable combat footwork** is clicked. Upgrading preserves actors, map, saved weapon conditions, clocks and random streams and leaves the run paused. Save under a new name to preserve the older file. There are no new actor or map fields for footwork; decisions use current saved state and local observations.

Validation: 293 simulation/presentation cases / 320,806 assertions; 779 legacy-input UI assertions and 3 NewOnly assertions. Coverage includes blocked-knockback recovery, attack resumption, unchanged wear/cooldown/random streams, exact save continuation, older stationary recovery, visible versus hidden threats, solid corners, cardinal doorway movement, furniture, full cells, frozen/infected/unarmed priorities and explicit upgrade controls. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles; native Unity appearance and input require local testing. No Windows executable was built here.
