# Live generated houses — implementation handoff

Implemented under the user's direct instruction to proceed. The user makes scope/interface decisions; the Lead assembles/integrates and publishes. No additional Lead approval is being requested or used as an implementation gate. NPC decisions/needs/movement/driving/cargo logic remain with the dedicated NPC developer. This change supplies physical room/fixture data and the spatial membership needed to use existing systems.

## Source and integration

- Implementation worktree `/tmp/outbreak-world-res-01`, branch `world/res-01`, clean after commits.
- Prior layout source `ba42070d23bb8c5e9482e22ea8025e4b0cd69574`.
- House placement commit `2363404b2d32f38dc669e9bc419159d6c40d944b`.
- Final area-marker/resident compatibility commit `20380bec26bf680865a2b22662872db115a390c4`.
- Underlying maps baseline `8e92e361206507219a725d698fca55dd26de5ff9`; prior residential/park commit2843a82 and building layouts ba42070 are prerequisites for the Python changes.
- Original `/workspace/OutbreakGrid` unfinished map work and checkout preserved. No merge, push or publication performed.
- Instructions/roadmap/handoffs inspected from docs assignment8de4e9 and latest NPC branch0aa8ec34. The user's newer direction supersedes the old Lead approval requirement.
- Current published main inspected at f1b170b (Foundation131); latest NPC implementation inspected/tested at0aa8ec34aa168e4797bfcd672053755dcdfaa034 on npc/f137-familiar-greetings.

The root `HOUSE-PLACEMENT.patch` contains both house commits against ba42070 (15 paths; `changed-files.txt`). `SHARED-FILES.patch` contains only the five narrow changes to existing C# files, verified to apply to both the Lead Foundation131 and latest NPC137 trees. New/exclusively owned files are supplied under `source/`. Shared-file replacements from the older maps checkout are deliberately not supplied as overwrite instructions. Apply the narrow patch to the actual latest game tree, preserving NPC/Lead work.

`world-houses.bundle` exposes the complete local World branch for review with an8e92e36 prerequisite. It includes prior map increments; do not merge its older game ancestry over newer NPC branches. The Lead can import review refs or apply appropriate commits/patches, then assemble and publish. No native Unity player build is claimed.

## Working behavior

Use the new optional `--house-layout compact|spacious` flag for generated districts or mixed cities. Residential districts become house-only, one-storey template layouts; other district types keep existing geometry/use hints. Default recipes and old maps are unchanged.

- Houses have a larger living room and a smaller storage room separated by a real wall/internal door.
- Both rooms and their internal doorway belong to one home identity. Internal doors are excluded from the outdoor-exit list.
- Numeric home identity remains the canonical smallest floor-cell key plus one. Group membership is explicit, so other ordinary buildings remain independent.
- A real empty household storage box is placed in the storage room using the existing30-capacity default and existing reserve behavior. No food or NPC population is generated.
- Usable living-area cells carry existing rest spots with `area: true`, receiving the existing recovery effect without new energy rates or decision rules. The renderer skips bed furniture for these area markers. Ordinary beds keep their existing graphics and semantics.
- Seating remains a geometry/graphics hint; no sofa object implementation is claimed.
- Lot gaps/setbacks and street reservations are preserved. Rotations fit each lot; exterior doors connect to the original street network. Existing pedestrian spurs remain shared walk space.
- Mixed-city assembly moves group anchors, fixture coordinates, layout origins and parcel/room keys together. Limits include physical-room and recovery-site budgets.

The C# room membership implementation is generic infrastructure. It can support the later store/restaurant/church layout work, but only house templates are automatically materialized in this increment. Prototype building-pack exports remain inactive schema1 layouts. Restaurant staff, order computers, cooking/counter mechanics and church-specific behaviors are not added here. C# CitySetup/default UI city-generation menus are not rewritten; use the generator flag or load the provided maps after compiling the matching reader.

## Map/persistence interface

Map schema14 adds optional `buildingRooms` records: `{id, rooms:[{id,role,x,y},...]}` where x/y are floor anchors for separate, door-connected physical rooms. It also supports `RestSpotDocument.area` (optional false by default). Schema14 requires membership records or area-marked rest spots. Grouping and area flags are retained through capture/restore; old maps/saves without them retain previous output and behavior. Older clients cannot read schema14 maps, so compile the new contract/core files before importing these examples. Existing saves are not automatically repartitioned/migrated.

