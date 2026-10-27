using System; // Validate saved bounds before publishing a restored world.
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class HappinessRules
    {
        // Provisional mood tuning: one pulse per twelve game minutes, staggered by stable identity.
        public const int Minimum=-50,Maximum=50,CadenceTicks=120,CompanyRadius=3,DangerRadius=8,MaximumNearbyPeople=4;
        public static int Preference(int charisma){return charisma<=4?-1:charisma>=6?1:0;}
    }
    public struct HappinessSnapshot
    {
        // Detached observations describe current contributors; requesting this snapshot never advances a pulse.
        public readonly bool Available,Paused,Indoors,Danger;
        public readonly int Value,NearbyPeople,SocialContribution,EnvironmentContribution,Change;
        private readonly int preference;
        internal HappinessSnapshot(bool available,bool paused,int value,int nearby,bool indoors,bool danger,int social,int environment,int change,int preference)
        {
            Available=available;Paused=paused;Value=value;NearbyPeople=nearby;Indoors=indoors;Danger=danger;SocialContribution=social;EnvironmentContribution=environment;Change=change;this.preference=preference;
        }
        public string SocialPreference { get { return preference<0?"Prefers solitude":preference>0?"Prefers company":"Socially neutral"; } }
        public string EnvironmentPreference { get { return preference<0?"Prefers indoors":preference>0?"Prefers outdoors":"Environment neutral"; } }
        public string Description
        {
            get
            {
                if(!Available)return "Happiness unavailable";
                if(Paused)return "Riding: happiness changes paused";
                // Formatting belongs only to an explicitly requested presentation; authoritative pulses allocate no strings.
                return "Visible nearby people: "+NearbyPeople+"; social "+SocialContribution.ToString("+0;-0;0")+"; "+(Indoors?"indoors":"outdoors")+" "+EnvironmentContribution.ToString("+0;-0;0")+"; change per pulse "+Change.ToString("+0;-0;0")+(Danger?" (danger suspends positive gains)":"");
            }
        }
    }
    public sealed partial class World
    {
        private static void ClearHappiness(ref ActorState actor)
        {
            ClearSocialInteraction(ref actor);actor.happiness=0;actor.nextHappinessTick=0; // Death and turning retain no human mood or cadence.
        }
        private HappinessSnapshot ObserveHappiness(int index)
        {
            ActorState observer=actors[index];int preference=HappinessRules.Preference(observer.charisma);
            bool available=HappinessEnabled && observer.species==Species.Human && observer.health>0;
            if(!available)return new HappinessSnapshot(false,false,observer.happiness,0,false,false,0,0,0,preference);
            if(ActorRiding(observer.id))return new HappinessSnapshot(true,true,observer.happiness,0,false,false,0,0,0,preference);
            bool danger=observer.stress>0 || observer.turnAtTick!=0 || observer.grappledById!=0 || HasRecentWarning(observer) || HasRecentDoorNoise(observer),indoors=HouseAt(observer.x,observer.y)!=0;
            int nearby=0;bool lookForDanger=!danger && occupancy.HasSpecies(Species.Zombie);
            for(int i=0;i<count;i++)
            {
                ActorState other=actors[i];if(i==index || other.health<=0)continue;
                int x=other.x-observer.x,y=other.y-observer.y;
                // Distance tests precede rays and rider lookup. Only a small local observation can affect mood.
                int radius=other.species==Species.Human?HappinessRules.CompanyRadius:HappinessRules.DangerRadius;
                if(Math.Abs(x)>radius || Math.Abs(y)>radius || x*x+y*y>radius*radius)continue;
                if(other.species==Species.Human)
                {
                    if(nearby<HappinessRules.MaximumNearbyPeople && !ActorRiding(other.id) && Visible(observer,other))nearby++;
                }
                else if(lookForDanger && Visible(observer,other)){danger=true;lookForDanger=false;}
                if(nearby==HappinessRules.MaximumNearbyPeople && !lookForDanger)break; // Saturated company needs no further scan once danger is already resolved.
            }
            // Charisma affects two transparent preferences, not hidden knowledge, needs or movement efficiency.
            int social=preference*(nearby>0?1:-1),environment=preference*(indoors?-1:1),change=social+environment;
            if(danger)change=Math.Min(0,change);
            return new HappinessSnapshot(true,false,observer.happiness,nearby,indoors,danger,social,environment,change,preference);
        }
        public HappinessSnapshot HappinessKnownBy(int index)
        {
            if(index<0 || index>=count)throw new ArgumentOutOfRangeException("index");
            return ObserveHappiness(index);
        }
        private void AdvanceHappiness()
        {
            if(!HappinessEnabled)return;
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.species!=Species.Human || actor.health<=0)continue;
                if(actor.nextHappinessTick==0?Tick%HappinessRules.CadenceTicks!=actor.id%HappinessRules.CadenceTicks:Tick<actor.nextHappinessTick)continue;
                HappinessSnapshot observation=ObserveHappiness(i);
                actor.happiness=Math.Max(HappinessRules.Minimum,Math.Min(HappinessRules.Maximum,actor.happiness+observation.Change));
                actor.nextHappinessTick=Tick+HappinessRules.CadenceTicks;actors[i]=actor; // Rider pulses retain mood while keeping an exact saved clock.
            }
        }
        private void ValidateHappiness()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool active=actor.happiness!=0 || actor.nextHappinessTick!=0;
                if(actor.happiness<HappinessRules.Minimum || actor.happiness>HappinessRules.Maximum || actor.nextHappinessTick<0 || (actor.nextHappinessTick!=0 && (actor.nextHappinessTick<=Tick || actor.nextHappinessTick>Tick+HappinessRules.CadenceTicks)) || (active && (!HappinessEnabled || actor.species!=Species.Human || actor.health<=0)))throw new ArgumentException("Invalid happiness value, owner or cadence.");
            }
        }
    }
}
