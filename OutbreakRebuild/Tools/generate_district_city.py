"""Reference-calibrated district templates and connected mixed-city composition.

Building use/storey fields are placement hints for procedural consumers, not NPC jobs
or simulated upper floors. All terrain continues to use the existing OBMAP2 format.
"""
import argparse
from collections import Counter, deque
import copy
import hashlib
import json
import math
from pathlib import Path

from shapely.geometry import LineString, box

import city_atlas as atlas
import generate_reference_city as lots
import generate_seeded_city as streets

VERSION="district-city-3"
CATALOGUE=atlas.ROOT/"CityReferences/district_catalogue.json"
COLOURS={"apartments":"#9d80c8","terrace-home":"#87b6ce","detached-home":"#8eb887",
         "retail":"#dfac65","office":"#639ac4","hotel":"#cc879e"}
HINTS={"apartments":"Home","terrace-home":"Home","detached-home":"Home","retail":"Store","office":None,"hotel":None}


def digest(value):return hashlib.sha256(streets.canonical(value)).hexdigest()


def stream_seed(seed,domain):
    # Avalanche nearby user seeds before xorshift: avoid correlated initial block/park decisions.
    return int(digest({"seed":seed,"stream":domain})[:8],16) or 1


def load_profile(identity):
    if identity is None:return None
    if not identity or any(c not in "abcdefghijklmnopqrstuvwxyz0123456789-" for c in identity):
        raise ValueError("Invalid reference id")
    return json.loads((atlas.ROOT/"CityReferences/atlas"/(identity+".profile.json")).read_text())


def district_recipe(kind,seed=214,overrides=None):
    catalogue=json.loads(CATALOGUE.read_text())
    if kind not in catalogue["districts"]:raise ValueError("Unknown district type")
    spec=copy.deepcopy(catalogue["districts"][kind]);profile=load_profile(spec["reference_id"])
    settings=streets.preset_settings(spec["base_preset"],seed)
    settings.update(spec["settings"])
    # No v2 reference is substituted: this recipe records its own surveyed calibration.
    settings["reference_id"]=None
    if spec["match_reference_blocks"] and profile:
        area=profile["metrics"].get("median_closed_block_area_m2") or 10000
        aspect=min(3,profile["metrics"].get("median_closed_block_aspect_ratio") or 1)
        settings["block_width_cells"]=max(26,min(48,round(math.sqrt(area*aspect)/settings["cell_metres"])))
        settings["block_height_cells"]=max(24,min(40,round(math.sqrt(area/aspect)/settings["cell_metres"])))
        if spec["category"]=="business":
            # Adapt surveyed spacing to fit larger game-scale office footprints and entrances.
            settings["block_width_cells"]=max(40,settings["block_width_cells"])
            settings["block_height_cells"]=max(36,settings["block_height_cells"])
    if overrides:settings.update(overrides)
    streets.validate(settings)
    recipe={"generator_version":VERSION,"kind":"district","district_type":kind,"seed":seed,
            "name":spec["name"],"category":spec["category"],"street_settings":settings,
            "footprint_sizes":spec["footprint_sizes"],"placement_mode":"frontage",
            "lot_gap_cells":spec["lot_gap_cells"],"uses":spec["uses"],"storeys":spec["storeys"],
            "reference_id":spec["reference_id"],"reference_profile_sha256":digest(profile) if profile else None}
    return recipe


