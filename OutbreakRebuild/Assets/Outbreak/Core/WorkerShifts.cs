using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum HumanOccupation { Consumer, Courier, Clerk }
    public sealed partial class World
    {
        private static HumanOccupation OccupationOf(ActorState actor)
        {
            return actor.clerkStoreId!=0?HumanOccupation.Clerk:actor.courierBoxes!=null || actor.distributionCenterId!=0?HumanOccupation.Courier:HumanOccupation.Consumer;
        }
        public HumanOccupation AssignedOccupation(int index){return OccupationOf(ActorAt(index));}
        public HumanOccupation EffectiveOccupation(int index){return EffectiveOccupationOf(ActorAt(index));}
        public HumanOccupation Occupation(int index){return EffectiveOccupation(index);}
        private HumanOccupation EffectiveOccupationOf(ActorState actor)
        {
            // Employment persists; only an off-clock worker free of work cargo joins consumer routines.
            HumanOccupation assigned=OccupationOf(actor);
            return OffDutyConsumersEnabled && assigned!=HumanOccupation.Consumer && !WorkerClockOn(actor) && !FinishingWorkerLoad(actor)?HumanOccupation.Consumer:assigned;
        }
        private bool IdleOffDutyWorkVehicle(int index,ActorState actor)
        {
            int vehicle=AssignedVehicle(index);if(!OffDutyConsumersEnabled || EffectiveOccupationOf(actor)!=HumanOccupation.Consumer || OccupationOf(actor)==HumanOccupation.Consumer || vehicle<0)return false;
            var car=vehicles[vehicle];return !car.consumerDriver && !car.driverAboard && car.deliveryPhase==VehicleDeliveryPhase.None && car.cargoFood==0 && car.handFood==0;
        }
        private bool ClerkWorkingNow(ActorState actor)
        {
            if(!WorkerShiftsEnabled || actor.clerkShiftHours==0)return true;
            return (CourierClockHour-actor.clerkShiftStartHour+24)%24<actor.clerkShiftHours;
        }
        private bool WorkerClockOn(ActorState actor){return actor.clerkStoreId!=0?ClerkWorkingNow(actor):CourierWorkingNow(actor);}
        private bool FinishingWorkerLoad(ActorState actor)
        {
            if(!IsShopping(actor) && actor.restockPhase!=RestockPhase.None || actor.clerkShelf!=0 && actor.carriedFood>FoodReserve(actor))return true;
            if(OffDutyConsumersEnabled)foreach(var car in vehicles)if(car.driverId==actor.id && !car.consumerDriver && (car.deliveryPhase!=VehicleDeliveryPhase.None || car.driverAboard || car.cargoFood>0 || car.handFood>0))return true;
            return false;
        }
        private bool WorkerMayShopOffDuty(ActorState actor)
        {
            return WorkerShiftsEnabled && OccupationOf(actor)!=HumanOccupation.Consumer && !WorkerClockOn(actor) && !FinishingWorkerLoad(actor);
        }
        public bool WorkerOnDuty(int index)
        {
            ActorState actor=ActorAt(index);return actor.health>0 && actor.species==Species.Human && actor.turnAtTick==0 && !IsShopping(actor) && OccupationOf(actor)!=HumanOccupation.Consumer && (WorkerClockOn(actor) || FinishingWorkerLoad(actor));
        }
        public string WorkerStatus(int index)
        {
            ActorState actor=ActorAt(index);if(actor.species!=Species.Human || actor.health<=0)return "No active occupation";
            if(OccupationOf(actor)==HumanOccupation.Consumer)return "Consumer";
            if(actor.turnAtTick!=0)return "Infected: work paused";
            if(!WorkerClockOn(actor) && FinishingWorkerLoad(actor))return actor.clerkStoreId!=0?"Finishing current shelf load":"Finishing current delivery";
            if(IsShopping(actor))return "Finishing personal shopping";
            if(actor.workerCommuteId!=0)return actor.workerCommuteId==actor.assignedHouseId?"Off duty: returning home":"On duty: commuting to work";
            if(OffDutyConsumersEnabled && !WorkerClockOn(actor) && actor.leisurePhase!=HumanLeisurePhase.None)return "Off duty: "+LeisureStatus(index);
            if(!WorkerClockOn(actor))return HouseAt(actor.x,actor.y)==actor.assignedHouseId?"Off duty: home/rest":"Off duty: return home when safe";
            return "On duty";
        }
        public bool TrySetClerkSchedule(int index,int startHour,int workHours)
        {
            if(!WorkerShiftsEnabled || index<0 || index>=count || startHour<0 || startHour>23 || workHours<0 || workHours>23 || (workHours==0 && startHour!=0))return false;
            ActorState actor=actors[index];if(actor.health<=0 || actor.species!=Species.Human || actor.turnAtTick!=0 || actor.clerkStoreId==0 || (workHours>0 && actor.assignedHouseId==0))return false;
            actor.clerkShiftStartHour=startHour;actor.clerkShiftHours=workHours;actors[index]=actor;return true;
        }
        private int Workplace(ActorState actor)
        {
            if(actor.clerkStoreId!=0)return actor.clerkStoreId;
            int key=actor.courierSourceBox-1;return key<0?0:HouseAt(key%Width,key/Width);
        }
        private bool WorkerDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return WorkerShiftsEnabled && actor.workerCommuteId!=0 && actor.workerCommuteId!=room.id && (SafeNeedsHomeReturn(actor,threat) || SafeRestock(actor,threat) && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy);
        }
        private bool TryWorkerRoutine(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(!WorkerShiftsEnabled || OccupationOf(actor)==HumanOccupation.Consumer)return false;
            if(actor.turnAtTick!=0){actor.workerCommuteId=0;actors[index]=actor;return false;}
            if(actor.restockPhase!=RestockPhase.None || IsShopping(actor))return false;
            if(OccupationOf(actor)==HumanOccupation.Courier && actor.courierShiftHours==0 && actor.assignedHouseId==Workplace(actor))return false;
            bool working=WorkerClockOn(actor);Room actual=FindRoom(HouseAt(actor.x,actor.y));
            if(!working && actor.clerkStoreId!=0)
            {
                ResetClerkCall(ref actor);
                if(actor.clerkShelf!=0 && actor.carriedFood>FoodReserve(actor)){actors[index]=actor;return false;}
                actor.clerkShelf=0;
                if(actual!=null && actual.id==actor.clerkStoreId && actor.carriedFood>FoodReserve(actor) && SafeRestock(actor,threat))
                {
                    StoreDocument store;if(stores.TryGetValue(actor.clerkStoreId,out store) && TryUseStorage(index,ref actor,threat,actual,true,out direction,store.supplyBox))return true;
                }
                actors[index]=actor;
            }
            if(actor.assignedHouseId==0)return false;
            if(OffDutyConsumersEnabled && !working && EffectiveOccupationOf(actor)==HumanOccupation.Consumer && ((actual!=null && actual.id==actor.assignedHouseId) || actor.leisurePhase!=HumanLeisurePhase.None))
            {
                // Once home, or on an optional walk, consumer routines take over; preserve the established physical commute from work.
                if(actor.workerCommuteId!=0){actor.workerCommuteId=0;actor.courierReturningHome=false;actor.homeShelterId=0;ClearShelterDeparture(ref actor);actors[index]=actor;}
                return false;
            }
            int desired=working?Workplace(actor):actor.assignedHouseId;Room destination=FindRoom(desired);if(destination==null)return false;
            if(actual==destination)
            {
                if(actor.workerCommuteId!=0){actor.workerCommuteId=0;actor.courierReturningHome=false;actor.homeShelterId=0;ClearShelterDeparture(ref actor);actor.shelterId=destination.id;actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;actors[index]=actor;}
                if(working && actor.clerkStoreId==0 && actor.carriedFood>FoodReserve(actor) && SafeRestock(actor,threat))return TryUseStorage(index,ref actor,threat,destination,true,out direction,actor.courierSourceBox);
                return false;
            }
            bool needsHomeReturn=SafeNeedsHomeReturn(actor,threat);
            if(!needsHomeReturn && (!SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy))return false;
            if(needsHomeReturn && actor.foodExploreRoomId!=0)CancelFoodExploration(index,ref actor); // Release the old foraging goal atomically before taking ownership of the home route.
            if(actor.workerCommuteId!=desired){actor.workerCommuteId=desired;actor.courierReturningHome=false;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);}
            if(needsHomeReturn && TryHomeReturnRest(index,ref actor))return true;
            Room current=FindRoom(actor.shelterId);
            if(actual!=null && actual!=destination && (current==null || current.id!=actual.id)){actor.shelterId=actual.id;actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;current=actual;}
            if(current!=null && current!=destination && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None)){actors[index]=actor;return TryShelter(index,ref actor,threat,out direction);}
            if(KnownRoomDanger(actor,destination) || RoomAdmissions(destination,actor.id)>=destination.Capacity){actors[index]=actor;return true;}
            int best=int.MaxValue,goal=-1;
            foreach(int cell in destination.cells)
            {
                int length;if(!map.CanWalk(cell%Width,cell/Width))continue;
                if(ShelterRoute(actor,cell%Width,cell/Width,0,out length)>=0 && length<best){best=length;goal=cell;}
            }
            if(goal<0){actors[index]=actor;return true;}
            actor.shelterId=destination.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;actors[index]=actor;return TryShelter(index,ref actor,threat,out direction);
        }
        private void ValidateWorkerShifts()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool scheduled=actor.clerkShiftStartHour!=0 || actor.clerkShiftHours!=0;
                if(scheduled && (!WorkerShiftsEnabled || actor.species!=Species.Human || actor.health<=0 || actor.clerkStoreId==0 || actor.assignedHouseId==0 || actor.clerkShiftStartHour<0 || actor.clerkShiftStartHour>23 || actor.clerkShiftHours<1 || actor.clerkShiftHours>23))throw new ArgumentException("Invalid clerk shift.");
                if(IsShopping(actor) && actor.clerkStoreId!=0 && (actor.clerkShelf!=0 || actor.clerkPhone!=0))throw new ArgumentException("Shopping clerk retained a work task.");
                if(actor.workerCommuteId!=0 && (!WorkerShiftsEnabled || actor.species!=Species.Human || actor.health<=0 || OccupationOf(actor)==HumanOccupation.Consumer || actor.assignedHouseId==0 || actor.restockPhase!=RestockPhase.None || (actor.workerCommuteId!=actor.assignedHouseId && actor.workerCommuteId!=Workplace(actor))))throw new ArgumentException("Invalid worker commute.");
            }
        }
    }
}
