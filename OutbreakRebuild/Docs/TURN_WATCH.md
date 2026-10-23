# Foundation 85: turn watch

The encounter summary shows the next pending actor and remaining simulation seconds. At five seconds or less it changes from **Next turn** to **Turning soon**. Living infections and corpses with reanimation enabled are included; equal deadlines use actor identity order.

**Watch next turn** focuses the earliest pending actor and starts playback at the existing speed. It executes ordinary simulation ticks, then pauses and focuses the earliest pending actor one tick before conversion. Click **Step 1 tick** to observe the turn. If the next turn is already one tick away, the watch leaves the simulation paused immediately. This feature does not fast-forward or skip ticks, including on long infection settings.

The next pending actor is reassessed every tick, so the watch follows earlier deadlines that arise during play. It is a one-shot watch, cleared after pausing. **Cancel turn watch** cancels the watch while playback continues. Manual pause, main menu, restart, and loading another world clear the watch. If no pending turns remain, watching ends and ordinary playback continues. The watch is a session control, not saved simulation state.

Try a run with a pending infected human or corpse, then choose **Watch next turn**. **Inspect Rising Zombie Paused** already starts one tick before conversion and therefore pauses immediately; **Step 1 tick** shows the get-up phase.

No rules-version, infection timing, save format or scheduler changes. Automated UI checks cover countdown thresholds, exact stopping under a large 3x frame, equality with ordinary deterministic tick execution, immediate pause when already imminent, stepping to conversion, empty-list availability, cancellation, manual pause and world replacement.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
