using System; // Clamp physical repair to the retained health limit.
using Outbreak.Contracts; // Repair terrain from an actual sheltered human action.
namespace Outbreak.Core // Advance repairs only through normal simulation opportunities.
{
    public static class DoorRepairRules
    {
        public const int HealthPerAction = 5; // Restore five HP per ordinary one-second human opportunity.
    }
    public sealed partial class World
    {
        private bool TryRepairDoor(int index, ref ActorState actor, Room room, int threatIndex) // Quiet, uninfected occupants can maintain a reachable door from inside.
        {
            if (room == null || actor.species != Species.Human || actor.turnAtTick != 0 || actor.stress != 0 || threatIndex >= 0 || actor.shelterState != ShelterState.Inside || !room.Contains(Key(actor.x, actor.y)) || HasRecentDoorNoise(actor) || HasRecentWarning(actor)) return false;
            foreach (int key in room.doors) // Canonical room order selects at most one repair per opportunity.
            {
                int x = key % Width, y = key / Width, health = map.DoorHealthAt(x, y); if (map.Get(x, y) != Terrain.DoorClosed || health <= 0 || health >= DoorRules.MaxHealth || Math.Abs(actor.x - x) + Math.Abs(actor.y - y) != 1) continue;
                map.SetDoorHealth(x, y, Math.Min(DoorRules.MaxHealth, health + DoorRepairRules.HealthPerAction)); DoorConditionRevision++; ClearShelterDeparture(ref actor); actors[index] = actor; return true; // Work consumes movement and delays departure; it never changes collision, resurrects a broken door or emits an attack impact.
            }
            return false;
        }
    }
}
