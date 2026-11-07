# City packs as runtime generation sources

**Set Up Generated City** now defaults to **City source: City packs**, **City pack: Mixed city**. Choose a theme, seed, block counts and residential density, then **Start generated city**. Mixed and the other nine themes now have individual named buttons, instead of a cycling selector. The corrected build identifies itself as **City packs 19 / rules 114**. It starts paused unless you change that option. Restart reproduces those choices. Main Menu and Resume preserve the current run. Choose **City source: Classic** for the previous generator and its street/zoning controls; old previews and saved cities are unchanged.

Mixed combines seeded district characters within one connected network. The nine individual themes are Dense urban core, Urban apartment district, Compact terrace residential, Garden suburb, Cul-de-sac suburb, Connected residential suburb, Central business district, Business campus and Retail and local business. Names come from the pack catalogue; the menu displays their original labels.

New cities start zoomed into a resident's neighborhood so stations are readable. Expand **Generated city stations** in the sidebar: **Find food station** selects and zooms to an actual inventory; **Find rest station** centers a real bed marker; **City overview** zooms out using the map-panel aspect ratio. Navigation does not award food/recovery or advance a paused world. Large maps retain the existing bounded viewport and panning.

Start a **fresh generated city** to receive runtime2 streets and visible stations. Older saved maps keep their original terrain/markers and do not silently regenerate. The independent runtime1 continuation remains exact. This fixes station visibility and discovery; it does not automatically furnish every house with the separate local household-mode kitchen/media objects.

## What supplies the seed sources

`CityReferences/district_catalogue.json`, the existing survey profile snapshots and `Tools/generate_reference_city.py` are authoritative authoring sources. The offline exporter calls the original validated `building_blueprint` function and expands the original district recipes. It bundles nine calibrated profiles and **24 actual building layouts**: compact/spacious house, store and distribution-center templates in all four rotations. The game creates new layouts from these sources; it does not merely choose one of the seed214/215 example maps.

`Assets/Outbreak/Core/CityPackData.cs` is generated C# data. Unity needs no Python installation, geographic library, network connection or external pack-file lookup. The manifest records exact source/output SHA256 values. Each generated map's `authoringJson` records runtime version, source hash, seed, theme, dimensions/density and every parcel's district/template identity and origin. Save/load retains actual map geometry and inventories; it does not regenerate a save using a later catalogue.

District calibration retains the existing attribution: **© OpenStreetMap contributors**, ODbL1.0, alongside the fictional template choices. Full survey provenance and terms remain in [CityReferences/NOTICE.md](../CityReferences/NOTICE.md). Apartment, office, hotel and storey descriptions remain authoring hints; this runtime uses the existing Home/Store/DistributionCenter gameplay roles, with one resident per assigned home. Restaurant/church draft layouts and new occupations are not activated by this integration.

## Native generation contract

This is **city-pack-runtime-2**, a native seeded adaptation, not byte-identical Python `district-city-3` recipe replay. The unchanged Python generator remains the exact-replay route for exported recipes. Runtime themes reuse calibrated block dimensions/variation, road and sidewalk widths, setbacks, lot gaps, loop/spur tendencies, park fraction and footprint-size preference for compact/spacious selection. Supported Home/Store/DC locations use the existing neighborhood-center zoning policy rather than fabricating the pack's unsupported land-use jobs.

A seeded spanning network connects every junction without mandatory full-width rows or an outer road ring. Recipe-influenced optional loops and dead-end spurs use separate deterministic streams. Junctions stagger by up to3cells; orthogonal street segments bend between their offsets. Extra reserved corridor clearance protects every template and its setbacks. Mixed reduces lattice completion; dense-urban themes can still intentionally retain more grid connections. Curbs include connected cul-de-sac caps; actual junction positions receive crosswalks. Short pedestrian links join public and service entrances between the distribution center and stores, avoiding vehicle-road detours without adding roads or changing movement costs. Open-space blocks keep enough homes for workers and at least one consumer where the requested density supports one. Their cross paths connect to the sidewalk graph. Parks are physical layout/provenance here, not automatic global NPC park knowledge. Roads use the pack's ordinary bidirectional terrain; directed lane/parking authoring is a separate infrastructure increment.

Building variants and rotations are seeded independently of movement RNG. Minimum block dimensions adapt to the largest actual template, selected density, setbacks and clearance. A minimum one-cell lot gap keeps all rotated exterior doors usable, including in source themes whose authoring gap is zero. Every public and service door receives a real, obstacle-avoiding sidewalk path; paths cannot carve through another building. Interior walls and doors preserve the exact source template, and existing room groups join living/storage, sales/stock and dispatch/warehouse rooms into their building identities.

Stores materialize their sales shelves and rotated request phone. Each building retains one authoritative inventory, matching the pack's existing supply materialization; additional warehouse rack positions remain authoring hints. The distribution computer remains the pack's solid authoring placeholder, without new order-computer behavior. Every building now has one **visible non-area recovery marker**, drawn as a bed. Houses retain their pantry; stores retain real food shelves and stock storage; distribution buildings retain their finite supply inventory. These use the existing food/recovery contracts and rates. The separate household furniture/snack mode is not automatically applied across these cities.

## Population, bounds and validation

Existing APIs assign one distribution courier, one clerk per store, separate homes and the existing7:00/12-hour work schedules. Food remains finite:40central meals,4per store and3per household including personal inventory. Seeded pair-balanced hunger/carried-food variation remains. This integration does not resolve the shelved supply/shift problems; more shelves and larger distances change demand and travel. In the controlled four-hour Mixed run, the initial new street layout credited0meals; public/service footpaths restore2, still below the prior layout3. SuburbanGrid and BusinessCore credit3each. No NPC rate changes or extra food hide that limit.

Two-room buildings consume two physical room slots. Setup rejects configurations exceeding256physical rooms/128buildings before generating terrain. Existing32768floor and4096recovery-marker bounds remain. The preview summary includes seed-selected open-space omissions and matches the actual population, buildings and food.

Recheck authoring sources with standard Python:

```
python3 OutbreakRebuild/Tools/export_runtime_city_packs.py --check
```

After intentionally changing the source packs, regenerate with the existing atlas tooling environment (requirements remain pinned in Tools/requirements-atlas.txt):

```
/workspace/toolchains/atlas-venv/bin/python OutbreakRebuild/Tools/export_runtime_city_packs.py
```

Run the documented unified runner sequentially. The portable observer under `harness/diagnostics/citypack18` covers four-hour live cities with exact food accounting and replay. See BUILD_VALIDATION.md for final counts and results. Core and UI API-double checks do not establish native Unity compilation, rendering, input, dialogs or player execution. Native testing remains paused; Unity stays6000.6.3f1 and no EXE is claimed.
