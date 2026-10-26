using System;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
using Outbreak.UnityAdapter;
public static partial class Program
{
    private static bool TestJunctionCell(World world,int x,int y){int mask=world.RoadLaneMaskAt(x,y);return mask!=0 && (mask&(mask-1))!=0;}
    private static void CheckVehicleJunctionCases()
    {
        Check("Vehicle headings rotate smoothly by the shortest angle without frame resets",()=>{
            for(int h=0;h<4;h++)foreach(int turn in new[]{-1,1}){
                var pose=new VehicleHeadingPresentation();pose.Snap(h,0);pose.Observe((h+turn+4)%4,3);Require(pose.Angle(3)==h*90,"Turn snapped at its start.");
                Require(Math.Abs(pose.Angle(4.5)-(h*90+turn*45))<1e-9,"Turn lacked an intermediate angle or took the long way.");pose.Observe((h+turn+4)%4,4);Require(Math.Abs(pose.Angle(6)-(h*90+turn*90))<1e-9,"Unchanged headings restarted the turn.");
                double previous=pose.Angle(3);for(int frame=1;frame<=30;frame++){double angle=pose.Angle(3+frame/10.0);Require((angle-previous)*turn>=0 && Math.Abs(angle-previous)<5,"Heading reversed or jumped during a turn.");previous=angle;}
            }
            var interrupted=new VehicleHeadingPresentation();interrupted.Snap(0,0);interrupted.Observe(1,3);double before=interrupted.Angle(4);interrupted.Observe(3,4);Require(interrupted.Angle(4)==before && Math.Abs(interrupted.Angle(7)+90)<1e-9,"Interrupted turn jumped or failed to finish.");interrupted.Snap(2,8);Require(interrupted.Angle(8)==180 && interrupted.Angle(100)==180,"Load/pause snap retained a stale rotation.");
        });
        Check("Simultaneous junction arrivals yield deterministically and clear without overlap",()=>{
            World world=EncounterScenarios.VehicleIntersectionTest();Require(world.JunctionTrafficEnabled,"Junction feature missing.");uint random=world.Capture().randomState;bool yielded=false;
            for(int t=1;t<=60;t++){
                world.Step();int inside=0;for(int i=0;i<world.VehicleCount;i++){var car=world.VehicleAtIndex(i);if(TestJunctionCell(world,car.x,car.y))inside++;}
                Require(inside<=1,"Multiple vehicles occupied a marked junction.");yielded|=world.VehicleStatus(1)=="Waiting for intersection";
                if(t==world.VehicleMoveTicks)Require(world.VehicleAtIndex(0).x==3 && world.VehicleAtIndex(1).y==5 && world.VehicleAtIndex(1).junctionWaitTick==world.VehicleMoveTicks,"Simultaneous arrival tie did not use stable vehicle identity.");
            }
            Require(yielded && world.VehicleStatus(0)=="Arrived" && world.VehicleStatus(1)=="Arrived" && world.Capture().randomState==random,"Junction trial failed arrival or consumed random draws.");
        });
        Check("Independent intersections allow traffic at the same time",()=>{
            WorldMap map=CityGenerator.Streets(214,1,1);World world=new World(map,214,1000,true,hunger:true);int id,x=map.Width-5,y=map.Height-5;
            Require(world.TryPlaceVehicle(2,4,out id) && world.TrySetVehicleDestination(0,9,4) && world.TryPlaceVehicle(x-1,y+1,out id) && world.TrySetVehicleDestination(1,x+2,y+1),"Could not arrange independent junctions.");Run(world,3);
            Require(TestJunctionCell(world,world.VehicleAtIndex(0).x,world.VehicleAtIndex(0).y) && TestJunctionCell(world,world.VehicleAtIndex(1).x,world.VehicleAtIndex(1).y),"One junction unnecessarily locked the entire road network.");
        });
        Check("Junction priority uses waiting age before vehicle identity",()=>{
            var saved=EncounterScenarios.VehicleIntersectionTest().Capture();saved.vehicles[0].x=1;World world=World.Restore(saved);int id;Require(world.TryPlaceVehicle(3,3,out id),"Could not place junction blocker.");Run(world,world.VehicleMoveTicks*3);
            Require(world.VehicleAtIndex(0).junctionWaitTick==world.VehicleMoveTicks*2 && world.VehicleAtIndex(1).junctionWaitTick==world.VehicleMoveTicks,"Approach arrival times not recorded.");Require(world.TrySetVehicleDestination(2,2,3),"Junction blocker could not leave.");Run(world,world.VehicleMoveTicks*2);
            Require(TestJunctionCell(world,world.VehicleAtIndex(1).x,world.VehicleAtIndex(1).y) && !TestJunctionCell(world,world.VehicleAtIndex(0).x,world.VehicleAtIndex(0).y),"Newer lower-ID arrival stole right-of-way.");
            Run(world,60);Require(world.VehicleStatus(0)=="Arrived" && world.VehicleStatus(1)=="Arrived","A queued approach starved.");
        });
        Check("Blocked exits hold cars outside while a different clear approach proceeds",()=>{
            World world=EncounterScenarios.VehicleIntersectionTest();int id;Require(world.TryPlaceVehicle(5,4,out id),"Could not obstruct exit.");Run(world,3);
            Require(world.VehicleAtIndex(0).x==2 && world.VehicleStatus(0)=="Waiting for clear exit" && TestJunctionCell(world,world.VehicleAtIndex(1).x,world.VehicleAtIndex(1).y),"Blocked exit admitted a car or blocked another clear approach.");
            Require(world.TrySetVehicleDestination(2,14,4),"Could not release exit obstruction.");Run(world,90);Require(world.VehicleStatus(0)=="Arrived" && world.VehicleStatus(1)=="Arrived","Traffic failed to resume after exit clearance.");
        });
        Check("Junction queues and occupants resume exactly across saves",()=>{
            foreach(int snapshotTick in new[]{3,6,9,12}){
                World world=EncounterScenarios.VehicleIntersectionTest();Run(world,snapshotTick);World copy=Documents.ParseSave(State(world));Require(State(copy)==State(world),"Junction save changed queue data.");
                for(int t=0;t<40;t++){world.Step();copy.Step();Require(State(world)==State(copy),"Saved junction queue diverged.");}
            }
        });
        Check("Junction commands clear queues reject junction destinations and preserve legacy opt-in",()=>{
            World world=EncounterScenarios.VehicleIntersectionTest();Run(world,3);string before=State(world);Require(!world.TrySetVehicleDestination(1,4,3) && State(world)==before,"Destination allowed a car to park inside junction.");
            Require(world.TryStopVehicle(1) && world.VehicleAtIndex(1).junctionWaitTick==0,"Stop retained priority claim.");Require(world.TrySetVehicleDestination(1,4,1) && world.VehicleAtIndex(1).junctionWaitTick==0,"Retarget retained old queue age.");
            var old=EncounterScenarios.VehicleIntersectionTest().Capture();old.rulesVersion=EncounterRules.LaneTrafficVersion;for(int c=0;c<old.vehicles.Length;c++)old.vehicles[c].paint=VehiclePaint.Blue;World legacy=World.Restore(old);Require(!legacy.JunctionTrafficEnabled && legacy.LaneTrafficEnabled,"Old lane saves silently enabled junction behavior.");Run(legacy,3);int inside=0;for(int i=0;i<legacy.VehicleCount;i++){var car=legacy.VehicleAtIndex(i);if(TestJunctionCell(legacy,car.x,car.y))inside++;}Require(inside==2,"Legacy junction movement changed.");
        });
        Check("Junction save validation rejects malformed and unsupported queue fields",()=>{
            Action<Action<SaveDocument>> bad=change=>{var world=EncounterScenarios.VehicleIntersectionTest();Run(world,3);var saved=world.Capture();change(saved);Reject(()=>World.Restore(saved));};
            bad(s=>s.vehicles[1].junctionWaitTick=-1);bad(s=>s.vehicles[1].junctionWaitTick=4);bad(s=>s.vehicles[1].junctionWaitTick=0);bad(s=>s.rulesVersion=EncounterRules.LaneTrafficVersion);
            bad(s=>{s.vehicles[1].hasDestination=false;s.vehicles[1].destinationX=0;s.vehicles[1].destinationY=0;s.vehicles[1].nextMoveTick=0;s.vehicles[1].waitReason=VehicleWaitReason.None;});
        });
        Check("Pedestrians on an intended junction path retain priority",()=>{
            World world=EncounterScenarios.VehicleIntersectionTest();int id;Require(world.TrySpawn(Species.Human,3,4,out id),"Could not place junction pedestrian.");var saved=world.Capture();saved.actors[0].id=4;saved.nextId=5;world=World.Restore(saved);Run(world,3);
            Require(world.VehicleAtIndex(0).x==2 && world.VehicleStatus(0)=="Yielding to pedestrian","Right-of-way displaced pedestrian safety.");
            saved=world.Capture();saved.actors=new ActorState[0];world=World.Restore(saved);Run(world,60);Require(world.VehicleStatus(0)=="Arrived" && world.VehicleStatus(1)=="Arrived","Pedestrian clearance did not resume traffic.");
        });
    }
}
