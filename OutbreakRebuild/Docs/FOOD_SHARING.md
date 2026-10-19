Latest build: [Remembered shelves](SHELF_MEMORY.md).

# Sharing food

Current build: **Foundation 20 / sharing food**, rules `encounter-prototype-19`.

Humans with spare carried meals can now help hungry neighbors. One meal moves between their inventories, and the recipient eats it on a separate action. Help can spare a hungry shelter occupant an unnecessary supply trip.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Food Sharing Paused**. Human **#1** starts with three meals; human **#2**, below it, has 80 hunger and no food.
4. Select human **#2**, then click **Step 1 tick**. Its inventory now holds **1/3 meals**, while hunger is still **80/100**. Human #1 kept two meals.
5. Click **Step 1 tick** again. Human #2 eats the gift on its own opportunity, reducing hunger to **30/100** and inventory to zero.
6. Continue **Step 50 ticks**. Both stay sheltered; the recipient no longer needs a food trip.

New configured encounters enable sharing automatically. Foundation 19 saves keep separate-inventory behavior until **Load Run → Enable food sharing**. The upgrade remains paused and preserves inventories, hunger, terrain, clock and random streams without changing its source file.

## Rules and limits

- A donor must have at least two meals and be below the hunger threshold. It keeps one meal in reserve and eats for itself first if hungry.
- A recipient must be hungry, carry no food and occupy the same or a cardinally adjacent cell with direct visibility. Sharing never transfers food across a closed door or to remote people.
- The hungriest eligible neighbor gets priority. Equal hunger uses stable actor identity, so continuation is deterministic.
- Sharing consumes the donor's ordinary action instead of movement, repair or searching. It transfers exactly one meal without creating stock, lowering hunger immediately or consuming random draws.
- The recipient eats on its next ordinary opportunity when safe. Receiving a meal cancels its pending shelter departure and remembered search target so it can use the food it now carries.
- Infection, a zombie visible to either participant, or a recent warning/door impact postpones sharing. Donors also finish active fight/flight/freeze responses first. This milestone covers nearby handoffs; seeking distant recipients, requesting deliveries and trading are future work.
- Hunger, shelf searching, finite stock, bravery-based food-trip risks and return trips remain available. Shared inventory is part of the normal running save.

## Verification

Production-core checks cover transfer versus eating timing, inventory conservation, donor reserve and own-hunger priority, greatest-need selection and stable ties, fed/already-supplied recipients, infection and heard/visible danger, distance and receiver-only sight, canceled departures, pause and exact continuation, and independently recorded prototype-18 behavior. The UI harness exercises the new menu, recipient selection, inventory/hunger inspector, tick controls and explicit prior-save upgrade with source-file preservation.

Validation passed: **159 core acceptance cases / 34,526 assertions**, **449 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still require local Unity testing.
