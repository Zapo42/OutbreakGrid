using System;
using System.IO;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
namespace Outbreak.Diagnostics
{
    // Observe real ticks without providing hidden knowledge, stock, free energy or corrective movement.
    internal static class NpcDailyRoutineChecks
    {
        private sealed class Trace
        {
            public int Moves,Meals,HomeReturns,BoundaryChanges,RapidChanges,LongestActiveWait,ActiveWait;
            public string FirstDamage="none",Death="none";
            public long LastBoundary=-1000;public bool Work,Leisure,Shopping,Left,ReturnedFromLeisure;
        }
        private static string State(World world){return Documents.Encode(world.Capture());}
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static int HomeFood(World world,int house)
        {
            int food=0;foreach(var box in world.StorageBoxSites())if(world.HouseAt(box.x,box.y)==house)food+=StorageRules.Count(box,InventoryItem.Food);return food;
        }
        private static bool Traveling(ActorState a)
        {
            return a.workerCommuteId!=0 || a.shoppingStoreId!=0 || a.restockPhase!=RestockPhase.None || a.leisurePhase==HumanLeisurePhase.Outbound || a.leisurePhase==HumanLeisurePhase.Returning;
        }
        private static string TaskKey(ActorState a)
        {return a.workerCommuteId+":"+a.shoppingStoreId+":"+a.restockPhase+":"+a.leisurePhase;}
        private static void Run(string name,World world,int ticks,string directory,StringBuilder report,string control=null)
        {
            int count=world.Count,startingFood=world.FoodSupplies().RemainingMeals,eaten=0;var previous=world.Capture().actors;var trace=new Trace[count];for(int i=0;i<count;i++)trace[i]=new Trace();
            File.WriteAllText(Path.Combine(directory,name+"-initial.obsave2"),State(world)); // Keep the exact reproducible starting point beside the observations.
            using(var csv=new StreamWriter(Path.Combine(directory,name+".csv")))
            {
                csv.WriteLine("tick,id,x,y,house,assignedHome,assignedOccupation,effectiveOccupation,onDuty,health,hunger,energyRemaining,carriedFood,commute,shoppingStore,restockPhase,leisurePhase,shoppingResting,courierOrder,restockDeadline,restockTarget,homeShelter,shelter,foodExploreRoom");
                for(int t=0;t<ticks;t++)
                {
                    // A bounded replay window tests all saved state without doubling the entire long run.
                    if(t==ticks/2){string saved=State(world);World copy=Documents.ParseSave(saved);for(int step=0;step<20;step++)world.Step();for(int step=0;step<20;step++)copy.Step();Require(State(world)==State(copy),name+": save continuation diverged.");world=Documents.ParseSave(saved);}
                    world.Step();Require(world.Count==count,name+": diagnostic expects stable actor count.");
                    for(int i=0;i<count;i++)
                    {
                        ActorState a=world.ActorAt(i),before=previous[i];Trace r=trace[i];int house=world.HouseAt(a.x,a.y),oldHouse=world.HouseAt(before.x,before.y);bool moved=a.x!=before.x || a.y!=before.y;
                        Require(a.id==before.id && a.hunger>=0 && a.hunger<=100 && a.energySpent>=0 && a.energySpent<=100,name+": identity/needs bounds failed.");
                        // Keep first harm/death context so a compatibility replay cannot be mistaken for successful survival.
                        string context="tick="+world.Tick+" pos="+a.x+","+a.y+" house="+house+" hunger="+before.hunger+" energy="+(100-before.energySpent)+" hand="+before.carriedFood+" order="+before.courierOrderId+" restock="+before.restockPhase+" target="+before.restockBox+" commute="+before.workerCommuteId;
                        if(a.health<before.health && r.FirstDamage=="none")r.FirstDamage=context;
                        if(a.health==0 && before.health>0)r.Death=context;
                        if(moved)r.Moves++;if(a.hunger<before.hunger && a.health>0 && before.health>0){r.Meals++;eaten++;}
                        if(house!=oldHouse){r.BoundaryChanges++;if(world.Tick-r.LastBoundary<=100)r.RapidChanges++;r.LastBoundary=world.Tick;if(house==a.assignedHouseId && oldHouse!=house)r.HomeReturns++;}
                        r.Left|=house!=a.assignedHouseId;r.Work|=world.WorkerOnDuty(i) && house==(a.clerkStoreId!=0?a.clerkStoreId:a.distributionCenterId);r.Leisure|=a.leisurePhase!=HumanLeisurePhase.None;r.Shopping|=a.shoppingStoreId!=0;r.ReturnedFromLeisure|=r.Leisure && a.leisurePhase==HumanLeisurePhase.None && house==a.assignedHouseId;
                        bool waiting=a.health>0 && Traveling(a) && !a.shoppingResting && a.stress==0 && a.turnAtTick==0 && a.energySpent<=20 && !moved && a.carriedFood==before.carriedFood && TaskKey(a)==TaskKey(before);
                        r.ActiveWait=waiting?r.ActiveWait+1:0;r.LongestActiveWait=Math.Max(r.LongestActiveWait,r.ActiveWait);
                        if(t%30==0 || house!=oldHouse || a.health==0 && before.health>0)
                            csv.WriteLine(world.Tick+","+a.id+","+a.x+","+a.y+","+house+","+a.assignedHouseId+","+world.AssignedOccupation(i)+","+world.EffectiveOccupation(i)+","+world.WorkerOnDuty(i)+","+a.health+","+a.hunger+","+(100-a.energySpent)+","+a.carriedFood+","+a.workerCommuteId+","+a.shoppingStoreId+","+a.restockPhase+","+a.leisurePhase+","+a.shoppingResting+","+a.courierOrderId+","+a.restockUntilTick+","+a.restockBox+","+a.homeShelterId+","+a.shelterId+","+a.foodExploreRoomId);
                        previous[i]=a;
                    }
                }
            }
            if(control=="workers")Require(trace[0].Work && trace[1].Work && trace[0].HomeReturns>0 && trace[1].HomeReturns>0,name+": controlled workers did not attend work and return home.");
            if(control=="shopping")Require(trace[0].Shopping && trace[0].HomeReturns>0 && HomeFood(world,world.ActorAt(0).assignedHouseId)>0,name+": controlled shopper failed physical home resupply.");
            if(control=="leisure")Require(trace[0].Left && trace[0].ReturnedFromLeisure && trace[0].Moves>0,name+": controlled leisure did not physically leave and return.");
            report.AppendLine(name+": PASS observation and replay; ticks="+ticks+", actors="+count+", food start="+startingFood+", end="+world.FoodSupplies().RemainingMeals+", observed meals="+eaten);
            for(int i=0;i<count;i++){var a=world.ActorAt(i);var r=trace[i];report.AppendLine("  id="+a.id+" moves="+r.Moves+" homeReturns="+r.HomeReturns+" work="+r.Work+" shopping="+r.Shopping+" leisure="+r.Leisure+" longestActiveWait="+r.LongestActiveWait+" rapidRoomChanges="+r.RapidChanges+" finalHealth="+a.health+" hunger="+a.hunger+" energy="+(100-a.energySpent)+" firstDamage=["+r.FirstDamage+"] death=["+r.Death+"]");if(r.LongestActiveWait>=300 || r.RapidChanges>=6 || a.health<100 || a.hunger>=75)report.AppendLine("  REVIEW id="+a.id+": inspect timeline and supply/work/rest context; this flag is not proof of a navigation bug.");}
            File.WriteAllText(Path.Combine(directory,name+"-final.obsave2"),State(world));Console.WriteLine(name+" completed.");
        }
        public static int Main(string[] args)
        {
            string directory=Path.GetFullPath(args.Length>0?args[0]:"npc-daily-routine-results");Directory.CreateDirectory(directory);var report=new StringBuilder();report.AppendLine("NPC daily-routine observation — hosted core only; not native Unity or a performance certification.");report.AppendLine("CSV hunger is internal hunger burden (0 fed,100 starved); energyRemaining is100-energySpent. Review flags are heuristic, not acceptance failures. Food totals/meal counts are observations, not a conservation proof. Dead actors have needs reset by existing cleanup; hunger0 on a corpse does not mean it was fed.");
            try
            {
                if(args.Length>1)Run("cartographer-map",Documents.ParseSave(File.ReadAllText(args[1])),14400,directory,report);
                else
                {
                    Run("controlled-workers",EncounterScenarios.WorkerShiftTest(true),1000,directory,report,"workers");
                    Run("controlled-shopping",EncounterScenarios.LargeCityShoppingTest(),1800,directory,report,"shopping");
                    Run("controlled-leisure",EncounterScenarios.HumanLeisureTest(),500,directory,report,"leisure");
                    Run("generated-city-day",CitySetup.Create(214,4,2,2,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters),14400,directory,report);
                }
                report.AppendLine(args.Length>1?"Supplied map observed without stock/geometry changes; replay pass is not survival acceptance.":"Generated baseline only; use the supplied-map argument for separate cartographer observations.");File.WriteAllText(Path.Combine(directory,"DAILY_ROUTINE_REPORT.txt"),report.ToString());return 0;
            }
            catch(Exception error){report.AppendLine("FAIL: "+error);File.WriteAllText(Path.Combine(directory,"DAILY_ROUTINE_REPORT.txt"),report.ToString());Console.Error.WriteLine(error);return 1;}
        }
    }
}
