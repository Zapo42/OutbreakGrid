using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class CourierRules
    {
        public const int MaxDestinations=8,RouteRadius=64,TaskTicks=3600;
    }
    public sealed partial class World
    {
        public int HouseAt(int x,int y)
        {
            if(!map.Contains(x,y))return 0;foreach(Room room in rooms)if(room.Contains(Key(x,y)))return room.id;return 0;
        }
        private bool CanEditCourier(int index)
        {
            return CourierAssignmentsEnabled && index>=0 && index<count && actors[index].species==Species.Human && actors[index].health>0 && actors[index].turnAtTick==0 && actors[index].restockPhase==RestockPhase.None;
        }
        public bool TryAssignHouse(int index,int x,int y)
        {
            int house=HouseAt(x,y);if(!CanEditCourier(index) || house==0)return false;
            ActorState actor=actors[index];actor.assignedHouseId=house;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
        public bool TrySetCourierSource(int index,int x,int y)
        {
            if(!CanEditCourier(index) || actors[index].clerkStoreId!=0 || actors[index].assignedHouseId==0 || map.StorageBoxAt(x,y)==null || HouseAt(x,y)==0)return false;
            ActorState actor=actors[index];int key=Key(x,y)+1;if(!DistributionSourceAllowed(actor,key))return false;if(actor.courierBoxes!=null && Array.IndexOf(actor.courierBoxes,key)>=0)return false;
            actor.courierSourceBox=key;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
        public bool TryToggleCourierDestination(int index,int x,int y)
        {
            if(!CanEditCourier(index) || actors[index].assignedHouseId==0 || actors[index].courierSourceBox==0 || map.StorageBoxAt(x,y)==null || HouseAt(x,y)==0)return false;
            ActorState actor=actors[index];int key=Key(x,y)+1;if(key==actor.courierSourceBox)return false;
            int[] old=actor.courierBoxes??new int[0];int existing=Array.IndexOf(old,key);
            if(existing<0 && old.Length>=CourierRules.MaxDestinations)return false;
            int[] next=new int[old.Length+(existing<0?1:-1)];int n=0;for(int i=0;i<old.Length;i++)if(i!=existing)next[n++]=old[i];if(existing<0)next[n]=key;
            actor.courierBoxes=next.Length==0?null:next;if(actor.courierBoxes==null)ClearCourierSchedule(ref actor);actor.courierNextBox=0;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
        public bool TryClearCourierRoute(int index)
        {
            if(!CanEditCourier(index))return false;ActorState actor=actors[index];actor.distributionCenterId=0;ClearCourierSchedule(ref actor);actor.courierSourceBox=0;actor.courierBoxes=null;actor.courierNextBox=0;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
        private static void ClearCourierAssignment(ref ActorState actor)
        {
            actor.assignedHouseId=0;actor.distributionCenterId=0;ClearCourierSchedule(ref actor);actor.courierSourceBox=0;actor.courierBoxes=null;actor.courierNextBox=0;actor.courierReturningHome=false;
        }
        private bool AssignedCourierDepartureAllowed(ActorState actor,Room room,int threat)
        {
            if(!CourierAssignmentsEnabled || actor.assignedHouseId==0 || !SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy)return false;
            if(actor.courierReturningHome)return room.id!=actor.assignedHouseId;
            if(actor.courierBoxes==null && actor.courierOrderId==0)return false;
            if(actor.restockPhase==RestockPhase.Collecting){Room source=DeliverySourceRoom(actor);return source!=null && room.id!=source.id;}
            return actor.restockPhase==RestockPhase.Returning && room.id!=actor.restockHomeId;
        }
        private bool StartAssignedCourierJob(int index,ref ActorState actor,int threat)
        {
            if(!CourierWorkingNow(actor) || actor.courierBoxes==null || !SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.carriedFood>FoodReserve(actor) || actor.foodExploreRoomId!=0)return false;
            Room current=FindRoom(actor.shelterId);if(current==null || !current.Contains(Key(actor.x,actor.y)))return false;
            int sourceKey=actor.courierSourceBox-1;StorageBoxDocument source=map.StorageBoxAt(sourceKey%Width,sourceKey/Width);int food,space;
            if(source==null || source.blockDonations || (KnownStorageState(actor,source,out food,out space) && food>=0 && food<=EffectiveFoodReserve(source)))return false;
            for(int n=0;n<actor.courierBoxes.Length;n++)
            {
                int cursor=(actor.courierNextBox+n)%actor.courierBoxes.Length,key=actor.courierBoxes[cursor]-1;
                StorageBoxDocument target=map.StorageBoxAt(key%Width,key/Width);Room destination=FindRoom(HouseAt(key%Width,key/Width));int goal,length;
                if(target==null || EffectiveFoodReserve(target)==0 || RestockClaimed(key+1,actor.id) || (KnownStorageState(actor,target,out food,out space) && (space==0 || food>=EffectiveFoodReserve(target))) || !FoodReturnGoal(actor,destination,0,out goal,out length))continue;
                actor.courierNextBox=(cursor+1)%actor.courierBoxes.Length;actor.courierReturningHome=false;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);
                actor.restockSourceBox=actor.courierSourceBox;actor.restockBox=key+1;actor.restockHomeId=destination.id;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+CourierRules.TaskTicks;actors[index]=actor;return true;
            }
            return false;
        }
        private bool TryReturnToAssignedHouse(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(!SafeRestock(actor,threat))return false;Room home=FindRoom(actor.assignedHouseId);if(home==null)return false;
            if(home.Contains(Key(actor.x,actor.y))){actor.courierReturningHome=false;actors[index]=actor;return false;}
            if(!actor.courierReturningHome){ClearShelterDeparture(ref actor);actor.courierReturningHome=true;}
            Room current=FindRoom(actor.shelterId);
            if(current!=null && current.id!=home.id && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None)){actors[index]=actor;return TryShelter(index,ref actor,threat,out direction);}
            int goal,length;if(!FoodReturnGoal(actor,home,0,out goal,out length)){actor.courierReturningHome=false;actors[index]=actor;return false;}
            actor.homeShelterId=home.id;actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;actors[index]=actor;return TryShelter(index,ref actor,threat,out direction);
        }
        private void ValidateCourierAssignments()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool present=actor.assignedHouseId!=0 || actor.courierSourceBox!=0 || actor.courierBoxes!=null || actor.courierNextBox!=0 || actor.courierReturningHome;
                if(!present)continue;
                if(!CourierAssignmentsEnabled || actor.species!=Species.Human || actor.health<=0 || FindRoom(actor.assignedHouseId)==null || (actor.courierReturningHome && actor.restockPhase!=RestockPhase.None))throw new ArgumentException("Invalid assigned house.");
                int source=actor.courierSourceBox-1;if(actor.courierSourceBox!=0 && (actor.courierSourceBox<1 || map.StorageBoxAt(source%Width,source/Width)==null || HouseAt(source%Width,source/Width)==0))throw new ArgumentException("Invalid assigned source box.");
                if(actor.courierBoxes==null){if(actor.courierNextBox!=0)throw new ArgumentException("Inactive courier has a route cursor.");continue;}
                if(actor.courierSourceBox==0 || actor.courierBoxes.Length<1 || actor.courierBoxes.Length>CourierRules.MaxDestinations || actor.courierNextBox<0 || actor.courierNextBox>=actor.courierBoxes.Length)throw new ArgumentException("Invalid courier route.");
                for(int j=0;j<actor.courierBoxes.Length;j++)
                {
                    int key=actor.courierBoxes[j]-1;if(key<0 || key==source || map.StorageBoxAt(key%Width,key/Width)==null || HouseAt(key%Width,key/Width)==0)throw new ArgumentException("Invalid courier destination.");
                    for(int k=0;k<j;k++)if(actor.courierBoxes[k]==key+1)throw new ArgumentException("Duplicate courier destination.");
                }
                if(actor.courierOrderId==0 && actor.restockPhase!=RestockPhase.None && (actor.restockSourceBox!=actor.courierSourceBox || Array.IndexOf(actor.courierBoxes,actor.restockBox)<0))throw new ArgumentException("Courier task differs from assignment.");
            }
        }
    }
}
