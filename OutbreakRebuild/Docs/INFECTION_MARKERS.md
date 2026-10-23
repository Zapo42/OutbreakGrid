# Foundation 84: infection markers

Visible living humans with an active infection deadline show one orange square inside the upper-right corner of their body. Human corpses awaiting reanimation show two pink squares. Their different shapes distinguish the states as well as their colors. Healthy humans, uninfected corpses, and zombies do not receive these markers. Old rules without corpse reanimation do not show pending corpse markers.

Markers follow the rendered body and remain visible during selection and contact flashes. The existing health bar and grey-to-green get-up transition remain separate. On conversion the infection marker disappears, leaving the zombie transition. Markers are an observer aid; humans do not learn infection status or change decisions because of them.

The inspector includes **Hide infection markers / Show infection markers** and a legend. This display preference remains in the current application session across restart and scenario changes; it is not stored in simulation saves. Markers default to visible on application startup.

To inspect: open **Inspect Reanimation Escape Paused** to see a pending corpse marker; step once to see it disappear as reanimation begins. **Next infected human** or **Soonest turn** can focus an infected human in a run. Toggle the display to hide or show markers.

This update changes presentation only, with no rules-version or save format changes. Automated UI checks cover marker shape/count, selection visibility, display toggle and state preservation, conversion removal, restart, and bodies without infection.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
