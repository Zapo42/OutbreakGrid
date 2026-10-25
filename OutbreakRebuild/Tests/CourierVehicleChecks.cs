using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static World SeekCourierVehicle(Func<World,bool> goal,int max=2600)
    {
        World world=EncounterScenarios.VehicleDeliveryTest();for(int t=0;t<max && !goal(world);t++)world.Step();Require(goal(world),"Vehicle preview did not reach required stage.");return world;
    }
    private static void CheckCourierVehicleCases()
    {
        Check("Courier physically loads boards drives and credits both finite store orders",()=>{
            World world=EncounterScenarios.VehicleDeliveryTest();int initial=world.FoodSupplies().RemainingMeals,meals=0;bool carrying=false,driving=false,returning=false,unloading=false;
            for(int t=0;t<2500 && world.CompletedSupplyOrders().Length<2;t++){
                int[] hunger=new int[world.Count];for(int i=0;i<world.Count;i++)hunger[i]=world.ActorAt(i).hunger;world.Step();for(int i=0;i<world.Count;i++)if(world.ActorAt(i).hunger<hunger[i])meals++;
                var car=world.VehicleAtIndex(0);var actor=world.ActorAt(0);carrying|=car.handFood>0 && !car.driverAboard;driving|=car.driverAboard && car.deliveryPhase==VehicleDeliveryPhase.DrivingToDelivery;returning|=car.deliveryPhase==VehicleDeliveryPhase.DrivingToSource;unloading|=car.deliveryPhase==VehicleDeliveryPhase.Unloading;
                Require(car.cargoFood+car.handFood<=CourierVehicleRules.CargoMeals && world.FoodSupplies().RemainingMeals+meals==initial,"Vehicle transfer created or lost food.");
                Require(!car.driverAboard || (world.ActorInVehicle(0) && actor.x==car.x && actor.y==car.y),"Driver and vehicle separated while aboard.");
            }
            Require(carrying && driving && returning && unloading && world.CompletedSupplyOrders().Length>=2,"Vehicle failed complete physical delivery cycle to both stores.");var done=world.CompletedSupplyOrders();Require(done[0].supplyBox!=done[1].supplyBox && done[0].deliveredMeals==12 && done[1].deliveredMeals==12,"Vehicle did not deliver exact requested quantities to distinct boxes.");
        });
        Check("Courier vehicle saves resume exactly during walking loading riding and unloading",()=>{
            World world=EncounterScenarios.VehicleDeliveryTest();var seen=new HashSet<VehicleDeliveryPhase>();
            for(int t=0;t<2400;t++){world.Step();var phase=world.VehicleAtIndex(0).deliveryPhase;if(!seen.Add(phase))continue;World copy=Documents.ParseSave(State(world));Run(world,15);Run(copy,15);t+=15;Require(State(world)==State(copy),"Vehicle phase diverged after a saved continuation.");}
            Require(seen.Contains(VehicleDeliveryPhase.BoardingForDelivery) && seen.Contains(VehicleDeliveryPhase.DrivingToDelivery) && seen.Contains(VehicleDeliveryPhase.Unloading) && seen.Contains(VehicleDeliveryPhase.DrivingToSource),"Save checks missed a delivery phase.");
        });
        Check("Autonomous vehicle pause retains rider and shipment and resumes without manual steering",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard && w.VehicleAtIndex(0).hasDestination);var car=world.VehicleAtIndex(0);Require(world.TryStopVehicle(0),"Assigned vehicle could not pause.");Run(world,30);var paused=world.VehicleAtIndex(0);Require(paused.x==car.x && paused.y==car.y && paused.driverAboard && paused.deliveriesPaused,"Stop did not pause the delivery.");string before=State(world);int id;Require(!world.TrySetVehicleDestination(0,car.x,car.y) && !world.TryUnassignVehicleCourier(0) && !world.TrySpawn(Species.Human,car.x,car.y,out id) && State(world)==before,"Invalid active-vehicle command changed state.");World copy=Documents.ParseSave(State(world));Require(world.TryResumeVehicleCourier(0) && copy.TryResumeVehicleCourier(0),"Delivery could not resume.");Run(world,120);Run(copy,120);Require(State(world)==State(copy) && !world.VehicleAtIndex(0).deliveriesPaused,"Resumed delivery did not continue exactly.");
        });
        Check("Vehicle driver and cargo validation rejects corrupt occupancy clocks assignments and legacy fields",()=>{
            Action<Action<SaveDocument>> bad=change=>{World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard);var saved=world.Capture();change(saved);Reject(()=>World.Restore(saved));};
            bad(s=>s.rulesVersion=EncounterRules.DistrictRoutesVersion);bad(s=>s.vehicles[0].driverId=9999);bad(s=>s.vehicles[0].cargoFood=25);bad(s=>s.vehicles[0].cargoFood=-1);bad(s=>s.vehicles[0].handFood=1);bad(s=>s.vehicles[0].driverAboard=false);bad(s=>s.actors[0].x++);bad(s=>s.vehicles[0].orderId=9999);bad(s=>s.vehicles[0].sourceBox=1);bad(s=>s.vehicles[0].exitAtTick=s.tick+100);bad(s=>s.vehicles[0].nextRoutePlanTick=-1);bad(s=>s.vehicles[0].lastMealTick=s.tick+1);bad(s=>s.vehicles[0].driveEffort=10);
            var old=EncounterScenarios.VehicleDeliveryTest().Capture();old.vehicles[0].driverId=0;old.rulesVersion=EncounterRules.DistrictRoutesVersion;World legacy=World.Restore(old);string original=State(legacy);Require(!legacy.CourierVehiclesEnabled && !legacy.TryAssignVehicleCourier(0,0) && State(legacy)==original,"Old district run silently enabled courier vehicles.");
        });
        Check("Cancelling a store request atomically retains the loaded vehicle shipment",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard && w.VehicleAtIndex(0).cargoFood>0);var car=world.VehicleAtIndex(0);int room=world.HouseAt((car.targetBox-1)%world.Width,(car.targetBox-1)/world.Width);bool placed=false;int bx=0,by=0;for(int y=0;y<world.Height && !placed;y++)for(int x=0;x<world.Width && !placed;x++)if(world.HouseAt(x,y)==room && world.TryPlaceStorageBox(x,y)){placed=true;bx=x;by=y;}Require(placed && world.TryConfigureStore(bx,by),"Could not reconfigure store during a delivery.");var returned=world.VehicleAtIndex(0);Require(returned.orderId==0 && returned.cargoFood==car.cargoFood && returned.deliveryPhase==VehicleDeliveryPhase.DrivingToSource && !returned.hasDestination,"Cancelled order deleted cargo or kept the wrong route.");Require(State(world)==State(Documents.ParseSave(State(world))),"Same-tick cancellation left an invalid save.");
        });
        Check("A full destination sends its untouched excess shipment back to the source",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard && w.VehicleAtIndex(0).cargoFood>0);var saved=world.Capture();saved.actors=new[]{saved.actors[0],saved.actors[1],saved.actors[2]};int key=saved.vehicles[0].targetBox-1;WorldMap map=WorldMap.Restore(saved.map);var box=map.StorageBoxAt(key%map.Width,key/map.Width);StorageRules.ChangeFood(box,box.capacity-StorageRules.Used(box));map.SetStorageBox(box);saved.map=map.Capture();world=World.Restore(saved);Require(world.TryConfigureStore(box.x,box.y,1),"Could not hold shelves above their test target.");int source=world.VehicleAtIndex(0).sourceBox-1,sourceFood=StorageRules.Count(world.StorageBoxAt(source%world.Width,source/world.Width),InventoryItem.Food),cargo=world.VehicleAtIndex(0).cargoFood;
            for(int t=0;t<2000 && world.VehicleAtIndex(0).deliveryPhase!=VehicleDeliveryPhase.None;t++)world.Step();Require(world.VehicleAtIndex(0).deliveryPhase==VehicleDeliveryPhase.None && StorageRules.Count(world.StorageBoxAt(source%world.Width,source/world.Width),InventoryItem.Food)>=sourceFood+cargo,"Full crate return failed: phase="+world.VehicleAtIndex(0).deliveryPhase+", source="+StorageRules.Count(world.StorageBoxAt(source%world.Width,source/world.Width),InventoryItem.Food)+", expected="+(sourceFood+cargo));
        });
        Check("An incapacitated courier drops a recoverable finite shipment without deleting cargo",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).handFood>0 && w.VehicleAtIndex(0).deliveryPhase==VehicleDeliveryPhase.BoardingForDelivery && w.HouseAt(w.ActorAt(0).x,w.ActorAt(0).y)==0 && w.Tick%HungerRules.GainTicks==HungerRules.GainTicks-1);var saved=world.Capture();int shipment=saved.vehicles[0].handFood,home=saved.actors[0].assignedHouseId,center=saved.actors[0].distributionCenterId;var sourceBox=world.StorageBoxAt((saved.vehicles[0].sourceBox-1)%world.Width,(saved.vehicles[0].sourceBox-1)/world.Width);int sourceBefore=StorageRules.Count(sourceBox,InventoryItem.Food);saved.actors[0].health=1;saved.actors[0].hunger=100;saved.actors[0].energySpent=100;saved.actors[0].carriedFood=0;world=World.Restore(saved);world.Step();var dropped=world.VehicleAtIndex(0);Require(dropped.shipmentDropped && dropped.handFood==shipment && dropped.deliveriesPaused && dropped.orderId==0,"Incapacitated courier deleted or retained a live order claim for dropped cargo.");world=Documents.ParseSave(State(world));
            int newIndex=world.Count,id;bool spawned=false;for(int y=0;y<world.Height && !spawned;y++)for(int x=0;x<world.Width && !spawned;x++)if(world.HouseAt(x,y)==center)spawned=world.TrySpawn(Species.Human,x,y,out id);Require(spawned,"Could not add replacement courier.");StorageBoxDocument house=null;foreach(var box in world.StorageBoxSites())if(world.HouseAt(box.x,box.y)==home){house=box;break;}Require(house!=null && world.TryAssignHouse(newIndex,house.x-1,house.y) && world.TryAssignDistributionCourier(newIndex,center) && world.TryAssignVehicleCourier(0,newIndex),"Replacement courier could not recover assignment.");
            for(int t=0;t<1000 && world.VehicleAtIndex(0).deliveryPhase!=VehicleDeliveryPhase.None;t++)world.Step();Require(world.VehicleAtIndex(0).deliveryPhase==VehicleDeliveryPhase.None && world.VehicleAtIndex(0).handFood==0 && StorageRules.Count(world.StorageBoxAt(sourceBox.x,sourceBox.y),InventoryItem.Food)>=sourceBefore+shipment,"Recovered shipment did not physically return to its source.");
        });
        Check("A starving seated driver stops and exits without leaving invalid overlapping state",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard);var saved=world.Capture();saved.actors[0].health=1;saved.actors[0].hunger=100;saved.actors[0].energySpent=100;saved.actors[0].carriedFood=0;saved.vehicles[0].cargoFood=0;world=World.Restore(saved);Run(world,35);var car=world.VehicleAtIndex(0);Require(world.ActorAt(0).health==0 && car.deliveriesPaused && !car.driverAboard && car.orderId==0 && !car.hasDestination,"Incapacitated seated driver kept driving or stayed overlapped with the car.");Require(State(world)==State(Documents.ParseSave(State(world))),"Incapacitated vehicle state could not reload.");
        });
        Check("Vehicle exit waits for the arrival movement to finish and driving consumes energy",()=>{
            World world=SeekCourierVehicle(w=>w.VehicleAtIndex(0).driverAboard && w.VehicleAtIndex(0).hasDestination);int initial=world.ActorAt(0).energySpent;bool waiting=false;
            for(int t=0;t<900;t++){world.Step();var car=world.VehicleAtIndex(0);if(car.exitAtTick>world.Tick){waiting=true;Require(car.driverAboard && car.nextMoveTick==0,"Courier left before its arrival movement completed.");}if(car.deliveryPhase==VehicleDeliveryPhase.Unloading)break;}
            Require(waiting && world.ActorAt(0).energySpent>initial && !world.ActorInVehicle(0),"Arrival delay, energy charge or disembarking failed.");
        });
    }
}
