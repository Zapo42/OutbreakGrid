using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Outbreak.Core;
using Outbreak.Persistence;
public static class CityPerformance
{
    public static void Main(string[] args)
    {
        if(args.Length==2 && args[0]=="--export"){
            Directory.CreateDirectory(args[1]);foreach(var scene in new[]{(4,2,2),(6,5,2),(6,6,1)}){World world=CitySetup.Create(214,scene.Item1,scene.Item2,scene.Item3,CityStreetLayout.Staggered);string key=scene.Item1+"-"+scene.Item2+"-"+scene.Item3;File.WriteAllText(Path.Combine(args[1],"perf122-start-"+key+".json"),Documents.Encode(world.Capture()));for(int tick=0;tick<500;tick++)world.Step();File.WriteAllText(Path.Combine(args[1],"perf122-end-"+key+".json"),Documents.Encode(world.Capture()));}return;
        }
        // Optional before/after fixture files let two revisions benchmark identical saved geometry and state.
        if(args.Length==1){foreach(string key in new[]{"4-2-2","6-5-2","6-6-1"})Measure(key,File.ReadAllText(Path.Combine(args[0],"perf122-start-"+key+".json")),File.ReadAllText(Path.Combine(args[0],"perf122-end-"+key+".json")));return;}
        foreach(var scene in new[]{(4,2,4),(6,3,4),(6,5,2),(6,6,1)}){World world=CitySetup.Create(214,scene.Item1,scene.Item2,scene.Item3,CityStreetLayout.Staggered);Measure(scene.ToString(),Documents.Encode(world.Capture()),null);}
    }
    private static void Measure(string label,string start,string expected)
    {
        // Warm JIT/serializer separately. Time ticks only; include their managed allocations.
        World warm=Documents.ParseSave(start);for(int tick=0;tick<100;tick++)warm.Step();double[] times=new double[3];long[] bytes=new long[3];
        for(int pass=0;pass<3;pass++){
            World world=Documents.ParseSave(start);long allocated=GC.GetAllocatedBytesForCurrentThread();var timer=Stopwatch.StartNew();for(int tick=0;tick<500;tick++)world.Step();timer.Stop();times[pass]=timer.Elapsed.TotalMilliseconds;bytes[pass]=GC.GetAllocatedBytesForCurrentThread()-allocated;
            string finish=Documents.Encode(world.Capture());if(expected==null)expected=finish;else if(finish!=expected)throw new Exception("Deterministic continuation mismatch: "+label);
        }
        Array.Sort(times);Array.Sort(bytes);Console.WriteLine(string.Format(CultureInfo.InvariantCulture,"{0}: 500 ticks median {1:F1} ms ({2:F3} ms/tick), allocated {3:F2} MiB; exact continuation PASS. Core only; Unity rendering excluded.",label,times[1],times[1]/500,bytes[1]/1048576.0));
    }
}
