"""Generate fictional building lots inside a surveyed street layout; preserve attribution and scale.

This first map-generation stage reuses the surveyed roads rather than synthesizing a new
street graph. Building footprints and entrance paths are newly generated from a seed.
"""
import argparse
from collections import deque
import json
import math
from pathlib import Path
from shapely.affinity import rotate, translate

from shapely.geometry import box

import city_atlas as atlas

GENERATOR_VERSION = "cardinal-city-2"


class StableRandom:
    """Versioned xorshift32 stream with unbiased integer sampling, independent of Python's RNG."""
    def __init__(self, seed):
        self.state = seed & 0xffffffff or 0x6D2B79F5

    def word(self):
        value = self.state
        value ^= (value << 13) & 0xffffffff
        value ^= value >> 17
        value ^= (value << 5) & 0xffffffff
        self.state = value & 0xffffffff
        return self.state

    def randrange(self, stop):
        if stop < 1 or stop > 0x100000000:
            raise ValueError("Random range must fit an unsigned 32-bit word")
        limit = 0x100000000 - 0x100000000 % stop
        value = self.word()
        while value >= limit:
            value = self.word()
        return value % stop

    def randint(self, low, high):
        return low + self.randrange(high - low + 1)

    def choice(self, items):
        return items[self.randrange(len(items))]

    def fraction(self):
        return self.word() / 0x100000000


def align_cardinal(projected, grid):
    """Rotate the dominant perpendicular street directions onto the game axes, without distorting topology."""
    cosine = sine = length = 0.0
    for geometry, props in projected:
        if props["category"] != "road":
            continue
        lines = geometry.geoms if geometry.geom_type == "MultiLineString" else [geometry]
        for line in lines:
            if line.geom_type != "LineString":
                continue
            coords = list(line.coords)
            for first, second in zip(coords, coords[1:]):
                distance = math.dist(first, second)
                angle = math.atan2(second[1]-first[1], second[0]-first[0])
                cosine += distance*math.cos(4*angle)
                sine += distance*math.sin(4*angle)
                length += distance
    if not length:
        raise ValueError("No street directions to align")
    degrees = math.degrees(math.atan2(sine, cosine)/4)
    rotated = [(rotate(g, -degrees, origin=(0, 0)), p) for g,p in projected]
    left = min(g.bounds[0] for g,p in rotated)
    top = min(g.bounds[1] for g,p in rotated)
    scale = grid["cell_metres"]
    shift_x, shift_y = scale-left, scale-top
    result = [(translate(g, shift_x, shift_y), p) for g,p in rotated]
    width = math.ceil(max(g.bounds[2] for g,p in result)/scale)+1
    height = math.ceil(max(g.bounds[3] for g,p in result)/scale)+1
    if width*height > atlas.MAX_CELLS:
        raise ValueError("Aligned map exceeds the one-million-cell limit")
    aligned = dict(grid, width=width, height=height, north_up=False,
                   reference_grid=grid, rotation_degrees=-degrees,
                   alignment_translation_metres=[shift_x, shift_y],
                   orientation="Dominant surveyed street axes aligned to game east/west and north/south; geographic north is rotated.")
    return result, aligned


def unpack(document):
    width,height=document["width"],document["height"]
    cells=bytearray(width*height)
    for chunk in document["chunks"]:
        for i,tile in enumerate(chunk["cells"]):
            x,y=chunk["x"]*32+i%32,chunk["y"]*32+i//32
            if x<width and y<height:
                cells[y*width+x]=tile
    return cells


