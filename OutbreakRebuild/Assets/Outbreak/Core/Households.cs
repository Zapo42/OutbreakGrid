using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed class HouseholdResident
    {
        public int ActorIndex,Id,CurrentHouseId;
        public bool AtHome,Courier,Infected,Busy;
        public string CourierStatus;
    }
    public sealed class HouseholdOverview
    {
        public int HouseId,FocusX,FocusY,Capacity,Boxes,AtHome,Visitors;
        public HouseholdResident[] Residents;
        public int Away { get { return Residents.Length-AtHome; } }
    }
    public sealed partial class World
    {
        public HouseholdOverview[] Households()
        {
            if(!CourierAssignmentsEnabled)return new HouseholdOverview[0];
            HouseholdOverview[] result=new HouseholdOverview[rooms.Length];
            for(int r=0;r<rooms.Length;r++)
            {
                Room room=rooms[r];List<HouseholdResident> residents=new List<HouseholdResident>();
                HouseholdOverview row=new HouseholdOverview{HouseId=room.id,FocusX=room.cells[0]%Width,FocusY=room.cells[0]/Width,Capacity=room.Capacity};
                foreach(StorageBoxDocument box in map.StorageRecords)if(room.Contains(Key(box.x,box.y)))row.Boxes++;
                for(int i=0;i<count;i++)
                {
                    ActorState actor=actors[i];if(actor.species!=Species.Human || actor.health<=0)continue;
                    bool inside=room.Contains(Key(actor.x,actor.y));
                    if(actor.assignedHouseId!=room.id){if(inside)row.Visitors++;continue;}
                    if(inside)row.AtHome++;
                    residents.Add(new HouseholdResident{ActorIndex=i,Id=actor.id,CurrentHouseId=HouseAt(actor.x,actor.y),AtHome=inside,Courier=actor.courierBoxes!=null,Infected=actor.turnAtTick!=0,Busy=actor.restockPhase!=RestockPhase.None,CourierStatus=actor.courierBoxes==null?null:CourierScheduleStatus(i)});
                }
                row.Residents=residents.ToArray();result[r]=row;
            }
            return result; // Detached player information; no household totals enter human decisions.
        }
        public bool TryUnassignHouse(int index)
        {
            if(!CanEditCourier(index))return false;ActorState actor=actors[index];
            if(actor.assignedHouseId==0 || actor.courierSourceBox!=0 || actor.courierBoxes!=null)return false;
            if(actor.courierReturningHome)
            {
                actor.homeShelterId=0;
                if(actor.shelterId==actor.assignedHouseId && actor.shelterState==ShelterState.Seeking)ClearShelter(ref actor);
                else ClearShelterDeparture(ref actor);
            }
            actor.assignedHouseId=0;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
    }
}
