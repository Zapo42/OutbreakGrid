"""Check cardinal orientation, graph connectivity and recipe replay against actual map outputs."""
from collections import deque
import copy
import json
import math
import unittest

from shapely.affinity import rotate
from shapely.geometry import LineString, Polygon

import city_atlas as atlas
import generate_reference_city as lots
import generate_seeded_city as cities


class SeededCityChecks(unittest.TestCase):
    def settings(self,name="suburban-dead-ends",seed=214):
        settings=cities.preset_settings(name,seed)
        settings.update(columns=3,rows=3,max_buildings=8,park_fraction=0)
        return settings

    def test_alignment_preserves_geometry_and_removes_dominant_rotation(self):
        polygon=Polygon([(0,0),(200,0),(200,100),(0,100)])
        projected=[(rotate(polygon,31,origin=(0,0)),{"category":"block"})]
        for first,last in [((0,0),(200,0)),((200,0),(200,100)),((200,100),(0,100)),((0,100),(0,0))]:
            projected.append((rotate(LineString([first,last]),31,origin=(0,0)),{"category":"road"}))
        aligned,grid=lots.align_cardinal(projected,{"width":60,"height":60,"cell_metres":5,"north_up":True})
        self.assertAlmostEqual(grid["rotation_degrees"],-31,places=8)
        self.assertFalse(grid["north_up"])
        self.assertAlmostEqual(aligned[0][0].area,polygon.area,places=5)
        for (before,props),(after,_) in zip(projected,aligned):
            self.assertAlmostEqual(before.length,after.length,places=6)
            self.assertGreaterEqual(after.bounds[0],0);self.assertGreaterEqual(after.bounds[1],0)
            if props["category"]=="road":
                a,b=list(after.coords)
                self.assertTrue(abs(a[0]-b[0])<1e-6 or abs(a[1]-b[1])<1e-6)

    def test_known_random_stream_is_version_stable(self):
        rng=lots.StableRandom(1)
        self.assertEqual([rng.word() for _ in range(3)],[270369,67634689,2647435461])

    def test_all_presets_produce_connected_cardinal_roads_and_valid_building_paths(self):
        for name in sorted(cities.PRESETS):
            with self.subTest(preset=name):
                settings=self.settings(name)
                profile=cities.profile_for(settings["reference_id"],atlas.ROOT/"CityReferences/atlas")
                document,metadata,_,recipe=cities.generate_city(settings,profile)
                self.assertGreater(len(metadata["buildings"]),0)
                self.assertLessEqual(len(metadata["buildings"]),8)
                self.assertEqual(metadata["recipe"],recipe)
                for segment in metadata["street_segments"]:
                    a,b=segment["points"]
                    self.assertTrue(a[0]==b[0] or a[1]==b[1])
                cells=lots.unpack(document);width,height=document["width"],document["height"]
                roads={i for i,tile in enumerate(cells) if tile==6}
                queue=deque([min(roads)]);visited={min(roads)}
                while queue:
                    cell=queue.popleft();x,y=cell%width,cell//width
                    for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                        nxt=ny*width+nx
                        if 0<=nx<width and 0<=ny<height and nxt in roads and nxt not in visited:
                            visited.add(nxt);queue.append(nxt)
                self.assertEqual(roads,visited,"Road raster contains a disconnected fragment")
                for building in metadata["buildings"]:
                    x,y=building["door"]
                    self.assertEqual(cells[y*width+x],9)
                    self.assertTrue(all(cells[py*width+px] in (5,6) for px,py in building["path_cells"]))
                self.assertIn("provenance",document["authoringJson"])

    def test_serialized_recipe_replays_the_exact_city(self):
        settings=self.settings()
        original,metadata,svg,recipe=cities.generate_city(settings)
        stored=json.loads(json.dumps(recipe))
        replay,_,replay_svg,replayed=cities.generate_city(stored["settings"])
        self.assertEqual(cities.canonical(original),cities.canonical(replay))
        self.assertEqual(svg,replay_svg)
        self.assertEqual(recipe,replayed)

    def test_seed_changes_streets_and_lots(self):
        first=self.settings(seed=214);second=self.settings(seed=215)
        _,a,_,_=cities.generate_city(first)
        _,b,_,_=cities.generate_city(second)
        self.assertNotEqual(a["street_segments"],b["street_segments"])
        self.assertNotEqual(a["buildings"],b["buildings"])

    def test_lot_density_control_keeps_street_stream_independent(self):
        first=self.settings();second=copy.deepcopy(first);second["building_coverage"]=.4
        streets_a,grid_a=cities.street_layout(first);streets_b,grid_b=cities.street_layout(second)
        self.assertEqual(grid_a,grid_b)
        self.assertEqual([(g.wkb,p) for g,p in streets_a if p["category"]=="road"],[(g.wkb,p) for g,p in streets_b if p["category"]=="road"])
        a,_,_,ra=cities.generate_city(first);b,_,_,rb=cities.generate_city(second)
        self.assertNotEqual(ra["recipe_id"],rb["recipe_id"])

    def test_settings_and_reference_requirements_are_validated(self):
        bad_values={"seed":0,"columns":11,"block_variation":float("nan"),"loop_chance":1.1,"building_coverage":.9,"park_fraction":-.1,"cell_metres":0,"footprint_sizes":[[2,5]]}
        for name,value in bad_values.items():
            settings=self.settings();settings[name]=value
            with self.subTest(setting=name),self.assertRaises(ValueError):cities.generate_city(settings)
        settings=self.settings();settings["unknown"]=1
        with self.assertRaises(ValueError):cities.generate_city(settings)
        with self.assertRaises(ValueError):cities.generate_city(self.settings("compact-grid"))


if __name__=="__main__":unittest.main()
