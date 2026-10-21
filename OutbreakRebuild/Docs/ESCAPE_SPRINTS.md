# Foundation 76: Escape sprints

Humans in Flight can take an extra escape step between their normal decisions when a visible zombie is within six cells and they have at least **30 energy**. The extra step falls two ticks after the normal opportunity, giving a 2/3-tick running cadence instead of walking every five ticks while eligibility holds.

Each extra sprint step costs **2 energy** on a straight move or **4** diagonally. Regular steps retain their existing walking cost. Falling below 30 energy stops bonus steps; ordinary retreat continues. Infection does not prevent a still-human actor from escaping, and conversion returns it to normal zombie behavior.

Bonus opportunities only perform movement. They do not repeat stress decisions, warnings, eating, searching or combat actions. They require current sight of a nearby zombie and an existing Flight reaction. Frozen/staggered humans, active combat cooldowns and door bracing prevent sprinting. Sprint movement respects walls, occupancy and the existing route geometry, refuses steps closer to the nearest observed zombie, and cannot enter or leave a door cell. Normal decision and door-crossing pace remains unchanged.

Movement interpolation follows the shorter intervals. If a new step interrupts a prior visual segment, it continues from the rendered position through the prior waypoint rather than jumping forward or cutting its corner. Pausing and manual stepping continue to show exact authoritative positions.

## Try it

Choose **Inspect Escape Sprint Paused**, then **Run**. The human in the corridor starts at x20 with a zombie at x24:

- Tick 1: normal retreat to x19; 99 energy.
- Tick 2: zombie pursues to x23.
- Tick 3: extra escape step to x18; 97 energy.

Continue to see sprint eligibility stop when energy runs low or the zombie falls too far behind. The inspector shows **Escape sprint ready: extra steps cost more energy** while the human meets the reaction, energy and sight conditions; blocked routes can still prevent an actual step. Restart restores the starting chase.

New runs use encounter rules 61. Foundation 75 saves retain rules 60 until **Enable escape sprints** is clicked. Earlier upgrades enable current behavior too. Existing actor/save fields retain exact sprint cadence through stable identity, tick, energy, reaction and sight; no extra persisted sprint timer is needed.

Automated checks cover actual extra movement and energy costs, unchanged zombie and decision cadence, diagonal cost, low-energy cutoff, wall/door/visibility limits, interpolation continuity, exact save continuation, earlier walking rules and UI upgrade/restart controls. UI checks use narrow Unity API doubles. Native Unity rendering and input require editor testing.
