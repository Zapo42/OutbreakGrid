# NPC-SHOPPING-13 validation

Reuses ../shopping11/StoreAudit.cs and unchanged107 inputs/hashes. Run sequentially with .NET8:

```sh
dotnet run --project harness/diagnostics/shopping13/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/shopping110 110
dotnet run --project harness/diagnostics/shopping13/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/shopping109-control 109 delivery
```

Four seed214/100x56/five-actor/203meal trials each run15600ticks. Only the requested rules string changes on a detached input; inventories, maps, roles, knowledge and schedules are unchanged. Exit0 checks food accounting, physical adjacent delivery credits and exact replay, not healthy city life. See shopping11/README.md for observer definitions. Summaries/raw-report hashes and final checks are in harness/validation/shopping13; full event arrays/CSV/checkpoints regenerate with these commands. Native Unity stays paused.
