# NPC-F134 — known park visits

Baseline: 34a36a32cdf9b8417c5b74c5fd6a184d7e28e449 (NPC off-duty handoff). NPC-only task; RESTORE42 unchanged.

Scope: NPC-authored park familiarity, selection of a reachable known park during free time, a short visit and physical home return. Existing needs/work/safety and energy costs stay authoritative. The world/UI agent owns actual park designation, placement, visuals and registration calls. No automatic hidden park discovery or city changes.

Single writer: dedicated NPC developer in isolated npc/f134-park-visits worktree. Owned files: new Core/ParkVisits.cs; HumanLeisure.cs selection/status/pause; ShelfMemory.cs detached knowledge; Hunger.cs cleanup; Contracts/Documents.cs optional NPC state; minimal World/Encounter rules103 gate and validation; NPC behavior tests and this handoff/log. Shared save/tick wiring requires lead integration review.

Acceptance: known/reachable park selected, unknown/blocked/far parks never remotely used, no foreign-building/road transit, actual paid travel, bounded park stay, return home, worker/needs/danger interruptions, exact save continuation, defensive memory bounds and legacy102 compatibility. Core suite and focused park/worker tests will run. Native Unity testing remains paused.

## NPC API and behavior

- `TryRememberPark(actorIndex,x,y)` supplies explicit authored familiarity or a real observation from the world layer. It is idempotent for existing knowledge and rejects dead/nonhuman actors, older rules, invalid coordinates, indoor/blocked terrain or more than eight locations. It does not mark terrain as a park or discover locations automatically.
- `TryForgetPark(actorIndex,x,y)` removes that NPC's knowledge; forgetting an active destination ends the optional outing and existing home/shelter routines take over. Do not broadcast deletion knowledge to NPCs merely because hidden world metadata changed.
- `ActorAt(index).knownParks` is a detached sorted array of cell key+1. Location knowledge is allowed to become stale/blocked, as beliefs are not a live world inventory. Selection still verifies current passable routes.
- `leisureParkVisit` identifies the current outing through outbound, pause and return. `LeisureStatus` reports Walking to known park / Visiting park / Returning home from park.
- During eligible free time, the existing local search favors the nearest reachable known park. Trips remain within the existing 12-cell entrance radius and 20-step initial path budget. Ordinary roads and unrelated buildings are excluded; crosswalks remain allowed. Unreachable/far/unknown parks fall back to ordinary leisure.
- Park stay: 120 ticks (12 game minutes), followed by physical home return and the existing cooldown. Every real step uses existing movement energy. Needs, danger, injuries/infection, work shifts and cargo obligations retain priority for consumers and off-duty workers.
- No flat park happiness bonus, energy bonus or happiness-maximizing planner was added. Existing outdoor/company preferences continue to apply to actual observations during a visit.

## Saved state and compatibility

NPC fields: optional knownParks and leisureParkVisit. Known arrays are bounded/canonical, cloned on actor/save boundaries, cleared on death/turning, and retained across home changes. Clearing leisure clears the active park flag. Rules103 gates park behavior; OffDutyConsumersVersion aliases102. Shared World/Encounter validation wiring has one writer and requires project-lead integration review.

Independent baseline102 HumanLeisureTest after300ticks SHA256: 2DA13FF9F743969B08F933389A6356325EB414733403F50320F8922218D09F20, produced from the pre-feature source. The new code matches that continuation when parks are disabled.

## Validation evidence

- Build: PASS, zero warnings/errors.
- Focused park cases: PASS, ten cases / 967 assertions (`/tmp/npc134-focus.log`). Covers full paid outing/home return, bounded stationary stay, unknown/far/blocked destinations, closest reachable selection, exact phase saves/pause, work/needs interruptions, forgetting, detached/capped/corrupt knowledge, death/turning cleanup and old102 continuation.
- Full core result: PASS, exit 0, **657 cases / 739984 assertions**. Tested source commit 9fd55d1; command `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak134-npc-results`.
- Full log `/tmp/npc134-core.log`, report `/tmp/outbreak134-npc-results/FOUNDATION_CHECKS.txt`.
- Native Unity, general UI/release checks, Windows player and Mono: NOT RUN. No independent specialist review claimed for this NPC increment.

## Project-lead integration

Integrate 9fd55d1 atop npc/f133-off-duty, plus this final handoff/log commit. Review shared optional actor fields and rules103 wiring. The NPC branch contains the earlier pending happiness/off-duty integrations; avoid applying their original specialist commits twice.

The world/UI owner must supply real park designation/placement and call TryRememberPark only for authored familiarity or actual NPC observations. A minimal controlled integration can use HumanLeisureTest and TryRememberPark(0,11,9); make the park location visually identifiable. This code does not generate parks, add an editor brush, or claim parks exist in generated cities.

Add explicit park upgrade controls using ParkVisitsEnabled and include OffDutyConsumersVersion102 (as well as the previous HappinessVersion101) in RestartScenario's legacy-energy exclusions. Update preview/build labels and validate general UI/input/ABI/config behavior before publication. RESTORE42 and the Foundation82 native Unity checkpoint stay unchanged.
