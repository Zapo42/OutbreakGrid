# Building layouts — handoff to Lead Designer

User-requested scope: physical layouts for homes with living/storage rooms, stores with multiple sales shelves and a separate stockroom, a distribution-center order computer site, a restaurant/bar with kitchen and dining areas and two entrances, and a church. This is a geometry/prototype handoff; NPC and gameplay integration belong to the existing coder/Lead. Lead task ID for this follow-on is pending; no new roadmap has been invented or edited.

## Source, ownership and artifacts

- Assignment/instructions read: AGENTS.md, PLANS.md, agents/world.md and approved CITY_LIFE_AND_TRANSPORT_PLAN.md from assignment commit `8de4e935dbf48bff36225741f12ff6d202842d40`, plus handoffs and AUTOMATED_CHECKS.md.
- Maps base: `8e92e361206507219a725d698fca55dd26de5ff9`, branch `maps/city-reference-pack-20261026`.
- Prior residential/park increment: local `2843a8292ec9eea67e88a9cd7c0e0cffbf2f9bbc`.
- This layout increment: local `ba42070d23bb8c5e9482e22ea8025e4b0cd69574`, branch `world/res-01`, worktree `/tmp/outbreak-world-res-01`; clean after commit.
- Original `/workspace/OutbreakGrid` uncommitted map work preserved. No source reset, merge or publication performed.
- Same four exclusively assigned paths changed:
  - OutbreakRebuild/Tools/generate_reference_city.py
  - OutbreakRebuild/Tools/generate_district_city.py
  - OutbreakRebuild/Tools/test_district_city.py
  - OutbreakRebuild/CityReferences/districts/README.md
- No production C#, shared Contracts, persistence, NPC behavior/tests, scenes/settings, dependencies, assets, .meta files, existing catalogue/recipes or bundled maps changed. Native Unity remains paused at pinned6000.6.3f1.
- Lead-owned PLANS.md, role documents, BUILD_VALIDATION.md and harness/build-log.md were not edited. Please transfer accepted evidence into those records.

The `pack/` folder contains `index.html`, `manifest.json`, ten technical SVG plans, ten draft blueprint JSON files and ten loadable OBMAP2 terrain shells. `BUILDING-LAYOUTS.patch` applies this increment on top of prior2843a82. `MAPS-COMBINED.patch` includes both new increments against8e92e36. `world-layouts.bundle` contains both local commits and requires the existing8e92e36 base. `source/` contains final copies of the four files for review.

## What is built

Every type has compact and spacious variants and supports all four cardinal rotations. Local-cell blueprints contain stable role-based IDs, room floor cells, walls, internal/external doors, proposed fixture footprints, approach cells and relevant draft interaction sites. The seed selects pack orientations deterministically. Units are simulation cells; destination maps supply their metric scale. These are fictional game layouts, not measured architectural plans.

| Type | Physical layout and proposed fixture sites |
| --- | --- |
| House | Larger living area with seating; smaller storage room behind an internal door. Storage shelf is in the storage room. No bed. Recovery region covers usable living-area floor. |
| Store | Multiple sales shelf positions (six compact, eight spacious), reachable aisles, separate stockroom with storage shelf, internal door, public and receiving entrances. |
| Distribution center | Dispatch office with order-computer footprint and adjacent courier interaction cell; separate warehouse with racks and service entrance. |
| Restaurant/bar | Separate dining/bar and kitchen rooms, public dining and kitchen service entrances (exactly two external entrances), internal staff door, three across-counter handoff positions with opposite waiter/cook approaches, tables/bar/cooking/storage sites. |
| Church | Sanctuary, central aisle, pews and altar; separate vestry/storage room reached through an internal door. |

Preview maps have connected perimeter streets/sidewalks and physical external entrance paths. Their walls/doors genuinely separate the two floor regions. Geometry validation also treats all proposed fixtures as solid, ensuring that final fixture placement need not obstruct doors, aisles or interaction sites.

## Runtime boundary — essential

OBMAP2 shells are schema1 existing terrain only. All fixture objects and planned interactions remain **inactive draft hints in opaque authoringJson** and technical plans. Preview `fixtures_materialized` and `mechanics_enabled` are false. The shell does not supply live furniture collision, shelf inventory, storage, rest spots, order computers, cook/waiter actions, food transfers, church mechanics or new NPCs. In-game actors could walk over non-wall fixture hint sites until integration materializes those fixtures. The technical plan shows their proposed blocked footprints.

