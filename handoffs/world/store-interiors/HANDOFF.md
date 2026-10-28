# World — live store sales floors and stockrooms

User-assigned follow-up to live house interiors. Baseline28dcbab (transfer branch), previous World implementation20380bec26bf680865a2b22662872db115a390c4. Work branch `world/store-interiors`. Existing NPC developer retains all behavior/diagnostics; no replacement coder. Keep local until publication is requested.

## Implemented

Opt-in `--store-layout compact|spacious` materializes existing store blueprints as schema14 maps: sales floor and separate stockroom share one store identity across a real internal doorway. Public and service doors each connect to the original street network. Compact stores have6 real shelves; spacious8. One real capacity30 supply box/reserve6 lives in the stockroom, and one real phone lives on the sales floor. Shelves/box/phone are solid existing runtime objects. The remaining draft rack cells stay ordinary floor hints because the current store contract owns a single supplyBox.

Generated shelves and boxes start empty. No NPCs, food, jobs or implicit shop discovery are invented by map generation. Existing World.TryConfigureStore(stockX,stockY) activates the shop; existing TryAssignClerk(actorIndex,storeId) accepts a clerk in either grouped room. A configured save retains store/clerk state; a map alone contains terrain/fixtures/membership. This is the same import/CLI workflow as live houses; default C# CitySetup/UI city generation has not been rewritten.

City mode uses store layouts for urban/business districts and house layouts for residential/suburban districts. It translates shelves, stock, phones and metadata references with room anchors. Physical-room budgets count2 per store. The new optional recipe fields preserve replay and existing recipe output; store defaults enlarge block spacing to fit aligned lots with street clearance, while explicit dimensions can still return the supported no-safe-lot error.

## Files / ownership

- OutbreakRebuild/Tools/generate_reference_city.py — store materializer, doors and live fixture records.
- OutbreakRebuild/Tools/generate_district_city.py — optional recipe/CLI, composition, live fixture/entrance/circulation validation, technical shelf marks.
- OutbreakRebuild/Tools/test_district_city.py — seeded replay, all4 rotations, actual fixtures, invalid/missing records, obstructed doorway and composition checks.
- OutbreakRebuild/CityReferences/districts/README.md — commands, contracts and limitations.
- OutbreakRebuild/Tests/StoreLayoutChecks.cs and .csproj — physical access and existing store APIs, map/save roundtrips, paused inspection saves.
- This handoff.

No Core, Contracts, Persistence, NPC behavior/test files, tick integration, scene, settings, dependency, GUID or asset .meta changes. Uses the previous World schema14 implementation; compile that reader before loading. Unity remains6000.6.3f1. Native testing paused.

## Reproduce

From repository root:

```sh
python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py'
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/stores district --type business-core --seed 214 --blocks-across 2 --blocks-down 2 --store-layout compact
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/stores district --type business-core --seed 215 --blocks-across 2 --blocks-down 2 --store-layout spacious
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/stores city --seed 214 --districts garden-suburb business-core --house-layout compact --store-layout spacious
dotnet build OutbreakRebuild/Tests/StoreLayoutChecks.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/store-checks/obj/ -p:OutputPath=/tmp/store-checks/bin/
dotnet /tmp/store-checks/bin/StoreLayoutChecks.dll /tmp/stores
```

Actual Python `/workspace/.outbreak-tools/atlas-venv/bin/python`; actual SDK `/workspace/.outbreak-tools/dotnet/dotnet`. Required existing cloud .NET environment: DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home; XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime; XDG_DATA_HOME=/workspace/.outbreak-tools/data; NUGET_PACKAGES=/workspace/.outbreak-tools/nuget; DOTNET_CLI_TELEMETRY_OPTOUT=1; DOTNET_NOLOGO=1.

Run tests sequentially in each checkout. For current NPC integration, existing staging `/tmp/world-house-final-npc137` is an archive of0aa8ec34aa168e4797bfcd672053755dcdfaa034 plus prior narrow World schema14 changes, preserving all NPC source. New StoreLayoutChecks project is compiled there. Unified runner command:

```sh
python3 OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-store-npc137-results
```

## Acceptance and limitations

Final results and exact artifacts are recorded in the local transfer packet. Baseline40 Python tests; expanded44. Geometry verifies actual fixture collisions, both exterior approaches, internal circulation and stock/shelf/phone interaction access. Store API checks prove one store identity, all sales shelves counted, clerk assignment accepted and configured saves roundtrip exactly. No NPC decisions were ticked; stock fetching, restocking, shopping, supply calls and daily survival remain for the NPC developer's diagnostics.

Inspection saves are explicitly different from generated maps: one clerk per store,24 finite stockroom meals/store and initially empty shelves. They do not invent assigned homes or shifts. The save uses its map recipe seed. These are paused inspection fixtures, not the daily-routine scenario or a claimed starvation fix.

Keep the known NPC137 LegacyUI outline assertion separate from this geometry increment; it also fails on the unmodified NPC source. No native player build or Windows executable is claimed. The Lead should apply this narrow increment atop the previous house integration, preserve current NPC changes, re-run the unified runner sequentially, and coordinate live behavior observations with the existing NPC developer.

## Final results

- Python baseline:40 tests PASS (`/tmp/world-store-baseline.log`); final44 PASS (`/tmp/world-store-python.log`).
- Replay:36 found legacy/house/store recipe-map pairs canonical-identical (`/tmp/world-stores/replay.log`). Existing recipes/maps not rewritten.
- Focused final NPC137 build:PASS0 warnings/errors. Store checks:857 assertions PASS across4 maps/22 stores, with configured inspection saves exported. Exact map recipe seed retained for each inspection save. Evidence `/tmp/world-stores/build.log`, `infrastructure.log`.
- Unified NPC137 runner:PARTIAL/exit1. Configuration,692 core acceptance cases/742922 assertions and strict Windows ABI compilation PASS. LegacyUI fails existing assertion `Off-duty clerk or finishing courier lost duty outline distinction.` at UiHarness.cs:98. Matching unmodified-NPC137 baseline evidence is carried from the previous house validation (`world-house-npc137-baseline-ui.log`). No production C# source changed in this increment.
- NewOnly UI separately:PASS3 assertions/exit0 (`/tmp/world-stores/newonly.log`), after the fail-fast runner and focused checks completed.
- `git diff --check`:PASS. Native Unity/player/dialog execution:NOT RUN, paused.

Local packet `/workspace/shared/outbreak-store-interiors` contains maps, recipes, generation metadata, SVG plans, staffed inspection saves, source, patch, source revision and evidence. The sibling ZIP is the reviewable transfer artifact. Source branch is local; nothing from this increment was pushed or merged.

Examples: business-core214 compact5 stores (plus an explicit tighter-block compact214 example4 stores), business-core215 spacious4 stores, mixed-city214 with12 grouped homes and9 spacious stores. Use the `.obsave` for configured shops/clerks; `.obmap2` is deliberately unstaffed/empty. NPC behavioral acceptance remains pending with its existing owner.
