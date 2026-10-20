# Foundation 48: bat supplies in ordinary encounters

**Start Encounter** now starts the usual 24 humans and one zombie with six finite ground bats. **Set Up Encounter** adds **Bats at start**, also defaulting to six. Set it to zero for an unarmed start. The arena supports 0–24 bats and the district supports 0–360, with one pickup per cell in each map's authored human spawn area. Invalid input leaves the start button disabled and preserves the current run.

Bats are scattered in the human starting area using the chosen seed. They do not replace shelves, doors, terrain or actors. The same seed and settings reproduce the same placement; increasing only the bat count preserves earlier placements. Supply placement uses its own temporary random stream, so changing the bat count leaves initial actors and simulation random streams unchanged. The human and zombie population settings still count bodies only.

Humans start unarmed. Taking a bat consumes a normal action and remains subject to the existing pickup safety rules. Calm humans can seek visible equipment; food, shelter, infection and danger retain their priorities. Supplies never respawn automatically. The current combat, learned skills, fast swings and hit flashes all apply during ordinary encounters.

## Try it

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm **Foundation 48 / encounter bat supplies** appears on the menu.
3. Choose **Set Up Encounter**, leave the arena, 24 humans, one zombie and six bats selected, then click **Start configured encounter**. It starts paused by default.
4. Scroll the inspector to **WEAPON OVERVIEW**. It shows ground bats, armed humans and bats retained by zombies, including defeated zombies.
5. Click **Next ground bat** to pause and center the camera on a supply. Repeated clicks cycle through remaining bats and wrap around. This only changes the player's view; humans still need their own sight and pickup opportunity.
6. Click **Run**, or step ticks, and watch supplies move from the ground count into the armed-human count as people collect them. Select a human to inspect its equipment and practice progress.

For a peaceful pickup check, configure 24 humans, zero zombies and six bats. For an unarmed comparison, repeat with zero bats. The quick **Start Encounter** button also uses the new six-bat default.

Existing saves retain their actual saved equipment and ground pickups; loading or resuming does not add supplies. There is no rules upgrade for this release: encounter-prototype-40 continues unchanged. The bat-count setting affects new configured starts, rather than modifying a retained run. Earlier saves continue using their saved behavior generation and existing upgrade controls.

Validation: 273 simulation/presentation cases / 318,522 assertions; 727 legacy-input UI assertions and 3 NewOnly assertions. Checks cover exact counts from zero through each map's maximum, invalid input, unique passable placement, seed repeatability, unchanged initial actors/food/random streams, finite ordinary pickups, save continuation, template ownership, custom setup counts, overview refresh and focus wraparound without advancing time. Project configuration and whitespace checks passed.

The UI harness uses narrow Unity API doubles. Native Unity appearance and input still need local testing; no Windows executable was built here.
