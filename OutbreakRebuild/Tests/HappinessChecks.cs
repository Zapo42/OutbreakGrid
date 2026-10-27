using System;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    // Keep people physically stationary so preference tests cannot confuse movement with a mood effect.
    private static World HappinessBox(bool indoors=false,int charisma=5,int companions=0,bool hidden=false)
    {
        WorldMap map=new WorldMap(11,9,"Happiness observation cells","happiness-qa-1");for(int y=0;y<9;y++)for(int x=0;x<11;x++)map.Set(x,y,Terrain.Wall);
        map.Set(3,3,indoors?Terrain.Floor:Terrain.Grass);if(indoors)map.Set(4,3,Terrain.DoorClosed);
        if(hidden)map.Set(5,3,Terrain.Grass);if(companions>3)map.Set(3,4,indoors?Terrain.Floor:Terrain.Grass);
        World world=new World(map,214,encounter:true,hunger:true);int id;Require(world.TrySpawn(Species.Human,3,3,out id),"Could not place happiness observer.");
        for(int i=0;i<companions;i++)Require(world.TrySpawn(Species.Human,hidden?5:3,!hidden && i>=3?4:3,out id),"Could not place happiness companion.");
        var saved=world.Capture();for(int i=0;i<saved.actors.Length;i++){saved.actors[i].charisma=charisma;saved.actors[i].hunger=0;saved.actors[i].carriedFood=1;if(indoors){saved.actors[i].assignedHouseId=world.HouseAt(3,3);saved.actors[i].nextLeisureTick=600;}}
        return World.Restore(saved);
    }
    private static string WithoutHappiness(World world)
    {
        var saved=world.Capture();saved.rulesVersion="encounter-prototype-100";for(int i=0;i<saved.actors.Length;i++){saved.actors[i].happiness=0;saved.actors[i].nextHappinessTick=0;}return Documents.Encode(saved);
    }
    private static void CheckHappinessCases()
    {
        Check("Happiness starts neutral and reflects opposing social and environment preferences",()=>{
            foreach(int charisma in new[]{2,5,8})foreach(bool indoors in new[]{false,true})foreach(bool company in new[]{false,true}){
                World world=HappinessBox(indoors,charisma,company?1:0);var before=world.HappinessKnownBy(0);Require(world.HappinessEnabled && world.ActorAt(0).happiness==0,"Happiness was not initially neutral.");Require(before.Available && before.Indoors==indoors && before.NearbyPeople==(company?1:0),"Observed environment or company was wrong.");
                if(charisma==5)Require(before.SocialContribution==0 && before.EnvironmentContribution==0,"Neutral charisma invented a preference.");else{
                    bool socialGood=charisma<5?!company:company,environmentGood=charisma<5?indoors:!indoors;
                    Require((socialGood?before.SocialContribution>0:before.SocialContribution<0) && (environmentGood?before.EnvironmentContribution>0:before.EnvironmentContribution<0),"Charisma did not balance opposing preferences.");
                }
                world.Step();Require(world.ActorAt(0).happiness==before.Change,"First bounded observation did not apply its stated contribution.");
            }
        });
        Check("Happiness updates on a bounded cadence and observer queries cannot advance it",()=>{
            World world=HappinessBox(true,2);world.Step();int value=world.ActorAt(0).happiness;long next=world.ActorAt(0).nextHappinessTick;Require(value>0 && next==world.Tick+HappinessRules.CadenceTicks,"First pulse did not schedule the cadence.");string state=State(world);
            for(int i=0;i<30;i++)world.HappinessKnownBy(0);Require(State(world)==state,"Inspector queries changed the authoritative world.");TickClock clock=new TickClock();clock.Advance(999,world.Step);Require(State(world)==state,"Paused time advanced happiness.");
            while(world.Tick+1<next){world.Step();Require(world.ActorAt(0).happiness==value,"Happiness advanced between sample ticks.");}world.Step();Require(world.ActorAt(0).happiness>value,"Scheduled happiness pulse did not occur.");
        });
        Check("Visible living companions count while walls and dead people do not",()=>{
            World seen=HappinessBox(false,8,1),hidden=HappinessBox(false,8,1,true);Require(seen.HappinessKnownBy(0).NearbyPeople==1 && hidden.HappinessKnownBy(0).NearbyPeople==0,"Company ignored line of sight.");var saved=seen.Capture();saved.actors[1].health=0;saved.actors[1].carriedFood=0;World dead=World.Restore(saved);Require(dead.HappinessKnownBy(0).NearbyPeople==0,"A dead companion still counted as company.");
            World crowded=HappinessBox(false,8,5);Require(crowded.HappinessKnownBy(0).NearbyPeople==4,"Nearby company did not apply its bounded cap.");
        });
        Check("Vehicle riders pause their own happiness and are excluded from pedestrian company",()=>{
            World world=EncounterScenarios.ConsumerVehicleShoppingTest();for(int t=0;t<400 && !world.ActorInVehicle(0);t++)world.Step();Require(world.ActorInVehicle(0),"Vehicle fixture never boarded.");var rider=world.HappinessKnownBy(0);Require(rider.Available && rider.Paused && rider.Change==0,"Seated rider still evaluated an outdoor social preference.");var car=world.VehicleAtIndex(0);int observer=world.Count,id;bool placed=false;
            foreach(var offset in new[]{(1,0),(-1,0),(0,1),(0,-1)})if(!placed)placed=world.TrySpawn(Species.Human,car.x+offset.Item1,car.y+offset.Item2,out id);
            Require(placed && world.HappinessKnownBy(observer).NearbyPeople==0,"Seated driver counted as a nearby pedestrian.");
        });
        Check("Happiness clamps at both signed limits and danger cannot produce a positive gain",()=>{
            foreach(bool positive in new[]{false,true}){
                var saved=HappinessBox(false,positive?8:2,1).Capture();saved.actors[0].happiness=positive?49:-49;World world=World.Restore(saved);world.Step();Require(world.ActorAt(0).happiness==(positive?50:-50),"Happiness escaped its signed range.");
            }
            var danger=HappinessBox(false,8,1).Capture();danger.actors[0].stress=80;danger.actors[0].reaction=HumanReaction.Aware;World unsafeWorld=World.Restore(danger);Require(unsafeWorld.HappinessKnownBy(0).Danger && unsafeWorld.HappinessKnownBy(0).Change<=0,"Stress allowed positive happiness.");unsafeWorld.Step();Require(unsafeWorld.ActorAt(0).happiness<=0,"Danger increased happiness.");
        });
        Check("Only an observed living zombie blocks otherwise positive happiness",()=>{
            foreach(bool hidden in new[]{false,true}){
                var saved=HappinessBox(false,8,1).Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(5,3,Terrain.Grass);if(!hidden)map.Set(4,3,Terrain.Grass);saved.map=map.Capture();World world=World.Restore(saved);int id;Require(world.TrySpawn(Species.Zombie,5,3,out id),"Could not place happiness danger fixture.");var observed=world.HappinessKnownBy(0);Require(observed.Danger==!hidden && (hidden?observed.Change>0:observed.Change==0),"Hidden zombie knowledge or visible danger changed positive mood incorrectly.");
                if(!hidden){saved=world.Capture();saved.actors[2].health=0;World dead=World.Restore(saved);Require(!dead.HappinessKnownBy(0).Danger && dead.HappinessKnownBy(0).Change>0,"Dead zombie still suppressed a positive mood.");}
            }
        });
        Check("Happiness save continuation retains both pulse timing and all existing simulation state",()=>{
            World world=EncounterScenarios.HappinessPreferencesTest();Run(world,3);World copy=Documents.ParseSave(State(world));for(int t=0;t<250;t++){world.Step();copy.Step();Require(State(world)==State(copy),"Happiness pulse or saved world diverged after loading.");if(t==119)copy=Documents.ParseSave(State(world));}
            World current=EncounterScenarios.HumanLeisureTest();var saved=current.Capture();saved.rulesVersion="encounter-prototype-100";World previous=World.Restore(saved);
            for(int t=0;t<300;t++){current.Step();previous.Step();Require(WithoutHappiness(current)==State(previous),"Happiness changed movement, energy, needs, inventories or random streams.");}
        });
        Check("Death and turning clear saved human happiness and make the observer unavailable",()=>{
            var saved=HappinessBox().Capture();saved.actors[0].happiness=40;saved.actors[0].health=1;saved.actors[0].hunger=100;saved.actors[0].energySpent=100;saved.actors[0].carriedFood=0;World death=World.Restore(saved);Run(death,30);var dead=death.ActorAt(0);Require(dead.health==0 && dead.happiness==0 && dead.nextHappinessTick==0 && !death.HappinessKnownBy(0).Available,"Death kept active happiness.");Require(State(death)==State(Documents.ParseSave(State(death))),"Dead person's happiness save is invalid.");
            saved=HappinessBox().Capture();saved.actors[0].happiness=-20;saved.actors[0].turnAtTick=10;World turn=World.Restore(saved);Run(turn,10);var zombie=turn.ActorAt(0);Require(zombie.species==Species.Zombie && zombie.happiness==0 && zombie.nextHappinessTick==0 && !turn.HappinessKnownBy(0).Available,"Turning retained human happiness.");
        });
        Check("Happiness rejects corrupt ranges clocks and older-rule state transactionally",()=>{
            Action<Action<SaveDocument>> bad=change=>{var saved=HappinessBox().Capture();change(saved);Reject(()=>World.Restore(saved));};bad(s=>s.actors[0].happiness=-51);bad(s=>s.actors[0].happiness=51);bad(s=>s.actors[0].nextHappinessTick=-1);bad(s=>s.actors[0].nextHappinessTick=HappinessRules.CadenceTicks+1);bad(s=>{s.actors[0].happiness=1;s.rulesVersion="encounter-prototype-100";});bad(s=>{s.actors[0].nextHappinessTick=1;s.rulesVersion="encounter-prototype-100";});bad(s=>{s.actors[0].health=0;s.actors[0].happiness=1;});
            World world=HappinessBox();world.Step();var expired=world.Capture();expired.actors[0].nextHappinessTick=expired.tick;Reject(()=>World.Restore(expired));
        });
        Check("Legacy rules100 retains independently recorded pre-happiness continuation",()=>{
            var saved=EncounterScenarios.HomeSupplyInspectionTest().Capture();saved.rulesVersion="encounter-prototype-100";World legacy=World.Restore(saved);Require(!legacy.HappinessEnabled,"Older run silently enabled happiness.");Run(legacy,600);string digest;using(var sha=SHA256.Create())digest=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(legacy)))).Replace("-","");Require(digest=="B6454D3C9612FC00540E4AEFF1556C95741653153B37BACE1916FF2E10DBA7EB","Rules100 continuation changed from the independent baseline.");
        });
    }
}
