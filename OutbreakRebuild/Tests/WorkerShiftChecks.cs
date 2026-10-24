using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckWorkerShiftCases()
    {
        Check("Workers finish loaded work at shift end and commute to a separate home with exact continuation",()=>{
            World world=EncounterScenarios.WorkerShiftTest(),copy=Documents.ParseSave(State(world));int eaten=0;ActorState[] previous=world.Capture().actors;bool finishing=false,clerkHome=false,courierHome=false;
            for(int tick=0;tick<560;tick++)
            {
                world.Step();copy.Step();Require(State(world)==State(copy),"Worker commute diverged after saving.");for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.hunger<previous[i].hunger)eaten++;previous[i]=actor;}
                ActorState courier=world.ActorAt(0),clerk=world.ActorAt(1);if(world.Tick>=600 && courier.restockPhase!=RestockPhase.None){finishing=true;Require(world.WorkerOnDuty(0),"Loaded courier became off duty before finishing its delivery.");}
                clerkHome|=world.Tick>600 && world.HouseAt(clerk.x,clerk.y)==clerk.assignedHouseId && clerk.workerCommuteId==0;courierHome|=world.Tick>600 && world.HouseAt(courier.x,courier.y)==courier.assignedHouseId && courier.restockPhase==RestockPhase.None && courier.workerCommuteId==0;
                Require(world.FoodSupplies().RemainingMeals+eaten==33,"Work/home routines created or lost food.");if(tick%25==0)copy=Documents.ParseSave(State(world));
            }
            Require(finishing && clerkHome && courierHome && !world.WorkerOnDuty(0) && !world.WorkerOnDuty(1) && world.Occupation(2)==HumanOccupation.Consumer,"Workers failed home return, off-duty state or consumer role.");Require(StorageRules.Count(world.StorageBoxAt(24,7),InventoryItem.Food)==18 && world.ShelfAt(13,7).food==4,"Shift end started another pickup or discarded clerk/delivery food.");
        });
        Check("A clerk carrying a shelf load completes it after closing before returning home",()=>{
            SaveDocument saved=EncounterScenarios.WorkerShiftTest().Capture();saved.tick=599;saved.actors[1].carriedFood=3;saved.actors[1].clerkShelf=7*32+13+1;Array.Find(saved.map.storageBoxes,b=>b.x==14).items[0].quantity=2;World world=World.Restore(saved);Run(world,3);Require(world.WorkerOnDuty(1) && world.WorkerStatus(1)=="Finishing current shelf load" && world.ActorAt(1).workerCommuteId==0,"Loaded clerk abandoned its shelf load at shift end.");Run(world,110);Require(world.ShelfAt(13,7).food==2 && world.ActorAt(1).carriedFood==1 && world.HouseAt(world.ActorAt(1).x,world.ActorAt(1).y)==world.ActorAt(1).assignedHouseId && !world.WorkerOnDuty(1),"Clerk failed to finish the load and return home off duty.");Require(world.FoodSupplies().RemainingMeals==33,"Finishing a clerk shift changed total food.");
        });
        Check("A new shift commutes home-based workers to their actual workplaces before beginning work",()=>{
            World world=EncounterScenarios.WorkerShiftTest(true);Require(!world.WorkerOnDuty(0) && !world.WorkerOnDuty(1),"Workers began before their scheduled shift.");bool courierCommute=false,clerkCommute=false,courierAtWork=false,clerkAtWork=false;
            for(int i=0;i<400;i++){world.Step();ActorState a=world.ActorAt(0),b=world.ActorAt(1);courierCommute|=a.workerCommuteId!=0;clerkCommute|=b.workerCommuteId!=0;courierAtWork|=world.HouseAt(a.x,a.y)==a.distributionCenterId;clerkAtWork|=world.HouseAt(b.x,b.y)==b.clerkStoreId;}
            Require(courierCommute && clerkCommute && courierAtWork && clerkAtWork && world.ShelfAt(13,7).food>0,"Scheduled start failed to commute workers and replenish shelves.");
        });
        Check("A resident beside the home doorway does not shut out a commuting worker",()=>{
            World world=EncounterScenarios.WorkerShiftTest(true);SaveDocument saved=world.Capture();ActorState courier=saved.actors[0],resident=saved.actors[2];courier.id=3;resident.id=1;saved.actors[0]=resident;saved.actors[2]=courier;world=World.Restore(saved);Require(world.TrySetClerkSchedule(1,19,12),"Could not keep the doorway resident's worker off duty.");saved=world.Capture();saved.actors[1].x=12;saved.actors[1].y=5;saved.actors[1].shelterId=world.HouseAt(12,5);saved.actors[1].shelterX=12;saved.actors[1].shelterY=5;saved.actors[1].shelterState=ShelterState.Inside;world=World.Restore(saved);bool home=false;for(int i=0;i<250;i++){world.Step();ActorState clerk=world.ActorAt(1);home|=world.HouseAt(clerk.x,clerk.y)==clerk.assignedHouseId && clerk.workerCommuteId==0;}Require(home,"Door-side resident repeatedly closed the door before the commuting clerk crossed.");
        });
        Check("Clerk schedules follow day overnight and repeated-day boundaries and require an assigned home",()=>{
            foreach(int[] sample in new[]{new[]{0,7,12,1},new[]{7199,7,12,1},new[]{7200,7,12,0},new[]{14400,7,12,1},new[]{7200,19,12,1},new[]{14399,19,12,1},new[]{14400,19,12,0}})
            {
                SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();saved.tick=sample[0];saved.actors[0].shelfMemory=null;World world=World.Restore(saved);Require(world.TrySetClerkSchedule(1,sample[1],sample[2]) && world.WorkerOnDuty(1)==(sample[3]==1),"Clerk shift boundary was wrong.");
            }
            World noHome=EncounterScenarios.StoreClerkTest();Require(!noHome.TrySetClerkSchedule(0,7,12),"Scheduled clerk had no home to return to.");
            World editable=EncounterScenarios.WorkerShiftTest();ActorState before=editable.ActorAt(1);Require(editable.TrySetClerkSchedule(1,19,12),"Could not edit clerk shift.");ActorState after=editable.ActorAt(1);after.clerkShiftStartHour=before.clerkShiftStartHour;after.clerkShiftHours=before.clerkShiftHours;Require(Documents.Encode(before)==Documents.Encode(after),"Schedule edit changed cargo, role or current action.");
        });
        Check("Off-duty clerks can shop as residents without erasing their occupation",()=>{
            World world=EncounterScenarios.StoreShoppingTest();Require(world.TryUnassignClerk(1) && world.TryAssignHouse(0,3,5),"Could not prepare a resident clerk.");SaveDocument saved=world.Capture();saved.actors[0].clerkStoreId=world.HouseAt(12,5);saved.actors[0].clerkShiftStartHour=19;saved.actors[0].clerkShiftHours=12;world=World.Restore(saved);bool shopping=false;
            for(int i=0;i<500;i++){world.Step();ActorState actor=world.ActorAt(0);shopping|=actor.shoppingMeals>0;if(actor.shoppingMeals>0)Require(!world.WorkerOnDuty(0),"Personal shopping was displayed as working.");if(i%20==0)world=Documents.ParseSave(State(world));}
            Require(shopping && world.ActorAt(0).clerkStoreId!=0 && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)>0,"Off-duty clerk lost its job or could not resupply home.");
        });
        Check("Off-duty center couriers can shop for home without rewriting their source or destinations",()=>{
            World world=EncounterScenarios.StoreShoppingTest();SaveDocument saved=world.Capture();WorldMap map=WorldMap.Restore(saved.map);
            for(int y=9;y<=11;y++)for(int x=17;x<=21;x++)map.Set(x,y,x==17 || x==21 || y==9 || y==11?Terrain.Wall:Terrain.Floor);map.Set(17,10,Terrain.DoorClosed);map.SetStorageBox(new StorageBoxDocument{x=20,y=10,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=10}}});saved.map=map.Capture();world=World.Restore(saved);Require(world.TryConfigureDistributionCenter(20,10) && world.TryAssignDistributionCourier(0,world.HouseAt(20,10)) && world.TrySetCourierSchedule(0,19,12),"Could not assign an off-duty center courier.");int source=world.ActorAt(0).courierSourceBox,center=world.ActorAt(0).distributionCenterId;bool shopping=false;
            for(int i=0;i<500;i++){world.Step();shopping|=world.ActorAt(0).shoppingMeals>0;if(i%20==0)world=Documents.ParseSave(State(world));}
            Require(shopping && world.ActorAt(0).courierSourceBox==source && world.ActorAt(0).distributionCenterId==center && world.ActorAt(0).courierBoxes==null && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)>0,"Personal shopping rewrote the courier role or failed home delivery.");
        });
        Check("Blocked commutes and danger preserve geometry while invalid worker saves reject",()=>{
            World world=EncounterScenarios.WorkerShiftTest(true);Run(world,70);Require(world.ActorAt(0).workerCommuteId!=0,"Missing active worker commute.");SaveDocument before=world.Capture();int food=world.FoodSupplies().RemainingMeals;Require(!world.TryAssignHouse(0,3,9),"Busy commute allowed an assignment rewrite.");Require(world.FoodSupplies().RemainingMeals==food,"Assignment refusal moved food.");
            Action<Action<SaveDocument>> bad=change=>{SaveDocument saved=world.Capture();change(saved);Reject(()=>World.Restore(saved));};bad(s=>s.actors[0].workerCommuteId=999999);bad(s=>s.actors[1].clerkShiftHours=24);bad(s=>s.actors[2].clerkShiftHours=1);bad(s=>s.rulesVersion=EncounterRules.StoreDiscoveryVersion);
            before.actors[0].turnAtTick=before.tick+InfectionRules.Delay(before.actors[0],1);World infected=World.Restore(before);Run(infected,5);Require(infected.ActorAt(0).workerCommuteId==0 && !infected.WorkerOnDuty(0),"Infection retained active work commute or duty state.");
            SaveDocument blockedSave=EncounterScenarios.WorkerShiftTest(true).Capture();WorldMap blockedMap=WorldMap.Restore(blockedSave.map);blockedMap.SetStorageBox(new StorageBoxDocument{x=20,y=9,capacity=6,items=new ItemStackDocument[0]});blockedSave.map=blockedMap.Capture();World blocked=World.Restore(blockedSave);for(int i=0;i<250;i++){blocked.Step();if(i%20==0)blocked=Documents.ParseSave(State(blocked));}Require(blocked.ActorAt(0).restockPhase==RestockPhase.None && blocked.HouseAt(blocked.ActorAt(0).x,blocked.ActorAt(0).y)!=blocked.ActorAt(0).distributionCenterId,"Blocked commute started a delivery or crossed an obstructed door.");
            SaveDocument legacy=EncounterScenarios.StoreShoppingTest().Capture();legacy.rulesVersion=EncounterRules.StoreDiscoveryVersion;World old=World.Restore(legacy);Require(!old.WorkerShiftsEnabled && !old.TrySetClerkSchedule(1,7,12),"Old rules acquired clerk shifts.");
        });
    }
}
