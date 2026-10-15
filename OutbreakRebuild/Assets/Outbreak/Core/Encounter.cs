using System; // Keep encounter behavior independent of Unity.
using Outbreak.Contracts; // Preserve actor identities and portable infection state.
namespace Outbreak.Core // Execute all encounter work through the authoritative world clock.
{ // Begin the explicitly versioned prototype.
    public static class EncounterRules // Expose provisional tuning rather than claiming final combat rules.
    { // Begin prototype constants measured in logical cells and ticks.
        public const string Version = "encounter-prototype-1"; // Separate changed behavior from foundation saves.
        public const int SightRadius = 12, MoveInterval = 5, BiteDamage = 10, BaseTurnDelay = 50, EnduranceDelay = 5; // Prototype: one move/second, bite on contact, ten seconds plus END delay.
        public const int MaxTurnDelay = BaseTurnDelay + 10 * EnduranceDelay; // Bound persisted infection deadlines using the supported stat range.
    } // Finish prototype tuning.
    public sealed partial class World // Extend the same authoritative owner without adding engine dependencies.
    { // Begin encounter behavior.
        public bool Encounter { get; private set; } // Keep scenario mode immutable after construction.
        private readonly int[] routeQueue = new int[625], routeFirst = new int[625], routeSeen = new int[625]; // Bound reusable navigation storage to the local 25 by 25 sight region.
        private int routeStamp; // Reuse visited storage without retaining a history of queries.
        private bool ClearStep(int x, int y, int nx, int ny) // Share the same solid-corner rule between sight, pursuit and contact.
        { // Begin geometry validation.
            return map.CanWalk(nx, ny) && (nx == x || ny == y || (map.CanWalk(nx, y) && map.CanWalk(x, ny))); // Never cross a solid barrier or cut its corner.
        } // Finish geometry validation.
        private bool Visible(ActorState from, ActorState to) // Trace a bounded ray through solid collision cells.
        { // Begin deterministic integer line traversal.
            int x = from.x, y = from.y, ax = Math.Abs(to.x - x), ay = Math.Abs(to.y - y), sx = Math.Sign(to.x - x), sy = Math.Sign(to.y - y), error = ax - ay; // Avoid floating-point visibility disagreements.
            while (x != to.x || y != to.y) // Visit every crossed step, including diagonal side clearance.
            { // Begin one ray step.
                int twice = 2 * error, nx = x, ny = y; if (twice > -ay) { error -= ay; nx += sx; } if (twice < ax) { error += ax; ny += sy; } // Follow a fixed Bresenham tie order.
                if (!ClearStep(x, y, nx, ny)) return false; x = nx; y = ny; // Stop at the first solid obstruction.
            } // Finish ray traversal.
            return true; // The destination is visible under the prototype collision model.
        } // Finish sight tracing.
        private int NearestVisible(ActorState actor, Species wanted) // Select only locally visible targets, breaking ties by stable identity.
        { // Begin a bounded-range population scan.
            int best = -1, distance = EncounterRules.SightRadius * EncounterRules.SightRadius + 1; // Use a circular twelve-cell sight region.
            for (int i = 0; i < count; i++) // Actors remain in ascending identity order.
            { // Begin target examination.
                ActorState other = actors[i]; if (other.species != wanted) continue; int x = other.x - actor.x, y = other.y - actor.y, squared = x * x + y * y; // Consider only the opposing species.
                if (squared < distance && Visible(actor, other)) { distance = squared; best = i; } // Equal distances retain the earlier identity.
            } // Finish target examination.
            return best; // Minus one means there is no visible opponent.
        } // Finish target acquisition.
        private bool ZombieAt(int x, int y) // Enforce human blocking by an unstaggered zombie in this prototype.
        { // Begin a local occupancy query.
            for (int i = 0; i < count; i++) if (actors[i].species == Species.Zombie && actors[i].x == x && actors[i].y == y) return true; return false; // No staggering system exists yet, so every zombie blocks entry.
        } // Finish blocking query.
        private bool MayMove(ActorState actor, int direction) // Combine terrain and live occupancy without mutating state.
        { // Begin candidate validation.
            int nx = actor.x + dx[direction], ny = actor.y + dy[direction]; // Translate one neighboring step.
            return ClearStep(actor.x, actor.y, nx, ny) && occupancy.Allows(Key(nx, ny), actor.species) && (actor.species != Species.Human || !ZombieAt(nx, ny)); // Humans cannot walk through zombies.
        } // Finish candidate validation.
        private int Route(ActorState actor, ActorState target) // Find a shortest eight-neighbor route within a bounded local query.
        { // Begin reusable breadth-first search.
            if (routeStamp == int.MaxValue) { Array.Clear(routeSeen, 0, routeSeen.Length); routeStamp = 0; } routeStamp++; // Safely recycle the finite generation counter.
            int left = actor.x - 12, top = actor.y - 12, start = 12 * 25 + 12, head = 0, tail = 1; routeQueue[0] = start; routeSeen[start] = routeStamp; routeFirst[start] = 8; // Anchor the search window to the moving actor.
            while (head < tail) // Never visit more than 625 cells in one query.
            { // Begin one expanded navigation cell.
                int cell = routeQueue[head++], x = left + cell % 25, y = top + cell / 25; if (x == target.x && y == target.y) return routeFirst[cell]; // Return the first step of the shortest discovered route.
                for (int d = 0; d < 8; d++) // Use a stable clockwise neighbor order.
                { // Begin candidate expansion.
                    int nx = x + dx[d], ny = y + dy[d], lx = nx - left, ly = ny - top; if (lx < 0 || ly < 0 || lx >= 25 || ly >= 25 || !ClearStep(x, y, nx, ny)) continue; // Stay inside the bounded terrain query.
                    int next = ly * 25 + lx; if (routeSeen[next] == routeStamp || !occupancy.Allows(Key(nx, ny), actor.species)) continue; // Respect current crowd capacity on each path cell.
                    routeSeen[next] = routeStamp; routeFirst[next] = cell == start ? d : routeFirst[cell]; routeQueue[tail++] = next; // Retain only the first direction and finite frontier.
                } // Finish candidate expansion.
            } // Finish bounded search.
            return 8; // Wait when no route is available instead of walking through a barrier.
        } // Finish navigation.
        private void StepEncounter() // Keep infection, conversion, bites and movement on the same logical clock.
        { // Begin a complete encounter tick.
            for (int i = 0; i < count; i++) // Convert every due human before movement or target selection.
            { // Begin deadline processing.
                ActorState actor = actors[i]; if (actor.turnAtTick == 0 || actor.turnAtTick > Tick) continue; // Ignore healthy humans and future deadlines.
                occupancy.Change(Key(actor.x, actor.y), Species.Human, -1); actor.species = Species.Zombie; actor.turnAtTick = 0; actor.charisma = 0; actor.intelligence = 0; actor.strength = Math.Max(1, actor.strength); actor.perception = Math.Max(1, actor.perception); actors[i] = actor; occupancy.Change(Key(actor.x, actor.y), Species.Zombie, 1); // Preserve identity, position, health and inherited physical stats while updating crowd counts.
            } // Finish turning deadlines.
            for (int i = 0; i < count; i++) // Process stable identity order for reproducible encounters.
            { // Begin one actor decision.
                ActorState actor = actors[i]; if (Tick % EncounterRules.MoveInterval != actor.id % EncounterRules.MoveInterval) continue; // Stagger movement and contact opportunities over ticks.
                int targetIndex = NearestVisible(actor, actor.species == Species.Zombie ? Species.Human : Species.Zombie), direction = 8; // Reacquire locally rather than using global knowledge.
                if (targetIndex >= 0) // Chase or retreat only when an opponent is visible.
                { // Begin encounter response.
                    ActorState target = actors[targetIndex]; int distance = Math.Max(Math.Abs(target.x - actor.x), Math.Abs(target.y - actor.y)); // Contact includes the same cell and unobstructed adjacent cells.
                    if (actor.species == Species.Zombie && distance <= 1) // Use a deterministic contact bite until grabbing and evasion are implemented.
                    { // Begin one bite opportunity.
                        if (target.turnAtTick == 0) { target.health = Math.Max(1, target.health - EncounterRules.BiteDamage); target.turnAtTick = Tick + EncounterRules.BaseTurnDelay + target.endurance * EncounterRules.EnduranceDelay; actors[targetIndex] = target; } continue; // A later bite cannot reset an existing infection deadline.
                    } // Finish contact handling.
                    if (actor.species == Species.Zombie) direction = Route(actor, target); // Follow a collision-safe shortest local route to the visible human.
                    else // Retreat from the nearest observed threat without granting full planned fear behavior.
                    { // Begin prototype fleeing.
                        int best = (actor.x - target.x) * (actor.x - target.x) + (actor.y - target.y) * (actor.y - target.y); // Prefer staying over moving closer to danger.
                        for (int d = 0; d < 8; d++) if (MayMove(actor, d)) { int x = actor.x + dx[d] - target.x, y = actor.y + dy[d] - target.y, score = x * x + y * y; if (score > best) { best = score; direction = d; } } // Stable ties retain the first equally safe route.
                    } // Finish prototype fleeing.
                } // Finish encounter response.
                else if (actor.species == Species.Human) direction = random.Below(9); // Humans wander without a visible threat; zombies wait without omniscient pursuit.
                if (direction == 8 || !MayMove(actor, direction)) continue; occupancy.Change(Key(actor.x, actor.y), actor.species, -1); actor.x += dx[direction]; actor.y += dy[direction]; actors[i] = actor; occupancy.Change(Key(actor.x, actor.y), actor.species, 1); // Commit geometry and population counts together.
            } // Finish actor decisions.
        } // Finish authoritative encounter tick.
    } // Finish the world extension.
} // Finish the engine-independent prototype.
