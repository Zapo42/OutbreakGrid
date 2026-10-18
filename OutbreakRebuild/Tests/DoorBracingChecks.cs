using System; // Reject invalid defensive state before replacing a world.
using System.IO; // Read independent pre-bracing traces.
using Outbreak.Contracts; // Inspect detached action and map records.
using Outbreak.Core; // Run real human opportunities and incoming attacks.
using Outbreak.Persistence; // Verify exact portable continuation.
public static partial class Program
{
    private static void CheckDoorBracingCases()
    {
        Check("A sheltered listener braces only after hearing a real strike and halves later damage", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,2);Require(w.DoorHealthAt(3,2)==84 && w.ActorAt(0).bracedDoor==0,"Door gained defense before a human action.");Run(w,4);Require(w.ActorAt(0).bracedDoor==28 && w.ActorAt(0).braceUntilTick==11 && w.DoorIsBraced(3,2),"Adjacent listener failed to physically brace.");w.Step();Require(w.DoorHealthAt(3,2)==76 && w.ActorAt(0).x==2 && w.ActorAt(0).y==2,"Bracing moved or repaired the human or failed to reduce damage.");Run(w,25);Require(w.DoorHealthAt(3,2)==36,"Repeated defense did not buy the expected time.");Inspect(w);
        });
        Check("Bracing buys time but sustained attacks still break the door and release defense", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,56);Require(w.DoorHealthAt(3,2)==4 && w.DoorIsBraced(3,2),"Defense became invulnerable or ended early.");w.Step();Require(w.DoorHealthAt(3,2)==0 && w.TerrainAt(3,2)==Terrain.DoorOpen && w.ActorAt(0).bracedDoor==0 && !w.DoorIsBraced(3,2),"A breach retained a solid door or stale defense.");Require(State(w)==State(Documents.ParseSave(State(w))),"Breach state failed validation.");Run(w,10);Require(w.ActorAt(0).turnAtTick>w.Tick,"Breach failed to resume ordinary contact.");
        });
        Check("Quiet ends bracing at the hearing deadline and ordinary recovery resumes", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,6);SaveDocument s=w.Capture();Array.Resize(ref s.actors,1);w=World.Restore(s);Run(w,20);Require(w.ActorAt(0).braceUntilTick==27 && w.DoorHealthAt(3,2)==84,"Holding healed damage or extended beyond heard danger.");w.Step();Require(w.ActorAt(0).bracedDoor==0 && w.ActorAt(0).braceUntilTick==0,"Quiet did not release defense at the exact deadline.");Run(w,4);Require(w.ActorAt(0).stress<100,"Quiet defense prevented ordinary stress recovery.");Inspect(w);
        });
        Check("Bracing pauses and survives exact save continuation including breach", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,6);string before=State(w);TickClock c=new TickClock();c.Advance(100,w.Step);Require(State(w)==before,"Paused defense consumed time.");World resumed=Documents.ParseSave(before);for(int i=0;i<100;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Braced-door save continuation diverged.");}Inspect(w);
        });
        Check("Infection and distance prevent physical defense", () => {
            World w=EncounterScenarios.DoorBrace();SaveDocument s=w.Capture();s.actors[0].turnAtTick=65;w=World.Restore(s);Run(w,7);Require(w.ActorAt(0).bracedDoor==0 && w.DoorHealthAt(3,2)==68,"Infected human continued holding a door.");
            w=EncounterScenarios.DoorBrace();Run(w,2);s=w.Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(2,3,Terrain.Floor);m.Set(2,4,Terrain.Wall);s.map=m.Capture();s.actors[0].y=3;w=World.Restore(s);Run(w,7);Require(w.ActorAt(0).bracedDoor==0 && w.DoorHealthAt(3,2)==68,"Human braced a door from a distant or diagonal cell.");Run(w,5);Require(w.DoorHealthAt(3,2)==60,"Arriving beside the door did not enable later physical defense.");
        });
        Check("A different heard door impact releases the old brace without invalidating a save", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,6);SaveDocument s=w.Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(6,2,Terrain.DoorClosed);s.map=m.Capture();w=World.Restore(s);int id;w.TrySpawn(Species.Zombie,7,2,out id);s=w.Capture();s.actors[2].targetId=1;s.actors[2].lastSeenX=6;s.actors[2].lastSeenY=2;s.actors[2].searchUntilTick=81;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).bracedDoor==0 && w.ActorAt(0).heardDoorX==6 && w.DoorHealthAt(6,2)==84,"Switching heard door left stale physical defense.");Require(State(w)==State(Documents.ParseSave(State(w))),"Changed heard source invalidated a live save.");
        });
        Check("Door brace validation rejects unsupported, stale and impossible actions", () => {
            World w=EncounterScenarios.DoorBrace();Run(w,6);SaveDocument s=w.Capture();s.rulesVersion=EncounterRules.AttractionVersion;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].braceUntilTick=s.tick;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].braceUntilTick=s.tick+6;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].bracedDoor=29;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].bracedDoor=0;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].turnAtTick=s.tick+20;Reject(()=>World.Restore(s));s=w.Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(3,2,Terrain.DoorOpen);s.map=m.Capture();Reject(()=>World.Restore(s));
        });
        Check("Prototype-fourteen saves retain independent unbraced-door behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v14-input.json")));Require(w.ZombieWarningInvestigation && !w.DoorBracing,"Older save silently enabled physical defense.");Run(w,80);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v14-expected.json")))),"Prior release behavior changed.");
        });
    }
}
