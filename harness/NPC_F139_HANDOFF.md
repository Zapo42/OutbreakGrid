# NPC-F139 — investigate hunger/deaths with food remaining

Baseline:4e0fe6dd93ec0cc5c4469deeb239b20d9db19a15. Local-only branch npc/f139-hunger-investigation, durable worktree /workspace/npc-f139. User authorized investigation now and the cartographer-map test once their package arrives. No cartographer package has arrived.

Scope: reproduce F138 generated-city findings, locate remaining food, trace needs/work/home decisions, establish narrow causal reproductions and record actionable findings. No automatic map redesign, unlimited food, hidden knowledge or rule upgrades. Owned diagnostic/test sources and handoff/build-log only until investigation establishes a concrete next increment. No production changes are planned for this investigative task.

Acceptance: same baseline reproduced from actual source, actor-specific timing/stock/decision evidence, distinguish overall finite supply from stranded stock and blocked choices, deterministic reproducible snapshots and causal controls, clear limits and lead/cartographer handoff. Keep local; no push or package unless requested. Native Unity remains paused.

Risks: surviving stock need not be accessible/known to the hungry NPC; test-only interventions must never be presented as gameplay fixes. Rest, cargo obligations and actual shortages must not be mislabeled navigation bugs. Environment restart cleared /tmp, but local commits survived; work and regenerated evidence now use /workspace paths.

## Findings

The regenerated F138 day matches the supplied baseline final byte-for-byte. All four deaths (ids2,3,6,7) have diedFromDeprivation=true. Both stores end with zero shelf/crate meals. The24remaining meals are20in the distribution center at116,11 and2each in the clerks' assigned homes at81,9 and67,30. Those supplies are not equivalent to publicly accessible or personally observed stock.

### A: hunger disables the explicit home commute

WorkerShifts.TryWorkerRoutine uses SafeRestock before establishing a commute; WorkerDepartureAllowed uses the same gate while leaving the current room. SafeRestock rejects hunger>=40 except the special empty-home shopping condition. The clerks' homes are not empty, and neither clerk has inspected its home box, so that exception does not provide a return-home decision. Returning to an authored home should not require remote knowledge that food remains there.

Controlled contrast from tick7200, actor2: normalize health100 and energy100 in BOTH copies; change only hunger(60versus0). After5ticks the hungry actor has no commute, while the fed copy has commute4472(home). After500ticks the hungry actor remains at store1480, hunger87, energy0, with no commute; the fed actor reaches home4472, hunger21, energy100. No extra food or route edits were supplied. This isolates a hunger-dependent decision failure from starting injury, exhaustion or an impossible home route.

A separate small32x20 control is important: a hungry clerk can eventually find its nearby home through fallback foraging even though the explicit commute is not started. Therefore the gate is not proof that every hungry worker is permanently immobile. Distance exposes it in the generated map.

Sources: Core/WorkerShifts.cs TryWorkerRoutine/WorkerDepartureAllowed, Core/StorageRestocking.cs SafeRestock, Core/ShoppingContinuity.cs CanSeekFoodForEmptyHome.

### B: return travel preempts eating an already carried meal

At tick3600 the real courier is alive, hunger60, energy0 and carrying3meals during RestockPhase.Returning. Its next normal decision retains hunger60 and all3meals. Hunger.TryForage calls ReturnFromFoodDanger before its carried-meal consumption branch; that return guard can stop the method before eating.

Test-only counterfactual on a detached copy: direct TryForage invocation still returns false with the return flag intact. Clear only homeShelterId on a second diagnostic copy and invoke the same method: it consumes exactly1meal, hunger60→10, carried3→2, total finite food−1. Shipment phase and food stock are not fabricated. This isolates the return-before-eat ordering; it is NOT a normal-world tick, production fix, or proof that changing that single flag is a safe implementation. Work/restock routines also run before general foraging and need coordinated priority repair.

Sources: Core/Hunger.cs TryForage, Core/FoodTripRetreat.cs ReturnFromFoodDanger, Core/HumanReactions.cs decision ordering, Core/StorageRestocking.cs returning work route.

### C: the map is also under-supplied for a below-max-hunger day

All10actors start hunger10. Natural hunger gain is480per14400ticks. Each needs at least8meals to end below100, so the idealized minimum is80meals, before movement-recovery hunger. Initial finite stock is78. This proves the current map cannot meet that below-max-hunger target for everybody even with perfect distribution; it does not prove every death is inevitable, since health loss also requires zero energy.

Both pending orders remain partially delivered (3/12and4/12), with20center meals still physically present. Work/need priority and throughput need repair before attributing all hunger to initial quantity. Center reserve/access policy remains legitimate and unchanged; do not grant consumers hidden center inventory or bypass courier deliveries.

## Reproduce and evidence

`DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/NpcDailyRoutineChecks.csproj -- /workspace/artifacts/npc139-baseline`

`DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/NpcHungerInvestigation.csproj -- /workspace/artifacts/npc139-investigation /workspace/artifacts/npc139-baseline/generated-city-day-final.obsave2`

Both completed exit0. The first is the unchanged F138 observer with controlled goals/replay windows; the second reproduces the full day and asserts the investigative contrasts above. Zero exit means the reproduction/causal contrasts held, NOT that daily-routine health passed. The second argument to the investigation runner is optional and compares the regenerated final save exactly to an independently generated baseline file.

Tracked concise evidence: harness/NPC_F139_EVIDENCE.txt. Durable artifacts: /workspace/artifacts/npc139-baseline (initial/final saves and CSVs), /workspace/artifacts/npc139-investigation (original snapshots and explicit diagnostic controls), logs /workspace/artifacts/npc139-baseline.log and npc139-investigation.log. Earlier exploratory trace/control logs are /workspace/artifacts/npc139-probe.log and npc139-controlled.log. Experimental health/hunger/return-flag interventions are diagnostic copies only.

No production Assets or existing FoundationChecks entrypoint was changed. The full692-case suite was not rerun for this diagnostic-only task; its prior evidence remains NPC-F137, not a new verification claim. Native Unity, independent specialist QA and cartographer-package validation NOT RUN. Test version remains106 and Unity6000.6.3f1 remains pinned. RESTORE42 unchanged.

## Recommended next increment and lead/cartographer handoff

Repair urgent needs versus work/home-return priorities in a separate approved NPC increment: a safe carried-meal action must not be indefinitely bypassed by work/return travel, and a hungry off-duty worker needs a directed home-return path without magically knowing home contents. Preserve danger/infection priority, physical steps and inventory transfers, cargo claims, legacy rule behavior and deterministic saves. Convert the investigative contrasts into desired-behavior regression tests when implementing that change.

The findings establish contributing mechanisms, not a complete attribution of every death or a new stock-access policy. After priority fixes, rerun the same unmodified map before assessing delivery throughput and authoring a separately labeled well-supplied24-hour control. Food budget/map authoring belongs with the cartographer/lead, not an automatic NPC stock refill.

Cartographer-package testing remains already authorized and queued for when the user sends it. Do not ask them to select it again. No external messages, GitHub push or transfer package was made. Keep this branch local; produce the handoff package only on request.
