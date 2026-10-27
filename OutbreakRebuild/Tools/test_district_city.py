"""District density, use hints, composition, topology and exact recipe replay regressions."""
import copy
import json
import unittest
from unittest.mock import patch

import city_atlas as atlas
import generate_district_city as districts
import generate_seeded_city as streets
import generate_reference_city as lots


class DistrictChecks(unittest.TestCase):
    def test_residential_modules_respect_spacing_setbacks_and_replay(self):
        for kind in ("terrace-residential","garden-suburb","suburban-grid"):
            for seed in (1,214,0xffffffff):
                with self.subTest(kind=kind,seed=seed):
                    recipe=districts.district_recipe(kind,seed,{"columns":2,"rows":2,"max_buildings":16,"park_fraction":0},residential_lots=True)
                    document,metadata,_=districts.generate_district(recipe)
                    # A different prior call must not affect this recipe's random streams.
                    districts.generate_district(districts.district_recipe("business-core",17,{"columns":2,"rows":2,"max_buildings":2,"park_fraction":0}))
                    self.assertEqual(streets.canonical(document),streets.canonical(districts.generate_district(copy.deepcopy(recipe))[0]))
                    projected,grid=streets.street_layout(dict(recipe["street_settings"],seed=metadata["stream_seeds"]["streets"]))
                    blocks=[g for g,p in projected if p["category"]=="block"]
                    scale=grid["cell_metres"];gap=recipe["lot_gap_cells"]
                    for i,b in enumerate(metadata["buildings"]):
                        shape=districts.box(b["left"]*scale,b["top"]*scale,(b["left"]+b["width"])*scale,(b["top"]+b["height"])*scale)
                        self.assertTrue(blocks[b["block"]].buffer(-scale*recipe["street_settings"]["setback_cells"]).covers(shape))
                        for other in metadata["buildings"][:i]:
                            self.assertTrue(b["left"]>=other["left"]+other["width"]+gap or other["left"]>=b["left"]+b["width"]+gap or b["top"]>=other["top"]+other["height"]+gap or other["top"]>=b["top"]+b["height"]+gap)
                    districts.assert_usable(document,metadata)

    def test_residential_entrance_targets_street_instead_of_neighbour_spur(self):
        width=20;height=20;cells=bytearray(width*height)
        for x in range(width):cells[2*width+x]=5
        cells[10*width+12]=5
        street={(x,2) for x in range(width)}
        door,path=lots.entrance_path(cells,width,height,8,8,4,4,street)
        self.assertEqual(path[0][1],2)
        self.assertEqual(door[:2],(10,8))
        self.assertTrue(all(not (8<=x<12 and 8<=y<12) for x,y in path))

    def test_impossible_residential_inset_does_not_relax_setback(self):
        recipe=districts.district_recipe("terrace-residential",214,{"columns":2,"rows":2,"block_width_cells":14,"block_height_cells":14,"setback_cells":6,"park_fraction":0},residential_lots=True)
        recipe["footprint_sizes"]=[[20,14]]
        with self.assertRaisesRegex(ValueError,"No safe building"):
            districts.generate_district(recipe)

    def test_six_park_templates_replay_without_house_overlap(self):
        specs=[{"size":size,"variant":variant} for size in ("small","medium","large") for variant in ("cross","loop")]
        recipe=districts.district_recipe("suburban-grid",214,{"columns":3,"rows":2,"block_width_cells":40,"block_height_cells":40,"park_fraction":0,"max_buildings":24},residential_lots=True,parks=specs)
        document,metadata,_=districts.generate_district(recipe)
        self.assertEqual(streets.canonical(document),streets.canonical(districts.generate_district(copy.deepcopy(recipe))[0]))
        self.assertEqual([(p["size"],p["variant"]) for p in metadata["parks"]],[(p["size"],p["variant"]) for p in specs])
        self.assertEqual(metadata["omitted_parks"],[])
        self.assertEqual([p["width"] for p in metadata["parks"]],[6,6,10,10,16,16])
        districts.assert_usable(document,metadata)
        cells=lots.unpack(document);width=document["width"]
        for p in metadata["parks"]:
            self.assertTrue(all(cells[y*width+x] in (0,5) for y in range(p["top"],p["top"]+p["height"]) for x in range(p["left"],p["left"]+p["width"])))

    def test_tight_park_requests_are_reported_and_bad_specs_rejected(self):
        recipe=districts.district_recipe("garden-suburb",1,{"columns":2,"rows":2,"block_width_cells":14,"block_height_cells":14,"park_fraction":0,"max_buildings":2},parks=[{"size":"large","variant":"loop"}])
        _,metadata,_=districts.generate_district(recipe)
        self.assertEqual(metadata["parks"],[])
        self.assertEqual(len(metadata["omitted_parks"]),1)
        recipe["parks"]=[{"size":"huge","variant":"loop"}]
        with self.assertRaises(ValueError):districts.generate_district(recipe)

    def test_park_metadata_is_translated_when_composing_city(self):
        recipe=districts.city_recipe(["suburban-grid","garden-suburb"],214)
        for r in recipe["districts"]:
            r["street_settings"].update(columns=2,rows=2,max_buildings=8,park_fraction=0)
            r["parks"]=[{"size":"small","variant":"loop"}]
        document,metadata,_=districts.compose_city(recipe)
        self.assertEqual(len(metadata["parks"]),2)
        self.assertEqual(len({p["park_id"] for p in metadata["parks"]}),2)
        districts.assert_usable(document,metadata)

    def test_all_open_space_roll_retains_one_buildable_block(self):
        original=streets.street_layout
        def all_open(settings):
            projected,grid=original(settings)
            for geometry,props in projected:
                if props["category"]=="block":props["open_space"]=True
            return projected,grid
        with patch.object(districts.streets,"street_layout",side_effect=all_open):
            document,metadata,_=districts.generate_district(districts.district_recipe("garden-suburb"))
        self.assertTrue(metadata["buildings"])
        self.assertIn("open_space_recovery",metadata)
        districts.assert_usable(document,metadata)

    def test_all_catalogue_types_generate_and_replay_usable_maps(self):
        catalogue=json.loads(districts.CATALOGUE.read_text())
        for kind in catalogue["districts"]:
            with self.subTest(kind=kind):
                recipe=districts.district_recipe(kind,214,{"columns":2,"rows":2,"max_buildings":16,"park_fraction":0})
                document,metadata,svg=districts.generate_district(recipe)
                replay=districts.generate_district(json.loads(json.dumps(recipe)))
                self.assertEqual(streets.canonical(document),streets.canonical(replay[0]))
                self.assertEqual(svg,replay[2]);districts.assert_usable(document,metadata)
                self.assertEqual(metadata["category"],catalogue["districts"][kind]["category"])
                self.assertTrue(metadata["buildings"])
                for b in metadata["buildings"]:
                    self.assertIn(b["use_hint"],recipe["uses"])
                    self.assertTrue(recipe["storeys"][0]<=b["storeys_hint"]<=recipe["storeys"][1])

    def test_dense_urban_uses_more_ground_coverage_than_garden_suburb(self):
        for seed in (214,215):
            dense=districts.generate_district(districts.district_recipe("dense-urban",seed))[1]
            suburb=districts.generate_district(districts.district_recipe("garden-suburb",seed))[1]
            self.assertGreater(dense["actual_building_coverage"],suburb["actual_building_coverage"]*2)

    def test_business_and_residential_intents_are_distinct(self):
        business=districts.generate_district(districts.district_recipe("business-core"))[1]
        homes=districts.generate_district(districts.district_recipe("terrace-residential"))[1]
        self.assertGreater(business["use_counts"].get("office",0),0)
        self.assertGreater(homes["use_counts"].get("terrace-home",0),0)
        self.assertGreater(sum(b["width"]*b["height"] for b in business["buildings"])/len(business["buildings"]),sum(b["width"]*b["height"] for b in homes["buildings"])/len(homes["buildings"])*2)

    def test_mixed_city_has_unique_parcels_connected_portals_and_exact_replay(self):
        recipe=districts.city_recipe(["dense-urban","business-core","terrace-residential","garden-suburb"],214)
        for item in recipe["districts"]:item["street_settings"].update(columns=2,rows=2,max_buildings=12,park_fraction=0)
        document,metadata,_=districts.compose_city(recipe)
        replay=districts.compose_city(json.loads(json.dumps(recipe)))
        self.assertEqual(streets.canonical(document),streets.canonical(replay[0]))
        self.assertEqual(len(metadata["districts"]),4);self.assertEqual(len(metadata["portals"]),4)
        self.assertEqual(sum(1 for segment in metadata["street_segments"] if segment.get("connector")),4)
        ids=[b["parcel_id"] for b in metadata["buildings"]]
        self.assertEqual(len(ids),len(set(ids)))
        self.assertEqual({b["district_index"] for b in metadata["buildings"]},{0,1,2,3})
        districts.assert_usable(document,metadata)
        cells=lots.unpack(document);width=document["width"]
        for portal in metadata["portals"]:
            for x,y in (portal["start_cells"],portal["end_cells"]):self.assertEqual(cells[int(y)*width+int(x)],6)

    def test_city_seed_changes_streets_parcels_and_child_seeds(self):
        a=districts.city_recipe(["garden-suburb","business-campus"],214)
        b=districts.city_recipe(["garden-suburb","business-campus"],215)
        self.assertNotEqual([r["seed"] for r in a["districts"]],[r["seed"] for r in b["districts"]])
        self.assertNotEqual(districts.compose_city(a)[0]["chunks"],districts.compose_city(b)[0]["chunks"])

    def test_incompatible_scales_budgets_rooms_and_changed_profiles_fail(self):
        recipe=districts.city_recipe(["garden-suburb","business-core"])
        recipe["districts"][1]["street_settings"]["cell_metres"]=2
        with self.assertRaises(ValueError):districts.compose_city(recipe)
        recipe=districts.city_recipe(["garden-suburb"]*5)
        for r in recipe["districts"]:r["street_settings"]["max_buildings"]=64
        with self.assertRaises(ValueError):districts.compose_city(recipe)
        recipe=districts.district_recipe("business-core");recipe["footprint_sizes"]=[[20,20]]
        with self.assertRaises(ValueError):districts.generate_district(recipe)
        recipe=districts.district_recipe("dense-urban");recipe["reference_profile_sha256"]="changed"
        with self.assertRaises(ValueError):districts.generate_district(recipe)
        recipe=districts.district_recipe("dense-urban");recipe["district_type"]="../outside"
        with self.assertRaises(ValueError):districts.generate_district(recipe)


if __name__=="__main__":unittest.main()
