"""Survey real map geometry and export a common-scale reference atlas and OBMAP2 templates.

No API keys. Download only explicitly selected small areas; cache original responses.
OSM-derived data retain ODbL attribution. This measures layouts, not an ML model.
"""
import argparse
import bz2
from collections import Counter, defaultdict
from datetime import datetime, timezone
import hashlib
import html
import json
import math
from pathlib import Path
import sqlite3
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET

from pyproj import CRS, Transformer
from shapely import from_wkb, from_wkt
from shapely.geometry import LineString, Point, Polygon, box, mapping, shape
from shapely.ops import polygonize, transform, unary_union

ROOT = Path(__file__).resolve().parents[1]
OSM_ATTRIBUTION = "© OpenStreetMap contributors — https://www.openstreetmap.org/copyright (ODbL 1.0)"
ROAD_TYPES = {"motorway", "motorway_link", "trunk", "trunk_link", "primary", "primary_link",
              "secondary", "secondary_link", "tertiary", "tertiary_link", "unclassified",
              "residential", "living_street", "service"}
MAX_CELLS = 1_000_000


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False) + "\n")


def feature(geometry, category, identity, tags=None):
    return {"type": "Feature", "geometry": mapping(geometry),
            "properties": {"category": category, "source_id": str(identity), "tags": tags or {}}}


def category(tags):
    if tags.get("building") not in (None, "no"):
        return "building"
    if tags.get("highway") in ROAD_TYPES and tags.get("area") != "yes":
        return "road"
    if tags.get("boundary") == "administrative":
        return "boundary"
    if tags.get("landuse") or tags.get("leisure") in ("park", "garden", "nature_reserve"):
        return "landuse"
    if tags.get("natural") == "water" or tags.get("waterway") == "riverbank":
        return "water"
    return None


def osm_features(elements):
    """Overpass out geom ways, or XML converted to the same structure. No invented closures."""
    result = []
    skipped = Counter()
    for item in elements:
        if item.get("type") != "way":
            continue
        tags = item.get("tags", {})
        kind = category(tags)
        if not kind:
            continue
        points = [(p["lon"], p["lat"]) for p in item.get("geometry", [])]
        if len(points) < 2:
            skipped["missing_geometry"] += 1
            continue
        closed = len(points) >= 4 and points[0] == points[-1]
        if kind in ("building", "landuse", "water") and not closed:
            skipped["unclosed_polygon"] += 1
            continue
        geom = LineString(points) if kind in ("road", "boundary") else Polygon(points)
        if not geom.is_valid or geom.is_empty:
            skipped["invalid_geometry"] += 1
            continue
        result.append(feature(geom, kind, f"way/{item['id']}", tags))
    return result, dict(skipped)


def import_osm(path):
    data = Path(path).read_bytes()
    if path.suffix == ".bz2":
        data = bz2.decompress(data)
    if data.lstrip().startswith(b"{"):
        document = json.loads(data)
        if document.get("remark"):
            raise ValueError("Overpass reported an incomplete/failed query: " + document["remark"])
        return osm_features(document["elements"])
    root = ET.fromstring(data)
    nodes = {n.attrib["id"]: {"lon": float(n.attrib["lon"]), "lat": float(n.attrib["lat"])}
             for n in root.findall("node")}
    elements = []
    for way in root.findall("way"):
        refs = [n.attrib["ref"] for n in way.findall("nd")]
        # A missing interior node must not turn into a made-up straight connection.
        geometry = [nodes[n] for n in refs] if all(n in nodes for n in refs) else []
        elements.append({"type": "way", "id": way.attrib["id"], "geometry": geometry,
                         "tags": {t.attrib["k"]: t.attrib["v"] for t in way.findall("tag")}})
    return osm_features(elements)


