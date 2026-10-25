using System; // Validate bounded furniture and stock before publication.
using System.Collections.Generic; // Keep shelf storage proportional to authored furniture.
using Outbreak.Contracts; // Export stocks and search state with the map.
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private readonly Dictionary<int, ShelfDocument> shelves = new Dictionary<int, ShelfDocument>();
        internal System.Collections.Generic.IEnumerable<ShelfDocument> ShelfRecords { get { return shelves.Values; } }
        public bool HasShelves { get { return shelves.Count != 0; } }
        public ShelfDocument ShelfAt(int x, int y) { ShelfDocument s; return Contains(x,y) && shelves.TryGetValue(y*Width+x,out s) ? new ShelfDocument { x=s.x,y=s.y,food=s.food,searched=s.searched } : null; } // Return detached data, never a mutable live container.
        public void SetShelf(int x, int y, int food, bool searched = false)
        {
            if (!Contains(x,y) || !TerrainRules.Walkable(Get(x,y)) || Get(x,y)==Terrain.DoorOpen || PhoneAt(x,y) || StorageBoxAt(x,y)!=null || RestSpotAt(x,y) || WeaponAt(x,y)!=null || food<0 || food>1000 || (!shelves.ContainsKey(y*Width+x) && shelves.Count>=4096)) throw new ArgumentException("Shelf needs a non-door walkable terrain cell, 0–1000 food and room in the 4096-shelf limit.");
            if(!shelves.ContainsKey(y*Width+x))DistrictSurfaceChanged(x,y);
            shelves[y*Width+x]=new ShelfDocument { x=x,y=y,food=food,searched=searched };
        }
        internal ShelfDocument[] CaptureShelves()
        {
            if(shelves.Count==0)return null;List<int> keys=new List<int>(shelves.Keys);keys.Sort();ShelfDocument[] result=new ShelfDocument[keys.Count];for(int i=0;i<keys.Count;i++){ShelfDocument s=shelves[keys[i]];result[i]=new ShelfDocument{x=s.x,y=s.y,food=s.food,searched=s.searched};}return result;
        }
        private void RestoreShelves(MapDocument source)
        {
            if(source.schemaVersion<3){if(source.shelves!=null)throw new ArgumentException("Shelves require a version-three map.");return;}
            if(source.schemaVersion>=4 && source.shelves==null)return;
            if(source.shelves==null || source.shelves.Length==0 || source.shelves.Length>4096)throw new ArgumentException("Invalid shelf collection.");
            foreach(ShelfDocument s in source.shelves){if(s==null || !Contains(s.x,s.y) || shelves.ContainsKey(s.y*Width+s.x))throw new ArgumentException("Invalid or duplicate shelf.");SetShelf(s.x,s.y,s.food,s.searched);}
        }
    }
    public sealed partial class World
    {
        public ShelfDocument[] ShelfSites() { return map.CaptureShelves() ?? new ShelfDocument[0]; } // Supply detached debug/presentation records, not AI knowledge.
        public ShelfDocument ShelfAt(int x,int y) { return map.ShelfAt(x,y); }
        public bool TryPlaceShelf(int x,int y,int food)
        {
            if(map.ParkingExitAt(x,y)>=0 || VehicleAt(x,y) || !HungerEnabled || map.RestSpotAt(x,y) || map.WeaponAt(x,y)!=null || !map.Contains(x,y) || !map.CanWalk(x,y) || IsDoor(x,y) || food<0 || food>1000)return false;
            for(int i=0;i<count;i++)if((actors[i].x==x && actors[i].y==y) || (actors[i].shelterId!=0 && actors[i].shelterX==x && actors[i].shelterY==y))return false;
            if(ShelfSites().Length>=4096)return false;map.SetShelf(x,y,food);TerrainRevision++;return true; // Furniture is solid and cannot be placed through a body or reserved destination.
        }
        internal void SeedScenarioShelves()
        {
            for(int i=0;i<rooms.Length;i+=2){Room room=rooms[i];foreach(int key in room.cells)if(map.CanWalk(key%Width,key/Width)){map.SetShelf(key%Width,key/Width,6);break;}}
            if(map.CanWalk(Width-3,Height-3) && !IsDoor(Width-3,Height-3))map.SetShelf(Width-3,Height-3,12); // Some rooms start without supplies, and outdoor stocks offer a reason to forage.
        }
    }
}
