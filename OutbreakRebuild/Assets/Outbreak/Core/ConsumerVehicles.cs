using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class ConsumerVehicleRules { public const int CargoMeals=48,CartMeals=6; }
    public sealed partial class World
    {
        public bool TryAssignVehicleShopper(int vehicleIndex,int actorIndex)
        {
            if(!ConsumerVehiclesEnabled || vehicleIndex<0 || vehicleIndex>=vehicles.Count || actorIndex<0 || actorIndex>=count || AssignedVehicle(actorIndex)>=0)return false;
            var actor=actors[actorIndex];var car=vehicles[vehicleIndex];Room home=FindRoom(actor.assignedHouseId);
            if(home==null || BuildingUseForHouse(home.id)!=BuildingUse.Home || actor.species!=Species.Human || actor.health<=0 || actor.turnAtTick!=0 || actor.clerkStoreId!=0 || actor.courierSourceBox!=0 || actor.restockPhase!=RestockPhase.None || actor.workerCommuteId!=0 || actor.grappledById!=0 || car.driverAboard || (car.driverId!=0 && HealthyVehicleDriver(car)) || (!car.consumerDriver && car.driverId!=0))return false;
            if(car.consumerPhase!=ConsumerVehiclePhase.None && (car.sourceBox<=0 || HouseAt((car.sourceBox-1)%Width,(car.sourceBox-1)/Width)!=home.id))return false;
            if(car.consumerPhase==ConsumerVehiclePhase.None && ParkingDoorDistance(Key(car.x,car.y),home)>64)return false;
            StopVehicleMotion(ref car);car.consumerDriver=true;car.driverId=actor.id;car.deliveriesPaused=false;
            if(car.consumerPhase!=ConsumerVehiclePhase.None){SetConsumerTask(ref actor,car);car.consumerPhase=car.shipmentDropped?ConsumerVehiclePhase.RecoverCart:ConsumerVehiclePhase.BoardingForHome;}
            actors[actorIndex]=actor;vehicles[vehicleIndex]=car;return true;
        }
        private void SetConsumerTask(ref ActorState actor,VehicleState car)
        {
            actor.shoppingMeals=1;actor.shoppingStoreId=HouseAt((car.targetBox-1)%Width,(car.targetBox-1)/Width);actor.shoppingShelf=0;actor.shoppingSearchCell=0;actor.shoppingResting=false;
            actor.restockBox=car.sourceBox;actor.restockHomeId=actor.assignedHouseId;actor.restockSourceBox=0;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+CourierRules.TaskTicks;actor.courierReturningHome=false;actor.homeShelterId=0;ClearShelterDeparture(ref actor);
        }
        private bool StartConsumerTrip(int index,ref ActorState actor,ref VehicleState car)
        {
            if((actor.health<ShelterRecoveryRules.ExtraReserveBelowHealth && !CanSeekFoodForEmptyHome(actor)) || actor.grappledById!=0 || actor.stress!=0 || actor.freezeUntilTick>Tick || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || (EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy && !CanSeekFoodForEmptyHome(actor)) || actor.restockPhase!=RestockPhase.None)return false;
            Room home=FindRoom(actor.assignedHouseId);if(home==null || BuildingUseForHouse(home.id)!=BuildingUse.Home || !home.Contains(Key(actor.x,actor.y)))return false;
            var knowledge=HomeSuppliesKnownBy(index);if(knowledge.Boxes==0 || knowledge.KnownBoxes!=knowledge.Boxes)return false;
            StorageBoxDocument target=null;int goal=0;
            foreach(var box in map.StorageRecords){if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box) || RestockClaimed(Key(box.x,box.y)+1,actor.id))continue;var note=HomeNote(actor,box);int reserve=EffectiveFoodReserve(box);if(note.HasValue && note.Value.space>0 && reserve>0 && note.Value.food<=StoreShoppingRules.TriggerMeals(reserve,actor.intelligence)){target=box;goal=Math.Min(note.Value.space,reserve-note.Value.food);break;}}
            if(target==null)return false;int shop=KnownShoppingStore(actor);StoreDocument store;if(shop==0 || !stores.TryGetValue(shop,out store))return false;
            car.sourceBox=Key(target.x,target.y)+1;car.targetBox=store.supplyBox;car.purchaseGoal=Math.Min(ConsumerVehicleRules.CargoMeals,goal);car.consumerPhase=car.cargoFood>0?ConsumerVehiclePhase.FetchHomeCart:ConsumerVehiclePhase.BoardingForStore;
            car.shoppingStops=car.cargoFood>0?0:1;SetConsumerTask(ref actor,car);actor.shoppingEndReason=ShoppingEndReason.None;actors[index]=actor;return true;
        }
        private void BeginConsumerReturn(ref ActorState actor,ref VehicleState car)
        {
            actor.shoppingResting=false;actor.restockPhase=RestockPhase.Returning;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);
            car.consumerPhase=car.handFood>0?ConsumerVehiclePhase.LoadingCar:ConsumerVehiclePhase.BoardingForHome;car.purchaseGoal=car.cargoFood+car.handFood;
        }
        private void FinishConsumerTask(int index,ref ActorState actor,ref VehicleState car)
        {
            ClearRestocking(ref actor);ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);car.consumerPhase=ConsumerVehiclePhase.None;car.sourceBox=car.targetBox=car.purchaseGoal=car.shoppingStops=0;car.shipmentDropped=false;car.shipmentX=car.shipmentY=0;
            Room home=FindRoom(actor.assignedHouseId);int goal,length;if(home!=null && !home.Contains(Key(actor.x,actor.y)) && FoodReturnGoal(actor,home,0,out goal,out length)){actor.homeShelterId=home.id;actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;}actors[index]=actor;
        }
        private void PrepareConsumerVehicle(int vehicleIndex)
        {
            var car=vehicles[vehicleIndex];int index=VehicleDriverIndex(car.driverId);if(index<0)return;
            if(!HealthyVehicleDriver(car)){
                StopVehicleMotion(ref car);car.deliveriesPaused=true;
                if(car.handFood>0 && !car.shipmentDropped){car.shipmentDropped=true;car.shipmentX=actors[index].x;car.shipmentY=actors[index].y;car.consumerPhase=ConsumerVehiclePhase.RecoverCart;}
                vehicles[vehicleIndex]=car;if(car.driverAboard)ExitVehicle(vehicleIndex,true);return;
            }
            var actor=actors[index];if(car.driverAboard){EatCourierMeal(ref actor,ref car);actors[index]=actor;}
            if(car.consumerPhase!=ConsumerVehiclePhase.None && !IsShopping(actor)){SetConsumerTask(ref actor,car);BeginConsumerReturn(ref actor,ref car);actors[index]=actor;}
            vehicles[vehicleIndex]=car;if(!car.driverAboard || car.deliveriesPaused)return;
            bool toStore=car.consumerPhase==ConsumerVehiclePhase.DrivingToStore;if(!toStore && car.consumerPhase!=ConsumerVehiclePhase.DrivingHome)return;
            if(!car.hasDestination && (car.nextRoutePlanTick==0 || car.nextRoutePlanTick<=Tick)){car.nextRoutePlanTick=Tick+30;vehicles[vehicleIndex]=car;int park=VehicleParking(car,toStore?car.targetBox:car.sourceBox);if(park>=0)SetVehicleDestination(vehicleIndex,park%Width,park/Width,true);}
        }
        private void FinishConsumerVehicle(int vehicleIndex)
        {
            var car=vehicles[vehicleIndex];if(!car.driverAboard || car.deliveriesPaused || !car.hasDestination || car.nextMoveTick!=0)return;
            bool store=car.consumerPhase==ConsumerVehiclePhase.DrivingToStore;if(!store && car.consumerPhase!=ConsumerVehiclePhase.DrivingHome)return;
            if(car.exitAtTick==0){car.exitAtTick=Tick+VehicleRules.MoveTicks;vehicles[vehicleIndex]=car;return;}if(Tick<car.exitAtTick || !ExitVehicle(vehicleIndex))return;
            car=vehicles[vehicleIndex];StopVehicleMotion(ref car);car.consumerPhase=store?ConsumerVehiclePhase.Shopping:ConsumerVehiclePhase.FetchHomeCart;vehicles[vehicleIndex]=car;
        }
        private int ConsumerCarWalk(int index,ref ActorState actor,VehicleState car)
        {
            int best=-1,bestLength=int.MaxValue;for(int d=0;d<4;d++){int x=car.x+carDX[d],y=car.y+carDY[d],length;if(!map.CanWalk(x,y) || VehicleAt(x,y))continue;if(ShelterRoute(actor,x,y,0,out length)>=0 && length<bestLength){best=Key(x,y);bestLength=length;}}
            return best<0?8:WalkCourierVehicle(index,ref actor,best%Width,best/Width);
        }
        private bool ConsumerAtCar(ActorState actor,VehicleState car){return Math.Abs(actor.x-car.x)+Math.Abs(actor.y-car.y)==1;}
        private bool TryConsumerVehicleRoutine(int index,ref ActorState actor,int threat,int carIndex,out int direction)
        {
            direction=8;var car=vehicles[carIndex];if(!HealthyVehicleDriver(car) || car.driverAboard || threat>=0 || actor.grappledById!=0 || actor.freezeUntilTick>Tick || actor.stress!=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor))return false;
            ObserveShelves(ref actor);ObserveStorageBoxes(ref actor);ObserveStoreEntrances(ref actor);
            var doorContext=car;doorContext.deliveryPhase=car.consumerPhase==ConsumerVehiclePhase.Shopping?VehicleDeliveryPhase.Unloading:car.consumerPhase==ConsumerVehiclePhase.UnloadingHome?VehicleDeliveryPhase.Collecting:VehicleDeliveryPhase.None;CloseCourierDepartureDoors(actor,doorContext);
            int personal=actor.carriedFood,cart=car.handFood;EatCourierMeal(ref actor,ref car);actors[index]=actor;vehicles[carIndex]=car;if(personal!=actor.carriedFood || cart!=car.handFood)return true;
            if(car.deliveriesPaused)return car.consumerPhase!=ConsumerVehiclePhase.None;
            if(car.consumerPhase==ConsumerVehiclePhase.None){if(!StartConsumerTrip(index,ref actor,ref car))return false;vehicles[carIndex]=car;}
            if(TryShoppingRest(index,ref actor))return true;
            if(car.consumerPhase==ConsumerVehiclePhase.RecoverCart){if(Math.Abs(actor.x-car.shipmentX)+Math.Abs(actor.y-car.shipmentY)<=1){car.shipmentDropped=false;car.shipmentX=car.shipmentY=0;BeginConsumerReturn(ref actor,ref car);}else direction=WalkCourierVehicle(index,ref actor,car.shipmentX,car.shipmentY);actors[index]=actor;vehicles[carIndex]=car;return true;}
            if(car.consumerPhase==ConsumerVehiclePhase.BoardingForStore || car.consumerPhase==ConsumerVehiclePhase.BoardingForHome){
                if(ConsumerAtCar(actor,car)){bool store=car.consumerPhase==ConsumerVehiclePhase.BoardingForStore;if(BoardVehicle(index,carIndex)){car=vehicles[carIndex];car.consumerPhase=store?ConsumerVehiclePhase.DrivingToStore:ConsumerVehiclePhase.DrivingHome;vehicles[carIndex]=car;}}else direction=ConsumerCarWalk(index,ref actor,car);return true;
            }
            if(car.consumerPhase==ConsumerVehiclePhase.LoadingCar || car.consumerPhase==ConsumerVehiclePhase.ReturnCart){
                if(!ConsumerAtCar(actor,car)){direction=ConsumerCarWalk(index,ref actor,car);return true;}
                int load=Math.Min(car.handFood,ConsumerVehicleRules.CargoMeals-car.cargoFood);car.handFood-=load;car.cargoFood+=load;
                if(car.handFood==0){if(car.consumerPhase==ConsumerVehiclePhase.ReturnCart)FinishConsumerTask(index,ref actor,ref car);else car.consumerPhase=car.cargoFood<car.purchaseGoal?ConsumerVehiclePhase.Shopping:ConsumerVehiclePhase.BoardingForHome;}
                vehicles[carIndex]=car;return true;
            }
            if(car.consumerPhase==ConsumerVehiclePhase.FetchHomeCart){
                if(car.cargoFood==0){FinishConsumerTask(index,ref actor,ref car);vehicles[carIndex]=car;return true;}
                if(!ConsumerAtCar(actor,car)){direction=ConsumerCarWalk(index,ref actor,car);return true;}
                int take=Math.Min(ConsumerVehicleRules.CartMeals,car.cargoFood);car.cargoFood-=take;car.handFood=take;car.consumerPhase=ConsumerVehiclePhase.UnloadingHome;vehicles[carIndex]=car;return true;
            }
            if(car.consumerPhase==ConsumerVehiclePhase.UnloadingHome){
                int key=car.sourceBox-1;var box=map.StorageBoxAt(key%Width,key/Width);if(box==null){car.deliveriesPaused=true;vehicles[carIndex]=car;return true;}
                if(IsRestrictedSupplyBox(box)){car.consumerPhase=ConsumerVehiclePhase.ReturnCart;vehicles[carIndex]=car;return true;}
                if(Math.Abs(actor.x-box.x)+Math.Abs(actor.y-box.y)!=1){int goal=CourierBoxGoal(actor,box);if(goal>=0)direction=WalkCourierVehicle(index,ref actor,goal%Width,goal/Width);return true;}
                int amount=Math.Min(car.handFood,Math.Min(box.capacity-StorageRules.Used(box),Math.Max(0,EffectiveFoodReserve(box)-StorageRules.Count(box,InventoryItem.Food))));
                var update=StorageRules.Copy(box);StorageRules.ChangeFood(update,amount);if(amount>0)map.SetStorageBox(update);RememberStorage(ref actor,StorageObservation(update));car.handFood-=amount;
                if(car.handFood>0)car.consumerPhase=ConsumerVehiclePhase.ReturnCart;else if(car.cargoFood>0 && StorageRules.Count(update,InventoryItem.Food)<EffectiveFoodReserve(update) && StorageRules.Used(update)<update.capacity)car.consumerPhase=ConsumerVehiclePhase.FetchHomeCart;else FinishConsumerTask(index,ref actor,ref car);
                actors[index]=actor;vehicles[carIndex]=car;return true;
            }
            if(car.consumerPhase==ConsumerVehiclePhase.Shopping)return CollectConsumerCart(index,ref actor,ref car,carIndex,out direction);
            return true;
        }
        private bool CollectConsumerCart(int index,ref ActorState actor,ref VehicleState car,int carIndex,out int direction)
        {
            direction=8;int storeId=HouseAt((car.targetBox-1)%Width,(car.targetBox-1)/Width);Room store=FindRoom(storeId);var home=FindRoom(actor.assignedHouseId);
            if(car.cargoFood>=car.purchaseGoal){BeginConsumerReturn(ref actor,ref car);actors[index]=actor;vehicles[carIndex]=car;return true;}
            var known=KnownShoppingShelf(actor,storeId);
            if(!known.HasValue){actor.shoppingShelf=0;actor.shoppingStoreId=storeId;TryDiscoverShoppingStore(index,ref actor,home,out direction);if(actor.shoppingStoreId!=storeId || actor.restockPhase==RestockPhase.Returning){int next=KnownShoppingStore(actor);if(next!=0 && next!=storeId && car.shoppingStops<KnownConsumerStoreCount(actor) && car.cargoFood<car.purchaseGoal){car.shoppingStops++;car.targetBox=stores[next].supplyBox;car.consumerPhase=ConsumerVehiclePhase.BoardingForStore;actor.shoppingStoreId=next;actor.shoppingShelf=0;actor.shoppingSearchCell=0;actor.restockPhase=RestockPhase.Collecting;}else BeginConsumerReturn(ref actor,ref car);actors[index]=actor;vehicles[carIndex]=car;}return true;}
            int key=Key(known.Value.x,known.Value.y);var shelf=map.ShelfAt(known.Value.x,known.Value.y);actor.shoppingShelf=key+1;int goal=-1,length=int.MaxValue,route=-1;
            for(int d=0;d<4;d++){int x=shelf.x+carDX[d],y=shelf.y+carDY[d],steps;if(!map.CanWalk(x,y) || !store.Contains(Key(x,y)))continue;int candidate=ShelterRoute(actor,x,y,0,out steps);if(candidate>=0 && steps<length){goal=Key(x,y);length=steps;route=candidate;}}
            if(route<0){actor.shoppingEndReason=ShoppingEndReason.NoRoute;BeginConsumerReturn(ref actor,ref car);actors[index]=actor;vehicles[carIndex]=car;return true;}
            if(length>0){direction=WalkCourierVehicle(index,ref actor,goal%Width,goal/Width);return true;}
            if(!shelf.searched && actor.shelfSearchProgress<2){actor.shelfSearchProgress++;actors[index]=actor;EmitSearchNoise(actor);return true;}
            int pocket=Math.Min(Math.Max(0,FoodReserve(actor)-actor.carriedFood),shelf.food);int take=Math.Min(ConsumerVehicleRules.CartMeals,Math.Min(shelf.food-pocket,car.purchaseGoal-car.cargoFood));
            actor.carriedFood+=pocket;car.handFood+=take;map.SetShelf(shelf.x,shelf.y,shelf.food-pocket-take,true);RememberShelf(ref actor,new ShelfMemoryRecord{x=shelf.x,y=shelf.y,food=shelf.food-pocket-take,observedTick=Tick});actor.shelfSearchProgress=0;
            if(take>0)car.consumerPhase=ConsumerVehiclePhase.LoadingCar;else actor.shoppingShelf=0;actors[index]=actor;vehicles[carIndex]=car;EmitSearchNoise(actor);return true;
        }
        private int KnownConsumerStoreCount(ActorState actor){var known=new HashSet<int>();if(actor.familiarStoreId!=0)known.Add(actor.familiarStoreId);if(actor.additionalFamiliarStores!=null)foreach(int id in actor.additionalFamiliarStores)known.Add(id);if(actor.storeMemory!=null)foreach(var note in actor.storeMemory)known.Add(note.roomId);return Math.Min(StoreDiscoveryRules.MemoryLimit,known.Count);}
        public string ConsumerVehicleStatus(int index)
        {
            var car=VehicleAtIndex(index);if(!HealthyVehicleDriver(car))return "Shopper incapacitated; cargo retained";if(car.deliveriesPaused)return "Vehicle shopping paused";
            switch(car.consumerPhase){case ConsumerVehiclePhase.None:return "Waiting for home supply shortage";case ConsumerVehiclePhase.BoardingForStore:return "Walking to shopping vehicle";case ConsumerVehiclePhase.DrivingToStore:return "Driving to store parking";case ConsumerVehiclePhase.Shopping:return "Collecting a shopping cart load";case ConsumerVehiclePhase.LoadingCar:return "Hauling cart to parked vehicle";case ConsumerVehiclePhase.BoardingForHome:return "Boarding for home";case ConsumerVehiclePhase.DrivingHome:return "Driving groceries home";case ConsumerVehiclePhase.FetchHomeCart:return "Fetching groceries from vehicle";case ConsumerVehiclePhase.UnloadingHome:return "Hauling groceries to home storage";case ConsumerVehiclePhase.ReturnCart:return "Returning excess groceries to vehicle";default:return "Recovering abandoned shopping cart";}
        }
        private void ValidateConsumerVehicle(VehicleState car,HashSet<int> assigned)
        {
            int index=VehicleDriverIndex(car.driverId);if(!ConsumerVehiclesEnabled || index<0 || !assigned.Add(car.driverId) || car.orderId!=0 || car.deliveryPhase!=VehicleDeliveryPhase.None || car.consumerPhase<ConsumerVehiclePhase.None || car.consumerPhase>ConsumerVehiclePhase.RecoverCart || car.cargoFood<0 || car.cargoFood>ConsumerVehicleRules.CargoMeals || car.handFood<0 || car.handFood>ConsumerVehicleRules.CartMeals || car.cargoFood+car.handFood>ConsumerVehicleRules.CargoMeals || car.shoppingStops<0 || car.shoppingStops>StoreDiscoveryRules.MemoryLimit || car.purchaseGoal<0 || car.purchaseGoal>ConsumerVehicleRules.CargoMeals || car.driveEffort<0 || car.driveEffort>=CourierVehicleRules.DriveEffortCells)throw new ArgumentException("Invalid consumer vehicle or shopping cargo.");
            var actor=actors[index];if(HealthyVehicleDriver(car) && car.consumerPhase==ConsumerVehiclePhase.None && actor.restockPhase!=RestockPhase.None)throw new ArgumentException("Idle shopping vehicle has a conflicting task.");if(HealthyVehicleDriver(car) && (actor.assignedHouseId==0 || actor.clerkStoreId!=0 || actor.courierSourceBox!=0 || actor.courierOrderId!=0 || (car.consumerPhase!=ConsumerVehiclePhase.None && (!IsShopping(actor) || actor.restockBox!=car.sourceBox))))throw new ArgumentException("Consumer vehicle lost its shopper task.");
            if(car.lastMealTick<0 || car.lastMealTick>Tick || car.nextRoutePlanTick<0 || car.nextRoutePlanTick>Tick+30 || (car.nextRoutePlanTick!=0 && (!car.driverAboard || car.hasDestination)) || car.exitAtTick<0 || car.exitAtTick>Tick+VehicleRules.MoveTicks || (car.exitAtTick!=0 && (!car.driverAboard || !car.hasDestination || car.nextMoveTick!=0)))throw new ArgumentException("Invalid consumer vehicle clock.");
            if((!car.driverAboard && car.hasDestination) || (car.driverAboard && (actor.x!=car.x || actor.y!=car.y || actor.grappledById!=0 || car.handFood!=0 || car.consumerPhase==ConsumerVehiclePhase.None)) || (!HealthyVehicleDriver(car) && !car.deliveriesPaused))throw new ArgumentException("Invalid consumer vehicle occupant.");
            if(HealthyVehicleDriver(car) && !car.deliveriesPaused && !car.driverAboard && (car.consumerPhase==ConsumerVehiclePhase.DrivingToStore || car.consumerPhase==ConsumerVehiclePhase.DrivingHome))throw new ArgumentException("Driving shopper lost its occupant.");
            if(car.consumerPhase==ConsumerVehiclePhase.None){if(car.sourceBox!=0 || car.targetBox!=0 || car.purchaseGoal!=0 || car.shoppingStops!=0 || car.handFood!=0 || car.shipmentDropped)throw new ArgumentException("Idle shopper retained an active cart.");}
            else {int source=car.sourceBox-1,target=car.targetBox-1;if(car.sourceBox<=0 || car.targetBox<=0 || !map.Contains(source%Width,source/Width) || !map.Contains(target%Width,target/Width) || map.StorageBoxAt(source%Width,source/Width)==null || map.StorageBoxAt(target%Width,target/Width)==null || !stores.ContainsKey(HouseAt(target%Width,target/Width)))throw new ArgumentException("Consumer shopping lost a home or store box.");}
            if(car.shipmentDropped){if(car.handFood<=0 || car.consumerPhase!=ConsumerVehiclePhase.RecoverCart || !map.Contains(car.shipmentX,car.shipmentY))throw new ArgumentException("Invalid abandoned shopping cart.");}else if(car.shipmentX!=0 || car.shipmentY!=0)throw new ArgumentException("Inactive shopping cart retained a ground position.");
        }
    }
}
