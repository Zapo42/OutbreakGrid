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


class BuildingBlueprintChecks(unittest.TestCase):
    def test_all_layout_variants_and_rotations_are_bounded_and_deterministic(self):
        for kind in lots.BUILDING_TYPES:
            for variant in lots.BUILDING_VARIANTS:
                for rotation in range(4):
                    with self.subTest(kind=kind,variant=variant,rotation=rotation):
                        blueprint=lots.building_blueprint(kind,variant,rotation)
                        self.assertTrue(lots.validate_building_blueprint(blueprint))
                        lots.building_blueprint("store","spacious",(rotation+1)%4)
                        self.assertEqual(streets.canonical(blueprint),streets.canonical(lots.building_blueprint(kind,variant,rotation)))
                        document,metadata=districts.building_preview_map(blueprint)
                        cells=lots.unpack(document);width=document["width"]
                        # Independent terrain scan matches the engine's floor-only room separation.
                        pending={i for i,tile in enumerate(cells) if tile==7};components=[]
                        while pending:
                            start=min(pending);pending.remove(start);component={start};queue=[start]
                            while queue:
                                key=queue.pop();x,y=key%width,key//width
                                for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                                    neighbour=ny*width+nx
                                    if neighbour in pending:pending.remove(neighbour);component.add(neighbour);queue.append(neighbour)
                            components.append(component)
                        ox,oy=metadata["origin_cells"]
                        self.assertEqual({frozenset(c) for c in components},{frozenset((y+oy)*width+x+ox for x,y in room["cells"]) for room in blueprint["rooms"]})
                        for door in blueprint["doors"]:
                            x,y=door["cell"];x+=ox;y+=oy
                            self.assertEqual(cells[y*width+x],9)
                            if door["external"]:
                                self.assertTrue(any(cells[ny*width+nx]==5 for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1))))

    def test_house_living_area_and_separate_storage_do_not_introduce_bed(self):
        blueprint=lots.building_blueprint("house")
        rooms={r["id"]:r for r in blueprint["rooms"]}
        self.assertGreater(len(rooms["living"]["cells"]),len(rooms["storage"]["cells"]))
        self.assertEqual(rooms["storage"]["role"],"household-storage")
        self.assertTrue(any(set(d["connects"])=={"living","storage"} for d in blueprint["doors"]))
        self.assertTrue(all(f["room_id"]=="storage" for f in blueprint["fixtures"] if f["role"]=="storage-shelf"))
        self.assertEqual(blueprint["recovery_area"]["room_id"],"living")
        self.assertGreater(len(blueprint["recovery_area"]["cells"]),1)
        self.assertFalse(any(f["role"]=="bed" for f in blueprint["fixtures"]))

    def test_retail_and_distribution_have_distinct_fixture_and_room_roles(self):
        store=lots.building_blueprint("store")
        shelves=[f for f in store["fixtures"] if f["role"]=="sales-shelf"]
        self.assertGreaterEqual(len(shelves),4)
        self.assertTrue(all(f["room_id"]=="front" for f in shelves))
        self.assertTrue(all(f["room_id"]=="rear" for f in store["fixtures"] if f["role"]=="storage-shelf"))
        center=lots.building_blueprint("distribution-center")
        computers=[f for f in center["fixtures"] if f["role"]=="order-computer"]
        self.assertEqual(len(computers),1);self.assertEqual(computers[0]["room_id"],"front")
        pickup=center["interactions"][0]
        self.assertEqual(pickup["fixture_id"],computers[0]["id"])
        self.assertEqual(pickup["approach_cells"],computers[0]["approach_cells"])

    def test_restaurant_has_two_room_specific_entrances_and_opposite_counter_approaches(self):
        for rotation in range(4):
            blueprint=lots.building_blueprint("restaurant-bar",rotation=rotation)
            entrances=[d for d in blueprint["doors"] if d["external"]]
            self.assertEqual(len(entrances),2)
            self.assertEqual({d["connects"][0] for d in entrances},{"front","rear"})
            self.assertEqual(len(blueprint["interactions"]),3)
            for handoff in blueprint["interactions"]:
                cx,cy=handoff["counter_cell"];a,b=handoff["sides"]
                self.assertEqual(a["role"],"waiter");self.assertEqual(b["role"],"cook")
                self.assertEqual([a["cell"][i]+b["cell"][i] for i in (0,1)],[2*cx,2*cy])
                self.assertIn([cx,cy],blueprint["walls"])

    def test_invalid_geometry_blocks_inaccessible_fixture_or_room(self):
        blueprint=lots.building_blueprint("house")
        blueprint["fixtures"][0]["approach_cells"]=[[0,0]]
        with self.assertRaisesRegex(ValueError,"approach"):lots.validate_building_blueprint(blueprint)
        blueprint=lots.building_blueprint("house")
        door=next(d for d in blueprint["doors"] if not d["external"])
        blueprint["doors"].remove(door);blueprint["walls"].append(door["cell"])
        with self.assertRaises(ValueError):lots.validate_building_blueprint(blueprint)
        blueprint=lots.building_blueprint("restaurant-bar")
        blueprint["interactions"][0]["sides"][1]["room_id"]="front"
        with self.assertRaises(ValueError):lots.validate_building_blueprint(blueprint)
        with self.assertRaises(ValueError):lots.building_blueprint("house",rotation=4)

    def test_building_pack_replays_and_declares_inactive_fixture_mechanics(self):
        import tempfile
        from pathlib import Path
        with tempfile.TemporaryDirectory() as a,tempfile.TemporaryDirectory() as b:
            manifest=districts.write_building_pack(a,214)
            self.assertEqual(len(manifest["templates"]),10)
            self.assertEqual(manifest,districts.write_building_pack(b,214))
            for path in Path(a).iterdir():self.assertEqual(path.read_bytes(),(Path(b)/path.name).read_bytes())
            self.assertTrue(any(entry["type"]=="church" for entry in manifest["templates"]))
            for path in Path(a).glob("*.obmap2"):
                doc=json.loads(path.read_text());metadata=json.loads(doc["authoringJson"])
                self.assertEqual(doc["schemaVersion"],1)
                self.assertFalse(metadata["fixtures_materialized"]);self.assertFalse(metadata["mechanics_enabled"])
                self.assertFalse(any(key in doc for key in ("shelves","storageBoxes","restSpots")))


if __name__=="__main__":unittest.main()
