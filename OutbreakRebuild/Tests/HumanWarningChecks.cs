using System; // Resolve fixture paths and validate saved records.
using System.IO; // Read independently generated prior-release traces.
using Outbreak.Contracts; // Inspect detached actor memory.
using Outbreak.Core; // Exercise actual warning events.
using Outbreak.Persistence; // Verify deterministic continuation.
public static partial class Program // Extend simulation acceptance.
{
    private static void CheckHumanWarningCases()
    {
        Check("A witnessed zombie prompts a local warning without hidden target knowledge", () => {
            World w = EncounterScenarios.HumanWarning(); Run(w, 2); ActorState a = w.ActorAt(0);
            Require(a.heardWarningUntilTick == 27 && a.heardWarningX == 6 && a.heardWarningY == 2 && a.stress == 8, "Closed-door listener did not hear the actual witness.");
            Require(a.targetId == 0 && a.avoidUntilTick == 0 && a.nextWarningTick == 0 && a.shelterCalmSinceTick == 0, "Warning invented a sighting, relay or safe exit.");
            Require(w.ActorAt(1).nextWarningTick == 52 && w.ActorAt(2).heardWarningUntilTick == 0, "Witness cooldown or human-only listening failed."); Inspect(w);
        });
        Check("Warnings respect enclosing walls and eight-step acoustic reach", () => {
            World w = EncounterScenarios.HumanWarning(); SaveDocument s = w.Capture(); WorldMap m = WorldMap.Restore(s.map);
            for (int y=4;y<=6;y++) for(int x=7;x<=9;x++) m.Set(x,y,Terrain.Wall); m.Set(8,5,Terrain.Floor); s.map=m.Capture(); w=World.Restore(s); int id;
            w.TrySpawn(Species.Human,8,5,out id); w.TrySpawn(Species.Human,1,6,out id); Run(w,2);
            Require(w.ActorAt(0).heardWarningUntilTick==27 && w.ActorAt(3).heardWarningUntilTick==0 && w.ActorAt(4).heardWarningUntilTick==0,"Warning crossed a sealed wall or exceeded its acoustic range.");
        });
        Check("Warnings pause, expire exactly, resume deterministically and allow quiet recovery", () => {
            World w=EncounterScenarios.HumanWarning(); Run(w,2); string before=State(w); TickClock c=new TickClock(); c.Advance(100,w.Step); Require(State(w)==before,"Pause consumed warning time.");
            World resumed=Documents.ParseSave(before); for(int i=0;i<48;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Warning save continuation diverged.");if(w.Tick==27) Require(w.ActorAt(0).heardWarningUntilTick==0,"Warning did not expire at its exact deadline.");}
            Require(w.ActorAt(0).stress==0,"Sheltered listener did not recover after quiet."); Inspect(w);
        });
        Check("Hearing alone never relays a warning and conversion clears human communication", () => {
            World w=EncounterScenarios.HumanWarning();Run(w,2);SaveDocument s=w.Capture(); Array.Resize(ref s.actors,2);w=World.Restore(s);Run(w,60);Require(w.ActorAt(0).nextWarningTick==0 && w.ActorAt(1).nextWarningTick==0 && w.ActorAt(0).heardWarningUntilTick==0,"Hearing or expired sight generated another warning.");
            w=EncounterScenarios.HumanWarning();Run(w,2);s=w.Capture();s.actors[0].turnAtTick=s.tick+1;w=World.Restore(s);w.Step();Require(w.ActorAt(0).species==Species.Zombie && w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(0).nextWarningTick==0,"Conversion retained human warning state.");Inspect(w);
        });
        Check("A continuing witness waits fifty ticks before repeating its warning", () => {
            SaveDocument s=EncounterScenarios.HumanWarning().Capture(); WorldMap m=new WorldMap(12,8); for(int y=0;y<8;y++) for(int x=0;x<12;x++) m.Set(x,y,Terrain.Wall); m.Set(2,2,Terrain.Floor);m.Set(3,2,Terrain.DoorClosed);m.Set(4,2,Terrain.Grass);m.Set(5,2,Terrain.Grass);s.map=m.Capture();s.actors[1].x=4;s.actors[2].x=5;World w=World.Restore(s);
            Run(w,2);Require(w.ActorAt(0).heardWarningUntilTick==27 && w.ActorAt(1).nextWarningTick==52,"Initial warning timing changed.");Run(w,49);Require(w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(1).nextWarningTick==52,"Witness repeated before cooldown expired.");w.Step();Require(w.ActorAt(0).heardWarningUntilTick==77 && w.ActorAt(0).stress==8 && w.ActorAt(1).nextWarningTick==102,"Continuing visual danger did not generate one fresh warning at the next eligible opportunity.");Inspect(w);
        });
        Check("Warning validation rejects unsupported rules, stale memory and invalid deadlines", () => {
            World w=EncounterScenarios.HumanWarning();Run(w,2); SaveDocument s=w.Capture();s.rulesVersion=EncounterRules.InvestigationVersion;Reject(()=>World.Restore(s));
            s=w.Capture();s.actors[0].heardWarningUntilTick=s.tick;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].heardWarningUntilTick=s.tick+26;Reject(()=>World.Restore(s));
            s=w.Capture();s.actors[0].heardWarningX=1;s.actors[0].heardWarningY=1;Reject(()=>World.Restore(s));s=w.Capture();s.actors[1].nextWarningTick=s.tick+51;Reject(()=>World.Restore(s));
            s=w.Capture();s.actors[2].heardWarningUntilTick=s.tick+25;s.actors[2].heardWarningX=6;s.actors[2].heardWarningY=2;Reject(()=>World.Restore(s));
        });
        Check("Prototype-twelve saves preserve independent pre-warning continuation", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v12-input.json")));Require(w.ZombieSoundInvestigation && !w.HumanWarnings,"Old save silently enabled warnings.");Run(w,80);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v12-expected.json")))),"Prior-release continuation changed.");
        });
    }
}
