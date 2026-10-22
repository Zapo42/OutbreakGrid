namespace Outbreak.Core
{
    public struct FoodOverview
    {
        public const int VeryHungryAt=80;
        public int Humans,Hungry,VeryHungry,Infected,SeekingFood,Searching,InvestigatingRooms,CarriedMeals,ShelfMeals,StockedShelves,EmptyShelves,UnsearchedShelves;
        public int RemainingMeals { get { return CarriedMeals+ShelfMeals; } }
    }
    public sealed partial class World
    {
        public FoodOverview FoodSupplies()
        {
            FoodOverview result=new FoodOverview();
            for(int i=0;i<count;i++)
            {
                var actor=actors[i];if(actor.species!=Outbreak.Contracts.Species.Human || actor.health==0)continue;
                result.Humans++;result.CarriedMeals+=actor.carriedFood;
                if(actor.hunger>=HungerRules.SeekAt)result.Hungry++;
                if(actor.hunger>=FoodOverview.VeryHungryAt)result.VeryHungry++;
                if(actor.turnAtTick!=0)result.Infected++;
                else {if(actor.foodExploreRoomId!=0)result.InvestigatingRooms++;if(actor.hunger>=HungerRules.SeekAt && actor.carriedFood==0 && actor.foodUntilTick>Tick)result.SeekingFood++;if(actor.shelfSearchProgress>0)result.Searching++;}
            }
            foreach(var shelf in map.ShelfRecords){result.ShelfMeals+=shelf.food;if(shelf.food>0)result.StockedShelves++;else result.EmptyShelves++;if(!shelf.searched)result.UnsearchedShelves++;}
            return result; // Read-only player totals; never update personal knowledge, stock, clocks or random streams.
        }
    }
}
