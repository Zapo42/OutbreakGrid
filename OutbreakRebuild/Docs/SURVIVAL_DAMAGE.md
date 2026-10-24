# Foundation 101 — hunger and exhaustion health loss

Living humans now lose one health point every 30 ticks while both hunger is 100 and energy is zero. This is six seconds at 1× playback speed, or three in-game minutes. Damage uses the same deterministic needs interval as hunger updates, so pause, speed controls, manual stepping and save/reload behave consistently.

Eating enough to bring hunger below 100, or recovering any energy, stops this damage. Hunger or low energy by itself does not trigger the combined deprivation penalty. Ordinary health recovery, food use, energy costs and danger behavior remain active.

The selected human's **Vitals & needs** section shows **Deteriorating: maximum hunger and no energy**. Open **Inspect Hunger Damage Paused** and step 50 ticks: the human starts at ten health and loses one point. The fixture has no food or rest access so the condition is easy to observe.

At zero health, the human becomes a corpse and active work, claims, clerk/courier/home assignments and combat state are cleared. A held bat drops using the existing death behavior. The inspector identifies **Died from hunger and exhaustion** rather than incorrectly calling the death a mauling.

An uninfected human does not become infected just because they die from deprivation. An already infected human retains their scheduled turn deadline and reanimates under the existing corpse/get-up rules. The human death-cause marker is cleared on conversion.

Older rules retain their earlier behavior until explicitly upgraded. Current-rule long scarcity checks now allow deprivation deaths and distinguish eating from the hunger-state reset on death; they still account for the original finite food supply. Dedicated checks cover the damage cadence, food/energy protection, saved continuation, equipment/death cleanup, infected reanimation, courier claim release and inspection labels.

Native Unity testing remains paused after Foundation 82. This correction is covered by automated core and UI checks; it is not recorded as newly tested in Unity.
