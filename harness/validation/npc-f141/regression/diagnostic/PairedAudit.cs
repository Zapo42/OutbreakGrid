// Observation-only integration diagnostic; source/game behavior is not modified or corrected by this program.
using System;
using System.IO;
using System.Text;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static class PairedAudit
{
 static string OrderState(World w){var s=new StringBuilder();foreach(var o in w.PendingSupplyOrders())s.Append(o.id+":"+o.deliveredMeals+"/"+o.requestedMeals+"@");foreach(var o in w.CompletedSupplyOrders())s.Append(o.id+":"+o.deliveredMeals+"/"+o.requestedMeals+"@");return s.ToString();}
 static int Credit(World w){int n=0;foreach(var o in w.PendingSupplyOrders())n+=o.deliveredMeals;foreach(var o in w.CompletedSupplyOrders())n+=o.deliveredMeals;return n;}
 public static int Main(string[] args){string root=args[0];Directory.CreateDirectory(root);World seed=CitySetup.Create(214,4,2,2,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);string initial=Documents.Encode(seed.Capture());File.WriteAllText(Path.Combine(root,"initial107.obsave2"),initial);
 foreach(string mode in new[]{"106","107"}){
 var saved=Documents.ParseSave(initial).Capture();if(mode=="106")saved.rulesVersion=EncounterRules.SocialFamiliarityVersion;var w=World.Restore(saved);File.WriteAllText(Path.Combine(root,"initial"+mode+".obsave2"),Documents.Encode(w.Capture()));
 using var csv=new StreamWriter(Path.Combine(root,"courier"+mode+".csv"));using var events=new StreamWriter(Path.Combine(root,"events"+mode+".csv"));
 csv.WriteLine("tick,id,x,y,house,hunger,energy,health,hand,phase,order,sourceBox,destBox,commute,shelterState,shelterId,exitPhase,returningHome,shopping,clerkShelf,restockUntil,credited,centerStock,status");
 events.WriteLine("tick,id,event,x,y,hungerBefore,hungerAfter,energyBefore,energyAfter,healthBefore,healthAfter,handBefore,handAfter,phaseBefore,phaseAfter,orderBefore,orderAfter,credited");
 int credit=0;for(int t=0;t<14400;t++){var before=w.Capture().actors;w.Step();int nextCredit=Credit(w);for(int i=0;i<w.Count;i++){var a=w.ActorAt(i);var b=before[i];string ev="";if(a.hunger<b.hunger)ev+="meal|";if(a.carriedFood>b.carriedFood)ev+="pickup|";if(a.carriedFood<b.carriedFood)ev+="food-out|";if(a.health<b.health)ev+="damage|";if(a.restockPhase!=b.restockPhase)ev+="phase|";if(a.courierOrderId!=b.courierOrderId)ev+="claim|";if(nextCredit!=credit&&a.distributionCenterId!=0)ev+="credit|";if(a.workerCommuteId!=b.workerCommuteId)ev+="commute|";
 if(ev!="")events.WriteLine(string.Join(",",w.Tick,a.id,ev,a.x,a.y,b.hunger,a.hunger,100-b.energySpent,100-a.energySpent,b.health,a.health,b.carriedFood,a.carriedFood,b.restockPhase,a.restockPhase,b.courierOrderId,a.courierOrderId,nextCredit));
 if(a.distributionCenterId!=0)csv.WriteLine(string.Join(",",w.Tick,a.id,a.x,a.y,w.HouseAt(a.x,a.y),a.hunger,100-a.energySpent,a.health,a.carriedFood,a.restockPhase,a.courierOrderId,a.courierSourceBox,a.restockBox,a.workerCommuteId,a.shelterState,a.shelterId,a.shelterExitPhase,a.courierReturningHome,a.shoppingMeals,a.clerkShelf,a.restockUntilTick,nextCredit,w.DistributionCenters()[0].StockMeals,w.WorkerStatus(i)));
 }credit=nextCredit;}
 string final=Documents.Encode(w.Capture());File.WriteAllText(Path.Combine(root,"final"+mode+".obsave2"),final);int deaths=0;for(int i=0;i<w.Count;i++)if(w.ActorAt(i).health<=0)deaths++;
 Console.WriteLine("rules="+mode+" credits="+credit+" deaths="+deaths+" stock="+w.DistributionCenters()[0].StockMeals+" orders="+OrderState(w));
 }
 return 0;}
}
