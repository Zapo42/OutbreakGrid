# Shopping return recovery — NPC-SHOPPING-11

This increment addresses the existing personal-shopping empty-return investigation, after the completed worker uniforms. No new colors, roles, map layouts, staff, food, movement speeds or needs rates are introduced. Native Unity testing remains paused.

## Demonstrated cause and narrow change

On the unchanged delivery fixture under published rules 107, consumer 4 begins a legitimate proactive shopping trip with home stock below its target. It eats its personal meal during travel. At tick 5404, hunger 40 with no carried food triggers the old restocking safety gate, which also requires hunger below 40 or personally remembered completely empty home cabinets. The shopper abandons its farther-store search. That same gate prevents shopping rest on the resulting empty return; energy reaches0. This is a demonstrated failure, not proof that every starvation event has the same cause.

Rules 108 allow an **already returning, unexpired, safe personal-shopping task** to take its existing paid recovery breaks despite hunger/injury or aging home observations. Breaks remain off roads/doorways, retain destination/deadline, and exchange hunger for energy at the unchanged rate. Eating real carried food still takes priority. Original hunger aborts during collection and initial shopping eligibility are preserved; this does not force a starving shopper to keep searching empty shops. It reveals no hidden stock, extends no deadlines, creates no food, grants no free movement and changes no courier claims. Danger, infection, grapple/freeze, stress and recent warnings/noise retain their guards. Real depleted stores can still produce legitimate empty returns.

The initial broader candidate continued hungry collection as well. Its delivery trial left two deaths but changed the victims to both consumers, including one who previously survived. It was rejected before publication. Final acceptance applies to return recovery only.

## Saves and controls

New scenarios use rules 108. Loading107 or earlier keeps its original behavior. The existing inspector offers **Enable shopping return recovery** for107; this explicitly upgrades a paused detached run. Save to a new filename if preserving the original. Restart retains enabled rules and the starting checkpoint's facts, without converting107 energy, vehicle timers or ordinary corpses as legacy data. There are no new serialized fields or schema changes. Unity remains 6000.6.3f1.

## Validation

The narrowed delivery trial retains 15 credited deliveries and 158 remaining meals, with the same two actors dying as before. Consumer4 completes the first empty return at tick 5949 with energy 44, then eats actual home food; it still later buys only 1 shelf meal, deposits 0 shopping meals and dies at 13290 (earlier than the previous 13740). Surviving actors 1/2/5 finish at health 100/100/33 versus 69/43/19. Death count alone would hide this tradeoff. Later empty-store trips, task expiry and next-day work remain unfinished.

Results and limitations are recorded in BUILD_VALIDATION.md and harness/validation/shopping11. The portable read-only city observer, unchanged starting saves and reproduction commands are in harness/diagnostics/shopping11. Hosted core tests, UI API doubles and Windows ABI compilation do not establish native rendering/input, dialogs or player execution. No Windows EXE is supplied.

When native testing is explicitly resumed, useful manual checks are:

1. Current Tests → **Inspect Large City Shopping Paused**. Follow the selected shopper, run/step, and watch destination, food, energy and home deposits. An empty-handed hungry return should retain paid rest; collection may still be interrupted by hunger. Travel/rest still cost needs. Pause, speed and Restart should work.
2. Load the supplied delivery starting save from harness/diagnostics/shopping11/inputs. It remains rules 107 until **Enable shopping return recovery** is pressed. Verify that upgrading stays paused and preserves inventory/tick; save separately, then run. This is a diagnostic city, not a promised healthy-city scenario.
3. Save during an ongoing shopping break, reload, and step. The task should retain its destination/deadline and resume the paid break/travel; visual inspection is additional evidence, not a replacement for exact hosted continuation tests.
