using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private void AdvanceDeprivation(ref ActorState actor)
        {
            if(!SurvivalDamageEnabled || actor.species!=Species.Human || actor.health<=0 || actor.hunger<100 || EnergyRules.Remaining(actor)>0 || Tick%HungerRules.GainTicks!=0)return;
            actor.health--;
            if(actor.health>0)return;
            long infection=ReanimationEnabled?actor.turnAtTick:0;
            if(actor.meleeWeapon==MeleeWeapon.Bat)map.DropBat(actor.x,actor.y,actor.batWear);
            actor.meleeWeapon=MeleeWeapon.None;actor.batWear=0;ClearHumanReaction(ref actor);ClearPursuit(ref actor);ClearSearchHearing(ref actor);actor.turnAtTick=infection;actor.diedFromDeprivation=true;
        }
        private void ValidateDeprivationState()
        {
            for(int i=0;i<count;i++)if(actors[i].diedFromDeprivation && (!SurvivalDamageEnabled || actors[i].species!=Species.Human || actors[i].health!=0))throw new ArgumentException("Invalid deprivation death state.");
        }
    }
}
