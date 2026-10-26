using System;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
using Outbreak.UnityAdapter;
public static partial class Program
{
    private static void CheckVehicleVariationCases()
    {
        Check("Faster cars finish the same route sooner while old rules retain their pace",()=>{
            World fast=EncounterScenarios.VehicleRoutingTest();var saved=fast.Capture();saved.rulesVersion=EncounterRules.ConsumerVehiclesVersion;saved.vehicles[0].nextMoveTick=3;World old=World.Restore(saved);
            Require(fast.VehicleMoveTicks==2 && old.VehicleMoveTicks==3,"Vehicle pace migration changed old rules.");
            int fastArrival=0,oldArrival=0;for(int t=1;t<=100;t++){fast.Step();old.Step();if(fastArrival==0 && fast.VehicleStatus(0)=="Arrived")fastArrival=t;if(oldArrival==0 && old.VehicleStatus(0)=="Arrived")oldArrival=t;}
            Require(fastArrival>0 && oldArrival*2==fastArrival*3,"Faster route was not exactly one third shorter.");
            Require(!old.TrySetVehiclePaint(0,VehiclePaint.Red),"Legacy vehicle accepted new paint rules.");
        });
        Check("Eight vehicle paints persist and invalid paints are rejected transactionally",()=>{
            World world=EncounterScenarios.FasterVehicleColorsTest();Require(world.VehicleCount==8,"Color preview lacks eight vehicles.");
            for(int i=0;i<8;i++)Require(world.VehicleAtIndex(i).paint==(VehiclePaint)i,"Paint assignment was not deterministic.");
            Require(world.TrySetVehiclePaint(0,VehiclePaint.Red),"Paint selection failed.");string before=State(world);
            Require(!world.TrySetVehiclePaint(-1,VehiclePaint.Blue) && !world.TrySetVehiclePaint(8,VehiclePaint.Blue) && !world.TrySetVehiclePaint(0,(VehiclePaint)8) && State(world)==before,"Invalid paint changed the world.");
            World copy=Documents.ParseSave(before);for(int t=0;t<80;t++){world.Step();copy.Step();Require(State(world)==State(copy),"Paint or fast clock changed after save/load.");}
            var bad=world.Capture();bad.vehicles[0].paint=(VehiclePaint)8;Reject(()=>World.Restore(bad));bad=world.Capture();bad.rulesVersion=EncounterRules.ConsumerVehiclesVersion;Reject(()=>World.Restore(bad));
        });
        Check("Faster turn presentation completes over the two-tick movement interval",()=>{
            var pose=new VehicleHeadingPresentation();pose.Snap(0,0);pose.Observe(1,2,2);Require(pose.Angle(2)==0 && Math.Abs(pose.Angle(3)-45)<1e-9,"Fast turn snapped or used old timing.");pose.Observe(1,3,2);Require(pose.Angle(4)==90,"Repeated heading restarted fast rotation.");
            pose.Observe(2,4,2);double before=pose.Angle(5);pose.Observe(3,5,2);Require(pose.Angle(5)==before && pose.Angle(7)==270,"Interrupted fast turn jumped.");pose.Snap(0,8);Require(pose.Angle(9)==0,"Snap retained a turn.");
        });
    }
}
