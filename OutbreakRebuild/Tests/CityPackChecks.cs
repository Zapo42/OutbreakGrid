using System;
using System.IO;using System.Security.Cryptography;using System.Text;
using System.Linq;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static HashSet<int> PackFlood(WorldMap map,int start,Func<int,int,bool> pass)
    {
        var seen=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);int[] dx={0,1,0,-1},dy={-1,0,1,0};
        while(queue.Count>0){int key=queue.Dequeue(),x=key%map.Width,y=key/map.Width;for(int d=0;d<4;d++){int nx=x+dx[d],ny=y+dy[d],next=ny*map.Width+nx;if(map.Contains(nx,ny) && pass(nx,ny) && seen.Add(next))queue.Enqueue(next);}}return seen;
    }
    private static void CheckCityPackCases()
    {
        Check("Runtime city packs retain nine calibrated themes and all24 validated building template rotations",()=>{
            Require(CityPackData.Profiles.Length==9 && CityPackData.Blueprints.Length==24 && CityPackSetup.SourceHash.Length==64,"Bundled city sources missing.");
            for(int use=1;use<=3;use++)for(int variant=0;variant<2;variant++)for(int rotation=0;rotation<4;rotation++){var b=CityPackData.Blueprints[(use-1)*8+variant*4+rotation];Require((int)b.Use==use && b.RoomIds.Length==2 && b.Anchors.Length==4 && b.Id.EndsWith("-r"+rotation) && b.External.Length==(use==1?2:4),"Template layout/rotation contract changed.");}
        });
        Check("City-pack setups reject invalid inputs and physical-room overflow before generation",()=>{
            foreach(int side in new[]{int.MinValue,0,1,11,int.MaxValue}){Require(CityPackSetup.ValidationError(214,side,2)!=null && CityPackSetup.ValidationError(214,2,side)!=null,"Invalid pack dimensions accepted.");Reject(()=>CityPackSetup.Create(214,side,2));}
            Reject(()=>CityPackSetup.Create(0,4,2));Reject(()=>CityPackSetup.GenerateMap(214,2,2,0));Reject(()=>CityPackSetup.Describe(214,8,8,4));Reject(()=>CityPackSetup.Create(214,2,2,2,(CityPackTheme)10));Reject(()=>CityPackSetup.Create(214,2,2,2,(CityPackTheme)(-1)));Require(CityPackSetup.ValidationError(214,8,8,2)==null,"Supported250-room setup rejected.");
        });
        Check("Pack lots keep exact room templates physical spacing and connected sidewalks across themes seeds and densities",()=>{
            for(int theme=0;theme<10;theme++)foreach(uint seed in new[]{1u,214u,uint.MaxValue})foreach(int density in new[]{1,2,4}){
                var plan=CityPackGenerator.Plan(seed,2,2,density,(CityPackTheme)theme);WorldMap map=CityPackGenerator.Build(plan);var document=map.Capture();Require(document.restSpots.Length==plan.Parcels.Count && document.restSpots.All(r=>!r.area),"Generated buildings lack visible rest stations.");Require(document.storageBoxes.Length==plan.Parcels.Count,"Generated buildings lack real food storage stations.");Require(document.schemaVersion==14 && !document.householdLife && document.buildingRooms.Length==plan.Parcels.Count && document.authoringJson.Contains(CityPackSetup.SourceHash),"Pack groups/provenance or existing needs policy lost.");
                var sidewalks=new HashSet<int>();var roads=new HashSet<int>();for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++){Terrain t=map.Get(x,y);if(t==Terrain.Sidewalk || t==Terrain.Crosswalk)sidewalks.Add(y*map.Width+x);if(TerrainRules.RoadSurface(t))roads.Add(y*map.Width+x);}
                Require(sidewalks.Count>0 && PackFlood(map,sidewalks.First(),(x,y)=>sidewalks.Contains(y*map.Width+x)).SetEquals(sidewalks),"Pack sidewalk/crosswalk graph disconnected: theme "+theme+", seed "+seed+", density "+density);Require(roads.Count>0 && PackFlood(map,roads.First(),(x,y)=>roads.Contains(y*map.Width+x)).SetEquals(roads),"Seeded street backbone disconnected.");
                foreach(var parcel in plan.Parcels){var b=parcel.Blueprint;var profile=parcel.Profile;
                    Require(parcel.X>=plan.Xs[parcel.Column]+plan.Road+plan.Sidewalk+profile.Setback && parcel.X+b.Width-1<=plan.Xs[parcel.Column+1]-plan.Sidewalk-profile.Setback && parcel.Y>=plan.Ys[parcel.Row]+plan.Road+plan.Sidewalk+profile.Setback && parcel.Y+b.Height-1<=plan.Ys[parcel.Row+1]-plan.Sidewalk-profile.Setback,"Template broke its district setback.");
                    foreach(var other in plan.Parcels)if(other!=parcel){int gap=parcel.Column==other.Column && parcel.Row==other.Row?Math.Max(1,profile.Gap):0;Require(parcel.X+b.Width+gap<=other.X || other.X+other.Blueprint.Width+gap<=parcel.X || parcel.Y+b.Height+gap<=other.Y || other.Y+other.Blueprint.Height+gap<=parcel.Y,"Pack footprints overlap or violate lot spacing.");}
                    for(int i=0;i<b.Walls.Length;i+=2)Require(map.Get(parcel.X+b.Walls[i],parcel.Y+b.Walls[i+1])==Terrain.Wall,"Pack wall changed during path placement.");
                    for(int i=0;i<b.Doors.Length;i+=2)Require(map.Get(parcel.X+b.Doors[i],parcel.Y+b.Doors[i+1])==Terrain.DoorClosed,"Template door missing.");
                    for(int i=0;i<b.External.Length;i+=2){int x=b.External[i],y=b.External[i+1],outsideX=parcel.X+x+(x==0?-1:x==b.Width-1?1:0),outsideY=parcel.Y+y+(y==0?-1:y==b.Height-1?1:0);Require(map.Get(outsideX,outsideY)==Terrain.Sidewalk && sidewalks.Contains(outsideY*map.Width+outsideX),"Public/service entrance lacks a connected clear sidewalk.");}
                    var accessible=PackFlood(map,parcel.SpawnY*map.Width+parcel.SpawnX,(x,y)=>x>=parcel.X && x<parcel.X+b.Width && y>=parcel.Y && y<parcel.Y+b.Height && (map.CanWalk(x,y) || map.Get(x,y)==Terrain.DoorClosed));
                    var beds=document.restSpots.Where(r=>r.x>=parcel.X && r.x<parcel.X+b.Width && r.y>=parcel.Y && r.y<parcel.Y+b.Height).ToArray();Require(beds.Length==1 && accessible.Contains(beds[0].y*map.Width+beds[0].x),"Bed station is absent or inaccessible inside its own building.");
                    foreach(var shelf in document.shelves??Array.Empty<ShelfDocument>())if(shelf.x>=parcel.X && shelf.x<parcel.X+b.Width && shelf.y>=parcel.Y && shelf.y<parcel.Y+b.Height)Require(new[]{-1,1}.Any(dx=>accessible.Contains(shelf.y*map.Width+shelf.x+dx)) || new[]{-1,1}.Any(dy=>accessible.Contains((shelf.y+dy)*map.Width+shelf.x)),"A generated food shelf has no reachable approach.");
                    for(int i=0;i<b.Anchors.Length;i+=2)Require(accessible.Contains((parcel.Y+b.Anchors[i+1])*map.Width+parcel.X+b.Anchors[i]),"An internal template room is unreachable.");
                    foreach(var box in document.storageBoxes)if(box.x==parcel.BoxX && box.y==parcel.BoxY)Require(new[]{-1,1}.Any(dx=>accessible.Contains(box.y*map.Width+box.x+dx)) || new[]{-1,1}.Any(dy=>accessible.Contains((box.y+dy)*map.Width+box.x)),"Pack stock box has no physical approach.");
                }
                Require(Documents.Encode(document)==Documents.Encode(WorldMap.Restore(document).Capture()),"Pack map roundtrip not canonical.");
            }
        });
        Check("Pack generation repeats exact seeds varies geometry and preserves source metadata after saving",()=>{
            foreach(var theme in new[]{CityPackTheme.Mixed,CityPackTheme.GardenSuburb,CityPackTheme.DenseUrban}){string first=State(CityPackSetup.Create(214,4,2,2,theme));Require(first==State(CityPackSetup.Create(214,4,2,2,theme)),"Pack seed/settings did not repeat.");var other=CityPackSetup.GenerateMap(215,4,2,2,theme).Capture();Require(first!=Documents.Encode(other) && CityPackSetup.GenerateMap(214,4,2,2,theme).Capture().chunks.SelectMany(c=>c.cells).SequenceEqual(other.chunks.SelectMany(c=>c.cells))==false,"Different seed changed only metadata.");World restored=Documents.ParseSave(first);Require(State(restored)==first && restored.Capture().map.authoringJson.Contains("homes_per_block"),"Source/settings metadata lost in live save.");}
        });
        Check("All city-pack themes create separately housed staff with finite seeded food and exact summaries",()=>{
            for(int theme=0;theme<10;theme++)foreach(uint seed in new[]{1u,214u,uint.MaxValue}){
                var w=CityPackSetup.Create(seed,4,2,2,(CityPackTheme)theme);var s=CityPackSetup.Describe(seed,4,2,2,(CityPackTheme)theme);Require(!w.ShelterScanLimited && w.ShelterRoomCount==s.Buildings && w.Count==s.Humans && w.Count==s.Homes && w.Width==s.Width && w.Height==s.Height && w.FoodSupplies().RemainingMeals==s.Meals && w.StoreSupplies().Length==s.Stores && w.DistributionCenters().Length==1,"Pack preview summary differs from real city.");
                var homes=new HashSet<int>();int hunger=0,food=0;for(int i=0;i<w.Count;i++){var a=w.ActorAt(i);Require(homes.Add(a.assignedHouseId) && a.assignedHouseId!=0,"Pack housing lost uniqueness.");int pantry=w.StorageBoxSites().Where(b=>w.HouseAt(b.x,b.y)==a.assignedHouseId).Sum(b=>StorageRules.Count(b,InventoryItem.Food));Require(pantry+a.carriedFood==3,"Template household manufactured food.");hunger+=a.hunger;food+=a.carriedFood;if(w.Occupation(i)!=HumanOccupation.Consumer)Require(w.HouseAt(a.x,a.y)!=a.assignedHouseId,"Pack employee lives in workplace.");}Require(hunger==w.Count*10 && food==w.Count,"Template generation changed mean needs or food.");
                Run(w,20);World copy=Documents.ParseSave(State(w));Run(w,30);Run(copy,30);Require(State(w)==State(copy),"Live pack saved continuation diverged.");
            }
        });
        Check("Mixed cities omit full grid rows and use staggered connected branching streets",()=>{
            foreach(uint seed in new[]{1u,214u,215u,uint.MaxValue}){
                var p=CityPackGenerator.Plan(seed,4,2,2,CityPackTheme.Mixed);var edges=CityPackGenerator.StreetNetwork(p);int gridEdges=p.Rows*(p.Columns+1)+(p.Rows+1)*p.Columns;Require(edges.Count(e=>e.Full)>=p.NodeX.Length-1 && edges.Count(e=>e.Full)<gridEdges,"Mixed retained a compulsory full lattice.");
                int straightRows=0;for(int row=0;row<=p.Rows;row++)if(Enumerable.Range(0,p.Columns).All(col=>edges.Any(e=>e.Full && e.A==row*(p.Columns+1)+col && e.B==e.A+1)))straightRows++;
                Require(straightRows<p.Rows+1 && Enumerable.Range(0,p.NodeX.Length).Any(i=>p.NodeX[i]!=p.Xs[i%(p.Columns+1)]) && Enumerable.Range(0,p.NodeY.Length).Any(i=>p.NodeY[i]!=p.Ys[i/(p.Columns+1)]),"Mixed roads have no omitted rows or real intersection offsets.");
                WorldMap map=CityPackGenerator.Build(p);int longest=0;for(int y=0;y<map.Height;y++){int run=0;for(int x=0;x<map.Width;x++){run=TerrainRules.RoadSurface(map.Get(x,y))?run+1:0;longest=Math.Max(longest,run);}}Require(longest<map.Width*0.85,"Mixed still contains full-width straight street bands.");
            }
        });
        Check("Generated beds provide actual paid recovery and old runtime1 worlds continue unchanged",()=>{
            World w=CityPackSetup.Create(214,2,2,2);var saved=w.Capture();int index=Array.FindIndex(saved.actors,a=>a.clerkStoreId==0 && a.distributionCenterId==0);var actor=saved.actors[index];var bed=saved.map.restSpots.First(r=>w.HouseAt(r.x,r.y)==actor.assignedHouseId);actor.x=bed.x;actor.y=bed.y;actor.shelterId=actor.assignedHouseId;actor.shelterX=bed.x;actor.shelterY=bed.y;actor.shelterState=ShelterState.Inside;actor.energySpent=80;actor.hunger=10;actor.carriedFood=1;saved.actors[index]=actor;w=World.Restore(saved);Run(w,5);Require(w.ActorAt(index).energySpent<80 && w.ActorAt(index).hunger>10 && w.ActorAt(index).x==bed.x && w.ActorAt(index).y==bed.y,"Visible bed was only decorative or recovery became free.");
            w=Documents.ParseSave(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","citypack1-mixed214-initial.json")));Run(w,2400);Require(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(State(w))))=="EAB19F8A02CACA4A461A1980D9D2A61160B6A552AD149C2F298C77DD97D6B80B","Published runtime1 save behavior changed.");
        });
        Check("Mixed public and service walkways retain real paid courier deliveries",()=>{
            World w=CityPackSetup.Create(214,2,2,2,CityPackTheme.Mixed);int food=w.FoodSupplies().RemainingMeals;Run(w,2400);int credits=w.CompletedSupplyOrders().Sum(o=>o.deliveredMeals)+w.PendingSupplyOrders().Sum(o=>o.deliveredMeals);Require(credits>0 && w.FoodSupplies().RemainingMeals<food && w.Capture().actors.All(a=>a.health==100),"Mixed street detours lost all delivery access or added free supplies.");
        });
        Check("Expanded packed cities fit the existing256-room and finite-floor contracts",()=>{
            var world=CityPackSetup.Create(214,8,8,2);var summary=CityPackSetup.Describe(214,8,8,2);Require(world.ShelterRoomCount==summary.Buildings && !world.ShelterScanLimited && world.Capture().map.buildingRooms.Sum(g=>g.rooms.Length)<=256 && world.RestSpotSites().Length<=4096,"Expanded pack silently cropped rooms or recovery areas.");
        });
    }
}
