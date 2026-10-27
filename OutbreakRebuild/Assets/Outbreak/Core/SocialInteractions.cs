using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class SocialInteractionRules
    {
        // Greetings are instantaneous; these clocks bound opportunity scans and prevent repeated pair rewards.
        public const int Radius=2,CooldownTicks=240,RecentTicks=120;
    }
    public struct SocialInteractionSnapshot
    {
        public readonly bool Available,Recent;
        public readonly int PartnerId,HappinessGain;
        public readonly long CooldownRemaining;
        internal SocialInteractionSnapshot(bool available,bool recent,int partner,int gain,long cooldown)
        {Available=available;Recent=recent;PartnerId=partner;HappinessGain=gain;CooldownRemaining=cooldown;}
    }
    public sealed partial class World
    {
        private static void ClearSocialInteraction(ref ActorState actor)
        {actor.nextSocialTick=0;actor.lastSocialPartnerId=actor.lastSocialGain=0;}
        public SocialInteractionSnapshot SocialInteractionKnownBy(int index)
        {
            ActorState actor=ActorAt(index);bool available=SocialInteractionsEnabled && actor.species==Species.Human && actor.health>0;
            bool recent=available && actor.nextSocialTick>Tick && Tick-(actor.nextSocialTick-SocialInteractionRules.CooldownTicks)<SocialInteractionRules.RecentTicks;
            return new SocialInteractionSnapshot(available,recent,recent?actor.lastSocialPartnerId:0,recent?actor.lastSocialGain:0,available?Math.Max(0,actor.nextSocialTick-Tick):0);
        }
        private bool SociallyAvailable(ActorState actor)
        {
            // Work, shopping, rest needs, danger and ongoing travel cannot be delayed by a social opportunity.
            if(actor.charisma<5 || actor.nextSocialTick>Tick || !LeisureSafe(actor,-1,true) || ActorRiding(actor.id))return false;
            return actor.leisurePhase==HumanLeisurePhase.Pausing || actor.leisurePhase==HumanLeisurePhase.None && HouseAt(actor.x,actor.y)==actor.assignedHouseId;
        }
        private void ApplyGreeting(ref ActorState actor,int partnerId)
        {
            int gain=actor.charisma>=6?2:1;gain=Math.Min(gain,HappinessRules.Maximum-actor.happiness);
            actor.happiness+=gain;actor.lastSocialGain=gain;actor.lastSocialPartnerId=partnerId;actor.nextSocialTick=Tick+SocialInteractionRules.CooldownTicks;
        }
        private void AdvanceSocialInteractions()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];
                if(actor.nextSocialTick!=0 && Tick>=actor.nextSocialTick){ClearSocialInteraction(ref actor);actors[i]=actor;}
                // Stable cadence and no RNG draws preserve reproducibility. Neither greeting nor its query moves an actor.
                if(Tick%HappinessRules.CadenceTicks!=actor.id%HappinessRules.CadenceTicks || !SociallyAvailable(actor) || ObserveHappiness(i).Danger)continue;
                int partner=-1,best=int.MaxValue;
                for(int j=0;j<count;j++)
                {
                    if(i==j)continue;ActorState other=actors[j];int x=other.x-actor.x,y=other.y-actor.y;
                    if(Math.Abs(x)>SocialInteractionRules.Radius || Math.Abs(y)>SocialInteractionRules.Radius)continue;
                    int distance=x*x+y*y;if(distance>SocialInteractionRules.Radius*SocialInteractionRules.Radius || distance>best || !SociallyAvailable(other) || !Visible(actor,other))continue;
                    if(distance==best && other.id>=actors[partner].id)continue;
                    partner=j;best=distance;
                }
                // The closest willing partner must also perceive safety; do not infer their hidden surroundings.
                if(partner<0 || ObserveHappiness(partner).Danger)continue;
                ActorState companion=actors[partner];ApplyGreeting(ref actor,companion.id);ApplyGreeting(ref companion,actor.id);actors[i]=actor;actors[partner]=companion;
            }
        }
        private void ValidateSocialInteractions()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool any=actor.nextSocialTick!=0 || actor.lastSocialPartnerId!=0 || actor.lastSocialGain!=0;if(!any)continue;
                if(!SocialInteractionsEnabled || actor.species!=Species.Human || actor.health<=0 || actor.nextSocialTick<=Tick || actor.nextSocialTick>Tick+SocialInteractionRules.CooldownTicks || actor.nextSocialTick<SocialInteractionRules.CooldownTicks+1 || actor.lastSocialPartnerId<=0 || actor.lastSocialPartnerId==actor.id || actor.lastSocialGain<0 || actor.lastSocialGain>2)throw new ArgumentException("Invalid social interaction owner, cooldown or gain.");
                // A remembered partner may later die or leave the area; history is not a live proximity requirement.
                bool found=false;for(int j=0;j<count;j++)if(actors[j].id==actor.lastSocialPartnerId){found=true;break;}
                if(!found)throw new ArgumentException("Unknown social interaction partner.");
            }
        }
    }
}