def import_graphml(path):
    """Read an OSMnx graph, preserving its full projected edge geometry and deduplicating directions."""
    ns = {"g": "http://graphml.graphdrawing.org/xmlns"}
    root = ET.parse(path).getroot()
    keys = {k.attrib["id"]: k.attrib["attr.name"] for k in root.findall("g:key", ns)}

    def attrs(element):
        return {keys[d.attrib["key"]]: d.text for d in element.findall("g:data", ns)}

    graph = root.find("g:graph", ns)
    metadata = attrs(graph)
    convert = Transformer.from_crs(CRS.from_user_input(metadata["crs"]), 4326, always_xy=True)
    nodes = {n.attrib["id"]: attrs(n) for n in graph.findall("g:node", ns)}
    seen, result = set(), []
    for edge in graph.findall("g:edge", ns):
        tags = attrs(edge)
        if tags.get("highway") not in ROAD_TYPES:
            continue
        if "geometry" in tags:
            geom = transform(convert.transform, from_wkt(tags["geometry"]))
        else:
            ends = [nodes[edge.attrib[k]] for k in ("source", "target")]
            geom = LineString([(float(n["lon"]), float(n["lat"])) for n in ends])
        identity = (tags.get("osmid"), geom.normalize().wkb)
        if identity in seen:
            continue
        seen.add(identity)
        result.append(feature(geom, "road", tags.get("osmid", str(len(result))), tags))
    return result, {"note": "Road-only published sample; buildings and district polygons are unavailable."}


def import_gpkg(path):
    """Read only the published streets/buildings layers; synthetic tessellation is not district data."""
    result = []
    with sqlite3.connect(path.resolve().as_uri()+"?mode=ro", uri=True) as connection:
        for table, column, srs in connection.execute("select table_name,column_name,srs_id from gpkg_geometry_columns"):
            if table not in ("streets", "buildings") or column != "geom":
                continue
            project = Transformer.from_crs(srs, 4326, always_xy=True)
            for index, (blob,) in enumerate(connection.execute('select geom from "'+table+'"')):
                # GeoPackage header: eight fixed bytes followed by a variable-size envelope.
                if blob[:2] != b"GP":
                    raise ValueError("Invalid GeoPackage header")
                envelope = (blob[3] >> 1) & 7
                sizes = {0:0, 1:32, 2:48, 3:48, 4:64}
                if envelope not in sizes:
                    raise ValueError("Unsupported GeoPackage envelope")
                geom = transform(project.transform, from_wkb(blob[8+sizes[envelope]:]))
                result.append(feature(geom,"road" if table=="streets" else "building",table+"/"+str(index)))
    return result, {"note":"Published morphology sample. Upstream street/building lineage is not specified; no OSM district claims."}


def validate_bbox(bounds):
    south, west, north, east = bounds
    if not (-85 <= south < north <= 85 and -180 <= west < east <= 180):
        raise ValueError("bbox must be south west north east, excluding poles and antimeridian crossings")
    if north - south > .1 or east - west > .2:
        raise ValueError("Use a neighborhood survey window, not a whole-city download")


def download(sample, cache, endpoint):
    validate_bbox(sample["bbox"])
    if not endpoint.startswith("https://"):
        raise ValueError("Overpass endpoint must use HTTPS")
    bounds = ",".join(map(str, sample["bbox"]))
    query = (f'[out:json][timeout:60];(way[highway]({bounds});way[building]({bounds});'
             f'way[landuse]({bounds});way[leisure=park]({bounds});'
             f'way[natural=water]({bounds});way[boundary=administrative]({bounds}););out tags geom;')
    path = cache / (sample["id"] + ".overpass.json")
    if not path.exists():
        request = urllib.request.Request(endpoint+"?"+urllib.parse.urlencode({"data": query}),
                                         headers={"User-Agent": "OutbreakGrid-CityAtlas/1.0"})
        with urllib.request.urlopen(request, timeout=90) as response:
            data = response.read(32 * 1024 * 1024 + 1)
        if len(data) > 32 * 1024 * 1024:
            raise ValueError("Overpass response exceeds 32 MiB")
        parsed = json.loads(data)
        if parsed.get("remark") or not parsed.get("elements"):
            raise ValueError("Overpass query failed or returned no features")
        cache.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
    return path


