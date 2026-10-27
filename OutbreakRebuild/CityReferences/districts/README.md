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

If every block rolls as open space, the central block is deterministically retained for buildings so a valid sparse-district seed does not become an accidental park-only failure. Buildings favour street frontages, retain physical entrances and paths, and distribute the global building budget across eligible blocks. Coverage is a placement target; actual coverage is reported separately. Close-set plots retain separate interiors. Larger footprints obey the game's 256-floor-cell per-room limit.

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

All 25 Python atlas/generation/district tests pass, including all nine district types, exact replay, density differences, office versus residential footprint differences, connected road portals, parcel uniqueness and rejection of incompatible scales or oversized rooms. All 20 examples are replayed during the example build. Production C# import checks separately validate maps, room discovery, serialization round-trips, actor spawning and ticks. Native Unity rendering remains unverified and paused under the existing repository instructions.

The standalone pack supports Python map generation. Running the C# import-check project requires the existing OutbreakGrid checkout's production C# source; it is not a bundled game executable.

The scope remains maps and map generation; confirm before acting on unrelated requests. See `../NOTICE.md` for source attribution and data terms.

## WORLD-RES-01: aligned residential lots and park templates

The optional `residential` placement mode uses fixed lot modules sized from the
largest allowed footprint dimension plus the recipe's `lot_gap_cells`. It tries
edge lots first and uses the existing seeded footprint/orientation choices,
block inset (`setback_cells`), terrain reservations and entrance search. A lot
that cannot fit is omitted; a district with no fitting houses still raises the
existing generation error. Defaults and old recipes retain their previous modes.
This is physical map geometry, without NPC decisions or new building/save types.

From `OutbreakRebuild`, with the atlas Python dependencies installed:

```bash
python Tools/generate_district_city.py --output /tmp/residential-parks district \
  --type suburban-grid --seed 214 --residential-lots \
  --blocks-across 3 --blocks-down 2 --block-width-cells 40 --block-height-cells 40 \
  --park small:cross --park small:loop \
  --park medium:cross --park medium:loop \
  --park large:cross --park large:loop
```

Each park is an unobstructed grass lot with connected sidewalk paths. `cross`
provides two crossing paths; `loop` adds an internal perimeter circuit to those
paths. Small is 6×6 cells, medium 10×10 and large 16×16 (30×30, 50×50 and
80×80 metres at the default 5 metres/cell). These are explicit design sizes,
not measurements from reference surveys. There are no new art assets, trees,
benches, leisure behaviors or happiness effects in this increment.

Up to six park requests can be recorded in the optional district recipe field
`parks`, e.g. `[{"size":"small","variant":"loop"}]`. Python callers can use
`district_recipe(..., residential_lots=True, parks=[...])`. Mixed-city callers
can put the same `parks` list on each desired child district recipe before
`compose_city`; exported recipes replay both options without extra flags.
Parks are placed in request order in distinct fitting street-enclosed blocks,
with one-cell boundary clearance. They reserve their entire grass/path lot
before houses are placed and connect to the street sidewalk network. Large
parks need sufficiently large blocks. Placement is bounded to block centres;
a request that cannot fit safely is recorded in `omitted_parks` rather than
shrunk or forced over roads/houses. Inspect this list when requesting a count.

Optional authoring metadata includes `parks` (stable IDs, size, variant, cell
bounds, entrance, and path cells) and `omitted_parks`. Composed cities translate
park geometry and prefix IDs with the district index. Existing OBMAP2 schema
and terrain IDs are unchanged. Old recipes without `parks` keep their existing
metadata shape and exact replay; new mode/options are included in recipe IDs.
Park metadata is an authoring hint, not a persisted game leisure-destination
contract. Any gameplay use requires a separately coordinated Lead/NPC task.

Validation includes module gaps/setbacks across residential types, seeds 1,
214 and 4294967295, all six park templates, tight-block omission, mixed-city
translations, and connected interior→door→sidewalk access. Run:

```bash
python -m unittest discover -s Tools -p 'test_*city*.py' -v
```

