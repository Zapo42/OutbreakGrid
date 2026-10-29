using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static class DistributionLayoutChecks
{
    private static int assertions;
    private static void Require(bool ok,string message){assertions++;if(!ok)throw new InvalidOperationException(message);}
    private static int[] Point(JsonElement point){return new[]{point[0].GetInt32(),point[1].GetInt32()};}
    private static int[] Anchor(JsonElement building,string role)
    {
        var layout=building.GetProperty("interior_layout");var origin=Point(layout.GetProperty("origin_cells"));
        foreach(var room in layout.GetProperty("blueprint").GetProperty("rooms").EnumerateArray())if(room.GetProperty("role").GetString()==role){var p=Point(room.GetProperty("cells")[0]);return new[]{origin[0]+p[0],origin[1]+p[1]};}throw new Exception("Missing room "+role);
    }
    private static bool Route(WorldMap map,JsonElement building,int[] start,int[] end)
    {
        // Bound the physical check to this building and respect all live furniture.
        int left=building.GetProperty("left").GetInt32(),top=building.GetProperty("top").GetInt32(),width=building.GetProperty("width").GetInt32(),height=building.GetProperty("height").GetInt32();
        var queue=new Queue<int>();var seen=new HashSet<int>();int goal=end[1]*map.Width+end[0],first=start[1]*map.Width+start[0];queue.Enqueue(first);seen.Add(first);
        while(queue.Count>0){int c=queue.Dequeue();if(c==goal)return true;foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=c%map.Width+d.Item1,y=c/map.Width+d.Item2,key=y*map.Width+x;if(x<left||y<top||x>=left+width||y>=top+height||seen.Contains(key))continue;if(map.Get(x,y)!=Terrain.DoorClosed&&!map.CanWalk(x,y))continue;seen.Add(key);queue.Enqueue(key);}}return false;
    }
    public static int Main(string[] args)
    {
        int maps=0,centers=0,couriers=0;var sites=new List<object>();
        foreach(string path in Directory.GetFiles(args[0],"*.obmap2"))
        {
            var map=Documents.ParseMap(File.ReadAllText(path));string original=Documents.Encode(map.Capture());Require(Documents.Encode(Documents.ParseMap(original).Capture())==original,"Map roundtrip");
            using(var metadata=JsonDocument.Parse(map.AuthoringJson))
            {
                var buildings=metadata.RootElement.GetProperty("buildings").EnumerateArray().ToArray();var dc=buildings.Where(b=>b.TryGetProperty("interior_layout",out var l)&&l.TryGetProperty("warehouse_storage_materialized",out var f)&&f.GetBoolean()).ToArray();Require(dc.Length>0,"Map has no centers");
                foreach(var b in dc){var stock=Point(b.GetProperty("interior_layout").GetProperty("stock_box_cells"));var box=map.StorageBoxAt(stock[0],stock[1]);Require(box!=null&&box.capacity==100&&box.items.Length==0,"Expected finite empty100-capacity warehouse box");box.items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=80}};map.SetStorageBox(box);}
                var world=new World(map,metadata.RootElement.GetProperty("seed").GetUInt32(),encounter:true,hunger:true);Require(world.ShelterRoomCount==buildings.Length,"Grouped building count differs");
                foreach(var shop in buildings.Where(b=>b.GetProperty("game_building_use_hint").GetString()=="Store"&&b.TryGetProperty("interior_layout",out var l)&&l.TryGetProperty("stock_box_cells",out _)))
                {var stock=Point(shop.GetProperty("interior_layout").GetProperty("stock_box_cells"));Require(world.TryConfigureStore(stock[0],stock[1]),"Mixed-city store configuration failed");}
                foreach(var b in dc)
                {
                    centers++;var layout=b.GetProperty("interior_layout");var blueprint=layout.GetProperty("blueprint");var origin=Point(layout.GetProperty("origin_cells"));var dispatch=Anchor(b,"dispatch-office");var warehouse=Anchor(b,"warehouse");var stock=Point(layout.GetProperty("stock_box_cells"));int id=world.HouseAt(dispatch[0],dispatch[1]);
                    Require(id!=0&&world.HouseAt(warehouse[0],warehouse[1])==id&&world.HouseAt(stock[0],stock[1])==id,"Dispatch and warehouse split DC identity");Require(world.TryConfigureDistributionCenter(stock[0],stock[1]),"Existing DC API rejected warehouse supply box");Require(world.BuildingUseAt(dispatch[0],dispatch[1])==BuildingUse.DistributionCenter&&world.BuildingUseAt(stock[0],stock[1])==BuildingUse.DistributionCenter,"DC role missing in one room");Require(!map.CanWalk(stock[0],stock[1]),"Supply box not solid");
                    var computer=layout.GetProperty("order_computer");var cell=Point(computer.GetProperty("cell"));Require(map.Get(cell[0],cell[1])==Terrain.Wall&&!map.CanWalk(cell[0],cell[1]),"Computer footprint is not a solid reservation");Require(!computer.GetProperty("interaction_implemented").GetBoolean(),"Geometry claims computer mechanics");
                    sites.Add(new{mapId=map.Id,rulesVersion=world.Capture().rulesVersion,centerId=id,supplyBoxId=stock[1]*map.Width+stock[0]+1,
                        stockCell=stock,computerId=computer.GetProperty("id").GetString(),computerCell=cell,
                        approachCells=computer.GetProperty("approach_cells").EnumerateArray().Select(Point).ToArray(),dispatchFloor=dispatch,
                        computerRepresentation="reserved-solid-terrain",interactionImplemented=false});
                    foreach(var approach in computer.GetProperty("approach_cells").EnumerateArray()){var p=Point(approach);Require(world.HouseAt(p[0],p[1])==id&&Route(map,b,dispatch,p),"Computer approach inaccessible");}
                    foreach(var fixture in blueprint.GetProperty("fixtures").EnumerateArray())foreach(var approach in fixture.GetProperty("approach_cells").EnumerateArray()){var p=Point(approach);Require(Route(map,b,dispatch,new[]{origin[0]+p[0],origin[1]+p[1]}),"Warehouse rack or computer approach inaccessible");}
                    foreach(var door in blueprint.GetProperty("doors").EnumerateArray()){var local=Point(door.GetProperty("cell"));int x=origin[0]+local[0],y=origin[1]+local[1];Require(Route(map,b,dispatch,new[]{x,y}),"Door disconnected from dispatch");Require(door.GetProperty("external").GetBoolean()?world.HouseAt(x,y)==0:world.HouseAt(x,y)==id,"Internal/exterior doorway identity changed");}
                    // Only the mixed example has a genuine separate home. Do not
                    // invent a warehouse-home assignment for standalone centers.
                    var homes=buildings.Where(h=>h.GetProperty("game_building_use_hint").GetString()=="Home"&&h.TryGetProperty("interior_layout",out _)).ToArray();
                    if(homes.Length>0){int index=world.Count,actorId;Require(world.TrySpawn(Species.Human,dispatch[0],dispatch[1],out actorId),"Courier spawn");var home=Anchor(homes[0],"living-area");Require(world.TryAssignHouse(index,home[0],home[1])&&world.ActorAt(index).assignedHouseId!=id,"Courier needs separate real home");Require(world.TryAssignDistributionCourier(index,id)&&world.TrySetCourierSchedule(index,7,12),"Existing courier/shift contract rejected grouped DC");couriers++;}
                }
                foreach(var row in world.DistributionCenters())Require(row.StockMeals==80&&row.AvailableMeals==74&&row.Capacity==100,"Finite stock/reserve accounting changed");
                string save=Documents.Encode(world.Capture());Require(Documents.Encode(Documents.ParseSave(save).Capture())==save,"Configured center/courier save roundtrip");
                // Map metadata carries the computer position; no new save fields or
                // behavior state are introduced by this physical reservation.
                var restored=Documents.ParseSave(save);Require(restored.Capture().map.authoringJson==map.AuthoringJson,"Computer geometry metadata lost");
                Documents.Write(Path.ChangeExtension(path,"obsave"),save);
            }
            maps++;
        }
        File.WriteAllText(Path.Combine(args[0],"distribution-sites.json"),JsonSerializer.Serialize(sites,new JsonSerializerOptions{WriteIndented=true}));
        Require(maps>=3&&centers>=3&&couriers>0,"Need compact, spacious and mixed-map coverage");Console.WriteLine("PASS "+assertions+" DC geometry/API assertions across "+maps+" maps, "+centers+" centers, "+couriers+" courier assignments; exact roundtrips; no behavior ticks.");return 0;
    }
}
