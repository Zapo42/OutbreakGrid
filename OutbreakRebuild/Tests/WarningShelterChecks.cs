using System; // Build detached listener-only worlds and invalid timing controls.
using System.IO; // Read independently executed prior-release traces.
using Outbreak.Contracts; // Inspect shelter reservations without inventing sightings.
using Outbreak.Core; // Exercise real hearing, routes, doors and recovery.
using Outbreak.Persistence; // Verify exact authoritative continuation.
public static partial class Program
{
    private static World LoneWarnedListener() // Retain only the actual listener after a real warning event.
    {
        World w=EncounterScenarios.WarningShelter();w.Step();SaveDocument s=w.Capture();s.actors=new[]{s.actors[2]};return World.Restore(s);
    }
    private static void CheckWarningShelterCases()
    {
        Check("An outdoor human hears a real warning and enters a shelter without seeing the zombie", () => {
            World w=EncounterScenarios.WarningShelter();w.Step();ActorState a=w.ActorAt(2);Require(w.WarningSheltering && a.heardWarningUntilTick==26 && a.heardWarningX==6 && a.heardWarningY==2 && a.stress==8 && a.shelterId==0,"Witness warning failed before the listener's opportunity.");Run(w,2);a=w.ActorAt(2);Require(a.shelterId==27 && a.shelterState==ShelterState.Seeking && w.TerrainAt(2,3)==Terrain.DoorOpen,"Warning did not prompt a real shelter route.");Run(w,10);a=w.ActorAt(2);Require(a.shelterState==ShelterState.Inside && a.x==2 && a.y==2 && w.TerrainAt(2,3)==Terrain.DoorClosed,"Listener failed to enter and close behind itself.");Require(a.targetId==0 && a.avoidUntilTick==0 && a.turnAtTick==0,"Hearing invented visual threat knowledge or contact.");Inspect(w);
        });
        Check("A remembered warning suffices after the speaker leaves and quiet later permits departure", () => {
            World w=LoneWarnedListener();Run(w,12);Require(w.ActorAt(0).shelterState==ShelterState.Inside,"Shelter decision required the hidden speaker to remain alive.");Run(w,68);Require(w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(0).stress==0 && w.ActorAt(0).shelterId==0 && w.ActorAt(0).turnAtTick==0,"Quiet did not restore ordinary shelter recovery and departure.");Inspect(w);
        });
        Check("No warning and an already expired warning do not trigger shelter seeking", () => {
            World w=LoneWarnedListener();SaveDocument s=w.Capture();s.actors[0].heardWarningUntilTick=0;s.actors[0].heardWarningX=0;s.actors[0].heardWarningY=0;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).shelterId==0,"Stress alone was treated as a fresh warning.");
            w=LoneWarnedListener();s=w.Capture();s.actors[0].heardWarningUntilTick=2;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).heardWarningUntilTick==0 && w.ActorAt(0).shelterId==0,"Expired hearing started a new shelter reservation.");
        });
        Check("A heard door impact also prompts cover without an unseen zombie target", () => {
            World w=LoneWarnedListener();SaveDocument s=w.Capture();s.actors[0].heardWarningUntilTick=0;s.actors[0].heardWarningX=0;s.actors[0].heardWarningY=0;s.actors[0].heardDoorX=2;s.actors[0].heardDoorY=3;s.actors[0].heardDoorUntilTick=26;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).shelterId==27 && w.ActorAt(0).targetId==0 && w.ActorAt(0).avoidUntilTick==0,"Heard impact did not prompt shelter independently of visual knowledge.");
        });
        Check("Full and unreachable shelters do not overbook or teleport warned listeners", () => {
            World w=LoneWarnedListener();int id;w.TrySpawn(Species.Human,2,2,out id);SaveDocument s=w.Capture();s.actors[1].shelterId=27;s.actors[1].shelterX=2;s.actors[1].shelterY=2;s.actors[1].shelterState=ShelterState.Inside;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).shelterId==0 && w.ActorAt(0).x==2 && w.ActorAt(0).y==4,"Warning bypassed the room's admission limit.");
            w=LoneWarnedListener();s=w.Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(2,4,Terrain.Grass);m.Set(1,4,Terrain.Wall);m.Set(3,4,Terrain.Wall);m.Set(2,5,Terrain.Wall);s.map=m.Capture();s.actors[0].x=2;s.actors[0].y=6;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).shelterId==0 && w.ActorAt(0).y==6,"Warning bypassed collision to reserve an unreachable room.");Inspect(w);
        });
        Check("Warning shelter travel pauses and saves exactly through entry and recovery", () => {
            World w=LoneWarnedListener();Run(w,2);string before=State(w);TickClock c=new TickClock();c.Advance(100,w.Step);Require(State(w)==before,"Paused shelter response consumed time.");World resumed=Documents.ParseSave(before);for(int i=0;i<100;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Warning shelter route diverged after load.");}Inspect(w);
        });
        Check("Prototype-fifteen saves retain independently recorded bracing behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v15-input.json")));Require(w.DoorBracing && !w.WarningSheltering,"Older save silently enabled shelter seeking after hearing.");Run(w,80);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v15-expected.json")))),"Previous bracing continuation changed.");
        });
    }
}