def survey(features, bounds, cell_metres):
    validate_bbox(bounds)
    if not math.isfinite(cell_metres) or cell_metres < 1 or cell_metres > 100:
        raise ValueError("cell size must be between 1 and 100 metres")
    south, west, north, east = bounds
    lat, lon = (south + north) / 2, (west + east) / 2
    crs = CRS.from_proj4(f"+proj=aeqd +lat_0={lat} +lon_0={lon} +datum=WGS84 +units=m")
    projector = Transformer.from_crs(4326, crs, always_xy=True)
    footprint = transform(projector.transform, box(west, south, east, north))
    left, bottom, right, top = footprint.bounds
    width, height = math.ceil((right-left)/cell_metres), math.ceil((top-bottom)/cell_metres)
    if width * height > MAX_CELLS:
        raise ValueError("Grid exceeds one million cells; increase cell size or reduce the area")
    projected, counts = [], Counter()
    for item in features:
        geom = shape(item["geometry"])
        if not geom.is_valid:
            raise ValueError("Invalid input geometry: " + item["properties"]["source_id"])
        geom = transform(projector.transform, geom).intersection(footprint)
        if geom.is_empty:
            continue
        counts[item["properties"]["category"]] += 1
        # Coordinates use metres with the screen origin in the north-west.
        geom = transform(lambda x, y, z=None: (x-left, top-y), geom)
        projected.append((geom, item["properties"]))
    roads = [g for g, p in projected if p["category"] == "road"]
    if not roads:
        raise ValueError("Survey contains no drivable street geometry")
    # Unary union nodes real crossings. Bridge/tunnel layers are not merged into driving connectivity.
    layers = defaultdict(list)
    for geom, props in projected:
        if props["category"] == "road":
            tags = props.get("tags", {})
            layer = str(tags.get("layer", "0")), str(tags.get("bridge", "no")), str(tags.get("tunnel", "no"))
            layers[layer].append(geom)
    lengths, degrees, boundary_nodes = [], Counter(), set()
    cos4 = sin4 = total_length = curved_length = 0.0
    for group in layers.values():
        network = unary_union(group)
        edges = list(network.geoms) if hasattr(network, "geoms") else [network]
        layer_degrees = Counter()
        for edge in edges:
            if edge.geom_type != "LineString" or edge.length < .1:
                continue
            coords = list(edge.coords)
            lengths.append(edge.length)
            chord = Point(coords[0]).distance(Point(coords[-1]))
            if chord > 1 and edge.length / chord > 1.05:
                curved_length += edge.length
            for end in (coords[0], coords[-1]):
                node = tuple(round(v, 3) for v in end)
                layer_degrees[node] += 1
                if min(end[0], end[1], right-left-end[0], top-bottom-end[1]) < cell_metres * 2:
                    boundary_nodes.add(node)
            for a, b in zip(coords, coords[1:]):
                length = math.dist(a, b)
                angle = math.atan2(b[1]-a[1], b[0]-a[0])
                cos4 += length * math.cos(4*angle)
                sin4 += length * math.sin(4*angle)
                total_length += length
        # Keep different layers separate, including nodes sharing a coordinate.
        for node, degree in layer_degrees.items():
            if node not in boundary_nodes:
                degrees[degree] += 1
    # Closed ground-level road loops define survey blocks, never administrative districts.
    ground = [g for (layer, bridge, tunnel), group in layers.items()
              if layer == "0" and bridge in ("no", "False", "false") and tunnel in ("no", "False", "false") for g in group]
    blocks = [p for p in polygonize(unary_union(ground)) if p.area >= 1000]
    block_areas, block_aspects = [], []
    for index, block in enumerate(blocks):
        block_areas.append(block.area)
        corners = list(block.minimum_rotated_rectangle.exterior.coords)
        sides = [math.dist(corners[i],corners[i+1]) for i in (0,1)]
        block_aspects.append(max(sides)/min(sides))
        projected.append((block,{"category":"block","source_id":"derived-block/"+str(index),
                                 "tags":{"derived":"closed ground-level street loop; not an official district"}}))
    def median(values):
        values=sorted(values)
        return round((values[(len(values)-1)//2]+values[len(values)//2])/2,2) if values else None
    area = footprint.area / 1_000_000
    def coverage(kind):
        geoms = [g for g, p in projected if p["category"] == kind and g.geom_type in ("Polygon", "MultiPolygon")]
        return unary_union(geoms).area / footprint.area if geoms else None
    grid_alignment = math.hypot(cos4, sin4) / total_length if total_length else 0
    interior_nodes = sum(n for degree, n in degrees.items() if degree != 2)
    dead_end_share = degrees[1] / interior_nodes if interior_nodes else None
    lengths.sort()
    typical_length = lengths[len(lengths)//2]
    recommended_layout = "Grid" if grid_alignment > .8 and (dead_end_share or 0) < .15 else "Neighborhood" if (dead_end_share or 0) > .2 else "Staggered"
    profile = {
        "area_km2": round(area, 6), "feature_counts": dict(counts),
        "street_length_km": round(total_length / 1000, 4),
        "street_density_km_per_km2": round(total_length / 1000 / area, 4),
        "median_noded_street_segment_metres": round(typical_length, 2),
        "fourfold_orientation_alignment": round(grid_alignment, 4),
        "curved_street_length_share": round(curved_length / total_length, 4),
        "interior_junctions": sum(n for degree, n in degrees.items() if degree >= 3),
        "interior_dead_ends": degrees[1],
        "interior_dead_end_share": round(dead_end_share, 4) if dead_end_share is not None else None,
        "building_coverage": coverage("building"),
        "closed_street_blocks": len(blocks),
        "median_closed_block_area_m2": median(block_areas),
        "median_closed_block_aspect_ratio": median(block_aspects),
        "generation_hint": {"existing_street_layout": recommended_layout,
                            "observed_segment_cells": round(typical_length/cell_metres, 2),
                            "zoning": "NeighborhoodCenters",
                            "note": "Measured reference hint only; runtime presets are not automatically changed."},
        "limitations": ["Sample windows are not administrative districts.",
                       "Missing buildings or land-use tags mean unknown, not zero.",
                       "Boundary junctions are excluded; disconnected or incomplete extracts can bias metrics.",
                       "Crossings are geometric within tagged layers; this is not a routable traffic model.",
                       "Median segment length is not a measured block-size distribution."]}
    return projected, {"width": width, "height": height, "cell_metres": cell_metres,
                       "survey_bbox_south_west_north_east": bounds,
                       "projected_origin_x": left, "projected_origin_y": top,
                       "projection": crs.to_string(), "north_up": True}, profile


def export_map(sample, projected, grid, provenance,road_width_cells=1.5,sidewalk_width_cells=.75):
    """Geometry-only template: road corridors and solid footprint obstacles, no invented rooms/jobs."""
    width, height, scale = grid["width"], grid["height"], grid["cell_metres"]
    terrain = bytearray(width*height)
    shapes = []
    for geom, props in projected:
        kind = props["category"]
        if kind == "road":
            # Visual corridor convention, NOT surveyed road width or vehicle lanes.
            street_width=props.get("width_cells",road_width_cells)
            if not math.isfinite(street_width) or not .5<=street_width<=8 or not math.isfinite(sidewalk_width_cells) or not .25<=sidewalk_width_cells<=4:
                raise ValueError("Invalid road or sidewalk width")
            shapes.extend([(geom.buffer(scale*(street_width/2+sidewalk_width_cells)), 5), (geom.buffer(scale*street_width/2), 6)])
        elif kind == "building":
            shapes.append((geom, 8))
        elif kind == "water":
            shapes.append((geom, 12))
    # All sidewalks first, then roads, then obstacles; road order cannot erase a junction.
    for geom, tile in sorted(shapes, key=lambda item: {5:0,6:1,8:2,12:3}[item[1]]):
        x0, y0, x1, y1 = geom.bounds
        for y in range(max(0,int(y0/scale)), min(height, math.ceil(y1/scale))):
            for x in range(max(0,int(x0/scale)), min(width, math.ceil(x1/scale))):
                if geom.covers(Point((x+.5)*scale, (y+.5)*scale)):
                    terrain[y*width+x] = tile
    chunks = []
    for cy in range((height+31)//32):
        for cx in range((width+31)//32):
            cells = [terrain[y*width+x] if x<width and y<height else 0
                     for y in range(cy*32,cy*32+32) for x in range(cx*32,cx*32+32)]
            if any(cells):
                chunks.append({"x":cx,"y":cy,"cells":cells})
    return {"kind":"outbreak.map","schemaVersion":1,"id":"reference-"+sample["id"],
            "name":sample["name"]+" — geometry template", "width":width,"height":height,
            "chunkSize":32,"chunks":chunks,
            "authoringJson":json.dumps({"reference":sample,"grid":grid,"provenance":provenance,
                "scope":"Road widths are symbolic; solid building obstacles; no rooms, vehicle lanes or population."},ensure_ascii=False)}


def svg_map(projected, grid, profile):
    width, height = grid["width"]*grid["cell_metres"], grid["height"]*grid["cell_metres"]
    colours = {"road":"#e8cf9b", "building":"#7c97b5", "landuse":"#446958", "water":"#457e9b", "boundary":"#ee8585", "block":"#2b3946"}
    orientation="North-up layout" if grid.get("north_up",True) else "Street axes aligned to game cardinal directions; geographic north rotated"
    parts = [f'<svg viewBox="0 0 {width} {height}" xmlns="http://www.w3.org/2000/svg" role="img" aria-label="{orientation}">',
             '<rect width="100%" height="100%" fill="#17232b"/>']
    for geom, props in sorted(projected, key=lambda item: {"block":0,"landuse":1,"water":2,"building":3,"road":4,"boundary":5}[item[1]["category"]]):
        kind = props["category"]
        def draw(g):
            if g.geom_type in ("MultiPolygon", "MultiLineString", "GeometryCollection"):
                for child in g.geoms:
                    draw(child)
            elif g.geom_type in ("Polygon", "LineString"):
                rings = [list(g.exterior.coords), *[list(r.coords) for r in g.interiors]] if g.geom_type=="Polygon" else [list(g.coords)]
                d = " ".join("M"+" L".join(f"{x:.2f},{y:.2f}" for x,y in ring)+( " Z" if g.geom_type=="Polygon" else "") for ring in rings)
                fill = ("#446958" if props.get("open_space") else colours[kind]) if g.geom_type=="Polygon" else "none"
                stroke_width=props.get("width_cells",.6)*grid["cell_metres"] if kind=="road" else 1
                parts.append(f'<path d="{d}" fill="{fill}" fill-rule="evenodd" stroke="{colours[kind]}" stroke-width="{stroke_width}"/>')
        draw(geom)
    # Toggleable grid, every cell at the same metric scale; major lines every ten cells.
    scale = grid["cell_metres"]
    parts.append('<g class="grid" stroke="#ffffff" stroke-opacity=".16" stroke-width=".5">')
    for x in range(0,grid["width"]+1):
        parts.append(f'<path d="M{x*scale},0 V{height}"/>')
    for y in range(0,grid["height"]+1):
        parts.append(f'<path d="M0,{y*scale} H{width}"/>')
    parts.append('</g>')
    direction="north ↑" if grid.get("north_up",True) else "game north ↑ (rotated)"
    parts.append(f'<g stroke="white" fill="white"><path d="M15,{height-15} h100"/><text x="15" y="{height-22}" stroke="none" font-size="12">100 m · {direction}</text></g></svg>')
    return "".join(parts)


def build(sample, features, provenance, output, cell_metres=5):
    output.mkdir(parents=True, exist_ok=True)
    projected, grid, profile = survey(features, sample["bbox"], cell_metres)
    document = {"schema_version":1,"sample":sample,"provenance":provenance,"grid":grid,"metrics":profile}
    write_json(output/(sample["id"]+".profile.json"), document)
    write_json(output/(sample["id"]+".geojson"), {"type":"FeatureCollection","features":features,"provenance":provenance})
    inverse=Transformer.from_crs(grid["projection"],4326,always_xy=True)
    inferred=[]
    for geometry,props in projected:
        if props["category"]!="block":
            continue
        geometry=transform(lambda x,y,z=None:(x+grid["projected_origin_x"],grid["projected_origin_y"]-y),geometry)
        geometry=transform(inverse.transform,geometry)
        inferred.append({"type":"Feature","geometry":mapping(geometry),"properties":props})
    write_json(output/(sample["id"]+".blocks.geojson"),{"type":"FeatureCollection","features":inferred,"provenance":provenance})
    write_json(output/(sample["id"]+".obmap2"), export_map(sample,projected,grid,provenance))
    (output/(sample["id"]+".svg")).write_text(svg_map(projected,grid,profile))
    return document


def build_index(output, selection):
    cards, present = [], set()
    for path in sorted(output.glob("*.profile.json")):
        doc = json.loads(path.read_text()); sample=doc["sample"]; metrics=doc["metrics"]; identity=sample["id"]
        present.add(identity)
        svg=(output/(identity+".svg")).read_text()
        details = f'{metrics["street_length_km"]} km streets · {metrics.get("closed_street_blocks", "unmeasured")} closed blocks · {doc["grid"]["cell_metres"]} m/cell'
        cards.append(f'<article><h2>{html.escape(sample["name"])}</h2><p>{html.escape(sample.get("form","Published reference sample"))}</p>{svg}<p>{details}</p><p>{html.escape(doc["provenance"]["attribution"])}</p><a href="{identity}.profile.json">Measurements</a> · <a href="{identity}.geojson">Geography</a> · <a href="{identity}.obmap2">Game template</a></article>')
    generated_cards=[]
    generated=output.parent/"generated"
    for path in sorted(generated.glob("*.generation.json")):
        doc=json.loads(path.read_text());stem=path.name.removesuffix(".generation.json")
        artwork=(generated/(stem+".svg")).read_text()
        generated_cards.append(f'<article><h2>{html.escape(doc["reference_id"])} · seed {doc["seed"]}</h2><p>{len(doc["buildings"])} fictional buildings with doors and street paths.</p>{artwork}<p>Surveyed roads retained; new seeded lots. No population or assigned roles.</p><a href="../generated/{stem}.obmap2">Loadable generated map</a> · <a href="../generated/{stem}.generation.json">Generation recipe</a></article>')
    pending = [s for s in selection["samples"] if s["id"] not in present]
    pending_html = "".join(f'<li>{html.escape(s["name"])} — {html.escape(s["form"])}</li>' for s in pending)
    page = '''<!doctype html><meta charset="utf-8"><title>OutbreakGrid · real-world city atlas</title>
<style>body{background:#101920;color:#e2eaf0;font:16px system-ui;margin:30px auto;max-width:1200px;padding:0 20px}h1{font-size:32px}a{color:#8ac8ff}.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:24px}article{background:#1c2a35;padding:20px;border-radius:12px}svg{width:100%;max-height:520px}body.hide-grid .grid{display:none}p{line-height:1.5}button{padding:10px}</style>
<h1>Real places → city-generation references</h1><p>North-up geography on a consistent metric grid. These are measured samples, not complete cities or an AI-trained model. Building coverage and district data depend on source availability.</p>
<button onclick="document.body.classList.toggle('hide-grid')">Show / hide grid</button><p>Roads: sand · buildings: blue-grey · street-enclosed blocks: slate · tagged land use: green · water: blue · available boundary ways: coral. Each view includes a 100 m scale. Road widths are symbolic.</p>
<div class="cards">'''+"".join(cards)+'''</div><h2>Generated map examples</h2><p>These reuse a surveyed street backbone and generate new buildings inside its closed blocks. They are fictional maps, not surveys of real buildings.</p><div class="cards">'''+"".join(generated_cards)+'''</div><h2>Selected next surveys — not downloaded</h2><ul>'''+pending_html+'''</ul><p>Complete these through the Overpass download command once network access is enabled. Survey windows are not official district boundaries; multipolygon relations are not reconstructed in this first version.</p>'''
    (output/"index.html").write_text(page)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--selection",type=Path,default=ROOT/"CityReferences/selection.json")
    parser.add_argument("--output",type=Path,default=ROOT/"CityReferences/atlas")
    parser.add_argument("--cache",type=Path,default=ROOT/"CityReferences/cache")
    parser.add_argument("--cell-metres",type=float,default=5)
    sub=parser.add_subparsers(dest="command",required=True)
    dl=sub.add_parser("download");dl.add_argument("--sample",required=True);dl.add_argument("--endpoint",default="https://overpass-api.de/api/interpreter")
    local=sub.add_parser("import");local.add_argument("path",type=Path);local.add_argument("--id",required=True);local.add_argument("--name",required=True);local.add_argument("--bbox",nargs=4,type=float);local.add_argument("--source-url",required=True);local.add_argument("--attribution",default=OSM_ATTRIBUTION)
    sub.add_parser("index")
    args=parser.parse_args(); selection=json.loads(args.selection.read_text())
    if args.command=="index":
        build_index(args.output,selection);return
    if args.command=="download":
        sample=next((s for s in selection["samples"] if s["id"]==args.sample),None)
        if sample is None:
            parser.error("Unknown sample; choose an id from selection.json")
        path=download(sample,args.cache,args.endpoint);source=args.endpoint
    else:
        sample={"id":args.id,"name":args.name,"bbox":args.bbox};path=args.path;source=args.source_url
    if not sample["id"] or any(c not in "abcdefghijklmnopqrstuvwxyz0123456789-" for c in sample["id"]):
        parser.error("sample id must contain only lowercase letters, digits and hyphens")
    reader={".graphml":import_graphml,".gpkg":import_gpkg}.get(path.suffix,import_osm)
    features,skipped=reader(path)
    if not sample["bbox"]:
        west,south,east,north=unary_union([shape(f["geometry"]) for f in features]).bounds
        sample["bbox"]=[south,west,north,east]
    provenance={"source_url":source,"sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
                "processed_utc":datetime.now(timezone.utc).isoformat(),"attribution":getattr(args,"attribution",OSM_ATTRIBUTION),
                "skipped":skipped,"scope":"Historical published sample" if args.command=="import" else "Overpass survey response"}
    if path.suffix==".json":
        snapshot=json.loads(path.read_text()).get("osm3s",{}).get("timestamp_osm_base")
        if snapshot:provenance["osm_base_timestamp_utc"]=snapshot
    doc=build(sample,features,provenance,args.output,args.cell_metres)
    build_index(args.output,selection)
    print(f'Built {sample["name"]}: {doc["grid"]["width"]} × {doc["grid"]["height"]} cells; {doc["metrics"]["street_length_km"]} km streets')


if __name__=="__main__":
    main()
