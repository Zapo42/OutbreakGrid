# Foundation 89: box memory

Humans now remember up to eight storage boxes they have personally seen. An observation records the box location, last-seen food stock, free space and observation tick. Observations remain for six game hours (3600 ticks) unless refreshed. When memory is full, newer observations replace older ones; distance and stable cell order break ties. Visible observations from the same tick favor nearer boxes.

## Using remembered supplies

A hungry human can return around a corner to a remembered stocked box after losing sight. A shelter occupant carrying surplus food can walk to a remembered box in the same room. A remembered low box can receive a restocking assignment even when currently out of sight. Existing shelter-room boundaries, safety checks, food reserves, capacity and physical routes remain in effect.

Knowledge changes only through sight or the observer's own transfers. A hidden box being emptied does not immediately change the human's memory or route. A remembered empty box refilled out of sight stays remembered empty until seen again. Seeing the box refreshes its current stock and space; a deposit or withdrawal updates the acting human's record immediately. Other humans retain their own observations.

A returning restocker keeps its assigned box destination even if other observations displaced the box's memory entry. That task does not reveal its hidden stock. Player reserve settings remain explicit orders; setting zero can still stop deliberate collection. Personal stock memory does not override capacity or create food.

Memory expires on simulation ticks and pauses with the simulation. Saving retains exact observations and age. Conversion and death clear human box memory. Restart restores the scenario's original knowledge and inventory state. Box locations are not shared with other humans in this increment.

## Inspect it

Choose **Inspect Box Memory Paused**. The fixture begins after a real box observation, with the hungry observer now on the other side of a wall. Run or step 100 ticks to watch the human go around the corner, refresh the observation, withdraw three meals and eat one later.

Select a human for **Remembered boxes: N/8**, followed by last-seen meals, free slots and observation age. These labels show personal knowledge. Selecting a box or viewing player food totals still shows current inventory for the player.

## Compatibility and validation

Foundation 88 saves retain sight-only box behavior until **Enable box memory** is selected. The current rules are `encounter-prototype-71`; generation 70 remains `ReserveTargetsVersion`. Actor observations use an optional saved array. Map schema and box inventories are unchanged. No old run gains observations without an explicit upgrade and a subsequent sighting.

Core checks cover routing around walls, food conservation and save continuation, hidden stock privacy for full and remembered-empty boxes, refreshing at sight, in-room deposits and assignment from memory, bounded eviction and canonical order, defensive copies, exact expiry, malformed observations, old rules and conversion cleanup. UI checks cover the controlled scene, personal knowledge labels, read-only rendering, refresh after withdrawal, hidden-current-stock isolation, restart and upgrade without resetting energy.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
