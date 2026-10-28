# NPC-F141-R1 — correct the courier needs/claim regression

User selected the dedicated NPC-coder correction before main release. This is a follow-up to F141, not a new gameplay milestone or authorization to redesign the food system.

## Baseline and ownership

- Use `lead/npc-f141-integration-20261028`, source-review baseline `2aeac34bd3c9750ceac610c9f60f47a4f1971d87` (compiled integration source `eb630f125339d458d005dcbc62d529c3d7486052`). Later assignment-only commits do not change production code. Inspect actual HEAD/status and preserve unfinished work.
- Do not base a broad replacement on the older transfer branch. This candidate includes World schema14 and reviewed UI integration that must survive the fix.
- Existing dedicated NPC coder is the sole implementation writer for:
  - `OutbreakRebuild/Assets/Outbreak/Core/NeedsPriority.cs`
  - `OutbreakRebuild/Assets/Outbreak/Core/StorageRestocking.cs`
  - `OutbreakRebuild/Assets/Outbreak/Core/OrderFulfillment.cs`
  - `OutbreakRebuild/Assets/Outbreak/Core/WorkerShifts.cs`
  - `OutbreakRebuild/Tests/NeedsPriorityChecks.cs`
  - `OutbreakRebuild/Tests/NpcCourierThroughputAudit.cs`
  - `OutbreakRebuild/Tests/NpcDailyRoutineChecks.cs`
- This is a maximum allowed file set, not a request to change every file. Extend the existing needs test registration rather than editing shared manifests/Program.cs unnecessarily. Diagnostic changes may add observations but must not weaken accounting/replay assertions or rewrite inputs to hide the regression.
- Lead retains World/Encounter version wiring, Contracts/persistence, UI, shared manifests, plan/validation/build records and final integration/publication. If the fix needs any additional file or state/version contract, coordinate its exact ownership first. Do not silently change rules/save versions or expand into unrelated behavioral systems.
- One writer per file. Use the existing isolated checkout or sequence work; do not create a new worktree without an explicit request. No independent main merge or GitHub push; return the local fix/handoff for Lead assembly.

## Reproduced failure

Read `OutbreakRebuild/Docs/NPC_F141_INTEGRATION_REVIEW.md` and `harness/validation/npc-f141/regression/REVIEW.md` before editing. Raw events, stage excerpts, initial/final saves and observation source are in that evidence directory.

Same generated seed214 city,4×2 blocks,2 residents/house,14400ticks; initial states match except106/107 rules. No stock, map, staffing or capacity changes:

| Outcome | Rules106 | Candidate107 |
| --- | ---: | ---: |
| Credited meals | 7 | 4 |
| Deaths | 4 | 2 |
| Center food remaining | 20 | 25 |
| Courier final health | 100 | 69 |

Observed missing credits:

1. At tick5601 the107 courier eats carried stock2→1 near the destination. The remaining meal is personal reserve, so the returning task clears at5611 without a deposit.106 credits one meal at5621.
2.107 claims again at6506 with hunger38/no food, reaches40 before source pickup, returns empty by6556 and holds the claim until expiry10106.106 instead picks up three meals and credits two at7126.

Review priority eating versus available personal/delivery stock, hunger-marginal dispatch, and empty-return claim ownership/retry/expiry. An empty task should not waste a whole delivery window or unnecessarily masquerade as an unfinished load preventing needs/home recovery. Establish the narrow invariant in tests rather than patching these specific tick numbers.

The courier's own off-duty commute is not the immediate cause of the lost credits; all occur before that boundary. Other actors also change under107, so do not claim every interaction is fully isolated.

## Acceptance

1. Keep safe carried-meal priority, real physical consumption, actual movement/rest hunger costs, danger/infection/grapple/freeze priorities and off-duty home-return improvements. Do not restore starvation, forbid needed eating, grant food/knowledge, teleport, or add supply/staffing/capacity/speed changes to recover counts.
2. Add focused cases that fail on the current candidate for the relevant meal/reserve/departure and empty-return/claim lifecycle. Verify real transfer credits, prompt valid release/recovery/retry of unusable claims, coherent immediate saves and exact continuation. Do not count eaten meals as delivered or silently drop requested quantities.
3. Preserve the existing needs cases and independent rules106 continuation digest. Rules106 and earlier behavior must remain unchanged. Retain current World metadata/GUIDs and reviewed UI upgrades/restarts.
4. Repeat paired same-initial-map/supply trials. Demonstrate the reproduced delivery loss and empty-claim waste are corrected; report credits, source flows, deaths, courier health, claim events and exact input/source IDs. Any remaining decline against the106 benchmark of7 credits requires an explanation and explicit acceptance before release; do not replace that comparison with a weaker diagnostic pass.
5. Rerun the existing short-route/larger-district controls, full core suite and daily/courier replay/accounting diagnostics sequentially. Report changed outcomes honestly; current candidate controls are12 and20 credits. Lead runs final unified UI/World regression after integration.
6. Diagnose the actual supplied cartographer save separately. Its current replay evidence is `harness/validation/npc-f141/cartographer/`:8/11 deaths106 versus7/11 deaths107, with24/25 food remaining. This64×36 fixture is not the generated city. Report exact actor/tick/task context; do not claim geometry fault or healthy survival without evidence. Include this fixture in follow-up observations without altering its stock or geometry.

No requirement to solve every city-supply issue in this increment. Unresolved deaths/supply limitations must remain explicit; fixing the regression is not global food-system certification. Native Unity/player testing remains paused; no EXE claim.

## Return to Lead

Provide task ID, exact base/source commit, local branch, changed files, focused and full commands/results, original/fixed paired evidence, legacy continuation evidence, cartographer observations, limitations and integration instructions. Supply a local patch or bundle/transfer package through the established handoff; do not independently publish it. The Lead will review the actual correction, rerun integrated checks and decide whether main can advance.

This file assigns work; it does not claim the separate NPC chat has been contacted or that implementation has started. Main remains at `1b05a73` while this release blocker is open.