Membership records are detached, canonical and bounded. Invalid/duplicate identities, overlapping room components, invalid anchors, missing enclosure, disconnected groups, invalid entrances and floor/room limits are rejected before replacing existing membership. Floor cells determine capacity/targets; internal door membership permits existing indoor traversal across the partition. Only exterior doors participate in exits. The new metadata consumes no RNG.

`Core/RestSpots.cs` changes are map storage/capture/restore only. Its NPC preference routine is unchanged. Recovery rates remain in the existing shelter recovery code, which is not modified. The tiny `FoundationApp.cs` render guard prevents area cells from being drawn as beds; no UI redesign or unrelated outline fix is included. New C# assets have stable .meta GUIDs; preserve them. Unity remains6000.6.3f1, dependencies/scenes/settings unchanged; native testing remains paused.

## Changed files

- Contracts/Documents.cs: optional membership array and optional rest-area marker.
- New Contracts/BuildingRoomsDocuments.cs (+.meta): typed building/room anchor records.
- New Core/BuildingRooms.cs (+.meta): validation, defensive records and spatial grouping.
- Core/WorldMap.cs: schema14, capture/restore, area-marker terrain cleanup.
- Core/Shelters.cs: grouping hook and internal-door membership.
- Core/RestSpots.cs: persist/restore area markers with legacy defaults.
- Unity/FoundationApp.cs: one bed-rendering exclusion for area markers.
- Tools/generate_reference_city.py: template fitting, connected doors and materialized fixtures/membership.
- Tools/generate_district_city.py: opt-in recipes/CLI, mixed-city transforms, budgets and diagnostic SVGs.
- Tools/test_district_city.py: generator geometry/replay/fixture/composition cases.
- New Tests/BuildingRoomChecks.cs and .csproj: focused spatial/persistence infrastructure checks; existing NPC test files and shared test manifests untouched.
- CityReferences/districts/README.md: usage/interface/limits.

## Provided maps

All files are in `maps/`; each includes `.obmap2`, `.recipe.json`, `.generation.json` and diagnostic `.svg`:

| Example | Grid | Buildings | Grouped houses | Living recovery cells | Storage boxes |
| --- | --- | --- | --- | --- | --- |
| suburban-grid214 compact |70x72|7|7|231|7|
| suburban-grid215 spacious |79x67|4|4|244|4|
| garden-suburb214 compact |66x69|4|4|132|4|
| mixed-city214 garden-suburb + business-core |282x126|38|12|396|12|

The mixed city has38 shelter/building identities even though its12 homes contain two physical rooms each. Business roles remain existing authoring hints; no jobs/supply stocks/population were invented. SVG green markers show recovery areas and brown markers show storage; native rendering was not executed.

## Exact reproduction

From a maps checkout with the existing atlas Python dependencies:

```bash
/workspace/.outbreak-tools/atlas-venv/bin/python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py' -v
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-handoff/maps district --type suburban-grid --seed 214 --blocks-across 2 --blocks-down 2 --house-layout compact
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-handoff/maps district --type suburban-grid --seed 215 --blocks-across 2 --blocks-down 2 --house-layout spacious
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-handoff/maps district --type garden-suburb --seed 214 --blocks-across 2 --blocks-down 2 --house-layout compact
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-handoff/maps city --seed 214 --districts garden-suburb business-core --house-layout compact
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-replay replay /tmp/world-house-handoff/maps/suburban-grid-214-9cea487c8d68.recipe.json
cmp /tmp/world-house-replay/suburban-grid-214-9cea487c8d68.obmap2 /tmp/world-house-handoff/maps/suburban-grid-214-9cea487c8d68.obmap2
```

PASS:40 Python tests (baseline37); fresh-process active-map replay byte-identical; all20 existing bundled map recipes replay canonical-identically. No existing fixtures/recipes/maps rewritten. Logs are in `evidence/`.

For .NET commands, this environment used:

