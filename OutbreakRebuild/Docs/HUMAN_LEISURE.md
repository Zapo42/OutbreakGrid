# Purposeful free-time walks — Foundation 131

Open **Inspect Neighborhood Walks Paused** from Current Tests. Select the resident and watch **Free-time activity** while stepping or running: **Taking a walk → Pausing outside → Returning home**. The resident begins inside a stocked home; the separate shop is not part of the outing. Restart scenario restores the same authored setup, paused.

## First slice

Pure consumers with assigned homes can take occasional short walks. They must be healthy, uninfected, calm, fed, free of urgent work/food tasks, and rested. Clerks and couriers retain their current routines; off-duty worker leisure is deferred. This increment does not add happiness, parks or bars.

A walk selects a reachable outdoor destination near one real home entrance, physically opens/crosses the door, pauses, then returns through that entrance to its original indoor cell. It uses normal actor movement and collision checks. Every actual step costs existing movement energy; decorative idle animation remains separate and free. Recovered energy comes from existing rest/food rules, not a free leisure refill.

Destinations and routes stay within a bounded 12-cell radius around the selected entrance. The planner prefers a modest six-cell outdoor distance and limits the initial path length; it uses grass, sidewalks and crosswalks, excluding ordinary vehicle roads and unrelated room interiors/doors. A reused local search buffer caps work at 625 cells. Identity/time-based target variation does not consume simulation random numbers.

## Priorities and timing

Shopping, home-stock inspection, jobs, meals and safety retain their established decision order. New needs, injuries, infection, grapples, observed danger or warnings cancel optional leisure. A safe ongoing walk bypasses only the old unconditional routine return-home action; it does not bypass genuine shopping or delivery tasks. Blocked routes end the optional outing and leave existing needs/shelter routines in control.

Current provisional tuning:

- Initial ordinary wait: 300 ticks plus an identity stagger of up to 119 ticks.
- Cooldown after a trip or interruption: 600 ticks plus that stagger. Eligible fed/rested residents wait at home between outings rather than falling into the old automatic departure.
- Minimum start energy: 80; the resident currently allows normal shelter rest to finish first. Start hunger must be below 25 internally, corresponding to more than 75 on the descending food bar.
- During a walk: energy at least 40, internal hunger below 35, full health and no urgent task/danger.
- Outdoor pause: 20 ticks. Outbound and return legs each have a 600-tick safety timeout.

One tick is six game seconds. The controlled preview authors its initial wait as one tick so testing does not require waiting for the normal interval. Actual observation timings can vary with door actions, crowding, needs and navigation.

## Saves and older runs

Rules generation 100 saves the walk phase, target, entrance, indoor return cell, deadline and cooldown. All phases resume deterministically. Invalid fields and leisure data in older rules are rejected before replacing the current world. Changing or removing a home clears walk/cooldown data immediately, keeping the next save loadable. Death/turning clears human leisure state.

Older runs retain their original behavior until **Enable purposeful walks** is chosen. The upgrade preserves existing actors, needs, inventory, random state and progress. Restarting an upgraded scenario retains the enabled current rules. `RESTORE42` remains the pre-team Foundation 130 checkpoint; `foundation-82-unity-checkpoint` remains the last user-reported working native Unity build.

## Validation limits

Automated coverage checks full physical outings and movement cost; normal cooldown behavior; saves during outbound/pause/return/cooldown; home assignment changes; hunger, energy, injury, danger, shopping and infection priority; blocked exteriors; malformed records; and an independent rules-99 continuation digest. UI checks exercise actual preview/step/restart/upgrade handlers through narrow Unity API doubles.

See [build validation](BUILD_VALIDATION.md) and [team verification log](../../harness/build-log.md) for executed counts. Native Unity 6000.6.3f1 import, rendering, runtime and Windows player testing remain NOT RUN under the user's paused-testing instruction. The next documented gameplay increment is the saved happiness foundation.
