using System;
using System.Text;
using System.Security.Cryptography;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World ParkWalker()
    {
        World w=EncounterScenarios.HumanLeisureTest();Require(w.TryRememberPark(0,11,9),"Could not author familiar park.");return w;
    }
    private static World AtParkPhase(HumanLeisurePhase phase)
    {
        World w=ParkWalker();for(int t=0;t<500 && w.ActorAt(0).leisurePhase!=phase;t++)w.Step();Require(w.ActorAt(0).leisureParkVisit && w.ActorAt(0).leisurePhase==phase,"Park phase never reached.");return w;
    }
    private static void CheckParkVisitCases()
    {
        Check("A known park attracts a physical paid visit and a return to the assigned home",()=>{
            World w=ParkWalker();bool visited=false,returned=false;int moves=0;
            for(int t=0;t<500;t++){var before=w.ActorAt(0);w.Step();var a=w.ActorAt(0);Require(w.HouseAt(a.x,a.y)==0 || w.HouseAt(a.x,a.y)==a.assignedHouseId,"Park visitor trespassed in another building.");if(a.x!=before.x || a.y!=before.y){moves++;Require(a.energySpent>before.energySpent,"Park travel was free.");}if(a.leisurePhase==HumanLeisurePhase.Pausing){visited=true;Require(a.leisureParkVisit && a.x==11 && a.y==9 && w.LeisureStatus(0)=="Visiting park","Visitor did not reach the actual park.");}if(visited && a.leisurePhase==HumanLeisurePhase.None && w.HouseAt(a.x,a.y)==a.assignedHouseId){returned=true;break;}}
            Require(visited && returned && moves>4 && w.ActorAt(0).knownParks.Length==1,"Park outing failed to return or forgot its location.");
        });
        Check("A park stay is bounded and does not spend movement energy while stationary",()=>{
            World w=AtParkPhase(HumanLeisurePhase.Pausing);var a=w.ActorAt(0);long end=a.leisureUntilTick;Require(end==w.Tick+ParkVisitRules.StayTicks,"Park stay duration is wrong.");
            while(w.Tick+1<end){w.Step();var next=w.ActorAt(0);Require(next.x==a.x && next.y==a.y && next.energySpent==a.energySpent,"Stationary park stay moved or drained movement energy.");}Run(w,6);Require(w.ActorAt(0).leisurePhase==HumanLeisurePhase.Returning,"Park visitor did not leave after its stay.");
        });
        Check("Unknown parks are not invented and farther known locations do not trigger remote trips",()=>{
            World w=EncounterScenarios.HumanLeisureTest();Run(w,10);Require(!w.ActorAt(0).leisureParkVisit && w.ActorAt(0).knownParks==null,"NPC invented park knowledge.");w=EncounterScenarios.HumanLeisureTest();Require(w.TryRememberPark(0,25,15),"Could not author distant familiarity.");Run(w,10);Require(!w.ActorAt(0).leisureParkVisit,"Local outing ignored its bounded travel budget.");
        });
        Check("Nearest reachable known park wins while blocked knowledge falls back safely",()=>{
            World w=ParkWalker();Require(w.TryRememberPark(0,8,9),"Could not author nearer park.");w.Step();Require(w.ActorAt(0).leisureParkVisit && w.ActorAt(0).leisureX==8 && w.ActorAt(0).leisureY==9,"Nearer reachable park was not selected.");
            var saved=ParkWalker().Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(11,9,Terrain.Wall);saved.map=map.Capture();w=World.Restore(saved);Run(w,10);Require(!w.ActorAt(0).leisureParkVisit && w.ActorAt(0).knownParks.Length==1,"Stale blocked location caused an invalid visit or lost knowledge.");Require(State(w)==State(Documents.ParseSave(State(w))),"Blocked park fallback cannot save.");
        });
        Check("Park visits retain exact outbound stay return and cooldown save continuation",()=>{
            foreach(var phase in new[]{HumanLeisurePhase.Outbound,HumanLeisurePhase.Pausing,HumanLeisurePhase.Returning}){World w=AtParkPhase(phase),copy=Documents.ParseSave(State(w));string state=State(w);new TickClock().Advance(200,w.Step);Require(State(w)==state,"Paused park visit advanced.");for(int t=0;t<150;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Park save lost phase or timing.");}}
        });
        Check("Needs danger and a new worker shift interrupt optional park visits",()=>{
            foreach(int condition in new[]{0,1,2}){var saved=AtParkPhase(HumanLeisurePhase.Pausing).Capture();if(condition==0)saved.actors[0].hunger=60;if(condition==1)saved.actors[0].energySpent=80;if(condition==2){saved.actors[0].stress=60;saved.actors[0].reaction=HumanReaction.Aware;}World w=World.Restore(saved);Run(w,5);Require(!w.ActorAt(0).leisureParkVisit,"Park visit outranked an urgent condition.");}
            World worker=OffDutyWorker(true);Require(worker.TryRememberPark(0,8,9) && worker.TrySetClerkSchedule(0,8,1),"Could not prepare worker park visit.");bool visited=false;while(worker.Tick<605){worker.Step();visited|=worker.ActorAt(0).leisureParkVisit;}Require(visited && !worker.ActorAt(0).leisureParkVisit && worker.Occupation(0)==HumanOccupation.Clerk,"Shift failed to interrupt park leisure.");
        });
        Check("Forgetting an active park ends the outing without corrupting its save",()=>{
            World w=AtParkPhase(HumanLeisurePhase.Pausing);Require(w.TryForgetPark(0,11,9),"Could not forget park.");Require(!w.ActorAt(0).leisureParkVisit && w.ActorAt(0).knownParks==null && State(w)==State(Documents.ParseSave(State(w))),"Forgotten active park left stale saved progress.");
        });
        Check("Park knowledge is bounded detached canonical and rejects invalid input without mutation",()=>{
            World w=ParkWalker();string before=State(w);Require(!w.TryRememberPark(-1,11,9) && !w.TryRememberPark(0,-1,9) && !w.TryRememberPark(0,4,9) && State(w)==before,"Invalid familiarity changed the world.");for(int x=7;x<14;x++)Require(w.TryRememberPark(0,x,15),"Could not fill park knowledge.");Require(!w.TryRememberPark(0,14,15),"Park knowledge exceeded its cap.");var a=w.ActorAt(0);a.knownParks[0]=0;Require(w.ActorAt(0).knownParks[0]>0,"Knowledge escaped by alias.");var s=w.Capture();s.actors[0].knownParks[0]=0;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].knownParks=new[]{1,1};Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].knownParks=new int[9];Reject(()=>World.Restore(s));s=w.Capture();s.rulesVersion=EncounterRules.OffDutyConsumersVersion;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].leisureParkVisit=true;Reject(()=>World.Restore(s));
        });
        Check("Turning and deprivation death clear human park familiarity and visit state",()=>{
            var s=AtParkPhase(HumanLeisurePhase.Pausing).Capture();s.actors[0].turnAtTick=s.tick+1;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).species==Species.Zombie && w.ActorAt(0).knownParks==null && !w.ActorAt(0).leisureParkVisit,"Turning retained park plans.");w=HappinessBox();Require(w.TryRememberPark(0,3,3),"Could not author outdoor familiarity.");s=w.Capture();s.actors[0].health=1;s.actors[0].hunger=100;s.actors[0].energySpent=100;s.actors[0].carriedFood=0;w=World.Restore(s);Run(w,30);Require(w.ActorAt(0).health==0 && w.ActorAt(0).knownParks==null,"Death retained park knowledge.");
        });
        Check("Rules102 keeps independently recorded pre-park continuation",()=>{
            var saved=EncounterScenarios.HumanLeisureTest().Capture();saved.rulesVersion=EncounterRules.OffDutyConsumersVersion;World w=World.Restore(saved);Require(!w.ParkVisitsEnabled && !w.TryRememberPark(0,11,9),"Older run silently gained park visits.");Run(w,300);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(hash=="2DA13FF9F743969B08F933389A6356325EB414733403F50320F8922218D09F20","Previous leisure behavior changed.");
        });
    }
}
