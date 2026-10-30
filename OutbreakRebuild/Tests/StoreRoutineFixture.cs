using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static class StoreRoutineFixture
{
    private static int assertions;
    private static void Check(bool ok,string message){assertions++;if(!ok)throw new InvalidOperationException(message);}
    private static int[] Point(JsonElement point){return new[]{point[0].GetInt32(),point[1].GetInt32()};}
    private static int[] Anchor(JsonElement building,string role)
    {
        var layout=building.GetProperty("interior_layout");var origin=Point(layout.GetProperty("origin_cells"));
        foreach(var room in layout.GetProperty("blueprint").GetProperty("rooms").EnumerateArray())if(room.GetProperty("role").GetString()==role){var p=Point(room.GetProperty("cells")[0]);return new[]{p[0]+origin[0],p[1]+origin[1]};}
        throw new Exception("Missing room "+role);
    }
    private static List<int[]> PathBetween(WorldMap map,int[] start,int[] goal)
    {
        var queue=new Queue<int>();var previous=new Dictionary<int,int>();int first=start[1]*map.Width+start[0],last=goal[1]*map.Width+goal[0];
        Func<int,int,bool> passable=(x,y)=>{Terrain t=map.Get(x,y);return map.Contains(x,y)&&(t==Terrain.DoorClosed||map.CanWalk(x,y))&&(t==Terrain.Floor||t==Terrain.DoorClosed||t==Terrain.DoorOpen||t==Terrain.Sidewalk||t==Terrain.Crosswalk);};
        Check(passable(start[0],start[1])&&passable(goal[0],goal[1]),"Route endpoint blocked");queue.Enqueue(first);previous[first]=-1;
        while(queue.Count>0&&!previous.ContainsKey(last)){int c=queue.Dequeue();foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=c%map.Width+d.Item1,y=c/map.Width+d.Item2,key=y*map.Width+x;if(passable(x,y)&&!previous.ContainsKey(key)){previous[key]=c;queue.Enqueue(key);}}}
        Check(previous.ContainsKey(last),"Disconnected route "+string.Join(",",start)+" to "+string.Join(",",goal));var result=new List<int[]>();for(int c=last;c!=-1;c=previous[c])result.Add(new[]{c%map.Width,c/map.Width});result.Reverse();return result;
    }
    private static int TotalFood(SaveDocument save)
    {
        int total=save.actors.Sum(a=>a.carriedFood);
        if(save.map.shelves!=null)total+=save.map.shelves.Sum(s=>s.food);
        if(save.map.storageBoxes!=null)foreach(var box in save.map.storageBoxes)foreach(var item in box.items)if(item.item==InventoryItem.Food)total+=item.quantity;
        return total;
    }
    public static int Main(string[] args)
    {
        string input=args[0],output=args[1];Directory.CreateDirectory(output);var original=Documents.ParseMap(File.ReadAllText(input));
        using(var metadata=JsonDocument.Parse(original.AuthoringJson))
        {
            uint seed=metadata.RootElement.GetProperty("seed").GetUInt32();var buildings=metadata.RootElement.GetProperty("buildings").EnumerateArray().ToArray();var stores=buildings.Where(b=>b.GetProperty("game_building_use_hint").GetString()=="Store").ToArray();var homes=buildings.Where(b=>b.GetProperty("game_building_use_hint").GetString()=="Home").ToArray();
            Check(stores.Length==2&&homes.Length==3,"Fixture building count");int[] dc={42,12},dcStock=Point(metadata.RootElement.GetProperty("distribution_stock")),park=Point(metadata.RootElement.GetProperty("park").GetProperty("destination"));
            var allCases=new List<object>();
            foreach(string scenario in new[]{"restocking","shopping","delivery","long-shopping"})
            {
                var map=WorldMap.Restore(original.Capture());
                foreach(var box in map.Capture().storageBoxes)
                {
                    int meals=box.x==dcStock[0]&&box.y==dcStock[1]?(scenario=="delivery"?180:scenario=="long-shopping"?168:120):box.capacity==30&&stores.Any(b=>b.GetProperty("interior_layout").GetProperty("stock_box_cells")[0].GetInt32()==box.x&&b.GetProperty("interior_layout").GetProperty("stock_box_cells")[1].GetInt32()==box.y)?(scenario=="restocking"?30:0):scenario=="long-shopping"?0:6;
                    box.items=meals==0?new ItemStackDocument[0]:new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=meals}};map.SetStorageBox(box);
                }
                foreach(var shelf in map.Capture().shelves)map.SetShelf(shelf.x,shelf.y,scenario=="shopping"||scenario=="long-shopping"&&shelf.x>50?5:0,true);
                var world=new World(map,seed,encounter:true,hunger:true);Check(world.ShelterRoomCount==6,"Grouped building count differs");
                foreach(var b in stores){var stock=Point(b.GetProperty("interior_layout").GetProperty("stock_box_cells"));Check(world.TryConfigureStore(stock[0],stock[1]),"Configure store");}
                Check(world.TryConfigureDistributionCenter(dcStock[0],dcStock[1]),"Configure DC");
                // Stable spawn order gives portable actor IDs; APIs accept actor indices.
                var spawn=new[]{dc,Anchor(stores[0],"sales-floor"),Anchor(stores[1],"sales-floor"),Anchor(homes[0],"living-area"),Anchor(homes[1],"living-area")};int[] homeIndices={2,0,1,0,1};
                for(int i=0;i<spawn.Length;i++){int id;Check(world.TrySpawn(Species.Human,spawn[i][0],spawn[i][1],out id),"Spawn actor");var home=Anchor(homes[homeIndices[i]],"living-area");Check(world.TryAssignHouse(i,home[0],home[1]),"Assign home");}
                Check(world.TryAssignDistributionCourier(0,world.HouseAt(dcStock[0],dcStock[1]))&&world.TrySetCourierSchedule(0,7,12),"Assign courier and shift");
                for(int i=0;i<2;i++)Check(world.TryAssignClerk(i+1,world.HouseAt(spawn[i+1][0],spawn[i+1][1]))&&world.TrySetClerkSchedule(i+1,7,12),"Assign clerk and shift");
                var prepared=world.Capture();for(int i=0;i<prepared.actors.Length;i++){var a=prepared.actors[i];a.hunger=10;a.carriedFood=1;a.shelterId=world.HouseAt(a.x,a.y);a.shelterX=a.x;a.shelterY=a.y;a.shelterState=ShelterState.Inside;prepared.actors[i]=a;}world=World.Restore(prepared);
                world.TryFamiliarizeResidentsWithShops();
                if(scenario=="long-shopping")
                {
                    // Authored prior visits establish both locations, never remote shelf
                    // inventories. Otherwise a farther unknown shop is not a fair trial.
                    var familiar=world.Capture();var resident=familiar.actors[3];
                    resident.storeMemory=world.StoreSupplies().OrderBy(s=>s.StoreId).Select(s=>new FoodRoomVisit{roomId=s.StoreId,observedTick=0}).ToArray();familiar.actors[3]=resident;world=World.Restore(familiar);
                }
                for(int i=0;i<world.Count;i++)Check(world.TryRememberPark(i,park[0],park[1]),"Park familiarity");
                var routes=new List<object>();Action<string,int[],int[]> route=(name,start,goal)=>{var cells=PathBetween(map,start,goal);routes.Add(new{name,steps=cells.Count-1,cells});};
                for(int i=0;i<2;i++)
                {
                    var home=Anchor(homes[i],"living-area");var sales=Anchor(stores[i],"sales-floor");route("home-"+i+" to own store",home,sales);route("DC to store-"+i,dc,sales);route("home-"+i+" to park",home,park);
                    var layout=stores[i].GetProperty("interior_layout");var origin=Point(layout.GetProperty("origin_cells"));
                    foreach(var fixture in layout.GetProperty("blueprint").GetProperty("fixtures").EnumerateArray())foreach(var approach in fixture.GetProperty("approach_cells").EnumerateArray()){var p=Point(approach);route("store-"+i+" "+fixture.GetProperty("id").GetString()+" approach",sales,new[]{p[0]+origin[0],p[1]+origin[1]});}
                    var phone=Point(layout.GetProperty("phone_cells"));Check(map.PhoneAt(phone[0],phone[1]),"Phone missing");route("store-"+i+" phone approach",sales,new[]{phone[0]+1,phone[1]});
                    foreach(var door in layout.GetProperty("blueprint").GetProperty("doors").EnumerateArray()){var p=Point(door.GetProperty("cell"));route("store-"+i+" "+door.GetProperty("id").GetString(),home,new[]{p[0]+origin[0],p[1]+origin[1]});}
                    var pantry=Anchor(homes[i],"household-storage");route("home-"+i+" pantry room",home,pantry);
                }
                for(int h=0;h<homes.Length;h++)
                {
                    var origin=Point(homes[h].GetProperty("interior_layout").GetProperty("origin_cells"));
                    var rack=homes[h].GetProperty("interior_layout").GetProperty("blueprint").GetProperty("fixtures").EnumerateArray().First(f=>f.GetProperty("role").GetString()=="storage-shelf");
                    var p=Point(rack.GetProperty("approach_cells")[0]);route("home-"+h+" actual pantry approach",Anchor(homes[h],"living-area"),new[]{origin[0]+p[0],origin[1]+p[1]});
                }
                route("long shopping home-0 to store-1",Anchor(homes[0],"living-area"),Anchor(stores[1],"sales-floor"));route("courier home to DC",Anchor(homes[2],"living-area"),dc);route("DC stock approach",dc,new[]{dcStock[0]-1,dcStock[1]});route("courier home to park",Anchor(homes[2],"living-area"),park);
                var save=world.Capture();Check(save.tick==0&&world.CourierClockHour==7,"Unintended simulation advancement");Check(TotalFood(save)==203,"Finite food not equal between cases");Check(scenario!="long-shopping"||world.ActorAt(3).storeMemory.Length==2,"Long-trip resident lacks both known shop locations");
                Check(world.StoreSupplies().Length==2&&world.StoreSupplies().All(s=>s.Shelves==6&&s.Clerks==1),"Stores lost shelves/staff");Check(world.DistributionCenters().Length==1&&world.DistributionCenters()[0].Couriers==1,"DC lost courier");
                string stem="store-validation-"+seed+"-"+scenario,json=Documents.Encode(save);Check(Documents.Encode(Documents.ParseSave(json).Capture())==json,"Assigned save roundtrip");string mapJson=Documents.Encode(save.map);Check(Documents.Encode(Documents.ParseMap(mapJson).Capture())==mapJson,"Map roundtrip");
                Documents.Write(Path.Combine(output,stem+".obsave"),json);Documents.Write(Path.Combine(output,stem+".obmap2"),mapJson);
                var assignments=new List<object>();for(int i=0;i<world.Count;i++){var a=world.ActorAt(i);assignments.Add(new{index=i,id=a.id,role=world.AssignedOccupation(i).ToString(),homeId=a.assignedHouseId,workStoreId=a.clerkStoreId,distributionCenterId=a.distributionCenterId,spawn=new[]{a.x,a.y},shift=i<3?"07:00-19:00":"none",health=a.health,hunger=a.hunger,energySpent=a.energySpent,carriedFood=a.carriedFood,familiarStoreId=a.familiarStoreId,knownStoreLocations=a.storeMemory});}
                allCases.Add(new{scenario,seed,tick=save.tick,clockHour=world.CourierClockHour,totalFood=TotalFood(save),assignments,routes,storeOverviews=world.StoreSupplies(),distributionCenters=world.DistributionCenters(),stockBoxes=save.map.storageBoxes,shelves=save.map.shelves});
            }
            File.WriteAllText(Path.Combine(output,"assignments-inventories-routes.json"),JsonSerializer.Serialize(new{npcSource="0aa8ec34aa168e4797bfcd672053755dcdfaa034",worldSource="19e18e74e54db7bb13a888a8ae9090c832af9bbf",cases=allCases},new JsonSerializerOptions{WriteIndented=true,IncludeFields=true}));
            Console.WriteLine("PASS "+assertions+" fixture assertions; 4 cases; 5 assigned NPCs/case; 203 finite meals/case; exact map/save roundtrips; no NPC decision ticks.");return 0;
        }
    }
}
