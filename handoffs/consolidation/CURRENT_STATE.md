# Verified state at consolidation

## Sources and publication

Repository: Zapo42/OutbreakGrid, https://github.com/Zapo42/OutbreakGrid.

| Source | Revision | Status verified here |
|---|---|---|
|Lead main|1b05a73c0994720230524c519dac84fc58d0bbd3|Published; earlier World houses integrated by Lead|
|Original maps baseline|8e92e361206507219a725d698fca55dd26de5ff9|Ancestor in bundle|
|Residential/parks|2843a8292ec9eea67e88a9cd7c0e0cffbf2f9bbc|World history|
|Building prototypes|ba42070d23bb8c5e9482e22ea8025e4b0cd69574|World history|
|Live houses|2363404b2d32f38dc669e9bc419159d6c40d944b|World history; Lead assembled separately|
|Area-rest/save compatibility|20380bec26bf680865a2b22662872db115a390c4|World history; prior handoff|
|Published house transfer|28dcbab581748b89524f5ca8fabda9816b5f4213|world/lead-transfer-20261028|
|Live stores|19e18e74e54db7bb13a888a8ae9090c832af9bbf|Published as ancestor of fixture handoff|
|Store diagnostic fixtures|efa363489e0a2694bae645908169cf69d8f404b7|Published as ancestor of fixture handoff|
|Published store fixture handoff|b78fccc64ee9c81ea2f4b92f71054d9db674cf28|world/store-validation-handoff-20261028|
|Latest DC/World implementation|d499d033d437b72a6de96be21e60acd932a301d2|Local world/distribution-center-interiors; included in full bundle|
|NPC137 baseline|0aa8ec34aa168e4797bfcd672053755dcdfaa034|Published npc/f137-familiar-greetings|
|Original dirty checkout|c563076f608bd4e9fcbaeac18b0d1508c5f30232|Local work branch; separate unfinished snapshot|

Published fixture URL:
https://github.com/Zapo42/OutbreakGrid/tree/world/store-validation-handoff-20261028/handoffs/world/store-validation

Main and NPC refs were checked read-only against origin during packaging. No
merge, main modification or new push occurred as part of this transfer.

## Implemented physical world

- Seeded cardinal street/district composition and reference-calibrated layouts,
  deterministic recipes, lots/setbacks/gaps and sidewalk-connected entrances.
- Six park templates: small/medium/large × cross/loop. Grass/sidewalk geometry;
  generated park metadata alone is not the NPC park-knowledge runtime contract.
- Explicit schema14 building-room groups: real rooms connected through internal
  doors share one home/store/DC identity. Internal doors are excluded from outdoor
  exits. Legacy ungrouped maps retain prior behavior.
- Houses: compact/spacious, four rotations; living recovery area and separate pantry
  room, real empty household storage. Area rest markers use existing recovery;
  they are not beds. Native renderer skips bed furniture for area markers.
- Stores: compact6/spacious8 real solid sales shelves, separate stockroom, one
  authoritative30-capacity supply box and a real phone; connected public/service
  doors. Existing store/clerk APIs accept grouped rooms. Generated maps are empty
  and unstaffed until configured; configured state belongs in saves.
- DCs: compact/spacious dispatch and warehouse, one authoritative100-capacity box,
  reserve6, connected public/service/internal doors. Reserved computer footprint
  is Wall terrain8, with a valid walkable dispatch approach. Metadata and runtime
  center/supply IDs are supplied; no computer interaction or sprite is implemented.
- Mixed-city transforms preserve all fixture coordinates and namespaced identities.
  City DC option selects the first business district for one center; subsequent
  eligible districts can use live stores.

## Prototype or unfinished features

- Computer visit/receipt/order-claim behavior: NOT IMPLEMENTED. Existing order
  behavior is unchanged. The blocked computer coordinate has no floor membership;
  use its approach to resolve the center identity. Future object/persistence and
  graphics integration must replace the wall placeholder deliberately.
