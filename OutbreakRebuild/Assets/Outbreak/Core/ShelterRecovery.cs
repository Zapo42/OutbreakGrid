using System; // Reject malformed saved exit progress before replacing a running world.
using Outbreak.Contracts; // Keep exit phases portable and independent of Unity.
namespace Outbreak.Core // Advance recovery and inspection only through authoritative actor opportunities.
{ // Begin shelter departure behavior.
    public static class ShelterRecoveryRules // Publish provisional timing in logical ticks.
    { // Begin versioned prototype tuning.
        public const int HealPerRest=2, HungerPerRest=1, ExtraReserveBelowHealth=50, RecoveryReserveMeals=2;
        public const int CalmTicks = 25; // Wait five simulation seconds at zero stress before checking an exit.
    } // Finish prototype tuning.
    public sealed partial class World // Reuse the existing room catalog, collision checks and perception.
    { // Begin recovery implementation.
        private static void ClearShelterDeparture(ref ActorState actor) // Cancel every part of a previous inspection together.
        { // Begin canonical exit cleanup.
            actor.shelterCalmSinceTick = 0; actor.shelterExitPhase = ShelterExitPhase.None; actor.shelterExitDoor = 0; actor.shelterExitX = 0; actor.shelterExitY = 0; // Release only departure progress; retain the shelter unless its caller clears it.
        } // Finish exit cleanup.
        private bool OutsideRooms(int x, int y) // Avoid treating another room's interior as a return to outdoor routine.
        { // Begin catalog membership check.
            foreach (Room room in rooms) if (room.Contains(Key(x, y))) return false; return true; // Work remains bounded by the room catalog.
        } // Finish outdoor classification.
        private bool ExitGeometry(Room room, int door, int x, int y) // Validate the same cardinal exit geometry during selection and reload.
        { // Begin geometric validation.
            if (room == null || door <= 0 || !map.Contains(x, y) || !map.CanWalk(x, y) || IsDoor(x, y) || !OutsideRooms(x, y)) return false; int key = door - 1; if (Array.BinarySearch(room.doors, key) < 0) return false; int dxDoor = key % Width, dyDoor = key / Width; return Math.Abs(dxDoor - x) + Math.Abs(dyDoor - y) == 1 && map.Contains(2 * dxDoor - x, 2 * dyDoor - y) && room.Contains(Key(2 * dxDoor - x, 2 * dyDoor - y)); // Require a real door between this room's floor and an outdoor cell.
        } // Finish exit validation.
        private void ValidateShelterRecovery() // Protect exact saved phase and timing without introducing hidden target knowledge.
        { // Begin persisted recovery validation.
            for (int i = 0; i < count; i++) // Inspect detached restored actors in canonical identity order.
            { // Begin one actor record.
                ActorState a = actors[i]; bool active = a.shelterCalmSinceTick != 0 || a.shelterExitPhase != ShelterExitPhase.None || a.shelterExitDoor != 0 || a.shelterExitX != 0 || a.shelterExitY != 0; if (!active) continue; Room room = FindRoom(a.shelterId); if (!ShelterRecovery || a.species != Species.Human || room == null || a.shelterCalmSinceTick < 1 || a.shelterCalmSinceTick > Tick || a.shelterExitPhase < ShelterExitPhase.None || a.shelterExitPhase > ShelterExitPhase.Leaving) throw new ArgumentException("Invalid shelter recovery state."); // A bite may await its next decision; turning still clears the entire state immediately.
                if (a.shelterExitPhase == ShelterExitPhase.None) { if (a.shelterExitDoor != 0 || a.shelterExitX != 0 || a.shelterExitY != 0 || !room.Contains(Key(a.x, a.y))) throw new ArgumentException("Inactive shelter exit contains stale geometry."); } else if (Tick - a.shelterCalmSinceTick < ShelterRecoveryRules.CalmTicks || !ExitGeometry(room, a.shelterExitDoor, a.shelterExitX, a.shelterExitY) || (!room.Contains(Key(a.x, a.y)) && Key(a.x, a.y) != a.shelterExitDoor - 1)) throw new ArgumentException("Invalid shelter exit progress."); // A departure may be saved inside its actual doorway, but not at an unrelated outdoor cell.
                if (a.shelterExitPhase == ShelterExitPhase.Approaching && !room.Contains(Key(a.x, a.y))) throw new ArgumentException("Exit approach is outside its room."); if (a.shelterExitPhase == ShelterExitPhase.Checking && (a.x != 2 * ((a.shelterExitDoor - 1) % Width) - a.shelterExitX || a.y != 2 * ((a.shelterExitDoor - 1) / Width) - a.shelterExitY)) throw new ArgumentException("Exit inspection is not at its indoor observation cell."); if (a.shelterExitPhase == ShelterExitPhase.Leaving && Key(a.x, a.y) != a.shelterExitDoor - 1) throw new ArgumentException("Exit crossing is not in its doorway."); // Every persisted phase must match its physically reachable position.
            } // Finish one record.
        } // Finish recovery validation.
        private bool TryRestInShelter(int index,ref ActorState actor,Room room,int threatIndex)
        {
            if(actor.health<=0 || actor.health>=100 || actor.turnAtTick!=0 || actor.stress!=0 || actor.hunger>=HungerRules.SeekAt || threatIndex>=0 || HasRecentDoorNoise(actor) || HasRecentWarning(actor) || actor.shelterState!=ShelterState.Inside || actor.x!=actor.shelterX || actor.y!=actor.shelterY || !room.Contains(Key(actor.x,actor.y)))return false;
            CloseNearbyDoors(actor,room);ClearShelterDeparture(ref actor);actor.health=Math.Min(100,actor.health+ShelterRecoveryRules.HealPerRest);actor.hunger=Math.Min(100,actor.hunger+ShelterRecoveryRules.HungerPerRest);actors[index]=actor;return true;
        }
        private bool TryShelterDeparture(int index, ref ActorState actor, Room room, int threatIndex, out int direction) // Replace permanent hiding with a cautious, interruptible inspection.
        { // Begin one departure decision.
            direction = 8; bool inside = room.Contains(Key(actor.x, actor.y)); if ((HungerEnabled ? !HungryDepartureAllowed(actor, room, threatIndex) : actor.turnAtTick != 0 || actor.stress != 0 || HasRecentDoorNoise(actor) || HasRecentWarning(actor) || threatIndex >= 0) || (!inside && actor.shelterExitPhase == ShelterExitPhase.None)) { ClearShelterDeparture(ref actor); return false; } // Infection or a real sighting cancels inspection and restores the existing shelter retreat.
            if (HungerEnabled && inside) actor.homeShelterId = room.id; if (actor.shelterCalmSinceTick == 0) actor.shelterCalmSinceTick = Tick; if (Tick - actor.shelterCalmSinceTick < ShelterRecoveryRules.CalmTicks) return false; // Continue normal interior recovery during the calm interval.
            if (actor.shelterExitPhase == ShelterExitPhase.None) // Choose a reachable indoor observation position, not an unseen zombie's location.
            { // Begin bounded door selection.
                int best = int.MaxValue; foreach (int key in room.doors) for (int d = 0; d < 8; d += 2) { int x = key % Width + dx[d], y = key / Width + dy[d]; if (!ExitGeometry(room, key + 1, x, y)) continue; int distance; if (ShelterRoute(actor, 2 * (key % Width) - x, 2 * (key / Width) - y, 0, out distance) < 0 || distance >= best) continue; best = distance; actor.shelterExitDoor = key + 1; actor.shelterExitX = x; actor.shelterExitY = y; actor.shelterExitPhase = ShelterExitPhase.Approaching; } if (best == int.MaxValue) return false; // Remain sheltered if no usable outdoor exit exists.
            } // Finish selection.
            int doorX = (actor.shelterExitDoor - 1) % Width, doorY = (actor.shelterExitDoor - 1) / Width, stageX = 2 * doorX - actor.shelterExitX, stageY = 2 * doorY - actor.shelterExitY; // Keep observation on the room's side of the selected door.
            if (actor.shelterExitPhase == ShelterExitPhase.Approaching && (actor.x != stageX || actor.y != stageY)) { int length; direction = ShelterRoute(actor, stageX, stageY, 0, out length); if (direction < 0) { ClearShelterDeparture(ref actor); return false; } actors[index] = actor; return true; } // Approach using ordinary solid-corner and occupancy rules.
            if (map.Get(doorX, doorY) == Terrain.DoorClosed || actor.shelterExitPhase == ShelterExitPhase.Approaching) { SetDoor(doorX, doorY, true); actor.shelterExitPhase = ShelterExitPhase.Checking; int observed = NearestVisible(actor, Species.Zombie); if (observed >= 0 && (!HungerEnabled || !CanRiskFoodTrip(actor, observed))) { actor.stress = Math.Min(100, actor.stress + HumanRules.CloseStressGain); actor.threatPressure = ObservePressure(actor, actors[observed]); actor.reaction = HumanReaction.Aware; ClearShelterDeparture(ref actor); if (!DoorOccupied(doorX, doorY)) SetDoor(doorX, doorY, false); } actors[index] = actor; return true; } // Opening costs an opportunity; observe again before entering the doorway.
            ActorState lookout = new ActorState { x = actor.shelterExitX, y = actor.shelterExitY }; if (!Visible(actor, lookout) || !occupancy.Allows(Key(lookout.x, lookout.y), Species.Human)) { ClearShelterDeparture(ref actor); return false; } // A blocked sightline or crowded exit does not establish clearance.
            int nextX = inside ? doorX : lookout.x, nextY = inside ? doorY : lookout.y; for (int d = 0; d < 8; d += 2) if (actor.x + dx[d] == nextX && actor.y + dy[d] == nextY) { direction = d; break; } if (direction == 8 || !MayMove(actor, direction)) { ClearShelterDeparture(ref actor); return false; } if (HungerEnabled) { ActorState step = actor; step.x += dx[direction]; step.y += dy[direction]; int threats = KnownShelterThreats(actor, threatIndex); for (int j = 0; j < threats; j++) if (DistanceSquared(step, escapeThreats[j]) < 4) { direction = 8; ClearShelterDeparture(ref actor); return false; } } actor.shelterExitPhase = ShelterExitPhase.Leaving; actors[index] = actor; return true; // Only current sight and ordinary physical movement authorize the two exit steps.
        } // Finish departure decision.
        private bool FinishShelterDeparture(int index, ref ActorState actor, Room room) // Preserve inspection doors and release admission after physically reaching outdoors.
        { // Begin post-movement exit commitment.
            if (room == null || (actor.shelterExitPhase != ShelterExitPhase.Checking && actor.shelterExitPhase != ShelterExitPhase.Leaving)) return false; if (actor.x == actor.shelterExitX && actor.y == actor.shelterExitY && actor.shelterExitPhase == ShelterExitPhase.Leaving) { int x = (actor.shelterExitDoor - 1) % Width, y = (actor.shelterExitDoor - 1) / Width; if (!DoorOccupied(x, y)) SetDoor(x, y, false); ClearShelter(ref actor); actor.reaction = HumanReaction.Calm; } else actor.shelterState = room.Contains(Key(actor.x, actor.y)) ? ShelterState.Inside : ShelterState.Seeking; actors[index] = actor; return true; // Other bodies keep an occupied doorway open; no distant door closes magically.
        } // Finish departure commitment.
    } // Finish recovery implementation.
} // Finish engine-independent recovery.
