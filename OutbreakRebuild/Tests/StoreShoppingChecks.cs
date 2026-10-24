using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World LoadedShopper()
    {
        World world=EncounterScenarios.StoreShoppingTest();for(int i=0;i<250 && world.ActorAt(0).restockPhase!=RestockPhase.Returning;i++)world.Step();Require(world.ActorAt(0).shoppingShelf!=0 && world.ActorAt(0).carriedFood==3,"Shopper never collected a load.");return world;
    }
    private static void CheckStoreShoppingCases()
    {
        Check("Home shopping makes finite shelf pickups and home deposits while the clerk refills shelves",()=>{
            World world=EncounterScenarios.StoreShoppingTest(),copy=Documents.ParseSave(State(world));int eaten=0;ActorState[] previous=world.Capture().actors;bool collecting=false,returning=false,partial=false,refilled=false;
            for(int tick=0;tick<600;tick++)
            {
                world.Step();copy.Step();Require(State(world)==State(copy),"Shopping continuation diverged.");for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.hunger<previous[i].hunger)eaten++;previous[i]=actor;Require(actor.carriedFood<=HungerRules.CarryLimit,"Shopping exceeded inventory capacity.");}
                collecting|=world.ActorAt(0).shoppingShelf!=0 && world.ActorAt(0).restockPhase==RestockPhase.Collecting;returning|=world.ActorAt(0).shoppingShelf!=0 && world.ActorAt(0).restockPhase==RestockPhase.Returning;partial|=StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==2;refilled|=returning && world.ShelfAt(13,7).food==6 && StorageRules.Count(world.StorageBoxAt(14,3),InventoryItem.Food)<12;
                Require(world.FoodSupplies().RemainingMeals+eaten==20,"Shopping created, duplicated or lost food.");if(tick%25==0)copy=Documents.ParseSave(State(world));
            }
            Require(collecting && returning && partial && refilled && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==4 && world.ActorAt(0).shoppingShelf==0,"Shopping failed its partial/complete home restock or shelf refill.");
        });
        Check("Intelligence plans earlier while empty supplies still trigger less intelligent residents",()=>{
            SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();Array.Find(saved.map.storageBoxes,b=>b.x==4).items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=2}};saved.actors[0].intelligence=0;World low=World.Restore(saved);saved.actors[0].intelligence=10;World high=World.Restore(saved);low.Step();high.Step();Require(low.ActorAt(0).shoppingShelf==0 && high.ActorAt(0).shoppingShelf!=0,"Intelligence did not change early planning.");saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].intelligence=0;World empty=World.Restore(saved);empty.Step();Require(empty.ActorAt(0).shoppingShelf!=0,"Low intelligence prevented shopping at empty stock.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();Array.Find(saved.map.storageBoxes,b=>b.x==4).items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=4}};World full=World.Restore(saved);Run(full,10);Require(full.ActorAt(0).shoppingShelf==0,"Already stocked home triggered unnecessary shopping.");
        });
        Check("Store shopping requires fresh personal home knowledge and a known consumer shelf",()=>{
            SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].shelfMemory=null;World unknown=World.Restore(saved);unknown.Step();Require(unknown.ActorAt(0).shoppingShelf==0,"Planner used an unseen shelf's live stock.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].shelfMemory[0].food=0;World empty=World.Restore(saved);empty.Step();Require(empty.ActorAt(0).shoppingShelf==0,"Remembered empty shelf was selected.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].x=2;saved.actors[0].y=7;saved.actors[0].shelterX=2;saved.actors[0].shelterY=7; // Wall occlusion is tested by existing home checks; missing notes do not become global stock totals.
            World checkedHome=World.Restore(saved);checkedHome.Step();Require(checkedHome.HomeSuppliesKnownBy(0).KnownBoxes==1,"Visible home chest was not personally checked.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.map.shelves[0].searched=false;saved.map.shelves[0].food=0;Array.Find(saved.map.storageBoxes,b=>b.x==14).items=new ItemStackDocument[0];saved.actors[0].shelfMemory[0].food=-1;World unsearched=World.Restore(saved);unsearched.Step();Require(unsearched.ActorAt(0).shoppingShelf!=0,"Unsearched known shelf used hidden emptiness to prevent a visit.");Run(unsearched,250);Require(StorageRules.Count(unsearched.StorageBoxAt(4,3),InventoryItem.Food)==0,"Searching an empty shelf created a shipment.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.rulesVersion=EncounterRules.HomeSuppliesVersion;World legacy=World.Restore(saved);legacy.Step();Require(!legacy.StoreShoppingEnabled && legacy.ActorAt(0).shoppingShelf==0 && legacy.Capture().rulesVersion==EncounterRules.HomeSuppliesVersion,"Legacy home-check rules gained new shopping behavior.");
        });
        Check("Empty stale store shelves cause a safe empty return and full home boxes retain carried food",()=>{
            SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();Array.Find(saved.map.shelves,s=>s.x==13).food=0;Array.Find(saved.map.storageBoxes,b=>b.x==14).items=new ItemStackDocument[0];World empty=World.Restore(saved);bool returning=false;for(int i=0;i<300;i++){empty.Step();returning|=empty.ActorAt(0).shoppingMeals>0 && empty.ActorAt(0).restockPhase==RestockPhase.Returning;}Require(returning && StorageRules.Count(empty.StorageBoxAt(4,3),InventoryItem.Food)==0,"Stale shelf contents were treated as real goods.");
            World loaded=LoadedShopper();saved=loaded.Capture();Array.Find(saved.map.storageBoxes,b=>b.x==4).items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}};loaded=World.Restore(saved);int food=loaded.FoodSupplies().RemainingMeals;for(int i=0;i<220 && loaded.ActorAt(0).shoppingShelf!=0;i++)loaded.Step();Require(loaded.ActorAt(0).shoppingShelf==0 && loaded.ActorAt(0).carriedFood==3 && loaded.FoodSupplies().RemainingMeals==food,"Full home box deleted cargo or overflowed inventory.");
        });
        Check("Shopping keeps a personal meal when the resident starts without a ration",()=>{
            SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].carriedFood=0;World world=World.Restore(saved);for(int i=0;i<250 && world.ActorAt(0).restockPhase!=RestockPhase.Returning;i++)world.Step();Require(world.ActorAt(0).carriedFood==3 && world.ActorAt(0).shoppingMeals==2,"Missing personal ration reduced or overflowed the planned shipment.");for(int i=0;i<200 && StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==0;i++)world.Step();Require(StorageRules.Count(world.StorageBoxAt(4,3),InventoryItem.Food)==2 && world.ActorAt(0).carriedFood==1,"Home deposit failed to protect the personal ration.");
        });
        Check("A household does not send two shoppers to fill the same home box",()=>{
            World world=EncounterScenarios.StoreShoppingTest();int id;Require(world.TrySpawn(Species.Human,2,5,out id) && world.TryAssignHouse(2,2,5),"Could not add another household consumer.");SaveDocument saved=world.Capture();saved.actors[2].hunger=10;saved.actors[2].carriedFood=1;saved.actors[2].shelterId=world.HouseAt(2,5);saved.actors[2].shelterX=2;saved.actors[2].shelterY=5;saved.actors[2].shelterState=ShelterState.Inside;saved.actors[2].shelfMemory=new[]{new ShelfMemoryRecord{x=13,y=7,food=6,observedTick=0}};world=World.Restore(saved);Run(world,5);int jobs=0;for(int i=0;i<world.Count;i++)if(world.ActorAt(i).shoppingShelf!=0)jobs++;Require(jobs==1,"Two residents duplicated the home-box shopping claim.");Require(State(world)==State(Documents.ParseSave(State(world))),"Household shopping claim could not reload.");
        });
        Check("Hunger danger infection expiry and invalid shopping saves do not fabricate deliveries",()=>{
            World loaded=LoadedShopper();SaveDocument saved=loaded.Capture();saved.actors[0].hunger=80;World hungry=World.Restore(saved);int food=hungry.FoodSupplies().RemainingMeals;Run(hungry,5);Require(hungry.ActorAt(0).hunger<80 && hungry.FoodSupplies().RemainingMeals==food-1,"Returning shopper failed to eat or duplicated its meal.");
            saved=loaded.Capture();saved.actors[0].turnAtTick=saved.tick+InfectionRules.Delay(saved.actors[0],1);World infected=World.Restore(saved);infected.Step();Require(infected.ActorAt(0).shoppingShelf==0 && infected.ActorAt(0).shoppingMeals==0,"Infection retained a stale shopping task.");
            saved=loaded.Capture();saved.actors[0].restockUntilTick=saved.tick+1;World expired=World.Restore(saved);expired.Step();Require(expired.ActorAt(0).shoppingShelf==0 && expired.ActorAt(0).carriedFood==3,"Expired task lost goods or retained its claim.");
            Action<Action<SaveDocument>> bad=change=>{SaveDocument s=loaded.Capture();change(s);Reject(()=>World.Restore(s));};bad(s=>s.rulesVersion=EncounterRules.HomeSuppliesVersion);bad(s=>s.actors[0].shoppingShelf=-1);bad(s=>s.actors[0].shoppingMeals=3);bad(s=>s.actors[0].shoppingMeals=0);bad(s=>s.actors[0].restockHomeId=loaded.HouseAt(12,5));
            World threatened=EncounterScenarios.StoreShoppingTest();Run(threatened,50);ActorState shopper=threatened.ActorAt(0);int zombie;Require(threatened.TrySpawn(Species.Zombie,shopper.x+2,shopper.y,out zombie),"Could not create shopping-route threat.");Run(threatened,5);Require(threatened.ActorAt(0).shoppingShelf==0 || threatened.ActorAt(0).restockPhase==RestockPhase.Returning,"Visible danger failed to interrupt outbound shopping.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.actors[0].energySpent=80;World tired=World.Restore(saved);tired.Step();Require(tired.ActorAt(0).shoppingShelf==0,"Low-energy resident began a new shopping trip.");
        });
    }
}
