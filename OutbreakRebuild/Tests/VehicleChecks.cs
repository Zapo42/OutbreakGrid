using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckLaneCases()
    {
        Check("Lane traffic preserves a following gap and exact saved continuation",()=>{
            World world=EncounterScenarios.VehicleLaneTest();world.TryStopVehicle(0);Run(world,9);
            Require(world.VehicleAtIndex(1).x==9 && world.VehicleStatus(1)=="Keeping following distance","Follower failed to keep an empty cell behind parked lead.");
            World copy=Documents.ParseSave(State(world));Require(State(copy)==State(world),"Waiting lane save changed.");
            Require(world.TrySetVehicleDestination(0,23,10) && copy.TrySetVehicleDestination(0,23,10),"Lead could not resume.");
            for(int t=0;t<100;t++){
                var before=world.VehicleAtIndex(1);int mask=world.RoadLaneMaskAt(before.x,before.y);world.Step();copy.Step();var after=world.VehicleAtIndex(1);
                if(before.x!=after.x || before.y!=after.y)Require((mask&(1<<after.heading))!=0,"Car drove against lane direction.");
                Require(State(world)==State(copy),"Lane save continuation diverged.");
                var lead=world.VehicleAtIndex(0);Require(lead.x!=after.x || lead.y!=after.y,"Cars overlapped.");
            }
            Require(world.VehicleStatus(0)=="Arrived" && world.VehicleStatus(1)=="Arrived","Lane vehicles did not arrive.");
        });
        Check("Generated directed lanes remain connected across layouts and seeds",()=>{
            for(uint seed=1;seed<=12;seed++)foreach(CityStreetLayout layout in new[]{CityStreetLayout.Grid,CityStreetLayout.Neighborhood}){
                WorldMap map=CityGenerator.Streets(seed,4,2,layout);int start=-1,total=0;
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(TerrainRules.RoadSurface(map.Get(x,y))){Require(map.RoadLaneMaskAt(x,y)>0,"Road lacks lane metadata.");start=y*map.Width+x;total++;}
                for(int reverse=0;reverse<2;reverse++){
                    var seen=new HashSet<int>();var queue=new Queue<int>();queue.Enqueue(start);seen.Add(start);int[] dx={1,0,-1,0},dy={0,1,0,-1};
                    while(queue.Count>0){int key=queue.Dequeue(),x=key%map.Width,y=key/map.Width;for(int d=0;d<4;d++){
                        int nx=x+dx[d],ny=y+dy[d];if(!map.Contains(nx,ny) || !TerrainRules.RoadSurface(map.Get(nx,ny)))continue;
                        int mask=reverse==0?map.RoadLaneMaskAt(x,y):map.RoadLaneMaskAt(nx,ny),bit=1<<(reverse==0?d:(d+2)%4);
                        if((mask&bit)!=0 && seen.Add(ny*map.Width+nx))queue.Enqueue(ny*map.Width+nx);
                    }}Require(seen.Count==total,"Directed road network contains an unreachable lane.");
                }
            }
        });
        Check("Lane metadata rejects corrupt catalogs and preserves explicit legacy rules",()=>{
            SaveDocument saved=EncounterScenarios.VehicleLaneTest().Capture();Require(saved.map.schemaVersion==12,"Lane map schema missing.");
            Action<Action<SaveDocument>> bad=change=>{var s=EncounterScenarios.VehicleLaneTest().Capture();change(s);Reject(()=>World.Restore(s));};
            bad(s=>s.map.roadLanes[0].directions=16);bad(s=>s.map.roadLanes[0].directions=0);bad(s=>s.map.schemaVersion=11);bad(s=>s.map.roadLanes[0].x=0);bad(s=>s.map.roadLanes=new[]{s.map.roadLanes[0],s.map.roadLanes[0]});
            WorldMap map=WorldMap.Restore(saved.map);var lane=saved.map.roadLanes[0];int mask=map.RoadLaneMaskAt(lane.x,lane.y);map.Set(lane.x,lane.y,Terrain.Crosswalk);Require(map.RoadLaneMaskAt(lane.x,lane.y)==mask,"Crosswalk erased lane.");map.Set(lane.x,lane.y,Terrain.Floor);Require(map.RoadLaneMaskAt(lane.x,lane.y)==0,"Removed road retained lane.");
            saved.rulesVersion=EncounterRules.VehicleRoutesVersion;World old=World.Restore(saved);Require(old.VehicleRoutesEnabled && !old.LaneTrafficEnabled,"Legacy vehicle world silently enabled lanes.");
            saved.vehicles[1].waitReason=VehicleWaitReason.FollowingDistance;Reject(()=>World.Restore(saved));
        });
    }
    private static void CheckVehicleCases()
    {
        Check("Vehicle route follows only roads crosses zebra tiles turns and arrives exactly",()=>{
            World world=EncounterScenarios.VehicleRoutingTest(),copy=null;Require(world.Count==0 && world.VehicleCount==1 && world.VehicleRoutesEnabled,"Vehicle-only preview omitted its entity or feature.");
            bool crossed=false,turned=false;VehicleState before=world.VehicleAtIndex(0);uint random=world.Capture().randomState,reaction=world.Capture().reactionRandomState;
            for(int t=1;t<=100;t++)
            {
                world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Vehicle trip diverged across save/load.");}
                VehicleState car=world.VehicleAtIndex(0);Require(Math.Abs(car.x-before.x)+Math.Abs(car.y-before.y)<=1 && TerrainRules.RoadSurface(world.TerrainAt(car.x,car.y)),"Vehicle teleported, moved diagonally or left the roadway.");crossed|=world.TerrainAt(car.x,car.y)==Terrain.Crosswalk;turned|=car.heading==1;before=car;
                if(t==20)copy=Documents.ParseSave(State(world));
            }
            Require(crossed && turned && before.x==23 && before.y==11 && world.VehicleStatus(0)=="Arrived" && before.nextMoveTick==0,"Vehicle failed crossing/turn/arrival behavior.");Require(world.Capture().randomState==random && world.Capture().reactionRandomState==reaction,"Vehicle navigation consumed NPC random draws.");
            string arrived=State(world);VehicleState detached=world.VehicleAtIndex(0);detached.x=1;Require(State(world)==arrived,"Vehicle record escaped by reference.");Run(world,20);Require(world.VehicleAtIndex(0).x==23 && world.VehicleAtIndex(0).y==11,"Arrived vehicle kept driving.");
        });
        Check("Vehicle yields for a pedestrian on either half of its crossing then resumes",()=>{
            World world=EncounterScenarios.VehicleRoutingTest();SaveDocument saved=world.Capture();saved.vehicles[0].x=13;saved.vehicles[0].y=4;saved.vehicles[0].destinationX=16;saved.vehicles[0].destinationY=4;world=World.Restore(saved);int id;Require(world.TrySpawn(Species.Human,14,5,out id),"Could not arrange crossing pedestrian.");saved=world.Capture();saved.actors[0].id=4;saved.nextId=5;world=World.Restore(saved);Run(world,3);
            Require(world.VehicleAtIndex(0).x==13 && world.VehicleStatus(0)=="Yielding to pedestrian" && world.ActorAt(0).x==14 && world.ActorAt(0).y==5,"Vehicle entered an occupied crossing component.");
            saved=world.Capture();saved.actors=new ActorState[0];world=World.Restore(saved);World copy=Documents.ParseSave(State(world));Run(world,9);Run(copy,9);Require(world.VehicleStatus(0)=="Arrived" && State(world)==State(copy),"Vehicle failed to resume after the crossing cleared.");
        });
        Check("Vehicles and bodies cannot occupy or be placed in each other's cells",()=>{
            World world=EncounterScenarios.VehicleRoutingTest();int id;string before=State(world);Require(!world.TrySpawn(Species.Human,7,4,out id) && !world.TryPlaceVehicle(7,4,out id) && !world.TryPlaceShelf(7,4,6) && !world.TryPlaceStorageBox(7,4) && State(world)==before,"Placement allowed overlap with vehicle.");
            Require(world.TrySpawn(Species.Human,10,4,out id),"Could not place test pedestrian.");before=State(world);Require(!world.TryPlaceVehicle(10,4,out id) && State(world)==before,"Vehicle spawned on an actor.");
            for(int t=0;t<200;t++){world.Step();VehicleState car=world.VehicleAtIndex(0);ActorState actor=world.ActorAt(0);Require(car.x!=actor.x || car.y!=actor.y,"Walking or driving overlapped a body and vehicle.");}
            World blocked=EncounterScenarios.VehicleRoutingTest();Require(blocked.TryPlaceVehicle(8,4,out id),"Could not create parked traffic blocker.");Run(blocked,12);Require(blocked.VehicleAtIndex(0).x==7 && blocked.VehicleStatus(0)=="Waiting for vehicle","Vehicle drove through a parked vehicle.");Require(blocked.TrySetVehicleDestination(1,12,4),"Could not release parked traffic.");Run(blocked,30);Require(blocked.VehicleAtIndex(0).x>7,"Following vehicle did not resume after road cleared.");
        });
        Check("Vehicle commands reject non-road unreachable and occupied targets without changing state",()=>{
            World world=EncounterScenarios.VehicleRoutingTest();string before=State(world);int id;
            Require(!world.TryPlaceVehicle(7,3,out id) && !world.TrySetVehicleDestination(0,7,3) && !world.TrySetVehicleDestination(-1,14,4) && !world.TrySetVehicleDestination(0,-1,0) && State(world)==before,"Invalid road commands mutated state.");
            Require(world.TryStopVehicle(0) && world.VehicleStatus(0)=="Parked" && world.VehicleAtIndex(0).nextMoveTick==0,"Stop did not clear command clock.");Require(world.TrySetVehicleDestination(0,7,4) && world.VehicleStatus(0)=="Arrived","Current-cell destination failed.");
            SaveDocument saved=world.Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(10,8,Terrain.Road);saved.map=map.Capture();world=World.Restore(saved);before=State(world);Require(!world.TrySetVehicleDestination(0,10,8) && State(world)==before,"Disconnected roadway became a valid route.");
            World dynamic=EncounterScenarios.VehicleRoutingTest();Require(dynamic.TryPlaceStorageBox(23,11),"Could not block a future destination.");Run(dynamic,3);Require(dynamic.VehicleStatus(0)=="Route unavailable" && State(dynamic)==State(Documents.ParseSave(State(dynamic))),"Blocked route did not wait or remain saveable.");
        });
        Check("Vehicle save validation rejects corruption overlap legacy data and stale movement clocks",()=>{
            Action<Action<SaveDocument>> bad=change=>{SaveDocument saved=EncounterScenarios.VehicleRoutingTest().Capture();change(saved);Reject(()=>World.Restore(saved));};
            bad(s=>s.vehicles[0].x=0);bad(s=>s.vehicles[0].heading=4);bad(s=>s.vehicles[0].nextMoveTick=0);bad(s=>s.vehicles[0].nextMoveTick=4);bad(s=>s.vehicles[0].destinationY=3);bad(s=>s.vehicles[0].waitReason=(VehicleWaitReason)99);bad(s=>s.nextVehicleId=1);bad(s=>s.rulesVersion=EncounterRules.StorefrontDiscoveryVersion);bad(s=>s.vehicles=new[]{s.vehicles[0],s.vehicles[0]});
            World world=EncounterScenarios.VehicleRoutingTest();int id;world.TrySpawn(Species.Human,8,4,out id);SaveDocument collision=world.Capture();collision.actors[0].x=7;Reject(()=>World.Restore(collision));
            SaveDocument old=EncounterScenarios.VehicleRoutingTest().Capture();old.vehicles=null;old.nextVehicleId=0;old.rulesVersion=EncounterRules.StorefrontDiscoveryVersion;World legacy=World.Restore(old);string original=State(legacy);Require(!legacy.VehicleRoutesEnabled && !legacy.TryPlaceVehicle(7,4,out id) && State(legacy)==original,"Legacy world silently enabled vehicles.");
        });
        Check("Vehicle routing uses generated neighborhood roads and respects entity count bounds",()=>{
            WorldMap map=CityGenerator.Streets(214,2,2,CityStreetLayout.Neighborhood);World world=new World(map,214,1000,true,hunger:true);int id;Require(world.TryPlaceVehicle(3,4,out id) && world.TrySetVehicleDestination(0,map.Width-5,map.Height-8),"Generated roadway route was unavailable.");
            bool arrived=false;for(int t=0;t<600;t++){world.Step();VehicleState car=world.VehicleAtIndex(0);Require(TerrainRules.RoadSurface(world.TerrainAt(car.x,car.y)),"Generated vehicle left shared road surfaces.");if(world.VehicleStatus(0)=="Arrived"){arrived=true;break;}}Require(arrived,"Generated neighborhood vehicle never arrived.");
            World capacity=EncounterScenarios.VehicleRoutingTest();for(int x=8;x<8+VehicleRules.Maximum-1;x++)Require(capacity.TryPlaceVehicle(x,5,out id),"Vehicle cap was reached early.");string before=State(capacity);Require(!capacity.TryPlaceVehicle(3,9,out id) && State(capacity)==before,"Vehicle cap allowed an extra car or changed state.");
        });
    }
}
