"""Rebuild the three historical atlas pilots from checksum-pinned public research samples."""
import hashlib
from pathlib import Path
import subprocess
import sys
import urllib.request

import city_atlas

SAMPLES = [
    ("west-oakland", "West Oakland — published street/building sample", ".osm.bz2",
     "https://raw.githubusercontent.com/gboeing/osmnx/main/tests/input_data/West-Oakland.osm.bz2",
     "92efe9ed4f803961e1b552d0e769fc10703814efa827e9a6fb013004e00bbeae",
     ["--bbox","37.80615","-122.30258","37.80914","-122.29825"]),
    ("manhattan", "Manhattan — Upper East Side street sample", ".graphml",
     "https://raw.githubusercontent.com/pysal/momepy/main/momepy/datasets/nyc_graph.graphml",
     "d071cec23cfcd659b3571fbcc7ca21cf3500f0e4d3c30c6f4facc71517b280ae", []),
    ("prague-bubenec", "Prague — Bubeneč residential quarter sample", ".gpkg",
     "https://raw.githubusercontent.com/pysal/momepy/main/momepy/datasets/bubenec.gpkg",
     "fab4f3047ce5a63dec99943a09e3c8dba1f18d9a4e3b7b21ea377e28a67a55a4",
     ["--attribution", "Momepy sample data; © 2018–2021 Martin Fleischmann and PySAL Developers, BSD-3-Clause. Upstream geometry lineage unspecified; not claimed as OSM."]),
]


def main():
    cache=city_atlas.ROOT/"CityReferences/cache"
    cache.mkdir(parents=True,exist_ok=True)
    for identity,name,suffix,url,digest,extra in SAMPLES:
        path=cache/(identity+suffix)
        if not path.exists():
            request=urllib.request.Request(url,headers={"User-Agent":"OutbreakGrid-CityAtlas/1.0"})
            with urllib.request.urlopen(request,timeout=60) as response:
                data=response.read(8*1024*1024+1)
            if len(data)>8*1024*1024 or hashlib.sha256(data).hexdigest()!=digest:
                raise ValueError("Source size/checksum mismatch: "+identity+". Review upstream changes before updating the source pin.")
            path.write_bytes(data)
        if hashlib.sha256(path.read_bytes()).hexdigest()!=digest:
            raise ValueError("Cached source checksum mismatch: "+str(path))
        subprocess.run([sys.executable,str(Path(__file__).with_name("city_atlas.py")),"import",str(path),
                        "--id",identity,"--name",name,"--source-url",url,*extra],check=True)


if __name__=="__main__":main()
