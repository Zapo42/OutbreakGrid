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


def district_recipe(kind,seed=214,overrides=None,*,residential_lots=False,parks=None,house_layout=None,store_layout=None,distribution_layout=None):
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
    if residential_lots:recipe["placement_mode"]="residential"
    if parks is not None:recipe["parks"]=copy.deepcopy(parks)
    if house_layout is not None:
        blueprint=lots.building_blueprint("house",house_layout)
        if recipe["category"] not in ("residential","suburban"):raise ValueError("House layouts require a residential district")
        home=max(((weight,use) for use,weight in recipe["uses"].items() if HINTS[use]=="Home"),default=None)
        if home is None:raise ValueError("District has no residential use")
        recipe.update(house_layout=house_layout,house_layout_version=lots.BUILDING_LAYOUT_VERSION,placement_mode="residential",footprint_sizes=[[blueprint["width"],blueprint["height"]]],uses={home[1]:100},storeys=[1,1])
        if overrides and "max_buildings" in overrides and settings["max_buildings"]>lots.house_layout_budget(house_layout):
            raise ValueError("Requested house count exceeds room/rest limits")
        settings["max_buildings"]=min(settings["max_buildings"],lots.house_layout_budget(house_layout))
    if store_layout is not None:
        if house_layout is not None:raise ValueError("Choose house or store layout per district")
        blueprint=lots.building_blueprint("store",store_layout)
        recipe.update(store_layout=store_layout,store_layout_version=lots.BUILDING_LAYOUT_VERSION,
            placement_mode="residential",footprint_sizes=[[blueprint["width"],blueprint["height"]]],uses={"retail":100},storeys=[1,1])
        if overrides and "max_buildings" in overrides and settings["max_buildings"]>128:raise ValueError("Store room budget exceeds 256")
        settings["max_buildings"]=min(settings["max_buildings"],128)
        # Larger stockroom templates need room for their square aligned module,
        # street reservations and setbacks. Explicit user dimensions still win.
        for key in ("block_width_cells","block_height_cells"):
            if not overrides or key not in overrides:settings[key]=max(settings[key],max(blueprint["width"],blueprint["height"])+30)
        streets.validate(settings)
    if distribution_layout is not None:
        if house_layout is not None or store_layout is not None:raise ValueError("Choose one building layout per district")
        if recipe["category"]!="business":raise ValueError("Distribution layouts require a business district")
        blueprint=lots.building_blueprint("distribution-center",distribution_layout)
        recipe.update(distribution_layout=distribution_layout,distribution_layout_version=lots.BUILDING_LAYOUT_VERSION,
            placement_mode="residential",footprint_sizes=[[blueprint["width"],blueprint["height"]]],uses={"office":100},storeys=[1,1])
        if overrides and "max_buildings" in overrides:
            if settings["max_buildings"]>128:raise ValueError("Distribution room budget exceeds 256")
        else:settings["max_buildings"]=1
        for key in ("block_width_cells","block_height_cells"):
            if not overrides or key not in overrides:settings[key]=max(settings[key],max(blueprint["width"],blueprint["height"])+30)
        streets.validate(settings)
    return recipe