def validate_recipe(recipe):
    required={"generator_version","kind","district_type","seed","name","category","street_settings", "footprint_sizes","placement_mode","lot_gap_cells","uses","storeys","reference_id","reference_profile_sha256"}
    if set(recipe)!=required or recipe["generator_version"]!=VERSION or recipe["kind"]!="district":
        raise ValueError("Unsupported or incomplete district recipe")
    if not isinstance(recipe["district_type"],str) or not recipe["district_type"] or any(c not in "abcdefghijklmnopqrstuvwxyz0123456789-" for c in recipe["district_type"]):
        raise ValueError("District ids must contain only lowercase letters, digits and hyphens")
    if recipe["category"] not in ("urban","residential","suburban","business"):
        raise ValueError("Unknown district category")
    streets.validate(recipe["street_settings"])
    if type(recipe["seed"]) is not int or not 1<=recipe["seed"]<=0xffffffff or recipe["seed"]!=recipe["street_settings"]["seed"]:
        raise ValueError("Recipe and street seeds must match and be nonzero uint32 values")
    if recipe["placement_mode"] not in ("frontage","scatter") or type(recipe["lot_gap_cells"]) is not int or not 0<=recipe["lot_gap_cells"]<=4:
        raise ValueError("Invalid lot placement controls")
    if not isinstance(recipe["footprint_sizes"],list) or not 1<=len(recipe["footprint_sizes"])<=16:
        raise ValueError("At least one bounded footprint recipe is required")
    for pair in recipe["footprint_sizes"]:
        if not isinstance(pair,list) or len(pair)!=2 or any(type(v) is not int or not 4<=v<=20 for v in pair) or (pair[0]-2)*(pair[1]-2)>256:
            raise ValueError("Footprints must fit the game's 256-interior-cell room limit")
    if not recipe["uses"] or any(k not in COLOURS or type(v) is not int or not 1<=v<=100 for k,v in recipe["uses"].items()):
        raise ValueError("Invalid building-use weights")
    levels=recipe["storeys"]
    if not isinstance(levels,list) or len(levels)!=2 or any(type(v) is not int for v in levels) or not 1<=levels[0]<=levels[1]<=40:
        raise ValueError("Invalid design storey range")
    profile=load_profile(recipe["reference_id"])
    if (digest(profile) if profile else None)!=recipe["reference_profile_sha256"]:
        raise ValueError("Reference profile changed; preserve the bundled snapshot for exact replay")
    return profile


def assert_usable(document,metadata):
    cells=lots.unpack(document);width,height=document["width"],document["height"]
    if len(metadata["buildings"])>256 or cells.count(7)>32768:
        raise ValueError("Map exceeds the game's room/floor scan budget; reduce building limits")
    roads={i for i,tile in enumerate(cells) if tile==6}
    if not roads:raise ValueError("Map has no roads")
    first=min(roads);visited={first};queue=deque([first])
    while queue:
        cell=queue.popleft();x,y=cell%width,cell//width
        for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
            key=ny*width+nx
            if 0<=nx<width and 0<=ny<height and key in roads and key not in visited:
                visited.add(key);queue.append(key)
    if roads!=visited:raise ValueError("Road network is disconnected")
    for building in metadata["buildings"]:
        x,y=building["door"]
        if cells[y*width+x]!=9 or not building["path_cells"] or any(cells[py*width+px] not in (5,6) for px,py in building["path_cells"]):
            raise ValueError("Building has no physical entrance path")
        if (building["width"]-2)*(building["height"]-2)>256:raise ValueError("Room is too large")


def render_map(document,metadata):
    """Accurate terrain raster plus colour-coded building-use hints; all geometry in map metres."""
    width,height=document["width"],document["height"];scale=metadata["grid"]["cell_metres"]
    cells=lots.unpack(document)
    palette={5:"#65776c",6:"#e8cf9b",7:"#bdcbae",8:"#7c97b5",9:"#f4a259",12:"#457e9b"}
    parts=[f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width*scale} {height*scale}" role="img" aria-label="Fictional cardinal district map">', '<rect width="100%" height="100%" fill="#17232b"/>']
    for y in range(height):
        x=0
        while x<width:
            tile=cells[y*width+x];start=x;x+=1
            while x<width and cells[y*width+x]==tile:x+=1
            if tile in palette:parts.append(f'<rect x="{start*scale}" y="{y*scale}" width="{(x-start)*scale}" height="{scale}" fill="{palette[tile]}"/>')
    for b in metadata["buildings"]:
        parts.append(f'<rect x="{(b["left"]+1)*scale}" y="{(b["top"]+1)*scale}" width="{(b["width"]-2)*scale}" height="{(b["height"]-2)*scale}" fill="{COLOURS[b["use_hint"]]}"/>')
    for district in metadata.get("districts",[]):
        x,y,w,h=district["bounds_cells"]
        parts.append(f'<rect x="{x*scale}" y="{y*scale}" width="{w*scale}" height="{h*scale}" fill="none" stroke="#ffffff" stroke-opacity=".45" stroke-width="2"/>')
    parts.append('</svg>')
    return ''.join(parts)


