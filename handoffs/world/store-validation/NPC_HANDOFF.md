# NPC developer handoff — live-store routine validation

User approved this test preparation after the World live-store increment. World owns geometry; the existing NPC developer owns all behavior diagnostics and fixes. This is a concrete local fixture and test request, not a claim that cross-chat delivery or behavioral acceptance occurred. No tool in this chat can contact the separate NPC chat directly. Give this packet to that existing developer through the Lead.

## Inputs and source

- World store source `19e18e74e54db7bb13a888a8ae9090c832af9bbf`.
- NPC source `0aa8ec34aa168e4797bfcd672053755dcdfaa034` plus narrow prior World20380be room/area-reader changes. Exact source snapshot included for reproduction; do not overwrite newer NPC work with it.
- New local branch `world/store-validation-fixture`. Packet SOURCE-REVISION.txt identifies the final fixture commit.
- Seed214. Controlled100×56 authored map uses the live compact house/store materializers, not a replacement generator. Two six-shelf stores, three two-room homes, simple one-room DC, marked crossings, connected sidewalks and one park.
- Saves use `encounter-prototype-106` rules and schema14 maps; load them with the matching NPC137 + World reader, not an older game build.
- Every case begins tick0 /07:00 before any decision tick. No zombies, vehicles, pending fabricated orders, dispatch-computer mechanics or forced shopping destinations.
- Load the **.obsave** to preserve homes, roles, shops, shifts and familiarity. Map-only import does not retain assignments or configured store/DC state.

## Cases — same203 finite initial meals

| Save suffix | Store A stock/shelves | Store B stock/shelves | DC | Home pantries | Carried |
|---|---|---|---|---|---|
| restocking |30 /0|30 /0|120|6 each|1 each|
| shopping |0 /30|0 /30|120|6 each|1 each|
| delivery |0 /0|0 /0|180|6 each|1 each|
| long-shopping |0 /0|0 /30|168|0 each|1 each|

Shelves in shopping cases have5 meals each across6 shelves; the existing store shelfTarget remains6. Stores may therefore make legitimate supply requests even in shopping cases. These cases change inventory placement, not AI priorities or knowledge of hidden stock. Equal total food is a control, not proof that each destination is sufficiently supplied or that any person can access it in time.

Files: `cases/store-validation-214-<suffix>.obsave` and matching `.obmap2`. The geometry-only map is `store-validation-214.obmap2`; it is unpopulated and empty. The manifest `cases/assignments-inventories-routes.json` contains exact stock positions/amounts, initial counters, actor IDs, home/work IDs and full measured route cells for every case.

## Assignments

| Stable ID / index | Job | Home ID | Workplace ID | Spawn | Shift |
|---|---|---|---|---|---|
|1 /0|Courier|3745 (Home C)|842 DC|(42,12)|07:00–19:00|
|2 /1|Clerk|3710 (Home A)|910 Store A|(20,18)|07:00–19:00|
|3 /2|Clerk|3778 (Home B)|978 Store B|(88,18)|07:00–19:00|
|4 /3|Consumer|3710 (Home A)|—|(16,43)|none|
|5 /4|Consumer|3778 (Home B)|—|(84,43)|none|

Both workers and consumers retain real home assignments. All actors start hunger10, one carried food and ordinary seeded spawn attributes. Each resident starts familiar with the nearest shop through the existing explicit resident/shop-familiarization API. In long-shopping only, consumer4 additionally has two authored prior-visit location notes at tick0, so both stores are known without shelf-stock knowledge. These normal notes expire under existing3600-tick memory rules; record any expiry. No hidden inventory knowledge is granted. Park destination(63,40) is explicitly familiar through TryRememberPark. Existing behavior decides whether and when to visit.

## Static access already checked by World

41 cardinal pedestrian/interaction routes per case. Runtime collision checks respect solid shelves, boxes and phones; use Floor, Sidewalk, Crosswalk and traversable door cells, excluding grass and ordinary-road shortcuts.

- Home A→Store A33 steps; Home B→Store B33.
- Home A→Store B101 steps, available for longer shopping diagnostics.
- DC→Store A58; DC→Store B62; Courier Home C→DC40.
- Both public/service entrances and internal stockroom doors have measured routes.
- Every sales shelf and draft stock approach, phone approach, each actual household pantry approach, DC stock approach and park approach is checked.
- 41 routes are repeated across four inventories because geometry is shared; these are not164 different layouts.

Static reachability assumes ordinary closed doors can be opened. It does not prove actor-specific route budgets, movement success, congestion handling, appropriate schedules, restocking or adequate food/energy for the trip. The long-shopping case leaves the nearer store empty, but the courier may replenish it before a consumer chooses; record what actually occurs rather than declaring a forced long trip.

