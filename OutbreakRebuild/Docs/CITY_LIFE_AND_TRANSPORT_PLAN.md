# City life, transport and presentation workflow

## Improved development brief

Continue the Unity 6000.6.3f1 rebuild in small, independently reviewable builds. First complete the selected parking-bay and blocked-traffic recovery increment. Preserve finite inventory, assigned homes/jobs, schedules, physical trips, deterministic save/resume and bounded work on large cities.

Prioritize the reported large-city shopping defect next: NPCs leave home, wander briefly, then return without obtaining food despite an available stocked shop. Reproduce this with a seeded large-city scenario and expose the actual trip stage/reason for interruption. Fix goal continuity and doorway/outdoor routing; do not disguise failed errands as leisure.

Replace idle shifting with occasional meaningful walks when safe and free from essential duties. Keep visual idle motion separate from simulated travel: actual movement consumes energy under the existing movement rules. Emergency escape, hunger, health and active work retain priority.

Introduce a saved happiness meter ranging from **−50 to +50**, starting at a neutral baseline. Use charisma to balance the benefit of solitude versus company and indoor versus outdoor activity. Charisma influences preferences rather than granting knowledge of hidden people or places. Start with documented provisional weights and let observation/testing guide tuning; do not assume everyone enjoys crowds or outdoors equally. Show the value and current contributing factors in the selected NPC inspector.

Then add known leisure destinations incrementally: parks for outdoor walks and evening-opening bars for social visits. Respect opening hours, danger, travel distance, jobs, home supplies, hunger and energy. Test a modest happiness-related reduction in energy consumption only after happiness and leisure work; every real movement must still have an energy cost. Record and tune the benefit rather than making unlimited free travel possible.

Consumer vehicle shopping should mitigate long-distance food consumption through shorter travel time, lower exertion and larger carried loads. Build home → vehicle → shop → vehicle → home shopping on the existing courier driving/parking foundation, with physical boarding, transfers and safe fallback. This is the next transport increment; do not treat long walking-trip food costs as the intended permanent solution.

Improve transport in separate increments: modestly faster cars with matching movement/turn presentation; saved vehicle types and distinct colors; bicycles with distinct movement, effort and carrying limits; finite fuel for motor vehicles, consumption, refueling and clear empty-fuel behavior. Bicycles do not consume motor fuel. Introduce fuel through an explicit supply model without adding weapons to storage.

Refresh graphics toward crisp, detailed pixel-like tiles and sprites inspired by Dwarf Fortress, while preserving existing colors, detail, readable occupations, building identity and object states. Keep simulation cells separate from sprite resolution. Begin with a small representative set before replacing all visuals.

Group historical previews under an **Old Test Scenarios** menu. Keep the newest/current milestone tests prominent and retain access to regression fixtures. The menu reorganization must not remove authored scenarios or their restart behavior.

## Increment queue

1. **Foundation 126: parking bays and blocked-traffic recovery (implemented).** Off-lane single-cell bays, reserved courier destinations, safe merges, bounded detours, saved waiting state and observable tests. No vehicle teleporting, pushing pedestrians aside or ignoring lane direction.
2. **Foundation 127: large-city shopping regression (implemented).** Seeded reproduction, persistent store destination and informative interruption reasons; verify home → store → home food delivery over longer routes.
3. **Foundation 128: scenario menu organization (implemented).** Current tests at the top; collapsible Old Test Scenarios collection containing historical previews.
4. **Purposeful free-time walking.** Stable outdoor walk targets and returns, occasional safe walks, actual energy accounting. Preserve zero-cost visual idle animation separately.
5. **Happiness foundation.** Saved −50…+50 value; charisma-based social/environment preferences, inspector explanations, exact continuation.
6. **Parks.** Distinct generated/authored destination and safe bounded leisure walks.
7. **Evening bars.** Building identity, opening hours and interruptible social visits.
8. **Happiness and energy tuning.** Modest benefit with a nonzero movement cost, observed over full daily routines.
9. **Vehicle speed.** Increase speed modestly; verify braking, turning, crossing safety, boarding/arrival timing and fast-forward.
10. **Vehicle types and colors.** Saved identity, readable silhouettes and modest type-specific capacities/speed where justified.
11. **Bicycles.** Distinct transport with stamina costs, capacity and parking rules.
12. **Motor fuel.** Finite fuel, consumption, refueling jobs and safe recovery when empty.
13. **Pixel presentation trial**, then staged rollout after visual review; preserve palette/detail and avoid coupling rendering to simulation.

Each increment: update the workflow status, run relevant core/UI/input and configuration checks, update BUILD_VALIDATION.md, publish the completed build with its ZIP link, and offer a few next options. Foundation 82 remains the native Unity rollback checkpoint until the user explicitly resumes Unity testing. Deferred queue entries are requirements, not claims of implemented behavior.

Foundation 126 automated validation: 594 full core cases / 731177 assertions, 18 final focused courier/parking cases, 2123 UI assertions and 3 NewOnly input assertions. Native Unity remains untested. [Usage and exact limits](PARKING_AND_TRAFFIC_RECOVERY.md). Next queued fix: reproduce and resolve interrupted shopping in larger generated cities.

Foundation 127 reproduces the no-known-shop departure loop and long-walk energy return. Generated residents now have persistent familiar shop locations, safe paid rest breaks retain the trip, and the inspector explains activity/interruption. [Exact scope and usage](CITY_SHOPPING_CONTINUITY.md). Remaining long-distance food-capacity limits are documented.

User steering after Foundation 127: vehicle use should mitigate food consumption. Prioritize consumer shopping by vehicle as the next transport option, with driving reducing time/exertion and vehicle cargo increasing the useful load brought home. Preserve needs and finite inventory; do not simply turn hunger off while driving.

### Consumer vehicle shopping: cargo and cart requirements

Use larger vehicle cargo storage, separate from the NPC's personal food inventory. Add a finite shopping-cart/haul inventory for each shopping trip so a resident can leave the car parked and make multiple shelf → cart → vehicle loading trips before driving home. Keep the cart load distinct from personal rations; its capacity and movement cost should be explicit and saved. Model unloading at home physically too, with multiple haul trips if necessary.

Conserve items at every step: shelf → shopping cart → vehicle cargo → home haul/cart → home storage. Capacity, target deficit and stock limit each actual transfer. Avoid remote pickups, duplicated food, invisible infinite carry capacity or counting reserved food as already delivered. Save/load, interruption, abandoned carts, blocked parking and excess-load returns need incremental checks. Vehicle types can later vary cargo capacity; the first consumer-driving build should establish this parked-car/cart cycle before fuel or additional cosmetic variants.

Foundation 128 keeps six recent previews directly accessible and moves historical scenarios into a collapsed-by-default Old Test Scenarios collection. All original launch handlers remain available. [Menu guide](SCENARIO_MENU.md).

Foundation 129 implements the first pure-consumer vehicle shopping cycle: 48-meal cargo, six-meal cart loads, physical repeated loading/unloading, saved phases, excess retention and household cart recovery. It also fixes the reported depleted-nearby-store/hungry-return loop using bounded additional familiar shop locations and necessary food trips when home is known empty. [Scope and controls](CONSUMER_VEHICLE_SHOPPING.md). Worker driving, automatic ownership, fuel and vehicle types remain later increments.

Foundation 130 adds two-tick road travel with matching visual turn/translation timing and eight saved, selectable paint colors. Older runs retain three-tick travel until explicitly upgraded. [Controls and compatibility](VEHICLE_SPEED_AND_COLORS.md). Purposeful free-time walks and the happiness foundation remain upcoming choices.
