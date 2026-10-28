# NPC-F140 — courier throughput audit

Baseline6fc9f5921fa7b7c8151e038b205e9152346ef83e. Local-only npc/f140-courier-audit, /workspace/npc-f140; sole NPC writer. Approved task: audit NPC courier throughput, not repair priorities, rebalance stock or edit cartographer geometry. Cartographer package remains queued for arrival.

Own standalone Tests/NpcCourierThroughputAudit.cs/.csproj, evidence, handoff/build-log. No production, contract, world tick, map, UI or release changes. Dependencies: physical source pickup, restock phases, authoritative order credits, schedules, deterministic saves. Risks: source withdrawals are not delivered stock; time in a phase is not proof of a blocked route; observations across different maps are not causal worker-count comparisons.

Acceptance: generated-day delivery/stock totals reproduce F139; event-level actual order credits and pickups, per-courier mutually exclusive time totals, movement/need flags, partial/completed orders, bounded replay determinism; controlled short route and larger multi-courier fixture. Explicit limitations and local-only handoff. Native Unity paused.

## Findings

Generated city, unchanged seed214/4x2blocks/2homes-per-block/staggered/neighborhood centers: one walking courier,07:00–19:00 shift,14400observed ticks(one day). Five claims produced five credited delivery events totaling7meals: store1480 received3/12, store7139 received4/12; neither order completed,17requested meals outstanding. Both stores were physically reached.

Center stock40→20, with23observed per-tick outward meals and3inward meals. Final dispatchable stock is16, with4held by the existing reserve policy. Source pickup includes personal reserve and is not delivery throughput. Do not label the23-versus7 difference lost inventory. This audit reconciles observed center deltas, not every individual's lifetime food provenance.

### Scheduled time and round trips

Post-tick scheduled-window phase samples total7200ticks:
- delivery-return3155ticks(5h15m30s);
- work/home commute2634ticks(4h23m24s);
- collecting260ticks(26m);
- on-duty/no-active-delivery1150ticks(1h55m);
- personal shopping1tick at the final next-day07:00 boundary.

Delivery-return plus commute is5789/7200ticks, about80.4%. These phases include movement cooldowns, door interactions, recovery/other stationary activity; they are not a stopwatch of pathfinding CPU time or proof of being stuck. Full-day personal shopping is5855ticks, of which5854are outside the sampled scheduled window. It must not be blamed as nearly ten hours of lost on-shift work.

Center-to-crate Manhattan distances are101cells(store1480) and64cells(store7139); these are geometric descriptors, not computed path lengths. Claim-to-credit durations:
- order1:845,860,865ticks, each visit crediting1meal;
- order2:420,425ticks, each visit crediting2meals.

The healthy walking carry limit is3with an existing1meal personal reserve, so at most2meals are normally deliverable per visit. The two12-meal requests therefore need at least12successful full-payload visits; only5credited visits occurred, averaging1.4meals each. This is a capacity/distance constraint compounded by needs behavior, not proof that a speed change alone is appropriate.

The generated courier spent542ticks both carrying food and in an active restock task while hunger>=40. This overlaps phases and does not by itself identify the exact gate executing. F139 separately reproduced the eating/return priority defect. Whole-day hunger/low-energy counters include off-duty personal shopping; they are not extra phase totals or on-shift blocked time.

### Controls

- Short-route fulfillment:2400ticks(4hours), one courier,12credited meals,3completed4-meal orders, zero outstanding at end. Center30→16;14outward,0inward; observed courier meal events2.
- Larger district:2400ticks, two couriers,19credited meals(10+9),2completed orders and11meals outstanding across remaining orders. Center80→53;27outward,0inward.

The maps, initial stocks, demand, schedule rules and distances differ. These controls demonstrate that physical fulfillment and completed orders work in other existing scenarios; they do not isolate the causal effect of adding couriers, choosing vehicles, or changing storage capacity on the generated map.

## Validation and reproducibility

`DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/NpcCourierThroughputAudit.csproj -- /workspace/artifacts/npc140-courier-audit`

PASS exit0 for all3scenarios: exact20-tick replay windows, nondecreasing authoritative order credits with courier attribution, no unnoticed order-history removal, physical stock support for observed pickups, center delta reconciliation, phase totals equal to each run horizon, completed orders in both controls, and generated baseline7credited/17outstanding/20center stock. The generated final save additionally matches /workspace/artifacts/npc139-baseline/generated-city-day-final.obsave2 byte-for-byte.

Artifacts: COURIER_AUDIT.txt, per-scenario events CSV, initial/final saves under /workspace/artifacts/npc140-courier-audit; log /workspace/artifacts/npc140-courier-audit.log. Tracked report copy: harness/NPC_F140_EVIDENCE.txt. CSV event rows not sampling actor vitals leave those fields blank, not false zero values; source/phase observations carry actual post-tick vitals. Nine-column CSV shape was verified after this formatting cleanup. Phase records are observations, not necessarily one unique phase-transition event. Per-tick source deltas can net simultaneous actions; no transaction-log instrumentation was inserted into production.

## Limits and next action

This is a diagnostic-only increment. No production Assets, map, rules, save fields, UI or existing FoundationChecks registration changed. The692-case suite was not rerun; its prior result is NPC-F137 evidence, not new F140 evidence. Native Unity/player, native performance certification and independent specialist QA NOT RUN. This audit does not mark the generated city's health as passing.

Recommended order: first repair urgent eating/home-return priorities identified by F139, then rerun this exact unchanged map. Use that result to design a paired staffing or physical vehicle-capacity trial on the same map, keeping finite supplies and reserve rules explicit. Leave map/road/parking authoring to the cartographer; do not infer all remaining stock should be remotely available to consumers.

Cartographer-package testing is already authorized and queued for arrival. User relay remains the coordination mechanism. Source/evidence remain local; no GitHub push or transfer package unless requested. RESTORE42, Unity6000.6.3f1 and Foundation82 native checkpoint remain fixed.
