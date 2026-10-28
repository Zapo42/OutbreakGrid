using System;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World OffDutyWorker(bool clerk)
    {
        // Isolate a real assigned worker in a stocked home, with a long off-duty window for the complete outing.
        var saved=EncounterScenarios.WorkerShiftTest(true).Capture();var actor=saved.actors[clerk?1:0];
        actor.clerkShiftStartHour=clerk?10:0;actor.clerkShiftHours=clerk?1:0;actor.courierShiftStartHour=clerk?0:10;actor.courierShiftHours=clerk?0:1;
        actor.hunger=0;actor.energySpent=0;actor.nextLeisureTick=saved.tick+1;saved.actors=new[]{actor};
        foreach(var box in saved.map.storageBoxes)if(box.x==4)box.foodReserve=6;
        return World.Restore(saved);
    }
    private static void CheckOffDutyConsumerCases()
    {
        Check("Off-duty clerks and couriers expose consumer roles without erasing employment",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk);var a=w.ActorAt(0);var assigned=clerk?HumanOccupation.Clerk:HumanOccupation.Courier;
                Require(w.OffDutyConsumersEnabled && w.Occupation(0)==HumanOccupation.Consumer && w.EffectiveOccupation(0)==HumanOccupation.Consumer && w.AssignedOccupation(0)==assigned && !w.WorkerOnDuty(0),"Off-duty role did not become consumer.");
                World copy=Documents.ParseSave(State(w));Require(copy.AssignedOccupation(0)==assigned && copy.ActorAt(0).assignedHouseId==a.assignedHouseId && State(copy)==State(w),"Temporary role destroyed job/home or save state.");}
        });
        Check("Both off-duty occupations complete paid leisure outings and physically return home",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk);bool left=false,pause=false,back=false,done=false;int moves=0;
                for(int t=0;t<500;t++){var before=w.ActorAt(0);w.Step();var a=w.ActorAt(0);if(a.x!=before.x || a.y!=before.y){moves++;Require(a.energySpent>before.energySpent,"Off-duty movement was free.");}left|=w.HouseAt(a.x,a.y)==0;pause|=a.leisurePhase==HumanLeisurePhase.Pausing;back|=a.leisurePhase==HumanLeisurePhase.Returning;if(back && a.leisurePhase==HumanLeisurePhase.None && w.HouseAt(a.x,a.y)==a.assignedHouseId){done=true;break;}}
                Require(left && pause && back && done && moves>4 && w.Occupation(0)==HumanOccupation.Consumer,"Worker failed full off-duty walk.");}
        });
        Check("New shifts interrupt optional walks and restore the permanent job commute",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk);if(clerk)w.TrySetClerkSchedule(0,8,1);else w.TrySetCourierSchedule(0,8,1);
                bool walked=false,resumed=false;for(int t=0;t<220;t++){w.Step();var a=w.ActorAt(0);walked|=a.leisurePhase!=HumanLeisurePhase.None;if(w.Tick>=600){Require(a.leisurePhase==HumanLeisurePhase.None && w.Occupation(0)==w.AssignedOccupation(0),"Work start left consumer leisure active.");resumed|=a.workerCommuteId!=0 || w.HouseAt(a.x,a.y)==(clerk?a.clerkStoreId:a.distributionCenterId);}}
                Require(walked && resumed,"Shift did not resume actual work travel after leisure.");}
        });
        Check("Off-duty routines save exactly through leisure and the next shift",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk),copy=Documents.ParseSave(State(w));for(int t=0;t<220;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Off-duty save continuation diverged.");if(t%23==0)copy=Documents.ParseSave(State(w));}}
        });
        Check("Overnight shifts and zero-hour always-on schedules preserve work identity",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk);if(clerk)w.TrySetClerkSchedule(0,22,10);else w.TrySetCourierSchedule(0,22,10);Require(w.Occupation(0)==w.AssignedOccupation(0),"Overnight shift was treated as off duty.");if(clerk)w.TrySetClerkSchedule(0,0,0);else w.TrySetCourierSchedule(0,0,0);Require(w.WorkerOnDuty(0) && w.Occupation(0)==w.AssignedOccupation(0),"Always-on schedule became consumer.");}
        });
        Check("A loaded clerk finishes the finite shelf load before joining consumer routines",()=>{
            var saved=EncounterScenarios.WorkerShiftTest().Capture();saved.tick=599;saved.actors[1].carriedFood=3;saved.actors[1].clerkShelf=7*32+14;Array.Find(saved.map.storageBoxes,b=>b.x==14).items[0].quantity=2;World w=World.Restore(saved);Run(w,3);
            Require(w.WorkerOnDuty(1) && w.Occupation(1)==HumanOccupation.Clerk,"Shelf load was abandoned for leisure.");Run(w,110);Require(w.Occupation(1)==HumanOccupation.Consumer && w.AssignedOccupation(1)==HumanOccupation.Clerk && w.ShelfAt(13,7).food==2 && w.FoodSupplies().RemainingMeals==33 && w.HouseAt(w.ActorAt(1).x,w.ActorAt(1).y)==w.ActorAt(1).assignedHouseId,"Shelf completion lost supplies or employment: "+w.ActorAt(1).x+","+w.ActorAt(1).y+" commute "+w.ActorAt(1).workerCommuteId+" returning "+w.ActorAt(1).courierReturningHome+" homeGoal "+w.ActorAt(1).homeShelterId+" exit "+w.ActorAt(1).shelterExitPhase);
        });
        Check("An idle assigned work van does not trap an off-duty courier at home",()=>{
            var saved=OffDutyWorker(false).Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(8,12,Terrain.Road);saved.map=map.Capture();World w=World.Restore(saved);int id;Require(w.TryPlaceVehicle(8,12,out id) && w.TryAssignVehicleCourier(0,0),"Could not assign empty work vehicle.");bool walked=false;for(int t=0;t<120;t++){w.Step();walked|=w.ActorAt(0).leisurePhase!=HumanLeisurePhase.None;}
            Require(walked && w.VehicleAtIndex(0).driverId==w.ActorAt(0).id && !w.VehicleAtIndex(0).driverAboard && w.VehicleAtIndex(0).deliveryPhase==VehicleDeliveryPhase.None,"Idle vehicle blocked leisure or started off-duty work.");
        });
        Check("Off-duty workers shop for their own empty home instead of taking a leisure walk",()=>{
            foreach(bool clerk in new[]{false,true}){var saved=OffDutyWorker(clerk).Capture();foreach(var box in saved.map.storageBoxes)if(box.x==4)box.items=new ItemStackDocument[0];saved.actors[0].storageMemory=new[]{new StorageMemoryRecord{x=4,y=7,food=0,space=30,observedTick=saved.tick}};saved.actors[0].familiarStoreId=Array.Find(saved.stores,s=>true).storeId;foreach(var shelf in saved.map.shelves)shelf.food=12;World w=World.Restore(saved);bool shopping=false;
                for(int t=0;t<120;t++){w.Step();var a=w.ActorAt(0);shopping|=a.shoppingMeals>0;Require(a.leisurePhase==HumanLeisurePhase.None,"Optional walk replaced essential personal groceries.");}Require(shopping,"Worker did not begin consumer shopping.");}
        });
        Check("A courier finishes a loaded vehicle shipment before becoming an off-duty consumer",()=>{
            World w=EncounterScenarios.VehicleDeliveryTest();for(int t=0;t<500 && w.VehicleAtIndex(0).handFood+w.VehicleAtIndex(0).cargoFood==0;t++)w.Step();
            Require(w.VehicleAtIndex(0).handFood+w.VehicleAtIndex(0).cargoFood>0 && w.TrySetCourierSchedule(0,19,1),"Could not close shift during a real shipment.");
            Require(w.Occupation(0)==HumanOccupation.Courier && w.WorkerOnDuty(0),"Loaded vehicle was abandoned for consumer leisure.");
            bool completed=false;for(int t=0;t<1600;t++){w.Step();if(w.VehicleAtIndex(0).deliveryPhase==VehicleDeliveryPhase.None){completed=true;break;}}
            Require(completed && w.VehicleAtIndex(0).cargoFood==0 && w.VehicleAtIndex(0).handFood==0 && w.Occupation(0)==HumanOccupation.Consumer && w.AssignedOccupation(0)==HumanOccupation.Courier,"Shipment never finished or job was lost: "+w.VehicleAtIndex(0).deliveryPhase+" role "+w.Occupation(0)+" status "+w.WorkerStatus(0)+" tick "+w.Tick+" cargo "+w.VehicleAtIndex(0).cargoFood+" hand "+w.VehicleAtIndex(0).handFood);
            Require(State(w)==State(Documents.ParseSave(State(w))),"Closed-shift courier shipment could not save exactly.");
        });
        Check("Legacy rules101 retains independently recorded worker shift continuation",()=>{
            var saved=EncounterScenarios.WorkerShiftTest().Capture();saved.rulesVersion=EncounterRules.HappinessVersion;World w=World.Restore(saved);Require(!w.OffDutyConsumersEnabled,"Legacy workers silently gained new roles.");Run(w,200);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(hash=="B573D8C04701EA54A7C6B80253FC3B4EF1322923CB4FA57196CD4B18F81CF8C4","Legacy worker behavior changed.");
        });
    }
}
