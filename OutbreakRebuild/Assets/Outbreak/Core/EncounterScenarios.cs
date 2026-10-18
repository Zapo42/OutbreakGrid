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
            World world = new World(map, 214, 1000, true, humanReactions: false); int id; world.TrySpawn(Species.Zombie, 2, 3, out id); world.TrySpawn(Species.Human, 4, 3, out id); return world; // Stable IDs stagger pursuit and fleeing so the human breaks sight at tick seven.
        } // Finish reproducible scenario setup.
        public static World HumanResponse(HumanReaction reaction, bool group = false) // Isolate the three courage outcomes with reviewable natural SPECIAL values.
        { // Begin a controlled human scenario.
            if (reaction != HumanReaction.Fight && reaction != HumanReaction.Freeze && reaction != HumanReaction.Flight) throw new System.ArgumentException("Choose a courage response fixture."); // Reject labels that do not define a controlled threshold case.
            WorldMap map = new WorldMap(14, 10, group ? "Group support test" : reaction + " reaction test", "human-response-1"); // Keep the scenario compact and named in saved maps.
            for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++) if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1) map.Set(x, y, Terrain.Wall); // Bound movement with visible terrain.
            World world = new World(map, 214, 1000, true); int id; world.TrySpawn(Species.Human, 5, 5, out id); if (group) { world.TrySpawn(Species.Human, 4, 4, out id); world.TrySpawn(Species.Human, 4, 6, out id); } world.TrySpawn(Species.Zombie, 9, 5, out id); // Observe a single threat from four cells away before contact.
            SaveDocument saved = world.Capture(); if (reaction == HumanReaction.Fight && !group) saved.actors[0].stress = 41; for (int i = 0; i < saved.actors.Length - 1; i++) { int stat = i > 0 || reaction == HumanReaction.Fight ? 5 : reaction == HumanReaction.Flight ? 1 : 3; saved.actors[i].strength = stat; saved.actors[i].perception = stat; saved.actors[i].charisma = stat; saved.actors[i].bravery = Rules.Bravery(stat, stat, stat); } return World.Restore(saved); // Derive bravery from explicit stats instead of hardcoding a displayed trait.
        } // Finish reaction scenario creation.
    } // Finish scenario construction.
} // Finish engine-independent fixtures.
