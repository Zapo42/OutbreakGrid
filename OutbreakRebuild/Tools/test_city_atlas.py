"""Geometry/scale regressions for the real-world survey pipeline. No network required."""
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from shapely.geometry import LineString, Polygon
import city_atlas as atlas
import generate_reference_city as generator


class AtlasChecks(unittest.TestCase):
    bounds=[0,0,.01,.01]

    def roads(self):
        # A square enclosing one measured block, with all junctions away from the crop.
        result=[]
        for index,coords in enumerate([[(.002,.002),(.008,.002)],[(.008,.002),(.008,.008)],
                                       [(.008,.008),(.002,.008)],[(.002,.008),(.002,.002)]]):
            result.append(atlas.feature(LineString(coords),"road",str(index),{"highway":"residential"}))
        return result

    def test_metric_scale_and_closed_block(self):
        _,grid,metrics=atlas.survey(self.roads(),self.bounds,5)
        self.assertTrue(220<=grid["width"]<=224)
        self.assertTrue(220<=grid["height"]<=224)
        self.assertEqual(metrics["closed_street_blocks"],1)
        self.assertTrue(440000<metrics["median_closed_block_area_m2"]<450000)
        self.assertAlmostEqual(metrics["median_closed_block_aspect_ratio"],1,delta=.02)
        self.assertGreater(metrics["fourfold_orientation_alignment"],.99)
        self.assertIsNone(metrics["building_coverage"])

    def test_roads_outside_window_are_clipped_and_boundary_ends_excluded(self):
        feature=atlas.feature(LineString([(-.01,.005),(.02,.005)]),"road","crossing")
        _,_,metrics=atlas.survey([feature],self.bounds,5)
        self.assertTrue(1.10<metrics["street_length_km"]<1.12)
        self.assertEqual(metrics["interior_dead_ends"],0)
        self.assertIsNone(metrics["interior_dead_end_share"])

    def test_duplicate_directions_do_not_double_street_lengths(self):
        one=self.roads()
        _,_,single=atlas.survey(one,self.bounds,5)
        duplicate=one+[atlas.feature(LineString(list(f["geometry"]["coordinates"])[::-1]),"road","reverse") for f in one]
        _,_,double=atlas.survey(duplicate,self.bounds,5)
        self.assertEqual(single["street_length_km"],double["street_length_km"])
        self.assertEqual(single["closed_street_blocks"],double["closed_street_blocks"])

    def test_bridge_crossing_does_not_create_a_ground_junction(self):
        roads=[atlas.feature(LineString([(.002,.005),(.008,.005)]),"road","ground"),
               atlas.feature(LineString([(.005,.002),(.005,.008)]),"road","bridge",{"bridge":"yes","layer":"1"})]
        _,_,metrics=atlas.survey(roads,self.bounds,5)
        self.assertEqual(metrics["interior_junctions"],0)
        roads[1]["properties"]["tags"]={}
        _,_,metrics=atlas.survey(roads,self.bounds,5)
        self.assertEqual(metrics["interior_junctions"],1)

    def test_unclosed_building_and_incomplete_way_are_not_invented(self):
        elements=[{"type":"way","id":1,"tags":{"building":"yes"},
                   "geometry":[{"lon":0,"lat":0},{"lon":0,"lat":1},{"lon":1,"lat":1}]},
                  {"type":"way","id":2,"tags":{"highway":"residential"},"geometry":[]}]
        features,skipped=atlas.osm_features(elements)
        self.assertEqual(features,[])
        self.assertEqual(skipped,{"unclosed_polygon":1,"missing_geometry":1})
        with tempfile.TemporaryDirectory() as folder:
            path=Path(folder)/"missing.osm"
            path.write_text('<osm><node id="1" lat="0" lon="0"/><node id="3" lat="1" lon="1"/><way id="4"><nd ref="1"/><nd ref="2"/><nd ref="3"/><tag k="highway" v="residential"/></way></osm>')
            self.assertEqual(atlas.import_osm(path)[0],[])

    def test_grid_limits_and_empty_surveys_fail(self):
        for size in [0,float("nan"),101]:
            with self.assertRaises(ValueError):atlas.survey(self.roads(),self.bounds,size)
        with self.assertRaises(ValueError):atlas.survey([],self.bounds,5)
        with self.assertRaises(ValueError):atlas.survey(self.roads(),[0,0,.05,.05],1)
        with self.assertRaises(ValueError):atlas.validate_bbox([0,179,1,-179])

    def test_map_chunks_partial_edges_and_attribution(self):
        projected,grid,_=atlas.survey(self.roads(),self.bounds,5)
        sample={"id":"test","name":"Test"}
        document=atlas.export_map(sample,projected,grid,{"attribution":"source attribution"})
        self.assertEqual(document["kind"],"outbreak.map")
        self.assertIn("source attribution",document["authoringJson"])
        cells={}
        for chunk in document["chunks"]:
            self.assertEqual(len(chunk["cells"]),1024)
            for i,tile in enumerate(chunk["cells"]):
                x,y=chunk["x"]*32+i%32,chunk["y"]*32+i//32
                if x>=grid["width"] or y>=grid["height"]:self.assertEqual(tile,0)
                else:cells[x,y]=tile
        self.assertIn(6,cells.values())
        self.assertIn(5,cells.values())
        self.assertNotIn(7,cells.values()) # No made-up interiors or population.

    def test_overpass_failure_is_not_cached(self):
        class Response:
            def __enter__(self):return self
            def __exit__(self,*args):pass
            def read(self,*args):return b'{"remark":"runtime error: timeout","elements":[]}'
        with tempfile.TemporaryDirectory() as folder,patch("urllib.request.urlopen",return_value=Response()):
            sample={"id":"timeout","bbox":self.bounds}
            with self.assertRaises(ValueError):atlas.download(sample,Path(folder),"https://overpass-api.de/api/interpreter")
            self.assertEqual(list(Path(folder).iterdir()),[])

    def test_build_preserves_real_features_and_exports_derived_blocks_separately(self):
        with tempfile.TemporaryDirectory() as folder:
            output=Path(folder)
            sample={"id":"test","name":"Test","bbox":self.bounds}
            atlas.build(sample,self.roads(),{"attribution":"test"},output)
            measured=json.loads((output/"test.geojson").read_text())
            blocks=json.loads((output/"test.blocks.geojson").read_text())
            self.assertEqual(len(measured["features"]),4)
            self.assertEqual(len(blocks["features"]),1)
            self.assertIn("not an official district",blocks["features"][0]["properties"]["tags"]["derived"])

    def test_seeded_buildings_are_repeatable_bounded_and_have_physical_paths(self):
        sample={"id":"test","name":"Test","bbox":self.bounds}
        source={"attribution":"test source"}
        first,metadata,_=generator.generate(sample,self.roads(),source,214,max_buildings=3)
        repeat,_,_=generator.generate(sample,self.roads(),source,214,max_buildings=3)
        other,other_metadata,_=generator.generate(sample,self.roads(),source,215,max_buildings=3)
        self.assertEqual(first,repeat)
        self.assertEqual(len(metadata["buildings"]),3)
        self.assertNotEqual(metadata["buildings"],other_metadata["buildings"])
        cells=generator.unpack(first);width=first["width"]
        for building in metadata["buildings"]:
            x,y=building["door"]
            self.assertEqual(cells[y*width+x],9)
            self.assertTrue(any(cells[(y+dy)*width+x+dx]==7 for dx,dy in ((0,1),(1,0),(0,-1),(-1,0))))
            self.assertTrue(all(cells[py*width+px] in (5,6) for px,py in building["path_cells"]))
            self.assertEqual(sum(abs(a-b) for a,b in zip(building["door"],building["path_cells"][-1])),1)
        projected,grid,_=atlas.survey(self.roads(),self.bounds,5)
        original=generator.unpack(atlas.export_map(sample,projected,grid,source))
        for i,tile in enumerate(original):
            if tile in (5,6,12):self.assertEqual(cells[i],tile)
        self.assertIn("test source",first["authoringJson"])
        self.assertIn("explicit fallback",metadata["footprint_recipe_source"])

    def test_generation_rejects_invalid_seed_limits_and_incomplete_blocks(self):
        sample={"id":"test","name":"Test","bbox":self.bounds}
        for seed in (0,-1,4294967296):
            with self.assertRaises(ValueError):generator.generate(sample,self.roads(),{},seed)
        for limit in (0,257):
            with self.assertRaises(ValueError):generator.generate(sample,self.roads(),{},214,max_buildings=limit)
        with self.assertRaises(ValueError):generator.generate(sample,self.roads()[:1],{},214)


if __name__=="__main__":unittest.main()
