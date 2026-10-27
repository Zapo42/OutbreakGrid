# Real-world layout reference atlas

Current district library: **[dense urban, residential, suburban and business generation](districts/README.md)**, with five new OSM survey windows, nine reusable district types and mixed-city composition. Open [districts/index.html](districts/index.html) for the latest examples.

This first increment turns published real-world map samples into a reusable, measurable reference library for OutbreakGrid. Open [atlas/index.html](atlas/index.html) to compare the maps and toggle a **5 metres per cell** grid. Every view is north-up with a 100 m scale. Source geometry, inferred street-enclosed blocks, measurement profiles and loadable `.obmap2` terrain templates accompany each sample.

## Cardinal layouts and reproducible city recipes

Open **[cardinal/index.html](cardinal/index.html)** for the current examples: three cardinal-aligned reference layouts, six newly generated street presets, and two additional seeds for comparison (11 maps total).

The adjusted references rotate the dominant perpendicular street directions toward game north/south and east/west. This preserves geometry, distances and junctions; geographic north is rotated, and secondary diagonal roads remain. The original geographic survey files remain in `atlas/`.

The six new presets generate **entirely new, strictly horizontal/vertical streets**: `compact-grid`, `long-block-grid`, `mixed-town`, `town-centre`, `suburban-loops`, and `suburban-dead-ends`. The first three adapt block dimensions from the corresponding reference measurements; compact-grid deliberately squares its blocks. Density, pruning and road hierarchy are explicit design settings. The suburban presets are designed morphology examples, not new geographic surveys.

Every new generated map has a complete `*.recipe.json` alongside it. Recipe version `cardinal-city-2` records all resolved settings, the seed, and the reference profile's SHA256 when applicable. A versioned xorshift32 generator uses independent street, open-space and lot streams. With the same recipe, reference profile, generator and pinned geometry-library versions, replay produces identical map bytes. Changing building density preserves the street stream. Changing the seed changes both street variation and lots. A street-variation value of zero deliberately disables spacing variation.

| Variable | Effect |
| --- | --- |
| `seed` | Repeatable layout and building choices |
| `columns`, `rows` | Nominal street-block counts |
| `block_width_cells`, `block_height_cells` | Nominal street spacing, in cells |
| `block_variation` | Seeded variation in spacing, 0–0.45 |
| `loop_chance` | Additional connections beyond the connected backbone, 0–1 |
| `dead_end_chance` | Short branches on omitted street edges, 0–1 |
| `road_width_cells`, `arterial_width_cells`, `arterial_every` | Local/major street widths and hierarchy |
| `sidewalk_width_cells` | Width of pedestrian corridors |
| `building_coverage`, `centre_density_multiplier` | Lot coverage target and centre concentration |
| `setback_cells`, `footprint_sizes` | Building clearance and possible dimensions |
| `park_fraction` | Chance a closed block is reserved as open space |
| `max_buildings`, `cell_metres` | Room limit and map scale |

Coverage is a target, not a guaranteed occupancy; space, clearances and usable entrance paths limit placement. Open-space blocks are grass reserves with metadata, not an implemented NPC park/leisure mechanic. The road backbone is connected, and every generated building has a physical door and path. Directed traffic lanes, furniture, supplies, jobs and population remain outside this map-generation increment.

From `OutbreakRebuild`, using the atlas virtual environment:

```bash
python Tools/generate_seeded_city.py --preset suburban-loops --seed 12345 \
  --columns 6 --rows 4 --building-coverage 0.18 --loop-chance 0.6
python Tools/generate_seeded_city.py \
  --recipe CityReferences/cardinal/layouts/compact-grid-214-890ace031b6b.recipe.json
python Tools/build_cardinal_examples.py
```

Use `/workspace/.outbreak-tools/atlas-venv/bin/python` for `python` in this environment. `--recipe` replays a complete configuration; optional explicit parameter flags create a revised recipe. Files include a recipe fingerprint to prevent different settings from silently sharing a filename. A changed reference profile is rejected when replaying an existing recipe. All 18 atlas/generation regression tests pass, including exact recipe replay, connectivity, cardinal segment direction, paths and seed variation.

