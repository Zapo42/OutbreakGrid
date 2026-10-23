# Foundation 92: shelter supply deliveries

A resident of an understocked shelter can now collect surplus food from a known box in another shelter and carry it home. This extends existing restocking jobs: the receiving shelter initiates the trip, and the human retains that room as home while visiting the donor.

## Choosing supplies

The human must meet the existing restocking eligibility: fed, calm, uninfected, at least 50 health, enough carrying space, and at least 60 energy. They must know a reachable donor box from personal observations or a report. The remembered food must exceed the donor box's current reserve target. A valid donor must be in another discovered shelter room; outdoor boxes are not donor shelters in this increment. Known danger, rejected rooms and full donor-room admission prevent assignment.

A known suitable donor box is preferred over a remembered shelf. Existing shelf restocking remains available when there is no usable donor. One restocker can claim each receiving box, while a large donor can serve several different receiving boxes. This does not introduce automatic priorities between struggling shelters.

## Physical transport and protected reserves

The courier checks the home exit, walks to the donor shelter, opens its door, enters, and takes food from beside the box. Pickup consumes a normal action and rechecks actual inventory:

`take = min(carry space, max(0, donor food - donor reserve))`

Every pickup uses current stock and the current donor reserve, so stale memories and concurrent couriers cannot take reserved meals. A donor reserve of zero permits all its food to count as surplus; zero still disables deliberate restocking into that box. Changing a reserve does not undo food already collected.

The courier returns through the existing shelter/door movement, carrying the actual meals in human inventory, and deposits surplus into the assigned receiving box. The normal personal reserve remains carried. Courier cargo is kept for the return rather than being put back into the donor's pantry. Walking consumes energy; safe rest can occur before departure. Doors are closed only when physically adjacent and unoccupied, including an abandoned donor-door inspection.

Hunger, stress, danger or low energy can interrupt collection. A zombie revealed when opening the donor door prompts a rejected-room memory and return attempt. A hungry courier can eat before depositing at home. If the receiving target is turned off after pickup, the loaded courier can still return and deposit ordinary surplus. Infection, conversion and the existing task timeout release assignments; carried food follows ordinary survival/inventory behavior.

## Player views

The human inspector shows **Supply delivery: Collecting / Returning**, the receiving box, home room and donor box. The food overview counts **Box-to-box deliveries** as a subset of restocking trips. Shelter rows show incoming and outgoing assignments. Food in transit counts as carried inventory where the courier physically is, and is not added to the receiving box before deposit.

Choose **Inspect Supply Delivery Paused** for a courier in a shelter with an empty box and remembered surplus in another shelter. Run or step: the first complete trip moves two meals, changing donor stock from 12 to 10 and recipient stock from 0 to 2 while the courier keeps one. Subsequent trips depend on remaining surplus, personal needs and safety. Restart restores original inventories and clears the assignment.

## Compatibility and validation

Foundation 90–91 saves retain prior behavior until **Enable supply deliveries** is selected. Rules are now `encounter-prototype-73`; generation 72 remains `BoxReportsVersion`. An optional actor source-box field saves courier assignments. Existing map schema and food-only inventory format are unchanged. Prior rules reject active courier source fields.

Automated checks cover complete round trips and door closure, movement energy, conservation and personal reserves, saved collection and return, known-surplus eligibility, hidden donor stock changes, depleted and concurrent donors, target cancellation, newly revealed zombies, hungry loaded couriers, malformed source state, infection cleanup, old rules, UI counters/inspection, upgrade and restart.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).

Foundation 93 adds [per-box donation controls](DELIVERY_CONTROLS.md) while preserving local transfers and already collected cargo.
