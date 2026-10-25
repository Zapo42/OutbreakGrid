# Foundation 127: persistent shopping trips in generated cities

Target: Unity 6000.6.3f1. Native Unity testing remains paused after Foundation 82.

## What failed

Seed 214 generated residents had assigned homes but no shelf memories or known shop locations. When their cabinet emptied, they planned a shopping trip, inspected the door and left. The next decision found no known shop, switched to a return task and brought them straight back without food. Reproduced on both a 4×2 district and an 8×8 city.

Providing a shop location exposed a second limit: trips longer than the fixed 60-energy outbound budget hit the return threshold before collection. Departure could also leave the ordinary home-food-return marker attached to an outbound shopping trip.

## Changes

New generated residents start with one **familiar shop location**, chosen from their neighborhood relative to their assigned home. This is authored background knowledge of a place, not knowledge of its shelf contents. It persists independently of short-lived sight reports. They select a reachable known shop before leaving, keep that destination while travelling, and release the stale home-return marker after actually exiting.

Safe shoppers with a known destination can take short breaks at 40 remaining energy until they reach 60. Breaks happen off roads, crosswalks and doorways, outside their own home. Each decision restores five energy and costs one hunger point. The saved rest flag gives the break a stable endpoint instead of alternating between one walk and one rest. Existing threat, infection, stress, warning, hunger and task-expiry rules still interrupt work. Every real movement keeps its normal energy cost. Trips do not teleport or receive free food.

The selected-NPC inspector shows the familiar shop, current shopping activity and retained interruption reasons: no reachable known store, unavailable route, empty/unavailable stores, danger, hunger, a satisfied home reserve, or timeout. During a rest it explicitly says the destination is retained.

## Try it

Choose **Inspect Large City Shopping Paused**. This controlled scenario has one shopper on the complete 8×8, 247-building city map, an empty assigned home cabinet, a familiar shop location and no initial shelf-stock observations. Run or use Step 100 repeatedly to follow the trip. The automated run physically deposits food at home around tick 983. This fixture isolates the shopping decision; it does not represent a full populated-city performance measurement or an inexhaustible store.

For an existing run, use **Enable persistent shopping trips** if offered. Select a human and choose **Assign familiar neighborhood shops**. This explicitly fills missing shop-location familiarity for living assigned residents, without replacing existing familiarity or revealing cabinet/shelf stock. New generated cities do this during scenario setup. Older rules 96 and earlier preserve their behavior until upgraded; loading a save does not grant new background knowledge automatically.

Run rules `encounter-prototype-97` save familiar shop IDs, the rest flag and the last interruption reason. These fields are validated against existing stores, biological state and active tasks. Death/conversion clears human-only data. Restart preserves authored familiarity and resets the in-progress trip.

## Limits

Carry capacity, finite supply, hunger, job schedules, physical walking and task timeouts are unchanged. A sufficiently distant journey can consume all carried food, leaving no surplus to deposit. Consumer vehicle shopping is the next transport step to mitigate these costs through shorter trips, lower exertion and larger cargo loads. Persistent locations are not a city-wide search patrol or a transport solution for every distance. Unknown authored/custom-map shoppers still discover shops through existing sight rules unless familiarity is explicitly assigned. The new inspector text helps distinguish these cases from a lost destination.

Leisure wandering, happiness, parks, bars, vehicle speed/types/fuel, pixel graphics and the Old Test Scenarios menu remain separate queued work in [the city-life workflow](CITY_LIFE_AND_TRANSPORT_PLAN.md).

Automated validation: 601 core cases / 732597 assertions, 23 focused cases, 2139 UI assertions and 3 NewOnly input assertions. The original rule-96 departure failure, finite home delivery, hidden-stock independence, rest/save continuation, danger interruption, explicit familiarity assignment and malformed-state rejection are covered. Native Unity remains unverified.
