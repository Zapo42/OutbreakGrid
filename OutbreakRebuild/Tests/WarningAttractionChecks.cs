using System; // Build detached save candidates and reject malformed state.
using System.IO; // Load independent prior-release traces.
using Outbreak.Contracts; // Inspect exact heard locations and target identity.
using Outbreak.Core; // Run production navigation and sound delivery.
using Outbreak.Persistence; // Exercise real save continuation.
public static partial class Program // Extend the executable acceptance suite.
{
    private static void CheckWarningAttractionCases()
    {
        Check("A zombie hears a human warning through a door without learning a hidden target", () => {
            World w=EncounterScenarios.WarningAttraction();Run(w,2);ActorState a=w.ActorAt(3);Require(w.ZombieWarningInvestigation && a.heardWarningX==6 && a.heardWarningY==2 && a.heardWarningUntilTick==27 && a.targetId==0 && a.stress==0 && a.nextWarningTick==0,"Hearing invented target or human reaction state.");Run(w,8);a=w.ActorAt(3);Require(a.x==3 && a.y==4 && a.targetId==0 && w.DoorHealthAt(4,4)==100,"Sound-only approach crossed or damaged its closed doorway.");Inspect(w);
        });
        Check("Warning investigation expires, pauses and resumes exactly", () => {
            World w=EncounterScenarios.WarningAttraction();Run(w,2);string before=State(w);TickClock c=new TickClock();c.Advance(100,w.Step);Require(State(w)==before,"Paused warning investigation advanced.");World resumed=Documents.ParseSave(before);for(int i=0;i<80;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Warning investigator diverged after load.");}Inspect(w);
            w=EncounterScenarios.WarningAttraction();Run(w,2);SaveDocument s=w.Capture();s.actors=new[]{s.actors[3]};w=World.Restore(s);Run(w,25);Require(w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(0).heardWarningX==0 && w.ActorAt(0).targetId==0,"Blocked warning failed exact expiry.");
        });
        Check("Arriving at an empty warning source clears it without tracking the speaker", () => {
            World w=EncounterScenarios.WarningAttraction();Run(w,2);SaveDocument s=w.Capture();s.actors=new[]{s.actors[3]};WorldMap m=WorldMap.Restore(s.map);m.Set(4,4,Terrain.DoorOpen);s.map=m.Capture();w=World.Restore(s);Run(w,22);Require(w.ActorAt(0).x==6 && w.ActorAt(0).y==2 && w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(0).targetId==0,"Arrival retained stale sound or invented a speaker target.");Inspect(w);
        });
        Check("Current sight and remembered humans override warning investigation", () => {
            World w=new World(new WorldMap(10,8),214,encounter:true);int id;w.TrySpawn(Species.Zombie,4,4,out id);w.TrySpawn(Species.Human,2,4,out id);SaveDocument s=w.Capture();s.actors[0].heardWarningX=7;s.actors[0].heardWarningY=4;s.actors[0].heardWarningUntilTick=25;w=World.Restore(s);w.Step();Require(w.ActorAt(0).x==3 && w.ActorAt(0).targetId==2,"A warning displaced current sight.");
            w=EncounterScenarios.WarningAttraction();Run(w,2);s=w.Capture();s.actors[3].targetId=2;s.actors[3].lastSeenX=2;s.actors[3].lastSeenY=5;s.actors[3].searchUntilTick=77;w=World.Restore(s);Run(w,2);Require(w.ActorAt(3).x==2 && w.ActorAt(3).y==5,"A warning displaced personal pursuit memory.");
        });
        Check("The newest sound wins and warning hearing respects acoustic walls and range", () => {
            World w=EncounterScenarios.WarningAttraction();Run(w,2);SaveDocument s=w.Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(1,4,Terrain.DoorOpen);s.map=m.Capture();s.tick=3;s.actors[3].heardDoorX=1;s.actors[3].heardDoorY=4;s.actors[3].heardDoorUntilTick=28;w=World.Restore(s);Run(w,2);Require(w.ActorAt(3).x==1 && w.ActorAt(3).heardDoorUntilTick==0 && w.ActorAt(3).heardWarningUntilTick==27,"Newer door impact lost priority.");s.actors[3].heardDoorUntilTick=27;s.actors[3].heardWarningUntilTick=28;w=World.Restore(s);Run(w,2);Require(w.ActorAt(3).x==3,"Newer warning lost priority.");
            w=EncounterScenarios.WarningAttraction();s=w.Capture();m=WorldMap.Restore(s.map);for(int y=4;y<=6;y++)for(int x=7;x<=9;x++)m.Set(x,y,Terrain.Wall);m.Set(8,5,Terrain.Floor);s.map=m.Capture();w=World.Restore(s);int id;w.TrySpawn(Species.Zombie,8,5,out id);w.TrySpawn(Species.Zombie,1,6,out id);Run(w,2);Require(w.ActorAt(3).heardWarningUntilTick==27 && w.ActorAt(4).heardWarningUntilTick==0 && w.ActorAt(5).heardWarningUntilTick==0,"Warning escaped its wall or range bounds.");
        });
        Check("Zombie warning saves reject unsupported rules, cooldowns and invalid sources", () => {
            World w=EncounterScenarios.WarningAttraction();Run(w,2);SaveDocument s=w.Capture();s.rulesVersion=EncounterRules.WarningVersion;Reject(()=>World.Restore(s));s=w.Capture();s.actors[3].nextWarningTick=3;Reject(()=>World.Restore(s));s=w.Capture();s.actors[3].heardWarningX=4;s.actors[3].heardWarningY=2;Reject(()=>World.Restore(s));s=w.Capture();s.actors[3].heardWarningUntilTick=s.tick;Reject(()=>World.Restore(s));
        });
        Check("Prototype-thirteen saves retain independent human-only warning behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v13-input.json")));Require(w.HumanWarnings && !w.ZombieWarningInvestigation,"Older save silently attracted zombies to warnings.");Run(w,80);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v13-expected.json")))),"Prior warning continuation changed.");
        });
    }
}
