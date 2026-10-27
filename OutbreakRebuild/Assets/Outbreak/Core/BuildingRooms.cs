using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private BuildingRoomsDocument[] buildingRooms;
        internal bool HasBuildingRooms { get { return buildingRooms!=null && buildingRooms.Length>0; } }
        public BuildingRoomsDocument[] BuildingRoomGroups()
        {
            if(!HasBuildingRooms)return new BuildingRoomsDocument[0];
            BuildingRoomsDocument[] result=new BuildingRoomsDocument[buildingRooms.Length];
            for(int i=0;i<result.Length;i++)
            {
                BuildingRoomsDocument item=buildingRooms[i];BuildingRoomDocument[] rooms=new BuildingRoomDocument[item.rooms.Length];
                for(int j=0;j<rooms.Length;j++){BuildingRoomDocument r=item.rooms[j];rooms[j]=new BuildingRoomDocument{id=r.id,role=r.role,x=r.x,y=r.y};}
                result[i]=new BuildingRoomsDocument{id=item.id,rooms=rooms};
            }
            return result; // Never let a caller mutate live membership through a captured record.
        }
        private void RestoreBuildingRooms(MapDocument source)
        {
            if(source.buildingRooms==null){if(source.schemaVersion==14)throw new ArgumentException("Version14 requires building room groups.");return;}
            if(source.schemaVersion<14)throw new ArgumentException("Building room groups require map version14.");
            SetBuildingRooms(source.buildingRooms);
        }
        public void SetBuildingRooms(BuildingRoomsDocument[] source)
        {
            if(source==null || source.Length==0 || source.Length>ShelterRules.MaxRooms)throw new ArgumentException("Invalid building group count.");
            HashSet<string> identities=new HashSet<string>(StringComparer.Ordinal);HashSet<int> visited=new HashSet<int>();
            List<BuildingRoomsDocument> groups=new List<BuildingRoomsDocument>();int totalRooms=0,totalFloors=0;
            foreach(BuildingRoomsDocument group in source)
            {
                if(group==null || string.IsNullOrWhiteSpace(group.id) || !identities.Add(group.id) || group.rooms==null || group.rooms.Length==0 || group.rooms.Length>ShelterRules.MaxRooms-totalRooms)throw new ArgumentException("Invalid or duplicate building group.");
                totalRooms+=group.rooms.Length;HashSet<string> names=new HashSet<string>(StringComparer.Ordinal);
                List<BuildingRoomDocument> records=new List<BuildingRoomDocument>();List<HashSet<int>> roomDoors=new List<HashSet<int>>();int groupFloors=0;
                foreach(BuildingRoomDocument room in group.rooms)
                {
                    if(room==null || string.IsNullOrWhiteSpace(room.id) || string.IsNullOrWhiteSpace(room.role) || !names.Add(room.id) || !Contains(room.x,room.y) || Get(room.x,room.y)!=Terrain.Floor)throw new ArgumentException("Invalid building room anchor or identity.");
                    int seed=room.y*Width+room.x;if(!visited.Add(seed))throw new ArgumentException("Building groups overlap a physical room.");
                    Queue<int> pending=new Queue<int>();pending.Enqueue(seed);HashSet<int> doors=new HashSet<int>();int floors=0;
                    while(pending.Count>0)
                    {
                        int key=pending.Dequeue(),x=key%Width,y=key/Width;floors++;groupFloors++;totalFloors++;
                        if(floors>ShelterRules.MaxRoomCells || groupFloors>ShelterRules.MaxRoomCells || totalFloors>ShelterRules.MaxFloors)throw new ArgumentException("Building floor budget exceeded.");
                        for(int d=0;d<4;d++)
                        {
                            int nx=x+(d==0?-1:d==1?1:0),ny=y+(d==2?-1:d==3?1:0);
                            if(!Contains(nx,ny))throw new ArgumentException("Building room is not enclosed.");
                            Terrain tile=Get(nx,ny);int next=ny*Width+nx;
                            if(tile==Terrain.Floor){if(visited.Add(next))pending.Enqueue(next);}
                            else if(tile==Terrain.DoorClosed || tile==Terrain.DoorOpen)doors.Add(next);
                            else if(tile!=Terrain.Wall && tile!=Terrain.Window)throw new ArgumentException("Building room is not enclosed.");
                        }
                    }
                    if(doors.Count==0 || doors.Count>ShelterRules.MaxDoors)throw new ArgumentException("Invalid physical room doors.");
                    roomDoors.Add(doors);records.Add(new BuildingRoomDocument{id=room.id,role=room.role,x=room.x,y=room.y});
                }
                // Group membership must describe rooms joined by actual doors, never remote rooms.
                HashSet<int> reached=new HashSet<int>{0};Queue<int> search=new Queue<int>();search.Enqueue(0);
                while(search.Count>0){int index=search.Dequeue();for(int j=0;j<roomDoors.Count;j++)if(!reached.Contains(j) && roomDoors[index].Overlaps(roomDoors[j])){reached.Add(j);search.Enqueue(j);}}
                if(reached.Count!=roomDoors.Count)throw new ArgumentException("Building rooms are not connected by internal doors.");
                Dictionary<int,int> counts=new Dictionary<int,int>();foreach(HashSet<int> doors in roomDoors)foreach(int key in doors){int n;counts.TryGetValue(key,out n);counts[key]=n+1;}
                int exits=0;foreach(int n in counts.Values)if(n==1)exits++;
                if(exits==0 || exits>ShelterRules.MaxDoors)throw new ArgumentException("Building needs bounded exterior entrances.");
                records.Sort((a,b)=>StringComparer.Ordinal.Compare(a.id,b.id));groups.Add(new BuildingRoomsDocument{id=group.id,rooms=records.ToArray()});
            }
            groups.Sort((a,b)=>StringComparer.Ordinal.Compare(a.id,b.id));buildingRooms=groups.ToArray(); // Publish only after all candidate groups validate.
        }
    }
    public sealed partial class World
    {
        private List<Room> GroupBuildingRooms(List<Room> physical)
        {
            if(!map.HasBuildingRooms)return physical; // Legacy maps retain exactly their original discovery and identities.
            Dictionary<int,Room> owner=new Dictionary<int,Room>();foreach(Room room in physical)foreach(int key in room.cells)owner.Add(key,room);
            HashSet<int> claimed=new HashSet<int>();List<Room> merged=new List<Room>();
            foreach(BuildingRoomsDocument group in map.BuildingRoomGroups())
            {
                List<int> cells=new List<int>();Dictionary<int,int> counts=new Dictionary<int,int>();
                foreach(BuildingRoomDocument member in group.rooms)
                {
                    Room room;if(!owner.TryGetValue(Key(member.x,member.y),out room) || !claimed.Add(room.id))throw new ArgumentException("Building room is absent or claimed twice.");
                    cells.AddRange(room.cells);foreach(int key in room.doors){int n;counts.TryGetValue(key,out n);counts[key]=n+1;}
                }
                cells.Sort();List<int> exterior=new List<int>(),interior=new List<int>();foreach(KeyValuePair<int,int> door in counts){if(door.Value==1)exterior.Add(door.Key);else interior.Add(door.Key);}
                exterior.Sort();interior.Sort();merged.Add(new Room{id=cells[0]+1,cells=cells.ToArray(),doors=exterior.ToArray(),interiorDoors=interior.ToArray()});
            }
            foreach(Room room in physical)if(!claimed.Contains(room.id))merged.Add(room);
            merged.Sort((a,b)=>a.id.CompareTo(b.id));return merged;
        }
    }
}
