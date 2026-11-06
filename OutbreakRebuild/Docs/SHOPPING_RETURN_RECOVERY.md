# Bounded shopping searches and return recovery — NPC-SHOPPING-12

Rules109 address the later empty-store retry/task-expiry loop after the published rules108 return-recovery increment. No new food, staff, schedules, geometry, colors, speeds or needs rates are introduced. Native Unity testing remains paused.

## Current behavior

A failed-shop observation normally expires after300ticks. Walking to the farther shop can take longer, so the same trip could select the already-empty shop again and repeat until its3600tick task expired. Rules109 retain genuine failed-shop observations during that bounded pedestrian search, preserving their observation times. A newer positive shelf observation can still win; storefront location alone reveals no inventory. Normal failed-visit cooldown resumes once collection ends. The existing eight-note memory limit remains.

A timed-out walking task releases its pantry claim and shopping deadline immediately, preserves physical carried food, and hands control to the existing ordinary home-return route and paid off-road recovery. The shopping task is not renewed. Movement, staged door checks, danger/infection guards, hunger costs and food transfers remain authoritative Core decisions. Assigned consumer vehicles keep their previous cart/cargo timeout lifecycle.

## Saves and controls

New scenarios use109. Loading108 or earlier retains the saved rules; rendering never upgrades a run. The inspector offers **Enable bounded shopping searches** for108. Older saves show their next missing-feature button, which explicitly upgrades the detached run to the current rules while paused. Save separately to preserve the original. Restart keeps enabled rules and checkpoint facts, including modern energy, vehicle clocks and uninfected deprivation corpses. No new save fields or schema changes. Unity remains6000.6.3f1.

## Validation and limits

Hosted checks pass723core cases/743792assertions plus UI/upgrade checks. The unchanged delivery trial still has2deaths and15credited meals; consumer5health improves33→100, with severe hunger remaining. Long-shopping worker2health worsens68→39 and worker3health61→44; next-day attendance is incomplete. These are documented regressions, not healthy-city acceptance.

Final hosted/city/control results are recorded in BUILD_VALIDATION.md and harness/validation/shopping12. Reproduction commands: harness/diagnostics/shopping12; unchanged source fixtures/input hashes: harness/diagnostics/shopping11. Trial exit0 proves physical-food accounting and exact replay, not healthy daily routines. Hosted UI API doubles/Windows ABI checks do not establish actual Unity rendering/input, dialogs or Windows player execution. No EXE is supplied.

When native testing is explicitly resumed:

1. Current Tests → **Inspect Large City Shopping Paused**. Follow task destination, food and energy. Confirmed empty shops should stay excluded during that search unless newer shelf knowledge changes the choice; pause, speed and Restart should work.
2. Load your rules108 save. Verify **Enable bounded shopping searches** upgrades only after the queued action, keeps the run paused and preserves tick/inventory. Save separately. An older supplied107 fixture uses its offered upgrade button.
3. Save during a shopping search or a return after timeout, reload, then step. Search observations/return intent must persist; an expired task must have no renewed pantry claim or shopping deadline. Ordinary travel/rest costs still apply.

## Historical rules108 background

### Demonstrated return-recovery defect

On the unchanged delivery fixture under published rules 107, consumer 4 begins a legitimate proactive shopping trip with home stock below its target. It eats its personal meal during travel. At tick 5404, hunger 40 with no carried food triggers the old restocking safety gate, which also requires hunger below 40 or personally remembered completely empty home cabinets. The shopper abandons its farther-store search. That same gate prevents shopping rest on the resulting empty return; energy reaches0. This is a demonstrated failure, not proof that every starvation event has the same cause.

Rules 108 allow an **already returning, unexpired, safe personal-shopping task** to take its existing paid recovery breaks despite hunger/injury or aging home observations. Breaks remain off roads/doorways, retain destination/deadline, and exchange hunger for energy at the unchanged rate. Eating real carried food still takes priority. Original hunger aborts during collection and initial shopping eligibility are preserved; this does not force a starving shopper to keep searching empty shops. It reveals no hidden stock, extends no deadlines, creates no food, grants no free movement and changes no courier claims. Danger, infection, grapple/freeze, stress and recent warnings/noise retain their guards. Real depleted stores can still produce legitimate empty returns.

The initial broader candidate continued hungry collection as well. Its delivery trial left two deaths but changed the victims to both consumers, including one who previously survived. It was rejected before publication. Final acceptance applies to return recovery only.
