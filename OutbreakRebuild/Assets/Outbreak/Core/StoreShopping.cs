using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StoreShoppingRules
    {
        public const int RoutineReviewTicks=300; // Thirty game minutes; urgent food-seeking never waits.
        public static int TriggerMeals(int target,int intelligence){return Math.Max(0,Math.Min(target-1,target*(intelligence+2)/12));}
    }
    public sealed partial class World
    {
        private bool StoreEntryCell(Room room,int key)
        {
            if(!room.Contains(key))return false;foreach(int door in room.doors)if(Math.Abs(key%Width-door%Width)+Math.Abs(key/Width-door/Width)==1)return true;return false;
        }
        private bool TryClearStoreEntry(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(!StoreEntryFlowEnabled || !WalkingShoppingTask(actor) || !SafeNeedsAction(actor,threat) || AssignedVehicle(index)>=0 || actor.shelterExitPhase!=ShelterExitPhase.None || (!actor.shoppingResting && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy))return false;
            Room room=FindRoom(HouseAt(actor.x,actor.y));if(room==null || !stores.ContainsKey(room.id) || !StoreEntryCell(room,Key(actor.x,actor.y)))return false;
            // Rest inside, away from the door's entry square. The normal move commit pays every step; plans and cargo stay intact.
            int best=int.MaxValue;for(int d=0;d<8;d+=2){int x=actor.x+dx[d],y=actor.y+dy[d],key=Key(x,y);if(!room.Contains(key) || StoreEntryCell(room,key) || IsDoor(x,y) || !MayMove(actor,d) || ZombieAt(x,y))continue;if(key<best){best=key;direction=d;}}
            return direction!=8;
        }
        private bool VisibleStoreEntryCrowd(ActorState observer,Room room)
        {
            if(room==null)return false;for(int i=0;i<count;i++){var other=actors[i];int key=Key(other.x,other.y);if(other.id==observer.id || other.species!=Species.Human || other.health<=0 || occupancy.Allows(key,Species.Human) || (!StoreEntryCell(room,key) && Array.BinarySearch(room.doors,key)<0))continue;
                if(DistanceSquared(observer,other)<=EncounterRules.SightRadius*EncounterRules.SightRadius && Visible(observer,other))return true;}
            return false; // Physical crowd observations reveal no shelf or stockroom inventory.
        }
        private bool StoreDoorHasVisibleArrival(ActorState observer,Room room,int door)
        {
            if(!StoreEntryFlowEnabled || !stores.ContainsKey(room.id) || NearestVisible(observer,Species.Zombie)>=0)return false;
            for(int i=0;i<count;i++){var other=actors[i];if(other.id!=observer.id && other.species==Species.Human && other.health>0 && !room.Contains(Key(other.x,other.y)) && Math.Abs(other.x-door%Width)+Math.Abs(other.y-door/Width)==1 && Visible(observer,other))return true;}
            return false; // Hold an already open store door for a visible arrival, without changing home-door policy.
        }
        private bool TryInspectBlockedStoreEntry(int index,ref ActorState actor,Room store,out int direction)
        {
            direction=8;if(!StoreEntryFlowEnabled || AssignedVehicle(index)>=0 || !SafeNeedsAction(actor,-1))return false;
            if(VisibleStoreEntryCrowd(actor,store)){
                // A waiting arrival steps off a road/doorway if possible; the existing shopping deadline still bounds the wait.
                if(IsDoor(actor.x,actor.y) || TerrainRules.RoadSurface(map.Get(actor.x,actor.y)))for(int d=0;d<8;d+=2){int x=actor.x+dx[d],y=actor.y+dy[d];if(map.CanWalk(x,y) && HouseAt(x,y)==0 && !IsDoor(x,y) && !TerrainRules.RoadSurface(map.Get(x,y)) && MayMove(actor,d)){direction=d;break;}}
                return true;
            }
            int best=int.MaxValue;foreach(int door in store.doors){int x=door%Width,y=door/Width;if(map.Get(x,y)!=Terrain.DoorClosed)continue;
                for(int d=0;d<8;d+=2){int nx=x+dx[d],ny=y+dy[d],length;if(!map.CanWalk(nx,ny) || HouseAt(nx,ny)!=0 || IsDoor(nx,ny) || TerrainRules.RoadSurface(map.Get(nx,ny)))continue;
                    // Inspect the actual closed entrance before treating an unreachable interior as a failed visit.
                    if(actor.x==nx && actor.y==ny){SetDoor(x,y,true);return true;}
                    int route=ShelterRoute(actor,nx,ny,0,out length,foodRoute:true);if(route>=0 && length<best){best=length;direction=route;}
                }
            }
            if(best==int.MaxValue)return false;if(direction<8 && map.Get(actor.x+dx[direction],actor.y+dy[direction])==Terrain.DoorClosed){SetDoor(actor.x+dx[direction],actor.y+dy[direction],true);direction=8;}return true;
        }
        private static bool IsShopping(ActorState actor){return actor.shoppingMeals>0;}
        private bool IsStoreShopper(ActorState actor)
        {
            return StoreShoppingEnabled && stores.Count>0 && actor.assignedHouseId!=0 && ((actor.clerkStoreId==0 && actor.courierSourceBox==0) || WorkerMayShopOffDuty(actor)) && BuildingUseForHouse(actor.assignedHouseId)==BuildingUse.Home;
        }
        private bool ShoppingDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return StoreShoppingEnabled && IsShopping(actor) && ((actor.restockPhase==RestockPhase.Returning && room.id!=actor.restockHomeId) || (StoreDiscoveryEnabled && actor.restockPhase==RestockPhase.Collecting && room.id!=actor.restockHomeId && room.id!=actor.shoppingStoreId)) && SafeRestock(actor,threat) && (EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy || CanSeekFoodForEmptyHome(actor));
        }
        private bool StartStoreShopping(int index,ref ActorState actor,int threat)
        {
            if((StoreDiscoveryEnabled && actor.shoppingRetryTick>Tick) || !SafeRestock(actor,threat) || (EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy && !CanSeekFoodForEmptyHome(actor)) || actor.carriedFood>FoodReserve(actor) || actor.foodExploreRoomId!=0)return false;
            Room home=FindRoom(actor.assignedHouseId);if(!home.Contains(Key(actor.x,actor.y)) || actor.shelterState!=ShelterState.Inside)return false;
            HomeSupplyKnowledge knowledge=HomeSuppliesKnownBy(index);if(knowledge.Boxes==0 || knowledge.KnownBoxes!=knowledge.Boxes)return false;
            // Spread consumer maintenance errands over existing action slots; workers retain food preparation around shifts.
            int slots=StoreShoppingRules.RoutineReviewTicks/5;
            if(DemandStaggeringEnabled && OccupationOf(actor)==HumanOccupation.Consumer && actor.health==100 && actor.hunger<HungerRules.SeekAt && knowledge.KnownMeals>0 && (Tick/5)%slots!=(actor.id%slots)*13%slots)return false;
            StorageBoxDocument target=null;int deficit=0;int sharedHome=PersonalFoodAccessEnabled && actor.hunger>=HungerRules.SeekAt && actor.carriedFood==0 && AssignedVehicle(index)<0 && CanSeekFoodForEmptyHome(actor)?home.id:0;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box) || RestockClaimed(Key(box.x,box.y)+1,actor.id,sharedHome))continue;
                StorageMemoryRecord? note=HomeNote(actor,box);int reserve=EffectiveFoodReserve(box);
                if(note.HasValue && note.Value.space>0 && reserve>0 && note.Value.food<=StoreShoppingRules.TriggerMeals(reserve,actor.intelligence)){target=box;deficit=reserve-note.Value.food;break;}
            }
            if(target==null)return false;
            ShelfMemoryRecord? chosen=KnownShoppingShelf(actor);
            if(!chosen.HasValue && !StoreDiscoveryEnabled)return false;
            actor.shoppingEndReason=ShoppingEndReason.None;actor.shoppingResting=false;
            actor.shoppingShelf=chosen.HasValue?Key(chosen.Value.x,chosen.Value.y)+1:0;
            actor.shoppingStoreId=StoreDiscoveryEnabled && chosen.HasValue?HouseAt(chosen.Value.x,chosen.Value.y):0;
            if(ShoppingContinuityEnabled && actor.shoppingStoreId==0)actor.shoppingStoreId=KnownShoppingStore(actor);
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
                if(!StoreShoppingEnabled || actor.species!=Species.Human || actor.health<=0 || ((actor.clerkStoreId!=0 || actor.courierSourceBox!=0) && !WorkerShiftsEnabled) || actor.courierOrderId!=0 || actor.restockPhase==RestockPhase.None || actor.assignedHouseId!=actor.restockHomeId || actor.restockSourceBox!=0 || actor.shoppingMeals<1 || actor.shoppingMeals>=HungerRules.CarryLimit || (actor.shoppingShelf==0 ? !StoreDiscoveryEnabled : key<0 || !map.Contains(key%Width,key/Width) || map.ShelfAt(key%Width,key/Width)==null || !stores.ContainsKey(HouseAt(key%Width,key/Width))))throw new ArgumentException("Invalid store shopping trip.");
            }
        }
    }
}
