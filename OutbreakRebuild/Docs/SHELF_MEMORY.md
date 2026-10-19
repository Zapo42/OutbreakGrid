# Remembered shelves

Current build: **Foundation 21 / remembered shelves**, rules `encounter-prototype-20`.

Humans now keep a personal notebook of up to eight shelves. They remember unknown contents, exposed food and empty shelves across meals, food sharing and later hunger. Confirmed remembered supplies take priority over an unknown shelf, and remembered empty shelves are skipped.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Shelf Memory Paused** and select human **#1**, on the left below the empty shelf.
4. The scene starts at tick **26**, after actual earlier searches. The inspector remembers **2, 3: empty** and **7, 4: last seen 3 meals**. The supply door is now closed and the human is hungry again after using its earlier carried meals.
5. Click **Step 10 ticks**. The human heads toward **7, 4**, despite not currently seeing it, rather than searching the nearer empty shelf.
6. Click **Step 10 ticks** four more times. It opens the door, reaches the remembered supply, takes the final three meals and eats one. The notebook now remembers **7, 4: empty**.

Scroll the inspector to see all notes. Each entry distinguishes last-seen contents from current hidden stock and shows its observation age.

New encounters enable this behavior automatically. Foundation 20 saves retain the earlier single-target behavior until **Load Run → Enable shelf memory**. The upgrade remains paused and preserves actors, inventory, active food targets, terrain, clock and random streams. It starts with no invented past discoveries; humans build notes on subsequent personal observations. The original save file stays unchanged.

## Rules and limits

- Humans notice visible shelf locations even while fed. Unopened shelves are recorded as **unsearched**, regardless of their actual contents. An actual search records the stock left after taking food. Exposed contents can also be learned through personal sight.
- A note does not track hidden changes. Another person can empty a shelf while the observer is away; the observer may revisit expecting food until fresh sight corrects that belief. Visible replenishment can correct a remembered-empty note.
- The notebook retains at most **eight** locations. Older observations are replaced first; equal-age observations prefer nearer shelves, with stable coordinate ties. This is deterministic across save/load and furniture insertion order.
- An unrefreshed note expires after **3,600 ticks**: six game hours, or twelve real minutes at normal speed. Expiry alone does not reveal current stock. Personal sight refreshes the observation time.
- Confirmed food takes priority over unknown shelves. Within each category, nearer shelves win, with stable coordinate ties. Empty notes are excluded until fresh observation changes them or the old note expires.
- Longer remembered trips use bounded local waypoints and the existing collision-safe shelter routes. Occupancy, doors, solid furniture and perceived danger still apply; remembering food never permits walking through obstacles or tracking unseen zombies.
- Food remains finite. Eating, sharing and returning home do not erase the notebook. Turning clears human shelf knowledge with hunger and inventory.
- Notes belong to individuals. Food sharing does not automatically share discoveries with other people. Snapshot and actor-read boundaries copy the notebook so external inspection cannot mutate live knowledge.

## Verification

Production-core checks cover real prior searches and a revisit beyond a closed door, hidden depletion, preference over a nearer unknown shelf, unopened-content independence, visible restocking, exact expiry, bounded deterministic replacement, personal knowledge isolation, defensive array ownership, longer remembered routes, invalid records, pause and exact continuation, conversion cleanup and an independent prototype-19 trace. UI checks cover scene selection, remembered-content labels, travel and depleted-stock correction, and explicit older-save upgrade with source-file preservation.

Validation passed: **172 core acceptance cases / 34,768 assertions**, **463 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still require local Unity testing.
