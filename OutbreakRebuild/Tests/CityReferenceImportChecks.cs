using System;
using System.IO;
using Outbreak.Core;
using Outbreak.Persistence;

public static class CityReferenceImportChecks
{
    public static int Main(string[] args)
    {
        if(args.Length<1 || args.Length>2)throw new ArgumentException("Pass the reference atlas directory, optionally followed by generated.");
        bool generated=args.Length==2 && args[1]=="generated";
        string[] paths=Directory.GetFiles(args[0],"*.obmap2");
        if(paths.Length<3)throw new InvalidOperationException("Expected the three published pilot samples.");
        foreach(string path in paths)
        {
            WorldMap map=Documents.ParseMap(File.ReadAllText(path));
            int roads=0;
            for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)
            {
                if(map.Get(x,y)==Terrain.Road)roads++;
                if(!generated && map.Get(x,y)==Terrain.Floor)throw new InvalidOperationException("Template invented an interior.");
            }
            if(roads==0 || !map.AuthoringJson.Contains("provenance") || !map.AuthoringJson.Contains("cell_metres"))throw new InvalidOperationException("Missing geometry, source or scale.");
            string encoded=Documents.Encode(map.Capture());
            if(encoded!=Documents.Encode(Documents.ParseMap(encoded).Capture()))throw new InvalidOperationException("Map round-trip diverged.");
            World world=new World(map,214,encounter:generated,hunger:generated);int actor;
            if(generated){using(var metadata=System.Text.Json.JsonDocument.Parse(map.AuthoringJson)){int buildings=metadata.RootElement.GetProperty("buildings").GetArrayLength();if(world.ShelterRoomCount!=buildings)throw new InvalidOperationException("Generated building interiors did not form distinct shelter rooms.");}}
            bool placed=false;
            for(int y=0;y<map.Height && !placed;y++)for(int x=0;x<map.Width && !placed;x++)
                if(map.Get(x,y)==Terrain.Road)placed=world.TrySpawn(Outbreak.Contracts.Species.Human,x,y,out actor);
            if(!placed)throw new InvalidOperationException("No usable road spawn.");
            for(int i=0;i<10;i++)world.Step();
            Console.WriteLine("PASS reference import/round-trip/actor ticks: "+Path.GetFileName(path)+" ("+map.Width+" x "+map.Height+", "+roads+" road cells)");
        }
        return 0;
    }
}