def validate_recipe(recipe):
    required={"generator_version","kind","district_type","seed","name","category","street_settings", "footprint_sizes","placement_mode","lot_gap_cells","uses","storeys","reference_id","reference_profile_sha256"}
    if set(recipe)-{"parks","house_layout","house_layout_version","store_layout","store_layout_version","distribution_layout","distribution_layout_version"}!=required or recipe["generator_version"]!=VERSION or recipe["kind"]!="district":
        raise ValueError("Unsupported or incomplete district recipe")
    if not isinstance(recipe["district_type"],str) or not recipe["district_type"] or any(c not in "abcdefghijklmnopqrstuvwxyz0123456789-" for c in recipe["district_type"]):
        raise ValueError("District ids must contain only lowercase letters, digits and hyphens")
    if recipe["category"] not in ("urban","residential","suburban","business"):
        raise ValueError("Unknown district category")
    streets.validate(recipe["street_settings"])
    if type(recipe["seed"]) is not int or not 1<=recipe["seed"]<=0xffffffff or recipe["seed"]!=recipe["street_settings"]["seed"]:
        raise ValueError("Recipe and street seeds must match and be nonzero uint32 values")
    if recipe["placement_mode"] not in ("frontage","scatter","residential") or type(recipe["lot_gap_cells"]) is not int or not 0<=recipe["lot_gap_cells"]<=4:
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
    parks=recipe.get("parks",[])
    if not isinstance(parks,list) or len(parks)>6:
        raise ValueError("Request at most six parks per district")
    for park in parks:
        if not isinstance(park,dict) or set(park)!={"size","variant"} or park["size"] not in ("small","medium","large") or park["variant"] not in ("cross","loop"):
            raise ValueError("Parks require small/medium/large size and cross/loop variant")
    if ("house_layout" in recipe)!=("house_layout_version" in recipe):raise ValueError("House recipes require a layout version")
    if "house_layout" in recipe:
        if recipe["house_layout_version"]!=lots.BUILDING_LAYOUT_VERSION:raise ValueError("Unsupported house layout version")
        blueprint=lots.building_blueprint("house",recipe["house_layout"])
        if recipe["category"] not in ("residential","suburban") or recipe["placement_mode"]!="residential" or recipe["footprint_sizes"]!=[[blueprint["width"],blueprint["height"]]] or any(HINTS[use]!="Home" for use in recipe["uses"]) or recipe["storeys"]!=[1,1] or recipe["street_settings"]["max_buildings"]>lots.house_layout_budget(recipe["house_layout"]):
            raise ValueError("House layout recipe conflicts with template or room/rest budget")
    if ("store_layout" in recipe)!=("store_layout_version" in recipe):raise ValueError("Store recipes require layout version")
    if "store_layout" in recipe:
        blueprint=lots.building_blueprint("store",recipe["store_layout"])
        if "house_layout" in recipe or recipe["store_layout_version"]!=lots.BUILDING_LAYOUT_VERSION or recipe["placement_mode"]!="residential" or recipe["uses"]!={"retail":100} or recipe["storeys"]!=[1,1] or recipe["footprint_sizes"]!=[[blueprint["width"],blueprint["height"]]] or recipe["street_settings"]["max_buildings"]>128:
            raise ValueError("Store recipe conflicts with layout or room budget")
    if ("distribution_layout" in recipe)!=("distribution_layout_version" in recipe):raise ValueError("Distribution recipes require layout version")
    if "distribution_layout" in recipe:
        blueprint=lots.building_blueprint("distribution-center",recipe["distribution_layout"])
        if "house_layout" in recipe or "store_layout" in recipe or recipe["category"]!="business" or recipe["distribution_layout_version"]!=lots.BUILDING_LAYOUT_VERSION or recipe["placement_mode"]!="residential" or recipe["uses"]!={"office":100} or recipe["storeys"]!=[1,1] or recipe["footprint_sizes"]!=[[blueprint["width"],blueprint["height"]]] or recipe["street_settings"]["max_buildings"]>128:
            raise ValueError("Distribution recipe conflicts with layout or room budget")
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
    # Validate physical sidewalk connectivity separately from the road-only topology check.
    walkable={i for i,tile in enumerate(cells) if tile in (5,6)}
    reachable=set(roads);queue=deque(sorted(roads))
    while queue:
        cell=queue.popleft();x,y=cell%width,cell//width
        for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
            key=ny*width+nx
            if 0<=nx<width and 0<=ny<height and key in walkable and key not in reachable:
                reachable.add(key);queue.append(key)
    occupied=set()
    for park in metadata.get("parks",[]):
        for x,y in park["path_cells"]:
            if not (0<=x<width and 0<=y<height) or cells[y*width+x]!=5 or y*width+x not in reachable:
                raise ValueError("Park path is not connected to the street sidewalk network")
        if tuple(park["entrance"]) not in {tuple(p) for p in park["path_cells"]}:
            raise ValueError("Park entrance is missing its path")
        footprint={(x,y) for y in range(park["top"],park["top"]+park["height"]) for x in range(park["left"],park["left"]+park["width"])}
        if footprint & occupied:raise ValueError("Park lots overlap")
        occupied.update(footprint)
    for building in metadata["buildings"]:
        left,top=building["left"],building["top"];w,h=building["width"],building["height"]
        footprint={(px,py) for py in range(top,top+h) for px in range(left,left+w)}
        if footprint & occupied or any(not (0<=px<width and 0<=py<height) for px,py in footprint):
            raise ValueError("Building lot overlaps another lot or leaves map bounds")
        occupied.update(footprint)
        x,y=building["door"]
        if not (left<=x<left+w and top<=y<top+h) or (x not in (left,left+w-1) and y not in (top,top+h-1)):
            raise ValueError("Door is not on its building boundary")
        neighbours=[(x-1,y),(x+1,y),(x,y-1),(x,y+1)]
        if not any(left<px<left+w-1 and top<py<top+h-1 and cells[py*width+px]==7 for px,py in neighbours):
            raise ValueError("Door has no interior floor access")
        if not any(0<=px<width and 0<=py<height and py*width+px in reachable for px,py in neighbours):
            raise ValueError("Door has no street-connected sidewalk access")
        if cells[y*width+x]!=9 or not building["path_cells"] or any(cells[py*width+px] not in (5,6) for px,py in building["path_cells"]):
            raise ValueError("Building has no physical entrance path")
        if (building["width"]-2)*(building["height"]-2)>256:raise ValueError("Room is too large")

        layout=building.get("interior_layout",{})
        if layout.get("sales_shelves_materialized") or layout.get("warehouse_storage_materialized"):
            # Verify the actual runtime-solid furniture, not just draft approach hints.
            blueprint=layout["blueprint"];blocked={(p["x"],p["y"]) for key in ("shelves","storageBoxes","phoneBlocks") for p in document.get(key,[])}
            reserved={tuple(p) for p in layout.get("reserved_fixture_cells",[])}
            blocked|=reserved
            floors={(left+x,top+y) for r in blueprint["rooms"] for x,y in r["cells"]}-reserved
            doors={(left+d["cell"][0],top+d["cell"][1]) for d in blueprint["doors"]}
            if any(cells[py*width+px]!=7 for px,py in floors) or any(cells[py*width+px]!=9 for px,py in doors):raise ValueError("Store room/door terrain differs from its layout")
            if any(cells[py*width+px]!=8 for px,py in reserved):raise ValueError("Reserved computer footprint is not solid terrain")
            if layout.get("warehouse_storage_materialized"):
                expected_reserved={(left+px,top+py) for f in blueprint["fixtures"] if f["role"]=="order-computer" for px,py in f["cells"]}
                computer=layout["order_computer"]
                if reserved!=expected_reserved or {tuple(computer["cell"])}!=reserved or computer["id"]!=layout["instance_id"]+"/order-computer" or computer["interaction_implemented"] is not False:raise ValueError("Distribution computer geometry/identity is inconsistent")
                expected_approaches=[[left+x,top+y] for f in blueprint["fixtures"] if f["role"]=="order-computer" for x,y in f["approach_cells"]]
                if computer["approach_cells"]!=expected_approaches:raise ValueError("Computer approach differs from dispatch template")
            actual=[(p["x"],p["y"]) for key in ("shelves","storageBoxes","phoneBlocks") for p in document.get(key,[]) if (p["x"],p["y"]) in footprint]
            if len(actual)!=len(set(actual)) or any(p not in floors for p in actual):raise ValueError("Store fixtures overlap or leave floor")
            expected_shelves={(left+px,top+py) for f in blueprint["fixtures"] if f["role"]=="sales-shelf" for px,py in f["cells"]}
            for key,expected in (("shelves",expected_shelves),("storageBoxes",{tuple(layout["stock_box_cells"])}),("phoneBlocks",{tuple(layout["phone_cells"])} if "phone_cells" in layout else set())):
                if {(p["x"],p["y"]) for p in document.get(key,[]) if (p["x"],p["y"]) in footprint}!=expected:raise ValueError("Store live fixture records are missing or misplaced")
            allowed=(floors|doors)-blocked;start=(x,y);inside={start};pending=deque([start])
            while pending:
                px,py=pending.popleft()
                for point in ((px-1,py),(px+1,py),(px,py-1),(px,py+1)):
                    if point in allowed and point not in inside:inside.add(point);pending.append(point)
            if allowed!=inside:raise ValueError("Store fixtures disconnect interior circulation")
            for entry in blueprint["doors"]:
                if not entry["external"]:continue
                ex,ey=left+entry["cell"][0],top+entry["cell"][1]
                if not any(0<=nx<width and 0<=ny<height and ny*width+nx in reachable for nx,ny in ((ex-1,ey),(ex+1,ey),(ex,ey-1),(ex,ey+1))):
                    raise ValueError("Store entrance lacks connected exterior access")
            for fixture in blueprint["fixtures"]:
                if not any((left+ax,top+ay) in inside for ax,ay in fixture["approach_cells"]):raise ValueError("Store shelf/stock approach blocked")
            for key in (("stock_box_cells","phone_cells") if "phone_cells" in layout else ("stock_box_cells",)):
                fx,fy=layout[key]
                if not any(p in inside for p in ((fx-1,fy),(fx+1,fy),(fx,fy-1),(fx,fy+1))):raise ValueError("Store fixture has no interaction access")


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
        if "interior_layout" in b:
            # Colour only real floor cells so internal walls and doors remain readable.
            for y in range(b["top"]+1,b["top"]+b["height"]-1):
                for x in range(b["left"]+1,b["left"]+b["width"]-1):
                    if cells[y*width+x]==7:parts.append(f'<rect x="{x*scale}" y="{y*scale}" width="{scale}" height="{scale}" fill="{COLOURS[b["use_hint"]]}"/>')
        else:
            parts.append(f'<rect x="{(b["left"]+1)*scale}" y="{(b["top"]+1)*scale}" width="{(b["width"]-2)*scale}" height="{(b["height"]-2)*scale}" fill="{COLOURS[b["use_hint"]]}"/>')
    for spot in document.get("restSpots",[]):
        parts.append(f'<circle cx="{(spot["x"]+.5)*scale}" cy="{(spot["y"]+.5)*scale}" r="{scale*.12}" fill="#31985a"/>')
    for storage in document.get("storageBoxes",[]):
        parts.append(f'<rect x="{(storage["x"]+.15)*scale}" y="{(storage["y"]+.15)*scale}" width="{scale*.7}" height="{scale*.7}" fill="#9a673e"/>')
    for shelf in document.get("shelves",[]):
        parts.append(f'<rect x="{(shelf["x"]+.2)*scale}" y="{(shelf["y"]+.2)*scale}" width="{scale*.6}" height="{scale*.6}" fill="#b83b38"/>')
    for building in metadata["buildings"]:
        computer=building.get("interior_layout",{}).get("order_computer")
        if computer:
            x,y=computer["cell"]
            parts.append(f'<rect x="{x*scale}" y="{y*scale}" width="{scale}" height="{scale}" fill="#323d60"><title>Reserved order-computer footprint; interaction pending</title></rect>')
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
        placement_mode=recipe["placement_mode"],lot_gap_cells=recipe["lot_gap_cells"],fair_block_budget=True,park_specs=recipe.get("parks"))
    rng=lots.StableRandom(streams["uses"])
    uses=sorted(recipe["uses"].items());total=sum(weight for use,weight in uses)
    for index,b in enumerate(metadata["buildings"]):
        roll=rng.randrange(total)
        for use,weight in uses:
            roll-=weight
            if roll<0:break
        b.update(parcel_id=f'{recipe["district_type"]}-{index+1}',district_type=recipe["district_type"],use_hint=use,
                 game_building_use_hint=HINTS[use],storeys_hint=rng.randint(*recipe["storeys"]))
        if "distribution_layout" in recipe:b["game_building_use_hint"]="DistributionCenter"
    if "house_layout" in recipe or "store_layout" in recipe or "distribution_layout" in recipe:
        # The original street raster remains the entrance target even when other houses have paths.
        backbone=[(g,p) for g,p in projected if p["category"] in ("road","water")]
        corridor=atlas.export_map({"id":"house-streets","name":"House streets"},backbone,grid,provenance,settings["road_width_cells"],settings["sidewalk_width_cells"])
        targets={(i%grid["width"],i//grid["width"]) for i,t in enumerate(lots.unpack(corridor)) if t in (5,6)}
        if "house_layout" in recipe:lots.materialize_house_layouts(document,metadata,recipe["house_layout"],targets)
        elif "store_layout" in recipe:lots.materialize_store_layouts(document,metadata,recipe["store_layout"],targets)
        else:lots.materialize_distribution_layouts(document,metadata,recipe["distribution_layout"],targets)
    fingerprint=digest(recipe)[:12]
    metadata.update(generator_version=VERSION,seed=recipe["seed"],stream_seeds=streams,recipe=copy.deepcopy(recipe),recipe_id=fingerprint,
                    district_type=recipe["district_type"],category=recipe["category"],
                    scope="Fictional seeded district with parcel/use/height hints. No assigned jobs, inhabitants, inventory, upper-floor simulation or directed traffic lanes.")
    if "house_layout" in recipe:metadata["scope"]="Seeded multi-room homes with explicit building identity, empty finite storage and living-area rest spots; no population or NPC decision changes."
    if "store_layout" in recipe:metadata["scope"]="Live two-room store geometry with empty shelves, finite stockroom storage and phone; configure store/staff through existing World APIs."
    if "distribution_layout" in recipe:metadata["scope"]="Live dispatch/warehouse geometry, finite empty DC storage and reserved solid computer position; computer interactions and NPC orders unchanged."
    if open_space_recovery is not None:metadata["open_space_recovery"]=open_space_recovery
    metadata["street_segments"]=[{"points_metres":list(g.coords),"width_cells":p["width_cells"],**p["tags"]} for g,p in projected if p["category"]=="road"]
    metadata["actual_building_coverage"]=round(sum(b["width"]*b["height"] for b in metadata["buildings"])/(document["width"]*document["height"]),6)
    metadata["use_counts"]=dict(Counter(b["use_hint"] for b in metadata["buildings"]))
    document.update(id=recipe["district_type"]+"-"+str(recipe["seed"])+"-"+fingerprint,name=recipe["name"]+" (seed "+str(recipe["seed"])+")")
    document["authoringJson"]=json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)
    assert_usable(document,metadata)
    return document,metadata,render_map(document,metadata)


def city_recipe(kinds,seed=214,columns=2,gap=12,cell_metres=5,house_layout=None,store_layout=None,distribution_layout=None):
    if not 1<=len(kinds)<=9 or type(columns) is not int or not 1<=columns<=3 or not 6<=gap<=30:
        raise ValueError("Compose 1–9 districts in 1–3 columns with 6–30 cells between slots")
    if type(seed) is not int or not 1<=seed<=0xffffffff:raise ValueError("Invalid city seed")
    rng=lots.StableRandom(stream_seed(seed,"city-districts"))
    budget=256//(len(kinds)*(2 if house_layout or store_layout or distribution_layout else 1))
    if house_layout:budget=min(budget,lots.house_layout_budget(house_layout)//len(kinds))
    districts=[];distribution_placed=False
    for kind in kinds:
        child=district_recipe(kind,rng.word() or 1,{"cell_metres":cell_metres,"max_buildings":min(budget,64)})
        if house_layout and child["category"] in ("residential","suburban"):
            child=district_recipe(kind,child["seed"],{"cell_metres":cell_metres,"max_buildings":min(budget,64)},house_layout=house_layout)
        if distribution_layout and not distribution_placed and child["category"]=="business":
            child=district_recipe(kind,child["seed"],{"cell_metres":cell_metres,"max_buildings":1},distribution_layout=distribution_layout)
            distribution_placed=True
        elif store_layout and child["category"] in ("urban","business"):
            child=district_recipe(kind,child["seed"],{"cell_metres":cell_metres,"max_buildings":min(budget,64)},store_layout=store_layout)
        districts.append(child)
    if distribution_layout and not distribution_placed:raise ValueError("City distribution layout needs a business district")
    return {"generator_version":VERSION,"kind":"city","seed":seed,"columns":columns,"gap_cells":gap,"districts":districts}


def compose_city(recipe):
    if set(recipe)!={"generator_version","kind","seed","columns","gap_cells","districts"} or recipe["generator_version"]!=VERSION or recipe["kind"]!="city":
        raise ValueError("Unsupported city recipe")
    districts=recipe["districts"];columns=recipe["columns"];gap=recipe["gap_cells"]
    if type(recipe["seed"]) is not int or not 1<=recipe["seed"]<=0xffffffff or not 1<=len(districts)<=9 or type(columns) is not int or not 1<=columns<=3 or type(gap) is not int or not 6<=gap<=30:
        raise ValueError("Invalid city seed, district count or slot layout")
    if sum(r["street_settings"]["max_buildings"] for r in districts)>256:raise ValueError("City building budgets exceed 256")
    if sum(r["street_settings"]["max_buildings"]*(2 if "house_layout" in r or "store_layout" in r or "distribution_layout" in r else 1) for r in districts)>256:
        raise ValueError("City physical room budgets exceed 256")
    if sum(r["street_settings"]["max_buildings"]*len(lots.building_blueprint("house",r["house_layout"])["recovery_area"]["cells"]) for r in districts if "house_layout" in r)>4096:
        raise ValueError("City living-area rest budget exceeds 4096")
    scales={r["street_settings"]["cell_metres"] for r in districts}
    if len(scales)!=1:raise ValueError("All districts must use the same metres-per-cell scale")
    scale=scales.pop();generated=[generate_district(r) for r in districts]
    slot_width=max(d["width"] for d,m,s in generated);slot_height=max(d["height"] for d,m,s in generated)
    rows=math.ceil(len(districts)/columns);margin=4
    width=margin*2+columns*slot_width+(columns-1)*gap;height=margin*2+rows*slot_height+(rows-1)*gap
    if width*height>atlas.MAX_CELLS:raise ValueError("Composed map exceeds the one-million-cell limit")
    cells=bytearray(width*height);placements=[];buildings=[];segments=[];parks=[];omitted_parks=[];groups=[];storage=[];rest=[];shelves=[];phones=[]
    for i,(document,metadata,_) in enumerate(generated):
        ox=margin+(i%columns)*(slot_width+gap);oy=margin+(i//columns)*(slot_height+gap)
        tile=lots.unpack(document)
        for y in range(document["height"]):
            cells[(oy+y)*width+ox:(oy+y)*width+ox+document["width"]]=tile[y*document["width"]:(y+1)*document["width"]]
        for b in metadata["buildings"]:
            moved=copy.deepcopy(b);moved.update(left=b["left"]+ox,top=b["top"]+oy,door=[b["door"][0]+ox,b["door"][1]+oy],
                path_cells=[[x+ox,y+oy] for x,y in b["path_cells"]],district_index=i,parcel_id=f'd{i}-'+b["parcel_id"])
            if "interior_layout" in moved:
                layout=moved["interior_layout"];lx,ly=layout["origin_cells"]
                layout.update(origin_cells=[lx+ox,ly+oy],instance_id=moved["parcel_id"],room_keys={k:moved["parcel_id"]+"/"+k for k in layout["room_keys"]})
                for key in ("stock_box_cells","phone_cells"):
                    if key in layout:layout[key]=[layout[key][0]+ox,layout[key][1]+oy]
                if "order_computer" in layout:
                    computer=layout["order_computer"];computer["id"]=moved["parcel_id"]+"/order-computer"
                    computer["cell"]=[computer["cell"][0]+ox,computer["cell"][1]+oy]
                    computer["approach_cells"]=[[x+ox,y+oy] for x,y in computer["approach_cells"]]
                    layout["reserved_fixture_cells"]=[[x+ox,y+oy] for x,y in layout["reserved_fixture_cells"]]
                moved["retained_walk_cells"]=[[x+ox,y+oy] for x,y in b["retained_walk_cells"]]
            buildings.append(moved)
        for group in document.get("buildingRooms",[]):
            moved=copy.deepcopy(group);moved["id"]=f'd{i}-'+group["id"]
            for room in moved["rooms"]:room["x"]+=ox;room["y"]+=oy
            groups.append(moved)
        for key,destination in (("storageBoxes",storage),("restSpots",rest),("shelves",shelves),("phoneBlocks",phones)):
            for item in document.get(key,[]):
                moved=copy.deepcopy(item);moved["x"]+=ox;moved["y"]+=oy;destination.append(moved)
        for park in metadata.get("parks",[]):
            moved=copy.deepcopy(park)
            moved.update(left=park["left"]+ox,top=park["top"]+oy,park_id=f'd{i}-'+park["park_id"],district_index=i,
                         entrance=[park["entrance"][0]+ox,park["entrance"][1]+oy],
                         path_cells=[[x+ox,y+oy] for x,y in park["path_cells"]])
            parks.append(moved)
        omitted_parks.extend(dict(p,district_index=i) for p in metadata.get("omitted_parks",[]))
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
    if any("parks" in r for r in districts):metadata.update(parks=parks,omitted_parks=omitted_parks)
    document={"kind":"outbreak.map","schemaVersion":1,"id":f'mixed-city-{recipe["seed"]}-{identity}',"name":f'Mixed district city (seed {recipe["seed"]})',
              "width":width,"height":height,"chunkSize":32,"chunks":[],"authoringJson":json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)}
    if groups:
        document.update(schemaVersion=14,buildingRooms=groups,storageBoxes=sorted(storage,key=lambda p:(p["y"],p["x"])),restSpots=sorted(rest,key=lambda p:(p["y"],p["x"])))
        if shelves:document["shelves"]=sorted(shelves,key=lambda p:(p["y"],p["x"]))
        if phones:document["phoneBlocks"]=sorted(phones,key=lambda p:(p["y"],p["x"]))
        metadata.update(expected_building_count=len(buildings),expected_physical_room_count=len(buildings)+len(groups),active_features=["building-room-membership","living-area-rest-spots","household-storage"])
        if shelves or any("distribution_layout" in r for r in districts):metadata["active_features"]=sorted({feature for _,child,_ in generated for feature in child.get("active_features",[])})
        document["authoringJson"]=json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)
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


def building_preview_map(blueprint,cell_metres=5):
    """Export existing terrain only; fixture and mechanic proposals remain opaque authoring data."""
    lots.validate_building_blueprint(blueprint)
    if not isinstance(cell_metres,(int,float)) or isinstance(cell_metres,bool) or not math.isfinite(cell_metres) or cell_metres<=0:
        raise ValueError("Preview cell scale must be positive and finite")
    margin=4;width=blueprint["width"]+margin*2;height=blueprint["height"]+margin*2
    cells=bytearray(width*height)
    # Connected perimeter road and sidewalk make each external doorway testable in isolation.
    for y in range(1,height-1):
        for x in range(1,width-1):
            if x in (1,width-2) or y in (1,height-2):cells[y*width+x]=6
            elif x in (2,width-3) or y in (2,height-3):cells[y*width+x]=5
    for room in blueprint["rooms"]:
        for x,y in room["cells"]:cells[(y+margin)*width+x+margin]=7
    for x,y in blueprint["walls"]:cells[(y+margin)*width+x+margin]=8
    for door in blueprint["doors"]:
        x,y=door["cell"];cells[(y+margin)*width+x+margin]=9
        if not door["external"]:continue
        dx=-1 if x==0 else 1 if x==blueprint["width"]-1 else 0
        dy=-1 if y==0 else 1 if y==blueprint["height"]-1 else 0
        px,py=x+margin+dx,y+margin+dy
        while cells[py*width+px]==0:
            cells[py*width+px]=5;px+=dx;py+=dy
        if cells[py*width+px] not in (5,6):raise ValueError("Preview entrance missed its sidewalk")
    grid={"width":width,"height":height,"cell_metres":cell_metres,"north_up":True,"projection":"fictional local metric grid"}
    metadata={"generator_version":lots.BUILDING_LAYOUT_VERSION,"grid":grid,"origin_cells":[margin,margin],
              "blueprint":copy.deepcopy(blueprint),"expected_room_count":len(blueprint["rooms"]),
              "fixtures_materialized":False,"mechanics_enabled":False,
              "provenance":{"attribution":"Fictional building geometry for Lead integration review."},
              "scope":"Terrain shell with real partitions/doors. Fixtures and interaction sites are draft authoring hints, not active game objects."}
    document={"kind":"outbreak.map","schemaVersion":1,"id":"blueprint-"+blueprint["id"],"name":"Building geometry review: "+blueprint["id"],
              "width":width,"height":height,"chunkSize":32,"chunks":[],"authoringJson":json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)}
    lots.pack(document,cells)
    return document,metadata


