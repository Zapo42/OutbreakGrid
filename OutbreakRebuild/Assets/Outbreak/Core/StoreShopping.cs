using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StoreShoppingRules
    {
        public static int TriggerMeals(int target,int intelligence){return Math.Max(0,Math.Min(target-1,target*(intelligence+2)/12));}
    }
    public sealed partial class World
    {
        private bool IsStoreShopper(ActorState actor)
        {
            return StoreShoppingEnabled && stores.Count>0 && actor.assignedHouseId!=0 && actor.clerkStoreId==0 && actor.courierSourceBox==0 && BuildingUseForHouse(actor.assignedHouseId)==BuildingUse.Home;
        }
        private bool ShoppingDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return StoreShoppingEnabled && actor.shoppingShelf!=0 && actor.restockPhase==RestockPhase.Returning && room.id!=actor.restockHomeId && SafeRestock(actor,threat) && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy;
        }
        private bool StartStoreShopping(int index,ref ActorState actor,int threat)
        {
            if(!SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.carriedFood>FoodReserve(actor) || actor.foodExploreRoomId!=0)return false;
            Room home=FindRoom(actor.assignedHouseId);if(!home.Contains(Key(actor.x,actor.y)) || actor.shelterState!=ShelterState.Inside)return false;
            HomeSupplyKnowledge knowledge=HomeSuppliesKnownBy(index);if(knowledge.Boxes==0 || knowledge.KnownBoxes!=knowledge.Boxes)return false;
            StorageBoxDocument target=null;int deficit=0;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box) || RestockClaimed(Key(box.x,box.y)+1,actor.id))continue;
                StorageMemoryRecord? note=HomeNote(actor,box);int reserve=EffectiveFoodReserve(box);
                if(note.HasValue && note.Value.space>0 && reserve>0 && note.Value.food<=StoreShoppingRules.TriggerMeals(reserve,actor.intelligence)){target=box;deficit=reserve-note.Value.food;break;}
            }
            if(target==null || actor.shelfMemory==null)return false;
            ShelfMemoryRecord chosen=new ShelfMemoryRecord();int best=int.MaxValue,rank=-1;
            foreach(ShelfMemoryRecord note in actor.shelfMemory)
            {
                if(note.food==0 || !stores.ContainsKey(HouseAt(note.x,note.y)) || AvoidBlockedFood(actor,note.x,note.y))continue;
                int distance=FoodShelfRouteLength(actor,note),priority=note.food>0?1:0;
                if(distance==int.MaxValue)continue;
                if(priority>rank || (priority==rank && distance<best)){chosen=note;best=distance;rank=priority;}
            }
            if(rank<0)return false;
            actor.shoppingShelf=Key(chosen.x,chosen.y)+1;actor.shoppingMeals=Math.Min(deficit,HungerRules.CarryLimit-FoodReserve(actor));
            if(actor.shoppingMeals<=0){actor.shoppingShelf=0;return false;}
            actor.courierReturningHome=false;actor.restockBox=Key(target.x,target.y)+1;actor.restockHomeId=home.id;actor.restockSourceBox=0;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+CourierRules.TaskTicks;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        private bool TryCollectShoppingShelf(int index,ref ActorState actor,Room home,out int direction)
        {
            direction=8;int key=actor.shoppingShelf-1;ShelfDocument shelf=map.ShelfAt(key%Width,key/Width);Room store=FindRoom(HouseAt(key%Width,key/Width));
            if(shelf==null || store==null || !stores.ContainsKey(store.id)){BeginRestockReturn(index,ref actor,home,-1);return false;}
            int best=int.MaxValue,goal=-1,route=-1;
            for(int d=0;d<8;d+=2)
            {
                int x=shelf.x+dx[d],y=shelf.y+dy[d],length;if(!map.CanWalk(x,y) || !store.Contains(Key(x,y)))continue;
                int candidate=ShelterRoute(actor,x,y,0,out length,foodRoute:true);if(candidate>=0 && length<best){best=length;goal=Key(x,y);route=candidate;}
            }
            if(goal<0){actor.blockedFoodX=shelf.x;actor.blockedFoodY=shelf.y;actor.blockedFoodUntilTick=Tick+FoodRouteMemoryRules.RetryTicks;BeginRestockReturn(index,ref actor,home,-1);return false;}
            actor.shelterId=store.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=store.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;
            actor.foodX=shelf.x;actor.foodY=shelf.y;actor.foodUntilTick=Tick+HungerRules.MemoryTicks;
            if(best>0){actor.shelfSearchProgress=0;direction=route;int nx=actor.x+dx[route],ny=actor.y+dy[route];if(map.Get(nx,ny)==Terrain.DoorClosed){SetDoor(nx,ny,true);direction=8;}actors[index]=actor;return true;}
            CloseNearbyDoors(actor,store);
            if(!shelf.searched && actor.shelfSearchProgress<2){actor.shelfSearchProgress++;actors[index]=actor;EmitSearchNoise(actor);return true;}
            int take=Math.Min(actor.shoppingMeals+Math.Max(0,FoodReserve(actor)-actor.carriedFood),Math.Min(HungerRules.CarryLimit-actor.carriedFood,shelf.food));
            map.SetShelf(shelf.x,shelf.y,shelf.food-take,true);actor.carriedFood+=take;RememberShelf(ref actor,new ShelfMemoryRecord{x=shelf.x,y=shelf.y,food=shelf.food-take,observedTick=Tick});
            actor.restockPhase=RestockPhase.Returning;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;EmitSearchNoise(actor);return true;
        }
        private void ValidateStoreShopping()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.shoppingShelf==0){if(actor.shoppingMeals!=0)throw new ArgumentException("Inactive shopping contains a quantity.");continue;}
                int key=actor.shoppingShelf-1;
                if(!StoreShoppingEnabled || actor.species!=Species.Human || actor.health<=0 || actor.clerkStoreId!=0 || actor.courierSourceBox!=0 || actor.courierOrderId!=0 || actor.restockPhase==RestockPhase.None || actor.assignedHouseId!=actor.restockHomeId || actor.restockSourceBox!=0 || actor.shoppingMeals<1 || actor.shoppingMeals>=HungerRules.CarryLimit || key<0 || !map.Contains(key%Width,key/Width) || map.ShelfAt(key%Width,key/Width)==null || !stores.ContainsKey(HouseAt(key%Width,key/Width)))throw new ArgumentException("Invalid store shopping trip.");
            }
        }
    }
}
