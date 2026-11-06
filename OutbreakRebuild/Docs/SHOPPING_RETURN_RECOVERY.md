# Shopping home completion and return recovery — NPC-SHOPPING-13

Rules110 address an arrived pedestrian return retaining its pantry claim while hungry. Earlier109 bounded searches and108 return recovery remain. No new food, staff, schedules, geometry, colors, speeds or needs rates are introduced. Native Unity testing remains paused.

## Current behavior

On physical home arrival, a pedestrian with no food above its personal reserve completes the shopping task before the hunger guard. Its pantry claim/deadline and stale departure/food goal are cleared, so normal home inspection, food-seeking and paid recovery can resume. Existing failed-store cooldown is retained; no new cooldown is added. Urgently hungry empty-handed residents can pay for a home pantry inspection even below the usual energy threshold. During an existing failed-shop wait, fresh personal observations that every home box is empty allow waiting in place and energy recovery; known usable food, unsearched home shelves and danger prevent that wait. This urgent wait cannot heal health. Rest/hunger rates and existing hunger caps remain, so alive is not the same as fed.

If an urgently hungry on-duty pedestrian worker personally knows home supplies are empty, its ordinary commute to its assigned workplace may proceed with paid rest. The workplace location is authored knowledge; food is neither promised nor granted. Normal staff food access must observe and withdraw it physically on arrival. Work loads, delivery claims and assigned vehicle routines retain ownership. Surplus food still needs a real pantry deposit, and assigned consumer vehicles retain their cart lifecycle. No inventory or stock knowledge is granted.

A failed-shop observation normally expires after300ticks. Walking to the farther shop can take longer, so the same trip could select the already-empty shop again and repeat until its3600tick task expired. Rules109 retain genuine failed-shop observations during that bounded pedestrian search, preserving their observation times. A newer positive shelf observation can still win; storefront location alone reveals no inventory. Normal failed-visit cooldown resumes once collection ends. The existing eight-note memory limit remains.

A timed-out walking task releases its pantry claim and shopping deadline immediately, preserves physical carried food, and hands control to the existing ordinary home-return route and paid off-road recovery. The shopping task is not renewed. Movement, staged door checks, danger/infection guards, hunger costs and food transfers remain authoritative Core decisions. Assigned consumer vehicles keep their previous cart/cargo timeout lifecycle.

## Saves and controls

New scenarios use110. Loading109 or earlier retains the saved rules; rendering never upgrades a run. The inspector offers **Enable home shopping completion** for109. Older saves show their next missing-feature button, which explicitly upgrades the detached run to the current rules while paused. Save separately to preserve the original. Restart keeps enabled rules and checkpoint facts, including modern energy, vehicle clocks and uninfected deprivation corpses. No new save fields or schema changes. Unity remains6000.6.3f1.

## Validation and limits

Hosted checks pass 732 core cases/744525 assertions plus UI/upgrade checks. The delivery trial improves from2deaths/15credited meals to1death/17credits; all three workers attend next-day workplaces. Long-shopping has0deaths and all workers attend. Individual risks remain: delivery consumer5now dies while IDs3/4survive, consumer4has only1health, and long-shopping consumer5health falls82→26. Some survivors remain at100hunger. Full individual results and rejected candidates are recorded below; this is not healthy-city acceptance.

Latest hosted/city/control results are recorded in BUILD_VALIDATION.md and harness/validation/shopping13. Reproduction commands: harness/diagnostics/shopping13; unchanged source fixtures/input hashes: harness/diagnostics/shopping11. Trial exit0 proves physical-food accounting and exact replay, not healthy daily routines. Hosted UI API doubles/Windows ABI checks do not establish actual Unity rendering/input, dialogs or Windows player execution. No EXE is supplied.

When native testing is explicitly resumed:

