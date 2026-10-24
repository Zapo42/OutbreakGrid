using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World ExhaustedHuman(int health=10,int energySpent=100,int hunger=100)
    {
        WorldMap map=new WorldMap(3,3);for(int y=0;y<3;y++)for(int x=0;x<3;x++)map.Set(x,y,Terrain.Wall);map.Set(1,1,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,1,1,out id);SaveDocument saved=world.Capture();saved.actors[0].health=health;saved.actors[0].energySpent=energySpent;saved.actors[0].hunger=hunger;return World.Restore(saved);
    }
    private static void CheckDeprivationCases()
    {
        Check("Maximum hunger and zero energy cause periodic saved health loss",()=>{
            World world=ExhaustedHuman(),copy=Documents.ParseSave(State(world));Run(world,29);Run(copy,29);Require(world.ActorAt(0).health==10,"Deprivation damaged before a needs interval.");world.Step();copy.Step();Require(world.ActorAt(0).health==9 && State(world)==State(copy),"Maximum hunger/empty energy did not damage or resume exactly.");Run(world,30);Require(world.ActorAt(0).health==8,"Health loss did not continue at the expected cadence.");
        });
        Check("Food or remaining energy prevent deprivation damage while legacy runs retain behavior",()=>{
            World world=ExhaustedHuman(10,99);Run(world,30);Require(world.ActorAt(0).health==10,"Nonempty energy caused deprivation damage.");world=ExhaustedHuman();SaveDocument saved=world.Capture();saved.actors[0].carriedFood=1;world=World.Restore(saved);Run(world,30);Require(world.ActorAt(0).health==10 && world.ActorAt(0).hunger<100,"Eating did not stop deprivation damage.");saved=ExhaustedHuman().Capture();saved.rulesVersion=EncounterRules.OrderFulfillmentVersion;world=World.Restore(saved);Run(world,30);Require(!world.SurvivalDamageEnabled && world.ActorAt(0).health==10,"Earlier rules silently gained survival damage.");
        });
        Check("Deprivation deaths clean up equipment and infected corpses retain their turn deadline",()=>{
            World world=ExhaustedHuman(1);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;world=World.Restore(saved);Run(world,30);ActorState dead=world.ActorAt(0);Require(dead.health==0 && dead.diedFromDeprivation && dead.species==Species.Human && dead.turnAtTick==0 && dead.meleeWeapon==MeleeWeapon.None && world.WeaponSites().Length==1 && State(Documents.ParseSave(State(world)))==State(world),"Starvation death retained active state, lost bat or infected a healthy corpse.");world=ExhaustedHuman(1);saved=world.Capture();saved.actors[0].turnAtTick=100;world=World.Restore(saved);Run(world,30);Require(world.ActorAt(0).health==0 && world.ActorAt(0).turnAtTick==100,"Infected deprivation death lost its reanimation timer.");Run(world,70);Require(world.ActorAt(0).species==Species.Zombie && !world.ActorAt(0).diedFromDeprivation,"Infected corpse failed conversion or kept human death state.");
        });
        Check("Courier deprivation death releases its claim without completing undelivered orders",()=>{
            World world=EncounterScenarios.CourierFulfillmentTest();Run(world,29);Require(world.ActorAt(0).courierOrderId==1,"Courier had not accepted order before death test.");SaveDocument saved=world.Capture();saved.actors[0].health=1;saved.actors[0].hunger=100;saved.actors[0].energySpent=100;world=World.Restore(saved);world.Step();Require(world.ActorAt(0).health==0 && world.ActorAt(0).courierOrderId==0 && world.SupplyOrderCourier(1)==0 && world.PendingSupplyOrders()[0].deliveredMeals==0 && world.CompletedSupplyOrders().Length==0 && State(Documents.ParseSave(State(world)))==State(world),"Courier death retained its order or credited lost cargo.");
        });
    }
}
