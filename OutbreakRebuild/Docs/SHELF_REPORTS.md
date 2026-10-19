# Shelf reports and clear empty-shelf placement

Current build: **Foundation 22 / sharing shelf reports**, rules `encounter-prototype-21`.

Humans can now tell adjacent people where they personally found food or an empty shelf. A listener records who told them and when the source last observed it, then can follow that report without seeing the shelf directly.

## Empty-shelf fix

The empty placement tool stored zero meals, but unsearched empty shelves used the same gold appearance as stocked ones. This made empty placements look stocked.

- **Place empty shelf** now produces a **grey** shelf and confirms **EMPTY shelf: 0 meals**.
- **Place stocked shelf** remains **gold**, with six meals.
- Clicking a shelf shows its actual **Stock: N meals** and whether humans have searched it.
- This is the player/author view. Humans still treat unopened contents as unknown until searching or personally seeing exposed contents; they do not read the display's stock information.

The regression check switches between stocked and empty tools, checks actual finite stocks and map export, and inspects the colours sent by the production draw calls.

## Try the next behavior

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Shelf Reports Paused**, then select hungry human **#2**, below human #1.
4. Click **Step 1 tick**. Its notebook now lists the shelf at **7, 4**, **Told by human #1**. It has no food yet, and the shelf remains hidden behind the closed partition.
5. Click **Step 50 ticks**. The listener opens the door, reaches the reported supply, searches and eats. Its note becomes **Personally observed**, and remembers the depleted shelf as empty.

New encounters enable reports automatically. Foundation 21 saves retain personal-only notebooks until **Load Run → Enable shelf reports**. The upgrade stays paused and preserves existing notes, inventories, targets, terrain, clock and random streams without changing the source file. The empty-shelf display correction also applies when viewing older runs.

## Conversation rules

- Conversations require visible humans in the same or cardinally adjacent cells. Infection, current zombie sightings and recent warning/door-impact hearing postpone conversation.
- Only fed, settled speakers talk. Eating and handing over a spare actual meal take priority; a speaker can keep its last reserve meal while telling a hungry neighbor about supplies.
- A conversation takes one normal speaker action and transfers one useful fact. Hungry recipients get priority, then food locations over empty locations, then fresher source observations; stable iteration resolves remaining ties.
- Unsearched contents are not presented as confirmed food or emptiness. Only personally observed facts are shared. A listener cannot relay hearsay to someone else until it has personally checked the shelf.
- The report preserves the original observation time. Retelling cannot extend its six-game-hour lifetime or peek at hidden current stocks. Fresher personal knowledge is protected from older reports.
- Identical information is not repeated every action. Unchanged stock needs a source observation at least 300 ticks newer before it is worth repeating. Actual changed contents can be reported sooner.
- An empty report cancels an active target for that shelf. Known food can guide a trip beyond sight, using the existing local routes, danger checks and physical door actions.
- The notebook still holds at most eight locations. Turning clears the source's own notes, but a listener can retain the historical report until its normal expiry or a fresh observation corrects it.
- These are quiet adjacent conversations, not warning shouts; they do not create a zombie sound cue in this milestone.

## Verification

Production-core checks cover dated transfer, hidden depletion, no relaying, no repeated chatter, meaningful refresh, fresher personal knowledge, empty-target cancellation, unknown-content exclusion, range/infection/danger, meal priority, original expiry, a source turning, provenance validation, pause and exact continuation, and independently recorded prototype-20 behavior. UI checks cover the new scene, reported/direct labels, learned travel, explicit older-save upgrade and the empty-shelf regression.

Validation passed: **185 core acceptance cases / 34,922 assertions**, **477 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

The UI harness records production draw colours and control dispatch through narrow Unity API doubles. Native Unity layout, physical input, pixel rendering and a Windows player still require local Unity testing.
