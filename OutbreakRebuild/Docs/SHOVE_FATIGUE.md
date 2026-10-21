# Foundation 70 / shove fatigue

Each successful defensive shove now adds one fatigue point, including a blocked push that still staggers its target. At 3/3, an unarmed human prioritizes escape instead of shoving again. Fatigue is separate from stress and does not add a random roll or change shove damage.

Recovery requires the shove cooldown to have ended, no visible zombie, no recent warning or door impact, and no infection. One point recovers per normal human opportunity under those conditions. Fatigue cannot be recovered while a zombie remains visible. Infection conversion clears human fatigue; bat fighting retains its existing behavior.

Choose **Inspect Shove Fatigue Paused**. The human begins at 2/3 fatigue from earlier exertion. **Step 1 tick** performs its final shove and reaches 3/3. **Step 10 ticks** shows it prioritizing retreat. Restart restores the original setup. Existing saves keep their earlier behavior until **Enable shove fatigue** is selected.

Fatigue is stored with the actor, and exact save continuation is checked. Earlier rules reject nonzero fatigue they do not support. Automated checks cover real shove costs, exhaustion, quiet recovery, cooldown gating, conversion cleanup, invalid saved values, older behavior, inspector updates, upgrade and restart. Native Unity appearance and desktop input require local verification.

Validation: 327 core cases / 320,905 assertions; 1,133 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
