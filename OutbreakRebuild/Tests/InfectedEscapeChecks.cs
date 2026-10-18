using System;
using System.IO; // Read independent previous-release continuation fixtures. // Validate observed avoidance deadlines and action priority.
using Outbreak.Contracts; // Construct explicit bitten and saved human fixtures.
using Outbreak.Core; // Exercise production escape planning.
using Outbreak.Persistence; // Compare the entire saved continuation.
public static partial class Program // Extend the existing core acceptance runner.
{ // Begin infected regression checks.
    private static int Separation(ActorState a, ActorState b) { int x = a.x - b.x, y = a.y - b.y; return x * x + y * y; } // Compare distance from an observed threat independently from the planner.
    private static void CheckInfectedEscapeCases() // Execute real infected decisions rather than checking labels alone.
    { // Begin infected acceptance cases.
        Check("Bitten humans override courage and freezing with escape", () => // Reproduce a brave, stressed human otherwise capable of approaching a zombie.
        { // Begin priority fixture.
            World world = EncounterScenarios.BittenEscape(); SaveDocument saved = world.Capture(); saved.actors[0].groupSupport = 6; saved.actors[0].supportUntilTick = 10; saved.actors[0].freezeUntilTick = 5; saved.actors[0].reaction = HumanReaction.Freeze; world = World.Restore(saved); ActorState before = world.ActorAt(0), zombie = world.ActorAt(1); Run(world, 1); ActorState after = world.ActorAt(0); Require(world.InfectedEscape && after.reaction == HumanReaction.Flight && after.freezeUntilTick == 0 && after.turnAtTick == before.turnAtTick && Separation(after, zombie) >= Separation(before, zombie), "Bitten human froze, fought, approached or reset its infection."); Require(after.avoidUntilTick == 26 && after.avoidX == zombie.x && after.avoidY == zombie.y, "Escape failed to remember only the observed zombie cell."); Inspect(world); // Identity and authoritative crowd counts remain intact.
        }); // Finish infection priority acceptance.
        Check("A real bite immediately replaces fight or freeze with escape", () => // Do not wait for the bitten human's next movement opportunity to record its attacker.
        { // Begin direct contact fixture.
            World world = new World(new WorldMap(2, 1), 1, 1000, true); int id; world.TrySpawn(Species.Zombie, 0, 0, out id); world.TrySpawn(Species.Human, 1, 0, out id); Run(world, 1); ActorState human = world.ActorAt(1); Require(human.turnAtTick > world.Tick && human.reaction == HumanReaction.Flight && human.freezeUntilTick == 0 && human.avoidX == 0 && human.avoidY == 0 && human.avoidUntilTick == 26, "Bite left a stale confrontation response or failed to record the actual attacker."); // The human's normal decision is on tick two, so the priority must be applied by the bite itself.
        }); // Finish immediate infection reaction.
        Check("Bitten humans prefer reachable obstacle cover", () => // Prefer an actual line-of-sight break over a longer open retreat.
        { // Begin the short-wall fixture.
            World world = EncounterScenarios.BittenEscape(); Run(world, 1); ActorState human = world.ActorAt(0); Require(human.y == 3 && (human.x == 4 || human.x == 5), "Infected escape did not choose the reachable wall screen."); World restored = Documents.ParseSave(State(world)); Run(world, 20); Run(restored, 20); Require(State(world) == State(restored), "Cover navigation or avoidance memory diverged after reload."); Inspect(world); // Continue through the zombie losing sight without consulting its hidden location.
        }); // Finish physical-cover acceptance.
        Check("Bitten humans use a human screen without approaching zombies", () => // Choose a reachable position with a person between the infected human and threat.
        { // Begin a controlled social-screen fixture.
            World world = new World(new WorldMap(12, 9), 4, 1000, true); int id; world.TrySpawn(Species.Human, 5, 4, out id); world.TrySpawn(Species.Human, 5, 6, out id); world.TrySpawn(Species.Zombie, 8, 4, out id); SaveDocument saved = world.Capture(); saved.actors[0].turnAtTick = 80; world = World.Restore(saved); Run(world, 1); ActorState human = world.ActorAt(0); Require(human.y > 4 && human.x <= 5 && Separation(human, world.ActorAt(2)) >= 9, "Human-screen preference attracted the bitten human toward the zombie or ignored the intervening person."); Inspect(world); // The ally is a tactical screen rather than an impassable shield.
        }); // Finish human-screen acceptance.
        Check("Infected avoidance survives sight loss, pause and exact expiry", () => // Prevent ordinary wandering from returning the human to danger after a bite.
        { // Begin memory fixture.
            WorldMap map = new WorldMap(7, 3); for (int y = 0; y < 3; y++) map.Set(3, y, Terrain.Wall); World world = new World(map, 3, 1000, true); int id; world.TrySpawn(Species.Human, 1, 1, out id); world.TrySpawn(Species.Zombie, 5, 1, out id); SaveDocument saved = world.Capture(); saved.actors[0].turnAtTick = 80; saved.actors[0].avoidX = 5; saved.actors[0].avoidY = 1; saved.actors[0].avoidUntilTick = 13; world = World.Restore(saved); string before = State(world); TickClock clock = new TickClock(); clock.Advance(100, world.Step); Require(State(world) == before, "Pause changed infected escape memory."); Run(world, 13); Require(world.ActorAt(0).avoidUntilTick == 0 && world.ActorAt(0).avoidX == 0, "Avoidance did not expire between human opportunities."); ActorState held = world.ActorAt(0); Run(world, 10); Require(world.ActorAt(0).x == held.x && world.ActorAt(0).y == held.y, "Bitten human wandered after cover memory expired."); // An unseen zombie does not get tracked by its actual new position.
        }); // Finish memory acceptance.
        Check("Infected escape avoids every currently observed zombie", () => // Prevent safety relative to one threat from attracting the human toward another.
        { // Begin a two-threat fixture.
            World world = new World(new WorldMap(9, 9), 4, 1000, true); int id; world.TrySpawn(Species.Human, 4, 4, out id); world.TrySpawn(Species.Zombie, 7, 4, out id); world.TrySpawn(Species.Zombie, 4, 1, out id); SaveDocument saved = world.Capture(); saved.actors[0].turnAtTick = 80; world = World.Restore(saved); ActorState before = world.ActorAt(0), first = world.ActorAt(1), second = world.ActorAt(2); Run(world, 1); Require(Separation(world.ActorAt(0), first) >= Separation(before, first) && Separation(world.ActorAt(0), second) >= Separation(before, second), "Escape approached a second visible zombie."); Inspect(world); // Compare against the positions observed before either zombie's opportunity.
        }); // Finish multi-threat acceptance.
        Check("Previous cautious saves retain their independent continuation", () => // Allow explicit updates without silently changing a loaded run.
        { // Begin prototype-four compatibility.
            string folder = Path.Combine(AppContext.BaseDirectory, "Fixtures"); World world = Documents.ParseSave(File.ReadAllText(Path.Combine(folder, "encounter-v4-input.json"))); Require(world.CautiousMovement && !world.InfectedEscape, "Old cautious save silently gained escape rules."); Run(world, 100); World expected = Documents.ParseSave(File.ReadAllText(Path.Combine(folder, "encounter-v4-expected.json"))); Require(State(world) == State(expected), "Prior cautious-rule continuation drifted."); // The fixture was executed with commit 45ce8e8 source.
        }); // Finish old-rule compatibility.
        Check("Invalid avoidance saves reject and conversion clears memory", () => // Bound the new saved fields and species transitions.
        { // Begin persisted-state checks.
            World world = EncounterScenarios.BittenEscape(); Run(world, 1); SaveDocument saved = world.Capture(); saved.actors[0].avoidUntilTick = saved.tick; Reject(() => World.Restore(saved)); saved = world.Capture(); saved.actors[0].avoidUntilTick = saved.tick + 26; Reject(() => World.Restore(saved)); saved = world.Capture(); saved.actors[0].avoidX = -1; Reject(() => World.Restore(saved)); saved = world.Capture(); saved.actors[0].turnAtTick = 0; Reject(() => World.Restore(saved)); saved = world.Capture(); saved.rulesVersion = EncounterRules.CautiousVersion; Reject(() => World.Restore(saved)); // Incompatible rules and observations cannot silently load.
            saved = world.Capture(); saved.actors[0].turnAtTick = saved.tick + 1; world = World.Restore(saved); Run(world, 1); Require(world.ActorAt(0).species == Species.Zombie && world.ActorAt(0).avoidUntilTick == 0 && world.ActorAt(0).avoidX == 0 && world.ActorAt(0).avoidY == 0, "Conversion retained human avoidance state."); Require(State(world) == State(Documents.ParseSave(State(world))), "Converted escape state did not round-trip."); // Keep turning on the original authoritative deadline.
        }); // Finish save and conversion acceptance.
    } // Finish new scenarios.
} // Finish infected escape checks.
