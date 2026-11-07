using System;using System.IO;using System.Linq;using System.Diagnostics;using System.Collections.Generic;using System.Text.Json;using System.Security.Cryptography;using System.Text;using Outbreak.Core;using Outbreak.Contracts;using Outbreak.Persistence;
class StoreFlowLoad
{
    // Match every starting fact; only the selected rules version differs. Observe outside the Core timer.
    static void Main(string[] args)
    {
        Directory.CreateDirectory(args[0]);string control=null;
        foreach(int rules in new[]{112,113}){
            var input=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters).Capture();input.rulesVersion="encounter-prototype-"+rules;string initial=Documents.Encode(input);if(control!=null && initial.Replace("prototype-113","prototype-112")!=control)throw new Exception("Starting facts changed.");control=initial;World w=World.Restore(input);int food=w.FoodSupplies().RemainingMeals,eaten=0,storeMeals=0,entries=0;var previous=w.Capture().actors;var stores=new HashSet<int>(w.StoreSupplies().Select(s=>s.StoreId));double[] times=new double[3600];var process=Process.GetCurrentProcess();double cpu=process.TotalProcessorTime.TotalMilliseconds;
            for(int t=0;t<times.Length;t++){
                long start=Stopwatch.GetTimestamp();w.Step();times[t]=(Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency;
                for(int i=0;i<w.Count;i++){var a=w.ActorAt(i);int room=w.HouseAt(a.x,a.y);if(stores.Contains(room) && w.HouseAt(previous[i].x,previous[i].y)!=room)entries++;if(a.hunger<previous[i].hunger){eaten++;if(stores.Contains(room))storeMeals++;}previous[i]=a;}
                if(w.FoodSupplies().RemainingMeals+eaten!=food)throw new Exception("Finite food ledger failed at "+w.Tick);
            }
            string final=Documents.Encode(w.Capture());var result=new{rules,population=w.Count,seed=214,ticks=3600,initialMeals=food,remainingMeals=w.FoodSupplies().RemainingMeals,eaten,storeMeals,storeEntries=entries,criticalHunger=w.Capture().actors.Count(a=>a.hunger>=80),deaths=w.Capture().actors.Count(a=>a.health<=0),meanCoreTickMs=times.Average(),maxCoreTickMs=times.Max(),processCpuIncludingObservationMs=process.TotalProcessorTime.TotalMilliseconds-cpu,finalSha256=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(final))),nativeUnity="NOT RUN — paused"};File.WriteAllText(Path.Combine(args[0],rules+".json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));
        }
    }
}
