using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool TryStoreFood(int index, ref ActorState actor, int threatIndex, out int direction)
        {
            direction = 8;
            if (actor.turnAtTick != 0 || actor.hunger >= HungerRules.SeekAt || actor.carriedFood <= FoodReserve(actor) || actor.shelterState != ShelterState.Inside || threatIndex >= 0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || actor.freezeUntilTick > Tick || (actor.reaction != HumanReaction.Calm && actor.reaction != HumanReaction.Aware)) return false;
            Room room = FindRoom(actor.shelterId); if (room == null || !room.Contains(Key(actor.x, actor.y))) return false;
            ShelfDocument chosen = null; int bestSteps=int.MaxValue, chosenDirection=8;
            foreach (ShelfDocument shelf in map.ShelfRecords)
            {
                if (!room.Contains(Key(shelf.x,shelf.y))) continue;
                bool adjacent = Math.Abs(shelf.x-actor.x)+Math.Abs(shelf.y-actor.y)==1;
                if (adjacent)
                {
                    if(shelf.food>=1000) continue;
                    if(bestSteps>0 || (bestSteps==0 && Key(shelf.x,shelf.y)<Key(chosen.x,chosen.y))) {chosen=shelf;bestSteps=0;chosenDirection=8;}
                    continue;
                }
                if(!PantryTrips)continue;
                bool known=ShelfVisible(actor,shelf); int knownFood=known && shelf.searched?shelf.food:-1;
                if(!known && actor.shelfMemory!=null)foreach(ShelfMemoryRecord note in actor.shelfMemory)if(note.x==shelf.x && note.y==shelf.y){known=true;knownFood=note.food;break;}
                if(!known || knownFood>=1000)continue;
                for(int d=0;d<8;d+=2)
                {
                    int x=shelf.x+dx[d],y=shelf.y+dy[d],length;
                    if(!room.Contains(Key(x,y)) || !map.CanWalk(x,y))continue;
                    int route=ShelterRoute(actor,x,y,0,out length,room);
                    if(route<0 || length==0)continue;
                    if(length<bestSteps || (length==bestSteps && chosen!=null && Key(shelf.x,shelf.y)<Key(chosen.x,chosen.y))) {chosen=shelf;bestSteps=length;chosenDirection=route;}
                }
            }
            if (chosen == null) return false;
            if(bestSteps>0) { direction=chosenDirection;ClearShelterDeparture(ref actor);actors[index]=actor;return true; } // Replan each opportunity; threats or hunger interrupt without stale task state.
            int amount = Math.Min(actor.carriedFood-FoodReserve(actor),1000-chosen.food); int stock = chosen.food+amount;
            map.SetShelf(chosen.x,chosen.y,stock,true); actor.carriedFood-=amount;
            RememberShelf(ref actor,new ShelfMemoryRecord { x=chosen.x,y=chosen.y,food=stock,observedTick=Tick });
            ClearShelterDeparture(ref actor); actors[index]=actor; return true; // Transfer finite meals, keep the personal reserve and record first-hand stock knowledge.
        }
    }
}
