# World handoff — distribution-center interiors

User selected option1: physical distribution-center interior and order-computer
position. Baselineb78fccc (published store-validation handoff), previous live store
implementation19e18e7. Local branch world/distribution-center-interiors; packet
SOURCE-REVISION.txt records final commit. No merge/push/publication was requested
for this increment. Original work and NPC source remain preserved.

Inspected AGENTS.md, agents/world.md, PLANS.md and approved city-life roadmap from
the assignment history and current Lead source. Lead main advanced to
1b05a73c0994720230524c519dac84fc58d0bbd3 with the earlier house integration; its
actual reader is used for current integration validation. Latest dedicated NPC
branch remains0aa8ec34aa168e4797bfcd672053755dcdfaa034. No replacement NPC agent or
claimed direct contact with the separate NPC chat.

## Delivered scope

- Compact/spacious opt-in live dispatch-office and warehouse rooms, one DC identity.
- Internal door; public and service entrances with street-connected sidewalks.
- One real empty capacity100 warehouse supply box, existing reserve6 policy.
- Accessible reserved order-computer position in dispatch. It is solid Wall terrain8,
  **not a computer object or interaction**. Native appearance remains a wall placeholder.
- Stable namespaced computer ID, world cell and approach cells in existing authoring
  metadata. No new Contracts/Persistence fields or schema; prior schema14 required.
- Mixed-city composition with homes, stores and a DC; fixture coordinates and room
  references move together. First business district receives one DC; subsequent
  urban/business districts use the store flag. Pure DC districts default to one center.
- Existing store materialization shares the supply-building geometry helper; prior
  store recipe/map output remains unchanged, checked by canonical replay.

No NPC needs, decisions, courier movement, cargo, order generation/claiming or
behavioral tests changed. No scenes/settings, dependencies, Unity version, asset
GUIDs/.meta or shared runner manifests changed. Native testing remains paused at
6000.6.3f1. This is the existing generator/import workflow; no C# CitySetup/menu rewrite.

## Exact changed files

- OutbreakRebuild/Tools/generate_reference_city.py
- OutbreakRebuild/Tools/generate_district_city.py
- OutbreakRebuild/Tools/test_district_city.py
- OutbreakRebuild/CityReferences/districts/README.md
- OutbreakRebuild/Tests/DistributionLayoutChecks.cs
- OutbreakRebuild/Tests/DistributionLayoutChecks.csproj
- This handoff.

## Computer/NPC and graphics integration contract

`interior_layout.order_computer` contains id, cell, approach_cells,
room_role=dispatch-office, representation=reserved-solid-terrain and
interaction_implemented=false. Its ID is instance_id + /order-computer, with city
prefixes applied consistently. Reserved cells are listed in reserved_fixture_cells.
`stock_box_cells` is the authoritative warehouse supply coordinate. Additional
rack positions remain hints because current DC state has one supplyBox.

The actual computer cell is blocked and has no floor-based HouseAt identity.
Use its walkable approach or dispatch floor to find the center. The focused tool
exports distribution-sites.json with actual center/supply IDs and all positions.
These records define geometry sites, not an approved new runtime computer API.

NPC developer: retain current behavior until you implement and validate the agreed
computer visit/receipt/claim sequence. No physical-computer requirement is enabled
by this patch. Return failed origin/target coordinates plus the stateful save for
World review. Graphics: replace the reserved-wall presentation with a computer
object/sprite when its physical/persistence contract is assigned. Do not treat the
placeholder as final computer artwork or claim functional order collection.

## Examples and save compatibility

Three example maps: compact business-core214 (one DC), spacious business-core215
(one DC), mixed-city214 (homes + configured stores + one spacious DC). Each map
has recipe, generation metadata and technical SVG. Maps start with empty inventory
and no inhabitants. Focused checks emit separate inspection saves with80 finite
warehouse meals per center. The mixed save has one courier assigned a genuine
separate generated home and a07:00–19:00 shift; stores are configured but unstaffed
and empty. Standalone saves have no fabricated warehouse-home assignment.

**Use the save matching your actual game source.** Lead-main and NPC137 inspection
saves are separated, since their saved rule versions differ. Maps load on either
matching schema14 reader, but the older Lead build cannot load NPC137 rule saves.
Exact source revisions/rule versions are included with each inspection directory.
No production save migration is performed. These are paused inspection fixtures;
no delivery, pickup, survivability or daily-routine outcome is claimed.

## Commands

