using System; // Reject invalid settings before constructing a replacement world.
using System.Collections.Generic; // Cache bounded authored spawn regions instead of scanning during every GUI pass.
using Outbreak.Contracts; // Apply the same explicit mixed-trait fixtures as the quick encounter.
namespace Outbreak.Core // Build scenarios without depending on Unity controls.
{ // Begin repeatable encounter setup.
    public enum ScenarioMap { ShelterArena, ShelterDistrict } // Select authored maps with stable identities.
    public static class ScenarioBuilder // Separate setup validation and placement from running behavior.
    { // Begin immutable authored setup catalog.
        public const int DefaultBats=6;
        private sealed class Definition // Keep cached terrain private so a running world cannot alter future starts.
        { // Begin one map and its nonoverlapping outdoor spawn regions.
            public WorldMap map; public int[] humans, zombies; // Store candidate cells in deterministic row-major order.
        } // Finish a setup definition.
        private static readonly Definition[] definitions = { Define(EncounterScenarios.MixedShelterMap(), 4, 5, 6, 4, 12, 7, 7, 3), Define(EncounterScenarios.ShelterDistrictMap(), 35, 35, 12, 30, 51, 35, 12, 30) }; // Build once, then clone through World construction.
        private static int[] OutdoorCells(WorldMap map, int left, int top, int width, int height) // Keep initial populations outside rooms and solid terrain.
        { // Begin bounded authored region collection.
            List<int> cells = new List<int>(); for (int y = top; y < top + height; y++) for (int x = left; x < left + width; x++) if (map.CanWalk(x, y) && map.Get(x, y) != Terrain.Floor && map.Get(x, y) != Terrain.DoorOpen) cells.Add(y * map.Width + x); return cells.ToArray(); // Walls and entrances never become initial spawn cells.
        } // Finish spawn region collection.
        private static Definition Define(WorldMap map, int hx, int hy, int hw, int hh, int zx, int zy, int zw, int zh) // Bind each map to its authored placement regions.
        { // Begin one catalog entry.
            return new Definition { map = map, humans = OutdoorCells(map, hx, hy, hw, hh), zombies = OutdoorCells(map, zx, zy, zw, zh) }; // Species regions remain spatially separate at the start.
        } // Finish definition construction.
        private static Definition Get(ScenarioMap choice) // Refuse unknown choices before indexing the catalog.
        { // Begin catalog lookup.
            int index = (int)choice; if (index < 0 || index >= definitions.Length) throw new ArgumentOutOfRangeException("choice"); return definitions[index]; // Never substitute another map for an invalid choice.
        } // Finish lookup.
        public static string MapName(ScenarioMap choice) { return Get(choice).map.Name; } // Present the actual authored map name.
        public static int HumanLimit(ScenarioMap choice) { return Math.Min(Rules.DefaultCapacity, Get(choice).humans.Length * Rules.HumansPerCell); } // Respect the human limit on every spawn cell.
        public static int ZombieLimit(ScenarioMap choice) { return Math.Min(Rules.DefaultCapacity, Get(choice).zombies.Length * Rules.BodiesPerCell); } // Keep body crowding separate from human crowding.
        public static int BatLimit(ScenarioMap choice){return Math.Min(MeleeRules.MaxPickups,Get(choice).humans.Length);} // One pickup per authored human-area cell.
        public static string ValidationError(ScenarioMap choice, int humans, int zombies, uint seed, int bats=DefaultBats) // Share the exact start policy between UI and scenario creation.
        { // Begin settings validation.
            Get(choice); if (humans < 0 || zombies < 0 || humans > Rules.DefaultCapacity || zombies > Rules.DefaultCapacity) return "Counts must be between 0 and 1000."; if (humans + zombies < 1 || humans + zombies > Rules.DefaultCapacity) return "Choose 1 to 1000 bodies total."; if (seed == 0) return "Seed must be between 1 and 4294967295."; if (humans > HumanLimit(choice)) return "This map's human spawn area holds " + HumanLimit(choice) + ". Lower the count or choose Shelter district."; if (zombies > ZombieLimit(choice)) return "This map's zombie spawn area holds " + ZombieLimit(choice) + ". Lower the count or choose Shelter district."; if(bats<0 || bats>BatLimit(choice))return "Bats must be between 0 and " + BatLimit(choice) + " for this map."; return null; // Invalid settings produce no world, actors or random draws.
        } // Finish validation.
        public static World Create(ScenarioMap choice, int humans = 24, int zombies = 1, uint seed = 214, int bats=DefaultBats) // Construct the complete candidate before any live screen replacement.
        { // Begin deterministic candidate creation.
            string error = ValidationError(choice, humans, zombies, seed, bats); if (error != null) throw new ArgumentException(error); Definition definition = Get(choice); World world = new World(definition.map, seed, Rules.DefaultCapacity, true, hunger: true); world.SeedScenarioShelves(); PlaceBats(world,definition.humans,bats,seed); Spawn(world, definition.humans, Species.Human, humans); Spawn(world, definition.zombies, Species.Zombie, zombies); SaveDocument saved = world.Capture(); for (int i = 0; i < humans; i++) { int stat = i % 3 == 0 ? 1 : i % 3 == 1 ? 3 : 5; saved.actors[i].strength = stat; saved.actors[i].perception = stat; saved.actors[i].charisma = stat; saved.actors[i].bravery = Rules.Bravery(stat, stat, stat); } return World.Restore(saved); // Keep provisional mixed traits explicit rather than claiming weighted population generation.
        } // Finish candidate creation.
        private static void PlaceBats(World world,int[] sites,int count,uint seed)
        {
            if(count==0)return;int[] cells=(int[])sites.Clone();RandomStream supplies=new RandomStream(seed^0xB5297A4Du); // Keep supplies independent of actor and reaction randomness; never shuffle the shared template.
            for(int i=0;i<count;i++){int pick=i+supplies.Below(cells.Length-i),key=cells[pick];cells[pick]=cells[i];cells[i]=key;if(!world.TryPlaceBat(key%world.Width,key/world.Width))throw new InvalidOperationException("Authored supply area could not place the requested bats.");}
        }
        private static void Spawn(World world, int[] cells, Species species, int count) // Spread one body per cell before adding another occupancy layer.
        { // Begin complete requested placement.
            for (int i = 0; i < count; i++) { int key = cells[i % cells.Length], id; if (!world.TrySpawn(species, key % world.Width, key / world.Width, out id)) throw new InvalidOperationException("Authored spawn region could not place the requested population."); } // Never publish a partially populated candidate.
        } // Finish placement.
    } // Finish scenario setup catalog.
} // Finish engine-independent encounter setup.
