using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckPhoneOrderCases()
    {
        Check("Clerks walk to a phone and make one saved replenishment order for an empty crate",()=>{
            World world=EncounterScenarios.PhoneSupplyOrderTest();World copy=Documents.ParseSave(State(world));bool walked=false,calling=false;
            for(int i=0;i<60 && world.PendingSupplyOrders().Length==0;i++){world.Step();copy.Step();Require(State(world)==State(copy),"Saved phone task diverged.");ActorState actor=world.ActorAt(0);walked|=actor.x!=3 || actor.y!=3;calling|=actor.clerkCallProgress==1;}
            Require(walked && calling && world.PendingSupplyOrders().Length==1,"Clerk skipped walking/calling or did not make an order.");SupplyOrderDocument order=world.PendingSupplyOrders()[0];Require(order.id==1 && order.storeId==27 && order.supplyBox==41 && order.phone==63 && order.requestedMeals==30 && order.requestedBy==1 && order.createdTick<=world.Tick,"Order did not identify actual phone, clerk and crate capacity.");int meals=world.FoodSupplies().RemainingMeals;Run(world,60);Require(world.PendingSupplyOrders().Length==1 && world.FoodSupplies().RemainingMeals==meals && !world.ActorAt(0).clerkNeedsSupplies,"Pending order duplicated calls or created supplies.");
        });
        Check("No phone or an unreachable phone prevents calls and does not teleport the clerk",()=>{
            SaveDocument saved=EncounterScenarios.PhoneSupplyOrderTest().Capture();saved.map.phoneBlocks=null;saved.map.schemaVersion=8;World world=World.Restore(saved);Run(world,40);Require(world.PendingSupplyOrders().Length==0,"Clerk called without a phone.");saved=EncounterScenarios.PhoneSupplyOrderTest().Capture();saved.actors[1].x=5;saved.actors[1].y=5;saved.actors[1].shelterX=5;saved.actors[1].shelterY=5;WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(2,4,0,true);map.SetShelf(3,5,0,true);saved.map=map.Capture();world=World.Restore(saved);Run(world,40);Require(world.PendingSupplyOrders().Length==0 && !world.TrySpawn(Species.Human,2,5,out int id),"Blocked phone was used or occupied phone tile admitted a body.");
        });
        Check("Multiple clerks share one pending order and snapshots detach order records",()=>{
            World world=EncounterScenarios.PhoneSupplyOrderTest();int id;Require(world.TrySpawn(Species.Human,5,3,out id) && world.TryAssignClerk(2,27),"Could not add a second clerk.");Run(world,60);Require(world.PendingSupplyOrders().Length==1 && world.PendingSupplyOrders()[0].id==1,"Concurrent clerks duplicated a store order.");string before=State(world);SupplyOrderDocument[] records=world.PendingSupplyOrders();records[0].requestedMeals=1;SaveDocument saved=world.Capture();saved.supplyOrders[0].id=999;Require(State(world)==before && State(Documents.ParseSave(before))==before,"Pending order records escaped by reference or failed JSON continuation.");
        });
        Check("Refilled crates cancel an unfinished call and infection interrupts phone use",()=>{
            World world=EncounterScenarios.PhoneSupplyOrderTest();for(int i=0;i<30 && world.ActorAt(0).clerkCallProgress==0;i++)world.Step();Require(world.ActorAt(0).clerkCallProgress==1,"Could not reach a saved call in progress.");SaveDocument saved=world.Capture();saved.map.storageBoxes[0].items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=2}};World refilled=World.Restore(saved);Run(refilled,5);Require(refilled.PendingSupplyOrders().Length==0 && refilled.ActorAt(0).clerkCallProgress==0,"Clerk called after observing replenished stock.");saved.actors[0].turnAtTick=saved.tick+100;saved.map.storageBoxes[0].items=new ItemStackDocument[0];World infected=World.Restore(saved);Run(infected,5);Require(infected.PendingSupplyOrders().Length==0 && infected.ActorAt(0).clerkPhone==0,"Infection did not interrupt the call.");
        });
        Check("Phone blocks are solid preserved map furniture with strict overlap and version checks",()=>{
            World world=EncounterScenarios.PhoneSupplyOrderTest();MapDocument map=world.Capture().map;Require(map.schemaVersion==11 && map.phoneBlocks.Length==1 && !WorldMap.Restore(map).CanWalk(2,5),"Phone map schema or collision was lost.");WorldMap copy=WorldMap.Restore(map);Reject(()=>copy.SetShelf(2,5,0));Reject(()=>copy.SetStorageBox(new StorageBoxDocument{x=2,y=5,capacity=30,items=new ItemStackDocument[0]}));Reject(()=>copy.SetRestSpot(2,5));Reject(()=>copy.SetWeapon(2,5,MeleeWeapon.Bat));map.schemaVersion=10;Reject(()=>WorldMap.Restore(map));Require(!world.TryPlacePhone(3,3) && !world.TryPlacePhone(4,3) && !world.TryPlacePhone(0,0),"Phone placement overlapped a body, furniture or outdoor terrain.");
        });
        Check("Order validation rejects wrong phones crates identities and unsupported rules",()=>{
            World world=EncounterScenarios.PhoneSupplyOrderTest();Run(world,20);Require(world.PendingSupplyOrders().Length==1,"Could not create validation order.");Action<Action<SaveDocument>> bad=change=>{SaveDocument saved=world.Capture();change(saved);Reject(()=>World.Restore(saved));};bad(s=>s.rulesVersion=EncounterRules.DistributionCentersVersion);bad(s=>s.supplyOrders[0].phone=1);bad(s=>s.supplyOrders[0].supplyBox=1);bad(s=>s.supplyOrders[0].requestedBy=999);bad(s=>s.supplyOrders[0].requestedMeals=31);bad(s=>s.supplyOrders[0].createdTick=s.tick+1);bad(s=>s.nextSupplyOrderId=1);bad(s=>s.supplyOrders=new[]{s.supplyOrders[0],s.supplyOrders[0]});bad(s=>s.actors[0].clerkPhone=1);SaveDocument old=EncounterScenarios.StoreClerkTest().Capture();old.rulesVersion=EncounterRules.DistributionCentersVersion;World legacy=World.Restore(old);Require(!legacy.SupplyRequestsEnabled && !legacy.TryPlacePhone(2,5),"Older center rules gained phones.");
        });
    }
}
