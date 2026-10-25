using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckParkingTrafficCases()
    {
        Check("Blocked traffic waits then follows a real alternate road with exact saved recovery",()=>{
            World world=EncounterScenarios.VehicleTrafficRecoveryTest();bool waited=false,detoured=false;var cells=new HashSet<int>();
            for(int t=0;t<350 && world.VehicleStatus(0)!="Arrived";t++){
                var before=world.VehicleAtIndex(0);world.Step();var car=world.VehicleAtIndex(0);waited|=car.blockedSinceTick>0;detoured|=car.detourUntilTick>world.Tick;cells.Add(car.y*world.Width+car.x);
                Require(Math.Abs(car.x-before.x)+Math.Abs(car.y-before.y)<=1,"Traffic recovery teleported a vehicle.");for(int i=1;i<world.VehicleCount;i++)Require(car.x!=world.VehicleAtIndex(i).x || car.y!=world.VehicleAtIndex(i).y,"Traffic recovery overlapped a parked car.");
                if(t%23==0){World copy=Documents.ParseSave(State(world));Run(copy,7);Run(world,7);Require(State(world)==State(copy),"Traffic detour diverged after save.");}
            }
            Require(waited && detoured && world.VehicleStatus(0)=="Arrived" && cells.Count>10,"Vehicle failed to recover through the alternate road.");
        });
        Check("No alternate road leaves traffic safely waiting until its blocker moves",()=>{
            var map=new WorldMap(18,8);for(int x=1;x<17;x++)map.Set(x,3,Terrain.Road);World world=new World(map,214,encounter:true,hunger:true);int id;world.TryPlaceVehicle(2,3,out id);world.TryPlaceVehicle(8,3,out id);world.TrySetVehicleDestination(0,14,3);Run(world,100);
            Require(world.VehicleAtIndex(0).x<8 && world.VehicleAtIndex(0).blockedSinceTick>0 && world.VehicleAtIndex(0).detourUntilTick==0,"Single corridor recovery bypassed an obstruction.");Require(world.TrySetVehicleDestination(1,16,3),"Could not release blocker.");Run(world,120);Require(world.VehicleStatus(0)=="Arrived","Waiting vehicle failed to resume after the lane cleared.");
        });
        Check("Parking bays persist independently reserve destinations and allow return to the lane",()=>{
            WorldMap map=WorldMap.Restore(EncounterScenarios.VehicleRoutingTest().Capture().map);map.SetParkingBay(8,3,1);World world=new World(map,214,encounter:true,hunger:true);int id;Require(world.TryPlaceVehicle(5,4,out id) && world.TryPlaceVehicle(17,4,out id),"Could not place parking trial cars.");Require(world.TrySetVehicleDestination(0,8,3),"Could not route into off-lane bay.");string before=State(world);Require(!world.TrySetVehicleDestination(1,8,3) && before==State(world),"Two cars claimed one parking destination.");Run(world,50);Require(world.VehicleAtIndex(0).x==8 && world.VehicleAtIndex(0).y==3 && world.TerrainAt(8,3)==Terrain.Sidewalk,"Vehicle failed to park off the road: "+world.VehicleAtIndex(0).x+","+world.VehicleAtIndex(0).y+" status="+world.VehicleStatus(0)+" terrain="+world.TerrainAt(8,3));World copy=Documents.ParseSave(State(world));Require(State(copy)==State(world) && world.ParkingBays().Length==1,"Parking save lost its catalog.");Require(world.TrySetVehicleDestination(0,20,12),"Parked vehicle could not rejoin the road.");Run(world,160);Require(world.VehicleStatus(0)=="Arrived","Parked car failed to leave the bay.");var detached=world.ParkingBays();detached[0].x=0;Require(world.ParkingBays()[0].x==8,"Parking catalog escaped by reference.");
        });
        Check("Courier bays preserve physical home delivery and exact saved journeys",()=>{
            var saved=EncounterScenarios.VehicleParkingTest().Capture();saved.actors=new[]{saved.actors[0]};World world=World.Restore(saved);bool parked=false;int[] homes=world.ActorAt(0).courierBoxes;
            for(int t=0;t<5000;t++){world.Step();var car=world.VehicleAtIndex(0);if(car.deliveryPhase==VehicleDeliveryPhase.Unloading){bool bay=false;foreach(var site in world.ParkingBays())bay|=site.x==car.x && site.y==car.y;Require(bay,"Courier unloaded while blocking a travel lane instead of using a bay.");parked=true;}
                if(t%131==0){var copy=Documents.ParseSave(State(world));Run(world,3);Run(copy,3);Require(State(world)==State(copy),"Bay delivery changed after save.");}
                bool full=true;foreach(int key in homes)full&=StorageRules.Count(world.StorageBoxAt((key-1)%world.Width,(key-1)/world.Width),InventoryItem.Food)==6;if(full && car.deliveryPhase==VehicleDeliveryPhase.None)break;
            }
            Require(parked,"Courier never parked to unload.");foreach(int key in homes)Require(StorageRules.Count(world.StorageBoxAt((key-1)%world.Width,(key-1)/world.Width),InventoryItem.Food)==6,"Bay courier did not fill both homes.");
        });
        Check("A parked car yields to a blocked bay exit and merges after traffic clears",()=>{
            WorldMap map=WorldMap.Restore(EncounterScenarios.VehicleRoutingTest().Capture().map);map.SetParkingBay(8,3,1);World world=new World(map,214,encounter:true,hunger:true);int id;
            Require(world.TryPlaceVehicle(8,3,out id) && world.TryPlaceVehicle(8,4,out id) && world.TrySetVehicleDestination(0,20,12),"Could not prepare occupied bay exit.");Run(world,60);Require(world.VehicleAtIndex(0).x==8 && world.VehicleAtIndex(0).y==3,"Parked vehicle drove through occupied exit.");Require(world.TrySetVehicleDestination(1,18,4),"Bay blocker could not depart.");Run(world,220);Require(world.VehicleStatus(0)=="Arrived","Bay vehicle failed to merge after its exit cleared.");
        });
        Check("Traffic recovery cannot use the opposing marked lane to bypass a blocker",()=>{
            WorldMap map=WorldMap.Restore(EncounterScenarios.VehicleLaneTest().Capture().map);World world=new World(map,214,encounter:true,hunger:true);int id;world.TryPlaceVehicle(7,5,out id);world.TryPlaceVehicle(12,5,out id);world.TrySetVehicleDestination(0,20,5);
            for(int t=0;t<150;t++){var before=world.VehicleAtIndex(0);world.Step();var after=world.VehicleAtIndex(0);if(before.x!=after.x || before.y!=after.y)Require((world.RoadLaneMaskAt(before.x,before.y)&(1<<after.heading))!=0,"Detour drove against the marked lane.");Require(after.x!=12 || after.y!=5,"Detour overlapped the blocker.");}
            Require(world.VehicleAtIndex(0).x<12 && world.VehicleStatus(0)!="Arrived","Recovery crossed an impassable directed segment.");
        });
        Check("Legacy traffic and corrupt bay or recovery records stay explicit",()=>{
            var old=EncounterScenarios.VehicleTrafficRecoveryTest().Capture();old.rulesVersion=EncounterRules.CourierVehiclesVersion;World legacy=World.Restore(old);Run(legacy,150);Require(!legacy.ParkingTrafficEnabled && legacy.VehicleAtIndex(0).x<10 && legacy.VehicleAtIndex(0).detourUntilTick==0,"Older traffic gained implicit recovery.");
            var map=WorldMap.Restore(EncounterScenarios.VehicleRoutingTest().Capture().map);map.SetParkingBay(8,3,1);var doc=map.Capture();Require(doc.schemaVersion==13,"Parking did not select its map schema.");doc.schemaVersion=12;Reject(()=>WorldMap.Restore(doc));doc=map.Capture();doc.parkingBays[0].exitDirection=5;Reject(()=>WorldMap.Restore(doc));doc=map.Capture();doc.parkingBays=new[]{doc.parkingBays[0],doc.parkingBays[0]};Reject(()=>WorldMap.Restore(doc));
            var saved=EncounterScenarios.VehicleTrafficRecoveryTest().Capture();saved.vehicles[0].blockedSinceTick=1;Reject(()=>World.Restore(saved));saved=EncounterScenarios.VehicleTrafficRecoveryTest().Capture();saved.vehicles[0].detourUntilTick=999;Reject(()=>World.Restore(saved));
            map.Set(8,4,Terrain.Grass);Require(!map.HasParkingBays && WorldMap.Restore(map.Capture()).ParkingBaySites().Length==0,"Removing bay entrance left stale parking data.");
        });
    }
}