def generate_district(recipe):
    profile=validate_recipe(recipe);settings=recipe["street_settings"]
    streams={name:stream_seed(recipe["seed"],name) for name in ("streets","lots","uses")}
    projected,grid=streets.street_layout(dict(settings,seed=streams["streets"]))
    block_choices=[(g,p) for g,p in projected if p["category"]=="block"]
    open_space_recovery=None
    if block_choices and all(p.get("open_space",False) for g,p in block_choices):
        # A valid sparse/cul-de-sac seed can have only one closed block. Keep a buildable district.
        geometry,props=min(block_choices,key=lambda item:((item[0].centroid.x-grid["width"]*grid["cell_metres"]/2)**2+(item[0].centroid.y-grid["height"]*grid["cell_metres"]/2)**2,item[1]["source_id"]))
        props["open_space"]=False;open_space_recovery=props["source_id"]
    provenance={"attribution":"Designed fictional district; no matching survey.","calibration":"Explicit design controls."}
    if profile:
        provenance={**profile["provenance"],"reference_profile_sha256":recipe["reference_profile_sha256"],
                    "calibration":"Block dimensions, source building coverage and street form are references. Layout, use weights and heights are explicit fictional design choices."}
    document,metadata,_=lots.generate_on_layout({"id":recipe["district_type"],"name":recipe["name"],"fictional":True},projected,grid,
        {"building_coverage":profile["metrics"]["building_coverage"] if profile else None},provenance,streams["lots"],settings["max_buildings"],
        density=settings["building_coverage"],setback_cells=settings["setback_cells"],footprint_sizes=recipe["footprint_sizes"],
        road_width_cells=settings["road_width_cells"],sidewalk_width_cells=settings["sidewalk_width_cells"],
        placement_mode=recipe["placement_mode"],lot_gap_cells=recipe["lot_gap_cells"],fair_block_budget=True)
    rng=lots.StableRandom(streams["uses"])
    uses=sorted(recipe["uses"].items());total=sum(weight for use,weight in uses)
    for index,b in enumerate(metadata["buildings"]):
        roll=rng.randrange(total)
        for use,weight in uses:
            roll-=weight
            if roll<0:break
        b.update(parcel_id=f'{recipe["district_type"]}-{index+1}',district_type=recipe["district_type"],use_hint=use,
                 game_building_use_hint=HINTS[use],storeys_hint=rng.randint(*recipe["storeys"]))
    fingerprint=digest(recipe)[:12]
    metadata.update(generator_version=VERSION,seed=recipe["seed"],stream_seeds=streams,recipe=copy.deepcopy(recipe),recipe_id=fingerprint,
                    district_type=recipe["district_type"],category=recipe["category"],
                    scope="Fictional seeded district with parcel/use/height hints. No assigned jobs, inhabitants, inventory, upper-floor simulation or directed traffic lanes.")
    if open_space_recovery is not None:metadata["open_space_recovery"]=open_space_recovery
    metadata["street_segments"]=[{"points_metres":list(g.coords),"width_cells":p["width_cells"],**p["tags"]} for g,p in projected if p["category"]=="road"]
    metadata["actual_building_coverage"]=round(sum(b["width"]*b["height"] for b in metadata["buildings"])/(document["width"]*document["height"]),6)
    metadata["use_counts"]=dict(Counter(b["use_hint"] for b in metadata["buildings"]))
    document.update(id=recipe["district_type"]+"-"+str(recipe["seed"])+"-"+fingerprint,name=recipe["name"]+" (seed "+str(recipe["seed"])+")")
    document["authoringJson"]=json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)
    assert_usable(document,metadata)
    return document,metadata,render_map(document,metadata)


