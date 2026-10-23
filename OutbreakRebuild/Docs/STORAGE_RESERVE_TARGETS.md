# Foundation 88: adjustable storage reserves

Each storage box now has its own desired food reserve. This is a restocking goal, separate from inventory capacity. A 30-item box can aim to hold six meals, twenty meals, or any whole number from zero to thirty.

## Controls

1. Select a box or use **Next storage box**.
2. Enter a whole number in **Food reserve target**, then click **Apply reserve**. Applying pauses the simulation.
3. Alternatively choose **Restocking off**, **Default reserve**, or **Fill to capacity**.

**Default reserve** restores six meals, capped by the box's capacity. Invalid text, negative values and targets above capacity disable Apply and leave the saved setting unchanged. Each box is configured independently. Changing a target does not change capacity or create, destroy or transfer food.

## Restocking behavior

Humans start deliberate restocking trips only when a box's observed food is below its target. Meeting the target stops new assignments. Pickups and deliveries happen in batches, so a delivery may exceed the desired reserve while remaining within capacity. Ordinary surplus deposits may also take the stock above the target.

A target of **zero** disables deliberate restocking for that box. It still accepts ordinary surplus deposits, and hungry humans can withdraw meals. A collector whose target becomes zero stops collecting on its next decision and requests a return home. A human already carrying supplies brings them back and can deposit the surplus normally. Existing carried food and box inventories are preserved.

A lowered positive target is respected when starting new trips, and a collector who can see that its box already meets the new target stops optional collection. Humans away from a box do not learn hidden stock changes. Existing danger, energy, hunger and personal food-reserve rules still apply.

## Persistence and older runs

Settings are saved with each box in maps and running saves. Explicit zero is distinct from an omitted setting. Older boxes without a setting retain their original six-meal default. Restart restores the starting checkpoint's target settings along with its food; edits made before the first simulation tick become part of that authored starting setup.

Foundation 87 runs retain fixed targets until **Enable reserve targets** is selected. Current rules are `encounter-prototype-70`; generation 69 remains `RestockingVersion`. Maps containing custom reserve settings use schema 9. Restoring every target to its default allows the map to use its previous schema again. Earlier builds cannot load custom-target maps or current-rules saves.

## Validation

Core checks cover independent targets, explicit zero, default compatibility, map/save roundtrips, defensive copies, invalid edits and malformed saves, actual restocking to a custom target, interruption during collection and return, food conservation, and prior rules. UI checks cover text entry and validation, Apply, all presets, pause/state preservation, checkpoint restart, upgrade and energy retention.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
