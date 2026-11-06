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