The counter's physical map shell uses Wall terrain as a solid partition placeholder. Current Wall visibility/action rules do not implement across-counter service. Counter collision, visibility and interaction implementation require agreement; do not infer functional service from the map shell.

Current C# room discovery treats each enclosed floor region as a separate shelter. Each example therefore discovers **two rooms**, not one multi-room building. The new templates are not automatically applied to procedural districts or existing saves. That avoids silently changing home/store identity or consuming new fixture contracts before agreement.

## Required Lead/NPC coordination

1. Agree building identity and room membership first. Preserve established building/home/store/storage IDs and old saves; room discovery currently derives shelter IDs from floor components. Do not attach each room as a separate house/store by accident. Use per-instance namespaces when stamping repeated template IDs into cities.
2. Implement the user's living-area recovery requirement with the NPC coder: geometry identifies the living region without a bed; recovery activation, target selection and rates are not changed here.
3. Map multiple sales shelves and backroom stock fixtures to the existing finite inventory system, keeping stockroom membership under its store/building and preserving walk/interaction access through internal doors.
4. Implement the user's physical order-computer requirement with the NPC coder: clerk calls must no longer automatically generate/assign courier jobs without a terminal visit. Agree the pending-call/request representation, terminal approach, physical claim/acknowledgment and save/interruption contract. This pack supplies the computer footprint and reachable approach; it does not modify current calls or orders.
5. Restaurant: preserve the two entrance-to-room associations and the kitchen/dining sides of each counter pair. Agree object collision/visibility and interaction/transfer contracts before cooks/waiters are added. No restaurant/bar mechanics or staffing decisions are implemented here.
6. Church: geometry only. No church-specific behavior, clergy, schedules or mechanics have been assumed.

The Lead may accept the templates independently of gameplay integration. Source main/Lead Foundation131 lacks the maps pipeline. Import/review the mapped baseline Python/data files before applying patches; never replace newer Lead/NPC C# with older C# from the maps branch. PLANS.md also mentions incomplete Foundation132 work not available here: preserve its actual state and verify against that integration checkpoint when available. Hosted evidence below covers the available assignment Foundation131 C# source only.

## Reproduction and validation

Python environment: `/workspace/.outbreak-tools/atlas-venv/bin/python` with existing Shapely/pyproj dependencies. Commands below run from `/tmp/outbreak-world-res-01` unless an absolute path is used.

```bash
/workspace/.outbreak-tools/atlas-venv/bin/python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py' -v
```

Baseline:31 PASS (`/tmp/world-buildings-baseline.log`). Final:37 PASS (`/tmp/world-building-tests.log`). New tests cover all40 type/variant/rotation combinations, fixture-aware access, independent floor-component comparison, external-door sidewalk neighbors, house room/recovery roles, multiple store shelves, computer approach, restaurant entrance/counter pairs, rejected blocked approaches/missing internal door/invalid rotation, church inclusion, and deterministic pack replay with explicitly inactive mechanics.

```bash
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py building-pack --directory /tmp/outbreak-building-handoff/pack --seed 214
/workspace/.outbreak-tools/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py building-pack --directory /tmp/outbreak-building-replay --seed 214
diff -qr /tmp/outbreak-building-handoff/pack /tmp/outbreak-building-replay
```

PASS, exit0: all generated pack files identical in fresh processes. Source API examples:

```python
import generate_reference_city as lots
import generate_district_city as districts
blueprint = lots.building_blueprint('restaurant-bar', 'spacious', rotation=1)
lots.validate_building_blueprint(blueprint)
document, metadata = districts.building_preview_map(blueprint)
# All blueprint positions are local. metadata['origin_cells'] locates them in this preview.
```

Legacy-map replay command:

```bash
PYTHONPATH=OutbreakRebuild/Tools /workspace/.outbreak-tools/atlas-venv/bin/python - <<'PY'
from pathlib import Path
import json
import generate_district_city as d
import generate_seeded_city as s
for f in sorted(Path('OutbreakRebuild/CityReferences/districts/maps').glob('*.recipe.json')):
    r=json.loads(f.read_text())
    got=(d.compose_city(r) if r['kind']=='city' else d.generate_district(r))[0]
    expected=json.loads(f.with_name(f.name.replace('.recipe.json','.obmap2')).read_text())
    assert s.canonical(got)==s.canonical(expected),f
    print('PASS', f.name)
PY
```

