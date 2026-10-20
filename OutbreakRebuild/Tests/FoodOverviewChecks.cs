using Outbreak.Contracts;
using Outbreak.Core;
public static partial class Program
{
    private static void CheckFoodOverviewCases()
    {
        Check("Food overview follows actual pickup and eating without changing the run", () => {
            World w=EncounterScenarios.QuickStockPickupTest();string before=State(w);FoodOverview food=w.FoodSupplies();Require(food.Humans==1 && food.Hungry==1 && food.VeryHungry==1 && food.CarriedMeals==0 && food.ShelfMeals==12 && food.StockedShelves==2 && food.UnsearchedShelves==1,"Initial food totals omitted known or unopened map stock.");Require(State(w)==before,"Reading global totals changed the run.");w.Step();food=w.FoodSupplies();Require(food.CarriedMeals==3 && food.ShelfMeals==9 && food.Hungry==1 && food.Searching==0,"Overview did not conserve stock through pickup.");for(int i=0;i<5;i++)w.Step();food=w.FoodSupplies();Require(food.CarriedMeals==2 && food.ShelfMeals==9 && food.Hungry==0 && food.VeryHungry==0,"Eating did not update needs and actual remaining supplies.");food.CarriedMeals=999;Require(w.FoodSupplies().CarriedMeals==2,"Editing a returned value changed the world.");
        });
        Check("Food overview counts need thresholds, infection and active searches separately", () => {
            World w=EncounterScenarios.SearchCoordinationTest();SaveDocument s=w.Capture();s.actors[0].hunger=39;s.actors[1].hunger=79;s.actors[1].turnAtTick=100;w=World.Restore(s);FoodOverview food=w.FoodSupplies();Require(food.Humans==2 && food.Hungry==1 && food.VeryHungry==0 && food.Infected==1 && food.SeekingFood==0,"Overview mixed infected humans, threshold bands or inactive trips.");s.actors[0].hunger=40;s.actors[1].hunger=80;w=World.Restore(s);food=w.FoodSupplies();Require(food.Hungry==2 && food.VeryHungry==1,"Need boundary values were miscounted.");w=EncounterScenarios.CarefulSearchTest();w.Step();food=w.FoodSupplies();Require(food.Searching==1 && food.SeekingFood==1 && food.CarriedMeals==0,"Partial search was absent from live task totals.");
        });
        Check("Empty shelves and converted humans update live totals", () => {
            World w=EncounterScenarios.SearchCoordinationTest();SaveDocument s=w.Capture();WorldMap map=WorldMap.Restore(s.map);map.SetShelf(4,3,0);s.map=map.Capture();s.actors=new[]{s.actors[0]};s.actors[0].turnAtTick=1;w=World.Restore(s);w.Step();FoodOverview food=w.FoodSupplies();Require(food.Humans==0 && food.Infected==0 && food.StockedShelves==1 && food.EmptyShelves==1 && food.ShelfMeals==6,"Conversion or empty furniture left stale food totals.");
        });
    }
}