1. Current Tests → **Inspect Large City Shopping Paused**. Follow the selected shopper home. When no deliverable surplus remains, the return claim should end even while hungry; food-seeking/inspection and recovery can resume normally. Confirmed-empty-shop search exclusions remain. Pause, speed and Restart should work.
2. Load your rules109 save. Verify **Enable home shopping completion** upgrades only after the queued action, keeps the run paused and preserves tick/inventory. Save separately. An older supplied107 fixture uses its offered upgrade button.
3. Save during a shopping search or a return after timeout, reload, then step. Search observations/return intent must persist; an expired task must have no renewed pantry claim or shopping deadline. Ordinary travel/rest costs still apply.

## Historical rules108 background

### Demonstrated return-recovery defect

On the unchanged delivery fixture under published rules 107, consumer 4 begins a legitimate proactive shopping trip with home stock below its target. It eats its personal meal during travel. At tick 5404, hunger 40 with no carried food triggers the old restocking safety gate, which also requires hunger below 40 or personally remembered completely empty home cabinets. The shopper abandons its farther-store search. That same gate prevents shopping rest on the resulting empty return; energy reaches0. This is a demonstrated failure, not proof that every starvation event has the same cause.

Rules 108 allow an **already returning, unexpired, safe personal-shopping task** to take its existing paid recovery breaks despite hunger/injury or aging home observations. Breaks remain off roads/doorways, retain destination/deadline, and exchange hunger for energy at the unchanged rate. Eating real carried food still takes priority. Original hunger aborts during collection and initial shopping eligibility are preserved; this does not force a starving shopper to keep searching empty shops. It reveals no hidden stock, extends no deadlines, creates no food, grants no free movement and changes no courier claims. Danger, infection, grapple/freeze, stress and recent warnings/noise retain their guards. Real depleted stores can still produce legitimate empty returns.

The initial broader candidate continued hungry collection as well. Its delivery trial left two deaths but changed the victims to both consumers, including one who previously survived. It was rejected before publication. Final acceptance applies to return recovery only.

## NPC-FOOD-14A — hungry household members can seek their own meal (rules111)

Baseline ffa1723. Consumer5 was blocked from shopping by another pedestrian's claim on the shared empty home pantry, despite personal observations and known stores. Hungry empty-handed foot residents who personally know home empty may now share a household shopping destination. Fed routine restocking, delivery/order and vehicle claims remain exclusive; duplicate vehicle claims are checked after vehicles restore. No food, stock knowledge, geometry, schedule, save field, dependency or editor changes.

Changed Core: Encounter.cs, World.cs, StoreShopping.cs, StorageRestocking.cs; FoundationApp.cs upgrade/restart/build label; CityShoppingContinuityChecks.cs and NpcRuleUpgradeUiChecks.cs. Reproducible shared observer plus shopping14 project and compact evidence. Failing-before test now passes; actual pickup/eating, every-tick finite accounting, fed exclusivity, malformed ordinary duplicate rejection and exact replay covered. Sequential unified runner PASS736 cases/744670 assertions, configuration/hosted Windows ABI, UI7222 Legacy/3 NewOnly. Upgrade checks118 assertions PASS. Legacy110 1000tick continuation digest DA37B9C117036E40B342E9496B4B63822B4E73E576140296E543AE5DEA124CFD unchanged; full110 delivery rerun compared separately.

Four unchanged203meal/15600tick city trials now have zero deaths: restocking124remaining/79eaten/9credits; shopping126/77/17; delivery151/52/17; long-shopping139/64/17. Delivery deaths110→111:1→0, credits17→17. All28 replay groups/840 exact continuation steps and finite food ledgers PASS. This does not establish adequate food access: some survivors still end at hunger100, and worker3 is exhausted with health96/97. Do not call the supply chain solved. See harness/validation/shopping14/SUMMARY.json and per-case individual outcomes.

Native Unity compilation/rendering/input/dialogs/player NOT RUN under pause; no EXE. Unity6000.6.3f1, dependencies, GUIDs and protected checkpoints preserved. Next authorized increment: seeded starting needs, stagger routine demand and measured movement cost; then shift/activity coverage after food is stable. Automatic publication and verified HTTPS project archive follow validation.