def city_recipe(kinds,seed=214,columns=2,gap=12,cell_metres=5):
    if not 1<=len(kinds)<=9 or type(columns) is not int or not 1<=columns<=3 or not 6<=gap<=30:
        raise ValueError("Compose 1–9 districts in 1–3 columns with 6–30 cells between slots")
    if type(seed) is not int or not 1<=seed<=0xffffffff:raise ValueError("Invalid city seed")
    rng=lots.StableRandom(stream_seed(seed,"city-districts"))
    budget=256//len(kinds)
    districts=[district_recipe(kind,rng.word() or 1,{"cell_metres":cell_metres,"max_buildings":min(budget,64)}) for kind in kinds]
    return {"generator_version":VERSION,"kind":"city","seed":seed,"columns":columns,"gap_cells":gap,"districts":districts}


def compose_city(recipe):
    if set(recipe)!={"generator_version","kind","seed","columns","gap_cells","districts"} or recipe["generator_version"]!=VERSION or recipe["kind"]!="city":
        raise ValueError("Unsupported city recipe")
    districts=recipe["districts"];columns=recipe["columns"];gap=recipe["gap_cells"]
    if type(recipe["seed"]) is not int or not 1<=recipe["seed"]<=0xffffffff or not 1<=len(districts)<=9 or type(columns) is not int or not 1<=columns<=3 or type(gap) is not int or not 6<=gap<=30:
        raise ValueError("Invalid city seed, district count or slot layout")
    if sum(r["street_settings"]["max_buildings"] for r in districts)>256:raise ValueError("City building budgets exceed 256")
    scales={r["street_settings"]["cell_metres"] for r in districts}
    if len(scales)!=1:raise ValueError("All districts must use the same metres-per-cell scale")
    scale=scales.pop();generated=[generate_district(r) for r in districts]
    slot_width=max(d["width"] for d,m,s in generated);slot_height=max(d["height"] for d,m,s in generated)
    rows=math.ceil(len(districts)/columns);margin=4
    width=margin*2+columns*slot_width+(columns-1)*gap;height=margin*2+rows*slot_height+(rows-1)*gap
    if width*height>atlas.MAX_CELLS:raise ValueError("Composed map exceeds the one-million-cell limit")
    cells=bytearray(width*height);placements=[];buildings=[];segments=[]
    for i,(document,metadata,_) in enumerate(generated):
        ox=margin+(i%columns)*(slot_width+gap);oy=margin+(i//columns)*(slot_height+gap)
        tile=lots.unpack(document)
        for y in range(document["height"]):
            cells[(oy+y)*width+ox:(oy+y)*width+ox+document["width"]]=tile[y*document["width"]:(y+1)*document["width"]]
        for b in metadata["buildings"]:
            moved=copy.deepcopy(b);moved.update(left=b["left"]+ox,top=b["top"]+oy,door=[b["door"][0]+ox,b["door"][1]+oy],
                path_cells=[[x+ox,y+oy] for x,y in b["path_cells"]],district_index=i,parcel_id=f'd{i}-'+b["parcel_id"])
            buildings.append(moved)
        bounds=[]
        for segment in metadata["street_segments"]:
            moved=copy.deepcopy(segment);moved["points_metres"]=[[x+ox*scale,y+oy*scale] for x,y in segment["points_metres"]]
            segments.append(moved);bounds.extend(moved["points_metres"])
        west=min(x for x,y in bounds)/scale;east=max(x for x,y in bounds)/scale
        north=min(y for x,y in bounds)/scale;south=max(y for x,y in bounds)/scale
        placements.append({"index":i,"district_type":districts[i]["district_type"],"category":metadata["category"],
                           "bounds_cells":[ox,oy,document["width"],document["height"]],"road_bounds_cells":[west,north,east,south],
                           "recipe_id":metadata["recipe_id"],"provenance":metadata["provenance"]})
    portals=[];connectors=[]
    for index,a in enumerate(placements):
        for other,side in ((index+1,"east"),(index+columns,"south")):
            if other>=len(placements) or side=="east" and index%columns==columns-1:continue
            b=placements[other];aw,an,ae,asouth=a["road_bounds_cells"];bw,bn,be,bs=b["road_bounds_cells"]
            if side=="east":
                low,high=max(an,bn),min(asouth,bs)
                if low>=high:raise ValueError("District road ranges do not overlap at portal")
                y=math.floor((low+high)/2)+.5;start,end=(ae,y),(bw,y)
            else:
                low,high=max(aw,bw),min(ae,be)
                if low>=high:raise ValueError("District road ranges do not overlap at portal")
                x=math.floor((low+high)/2)+.5;start,end=(x,asouth),(x,bn)
            # Narrow joins fit even the minimum two-cell frontage clearance of a local perimeter street.
            connectors.append((LineString([(x*scale,y*scale) for x,y in (start,end)]),{"category":"road","source_id":f'portal/{index}/{other}',"width_cells":2}))
            portals.append({"from_district":index,"to_district":other,"side":side,"start_cells":start,"end_cells":end,"road_width_cells":2,"sidewalk_width_cells":.5})
    grid={"width":width,"height":height,"cell_metres":scale,"north_up":True,"projection":"fictional local metric grid"}
    if connectors:
        connector_map=atlas.export_map({"id":"portals","name":"Portals"},connectors,grid,{"attribution":"Fictional city connectors"},2,.5)
        for index,tile in enumerate(lots.unpack(connector_map)):
            if not tile:continue
            if cells[index] in (7,8,9,12):raise ValueError("A district connector intersects a building or water")
            if cells[index]!=6:cells[index]=tile
        segments.extend({"points_metres":list(g.coords),"width_cells":p["width_cells"],"connector":True,"source_id":p["source_id"]} for g,p in connectors)
    identity=digest(recipe)[:12]
    metadata={"generator_version":VERSION,"recipe":copy.deepcopy(recipe),"recipe_id":identity,"seed":recipe["seed"],"grid":grid,
              "provenance":{"attribution":"Mixed fictional city; district source terms and provenance retained per region.","district_sources":[p["provenance"] for p in placements]},
              "districts":placements,"portals":portals,"buildings":buildings,"street_segments":segments,
              "use_counts":dict(Counter(b["use_hint"] for b in buildings)),
              "scope":"Connected district terrain and procedural parcel hints; no jobs, inhabitants, inventories, vehicle lanes or simulated upper floors."}
    document={"kind":"outbreak.map","schemaVersion":1,"id":f'mixed-city-{recipe["seed"]}-{identity}',"name":f'Mixed district city (seed {recipe["seed"]})',
              "width":width,"height":height,"chunkSize":32,"chunks":[],"authoringJson":json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)}
    lots.pack(document,cells);assert_usable(document,metadata)
    return document,metadata,render_map(document,metadata)


