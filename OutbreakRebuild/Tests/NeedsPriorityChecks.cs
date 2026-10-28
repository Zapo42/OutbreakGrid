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
        Check("Rules106 preserves independently recorded full worker continuation",()=>{
            var s=EncounterScenarios.WorkerShiftTest(true).Capture();s.rulesVersion=EncounterRules.SocialFamiliarityVersion;World w=World.Restore(s);Run(w,1000);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(!w.NeedsPriorityEnabled && hash=="DA53D4E02C456E017E76D4FCB2BDA12DA704EE6F8D3F6888E640689688C824EE","Old106 continuation changed from independent pre-fix source.");
        });
    }
}
