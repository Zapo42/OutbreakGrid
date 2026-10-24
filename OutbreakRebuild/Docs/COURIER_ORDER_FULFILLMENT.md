# Foundation 101 — courier order fulfillment

Distribution-center couriers now accept pending store phone orders automatically. They collect actual stock, carry it to the store's secondary crate, and credit only the food physically deposited. Partial deliveries persist until the requested quantity is delivered.

## Use the supply chain

1. Configure a store crate, shelves, clerk and reachable phone.
2. Configure a distribution-center stock box containing food above its protected reserve.
3. Give a courier a home and assign them to the center. Manual delivery destinations are optional for request fulfillment.
4. Let the clerk observe an empty crate and call. An eligible center courier accepts the request automatically.

Center couriers prioritize phone orders before their explicit destination routes. Requests are considered oldest first, using creation tick and order ID. One courier claims an order at a time; existing destination claims also prevent conflicting trips to the same box. On-demand couriers can use the existing daily shifts even without manual destinations.

The order supplies a destination address, not live stock knowledge. Couriers use their existing perception, source observations, routing and safety rules. Donor reserves, household food targets and **Block donations** remain effective. Order quantities override the recipient box's ordinary minimum restocking target, including a reserve of zero, because an explicit phone order requests an actual shipment.

## Partial shipments and completion

Healthy couriers normally carry two delivery meals plus one personal meal, within the existing three-meal carry limit. Loads are capped to the remaining order quantity, allowing a final one-meal shipment. Eating carried food can reduce the delivery quantity; only the actual deposit is credited.

A clerk may immediately move delivered food from the crate to shelves. That does not reduce the order's delivered count. Completion means cumulative requested meals were deposited, rather than requiring the crate to remain full while its clerk works. Completed requests move out of the pending queue into a saved history of up to 64 completed orders. A later empty crate can prompt a new phone order with a new identity.

Store management shows delivered/requested meals, the assigned courier or awaiting-courier state, and completed orders. **Observe courier #…** pauses and focuses the assigned human. The selected courier inspector identifies the active order; **Observe order #…** continues to focus the calling phone.

## Interruptions and routing

- Ending a shift permits the current load to finish, but prevents another pickup. Partial progress waits for a later eligible courier or work period.
- Task expiry, infection, death and cancelled trips release the claim. Already delivered quantities remain on the order.
- A full destination prevents further deposits; carried food is retained under the existing home-return behavior and is not falsely credited.
- Reconfiguring a store to a different crate invalidates its old pending order and immediately clears related claims, without discarding carried food or making the next save inconsistent.
- Insufficient center stock leaves the unfilled quantity pending. Designating a center or accepting a request does not create food.

Delivery routing can choose a reachable alternative when furniture isolates the preferred shelter corner. Clerks avoid repeatedly closing the entrance on an arriving order courier. Clerks also use their remembered store-shelf observations after returning from the phone, while rechecking stock at the shelf before transfer. These changes preserve physical movement and do not reveal hidden stock.

## Example and compatibility

Open **Inspect Courier Fulfillment Paused**. The store's crate holds four meals and starts empty. Its clerk calls for four; a center courier with no manual destinations accepts the order and makes two deliveries. Store management records partial progress and then completed order #1. The clerk moves the deliveries to the shelf. Center stock remains finite and protected.

This build also fixes [health loss under maximum hunger and zero energy](SURVIVAL_DAMAGE.md). Current saves use `encounter-prototype-82`; older phone-order saves require **Enable courier fulfillment**. Existing authored restart checkpoints retain their usual behavior, including shifts set before their first tick. The collapsible inspection layout from Foundation 100 remains in place.

Automated checks cover the entire phone/claim/delivery/completion chain, partial quantities, exact save continuation, competing couriers, claim expiry, shifts, donor reserves, small final loads, full/reconfigured destinations, completion history, blocked corner/door coordination, UI observation and upgrade/restart paths. Native Unity remains untested after Foundation 82; the rollback tag remains `foundation-82-unity-checkpoint`.

Foundation 102 adds a [three-store, two-courier shared-center trial](MULTI_STORE_SUPPLY_DISTRICT.md), with order-progress and stock-site observation controls.
