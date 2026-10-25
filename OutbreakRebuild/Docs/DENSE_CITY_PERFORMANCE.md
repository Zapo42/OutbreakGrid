Current update: [Foundation 123 expands building limits and adds district routes](EXPANDED_DISTRICT_ROUTES.md). The text below records Foundation 122 limits and measurements.

# Varied dense cities and faster routing — Foundation 122

Open **Inspect Compact City Paused** for a 20-resident city with 23 buildings and 108 meals. Or choose **Set Up Generated City** and cycle **Residential density** through one, two, three and four homes per residential block. The Large preset adapts to the room limit: 6×6 for one home, 6×5 for two, and 6×3 for three or four. Four homes across 6×3 blocks gives 60 residents, 63 buildings and 228 meals.

New Staggered maps use seeded block spacings from 27 to 41 cells, with side-road offsets retained. Geometry, setup measurements and building lots share the same axes. Three/four-home layouts split residential blocks into separate compact corner lots with outward entrances, individual inventories, beds and assigned residents. Home sizes fit the available space; no overlapping buildings or entrance paths through another home are permitted. Grid and Neighborhood stay available. Generate a new city to see geometry changes; saved maps and Restart preserve existing terrain.

## Performance changes

Profiling the previous build put roughly 80–85% of core tick time in long-route planning on the tested city scenes. This build:

- Reuses a binary heap and visited-cost dictionary per world instead of allocating a sorted-tree frontier for every A* query. Priority ordering and tie-breaks remain identical.
- Indexes room identity and floor-to-house membership once from discovered geometry. Path searches no longer scan every building for each candidate cell.
- Uses sparse furniture-cell indexes for route collision checks, keeping inventory catalogs and canonical serialization separate.
- Rejects empty zombie cells through occupancy counts before examining actor health/stagger state. Existing corpse and stagger behavior stays intact.

These indexes rebuild from saved geometry and stay bounded by actual room/furniture counts. Route scratch is reused, not serialized or retained per NPC. Routes still inspect current doors, furniture, vehicles and bodies; no stale path cache hides obstructions. Exact previous-build continuations and frozen simulation hashes are checked. Accelerated playback still executes ordinary simulation ticks.

## Reproducing core measurements

With a .NET 8 SDK, from OutbreakRebuild:

```sh
dotnet run -c Release --project Tests/CityPerformance.csproj
```

The benchmark reports the median of three runs of 500 ticks, with JIT warmup separate, managed tick allocations, and exact continuation checks. Its default scenes include the new 60-resident compact preset. World creation, serialization and Unity rendering are outside the timed interval.

For a matched before/after comparison, copy the two CityPerformance files into a separate checkout of Foundation 121 (`0fe71c4`), export its starting/final saves, then benchmark those same files with each revision:

```sh
dotnet run -c Release --project Tests/CityPerformance.csproj -- --export /tmp/outbreak-city-baseline
dotnet run -c Release --project Tests/CityPerformance.csproj -- /tmp/outbreak-city-baseline
```

Use the same machine and avoid concurrent workloads. Each result must match the baseline's full final save, including RNG state. The matched 54-resident dense city improved from **786.1 ms to 271.1 ms** per 500 ticks (**2.90×** throughput), with managed allocations dropping from **34.25 to 16.24 MiB**. Other matched scenes improved by 1.78× and 2.46×. See [the recorded results](CITY_PERFORMANCE_RESULTS.txt) for all measurements. These are core timings, not Unity FPS.

## Current limits

The current generator still allows 2–6 blocks per axis, at most 64 recognized buildings, 8,192 discovered floor cells and 1,000 actors per world. Long-route queries remain bounded to radius 256 and 16,384 visited cells. The new preset stays within those limits. These changes improve the current workload and remove per-building scans from path expansion; they do not establish performance for thousands of fully simulated residents or unlimited map dimensions. Larger cities will need further measured work on district routing, perception and rendering before raising those limits.

Automated core/UI checks and these headless benchmarks do not measure Unity frame rate, native rendering or actual Windows performance. Unity 6000.6.3f1 testing remains unverified under the standing instruction, and the Foundation 82 rollback checkpoint is unchanged.
