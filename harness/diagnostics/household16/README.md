# Two-hour household observation

Run sequentially with .NET8:

```
dotnet run --project harness/diagnostics/household16/HouseholdTrial.csproj -c Release -- /tmp/household-trial
```

Creates the authored two-resident household,46finite food items, ten typed object records. Runs1200ticks, verifies complete encoded save/reload continuation every step, reloads again every100ticks, and checks remaining food plus recorded snack/table-meal consumption every tick. Writes checkpoints and a compact RESULTS.json. No stock replenishment, altered fixture rates or native Unity execution. Unit checks separately cover outside snacking, blocked TV sight, off devices, seat collisions, healing/energy costs, preferences and old113 continuation.
