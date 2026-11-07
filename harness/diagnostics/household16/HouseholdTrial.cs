using System;using System.IO;using System.Linq;using System.Text.Json;using Outbreak.Core;using Outbreak.Contracts;using Outbreak.Persistence;
class HouseholdTrial
{
    static string State(World w){return Documents.Encode(w.Capture());}
    static void Main(string[] args)
    {
        Directory.CreateDirectory(args[0]);World w=EncounterScenarios.HouseholdLifeTest(),copy=Documents.ParseSave(State(w));int food=w.FoodSupplies().RemainingMeals,meals=0,snacks=0,rests=0;int[] minimumHealth=w.Capture().actors.Select(a=>a.health).ToArray();
        for(int t=0;t<1200;t++){
            w.Step();copy.Step();if(State(w)!=State(copy))throw new Exception("Mid-household replay diverged at "+w.Tick);
            for(int i=0;i<w.Count;i++){var a=w.ActorAt(i);minimumHealth[i]=Math.Min(minimumHealth[i],a.health);if(a.householdActivityTick!=w.Tick)continue;if(a.householdActivity==HouseholdActivity.TableMeal)meals++;if(a.householdActivity==HouseholdActivity.Snack)snacks++;if(a.householdActivity==HouseholdActivity.Resting)rests++;}
            if(w.FoodSupplies().RemainingMeals+meals+snacks!=food)throw new Exception("Whole-food accounting failed at "+w.Tick);
            if(w.Tick%100==0){File.WriteAllText(Path.Combine(args[0],"tick"+w.Tick+".obsave"),State(w));copy=Documents.ParseSave(State(w));}
        }
        var result=new{rules=w.Capture().rulesVersion,mapSchema=w.Capture().map.schemaVersion,ticks=w.Tick,initialFood=food,remainingFood=w.FoodSupplies().RemainingMeals,meals,snacks,restActions=rests,minimumHealth,actors=w.Capture().actors.Select(a=>new{a.id,a.x,a.y,a.hunger,a.carriedFood,a.energySpent,a.happiness,activity=a.householdActivity.ToString()}).ToArray(),exactReplaySteps=1200,nativeUnity="NOT RUN — paused"};File.WriteAllText(Path.Combine(args[0],"RESULTS.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
