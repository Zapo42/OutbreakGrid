"""Author a small controlled geometry fixture using the live house/store materializers.

No actors or behavior are simulated here. StoreRoutineFixture adds assignments and
finite inspection inventories through the existing Core contracts.
"""
import argparse
import json
from pathlib import Path

import generate_reference_city as lots
import generate_district_city as districts
import city_atlas as atlas


def build(seed=214):
    if type(seed) is not int or not 1<=seed<=0xffffffff:
        raise ValueError("Seed must be a nonzero uint32")
    width,height=100,56
    cells=bytearray(width*height)
    for x in range(width):cells[x]=cells[(height-1)*width+x]=8
    for y in range(height):cells[y*width]=cells[y*width+width-1]=8
    # One spine, with safe marked crossings at every north/south connection.
    for x in range(1,width-1):
        cells[24*width+x]=6
        cells[23*width+x]=cells[25*width+x]=5
    # The existing materializers target this immutable street raster.
    targets={(x,y) for y in (23,24,25) for x in range(1,width-1)}
    base={"kind":"outbreak.map","schemaVersion":1,"id":f"store-validation-{seed}",
          "name":f"Store validation (seed {seed})","width":width,"height":height,"chunkSize":32,"chunks":[]}
    lots.pack(base,cells)
    def buildings(kind,origins,w,h):
        return [{"left":x,"top":y,"width":w,"height":h,"parcel_id":f"{kind}-{i+1}",
                 "path_cells":[],"use_hint":"retail" if kind=="store" else "detached-home",
                 "game_building_use_hint":"Store" if kind=="store" else "Home","storeys_hint":1}
                for i,(x,y) in enumerate(origins)]
    store_meta={"buildings":buildings("store",[(8,8),(76,8)],14,12)}
    districts.lots.materialize_store_layouts(base,store_meta,"compact",targets)
    # Materializers intentionally own their feature arrays; retain store fixtures
    # while adding homes on the same terrain rather than resetting the map.
    retained={key:base[key] for key in ("buildingRooms","storageBoxes","shelves","phoneBlocks")}
    home_meta={"buildings":buildings("home",[(8,36),(76,36),(43,36)],10,9)}
    lots.materialize_house_layouts(base,home_meta,"compact",targets)
    for key,value in retained.items():base[key]=value+base.get(key,[]) if key not in ("shelves","phoneBlocks") else value
    # A deliberately simple distribution building uses existing one-room geometry;
    # no dispatch-computer behavior or new DC template mechanics are introduced.
    cells=lots.unpack(base)
    left,top,w,h=40,7,12,12
    for y in range(top,top+h):
        for x in range(left,left+w):cells[y*width+x]=8 if x in (left,left+w-1) or y in (top,top+h-1) else 7
    cells[(top+h-1)*width+46]=9
    for y in range(19,24):cells[y*width+46]=5
    dc={"left":left,"top":top,"width":w,"height":h,"parcel_id":"distribution-1",
        "door":[46,18],"path_cells":[[46,y] for y in range(19,24)],"use_hint":"office","game_building_use_hint":"DistributionCenter","storeys_hint":1}
    base["storageBoxes"].append({"x":43,"y":10,"capacity":240,"foodReserve":6,"items":[]})
    # Park loop has one explicit familiar destination and a sidewalk to the spine.
    for x in range(59,68):cells[36*width+x]=cells[44*width+x]=5
    for y in range(36,45):cells[y*width+59]=cells[y*width+67]=5
    for y in range(25,41):cells[y*width+63]=5
    for i,t in enumerate(cells):
        if i//width==24 and cells[23*width+i%width]==5 and cells[25*width+i%width]==5 and (i%width in (46,63) or cells[22*width+i%width]==5 or cells[26*width+i%width]==5):cells[i]=16
    metadata={"seed":seed,"generator_version":"store-validation-1","grid":{"cell_metres":1},
        "buildings":store_meta["buildings"]+home_meta["buildings"]+[dc],
        "scope":"Authored controlled test geometry using live generated templates; no behavior ticks.",
        "park":{"bounds":[59,36,9,9],"destination":[63,40]},"distribution_stock":[43,10],
        "active_features":["building-room-membership","sales-shelves","stockroom-storage","store-phones","living-area-rest-spots"]}
    # Validate before crossing tiles are substituted: the general district topology
    # checker models the generator's ordinary Road/Sidewalk raster, not Crosswalk.
    checked=bytearray(cells)
    for i,t in enumerate(checked):
        if t==16:checked[i]=6
    lots.pack(base,checked)
    districts.assert_usable(base,metadata)
    lots.pack(base,cells)
    base["authoringJson"]=json.dumps(metadata,sort_keys=True,separators=(",",":"))
    return base,metadata


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output",type=Path,required=True);parser.add_argument("--seed",type=int,default=214)
    args=parser.parse_args();document,metadata=build(args.seed);args.output.mkdir(parents=True,exist_ok=True)
    stem=f"store-validation-{args.seed}"
    atlas.write_json(args.output/(stem+".obmap2"),document)
    atlas.write_json(args.output/(stem+".geometry.json"),metadata)
    artwork=districts.render_map(document,metadata)
    crossings="".join(f'<rect x="{i%document["width"]}" y="{i//document["width"]}" width="1" height="1" fill="#ffffff"/>' for i,t in enumerate(lots.unpack(document)) if t==16)
    (args.output/(stem+".svg")).write_text(artwork.replace("</svg>",crossings+"</svg>"))
    print("PASS controlled geometry authored: 2 stores, 3 homes, DC, park; no NPC ticks")


if __name__=="__main__":main()