## What is present

| Survey | Source | Included geography |
| --- | --- | --- |
| West Oakland, California | OSMnx's historical OpenStreetMap extract | Streets, 23 mapped building footprints and available land-use tags in a small crop |
| Manhattan, New York | Momepy's OSMnx street-graph sample, created in 2022 | Street geometry only; building coverage and zoning are unknown |
| Bubeneč, Prague | Momepy's published morphology sample | Streets and 144 building footprints; upstream geometry lineage is unspecified, so this is not represented as an OSM extract |

These are **small historical survey samples**, not complete cities, current mapping, or representative statistical samples of each whole city. Their different crop sizes limit direct density comparisons. The original sources, SHA256 hashes, processing times, attribution, projection and missing-data notes are saved in each profile. See [NOTICE.md](NOTICE.md) for source and data terms.

`selection.json` selects six additional approximately 1.2 km-square survey windows: Chicago's Lincoln Park, Boston's Back Bay/South End, South Philadelphia, Irvine's Woodbridge, residential Naperville and Great Linford in Milton Keynes. These cover compact/regular grids, intersecting grids and suburban street forms. Those original larger windows remain pending. Network access now works through supported GET requests: five additional smaller district windows have been surveyed and are documented in [the current district library](districts/README.md). No API key is required.

## What the maps teach generation

The pipeline measures street density, lengths between noded street junctions, fourfold orientation alignment, curved-street share, interior junction/dead-end counts, closed ground-level street blocks and their areas/aspect ratios. It retains mapped buildings, water and available land-use/boundary ways. It excludes crop-edge nodes when counting dead ends and keeps tagged bridges/tunnels separate when finding junctions.

The Manhattan sample strongly aligns to two perpendicular street directions; Bubeneč has substantially more varied orientations. That is evidence for multiple street-layout families rather than applying the same staggered grid everywhere. Closed-loop block area and aspect distributions can inform rectangular versus elongated lots; mapped footprint coverage can inform building spacing. OSM land-use tags can later guide residential/commercial/industrial district composition where coverage is adequate.

Profiles contain a transparent recommendation for the existing Grid, Staggered or Neighborhood street layout. These are **reference hints**, not changes to the procedural generator or an AI model trained on maps. Raw segment length is not block spacing. The next generator increment should consume measured block/lot distributions while preserving connected entrances, pedestrian paths, legal vehicle lanes, finite supplies and deterministic save/resume. Suburban parameters should wait for the selected suburban surveys rather than be fabricated from city-only samples.

### Seeded map generation

`Tools/generate_reference_city.py` provides a first concrete use of the references: it keeps the measured street backbone, finds its closed ground-level blocks, and creates **new fictional buildings** inside them. It samples footprint dimensions from the source where available, adapts them to 4–12 game cells per side, and uses measured footprint coverage as a bounded placement target. Manhattan has no footprint data, so its explicit fallback uses 5×7, 6×8 and 5×5-cell recipes and 12% target coverage; these are game design defaults, not inferred New York measurements.

Every accepted building fits inside a street-enclosed block, retains spacing from other buildings, has an interior and door, and gets a physically walkable path to the street. Placement has bounded attempts, a maximum of 256 buildings, a saved seed and a saved recipe. Existing road/sidewalk/water cells remain unchanged. The seed-214 examples in `generated/` contain 4 West Oakland-style, 41 Manhattan-style and 20 Bubeneč-style buildings. Load these `.obmap2` files to get generated interiors; the atlas's source templates intentionally have solid footprint obstacles instead.

```bash
/workspace/.outbreak-tools/atlas-venv/bin/python Tools/generate_reference_city.py --reference manhattan --seed 214
/workspace/.outbreak-tools/atlas-venv/bin/python Tools/city_atlas.py index
```

