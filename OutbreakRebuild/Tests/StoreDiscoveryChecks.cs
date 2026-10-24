using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckStoreDiscoveryCases()
    {
        Check("No-memory shoppers discover entrances check an empty store and use an alternative before returning home",()=>{
            World world=EncounterScenarios.StoreDiscoveryTest(),copy=Documents.ParseSave(State(world));Require(world.ActorAt(0).storeMemory==null && world.ActorAt(0).shelfMemory==null,"Discovery fixture seeded store knowledge.");int eaten=0;ActorState previous=world.ActorAt(0);bool first=false,second=false,failed=false,loaded=false;
            for(int tick=0;tick<550;tick++)
            {
                world.Step();copy.Step();ActorState actor=world.ActorAt(0);Require(State(world)==State(copy),"Discovery continuation diverged.");if(actor.hunger<previous.hunger)eaten++;previous=actor;
                first|=world.HouseAt(actor.x,actor.y)==world.HouseAt(11,6);second|=world.HouseAt(actor.x,actor.y)==world.HouseAt(18,13);failed|=actor.storeFailures!=null;loaded|=actor.carriedFood==3;
                Require(actor.carriedFood<=HungerRules.CarryLimit && world.FoodSupplies().RemainingMeals+eaten==15,"Discovery created food or exceeded inventory.");if(tick%10==0)copy=Documents.ParseSave(State(world));
            }
            Require(first && second && failed && loaded && StorageRules.Count(world.StorageBoxAt(4,7),InventoryItem.Food)==2,"Discovery did not complete empty-store fallback and home deposit.");Require(world.ActorAt(0).storeMemory!=null && world.ActorAt(0).storeMemory.Length==2,"Observed store entrances were not remembered.");
        });
        Check("Hidden stock cannot influence initial store discovery or the chosen entrance",()=>{
            SaveDocument saved=EncounterScenarios.StoreDiscoveryTest().Capture();Array.Find(saved.map.shelves,s=>s.x==18).food=0;World empty=World.Restore(saved),stocked=EncounterScenarios.StoreDiscoveryTest();
            for(int tick=0;tick<70;tick++)
            {
                empty.Step();stocked.Step();ActorState a=empty.ActorAt(0),b=stocked.ActorAt(0);Require(a.x==b.x && a.y==b.y && a.shoppingStoreId==b.shoppingStoreId && a.shoppingShelf==b.shoppingShelf,"Unseen shelf stock changed discovery choices.");Require(a.shelfMemory==null || Array.TrueForAll(a.shelfMemory,n=>n.x!=18),"Remote shelf contents leaked through an entrance observation.");
            }
        });
        Check("All empty stores lead to a remembered failure and a bounded retry delay",()=>{
            SaveDocument saved=EncounterScenarios.StoreDiscoveryTest().Capture();Array.Find(saved.map.shelves,s=>s.x==18).food=0;World world=World.Restore(saved);bool delay=false,home=false;
            for(int tick=0;tick<650;tick++)
            {
                world.Step();ActorState actor=world.ActorAt(0);delay|=actor.shoppingRetryTick>world.Tick;home|=delay && world.HouseAt(actor.x,actor.y)==actor.assignedHouseId && actor.shoppingMeals==0;
                if(actor.shoppingRetryTick>world.Tick && actor.shoppingMeals==0)Require(actor.restockPhase==RestockPhase.None,"Waiting shopper retained a destination claim.");
            }
            Require(delay && home && StorageRules.Count(world.StorageBoxAt(4,7),InventoryItem.Food)==0,"Empty shops looped without returning home and waiting.");
        });
        Check("Legacy shopping rules preserve known-shelf-only choices and active trips upgrade without new knowledge",()=>{
            SaveDocument saved=EncounterScenarios.StoreShoppingTest().Capture();saved.rulesVersion=EncounterRules.StoreShoppingVersion;saved.actors[0].shelfMemory=null;World old=World.Restore(saved);Run(old,5);Require(!old.StoreDiscoveryEnabled && old.ActorAt(0).shoppingMeals==0 && old.ActorAt(0).storeMemory==null,"Old rules gained store discovery.");
            saved=EncounterScenarios.StoreShoppingTest().Capture();saved.rulesVersion=EncounterRules.StoreShoppingVersion;old=World.Restore(saved);Run(old,50);Require(old.ActorAt(0).shoppingShelf!=0 && old.ActorAt(0).shoppingStoreId==0,"Old trip did not retain its original representation.");saved=old.Capture();saved.rulesVersion=EncounterRules.Version;World upgraded=World.Restore(saved);Run(upgraded,400);Require(StorageRules.Count(upgraded.StorageBoxAt(4,3),InventoryItem.Food)>=2,"Upgraded active trip could not finish.");
        });
        Check("Discovery notes are detached expire and reject malformed or old-rule save data",()=>{
            World world=EncounterScenarios.StoreDiscoveryTest();Run(world,60);ActorState actor=world.ActorAt(0);Require(actor.storeMemory!=null && actor.storeMemory.Length==2,"Missing discovered entrances.");actor.storeMemory[0].roomId=0;Require(world.ActorAt(0).storeMemory[0].roomId!=0,"ActorAt exposed store memory by reference.");SaveDocument original=world.Capture();original.actors[0].storeMemory[0].roomId=0;Require(world.ActorAt(0).storeMemory[0].roomId!=0,"Capture exposed store memory by reference.");
            Action<Action<SaveDocument>> bad=change=>{SaveDocument s=world.Capture();change(s);Reject(()=>World.Restore(s));};bad(s=>s.rulesVersion=EncounterRules.StoreShoppingVersion);bad(s=>s.actors[0].storeMemory=new FoodRoomVisit[0]);bad(s=>s.actors[0].storeMemory[0].observedTick=s.tick+1);bad(s=>s.actors[0].shoppingStoreId=-1);bad(s=>s.actors[0].shoppingSearchCell=99999);bad(s=>s.actors[0].shoppingRetryTick=s.tick+StoreDiscoveryRules.RetryTicks+1);bad(s=>s.actors[0].storeFailures=new[]{new FoodRoomVisit{roomId=1,observedTick=s.tick}});
            SaveDocument stale=EncounterScenarios.StoreDiscoveryTest().Capture();stale.tick=StoreDiscoveryRules.MemoryTicks;stale.actors[0].storeMemory=new[]{new FoodRoomVisit{roomId=world.HouseAt(11,6),observedTick=0}};Reject(()=>World.Restore(stale));stale.actors[0].storeMemory[0].observedTick=1;World expiring=World.Restore(stale);expiring.Step();Require(expiring.ActorAt(0).storeMemory==null,"Store memory did not expire at its six-hour boundary.");
            stale=EncounterScenarios.StoreDiscoveryTest().Capture();stale.tick=StoreDiscoveryRules.RetryTicks;stale.actors[0].storeFailures=new[]{new FoodRoomVisit{roomId=world.HouseAt(11,6),observedTick=0}};Reject(()=>World.Restore(stale));stale.actors[0].storeFailures[0].observedTick=1;expiring=World.Restore(stale);expiring.Step();Require(expiring.ActorAt(0).storeFailures==null,"Failed-visit cooldown did not expire at its exact boundary.");
        });
        Check("Blocked store doors and low energy do not force entry or conjure supplies",()=>{
            SaveDocument saved=EncounterScenarios.StoreDiscoveryTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetStorageBox(new StorageBoxDocument{x=8,y=5,capacity=30,items=new ItemStackDocument[0]});map.SetStorageBox(new StorageBoxDocument{x=15,y=12,capacity=30,items=new ItemStackDocument[0]});saved.map=map.Capture();World blocked=World.Restore(saved);Run(blocked,300);Require(StorageRules.Count(blocked.StorageBoxAt(4,7),InventoryItem.Food)==0,"Blocked entrance granted a hidden-stock pickup.");
            saved=EncounterScenarios.StoreDiscoveryTest().Capture();saved.actors[0].energySpent=80;World tired=World.Restore(saved);tired.Step();Require(tired.ActorAt(0).shoppingMeals==0 && tired.ActorAt(0).storeMemory==null,"Exhausted human started a discovery trip or knew unseen stores.");
        });
    }
}
