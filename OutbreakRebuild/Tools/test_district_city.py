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
