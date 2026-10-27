using System;
using System.Collections.Generic;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckMultiStoreCases()
    {
        Check("Larger district serves three separate homes and near/far stores without creating food",()=>{
            World world=EncounterScenarios.LargerDistrictTrial();
            Require(world.Width==64 && world.Height==36 && world.Count==11 && world.ShelterRoomCount==7 && world.StoreSupplies().Length==3,"Larger district geometry/population missing.");
            Require(world.FoodSupplies().RemainingMeals==109 && world.DistributionCenters()[0].Couriers==2,"Finite starting stock or courier roles missing.");
            Require(State(world)==State(EncounterScenarios.LargerDistrictTrial()),"District seed did not repeat.");
            HashSet<int> homes=new HashSet<int>();for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);homes.Add(actor.assignedHouseId);Require(actor.assignedHouseId!=0 && world.BuildingUseAt((actor.assignedHouseId-1)%world.Width,(actor.assignedHouseId-1)/world.Width)==BuildingUse.Home,"Resident assigned to workplace instead of home.");}
            Require(homes.Count==3,"Assigned homes were not distinct.");
            ActorState[] previous=world.Capture().actors;int eaten=0;HashSet<int> supplied=new HashSet<int>(),stockedHomes=new HashSet<int>();bool concurrent=false;World copy=null;
            for(int t=1;t<=2400;t++)
            {
                world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Long-route saved continuation diverged.");}
                for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);Require(actor.health>0,"District resident died during controlled supply trial.");if(actor.hunger<previous[i].hunger)eaten++;previous[i]=actor;}
                Require(world.FoodSupplies().RemainingMeals+eaten==109,"Long trips duplicated or lost food.");
                concurrent|=world.ActorAt(0).courierOrderId!=0 && world.ActorAt(1).courierOrderId!=0;
                foreach(SupplyOrderDocument order in world.PendingSupplyOrders())if(order.deliveredMeals>0)supplied.Add(order.storeId);
                foreach(SupplyOrderDocument order in world.CompletedSupplyOrders())supplied.Add(order.storeId);
                foreach(int home in homes)if(world.HouseholdFoodSupplies(home).BoxMeals>0)stockedHomes.Add(home);
                if(t==800)world=Documents.ParseSave(State(world));
                if(t==2360)copy=Documents.ParseSave(State(world));
            }
            Require(concurrent && supplied.Count==3 && stockedHomes.Count==3 && world.CompletedSupplyOrders().Length>0,"Long routes failed parallel courier service, all-store delivery or all-home groceries.");
        });
        Check("Supply chain visual preview separates assigned home from work without changing saved food",()=>{
            World world=EncounterScenarios.SupplyChainIdentityTest();int home=world.HouseAt(4,19);Require(world.Count==8 && world.ShelterRoomCount==5 && world.FoodSupplies().RemainingMeals==40,"Preview omitted the separate home or finite supplies.");
            for(int i=0;i<world.Count;i++)Require(world.ActorAt(i).assignedHouseId==home,"Worker or consumer lacked the separate assigned home.");
            Require(world.BuildingUseAt(4,19)==BuildingUse.Home && world.BuildingUseAt(17,3)==BuildingUse.Store && world.BuildingUseAt(5,9)==BuildingUse.DistributionCenter && world.BuildingUseAt(0,0)==BuildingUse.None,"Building metadata confused work, residence or outdoors.");
            var uses=world.BuildingUses();uses.Clear();Require(world.BuildingUses().Count==5,"Building metadata escaped by reference.");Require(world.HouseholdFoodSupplies(home).Residents==8 && world.HouseholdFoodSupplies(home).BoxMeals==6,"Home food/resident overview was wrong.");Require(world.ActorAt(0).distributionCenterId==319 && world.ActorAt(2).clerkStoreId==121,"Assigning home erased work roles.");
            Require(State(world)==State(Documents.ParseSave(State(world))) && State(world)==State(EncounterScenarios.SupplyChainIdentityTest()),"Preview did not repeat or save exactly.");
        });
        Check("Shortage dispatch serves the least fulfilled reported share and preserves legacy order",()=>{
            World world=EncounterScenarios.MultiStoreSupplyTrial();world.TrySetCourierSchedule(0,19,12);world.TrySetCourierSchedule(1,19,12);Run(world,50);
            SaveDocument saved=world.Capture();Require(saved.supplyOrders.Length==3,"Clerks did not report all three shortages.");
            SupplyOrderDocument oldest=saved.supplyOrders[0];foreach(SupplyOrderDocument order in saved.supplyOrders)if(order.createdTick<oldest.createdTick || (order.createdTick==oldest.createdTick && order.id<oldest.id))oldest=order;
            // The old dispatch record predates store discovery and mood state; omit both when authoring that legacy fixture.
            oldest.deliveredMeals=3;World current=World.Restore(saved);SaveDocument oldSave=World.Restore(saved).Capture();oldSave.rulesVersion=EncounterRules.SurvivalDamageVersion;for(int i=0;i<oldSave.actors.Length;i++){oldSave.actors[i].storeMemory=null;oldSave.actors[i].storeFailures=null;oldSave.actors[i].happiness=0;oldSave.actors[i].nextHappinessTick=0;}World legacy=World.Restore(oldSave);
            SupplyOrderDocument next=null;foreach(SupplyOrderDocument order in saved.supplyOrders)if(order.id!=oldest.id && (next==null || current.CompareSupplyOrderPriority(order,next)<0))next=order;
            Require(current.ShortagePrioritiesEnabled && !legacy.ShortagePrioritiesEnabled && legacy.Capture().rulesVersion==EncounterRules.SurvivalDamageVersion,"Legacy rules silently changed.");
            Require(current.CompareSupplyOrderPriority(next,oldest)<0 && legacy.CompareSupplyOrderPriority(oldest,next)<0,"Dispatch ignored reported shortage or old order age.");
            var half=new SupplyOrderDocument{id=1,requestedMeals=4,deliveredMeals=2,createdTick=1};var quarter=new SupplyOrderDocument{id=2,requestedMeals=8,deliveredMeals=2,createdTick=2};Require(current.CompareSupplyOrderPriority(quarter,half)<0,"Different capacities used meal counts instead of fulfilled shares.");quarter.deliveredMeals=4;Require(current.CompareSupplyOrderPriority(half,quarter)<0,"Equal shares ignored request age.");quarter.createdTick=1;Require(current.CompareSupplyOrderPriority(half,quarter)<0,"Equal shares and age ignored identity tie break.");
            current.TrySetCourierSchedule(0,7,12);legacy.TrySetCourierSchedule(0,7,12);World copy=Documents.ParseSave(State(current));Run(current,5);Run(copy,5);Run(legacy,5);
            Require(current.ActorAt(0).courierOrderId==next.id && legacy.ActorAt(0).courierOrderId==oldest.id,"Actual courier claim did not follow dispatch priority.");Require(State(current)==State(copy),"Priority dispatch diverged after saving.");
            int claim=current.ActorAt(0).courierOrderId;Run(current,5);Require(current.ActorAt(0).courierOrderId==claim,"Priority reordered an already active trip.");
        });
        Check("Multi-store district starts with explicit finite stock roles phones and different travel distances",()=>{
            World world=EncounterScenarios.MultiStoreSupplyTrial();Require(world.Count==8 && world.ShelterRoomCount==4 && world.StoreSupplies().Length==3 && world.DistributionCenters().Length==1 && world.PhoneBlocks().Length==3,"District geometry or role sites were missing.");Require(world.FoodSupplies().RemainingMeals==34 && world.DistributionCenters()[0].StockMeals==26 && world.DistributionCenters()[0].AvailableMeals==20 && world.DistributionCenters()[0].Couriers==2,"Starting finite stock or staff count was wrong.");foreach(StoreOverview store in world.StoreSupplies())Require(store.Clerks==1 && store.Shelves==1 && store.ShelfMeals==1 && store.CrateMeals==0 && world.StorageBoxAt((store.SupplyBox-1)%world.Width,(store.SupplyBox-1)/world.Width).capacity==4,"Store did not start with one clerk and finite consumer stock.");Require(world.ActorAt(0).courierBoxes==null && world.ActorAt(1).courierBoxes==null && world.ActorAt(3).hunger==80 && world.ActorAt(5).hunger==80 && world.ActorAt(7).hunger==80,"Scenario bypassed requests or lacked actual hungry consumers.");Require(State(world)==State(EncounterScenarios.MultiStoreSupplyTrial()),"Seeded scenario did not repeat exactly.");
        });
        Check("Four-hour multi-store trials conserve food and leave genuine shortages across three seeds",()=>{
            foreach(uint seed in new uint[]{214,123,999})
            {
                World world=EncounterScenarios.MultiStoreSupplyTrial(seed);ActorState[] previous=world.Capture().actors;int eaten=0,lostOnDeath=0;HashSet<int> supplied=new HashSet<int>();bool competing=false,parallel=false,partial=false;
                for(int tick=1;tick<=2400;tick++)
                {
                    world.Step();for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.health>0 && previous[i].health>0 && actor.hunger<previous[i].hunger)eaten++;if(actor.health==0 && previous[i].health>0)lostOnDeath+=previous[i].carriedFood;previous[i]=actor;}
                    SupplyOrderDocument[] orders=world.PendingSupplyOrders();competing|=orders.Length>1;HashSet<int> claims=new HashSet<int>();int active=0;
                    for(int i=0;i<2;i++){ActorState actor=world.ActorAt(i);if(actor.courierOrderId!=0){active++;Require(claims.Add(actor.courierOrderId),"Two couriers claimed the same order.");}}
                    parallel|=active==2;
                    foreach(SupplyOrderDocument order in orders){Require(order.deliveredMeals>=0 && order.deliveredMeals<order.requestedMeals,"Pending order quantity was invalid.");if(order.deliveredMeals>0){partial=true;supplied.Add(order.storeId);}}
                    foreach(SupplyOrderDocument order in world.CompletedSupplyOrders()){Require(order.deliveredMeals==order.requestedMeals,"Completed order was underfilled.");supplied.Add(order.storeId);}
                    Require(world.FoodSupplies().RemainingMeals+eaten+lostOnDeath==34 && world.Count==8,"Competing stores created, duplicated or lost food.");
                    if(tick%600==0)world=Documents.ParseSave(State(world));
                }
                Require(competing && parallel && partial && supplied.Count==3 && world.CompletedSupplyOrders().Length>=2,"Trial failed competing/parallel requests, partial progress or service to every store.");Require(world.DistributionCenters()[0].AvailableMeals==0 && world.PendingSupplyOrders().Length>0 && world.DistributionCenters()[0].ActiveTrips==0,"Finite center did not stop new shipments with real orders still pending.");Console.WriteLine("MULTISTORE seed "+seed+": tick "+world.Tick+", eaten "+eaten+", food left "+world.FoodSupplies().RemainingMeals+", completed "+world.CompletedSupplyOrders().Length+", pending "+world.PendingSupplyOrders().Length+", center available 0.");
            }
        });
        Check("Multi-store saves resume exactly while couriers share the center and stores consume deliveries",()=>{
            World world=EncounterScenarios.MultiStoreSupplyTrial();Run(world,80);World copy=Documents.ParseSave(State(world));for(int i=0;i<600;i++){world.Step();copy.Step();Require(State(world)==State(copy),"Shared-center continuation diverged.");}Run(world,1720);copy=Documents.ParseSave(State(world));Run(world,100);Run(copy,100);Require(State(world)==State(copy) && world.PendingSupplyOrders().Length>0,"Supply-shortage continuation diverged or lost outstanding work.");
        });
    }
}