Also run the Lead's documented unified hosted runner sequentially after
integration. The maps source baseline predates its repair; do not replace that
runner or alter test manifests as part of World-owned file changes. Native
Unity testing remains paused; generated terrain is not native visual evidence.

## Multi-room building layout review pack

User-requested follow-on: reusable house, store, distribution-center,
restaurant/bar and church layouts. Generate the review pack in a fresh output
folder, without changing existing city recipes:

```bash
python Tools/generate_district_city.py building-pack \
  --directory /tmp/outbreak-building-pack --seed 214
```

The pack contains ten examples: compact and spacious versions of each building,
with seeded cardinal rotations. `index.html` links technical `.svg` floor plans,
local-cell `.blueprint.json` templates, and loadable `.obmap2` terrain shells.
`manifest.json` records the seed, examples and required Lead decisions. The
Python API `generate_reference_city.building_blueprint(type, variant, rotation)`
returns a reusable layout; `rotation` is 0–3 clockwise quarter turns. Call
`generate_district_city.building_preview_map(blueprint)` to inspect its shell.
These are game-cell designs, not measured architectural plans.

| Building | Rooms and planned fixtures |
| --- | --- |
| House | Large living area with seating and an identified recovery region; smaller storage room behind an internal door, containing the household storage shelf. No bed fixture. |
| Store | Sales floor with multiple shelf positions and clear approach aisles; separate stockroom with storage shelving, an internal door and receiving entrance. |
| Distribution center | Dispatch office containing an order-computer position and courier approach point; separate warehouse with storage racks and service entrance. |
| Restaurant/bar | Dining/bar area and separate kitchen; public dining entrance and kitchen service entrance; internal staff door; service counter with opposite cook/waiter interaction cells; tables, bar, cooking and storage positions. |
| Church | Sanctuary with central aisle, pew positions and altar; separate vestry/storage room behind an internal door. |

Blueprint coordinates are local simulation cells. Preview authoring metadata
provides `origin_cells` for translation to its map. Room IDs, fixture IDs and
interaction IDs are stable **draft authoring identifiers**, not current saved
home/job/object identities. A future city placer must namespace them per
building instance, verify the template footprint fits its lot, and retain all
room/door/fixture transforms together. The new layouts are **not automatically
applied** to the existing single-room district generator or existing saves.

Each template describes room floor cells, enclosing walls, exterior/internal
doors, proposed solid fixture footprints and accessible interaction cells.
Geometry checks treat proposed fixtures as obstacles and verify usable room
floor remains connected, every room is accessible from outside, fixtures have
adjacent reachable approaches, and each service counter has opposite kitchen
and dining approaches. Tests cover all five types, both variants and four
rotations, malformed/blocked layouts and exact pack replay.

**Current runtime boundary:** OBMAP2 shells use existing schema 1 floors, walls,
doors and sidewalks. Fixtures are drawn in technical SVG plans and retained in
opaque `authoringJson`; they are not materialized as game shelves, storage,
rest spots, computers, furniture or counters. The shell therefore has no live
fixture collision/inventory or new mechanics. The restaurant counter uses wall
terrain as a solid partition placeholder; visibility and across-counter actions
need an agreed counter implementation. No NPCs, supplies or behaviors are added.

The living region records the intended energy-recovery area; the NPC coder and
Lead own activation, rates and rest decisions. The order-computer record gives
an explicit courier interaction site; clerk calls still use current behavior
until their coder implements a physical terminal visit/claim protocol. Kitchen
handoff pairs are geometry only: no cooking, waiting, food transfers, bar or
church mechanics have been implemented.

Before gameplay integration, the Lead must agree building-to-room grouping,
finite inventory fixture mappings, recovery semantics, order claiming, counter
collision/visibility and interaction rules with the appropriate coders. The
current engine discovers **each enclosed room as a separate shelter**, so these
two-room shells cannot safely replace assigned homes or stores until that
membership/identity contract is resolved. No shared contracts, persistence,
NPC logic, Unity settings, assets, GUIDs or dependencies change in this pack.
Native Unity testing remains paused. Run the Python checks above and the
Lead's sequential unified hosted runner before accepting integration.
