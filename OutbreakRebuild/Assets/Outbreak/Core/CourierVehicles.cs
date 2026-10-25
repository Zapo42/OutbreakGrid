using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class CourierVehicleRules { public const int CargoMeals=24,DriveEffortCells=10,ParkingRadius=48; }
    public sealed partial class World
    {
        private int VehicleDriverIndex(int id){for(int i=0;i<count;i++)if(actors[i].id==id)return i;return -1;}
        private bool ActorRiding(int id){if(!CourierVehiclesEnabled || id==0)return false;foreach(var car in vehicles)if(car.driverId==id && car.driverAboard)return true;return false;}
        public bool ActorInVehicle(int index){return index>=0 && index<count && ActorRiding(actors[index].id);}
        public int AssignedVehicle(int actorIndex){if(!CourierVehiclesEnabled || actorIndex<0 || actorIndex>=count)return -1;for(int i=0;i<vehicles.Count;i++)if(vehicles[i].driverId==actors[actorIndex].id)return i;return -1;}
        private bool HealthyVehicleDriver(VehicleState car){int index=VehicleDriverIndex(car.driverId);return index>=0 && actors[index].species==Species.Human && actors[index].health>0 && actors[index].turnAtTick==0;}
        public bool TryAssignVehicleCourier(int vehicleIndex,int actorIndex)
        {
            if(!CourierVehiclesEnabled || vehicleIndex<0 || vehicleIndex>=vehicles.Count || actorIndex<0 || actorIndex>=count || AssignedVehicle(actorIndex)>=0)return false;
            VehicleState car=vehicles[vehicleIndex];ActorState actor=actors[actorIndex];if(actor.species!=Species.Human || actor.health<=0 || actor.turnAtTick!=0 || actor.distributionCenterId==0 || actor.courierSourceBox==0 || actor.restockPhase!=RestockPhase.None || actor.courierOrderId!=0 || actor.grappledById!=0 || car.driverAboard || (car.driverId!=0 && HealthyVehicleDriver(car)))return false;
            int source=actor.courierSourceBox-1;if(car.sourceBox!=0 && car.sourceBox!=actor.courierSourceBox)return false;if(car.deliveryPhase==VehicleDeliveryPhase.None && Math.Max(Math.Abs(car.x-source%Width),Math.Abs(car.y-source/Width))>64)return false;
            StopVehicleMotion(ref car);car.driverId=actor.id;car.deliveriesPaused=false;
            if(car.shipmentDropped)car.deliveryPhase=VehicleDeliveryPhase.RecoveringShipment;else if(car.cargoFood>0)car.deliveryPhase=VehicleDeliveryPhase.BoardingForSource;
            vehicles[vehicleIndex]=car;return true;
        }
        public bool TryUnassignVehicleCourier(int index)
        {
            if(index<0 || index>=vehicles.Count)return false;var car=vehicles[index];if(car.driverId==0 || car.driverAboard || car.deliveryPhase!=VehicleDeliveryPhase.None || car.cargoFood!=0 || car.handFood!=0)return false;car.driverId=0;car.deliveriesPaused=false;car.driveEffort=0;car.lastMealTick=0;vehicles[index]=car;return true;
        }
        public bool TryResumeVehicleCourier(int index){if(index<0 || index>=vehicles.Count || !HealthyVehicleDriver(vehicles[index]))return false;var car=vehicles[index];car.deliveriesPaused=false;vehicles[index]=car;return true;}
        private static void StopVehicleMotion(ref VehicleState car){car.hasDestination=false;car.destinationX=car.destinationY=0;car.exitAtTick=car.nextRoutePlanTick=0;car.nextMoveTick=car.junctionWaitTick=0;car.waitReason=VehicleWaitReason.None;}
        private bool ExitVehicle(int carIndex,bool emergency=false)
        {
            var car=vehicles[carIndex];if(!car.driverAboard)return true;int index=VehicleDriverIndex(car.driverId);if(index<0)return false;
            int chosen=-1;for(int pass=0;pass<2 && chosen<0;pass++)for(int d=0;d<4;d++){int x=car.x+carDX[d],y=car.y+carDY[d];if(!map.CanWalk(x,y) || VehicleAt(x,y) || BodyAt(x,y) || (pass==0 && map.Get(x,y)!=Terrain.Sidewalk))continue;chosen=Key(x,y);break;}
            if(chosen<0)return false;ActorState actor=actors[index];occupancy.Change(Key(actor.x,actor.y),actor.species,-1);actor.x=chosen%Width;actor.y=chosen/Width;ClearShelter(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;occupancy.Change(chosen,actor.species,1);car.driverAboard=false;vehicles[carIndex]=car;return true;
        }
        private bool BoardVehicle(int index,int carIndex)
        {
            var car=vehicles[carIndex];ActorState actor=actors[index];if(!HealthyVehicleDriver(car) || actor.grappledById!=0 || Math.Abs(actor.x-car.x)+Math.Abs(actor.y-car.y)!=1)return false;
            occupancy.Change(Key(actor.x,actor.y),actor.species,-1);actor.x=car.x;actor.y=car.y;ClearShelter(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;occupancy.Change(Key(actor.x,actor.y),actor.species,1);car.driverAboard=true;car.cargoFood+=car.handFood;car.handFood=0;vehicles[carIndex]=car;return true;
        }
        private void MoveVehicleDriver(VehicleState moved)
        {
            if(!moved.driverAboard)return;int index=VehicleDriverIndex(moved.driverId);if(index<0)return;var actor=actors[index];occupancy.Change(Key(actor.x,actor.y),actor.species,-1);actor.x=moved.x;actor.y=moved.y;occupancy.Change(Key(actor.x,actor.y),actor.species,1);
            int carIndex=AssignedVehicle(index);var car=vehicles[carIndex];car.driveEffort++;if(car.driveEffort==CourierVehicleRules.DriveEffortCells){car.driveEffort=0;if(actor.energySpent<EnergyRules.Maximum)actor.energySpent++;else actor.hunger=Math.Min(100,actor.hunger+1);}actors[index]=actor;vehicles[carIndex]=car;
        }
        private void EatCourierMeal(ref ActorState actor,ref VehicleState car)
        {
            if(actor.hunger<HungerRules.SeekAt || car.lastMealTick==Tick)return;if(actor.carriedFood>0)actor.carriedFood--;else if(car.driverAboard && car.cargoFood>0)car.cargoFood--;else if(car.handFood>0 && !car.shipmentDropped)car.handFood--;else return;actor.hunger=Math.Max(0,actor.hunger-HungerRules.MealRelief);car.lastMealTick=Tick;
        }
        private void PrepareVehicleDrivers()
        {
            if(!CourierVehiclesEnabled)return;
            for(int i=0;i<vehicles.Count;i++){var car=vehicles[i];if(car.driverId==0)continue;int index=VehicleDriverIndex(car.driverId);if(index<0)continue;
                if(!HealthyVehicleDriver(car)){
                    StopVehicleMotion(ref car);car.deliveriesPaused=true;car.orderId=0;if(car.handFood>0 && !car.shipmentDropped){car.shipmentDropped=true;car.shipmentX=actors[index].x;car.shipmentY=actors[index].y;car.deliveryPhase=VehicleDeliveryPhase.RecoveringShipment;}vehicles[i]=car;if(car.driverAboard)ExitVehicle(i,true);continue;
                }
                var actor=actors[index];if(car.driverAboard){EatCourierMeal(ref actor,ref car);actors[index]=actor;vehicles[i]=car;}
                if(car.orderId!=0 && !VehicleOrderExists(car.orderId)){car.orderId=0;car.deliveryPhase=car.driverAboard?VehicleDeliveryPhase.DrivingToSource:VehicleDeliveryPhase.BoardingForSource;StopVehicleMotion(ref car);vehicles[i]=car;}
                if(!car.driverAboard || car.deliveriesPaused)continue;
                if(car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery || car.deliveryPhase==VehicleDeliveryPhase.DrivingToSource){
                    if(!car.hasDestination && (car.nextRoutePlanTick==0 || car.nextRoutePlanTick<=Tick)){car.nextRoutePlanTick=Tick+30;vehicles[i]=car;int box=car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery?car.targetBox:car.sourceBox;int park=VehicleParking(car,box);if(park>=0)SetVehicleDestination(i,park%Width,park/Width,true);}
                }
            }
        }
        private void FinishVehicleDrivers()
        {
            if(!CourierVehiclesEnabled)return;for(int i=0;i<vehicles.Count;i++){var car=vehicles[i];if(!car.driverAboard || car.deliveriesPaused || !car.hasDestination || car.nextMoveTick!=0)continue;
                bool delivery=car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery;if(!delivery && car.deliveryPhase!=VehicleDeliveryPhase.DrivingToSource)continue;if(car.exitAtTick==0){car.exitAtTick=Tick+VehicleRules.MoveTicks;vehicles[i]=car;continue;}if(Tick<car.exitAtTick)continue;if(!ExitVehicle(i))continue;car=vehicles[i];StopVehicleMotion(ref car);car.handFood=car.cargoFood;car.cargoFood=0;car.deliveryPhase=delivery?VehicleDeliveryPhase.Unloading:VehicleDeliveryPhase.Collecting;if(!delivery)car.targetBox=car.sourceBox;vehicles[i]=car;
            }
        }
        private void CancelVehicleOrder(int orderId)
        {
            // Return retained cargo; never delete a shipment when its request is cancelled.
            for(int i=0;i<vehicles.Count;i++){var car=vehicles[i];if(car.orderId!=orderId)continue;car.orderId=0;StopVehicleMotion(ref car);if(car.shipmentDropped)car.deliveryPhase=VehicleDeliveryPhase.RecoveringShipment;else if(car.driverAboard)car.deliveryPhase=VehicleDeliveryPhase.DrivingToSource;else if(car.handFood+car.cargoFood>0)car.deliveryPhase=VehicleDeliveryPhase.BoardingForSource;else ClearVehicleDelivery(ref car);vehicles[i]=car;}
        }
        private bool VehicleOrderExists(int id){foreach(var order in supplyOrders.Values)if(order.id==id)return true;return false;}
        private SupplyOrderDocument VehicleOrder(int id){foreach(var order in supplyOrders.Values)if(order.id==id)return order;return null;}
        private int VehicleParking(VehicleState car,int boxKey)
        {
            if(boxKey<=0)return -1;int key=boxKey-1,bx=key%Width,by=key/Width;Room room=FindRoom(HouseAt(bx,by));if(room==null)return -1;
            var candidates=new List<int>();for(int y=Math.Max(0,by-CourierVehicleRules.ParkingRadius);y<=Math.Min(Height-1,by+CourierVehicleRules.ParkingRadius);y++)for(int x=Math.Max(0,bx-CourierVehicleRules.ParkingRadius);x<=Math.Min(Width-1,bx+CourierVehicleRules.ParkingRadius);x++){
                if(map.Get(x,y)!=Terrain.Road || !VehicleRoad(x,y) || JunctionCell(x,y) || VehicleAt(x,y,car.id) || BodyAt(x,y,car.driverAboard?car.driverId:0))continue;bool exit=false;for(int d=0;d<4;d++)if(map.Contains(x+carDX[d],y+carDY[d]) && map.Get(x+carDX[d],y+carDY[d])==Terrain.Sidewalk && map.CanWalk(x+carDX[d],y+carDY[d]))exit=true;if(exit)candidates.Add(Key(x,y));
            }
            candidates.Sort((a,b)=>{int da=ParkingDoorDistance(a,room),db=ParkingDoorDistance(b,room);int compare=da.CompareTo(db);return compare==0?a.CompareTo(b):compare;});
            for(int i=0;i<Math.Min(32,candidates.Count);i++){int cell=candidates[i];if(VehicleRoute(car,cell%Width,cell/Width)>=0)return cell;}return -1;
        }
        private int ParkingDoorDistance(int cell,Room room){int result=int.MaxValue;foreach(int door in room.doors)result=Math.Min(result,Math.Abs(cell%Width-door%Width)+Math.Abs(cell/Width-door/Width));return result;}
        private int CourierBoxGoal(ActorState actor,StorageBoxDocument box)
        {
            int result=-1,best=int.MaxValue;Room room=FindRoom(HouseAt(box.x,box.y));for(int d=0;d<4;d++){int x=box.x+carDX[d],y=box.y+carDY[d],length;if(!map.CanWalk(x,y) || room==null || !room.Contains(Key(x,y)))continue;if(ShelterRoute(actor,x,y,0,out length)>=0 && length<best){result=Key(x,y);best=length;}}return result;
        }
        private int WalkCourierVehicle(int index,ref ActorState actor,int x,int y)
        {
            int roomId=HouseAt(x,y);Room room=FindRoom(roomId);ClearShelterDeparture(ref actor);if(room==null)ClearShelter(ref actor);else{actor.shelterId=roomId;actor.shelterX=x;actor.shelterY=y;actor.shelterState=room.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;}
            int length,direction=ShelterRoute(actor,x,y,0,out length);if(direction>=0 && direction<8 && map.Get(actor.x+dx[direction],actor.y+dy[direction])==Terrain.DoorClosed){SetDoor(actor.x+dx[direction],actor.y+dy[direction],true);direction=8;}actors[index]=actor;return direction<0?8:direction;
        }
        private void ClearVehicleDelivery(ref VehicleState car){car.deliveryPhase=VehicleDeliveryPhase.None;car.sourceBox=car.targetBox=car.orderId=0;car.shipmentDropped=false;car.shipmentX=car.shipmentY=0;}
        private bool StartVehicleDelivery(int index,ref ActorState actor,ref VehicleState car)
        {
            if(!CourierWorkingNow(actor) || actor.workerCommuteId!=0 || actor.restockPhase!=RestockPhase.None || actor.grappledById!=0 || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.hunger>=HungerRules.SeekAt)return false;
            SupplyOrderDocument chosen=null;foreach(var order in supplyOrders.Values){if(SupplyOrderCourier(order.id)!=0 || order.supplyBox==actor.courierSourceBox)continue;int key=order.supplyBox-1,knownFood,knownSpace;var destination=map.StorageBoxAt(key%Width,key/Width);if(destination==null || (KnownStorageState(actor,destination,out knownFood,out knownSpace) && knownSpace==0))continue;if(chosen==null || CompareSupplyOrderPriority(order,chosen)<0)chosen=order;}
            if(chosen==null)return StartVehicleHomeDelivery(index,ref actor,ref car);int source=actor.courierSourceBox-1;StorageBoxDocument box=map.StorageBoxAt(source%Width,source/Width);int food,space;if(box==null || box.blockDonations || (KnownStorageState(actor,box,out food,out space) && food>=0 && food<=EffectiveFoodReserve(box)))return false;
            car.sourceBox=actor.courierSourceBox;car.targetBox=chosen.supplyBox;car.orderId=chosen.id;car.deliveryPhase=VehicleDeliveryPhase.Collecting;ClearShelterDeparture(ref actor);actor.homeShelterId=0;actors[index]=actor;return true;
        }
        // Assigned home routes use remembered stock; actual quantities are checked at pickup and delivery.
        private bool StartVehicleHomeDelivery(int index,ref ActorState actor,ref VehicleState car)
        {
            if(actor.courierBoxes==null)return false;
            for(int n=0;n<actor.courierBoxes.Length;n++){
                int cursor=(actor.courierNextBox+n)%actor.courierBoxes.Length,key=actor.courierBoxes[cursor]-1,food,space;
                var target=map.StorageBoxAt(key%Width,key/Width);
                if(target==null || IsRestrictedSupplyBox(target) || EffectiveFoodReserve(target)==0 || RestockClaimed(key+1,actor.id) || (KnownStorageState(actor,target,out food,out space) && (space==0 || food>=EffectiveFoodReserve(target))))continue;
                int source=actor.courierSourceBox-1;var box=map.StorageBoxAt(source%Width,source/Width);
                if(box==null || box.blockDonations || (KnownStorageState(actor,box,out food,out space) && food>=0 && food<=EffectiveFoodReserve(box)))return false;
                car.sourceBox=actor.courierSourceBox;car.targetBox=key+1;car.orderId=0;car.deliveryPhase=VehicleDeliveryPhase.Collecting;
                actor.courierNextBox=(cursor+1)%actor.courierBoxes.Length;actor.courierReturningHome=false;actor.homeShelterId=0;ClearShelterDeparture(ref actor);actors[index]=actor;return true;
            }
            return false;
        }
        private int VehicleHomeDemand(VehicleState car)
        {
            int key=car.targetBox-1;var box=map.StorageBoxAt(key%Width,key/Width);
            return box==null || IsRestrictedSupplyBox(box)?0:Math.Max(0,EffectiveFoodReserve(box)-StorageRules.Count(box,InventoryItem.Food));
        }
        private void CreditVehicleOrder(VehicleState car,int amount)
        {
            var order=VehicleOrder(car.orderId);if(order==null || order.supplyBox!=car.targetBox)return;order.deliveredMeals+=amount;if(order.deliveredMeals==order.requestedMeals){completedSupplyOrders.Add(order.id,CopyOrder(order));supplyOrders.Remove(order.storeId);if(completedSupplyOrders.Count>64){int first=0;foreach(int id in completedSupplyOrders.Keys){first=id;break;}completedSupplyOrders.Remove(first);}}
        }
        private void CloseCourierDepartureDoors(ActorState actor,VehicleState car)
        {
            int intended=car.deliveryPhase==VehicleDeliveryPhase.Collecting?car.sourceBox:car.deliveryPhase==VehicleDeliveryPhase.Unloading?car.targetBox:0;
            int intendedRoom=intended>0?HouseAt((intended-1)%Width,(intended-1)/Width):car.deliveryPhase==VehicleDeliveryPhase.RecoveringShipment?HouseAt(car.shipmentX,car.shipmentY):0;
            foreach(int box in new[]{car.sourceBox,car.targetBox}){if(box<=0)continue;Room room=FindRoom(HouseAt((box-1)%Width,(box-1)/Width));if(room==null || room.id==intendedRoom || room.Contains(Key(actor.x,actor.y)))continue;foreach(int door in room.doors){int x=door%Width,y=door/Width;if(Math.Abs(actor.x-x)+Math.Abs(actor.y-y)<=1 && map.Get(x,y)==Terrain.DoorOpen && !DoorOccupied(x,y) && !OrderCourierWaitingAtDoor(room,door))SetDoor(x,y,false);}}
        }
        private bool TryCourierVehicleRoutine(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;int carIndex=AssignedVehicle(index);if(carIndex<0)return false;var car=vehicles[carIndex];if(!HealthyVehicleDriver(car) || car.driverAboard)return false;
            if(threat>=0 || actor.freezeUntilTick>Tick || actor.grappledById!=0)return false;
            CloseCourierDepartureDoors(actor,car);int beforeFood=actor.carriedFood,beforeHand=car.handFood;EatCourierMeal(ref actor,ref car);if(actor.carriedFood!=beforeFood || car.handFood!=beforeHand){actors[index]=actor;vehicles[carIndex]=car;return true;}
            if(car.deliveriesPaused)return car.deliveryPhase!=VehicleDeliveryPhase.None;
            if(car.deliveryPhase==VehicleDeliveryPhase.None){if(!StartVehicleDelivery(index,ref actor,ref car))return false;vehicles[carIndex]=car;}
            if(car.deliveryPhase==VehicleDeliveryPhase.RecoveringShipment){if(Math.Abs(actor.x-car.shipmentX)+Math.Abs(actor.y-car.shipmentY)<=1){car.shipmentDropped=false;car.shipmentX=car.shipmentY=0;car.deliveryPhase=VehicleDeliveryPhase.BoardingForSource;vehicles[carIndex]=car;}else direction=WalkCourierVehicle(index,ref actor,car.shipmentX,car.shipmentY);return true;}
            if(car.deliveryPhase==VehicleDeliveryPhase.Collecting || car.deliveryPhase==VehicleDeliveryPhase.Unloading){
                bool loading=car.deliveryPhase==VehicleDeliveryPhase.Collecting;int key=(loading?car.sourceBox:car.targetBox)-1;StorageBoxDocument box=map.StorageBoxAt(key%Width,key/Width);if(box==null){car.deliveriesPaused=true;vehicles[carIndex]=car;return true;}
                if(Math.Abs(actor.x-box.x)+Math.Abs(actor.y-box.y)!=1){int goal=CourierBoxGoal(actor,box);if(goal>=0)direction=WalkCourierVehicle(index,ref actor,goal%Width,goal/Width);return true;}
                if(loading && car.handFood==0){if(car.orderId==0 && car.targetBox==car.sourceBox){ClearVehicleDelivery(ref car);vehicles[carIndex]=car;return true;}if(box.blockDonations){ClearVehicleDelivery(ref car);vehicles[carIndex]=car;return true;}var order=VehicleOrder(car.orderId);if(order==null && car.orderId!=0){ClearVehicleDelivery(ref car);vehicles[carIndex]=car;return true;}int available=Math.Max(0,StorageRules.Count(box,InventoryItem.Food)-EffectiveFoodReserve(box));int personal=Math.Min(HungerRules.CarryLimit-actor.carriedFood,available);actor.carriedFood+=personal;available-=personal;int amount=Math.Min(CourierVehicleRules.CargoMeals,Math.Min(available,order!=null?order.requestedMeals-order.deliveredMeals:EffectiveFoodReserve(map.StorageBoxAt((car.targetBox-1)%Width,(car.targetBox-1)/Width))));var update=StorageRules.Copy(box);StorageRules.ChangeFood(update,-personal-amount);map.SetStorageBox(update);RememberStorage(ref actor,StorageObservation(update));car.handFood=amount;if(amount>0)car.deliveryPhase=VehicleDeliveryPhase.BoardingForDelivery;else ClearVehicleDelivery(ref car);actors[index]=actor;vehicles[carIndex]=car;return true;}
                int unload=Math.Min(car.handFood,box.capacity-StorageRules.Used(box));if(!loading){var order=VehicleOrder(car.orderId);if(order!=null)unload=Math.Min(unload,order.requestedMeals-order.deliveredMeals);else unload=Math.Min(unload,VehicleHomeDemand(car));}
                RememberStorage(ref actor,StorageObservation(box));
                if(unload>0){var update=StorageRules.Copy(box);StorageRules.ChangeFood(update,unload);map.SetStorageBox(update);RememberStorage(ref actor,StorageObservation(update));if(!loading)CreditVehicleOrder(car,unload);car.handFood-=unload;}
                if(loading && car.handFood==0)ClearVehicleDelivery(ref car);else if(!loading){car.orderId=0;car.deliveryPhase=VehicleDeliveryPhase.BoardingForSource;}actors[index]=actor;vehicles[carIndex]=car;return true;
            }
            if(car.deliveryPhase==VehicleDeliveryPhase.BoardingForDelivery || car.deliveryPhase==VehicleDeliveryPhase.BoardingForSource){
                if(Math.Abs(actor.x-car.x)+Math.Abs(actor.y-car.y)==1){if(BoardVehicle(index,carIndex)){car=vehicles[carIndex];car.deliveryPhase=car.deliveryPhase==VehicleDeliveryPhase.BoardingForDelivery?VehicleDeliveryPhase.DrivingToDelivery:VehicleDeliveryPhase.DrivingToSource;vehicles[carIndex]=car;}return true;}
                int best=-1,lengthBest=int.MaxValue;for(int d=0;d<4;d++){int x=car.x+carDX[d],y=car.y+carDY[d],length;if(!map.CanWalk(x,y) || VehicleAt(x,y))continue;if(ShelterRoute(actor,x,y,0,out length)>=0 && length<lengthBest){best=Key(x,y);lengthBest=length;}}if(best>=0)direction=WalkCourierVehicle(index,ref actor,best%Width,best/Width);return true;
            }
            return true;
        }
        public string VehicleDeliveryStatus(int index){var car=VehicleAtIndex(index);if(car.driverId==0)return "No assigned courier";if(!HealthyVehicleDriver(car))return "Driver incapacitated; shipment retained";if(car.deliveriesPaused)return "Deliveries paused";if(car.driverAboard && !car.hasDestination)return "Waiting for reachable curb";return car.deliveryPhase==VehicleDeliveryPhase.None?"Waiting for supply order":car.deliveryPhase==VehicleDeliveryPhase.Collecting?"Collecting at source crate":car.deliveryPhase==VehicleDeliveryPhase.BoardingForDelivery?"Carrying shipment to vehicle":car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery?"Driving to delivery box":car.deliveryPhase==VehicleDeliveryPhase.Unloading?"Carrying shipment to destination box":car.deliveryPhase==VehicleDeliveryPhase.RecoveringShipment?"Recovering dropped shipment":car.deliveryPhase==VehicleDeliveryPhase.BoardingForSource?"Returning to vehicle":"Driving back to source";}
        private void ValidateVehicleDrivers()
        {
            var assigned=new HashSet<int>();var claims=new HashSet<int>();foreach(var car in vehicles){bool data=car.lastMealTick!=0 || car.nextRoutePlanTick!=0 || car.exitAtTick!=0 || car.driverId!=0 || car.driverAboard || car.cargoFood!=0 || car.handFood!=0 || car.sourceBox!=0 || car.targetBox!=0 || car.orderId!=0 || car.driveEffort!=0 || car.deliveriesPaused || car.shipmentDropped || car.shipmentX!=0 || car.shipmentY!=0 || car.deliveryPhase!=VehicleDeliveryPhase.None;if(!CourierVehiclesEnabled && data)throw new ArgumentException("Courier vehicles require current rules.");
                if(car.driverId==0){if(data)throw new ArgumentException("Unassigned vehicle retained delivery data.");continue;}int index=VehicleDriverIndex(car.driverId);if(index<0 || !assigned.Add(car.driverId) || car.cargoFood<0 || car.handFood<0 || car.cargoFood>CourierVehicleRules.CargoMeals || car.handFood>CourierVehicleRules.CargoMeals || car.cargoFood+car.handFood>CourierVehicleRules.CargoMeals || car.driveEffort<0 || car.driveEffort>=CourierVehicleRules.DriveEffortCells || car.deliveryPhase<VehicleDeliveryPhase.None || car.deliveryPhase>VehicleDeliveryPhase.RecoveringShipment)throw new ArgumentException("Invalid vehicle courier, cargo or phase.");
                if(HealthyVehicleDriver(car) && (actors[index].distributionCenterId==0 || actors[index].courierSourceBox==0 || actors[index].restockPhase!=RestockPhase.None || actors[index].courierOrderId!=0))throw new ArgumentException("Vehicle driver has an incompatible work assignment.");
                if(car.orderId!=0 && !claims.Add(car.orderId))throw new ArgumentException("Vehicle order claimed twice.");
                if(car.lastMealTick<0 || car.lastMealTick>Tick)throw new ArgumentException("Invalid vehicle meal clock.");
                if(car.nextRoutePlanTick<0 || car.nextRoutePlanTick>Tick+30 || (car.nextRoutePlanTick!=0 && (!car.driverAboard || car.hasDestination)))throw new ArgumentException("Invalid vehicle route retry clock.");
                if(car.exitAtTick<0 || car.exitAtTick>Tick+VehicleRules.MoveTicks || (car.exitAtTick!=0 && (!car.driverAboard || !car.hasDestination || car.nextMoveTick!=0)))throw new ArgumentException("Invalid vehicle exit clock.");
                if(!car.driverAboard && car.hasDestination)throw new ArgumentException("Unoccupied courier vehicle retained an active route.");
                if(car.orderId!=0)for(int a=0;a<count;a++)if(actors[a].courierOrderId==car.orderId)throw new ArgumentException("Supply order claimed by driver and pedestrian.");
                if(car.driverAboard && actors[index].grappledById!=0)throw new ArgumentException("Vehicle occupant is also grappled.");
                if(HealthyVehicleDriver(car) && !car.deliveriesPaused && !car.driverAboard && (car.deliveryPhase==VehicleDeliveryPhase.DrivingToSource || car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery))throw new ArgumentException("Driving vehicle lost its occupant.");
                if(car.driverAboard && (actors[index].x!=car.x || actors[index].y!=car.y || car.handFood!=0 || car.deliveryPhase==VehicleDeliveryPhase.None))throw new ArgumentException("Invalid vehicle occupant.");
                if(!HealthyVehicleDriver(car) && !car.deliveriesPaused)throw new ArgumentException("Incapacitated vehicle must be stopped.");
                if(car.deliveryPhase==VehicleDeliveryPhase.None){if(car.driverAboard || car.cargoFood!=0 || car.handFood!=0 || car.sourceBox!=0 || car.targetBox!=0 || car.orderId!=0)throw new ArgumentException("Idle vehicle retained a shipment.");}
                else {int source=car.sourceBox-1,target=car.targetBox-1;if(car.sourceBox<=0 || car.targetBox<=0 || !map.Contains(source%Width,source/Width) || !map.Contains(target%Width,target/Width) || map.StorageBoxAt(source%Width,source/Width)==null || map.StorageBoxAt(target%Width,target/Width)==null)throw new ArgumentException("Vehicle shipment lost a crate.");if(car.orderId!=0 && (VehicleOrder(car.orderId)==null || VehicleOrder(car.orderId).supplyBox!=car.targetBox))throw new ArgumentException("Vehicle shipment lost its order.");}
                if(car.shipmentDropped){if(car.handFood==0 || !map.Contains(car.shipmentX,car.shipmentY) || car.deliveryPhase!=VehicleDeliveryPhase.RecoveringShipment)throw new ArgumentException("Invalid dropped vehicle shipment.");}else if(car.shipmentX!=0 || car.shipmentY!=0)throw new ArgumentException("Inactive ground shipment retained position.");
            }
        }
    }
}
