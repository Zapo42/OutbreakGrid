# Foundation 106: planned home → store → home trips

Target editor: **Unity 6000.6.3f1**. Native Unity testing remains paused after Foundation 82.

## Observe a trip

Open **Inspect Store Shopping Paused**. The selected consumer has an assigned home with an empty chest and a four-meal reserve. It personally remembers a store shelf from an earlier visit. The store has a clerk, six meals on the shelf and twelve in back stock. Both humans carry one personal meal: total starting food is 20.

Run, step, or use the existing 5x/10x/20x/50x buttons. The resident checks its home, inspects the doorway, walks to the store, picks up two meals for home, and returns to deposit them while retaining its personal meal. A second trip fills this fixture's four-meal target. The clerk replenishes the shelf from finite back stock. Nothing spawns food. The preview keeps the center/courier logic separate so this new consumer leg is easy to observe.

The selected-item inspector shows **Shopping: Collecting from store shelf** or **Shopping: Returning food home**, planned meals and the home-box coordinates. The map target changes from **Shopping shelf** to **Home food deposit**. Restart restores the empty home and original roles, stock and memory.

## Decisions and intelligence

A healthy, safe, fed consumer with enough energy can plan a trip from its assigned residence. Clerks and couriers retain their jobs; off-duty shopping comes with the later shift/commute work. Consumers only plan when every home box has a recent first-hand observation. Unknown, reported or stale home supplies require inspection first.

The consumer uses each box's configured effective reserve as its target. Higher intelligence triggers a trip while more of that reserve remains. For target T and intelligence I, the trigger is `min(T - 1, floor(T * (I + 2) / 12))`, with a minimum of zero. For a four-meal target, intelligence 0 waits until empty; intelligence 10 plans at three meals. A full target never triggers shopping, and even intelligence 0 can shop at zero food. This changes planning, not hidden information or danger tolerance.

Only personally remembered/visible shelves inside configured stores are grocery sources. Confirmed food is preferred over known unsearched shelves, then reachable shorter routes; empty notes are skipped. A remembered quantity can be wrong. Actual stock is checked at the shelf, with normal search time for an unsearched shelf. No live hidden stock is used to select a store.

This first increment does not discover unknown markets automatically or create universal store knowledge. A resident without a known shelf keeps its existing hunger-driven exploration/foraging behavior until it learns a source. A failed/empty shelf visit returns home; another known store can be selected on the next trip. Discovering markets earlier and switching stores during a trip are future refinements.

## Finite inventory and interruptions

Each trip serves one home box and one store shelf. Planned household goods are capped by the observed deficit and carrying space after the personal ration. A missing personal ration can also be collected within the three-meal carry limit. The resident can eat carried food when hungry, consuming a real meal and an action. Home deposits preserve its personal reserve.

The home box is claimed while the task is active, preventing duplicate household trips to that same box. Another box can have its own trip. Delivery/source jobs and shopping remain distinct. Shopping never takes the store's back-stock crate or distribution stock.

Threats, warnings, infection, low energy and task expiry interrupt the trip using existing shelter/return rules. Source and home door entry use ordinary collision-safe movement and door inspection. Clerks leave an incoming shopper's doorway open long enough to cross, extending the existing delivery courtesy. Empty shelves return no goods. A home box that fills during travel cannot overflow or delete cargo; the task releases and remaining food stays carried. If a home box is repurposed as restricted work stock, the task cancels without discarding carried food.

Trips use the existing bounded routes and a six-game-hour deadline. This is not yet the large-map/vehicle milestone. Workers' shifts, occupation outlines, commuting and store specialization remain later increments.

## Saves and validation

New scenarios use **encounter-prototype-85**. Earlier runs preserve their rules until explicitly upgraded with **Enable store shopping**. Save under another name to retain older behavior. Restart preserves the loaded generation and current enabled upgrades.

Saved shopping state includes shelf, planned quantity, home-box claim, collecting/returning phase and deadline. Malformed records and shopping state under older rules reject before replacement. Mid-trip loading resumes the same stock, movement and random continuation. The selected-only H/F/E bars, descending hunger display and testing speeds remain available.

Automated coverage checks repeated finite pickups/deposits and clerk refills, actual intelligence choices, known/unsearched/empty shelves, personal rations, duplicate claims, full destination inventory, hunger/danger/infection/expiry, malformed saves, exact continuation, old-rule preservation and UI phase/target/upgrade/restart controls. Native Unity import, rendering, input and layout remain unverified while testing is paused.