- Restaurant/bar: two-room dining/kitchen, two external entrances, staff door,
  counter handoff positions and fixture hints exist as prototypes. No live cooking,
  waiter mechanics, opening hours or materialized restaurant object system.
- Church: sanctuary/vestry and fixture hints exist as prototypes; no live mechanics.
- Auxiliary warehouse racks and extra store rack cells are placement hints;
  current store/DC runtime contracts select one supplyBox each.
- Default C# CitySetup/UI city-generation menus were not rewritten. Live layouts
  use optional Python generator flags and map/save import.
- NPC behavior changes are in the NPC source branch, not implemented by this World
  chat. Their integration into the Lead tree is not assumed.

## NPC trial state

The earlier daily-routine fixture and newer four store cases are all supplied.
Newest cases use seed214,100×56 geometry,5 assigned NPCs,2 six-shelf stores,3 homes,
DC and park; tick0/07:00; workers07:00–19:00;203 finite initial meals/case.

- restocking:30 stock meals/store, empty sales shelves.
- shopping:5 meals/shelf, empty stock boxes.
- delivery: empty stores,180 DC meals.
- long-shopping: nearer shop/home pantries empty, farther shelves supplied,168 DC
  meals. Consumer4 explicitly knows both store locations, never hidden stocks;
  existing location memory expires normally after3600 ticks.

41 static routes/case; local shopping33 steps and alternative store101 steps.
No decision ticks were run by the fixture authoring checks. The NPC developer's
requested stock→shelf, shopping→home, physical delivery, shifts, mid-action resume
and survival results have NOT been received/verified here. Treat outcomes pending,
even though existing independent core regression suites passed.

Reported baseline: four NPC deaths with food remaining. Original save/revision/seed
not available. No reproduction or cause claimed. Preserve that distinction.

## Validation evidence

| Increment / actual source | Result |
|---|---|
|Houses Python|40 PASS;20 legacy recipes reproduce;1264 focused infrastructure assertions PASS|
|Stores Python|44 PASS;857 focused store assertions PASS over22 stores/4 maps|
|Store trial fixtures|2 Python PASS;453 fixture assertions PASS;4 cases/5 NPCs/203 meals/41 routes;12 files fresh-process identical|
|DC final Python|48 PASS;2 existing fixture-authoring tests PASS;39 old/new recipe-map pairs reproduce|
|DC fresh CLI replay|12 map/recipe/metadata/SVG files byte-identical|
|DC on Lead1b05a73|93 focused assertions PASS;3 centers/3 maps/1 separate-home courier;0 build warnings/errors|
|DC on NPC0aa8ec34+World schema14|93 focused assertions PASS; same coverage;0 build warnings/errors|
|Lead1b05a73 unified runner|PASS:627 core cases/737155 assertions;Windows ABI;LegacyUI2463;NewOnly3|
|Earlier NPC137 full runner+World schema14|PARTIAL:692 core cases/742922 assertions and ABI PASS;LegacyUI outline assertion fails;NewOnly3 separately PASS|

The NPC137 LegacyUI assertion `Off-duty clerk or finishing courier lost duty outline
distinction.` at UiHarness.cs:98 was independently reproduced on unmodifiedNPC137.
Do not present its full runner as green or attribute it to DC geometry. Full NPC137
suite was not repeated for the final DC Python/geometry change; focused compatibility
was checked on its actual source. Logs and source provenance remain in packages.

No native Unity, player builds or native desktop dialogs tested; testing remains
paused. No new dependencies/editor versions, scenes or asset GUIDs were changed.

## Save compatibility

Schema14 maps require the World reader. Lead-main inspection saves use
encounter-prototype-100; NPC137 inspection saves use encounter-prototype-106.
DC package separates them under inspections/lead-main and inspections/npc137.
Older Lead builds cannot load the NPC106 saves. Map-only files omit NPC assignments,
configured stores/DCs, shifts and familiarity; load .obsave for those states.
