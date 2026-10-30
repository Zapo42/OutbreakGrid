using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static class StoreLayoutChecks
{
    private static int assertions;
    private static void Require(bool condition,string message){assertions++;if(!condition)throw new InvalidOperationException(message);}
    private static bool InteriorRoute(WorldMap map,int left,int top,int width,int height,int ax,int ay,int bx,int by)
    {
        // Test physical cardinal circulation through doors while respecting solid furniture.
        var q=new Queue<int>();var seen=new HashSet<int>();int start=ay*map.Width+ax,goal=by*map.Width+bx;q.Enqueue(start);seen.Add(start);
        while(q.Count>0){int c=q.Dequeue();if(c==goal)return true;foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=c%map.Width+d.Item1,y=c/map.Width+d.Item2,key=y*map.Width+x;if(x<left||y<top||x>=left+width||y>=top+height||seen.Contains(key))continue;Terrain t=map.Get(x,y);if(t!=Terrain.DoorClosed && !map.CanWalk(x,y))continue;seen.Add(key);q.Enqueue(key);}}return false;
    }
    public static int Main(string[] args)
    {
        int maps=0,stores=0;
        foreach(string path in Directory.GetFiles(args[0],"*.obmap2"))
        {
            WorldMap map=Documents.ParseMap(File.ReadAllText(path));MapDocument original=map.Capture();
            string encoded=Documents.Encode(original);Require(Documents.Encode(Documents.ParseMap(encoded).Capture())==encoded,"Map roundtrip");
            // Supply finite inspection stock without changing generated zero-food maps.
            using(JsonDocument metadata=JsonDocument.Parse(map.AuthoringJson))
            {
                foreach(var b in metadata.RootElement.GetProperty("buildings").EnumerateArray())
                {
                    if(!b.TryGetProperty("interior_layout",out var layout)||!layout.TryGetProperty("sales_shelves_materialized",out var live)||!live.GetBoolean())continue;
                    var stock=layout.GetProperty("stock_box_cells");StorageBoxDocument box=map.StorageBoxAt(stock[0].GetInt32(),stock[1].GetInt32());
                    Require(box!=null && box.items.Length==0,"Generated stock not empty");box.items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=24}};map.SetStorageBox(box);
                }
                World world=new World(map,metadata.RootElement.GetProperty("seed").GetUInt32(),encounter:true,hunger:true);
                foreach(var b in metadata.RootElement.GetProperty("buildings").EnumerateArray())
                {
                    if(!b.TryGetProperty("interior_layout",out var layout)||!layout.TryGetProperty("sales_shelves_materialized",out var live)||!live.GetBoolean())continue;
                    stores++;int left=b.GetProperty("left").GetInt32(),top=b.GetProperty("top").GetInt32(),width=b.GetProperty("width").GetInt32(),height=b.GetProperty("height").GetInt32();
                    var blueprint=layout.GetProperty("blueprint");var front=blueprint.GetProperty("rooms")[0].GetProperty("cells")[0];int fx=left+front[0].GetInt32(),fy=top+front[1].GetInt32();int id=world.HouseAt(fx,fy);
                    var stock=layout.GetProperty("stock_box_cells");int sx=stock[0].GetInt32(),sy=stock[1].GetInt32();
                    Require(world.HouseAt(sx,sy)==id && id!=0,"Stockroom is a separate store identity");Require(world.TryConfigureStore(sx,sy),"Existing store contract rejected stockroom box");
                    int actorId,index=world.Count;Require(world.TrySpawn(Species.Human,fx,fy,out actorId),"Sales floor clerk spawn");Require(world.TryAssignClerk(index,id),"Existing clerk assignment rejected grouped store");
                    int shelves=0;
                    foreach(var shelf in world.ShelfSites())if(world.HouseAt(shelf.x,shelf.y)==id){shelves++;Require(!map.CanWalk(shelf.x,shelf.y),"Shelf not solid");Require(shelf.food==0,"Generation invented shelf food");}
                    Require(shelves==(blueprint.GetProperty("variant").GetString()=="compact"?6:8),"Wrong sales shelf count");
                    foreach(var fixture in blueprint.GetProperty("fixtures").EnumerateArray())foreach(var approach in fixture.GetProperty("approach_cells").EnumerateArray())
                    {int x=left+approach[0].GetInt32(),y=top+approach[1].GetInt32();Require(InteriorRoute(map,left,top,width,height,fx,fy,x,y),"Clerk cannot physically approach shelf/stock");}
                    var phone=layout.GetProperty("phone_cells");int px=phone[0].GetInt32(),py=phone[1].GetInt32();Require(map.PhoneAt(px,py)&&!map.CanWalk(px,py),"Missing solid phone");
                    foreach(var door in blueprint.GetProperty("doors").EnumerateArray())
                    {int x=left+door.GetProperty("cell")[0].GetInt32(),y=top+door.GetProperty("cell")[1].GetInt32();Require(InteriorRoute(map,left,top,width,height,fx,fy,x,y),"Entrance or stockroom door unreachable");Require(door.GetProperty("external").GetBoolean()?world.HouseAt(x,y)==0:world.HouseAt(x,y)==id,"Door identity incorrect");}
                }
                foreach(var row in world.StoreSupplies())Require(row.Shelves>=6 && row.CrateMeals==24 && row.ShelfMeals==0 && row.Clerks==1,"Existing store overview loses fixtures or staffing");
                string save=Documents.Encode(world.Capture());Require(Documents.Encode(Documents.ParseSave(save).Capture())==save,"Configured store/clerk save roundtrip");
                Documents.Write(Path.ChangeExtension(path,"obsave"),save);
            }
            maps++;
        }
        Require(maps>=3 && stores>=3,"Expected representative store maps");Console.WriteLine("PASS "+assertions+" store geometry/contract assertions across "+maps+" maps, "+stores+" stores; configured inspection saves exported; no NPC decision ticks.");return 0;
    }
}
