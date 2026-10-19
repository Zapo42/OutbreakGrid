# Hunger, food inventory and searchable shelves

Current build: **Foundation 19 / hunger, inventory and shelves**, rules `encounter-prototype-18`.

Humans now have a reason to stay in supplied rooms and leave empty ones. Hunger grows over time. Hungry humans search shelves, carry up to three meals, eat from their inventory and return to their shelter with spare food. Bravery and hunger affect the risk they accept when leaving.

## Try the complete trip

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Hungry Forager Paused**, then select human **#1** inside the small room.
4. Click **Step 50 ticks**. The human has left, searched the outdoor shelf and collected **3/3 meals**. Hunger is still high: searching and eating are separate actions.
5. Click **Step 50 ticks** again. It has eaten one meal and returned home with **2/3 meals**, closing the door behind itself.
6. Compare **Inspect Timid Forager Paused**: hunger and initial stress are identical, but lower bravery keeps this human inside at tick 50. It makes its trip later as stress falls and hunger rises.
7. Try **Inspect Stocked Shelter Paused → Step 10 ticks**: the human searches the shelf inside its room, eats and stays sheltered.

Golden blocks are solid shelves. Click one to inspect it: contents are **unknown until searched**. After a search, the inspector shows remaining meals; a searched-empty shelf becomes grey. The actual human must reach an adjacent cell and spend an action to search. Clicking furniture only inspects it.

## Add shelves to a map

During a hunger-enabled run, use **Place stocked shelf** or **Place empty shelf**, then click a free map cell. Stocked shelves contain six meals; empty shelves still need searching before the human knows they are empty. Shelves cannot be placed through bodies, doors or reserved shelter destinations. Use **Cancel placement** to stop placing.

New quick/configured encounters enable hunger and include finite supplies: alternating rooms have stocked shelves, and an outdoor shelf provides another search location. Existing Foundation 18 saves keep their previous behavior until **Load Run → Enable hunger and inventory**. The upgrade starts paused with zero hunger and empty inventories, preserves existing actors/terrain/clocks/random streams and leaves the source file intact. Add shelves to older maps after upgrading; they are not silently inserted into a saved run.

## Needs and decisions

- Hunger ranges from **0 to 100**, gaining **one point every 30 ticks**: six real seconds at normal speed, or three game minutes. A newly fed human reaches the search threshold after about four real minutes at normal speed. Hunger pauses with the simulation.
- At **40 hunger**, humans start seeking supplies. A meal removes **50 hunger**, consumes one carried meal and costs an action. A search is a separate action, transferring as much finite shelf stock as the three-meal inventory can hold.
- A supplied, fed shelter occupant stays inside. If hungry and carrying food, it eats first. If it can see a potential supply shelf inside its room, it searches there. If the room lacks known supplies, it considers a trip outside, checks the exit and closes doors behind itself.
- Accepted food-trip risk is `clamp(hunger - 40 + 8 × bravery, 0, 80)`. Perceived risk combines current stress, recent warning/impact hearing and personally visible zombies. Increasing hunger raises tolerance; higher bravery raises it sooner. Immediate visible danger within two cells blocks the trip regardless of hunger. Routing also avoids stepping into the immediate contact zone.
- Humans only choose shelves they personally observe or remember briefly. Unsearched full and empty shelves look equally promising. Other humans can see the contents after a shelf has been searched and exposed, but no one tracks hidden stock changes through walls.
- A successful forager eats, then tries to return to its previous shelter with spare meals. Unsafe or full homes do not bypass ordinary shelter admission and danger checks. When no shelf is known, outdoor humans use ordinary local exploration; no global food-location knowledge is granted.
- Infection retains escape priority. Turning clears human hunger, inventory and supply-trip memory. This first food inventory supports meals; equipment, dropping/trading items, body looting and starvation damage are future work.
- Hunger takes priority over quiet door maintenance. Bracing continues when a food trip remains too dangerous. Repair and earlier controlled diagnostics retain their previous rules where those diagnostics isolate an older milestone.

## Persistence and limits

Shelves are sparse, solid furniture in **map schema 3** with finite stock and searched state. Map exports preserve them, including empty shelves; running saves also preserve each human's hunger, carried meals, remembered shelf and return destination. Maps without shelves retain schema 1/2 as appropriate. Older builds reject schema-3 maps rather than silently discarding furniture. The prototype supports up to 4,096 shelves and 0–1,000 meals per shelf.

## Verification

Production-core checks cover clock/pause/caps, stocked and empty rooms, full supply trips and return, hunger/bravery comparisons, immediate danger, unopened-stock independence, competing searchers and inventory limits, solid furniture and hidden shelves, map ownership and validation, conversion cleanup, sensory-memory expiry, exact continuation, a mixed district run and an independently recorded prototype-17 repair trace. UI checks exercise the three scenes, inventory display, shelf inspection, stocked/empty placement and explicit prior-save upgrade.

Validation passed: **150 core acceptance cases / 34,344 assertions**, **437 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity layout, rendering, physical input and a Windows player still require local Unity testing.
