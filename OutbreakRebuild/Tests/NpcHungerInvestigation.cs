using System;
using System.IO;
using System.Reflection;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
namespace Outbreak.Diagnostics
{
    // This reproduces known problems. A zero exit means the investigative contrasts held, not that NPC health passed.
    internal static class NpcHungerInvestigation
    {
        private static readonly StringBuilder Report=new StringBuilder();
        private static string State(World w){return Documents.Encode(w.Capture());}
        private static World Copy(World w){return Documents.ParseSave(State(w));}
        private static void Note(string message){Report.AppendLine(message);Console.WriteLine(message);}
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private static void Run(World w,int ticks){for(int i=0;i<ticks;i++)w.Step();}
        private static int HomeMeals(World w,int house)
        {int food=0;foreach(var box in w.StorageBoxSites())if(w.HouseAt(box.x,box.y)==house)food+=StorageRules.Count(box,InventoryItem.Food);return food;}
        private static void ActorLine(string label,World w,int index)
        {
            var a=w.ActorAt(index);Note(label+": tick="+w.Tick+" id="+a.id+" health="+a.health+" hunger="+a.hunger+" energy="+(100-a.energySpent)+" hand="+a.carriedFood+" house="+w.HouseAt(a.x,a.y)+" assignedHome="+a.assignedHouseId+" commute="+a.workerCommuteId+" restock="+a.restockPhase);
        }
        public static int Main(string[] args)
        {
            string output=Path.GetFullPath(args.Length>0?args[0]:"npc-hunger-investigation");Directory.CreateDirectory(output);
            try
            {
                Note("NPC-F139 investigation only. Counterfactuals below modify diagnostic copies, never production behavior. Native Unity NOT RUN.");
                World city=CitySetup.Create(214,4,2,2,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters),courier=null,closing=null;
                var historical=city.Capture();historical.rulesVersion=EncounterRules.SocialFamiliarityVersion;city=World.Restore(historical);Note("Historical rules106 reproduction; current107 repair deliberately disabled.");
                int initialFood=city.FoodSupplies().RemainingMeals;File.WriteAllText(Path.Combine(output,"initial.obsave2"),State(city));
                for(int t=1;t<=14400;t++){city.Step();if(t==3600)courier=Copy(city);if(t==7200)closing=Copy(city);}
                int deaths=0;for(int i=0;i<city.Count;i++)if(city.ActorAt(i).health==0)deaths++;
                Note("Generated day: deaths="+deaths+" foodStart="+initialFood+" foodEnd="+city.FoodSupplies().RemainingMeals);
                foreach(var box in city.StorageBoxSites())if(StorageRules.Count(box,InventoryItem.Food)>0)Note("Remaining box="+box.x+","+box.y+" house="+city.HouseAt(box.x,box.y)+" meals="+StorageRules.Count(box,InventoryItem.Food));
                foreach(var store in city.StoreSupplies())Note("Store="+store.StoreId+" shelfMeals="+store.ShelfMeals+" crateMeals="+store.CrateMeals);
                File.WriteAllText(Path.Combine(output,"final.obsave2"),State(city));File.WriteAllText(Path.Combine(output,"courier-tick3600.obsave2"),State(courier));File.WriteAllText(Path.Combine(output,"closing-tick7200.obsave2"),State(closing));
                if(args.Length>1){Require(State(city)==File.ReadAllText(args[1]),"Regenerated day differs from supplied baseline final.");Note("PASS exact supplied baseline continuation.");}

                // Normalize health and energy in both copies, then vary hunger alone. This is not a gameplay remedy.
                var saved=closing.Capture();saved.actors[1].health=100;saved.actors[1].energySpent=0;saved.actors[1].hunger=60;
                World hungry=World.Restore(saved);saved.actors[1].hunger=0;World fed=World.Restore(saved);
                Require(HomeMeals(hungry,hungry.ActorAt(1).assignedHouseId)==2,"Missing own-home stock control.");
                Run(hungry,5);Run(fed,5);ActorLine("hungry commute",hungry,1);ActorLine("fed commute control",fed,1);
                Require(hungry.ActorAt(1).workerCommuteId==0 && fed.ActorAt(1).workerCommuteId==fed.ActorAt(1).assignedHouseId,"Expected hunger-specific commute contrast not reproduced.");
                Run(hungry,495);Run(fed,495);ActorLine("hungry after500",hungry,1);ActorLine("fed after500",fed,1);
                Require(hungry.HouseAt(hungry.ActorAt(1).x,hungry.ActorAt(1).y)!=hungry.ActorAt(1).assignedHouseId && fed.HouseAt(fed.ActorAt(1).x,fed.ActorAt(1).y)==fed.ActorAt(1).assignedHouseId,"Long-distance return contrast not reproduced.");
                File.WriteAllText(Path.Combine(output,"hungry-commute-control.obsave2"),State(hungry));File.WriteAllText(Path.Combine(output,"fed-commute-control.obsave2"),State(fed));
                Note("CONFIRMED: ordinary commute safety reuses restocking hunger/health gates; home location is known, but no hidden home stock is revealed to the NPC.");

                ActorLine("courier before",courier,0);World normal=Copy(courier);normal.Step();ActorLine("normal next decision",normal,0);
                Require(courier.ActorAt(0).hunger==60 && courier.ActorAt(0).carriedFood==3 && normal.ActorAt(0).hunger==60 && normal.ActorAt(0).carriedFood==3,"Expected loaded hungry courier reproduced differently.");
                // A direct internal-routine contrast isolates the return-before-eat guard. It is deliberately not a normal tick or proposed patch.
                MethodInfo forage=typeof(World).GetMethod("TryForage",BindingFlags.Instance|BindingFlags.NonPublic);
                Require(forage!=null,"Forage diagnostic hook missing.");
                World routed=Copy(courier);object[] routedArgs={0,routed.ActorAt(0),-1,8};Require(!(bool)forage.Invoke(routed,routedArgs),"Returning forage was not preempted.");
                saved=courier.Capture();saved.actors[0].homeShelterId=0;World bypass=World.Restore(saved);int foodBefore=bypass.FoodSupplies().RemainingMeals;object[] bypassArgs={0,bypass.ActorAt(0),-1,8};
                Require((bool)forage.Invoke(bypass,bypassArgs),"Diagnostic return-flag bypass did not reach the eater.");ActorLine("test-only return-flag bypass",bypass,0);
                Require(bypass.ActorAt(0).hunger==10 && bypass.ActorAt(0).carriedFood==2 && bypass.FoodSupplies().RemainingMeals==foodBefore-1,"Eating counterfactual did not consume exactly one finite meal.");
                Note("CONFIRMED: ReturnFromFoodDanger can preempt consumption of an already carried meal before TryForage reaches its eating branch. No extra meal, teleport, supply transfer or shipment completion was created.");
                Note("Budget: ten actors start hunger10; natural gain480/day requires at least8 meals each to finish below100, or80 total before additional recovery costs. Initial78 cannot meet that target even with perfect delivery. This is not proof every death is unavoidable.");
                Note("Result: investigative contrasts PASS; daily routine health FAIL/needs repair. No production rules or map edits. Cartographer package still pending.");
                File.WriteAllText(Path.Combine(output,"HUNGER_INVESTIGATION.txt"),Report.ToString());return 0;
            }
            catch(Exception error){Note("INVESTIGATION FAILURE: "+error);File.WriteAllText(Path.Combine(output,"HUNGER_INVESTIGATION.txt"),Report.ToString());return 1;}
        }
    }
}
