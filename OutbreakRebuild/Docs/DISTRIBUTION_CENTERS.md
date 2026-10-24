# Foundation 99 — distribution centers

A distribution center is a discovered room with one designated stock box and assigned couriers. It uses actual food inventory; designation never creates stock. This increment establishes the center and staff relationship before automatic store requests are introduced.

## Configure a center

1. Select a storage box inside a discovered room.
2. Click **Use as distribution stock box**. Its room becomes a distribution center using that box.
3. Select a living, uninfected human with an assigned home. Clear any clerk role and finish active supply work first.
4. Click **Assign courier to distribution center**, select the center's stock box, then click **Assign courier to this center**.

Assignment makes the center box the courier's source. Home, destinations, destination order and work shift are preserved. You may assign a courier before choosing destinations; they wait for work rather than starting unrelated restocking tasks. Add explicit destinations with the existing courier editor to make deliveries in this build. The source box cannot also appear among that courier's destinations.

Open **Distribution centers** to inspect stock, box capacity, meals available above the effective reserve, assigned couriers and active deliveries from that center. Donation blocks reduce available-for-delivery stock to zero. The player totals do not feed hidden stock information into NPC decisions.

## Membership and stock safety

**Unassign distribution center** removes membership while preserving the independent courier route, source, home, shift and cargo. Active trips must finish before changing membership. You may assign an idle courier to another center; their source changes to that center's box, while their destinations stay unchanged.

A bound courier cannot choose a different source through the ordinary source editor. Unassign center membership first, or assign them to another configured center. A center's stock box cannot be replaced while any couriers are assigned to that center. Unassign staff first; independent routes using the old box remain intact.

Center stock and store back stock cannot designate the same box. Ordinary consumers cannot withdraw from center stock or treat it as accessible room food. Assigned center staff can obtain personal meals from their own stock. Automatic generic restocking/donor selection skips restricted stock; explicitly assigned courier tasks can collect or deliver as configured. Their donor reserves, household targets, donation controls, carrying limits, hunger, energy and shifts still apply.

Clearing a courier route removes center membership. Death and conversion also clear membership, leaving the center and remaining stock intact. Center configuration, courier membership and in-progress deliveries persist in running saves under encounter rules 79. Older saves require **Enable distribution centers**. Scenario restart uses the existing authored checkpoint; center assignments are not included in reusable terrain-map exports.

## Center → store → shelf example

Open **Inspect Distribution Center Paused**. Center #183 has 30 meals in its stock box at (4, 3), with 24 initially available above its six-meal reserve. Its courier has an explicit destination at store #192's crate at (13, 3). A clerk serves that store's empty shelf at (12, 7).

Run or step to watch the courier physically collect a load, travel to the store and deposit it into the store crate. The clerk then collects delivered food and carries it to the shelf. Food moves between existing inventories, and the center's reserve remains protected. Restart restores the configured center, courier, store, clerk and original stock.

## Next stage

This build uses explicit destinations. Stores still show empty-crate shortages but do not create requests, and centers do not dispatch couriers automatically. The next requested stage is a persistent empty-crate supply request, followed by courier acceptance, fulfillment and completion without duplicate orders or duplicated food.

Automated checks cover physical center-to-store-to-shelf delivery, saved continuation, food conservation, stock protection, membership/route preservation, stock-box replacement restrictions, ordinary/staff access, multiple centers, malformed/legacy saves, conversion cleanup and UI configuration/upgrade/restart paths. Native Unity remains untested after Foundation 82; `foundation-82-unity-checkpoint` is unchanged.
