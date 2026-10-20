# Foundation 43: food scarcity trial and longer-run validation

This build adds a repeatable food-balance scenario and a one-game-hour step control. The trial has twelve humans, four rooms, 24 starting meals and no zombies. Some supplies are behind closed doors, one room has an empty shelf, and two outdoor shelves provide exposed food and a known empty source. Starting hunger is 60, 70 or 80; bravery spans -3, 0 and +3. This isolates the food loop while still exercising crowding, shared shelves, pantry storage, room exploration and doors.

## Try it

1. Choose **Inspect Food Scarcity Trial Paused**.
2. In the right-hand **FOOD OVERVIEW**, click **Step 1 game hour**. It advances 600 ordinary simulation ticks and remains paused. In the default trial, seven meals remain after the first hour: two on shelves and five in inventories. All twelve humans have eaten at least once.
3. Click it twice more. After three game hours, all 24 starting meals have been consumed and the overview shows **Food supply exhausted**. Some people are still full from their last meal; hunger rises over subsequent hours.
4. Keep stepping to watch scarcity. The overview includes **Investigating rooms**, while **Next hungry human** and the target markers help inspect individual decisions. Existing **Place stocked shelf** can introduce six new meals on a free cell near a hungry human to inspect recovery.

The hour button is available in food-enabled runs, is disabled during playback, and uses the same movement, search, sharing, eating and timer logic as the smaller step buttons. It clears tick debt and does not change playback speed. The existing header buttons keep their smaller batches.

## Measured balance baseline

Three seeded trials each ran 7,200 ticks, or twelve game hours. These are authored scenario results, not a guarantee about every map or population.

| Seed | All twelve first fed by tick | All 24 meals consumed by tick | Meals remaining at tick 7,200 |
| --- | ---: | ---: | ---: |
| 214 (menu default) | 181 | 1,594 | 0 |
| 123 | 174 | 1,891 | 0 |
| 999 | 181 | 1,591 | 0 |

Every tick reconciled all food on shelves and in inventories against actual meals eaten. All 24 meals were accounted for in every seed. Every human moved after total depletion, room investigations continued, and needs, inventory and notebook sizes stayed bounded. Hourly checkpoints validated occupancy and restored full saved state.

Separate checks serialized and resumed runs across the carried-food and exhausted-food phases, with identical future state after resuming. After six game hours of scarcity, placing one ordinary six-meal shelf beside a hungry human restarted eating without losing or duplicating the new food.

The player overview now reports total remaining meals and room investigations. These are read-only global diagnostics; humans still make decisions from their own observations and memories. No food-choice, hunger-rate or risk-tolerance rule was changed in this pass. Saved rules remain encounter-prototype-37, so Foundation 42 runs need no behavior upgrade.

Hunger still caps at 100 without starvation damage or death. Exploration still investigates one point per room visit, rather than sweeping every corner of large or irregular rooms. The trial verifies the current basic food loop; starvation consequences, timed eating and the larger town supply economy remain separate work.

Automated checks execute production simulation and UI handlers with narrow Unity API doubles. Native Unity rendering and input still require local testing.

Validation: 251 simulation cases / 317,530 assertions; 633 legacy-input UI assertions and 3 NewOnly assertions. The UI checks exercise paused and disabled hour stepping, exact 600-tick continuation, refreshed supply totals, total depletion and retaining the run through the main menu. Project configuration and whitespace checks passed.