PASS all20 existing bundled maps exactly unchanged (`/tmp/world-building-legacy-replay.log`). Existing fixtures/maps were not rewritten. `git diff --check`:PASS. Final fresh-process pack reproduction includes deterministic SVG technical plans, not native rendering evidence.

Full hosted checks and actual C# map import/room discovery results follow after completion. Native Unity editor/runtime, native Windows dialogs/player and Mono execution:NOT RUN; user-paused native testing preserved. Independent QA and Lead acceptance remain pending.

## Completed hosted and production-shell checks

Hosted validation staging was extracted from assignment commit8de4e9 at `/tmp/world-building-integration`, with the four final World files overlaid for inspection. No C# or runner files were edited there. This branch predates the repaired runner, so its older runner was not changed. The Lead's current documented runner was executed sequentially:

```bash
DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime XDG_DATA_HOME=/workspace/.outbreak-tools/data NUGET_PACKAGES=/workspace/.outbreak-tools/nuget DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 python3 /tmp/world-building-integration/OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/.outbreak-tools/dotnet/dotnet --output /tmp/world-building-hosted-results
```

PASS, exit0: Unity target/module configuration;627 core acceptance cases /737155 assertions; real Windows branch compilation and64-bit ABI; Legacy UI2463 assertions; NewOnly UI3 assertions. Log `/tmp/world-building-hosted.log`; combined report `/tmp/world-building-hosted-results/FOUNDATION_CHECKS.txt`. This is a C# regression guard against available Foundation131, not proof of live fixture/behavior integration or native rendering. Python generation is validated separately by the37 tests.

After hosted checks completed, a disposable source-level shell test was built against the same actual Lead C# Contracts/Core/Persistence sources. Packaged portable harness `validation/ShellChecks.csproj` and `ShellChecks.cs` checks all ten maps with the production parser, exact map serialization round-trip, retained opaque authoring hints, explicitly inactive mechanics, and actual World shelter discovery.

```bash
DOTNET_CLI_HOME=/workspace/.outbreak-tools/dotnet-home XDG_RUNTIME_DIR=/workspace/.outbreak-tools/runtime XDG_DATA_HOME=/workspace/.outbreak-tools/data NUGET_PACKAGES=/workspace/.outbreak-tools/nuget DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 /workspace/.outbreak-tools/dotnet/dotnet run --project /tmp/outbreak-building-handoff/validation/ShellChecks.csproj -p:GameProject=/tmp/world-building-integration/OutbreakRebuild --no-launch-profile -- /tmp/outbreak-building-handoff/pack
```

PASS all10 shells; each discovers the expected two enclosed rooms. No actors spawned, no NPC behavior changed or authored behavioral tests run by this temporary harness. Log `/tmp/world-building-portable-shell-checks.log`. On another checkout, replace the executable and GameProject paths and pass the unpacked `pack/` directory. Run sequentially after the unified runner. This disposable harness does not edit the repository's shared test manifests.

Validation/evidence copies are included in this packet. The single-commit patch and combined baseline patch contain only the four owned repository files; temporary validation files, review maps and plans are packet artifacts, not committed shared files.

## Integration procedure for the Lead

1. Preserve current uncommitted work and the actual latest NPC integration checkpoint. Review the four source changes and this runtime-boundary section.
2. With the existing8e92e36 maps base available, the offline bundle can expose the review branch without merging or publishing:

```bash
git bundle verify /path/to/world-layouts.bundle
git fetch /path/to/world-layouts.bundle refs/heads/world/res-01:refs/heads/review/world-layouts
```

3. `BUILDING-LAYOUTS.patch` is only the new layout step (base2843a82). `MAPS-COMBINED.patch` combines the residential/parks step and layouts step (base8e92e36). Apply only the appropriate one after independent review. Avoid merging the entire older maps C# ancestry over later NPC work. Source copies are provided for inspection, not as permission to overwrite divergent files.
4. Run the Python generation checks in the integrated maps checkout, generate a fresh pack, run the documented unified hosted runner sequentially, then the portable shell geometry check. Confirm existing map recipe compatibility on the actual integration tree.
5. Record acceptance in Lead-owned PLANS.md, BUILD_VALIDATION.md and harness/build-log.md. Agree the room/building identity and fixture/NPC interfaces before automatic placement into live cities. No automatic code/scene/save migration is included here.

No independent merge, push or publication was performed. Native Unity/Windows player validation remains NOT RUN. No next implementation task beyond the user's requested layouts is assumed.
