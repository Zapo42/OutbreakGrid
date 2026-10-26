# Foundation 129: consumer vehicles and finite shopping carts

Target Unity 6000.6.3f1. Native Unity testing remains paused after Foundation 82.

## Try the new build

Choose **Inspect Vehicle Shopping Paused** in Current Tests. It uses one assigned consumer on the full 8×8 city map, a home reserve of 12 meals, a stocked shop and home/store parking bays. Run or Step 100 repeatedly. In the controlled run, the resident finishes around tick 1383 after two store loading trips and two home unloading trips. Observe the vehicle or select the shopper to inspect both inventories. Restart restores the authored empty vehicle and home supplies.

For a custom setup:

1. Give a living pure consumer an assigned home with a food storage box and reserve target. Fill missing background shop locations with **Assign familiar neighborhood shops** in the selected NPC inspector.
2. Place parking near home and the store using the existing parking controls. Place a vehicle near home, select it and choose **Assign shopper #…**. Workers retain their courier/clerk roles in this increment.
3. Let the resident inspect home supplies. A sufficiently low remembered home stock starts a trip. The vehicle panel shows its stage, cargo, cart and trip target. **Stop vehicle** pauses the job; **Resume vehicle shopping** continues it. An empty, idle shopping vehicle can be unassigned.

Older runs keep rules 97 or earlier until explicitly upgraded through **Enable consumer vehicle shopping**. Existing assigned residents can then gain alternate familiar shop locations through **Assign familiar neighborhood shops**. Loading alone does not reveal new shop or inventory knowledge.

## Inventories and physical transfers

- Personal rations keep the existing three-meal maximum and normal reserve policy.
- Consumer vehicle cargo holds **48 meals**. Courier shipment capacity stays at 24 in this increment.
- A separate trip cart holds **six meals**. It is a hauling inventory tied to this shopper/vehicle, not an extra personal pocket.

The shopper walks to the vehicle, boards, drives along legal roads and parks near the store. Shop stock is learned through ordinary observation/search. At a shelf the shopper first tops up its personal ration reserve from real shelf food, then puts a finite load into the cart. It walks the cart to the stationary car and transfers it only when adjacent. It repeats until the planned home deficit/cargo limit is reached or available known stock is exhausted.

The vehicle then drives home. The resident retrieves up to six meals, walks them to the actual home box, deposits only its current reserve deficit/space, and returns for another load if needed. If home no longer needs the load, excess returns to vehicle cargo. Retained vehicle food can replenish a later home shortage without another purchase. Every meal exists in exactly one inventory and all compartments contribute to the food overview.

Loaded carts have a small basket-and-wheel graphic beside their shopper; vehicle cargo is shown in the car. Empty carts are not separate pickup resources in this first inventory-focused implementation. Walking keeps its normal energy cost, including while hauling; driving uses the existing one-energy-per-ten-road-cells rule. Needs continue while riding. Personal rations are eaten first; only physically accessible cargo/cart food can be used afterward. No remote shelf collection or home deposit occurs.

## Depleted nearby stores and hunger

Generated residents now have a primary familiar store and up to seven additional familiar locations. These are authored locations, not stock reports, and are saved defensively. Recently unsuccessful shops are skipped when choosing another reachable familiar/observed shop. Vehicle shoppers can drive to another known store in the same outing; stops are bounded by the number of known locations, capped at eight, and the normal task timeout still applies.

The reported walking-test loop is also corrected under current rules: hunger alone does not send a shopper back to a home whose personally remembered boxes are all empty. It can start/continue a necessary food trip to a known shop, including a farther familiar store after the nearest is depleted. Threats, warnings, infection, actual starvation and blocked/unreachable routes still matter. The regression fixture verifies reaching the farther store, eating and bringing food home. It does not grant knowledge of remote stock or unlimited survival over any distance.

## Interruptions and limits

Saved phase, occupant, cargo, cart, route/retry clocks, needs, home claim and task resume deterministically. A paused trip retains its supplies. An incapacitated shopper stops driving; a loaded on-foot cart remains at its actual ground position with its finite food. A healthy explicitly assigned replacement from the same household can recover that cart and take supplies home. Other consumers do not automatically loot abandoned carts. Blocked exits/bays wait; existing bounded traffic recovery and lane/crossing guards remain in use.

The first assignment API supports pure consumers, one driver/cart per vehicle, food only and the existing maximum of 16 vehicles. It does not automatically assign cars to every household or add worker commuting by car, fuel, vehicle types/colors, increased speed, checkout/payment, cart ownership as a separate resource, weapons cargo or general abandoned-object looting. These remain later increments. A very long or dangerous trip can still fail or consume supplies; capacity and driving make bulk transport practical without disabling hunger.

Automated validation: 611 full core cases / 738553 assertions, 33 focused cases, 2315 UI assertions and 3 NewOnly input assertions passed. Coverage includes finite multi-cart transfers, a cargo load over 24 meals, exact saved phases, pause/resume, excess retention/reuse, empty stores, farther-store fallback for walking and driving, household cart recovery and corrupt-state/legacy rejection. Native Unity remains unverified.
