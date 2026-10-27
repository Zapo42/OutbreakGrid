"""Generate new cardinal street networks and lots from complete, replayable recipes."""
import argparse
import copy
import hashlib
import json
import math
from pathlib import Path

from shapely.geometry import LineString
from shapely.ops import polygonize, unary_union

import city_atlas as atlas
from generate_reference_city import GENERATOR_VERSION, StableRandom, generate_on_layout

PRESETS = {
    "compact-grid": {"reference_id":"west-oakland","columns":5,"rows":4,"loop_chance":1.,"dead_end_chance":0.,"block_variation":.12,"building_coverage":.24},
    "long-block-grid": {"reference_id":"manhattan","columns":4,"rows":4,"loop_chance":1.,"dead_end_chance":0.,"block_variation":.12,"building_coverage":.23},
    "mixed-town": {"reference_id":"prague-bubenec","columns":4,"rows":4,"loop_chance":.65,"dead_end_chance":.3,"block_variation":.3,"building_coverage":.18},
    "town-centre": {"reference_id":None,"columns":5,"rows":4,"block_width_cells":25,"block_height_cells":23,"loop_chance":.85,"dead_end_chance":.15,"centre_density_multiplier":2.,"park_fraction":.15,"building_coverage":.18},
    "suburban-loops": {"reference_id":None,"columns":5,"rows":4,"block_width_cells":30,"block_height_cells":26,"loop_chance":.45,"dead_end_chance":.45,"park_fraction":.15,"building_coverage":.12},
    "suburban-dead-ends": {"reference_id":None,"columns":5,"rows":4,"block_width_cells":32,"block_height_cells":27,"loop_chance":.1,"dead_end_chance":.85,"park_fraction":.12,"building_coverage":.10},
}

DEFAULTS = {"columns":4,"rows":4,"block_width_cells":24,"block_height_cells":24,
            "block_variation":.2,"loop_chance":.8,"dead_end_chance":.2,
            "road_width_cells":2.,"arterial_width_cells":4.,"arterial_every":3,
            "sidewalk_width_cells":1.,"building_coverage":.18,"setback_cells":3.,
            "park_fraction":.08,"centre_density_multiplier":1.,"max_buildings":128,
            "cell_metres":5.,"footprint_sizes":[[4,5],[5,6],[6,7],[7,8]],"reference_id":None}


def canonical(value):
    return json.dumps(value,sort_keys=True,separators=(",",":"),ensure_ascii=False,allow_nan=False).encode()


def profile_for(identity,directory):
    if identity is None:return None
    if identity not in ("west-oakland","manhattan","prague-bubenec"):
        raise ValueError("Unknown reference profile")
    return json.loads((directory/(identity+".profile.json")).read_text())


def preset_settings(name,seed=214,directory=atlas.ROOT/"CityReferences/atlas"):
    if name not in PRESETS:raise ValueError("Unknown city preset")
    settings=copy.deepcopy(DEFAULTS)
    settings.update(PRESETS[name]);settings.update(preset=name,seed=seed)
    profile=profile_for(settings["reference_id"],directory)
    if profile:
        metrics=profile["metrics"]
        area=metrics.get("median_closed_block_area_m2") or 10000
        aspect=metrics.get("median_closed_block_aspect_ratio") or 1
        if name=="compact-grid":aspect=1.0 # Explicit design adaptation: compact square blocks.
        scale=settings["cell_metres"]
        settings["block_width_cells"]=max(16,min(44,round(math.sqrt(area*aspect)/scale)))
        settings["block_height_cells"]=max(16,min(36,round(math.sqrt(area/aspect)/scale)))
    return settings


def validate(settings):
    expected=set(DEFAULTS)|{"preset","seed"}
    if set(settings)!=expected:
        raise ValueError("Recipe settings missing or unknown: "+str(set(settings)^expected))
    if settings["preset"] not in PRESETS:raise ValueError("Unknown preset")
    limits={"seed":(1,4294967295),"columns":(2,10),"rows":(2,10),
            "block_width_cells":(14,80),"block_height_cells":(14,80),"max_buildings":(1,256),"arterial_every":(0,10)}
    for name,(low,high) in limits.items():
        value=settings[name]
        if type(value) is not int or not low<=value<=high:
            raise ValueError(f"{name} must be an integer between {low} and {high}")
    numeric={"block_variation":(0,.45),"loop_chance":(0,1),"dead_end_chance":(0,1),
             "road_width_cells":(1,4),"arterial_width_cells":(1,8),"sidewalk_width_cells":(.5,3),
             "building_coverage":(.03,.5),"setback_cells":(2,6),"park_fraction":(0,.5),
             "centre_density_multiplier":(1,3),"cell_metres":(1,20)}
    for name,(low,high) in numeric.items():
        value=settings[name]
        if isinstance(value,bool) or not isinstance(value,(int,float)) or not math.isfinite(value) or not low<=value<=high:
            raise ValueError(f"{name} must be between {low} and {high}")
    if settings["arterial_width_cells"]<settings["road_width_cells"]:
        raise ValueError("Arterial width must be at least the local road width")
    if settings["reference_id"] not in (None,"west-oakland","manhattan","prague-bubenec"):
        raise ValueError("Unknown reference id")
    sizes=settings["footprint_sizes"]
    if not isinstance(sizes,list) or not 1<=len(sizes)<=16 or any(not isinstance(pair,list) or len(pair)!=2 or any(type(v) is not int or not 4<=v<=12 for v in pair) for pair in sizes):
        raise ValueError("Footprint sizes must be 1–16 integer pairs with sides between 4 and 12 cells")


