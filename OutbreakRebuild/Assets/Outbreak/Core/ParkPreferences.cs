using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public struct ParkPreferenceSnapshot
    {
        // These are present observations, not a claim that an unseen park is empty or a historical choice reason.
        public readonly bool Available,Known,Visible;
        public readonly int VisiblePeople,SocialContribution,InitialDelayTicks,CooldownTicks;
        internal ParkPreferenceSnapshot(bool available,bool known,bool visible,int people,int social,int initial,int cooldown)
        {Available=available;Known=known;Visible=visible;VisiblePeople=people;SocialContribution=social;InitialDelayTicks=initial;CooldownTicks=cooldown;}
    }
    public sealed partial class World
    {
        // Eight known parks bound this scratch buffer; it is rebuilt only when choosing a new outing, never per path cell.
        private int[] parkSocialScores;
        private int PreferredLeisureDelay(ActorState actor,int baseline)
        {
            if(!ParkPreferencesEnabled)return baseline;
            int preference=HappinessRules.Preference(actor.charisma);return preference<0?baseline*2:preference>0?baseline/2:baseline;
        }
        private bool ParkCurrentlyVisible(ActorState observer,int x,int y)
        {
            int ax=x-observer.x,ay=y-observer.y;
            return ax*ax+ay*ay<=EncounterRules.SightRadius*EncounterRules.SightRadius && Visible(observer,new ActorState{x=x,y=y});
        }
        private int ObservedParkPeople(ActorState observer,int x,int y)
        {
            int people=0;ActorState site=new ActorState{x=x,y=y};
            for(int i=0;i<count;i++)
            {
                ActorState other=actors[i];if(other.id==observer.id || other.species!=Species.Human || other.health<=0)continue;
                int ax=other.x-x,ay=other.y-y;if(Math.Abs(ax)>HappinessRules.CompanyRadius || Math.Abs(ay)>HappinessRules.CompanyRadius || ax*ax+ay*ay>HappinessRules.CompanyRadius*HappinessRules.CompanyRadius)continue;
                int ox=other.x-observer.x,oy=other.y-observer.y;
                // A person's real position must first be visible from the NPC's actual location. The second ray checks company at the destination.
                if(ox*ox+oy*oy>EncounterRules.SightRadius*EncounterRules.SightRadius || !Visible(observer,other) || !Visible(site,other))continue;
                if(++people==HappinessRules.MaximumNearbyPeople)break;
            }
            return people;
        }
        public ParkPreferenceSnapshot ParkPreferenceKnownBy(int index,int x,int y)
        {
            ActorState actor=ActorAt(index);bool available=ParkPreferencesEnabled && actor.species==Species.Human && actor.health>0 && !ActorRiding(actor.id);
            bool known=available && map.Contains(x,y) && KnowsPark(actor,Key(x,y));
            bool visible=known && map.CanWalk(x,y) && LeisureOutdoor(x,y) && ParkCurrentlyVisible(actor,x,y);
            int people=visible?ObservedParkPeople(actor,x,y):0,social=visible?HappinessRules.Preference(actor.charisma)*(people>0?1:-1):0;
            return new ParkPreferenceSnapshot(available,known,visible,people,social,PreferredLeisureDelay(actor,HumanLeisureRules.InitialDelayTicks),PreferredLeisureDelay(actor,HumanLeisureRules.CooldownTicks));
        }
        private void PrepareParkPreferences(ActorState actor)
        {
            if(!ParkPreferencesEnabled || actor.knownParks==null || HappinessRules.Preference(actor.charisma)==0)return;
            if(parkSocialScores==null)parkSocialScores=new int[ParkVisitRules.MaximumKnown];
            for(int i=0;i<actor.knownParks.Length;i++)
            {
                int key=actor.knownParks[i]-1,x=key%Width,y=key/Width;parkSocialScores[i]=0;
                if(!map.CanWalk(x,y) || !LeisureOutdoor(x,y) || !ParkCurrentlyVisible(actor,x,y))continue;
                int people=ObservedParkPeople(actor,x,y);parkSocialScores[i]=HappinessRules.Preference(actor.charisma)*(people>0?1:-1);
            }
        }
        private int ParkSocialContribution(ActorState actor,int cell)
        {
            if(!ParkPreferencesEnabled || actor.knownParks==null || HappinessRules.Preference(actor.charisma)==0)return 0;
            int at=Array.BinarySearch(actor.knownParks,cell+1);return at<0?0:parkSocialScores[at];
        }
    }
}