## Requested diagnostic runs — NPC owner

1. **Restocking:** load restocking save. Observe clerks2 and3 physically enter their own stockrooms, withdraw finite food, cross internal doors and stock more than one sales shelf. Record box, carried and shelf deltas plus action ticks. A single shelf filled is not evidence that the remaining shelves are serviced.
2. **Shopping:** load shopping save. Observe consumers4 and5 discover actual shelf availability, enter, buy and return food to their assigned homes when their existing household-supply rules call for it. Initially stocked pantries may legitimately postpone a trip; do not count postponement as a failure.
3. **Delivery:** load delivery save. Observe existing clerk calls, order identity/ownership and courier1's physical collection→delivery→return. Delivery must credit the correct stockroom supply box, without food duplication or an unearned completed order.
4. **Longer shopping:** load long-shopping save. Observe consumer4's choice with empty Home A and empty Store A while Store B has food. Record interruptions, actual chosen store and any intervening delivery. Do not patch World geometry or force a destination merely to manufacture a passing behavioral result.
5. **Work/home:** follow actual shifts across19:00 and the next07:00. Check home return, needs/rest, possible finishing cargo and next-day commute. Continue at least one game day (14400 ticks) for the daily-routine portion; report the actual horizon and unmet preconditions rather than an arbitrary pass.
6. **Save/resume:** capture during stock handling, shopping and courier travel. Compare an uninterrupted continuation with restored continuation for the same tick count, including actor/task/door state, inventories, assignments, claims and order history.
7. **Survival/accounting:** log food consumed/transferred, total remaining supply and deaths with actor IDs/ticks. Account for food eaten and discarded/dropped under existing rules; equal initial food need not remain equal after simulation. If a death occurs, preserve a pre-death save and available supply by location, not just the aggregate count.

Use your existing diagnostics and behavioral tests; World has not written replacements. Return any geometry failure as an exact origin/destination coordinate pair and stateful save. World will reproduce and repair physical access; decisions, needs, work/cargo scheduling and behavioral regression tests remain yours.

## Original reported baseline

Four NPC deaths with food remaining were reported. That original save, revision and seed are not available here. These fixtures do not reproduce that observation by assertion and do not establish a cause. Please run the original save separately when available; compare physical access, usable supply distribution and work/home routine state without assuming which is responsible.

## Result record

All behavioral outcomes are **PENDING — NPC developer**. Fill the sibling DIAGNOSTIC_RESULTS.md with actual source revision, save hash, case, tick horizon, actor ID/index, action stage, relevant coordinates, food/energy/health values, outcome and evidence. Include pre-failure/save-resume files. Do not mark the tests above PASS based only on World geometry checks.

## Reproduction / geometry evidence

From the supplied source root (Python requires existing Shapely/pyproj; C# requires .NET8):

```sh
python OutbreakRebuild/Tools/generate_store_validation.py --output /tmp/store-validation --seed 214
python -m unittest discover -s OutbreakRebuild/Tools -p test_store_validation.py -v
dotnet build OutbreakRebuild/Tests/StoreRoutineFixture.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/store-routine-fixture/obj/ -p:OutputPath=/tmp/store-routine-fixture/bin/
dotnet /tmp/store-routine-fixture/bin/StoreRoutineFixture.dll /tmp/store-validation/store-validation-214.obmap2 /tmp/store-validation/cases
```

Actual Python `/workspace/.outbreak-tools/atlas-venv/bin/python`; SDK `/workspace/.outbreak-tools/dotnet/dotnet`. Hosted .NET environment uses DOTNET_CLI_HOME, XDG_RUNTIME_DIR, XDG_DATA_HOME, NUGET_PACKAGES under existing `/workspace/.outbreak-tools` directories as documented in earlier World handoffs. Repeat generation/export in a fresh process and compare12 map/geometry/SVG/save/manifest files byte-for-byte.

Geometry acceptance:2 Python checks PASS;Final C# fixture assertion count is in VALIDATION.md;4 cases with5 NPCs,203 meals and41 measured routes each; exact map/save roundtrips; fresh-process12-file byte-identical replay. No behavior ticks or manual Unity tests. Final unified runner result is in VALIDATION.md/evidence; preserve the previously identified NPC137 LegacyUI outline failure distinction. Native Unity remains paused at6000.6.3f1, no player build claimed.

Only fixture authoring, physical validation and handoff files were added. No production Core/Contracts/Persistence, NPC behavior or behavioral tests, scenes, settings, dependency versions, GUIDs or shared test manifests changed. Keep local. No independent merge, push or publication.
