using System; // Reject malformed physical defense before publishing a saved world.
using Outbreak.Contracts; // Keep a brace on the human actor rather than healing terrain.
namespace Outbreak.Core // Resolve temporary door defense through normal human opportunities.
{
    public static class DoorBraceRules
    {
        public const int HoldTicks = 5; // Renew once per human decision; defense never becomes a permanent barricade.
    }
    public sealed partial class World
    {
        private static void ClearDoorBrace(ref ActorState actor) { actor.bracedDoor = 0; actor.braceUntilTick = 0; } // Release both pieces of saved progress together.
        private bool BraceGeometry(ActorState actor, int door) // Require the human to stand on the room side of its own adjacent closed door.
        {
            if (door <= 0 || door > Width * Height || actor.species != Species.Human || actor.turnAtTick != 0 || actor.shelterState != ShelterState.Inside) return false;
            Room room = FindRoom(actor.shelterId); int key = door - 1, x = key % Width, y = key / Width;
            return room != null && room.Contains(Key(actor.x, actor.y)) && Array.BinarySearch(room.doors, key) >= 0 && Math.Abs(actor.x - x) + Math.Abs(actor.y - y) == 1 && map.Get(x, y) == Terrain.DoorClosed && map.DoorHealthAt(x, y) > 0;
        }
        private bool TryBraceDoor(int index, ref ActorState actor, Room room, int threatIndex) // React only to a recently heard strike on this specific door.
        {
            ClearDoorBrace(ref actor); if (threatIndex >= 0 || !HasRecentDoorNoise(actor)) return false;
            int door = Key(actor.heardDoorX, actor.heardDoorY) + 1; if (!BraceGeometry(actor, door)) return false;
            actor.bracedDoor = door; actor.braceUntilTick = Math.Min(Tick + DoorBraceRules.HoldTicks, actor.heardDoorUntilTick); ClearShelterDeparture(ref actor); actors[index] = actor; return true; // Holding consumes the opportunity rather than moving, repairing or leaving.
        }
        public bool DoorIsBraced(int x, int y) // Report the same physical defense used by incoming attacks.
        {
            if (!DoorBracing || !map.Contains(x, y)) return false; int door = Key(x, y) + 1;
            for (int i = 0; i < count; i++) { ActorState a = actors[i]; if (a.bracedDoor == door && a.braceUntilTick > Tick && a.heardDoorUntilTick > Tick && BraceGeometry(a, door)) return true; } return false; // Additional occupants cannot stack the reduction.
        }
        private void ReleaseDoorBraces(int door) // Opening or breaking a door immediately removes its physical defense.
        {
            for (int i = 0; i < count; i++) if (actors[i].bracedDoor == door) { ActorState a = actors[i]; ClearDoorBrace(ref a); actors[i] = a; }
        }
        private void ValidateDoorBracing() // Keep the saved action tied to a real listener, doorway and bounded clock.
        {
            for (int i = 0; i < count; i++) { ActorState a = actors[i]; if (a.bracedDoor == 0) { if (a.braceUntilTick != 0) throw new ArgumentException("Inactive door brace has a deadline."); continue; } if (!DoorBracing || a.braceUntilTick <= Tick || a.braceUntilTick - Tick > DoorBraceRules.HoldTicks || a.braceUntilTick > a.heardDoorUntilTick || !HasRecentDoorNoise(a) || a.bracedDoor != Key(a.heardDoorX, a.heardDoorY) + 1 || !BraceGeometry(a, a.bracedDoor) || a.shelterExitPhase != ShelterExitPhase.None || a.shelterCalmSinceTick != 0) throw new ArgumentException("Invalid physical door brace."); }
        }
    }
}
