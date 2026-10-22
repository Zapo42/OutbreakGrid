using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class InfectionRules
    {
        public const int MaximumDelay=150,MaximumScale=432,OneDayScale=144,ThreeDayScale=432;
        public static int Delay(ActorState actor,int scale=1){return (50+actor.strength*3+actor.endurance*5+actor.luck*2)*scale;}
    }
    public static class GrappleRules
    {
        public const int HoldTicks=3,StruggleEnergy=5,MaulDamage=15;
        public static int EscapeChance(ActorState human,ActorState zombie){return Math.Max(5,Math.Min(90,15+4*human.strength+2*human.endurance+human.luck-2*zombie.strength-human.stress/10));}
    }
    public sealed partial class World
    {
        private static void ClearGrapple(ref ActorState actor){actor.grappledById=0;actor.grappleUntilTick=0;}
        private int GrappleAttacker(int id){for(int i=0;i<count;i++)if(actors[i].id==id)return i;return -1;}
        public void SetInfectionTimeScale(int scale){if(!MaulingEnabled || Tick!=0 || scale<1 || scale>InfectionRules.MaximumScale)throw new ArgumentException("Set infection pace before starting a current-rules scenario.");InfectionTimeScale=scale;}
        private bool ValidGrapple(ActorState human)
        {
            int index=GrappleAttacker(human.grappledById);if(index<0)return false;ActorState zombie=actors[index];
            return human.species==Species.Human && human.health>0 && human.turnAtTick>Tick && zombie.species==Species.Zombie && zombie.health>0 && zombie.staggerUntilTick<=Tick && Math.Max(Math.Abs(human.x-zombie.x),Math.Abs(human.y-zombie.y))<=1 && Visible(human,zombie);
        }
        private void UpdateGrapples(){for(int i=0;i<count;i++){ActorState actor=actors[i];if(actor.grappledById!=0 && (actor.grappleUntilTick<=Tick || !ValidGrapple(actor))){ClearGrapple(ref actor);actors[i]=actor;}}}
        private void ValidateGrapples()
        {
            foreach(ActorState actor in actors){if(actor.id==0)continue;if(actor.grappledById==0){if(actor.grappleUntilTick!=0)throw new ArgumentException("Inactive grapple has a deadline.");}else if(!MaulingEnabled || actor.grappleUntilTick<=Tick || actor.grappleUntilTick-Tick>GrappleRules.HoldTicks || !ValidGrapple(actor))throw new ArgumentException("Invalid grapple.");if(actor.health==0 && actor.species==Species.Human && (actor.turnAtTick!=0 || actor.energySpent!=0 || actor.nextShoveTick!=0 || actor.nextMeleeTick!=0 || actor.shelterId!=0 || actor.carriedFood!=0 || actor.meleeWeapon!=MeleeWeapon.None || actor.grappledById!=0))throw new ArgumentException("Dead human retained active state.");}
        }
        private bool TryStruggle(int index)
        {
            ActorState human=actors[index];if(human.grappledById==0)return true;
            int attacker=GrappleAttacker(human.grappledById);if(attacker<0){ClearGrapple(ref human);actors[index]=human;return true;}
            if(EnergyRules.Remaining(human)<GrappleRules.StruggleEnergy)return false;
            human.energySpent+=GrappleRules.StruggleEnergy;bool escaped=reactionRandom.Below(100)<GrappleRules.EscapeChance(human,actors[attacker]);if(escaped)ClearGrapple(ref human);actors[index]=human;return escaped;
        }
        private void MaulHuman(ActorState attacker,int targetIndex)
        {
            ActorState target=actors[targetIndex];if(target.species!=Species.Human || target.health==0)return;
            bool infected=target.turnAtTick!=0;int chance=infected?0:BiteRules.DodgeChance(target,attacker,Tick);bool evaded=!infected && chance>0 && reactionRandom.Below(100)<chance;
            if(!evaded)
            {
                ClearDoorBrace(ref target);target.health=Math.Max(0,target.health-(infected?GrappleRules.MaulDamage:EncounterRules.BiteDamage));
                if(target.health==0){if(target.meleeWeapon==MeleeWeapon.Bat)map.DropBat(target.x,target.y,target.batWear);target.meleeWeapon=MeleeWeapon.None;target.batWear=0;target.turnAtTick=0;ClearHumanReaction(ref target);int killer=GrappleAttacker(attacker.id);if(killer>=0){ActorState zombie=actors[killer];ClearPursuit(ref zombie);actors[killer]=zombie;}ClearPursuit(ref target);ClearSearchHearing(ref target);}
                else {if(!infected)target.turnAtTick=Tick+InfectionRules.Delay(target,InfectionTimeScale);target.grappledById=attacker.id;target.grappleUntilTick=Tick+GrappleRules.HoldTicks;target.reaction=HumanReaction.Flight;target.freezeUntilTick=0;target.freezeResolved=true;target.avoidX=attacker.x;target.avoidY=attacker.y;target.avoidUntilTick=Tick+AvoidMemoryTicks;}
                actors[targetIndex]=target;
            }
            if(biteActions==null)biteActions=new BiteAction[Capacity];biteActions[biteActionCount++]=new BiteAction{actorId=attacker.id,targetId=target.id,tick=Tick,dodgeChance=chance,evaded=evaded,mauling=infected,grapple=!evaded && target.health>0,lethal=target.health==0};
        }
    }
}
