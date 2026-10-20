# Foundation 51: bat durability

A fresh bat lasts twelve successful hits. Each hit adds one wear point; misses, carrying, collecting equipment and idle time do not wear it down. The twelfth hit deals its full normal damage, grants normal skill practice, then breaks the bat. The broken bat leaves the inventory and does not become a recoverable ground pickup. Earlier hits have the usual damage and accuracy; wear does not reduce them before breakage.

The human keeps learned blunt skill and practice. Its existing swing cooldown continues after breakage and is not reset by collecting another weapon. Unarmed defense and safe weapon collection use their existing decisions and normal action costs. The last swing remains visible for its normal 0.4-second sweep before the broken bat disappears.

## Condition follows the equipment

Partial wear is saved with a carried bat. Turning drops that exact used bat; a survivor collecting it inherits its condition. Mixed ground piles retain every bat's individual wear. Collecting from one pile chooses its best-condition bat, then removes only that item. Doorway closure and reopening preserve condition too. This does not grant humans knowledge of hidden piles or override their safety checks.

The selected carrier's inspector shows **Bat condition: N/12 hits remaining**. After breakage it briefly says **Bat broke**. **Next ground bat** also reports the best condition in the focused pile. Break feedback is transient and clears on loading; condition, inventory, skill and cooldown are saved.

## Try it

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm **Foundation 51 / bat durability** appears on the menu.
3. Choose **Inspect Bat Durability Paused**. The selected human's bat has **1/12 hits remaining**.
4. Click **Run** to watch its final swing, or **Step 1 tick** to inspect the result. The zombie takes the normal 38 damage, the human loses its bat, the inspector reports breakage, and the swing cooldown remains.
5. Use an ordinary configured encounter to watch bats gradually wear through combat. Select an armed human to inspect remaining hits. Humans can collect another visible bat when their existing food, shelter and danger priorities allow it.

New runs use encounter-prototype-44. Existing Foundation 50 saves retain encounter-prototype-43 with no durability until **Enable bat durability** is clicked. Upgrading starts existing bats fresh, preserves the current actors, map, tick and random states, and keeps the run paused. Save under a new name to preserve the original file. Earlier upgrade controls still enable the current complete rules.

Maps with used ground bats use schema 6 and optional sorted per-item wear data. Older fresh maps keep their existing schema where possible. Carried wear is an optional actor field, with an absent value meaning fresh. Invalid wear, broken equipment in inventories/piles, malformed condition arrays and older rules containing used weapons are rejected on load. Ground condition records and returned arrays are detached from authoritative state.

Validation: 288 simulation/presentation cases / 320,789 assertions; 771 legacy-input UI assertions and 3 NewOnly assertions. Checks cover hit/miss wear, full final-hit damage and fatal hits, retained skill and cooldown, replacement pickup, exact save continuation, individual condition through turning and mixed-pile recovery, doorway storage, detached arrays, malformed records, older rules and explicit upgrades, inspector condition and the final visible swing. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles; native Unity visuals and input require local testing. No Windows executable was built here.
