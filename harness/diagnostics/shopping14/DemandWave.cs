using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Core;
using Outbreak.Persistence;
class DemandWave
{
    // Compare identical geometry/food budgets. Only the authorized fresh needs and review policy differ.
    static void Main(string[] args)
    {
        Directory.CreateDirectory(args[0]);
        foreach(bool staggered in args.Length>1 && args[1]=="staggered"?new[]{true}:new[]{false,true})
        {
            var world=EncounterScenarios.GeneratedBuildingTrial(214,8,8,4,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters,staggered);
            if(!staggered){var saved=world.Capture();saved.rulesVersion=EncounterRules.PersonalFoodAccessVersion;world=World.Restore(saved);}
            var process=Process.GetCurrentProcess();double cpuStart=process.TotalProcessorTime.TotalMilliseconds;int food=world.FoodSupplies().RemainingMeals;var initial=world.Capture();int[] starts=new int[3600];double[] times=new double[3600];bool[] shopping=new bool[world.Count];
            for(int tick=0;tick<starts.Length;tick++){
                long began=Stopwatch.GetTimestamp();world.Step();times[tick]=(Stopwatch.GetTimestamp()-began)*1000.0/Stopwatch.Frequency;
                for(int i=0;i<world.Count;i++){bool now=world.ActorAt(i).shoppingMeals>0;if(now && !shopping[i])starts[tick]++;shopping[i]=now;}
            }
            int[] buckets=new int[144];for(int i=0;i<starts.Length;i++)buckets[i/25]+=starts[i];
            string final=Documents.Encode(world.Capture());File.WriteAllText(Path.Combine(args[0],staggered?"staggered-final.obsave":"homogeneous111-final.obsave"),final);
            var result=new{finalSha256=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(final))),staggered,rules=initial.rulesVersion,seed=214,population=world.Count,initialFood=food,initialHungerValues=initial.actors.Select(a=>a.hunger).Distinct().OrderBy(x=>x).ToArray(),initialCarriedValues=initial.actors.Select(a=>a.carriedFood).Distinct().OrderBy(x=>x).ToArray(),ticks=3600,shoppingStartsFirst300=starts.Take(300).Sum(),maxStartsOneTick=starts.Max(),maxStarts25Ticks=buckets.Max(),startsPer25Ticks=buckets,processCpuIncludingObservationMs=process.TotalProcessorTime.TotalMilliseconds-cpuStart,meanCoreTickMs=times.Average(),maxCoreTickMs=times.Max(),nativeRendering="NOT RUN; timings exclude renderer and observation"};
            string text=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true});File.WriteAllText(Path.Combine(args[0],staggered?"staggered.json":"homogeneous111.json"),text);Console.WriteLine(text);
        }
    }
}