def building_blueprint_svg(document,metadata):
    """Technical plan overlay, separate from game art and runtime fixture materialization."""
    from html import escape
    blueprint=metadata["blueprint"];ox,oy=metadata["origin_cells"];width,height=document["width"],document["height"]
    unit=28;cells=lots.unpack(document)
    palette={0:"#eef3e8",5:"#c1c9c4",6:"#938f83",7:"#fff9e9",8:"#35454c",9:"#edab65"}
    parts=[f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {max(800,width*unit)} {(height+6)*unit}" role="img" aria-label="{escape(blueprint["id"])} technical layout">',
           '<rect width="100%" height="100%" fill="white"/>']
    for y in range(height):
        for x in range(width):parts.append(f'<rect x="{x*unit}" y="{y*unit}" width="{unit}" height="{unit}" fill="{palette[cells[y*width+x]]}" stroke="#ffffff" stroke-width=".5"/>')
    for room in blueprint["rooms"]:
        x,y=min(room["cells"],key=lambda p:(p[1],p[0]))
        parts.append(f'<text x="{(x+ox)*unit+2}" y="{(y+oy)*unit+10}" font-size="9" font-family="sans-serif" fill="#243b45">{escape(room["role"])}</text>')
    for fixture in blueprint["fixtures"]:
        short={"order-computer":"PC","service-counter":"PASS","sales-shelf":"SH","storage-shelf":"ST","seating":"SOFA","dining-table":"TBL","bar-counter":"BAR","cooking-station":"COOK","pew":"PEW","altar":"ALT"}[fixture["role"]]
        for x,y in fixture["cells"]:
            px,py=(x+ox)*unit,(y+oy)*unit
            parts.append(f'<rect x="{px+2}" y="{py+2}" width="{unit-4}" height="{unit-4}" fill="#6d8ca0"><title>{escape(fixture["id"]+": "+fixture["role"])}</title></rect>')
            parts.append(f'<text x="{px+unit/2}" y="{py+17}" text-anchor="middle" font-size="7" font-family="sans-serif" fill="white">{short}</text>')
        for x,y in fixture["approach_cells"]:
            parts.append(f'<circle cx="{(x+ox+.5)*unit}" cy="{(y+oy+.5)*unit}" r="3" fill="#39a55c"/>')
    for y,text in enumerate((blueprint["id"],"Technical plan: orange doors; blue proposed fixtures; green interaction approaches.",
                              "SH sales shelf | ST storage | PC order computer | PASS kitchen service counter",
                              "Map shell contains terrain only. Fixtures, recovery and order mechanics await Lead/NPC integration.",
                              "Units: simulation cells. Draft room/fixture IDs are not persisted gameplay identities.")):
        parts.append(f'<text x="12" y="{(height+1+y)*unit}" font-family="sans-serif" font-size="11" fill="#243b45">{escape(text)}</text>')
    parts.append('</svg>')
    return ''.join(parts)


