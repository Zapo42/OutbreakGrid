using System;
using System.IO;
using System.Text.Json;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static class BuildingRoomChecks
{
    private static int assertions;
    private static void Require(bool condition,string message){assertions++;if(!condition)throw new InvalidOperationException(message);}
    private static void Reject(Action action){assertions++;try{action();}catch(ArgumentException){return;}throw new InvalidOperationException("Invalid building geometry was accepted.");}
    private static WorldMap Shell()
    {
        WorldMap map=new WorldMap(9,7);
        for(int y=0;y<7;y++)for(int x=0;x<9;x++)map.Set(x,y,x==0 || y==0 || x==8 || y==6 || x==5?Terrain.Wall:Terrain.Floor);
        map.Set(5,3,Terrain.DoorClosed);map.Set(3,6,Terrain.DoorClosed);return map;
    }
    private static BuildingRoomsDocument[] Members()
    {
        return new[]{new BuildingRoomsDocument{id="house-1",rooms=new[]{new BuildingRoomDocument{id="living",role="living-area",x=1,y=1},new BuildingRoomDocument{id="storage",role="household-storage",x=6,y=1}}}};
    }
    public static int Main(string[] args)
    {
        WorldMap map=Shell();string legacy=Documents.Encode(map.Capture());
        Require(new World(map,214,encounter:true,hunger:true).ShelterRoomCount==2,"Legacy rooms changed.");
        Require(Documents.Encode(Documents.ParseMap(legacy).Capture())==legacy,"Legacy bytes changed.");
        BuildingRoomsDocument[] members=Members();map.SetBuildingRooms(members);members[0].rooms[0].x=99;
        Require(map.BuildingRoomGroups()[0].rooms[0].x==1,"Input array aliases live metadata.");
        BuildingRoomsDocument[] snapshot=map.BuildingRoomGroups();snapshot[0].rooms[0].x=99;
        Require(map.BuildingRoomGroups()[0].rooms[0].x==1,"Output array aliases live metadata.");
        World world=new World(map,214,encounter:true,hunger:true);int id=world.HouseAt(1,1);
        Require(world.ShelterRoomCount==1 && id!=0 && id==world.HouseAt(6,1),"Storage became a second home.");
        Require(id==world.HouseAt(5,3),"Internal door leaves its home membership.");
        Require(world.HouseAt(3,6)==0,"Exterior entrance incorrectly became an indoor passage.");
        Require(map.Capture().schemaVersion==14,"Group schema missing.");
        string grouped=Documents.Encode(map.Capture());Require(Documents.Encode(Documents.ParseMap(grouped).Capture())==grouped,"Grouped map round-trip differs.");
        string saved=Documents.Encode(world.Capture());Require(Documents.Encode(Documents.ParseSave(saved).Capture())==saved,"Grouped save round-trip differs.");
        // Closed/open internal doors retain the same spatial identity without changing actor decisions.
        map.Set(5,3,Terrain.DoorOpen);World open=new World(map,214,encounter:true,hunger:true);
        Require(open.HouseAt(5,3)==open.HouseAt(1,1) && open.ShelterRoomCount==1,"Door state changed membership.");
        MapDocument broken=map.Capture();broken.schemaVersion=13;Reject(()=>WorldMap.Restore(broken));
        broken=map.Capture();broken.buildingRooms=null;Reject(()=>WorldMap.Restore(broken));
        broken=map.Capture();broken.schemaVersion=15;Reject(()=>WorldMap.Restore(broken));
        WorldMap candidate=Shell();BuildingRoomsDocument[] invalid=Members();invalid[0].rooms[1].x=2;Reject(()=>candidate.SetBuildingRooms(invalid));
        invalid=Members();invalid[0].rooms[1].id="living";Reject(()=>candidate.SetBuildingRooms(invalid));
        invalid=Members();invalid[0].rooms[1].x=0;Reject(()=>candidate.SetBuildingRooms(invalid));
        candidate.SetBuildingRooms(Members());string before=Documents.Encode(candidate.Capture());
        invalid=Members();invalid[0].rooms[0].role="";Reject(()=>candidate.SetBuildingRooms(invalid));
        Require(Documents.Encode(candidate.Capture())==before,"Failed validation changed existing membership.");
        WorldMap disconnected=Shell();disconnected.Set(5,3,Terrain.Wall);disconnected.Set(7,6,Terrain.DoorClosed);Reject(()=>disconnected.SetBuildingRooms(Members()));
        Console.WriteLine("PASS building grouping, internal/exterior doors, schema, aliases, corrupt input and legacy/map/save compatibility");
        if(args.Length>0)
        {
            string[] files=Directory.GetFiles(args[0],"*.obmap2");Require(files.Length>=3,"Expected at least three generated maps.");
            foreach(string path in files)
            {
                WorldMap generated=Documents.ParseMap(File.ReadAllText(path));World instance=new World(generated,214,encounter:true,hunger:true);
                using(JsonDocument metadata=JsonDocument.Parse(generated.AuthoringJson))
                {
                    MapDocument features=generated.Capture();Require(features.storageBoxes!=null && features.restSpots!=null && features.restSpots.Length>0,"Live house fixtures missing.");
                    foreach(StorageBoxDocument box in features.storageBoxes){Require(box.capacity==StorageRules.DefaultCapacity && box.items.Length==0,"House created food or changed storage capacity.");Require(!generated.CanWalk(box.x,box.y),"Storage has no physical collision.");}
                    int expected=metadata.RootElement.GetProperty("buildings").GetArrayLength();Require(instance.ShelterRoomCount==expected,"Generated houses changed room identity.");
                    foreach(BuildingRoomsDocument group in generated.BuildingRoomGroups())
                    {
                        int home=instance.HouseAt(group.rooms[0].x,group.rooms[0].y);Require(home!=0,"Missing house.");
                        foreach(BuildingRoomDocument room in group.rooms)Require(instance.HouseAt(room.x,room.y)==home,"Generated storage has separate ownership.");
                    }
                    foreach(JsonElement building in metadata.RootElement.GetProperty("buildings").EnumerateArray())
                    {
                        if(!building.TryGetProperty("interior_layout",out JsonElement layout))continue;
                        int ox=layout.GetProperty("origin_cells")[0].GetInt32(),oy=layout.GetProperty("origin_cells")[1].GetInt32();
                        JsonElement blueprint=layout.GetProperty("blueprint");
                        foreach(JsonElement door in blueprint.GetProperty("doors").EnumerateArray())
                        {
                            int x=ox+door.GetProperty("cell")[0].GetInt32(),y=oy+door.GetProperty("cell")[1].GetInt32();
                            Require(generated.Get(x,y)==Terrain.DoorClosed,"Generated internal/external door was lost.");
                            if(!door.GetProperty("external").GetBoolean())Require(instance.HouseAt(x,y)!=0,"Generated passage lost home membership.");
                        }
                    }
                }
                string encoded=Documents.Encode(generated.Capture());Require(Documents.Encode(Documents.ParseMap(encoded).Capture())==encoded,"Generated map round-trip differs.");
                string save=Documents.Encode(instance.Capture());Require(Documents.Encode(Documents.ParseSave(save).Capture())==save,"Generated save round-trip differs.");
                Console.WriteLine("PASS active house spatial identity and exact map/save round-trip: "+Path.GetFileName(path));
            }
        }
        Console.WriteLine("Building infrastructure assertions: "+assertions);return 0;
    }
}
