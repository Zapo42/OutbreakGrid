using System.Collections.Generic;
namespace Outbreak.Core
{
    public enum BuildingUse { None, Home, Store, DistributionCenter }
    public sealed partial class World
    {
        public BuildingUse BuildingUseAt(int x,int y)
        {
            return BuildingUseForHouse(HouseAt(x,y));
        }
        private BuildingUse BuildingUseForHouse(int id)
        {
            if(id==0)return BuildingUse.None;
            if(distributionCenters.ContainsKey(id))return BuildingUse.DistributionCenter;
            if(stores.ContainsKey(id))return BuildingUse.Store;
            return BuildingUse.Home;
        }
        public Dictionary<int,BuildingUse> BuildingUses()
        {
            Dictionary<int,BuildingUse> result=new Dictionary<int,BuildingUse>();
            foreach(Room room in rooms)result.Add(room.id,BuildingUseForHouse(room.id));
            return result; // Detached presentation metadata. Home means residential/shelter-capable, not ownership or access permission.
        }
    }
}
