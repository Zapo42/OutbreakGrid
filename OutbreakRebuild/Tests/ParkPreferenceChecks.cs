using System;
using System.Text;
using System.Security.Cryptography;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World PersonalParkWalker(int charisma=8,bool company=true,bool hidden=false)
    {
        World w=EncounterScenarios.HumanLeisureTest();Require(w.TryRememberPark(0,8,9) && w.TryRememberPark(0,13,9),"Could not author familiar parks.");
        var s=w.Capture();var map=WorldMap.Restore(s.map);map.Set(6,9,Terrain.DoorOpen);if(hidden)map.Set(10,9,Terrain.Wall);s.map=map.Capture();s.actors[0].charisma=charisma;w=World.Restore(s);
        if(company){int id;Require(w.TrySpawn(Species.Human,13,9,out id),"Could not place observed park company.");}return w;
    }
    private static void CheckParkPreferenceCases()
    {
        Check("Company seekers choose visible company while solitude and neutral NPCs choose the nearer quiet park",()=>{
            foreach(int charisma in new[]{2,5,8}){World w=PersonalParkWalker(charisma);var near=w.ParkPreferenceKnownBy(0,8,9);var far=w.ParkPreferenceKnownBy(0,13,9);Require(near.Available && near.Known && near.Visible && near.VisiblePeople==0 && far.VisiblePeople==1,"Known park observations are wrong.");w.Step();var a=w.ActorAt(0);Require(a.leisureParkVisit && a.leisureX==(charisma>=6?13:8) && a.leisureY==9,"Personality did not change the park choice.");}
        });
        Check("Walls keep crowd state unknown and hidden people cannot change the selected destination",()=>{
            World empty=PersonalParkWalker(8,false,true),hidden=PersonalParkWalker(8,true,true);var p=hidden.ParkPreferenceKnownBy(0,13,9);Require(p.Known && !p.Visible && p.VisiblePeople==0 && p.SocialContribution==0,"An unseen park was treated as confirmed quiet or revealed people.");empty.Step();hidden.Step();Require(empty.ActorAt(0).leisureX==hidden.ActorAt(0).leisureX && empty.ActorAt(0).leisureY==hidden.ActorAt(0).leisureY,"Hidden crowd positions changed a choice.");
        });
        Check("Only observed living on-foot people influence park company",()=>{
            var s=PersonalParkWalker().Capture();s.actors[1].health=0;World dead=World.Restore(s);Require(dead.ParkPreferenceKnownBy(0,13,9).VisiblePeople==0,"Dead NPC counted as park company.");
            World w=EncounterScenarios.ConsumerVehicleShoppingTest();for(int t=0;t<400 && !w.ActorInVehicle(0);t++)w.Step();Require(w.ActorInVehicle(0),"Vehicle fixture never boarded.");var car=w.VehicleAtIndex(0);bool placed=false;int observer=w.Count,id;
            for(int y=car.y-2;y<=car.y+2 && !placed;y++)for(int x=car.x-2;x<=car.x+2 && !placed;x++){if(x<0 || y<0 || x>=w.Width || y>=w.Height || w.HouseAt(x,y)!=0)continue;Terrain t=w.TerrainAt(x,y);if(t!=Terrain.Grass && t!=Terrain.Sidewalk && t!=Terrain.Crosswalk)continue;if(w.TrySpawn(Species.Human,x,y,out id)){Require(w.TryRememberPark(observer,x,y),"Could not author pedestrian park.");placed=true;Require(w.ParkPreferenceKnownBy(observer,x,y).VisiblePeople==0,"A seated driver counted as park company.");}}
            Require(placed,"Could not place nearby pedestrian observer.");
        });
        Check("Unknown distant and blocked parks cannot bypass familiarity visibility or route limits",()=>{
            World w=PersonalParkWalker();Require(!w.ParkPreferenceKnownBy(0,12,9).Known && w.ParkPreferenceKnownBy(0,12,9).VisiblePeople==0,"Unknown site exposed crowd observations.");Require(w.TryRememberPark(0,25,15),"Could not author far familiarity.");Require(!w.ParkPreferenceKnownBy(0,25,15).Visible,"Distant park bypassed sight radius.");var s=w.Capture();var map=WorldMap.Restore(s.map);map.Set(13,9,Terrain.Wall);s.map=map.Capture();s.actors=new[]{s.actors[0]};w=World.Restore(s);w.Step();Require(w.ActorAt(0).leisureParkVisit && w.ActorAt(0).leisureX==8 && w.ActorAt(0).leisureY==9,"Preference chose a blocked or far park.");
        });
        Check("Indoor outdoor and neutral preferences schedule bounded distinct waits and cooldowns",()=>{
            foreach(int charisma in new[]{2,5,8}){var s=EncounterScenarios.HumanLeisureTest().Capture();s.actors[0].nextLeisureTick=0;s.actors[0].charisma=charisma;World w=World.Restore(s);w.Step();int multiplier=charisma<5?2:1,divisor=charisma>5?2:1;long expected=w.Tick+HumanLeisureRules.InitialDelayTicks*multiplier/divisor+w.ActorAt(0).id%HumanLeisureRules.StaggerTicks;Require(w.ActorAt(0).nextLeisureTick==expected,"Personality initial wait is wrong.");var view=w.ParkPreferenceKnownBy(0,8,9);Require(view.InitialDelayTicks==HumanLeisureRules.InitialDelayTicks*multiplier/divisor && view.CooldownTicks==HumanLeisureRules.CooldownTicks*multiplier/divisor,"Inspector wait values disagree with behavior.");Require(State(w)==State(Documents.ParseSave(State(w))),"Personality wait cannot save.");
                s=w.Capture();s.actors[0].nextLeisureTick=s.tick+1;w=World.Restore(s);bool left=false,returned=false;for(int t=0;t<350;t++){w.Step();var a=w.ActorAt(0);left|=a.leisurePhase!=HumanLeisurePhase.None;if(left && a.leisurePhase==HumanLeisurePhase.None){returned=true;Require(a.nextLeisureTick==w.Tick+HumanLeisureRules.CooldownTicks*multiplier/divisor+a.id%HumanLeisureRules.StaggerTicks,"Personality cooldown is wrong.");break;}}Require(left && returned,"Personality outing failed to finish.");}
        });
        Check("A solitude-preferring resident defers a visibly busy park without creating stale route state",()=>{
            World original=PersonalParkWalker(2);Require(original.TryForgetPark(0,8,9),"Could not remove quiet alternative.");original.Step();var a=original.ActorAt(0);Require(a.leisurePhase==HumanLeisurePhase.None && a.nextLeisureTick==original.Tick+1200+a.id%120 && original.HouseAt(a.x,a.y)==a.assignedHouseId,"Busy known park forced a solitude outing.");Require(a.leisureDoor==0 && a.leisureUntilTick==0 && State(original)==State(Documents.ParseSave(State(original))),"Declined outing left stale geometry.");
        });
        Check("Changing company does not retarget an existing outing and actual steps still cost energy",()=>{
            World w=PersonalParkWalker();w.Step();var s=w.Capture();int targetX=s.actors[0].leisureX,targetY=s.actors[0].leisureY;s.actors[1].x=8;w=World.Restore(s);bool moved=false;for(int t=0;t<15;t++){var before=w.ActorAt(0);w.Step();var a=w.ActorAt(0);Require(a.leisureX==targetX && a.leisureY==targetY,"Moving people made the destination oscillate.");if(a.x!=before.x || a.y!=before.y){moved=true;Require(a.energySpent>before.energySpent,"Preference-based movement was free.");}}Require(moved,"Preference outing never physically moved.");
        });
        Check("Needs and a new worker shift retain priority over preferred parks",()=>{
            foreach(int condition in new[]{0,1,2}){var s=PersonalParkWalker().Capture();if(condition==0){s.actors[0].hunger=60;s.actors[0].carriedFood=0;}if(condition==1)s.actors[0].energySpent=80;if(condition==2){s.actors[0].stress=80;s.actors[0].reaction=HumanReaction.Aware;}World w=World.Restore(s);w.Step();Require(w.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Park preference overrode a need or danger.");}
            World worker=OffDutyWorker(true);var saved=worker.Capture();saved.actors[0].charisma=8;worker=World.Restore(saved);Require(worker.TryRememberPark(0,8,9) && worker.TrySetClerkSchedule(0,8,1),"Could not prepare preferred worker park.");bool visited=false;while(worker.Tick<605){worker.Step();visited|=worker.ActorAt(0).leisureParkVisit;}Require(visited && !worker.ActorAt(0).leisureParkVisit && worker.Occupation(0)==HumanOccupation.Clerk,"Preferred park displaced work shift.");
        });
        Check("Preference snapshots do not mutate choice scratch or authoritative state",()=>{
            World w=PersonalParkWalker();string before=State(w);for(int i=0;i<20;i++){w.ParkPreferenceKnownBy(0,8,9);w.ParkPreferenceKnownBy(0,13,9);w.ParkPreferenceKnownBy(1,8,9);}Require(State(w)==before,"Preference queries changed the simulation.");w.Step();Require(w.ActorAt(0).leisureX==13,"Another NPC's inspector changed prepared choice data.");Reject(()=>w.ParkPreferenceKnownBy(-1,8,9));Require(!w.ParkPreferenceKnownBy(0,-1,int.MaxValue).Known,"Invalid site was exposed.");
        });
        Check("Preference routes and long indoor cooldowns save exactly and reject incompatible clocks",()=>{
            foreach(int charisma in new[]{2,8}){World w=PersonalParkWalker(charisma);w.Step();World copy=Documents.ParseSave(State(w));for(int t=0;t<400;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Saved preference route or cooldown diverged.");if(t%61==0)copy=Documents.ParseSave(State(w));}}
            var s=PersonalParkWalker(2).Capture();s.actors[0].nextLeisureTick=1321;Reject(()=>World.Restore(s));s.actors[0].nextLeisureTick=1200;World longWait=World.Restore(s);Require(State(longWait)==State(Documents.ParseSave(State(longWait))),"Valid indoor wait rejected.");s.rulesVersion=EncounterRules.SocialInteractionsVersion;Reject(()=>World.Restore(s));
        });
        Check("Rules104 retains independently recorded nearest-park behavior and its original outing cadence",()=>{
            World w=EncounterScenarios.HumanLeisureTest();Require(w.TryRememberPark(0,8,9) && w.TryRememberPark(0,13,9),"Could not author baseline parks.");var s=w.Capture();s.rulesVersion=EncounterRules.SocialInteractionsVersion;s.actors[0].nextLeisureTick=0;w=World.Restore(s);Require(!w.ParkPreferencesEnabled && !w.ParkPreferenceKnownBy(0,8,9).Available,"Older rules silently gained preferences.");Run(w,800);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(hash=="5FFBC2348BBD7591FBA3CFBF614AFB4CB4BC25A924C89761479912918E699ED6","Rules104 no longer matches independent baseline.");
        });
    }
}
