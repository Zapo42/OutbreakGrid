using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private readonly SortedDictionary<int,HouseholdFurnitureDocument> householdFurniture=new SortedDictionary<int,HouseholdFurnitureDocument>();
        private readonly Dictionary<int,HouseholdFurnitureDocument> householdFootprint=new Dictionary<int,HouseholdFurnitureDocument>();
        internal bool HasWideOrComfyFurniture { get { foreach(var f in householdFurniture.Values)if(f.width==2 || f.kind==HouseholdFurnitureKind.ComfyChair)return true;return false; } }
        internal IEnumerable<HouseholdFurnitureDocument> HouseholdRecords { get { return householdFurniture.Values; } }
        public bool HouseholdLife { get; private set; }
        internal bool HasFurnitureAt(int x,int y){return Contains(x,y) && householdFootprint.ContainsKey(y*Width+x);}
        internal HouseholdFurnitureDocument FurnitureRecord(int x,int y){HouseholdFurnitureDocument f;return Contains(x,y) && householdFootprint.TryGetValue(y*Width+x,out f)?f:null;}
        internal static bool IsSeat(HouseholdFurnitureKind kind){return kind==HouseholdFurnitureKind.Chair || kind==HouseholdFurnitureKind.Couch || kind==HouseholdFurnitureKind.Bed || kind==HouseholdFurnitureKind.ComfyChair;}
        internal bool FurnitureBlocks(int x,int y){if(householdFurniture.Count==0)return false;var f=FurnitureRecord(x,y);return f!=null && !IsSeat(f.kind);}
        private static HouseholdFurnitureDocument CopyFurniture(HouseholdFurnitureDocument f){return new HouseholdFurnitureDocument{x=f.x,y=f.y,kind=f.kind,working=f.working,width=f.width};}
        public HouseholdFurnitureDocument FurnitureAt(int x,int y){var f=FurnitureRecord(x,y);return f==null?null:CopyFurniture(f);}
        internal HouseholdFurnitureDocument[] CaptureFurniture(){if(householdFurniture.Count==0)return null;var result=new HouseholdFurnitureDocument[householdFurniture.Count];int i=0;foreach(var f in householdFurniture.Values)result[i++]=CopyFurniture(f);return result;}
        public static int FurnitureWidth(HouseholdFurnitureDocument f){return f.width==2?2:1;}
        private void ValidateFurniture(HouseholdFurnitureDocument f)
        {
            if(f==null || f.kind<HouseholdFurnitureKind.KitchenTable || f.kind>HouseholdFurnitureKind.ComfyChair || f.width<0 || f.width>2 || f.width==2 && f.kind!=HouseholdFurnitureKind.Couch || (f.working && f.kind!=HouseholdFurnitureKind.Radio && f.kind!=HouseholdFurnitureKind.Television) || householdFurniture.Count>=256)throw new ArgumentException("Unsupported household furniture type, footprint or state.");
            // Check every covered cell before inserting anything, including the far couch seat.
            for(int offset=0;offset<FurnitureWidth(f);offset++){int x=f.x+offset,y=f.y;if(!Contains(x,y) || Get(x,y)!=Terrain.Floor || !CanWalk(x,y) || HasFurnitureAt(x,y) || RestSpotAt(x,y) || WeaponAt(x,y)!=null)throw new ArgumentException("Household furniture needs distinct free indoor floor.");}
        }
        public void SetFurniture(HouseholdFurnitureDocument f)
        {
            ValidateFurniture(f);var stored=CopyFurniture(f);householdFurniture.Add(f.y*Width+f.x,stored);for(int offset=0;offset<FurnitureWidth(f);offset++)householdFootprint.Add(f.y*Width+f.x+offset,stored);HouseholdLife=true;
        }
        private void RemoveFurnitureAt(int x,int y)
        {
            var f=FurnitureRecord(x,y);if(f==null)return;for(int offset=0;offset<FurnitureWidth(f);offset++)householdFootprint.Remove(f.y*Width+f.x+offset);householdFurniture.Remove(f.y*Width+f.x);
        }
        public void SetKitchenTableWithChairs(int x,int y)
        {
            var pieces=new[]{new HouseholdFurnitureDocument{x=x,y=y,kind=HouseholdFurnitureKind.KitchenTable},new HouseholdFurnitureDocument{x=x-1,y=y,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x+1,y=y,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x,y=y-1,kind=HouseholdFurnitureKind.Chair},new HouseholdFurnitureDocument{x=x,y=y+1,kind=HouseholdFurnitureKind.Chair}};
            // Validate the entire composite before writing any piece, including the global sparse bound.
            if(householdFurniture.Count>251)throw new ArgumentException("Too much household furniture.");foreach(var f in pieces)ValidateFurniture(f);foreach(var f in pieces)SetFurniture(f);
        }
        internal bool SetApplianceWorking(int x,int y,bool working){var f=FurnitureRecord(x,y);if(f==null || (f.kind!=HouseholdFurnitureKind.Radio && f.kind!=HouseholdFurnitureKind.Television))return false;f.working=working;return true;}
        private void RestoreFurniture(MapDocument source)
        {
            if(source.householdLife && source.schemaVersion!=15 && source.schemaVersion!=16 || (source.schemaVersion==15 || source.schemaVersion==16) && !source.householdLife || source.householdFurniture!=null && !source.householdLife)throw new ArgumentException("Household furniture requires household map schema15 or16.");
            HouseholdLife=source.householdLife;if(source.householdFurniture==null)return;if(source.householdFurniture.Length==0 || source.householdFurniture.Length>256)throw new ArgumentException("Invalid household furniture count.");foreach(var f in source.householdFurniture){if(source.schemaVersion<16 && f!=null && (f.width==2 || f.kind==HouseholdFurnitureKind.ComfyChair))throw new ArgumentException("Wide couches and comfy chairs require map schema16.");SetFurniture(f);}
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
