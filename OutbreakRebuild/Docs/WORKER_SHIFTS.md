# Foundation 108: worker shifts, home commutes and natural idling

Target editor: **Unity 6000.6.3f1**. Native Unity testing remains paused after Foundation 82.

## Observe shift changes

- **Inspect Worker Shift End Paused** starts at 07:54. A clerk and courier work until 08:00, then finish accepted work and return to a separate assigned home. A consumer is already at home. The home, store, center and personal inventories contain 33 meals in total.
- **Inspect Worker Shift Start Paused** starts at 07:54 with both workers at home and their shifts beginning at 08:00. They commute to the store and distribution center before beginning work.
- **Inspect Idle Motion Paused** encloses a healthy human in one cell so its stationary weight shifts, unchanged grid coordinates and unchanged energy are easy to observe. Run for a few seconds; the idle animation fades in after standing still.

The selected-item inspector shows occupation, duty/finishing status, personal shopping and home/work commute targets. Existing 5x/10x/20x/50x testing speeds and selected-only H/F/E bars remain available.

## Schedules and routines

Courier schedules retain their existing controls. Clerks now have day (07–19), night (19–07), custom start/duration and always-available controls under **Assignments & actions**. A scheduled clerk needs an assigned home; use the existing household/box assignment tools before scheduling an unassigned clerk. Custom starts use hours 0–23 and durations 1–23. Shifts repeat daily, support midnight crossings and end at the configured end hour. Always-available mode preserves continuous work.

Default unscheduled jobs remain always available. The update does not automatically assign residences or replace schedules in existing authored scenes. The new previews explicitly separate residence and workplace.

On duty, clerks travel to their assigned store and couriers to their configured source workplace. On arrival they use the existing replenishment and delivery logic. An on-duty courier can wait/rest at its source when there are no orders. Accepted courier deliveries finish after closing; no new job starts off duty. A clerk completes its current carried shelf load after closing and stops starting new pickups or phone calls. Unloaded pending shelf/call work cancels. If the shelf fills during cleanup, surplus can go back to the store crate. If no space is available, food remains carried when the worker leaves; finite inventories are preserved.

Off duty, workers retain job membership and use normal home eating, energy recovery, rest spots and consumer shopping. They can collect household groceries without rewriting courier routes, center membership or clerk assignment. A personal shopping trip already in progress finishes before a new work commute. Source-only center couriers also appear as couriers in the household roster. Removing center membership from a source-only courier clears its now-inapplicable courier schedule.

Travel uses the existing collision-safe routes, normal energy costs and staged door inspection. An incoming worker has the same doorway courtesy as an incoming shopper/delivery. Blocked/full destinations make the worker wait without starting a delivery; hunger, energy, warnings, infection and immediate danger retain priority. Infection pauses work/commuting. Busy commute or delivery assignments cannot be rewritten mid-action. Schedule edits change only the schedule fields and are reconsidered on the next decision.

## Occupation outlines

Living humans now have distinct outlines:

- **Brown:** on-duty courier or courier finishing an accepted delivery.
- **Yellow upper half / blue lower half:** on-duty clerk or clerk finishing a shelf load, divided horizontally.
- **Gray:** off-duty workers and workers doing personal shopping; infected workers' jobs are paused.
- **White:** consumers without an occupation.

The outlines show duty state throughout work and commuting to work. Existing yellow selection, infection, combat and health/need feedback remain visible. Zombies and corpses do not receive human occupation outlines. The legend is under **Map markers & navigation**.

## Energy-free idle motion

A calm, stationary human makes small, varied weight shifts inside its current grid cell. These presentation offsets leave the NPC's grid position unchanged. They use no energy, consume no AI randomness, create no footsteps and change no saved NPC state. Actual movement between cells continues to cost energy.

Idle motion fades in after standing still, stops during walking/contact, and is suppressed for door crossings, beds, phone/shelf work, active supply tasks, stress and infection. Waiting commuters can idle naturally. It follows simulation time and stops when paused. Maximum offsets stay inside the existing body-to-cell margins, so idling cannot move a human through a wall or doorway. Load/pause/resume reset the presentation blend as with existing walking animation.

## Saves, checks and limits

New scenarios use **encounter-prototype-87**. Earlier rules preserve their behavior until explicitly upgraded with **Enable worker shifts and commutes**. Save under a new name to preserve the old run. Clerk shift hours and commute destinations are saved and validated; jobs, groceries and commutes remain distinct. Death/conversion clears human work state. Restart restores authored schedules and positions.

Automated coverage checks shift boundaries, separate-home travel, post-shift shelf/delivery completion, doorway contention, normal eating/rest and off-duty grocery trips, retained assignments, blocked commutes, infection, malformed saves, exact continuation, clerk schedule controls, outline geometry/duty colors, and idle movement without energy or state changes. Native Unity import, screen layout, actual rendering and input remain unverified while testing is paused.

This increment uses the existing route range and stores. Larger districts, longer-distance route testing, vehicles and specialized stores remain separate milestones.