def street_layout(settings):
    """A connected seeded grid backbone, optional loops and finite branches; every segment cardinal."""
    validate(settings)
    rng=StableRandom(settings["seed"] ^ 0x57AEE75)
    columns,rows=settings["columns"],settings["rows"]
    margin=math.ceil(settings["arterial_width_cells"]/2+settings["sidewalk_width_cells"])+2
    def axes(count,nominal):
        points=[margin]
        for _ in range(count):
            variation=round(nominal*settings["block_variation"])
            points.append(points[-1]+max(14,nominal+rng.randint(-variation,variation)))
        return points
    xs=axes(columns,settings["block_width_cells"]);ys=axes(rows,settings["block_height_cells"])
    width,height=xs[-1]+margin+1,ys[-1]+margin+1
    if width*height>atlas.MAX_CELLS:raise ValueError("Map exceeds one million cells")
    edges=[];stride=columns+1;every=settings["arterial_every"]
    for row in range(rows+1):
        for column in range(columns):
            edges.append({"a":row*stride+column,"b":row*stride+column+1,
                          "first":(xs[column],ys[row]),"last":(xs[column+1],ys[row]),
                          "locked":row in (0,rows) or bool(every and row%every==0),
                          "arterial":bool(every and row%every==0)})
    for column in range(columns+1):
        for row in range(rows):
            edges.append({"a":row*stride+column,"b":(row+1)*stride+column,
                          "first":(xs[column],ys[row]),"last":(xs[column],ys[row+1]),
                          "locked":column in (0,columns) or bool(every and column%every==0),
                          "arterial":bool(every and column%every==0)})
    parents=list(range((columns+1)*(rows+1)))
    def root(node):
        while parents[node]!=node:
            parents[node]=parents[parents[node]];node=parents[node]
        return node
    def join(a,b):
        a,b=root(a),root(b)
        if a==b:return False
        parents[max(a,b)]=min(a,b);return True
    kept=set()
    for index,edge in enumerate(edges):
        if edge["locked"]:kept.add(index);join(edge["a"],edge["b"])
    order=list(range(len(edges)))
    for i in range(len(order)-1,0,-1):
        other=rng.randrange(i+1);order[i],order[other]=order[other],order[i]
    for index in order:
        edge=edges[index]
        if join(edge["a"],edge["b"]):kept.add(index)
    for index in order:
        if index not in kept and rng.fraction()<settings["loop_chance"]:kept.add(index)
    scale=settings["cell_metres"];projected=[]
    for index,edge in enumerate(edges):
        first,last=edge["first"],edge["last"]
        stub=False
        if index not in kept:
            if rng.fraction()>=settings["dead_end_chance"]:continue
            if rng.randrange(2):first,last=last,first
            fraction=.35+.2*rng.fraction()
            last=tuple(first[i]+(last[i]-first[i])*fraction for i in (0,1));stub=True
        geometry=LineString([(x*scale,y*scale) for x,y in (first,last)])
        projected.append((geometry,{"category":"road","source_id":"fictional-road/"+str(index),
                                   "width_cells":settings["arterial_width_cells"] if edge["arterial"] else settings["road_width_cells"],
                                   "tags":{"synthetic":True,"dead_end":stub,"arterial":edge["arterial"]}}))
    network=unary_union([g for g,p in projected])
    blocks=sorted(polygonize(network),key=lambda g:(g.bounds,g.normalize().wkb_hex))
    parks_rng=StableRandom(settings["seed"] ^ 0xA11CE)
    for index,block in enumerate(blocks):
        center=block.centroid
        normalized=max(abs(center.x/scale-width/2)/(width/2),abs(center.y/scale-height/2)/(height/2))
        multiplier=1+(settings["centre_density_multiplier"]-1)*max(0,1-normalized)
        projected.append((block,{"category":"block","source_id":"fictional-block/"+str(index),
                                 "open_space":parks_rng.fraction()<settings["park_fraction"],
                                 "density_multiplier":multiplier,"tags":{"synthetic":True}}))
    grid={"width":width,"height":height,"cell_metres":scale,"north_up":True,
          "projection":"fictional local metric grid; game north is up, no geographic CRS",
          "projected_origin_x":0,"projected_origin_y":0}
    return projected,grid


