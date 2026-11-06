// Adapted from the supplied NPC-F143 read-only observer; timelines/food/credit/replay definitions retained.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static class StoreAudit
{
    private sealed class Trace
    {
        public int Id,Home,Work,InternalEntries,Withdrawn,ShelfDeposits,ShelfBought,HomeDeposited,ShoppingDeposited,Moves,Meals;
        public long OffHome=-1,NextWork=-1,FirstDamage=-1,Death=-1;
        public HashSet<int> Shelves=new HashSet<int>(),ShopChoices=new HashSet<int>();
    }
    private static string State(World w){return Documents.Encode(w.Capture());}
    private static void Require(bool yes,string why){if(!yes)throw new InvalidOperationException(why);}
    private static int Key(int x,int y,int width){return y*width+x+1;}
    private static int Food(StorageBoxDocument b){return StorageRules.Count(b,InventoryItem.Food);}
    private static Dictionary<int,SupplyOrderDocument> Orders(World w){var d=new Dictionary<int,SupplyOrderDocument>();foreach(var o in w.PendingSupplyOrders())d.Add(o.id,o);foreach(var o in w.CompletedSupplyOrders())d.Add(o.id,o);return d;}
    private static void Replay(ref World w,string stage,string output,HashSet<string> done)
    {
        if(!done.Add(stage))return;
        string before=State(w);File.WriteAllText(Path.Combine(output,"replay-"+stage+"-tick"+w.Tick+".obsave"),before);
        World copy=Documents.ParseSave(before);Require(State(copy)==before,"Checkpoint roundtrip: "+stage);
        for(int n=0;n<30;n++){w.Step();copy.Step();Require(State(w)==State(copy),"Exact continuation failed: "+stage+" step"+n);}
        w=Documents.ParseSave(before); // The observation timeline is not advanced a second time by replay checks.
    }
    // Derive physical room membership from fixture floor geometry, independently of the merged building identity.
    private static (HashSet<int> cells,HashSet<int> doors) Physical(WorldMap map,int x,int y)
    {
        var cells=new HashSet<int>();var doors=new HashSet<int>();var q=new Queue<int>();int start=y*map.Width+x;cells.Add(start);q.Enqueue(start);
        while(q.Count>0){int k=q.Dequeue();foreach(var d in new[]{(-1,0),(1,0),(0,-1),(0,1)}){int nx=k%map.Width+d.Item1,ny=k/map.Width+d.Item2;if(!map.Contains(nx,ny))continue;Terrain t=map.Get(nx,ny);int next=ny*map.Width+nx;if(t==Terrain.Floor && cells.Add(next))q.Enqueue(next);if(t==Terrain.DoorClosed || t==Terrain.DoorOpen)doors.Add(next);}}
        return(cells,doors);
    }
    private static object Run(string name,string input,string output,string rules)
    {
        Directory.CreateDirectory(output);string raw=File.ReadAllText(input);World w=Documents.ParseSave(raw);Require(State(w)==raw,"Input is not exact roundtrip");
        if(rules=="108" || rules=="109" || rules=="110" || rules=="111" || rules=="112"){var upgraded=w.Capture();upgraded.rulesVersion="encounter-prototype-"+rules;w=World.Restore(upgraded);Require(State(w)==raw.Replace("encounter-prototype-107",upgraded.rulesVersion),"Upgrade changed facts beyond selected rules");}
        var initial=w.Capture();Require(initial.rulesVersion=="encounter-prototype-"+rules && initial.map.schemaVersion==14 && w.Count==5,"Wrong baseline/input");Require(w.Tick==0 && w.FoodSupplies().RemainingMeals==203,"Wrong fixture start");
        File.WriteAllText(Path.Combine(output,"initial.obsave"),State(w));
        var map=WorldMap.Restore(initial.map);var internalDoors=new HashSet<int>();var stockFloors=new Dictionary<int,HashSet<int>>();
        foreach(var g in initial.map.buildingRooms){var rear=Array.Find(g.rooms,r=>r.role=="stockroom");if(rear==null)continue;var front=Array.Find(g.rooms,r=>r.role=="sales-floor");var a=Physical(map,rear.x,rear.y);var b=Physical(map,front.x,front.y);a.doors.IntersectWith(b.doors);internalDoors.UnionWith(a.doors);stockFloors[w.HouseAt(rear.x,rear.y)]=a.cells;}
        var traces=initial.actors.Select(a=>new Trace{Id=a.id,Home=a.assignedHouseId,Work=a.clerkStoreId!=0?a.clerkStoreId:a.distributionCenterId}).ToArray();
        var replay=new HashSet<string>();var events=new List<object>();var shelfState=w.ShelfSites().ToDictionary(s=>Key(s.x,s.y,w.Width),s=>s.food);var boxState=w.StorageBoxSites().ToDictionary(b=>Key(b.x,b.y,w.Width),Food);var orders=Orders(w);int previousFood=203,meals=0,discarded=0,unattributed=0,credited=0,sourceOut=0,sourceIn=0;var creditEvents=new List<object>();var withdrawals=new List<object>();var stocked=new List<object>();var homeDeposits=new List<object>();var shopPickups=new List<object>();
        int dcBox=initial.actors[0].courierSourceBox;long sourceFirstDelivery=-1;int sourceFirstStore=0;
        using(var csv=new StreamWriter(Path.Combine(output,"actors.csv")))using(var foodCsv=new StreamWriter(Path.Combine(output,"food.csv")))
        {
            csv.WriteLine("tick,id,x,y,house,home,work,health,hunger,energy,hand,clerkShelf,shoppingStore,shoppingMeals,phase,order,target,deadline,commute,exitPhase,storeNotes");foodCsv.WriteLine("tick,total,carried,shelves,boxes,mealsCumulative,deathDiscardsCumulative,unattributedCumulative,credited");
            for(int tick=0;tick<15600;tick++)
            {
                var before=new ActorState[w.Count];for(int i=0;i<w.Count;i++)before[i]=w.ActorAt(i);
                string preDeath=before.Any(a=>a.health==1)?State(w):null;
                w.Step();int observedMeals=0,deathFood=0;bool changed=false;
                var shelfNow=w.ShelfSites().ToDictionary(s=>Key(s.x,s.y,w.Width),s=>s.food);var boxNow=w.StorageBoxSites().ToDictionary(b=>Key(b.x,b.y,w.Width),Food);var currentOrders=Orders(w);
                foreach(var pair in currentOrders)
                {
                    int old=orders.ContainsKey(pair.Key)?orders[pair.Key].deliveredMeals:0;int delta=pair.Value.deliveredMeals-old;Require(delta>=0,"Credit decreased");
                    if(delta>0){var owner=Array.Find(before,a=>a.courierOrderId==pair.Key);Require(owner.id!=0,"Unowned delivery credit");int target=pair.Value.supplyBox;Require(boxNow[target]-boxState[target]>=delta,"Credit without physical stockroom deposit");Require(owner.carriedFood-w.ActorAt(Array.FindIndex(before,a=>a.id==owner.id)).carriedFood>=delta,"Credit not backed by carried stock");Require(Math.Abs(owner.x-(target-1)%w.Width)+Math.Abs(owner.y-(target-1)/w.Width)==1,"Remote delivery");credited+=delta;var ev=new{tick=w.Tick,id=owner.id,order=pair.Key,store=pair.Value.storeId,box=target,meals=delta};creditEvents.Add(ev);events.Add(ev);if(sourceFirstDelivery<0){sourceFirstDelivery=w.Tick;sourceFirstStore=pair.Value.storeId;}changed=true;}
                }
                foreach(int id in orders.Keys)Require(currentOrders.ContainsKey(id),"Order history pruned; audit incomplete");
                for(int i=0;i<w.Count;i++)
                {
                    var a=w.ActorAt(i);var b=before[i];var tr=traces[i];Require(a.id==tr.Id,"Stable ID changed");bool moved=a.x!=b.x || a.y!=b.y;
                    if(moved){tr.Moves++;if(internalDoors.Contains(a.y*w.Width+a.x)){tr.InternalEntries++;events.Add(new{tick=w.Tick,id=a.id,kind="internal-door-entry",x=a.x,y=a.y});}}
                    if(a.health>0 && b.health>0 && a.hunger<b.hunger){observedMeals++;tr.Meals++;}
                    if(a.health<b.health && tr.FirstDamage<0){tr.FirstDamage=w.Tick;events.Add(new{tick=w.Tick,id=a.id,kind="first-damage",x=a.x,y=a.y,hungerBefore=b.hunger,energyBefore=100-b.energySpent,hand=b.carriedFood,phase=b.restockPhase.ToString(),order=b.courierOrderId,target=b.restockBox,commute=b.workerCommuteId});}
                    if(a.health==0 && b.health>0){tr.Death=w.Tick;deathFood+=b.carriedFood;Require(preDeath!=null,"No exact pre-death checkpoint");File.WriteAllText(Path.Combine(output,"pre-death-id"+a.id+"-tick"+(w.Tick-1)+".obsave"),preDeath);File.WriteAllText(Path.Combine(output,"death-id"+a.id+"-supplies.json"),JsonSerializer.Serialize(new{tick=w.Tick,actorBefore=b,remainingBoxes=w.StorageBoxSites(),remainingShelves=w.ShelfSites()},new JsonSerializerOptions{IncludeFields=true,WriteIndented=true}));}
                    if(w.Tick>=7200 && w.Tick<14400 && a.health>0 && w.HouseAt(a.x,a.y)==tr.Home && tr.OffHome<0)tr.OffHome=w.Tick;
                    if(w.Tick>=14400 && a.health>0 && tr.Work!=0 && w.WorkerOnDuty(i) && w.HouseAt(a.x,a.y)==tr.Work && tr.NextWork<0)tr.NextWork=w.Tick;
                    if(a.shoppingStoreId!=0)tr.ShopChoices.Add(a.shoppingStoreId);
                    string oldNotes=string.Join(";",(b.storeMemory??new FoodRoomVisit[0]).Select(n=>n.roomId+":"+n.observedTick));string notes=string.Join(";",(a.storeMemory??new FoodRoomVisit[0]).Select(n=>n.roomId+":"+n.observedTick));
                    if(a.id==4 && oldNotes!=notes)events.Add(new{tick=w.Tick,id=a.id,kind="store-notes-changed",before=oldNotes,after=notes});
                    foreach(var s in shelfNow)
                    {
                        int delta=s.Value-shelfState[s.Key],x=(s.Key-1)%w.Width,y=(s.Key-1)/w.Width;bool adjacent=Math.Abs(b.x-x)+Math.Abs(b.y-y)==1;
                        if(delta>0 && b.clerkStoreId!=0 && w.HouseAt(x,y)==b.clerkStoreId && b.carriedFood-a.carriedFood>=delta && adjacent){tr.ShelfDeposits+=delta;tr.Shelves.Add(s.Key);var ev=new{tick=w.Tick,id=a.id,shelf=s.Key,x,y,meals=delta};stocked.Add(ev);changed=true;}
                        if(delta<0 && a.carriedFood-b.carriedFood>=-delta && adjacent){tr.ShelfBought-=delta;var ev=new{tick=w.Tick,id=a.id,shelf=s.Key,store=w.HouseAt(x,y),meals=-delta,shoppingStore=b.shoppingStoreId};shopPickups.Add(ev);changed=true;}
                    }
                    foreach(var box in boxNow)
                    {
                        int delta=box.Value-boxState[box.Key],x=(box.Key-1)%w.Width,y=(box.Key-1)/w.Width;bool adjacent=Math.Abs(b.x-x)+Math.Abs(b.y-y)==1;
                        if(delta<0 && a.carriedFood-b.carriedFood>=-delta && adjacent){if(b.clerkStoreId!=0 && w.HouseAt(x,y)==b.clerkStoreId)tr.Withdrawn-=delta;withdrawals.Add(new{tick=w.Tick,id=a.id,box=box.Key,x,y,meals=-delta,stockroom=stockFloors.ContainsKey(b.clerkStoreId)&&stockFloors[b.clerkStoreId].Contains(b.y*w.Width+b.x)});changed=true;}
                        if(delta>0 && b.carriedFood-a.carriedFood>=delta && adjacent && w.HouseAt(x,y)==tr.Home){tr.HomeDeposited+=delta;bool shoppingReturn=b.shoppingMeals>0 && b.restockPhase==RestockPhase.Returning;if(shoppingReturn)tr.ShoppingDeposited+=delta;homeDeposits.Add(new{tick=w.Tick,id=a.id,box=box.Key,x,y,meals=delta,home=tr.Home,shoppingReturn,shoppingStore=b.shoppingStoreId});changed=true;}
                    }
                    if(w.Tick%25==0 || moved && (w.HouseAt(a.x,a.y)!=w.HouseAt(b.x,b.y) || internalDoors.Contains(a.y*w.Width+a.x)) || a.health==0 && b.health>0)
                        csv.WriteLine(string.Join(",",w.Tick,a.id,a.x,a.y,w.HouseAt(a.x,a.y),tr.Home,tr.Work,a.health,a.hunger,100-a.energySpent,a.carriedFood,a.clerkShelf,a.shoppingStoreId,a.shoppingMeals,a.restockPhase,a.courierOrderId,a.restockBox,a.restockUntilTick,a.workerCommuteId,a.shelterExitPhase,notes));
                }
                int dcDelta=boxNow[dcBox]-boxState[dcBox];if(dcDelta<0)sourceOut-=dcDelta;else sourceIn+=dcDelta;
                int remaining=w.FoodSupplies().RemainingMeals,loss=previousFood-remaining;Require(loss>=0,"Global finite food created");meals+=observedMeals;discarded+=deathFood;unattributed+=loss-observedMeals-deathFood;Require(unattributed==0,"Food loss not explained by physical meals/death discards at"+w.Tick);
                if(changed || w.Tick%30==0){var f=w.FoodSupplies();foodCsv.WriteLine(string.Join(",",w.Tick,remaining,f.CarriedMeals,f.ShelfMeals,f.StorageMeals,meals,discarded,unattributed,credited));}
                shelfState=shelfNow;boxState=boxNow;orders=currentOrders;previousFood=remaining;
                if(w.Tick==300 || w.Tick==1000 || w.Tick==7200 || w.Tick==14400)File.WriteAllText(Path.Combine(output,"tick"+w.Tick+".obsave"),State(w));
                if(w.Tick==300 || w.Tick==7200 || w.Tick==14400)Replay(ref w,"periodic-"+w.Tick,output,replay);
                if(Enumerable.Range(0,w.Count).Any(i=>{var a=w.ActorAt(i);return a.clerkShelf!=0 && a.carriedFood>1;}))Replay(ref w,"clerk-stock-handling",output,replay);
                if(Enumerable.Range(0,w.Count).Any(i=>w.ActorAt(i).shoppingMeals>0))Replay(ref w,"shopping",output,replay);
                if(w.ActorAt(0).courierOrderId!=0 && w.HouseAt(w.ActorAt(0).x,w.ActorAt(0).y)==0)Replay(ref w,"courier-travel",output,replay);
                if(Enumerable.Range(0,w.Count).Any(i=>internalDoors.Contains(w.ActorAt(i).y*w.Width+w.ActorAt(i).x)))Replay(ref w,"internal-door",output,replay);
            }
        }
        string final=State(w);Require(State(Documents.ParseSave(final))==final,"Final roundtrip failed");Require(203==w.FoodSupplies().RemainingMeals+meals+discarded,"Final food ledger");File.WriteAllText(Path.Combine(output,"final.obsave"),final);
        var result=new{name,source="NPC-SHOPPING-11 fixed-input observation; rules"+rules,start=0,end=w.Tick,finalHour=w.CourierClockHour,internalDoorCoordinates=internalDoors.Select(k=>new{k,x=k%w.Width,y=k/w.Width}).ToArray(),actors=traces.Select((t,i)=>new{t.Id,t.Home,t.Work,t.InternalEntries,t.Withdrawn,t.ShelfDeposits,DistinctStockedShelves=t.Shelves.ToArray(),t.ShelfBought,t.HomeDeposited,t.ShoppingDeposited,t.Moves,t.Meals,t.OffHome,t.NextWork,t.FirstDamage,t.Death,ShopChoices=t.ShopChoices.ToArray(),finalHealth=w.ActorAt(i).health,hunger=w.ActorAt(i).hunger,energy=100-w.ActorAt(i).energySpent}).ToArray(),initialFood=203,remainingFood=w.FoodSupplies().RemainingMeals,meals,discarded,unattributed,credited,sourceOut,sourceIn,sourceFinal=boxState[dcBox],sourceFirstDelivery,sourceFirstStore,replayStages=replay.ToArray(),stocked,withdrawals,shopPickups,homeDeposits,creditEvents,events};
        File.WriteAllText(Path.Combine(output,"RESULTS.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{IncludeFields=true,WriteIndented=true}));Console.WriteLine(name+": ticks="+w.Tick+" remaining="+w.FoodSupplies().RemainingMeals+" meals="+meals+" credits="+credited+" deaths="+traces.Count(t=>t.Death>=0)+" replayStages="+replay.Count);return result;
    }
    public static int Main(string[] args)
    {
        try{Directory.CreateDirectory(args[1]);foreach(string name in (args.Length>3?new[]{args[3]}:new[]{"restocking","shopping","delivery","long-shopping"}))Run(name,Path.Combine(args[0],"store-validation-214-"+name+".obsave"),Path.Combine(args[1],name),args.Length>2?args[2]:"108");Console.WriteLine("Observation/accounting/replay complete. Behavioral outcomes require review; native Unity NOT RUN.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
