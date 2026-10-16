using Outbreak.Contracts; // Define reproducible scenarios with real portable actor records.
namespace Outbreak.Core // Share the exact obstacle fixture between the shell and acceptance checks.
{ // Begin encounter scenario construction.
    public static class EncounterScenarios // Keep fixture creation separate from simulation decisions.
    { // Begin controlled scenarios.
        public static World CornerSearch() // Demonstrate losing sight at a right-angle corridor and searching the last seen cell.
        { // Begin authored corner fixture.
            WorldMap map = new WorldMap(7, 8, "Corner pursuit test", "corner-search-1"); // Keep the entire scenario visible at the minimum camera zoom.
            for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++) map.Set(x, y, Terrain.Wall); // Begin with a solid boundary rather than invisible escape routes.
            for (int x = 1; x <= 5; x++) map.Set(x, 3, Terrain.Floor); for (int y = 3; y <= 6; y++) map.Set(5, y, Terrain.Floor); // Carve the two legs of the right-angle corridor.
            World world = new World(map, 214, 1000, true); int id; world.TrySpawn(Species.Zombie, 2, 3, out id); world.TrySpawn(Species.Human, 4, 3, out id); return world; // Stable IDs stagger pursuit and fleeing so the human breaks sight at tick seven.
        } // Finish reproducible scenario setup.
    } // Finish scenario construction.
} // Finish engine-independent fixtures.
