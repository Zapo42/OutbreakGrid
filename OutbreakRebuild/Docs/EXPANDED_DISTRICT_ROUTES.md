# Expanded cities and district routes — Foundation 123

Open **Inspect Expanded City Paused** for an 8×8 staggered city with neighborhood-center zoning, four homes per residential block, **244 residents, 247 buildings and 780 starting meals**. Or choose **Expanded city (8 x 8 blocks)** in city setup; that button retains the selected residential density and zoning. The older Small/Standard/Large presets remain available.

City setup now accepts 2–10 blocks per axis, up to **256 recognized buildings** and **32,768 discovered floor cells**. Density is still one to four homes per residential block. The 8×8 four-home preset fits within these limits. A 10×10 two-home city has 194 residents and 197 buildings; 10×10 at four homes is rejected rather than silently dropping houses. Each recognized room still has at most 256 floor cells/four doors, and world capacity remains 1,000 actors.

## District routing

For assigned residents and couriers making a safe trip beyond 32 cells, the planner builds a shared network from actual connected sidewalks and crosswalks. It compresses degree-two corridors into edges between junctions and endpoints, plans between those waypoints, then validates nearby movement with the existing A* search. The first step, doors, crowds, zombies, vehicles, building restrictions and furniture still use live local collision rules. This planner does not reveal stock, discover hidden shops or grant distant zombie sight.

District query radius is **512 cells**, with at most **65,536 walkway cells**, **8,192 graph waypoints**, a **32-cell local query radius** and a 12-cell look-ahead waypoint. Disconnected components stay disconnected. Unsupported/missing walkway networks fall back to the existing bounded planner; arbitrary outdoor maps are not granted unlimited searches. This prioritizes connected pedestrian streets and may choose a different journey from the previous flat search.

The shared network is derived from geometry, not serialized as NPC memory or cached per actor. It rebuilds after loading and when walkway terrain or placed furniture changes. Stock updates and ordinary door opening do not force network reconstruction. Moving bodies are checked locally. Route choices, ordering and reconstruction are deterministic and consume no RNG. Source geometry is sparse; the planner does not allocate a dense array for the full world dimensions.

Empty opposing-species scans now use occupancy totals, distant support candidates are filtered before copying full actor records, and storage visibility avoids temporary shelf records. These preserve the old rules' simulation outcomes; frozen pre-optimization hashes remain checked. Terrain refreshes now fill a reusable managed pixel buffer and upload it in one batch, replacing one Unity native call per visible cell. Unchanged viewports still reuse their texture. This reduces API traffic; actual Unity frame-rate gains remain unmeasured.

Manual vehicles in upgraded runs also accept road destinations within radius 512, with a 32,768-road-cell search budget. Their entity cap remains 16; no NPC boarding, cargo or driving integration was added.

## Saves and compatibility

New runs use **encounter-prototype-94**. Existing rules-93 runs retain their prior 64-room/8,192-floor discovery caps and routing until **Enable expanded district routes** is clicked. Preserve old save files if you want to return to old behavior. Map terrain/lane schema remains 12; loading or restarting does not regenerate a city. New geometry requires starting a new generated city.

## Logistics and performance limits

The generator still supplies one distribution center, two stores in larger cities and the existing worker staffing. Walking couriers retain their food carry limit, hunger, energy and shifts. A very long walk can consume their provisions before delivery; the expanded preview is not a balanced large-city logistics scenario. Tests separately verify physical credited deliveries between closer districts. Additional local distribution hubs or vehicle cargo are the next logistics steps.

Benchmarks are core-only .NET Release measurements, with JIT warmup and the median of three 500-tick runs. They exclude setup, serialization and native Unity rendering. Use `dotnet run -c Release --project Tests/CityPerformance.csproj` to reproduce the expanded workloads. See EXPANDED_CITY_PERFORMANCE.txt for measured results. A large outbreak or thousands of residents still needs further profiling; raising the current caps again requires evidence.

Checks cover recognized housing/stock, actual travel beyond the former radius, graph compression, live furniture changes, disconnected networks, exact save/load continuation, legacy opt-in, distant credited deliveries and UI presets/restart. Unity 6000.6.3f1 play testing and frame-rate validation remain unverified under the standing instruction. The Foundation 82 rollback checkpoint is unchanged.
