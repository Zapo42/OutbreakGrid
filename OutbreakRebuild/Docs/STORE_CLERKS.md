# Foundation 98 — store clerks and shelf replenishment

This first store increment connects existing food shelves, finite storage inventories and human hunger. A store uses one discovered room, a designated secondary supply crate, that room's shelves and assigned clerks. Consumers are ordinary hungry humans using the existing shelf pickup/eating behavior.

## Set up a store

1. Place shelves and a storage box inside a discovered room. The box is the store's secondary/back-stock supply crate.
2. Select that box and click **Use as store supply crate**. The room becomes a store, with a default target of six meals per shelf.
3. Select a living, uninfected human inside that room. Clear any courier assignment and finish active supply work first, then click **Assign clerk to current store**.
4. Open **Store management** to inspect the store's clerk count, shelf meals, empty shelves and back-stock meals. Run or step to observe the clerk replenishing shelves.

A room has one configured store and one supply crate. Reconfiguring another box in that room changes its supply crate without duplicating the store. All shelves in that room participate. Multiple clerks can work in a store; they recheck actual stock at each transfer, so simultaneous visits cannot create food or overfill the shelf target.

Clerks start this increment inside their store. Dedicated commuting and clerk work shifts are later extensions, separate from existing courier schedules. Home assignments remain unchanged. Clerks and couriers are distinct assignments: clear a courier route before assigning its human as a clerk. **Unassign clerk** stops that role while retaining carried food.

## Shelf and crate behavior

Clerks choose visible shelves below the store's stock target, walk to a usable face of their supply crate, collect a batch into their existing carried-food inventory, walk to the shelf and deposit surplus meals. Each collection/deposit takes an ordinary action. Movement uses normal collision, occupancy and energy costs. A healthy clerk keeps one personal meal; the usual personal reserve rules still apply. Stocked shelves become searched/ready for ordinary consumers to take food.

A consumer taking the last meal leaves a genuinely empty shelf. The clerk replenishes it only from finite carried or back-stock food. Ordinary consumers cannot use the secondary crate directly or treat its contents as accessible room food; they use the shelves. Clerks may use their own crate for personal meals when hungry. Automatic outside box-donation selection skips store crates. Explicitly assigned courier deliveries can still deposit into them, preparing for the next supply-chain increments.

Moving food from the store's crate to its own shelf is an internal stock transfer, not an outgoing donation. It can use the crate's back stock regardless of its normal outgoing reserve. Household/donor settings continue to apply to courier transfers under the existing rules. Player store totals do not grant clerks hidden inventory knowledge: shelf selection uses sight, and crate collection requires reaching an adjacent face.

Hunger, infection, danger and low energy pause shelf work; ordinary eating, shelter and recovery remain available. Death or conversion clears the clerk assignment. Carried food remains part of the existing human inventory and food totals. No new inventory or free replenishment pool is introduced.

## Increment boundary and example

Empty back-stock crates are shown as **crate empty — needs supplies**. This build stops there: it does not yet create courier requests, assign distribution centers or dispatch deliveries automatically. Those are the next stages of the requested store → courier → distribution-center loop.

Open **Inspect Store Clerk Paused**. The store has one shelf with one meal, a back-stock crate with twelve meals, a clerk with one personal meal and a hungry consumer. **Step 10 ticks**: the consumer has emptied the shelf and eaten, while the clerk carries back stock. Continue stepping: the clerk fills the shelf to six meals, leaving six in the crate. Food totals reflect one meal eaten and the finite transfers. Restart restores original inventories and assignments.

Stores, their supply crates, shelf targets, clerk assignments and current shelf tasks persist in running saves under encounter rules 78. Older saves require **Enable store clerks**. Scenario restart retains its existing checkpoint behavior. Store/clerk assignments are scenario state rather than reusable terrain-map metadata.

Automated checks cover consumption and physical replenishment, exact saved continuation, empty crates, consumer access restrictions, competing clerks, role edits and retained cargo, conflicting courier roles, malformed/legacy records, conversion cleanup and UI setup/upgrade/restart paths. Native Unity remains untested after Foundation 82; `foundation-82-unity-checkpoint` remains unchanged.

Foundation 99 adds [distribution-center stock and assigned couriers](DISTRIBUTION_CENTERS.md). Persistent store requests and automatic fulfillment remain the next supply-chain stages.
