# City life, transport and presentation workflow

## Improved development brief

Continue the Unity 6000.6.3f1 rebuild in small, independently reviewable builds. First complete the selected parking-bay and blocked-traffic recovery increment. Preserve finite inventory, assigned homes/jobs, schedules, physical trips, deterministic save/resume and bounded work on large cities.

Prioritize the reported large-city shopping defect next: NPCs leave home, wander briefly, then return without obtaining food despite an available stocked shop. Reproduce this with a seeded large-city scenario and expose the actual trip stage/reason for interruption. Fix goal continuity and doorway/outdoor routing; do not disguise failed errands as leisure.

Replace idle shifting with occasional meaningful walks when safe and free from essential duties. Keep visual idle motion separate from simulated travel: actual movement consumes energy under the existing movement rules. Emergency escape, hunger, health and active work retain priority.

Introduce a saved happiness meter ranging from **−50 to +50**, starting at a neutral baseline. Use charisma to balance the benefit of solitude versus company and indoor versus outdoor activity. Charisma influences preferences rather than granting knowledge of hidden people or places. Start with documented provisional weights and let observation/testing guide tuning; do not assume everyone enjoys crowds or outdoors equally. Show the value and current contributing factors in the selected NPC inspector.

Then add known leisure destinations incrementally: parks for outdoor walks and evening-opening bars for social visits. Respect opening hours, danger, travel distance, jobs, home supplies, hunger and energy. Test a modest happiness-related reduction in energy consumption only after happiness and leisure work; every real movement must still have an energy cost. Record and tune the benefit rather than making unlimited free travel possible.

Improve transport in separate increments: modestly faster cars with matching movement/turn presentation; saved vehicle types and distinct colors; bicycles with distinct movement, effort and carrying limits; finite fuel for motor vehicles, consumption, refueling and clear empty-fuel behavior. Bicycles do not consume motor fuel. Introduce fuel through an explicit supply model without adding weapons to storage.

Refresh graphics toward crisp, detailed pixel-like tiles and sprites inspired by Dwarf Fortress, while preserving existing colors, detail, readable occupations, building identity and object states. Keep simulation cells separate from sprite resolution. Begin with a small representative set before replacing all visuals.

Group historical previews under an **Old Test Scenarios** menu. Keep the newest/current milestone tests prominent and retain access to regression fixtures. The menu reorganization must not remove authored scenarios or their restart behavior.

## Increment queue

1. **Foundation 126: parking bays and blocked-traffic recovery (implemented).** Off-lane single-cell bays, reserved courier destinations, safe merges, bounded detours, saved waiting state and observable tests. No vehicle teleporting, pushing pedestrians aside or ignoring lane direction.
2. **Large-city shopping regression.** Seeded reproduction, persistent store destination and informative interruption reasons; verify home → store → home food delivery over longer routes.
3. **Scenario menu organization.** Current tests at the top; collapsible Old Test Scenarios collection containing historical previews.
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