def write_building_pack(output,seed=214):
    """Write a deterministic review pack without changing bundled city recipes or maps."""
    if type(seed) is not int or not 1<=seed<=0xffffffff:raise ValueError("Seed must be a nonzero uint32")
    output=Path(output);output.mkdir(parents=True,exist_ok=True)
    rng=lots.StableRandom(stream_seed(seed,"building-layouts"));entries=[]
    for kind in lots.BUILDING_TYPES:
        for variant in lots.BUILDING_VARIANTS:
            blueprint=lots.building_blueprint(kind,variant,rng.randrange(4))
            document,metadata=building_preview_map(blueprint);stem=blueprint["id"]
            atlas.write_json(output/(stem+".blueprint.json"),blueprint)
            atlas.write_json(output/(stem+".obmap2"),document)
            (output/(stem+".svg")).write_text(building_blueprint_svg(document,metadata),encoding="utf-8")
            entries.append({"id":stem,"type":kind,"variant":variant,"blueprint":stem+".blueprint.json","map":stem+".obmap2","plan":stem+".svg"})
    manifest={"layout_version":lots.BUILDING_LAYOUT_VERSION,"seed":seed,"templates":entries,
              "status":"Geometry review pack; no runtime fixture/behavior integration",
              "lead_decisions":["Group multiple rooms under one building/home identity without changing old saves",
                                "Approve room recovery semantics and rates with NPC coder",
                                "Map proposed sales/storage fixtures to finite-inventory game objects",
                                "Agree order-computer visit/claim protocol with NPC coder",
                                "Agree cook/waiter counter transfer protocol and fixture collision/visibility"]}
    atlas.write_json(output/"manifest.json",manifest)
    # Plain technical documents, not production UI or replacement graphics assets.
    links=''.join(f'<li><a href="{entry["plan"]}">{entry["id"]}</a> | <a href="{entry["blueprint"]}">blueprint</a> | <a href="{entry["map"]}">terrain shell</a></li>' for entry in entries)
    (output/"index.html").write_text('<!doctype html><meta charset="utf-8"><title>Building geometry review pack</title><h1>Building geometry review pack</h1><p>Ten reusable local-cell layouts. Blueprints and technical plans include proposed fixture sites. OBMAP2 files contain walls, floors, doors and sidewalks only; fixtures and mechanics await integration.</p><ul>'+links+'</ul>',encoding="utf-8")
    return manifest


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output",type=Path,default=atlas.ROOT/"CityReferences/districts/maps")
    sub=parser.add_subparsers(dest="command",required=True)
    district=sub.add_parser("district");district.add_argument("--type",required=True);district.add_argument("--seed",type=int,default=214)
    district.add_argument("--house-layout",choices=lots.BUILDING_VARIANTS,help="Place live two-room houses with living rest areas and storage")
    district.add_argument("--store-layout",choices=lots.BUILDING_VARIANTS,help="Place live sales floors and separate stockrooms")
    district.add_argument("--distribution-layout",choices=lots.BUILDING_VARIANTS,help="Place a dispatch office and warehouse with reserved computer position")
    district.add_argument("--residential-lots",action="store_true",help="Use aligned residential lot modules")
    district.add_argument("--park",action="append",default=[],metavar="SIZE:VARIANT",help="Repeat up to six times; sizes small/medium/large, variants cross/loop")
    district.add_argument("--block-width-cells",type=int);district.add_argument("--block-height-cells",type=int)
    district.add_argument("--coverage",type=float);district.add_argument("--blocks-across",type=int);district.add_argument("--blocks-down",type=int)
    city=sub.add_parser("city");city.add_argument("--districts",nargs='+',default=["dense-urban","business-core","terrace-residential","garden-suburb"]);city.add_argument("--seed",type=int,default=214);city.add_argument("--city-columns",type=int,default=2)
    city.add_argument("--house-layout",choices=lots.BUILDING_VARIANTS)
    city.add_argument("--store-layout",choices=lots.BUILDING_VARIANTS)
    city.add_argument("--distribution-layout",choices=lots.BUILDING_VARIANTS,help="Reserve the first business district for one DC")
    pack=sub.add_parser("building-pack",help="Export draft multi-room templates and technical map shells")
    pack.add_argument("--directory",type=Path,required=True);pack.add_argument("--seed",type=int,default=214)
    replay=sub.add_parser("replay");replay.add_argument("recipe",type=Path)
    args=parser.parse_args()
    if args.command=="building-pack":
        manifest=write_building_pack(args.directory,args.seed)
        print(f'Wrote {len(manifest["templates"])} draft building templates to {args.directory}')
        return
    if args.command=="district":
        overrides={name:value for name,value in [("building_coverage",args.coverage),("columns",args.blocks_across),("rows",args.blocks_down),("block_width_cells",args.block_width_cells),("block_height_cells",args.block_height_cells)] if value is not None}
        parks=[]
        for value in args.park:
            parts=value.split(":")
            if len(parts)!=2:parser.error("--park requires SIZE:VARIANT")
            parks.append({"size":parts[0],"variant":parts[1]})
        recipe=district_recipe(args.type,args.seed,overrides,residential_lots=args.residential_lots,parks=parks if args.park else None,house_layout=args.house_layout,store_layout=args.store_layout,distribution_layout=args.distribution_layout)
    elif args.command=="city":recipe=city_recipe(args.districts,args.seed,args.city_columns,house_layout=args.house_layout,store_layout=args.store_layout,distribution_layout=args.distribution_layout)
    else:recipe=json.loads(args.recipe.read_text())
    save(compose_city(recipe) if recipe["kind"]=="city" else generate_district(recipe),args.output)


if __name__=="__main__":main()
