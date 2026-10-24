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
        private static bool IsShopping(ActorState actor){return actor.shoppingMeals>0;}
        private bool IsStoreShopper(ActorState actor)
        {
            return StoreShoppingEnabled && stores.Count>0 && actor.assignedHouseId!=0 && actor.clerkStoreId==0 && actor.courierSourceBox==0 && BuildingUseForHouse(actor.assignedHouseId)==BuildingUse.Home;
        }
        private bool ShoppingDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return StoreShoppingEnabled && IsShopping(actor) && ((actor.restockPhase==RestockPhase.Returning && room.id!=actor.restockHomeId) || (StoreDiscoveryEnabled && actor.restockPhase==RestockPhase.Collecting && room.id!=actor.restockHomeId && room.id!=actor.shoppingStoreId)) && SafeRestock(actor,threat) && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy;
        }
        private bool StartStoreShopping(int index,ref ActorState actor,int threat)
        {
            if((StoreDiscoveryEnabled && actor.shoppingRetryTick>Tick) || !SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.carriedFood>FoodReserve(actor) || actor.foodExploreRoomId!=0)return false;
            Room home=FindRoom(actor.assignedHouseId);if(!home.Contains(Key(actor.x,actor.y)) || actor.shelterState!=ShelterState.Inside)return false;
            HomeSupplyKnowledge knowledge=HomeSuppliesKnownBy(index);if(knowledge.Boxes==0 || knowledge.KnownBoxes!=knowledge.Boxes)return false;
            StorageBoxDocument target=null;int deficit=0;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box) || RestockClaimed(Key(box.x,box.y)+1,actor.id))continue;
                StorageMemoryRecord? note=HomeNote(actor,box);int reserve=EffectiveFoodReserve(box);
                if(note.HasValue && note.Value.space>0 && reserve>0 && note.Value.food<=StoreShoppingRules.TriggerMeals(reserve,actor.intelligence)){target=box;deficit=reserve-note.Value.food;break;}
            }
            if(target==null)return false;
            ShelfMemoryRecord? chosen=KnownShoppingShelf(actor);
            if(!chosen.HasValue && !StoreDiscoveryEnabled)return false;
            actor.shoppingShelf=chosen.HasValue?Key(chosen.Value.x,chosen.Value.y)+1:0;
            actor.shoppingStoreId=StoreDiscoveryEnabled && chosen.HasValue?HouseAt(chosen.Value.x,chosen.Value.y):0;
            actor.shoppingSearchCell=0;actor.shoppingMeals=Math.Min(deficit,HungerRules.CarryLimit-FoodReserve(actor));
            if(actor.shoppingMeals<=0){actor.shoppingShelf=0;return false;}
            actor.courierReturningHome=false;actor.restockBox=Key(target.x,target.y)+1;actor.restockHomeId=home.id;actor.restockSourceBox=0;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+CourierRules.TaskTicks;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        private bool TryCollectShoppingShelf(int index,ref ActorState actor,Room home,out int direction)
        {
            direction=8;int key=actor.shoppingShelf-1;ShelfDocument shelf=map.ShelfAt(key%Width,key/Width);Room store=FindRoom(HouseAt(key%Width,key/Width));
            if(shelf==null || store==null || !stores.ContainsKey(store.id)){BeginRestockReturn(index,ref actor,home,-1);return false;}
            if(StoreDiscoveryEnabled)
            {
                actor.shoppingStoreId=store.id;
                Room current=FindRoom(actor.shelterId);
                if(current!=null && current.id!=store.id && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None))return TryShelter(index,ref actor,-1,out direction);
            }
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
            if(StoreDiscoveryEnabled && take==0){actor.shoppingShelf=0;actor.shoppingStoreId=store.id;actor.shoppingSearchCell=0;ClearFoodMemory(ref actor);actors[index]=actor;EmitSearchNoise(actor);return true;}
            actor.restockPhase=RestockPhase.Returning;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;EmitSearchNoise(actor);return true;
        }
        private void ValidateStoreShopping()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(!IsShopping(actor)){if(actor.shoppingShelf!=0 || actor.shoppingMeals!=0)throw new ArgumentException("Inactive shopping contains state.");continue;}
                int key=actor.shoppingShelf-1;
                if(!StoreShoppingEnabled || actor.species!=Species.Human || actor.health<=0 || actor.clerkStoreId!=0 || actor.courierSourceBox!=0 || actor.courierOrderId!=0 || actor.restockPhase==RestockPhase.None || actor.assignedHouseId!=actor.restockHomeId || actor.restockSourceBox!=0 || actor.shoppingMeals<1 || actor.shoppingMeals>=HungerRules.CarryLimit || (actor.shoppingShelf==0 ? !StoreDiscoveryEnabled : key<0 || !map.Contains(key%Width,key/Width) || map.ShelfAt(key%Width,key/Width)==null || !stores.ContainsKey(HouseAt(key%Width,key/Width))))throw new ArgumentException("Invalid store shopping trip.");
            }
        }
    }
}