From the World repository with existing Shapely/pyproj dependencies:

```sh
python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py'
python -m unittest discover -s OutbreakRebuild/Tools -p test_store_validation.py
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-dc/maps district --type business-core --seed 214 --blocks-across 2 --blocks-down 2 --distribution-layout compact
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-dc/maps district --type business-core --seed 215 --blocks-across 2 --blocks-down 2 --distribution-layout spacious
python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-dc/maps city --seed 214 --districts garden-suburb business-core business-core --house-layout compact --store-layout compact --distribution-layout spacious
```

Actual Python /workspace/.outbreak-tools/atlas-venv/bin/python; SDK
/workspace/.outbreak-tools/dotnet/dotnet (8.0.427). Hosted .NET environment:
DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home;
XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime;
XDG_DATA_HOME=/workspace/.outbreak-tools/data;
NUGET_PACKAGES=/workspace/.outbreak-tools/nuget;
DOTNET_CLI_TELEMETRY_OPTOUT=1; DOTNET_NOLOGO=1.

In a current game source checkout, sequentially:

```sh
python3 OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-dc-main-results
dotnet build OutbreakRebuild/Tests/DistributionLayoutChecks.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/dc-checks/obj/ -p:OutputPath=/tmp/dc-checks/bin/
dotnet /tmp/dc-checks/bin/DistributionLayoutChecks.dll /tmp/dc-input
```

The focused tool takes a directory with at least3 example OBMAP2 maps and exports
matching inspection saves plus distribution-sites.json there. Include one mixed
map with a real separate home to exercise courier assignment. Core production
sources are unchanged; stage only this new test source/project over the actual
Lead/NPC baseline. Do not overwrite either baseline's Core from the older maps branch.

## Assembly

Apply the narrow seven-file patch on top of the prior house/store pipeline. Lead
main already contains earlier World houses but may still need the published store
increment19e18e7; preserve newer shared C# sources. The packet includes the exact
final generator source snapshot for review, not a wholesale game ancestry merge.

Load maps with the prior schema14 reader, configure centers at their warehouse
supply box with the existing API, or load the matching configured inspection save.
Run tests sequentially within the actual integrated checkout. Lead retains ownership
of PLANS.md and shared build-log records; these results are supplied here for integration.

Next work is a user choice: NPC-owned computer behavior integration, restaurant/bar
interiors, or further city-generation work. No unrelated roadmap work was started.

## Final validation results

- Baseline44 Python city tests PASS; final48 PASS. All4 rotations and both sizes
  checked, as well as blocked computer approaches, missing solid footprint,
  incompatible recipes, version controls, deterministic replay and mixed cities.
- Existing store-validation authoring tests:2 PASS after the helper refactor.
-39 legacy/house/store/DC recipe-map pairs replay canonical-identically.
-12 final DC map/recipe/metadata/technical SVG files byte-identical across fresh CLI
  processes. The early compact preview was refreshed after adding the computer
  marker; all final technical previews match the final renderer.
- Current Lead1b05a73 unified runner PASS/exit0: configuration;627 core acceptance
  cases/737155 assertions; strict Windows ABI compilation; LegacyUI2463 assertions;
  NewOnly UI3 assertions. Actual log evidence included, not a native Unity claim.
- Focused DC project on Lead1b05a73:PASS93 assertions,3 maps/3 centers/1 real-home
  courier assignment, build0 warnings/errors. Inspection rules encounter-prototype-100.
- Focused DC project on NPC0aa8ec34+prior World schema14:PASS93 assertions, same
  coverage, build0 warnings/errors. Inspection rules encounter-prototype-106.
- Full NPC137 runner was not repeated for this Python/geometry increment. The prior
  independently reproduced NPC137 LegacyUI outline failure remains outside World
  scope; focused DC compatibility is not a claim that the whole NPC branch is green.
- Exact map/save roundtrips retain computer metadata, configured center identity,
  finite stock/reserve accounting and mixed-map courier/home/shift assignment.
- git diff --check PASS. No NPC decision ticks in focused tests. Native Unity,
  native dialogs and Windows player execution NOT RUN, paused.

Local packet `/workspace/shared/outbreak-distribution-centers` includes the narrow
patch, final source, generator prerequisites/profile snapshots, examples, site
manifests, **separate** Lead100 and NPC106 inspection saves, pinned validation
source snapshots, exact logs, hashes and this handoff. This increment is local and
not pushed. Existing World store-validation branch remains published separately.
