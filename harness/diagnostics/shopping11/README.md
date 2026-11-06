# NPC-SHOPPING-11 fixed-input city observer

Read-only observer adapted from the supplied NPC-F143 StoreAudit. It uses production Core/Contracts/Persistence, five actors, 203 finite meals, unchanged seed214/100x56 fixtures, and15600ticks (07:00 through next-day09:00). These are diagnostics with known failures, not healthy-city acceptance scenarios. Native Unity is not run.

`inputs/` contains the exact canonical rules107 initial saves supplied in NPC-F143 after107-final. INPUT_HASHES.json records provenance and hashes. Never rewrite inputs or tune inventories, geometry, schedules, identities or knowledge. The108 mode changes only the rules string on a detached copy; the observer requires all other initial facts to be identical. The107 mode retains the input rules and supports a baseline control.

From repository root with .NET8 (substitute your dotnet path):

```sh
dotnet run --project harness/diagnostics/shopping11/StoreAudit.csproj -- harness/diagnostics/shopping11/inputs /tmp/shopping108 108
dotnet run --project harness/diagnostics/shopping11/StoreAudit.csproj -- harness/diagnostics/shopping11/inputs /tmp/shopping107 107 delivery
```

The optional fourth argument selects one of restocking, shopping, delivery or long-shopping. Run builds sequentially. Outputs include RESULTS.json, physical-food/actor CSVs, exact replay saves and final.obsave. Exit0 proves accounting/replay assertions, not survival or successful daily routines. No fake delivery credits: credited increments require an active matching order, physical carried-food reduction and increase in that exact adjacent stockroom box. A meal eaten is not a delivery. Every tick checks finite-food conservation against observed meals and death discards.

Seven checkpoint categories per fixture compare30complete encoded continuation steps each, then restore the observation timeline: internal door, clerk stock handling, courier travel, shopping, and periodic300/7200/14400. Original observer attribution rules are retained. Final summary/evidence are in harness/validation/shopping11 and OutbreakRebuild/Docs/BUILD_VALIDATION.md. The rejected broad candidate is separately labeled; final implementation enables recovery only during Returning, preserving original hungry collection aborts.
