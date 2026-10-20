using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private int FoodPickupAmount(ActorState forager,ShelfDocument shelf)
        {
            int take=Math.Min(HungerRules.CarryLimit-forager.carriedFood,shelf.food);
            if(!FairFoodPickup || take<=1)return take;
            int others=0;
            for(int i=0;i<count;i++)
            {
                ActorState other=actors[i];
                if(other.id==forager.id || other.species!=Species.Human || other.turnAtTick!=0 || other.hunger<HungerRules.SeekAt || other.carriedFood!=0 || DistanceSquared(other,new ActorState{x=shelf.x,y=shelf.y})>4 || !Visible(forager,other))continue;
                others++;
            }
            return Math.Min(take,Math.Max(1,shelf.food-others)); // Leave one possible meal per visible nearby hungry human, while always allowing the forager one.
        }
    }
}
