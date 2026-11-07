# NPC-FOOD-15 — physical store entry flow

Run sequentially using .NET8:

```
dotnet run --project harness/diagnostics/shopping15/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/store-flow113 113
dotnet run --project harness/diagnostics/shopping15/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/store-flow112-control 112 delivery
```

Shared observer and unchanged107 city inputs/hashes. Only the selected rule version changes, never stock, geometry, staff or schedules. Accounting/replay PASS does not imply sufficient food or native rendering performance. Read individual actor outcomes before publication.

For a matched larger-city observation:

```
dotnet run --project harness/diagnostics/shopping15/StoreFlowLoad.csproj -c Release -- /tmp/store-flow-load
```

Runs identical seed214/244resident/780meal starting facts under112/113 for3600ticks. Every tick checks remaining physical food plus observed meals eaten. Records store entries/meals, critical hunger/deaths and hosted Core timings; observations/encoding are outside per-tick timers. Process CPU includes observations. This is a finite six-hour observation, not a promise of day-long supply or native FPS. Changed final hashes are expected because113 intentionally changes movement and arrival timing. No starting-stock/rate/staff tuning is permitted to improve the comparison.
