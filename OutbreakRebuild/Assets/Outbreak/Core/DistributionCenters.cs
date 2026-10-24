using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public struct DistributionCenterOverview
    {
        public int CenterId,SupplyBox,StockMeals,AvailableMeals,Capacity,Couriers,ActiveTrips;
    }
    public sealed partial class World
    {
        private readonly SortedDictionary<int,DistributionCenterDocument> distributionCenters=new SortedDictionary<int,DistributionCenterDocument>();
        private bool IsDistributionSupplyBox(int x,int y)
        {
            if(!DistributionCentersEnabled || distributionCenters.Count==0)return false;int key=Key(x,y)+1;
            foreach(DistributionCenterDocument center in distributionCenters.Values)if(center.supplyBox==key)return true;return false;
        }
        private bool IsRestrictedSupplyBox(StorageBoxDocument box){return IsStoreSupplyBox(box) || IsDistributionSupplyBox(box.x,box.y);}
        private bool StaffUsesSupply(ActorState actor,StorageBoxDocument box)
        {
            DistributionCenterDocument center;return ClerkUsesSupply(actor,box) || (DistributionCentersEnabled && distributionCenters.TryGetValue(actor.distributionCenterId,out center) && center.supplyBox==Key(box.x,box.y)+1);
        }
        private bool DistributionSourceAllowed(ActorState actor,int source)
        {
            if(actor.distributionCenterId==0)return true;DistributionCenterDocument center;
            return DistributionCentersEnabled && distributionCenters.TryGetValue(actor.distributionCenterId,out center) && center.supplyBox==source;
        }
        public bool TryConfigureDistributionCenter(int x,int y)
        {
            int id=HouseAt(x,y);StorageBoxDocument box=map.StorageBoxAt(x,y);if(!DistributionCentersEnabled || id==0 || box==null || IsStoreSupplyBox(box))return false;
            DistributionCenterDocument existing;
            if(distributionCenters.TryGetValue(id,out existing) && existing.supplyBox!=Key(x,y)+1)
                for(int i=0;i<count;i++)if(actors[i].distributionCenterId==id)return false;
            distributionCenters[id]=new DistributionCenterDocument{centerId=id,supplyBox=Key(x,y)+1};return true;
        }
        public bool TryAssignDistributionCourier(int index,int centerId)
        {
            DistributionCenterDocument center;if(!DistributionCentersEnabled || !CanEditCourier(index) || !distributionCenters.TryGetValue(centerId,out center))return false;
            ActorState actor=actors[index];if(actor.clerkStoreId!=0 || actor.assignedHouseId==0)return false;
            // Prepare membership on a detached actor; failed checks leave the live route and cargo unchanged.
            if(actor.courierBoxes!=null && Array.IndexOf(actor.courierBoxes,center.supplyBox)>=0)return false;
            actor.distributionCenterId=centerId;actor.courierSourceBox=center.supplyBox;actor.courierReturningHome=false;actors[index]=actor;return true;
        }
        public bool TryUnassignDistributionCourier(int index)
        {
            if(!DistributionCentersEnabled || !CanEditCourier(index) || actors[index].distributionCenterId==0)return false;
            ActorState actor=actors[index];actor.distributionCenterId=0;actors[index]=actor;return true;
        }
        public DistributionCenterOverview[] DistributionCenters()
        {
            DistributionCenterOverview[] result=new DistributionCenterOverview[distributionCenters.Count];int n=0;
            foreach(DistributionCenterDocument center in distributionCenters.Values)
            {
                int key=center.supplyBox-1;StorageBoxDocument box=map.StorageBoxAt(key%Width,key/Width);
                DistributionCenterOverview row=new DistributionCenterOverview{CenterId=center.centerId,SupplyBox=center.supplyBox,StockMeals=StorageRules.Count(box,InventoryItem.Food),Capacity=box.capacity};
                row.AvailableMeals=box.blockDonations?0:Math.Max(0,row.StockMeals-EffectiveFoodReserve(box));
                for(int i=0;i<count;i++)if(actors[i].species==Species.Human && actors[i].health>0 && actors[i].distributionCenterId==center.centerId){row.Couriers++;if(actors[i].restockPhase!=RestockPhase.None && actors[i].restockSourceBox==center.supplyBox)row.ActiveTrips++;}
                result[n++]=row;
            }
            return result;
        }
        private DistributionCenterDocument[] CaptureDistributionCenters()
        {
            if(distributionCenters.Count==0)return null;DistributionCenterDocument[] result=new DistributionCenterDocument[distributionCenters.Count];int i=0;
            foreach(DistributionCenterDocument center in distributionCenters.Values)result[i++]=new DistributionCenterDocument{centerId=center.centerId,supplyBox=center.supplyBox};return result;
        }
        private void RestoreDistributionCenters(DistributionCenterDocument[] records)
        {
            if(records==null)return;if(!DistributionCentersEnabled || records.Length<1 || records.Length>ShelterRules.MaxRooms)throw new ArgumentException("Invalid distribution centers.");int previous=0;
            foreach(DistributionCenterDocument center in records)
            {
                Room room=center==null?null:FindRoom(center.centerId);int key=center==null?-1:center.supplyBox-1;StorageBoxDocument box=key<0?null:map.StorageBoxAt(key%Width,key/Width);
                if(center==null || center.centerId<=previous || room==null || key<0 || !room.Contains(key) || box==null || IsStoreSupplyBox(box))throw new ArgumentException("Invalid distribution center stock box.");
                distributionCenters.Add(center.centerId,new DistributionCenterDocument{centerId=center.centerId,supplyBox=center.supplyBox});previous=center.centerId;
            }
        }
        private void ValidateDistributionCouriers()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.distributionCenterId==0)continue;DistributionCenterDocument center;
                if(!DistributionCentersEnabled || actor.species!=Species.Human || actor.health<=0 || actor.clerkStoreId!=0 || actor.assignedHouseId==0 || !distributionCenters.TryGetValue(actor.distributionCenterId,out center) || actor.courierSourceBox!=center.supplyBox)throw new ArgumentException("Invalid distribution-center courier.");
            }
        }
    }
}