def pack(document,cells):
    width,height=document["width"],document["height"]
    chunks=[]
    for cy in range((height+31)//32):
        for cx in range((width+31)//32):
            values=[cells[y*width+x] if x<width and y<height else 0
                    for y in range(cy*32,cy*32+32) for x in range(cx*32,cx*32+32)]
            if any(values):chunks.append({"x":cx,"y":cy,"cells":values})
    document["chunks"]=chunks


def entrance_path(cells,width,height,left,top,w,h,target_cells=None):
    """Four candidate doors; bounded multi-source BFS chooses a physical path to a sidewalk."""
    doors=[(left+w//2,top,0,-1),(left+w//2,top+h-1,0,1),
           (left,top+h//2,-1,0),(left+w-1,top+h//2,1,0)]
    queue=deque();parents={};origins={}
    for door in doors:
        x,y,dx,dy=door;outside=(x+dx,y+dy)
        if 0<=outside[0]<width and 0<=outside[1]<height and cells[outside[1]*width+outside[0]] in (0,5,6):
            queue.append(outside);parents[outside]=None;origins[outside]=door
    while queue and len(parents)<=10000:
        position=queue.popleft();x,y=position
        if cells[y*width+x] in (5,6) and (target_cells is None or position in target_cells):
            path=[];door=origins[position]
            while position is not None:
                path.append(position);position=parents[position]
            return door,path
        for dx,dy in ((0,-1),(1,0),(0,1),(-1,0)):
            nxt=(x+dx,y+dy);nx,ny=nxt
            if not (0<=nx<width and 0<=ny<height) or nxt in parents:
                continue
            if left<=nx<left+w and top<=ny<top+h or cells[ny*width+nx] not in (0,5,6):
                continue
            parents[nxt]=position;origins[nxt]=origins[(x,y)];queue.append(nxt)
    return None


def generate(sample,features,provenance,seed,cell_metres=5,max_buildings=128,orientation="geographic"):
    projected,grid,metrics=atlas.survey(features,sample["bbox"],cell_metres)
    if orientation=="cardinal":
        projected,grid=align_cardinal(projected,grid)
    elif orientation!="geographic":
        raise ValueError("Orientation must be cardinal or geographic")
    document,metadata,artwork=generate_on_layout(sample,projected,grid,metrics,provenance,seed,max_buildings)
    if orientation=="cardinal":
        document["id"]+="-cardinal"
        document["name"]="Cardinal-aligned "+document["name"]
    return document,metadata,artwork


def place_parks(cells,width,height,blocks,cell_metres,specs):
    """Reserve park lots before houses; use only existing grass and sidewalk terrain."""
    sizes={"small":6,"medium":10,"large":16}
    occupied=set();parks=[];omitted=[]
    for index,spec in enumerate(specs):
        edge=sizes[spec["size"]];chosen=None
        # Separate parks use separate blocks. A requested park that cannot fit is reported.
        for block_index,block in enumerate(blocks):
            if any(p["block"]==block_index for p in parks):continue
            left=math.floor(block.centroid.x/cell_metres-edge/2)
            top=math.floor(block.centroid.y/cell_metres-edge/2)
            footprint={(x,y) for y in range(top,top+edge) for x in range(left,left+edge)}
            if not block.buffer(-cell_metres).covers(box(left*cell_metres,top*cell_metres,(left+edge)*cell_metres,(top+edge)*cell_metres)):continue
            if any(not (0<=x<width and 0<=y<height) or cells[y*width+x]!=0 or (x,y) in occupied for x,y in footprint):continue
            connection=entrance_path(cells,width,height,left,top,edge,edge)
            if connection is None:continue
            door,access=connection;cx=left+edge//2;cy=top+edge//2
            # Both variants connect all four sides. The loop adds a continuous internal circuit.
            paths={(cx,y) for y in range(top,top+edge)}|{(x,cy) for x in range(left,left+edge)}
            if spec["variant"]=="loop":
                paths|={(x,y) for x in range(left+1,left+edge-1) for y in (top+1,top+edge-2)}
                paths|={(x,y) for y in range(top+1,top+edge-1) for x in (left+1,left+edge-2)}
            paths.update(access)
            for x,y in paths:
                if cells[y*width+x]==0:cells[y*width+x]=5
            occupied.update(footprint)
            chosen={"park_id":f"park-{index+1}","size":spec["size"],"variant":spec["variant"],
                    "left":left,"top":top,"width":edge,"height":edge,"block":block_index,
                    "entrance":[door[0],door[1]],"path_cells":[list(v) for v in sorted(paths)]}
            parks.append(chosen);break
        if chosen is None:omitted.append({"request_index":index,"size":spec["size"],"variant":spec["variant"],"reason":"No unused block fits a connected park lot"})
    return occupied,parks,omitted


# These templates describe geometry and proposed fixture sites only. They do not
# create new gameplay object types or change the game's room/home save contracts.
BUILDING_LAYOUT_VERSION="building-layouts-draft-1"
BUILDING_TYPES=("house","store","distribution-center","restaurant-bar","church")
BUILDING_VARIANTS=("compact","spacious")


def building_blueprint(kind,variant="compact",rotation=0):
    """Return a local-cell, rotatable building layout for Lead/NPC integration review."""
    if kind not in BUILDING_TYPES or variant not in BUILDING_VARIANTS:
        raise ValueError("Unknown building type or layout variant")
    if type(rotation) is not int or rotation not in range(4):
        raise ValueError("Rotation must be 0, 1, 2 or 3 clockwise quarter turns")
    dimensions={"house":((10,9),(12,11)),"store":((14,12),(18,14)),
                "distribution-center":((16,12),(18,14)),
                "restaurant-bar":((18,14),(20,16)),"church":((16,14),(18,16))}
    width,height=dimensions[kind][BUILDING_VARIANTS.index(variant)]
    walls={(x,y) for y in range(height) for x in range(width)
           if x in (0,width-1) or y in (0,height-1)}
    rooms=[];doors=[];fixtures=[];interactions=[]

    def room(identity,role,left,top,w,h):
        rooms.append({"id":identity,"role":role,"cells":[[x,y] for y in range(top,top+h) for x in range(left,left+w)]})

    def door(identity,x,y,a,b):
        walls.discard((x,y));doors.append({"id":identity,"cell":[x,y],"connects":[a,b],"external":b=="outside"})

    def fixture(identity,role,cells,approaches,room_id):
        fixtures.append({"id":identity,"role":role,"room_id":room_id,"cells":[list(p) for p in cells],
                         "approach_cells":[list(p) for p in approaches],"blocks_movement":True,
                         "status":"proposed fixture; no game object instantiated"})

    if kind=="house":
        split=width-4
        walls.update((split,y) for y in range(1,height-1))
        room("living","living-area",1,1,split-1,height-2)
        room("storage","household-storage",split+1,1,width-split-2,height-2)
        door("front",3,height-1,"living","outside")
        door("storage-door",split,height//2,"living","storage")
        fixture("seating","seating",[(2,2),(3,2)],[(2,3),(3,3)],"living")
        fixture("home-storage","storage-shelf",[(width-2,2)],[(width-3,2)],"storage")
    else:
        split=height-5
        walls.update((x,split) for x in range(1,width-1))
        front_role={"store":"sales-floor","distribution-center":"dispatch-office",
                    "restaurant-bar":"dining-bar","church":"sanctuary"}[kind]
        rear_role={"store":"stockroom","distribution-center":"warehouse",
                   "restaurant-bar":"kitchen","church":"vestry-storage"}[kind]
        room("front",front_role,1,1,width-2,split-1)
        room("rear",rear_role,1,split+1,width-2,height-split-2)
        door("public-entrance",width//2,0,"front","outside")
        door("internal-door",width-3,split,"front","rear")
        if kind!="church":door("service-entrance",width-1,height-3,"rear","outside")
        if kind=="store":
            for y in (3,5):
                for x in range(3,width-3,3):
                    fixture(f"sales-shelf-{x}-{y}","sales-shelf",[(x,y)],[(x+1,y)],"front")
            fixture("store-stock","storage-shelf",[(x,height-2) for x in range(2,5)],[(x,height-3) for x in range(2,5)],"rear")
        elif kind=="distribution-center":
            fixture("order-computer","order-computer",[(2,2)],[(3,2)],"front")
            interactions.append({"id":"order-pickup","kind":"courier-order-pickup","fixture_id":"order-computer",
                                 "approach_cells":[[3,2]],"mechanics":"pending Lead/NPC agreement; no order generation or assignment changes"})
            for x in range(2,width-3,3):
                fixture(f"warehouse-rack-{x}","storage-shelf",[(x,height-2)],[(x,height-3)],"rear")
        elif kind=="restaurant-bar":
            # A low service counter occupies the partition. Wall terrain is only
            # a solid preview placeholder; across-counter interaction is a draft interface.
            counter=[(x,split) for x in range(3,6)]
            fixture("service-counter","service-counter",counter,[(x,split-1) for x in range(3,6)]+[(x,split+1) for x in range(3,6)],None)
            for x,y in counter:
                interactions.append({"id":f"service-handoff-{x}","kind":"across-counter","fixture_id":"service-counter",
                                     "counter_cell":[x,y],"sides":[{"role":"waiter","room_id":"front","cell":[x,y-1]},
                                                                             {"role":"cook","room_id":"rear","cell":[x,y+1]}],
                                     "mechanics":"proposed interaction positions; no item transfer or NPC logic"})
            for x in (3,width-4):
                for y in (3,6):fixture(f"table-{x}-{y}","dining-table",[(x,y)],[(x+1,y)],"front")
            fixture("bar","bar-counter",[(1,5),(1,6)],[(2,5),(2,6)],"front")
            fixture("cooking-station","cooking-station",[(2,height-2)],[(2,height-3)],"rear")
            fixture("kitchen-stock","storage-shelf",[(width-3,height-2)],[(width-3,height-3)],"rear")
        else:
            middle=width//2
            for y in (3,5):
                for side,xs in (("left",range(2,middle-1)),("right",range(middle+2,width-2))):
                    fixture(f"pew-{side}-{y}","pew",[(x,y) for x in xs],[(x,y+1) for x in xs],"front")
            fixture("altar","altar",[(middle,split-2)],[(middle,split-3)],"front")
            fixture("vestry-stock","storage-shelf",[(2,height-2)],[(2,height-3)],"rear")

    # Rotate every geometry field together; IDs describe purpose, not world coordinates.
    def transform(point):
        x,y=point;w,h=width,height
        for _ in range(rotation):x,y,w,h=h-1-y,x,h,w
        return [x,y]

    for r in rooms:r["cells"]=[transform(p) for p in r["cells"]]
    for d in doors:d["cell"]=transform(d["cell"])
    for f in fixtures:
        for key in ("cells","approach_cells"):f[key]=[transform(p) for p in f[key]]
    for interaction in interactions:
        if "counter_cell" in interaction:interaction["counter_cell"]=transform(interaction["counter_cell"])
        if "approach_cells" in interaction:interaction["approach_cells"]=[transform(p) for p in interaction["approach_cells"]]
        for side in interaction.get("sides",[]):side["cell"]=transform(side["cell"])
    result={"kind":"outbreak.building-blueprint.draft","layout_version":BUILDING_LAYOUT_VERSION,
            "id":f"{kind}-{variant}-r{rotation}","building_type":kind,"variant":variant,"rotation_quarter_turns":rotation,
            "width":height if rotation%2 else width,"height":width if rotation%2 else height,
            "coordinate_units":"local simulation cells; use the destination map's metres-per-cell scale",
            "rooms":rooms,"walls":[transform(p) for p in sorted(walls)],"doors":doors,
            "fixtures":fixtures,"interactions":interactions,
            "integration_status":"draft authoring geometry, not an agreed persisted gameplay contract"}
    if kind=="house":
        blocked={tuple(p) for f in fixtures for p in f["cells"]}
        result["recovery_area"]={"room_id":"living","cells":[p for p in rooms[0]["cells"] if tuple(p) not in blocked],
                                 "intended_effect":"energy recovery in living area; activation and rates owned by Lead/NPC coder"}
    validate_building_blueprint(result)
    return result


def validate_building_blueprint(blueprint):
    """Check enclosure and access with proposed fixtures treated as solid obstacles."""
    width,height=blueprint["width"],blueprint["height"]
    def point(p):
        if not isinstance(p,list) or len(p)!=2 or any(type(v) is not int for v in p) or not (0<=p[0]<width and 0<=p[1]<height):
            raise ValueError("Blueprint geometry leaves its local bounds")
        return tuple(p)
    walls={point(p) for p in blueprint["walls"]};doors={};room_cells={};owners={}
    if len(walls)!=len(blueprint["walls"]):raise ValueError("Duplicate wall cell")
    ids=set()
    for room in blueprint["rooms"]:
        if room["id"] in room_cells:raise ValueError("Duplicate room identity")
        cells={point(p) for p in room["cells"]}
        if not 1<=len(cells)<=256 or len(cells)!=len(room["cells"]):raise ValueError("Invalid room floor budget")
        if cells & (walls|set(owners)):raise ValueError("Overlapping rooms or walls")
        room_cells[room["id"]]=cells;owners.update((p,room["id"]) for p in cells)
    def adjacent(p):
        x,y=p;return ((x-1,y),(x+1,y),(x,y-1),(x,y+1))
    for door in blueprint["doors"]:
        p=point(door["cell"])
        if door["id"] in ids or p in doors or p in walls or p in owners:raise ValueError("Invalid or overlapping door")
        ids.add(door["id"]);doors[p]=door
        connected={owners[n] for n in adjacent(p) if n in owners}
        if p[0] in (0,width-1) or p[1] in (0,height-1):connected.add("outside")
        if connected!=set(door["connects"]) or door["external"]!=("outside" in connected):raise ValueError("Door connects the wrong rooms")
    if walls|set(doors)|set(owners)!={(x,y) for y in range(height) for x in range(width)}:
        raise ValueError("Blueprint has missing wall or floor geometry")
    blocked=set();fixtures={}
    for fixture in blueprint["fixtures"]:
        if fixture["id"] in fixtures:raise ValueError("Duplicate fixture identity")
        cells={point(p) for p in fixture["cells"]}
        if not cells or cells & (blocked|set(doors)):raise ValueError("Fixture blocks a door or overlaps another fixture")
        if fixture["role"]=="service-counter":
            if not cells<=walls:raise ValueError("Service counter must occupy its partition")
        elif not cells<=room_cells.get(fixture["room_id"],set()):raise ValueError("Fixture is outside its room")
        blocked.update(cells);fixtures[fixture["id"]]=fixture
    # Room connectivity is checked with all fixtures present, before allowing any door crossing.
    for identity,cells in room_cells.items():
        free=cells-blocked
        if not free:raise ValueError("Room has no usable floor")
        first=min(free);seen={first};queue=deque([first])
        while queue:
            for n in adjacent(queue.popleft()):
                if n in free and n not in seen:seen.add(n);queue.append(n)
        if seen!=free:raise ValueError("Fixtures divide a room's usable floor")
        if not any(identity in d["connects"] and any(n in free for n in adjacent(p)) for p,d in doors.items()):
            raise ValueError("Room has no accessible doorway")
    walkable=set(owners)-blocked|set(doors)
    entrances=[p for p,d in doors.items() if d["external"]]
    if not entrances:raise ValueError("Building has no exterior entrance")
    seen={entrances[0]};queue=deque(seen)
    while queue:
        for n in adjacent(queue.popleft()):
            if n in walkable and n not in seen:seen.add(n);queue.append(n)
    if seen!=walkable:raise ValueError("Building contains unreachable floor or doors")
    for fixture in fixtures.values():
        if not fixture["approach_cells"]:raise ValueError("Fixture has no interaction approach")
        for approach in fixture["approach_cells"]:
            p=point(approach)
            if p not in seen or not any(n in {tuple(v) for v in fixture["cells"]} for n in adjacent(p)):
                raise ValueError("Fixture approach is blocked or not adjacent")
    for interaction in blueprint["interactions"]:
        fixture=fixtures.get(interaction["fixture_id"])
        if fixture is None:raise ValueError("Interaction references a missing fixture")
        approaches={tuple(p) for p in fixture["approach_cells"]}
        for p in interaction.get("approach_cells",[]):
            if point(p) not in approaches:raise ValueError("Interaction is outside its fixture approaches")
        if interaction["kind"]=="across-counter":
            counter=point(interaction["counter_cell"])
            if counter not in {tuple(p) for p in fixture["cells"]}:raise ValueError("Missing counter cell")
            sides=interaction["sides"]
            if len(sides)!=2 or {s["role"] for s in sides}!={"cook","waiter"} or len({s["room_id"] for s in sides})!=2:
                raise ValueError("Counter needs distinct kitchen and dining approaches")
            positions=[]
            for side in sides:
                p=point(side["cell"]);positions.append(p)
                if p not in approaches or owners.get(p)!=side["room_id"] or p not in adjacent(counter):
                    raise ValueError("Counter interaction is outside its assigned room")
            if positions[0][0]+positions[1][0]!=2*counter[0] or positions[0][1]+positions[1][1]!=2*counter[1]:
                raise ValueError("Counter approaches must face each other")
    if "recovery_area" in blueprint:
        area=blueprint["recovery_area"];cells={point(p) for p in area["cells"]}
        if not cells or cells!=room_cells.get(area["room_id"],set())-blocked:
            raise ValueError("Recovery area must cover usable living-area floor")
    return True


def generate_on_layout(sample,projected,grid,metrics,provenance,seed,max_buildings=128,
                       density=None,setback_cells=2,footprint_sizes=None,road_width_cells=1.5,
                       sidewalk_width_cells=.75,placement_mode="scatter",lot_gap_cells=1,
                       fair_block_budget=False,park_specs=None):
    if not isinstance(seed,int) or not 1<=seed<=4294967295:
        raise ValueError("Seed must be between 1 and 4294967295")
    if not 1<=max_buildings<=256:
        raise ValueError("Choose between 1 and 256 buildings")
    if placement_mode not in ("scatter","frontage","residential") or type(lot_gap_cells) is not int or not 0<=lot_gap_cells<=4:
        raise ValueError("Invalid placement mode or lot clearance")
    cell_metres=grid["cell_metres"]
    blocks=[g for g,p in projected if p["category"]=="block"]
    if not blocks:
        raise ValueError("Reference has no complete street-enclosed blocks for safe lot generation")
    # Use surveyed road/water geometry; source buildings are measurements, not fictional occupants.
    backbone=[(g,p) for g,p in projected if p["category"] in ("road","water")]
    document=atlas.export_map(sample,backbone,grid,provenance,road_width_cells,sidewalk_width_cells)
    cells=unpack(document);original=bytes(cells);width,height=grid["width"],grid["height"]
    recipes=[]
    for geometry,props in projected:
        if props["category"]!="building" or geometry.area<=0:
            continue
        coords=list(geometry.minimum_rotated_rectangle.exterior.coords)
        if len(coords)<4:continue
        sides=[math.dist(coords[i],coords[i+1])/cell_metres for i in (0,1)]
        recipes.append(tuple(max(4,min(12,round(length))) for length in sides))
    if footprint_sizes is not None:
        recipes=[tuple(pair) for pair in footprint_sizes]
    fallback=not recipes
    if fallback:recipes=[(5,7),(6,8),(5,5)]
    coverage=metrics["building_coverage"] if metrics["building_coverage"] is not None else .12
    coverage=max(.03,min(.3,coverage))
    if density is not None:
        if not math.isfinite(density) or not .03<=density<=.5:
            raise ValueError("Building coverage target must be between .03 and .5")
        coverage=density
    if not math.isfinite(setback_cells) or not 1<=setback_cells<=6:
        raise ValueError("Building setback must be between one and six cells")
    average_cells=sum(w*h for w,h in recipes)/len(recipes)
    rng=StableRandom(seed ^ 0xB017D1A5);buildings=[];occupied=set()
    parks=[];omitted=[]
    if park_specs:
        occupied,parks,omitted=place_parks(cells,width,height,blocks,cell_metres,park_specs)
    # New residential lots connect to surveyed street sidewalks, not merely a neighbour's spur.
    street_targets={(i%width,i//width) for i,tile in enumerate(original) if tile in (5,6)} if placement_mode=="residential" else None
    remaining_blocks=sum(1 for g,p in projected if p["category"]=="block" and not p.get("open_space",False))
    for block_index,block in enumerate(blocks):
        props=next(p for g,p in projected if p["category"]=="block" and g is block)
        if props.get("open_space",False):continue
        inner=block.buffer(-cell_metres*setback_cells)
        if inner.is_empty:continue
        x0,y0,x1,y1=inner.bounds
        local_coverage=min(.5,coverage*props.get("density_multiplier",1))
        target=max(1,min(24,round(block.area*local_coverage/(cell_metres**2*average_cells))))
        if fair_block_budget:
            target=min(target,max(1,(max_buildings-len(buildings))//max(1,remaining_blocks)))
        remaining_blocks-=1
        placed=0
        candidates=[]
        if placement_mode=="residential":
            # Fixed lot modules prevent random slivers; nearest-edge lots are attempted first.
            lot_w=max(max(pair) for pair in recipes);stride=lot_w+lot_gap_cells
            candidates=[(x,y) for y in range(math.ceil(y0/cell_metres),math.floor(y1/cell_metres)-lot_w+1,stride)
                        for x in range(math.ceil(x0/cell_metres),math.floor(x1/cell_metres)-lot_w+1,stride)]
            candidates.sort(key=lambda xy:(min(xy[0]-x0/cell_metres,xy[1]-y0/cell_metres,x1/cell_metres-xy[0]-lot_w,y1/cell_metres-xy[1]-lot_w),xy[1],xy[0]))
        for attempt in range(len(candidates) if placement_mode=="residential" else target*100):
            if placed>=target or len(buildings)>=max_buildings:break
            w,h=rng.choice(recipes)
            if rng.randrange(2):w,h=h,w
            minimum_x,minimum_y=math.ceil(x0/cell_metres),math.ceil(y0/cell_metres)
            maximum_x,maximum_y=math.floor(x1/cell_metres)-w,math.floor(y1/cell_metres)-h
            if maximum_x<minimum_x or maximum_y<minimum_y:continue
            left,top=rng.randint(minimum_x,maximum_x),rng.randint(minimum_y,maximum_y)
            if placement_mode=="frontage":
                side=attempt%5
                if side==0:left=minimum_x
                elif side==1:top=minimum_y
                elif side==2:left=maximum_x
                elif side==3:top=maximum_y
            if placement_mode=="residential":left,top=candidates[attempt]
            if left<1 or top<1 or left+w>=width or top+h>=height:continue
            geometry=box(left*cell_metres,top*cell_metres,(left+w)*cell_metres,(top+h)*cell_metres)
            if not inner.covers(geometry):continue
            footprint={(x,y) for y in range(top,top+h) for x in range(left,left+w)}
            if any(cells[y*width+x]!=0 or (x,y) in occupied for x,y in footprint):continue
            connection=entrance_path(cells,width,height,left,top,w,h,street_targets)
            if connection is None:continue
            door,path=connection
            for x,y in footprint:
                cells[y*width+x]=8 if x in (left,left+w-1) or y in (top,top+h-1) else 7
            cells[door[1]*width+door[0]]=9
            for x,y in path:
                if cells[y*width+x]==0:cells[y*width+x]=5
            occupied.update((x,y) for y in range(top-lot_gap_cells,top+h+lot_gap_cells) for x in range(left-lot_gap_cells,left+w+lot_gap_cells))
            buildings.append({"left":left,"top":top,"width":w,"height":h,"door":[door[0],door[1]],"block":block_index,"path_cells":path})
            placed+=1
    if not buildings:
        raise ValueError("No safe building fits this reference at the selected scale")
    # Preserve every original corridor and water cell exactly.
    if any(tile in (5,6,12) and cells[i]!=tile for i,tile in enumerate(original)):
        raise AssertionError("Lot generation changed the surveyed street backbone")
    metadata={"schema_version":2,"generator_version":GENERATOR_VERSION,"rng_algorithm":"xorshift32/rejection-v1",
              "seed":seed,"reference_id":sample["id"],"reference":sample,
              "provenance":provenance,"grid":grid,"buildings":buildings,
              "source_block_count":len(blocks),"target_coverage":coverage,"setback_cells":setback_cells,
              "footprint_recipe_source":"explicit recipe" if footprint_sizes is not None else "explicit fallback — reference has no buildings" if fallback else "surveyed footprints, adapted to 4–12 game cells per side",
              "scope":"Surveyed street backbone; fictional seeded buildings and doors. No population, assigned roles, supplies or vehicle lanes."}
    if placement_mode!="scatter" or lot_gap_cells!=1 or fair_block_budget:
        metadata.update(placement_mode=placement_mode,lot_gap_cells=lot_gap_cells,fair_block_budget=fair_block_budget)
    if park_specs is not None:metadata.update(parks=parks,omitted_parks=omitted)
    document["id"]="generated-reference-"+sample["id"]+"-"+str(seed)
    document["name"]="Generated "+sample["id"]+" district (seed "+str(seed)+")"
    document["authoringJson"]=json.dumps(metadata,ensure_ascii=False,sort_keys=True)
    pack(document,cells)
    artwork=backbone+[(g,p) for g,p in projected if p["category"]=="block"]
    for i,building in enumerate(buildings):
        x,y,w,h=(building[k] for k in ("left","top","width","height"))
        artwork.append((box(x*cell_metres,y*cell_metres,(x+w)*cell_metres,(y+h)*cell_metres),{"category":"building","source_id":"fictional/"+str(i)}))
    return document,metadata,atlas.svg_map(artwork,grid,metrics)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reference",required=True)
    parser.add_argument("--seed",type=int,default=214)
    parser.add_argument("--max-buildings",type=int,default=128)
    parser.add_argument("--orientation",choices=["cardinal","geographic"],default="cardinal")
    parser.add_argument("--atlas",type=Path,default=atlas.ROOT/"CityReferences/atlas")
    parser.add_argument("--output",type=Path,default=atlas.ROOT/"CityReferences/generated")
    args=parser.parse_args()
    if not args.reference or any(c not in "abcdefghijklmnopqrstuvwxyz0123456789-" for c in args.reference):
        parser.error("Reference id must contain lowercase letters, digits and hyphens")
    source=json.loads((args.atlas/(args.reference+".geojson")).read_text())
    profile=json.loads((args.atlas/(args.reference+".profile.json")).read_text())
    document,metadata,artwork=generate(profile["sample"],source["features"],profile["provenance"],args.seed,profile["grid"]["cell_metres"],args.max_buildings,args.orientation)
    stem=args.reference+"-"+str(args.seed)+("-cardinal" if args.orientation=="cardinal" else "")
    atlas.write_json(args.output/(stem+".obmap2"),document)
    atlas.write_json(args.output/(stem+".generation.json"),metadata)
    (args.output/(stem+".svg")).write_text(artwork)
    print(f'Generated {stem}: {len(metadata["buildings"])} buildings with physical doors and street paths')


if __name__=="__main__":main()
