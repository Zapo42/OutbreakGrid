using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class FoodRouteMemoryRules { public const int RetryTicks=50; }
    public sealed partial class World
    {
        private static void ClearBlockedFood(ref ActorState actor){actor.blockedFoodX=0;actor.blockedFoodY=0;actor.blockedFoodUntilTick=0;}
        private bool AvoidBlockedFood(ActorState actor,int x,int y){return FoodRouteMemory && actor.blockedFoodUntilTick>Tick && actor.blockedFoodX==x && actor.blockedFoodY==y;}
        private void ValidateFoodRouteMemory()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.blockedFoodUntilTick==0){if(actor.blockedFoodX!=0 || actor.blockedFoodY!=0)throw new ArgumentException("Inactive blocked food route contains coordinates.");}
                else if(!FoodRouteMemory || actor.species!=Species.Human || actor.blockedFoodUntilTick<=Tick || actor.blockedFoodUntilTick-Tick>FoodRouteMemoryRules.RetryTicks || map.ShelfAt(actor.blockedFoodX,actor.blockedFoodY)==null)throw new ArgumentException("Invalid blocked food route.");
            }
        }
    }
}
