using System; // Validate recent hearing without publishing malformed saved state.
using Outbreak.Contracts; // Persist the heard impact cell separately from seen zombie knowledge.
namespace Outbreak.Core // Resolve local impacts through the authoritative tick and terrain.
{ // Begin door-impact hearing.
    public static class DoorHearingRules // Publish provisional acoustic and reaction tuning.
    { // Begin bounded sensory constants.
        public const int Radius = 8, MemoryTicks = 25, StressGain = 8; // An impact travels at most eight cardinal cells and remains recent for five simulation seconds.
    } // Finish hearing tuning.
    public sealed partial class World // Reuse terrain and actor ownership without an unbounded event history.
    { // Begin local sound propagation.
        private readonly int[] soundQueue = new int[289], soundDistance = new int[289]; // Reuse one seventeen-by-seventeen frontier per sound.
        private static void ClearDoorHearing(ref ActorState actor) { actor.heardDoorX = 0; actor.heardDoorY = 0; actor.heardDoorUntilTick = 0; } // Canonicalize expired or converted hearing.
        private bool HasRecentDoorNoise(ActorState actor) { return DoorHearing && actor.heardDoorUntilTick > Tick; } // Never infer noise from a hidden attacker's current state.
        private int RecoveryAfterDoorNoise(ActorState actor) { return (HasRecentDoorNoise(actor) || HasRecentWarning(actor)) ? 0 : HumanRules.Recovery; } // Resume ordinary recovery after recent impacts and warnings become stale.
        private void ValidateDoorHearing() // Reject unsupported, stale or impossible heard-source records.
        { // Begin hearing validation after the rest of the actor state is restored.
            for (int i = 0; i < count; i++) { ActorState a = actors[i]; if (a.heardDoorUntilTick == 0) { if (a.heardDoorX != 0 || a.heardDoorY != 0) throw new ArgumentException("Inactive hearing contains stale coordinates."); } else if (!DoorHearing || (a.species != Species.Human && !ZombieSoundInvestigation) || a.heardDoorUntilTick <= Tick || a.heardDoorUntilTick - Tick > DoorHearingRules.MemoryTicks || !IsDoor(a.heardDoorX, a.heardDoorY)) throw new ArgumentException("Invalid door-impact hearing."); } // Do not require listeners to remain at the position where they originally heard the event.
        } // Finish hearing validation.
        private int InvestigateDoorNoise(int index, ref ActorState actor) // Travel to the heard door without inventing a human target.
        { // Begin one investigation opportunity.
            if (actor.x == actor.heardDoorX && actor.y == actor.heardDoorY) { ClearDoorHearing(ref actor); actors[index] = actor; return 8; } return Route(actor, new ActorState { x = actor.heardDoorX, y = actor.heardDoorY }, true); // A closed door remains solid and cannot be attacked from sound knowledge alone.
        } // Finish sound investigation.
        private void PrepareSoundReach(int sourceX, int sourceY) // Bound propagation from an actual sound source.
        { // Begin one bounded acoustic query.
            for (int i = 0; i < soundDistance.Length; i++) soundDistance[i] = -1; int left = sourceX - 8, top = sourceY - 8, start = 144, head = 0, tail = 1; soundDistance[start] = 0; soundQueue[0] = start; // Anchor the frontier on the actual source cell.
            while (head < tail) // Visit each local cell at most once, up to a fixed eight-step travel budget.
            { // Begin one acoustic expansion.
                int cell = soundQueue[head++], distance = soundDistance[cell]; if (distance == DoorHearingRules.Radius) continue; int x = left + cell % 17, y = top + cell / 17; for (int d = 0; d < 8; d += 2) { int nx = x + dx[d], ny = y + dy[d], lx = nx - left, ly = ny - top; if (lx < 0 || ly < 0 || lx >= 17 || ly >= 17 || !map.Contains(nx, ny) || map.Get(nx, ny) == Terrain.Wall) continue; int next = ly * 17 + lx; if (soundDistance[next] != -1) continue; soundDistance[next] = distance + 1; soundQueue[tail++] = next; } // Walls block paths; doors, windows, fences and open terrain transmit this prototype's local sound.
            } // Finish bounded propagation.
        } // Finish shared acoustic propagation.
        private void EmitDoorImpact(int sourceX, int sourceY, int emitterId) // Apply a real impact using the same bounded acoustic query as a human warning.
        { // Begin one impact delivery.
            PrepareSoundReach(sourceX, sourceY); int left = sourceX - 8, top = sourceY - 8; // Keep previous impact geometry and listener ordering unchanged.
            for (int i = 0; i < count; i++) // Apply the event immediately in stable identity order, without retaining an event backlog.
            { // Begin one listener check.
                ActorState a = actors[i]; if (a.health==0 || a.id == emitterId || (a.species == Species.Zombie && !ZombieSoundInvestigation)) continue; int lx = a.x - left, ly = a.y - top; if (lx < 0 || ly < 0 || lx >= 17 || ly >= 17 || soundDistance[ly * 17 + lx] < 0) continue; if (a.bracedDoor != 0 && a.bracedDoor != Key(sourceX, sourceY) + 1) ClearDoorBrace(ref a); a.heardDoorX = sourceX; a.heardDoorY = sourceY; a.heardDoorUntilTick = Tick + DoorHearingRules.MemoryTicks; if (a.species == Species.Zombie) { actors[i] = a; continue; } a.stress = Math.Min(100, a.stress + DoorHearingRules.StressGain); ClearShelterDeparture(ref a); if (a.reaction == HumanReaction.Calm || a.reaction == HumanReaction.Aware) a.reaction = HumanReaction.Aware; actors[i] = a; // Do not replace flight, fighting, freezing, infection or visual threat memory with an unseen attacker.
            } // Finish listener reactions.
        } // Finish one impact event.
    } // Finish hearing implementation.
} // Finish bounded local impact hearing.
