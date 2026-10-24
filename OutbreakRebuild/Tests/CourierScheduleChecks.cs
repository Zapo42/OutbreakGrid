using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckCourierScheduleCases()
    {
        Check("Courier schedules follow displayed game hours across shift ends midnight and new days",()=>{
            foreach(int[] sample in new[]{new[]{0,7,12,1},new[]{7199,7,12,1},new[]{7200,7,12,0},new[]{14400,7,12,1},new[]{7199,19,12,0},new[]{7200,19,12,1},new[]{14399,19,12,1},new[]{14400,19,12,0},new[]{10200,0,6,1},new[]{13800,0,6,0}})
            {
                SaveDocument saved=EncounterScenarios.AssignedCourierTest().Capture();saved.tick=sample[0];World world=World.Restore(saved);Require(world.TrySetCourierSchedule(0,sample[1],sample[2]) && world.CourierOnDuty(0)==(sample[3]==1),"Incorrect shift boundary at tick "+sample[0]+".");Require(world.CourierClockHour==(sample[0]/600+7)%24,"Schedule clock disagreed with displayed seven-o'clock origin.");
            }
        });
        Check("Shift end finishes cargo delivery then returns home without another pickup",()=>{
            World world=EncounterScenarios.CourierScheduleTest(),copy=Documents.ParseSave(State(world));bool finishing=false,home=false;
            for(int i=0;i<450;i++)
            {
                world.Step();copy.Step();Require(State(world)==State(copy),"Saved shift transition diverged.");ActorState actor=world.ActorAt(0);
                if(world.Tick>=7200 && actor.restockPhase!=RestockPhase.None){finishing=true;Require(!world.CourierOnDuty(0) && world.CourierScheduleStatus(0)=="Finishing current delivery","Shift end cancelled an active delivery.");}
                if(world.Tick>7200 && StorageRules.Count(world.StorageBoxAt(13,3),InventoryItem.Food)==2 && actor.restockPhase==RestockPhase.None && world.HouseAt(actor.x,actor.y)==actor.assignedHouseId && !actor.courierReturningHome){home=true;break;}
            }
            Require(finishing && home && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==28 && StorageRules.Count(world.StorageBoxAt(42,3),InventoryItem.Food)==0,"Off-duty courier lost cargo, failed home return or started another destination.");Run(world,80);Require(world.ActorAt(0).restockPhase==RestockPhase.None && world.HouseAt(world.ActorAt(0).x,world.ActorAt(0).y)==183,"Courier left home for another off-duty job.");
        });
        Check("Off-duty couriers rest with normal hunger cost and can still eat",()=>{
            SaveDocument saved=EncounterScenarios.AssignedCourierTest().Capture();saved.actors[0].energySpent=60;World world=World.Restore(saved);world.TrySetCourierSchedule(0,19,12);Run(world,60);Require(world.ActorAt(0).energySpent==0 && world.ActorAt(0).hunger>10 && world.ActorAt(0).restockPhase==RestockPhase.None && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==30,"Off-duty schedule prevented energy recovery, skipped hunger cost or started work.");saved=EncounterScenarios.AssignedCourierTest().Capture();saved.actors[0].hunger=80;world=World.Restore(saved);world.TrySetCourierSchedule(0,19,12);world.Step();Require(world.ActorAt(0).hunger==30 && world.ActorAt(0).carriedFood==0 && world.ActorAt(0).restockPhase==RestockPhase.None,"Schedule suppressed a hungry courier's meal.");
        });
        Check("Schedule edits change future jobs without changing an active task or cargo",()=>{
            World world=EncounterScenarios.AssignedCourierTest();for(int i=0;i<30 && world.ActorAt(0).restockPhase!=RestockPhase.Returning;i++)world.Step();ActorState before=world.ActorAt(0);Require(before.carriedFood==3 && world.TrySetCourierSchedule(0,19,12),"Could not change loaded courier schedule.");ActorState after=world.ActorAt(0);after.courierShiftStartHour=0;after.courierShiftHours=0;Require(Documents.Encode(before)==Documents.Encode(after) && !world.CourierOnDuty(0),"Schedule edit rewrote cargo, current task, door progress or movement.");Require(world.TrySetCourierSchedule(0,0,0) && world.CourierOnDuty(0),"Always-available mode did not restore eligibility.");
        });
        Check("Couriers resume at shift start while unscheduled and old-rule couriers retain behavior",()=>{
            World world=EncounterScenarios.AssignedCourierTest();world.TrySetCourierSchedule(0,19,12);Run(world,10);Require(world.ActorAt(0).restockPhase==RestockPhase.None,"Off-duty courier started work.");world.TrySetCourierSchedule(0,7,12);Run(world,5);Require(world.ActorAt(0).restockPhase!=RestockPhase.None,"On-duty courier did not resume work.");SaveDocument saved=EncounterScenarios.AssignedCourierTest().Capture();saved.rulesVersion=EncounterRules.HouseholdFoodTargetsVersion;world=World.Restore(saved);Require(!world.CourierSchedulesEnabled && !world.TrySetCourierSchedule(0,19,12),"Older household rules gained schedules.");world.Step();Require(world.ActorAt(0).restockPhase==RestockPhase.Collecting,"Legacy continuous courier stopped working.");
        });
        Check("Saved schedules reject invalid configurations and clear with routes or conversion",()=>{
            World world=EncounterScenarios.AssignedCourierTest();Require(world.TrySetCourierSchedule(0,19,12) && State(Documents.ParseSave(State(world)))==State(world),"Overnight schedule did not survive save.");string before=State(world);Require(!world.TrySetCourierSchedule(-1,7,12) && !world.TrySetCourierSchedule(0,24,12) && !world.TrySetCourierSchedule(0,7,24) && !world.TrySetCourierSchedule(0,7,0) && State(world)==before,"Invalid schedule edit changed authoritative state.");Action<Action<SaveDocument>> bad=change=>{SaveDocument saved=world.Capture();change(saved);Reject(()=>World.Restore(saved));};bad(s=>s.rulesVersion=EncounterRules.HouseholdFoodTargetsVersion);bad(s=>s.actors[0].courierShiftHours=-1);bad(s=>s.actors[0].courierShiftHours=0);bad(s=>s.actors[0].courierShiftStartHour=24);bad(s=>s.actors[0].courierShiftHours=24);Require(world.TryClearCourierRoute(0) && world.ActorAt(0).courierShiftHours==0 && !world.TrySetCourierSchedule(0,7,12),"Clearing route left a stale schedule.");world=EncounterScenarios.AssignedCourierTest();world.TrySetCourierSchedule(0,19,12);world.TryToggleCourierDestination(0,13,3);world.TryToggleCourierDestination(0,42,3);Require(world.ActorAt(0).courierShiftHours==0,"Removing final stop retained schedule.");world=EncounterScenarios.AssignedCourierTest();world.TrySetCourierSchedule(0,19,12);SaveDocument infected=world.Capture();infected.actors[0].turnAtTick=1;world=World.Restore(infected);Require(!world.TrySetCourierSchedule(0,7,12),"Infected courier accepted a schedule edit.");world.Step();Require(world.ActorAt(0).species==Species.Zombie && world.ActorAt(0).courierShiftHours==0 && !world.CourierOnDuty(0),"Conversion retained active courier shift.");
        });
    }
}
