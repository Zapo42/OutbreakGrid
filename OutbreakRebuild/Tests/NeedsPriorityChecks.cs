using System;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World LoadedNeedsCourier(bool current=true)
    {
        var setup=EncounterScenarios.CourierFulfillmentTest().Capture();setup.rulesVersion=EncounterRules.SocialFamiliarityVersion;World w=World.Restore(setup);
        for(int t=0;t<700;t++){var a=w.ActorAt(0);if(a.courierOrderId!=0 && a.restockPhase==RestockPhase.Returning && a.homeShelterId!=0 && a.carriedFood>1){var s=w.Capture();s.rulesVersion=current?EncounterRules.Version:EncounterRules.SocialFamiliarityVersion;s.actors[0].hunger=60;s.actors[0].energySpent=100;return World.Restore(s);}w.Step();}
        throw new Exception("No actual loaded return fixture.");
    }
    private static World HungryHomeWorker(bool clerk,int spent=0,int health=100)
    {
        var s=EncounterScenarios.WorkerShiftTest().Capture();s.tick=600;var a=s.actors[clerk?1:0];a.hunger=60;a.carriedFood=0;a.energySpent=spent;a.health=health;s.actors=new[]{a};return World.Restore(s);
    }
    private static World InteriorFurnitureNeeds(bool clerk,bool current=true)
    {
        // Two real floor components share one closed internal door; furniture remains solid and finite.
        WorldMap map=new WorldMap(12,12,"Furniture internal-door regression","npc143-interior");
        for(int y=1;y<=10;y++)for(int x=1;x<=10;x++)map.Set(x,y,x==1 || x==10 || y==1 || y==10 || y==6?Terrain.Wall:Terrain.Floor);
        map.Set(4,6,Terrain.DoorClosed);map.Set(10,8,Terrain.DoorClosed);
        map.SetStorageBox(new StorageBoxDocument{x=8,y=3,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=16}}});
        if(clerk){map.SetShelf(3,8,0,true);map.SetShelf(6,8,0,true);}
        map.SetBuildingRooms(new[]{new BuildingRoomsDocument{id="two-room-building",rooms=new[]{new BuildingRoomDocument{id="front",role="living",x=3,y=8},new BuildingRoomDocument{id="rear",role="storage",x=3,y=3}}}});
        World w=new World(map,214,encounter:true,hunger:true);int id;Require(w.TrySpawn(Species.Human,4,7,out id),"Interior spawn failed.");int room=w.HouseAt(4,7);
        if(clerk)Require(w.TryConfigureStore(8,3) && w.TryAssignClerk(0,room),"Interior clerk assignment failed.");else Require(w.TryAssignHouse(0,4,7),"Interior home assignment failed.");
        var save=w.Capture();save.rulesVersion=current?EncounterRules.Version:EncounterRules.SocialFamiliarityVersion;save.actors[0].shelterId=room;save.actors[0].shelterX=4;save.actors[0].shelterY=7;save.actors[0].shelterState=ShelterState.Inside;save.actors[0].hunger=10;save.actors[0].carriedFood=1;return World.Restore(save);
    }
    private static void CheckNeedsPriorityCases()
    {
        Check("A hungry courier eats one physical meal before returning and retains its shipment claim",()=>{
            World w=LoadedNeedsCourier();var before=w.ActorAt(0);int food=w.FoodSupplies().RemainingMeals;for(int t=0;t<12 && w.ActorAt(0).carriedFood==before.carriedFood;t++)w.Step();var a=w.ActorAt(0);
            Require(a.carriedFood==before.carriedFood-1 && a.hunger<=11 && w.FoodSupplies().RemainingMeals==food-1,"Priority meal did not consume exactly one physical ration.");Require(a.x==before.x && a.y==before.y && a.courierOrderId==before.courierOrderId && a.restockBox==before.restockBox && a.restockSourceBox==before.restockSourceBox && a.restockUntilTick==before.restockUntilTick,"Eating teleported, cancelled or recreated a shipment.");Require(State(w)==State(Documents.ParseSave(State(w))),"Meal action left invalid saved state.");
        });
        Check("An upgraded loaded return eats while its rules106 copy preserves old return-before-eating",()=>{
            World current=LoadedNeedsCourier(),old=LoadedNeedsCourier(false);int hand=old.ActorAt(0).carriedFood;Run(current,5);Run(old,5);Require(current.ActorAt(0).carriedFood==hand-1 && old.ActorAt(0).carriedFood==hand && !old.NeedsPriorityEnabled,"Meal priority was absent or silently changed older rules.");
        });
        Check("Off-duty hungry clerks and couriers start real home commutes without knowing home inventory",()=>{
            foreach(bool clerk in new[]{false,true}){World w=HungryHomeWorker(clerk);int home=w.ActorAt(0).assignedHouseId;Require(w.HomeSuppliesKnownBy(0).KnownBoxes==0,"Fixture invented home observations.");Run(w,5);Require(w.ActorAt(0).workerCommuteId==home,"Hunger blocked an explicit off-duty return.");bool arrived=false;for(int t=0;t<300;t++){w.Step();var a=w.ActorAt(0);if(w.HouseAt(a.x,a.y)==home){arrived=true;break;}}Require(arrived && w.AssignedOccupation(0)==(clerk?HumanOccupation.Clerk:HumanOccupation.Courier),"Worker did not physically arrive or lost employment.");}
        });
        Check("Injured exhausted off-duty workers rest at hunger cost and pay for each homeward step",()=>{
            foreach(bool clerk in new[]{false,true}){World w=HungryHomeWorker(clerk,100,40);int home=w.ActorAt(0).assignedHouseId,moves=0,rests=0;bool arrived=false;for(int t=0;t<450;t++){var before=w.ActorAt(0);w.Step();var a=w.ActorAt(0);if(a.x!=before.x || a.y!=before.y){moves++;Require(a.energySpent>before.energySpent || before.energySpent==100 && (a.hunger>before.hunger || before.hunger==100),"Home movement was free.");}if(a.energySpent<before.energySpent){rests++;Require(a.x==before.x && a.y==before.y && a.hunger>before.hunger,"Home-route recovery was free or moved the actor.");}if(w.HouseAt(a.x,a.y)==home){arrived=true;break;}}Require(arrived && moves>4 && rests>0,"Exhaustion/injury prevented the paid physical home return.");}
        });
        Check("Visible danger and infection retain priority over the new meal action",()=>{
            foreach(bool infected in new[]{false,true}){World w=LoadedNeedsCourier();var a=w.ActorAt(0);var s=w.Capture();s.actors[0].energySpent=0;if(infected)s.actors[0].turnAtTick=s.tick+10;w=World.Restore(s);if(!infected){int id;bool placed=false;foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)})if(!placed)placed=w.TrySpawn(Species.Zombie,a.x+d.Item1,a.y+d.Item2,out id);Require(placed,"Could not place adjacent threat.");}Run(w,5);Require(w.ActorAt(0).carriedFood==a.carriedFood,"Meal preempted danger or infection escape.");}
        });
        Check("Stress does not authorize a forced home commute",()=>{
            var s=HungryHomeWorker(true).Capture();s.actors[0].stress=80;s.actors[0].reaction=HumanReaction.Aware;World w=World.Restore(s);Run(w,5);Require(w.ActorAt(0).workerCommuteId==0,"Return priority ignored unsafe stress.");
        });
        Check("Taking a home commute releases an earlier food exploration goal before any save",()=>{
            var s=HungryHomeWorker(true).Capture();s.actors[0].foodExploreRoomId=s.actors[0].shelterId;s.actors[0].foodExploreUntilTick=s.tick+100;World w=World.Restore(s);Run(w,5);Require(w.ActorAt(0).foodExploreRoomId==0,"Home commute retained a competing exploration owner.");for(int t=0;t<150;t++){w.Step();Require(State(w)==State(Documents.ParseSave(State(w))),"Return route transition left invalid exploration state.");}
        });
        Check("New meal and rest home-route states continue exactly through saves",()=>{
            foreach(World initial in new[]{LoadedNeedsCourier(),HungryHomeWorker(true,100,40)}){World w=initial,copy=Documents.ParseSave(State(w));for(int t=0;t<260;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Needs priority save continuation diverged.");if(t%37==0)copy=Documents.ParseSave(State(w));}}
        });
        Check("Exhausted loaded couriers take a paid safe break without consuming delivery stock",()=>{
            var s=LoadedNeedsCourier().Capture();s.actors[0].x=8;s.actors[0].y=5;s.actors[0].hunger=20;s.actors[0].energySpent=98;World w=World.Restore(s);var before=w.ActorAt(0);int food=w.FoodSupplies().RemainingMeals;
            for(int t=0;t<5 && w.ActorAt(0).energySpent>=before.energySpent;t++)w.Step();var a=w.ActorAt(0);
            Require(a.energySpent==before.energySpent-EnergyRules.RestGain/2 && a.hunger>=before.hunger+1,"Courier did not exchange hunger for a normal-action recovery break.");Require(a.x==before.x && a.y==before.y && a.carriedFood==before.carriedFood && food==w.FoodSupplies().RemainingMeals && a.courierOrderId==before.courierOrderId,"Break moved stock, consumed cargo or lost the claim.");Require(State(w)==State(Documents.ParseSave(State(w))),"Courier break invalidated save.");
        });
        Check("Empty and reserve-only returns promptly release real claims without credit or discarded food",()=>{
            foreach(int hand in new[]{0,1})foreach(bool offDuty in new[]{false,true}){
                var s=LoadedNeedsCourier().Capture();s.actors[0].hunger=90;s.actors[0].carriedFood=hand;if(offDuty){s.actors[0].courierShiftStartHour=8;s.actors[0].courierShiftHours=1;}
                World w=World.Restore(s);int order=w.ActorAt(0).courierOrderId;int requested=0,credit=0;foreach(var o in w.PendingSupplyOrders()){requested+=o.requestedMeals;credit+=o.deliveredMeals;}
                bool released=false;for(int t=0;t<20;t++){w.Step();Require(State(w)==State(Documents.ParseSave(State(w))),"Claim release left an invalid immediate save.");if(w.SupplyOrderCourier(order)==0){released=true;break;}}
                int remainingRequests=0,afterCredit=0;foreach(var o in w.PendingSupplyOrders()){remainingRequests+=o.requestedMeals;afterCredit+=o.deliveredMeals;}
                Require(released && requested==remainingRequests && credit==afterCredit,"Unusable claim retained/order changed: hand="+hand+" offDuty="+offDuty+" released="+released+" requested="+requested+"/"+remainingRequests+" credit="+credit+"/"+afterCredit+" phase="+w.ActorAt(0).restockPhase);Require(w.ActorAt(0).carriedFood==0,"Needed meal was withheld or food was created.");
            }
        });
        Check("Hunger before pickup releases the collecting claim instead of routing an empty delivery",()=>{
            var s=LoadedNeedsCourier().Capture();s.actors[0].hunger=60;s.actors[0].carriedFood=0;s.actors[0].restockPhase=RestockPhase.Collecting;s.actors[0].homeShelterId=0;World w=World.Restore(s);int order=w.ActorAt(0).courierOrderId;
            Run(w,5);Require(w.SupplyOrderCourier(order)==0 && w.ActorAt(0).restockPhase==RestockPhase.None,"Hungry collection became an empty held delivery.");Require(State(w)==State(Documents.ParseSave(State(w))),"Failed collection cleanup left invalid state.");
        });
        Check("Dispatch waits for imminent personal hunger instead of reserving a meal-less pickup",()=>{
            foreach(bool current in new[]{true,false}){
                var s=LoadedNeedsCourier(current).Capture();object[] cleared={s.actors[0]};typeof(World).GetMethod("ClearRestocking",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,cleared);var a=(ActorState)cleared[0];
                a.x=3;a.y=5;a.shelterId=a.assignedHouseId;a.shelterX=3;a.shelterY=5;a.shelterState=ShelterState.Inside;a.homeShelterId=0;a.hunger=39;a.carriedFood=0;a.energySpent=0;cleared[0]=a;typeof(World).GetMethod("ClearShelterDeparture",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,cleared);s.actors[0]=(ActorState)cleared[0];World w=World.Restore(s);
                object[] arguments={0,w.ActorAt(0),-1};bool started=(bool)typeof(World).GetMethod("StartSupplyOrderTrip",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(w,arguments);
                Require(started!=current,"Hunger-marginal dispatch guard absent or leaked into106.");Require(State(w)==State(Documents.ParseSave(State(w))),"Dispatch check left invalid claim state.");
            }
        });
        Check("A courier resumes paid travel after rest and credits only food physically unloaded",()=>{
            var s=LoadedNeedsCourier().Capture();s.actors[0].x=8;s.actors[0].y=5;s.actors[0].hunger=20;s.actors[0].energySpent=98;World w=World.Restore(s);int credited=0,moves=0;bool rested=false;
            Func<int> total=()=>{int n=0;foreach(var o in w.PendingSupplyOrders())n+=o.deliveredMeals;foreach(var o in w.CompletedSupplyOrders())n+=o.deliveredMeals;return n;};
            for(int t=0;t<400 && credited<2;t++){var before=w.ActorAt(0);int old=total();w.Step();var a=w.ActorAt(0);int delta=total()-old;Require(delta>=0,"Credits decreased.");if(delta>0){Require(before.carriedFood-a.carriedFood>=delta,"Consumed meal was credited as delivery.");credited+=delta;}if(a.energySpent<before.energySpent)rested=true;if(a.x!=before.x || a.y!=before.y){moves++;Require(a.energySpent>before.energySpent || before.energySpent==100 && (a.hunger>before.hunger || before.hunger==100),"Courier movement was free.");}if(t%13==0)Require(State(w)==State(Documents.ParseSave(State(w))),"Loaded rest/return save invalid.");}
            Require(rested && moves>0 && credited>=2,"Rested courier failed to physically unload deliverable stock.");World copy=Documents.ParseSave(State(w));for(int t=0;t<100;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Courier recovery continuation diverged.");}
        });
        Check("Rules106 keeps the historical hungry empty-return claim until its old lifecycle resolves",()=>{
            var s=LoadedNeedsCourier(false).Capture();s.actors[0].hunger=90;s.actors[0].carriedFood=0;World w=World.Restore(s);int order=w.ActorAt(0).courierOrderId;Run(w,5);Require(w.SupplyOrderCourier(order)==w.ActorAt(0).id,"Empty return correction leaked into legacy106.");
        });
        Check("Clerk and pantry routes open an internal door as an action before paying for entry",()=>{
            foreach(bool clerk in new[]{true,false}){World w=InteriorFurnitureNeeds(clerk);var before=w.ActorAt(0);int food=w.FoodSupplies().RemainingMeals;Run(w,1);var opened=w.ActorAt(0);Require(w.TerrainAt(4,6)==Terrain.DoorOpen,"Furniture task did not open its closed next-step door.");Require(opened.x==before.x && opened.y==before.y && opened.energySpent==before.energySpent && food==w.FoodSupplies().RemainingMeals,"Opening moved the NPC, charged a move or changed food.");Require(opened.shelterX==8 || opened.shelterX==7 || opened.shelterY<6,"Opening discarded its furniture destination.");Run(w,5);var entered=w.ActorAt(0);Require(entered.x==4 && entered.y==6 && entered.energySpent==opened.energySpent+EnergyRules.MoveCost,"Door entry skipped a cell or failed to charge real movement.");Require(State(w)==State(Documents.ParseSave(State(w))),"Door action save invalid.");}
        });
        Check("An internal-door clerk physically withdraws stock and fills multiple shelves promptly",()=>{
            World w=InteriorFurnitureNeeds(true);Run(w,600);Require(w.ShelfAt(3,8).food>0 && w.ShelfAt(6,8).food>0,"Clerk did not stock both sales shelves before hunger forced an unrelated route.");Require(w.FoodSupplies().RemainingMeals==17,"Door handling fabricated or consumed finite stock in the short fed trial.");
        });
        Check("Pantry inspection learns only after physical access and all door states resume exactly",()=>{
            foreach(bool clerk in new[]{true,false}){World w=InteriorFurnitureNeeds(clerk);if(!clerk)Require(w.HomeSuppliesKnownBy(0).KnownBoxes==0,"Fixture invented stock observations.");Run(w,1);if(!clerk)Require(w.HomeSuppliesKnownBy(0).KnownBoxes==0,"Opening granted hidden pantry knowledge.");World copy=Documents.ParseSave(State(w));for(int t=0;t<180;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Internal furniture/door continuation diverged.");if(t%19==0)copy=Documents.ParseSave(State(w));}if(!clerk)Require(w.HomeSuppliesKnownBy(0).KnownBoxes>0,"Resident never physically inspected its pantry.");}
        });
        Check("Rules106 furniture tasks retain their independently reproduced closed-door behavior",()=>{
            foreach(bool clerk in new[]{true,false}){World w=InteriorFurnitureNeeds(clerk,false);Run(w,10);Require(w.TerrainAt(4,6)==Terrain.DoorClosed && w.ActorAt(0).x==4 && w.ActorAt(0).y==7,"Door correction silently upgraded106.");}
        });
        Check("Rules106 preserves independently recorded full worker continuation",()=>{
            var s=EncounterScenarios.WorkerShiftTest(true).Capture();s.rulesVersion=EncounterRules.SocialFamiliarityVersion;World w=World.Restore(s);Run(w,1000);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(!w.NeedsPriorityEnabled && hash=="DA53D4E02C456E017E76D4FCB2BDA12DA704EE6F8D3F6888E640689688C824EE","Old106 continuation changed from independent pre-fix source.");
        });
    }
}
