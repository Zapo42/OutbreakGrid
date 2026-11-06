# NPC-FOOD-14 finite-food trials

Reuses the shared shopping11 observer and unchanged107 inputs/hashes. Run sequentially with .NET8:

```sh
dotnet run --project harness/diagnostics/shopping14/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/food111 111
dotnet run --project harness/diagnostics/shopping14/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/food110-control 110 delivery
```

Only the requested rules string changes. Food, geometry, staff, schedules and knowledge stay fixed. Exit0 checks physical accounting/adjacent credits/exact replay; inspect individual outcomes before claiming survival. See shopping11/README.md for definitions and input provenance. Native Unity remains paused.
