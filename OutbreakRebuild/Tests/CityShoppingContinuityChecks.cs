using System;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static int ShopperHomeFood(World world){var actor=world.ActorAt(0);int total=0;foreach(var box in world.StorageBoxSites())if(world.HouseAt(box.x,box.y)==actor.assignedHouseId)total+=StorageRules.Count(box,InventoryItem.Food);return total;}
    private static World InFlightHungryShopper(bool returning=false)
    {
        World w=EncounterScenarios.LargeCityShoppingTest();
        for(int t=0;t<800;t++){
            var a=w.ActorAt(0);Terrain ground=w.TerrainAt(a.x,a.y);
            if(a.shoppingMeals>0 && a.restockPhase==RestockPhase.Collecting && w.HouseAt(a.x,a.y)!=a.assignedHouseId && a.shelterExitPhase==ShelterExitPhase.None && ground!=Terrain.DoorOpen && ground!=Terrain.DoorClosed && !TerrainRules.RoadSurface(ground)){
                var save=w.Capture();a=save.actors[0];a.hunger=45;a.health=70;a.carriedFood=0;a.storageMemory=null;a.energySpent=returning?EnergyRules.Maximum:50;if(returning)a.restockPhase=RestockPhase.Returning;save.actors[0]=a;return World.Restore(save);
            }
            w.Step();
        }
        throw new Exception("No actual off-road shopping departure reached.");
    }
    private static void CheckCityShoppingContinuityCases()
    {
        Check("A failed store remains excluded throughout one long walking search",()=>{
            World w=EncounterScenarios.LargeCityShoppingTest();Run(w,500);var save=w.Capture();var a=save.actors[0];Require(a.shoppingMeals>0 && a.restockPhase==RestockPhase.Collecting && save.tick-a.restockUntilTick+CourierRules.TaskTicks>=300,"Long-search fixture did not reach a real ongoing trip.");int store=a.shoppingStoreId;Require(store!=0,"Missing current shop.");a.storeFailures=new[]{new FoodRoomVisit{roomId=store,observedTick=save.tick-StoreDiscoveryRules.RetryTicks+1}};save.actors[0]=a;w=World.Restore(save);long observed=a.storeFailures[0].observedTick;w.Step();a=w.ActorAt(0);Require(a.storeFailures!=null && Array.Exists(a.storeFailures,n=>n.roomId==store && n.observedTick==observed),"A within-trip failure expired before the farther-shop journey could finish.");Require(State(w)==State(Documents.ParseSave(State(w))),"Retained genuine failure did not validate through save.");
        });
        Check("An expired empty shopping task releases its claim and retains a paid home return",()=>{
            var save=InFlightHungryShopper(true).Capture();save.actors[0].restockUntilTick=save.tick+1;World w=World.Restore(save);var oldSave=Documents.ParseSave(State(w)).Capture();oldSave.rulesVersion="encounter-prototype-108";World old=World.Restore(oldSave);int food=w.FoodSupplies().RemainingMeals;Run(w,10);Run(old,10);var a=w.ActorAt(0);Require(a.restockPhase==RestockPhase.None && a.restockBox==0 && a.shoppingMeals==0 && a.shoppingEndReason==ShoppingEndReason.Expired,"Expired task retained or recreated a pantry claim.");Require(a.courierReturningHome && a.homeShelterId==a.assignedHouseId && a.energySpent<EnergyRules.Maximum && a.hunger>45,"Timeout stranded the hungry empty shopper without paid return recovery.");Require(!old.ActorAt(0).courierReturningHome && old.ActorAt(0).energySpent==EnergyRules.Maximum,"Rules108 unexpectedly gained the expiry handoff.");Require(w.FoodSupplies().RemainingMeals==food && a.carriedFood==0,"Expiry fabricated or disposed of meals.");World copy=Documents.ParseSave(State(w));Run(w,30);Run(copy,30);Require(State(w)==State(copy),"Expired return changed through save/reload.");bool arrived=false;for(int t=0;t<600;t++){var before=w.ActorAt(0);w.Step();a=w.ActorAt(0);if(a.x!=before.x || a.y!=before.y)Require(a.energySpent>before.energySpent || before.energySpent==EnergyRules.Maximum,"Timeout return movement was free.");if(w.HouseAt(a.x,a.y)==a.assignedHouseId){arrived=true;break;}}Require(arrived,"Expired pedestrian never physically arrived home.");Run(w,5);Require(!w.ActorAt(0).courierReturningHome,"Home arrival kept the expired return intent.");
        });
        Check("Timeout at home adds no food-seeking cooldown",()=>{
            var save=InFlightHungryShopper(true).Capture();var a=save.actors[0];int key=a.restockBox-1;WorldMap map=WorldMap.Restore(save.map);World geometry=World.Restore(save);bool placed=false;
            foreach(var d in new[]{new[]{-1,0},new[]{1,0},new[]{0,-1},new[]{0,1}}){int x=key%map.Width+d[0],y=key/map.Width+d[1];if(!map.CanWalk(x,y) || geometry.HouseAt(x,y)!=a.assignedHouseId)continue;a.x=x;a.y=y;a.shelterId=a.assignedHouseId;a.shelterState=ShelterState.Inside;a.shelterX=x;a.shelterY=y;placed=true;break;}
            Require(placed,"No usable home pantry approach.");a.restockUntilTick=save.tick+1;save.actors[0]=a;World w=World.Restore(save);w.Step();a=w.ActorAt(0);Require(a.restockPhase==RestockPhase.None && a.shoppingRetryTick==0 && !a.courierReturningHome,"Timeout imposed a new home cooldown on an empty hungry shopper.");
        });
        Check("Shopping timeout preserves a real carried load without recreating a task",()=>{
            var save=InFlightHungryShopper(true).Capture();save.actors[0].restockUntilTick=save.tick+1;save.actors[0].carriedFood=HungerRules.CarryLimit;save.actors[0].hunger=10;World w=World.Restore(save);int food=w.FoodSupplies().RemainingMeals;w.Step();Require(w.ActorAt(0).carriedFood==HungerRules.CarryLimit && w.FoodSupplies().RemainingMeals==food && w.ActorAt(0).restockPhase==RestockPhase.None,"Timeout ate, lost or credited physical cargo.");World copy=Documents.ParseSave(State(w));Run(w,20);Run(copy,20);Require(State(w)==State(copy),"Loaded timeout handoff did not continue exactly.");
        });
        Check("Consumer vehicle timeout retains the published108 cart lifecycle",()=>{
            World w=EncounterScenarios.ConsumerVehicleShoppingTest();Run(w,30);var save=w.Capture();Require(save.actors[0].shoppingMeals>0,"Vehicle fixture never started shopping.");save.actors[0].restockUntilTick=save.tick+1;w=World.Restore(save);var oldSave=Documents.ParseSave(State(w)).Capture();oldSave.rulesVersion=EncounterRules.ShoppingRecoveryVersion;World old=World.Restore(oldSave);Run(w,15);Run(old,15);var result=w.Capture();result.rulesVersion=EncounterRules.ShoppingRecoveryVersion;Require(Documents.Encode(result)==State(old),"Pedestrian timeout handling changed vehicle/cart continuation.");
        });
        Check("Danger still owns the action after a shopping task expires",()=>{
            var save=InFlightHungryShopper(true).Capture();save.actors[0].restockUntilTick=save.tick+1;save.actors[0].stress=40;save.actors[0].reaction=HumanReaction.Aware;World w=World.Restore(save);Run(w,10);Require(w.ActorAt(0).restockPhase==RestockPhase.None && w.ActorAt(0).energySpent==EnergyRules.Maximum && !w.ActorAt(0).shoppingResting,"Expired shopping recovery preempted danger handling.");
        });
        Check("Infection prevents the shopping-timeout recovery override",()=>{
            var save=InFlightHungryShopper(true).Capture();save.actors[0].restockUntilTick=save.tick+1;save.actors[0].turnAtTick=save.tick+10;World w=World.Restore(save);w.Step();Require(!w.ActorAt(0).courierReturningHome && w.ActorAt(0).restockPhase==RestockPhase.None,"An infected shopper acquired safe timeout recovery.");
        });
        Check("Hungry proactive shoppers still return rather than search indefinitely",()=>{
            World w=InFlightHungryShopper();var before=w.ActorAt(0);int food=w.FoodSupplies().RemainingMeals;Run(w,5);var a=w.ActorAt(0);
            Require(a.restockPhase==RestockPhase.Returning && a.shoppingEndReason==ShoppingEndReason.Hunger && a.shoppingStoreId==before.shoppingStoreId && a.restockBox==before.restockBox && a.restockUntilTick==before.restockUntilTick,"Hungry collection did not retain the bounded home-return safety policy.");
            if(a.x!=before.x || a.y!=before.y)Require(a.energySpent>before.energySpent,"Return movement did not cost energy.");Require(w.FoodSupplies().RemainingMeals==food && a.carriedFood==0,"Starting a return fabricated food.");
            World copy=Documents.ParseSave(State(w));Run(w,30);Run(copy,30);Require(State(w)==State(copy),"Hungry return changed across save/reload.");
        });
        Check("A hungry empty-handed shopping return takes a paid safe off-road break",()=>{
            foreach(bool exhaustedStores in new[]{false,true}){World w=InFlightHungryShopper(true);if(exhaustedStores){var saved=w.Capture();saved.actors[0].shoppingStoreId=0;saved.actors[0].shoppingShelf=0;saved.actors[0].shoppingSearchCell=0;w=World.Restore(saved);}var legacySave=w.Capture();legacySave.rulesVersion=EncounterRules.NeedsPriorityVersion;World legacy=World.Restore(legacySave);var before=w.ActorAt(0);int food=w.FoodSupplies().RemainingMeals;Run(w,10);var a=w.ActorAt(0);
            Run(legacy,10);Require(!legacy.ActorAt(0).shoppingResting,"Rules107 unexpectedly gained paid hungry return recovery.");Require(a.shoppingResting && a.energySpent<before.energySpent && a.hunger>before.hunger,"Hunger suppressed paid recovery on an empty shopping return.");Require(a.x==before.x && a.y==before.y && a.restockBox==before.restockBox && a.restockUntilTick==before.restockUntilTick && a.restockPhase==RestockPhase.Returning,"Rest moved the actor, replaced the return or extended its deadline.");Require(w.FoodSupplies().RemainingMeals==food && a.carriedFood==0,"Rest created or disposed of food.");
            World copy=Documents.ParseSave(State(w));Run(w,25);Run(copy,25);Require(State(w)==State(copy),"Hungry return rest changed across save/reload.");}
        });
        Check("Published108 shopping retains its independent pre-fix continuation",()=>{
            var save=EncounterScenarios.LargeCityShoppingTest().Capture();save.rulesVersion=EncounterRules.ShoppingRecoveryVersion;World w=World.Restore(save);Run(w,1000);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(w.ShoppingRecoveryEnabled && !w.BoundedShoppingEnabled && hash=="823C1576FF9206A051D5A625426402BB2343B901B19990F2B104677C10E8F826","Published108 continuation silently changed.");
        });
        Check("Published rules107 preserve independent pre-fix shopping continuation",()=>{
            var saved=EncounterScenarios.LargeCityShoppingTest().Capture();saved.rulesVersion=EncounterRules.NeedsPriorityVersion;World w=World.Restore(saved);Run(w,1000);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(w.NeedsPriorityEnabled && !w.ShoppingRecoveryEnabled && w.Capture().rulesVersion==EncounterRules.NeedsPriorityVersion && hash=="B5972893867842E453B2E4F9D04BCCD86043B34802787940D644765A74FC5530","Published107 shopping or save continuation silently changed.");
        });
        Check("Danger still interrupts an active hungry shopping trip",()=>{
            var saved=InFlightHungryShopper().Capture();saved.actors[0].stress=40;saved.actors[0].reaction=HumanReaction.Aware;World w=World.Restore(saved);Run(w,5);Require(w.ActorAt(0).restockPhase==RestockPhase.Returning && !w.ActorAt(0).shoppingResting && w.ActorAt(0).shoppingEndReason==ShoppingEndReason.Danger,"Unsafe active shopper continued shopping or resting.");
        });
        Check("Larger city shopping reproduces old empty departures and completes a real shelf-to-home trip",()=>{
            var original=EncounterScenarios.LargeCityShoppingTest().Capture();var old=EncounterScenarios.LargeCityShoppingTest().Capture();old.rulesVersion=EncounterRules.ParkingTrafficVersion;old.actors[0].familiarStoreId=0;old.actors[0].additionalFamiliarStores=null;World legacy=World.Restore(old);bool left=false,returned=false;
            for(int t=0;t<250;t++){legacy.Step();var actor=legacy.ActorAt(0);left|=legacy.HouseAt(actor.x,actor.y)!=actor.assignedHouseId;returned|=left && actor.restockPhase==RestockPhase.Returning && actor.carriedFood==1;}
            Require(!legacy.ShoppingContinuityEnabled && left && returned && ShopperHomeFood(legacy)==0,"Earlier shopping rules did not reproduce the reported empty trip.");
            World world=World.Restore(original);int initial=world.FoodSupplies().RemainingMeals,eaten=0;bool resting=false,picked=false,away=false;
            for(int t=0;t<1600 && ShopperHomeFood(world)==0;t++){
                var before=world.ActorAt(0);world.Step();var actor=world.ActorAt(0);if(actor.hunger<before.hunger)eaten++;resting|=actor.shoppingResting;picked|=actor.shoppingShelf!=0 && actor.carriedFood>1;away|=world.HouseAt(actor.x,actor.y)!=actor.assignedHouseId;
                Require(world.FoodSupplies().RemainingMeals+eaten==initial,"City shopping fabricated or lost food.");if(actor.x!=before.x || actor.y!=before.y)Require(actor.energySpent>before.energySpent || before.energySpent==EnergyRules.Maximum,"Actual shopping travel did not cost energy.");
                if(t%101==0){World copy=Documents.ParseSave(State(world));for(int j=0;j<4;j++){int hunger=world.ActorAt(0).hunger;world.Step();copy.Step();if(world.ActorAt(0).hunger<hunger)eaten++;Require(world.FoodSupplies().RemainingMeals+eaten==initial,"Saved continuation lost food.");}Require(State(world)==State(copy),"Long shopping continuation diverged after save.");}
            }
            Require(away && resting && picked && ShopperHomeFood(world)>0,"City shopper failed walking, resting, pickup or home deposit.");
        });
        Check("Familiar shops reveal locations without revealing hidden shelf stock",()=>{
            World world=EncounterScenarios.LargeCityShoppingTest();var actor=world.ActorAt(0);Require(actor.familiarStoreId!=0 && actor.shelfMemory==null && actor.storeMemory==null,"Familiarity fabricated inventory or sight observations.");var changed=world.Capture();foreach(var shelf in changed.map.shelves)shelf.food=0;World empty=World.Restore(changed);
            for(int t=0;t<100;t++){world.Step();empty.Step();var a=world.ActorAt(0);var b=empty.ActorAt(0);Require(a.x==b.x && a.y==b.y && a.shoppingStoreId==b.shoppingStoreId && a.shoppingShelf==b.shoppingShelf,"Remote shelf contents affected travel before observation.");}
        });
        Check("Shopping rest saves retain their destination and danger interrupts the break",()=>{
            World world=EncounterScenarios.LargeCityShoppingTest();for(int t=0;t<800 && !world.ActorAt(0).shoppingResting;t++)world.Step();var actor=world.ActorAt(0);Require(actor.shoppingResting && actor.shoppingStoreId!=0 && !TerrainRules.RoadSurface(world.TerrainAt(actor.x,actor.y)),"Shopper never took a safe off-road break.");World copy=Documents.ParseSave(State(world));Run(world,35);Run(copy,35);Require(State(world)==State(copy),"Rest break changed after a save.");var saved=copy.Capture();saved.actors[0].stress=40;saved.actors[0].reaction=HumanReaction.Aware;world=World.Restore(saved);Run(world,6);Require(!world.ActorAt(0).shoppingResting && world.ActorAt(0).shoppingEndReason==ShoppingEndReason.Danger,"Unsafe shopper kept resting or lost its interruption reason.");
        });
        Check("Existing saves can explicitly gain location familiarity without gaining food knowledge",()=>{
            var saved=EncounterScenarios.LargeCityShoppingTest().Capture();saved.rulesVersion=EncounterRules.ParkingTrafficVersion;saved.actors[0].familiarStoreId=0;saved.actors[0].additionalFamiliarStores=null;World old=World.Restore(saved);Require(old.TryFamiliarizeResidentsWithShops()==0 && old.ActorAt(0).familiarStoreId==0,"Older rules gained familiarity implicitly.");saved.rulesVersion=EncounterRules.Version;World current=World.Restore(saved);Require(current.TryFamiliarizeResidentsWithShops()==1 && current.ActorAt(0).shelfMemory==null && current.ActorAt(0).storageMemory==null,"Explicit familiarity revealed shelf or cabinet stock.");Require(current.TryFamiliarizeResidentsWithShops()==0,"Familiarity overwrote an existing shop assignment.");Require(State(current)==State(Documents.ParseSave(State(current))),"Familiar location did not survive save.");
        });
        Check("Hungry shoppers with empty homes try a farther familiar store after the nearest is depleted",()=>{
            World setup=EncounterScenarios.StoreDiscoveryTest();var saved=setup.Capture();saved.actors[0].hunger=45;saved.actors[0].carriedFood=0;saved.actors[0].familiarStoreId=setup.HouseAt(11,6);saved.actors[0].additionalFamiliarStores=new[]{setup.HouseAt(18,13)};World world=World.Restore(saved);bool farther=false,fed=false;for(int t=0;t<850;t++){world.Step();var actor=world.ActorAt(0);farther|=world.HouseAt(actor.x,actor.y)==setup.HouseAt(18,13);fed|=actor.hunger<45;if(farther && fed && ShopperHomeFood(world)>0)break;}Require(farther && fed && ShopperHomeFood(world)>0,"Hungry shopper bounced home rather than obtaining food from the farther familiar store.");Require(State(world)==State(Documents.ParseSave(State(world))),"Alternate-store trip did not round-trip through save.");var detached=world.ActorAt(0);Require(detached.additionalFamiliarStores!=null,"Alternate familiar shop was lost.");detached.additionalFamiliarStores[0]=0;Require(world.ActorAt(0).additionalFamiliarStores[0]!=0,"Alternate familiarity escaped by reference.");
        });
        Check("Shopping continuity validates unfamiliar ids rest state and older rules",()=>{
            Action<Action<SaveDocument>> bad=change=>{var saved=EncounterScenarios.LargeCityShoppingTest().Capture();change(saved);Reject(()=>World.Restore(saved));};bad(s=>s.actors[0].familiarStoreId=-1);bad(s=>s.actors[0].familiarStoreId=s.actors[0].assignedHouseId);bad(s=>s.actors[0].shoppingResting=true);bad(s=>s.actors[0].shoppingEndReason=(ShoppingEndReason)99);bad(s=>s.rulesVersion=EncounterRules.ParkingTrafficVersion);
        });
    }
}
