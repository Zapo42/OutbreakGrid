# NPC-FOOD-14 finite-food trials

Reuses the shared shopping11 observer and unchanged107 inputs/hashes. Run sequentially with .NET8:

```sh
dotnet run --project harness/diagnostics/shopping14/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/food111 111
dotnet run --project harness/diagnostics/shopping14/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/food110-control 110 delivery
```

Only the requested rules string changes. Food, geometry, staff, schedules and knowledge stay fixed. Exit0 checks physical accounting/adjacent credits/exact replay; inspect individual outcomes before claiming survival. See shopping11/README.md for definitions and input provenance. Native Unity remains paused.

## Movement load

`dotnet run --project harness/diagnostics/shopping14/MovementLoad.csproj -c Release -- /tmp/movement-load`

Extract `movement-inputs.zip` into the output directory first to compare the frozen pre-optimization rules111 workloads. Without existing inputs the tool creates54/244resident cities, sets hunger80 and transfers carried food into each owned pantry (total food unchanged). Saves the input, warms100ticks, then measures three independent250tick passes with per-tick timings and current-thread allocated bytes; serialization is outside the timer. Reuse the same saved inputs when comparing builds. Restore input SHA256 from harness/validation/food14-movement/RESULTS.json for independent comparisons: baseline3eb6f55 can generate them using the unchanged benchmark. Compare exact final hashes before interpreting timings. First-pass JIT and host scheduling affect wall times. This deliberately synchronized stress test is neither a survival trial nor native rendering/FPS evidence.

## Fresh-city demand wave (rules112)

`dotnet run --project harness/diagnostics/shopping14/DemandWave.csproj -c Release -- /tmp/demand-wave`

Runs the same seed214/8×8/four-home/244resident geometry and780meal budget twice for3600ticks: homogeneous initialization with rules111, then seeded diverse needs and routine reviews with rules112. Records actual shopping-task starts per tick and per25ticks, plus initial need distributions. The initial900tick observation was too short to capture the baseline shopping wave; use the3600tick result. These are finite fresh-city activity observations, not survival acceptance or native rendering benchmarks. Raw timing comparisons include host/JIT variation and should not be treated as FPS evidence.

Current integrated target is112: run the finite-food observer with final argument `112`. Keep all builds sequential. Use optional final `staggered` argument for DemandWave to run only the diverse112 comparison. It also writes the complete final save and SHA256; process CPU includes observations/encoding while per-tick Core timers exclude them.

To reproduce the route-only before/after comparison, use an isolated source copy of the final tree and disable only the new early-rejection `if` in LongRoutes.cs (the one calling RoomEntryBlocked). Run the same staggered3600tick input on both copies; compare entire final saved bytes before interpreting timings. Never modify main or protected tags for a control. The worker review narrowing does not affect this six-hour7am–1pm case because all jobs are on duty. The initial global-review26hour candidate is rejected separately in evidence; final four-city results use consumer-only review.
