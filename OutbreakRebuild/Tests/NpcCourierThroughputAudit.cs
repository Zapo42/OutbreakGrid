using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
namespace Outbreak.Diagnostics
{
    internal static class NpcCourierThroughputAudit
    {
        private sealed class Stats
        {
            public readonly SortedDictionary<string,int> Time=new SortedDictionary<string,int>(),ScheduledTime=new SortedDictionary<string,int>(),MovingTime=new SortedDictionary<string,int>();
            public int Starts,PickupEvents,SourcePickupMeals,DeliveryEvents,Delivered,MealEvents,Moves,HungryTicks,LowEnergyTicks,LoadedHungryTicks;
        }
        private static readonly StringBuilder Report=new StringBuilder();
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static string State(World world){return Documents.Encode(world.Capture());}
        private static Dictionary<int,SupplyOrderDocument> Orders(World w)
        {
            var result=new Dictionary<int,SupplyOrderDocument>();foreach(var o in w.PendingSupplyOrders())result.Add(o.id,o);foreach(var o in w.CompletedSupplyOrders())result.Add(o.id,o);return result;
        }
        private static string Phase(World w,int index,ActorState a)
        {
            if(a.health<=0)return "dead";if(a.shoppingMeals>0)return "personal-shopping";
            if(a.restockPhase==RestockPhase.Collecting)return "collecting";
            if(a.restockPhase==RestockPhase.Returning)return "returning-delivery";
            if(a.workerCommuteId!=0)return "work-home-commute";
            return w.CourierOnDuty(index)?"on-duty-no-delivery":"off-duty-no-delivery";
        }
        private static void Run(string name,World w,int ticks,string output)
        {
            var stats=new SortedDictionary<int,Stats>();for(int i=0;i<w.Count;i++)if(w.ActorAt(i).distributionCenterId!=0)stats.Add(w.ActorAt(i).id,new Stats());
            var previous=w.Capture().actors;var known=Orders(w);var requested=new Dictionary<int,int>();var deliveredByOrder=new Dictionary<int,int>();int credited=0,sourceOutflow=0,sourceInflow=0;long firstCredit=-1,lastCredit=-1;var initialCenters=w.DistributionCenters();
            File.WriteAllText(Path.Combine(output,name+"-initial.obsave2"),State(w));
            using(var csv=new StreamWriter(Path.Combine(output,name+"-events.csv")))
            {
                csv.WriteLine("tick,event,courierId,orderId,storeId,meals,hunger,energyRemaining,carriedFood");
                for(int t=0;t<ticks;t++)
                {
                    if(t==ticks/2){var save=w.Capture();World copy=World.Restore(save);for(int n=0;n<20;n++){w.Step();copy.Step();}Require(State(w)==State(copy),name+": replay diverged.");w=World.Restore(save);}
                    var sourceBefore=new Dictionary<int,int>();foreach(var c in w.DistributionCenters())sourceBefore[c.SupplyBox]=c.StockMeals;
                    w.Step();foreach(var c in w.DistributionCenters()){int change=c.StockMeals-sourceBefore[c.SupplyBox];if(change<0)sourceOutflow-=change;else sourceInflow+=change;}var currentOrders=Orders(w);
                    foreach(var item in currentOrders)
                    {
                        var order=item.Value;if(!requested.ContainsKey(order.id)){requested.Add(order.id,order.requestedMeals);deliveredByOrder.Add(order.id,0);csv.WriteLine(w.Tick+",order-observed,0,"+order.id+","+order.storeId+",0,,,");}
                        int old=known.ContainsKey(order.id)?known[order.id].deliveredMeals:0,delta=order.deliveredMeals-old;Require(delta>=0,name+": order credit decreased.");
                        if(delta>0)
                        {
                            credited+=delta;deliveredByOrder[order.id]+=delta;if(firstCredit<0)firstCredit=w.Tick;lastCredit=w.Tick;int owner=0;
                            foreach(var before in previous)if(before.courierOrderId==order.id && stats.ContainsKey(before.id)){owner=before.id;break;}
                            Require(owner!=0,name+": un-attributed credit; audit cannot infer its courier.");stats[owner].DeliveryEvents++;stats[owner].Delivered+=delta;
                            csv.WriteLine(w.Tick+",credited-delivery,"+owner+","+order.id+","+order.storeId+","+delta+",,,");
                        }
                    }
                    // Fixtures retain fewer than the64 completed-order history limit; refuse silently incomplete accounting.
                    foreach(var item in known)Require(currentOrders.ContainsKey(item.Key),name+": order removed/pruned; accounting requires another event source.");
                    for(int i=0;i<w.Count;i++)
                    {
                        var a=w.ActorAt(i);var before=previous[i];if(!stats.ContainsKey(a.id))continue;var s=stats[a.id];string phase=Phase(w,i,a);if(!s.Time.ContainsKey(phase))s.Time[phase]=0;s.Time[phase]++;
                        bool moved=a.x!=before.x || a.y!=before.y;if(!s.ScheduledTime.ContainsKey(phase)){s.ScheduledTime[phase]=0;s.MovingTime[phase]=0;}if(w.CourierOnDuty(i))s.ScheduledTime[phase]++;if(moved){s.Moves++;s.MovingTime[phase]++;}if(a.hunger>=40)s.HungryTicks++;if(a.energySpent>=60)s.LowEnergyTicks++;
                        if(a.carriedFood>0 && a.hunger>=40 && a.restockPhase!=RestockPhase.None)s.LoadedHungryTicks++;
                        if(a.health>0 && before.health>0 && a.hunger<before.hunger)s.MealEvents++;
                        if(a.courierOrderId!=0 && (a.courierOrderId!=before.courierOrderId || a.restockUntilTick>before.restockUntilTick)){s.Starts++;csv.WriteLine(w.Tick+",trip-claim,"+a.id+","+a.courierOrderId+",0,0,"+a.hunger+","+(100-a.energySpent)+","+a.carriedFood);}
                        int key=before.courierSourceBox-1;
                        if(key>=0 && a.carriedFood>before.carriedFood && Math.Abs(before.x-key%w.Width)+Math.Abs(before.y-key/w.Width)==1 && sourceBefore.ContainsKey(key+1))
                        {
                            var box=w.StorageBoxAt(key%w.Width,key/w.Width);int delta=a.carriedFood-before.carriedFood;
                            Require(sourceBefore[key+1]-StorageRules.Count(box,InventoryItem.Food)>=delta,name+": source pickup not physically supported.");s.PickupEvents++;s.SourcePickupMeals+=delta;
                            csv.WriteLine(w.Tick+",source-pickup,"+a.id+","+a.courierOrderId+",0,"+delta+","+a.hunger+","+(100-a.energySpent)+","+a.carriedFood);
                        }
                        if(phase!=PhaseForBefore(before) || a.courierOrderId!=before.courierOrderId || t%120==0)csv.WriteLine(w.Tick+",phase-"+phase+","+a.id+","+a.courierOrderId+",0,0,"+a.hunger+","+(100-a.energySpent)+","+a.carriedFood);
                    }
                    previous=w.Capture().actors;known=currentOrders;
                }
            }
            int outstanding=0;foreach(var order in w.PendingSupplyOrders())outstanding+=order.requestedMeals-order.deliveredMeals;
            Report.AppendLine(name+": observedTicks="+ticks+", creditedMeals="+credited+", completedOrders="+w.CompletedSupplyOrders().Length+", outstandingMeals="+outstanding+", firstCredit="+firstCredit+", lastCredit="+lastCredit);
            foreach(var c in initialCenters)Report.AppendLine("  initial center="+c.CenterId+" stock="+c.StockMeals+" dispatchable="+c.AvailableMeals);
            int initialCenterStock=0,finalCenterStock=0;foreach(var c in initialCenters)initialCenterStock+=c.StockMeals;foreach(var c in w.DistributionCenters()){finalCenterStock+=c.StockMeals;Report.AppendLine("  final center="+c.CenterId+" stock="+c.StockMeals+" dispatchable="+c.AvailableMeals);}Require(initialCenterStock+sourceInflow-sourceOutflow==finalCenterStock,name+": center stock flow mismatch.");Report.AppendLine("  observed per-tick center outflow="+sourceOutflow+" inflow="+sourceInflow+" netOutflow="+(sourceOutflow-sourceInflow));
            foreach(var order in requested)Report.AppendLine("  order="+order.Key+" requested="+order.Value+" creditedDuringRun="+deliveredByOrder[order.Key]);
            foreach(var pair in stats)
            {
                var s=pair.Value;int total=0;Report.AppendLine("  courier="+pair.Key+" claims="+s.Starts+" sourcePickups="+s.PickupEvents+" sourcePickupMeals="+s.SourcePickupMeals+" creditedDeliveryEvents="+s.DeliveryEvents+" delivered="+s.Delivered+" observedMealEvents="+s.MealEvents+" moves="+s.Moves+" hungerAtLeast40Ticks="+s.HungryTicks+" energyAtMost40Ticks="+s.LowEnergyTicks+" loadedHungryTaskTicks="+s.LoadedHungryTicks);
                foreach(var time in s.Time){total+=time.Value;Report.AppendLine("    "+time.Key+" ticks="+time.Value+" scheduledWindowTicks="+s.ScheduledTime[time.Key]+" movedPositionTicks="+s.MovingTime[time.Key]+" stationaryTicks="+(time.Value-s.MovingTime[time.Key]));}Require(total==ticks,name+": phase times do not partition the run.");
            }
            if(name!="generated-day")Require(credited>0 && w.CompletedSupplyOrders().Length>0,name+": control did not complete an order.");
            if(name=="generated-day")Require(credited==7 && outstanding==17 && w.DistributionCenters()[0].StockMeals==20,"F139 throughput baseline changed.");
            File.WriteAllText(Path.Combine(output,name+"-final.obsave2"),State(w));Console.WriteLine(name+" completed: "+credited+" credited meals.");
        }
        private static string PhaseForBefore(ActorState a)
        {return a.shoppingMeals>0?"personal-shopping":a.restockPhase==RestockPhase.Collecting?"collecting":a.restockPhase==RestockPhase.Returning?"returning-delivery":"other";}
        public static int Main(string[] args)
        {
            string output=Path.GetFullPath(args.Length>0?args[0]:"npc-courier-throughput");Directory.CreateDirectory(output);
            Report.AppendLine("NPC-F140 observational audit. Phase totals partition simulated ticks; hunger/energy flags overlap phases. Pickup includes personal reserve, so it is not delivery credit. Meal events are hunger decreases, not a full food-flow attribution. No gameplay intervention. Native Unity NOT RUN.");
            try
            {
                Run("generated-day",CitySetup.Create(214,4,2,2,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters),14400,output);
                Run("short-route-control",EncounterScenarios.CourierFulfillmentTest(),2400,output);
                Run("larger-district-control",EncounterScenarios.LargerDistrictTrial(),2400,output);
                Report.AppendLine("Audit accounting/replay checks PASS; does not certify healthy NPCs, pathfinding performance, or show that adding couriers alone fixes the generated city.");
                File.WriteAllText(Path.Combine(output,"COURIER_AUDIT.txt"),Report.ToString());return 0;
            }
            catch(Exception error){Report.AppendLine("FAIL: "+error);File.WriteAllText(Path.Combine(output,"COURIER_AUDIT.txt"),Report.ToString());Console.Error.WriteLine(error);return 1;}
        }
    }
}