def generate_city(settings,reference_profile=None):
    validate(settings)
    if settings["reference_id"] and (reference_profile is None or reference_profile["sample"]["id"]!=settings["reference_id"]):
        raise ValueError("Matching reference profile required to preserve provenance")
    digest=hashlib.sha256(canonical(reference_profile)).hexdigest() if reference_profile else None
    recipe={"generator_version":GENERATOR_VERSION,"settings":settings,"reference_profile_sha256":digest}
    identity=hashlib.sha256(canonical(recipe)).hexdigest()[:12]
    recipe["recipe_id"]=identity
    projected,grid=street_layout(settings)
    sample={"id":settings["preset"],"name":settings["preset"].replace('-',' ').title(),"fictional":True}
    provenance={"attribution":"Fictional designed city preset; no new geographic survey.","reference_profile_sha256":digest}
    if reference_profile:
        provenance.update(reference_profile["provenance"])
        provenance["reference_profile_sha256"]=digest
        provenance["adaptation"]="New cardinal street network using adapted reference block dimensions; not a geographic replica. Density and street pruning are explicit design controls."
    metrics={"building_coverage":settings["building_coverage"]}
    document,metadata,artwork=generate_on_layout(sample,projected,grid,metrics,provenance,settings["seed"],settings["max_buildings"],
        density=settings["building_coverage"],setback_cells=settings["setback_cells"],footprint_sizes=settings["footprint_sizes"],
        road_width_cells=settings["road_width_cells"],sidewalk_width_cells=settings["sidewalk_width_cells"])
    metadata.update(recipe=recipe,recipe_id=identity,
                    scope="New fictional cardinal streets, seeded building lots, doors and paths. No NPCs, jobs, supplies or directed vehicle lanes.")
    metadata["street_segments"]=[{"points":list(g.coords),"width_cells":p["width_cells"],**p["tags"]} for g,p in projected if p["category"]=="road"]
    metadata["blocks"]=[{"bounds_metres":g.bounds,"open_space":p["open_space"],"density_multiplier":p["density_multiplier"]} for g,p in projected if p["category"]=="block"]
    document["id"]=settings["preset"]+"-"+str(settings["seed"])+"-"+identity
    document["name"]=sample["name"]+" (seed "+str(settings["seed"])+")"
    document["authoringJson"]=json.dumps(metadata,sort_keys=True,ensure_ascii=False,allow_nan=False)
    return document,metadata,artwork,recipe


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--preset",choices=sorted(PRESETS),default=None)
    parser.add_argument("--recipe",type=Path)
    parser.add_argument("--atlas",type=Path,default=atlas.ROOT/"CityReferences/atlas")
    parser.add_argument("--output",type=Path,default=atlas.ROOT/"CityReferences/cardinal/layouts")
    for name,value in DEFAULTS.items():
        if name in ("reference_id","footprint_sizes"):continue
        parser.add_argument("--"+name.replace('_','-'),type=int if type(value) is int else float)
    parser.add_argument("--seed",type=int)
    args=parser.parse_args()
    prior=json.loads(args.recipe.read_text()) if args.recipe else None
    if prior and prior.get("generator_version")!=GENERATOR_VERSION:parser.error("Recipe generator version is not supported")
    settings=copy.deepcopy(prior["settings"]) if prior else preset_settings(args.preset or "compact-grid",args.seed if args.seed is not None else 214,args.atlas)
    if args.recipe and args.preset:parser.error("Use a recipe or a preset, not both")
    for name in [*DEFAULTS,"seed"]:
        value=getattr(args,name,None)
        if value is not None:settings[name]=value
    profile=profile_for(settings["reference_id"],args.atlas)
    if prior:
        digest=hashlib.sha256(canonical(profile)).hexdigest() if profile else None
        if digest!=prior["reference_profile_sha256"]:parser.error("Reference profile changed; exact recipe replay would be unsafe")
    document,metadata,artwork,recipe=generate_city(settings,profile)
    stem=document["id"]
    atlas.write_json(args.output/(stem+".obmap2"),document)
    atlas.write_json(args.output/(stem+".generation.json"),metadata)
    atlas.write_json(args.output/(stem+".recipe.json"),recipe)
    (args.output/(stem+".svg")).write_text(artwork)
    print(f'Generated {stem}: {document["width"]} × {document["height"]}, {len(metadata["buildings"])} buildings')


if __name__=="__main__":main()
