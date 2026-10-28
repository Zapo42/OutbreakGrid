# NPC-F141 courier regression review

Task: independent QA/diagnostics only; no production behavior/test edits, commits, pushes or native Unity runs. Owned scratch directory: `/workspace/review/npc141-regression`. Source is a `git archive` of c6902e059e8ac6a6fe2bfa8a8bbad2b238589830, not a worktree or main overlay. Diagnostic project only adds observation code under its temporary Diagnostics directory.

## Method and reproduction

Using unmodified production source, create `CitySetup.Create(214,4,2,2,Staggered,NeighborhoodCenters)` once; clone its captured state into rules106 and107. The two initial JSON documents are identical after removing `rulesVersion`. Run each for14400 ticks with no geometry, stock, staffing, speed or behavioral intervention. Record courier state every tick and every actor's meal/pickup/claim/credit/commute/health events.

Command from scratch source:

```sh
DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 /workspace/toolchains/dotnet/dotnet run --project Diagnostics/PairedAudit.csproj --no-launch-profile -- /workspace/review/npc141-regression/paired
```

Exit0. Rules106 reproduced7 credited meals,4 deaths, center stock20, courier health100; rules107 reproduced4 credited meals,2 deaths, center stock25, courier health69. Rules107 final save is byte-identical to supplied `evidence/current-city-final.obsave2` (SHA25625367fd9e7824af1a64c6f39bf24bce69632ce267f97b04561a1cf0c518c58c1). Rules106 aggregate results match supplied historical audit; the transfer does not supply its historical raw final save, so no independent raw106 byte-equality claim is made.

## Concrete observed mechanism

The first courier divergence is tick661: rules107 spends the action eating carried stock3→2 at hunger41→0; rules106 moves and hunger becomes42. That changes its later hunger/food trajectory while preserving physical inventory. It does not grant or lose food invisibly.

The missing three meal credits have specific stage evidence:

1. **One missed meal on order1.** In rules107 the courier eats at5161 (hand3→2) and again at5601 (hand2→1) at(16,12), inside destination building1480. The remaining1 is its existing personal reserve, so at5611 it clears its returning task/order without depositing food. In rules106 the corresponding trip credits1 meal at5621, bringing total credits4→5. This follows `TryPriorityMeal` plus the existing `carriedFood <= FoodReserve` completion path; it is not a delivery counter error.
2. **Two missed meals on the later dispatch.** Rules107 next claims order1 at6506 at(107,19), hunger38 and hand0. Hunger crosses40 before any source pickup; by6556 it is Returning empty with order1 still held and deadline10106. It travels to the destination empty and retains that unfinished task through expiry10106, with no further credit; courier health has fallen to2 by expiry. The106 courier instead starts order2 at6701 hunger3, picks up3 physical meals at6756 and credits2 at7126, reaching total7. Rules107's claim is already hunger-marginal; the existing no-food restock return/timeout interaction becomes a long unproductive route rather than a prompt retry.

All lost credits arise before the19:00 off-duty boundary at tick7200. Both trials record zero off-duty home commutes before7200 for the courier. Thus the courier's own off-duty-home-return path is not the immediate mechanism. Changed carried-meal timing and the consequent reserve/departure/task-state trajectory are directly observed. This is a same-map paired107-vs106 result, not a factorial ablation of every new helper or a proof about every seed. Other actors also change under107, so broader system attribution should not be overstated.

## Recommendation

The delivery regression is real and material:−3/7 credited meals (−42.9%), with courier final health100→69 and a multi-thousand-tick empty returning task. Fewer deaths is a separate improvement; aggregate remaining food and an insufficient day budget do not explain away the credit regression. Do not certify the city supply system as fixed or throughput-neutral.

Keep the integration in source-review/experimental status until the dedicated NPC coder reviews the empty-return/claim lifecycle and carried-meal/reserve/departure interaction, or the user explicitly accepts this documented tradeoff for release. The fix should preserve real eating, paid movement, finite stock and danger/legacy priorities; do not restore starvation just to recover delivery counts, silently refill food, or change staffing/capacity to hide the regression. The Lead owns the release decision and may integrate reviewed source separately from declaring release readiness.

Evidence: `paired.log`, `paired/courier106.csv`, `paired/courier107.csv`, both event files and initial/final saves, `critical-stage-excerpts.json`, `RESULTS.json`, plus the scratch diagnostic source. Native rendering/input/player testing remains paused and NOT RUN.
