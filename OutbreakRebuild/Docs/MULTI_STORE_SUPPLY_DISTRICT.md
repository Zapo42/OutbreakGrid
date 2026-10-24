# Foundation 102 — multi-store supply district

Open **Inspect Multi-Store Supply Paused** from the main menu. The scenario starts paused with three stores, one shared distribution center, two couriers, three clerks and three hungry consumers: eight humans in four buildings.

The map is 35×24 cells. Stores have different distances and approach directions from the center. Walkable street/sidewalk terrain makes the layout easy to follow; deliveries use the existing walking routes and door interactions. This scene isolates the supply chain without starting zombies. You can introduce them later with the existing placement tools.

## Starting stock and roles

The center stock box starts with **26 meals** in a thirty-meal inventory. Its ordinary six-meal reserve leaves **20 meals available for delivery**. Couriers and clerks each have one personal meal, and each consumer starts beside a shelf holding one meal. The scenario therefore starts with **34 actual meals total**.

Each store has a four-meal back-stock crate starting empty, one shelf with a six-meal replenishment target, one reachable phone, a clerk and a consumer. Workers and consumers have their own home assignments. Both couriers belong to the shared center and start without manual delivery destinations: phone requests drive their work. Existing shifts remain configurable; the scenario starts with continuous courier availability.

| Site | Room ID | Stock/crate | Shelf | Phone |
| --- | --- | --- | --- | --- |
| Shared center | 319 | (5, 9) | — | — |
| North store | 121 | (17, 3) | (16, 7) | (17, 6) |
| East store | 342 | (28, 9) | (27, 13) | (28, 12) |
| South store | 576 | (17, 16) | (16, 20) | (17, 19) |

## What to observe

1. Run or step fifty ticks. Consumers eat the starting shelf food, clerks call from their phones, and three store orders compete for two couriers.
2. Watch couriers claim different orders, share the center stock, deliver batches and leave partial/completed records. Clerks then transfer deliveries to shelves for consumers.
3. Open **Show Food & shelves** to use **Step 1 game hour**. Four presses advance to the four-hour automated-test checkpoint, 2400 ticks. At 1× playback that is eight real minutes.
4. Inspect the remaining pending requests once the center has no stock available above its reserve. This is finite-supply pressure, rather than an endless free replenishment loop. Staff personal meals retain their existing stock-access behavior.

Stores continue requesting as stock is used, so completion counts include repeat orders. Consumers may seek other food sources through their existing behavior; household membership stays separate from physical location.

## Navigation and order-status correction

The collapsible inspection layout is retained. **Observe store #…** selects and zooms to its crate; **Observe center #…** does the same for center stock. Their clicked-item inspectors include local store/center context. Use the top **Zoom −** control to widen the view again.

Store management now lists pending orders oldest first and shows delivered/requested meals, remaining quantity, and the actual assigned courier. This corrects a prior display that continued to say “Awaiting courier” during claimed work. **Observe courier #…** and **Observe order #…** retain their existing focus behavior. Inspection and sorting do not mutate the simulation or its request order.

Restart restores the original eight people, roles, thirty-four meals, phones and empty request history. Existing runs keep their own scenarios; use the new menu entry to load this district. Encounter rules stay unchanged from Foundation 101.

## Validation

Three four-hour trials use seeds 214, 123 and 999. Each exercises simultaneous/partial requests, service to all three stores, protected finite center stock, requests remaining after dispatchable stock runs out, bounded unique claims, and meal conservation including eating/death accounting. The default seed ends the four-hour checkpoint with 20 meals eaten, 14 remaining, three completed requests and three pending requests. Other seeds finish with 21 eaten and 13 remaining in the automated checks.

Additional checks cover exact seeded starts, saved continuation during active deliveries and shortages, actual menu/step/hour controls, truthful pending-order status, crate/center focus, contextual inspection, and scenario restart. Native Unity remains untested after Foundation 82; `foundation-82-unity-checkpoint` remains the rollback tag.
