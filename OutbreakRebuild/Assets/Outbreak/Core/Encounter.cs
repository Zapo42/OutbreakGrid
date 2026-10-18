using System; // Keep encounter behavior independent of Unity.
using Outbreak.Contracts; // Preserve actor identities and portable infection state.
namespace Outbreak.Core // Execute all encounter work through the authoritative world clock.
{ // Begin the explicitly versioned prototype.
    public static class EncounterRules // Expose provisional tuning rather than claiming final combat rules.
    { // Begin prototype constants measured in logical cells and ticks.
        public const string Version = "encounter-prototype-10", WanderVersion = "encounter-prototype-9", RecoveryVersion = "encounter-prototype-8", DoorVersion = "encounter-prototype-7", ShelterVersion = "encounter-prototype-6", EscapeVersion = "encounter-prototype-5", CautiousVersion = "encounter-prototype-4", ReactionVersion = "encounter-prototype-3", MemoryVersion = "encounter-prototype-2", LegacyVersion = "encounter-prototype-1"; // Separate changed behavior from foundation saves.
        public const int SightRadius = 12, MoveInterval = 5, BiteDamage = 10, BaseTurnDelay = 50, EnduranceDelay = 5; // Prototype: one move/second, bite on contact, ten seconds plus END delay.
        public const int SearchTicks = 75; // Prototype memory lasts fifteen simulation seconds after the latest observation.
        public const int MaxTurnDelay = BaseTurnDelay + 10 * EnduranceDelay; // Bound persisted infection deadlines using the supported stat range.
    } // Finish prototype tuning.
    public static class ZombieWanderRules // Publish provisional idle movement tuning in logical actor opportunities.
    { // Begin slow wandering tuning.
        public const int ChanceDenominator = 4; // One-in-four chance to step on each one-second movement opportunity.
    } // Finish idle tuning.
    public sealed partial class World // Extend the same authoritative owner without adding engine dependencies.
    { // Begin encounter behavior.
        public bool Encounter { get; private set; } // Keep scenario mode immutable after construction.
        public bool PursuitMemory { get; private set; } // Retain the old encounter behavior when continuing a prototype-one save.
        private readonly int[] routeQueue = new int[625], routeFirst = new int[625], routeSeen = new int[625]; // Bound reusable navigation storage to the local 25 by 25 sight region.
        private readonly int[] routeOrder = new int[8]; // Reuse the latest rules' direct-neighbor preference without per-query allocations.
        private readonly int[] wanderDirections = new int[8]; // Reuse the legal local steps for an idle zombie without per-tick allocations.
        private int routeStamp; // Reuse visited storage without retaining a history of queries.
        private static void ClearPursuit(ref ActorState actor) // Remove stale observations without retaining diagnostic history.
        { // Begin canonical memory cleanup.
            actor.targetId = 0; actor.lastSeenX = 0; actor.lastSeenY = 0; actor.searchUntilTick = 0; // Keep inactive memory identical across capture and restore.
        } // Finish memory cleanup.
        private void ValidatePursuit() // Validate all persisted observations after every actor identity is available.
        { // Begin transactional pursuit validation.
            for (int i = 0; i < count; i++) // Inspect each actor before publishing a restored world.
            { // Begin observation validation.
                ActorState actor = actors[i]; if (actor.targetId == 0) { if (actor.lastSeenX != 0 || actor.lastSeenY != 0 || actor.searchUntilTick != 0) throw new ArgumentException("Inactive pursuit contains stale data."); continue; } // Refuse hidden state attached to an inactive observation.
                if (!PursuitMemory || actor.species != Species.Zombie || actor.targetId == actor.id || !ValidObservedCell(actor.lastSeenX, actor.lastSeenY) || actor.searchUntilTick <= Tick || actor.searchUntilTick - Tick > EncounterRules.SearchTicks) throw new ArgumentException("Invalid pursuit observation."); // Reject wrong modes, geometry, species and expired or excessive deadlines.
                bool found = false; for (int j = 0; j < count; j++) if (actors[j].id == actor.targetId) { found = true; break; } if (!found) throw new ArgumentException("Pursuit target identity is absent."); // Permit a remembered human to have turned while unseen, without silently losing its stable identity.
            } // Finish actor validation.
        } // Finish pursuit validation.
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
            for (int i = 0; i < count; i++) if (actors[i].species == Species.Zombie && (!HumanReactions || actors[i].staggerUntilTick <= Tick) && actors[i].x == x && actors[i].y == y) return true; return false; // New runs permit passing staggered zombies; legacy rules retain their original blocking.
        } // Finish blocking query.
        private bool MayMove(ActorState actor, int direction) // Combine terrain and live occupancy without mutating state.
        { // Begin candidate validation.
            int nx = actor.x + dx[direction], ny = actor.y + dy[direction]; // Translate one neighboring step.
            return ClearStep(actor.x, actor.y, nx, ny) && occupancy.Allows(Key(nx, ny), actor.species) && (actor.species != Species.Human || !ZombieAt(nx, ny)); // Humans cannot walk through zombies.
        } // Finish candidate validation.
        private static int RoutePreference(ActorState actor, ActorState target, int direction) // Rank equivalent route openings by directness without changing collision geometry.
        { // Begin deterministic neighbor preference.
            int x = actor.x + dx[direction] - target.x, y = actor.y + dy[direction] - target.y; return x * x + y * y; // Stable insertion order resolves equal scores without consuming randomness.
        } // Finish geometric ranking.
        private int Route(ActorState actor, ActorState target, bool rememberDoors = false) // Find a shortest eight-neighbor route within a bounded local query.
        { // Begin reusable breadth-first search.
            if (routeStamp == int.MaxValue) { Array.Clear(routeSeen, 0, routeSeen.Length); routeStamp = 0; } routeStamp++; // Safely recycle the finite generation counter.
            for (int d = 0; d < 8; d++) routeOrder[d] = d; // Legacy rules retain the exact original clockwise tie order.
            if (CautiousMovement) for (int i = 1; i < 8; i++) { int value = routeOrder[i], j = i - 1; while (j >= 0 && RoutePreference(actor, target, value) < RoutePreference(actor, target, routeOrder[j])) { routeOrder[j + 1] = routeOrder[j]; j--; } routeOrder[j + 1] = value; } // Prefer steps aligned with the target instead of equivalent zigzag routes.
            int left = actor.x - 12, top = actor.y - 12, start = 12 * 25 + 12, head = 0, tail = 1; routeQueue[0] = start; routeSeen[start] = routeStamp; routeFirst[start] = 8; // Anchor the search window to the moving actor.
            while (head < tail) // Never visit more than 625 cells in one query.
            { // Begin one expanded navigation cell.
                int cell = routeQueue[head++], x = left + cell % 25, y = top + cell / 25; if (x == target.x && y == target.y) return routeFirst[cell]; // Return the first step of the shortest discovered route.
                for (int order = 0; order < 8; order++) // Preserve stable ties while favoring the more direct route in new runs.
                { // Begin candidate expansion.
                    int d = routeOrder[order], nx = x + dx[d], ny = y + dy[d], lx = nx - left, ly = ny - top; if (lx < 0 || ly < 0 || lx >= 25 || ly >= 25 || !(rememberDoors ? RememberedDoorStep(x, y, nx, ny) : ClearStep(x, y, nx, ny))) continue; // Stay inside the bounded terrain query.
                    int next = ly * 25 + lx; if (routeSeen[next] == routeStamp || !occupancy.Allows(Key(nx, ny), actor.species)) continue; // Respect current crowd capacity on each path cell.
                    routeSeen[next] = routeStamp; routeFirst[next] = cell == start ? d : routeFirst[cell]; routeQueue[tail++] = next; // Retain only the first direction and finite frontier.
                } // Finish candidate expansion.
            } // Finish bounded search.
            return 8; // Wait when no route is available instead of walking through a barrier.
        } // Finish navigation.
        private bool RememberedDoorStep(int x, int y, int nx, int ny) // Plan toward remembered terrain through closed doors without granting physical passage.
        { // Keep door planning cardinal and preserve solid diagonal corners.
            if (!map.Contains(nx, ny) || (!map.CanWalk(nx, ny) && map.Get(nx, ny) != Terrain.DoorClosed)) return false; // Walls remain impassable even during investigation.
            if (x != nx && y != ny) return map.Get(x, y) != Terrain.DoorClosed && map.Get(nx, ny) != Terrain.DoorClosed && map.CanWalk(x, ny) && map.CanWalk(nx, y); // Never cut a corner through a door or wall.
            return true; // MoveActor still checks actual solidity and stops the zombie outside the first closed door.
        } // Finish remembered-route planning.
        private int WanderZombie(ActorState actor) // Idle zombies occasionally shuffle without consulting hidden humans or expired sightings.
        { // Begin one slow wandering opportunity.
            if (random.Below(ZombieWanderRules.ChanceDenominator) != 0) return 8; int available = 0; for (int d = 0; d < 8; d++) if (MayMove(actor, d)) wanderDirections[available++] = d; return available == 0 ? 8 : wanderDirections[random.Below(available)]; // Use ordinary collision and crowd rules, choosing uniformly among legal adjacent cells.
        } // Finish idle direction selection.
        private void MoveActor(int index, ActorState actor, int direction) // Commit the same geometry and crowd checks for every rules version.
        { // Begin a complete neighboring move.
            if (direction == 8 || !MayMove(actor, direction)) return; occupancy.Change(Key(actor.x, actor.y), actor.species, -1); actor.x += dx[direction]; actor.y += dy[direction]; if (PursuitMemory && actor.targetId != 0 && actor.x == actor.lastSeenX && actor.y == actor.lastSeenY) ClearPursuit(ref actor); actors[index] = actor; occupancy.Change(Key(actor.x, actor.y), actor.species, 1); // Keep stable identity, observation cleanup and occupancy atomic.
        } // Finish movement commitment.
        private void StepEncounter() // Keep infection, conversion, bites and movement on the same logical clock.
        { // Begin a complete encounter tick.
            for (int i = 0; i < count; i++) // Convert every due human before movement or target selection.
            { // Begin deadline processing.
                ActorState actor = actors[i]; if (PursuitMemory && actor.targetId != 0 && actor.searchUntilTick <= Tick) { ClearPursuit(ref actor); actors[i] = actor; } // Expire memory even between movement opportunities.
                if (HumanReactions) { if (SheltersEnabled && actor.rejectedUntilTick != 0 && actor.rejectedUntilTick <= Tick) { actor.rejectedShelterId = 0; actor.rejectedUntilTick = 0; } if (InfectedEscape && actor.avoidUntilTick != 0 && actor.avoidUntilTick <= Tick) ClearAvoidance(ref actor); AdvanceReactionTimers(ref actor); actors[i] = actor; } // Complete every reaction deadline even between movement opportunities.
                if (actor.turnAtTick == 0 || actor.turnAtTick > Tick) continue; // Ignore healthy humans and future deadlines.
                occupancy.Change(Key(actor.x, actor.y), Species.Human, -1); actor.species = Species.Zombie; actor.turnAtTick = 0; if (HumanReactions) ClearHumanReaction(ref actor); actor.charisma = 0; actor.intelligence = 0; actor.strength = Math.Max(1, actor.strength); actor.perception = Math.Max(1, actor.perception); actors[i] = actor; occupancy.Change(Key(actor.x, actor.y), Species.Zombie, 1); // Preserve identity, position, health and inherited physical stats while updating crowd counts.
            } // Finish turning deadlines.
            for (int i = 0; i < count; i++) // Process stable identity order for reproducible encounters.
            { // Begin one actor decision.
                ActorState actor = actors[i]; if (Tick % EncounterRules.MoveInterval != actor.id % EncounterRules.MoveInterval || (HumanReactions && actor.staggerUntilTick > Tick)) continue; // Stagger movement and contact opportunities over ticks.
                int targetIndex = NearestVisible(actor, actor.species == Species.Zombie ? Species.Human : Species.Zombie), direction = 8; // Reacquire locally rather than using global knowledge.
                if (HumanReactions && actor.species == Species.Human) { direction = ReactHuman(i, targetIndex); MoveActor(i, actors[i], direction); if (SheltersEnabled) FinishShelterMove(i); continue; } // Evaluate stress and bravery before choosing a human action.
                if (targetIndex >= 0) // Chase or retreat only when an opponent is visible.
                { // Begin encounter response.
                    ActorState target = actors[targetIndex]; if (PursuitMemory && actor.species == Species.Zombie) { actor.targetId = target.id; actor.lastSeenX = target.x; actor.lastSeenY = target.y; actor.searchUntilTick = Tick + EncounterRules.SearchTicks; actors[i] = actor; } // Refresh memory only from a real current sight observation.
                    int distance = Math.Max(Math.Abs(target.x - actor.x), Math.Abs(target.y - actor.y)); // Contact includes the same cell and unobstructed adjacent cells.
                    if (actor.species == Species.Zombie && distance <= 1) // Use a deterministic contact bite until grabbing and evasion are implemented.
                    { // Begin one bite opportunity.
                        if (target.turnAtTick == 0) { target.health = Math.Max(1, target.health - EncounterRules.BiteDamage); target.turnAtTick = Tick + EncounterRules.BaseTurnDelay + target.endurance * EncounterRules.EnduranceDelay; if (InfectedEscape) { target.reaction = HumanReaction.Flight; target.freezeUntilTick = 0; target.freezeResolved = true; target.avoidX = actor.x; target.avoidY = actor.y; target.avoidUntilTick = Tick + AvoidMemoryTicks; } actors[targetIndex] = target; } continue; // A later bite cannot reset an existing infection deadline.
                    } // Finish contact handling.
                    if (actor.species == Species.Zombie) direction = Route(actor, target); // Follow a collision-safe shortest local route to the visible human.
                    else // Retreat from the nearest observed threat without granting full planned fear behavior.
                    { // Begin prototype fleeing.
                        int best = (actor.x - target.x) * (actor.x - target.x) + (actor.y - target.y) * (actor.y - target.y); // Prefer staying over moving closer to danger.
                        for (int d = 0; d < 8; d++) if (MayMove(actor, d)) { int x = actor.x + dx[d] - target.x, y = actor.y + dy[d] - target.y, score = x * x + y * y; if (score > best) { best = score; direction = d; } } // Stable ties retain the first equally safe route.
                    } // Finish prototype fleeing.
                } // Finish encounter response.
                else if (actor.species == Species.Human) direction = random.Below(9); // Legacy humans wander without a visible threat; idle zombie behavior is selected below.
                else if (PursuitMemory && actor.targetId != 0) // Search the observed cell without querying the hidden target's current location.
                { // Begin last-known-position search.
                    if (actor.x == actor.lastSeenX && actor.y == actor.lastSeenY) { ClearPursuit(ref actor); actors[i] = actor; } // An empty last-seen cell ends the search.
                    else direction = Route(actor, new ActorState { x = actor.lastSeenX, y = actor.lastSeenY }, DoorPursuit); // Route only to remembered terrain coordinates.
                } // Finish lost-sight search.
                else if (ZombieWandering) direction = WanderZombie(actor); // Try a nearby shuffle only when no visible human or active remembered search takes priority.
                if (TryAttackDoor(actor, direction)) continue; // A remembered blocking door costs an attack opportunity instead of a movement step.
                MoveActor(i, actor, direction); // Commit geometry and population counts together.
            } // Finish actor decisions.
        } // Finish authoritative encounter tick.
    } // Finish the world extension.
} // Finish the engine-independent prototype.
