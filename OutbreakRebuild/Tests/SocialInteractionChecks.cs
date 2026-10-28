using System;
using System.Text;
using System.Security.Cryptography;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World SocialPair(int first=8,int second=5)
    {
        var saved=HappinessBox(true,first,1).Capture();saved.rulesVersion=EncounterRules.Version;saved.actors[1].charisma=second;return World.Restore(saved);
    }
    private static string WithoutSocial(World w)
    {
        var s=w.Capture();s.rulesVersion=EncounterRules.ParkVisitsVersion;foreach(var index in new[]{0,1}){s.actors[index].familiarPeople=null;s.actors[index].happiness=0;s.actors[index].nextSocialTick=0;s.actors[index].lastSocialPartnerId=s.actors[index].lastSocialGain=0;}return Documents.Encode(s);
    }
    private static void CheckSocialInteractionCases()
    {
        Check("A nearby willing pair receives one bounded charisma-based greeting reward",()=>{
            World w=SocialPair(),old=World.Restore(w.Capture());var legacy=old.Capture();legacy.rulesVersion=EncounterRules.ParkVisitsVersion;old=World.Restore(legacy);w.Step();old.Step();
            var a=w.ActorAt(0);var b=w.ActorAt(1);Require(a.lastSocialPartnerId==b.id && b.lastSocialPartnerId==a.id,"Pair did not remember its observed partner.");Require(a.lastSocialGain==2 && b.lastSocialGain==1 && a.happiness==old.ActorAt(0).happiness+2 && b.happiness==old.ActorAt(1).happiness+1,"Greeting rewards did not match each person's preference.");
            Require(a.nextSocialTick==w.Tick+SocialInteractionRules.CooldownTicks && b.nextSocialTick==a.nextSocialTick,"Pair cooldown was not atomic.");int x=a.x,y=a.y;Run(w,120);Require(w.ActorAt(0).nextSocialTick==a.nextSocialTick && w.ActorAt(1).nextSocialTick==b.nextSocialTick,"Staggered partner pulses awarded duplicate greetings.");Require(w.ActorAt(0).x==x && w.ActorAt(0).y==y && w.ActorAt(0).energySpent==0,"Stationary greeting moved an NPC or charged movement energy.");
        });
        Check("Solitude preferences decline optional greetings without an imposed mood bonus",()=>{
            foreach(int first in new[]{2,5,8})foreach(int second in new[]{2,5,8}){World w=SocialPair(first,second);w.Step();bool willing=first>=5 && second>=5;Require((w.ActorAt(0).lastSocialPartnerId!=0)==willing && (w.ActorAt(1).lastSocialPartnerId!=0)==willing,"Willingness ignored either NPC's personality.");}
        });
        Check("Needs injuries infection fear and frozen reactions take priority over social rewards",()=>{
            foreach(int index in new[]{0,1})foreach(int condition in new[]{0,1,2,3,4}){var s=SocialPair().Capture();var a=s.actors[index];if(condition==0){a.hunger=70;a.carriedFood=0;}if(condition==1)a.energySpent=60;if(condition==2)a.health=90;if(condition==3)a.turnAtTick=10;if(condition==4){a.stress=70;a.reaction=HumanReaction.Aware;}s.actors[index]=a;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).nextSocialTick==0 && w.ActorAt(1).nextSocialTick==0,"Optional greeting overrode urgent state "+index+" / "+condition);}
            // The partner has not reached its own decision tick, so this freeze is still unresolved.
            var frozen=SocialPair().Capture();frozen.actors[1].freezeUntilTick=10;frozen.actors[1].reaction=HumanReaction.Freeze;World waiting=World.Restore(frozen);waiting.Step();Require(waiting.ActorAt(0).nextSocialTick==0,"A still-frozen partner greeted.");
        });
        Check("Only visible partners within greeting range can interact",()=>{
            foreach(int distance in new[]{0,1,2,3}){var s=SocialPair().Capture();var map=WorldMap.Restore(s.map);for(int x=3;x<=6;x++)map.Set(x,3,Terrain.Floor);map.Set(7,3,Terrain.DoorClosed);s.map=map.Capture();s.actors[1].x=3+distance;World w=World.Restore(s);w.Step();Require((w.ActorAt(0).nextSocialTick>0)==(distance<=SocialInteractionRules.Radius),"Greeting range used hidden or distant company.");}
            var hidden=SocialPair().Capture();var walls=WorldMap.Restore(hidden.map);walls.Set(4,3,Terrain.Wall);walls.Set(3,2,Terrain.DoorClosed);walls.Set(5,3,Terrain.Floor);walls.Set(5,4,Terrain.DoorClosed);hidden.map=walls.Capture();hidden.actors[1].x=5;hidden.actors[1].assignedHouseId=3*11+5+1;World blocked=World.Restore(hidden);blocked.Step();Require(blocked.ActorAt(0).nextSocialTick==0,"A greeting crossed a solid wall.");
        });
        Check("Actual visible danger blocks greetings while a hidden zombie grants no remote danger knowledge",()=>{
            foreach(bool hidden in new[]{false,true}){var s=SocialPair().Capture();var map=WorldMap.Restore(s.map);map.Set(5,3,Terrain.Grass);map.Set(4,3,hidden?Terrain.DoorClosed:Terrain.DoorOpen);s.map=map.Capture();World w=World.Restore(s);int id;Require(w.TrySpawn(Species.Zombie,5,3,out id),"Could not place social danger fixture.");Require(w.HappinessKnownBy(0).Danger==!hidden,"Danger fixture visibility is wrong.");w.Step();Require((w.ActorAt(0).nextSocialTick>0)==hidden,"Greeting used hidden danger or ignored a visible zombie.");}
        });
        Check("Walking and vehicle shopping cannot be interrupted by optional greetings",()=>{
            World w=AtParkPhase(HumanLeisurePhase.Outbound);int id;var a=w.ActorAt(0);Require(w.TrySpawn(Species.Human,a.x,a.y,out id),"Could not place fellow walker.");var s=w.Capture();s.actors[1]=a;s.actors[1].id=id;s.actors[0].charisma=s.actors[1].charisma=8;w=World.Restore(s);Run(w,5);Require(w.ActorAt(0).nextSocialTick==0 && w.ActorAt(1).nextSocialTick==0,"Walking was interrupted by a greeting.");
            w=EncounterScenarios.ConsumerVehicleShoppingTest();for(int t=0;t<400 && !w.ActorInVehicle(0);t++)w.Step();Require(w.ActorInVehicle(0),"Rider fixture never boarded.");a=w.ActorAt(0);Require(w.TrySpawn(Species.Human,a.x+1,a.y,out id),"Could not place nearby pedestrian.");s=w.Capture();s.actors[0].charisma=8;s.actors[1].charisma=8;s.actors[1].assignedHouseId=a.assignedHouseId;s.actors[1].carriedFood=1;w=World.Restore(s);w.Step();Require(w.ActorAt(0).nextSocialTick==0 && w.ActorAt(1).nextSocialTick==0,"Vehicle shopper acquired an on-foot greeting.");
        });
        Check("Park visitors can greet without changing their destinations pause or travel costs",()=>{
            World w=AtParkPhase(HumanLeisurePhase.Pausing);int id;Require(w.TrySpawn(Species.Human,11,10,out id),"Could not place second park visitor.");var s=w.Capture();var b=s.actors[0];b.id=id;b.x=b.leisureX=11;b.y=b.leisureY=10;b.knownParks=new[]{10*w.Width+11+1};b.charisma=8;s.actors[1]=b;s.actors[0].charisma=8;w=World.Restore(s);long until=w.ActorAt(0).leisureUntilTick;int energy=w.ActorAt(0).energySpent;while(w.Tick<until && w.ActorAt(0).nextSocialTick==0)w.Step();Require(w.ActorAt(0).nextSocialTick>0 && w.ActorAt(1).nextSocialTick>0,"Safe park visitors never greeted.");Require(w.ActorAt(0).leisurePhase==HumanLeisurePhase.Pausing && w.ActorAt(0).leisureUntilTick==until && w.ActorAt(0).energySpent==energy,"Greeting extended the visit, changed its route or drained energy.");
        });
        Check("Off-duty workers share consumer greetings while on-duty employment stays authoritative",()=>{
            foreach(bool clerk in new[]{false,true}){World w=OffDutyWorker(clerk);for(int t=0;t<400 && w.HouseAt(w.ActorAt(0).x,w.ActorAt(0).y)!=w.ActorAt(0).assignedHouseId;t++)w.Step();var a=w.ActorAt(0);Require(w.HouseAt(a.x,a.y)==a.assignedHouseId,"Worker never reached home.");int id;Require(w.TrySpawn(Species.Human,a.x,a.y,out id),"Could not place home companion.");var s=w.Capture();s.actors[0].charisma=8;s.actors[0].nextLeisureTick=s.tick+600;s.actors[0].energySpent=0;s.actors[1].charisma=5;s.actors[1].carriedFood=1;s.actors[1].assignedHouseId=a.assignedHouseId;s.actors[1].nextLeisureTick=s.tick+600;w=World.Restore(s);for(int t=0;t<120 && w.ActorAt(0).nextSocialTick==0;t++)w.Step();Require(w.ActorAt(0).nextSocialTick>0 && w.AssignedOccupation(0)==(clerk?HumanOccupation.Clerk:HumanOccupation.Courier),"Off-duty worker lost employment or could not greet.");
                s=w.Capture();foreach(int i in new[]{0,1}){s.actors[i].nextSocialTick=0;s.actors[i].lastSocialPartnerId=s.actors[i].lastSocialGain=0;}w=World.Restore(s);Require(clerk?w.TrySetClerkSchedule(0,0,0):w.TrySetCourierSchedule(0,0,0),"Could not restore always-on schedule.");Run(w,120);Require(w.ActorAt(0).nextSocialTick==0 && w.ActorAt(1).nextSocialTick==0,"Work was interrupted by optional greetings.");}
        });
        Check("Pair choice is stable and rewards cannot exceed the happiness limit",()=>{
            World w=SocialPair();int id;Require(w.TrySpawn(Species.Human,3,3,out id),"Could not place tied partner.");var s=w.Capture();s.actors[2].assignedHouseId=s.actors[0].assignedHouseId;s.actors[2].charisma=8;s.actors[2].carriedFood=1;s.actors[2].nextLeisureTick=600;s.actors[0].happiness=49;s.actors[1].happiness=50;w=World.Restore(s);w.Step();Require(w.ActorAt(0).lastSocialPartnerId==w.ActorAt(1).id && w.ActorAt(2).nextSocialTick==0,"Tie did not use stable identity or one NPC greeted twice.");Require(w.ActorAt(0).happiness<=50 && w.ActorAt(0).lastSocialGain<=2 && w.ActorAt(1).lastSocialGain==0,"Greeting exceeded the happiness bound or misreported actual gain.");
        });
        Check("Greeting snapshots are read-only and exact saves retain cooldown and subsequent exchanges",()=>{
            World w=SocialPair();w.Step();World copy=Documents.ParseSave(State(w));var snap=w.SocialInteractionKnownBy(0);Require(snap.Available && snap.Recent && snap.PartnerId==w.ActorAt(1).id && snap.HappinessGain==2 && snap.CooldownRemaining==240,"Recent greeting snapshot is wrong.");string before=State(w);for(int i=0;i<20;i++)w.SocialInteractionKnownBy(0);Require(State(w)==before,"Reading a greeting changed state.");new TickClock().Advance(999,w.Step);Require(State(w)==before,"Paused time advanced a greeting.");
            for(int t=0;t<400;t++){w.Step();copy.Step();Require(State(w)==State(copy),"Saved greeting continuation diverged.");if(t==119)Require(!w.SocialInteractionKnownBy(0).Recent && w.SocialInteractionKnownBy(0).PartnerId==0,"Expired recent display still exposes a stale conversation.");if(t==240)Require(w.ActorAt(0).nextSocialTick>241,"Expired cooldown prevented a later greeting.");}
        });
        Check("Death and turning clear social state without erasing the surviving person's observed history",()=>{
            foreach(bool turn in new[]{false,true}){World w=SocialPair();w.Step();var s=w.Capture();if(turn)s.actors[0].turnAtTick=s.tick+1;else{s.actors[0].health=1;s.actors[0].hunger=100;s.actors[0].carriedFood=0;s.actors[0].energySpent=100;}w=World.Restore(s);Run(w,turn?1:30);Require(w.ActorAt(0).nextSocialTick==0 && w.ActorAt(0).lastSocialPartnerId==0 && w.ActorAt(0).lastSocialGain==0 && !w.SocialInteractionKnownBy(0).Available,"Nonliving human kept social cooldown or inspector state.");Require(w.ActorAt(1).lastSocialPartnerId==w.ActorAt(0).id,"Surviving observer's actual memory disappeared remotely.");Require(State(w)==State(Documents.ParseSave(State(w))),"Historical partner death broke saving.");}
        });
        Check("Social history validates owner partner cadence and legacy fields before restore",()=>{
            Action<Action<SaveDocument>> bad=change=>{World w=SocialPair();w.Step();var s=w.Capture();change(s);Reject(()=>World.Restore(s));};bad(s=>s.actors[0].nextSocialTick=-1);bad(s=>s.actors[0].nextSocialTick=s.tick);bad(s=>s.actors[0].nextSocialTick=s.tick+241);bad(s=>s.actors[0].lastSocialPartnerId=9999);bad(s=>s.actors[0].lastSocialPartnerId=s.actors[0].id);bad(s=>s.actors[0].lastSocialGain=-1);bad(s=>s.actors[0].lastSocialGain=3);bad(s=>s.actors[0].nextSocialTick=0);bad(s=>s.rulesVersion=EncounterRules.ParkVisitsVersion);bad(s=>s.actors[0].health=0);Reject(()=>SocialPair().SocialInteractionKnownBy(-1));
        });
        Check("Greeting changes only mood and saved social history, preserving movement needs inventory and RNG",()=>{
            World current=SocialPair();var s=current.Capture();s.rulesVersion=EncounterRules.ParkVisitsVersion;World old=World.Restore(s);for(int t=0;t<300;t++){current.Step();old.Step();Require(WithoutSocial(current)==WithoutSocial(old),"Greeting changed non-social simulation state.");}
        });
        Check("Rules103 retains the independently recorded pre-social pair continuation",()=>{
            var s=SocialPair().Capture();s.rulesVersion=EncounterRules.ParkVisitsVersion;World w=World.Restore(s);Require(!w.SocialInteractionsEnabled && !w.SocialInteractionKnownBy(0).Available,"Old save silently enabled social rewards.");Run(w,300);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(w)))).Replace("-","");Require(hash=="DC6445178D8D4396DBC7AC42D10FD19E796FBB05495E035FB92068BFBE5CD73C","Older rules changed from the independent baseline.");
        });
    }
}