```bash
export DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home
export XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime
export XDG_DATA_HOME=/workspace/.outbreak-tools/data
export NUGET_PACKAGES=/workspace/.outbreak-tools/nuget
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
```

Run checks sequentially in each checkout. The two validation staging trees were archives of their respective baseline commits with only this change overlaid, not branch merges. Shared patches applied cleanly to both without replacing NPC files.

```bash
python3 /tmp/world-house-final-main131/OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-house-final-main131-results
python3 /tmp/world-house-final-npc137/OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-house-final-npc137-results
```

**Foundation131 with final changes:PASS, exit0.** Configuration;627 core cases /737155 assertions; Windows strict compilation/64-bit ABI; UI Legacy2463 assertions; UI NewOnly3 assertions.

**Latest NPC137 with final changes:PARTIAL, exit1 at pre-existing Legacy UI assertion.** Configuration,692 core cases /742922 assertions and Windows ABI PASS. Legacy UI fails `Off-duty clerk or finishing courier lost duty outline distinction.` at UiHarness.cs:98. This exact failure was reproduced on the unmodified0aa8ec34 branch:

```bash
# In /tmp/world-house-npc137-ui-baseline, an unmodified archive of0aa8ec34:
/workspace/.outbreak-tools/dotnet/dotnet run --project OutbreakRebuild/Tests/UiChecks.csproj --no-launch-profile -p:InputMode=Legacy -- /tmp/world-house-npc137-baseline-ui-report.txt
```

Exit1, same assertion/line. The NPC137 handoff already states general UI integration was not verified. No unrelated UI repair was attempted. Source main/Lead and latest NPC evidence are distinct; do not report NPC137 as fully passing. NewOnly was run separately after the fail-fast runner and focused checks; its result is recorded in evidence. The UI outline issue should be resolved by the UI/NPC integration owner before release.

Focused final infrastructure checks against the actual NPC137 sources plus this change:

```bash
# From /tmp/world-house-final-npc137, after the unified runner completed:
/workspace/.outbreak-tools/dotnet/dotnet build OutbreakRebuild/Tests/BuildingRoomChecks.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/world-house-final-npc137-infra/obj/ -p:OutputPath=/tmp/world-house-final-npc137-infra/bin/
/workspace/.outbreak-tools/dotnet/dotnet /tmp/world-house-final-npc137-infra/bin/BuildingRoomChecks.dll /tmp/world-house-handoff/maps
```

**PASS1264 infrastructure assertions, exit0.** Grouped versus legacy identity; internal/exterior doorway membership; defensive records; schema/area-only maps and corrupt input; stale area cleanup on terrain edits; real finite empty storage collision; all recovery markers distinguish areas from beds; exact map/save round-trips; a resident physically in storage can be assigned to the living-room home and retains that assignment after save/restore. No NPC decision ticks were added to this focused project.

`git diff --check`:PASS. Offline Git bundle verified. Native Unity editor/runtime, native dialogs, Windows player and Mono execution:NOT RUN; native testing remains paused. No .exe/release upload claimed.

## Assembly checklist

1. Preserve the latest NPC/Lead checkout. To load these maps, apply `SHARED-FILES.patch` and copy the two new asset sources with their .meta files from `source/OutbreakRebuild/Assets/Outbreak/` into matching paths. Do not overwrite shared files wholesale from the older maps branch.
2. Include the new focused test project/source. Existing C# project wildcards and Unity asmdefs pick up the new production files; no package, editor or scene changes are needed.
3. For regeneration, retain the existing maps pipeline/data through ba42070 and apply the full house patch (or appropriate two commits). Shared changes already applied in step1 must not be applied twice. The underlying immutable published maps branch supplies prerequisites if needed.
4. Validate sequentially as above on the actual assembly tree. Native tests remain paused. Preserve the known NPC137 UI-failure distinction rather than marking it as a house regression or a passing release.
5. The Lead can update its records and publish the assembled source through the normal workflow. This handoff requests no further scope approval. No independent merge/push was performed here.

Final separate NPC137 NewOnly UI check:PASS3 assertions, exit0 (evidence/world-house-final-npc137-newonly.log). Final focused infrastructure check:PASS1264 assertions. Source/patch/bundle paths are local artifacts; no remote publication was performed.
