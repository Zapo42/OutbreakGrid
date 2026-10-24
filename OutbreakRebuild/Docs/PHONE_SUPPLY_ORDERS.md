# Foundation 100 — phone replenishment orders

An empty store supply crate can now prompt a clerk to make a replenishment order using a physical phone block. This implements the store-request stage of the store → distribution center → courier supply chain. Pending requests do not yet dispatch couriers or create supplies.

## Set up

1. Configure the store's supply crate and assign a clerk as in Foundation 98.
2. Open **Show Placement tools**, choose **Place phone block**, then click a free indoor floor cell in the store's room. Phones are cyan solid furniture. Leave an adjacent walkable cell for the clerk.
3. Run or step. After observing an empty crate, the clerk finishes stocking food already carried, walks to a reachable phone and makes the call over two ordinary action opportunities.
4. Open **Store management** to see pending order ID, store, quantity, caller and phone. **Observe order #…** focuses the phone and opens its inspection details.

Clerks use personally observed or remembered crate information. An empty crate hidden from a clerk does not reveal itself through the player overview. A clerk can learn its state by seeing it or physically reaching it during stock work. Observing replenished stock before the call completes cancels that unfinished call. A missing or unreachable phone prevents ordering; it does not teleport the clerk or generate an order remotely.

Hunger, low energy, infection and danger interrupt phone work under existing clerk safety rules. A saved walking or calling task resumes through the same normal decisions. Multiple clerks may approach phones, but the store gets only one pending order. Another clerk seeing that pending request does not place a duplicate.

The order requests the supply crate's full capacity, currently thirty meals for a default crate. It records the exact store, crate, phone, clerk and creation tick. Pending records and their identity counter persist in running saves under encounter rules 80. They remain pending for the upcoming acceptance/fulfillment increment, even if the caller dies or turns. Reconfiguring a store to use a different crate invalidates its old pending order and phone tasks while preserving carried stock; new calls receive new order IDs.

## Phone maps and compatibility

Phones are independent furniture on indoor floor terrain, preserving the room's identity. They block walking, spawning and overlapping furniture placement. Phone-bearing map exports use map schema 11. Phone placement rejects occupied cells and reserved actor destinations. Phone locations survive reusable map export/import; store assignments, callers and pending requests remain running-scenario state.

Older runs require **Enable phone supply requests**. Existing scenarios without phones do not suddenly place orders. Scenario restart restores its authored phone, store and inventory checkpoint and removes orders created after it began.

## Example

Open **Inspect Phone Supply Order Paused**. The supply crate is empty, and a cyan phone stands at (2, 5). **Step 10 ticks** reaches the calling phase. Continue stepping to create order #1 for thirty meals. Observe that order to inspect its phone. Continued simulation does not create duplicate orders or add food to the empty crate.

Automated checks cover physical walking/calling, exact saved continuation, missing/blocked phones, competing clerks, refill/infection interruptions, finite inventories, furniture collision and map versions, malformed/legacy orders, UI placement/inspection and restart/upgrade paths. Native Unity remains untested after Foundation 82; `foundation-82-unity-checkpoint` remains unchanged.
