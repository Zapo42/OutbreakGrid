# Foundation 97 — courier schedules

Couriers can now work daily shifts and spend off-duty periods returning home and recovering. Schedules are optional: existing couriers remain available around the clock until configured.

## Set a shift

Select a living, uninfected human with an assigned courier route. Their inspector offers:

- **Day shift 07:00–19:00** — twelve working hours.
- **Night shift 19:00–07:00** — twelve working hours across midnight.
- Custom **Shift start hour (0–23)** and **Working hours (1–23)** fields, followed by **Apply courier schedule**.
- **Courier always available** — removes the shift restriction.

Editing pauses the simulation. Schedules use the displayed game clock, starting at 07:00 at tick zero. One game hour remains 600 ticks. Pausing freezes shift time; speed changes and manual stepping advance the same game clock. Shifts repeat daily, include their start hour and exclude their end hour. Overnight shifts and midnight starts are supported.

## Work and rest

A courier starts a new assigned delivery only while on duty and otherwise eligible. Ending a shift does not erase an active collection or delivery. The courier completes that current trip under existing safety, hunger and timeout rules, then stops taking new jobs and heads to their assigned house when safe. They do not start the next destination simply because more boxes need food.

Off-duty couriers use the existing movement, door, energy-recovery, meal and shelter behavior. Recovering energy still costs hunger. Hunger, infection and danger take priority over the timetable. They may eat or shelter elsewhere before returning home. No sleep animation, mandatory bed assignment or special energy refill is introduced. An off-duty courier already home stays available for normal survival actions and rest. The next working period allows new jobs when they are fed, safe and sufficiently rested.

You may change a schedule during an active trip. This changes future job eligibility while retaining the current cargo, task, destination, timeout and door progress. Source/destination and home editing retain their existing busy-task restrictions. Home reassignment keeps the courier's schedule; clearing the route, removing its last destination, death or conversion clears the schedule.

The inspector and household roster show **On duty**, **Finishing current delivery**, **Off duty: home/rest**, or **Off duty: return home when safe**. On-duty status describes the work window; it does not bypass hunger, danger, donor reserves, household food targets or donation controls.

## Saves and example

Schedules persist per courier under encounter rules 77 and follow the existing scenario restart checkpoint. Older saves keep their earlier behavior until **Enable courier schedules** is selected. Their couriers remain continuously available unless a shift is configured after upgrading. Schedule settings belong to humans in running saves, not reusable terrain-map exports.

Open **Inspect Courier Shift End Paused**. It starts at **18:54**, shortly before a day shift ends at 19:00. Step or run: the courier collects and completes the near-house delivery, then returns home. The far destination remains unserved during the off-duty period. Restart returns the scenario to 18:54 with its authored day shift.

Automated checks cover day/night/midnight boundaries, the next day, exact save continuation across shift end, physical cargo delivery and home return, no new off-duty pickup, eating and energy recovery, active-trip edits, configuration validation, old rules, route/conversion cleanup and UI controls/upgrade/restart paths. Native Unity remains untested after Foundation 82. The permanent rollback tag is still `foundation-82-unity-checkpoint`.
