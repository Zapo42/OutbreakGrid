# World generators and two-room house integration

Historical World/131 record. The [current local consolidation](CONSOLIDATION_STATUS_2026-10-30.md) additionally includes NPC R1/F143 and store/DC interiors; statements below describe this earlier increment and its original validation.

The World transfer at `28dcbab` (implementation `20380bec26bf680865a2b22662872db115a390c4`) is integrated selectively against Foundation 131. This does not import or certify NPC Foundations 132–137, change simulation rules100, or resume native Unity testing. The maps branch has older game ancestry and must not be merged wholesale over current NPC/Lead work.

## What is available

- Reference-calibrated, seeded district/city generators and their attribution/profile snapshots under `CityReferences/`.
- Opt-in `--house-layout compact|spacious` generation: a larger living room and separate storage room connected by a real internal door, one home identity, an empty existing-capacity storage box and living-area recovery markers.
- Four loadable schema14 house maps in `CityReferences/districts/houses/`, with recipes and technical SVGs.
- Ten prototype building layouts in `CityReferences/building-layouts/`. Store, DC, restaurant/bar and church fixtures/interactions remain authoring hints, not live mechanics. Technical SVGs are not native Unity rendering evidence.

Default/legacy recipes retain existing behavior. Recovery uses existing mechanics and rates; area markers do not draw bed furniture. No food, jobs or population is fabricated by house generation. Existing saves are not automatically repartitioned into multi-room homes.

## Reader compatibility

Ship schema14 maps with this matching reader. Older clients reject schema14; legacy maps retain their previous formats and behavior. Group records and area markers survive exact map/save roundtrips. Both new C# asset `.meta` GUIDs are preserved. Unity remains **6000.6.3f1** and dependencies/settings are unchanged.

Arbitrary terrain-editor changes after assigning room-group metadata can invalidate anchors/topology. Such editing is outside this increment; supported runtime door-state changes retain membership. Do not treat the new metadata as a general-purpose terrain-edit migration system.

## Reproduce generation and checks

From the repository root, prepare the pinned Python tooling once:

```sh
python3 -m venv /workspace/toolchains/atlas-venv
/workspace/toolchains/atlas-venv/bin/python -m pip install -r OutbreakRebuild/Tools/requirements-atlas.txt
```

Run the Python suite and generate a house district outside tracked references:

```sh
/workspace/toolchains/atlas-venv/bin/python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py' -v
/workspace/toolchains/atlas-venv/bin/python OutbreakRebuild/Tools/generate_district_city.py --output /tmp/world-house-review district --type suburban-grid --seed 214 --blocks-across 2 --blocks-down 2 --house-layout compact
```

Follow `AUTOMATED_CHECKS.md` for the full sequential hosted runner. The separate infrastructure project is **not included** in that runner. After the runner finishes, run it explicitly against the four bundled house maps (substitute your .NET8 executable if needed):

```sh
DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/BuildingRoomChecks.csproj --no-launch-profile -- OutbreakRebuild/CityReferences/districts/houses
DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/BuildingRoomUiChecks.csproj --no-launch-profile
```

Do not run .NET builds concurrently in one checkout; projects share intermediate state. The second focused project reuses existing UI API doubles to verify production drawing of recovery areas versus ordinary beds without modifying the shared UiHarness source. Neither focused project is included in the unified runner. Infrastructure assertions establish spatial grouping, fixture storage and exact serialization, not NPC decision-making, internal-door pathfinding, daily survival, native Unity rendering/input or Windows player execution. Current-run results are recorded in `BUILD_VALIDATION.md` and `harness/build-log.md`.

## Separate NPC handoff

The cartographer's [daily-routine handoff](https://github.com/Zapo42/OutbreakGrid/blob/28dcbab/handoffs/world/lead-transfer-20380be/START_HERE.md) and ZIP remain on `world/lead-transfer-20261028`. Inside the ZIP, `daily-routine/HANDOFF.md`, `daily-routine-214.obsave` and `assignments-and-routes.json` are intended for the existing NPC coder. Its exact source snapshot is a standalone reproduction fixture, not a replacement for production Core/Contracts/Persistence.

That save uses `encounter-prototype-106` (NPC137), beyond the Foundation131 rules100 in this integration. Do not load it as a supported Foundation131 save or downgrade it. Its static routes and save roundtrips do not establish NPC survival or diagnose the reported four deaths. NPC137's reported Legacy UI occupation-outline failure remains outside this transfer and must be resolved before certifying an NPC137 integrated release.
