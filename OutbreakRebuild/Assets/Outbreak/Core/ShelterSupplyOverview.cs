using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public struct ShelterSupplyOverview
    {
        public int RoomId,FocusX,FocusY,Occupants,Hungry,VeryHungry,CarriedMeals,BoxMeals,ShelfMeals,Boxes,BoxesBelowTarget,ReserveDeficit,RestockingTrips,IncomingDeliveries,OutgoingDeliveries;
        public int Meals { get { return CarriedMeals+BoxMeals+ShelfMeals; } }
        public bool ShortOnMeals { get { return Occupants>0 && Meals<Occupants; } }
        public bool NeedsAttention { get { return ShortOnMeals || ((Occupants>0 || RestockingTrips>0) && ReserveDeficit>0); } }
        public string Status
        {
            get
            {
                if(Occupants==0 && RestockingTrips==0)return "UNOCCUPIED";
                if(ShortOnMeals)return Meals==0?"NO FOOD":"LOW FOOD";
                return ReserveDeficit>0?"BELOW RESERVE":"SUPPLIED";
            }
        }
    }
    public sealed partial class World
    {
        public ShelterSupplyOverview[] ShelterSupplies()
        {
            if(!HungerEnabled)return new ShelterSupplyOverview[0];ShelterSupplyOverview[] result=new ShelterSupplyOverview[rooms.Length];
            for(int r=0;r<rooms.Length;r++)
            {
                Room room=rooms[r];ShelterSupplyOverview supply=new ShelterSupplyOverview{RoomId=room.id};
                int minX=Width,minY=Height,maxX=0,maxY=0;
                foreach(int key in room.cells){int x=key%Width,y=key/Width;minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                supply.FocusX=(minX+maxX)/2;supply.FocusY=(minY+maxY)/2;
                for(int i=0;i<count;i++)
                {
                    ActorState actor=actors[i];if(actor.species!=Species.Human || actor.health<=0)continue;
                    if(actor.restockHomeId==room.id && actor.restockPhase!=RestockPhase.None){supply.RestockingTrips++;if(actor.restockSourceBox!=0)supply.IncomingDeliveries++;}
                    if(actor.restockSourceBox>0 && actor.restockPhase!=RestockPhase.None && room.Contains(actor.restockSourceBox-1))supply.OutgoingDeliveries++;
                    if(!room.Contains(Key(actor.x,actor.y)))continue;
                    supply.Occupants++;supply.CarriedMeals+=actor.carriedFood;
                    if(actor.hunger>=HungerRules.SeekAt)supply.Hungry++;
                    if(actor.hunger>=FoodOverview.VeryHungryAt)supply.VeryHungry++;
                }
                foreach(StorageBoxDocument box in map.StorageRecords)
                {
                    if(!room.Contains(Key(box.x,box.y)))continue;
                    int food=StorageRules.Count(box,InventoryItem.Food);supply.Boxes++;supply.BoxMeals+=food;
                    int deficit=RestockingEnabled?Math.Max(0,StorageRules.FoodReserve(box)-food):0;
                    supply.ReserveDeficit+=deficit;if(deficit>0)supply.BoxesBelowTarget++;
                }
                foreach(ShelfDocument shelf in map.ShelfRecords)if(room.Contains(Key(shelf.x,shelf.y)))supply.ShelfMeals+=shelf.food;
                result[r]=supply;
            }
            Array.Sort(result,(a,b)=>a.RoomId.CompareTo(b.RoomId));return result; // Player census only: no personal knowledge, actions, assignments or random state change.
        }
    }
}
