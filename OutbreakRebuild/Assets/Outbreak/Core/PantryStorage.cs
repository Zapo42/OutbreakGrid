using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool TryStoreFood(int index, ref ActorState actor, int threatIndex)
        {
            if (actor.turnAtTick != 0 || actor.hunger >= HungerRules.SeekAt || actor.carriedFood < 2 || actor.shelterState != ShelterState.Inside || threatIndex >= 0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || actor.freezeUntilTick > Tick || (actor.reaction != HumanReaction.Calm && actor.reaction != HumanReaction.Aware)) return false;
            Room room = FindRoom(actor.shelterId); if (room == null || !room.Contains(Key(actor.x, actor.y))) return false;
            ShelfDocument chosen = null;
            foreach (ShelfDocument shelf in map.ShelfRecords)
                if (shelf.food < 1000 && room.Contains(Key(shelf.x,shelf.y)) && Math.Abs(shelf.x-actor.x)+Math.Abs(shelf.y-actor.y)==1 && (chosen==null || Key(shelf.x,shelf.y)<Key(chosen.x,chosen.y))) chosen=shelf;
            if (chosen == null) return false;
            int amount = Math.Min(actor.carriedFood-1,1000-chosen.food); int stock = chosen.food+amount;
            map.SetShelf(chosen.x,chosen.y,stock,true); actor.carriedFood-=amount;
            RememberShelf(ref actor,new ShelfMemoryRecord { x=chosen.x,y=chosen.y,food=stock,observedTick=Tick });
            ClearShelterDeparture(ref actor); actors[index]=actor; return true; // Transfer finite meals, keep one reserve and record first-hand stock knowledge.
        }
    }
}