def save(result,output):
    document,metadata,artwork=result;stem=document["id"]
    atlas.write_json(output/(stem+".obmap2"),document)
    atlas.write_json(output/(stem+".generation.json"),metadata)
    atlas.write_json(output/(stem+".recipe.json"),metadata["recipe"])
    (output/(stem+".svg")).write_text(artwork)
    print(f'{stem}: {document["width"]} × {document["height"]}, {len(metadata["buildings"])} buildings')
    return stem


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output",type=Path,default=atlas.ROOT/"CityReferences/districts/maps")
    sub=parser.add_subparsers(dest="command",required=True)
    district=sub.add_parser("district");district.add_argument("--type",required=True);district.add_argument("--seed",type=int,default=214)
    district.add_argument("--coverage",type=float);district.add_argument("--blocks-across",type=int);district.add_argument("--blocks-down",type=int)
    city=sub.add_parser("city");city.add_argument("--districts",nargs='+',default=["dense-urban","business-core","terrace-residential","garden-suburb"]);city.add_argument("--seed",type=int,default=214);city.add_argument("--city-columns",type=int,default=2)
    replay=sub.add_parser("replay");replay.add_argument("recipe",type=Path)
    args=parser.parse_args()
    if args.command=="district":
        overrides={name:value for name,value in [("building_coverage",args.coverage),("columns",args.blocks_across),("rows",args.blocks_down)] if value is not None}
        recipe=district_recipe(args.type,args.seed,overrides)
    elif args.command=="city":recipe=city_recipe(args.districts,args.seed,args.city_columns)
    else:recipe=json.loads(args.recipe.read_text())
    save(compose_city(recipe) if recipe["kind"]=="city" else generate_district(recipe),args.output)


if __name__=="__main__":main()
