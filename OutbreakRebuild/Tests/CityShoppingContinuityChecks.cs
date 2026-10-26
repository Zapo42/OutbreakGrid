using System;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static int ShopperHomeFood(World world){var actor=world.ActorAt(0);int total=0;foreach(var box in world.StorageBoxSites())if(world.HouseAt(box.x,box.y)==actor.assignedHouseId)total+=StorageRules.Count(box,InventoryItem.Food);return total;}
    private static void CheckCityShoppingContinuityCases()
    {
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
