# NPC-F141 integration candidate — historical release review

Superseded for current local source by [single-agent consolidation](CONSOLIDATION_STATUS_2026-10-30.md). R1 corrects the original generated7→4 delivery regression, while older-fixture survival and later personal-shopping/delivery failures remain. The original report/evidence below is preserved, not relabeled as a fresh current run.

This source-review branch integrates the NPC transfer into World-aware main. It is **not a released Foundation141 build**. Main remains `1b05a73c0994720230524c519dac84fc58d0bbd3` until the material courier regression is resolved or explicitly accepted. Native Unity/player testing remains paused.

## Source and shared integration

- Transfer `b37a589ab6df1f220d6786c35581ad08514dad65`; implementation `c6902e059e8ac6a6fe2bfa8a8bbad2b238589830`.
- Includes all NPC132–137 prerequisites and F138–141 diagnostics/fix. Earlier specialist history is retained once; applying only the four post-F137 commits would miss required source/state/tests.
- Preserves World schema14 homes/recovery markers, GUIDs, generators/references, current runner, editor/dependencies and unrelated changes. No actor/map fields were added by F141 itself; prerequisite optional NPC fields and memory detachment are retained.
- Old rules100–106 continue unchanged until explicit107 upgrade. Candidate107 saves need the matching candidate reader; preserve original saves and save upgraded runs under new names rather than expecting main's rules100 reader to load them. UI adds the missing upgrade controls/restart aliases, assigned-employment outline/inspector identity, a paused finite needs preview and candidate build label. Core retains effective off-duty behavior.
- Lead owns shared integration/UI wiring and new integration checks; the NPC coder retains behavior/tests. No NPC behavior was changed to improve totals. Exact paths: `harness/validation/npc-f141/INTEGRATED_PATHS.txt`.

## Release blocker

Independent paired trial: unmodified transfer source, seed214, `CitySetup.Create(214,4,2,2,Staggered,NeighborhoodCenters)`,14400ticks. Initial states are identical except rules version; geometry, stock, staffing and capacity are unchanged.

| Outcome | Rules106 | Rules107 |
| --- | ---: | ---: |
| Credited meals | 7 | 4 |
| Deaths | 4 | 2 |
| Remaining center food | 20 | 25 |
| Courier final health | 100 | 69 |

The three missing credits have stage evidence:

1. At5601, rules107 consumes carried stock2→1 near the destination. The remaining meal is personal reserve; it clears the returning task at5611 without depositing. Rules106 credits one meal at5621.
2. Rules107 claims again at6506 with hunger38/no food, reaches hunger40 before pickup, returns empty by6556 and holds the claim until expiry10106. Rules106 picks up three meals and credits two at7126.

These losses occur before the courier's off-duty boundary; its own home commute is not the immediate mechanism. Other actors also differ, so this is not a factorial proof about every helper/seed. The paired107 final save is byte-identical to the supplied snapshot. Fewer deaths and aggregate food do not establish throughput neutrality or usable supply. [Independent review and stage evidence](../../harness/validation/npc-f141/regression/REVIEW.md).

## Supplied cartographer save — separate full-day observation

The World20380be transfer's actual `daily-routine-214.obsave` was loaded on integrated source. Its original rules106 capture round-trips exactly. Two14400tick runs preserve its11 actors, geometry and initial stock: one keeps106, the other explicitly selects107. Twenty-tick replay windows every600ticks and final save roundtrips pass.

This distinct64×36 fixture is **not** the generated4×2 city above. It records8/11 deaths under106 and7/11 under107, with24/25 meals remaining respectively. Final retained-order credits are40/38; this observation is not a complete per-transfer throughput audit. No correction or inventory intervention was applied. Read the [actual report](../../harness/validation/npc-f141/cartographer/CARTOGRAPHER_REPLAY.txt).

The replay pass confirms interoperability, not acceptable survival or usable distribution. Seven deaths remain unacceptable evidence for claiming healthy daily routines. The generic daily observer's older “cartographer pending” note is superseded only for this supplied save by this separate run; no claim about other missing baseline saves is made.

## Existing NPC coder follow-up

Investigate priority eating versus personal reserve/delivery stock, hunger-marginal dispatch, and empty-return claim ownership/expiry. Preserve real eating, finite inventory, movement/rest costs, danger priorities and exact old106 continuation. Do not restore starvation to recover credits, refill food, change staffing/capacity or weaken accounting checks.

Return a narrow fix with same-initial-city paired trials, controlled routes, needs tests and exact save continuations. Also diagnose the supplied cartographer save with exact actor/tick/phase evidence; its poor survival is a separate observation, not proof of a World geometry fault. Demonstrate the delivery regression is corrected or present the tradeoff for explicit user acceptance. Lead owns follow-up integration and release validation. No city-supply resolution is claimed.

## Validation commands

Use .NET8 and run builds sequentially. In addition to the documented unified runner:

```sh
dotnet run --project OutbreakRebuild/Tests/NpcRuleUpgradeUiChecks.csproj --no-launch-profile
dotnet run --project OutbreakRebuild/Tests/NpcDailyRoutineChecks.csproj -- /tmp/npc141-daily
dotnet run --project OutbreakRebuild/Tests/NpcCourierThroughputAudit.csproj -- /tmp/npc141-courier
dotnet run --project OutbreakRebuild/Tests/BuildingRoomChecks.csproj -- OutbreakRebuild/CityReferences/districts/houses
dotnet run --project OutbreakRebuild/Tests/BuildingRoomUiChecks.csproj --no-launch-profile
```

Historical hunger investigation intentionally selects106; it is not current107 acceptance. The independent paired review used unmodified transfer source; the reproducible diagnostic copy under `harness/validation/npc-f141/regression/diagnostic` targets integrated sources. Preserve that distinction when comparing hashes.

API-double UI, static World assertions, replay and accounting passes do not establish native rendering/input, Windows execution, NPC survival or whole-day supply. Current results and limits are recorded in `BUILD_VALIDATION.md` and `harness/build-log.md`.
