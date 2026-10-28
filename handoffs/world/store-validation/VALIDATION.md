# Store-validation fixture — validation and integration record

User approved the live-store diagnostic preparation. Baseline World19e18e74e54db7bb13a888a8ae9090c832af9bbf; local branch world/store-validation-fixture. New files only:

- OutbreakRebuild/Tools/generate_store_validation.py
- OutbreakRebuild/Tools/test_store_validation.py
- OutbreakRebuild/Tests/StoreRoutineFixture.cs
- OutbreakRebuild/Tests/StoreRoutineFixture.csproj
- handoffs/world/store-validation/NPC_HANDOFF.md
- handoffs/world/store-validation/DIAGNOSTIC_RESULTS.md
- handoffs/world/store-validation/VALIDATION.md

Existing tracked work was clean and preserved. No production generator, Core/Contracts/Persistence, NPC behavior or behavioral test, scene, setting, dependency, shared runner/test manifest or asset GUID changes. No replacement NPC agent was created. Native Unity remains paused at6000.6.3f1. Nothing from this fixture increment was merged, pushed or published.

## Actual results

- Python authoring checks:2 PASS. Deterministic fixture,12 real shelves,6 stock/pantry boxes,2 phones,5 grouped buildings plus DC, no fixture collisions, known crossing terrain, seed validation.
- Final focused .NET build:PASS0 warnings/errors.453 fixture assertions PASS across4 cases,5 assigned NPCs/case,203 finite meals/case and41 checked routes/case. Real World APIs accept homes/jobs/shifts, grouped stores, DC and park familiarity. Exact map/save roundtrips. No NPC decision ticks.
- Separate-process reproduction:12 geometry/map/save/manifest files byte-identical. The long-shopping case explicitly gives consumer4 both known store locations through authored prior-visit records, never hidden shelf food or a forced destination.
- Documented unified runner, actual NPC137+prior World schema14 source:PARTIAL/exit1. Configuration,692 core acceptance cases/742922 assertions and Windows strict ABI compilation PASS. LegacyUI fails the existing assertion `Off-duty clerk or finishing courier lost duty outline distinction.` at UiHarness.cs:98. That assertion was independently reproduced on unmodified NPC137 in the previous validation; its original log is included. Do not report this as a full pass or claim a new geometry regression.
- NewOnly UI run separately after the fail-fast runner and final focused build/export:PASS3 assertions/exit0.
- git diff --check:PASS. Native Unity/player/native desktop dialogs:NOT RUN, paused.

These are physical authoring, API and serialization checks. Clerk restocking, shopping, courier allocation/delivery, shifts, save continuation during actual actions and the four-death report remain PENDING with the existing NPC developer. No cross-chat contact or cause is claimed.

## Exact commands and environment

Repository `/tmp/outbreak-world-res-01`; NPC validation staging `/tmp/world-house-final-npc137` (archive0aa8ec34aa168e4797bfcd672053755dcdfaa034 plus prior narrow World schema14 patches). The new standalone fixture project was copied into that staging tree; it is not added to the NPC behavioral harness or shared runner manifest.

```sh
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_store_validation.py --output /tmp/world-store-validation --seed 214
/workspace/.outbreak-tools/atlas-venv/bin/python -m unittest discover -s OutbreakRebuild/Tools -p test_store_validation.py -v
```

Cloud .NET environment used:

```sh
export DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home
export XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime
export XDG_DATA_HOME=/workspace/.outbreak-tools/data
export NUGET_PACKAGES=/workspace/.outbreak-tools/nuget
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
```

From NPC staging, sequentially:

```sh
python3 OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-store-validation-unified
/workspace/.outbreak-tools/dotnet/dotnet build OutbreakRebuild/Tests/StoreRoutineFixture.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/store-routine-fixture/obj/ -p:OutputPath=/tmp/store-routine-fixture/bin/
/workspace/.outbreak-tools/dotnet/dotnet /tmp/store-routine-fixture/bin/StoreRoutineFixture.dll /tmp/world-store-validation/store-validation-214.obmap2 /tmp/world-store-validation/cases
/workspace/.outbreak-tools/dotnet/dotnet /tmp/store-routine-fixture/bin/StoreRoutineFixture.dll /tmp/world-store-validation-replay/store-validation-214.obmap2 /tmp/world-store-validation-replay/cases
/workspace/.outbreak-tools/dotnet/dotnet run --project OutbreakRebuild/Tests/UiChecks.csproj --no-launch-profile -p:InputMode=NewOnly -- /tmp/world-store-validation/newonly-report.txt
```

The replay directory's geometry was separately authored with the same generator/seed in a fresh process. Compared its3 geometry files plus9 case files against the original output byte-for-byte. Final evidence logs in packet/evidence.

## Integration and handoff

Apply the narrow new-file fixture patch on top of the prior store/house implementation, or review the local fixture commit. These tools do not alter production game behavior or automatically add a Unity menu entry. The packet's source snapshot supports reproducing the pinned saves; it must not overwrite the Lead/NPC's newer Core files.

Load `cases/store-validation-214-restocking.obsave` first in a matching encounter-prototype-106/schema14 build. Other cases and intended diagnostics are described in NPC_HANDOFF.md. Record actual behavioral outcomes in DIAGNOSTIC_RESULTS.md, with exact source/save hashes, actor IDs, ticks and coordinates. The Lead can relay the packet to the existing NPC developer. No direct delivery to their separate chat was possible here. Return demonstrated geometry failures to World; behavior/needs/movement/cargo fixes stay with their existing owner.

Local complete packet: `/workspace/shared/outbreak-store-validation`. Source revision and payload integrity hashes are included. No additional scope approval is requested to consume these fixtures.
