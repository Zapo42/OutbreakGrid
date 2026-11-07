using System;using System.Linq;using System.IO;using System.Text;using System.Text.Json;using System.Security.Cryptography;using Outbreak.Core;using Outbreak.Contracts;using Outbreak.Persistence;
class CityPackTrial
{
    static string State(World w){return Documents.Encode(w.Capture());}
    static void MapSheet(World w,string path)
    {
        var map=WorldMap.Restore(w.Capture().map);var svg=new StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' width='"+map.Width*4+"' height='"+map.Height*4+"' viewBox='0 0 "+map.Width+" "+map.Height+"'>");
        for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++){Terrain t=map.Get(x,y);string color=t==Terrain.Wall?"#26333b":t==Terrain.Floor?"#C6C7AE":t==Terrain.DoorClosed?"#79583d":t==Terrain.Sidewalk?"#80877a":t==Terrain.Crosswalk?"#ba975f":t==Terrain.Road?"#454c50":"#556b3c";svg.Append("<rect x='"+x+"' y='"+y+"' width='1' height='1' fill='"+color+"'/>");}svg.Append("</svg>");File.WriteAllText(path,svg.ToString());
    }
    static void Main(string[] args)
    {
        Directory.CreateDirectory(args[0]);foreach(var theme in new[]{CityPackTheme.Mixed,CityPackTheme.SuburbanGrid,CityPackTheme.BusinessCore}){
            string dir=Path.Combine(args[0],theme.ToString());Directory.CreateDirectory(dir);World w=CityPackSetup.Create(214,2,2,2,theme),copy=Documents.ParseSave(State(w));File.WriteAllText(Path.Combine(dir,"initial.obsave"),State(w));MapSheet(w,Path.Combine(dir,"map.svg"));int initial=w.FoodSupplies().RemainingMeals,eaten=0,minHealth=100;bool publicDoorOpened=false,internalDoorOpened=false;int moved=0;
            var previous=w.Capture().actors;
            for(int t=0;t<2400;t++){
                previous=w.Capture().actors;w.Step();copy.Step();if(State(w)!=State(copy))throw new Exception("Pack exact replay diverged at "+w.Tick);
                for(int i=0;i<w.Count;i++){var a=w.ActorAt(i);if(a.health>0 && previous[i].health>0 && a.hunger<previous[i].hunger && previous[i].carriedFood-a.carriedFood==1)eaten++;minHealth=Math.Min(minHealth,a.health);if(a.x!=previous[i].x || a.y!=previous[i].y)moved++;}
                if(w.FoodSupplies().RemainingMeals+eaten!=initial)throw new Exception("Unexplained pack food change at "+w.Tick);
                if(t%100==0){copy=Documents.ParseSave(State(w));File.WriteAllText(Path.Combine(dir,"tick"+w.Tick+".obsave"),State(w));}
            }
            var final=w.Capture();var map=WorldMap.Restore(final.map);foreach(var p in CityPackGenerator.Plan(214,2,2,2,theme).Parcels){for(int i=0;i<p.Blueprint.Doors.Length;i+=2){int x=p.X+p.Blueprint.Doors[i],y=p.Y+p.Blueprint.Doors[i+1];if(map.Get(x,y)==Terrain.DoorOpen){bool outside=false;for(int j=0;j<p.Blueprint.External.Length;j+=2)if(p.Blueprint.External[j]==p.Blueprint.Doors[i] && p.Blueprint.External[j+1]==p.Blueprint.Doors[i+1])outside=true;if(outside)publicDoorOpened=true;else internalDoorOpened=true;}}}
            var result=new{theme=theme.ToString(),seed=214,ticks=w.Tick,initialFood=initial,remainingFood=w.FoodSupplies().RemainingMeals,eaten,minHealth,movementSteps=moved,publicDoorOpenAtEnd=publicDoorOpened,internalDoorOpenAtEnd=internalDoorOpened,creditedMeals=w.CompletedSupplyOrders().Sum(o=>o.deliveredMeals)+w.PendingSupplyOrders().Sum(o=>o.deliveredMeals),completedOrders=w.CompletedSupplyOrders().Length,exactReplaySteps=w.Tick,sourceSha256=CityPackSetup.SourceHash,finalSha256=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(State(w)))),nativeUnity="NOT RUN — paused"};File.WriteAllText(Path.Combine(dir,"RESULTS.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));File.WriteAllText(Path.Combine(dir,"final.obsave"),State(w));Console.WriteLine(JsonSerializer.Serialize(result));
        }
    }
}
