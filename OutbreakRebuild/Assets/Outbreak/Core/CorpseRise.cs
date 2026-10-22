using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class RiseRules
    {
        public const int DurationTicks=5;
        public static double Progress(ActorState actor,double tick){return actor.riseUntilTick==0?1:Math.Max(0,Math.Min(1,1-(actor.riseUntilTick-tick)/DurationTicks));}
    }
    public sealed partial class World
    {
        private void ValidateRiseState()
        {
            for(int i=0;i<count;i++){ActorState actor=actors[i];if(actor.riseUntilTick!=0 && (!RiseEnabled || actor.species!=Species.Zombie || actor.health<=0 || actor.riseUntilTick<=Tick || actor.riseUntilTick-Tick>RiseRules.DurationTicks))throw new ArgumentException("Invalid reanimation rise state.");}
        }
    }
}
