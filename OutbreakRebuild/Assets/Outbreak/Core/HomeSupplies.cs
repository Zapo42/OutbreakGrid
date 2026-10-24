using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class HomeSupplyRules { public const int RecheckTicks=300; }
    public struct HomeSupplyKnowledge
    {
        public int HouseId,Boxes,KnownBoxes,StaleBoxes,ReportedBoxes,KnownMeals;
        public int UnknownBoxes { get { return Boxes-KnownBoxes-StaleBoxes-ReportedBoxes; } }
    }
    public sealed partial class World
    {
        private bool HasCenterBusiness(ActorState actor,int id)
        {
            if((actor.distributionCenterId==id && (!WorkerShiftsEnabled || CourierWorkingNow(actor) || (actor.restockPhase!=RestockPhase.None && !IsShopping(actor)))) || actor.assignedHouseId==id)return true;
            Room room=FindRoom(id);return room!=null && actor.restockPhase!=RestockPhase.None && (actor.restockHomeId==id || (actor.restockSourceBox>0 && room.Contains(actor.restockSourceBox-1)));
        }
        private bool RoutineFoodSiteAllowed(ActorState actor,int x,int y)
        {
            if(!HomeSuppliesEnabled || distributionCenters.Count==0)return true;int id=HouseAt(x,y);
            return !distributionCenters.ContainsKey(id) || HasCenterBusiness(actor,id);
        }
        private bool EmergencyShelter(ActorState actor)
        {
            return actor.turnAtTick!=0 || actor.stress>0 || actor.reaction==HumanReaction.Flight || actor.rejectedShelterId!=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor);
        }
        private bool RoutineDistributionStepAllowed(ActorState actor,int x,int y)
        {
            if(!HomeSuppliesEnabled || actor.species!=Species.Human || distributionCenters.Count==0)return true;
            int id=HouseAt(x,y);return !distributionCenters.ContainsKey(id) || HasCenterBusiness(actor,id) || HouseAt(actor.x,actor.y)==id || EmergencyShelter(actor);
        }
        private StorageMemoryRecord? HomeNote(ActorState actor,StorageBoxDocument box)
        {
            if(actor.storageMemory!=null)foreach(StorageMemoryRecord note in actor.storageMemory)if(note.x==box.x && note.y==box.y)return note;return null;
        }
        private bool HomeBoxChecked(ActorState actor,StorageBoxDocument box)
        {
            StorageMemoryRecord? note=HomeNote(actor,box);return note.HasValue && note.Value.reportedBy==0 && Tick-note.Value.observedTick<HomeSupplyRules.RecheckTicks;
        }
        public HomeSupplyKnowledge HomeSuppliesKnownBy(int index)
        {
            if(index<0 || index>=count)throw new ArgumentOutOfRangeException("index");ActorState actor=actors[index];HomeSupplyKnowledge result=new HomeSupplyKnowledge();
            if(!HomeSuppliesEnabled || actor.species!=Species.Human || actor.health<=0 || actor.assignedHouseId==0)return result;
            result.HouseId=actor.assignedHouseId;Room home=FindRoom(result.HouseId);
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box))continue;result.Boxes++;StorageMemoryRecord? note=HomeNote(actor,box);
                if(!note.HasValue)continue;
                if(Tick-note.Value.observedTick>=HomeSupplyRules.RecheckTicks){result.StaleBoxes++;continue;}
                if(note.Value.reportedBy!=0){result.ReportedBoxes++;continue;}
                result.KnownBoxes++;result.KnownMeals+=note.Value.food;
            }
            return result; // Counts remembered food only. Never inspect live box contents for an away resident.
        }
        private bool TryInspectHomeSupplies(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;
            if(!HomeSuppliesEnabled || actor.assignedHouseId==0 || actor.restockPhase!=RestockPhase.None || actor.turnAtTick!=0 || threat>=0 || actor.stress!=0 || actor.freezeUntilTick>Tick || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy || (actor.hunger>=HungerRules.SeekAt && actor.carriedFood>0))return false;
            Room home=FindRoom(actor.assignedHouseId);bool inside=home.Contains(Key(actor.x,actor.y));
            // Workers keep their current occupation; their off-duty home routines are a later increment.
            if(!inside)return false;
            int best=int.MaxValue,face=-1,route=-1;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box) || HomeBoxChecked(actor,box))continue;
                int goal,length,candidate=StoreFurnitureRoute(actor,home,box.x,box.y,out goal,out length);
                if(candidate>=0 && length<best){best=length;face=goal;route=candidate;}
            }
            if(face<0)return false;
            actor.homeShelterId=0;actor.shelterId=home.id;actor.shelterState=ShelterState.Inside;actor.shelterX=face%Width;actor.shelterY=face/Width;ClearShelterDeparture(ref actor);
            direction=route;actors[index]=actor;return true; // Replanned each opportunity; normal sight/adjacency records the actual stock.
        }
    }
}
