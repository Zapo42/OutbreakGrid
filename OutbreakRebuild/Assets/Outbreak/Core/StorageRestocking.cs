using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class RestockRules
    {
        public const int ReserveMeals=6,TaskTicks=600,DepartureEnergy=60,ReturnEnergy=40;
    }
    public sealed partial class World
    {
        private static void ClearRestocking(ref ActorState actor)
        {
            actor.restockBox=0;actor.restockHomeId=0;actor.restockPhase=RestockPhase.None;actor.restockUntilTick=0;
        }
        private bool SafeRestock(ActorState actor,int threat)
        {
            return actor.health>=ShelterRecoveryRules.ExtraReserveBelowHealth && actor.turnAtTick==0 && actor.hunger<HungerRules.SeekAt && actor.stress==0 && threat<0 && actor.freezeUntilTick<=Tick && !HasRecentWarning(actor) && !HasRecentDoorNoise(actor);
        }
        private bool RestockDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return RestockingEnabled && actor.restockPhase==RestockPhase.Collecting && actor.restockHomeId==room.id && SafeRestock(actor,threat) && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy;
        }
        private ShelfDocument ChooseRestockShelf(ActorState actor,Room home)
        {
            if(home==null || actor.shelfMemory==null)return null;ShelfMemoryRecord best=new ShelfMemoryRecord();int rank=-1,steps=int.MaxValue;
            foreach(ShelfMemoryRecord note in actor.shelfMemory)
            {
                if(note.food==0 || home.Contains(Key(note.x,note.y)) || AvoidBlockedFood(actor,note.x,note.y))continue;
                int length=FoodShelfRouteLength(actor,note),priority=note.food>0?1:0;
                if(length==int.MaxValue)continue;
                if(priority>rank || (priority==rank && length<steps)){best=note;rank=priority;steps=length;}
            }
            return rank<0?null:new ShelfDocument{x=best.x,y=best.y}; // Read only remembered supplies, never hidden current shelf stock.
        }
        private bool RestockClaimed(int box,int exceptId)
        {
            for(int i=0;i<count;i++)if(actors[i].id!=exceptId && actors[i].health>0 && actors[i].species==Species.Human && actors[i].restockBox==box && actors[i].restockUntilTick>Tick)return true;return false;
        }
        private bool BeginRestockReturn(int index,ref ActorState actor,Room home,int threat)
        {
            actor.restockPhase=RestockPhase.Returning;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);
            int goal,length;if(home==null || !FoodReturnGoal(actor,home,KnownShelterThreats(actor,threat),out goal,out length)){ClearRestocking(ref actor);actor.homeShelterId=0;actors[index]=actor;return false;}
            actor.homeShelterId=home.id;actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=home.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;actors[index]=actor;return false;
        }
        private bool TryRestock(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;
            if(actor.restockPhase==RestockPhase.None)
            {
                Room current=FindRoom(actor.shelterId);
                if(!map.HasStorageBoxes || current==null || actor.shelterState!=ShelterState.Inside || !current.Contains(Key(actor.x,actor.y)) || !SafeRestock(actor,threat) || actor.carriedFood>FoodReserve(actor) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.homeShelterId!=0 || actor.foodExploreRoomId!=0 || ChooseRestockShelf(actor,current)==null)return false;
                StorageBoxDocument chosen=null;
                foreach(StorageBoxDocument box in map.StorageRecords){int food,space;if(current.Contains(Key(box.x,box.y)) && KnownStorageState(actor,box,out food,out space) && food>=0 && food<StorageRules.FoodReserve(box) && !RestockClaimed(Key(box.x,box.y)+1,actor.id)){chosen=box;break;}}
                if(chosen==null)return false;
                actor.restockBox=Key(chosen.x,chosen.y)+1;actor.restockHomeId=current.id;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+RestockRules.TaskTicks;actors[index]=actor;
            }
            Room home=FindRoom(actor.restockHomeId);StorageBoxDocument target=map.StorageBoxAt((actor.restockBox-1)%Width,(actor.restockBox-1)/Width);
            if(home==null || target==null){ClearRestocking(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
            if(actor.turnAtTick!=0){ClearRestocking(ref actor);actors[index]=actor;return false;}
            if(ReserveTargetsEnabled && actor.restockPhase==RestockPhase.Collecting && (StorageRules.FoodReserve(target)==0 || (StorageVisible(actor,target) && StorageRules.Count(target,InventoryItem.Food)>=StorageRules.FoodReserve(target))))
            {
                if(home.Contains(Key(actor.x,actor.y))){ClearRestocking(ref actor);ClearShelterDeparture(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
                return BeginRestockReturn(index,ref actor,home,threat);
            }
            if(actor.restockPhase==RestockPhase.Returning)
            {
                if(!home.Contains(Key(actor.x,actor.y)))return BeginRestockReturn(index,ref actor,home,threat);
                actor.homeShelterId=0;
                if(actor.hunger>=HungerRules.SeekAt){actors[index]=actor;return false;}
                if(actor.carriedFood<=FoodReserve(actor) || (BoxMemoryEnabled?KnownStorageFull(actor,target):StorageRules.Used(target)>=target.capacity)){ClearRestocking(ref actor);actors[index]=actor;return false;}
                if(TryUseStorage(index,ref actor,threat,home,true,out direction,actor.restockBox))
                {
                    if(actor.carriedFood<=FoodReserve(actor)){ClearRestocking(ref actor);actors[index]=actor;}return true;
                }
                return false;
            }
            bool inside=home.Contains(Key(actor.x,actor.y));
            if(!SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy || actor.carriedFood>=HungerRules.CarryLimit)
            {
                if(inside){ClearRestocking(ref actor);ClearShelterDeparture(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
                return BeginRestockReturn(index,ref actor,home,threat);
            }
            if(inside || actor.shelterExitPhase!=ShelterExitPhase.None)return false; // Use the existing staged door inspection until physically outdoors.
            int before=actor.carriedFood;
            if(CollectShelfFood(index,ref actor,threat,null,out direction,true))
            {
                if(actor.carriedFood>before){actor.restockPhase=RestockPhase.Returning;ClearFoodMemory(ref actor);actors[index]=actor;}return true;
            }
            return BeginRestockReturn(index,ref actor,home,threat);
        }
        private void ValidateRestocking()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool active=actor.restockPhase!=RestockPhase.None;
                if(!active){if(actor.restockBox!=0 || actor.restockHomeId!=0 || actor.restockUntilTick!=0)throw new ArgumentException("Inactive restock task contains state.");continue;}
                Room home=FindRoom(actor.restockHomeId);int key=actor.restockBox-1;
                if(!RestockingEnabled || actor.species!=Species.Human || actor.health<=0 || actor.restockPhase<RestockPhase.Collecting || actor.restockPhase>RestockPhase.Returning || actor.restockBox<=0 || home==null || !home.Contains(key) || map.StorageBoxAt(key%Width,key/Width)==null || actor.restockUntilTick<=Tick || actor.restockUntilTick-Tick>RestockRules.TaskTicks)throw new ArgumentException("Invalid restock task.");
                for(int j=0;j<i;j++)if(actors[j].restockBox==actor.restockBox)throw new ArgumentException("Duplicate storage restock claim.");
            }
        }
    }
}
