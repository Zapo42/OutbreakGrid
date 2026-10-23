# Foundation 91: shelter shortage overview

The simulation inspector now includes **SHELTER SUPPLIES**, a live player census of discovered shelter rooms. It shows current occupants, hungry and very hungry occupants, meals carried inside, meals in boxes and shelves, missing box reserves, and assigned restocking trips.

## Status definitions

- **NO FOOD:** living humans are physically inside and the room has zero meals.
- **LOW FOOD:** the room has fewer than one total meal per current occupant.
- **BELOW RESERVE:** immediate meals cover occupants, but at least one box is below its own food reserve target.
- **SUPPLIED:** current meals cover occupants and active box reserve goals are met. Hungry occupants can still be present while waiting to eat.
- **UNOCCUPIED:** no living humans are inside and no restocking trips are assigned to that room. An unused building is not flagged simply because its boxes are empty.

A shortage is flagged for insufficient immediate meals, or a positive reserve deficit in a room with occupants or assigned restockers. A restocker who has left the room remains an assigned trip, so that home's reserve shortage stays visible while they collect. Trip counts do not add extra occupants or count their outside carried meals as already delivered.

Hungry means hunger 40 or above; very hungry means 80 or above. Zombies and human corpses are excluded from occupants. Inbound reservations are not occupants until physically inside. Living infected humans inside still count as occupants.

Food totals include actual shelf contents, including unsearched food, plus box stock and carried meals physically inside. Outdoor supplies are excluded. Reserve deficits are summed per box: surplus in one box does not cancel another box's target. Zero targets create no reserve gap. Older food rules without restocking do not gain active reserve goals from the overview.

These are player stock totals. They do not reveal inventory to humans, change personal observations, assign jobs, or advance time. A food status measures quantities; it does not assess zombie safety or guarantee that every occupant can immediately access every meal.

## Controls

**Next shelter shortage** pauses, clears placement tools, zooms to room scale and centers the next flagged room in stable room-ID order, wrapping at the end. It is disabled when there are no shortages. **Show all shelters** includes supplied and unoccupied rooms; **Show shortages only** restores the default filter. Each displayed room also has **Focus shelter #ID**. A focused room stays visible if its shortage resolves, so the result can be observed.

**Inspect Shelter Shortages Paused** provides three rooms: one with hungry occupants and no food, with a restocker assigned; one with adequate food; and one unoccupied. Restart and loading recompute totals and clear the focused room. The all/shortage display preference remains a session setting.

## Validation and compatibility

The overview is computed from existing saved state, so no simulation rules or save schema change is required. Existing food-enabled runs can use it immediately. Maps without discovered shelters show an empty-state message, and existing shelter scan limits remain applicable.

Automated checks cover stable room IDs, read-only/defensive results, exact food and hunger counts, independent box deficits, empty and temporarily vacated rooms, inbound humans, corpses and zombies, deposits/withdrawals/eating/conversion, save restore, old rules, focus/camera/zoom, filters, cycling, tool cancellation, restart and empty maps.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
