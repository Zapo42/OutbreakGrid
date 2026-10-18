using System; // Compare geometric movement and visual timing precisely.
using System.IO; // Load independent previous-release saves.
using Outbreak.Contracts; // Inspect cautious stress and saved reaction values.
using Outbreak.Core; // Exercise actual decision and collision code.
using Outbreak.Persistence; // Verify complete deterministic continuation.
using Outbreak.UnityAdapter; // Test the real presentation helper without native Unity rendering.
public static partial class Program // Extend the same executable acceptance suite.
{ // Begin movement regression acceptance.
    private static void CheckMovementCases() // Pin the reported cautious approach and gridlike cadence regressions.
    { // Begin new behavior checks.
        Check("Wary supported humans hold distance instead of charging", () => // Reproduce a positive courage margin while stress is Aware or Cautious.
        { // Begin explicit stress cases.
            foreach (int stress in new[] { 1, 20, 22, 36 }) { World world = EncounterScenarios.HumanResponse(HumanReaction.Fight); SaveDocument saved = world.Capture(); saved.actors[0].stress = stress; saved.actors[0].groupSupport = 6; saved.actors[0].supportUntilTick = 10; world = World.Restore(saved); Run(world, 1); ActorState human = world.ActorAt(0); Require(world.CautiousMovement && human.x == 5 && human.y == 5 && human.reaction == HumanReaction.Aware && HumanRules.Margin(human) > 0, "Supported wary human charged the zombie."); World restored = Documents.ParseSave(State(world)); Run(world, 20); Run(restored, 20); Require(State(world) == State(restored), "Wary decisions changed after save/load."); Inspect(world); } // Social support cannot turn low-intensity awareness into automatic pursuit.
        }); // Finish the reported approach regression.
        Check("Cautious humans back away nearby and defend on contact", () => // Keep watching, retreat and defensive shove distinct from chasing.
        { // Begin nearby threat cases.
            World world = new World(new WorldMap(8, 1), 3, 1000, true); int id; world.TrySpawn(Species.Human, 2, 0, out id); world.TrySpawn(Species.Zombie, 5, 0, out id); SaveDocument saved = world.Capture(); saved.actors[0].stress = 22; saved.actors[0].bravery = 3; world = World.Restore(saved); Run(world, 1); Require(world.ActorAt(0).x == 1 && world.ActorAt(0).reaction == HumanReaction.Aware, "Cautious human did not back away from a nearby threat."); // Aware is not a disguised charge action.
            world = new World(new WorldMap(5, 1), 3, 1000, true); world.TrySpawn(Species.Human, 1, 0, out id); world.TrySpawn(Species.Zombie, 2, 0, out id); saved = world.Capture(); saved.actors[0].stress = 22; saved.actors[0].bravery = 3; world = World.Restore(saved); Run(world, 1); Require(world.ActorAt(0).x == 1 && world.ActorAt(1).staggerUntilTick > world.Tick && world.ActorAt(1).x == 3, "Cautious defense required advancing or failed to shove on contact."); Inspect(world); // Defensive contact remains available at positive courage margin.
        }); // Finish defensive awareness acceptance.
        Check("Direct retreat and pursuit avoid gratuitous diagonal zigzags", () => // Test actual cell choices rather than a rendering-only cosmetic change.
        { // Begin horizontal alignment fixture.
            World world = new World(new WorldMap(30, 20), 2, 1000, true); int id; world.TrySpawn(Species.Human, 10, 10, out id); world.TrySpawn(Species.Zombie, 15, 10, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = -3; world = World.Restore(saved); Run(world, 1); Require(world.ActorAt(0).x == 9 && world.ActorAt(0).y == 10, "Retreat preferred a gratuitous diagonal for its larger cell displacement."); Run(world, 1); Require(world.ActorAt(1).x == 14 && world.ActorAt(1).y == 10, "Pursuit chose an equivalent zigzag over the aligned route."); // Both decisions should keep a straight horizontal line in open terrain.
            WorldMap map = new WorldMap(3, 3); map.Set(1, 0, Terrain.Wall); map.Set(0, 1, Terrain.Wall); world = new World(map, 2, 1000, true); world.TrySpawn(Species.Zombie, 0, 0, out id); world.TrySpawn(Species.Human, 1, 1, out id); Run(world, 50); Require(world.ActorAt(0).x == 0 && world.ActorAt(0).y == 0 && world.ActorAt(1).turnAtTick == 0, "Direct-route preference broke corner collision or sight."); // Changing tie order must never bypass geometry.
        }); // Finish steering acceptance.
        Check("Visual motion spans the movement interval without wall shortcuts", () => // Exercise the actual Unity adapter's bounded interpolation data.
        { // Begin full-cadence motion checks.
            MovementPresentation motion = new MovementPresentation(); motion.Snap(0, 0, 0); motion.Observe(1, 0, 1); Require(Math.Abs(motion.X(2) - 0.2) < 1e-9 && Math.Abs(motion.X(4) - 0.6) < 1e-9 && motion.X(6) == 1, "Presentation still completes a move in just one tick."); motion.Observe(2, 0, 6); Require(motion.X(6) == 1 && Math.Abs(motion.X(7) - 1.2) < 1e-9, "Ordinary consecutive moves pause or jump between segments."); // Preserve steady travel across ordinary opportunities.
            motion.Observe(2, 1, 7, false); Require(motion.X(7) == 2 && motion.Y(7) == 1, "Unsafe compound corner movement was smoothed through a wall."); motion.Observe(6, 1, 8); Require(motion.X(8) == 6, "Compound displacement animated over unverified intermediate terrain."); motion.Snap(4, 3, 9); Require(motion.X(9) == 4 && motion.Y(9) == 3, "Pause or manual step did not show exact canonical state."); // Safety takes precedence for interruptions and debug stepping.
            for (int id = 1; id <= 1000; id++) Require(Math.Abs(MovementPresentation.OffsetX(id)) <= 0.09 && Math.Abs(MovementPresentation.OffsetY(id)) <= 0.09, "Stable within-cell offsets escaped the debug footprint margin."); // Offsets cannot carry a body across its cell boundary.
        }); // Finish presentation checks.
        Check("Prototype-three reaction saves keep their independent continuation", () => // Preserve prior saves while using new cautious rules for new encounters.
        { // Begin previous-release compatibility acceptance.
            string fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures"); World world = Documents.ParseSave(File.ReadAllText(Path.Combine(fixtures, "encounter-v3-input.json"))); Require(world.HumanReactions && !world.CautiousMovement && world.Capture().rulesVersion == EncounterRules.ReactionVersion, "Old reaction save silently changed movement rules."); Run(world, 100); World expected = Documents.ParseSave(File.ReadAllText(Path.Combine(fixtures, "encounter-v3-expected.json"))); Require(State(world) == State(expected), "Previous reaction continuation drifted."); // Compare against source executed from commit ee3e24d, not the new implementation.
        }); // Finish saved-rules regression.
    } // Finish all movement cases.
} // Finish movement acceptance.
