# Procedural urban, residential, suburban and business districts

Open **index.html** after extracting the pack. It contains nine district types at two seeds each, and two connected mixed-city examples: **20 generated maps** in total. Use **Load Map** for the `.obmap2` files in `maps/`. Complete recipes and parcel metadata accompany every map.

## New real-world survey references

Five bounded OpenStreetMap surveys were retrieved through the official Overpass API. Their geometry, bounding boxes, projections, raw-response SHA256, processing time, OSM base timestamp and attribution are saved in `../atlas/`. These are sample windows, not official administrative district boundaries or complete cities.

| Survey window | Mapped building footprints | Mapped footprint coverage |
| --- | ---: | ---: |
| Midtown Manhattan | 268 | 60.9% |
| Chicago Loop | 164 | 49.2% |
| Philadelphia residential rowhouses | 2,120 | 50.1% |
| Irvine / Woodbridge | 489 | 21.4% |
| Naperville residential suburb | 279 | 18.8% |

These coverage measurements describe what is mapped inside each crop. Incomplete tags, small crop sizes, clipped geometries and unsupported multipolygon relations limit comparisons. They are not claims about the density or zoning of an entire city. Original geographic samples remain north-up; generated maps use cardinal game streets.

The Vinohrady query returned HTTP 502 even after reducing the survey window. It was not treated as a successful survey. The urban-apartment recipe uses the existing published Prague/Bubeneč sample instead. The business-campus recipe is explicitly designed without a matching geographic survey.

## Reusable district types

| Type | Street and lot character | Procedural building hints |
| --- | --- | --- |
| `dense-urban` | Connected compact blocks, tight clearances, high coverage target | Apartments, retail, offices; 4–16 design storeys |
| `urban-apartments` | Urban perimeter blocks and medium footprints | Apartments and local retail; 3–7 design storeys |
| `terrace-residential` | Small close-set plots on a connected grid | Terrace homes and local retail; 2–3 design storeys |
| `garden-suburb` | Local loops, larger setbacks and open space | Mostly detached homes; 1–2 design storeys |
| `culdesac-suburb` | Collector backbone, branches and low coverage | Mostly detached homes; 1–2 design storeys |
| `suburban-grid` | Connected residential streets and moderate gaps | Detached homes and local retail; 1–3 design storeys |
| `business-core` | Connected streets, wider roads and larger footprints | Offices, retail, hotels; 6–20 design storeys |
| `business-campus` | Large blocks, generous spacing and open space | Larger office buildings; 2–5 design storeys |
| `retail-corridor` | Elongated blocks along major streets | Retail and offices; 1–4 design storeys |

The source measurements calibrate block form and provide observed footprint coverage. Recipe coverage targets, use weights, setbacks and storey ranges are **explicit design choices**. Minimum block dimensions are adapted so game-scale buildings, paths and doors fit. The catalogue is `../district_catalogue.json`; each exported recipe expands all settings, so replay does not silently pick up later catalogue changes.

Buildings favour street frontages, retain physical entrances and paths, and distribute the global building budget across eligible blocks. Coverage is a placement target; actual coverage is reported separately. Close-set plots retain separate interiors. Larger footprints obey the game's 256-floor-cell per-room limit.

Building metadata includes stable parcel identity, district identity, `use_hint`, an optional mapping to the game's existing Home/Store categories, and `storeys_hint`. **These are map-generation hints.** No inhabitants, jobs, inventory, office simulation, usable upper floors or directed vehicle lanes are created. Apartment/hotel capacity must be interpreted by a later scenario-generation step, not fabricated by the map reader.

## Seeded district and city generation

From the extracted pack's root or `OutbreakRebuild`, using Python from the atlas environment:

```bash
python Tools/generate_district_city.py district --type dense-urban --seed 12345
python Tools/generate_district_city.py district --type garden-suburb --seed 12345 --coverage 0.16 --blocks-across 4 --blocks-down 3
python Tools/generate_district_city.py city --seed 12345 --city-columns 2 \
  --districts dense-urban business-core terrace-residential garden-suburb
python Tools/generate_district_city.py replay path/to/exported.recipe.json
python Tools/build_district_examples.py
```

On this cloud machine use `/workspace/.outbreak-tools/atlas-venv/bin/python` for `python`. To install the tooling elsewhere:

```bash
python3 -m venv .atlas-venv
.atlas-venv/bin/pip install -r Tools/requirements-atlas.txt
```

The output path can be set before the subcommand: `python Tools/generate_district_city.py --output /tmp/city-example city --seed 12345`.

A mixed-city recipe expands each district's complete settings, assigns independent deterministic child seeds, places districts in non-overlapping slots, and adds physical cardinal road connections between neighbouring districts. Exported city metadata includes district bounds, road portals, unique parcel IDs and combined use counts. Maps have a shared metres-per-cell scale. Up to nine districts can be composed, with a maximum of 256 buildings and 32,768 interior cells across the entire city. A map that exceeds room, floor, grid or connection limits fails explicitly instead of silently dropping content.

Seed plus complete recipe, reference snapshot and generator version reproduce identical map bytes. Changing the city seed changes street spacing, branches, parcels and child seeds. Reference-profile hashes prevent a changed calibration snapshot from silently altering a replay. Old `cardinal-city-2` recipes remain compatible; the previous compact-grid export was replayed byte-for-byte after this extension.

Named street, lot and building-use streams are derived from the root seed with SHA256 before entering the versioned xorshift generator. This avoids correlated initial choices for nearby seeds while keeping each stream independent. Inter-district connectors use a narrow join that fits the minimum frontage clearance, and their segments are included in the exported combined street graph as well as the portal list.

## Validation and limits

All 24 Python atlas/generation/district tests pass, including all nine district types, exact replay, density differences, office versus residential footprint differences, connected road portals, parcel uniqueness and rejection of incompatible scales or oversized rooms. All 20 examples are replayed during the example build. Production C# import checks separately validate maps, room discovery, serialization round-trips, actor spawning and ticks. Native Unity rendering remains unverified and paused under the existing repository instructions.

The standalone pack supports Python map generation. Running the C# import-check project requires the existing OutbreakGrid checkout's production C# source; it is not a bundled game executable.

The scope remains maps and map generation; confirm before acting on unrelated requests. See `../NOTICE.md` for source attribution and data terms.
