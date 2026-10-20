# Foundation 49: dropped bats and shelter spacing

Humans now drop their bats at their actual cell when they turn. Other survivors can collect those bats using the existing safety checks and normal pickup action. A zombie never carries a usable bat under the new rules. After upgrading an older run, any bats retained by live or defeated zombies drop on the next simulation tick.

Several bats dropped in one cell form a finite pile. Collecting from a pile takes exactly one bat, and the weapon overview counts individual bats. **Next ground bat** focuses a pile and reports its quantity. A bat dropped in a doorway stays there through opening, closing and breaking; it can only be collected while the door is passable. Dropped equipment does not block movement or doors. Skills and practice remain with the actor rather than transferring with its bat.

## Shelter crowding fix

Previously, several humans could reserve the same deepest room cell even while other usable cells were free. They now strongly prefer unoccupied, unreserved destinations within their room. Already crowded occupants also reconsider shared destinations, with stable identity order deciding who keeps a contested position. They route to the replacement through their existing room, respecting collision and observed danger.

This changes where humans settle inside shelters. Room admission limits, infection escape, door work, hunger and leaving when safe retain their existing priorities. Bodies may still temporarily share cells while passing through tight routes; the change removes repeated corner destinations rather than replacing the grid movement system.

## Try it

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm the menu shows **Foundation 49 / dropped bats and shelter spacing**.
3. Choose **Inspect Weapon Drop Paused**, then **Step 1 tick**. The infected human turns, loses its attached bat and leaves one ground pickup. The weapon overview changes from one armed human to one ground bat.
4. Return to the menu and choose **Inspect Shelter Spacing Paused**. Four stressed humans initially share a corner and destination. Click **Run** or **Step 50 ticks** to see them spread to separate positions while remaining in the room.

For an existing Foundation 47/48 save, click **Enable drops and shelter spacing**, then step a tick or run. Loading alone preserves the saved state; upgrading keeps the run paused. Save under a new name to preserve the original file. New runs use encounter-prototype-42. Generation 41 represents drops alone; generation 40 and earlier retain their old behavior until explicitly upgraded.

Ground piles and doorway drops use map schema 5. Existing single-bat maps continue to use schema 4 where possible. Missing `extraBats` means one bat, preserving older records; each pile stores only its additional bats. Invalid quantities, older schemas with stack data and impossible total supplies are rejected. The total finite-bat bound includes 4,096 ground supplies plus up to 1,000 equipped carriers, so turning at full capacity can drop every carried bat.

Validation: 279 simulation/presentation cases / 320,738 assertions; 740 legacy-input UI assertions and 3 NewOnly assertions. Coverage includes turning and upgraded corpses, multiple drops into one pile, one-bat-per-action recovery, closed-door storage and reopening, malformed quantities and incompatible schemas, full-capacity conservation with 1,000 carriers and 4,096 ground supplies, separate room goals and positions, exact continuation after saving mid-spacing, unchanged older rules and explicit upgrade controls. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles; native Unity appearance and input require local testing. No Windows executable was built here.
