"""Physical authoring checks for the controlled NPC handoff fixture."""
import unittest

import generate_store_validation as fixture
import generate_reference_city as lots
import generate_seeded_city as streets


class StoreValidationGeometry(unittest.TestCase):
    def test_replay_and_live_fixtures(self):
        document,metadata=fixture.build(214)
        self.assertEqual(streets.canonical(document),streets.canonical(fixture.build(214)[0]))
        self.assertEqual(len(metadata["buildings"]),6)
        self.assertEqual(len(document["buildingRooms"]),5)
        self.assertEqual(len(document["shelves"]),12)
        self.assertEqual(len(document["storageBoxes"]),6)
        self.assertEqual(len(document["phoneBlocks"]),2)
        cells=lots.unpack(document)
        self.assertGreater(cells.count(16),0)
        points=[]
        for key in ("shelves","storageBoxes","phoneBlocks"):
            for item in document[key]:
                point=(item["x"],item["y"]);points.append(point)
                self.assertEqual(cells[point[1]*document["width"]+point[0]],7)
        self.assertEqual(len(points),len(set(points)))
        self.assertTrue(all(p["area"] for p in document["restSpots"]))
        self.assertNotEqual(document["id"],fixture.build(215)[0]["id"])

    def test_invalid_seed(self):
        for seed in (0,-1,True,2**32,"214"):
            with self.assertRaises(ValueError):fixture.build(seed)


if __name__=="__main__":unittest.main()
