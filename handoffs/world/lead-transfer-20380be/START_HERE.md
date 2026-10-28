# OutbreakGrid — World developer transfer to Lead

Local transfer prepared October 28, 2026. User authorizes the work; Lead owns final assembly and GitHub publication. Nothing in this package has been independently merged or pushed. Preserve current unfinished work and NPC ownership.

## Contents and status

| Folder | Deliverable | Status |
|---|---|---|
| `live-houses/` | Actual two-room generated houses, living recovery areas, separate household storage, schema14, patches, Git bundle, four maps, tests/evidence | Implemented and hosted-validated; see limitations below |
| `building-layouts/` | Compact/spacious, four-rotation house/store/DC/restaurant-bar/church geometry, ten previews, fixtures and interaction-position hints | Prototype geometry; **not all live mechanics** |
| `generator-snapshot/` | Exact tracked Tools and CityReferences from World HEAD, including prerequisite map-generation modules, recipes and attribution | Reproducible review snapshot; do not overwrite unrelated shared files |
| `daily-routine/` | Seed214 map, assigned-NPC save, plan, NPC assignments/shifts/routes, exact source snapshot and local authoring tool | Static geometry and serialization verified; NPC behavior investigation pending |

Read each folder's `HANDOFF.md` for exact changed paths, commands and results. The daily-routine folder is the handoff to the existing NPC developer; no direct delivery to their separate chat occurred.

## Source revisions

- Published maps baseline: `8e92e361206507219a725d698fca55dd26de5ff9`.
- Residential/park increment: `2843a8292ec9eea67e88a9cd7c0e0cffbf2f9bbc`.
- Building-layout prototype: `ba42070d23bb8c5e9482e22ea8025e4b0cd69574`.
- Live-house integration: `2363404b2d32f38dc669e9bc419159d6c40d944b`.
- Final World HEAD: `20380bec26bf680865a2b22662872db115a390c4`, branch `world/res-01`, clean at packaging.
- NPC validation/fixture source: `0aa8ec34aa168e4797bfcd672053755dcdfaa034` (`npc/f137-familiar-greetings`), plus narrow World changes.
- Separate Foundation131 validation: `f1b170bab2d6565097b292d0d83883f3c352afd6` (assignment documents at `8de4e935dbf48bff36225741f12ff6d202842d40`).

## Assembly order

1. Work on the Lead's current integration branch after preserving its uncommitted changes. Do not merge the maps branch's older game ancestry over newer NPC code.
2. For live house map loading, apply **only** `live-houses/SHARED-FILES.patch` to the current shared C# files, reviewing conflicts with current NPC/UI changes. Copy new asset files and their `.meta` files from `live-houses/source/` and include the focused test project/source. This patch covers five existing C# files; it was verified against Foundation131 and NPC137, not every later revision.
3. For map generation, use `generator-snapshot/OutbreakRebuild/Tools` and `CityReferences` as the exact final pipeline/reference snapshot, or retain maps prerequisites through ba42070 and apply `live-houses/HOUSE-PLACEMENT.patch`. The full house patch includes the same shared C# changes: **do not apply them twice**. The prototype building patch is historical; the final generator snapshot already includes it.
4. `live-houses/world-houses.bundle` provides review history with prerequisite8e92e36. It is not a standalone full repository clone. Run `git bundle verify` from a checkout containing that baseline before importing review refs; use selective integration, not a wholesale ancestry merge.
5. Re-run the documented unified runner sequentially in the actual integrated checkout, plus Python generation checks and the focused BuildingRoomChecks project. Preserve Unity6000.6.3f1, dependencies, GUIDs and `.meta` files. Native Unity testing remains paused.
6. Give `daily-routine/HANDOFF.md`, the save and assignments manifest to the existing NPC developer. Use save loading for roles, home assignments, shop identities and shifts; map-only import does not retain them. `daily-routine/source` contains an exact separate fixture source snapshot for reproducibility, **not instructions to overwrite the integrated game's Core**.

## Validation summary and release limits

- Python:40 tests PASS;20 legacy recipes reproduce canonically; fresh generated-house replay byte-identical.
- Main/Foundation131 unified runner:PASS,627 core cases,737155 assertions, strict Windows ABI compilation and both hosted UI modes.
- NPC137:692 core cases/742922 assertions PASS, ABI PASS, NewOnly UI3 assertions PASS. Full runner remains PARTIAL: LegacyUI fails `Off-duty clerk or finishing courier lost duty outline distinction.` The same assertion fails on unmodified NPC137. Existing UI/NPC integration owner must address it; do not mark the overall NPC137 runner green.
- Focused house geometry/persistence checks:1264 assertions PASS.
- Routine fixture:11 NPCs,21 connected cardinal access/routes, exact map/save roundtrips and separate-process deterministic replay PASS. No decision ticks or daily survival outcome was asserted.
- House schema14 requires the new reader before import. Older maps retain existing behavior; existing saves are not automatically repartitioned.
- Live house recovery/storage is implemented. Store shelves/stockrooms, DC order computer, restaurant kitchen/dining/counter sites and church in the prototype pack are geometry/authoring hints unless explicitly stated otherwise in the detailed handoff. No new order-computer, cooking, waiter or church mechanics are claimed.
- Original four-death baseline save/revision/seed was unavailable. No cause or behavioral fix is established. Static routes account for doors and fixtures, but do not prove runtime decisions, congestion, supply scheduling or starvation prevention.
- Native Unity/Windows player testing is paused; no executable release is supplied.

## Daily-routine essentials

Load `daily-routine/daily-routine-214.obsave`; seed214, tick0/game07:00, paused. Three shared homes, three staffed stores, two couriers, six consumers and one park. Workers07:00–19:00. DC80 food + store shelves18 + carried11 =109 initial food; home/store stock boxes start empty, retaining the existing NPC fixture. Short and longer shopping/delivery paths and every NPC's stable ID/home/job are in `assignments-and-routes.json`. NPC developer owns behavioral diagnostics; World owns demonstrated geometry faults.

## Package integrity

`SHA256SUMS.txt` covers every payload file. `TRANSFER-MANIFEST.json` records sizes/hashes. The sibling ZIP has a separate SHA256 file. Keep all supplied maps and attribution notices with their respective source files. No settings, dependencies, scenes or test manifests were changed to create this transfer.
