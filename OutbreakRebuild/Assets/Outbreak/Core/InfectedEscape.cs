using System; // Validate bounded avoidance memory and local planning.
using Outbreak.Contracts; // Keep escape decisions tied to saved human identity and infection state.
namespace Outbreak.Core // Own tactics independently from Unity presentation.
{ // Begin infected escape behavior.
    public sealed partial class World // Extend the existing authoritative world clock.
    { // Begin bounded tactical planning.
        public bool InfectedEscape { get; private set; } // Keep old saves reproducible until an explicit behavior upgrade.
        public const int AvoidMemoryTicks = 25; // Remember an observed zombie cell for five simulation seconds after sight loss.
        private readonly int[] escapeQueue = new int[81], escapeFirst = new int[81], escapeSteps = new int[81]; // Bound a local escape query to nine by nine cells.
        private readonly bool[] escapeSeen = new bool[81]; // Reuse visited storage rather than allocating a navigation history.
        private readonly ActorState[] escapeThreats = new ActorState[Rules.DefaultCapacity], escapeAllies = new ActorState[Rules.DefaultCapacity]; // Cache only opponents and human screens currently visible to this actor.
        private static void ClearAvoidance(ref ActorState actor) { actor.avoidX = 0; actor.avoidY = 0; actor.avoidUntilTick = 0; } // Canonicalize expired memory and conversion cleanup.
        private void ValidateAvoidance() // Reject incompatible or corrupt avoidance records transactionally.
        { // Begin persisted observation validation.
            for (int i = 0; i < count; i++) { ActorState a = actors[i]; if (a.avoidUntilTick == 0) { if (a.avoidX != 0 || a.avoidY != 0) throw new ArgumentException("Inactive avoidance contains stale coordinates."); } else if (!InfectedEscape || a.species != Species.Human || a.turnAtTick <= Tick || !ValidObservedCell(a.avoidX, a.avoidY) || a.avoidUntilTick <= Tick || a.avoidUntilTick - Tick > AvoidMemoryTicks) throw new ArgumentException("Invalid infected avoidance observation."); } // Hidden observations cannot leak into healthy actors or old rule versions.
        } // Finish observation validation.
        private int ReactInfected(int index, ActorState actor, int threatIndex) // Make survival override bravery and social pressure after a bite.
        { // Begin infected decision priority.
            actor.freezeUntilTick = 0; actor.freezeResolved = true; actor.reaction = HumanReaction.Flight; // Do not let an infected human freeze or deliberately charge danger.
            ActorState reference; if (threatIndex >= 0) { reference = actors[threatIndex]; actor.avoidX = reference.x; actor.avoidY = reference.y; actor.avoidUntilTick = Tick + AvoidMemoryTicks; actor.stress = Math.Min(100, actor.stress + HumanRules.CloseStressGain); actor.threatPressure = ObservePressure(actor, reference); } // Refresh memory only from a current sight observation.
            else if (actor.avoidUntilTick > Tick) reference = new ActorState { x = actor.avoidX, y = actor.avoidY, species = Species.Zombie }; // Search for cover relative to the observed cell, never the hidden actor's new position.
            else { actor.stress = Math.Max(0, actor.stress - RecoveryAfterDoorNoise(actor)); actor.threatPressure = 0; actor.reaction = HumanReaction.Aware; actors[index] = actor; return 8; } // Stay put without a known threat instead of randomly wandering back toward zombies.
            actors[index] = actor; return EscapeDirection(actor, reference, threatIndex >= 0); // Commit decision state before choosing a safe route opening.
        } // Finish infected priority handling.
        private static int DistanceSquared(ActorState a, ActorState b) { int x = a.x - b.x, y = a.y - b.y; return x * x + y * y; } // Compare local clearance without floating-point ties.
        private int EscapeScore(ActorState candidate, ActorState actor, int threats, int allies, int steps) // Rank actual occlusion and observed human screens without treating people as solid walls.
        { // Begin bounded safety scoring.
            int clearance = int.MaxValue, exposed = 0; for (int i = 0; i < threats; i++) { clearance = Math.Min(clearance, DistanceSquared(candidate, escapeThreats[i])); if (Visible(candidate, escapeThreats[i])) exposed++; } // Prefer breaking sight to all remembered visible threats rather than approaching another zombie.
            bool screened = false; ActorState threat = escapeThreats[0]; long vx = threat.x - candidate.x, vy = threat.y - candidate.y, length = vx * vx + vy * vy; // Use the primary observed threat for the soft human-screen preference.
            for (int i = 0; i < allies && !screened; i++) { long x = escapeAllies[i].x - candidate.x, y = escapeAllies[i].y - candidate.y, dot = x * vx + y * vy, cross = x * vy - y * vx; screened = dot > 0 && dot < length && cross * cross <= length; } // Reward a visible human between the escape cell and threat, never a human beyond the threat.
            return (threats - exposed) * 1000 + (screened ? 80 : 0) + Math.Min(256, clearance) * 2 - steps * 3; // Strongly prefer obstacle occlusion, then a human screen and more clearance with a short route.
        } // Finish tactical score.
        private int EscapeDirection(ActorState actor, ActorState reference, bool seeingThreat) // Search reachable nearby cover while forbidding steps closer to any observed zombie.
        { // Begin local candidate collection.
            int threats = 1, allies = 0; escapeThreats[0] = reference; // Keep the nearest currently observed or remembered zombie first.
            for (int i = 0; i < count; i++) { ActorState other = actors[i];if(other.health==0)continue; int distance = DistanceSquared(actor, other); if (other.species == Species.Human && other.id != actor.id && distance <= 36 && Visible(actor, other)) escapeAllies[allies++] = other; else if (seeingThreat && other.species == Species.Zombie && other.id != reference.id && distance <= 144 && Visible(actor, other)) escapeThreats[threats++] = other; } // No actual hidden zombie or human location contributes to the query.
            Array.Clear(escapeSeen, 0, escapeSeen.Length); int head = 0, tail = 1, start = 40, left = actor.x - 4, top = actor.y - 4, bestDirection = 8, bestScore = EscapeScore(actor, actor, threats, allies, 0); escapeQueue[0] = start; escapeSeen[start] = true; escapeFirst[start] = 8; escapeSteps[start] = 0; // Include holding current cover as a valid plan.
            while (head < tail) // Visit at most eighty-one local cells per decision.
            { // Begin one safe route expansion.
                int cell = escapeQueue[head++], x = left + cell % 9, y = top + cell / 9; // Decode the bounded search cell.
                for (int d = 0; d < 8; d++) // Keep path ties deterministic without adding random draws.
                { // Begin neighboring candidate validation.
                    int nx = x + dx[d], ny = y + dy[d], lx = nx - left, ly = ny - top; if (lx < 0 || ly < 0 || lx >= 9 || ly >= 9 || !ClearStep(x, y, nx, ny)) continue; int next = ly * 9 + lx; if (escapeSeen[next] || !occupancy.Allows(Key(nx, ny), Species.Human) || ZombieAt(nx, ny)) continue; // Respect geometry, corner clearance and current crowd limits throughout the route.
                    ActorState candidate = actor; candidate.x = nx; candidate.y = ny; bool safe = true; for (int j = 0; j < threats; j++) if (DistanceSquared(candidate, escapeThreats[j]) < DistanceSquared(actor, escapeThreats[j])) { safe = false; break; } if (!safe) continue; // Lateral movement is permitted for cover; a route cannot enter a cell closer than the starting position to an observed threat.
                    escapeSeen[next] = true; escapeSteps[next] = escapeSteps[cell] + 1; escapeFirst[next] = cell == start ? d : escapeFirst[cell]; escapeQueue[tail++] = next; int score = EscapeScore(candidate, actor, threats, allies, escapeSteps[next]); if (score > bestScore) { bestScore = score; bestDirection = escapeFirst[next]; } // Return the first step of the best reachable cover route.
                } // Finish neighbor expansion.
            } // Finish local search.
            return bestDirection; // Hold if no safe improvement exists instead of charging through danger.
        } // Finish bounded escape planning.
    } // Finish the world extension.
} // Finish infected escape.
