using System; // Construct independent recipient beliefs and compare timestamps.
using System.IO; // Read independently executed prior-release notebooks.
using Outbreak.Contracts; // Inspect reports without reading live hidden stocks.
using Outbreak.Core; // Run actual conversations, perception and foraging.
using Outbreak.Persistence; // Verify complete deterministic saved continuation.
public static partial class Program
{
    private static ShelfMemoryRecord ReportNote(ActorState actor,int x,int y) { if(actor.shelfMemory!=null)foreach(ShelfMemoryRecord note in actor.shelfMemory)if(note.x==x && note.y==y)return note;return new ShelfMemoryRecord{food=-2}; }
    private static void CheckShelfReportCases()
    {
        Check("A safe neighbor learns a dated food report and follows it without seeing the shelf", () => {
            World w=EncounterScenarios.ShelfReportTest();uint random=w.Capture().randomState;w.Step();ShelfMemoryRecord report=ReportNote(w.ActorAt(1),7,4);Require(w.ShelfReports && report.food==3 && report.observedTick==26 && report.reportedBy==1 && w.ActorAt(1).carriedFood==0,"Conversation invented stock, refreshed source time or failed provenance.");Require(w.ActorAt(0).carriedFood==1 && w.Capture().randomState==random && ReportNote(w.ActorAt(0),7,4).reportedBy==0,"Speaker gave away its last meal, consumed randomness or changed its own observation into hearsay.");w.Step();Require(w.ActorAt(1).foodX==7 && w.ActorAt(1).foodY==4 && ReportNote(w.ActorAt(1),7,4).reportedBy==1,"Recipient could not plan from a heard location behind the closed door.");Run(w,50);Require(w.ActorAt(1).carriedFood==2 && w.ActorAt(1).hunger<40 && ReportNote(w.ActorAt(1),7,4).reportedBy==0 && ReportNote(w.ActorAt(1),7,4).food==0,"Personal sight/search failed to replace the report or collect real food.");Inspect(w);
        });
        Check("Reports preserve stale beliefs rather than reading hidden current stocks", () => {
            World full=EncounterScenarios.ShelfReportTest();SaveDocument s=full.Capture();WorldMap map=WorldMap.Restore(s.map);map.SetShelf(7,4,0,true);s.map=map.Capture();World empty=World.Restore(s);for(int i=0;i<11;i++){full.Step();empty.Step();Require(Documents.Encode(full.ActorAt(1))==Documents.Encode(empty.ActorAt(1)) && full.Capture().randomState==empty.Capture().randomState,"Hidden depletion leaked through conversation or changed the heard route.");}Require(ReportNote(empty.ActorAt(1),7,4).food==3 && ReportNote(empty.ActorAt(1),7,4).observedTick==26,"Report replaced historical knowledge with remote current stock.");
        });
        Check("Hearsay is not relayed to another neighbor", () => {
            World w=EncounterScenarios.ShelfReportTest();w.Step();int id;w.TrySpawn(Species.Human,2,6,out id);SaveDocument s=w.Capture();s.actors[1].hunger=10;s.actors[2].hunger=80;w=World.Restore(s);Run(w,3);Require(ReportNote(w.ActorAt(1),7,4).reportedBy==1 && ReportNote(w.ActorAt(2),7,4).food==-2,"A listener relayed a report without personally seeing the shelf.");
        });
        Check("Unchanged facts do not consume every action and meaningful fresh sightings can refresh them", () => {
            World w=EncounterScenarios.ShelfReportTest();w.Step();uint random=w.Capture().randomState;Run(w,5);Require(w.Capture().randomState!=random && ReportNote(w.ActorAt(1),7,4).observedTick==26,"Repeated identical reports stalled normal action or refreshed their own age.");
            w=EncounterScenarios.ShelfReportTest();w.Step();SaveDocument s=w.Capture();s.tick=330;for(int i=0;i<s.actors[0].shelfMemory.Length;i++)if(s.actors[0].shelfMemory[i].x==7)s.actors[0].shelfMemory[i].observedTick=330;w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).observedTick==330 && ReportNote(w.ActorAt(1),7,4).reportedBy==1,"A meaningfully newer personal sighting failed to refresh an old report with its source time.");
        });
        Check("Older reports cannot overwrite fresher personal stock knowledge", () => {
            SaveDocument s=EncounterScenarios.ShelfReportTest().Capture();s.actors[1].shelfMemory=new[]{new ShelfMemoryRecord{x=7,y=4,food=0,observedTick=30}};World w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==0 && ReportNote(w.ActorAt(1),7,4).observedTick==30 && ReportNote(w.ActorAt(1),7,4).reportedBy==0,"Old supplied report overwrote a fresher direct observation of emptiness.");
        });
        Check("An empty-shelf report cancels a stale target without reading hidden locations", () => {
            SaveDocument s=EncounterScenarios.ShelfReportTest().Capture();s.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=2,y=3,food=0,observedTick=26}};s.actors[1].foodX=2;s.actors[1].foodY=3;s.actors[1].foodUntilTick=180;World w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),2,3).food==0 && ReportNote(w.ActorAt(1),2,3).reportedBy==1 && w.ActorAt(1).foodUntilTick==0 && w.ActorAt(1).foodX==0,"Empty report left its listener chasing a known-depleted target.");
        });
        Check("Unknown shelf contents are not reported as confirmed supplies", () => {
            SaveDocument s=EncounterScenarios.ShelfReportTest().Capture();for(int i=0;i<s.actors[0].shelfMemory.Length;i++)if(s.actors[0].shelfMemory[i].x==7)s.actors[0].shelfMemory[i].food=-1;World w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"An unsearched location was promoted into a food or empty report.");
        });
        Check("Distance, infection and recent danger prevent shelf conversations", () => {
            SaveDocument s=EncounterScenarios.ShelfReportTest().Capture();s.actors[1].y=6;World w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"Report reached a non-adjacent person.");
            for(int actor=0;actor<2;actor++){s=EncounterScenarios.ShelfReportTest().Capture();s.actors[actor].turnAtTick=35;w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"Infection did not preserve escape priority over talking.");s=EncounterScenarios.ShelfReportTest().Capture();s.actors[actor].heardWarningX=3;s.actors[actor].heardWarningY=4;s.actors[actor].heardWarningUntilTick=55;w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"Recent danger did not postpone conversation.");}
            w=EncounterScenarios.ShelfReportTest();int id;w.TrySpawn(Species.Zombie,3,5,out id);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"A visible zombie failed to interrupt conversation.");
        });
        Check("Real meal sharing and the speaker's own hunger have priority over reports", () => {
            SaveDocument s=EncounterScenarios.ShelfReportTest().Capture();s.actors[0].carriedFood=2;World w=World.Restore(s);w.Step();Require(w.ActorAt(1).carriedFood==1 && ReportNote(w.ActorAt(1),7,4).food==-2,"Speaker talked instead of handing over an available spare meal.");s=EncounterScenarios.ShelfReportTest().Capture();s.actors[0].hunger=80;w=World.Restore(s);w.Step();Require(w.ActorAt(0).hunger==30 && ReportNote(w.ActorAt(1),7,4).food==-2,"Hungry speaker postponed its own meal for conversation.");
        });
        Check("Shared notes expire from the original sighting time and survive a source turning", () => {
            World w=EncounterScenarios.ShelfReportTest();w.Step();SaveDocument s=w.Capture();s.tick=3625;w=World.Restore(s);w.Step();Require(ReportNote(w.ActorAt(1),7,4).food==-2,"Conversation extended the original observation's lifetime.");w=EncounterScenarios.ShelfReportTest();w.Step();s=w.Capture();s.actors[0].turnAtTick=32;w=World.Restore(s);w.Step();Require(w.ActorAt(0).species==Species.Zombie && ReportNote(w.ActorAt(1),7,4).reportedBy==1,"Historical report vanished when its source turned.");Require(State(w)==State(Documents.ParseSave(State(w))),"A remembered source turning invalidated the recipient's save.");
        });
        Check("Report source validation rejects self, future identities and incompatible rules", () => {
            World w=EncounterScenarios.ShelfReportTest();w.Step();SaveDocument s=w.Capture();s.actors[1].shelfMemory[0].reportedBy=2;Reject(()=>World.Restore(s));s=w.Capture();s.actors[1].shelfMemory[0].reportedBy=-1;Reject(()=>World.Restore(s));s=w.Capture();s.actors[1].shelfMemory[0].reportedBy=s.nextId;Reject(()=>World.Restore(s));s=w.Capture();s.rulesVersion=EncounterRules.ShelfMemoryVersion;Reject(()=>World.Restore(s));
        });
        Check("Reports pause and continue exactly through learned travel and direct correction", () => {
            World w=EncounterScenarios.ShelfReportTest();w.Step();string before=State(w);new TickClock().Advance(100,w.Step);Require(State(w)==before,"Pause aged or used a report.");World restored=Documents.ParseSave(before);for(int i=0;i<100;i++){w.Step();restored.Step();Require(State(w)==State(restored),"Conversation, provenance or learned route changed after loading.");}Inspect(w);
        });
        Check("Prototype-twenty saves retain independently recorded personal-only shelf behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v20-input.json")));Require(w.ShelfMemoryEnabled && !w.ShelfReports,"Previous notebook gained conversations silently.");Run(w,100);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v20-expected.json")))),"Earlier personal notebook continuation changed.");
        });
    }
}