Different seeds change the buildings, not the street graph. This command provides **reference-backed lot generation**. The cardinal generator documented above additionally synthesizes new street networks. Generated maps remain unpopulated and have no assigned homes/jobs, inventories, furniture or vehicle lanes. The next city-generation stage can build those on validated terrain; it should not invent zoning facts the source does not supply.

No official administrative district polygons are claimed. Road-enclosed blocks are inferred geometry and exported separately as `*.blocks.geojson`. Available boundary ways are retained, but OSM multipolygon/boundary relations and their holes are not reconstructed in this first importer. Missing tags mean unknown. The atlas does not infer commercial or industrial uses from building appearance.

## Use the game templates

In OutbreakGrid use **Load Map** and choose an `atlas/*.obmap2` file. These are schema-1 unpopulated geometry templates: roads, symbolic sidewalks, solid building-footprint obstacles and mapped water. Place actors on walkable cells to inspect movement. They do not contain furnished buildings, entrances, assigned homes, supplies or jobs. No vehicle lanes are invented from OSM road centerlines. The metric scale is retained in `authoringJson`; this does not change movement speed or the scale of existing generated scenarios.

The production C# map reader validates all three maps, exact map serialization round-trips, road spawning and ten simulation ticks per map. This does not establish native Unity rendering; Unity testing remains paused after Foundation 82.

## Reproduce and extend

From `OutbreakRebuild`:

```bash
python3 -m venv /workspace/.outbreak-tools/atlas-venv
/workspace/.outbreak-tools/atlas-venv/bin/pip install -r Tools/requirements-atlas.txt
/workspace/.outbreak-tools/atlas-venv/bin/python Tools/build_city_pilots.py
/workspace/.outbreak-tools/atlas-venv/bin/python -m unittest discover -s Tools -p test_city_atlas.py -v
```

`build_city_pilots.py` retrieves checksum-pinned published samples through their upstream GitHub locations, caches them in the ignored `CityReferences/cache/` directory, and regenerates the versioned atlas. A source checksum mismatch fails for review; the tool does not silently accept a changed upstream sample.

After `overpass-api.de` access is applied, download selected areas **one at a time**:

```bash
/workspace/.outbreak-tools/atlas-venv/bin/python Tools/city_atlas.py download --sample chicago-lincoln-park
/workspace/.outbreak-tools/atlas-venv/bin/python Tools/city_atlas.py download --sample irvine-woodbridge
```

Use the other IDs in `selection.json` for the remaining surveys. The command makes one bounded Overpass request, caches the successful response, then rebuilds the atlas index. It preserves HTTPS verification and rejects timeout remarks instead of caching partial responses as successful maps. Repeated commands use the cached response. Remove that area's cached file explicitly if a fresh survey is needed. Do not run bulk requests against public Overpass servers.

Local OSM XML, `.osm.bz2`, Overpass JSON, OSMnx GraphML or a Momepy-style GeoPackage can be imported with `city_atlas.py import`; `--help` describes the source/attribution and bounding-box arguments. `--cell-metres` is a global option before `import` or `download`. Every survey uses a local azimuthal-equidistant metric projection; projection and origin are exported to preserve the exact grid transform. Grids larger than one million cells are rejected.

To check the exported maps with the actual C# reader, activate the .NET environment described in the saved startup instructions, then:

```bash
dotnet build Tests/CityReferenceImportChecks.csproj -c Release \
  -p:BaseIntermediateOutputPath=/tmp/outbreak-reference-checks/obj/ \
  -p:OutputPath=/tmp/outbreak-reference-checks/bin/
dotnet /tmp/outbreak-reference-checks/bin/CityReferenceImportChecks.dll CityReferences/atlas
```

The code is separate from the game's existing city-generation paths; existing scenarios and saves are unchanged. This data-tool increment does not claim a new Unity-tested Foundation build.

Validation: 11 Python regression tests passed. The production C# reader loaded and round-tripped all three source templates and all three generated examples, verified that generated interiors form separate shelter rooms, spawned an actor on each map and executed ten ticks. Native Unity remains unverified.

Current user scope: **maps and map generation**. Confirm with the user before acting on requests outside that scope, including NPC shopping changes.
