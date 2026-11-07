# City-pack live integration observation

Run sequentially with the production source:

```
dotnet run --project harness/diagnostics/citypack18/CityPackTrial.csproj -c Release -- /tmp/citypack18-trial
```

Creates Mixed, SuburbanGrid and BusinessCore cities at seed214,2×2blocks/two homes per residential block. Observes2400ticks/four game hours, whole physical food accounting against independently observed carried-meal consumption, health, movement, actual credited deliveries (zero credits fail the controlled observation), complete save/reload replay each tick and periodic reloads. Writes initial/final saves, checkpoints, compact results and terrain geometry SVGs. End-of-trial door flags describe the final terrain, not whether a door was ever used. SVGs are hosted geometry illustrations, not native screenshots. No replenishment, NPC rule changes or survival guarantees. Native Unity testing remains paused.
