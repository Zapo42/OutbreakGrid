using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private readonly SortedDictionary<int,HouseholdFurnitureDocument> householdFurniture=new SortedDictionary<int,HouseholdFurnitureDocument>();
        internal IEnumerable<HouseholdFurnitureDocument> HouseholdRecords { get { return householdFurniture.Values; } }
        public bool HouseholdLife { get; private set; }
        internal bool HasFurnitureAt(int x,int y){return Contains(x,y) && householdFurniture.ContainsKey(y*Width+x);}
        internal HouseholdFurnitureDocument FurnitureRecord(int x,int y){HouseholdFurnitureDocument f;return Contains(x,y) && householdFurniture.TryGetValue(y*Width+x,out f)?f:null;}
        internal static bool IsSeat(HouseholdFurnitureKind kind){return kind==HouseholdFurnitureKind.Chair || kind==HouseholdFurnitureKind.Couch || kind==HouseholdFurnitureKind.Bed;}
        internal bool FurnitureBlocks(int x,int y){if(householdFurniture.Count==0)return false;var f=FurnitureRecord(x,y);return f!=null && !IsSeat(f.kind);}
        private static HouseholdFurnitureDocument CopyFurniture(HouseholdFurnitureDocument f){return new HouseholdFurnitureDocument{x=f.x,y=f.y,kind=f.kind,working=f.working};}
        public HouseholdFurnitureDocument FurnitureAt(int x,int y){var f=FurnitureRecord(x,y);return f==null?null:CopyFurniture(f);}
        internal HouseholdFurnitureDocument[] CaptureFurniture(){if(householdFurniture.Count==0)return null;var result=new HouseholdFurnitureDocument[householdFurniture.Count];int i=0;foreach(var f in householdFurniture.Values)result[i++]=CopyFurniture(f);return result;}
        private void ValidateFurniture(HouseholdFurnitureDocument f)
        {
            if(f==null || !Contains(f.x,f.y) || Get(f.x,f.y)!=Terrain.Floor || !CanWalk(f.x,f.y) || HasFurnitureAt(f.x,f.y) || RestSpotAt(f.x,f.y) || WeaponAt(f.x,f.y)!=null || f.kind<HouseholdFurnitureKind.KitchenTable || f.kind>HouseholdFurnitureKind.Radio || (f.working && f.kind!=HouseholdFurnitureKind.Radio && f.kind!=HouseholdFurnitureKind.Television) || householdFurniture.Count>=256)throw new ArgumentException("Household furniture needs distinct free indoor floor and a supported type/state.");
        }
        public void SetFurniture(HouseholdFurnitureDocument f){ValidateFurniture(f);householdFurniture.Add(f.y*Width+f.x,CopyFurniture(f));HouseholdLife=true;}
        public void SetKitchenTableWithChairs(int x,int y)
        {
            var pieces=new[]{new HouseholdFurnitureDocument{x=x,y=y,kind=HouseholdFurnitureKind.KitchenTable},new HouseholdFurnitureDocument{x=x-1,y=y,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x+1,y=y,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x,y=y-1,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x,y=y+1,kind=HouseholdFurnitureKind.Chair}};
            // Validate the entire composite before writing any piece, including the global sparse bound.
            if(householdFurniture.Count>251)throw new ArgumentException("Too much household furniture.");foreach(var f in pieces)ValidateFurniture(f);foreach(var f in pieces)SetFurniture(f);
        }
        internal bool SetApplianceWorking(int x,int y,bool working){var f=FurnitureRecord(x,y);if(f==null || (f.kind!=HouseholdFurnitureKind.Radio && f.kind!=HouseholdFurnitureKind.Television))return false;f.working=working;return true;}
        private void RestoreFurniture(MapDocument source)
        {
            if(source.householdLife && source.schemaVersion!=15 || source.schemaVersion==15 && !source.householdLife || source.householdFurniture!=null && !source.householdLife)throw new ArgumentException("Household furniture requires household map schema15.");
            HouseholdLife=source.householdLife;if(source.householdFurniture==null)return;if(source.householdFurniture.Length==0 || source.householdFurniture.Length>256)throw new ArgumentException("Invalid household furniture count.");foreach(var f in source.householdFurniture)SetFurniture(f);
        }
    }
    public sealed partial class World
    {
        public bool HouseholdActivitiesAvailable { get { return DomesticLifeEnabled && map.HouseholdLife; } }
        public HouseholdFurnitureDocument[] HouseholdFurnitureSites(){return map.CaptureFurniture()??new HouseholdFurnitureDocument[0];}
        public bool TrySetApplianceWorking(int x,int y,bool working){return HouseholdActivitiesAvailable && map.SetApplianceWorking(x,y,working);}
        public bool HouseholdSeatAt(int x,int y){if(!HouseholdActivitiesAvailable)return false;var f=map.FurnitureRecord(x,y);return HouseholdActivitiesAvailable && f!=null && WorldMap.IsSeat(f.kind);}
    }
}
