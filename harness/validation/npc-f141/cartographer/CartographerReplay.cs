// Observation-only integration diagnostic; source/game behavior is not modified or corrected by this program.
using System;
using System.IO;
using System.Text;
using Outbreak.Core;
using Outbreak.Persistence;
public static class CartographerReplay
{
    private static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
    private static string State(World w){return Documents.Encode(w.Capture());}
    public static int Main(string[] args)
    {
        string original=File.ReadAllText(args[0]);string output=args[1];Directory.CreateDirectory(output);
        World supplied=Documents.ParseSave(original);Require(State(supplied)==original,"Supplied106 save did not roundtrip exactly.");
        var report=new StringBuilder();report.AppendLine("Cartographer supplied seed214 save: current integrated reader; no source/geometry/stock intervention. Observation/replay, not survival acceptance.");
        foreach(string rules in new[]{EncounterRules.SocialFamiliarityVersion,EncounterRules.Version})
        {
            var initial=supplied.Capture();initial.rulesVersion=rules;World world=World.Restore(initial);int actors=world.Count;
            File.WriteAllText(Path.Combine(output,rules+"-initial.obsave2"),State(world));
            for(int tick=0;tick<14400;tick++)
            {
                world.Step();
                if(tick%600==0)
                {
                    string checkpoint=State(world);World replay=Documents.ParseSave(checkpoint);
                    for(int n=0;n<20;n++){world.Step();replay.Step();}
                    Require(State(world)==State(replay),"Cartographer save replay diverged.");world=Documents.ParseSave(checkpoint);
                }
            }
            Require(world.Count==actors && actors==11,"Unexpected actor count.");
            Require(world.Capture().rulesVersion==rules,"Legacy rules upgraded implicitly.");
            string final=State(world);Require(State(Documents.ParseSave(final))==final,"Final save invalid.");
            int deaths=0;for(int i=0;i<world.Count;i++)if(world.ActorAt(i).health<=0)deaths++;
            int credits=0;foreach(var o in world.PendingSupplyOrders())credits+=o.deliveredMeals;foreach(var o in world.CompletedSupplyOrders())credits+=o.deliveredMeals;
            File.WriteAllText(Path.Combine(output,rules+"-final.obsave2"),final);
            report.AppendLine(rules+": replay/roundtrip PASS; ticks="+world.Tick+" actors="+actors+" deaths="+deaths+" retainedOrderCredits="+credits+" foodRemaining="+world.FoodSupplies().RemainingMeals+" needsPriority="+world.NeedsPriorityEnabled);
        }
        report.AppendLine("No behavioral success threshold or starvation-prevention claim. Native Unity NOT RUN.");
        File.WriteAllText(Path.Combine(output,"CARTOGRAPHER_REPLAY.txt"),report.ToString());Console.Write(report.ToString());return 0;
    }
}
