using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class HouseholdFoodRules
    {
        public const int DefaultMealsPerResident=3,MaxMealsPerResident=12;
    }
    public struct HouseholdFoodSupply
    {
        public int HouseId,Residents,MealsPerResident,RequestedMeals,BoxTarget,UnallocatedMeals,BoxMeals,ReserveDeficit,AutomaticBoxes,ManualBoxes;
    }
    public sealed partial class World
    {
        private readonly SortedDictionary<int,int> householdFoodTargets=new SortedDictionary<int,int>();
        public int HouseholdMealsPerResident(int houseId)
        {
            int meals;return HouseholdFoodTargetsEnabled && householdFoodTargets.TryGetValue(houseId,out meals)?meals:0;
        }
        public bool TrySetHouseholdMealsPerResident(int houseId,int meals)
        {
            if(!HouseholdFoodTargetsEnabled || FindRoom(houseId)==null || meals<0 || meals>HouseholdFoodRules.MaxMealsPerResident)return false;
            if(meals==0)householdFoodTargets.Remove(houseId);else householdFoodTargets[houseId]=meals;return true;
        }
        public bool TryUseHouseholdFoodTarget(int x,int y)
        {
            if(HouseholdMealsPerResident(HouseAt(x,y))==0)return false;StorageBoxDocument box=map.StorageBoxAt(x,y);if(box==null)return false;
            box.foodReserve=null;map.SetStorageBox(box);return true;
        }
        private Dictionary<int,int> HouseholdBoxTargets(Room room,out HouseholdFoodSupply supply)
        {
            supply=new HouseholdFoodSupply{HouseId=room.id,MealsPerResident=HouseholdMealsPerResident(room.id)};
            for(int i=0;i<count;i++)if(actors[i].species==Species.Human && actors[i].health>0 && actors[i].assignedHouseId==room.id)supply.Residents++;
            supply.RequestedMeals=supply.Residents*supply.MealsPerResident;
            Dictionary<int,int> targets=new Dictionary<int,int>();List<StorageBoxDocument> automatic=new List<StorageBoxDocument>();int manual=0,capacity=0;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if(!room.Contains(Key(box.x,box.y)))continue;
                supply.BoxMeals+=StorageRules.Count(box,InventoryItem.Food);
                if(supply.MealsPerResident==0 || box.foodReserve.HasValue){int reserve=StorageRules.FoodReserve(box);targets.Add(Key(box.x,box.y),reserve);manual+=reserve;supply.ManualBoxes++;}
                else{automatic.Add(box);capacity+=box.capacity;supply.AutomaticBoxes++;}
            }
            int budget=Math.Min(capacity,Math.Max(0,supply.RequestedMeals-manual));
            int low=0,high=StorageRules.MaxCapacity;
            while(low<high)
            {
                int level=(low+high+1)/2,used=0;foreach(StorageBoxDocument box in automatic)used+=Math.Min(box.capacity,level);
                if(used<=budget)low=level;else high=level-1;
            }
            int allocated=0;foreach(StorageBoxDocument box in automatic)allocated+=Math.Min(box.capacity,low);
            int extra=budget-allocated;
            foreach(StorageBoxDocument box in automatic){int reserve=Math.Min(box.capacity,low);if(extra>0 && reserve<box.capacity){reserve++;extra--;}targets.Add(Key(box.x,box.y),reserve);}
            foreach(StorageBoxDocument box in map.StorageRecords)if(room.Contains(Key(box.x,box.y)))
            {
                int reserve=targets[Key(box.x,box.y)];supply.BoxTarget+=reserve;supply.ReserveDeficit+=Math.Max(0,reserve-StorageRules.Count(box,InventoryItem.Food));
            }
            supply.UnallocatedMeals=Math.Max(0,supply.RequestedMeals-supply.BoxTarget);return targets;
        }
        public HouseholdFoodSupply HouseholdFoodSupplies(int houseId)
        {
            Room room=FindRoom(houseId);if(room==null)return new HouseholdFoodSupply();HouseholdFoodSupply supply;HouseholdBoxTargets(room,out supply);return supply;
        }
        public int EffectiveFoodReserve(StorageBoxDocument box)
        {
            if(box==null)throw new ArgumentNullException("box");if(householdFoodTargets.Count==0)return StorageRules.FoodReserve(box);int houseId=HouseAt(box.x,box.y);
            if(HouseholdMealsPerResident(houseId)==0)return StorageRules.FoodReserve(box);
            HouseholdFoodSupply supply;Dictionary<int,int> targets=HouseholdBoxTargets(FindRoom(houseId),out supply);int target;
            return targets.TryGetValue(Key(box.x,box.y),out target)?target:StorageRules.FoodReserve(box);
        }
        private HouseholdFoodTargetDocument[] CaptureHouseholdFoodTargets()
        {
            if(householdFoodTargets.Count==0)return null;HouseholdFoodTargetDocument[] result=new HouseholdFoodTargetDocument[householdFoodTargets.Count];int i=0;
            foreach(var pair in householdFoodTargets)result[i++]=new HouseholdFoodTargetDocument{houseId=pair.Key,mealsPerResident=pair.Value};return result;
        }
        private void RestoreHouseholdFoodTargets(HouseholdFoodTargetDocument[] records)
        {
            if(records==null)return;if(!HouseholdFoodTargetsEnabled || records.Length<1 || records.Length>ShelterRules.MaxRooms)throw new ArgumentException("Invalid household food targets.");int previous=0;
            foreach(HouseholdFoodTargetDocument record in records)
            {
                if(record==null || record.houseId<=previous || FindRoom(record.houseId)==null || record.mealsPerResident<1 || record.mealsPerResident>HouseholdFoodRules.MaxMealsPerResident)throw new ArgumentException("Invalid household food target record.");
                householdFoodTargets.Add(record.houseId,record.mealsPerResident);previous=record.houseId;
            }
        }
    }
}
