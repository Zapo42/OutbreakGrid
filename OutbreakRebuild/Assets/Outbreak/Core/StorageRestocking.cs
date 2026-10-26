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
            actor.shoppingResting=false;actor.shoppingStoreId=0;actor.shoppingSearchCell=0;actor.shoppingShelf=0;actor.shoppingMeals=0;actor.courierOrderId=0;actor.restockSourceBox=0;actor.restockBox=0;actor.restockHomeId=0;actor.restockPhase=RestockPhase.None;actor.restockUntilTick=0;
        }
        private bool SafeRestock(ActorState actor,int threat)
        {
            return (actor.health>=ShelterRecoveryRules.ExtraReserveBelowHealth || CanSeekFoodForEmptyHome(actor)) && actor.turnAtTick==0 && (actor.hunger<HungerRules.SeekAt || CanSeekFoodForEmptyHome(actor)) && actor.stress==0 && threat<0 && actor.freezeUntilTick<=Tick && !HasRecentWarning(actor) && !HasRecentDoorNoise(actor);
        }
        private bool RestockDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return WorkerDepartureAllowed(actor,room,threat) || ShoppingDepartureAllowed(actor,room,threat) || AssignedCourierDepartureAllowed(actor,room,threat) || DeliverySourceDepartureAllowed(actor,room,threat) || RestockingEnabled && actor.restockPhase==RestockPhase.Collecting && actor.restockHomeId==room.id && SafeRestock(actor,threat) && (EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy || CanSeekFoodForEmptyHome(actor));
        }
        private ShelfDocument ChooseRestockShelf(ActorState actor,Room home)
        {
            if(home==null || actor.shelfMemory==null)return null;ShelfMemoryRecord best=new ShelfMemoryRecord();int rank=-1,steps=int.MaxValue;
            foreach(ShelfMemoryRecord note in actor.shelfMemory)
            {
                if(!RoutineFoodSiteAllowed(actor,note.x,note.y) || note.food==0 || home.Contains(Key(note.x,note.y)) || AvoidBlockedFood(actor,note.x,note.y))continue;
                int length=FoodShelfRouteLength(actor,note),priority=note.food>0?1:0;
                if(length==int.MaxValue)continue;
                if(priority>rank || (priority==rank && length<steps)){best=note;rank=priority;steps=length;}
            }
            return rank<0?null:new ShelfDocument{x=best.x,y=best.y}; // Read only remembered supplies, never hidden current shelf stock.
        }
        private bool RestockClaimed(int box,int exceptId)
        {
            foreach(var car in vehicles)if(car.driverId!=exceptId && !car.consumerDriver && car.targetBox==box && car.orderId==0 && HealthyVehicleDriver(car) && (car.deliveryPhase==VehicleDeliveryPhase.Collecting || car.deliveryPhase==VehicleDeliveryPhase.BoardingForDelivery || car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery || car.deliveryPhase==VehicleDeliveryPhase.Unloading))return true;
            foreach(var car in vehicles)if(car.consumerDriver && car.driverId!=exceptId && car.sourceBox==box && car.consumerPhase!=ConsumerVehiclePhase.None && HealthyVehicleDriver(car))return true;
            for(int i=0;i<count;i++)if(actors[i].id!=exceptId && actors[i].health>0 && actors[i].species==Species.Human && actors[i].restockBox==box && actors[i].restockUntilTick>Tick)return true;return false;
        }
        private bool BeginRestockReturn(int index,ref ActorState actor,Room home,int threat)
        {
            if(ShoppingContinuityEnabled && IsShopping(actor) && actor.restockPhase==RestockPhase.Collecting && actor.shoppingEndReason==ShoppingEndReason.None)actor.shoppingEndReason=actor.hunger>=HungerRules.SeekAt?ShoppingEndReason.Hunger:!SafeRestock(actor,threat)?ShoppingEndReason.Danger:ShoppingEndReason.NoRoute;
            actor.shoppingResting=false;actor.restockPhase=RestockPhase.Returning;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);
            int goal,length;if(home==null || !FoodReturnGoal(actor,home,KnownShelterThreats(actor,threat),out goal,out length)){ClearRestocking(ref actor);actor.homeShelterId=0;actors[index]=actor;return false;}
            actor.homeShelterId=home.id;actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=home.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;actors[index]=actor;return false;
        }
        private bool TryRestock(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(CourierVehiclesEnabled && AssignedVehicle(index)>=0)return false;if(WorkerShiftsEnabled && actor.workerCommuteId!=0)return false;
            if(StoresEnabled && actor.clerkStoreId!=0 && !(WorkerShiftsEnabled && (WorkerMayShopOffDuty(actor) || IsShopping(actor))))return false;
            if(StoreShoppingEnabled && IsShopping(actor) && actor.turnAtTick==0 && actor.hunger>=HungerRules.SeekAt && actor.carriedFood>0 && threat<0 && actor.freezeUntilTick<=Tick && !HasRecentWarning(actor) && !HasRecentDoorNoise(actor))
            {
                actor.carriedFood--;actor.hunger=Math.Max(0,actor.hunger-HungerRules.MealRelief);ClearFoodMemory(ref actor);actors[index]=actor;return true;
            }
            if(actor.courierOrderId!=0 && ActiveSupplyOrder(actor)==null){ClearRestocking(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;}
            bool hasHouse=CourierAssignmentsEnabled && actor.assignedHouseId!=0;
            bool assigned=hasHouse && (actor.courierBoxes!=null || (DistributionCentersEnabled && actor.distributionCenterId!=0)) && !(WorkerShiftsEnabled && (IsShopping(actor) || (!CourierWorkingNow(actor) && actor.restockPhase==RestockPhase.None)));
            if(hasHouse && actor.restockPhase==RestockPhase.None)
            {
                if(actor.courierReturningHome && HouseAt(actor.x,actor.y)==actor.assignedHouseId){actor.courierReturningHome=false;actors[index]=actor;}
                if(assigned){if(!StartSupplyOrderTrip(index,ref actor,threat) && !StartAssignedCourierJob(index,ref actor,threat)){if(WorkerShiftsEnabled && CourierWorkingNow(actor) && HouseAt(actor.x,actor.y)==Workplace(actor))return false;return TryReturnToAssignedHouse(index,ref actor,threat,out direction);}}
                // A safe leisure route owns its return; actual shopping and courier tasks still run in their established order.
                else if(HouseAt(actor.x,actor.y)!=actor.assignedHouseId && !(LeisureEnabled && actor.leisurePhase!=HumanLeisurePhase.None))return TryReturnToAssignedHouse(index,ref actor,threat,out direction);
            }
            if(actor.restockPhase==RestockPhase.None && IsStoreShopper(actor) && !StartStoreShopping(index,ref actor,threat))return false;
            if(WorkerMayShopOffDuty(actor) && actor.restockPhase==RestockPhase.None)return false;
            if(actor.restockPhase==RestockPhase.None)
            {
                Room current=FindRoom(actor.shelterId);
                if(!map.HasStorageBoxes || current==null || actor.shelterState!=ShelterState.Inside || !current.Contains(Key(actor.x,actor.y)) || !SafeRestock(actor,threat) || actor.carriedFood>FoodReserve(actor) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.homeShelterId!=0 || actor.foodExploreRoomId!=0)return false;
                StorageBoxDocument donor=SupplyDeliveriesEnabled?ChooseDeliverySource(actor,current):null;if(donor==null && ChooseRestockShelf(actor,current)==null)return false;
                StorageBoxDocument chosen=null;
                foreach(StorageBoxDocument box in map.StorageRecords){int food,space;if(!IsRestrictedSupplyBox(box) && current.Contains(Key(box.x,box.y)) && KnownStorageState(actor,box,out food,out space) && food>=0 && food<EffectiveFoodReserve(box) && !RestockClaimed(Key(box.x,box.y)+1,actor.id)){chosen=box;break;}}
                if(chosen==null)return false;
                actor.restockSourceBox=donor==null?0:Key(donor.x,donor.y)+1;actor.restockBox=Key(chosen.x,chosen.y)+1;actor.restockHomeId=current.id;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+RestockRules.TaskTicks;actors[index]=actor;
            }
            Room home=FindRoom(actor.restockHomeId);StorageBoxDocument target=map.StorageBoxAt((actor.restockBox-1)%Width,(actor.restockBox-1)/Width);
            if(home==null || target==null){ClearRestocking(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
            if(actor.turnAtTick!=0 || (IsShopping(actor) && IsRestrictedSupplyBox(target))){ClearRestocking(ref actor);actors[index]=actor;return false;}
            if(actor.courierOrderId==0 && ReserveTargetsEnabled && actor.restockPhase==RestockPhase.Collecting && (EffectiveFoodReserve(target)==0 || (StorageVisible(actor,target) && StorageRules.Count(target,InventoryItem.Food)>=EffectiveFoodReserve(target))))
            {
                if(ShoppingContinuityEnabled && IsShopping(actor))actor.shoppingEndReason=ShoppingEndReason.TargetSatisfied;
                if(home.Contains(Key(actor.x,actor.y))){ClearRestocking(ref actor);ClearShelterDeparture(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
                return BeginRestockReturn(index,ref actor,home,threat);
            }
            if(ShoppingContinuityEnabled && IsShopping(actor) && SafeRestock(actor,threat) && TryShoppingRest(index,ref actor))return true;
            if(actor.restockPhase==RestockPhase.Returning)
            {
                if(SupplyDeliveriesEnabled && actor.restockSourceBox!=0 && SafeRestock(actor,threat))
                {
                    Room sourceRoom=DeliverySourceRoom(actor);if(sourceRoom!=null && sourceRoom.id!=home.id && (sourceRoom.Contains(Key(actor.x,actor.y)) || (actor.shelterId==sourceRoom.id && actor.shelterExitPhase!=ShelterExitPhase.None)))return TryShelter(index,ref actor,threat,out direction);
                }
                if(!home.Contains(Key(actor.x,actor.y)))
                {
                    if((assigned || IsShopping(actor)) && SafeRestock(actor,threat))
                    {
                        Room current=FindRoom(actor.shelterId);
                        if(current!=null && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None))return TryShelter(index,ref actor,threat,out direction);
                        BeginRestockReturn(index,ref actor,home,threat);return TryShelter(index,ref actor,threat,out direction);
                    }
                    return BeginRestockReturn(index,ref actor,home,threat);
                }
                actor.homeShelterId=0;
                if(actor.hunger>=HungerRules.SeekAt){actors[index]=actor;return false;}
                if(actor.carriedFood<=FoodReserve(actor) || (BoxMemoryEnabled?KnownStorageFull(actor,target):StorageRules.Used(target)>=target.capacity)){ClearRestocking(ref actor);actors[index]=actor;return false;}
                if(TryUseStorage(index,ref actor,threat,home,true,out direction,actor.restockBox))
                {
                    if(actor.carriedFood<=FoodReserve(actor) || (actor.courierOrderId!=0 && ActiveSupplyOrder(actor)==null)){ClearRestocking(ref actor);actors[index]=actor;}return true;
                }
                return false;
            }
            if(assigned && SafeRestock(actor,threat) && actor.carriedFood<HungerRules.CarryLimit)
            {
                Room current=FindRoom(actor.shelterId),source=DeliverySourceRoom(actor);
                if(current!=null && current!=source && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None))return TryShelter(index,ref actor,threat,out direction);
                if(source!=null && source.Contains(Key(actor.x,actor.y)) && EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy)return TryShelter(index,ref actor,threat,out direction);
                if(TryCollectDeliveryFood(index,ref actor,home,threat,out direction))return true;
                ClearRestocking(ref actor);actors[index]=actor;return TryReturnToAssignedHouse(index,ref actor,threat,out direction);
            }
            bool inside=home.Contains(Key(actor.x,actor.y));
            if(!SafeRestock(actor,threat) || (EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy && !(ShoppingContinuityEnabled && IsShopping(actor) && actor.shoppingStoreId!=0)) || actor.carriedFood>=HungerRules.CarryLimit)
            {
                if(inside){ClearRestocking(ref actor);ClearShelterDeparture(ref actor);ClearFoodMemory(ref actor);actors[index]=actor;return false;}
                return BeginRestockReturn(index,ref actor,home,threat);
            }
            if(inside || actor.shelterExitPhase!=ShelterExitPhase.None)return false; // Use the existing staged door inspection until physically outdoors.
            if(SupplyDeliveriesEnabled && actor.restockSourceBox!=0)
            {
                if(TryCollectDeliveryFood(index,ref actor,home,threat,out direction))return true;
                return BeginRestockReturn(index,ref actor,home,threat);
            }
            if(IsShopping(actor) && StoreDiscoveryEnabled && actor.shoppingShelf==0)return TryDiscoverShoppingStore(index,ref actor,home,out direction);
            if(IsShopping(actor))return TryCollectShoppingShelf(index,ref actor,home,out direction);
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
                if(!active){if(actor.restockSourceBox!=0 || actor.restockBox!=0 || actor.restockHomeId!=0 || actor.restockUntilTick!=0)throw new ArgumentException("Inactive restock task contains state.");continue;}
                Room home=FindRoom(actor.restockHomeId);int key=actor.restockBox-1;
                if(!RestockingEnabled || actor.species!=Species.Human || actor.health<=0 || actor.restockPhase<RestockPhase.Collecting || actor.restockPhase>RestockPhase.Returning || actor.restockBox<=0 || home==null || !home.Contains(key) || map.StorageBoxAt(key%Width,key/Width)==null || actor.restockUntilTick<=Tick || actor.restockUntilTick-Tick>(CourierAssignmentsEnabled && (actor.courierBoxes!=null || actor.courierOrderId!=0 || IsShopping(actor))?CourierRules.TaskTicks:RestockRules.TaskTicks))throw new ArgumentException("Invalid restock task.");
                if(actor.restockSourceBox!=0)
                {
                    Room sourceRoom=DeliverySourceRoom(actor);int sourceKey=actor.restockSourceBox-1;
                    if(!SupplyDeliveriesEnabled || actor.restockSourceBox<1 || actor.restockSourceBox==actor.restockBox || sourceRoom==null || (sourceRoom.id==home.id && !(CourierAssignmentsEnabled && (actor.courierBoxes!=null || actor.courierOrderId!=0))) || map.StorageBoxAt(sourceKey%Width,sourceKey/Width)==null)throw new ArgumentException("Invalid supply delivery source.");
                }
                for(int j=0;j<i;j++)if(actors[j].restockBox==actor.restockBox)throw new ArgumentException("Duplicate storage restock claim.");
            }
        }
    }
}
