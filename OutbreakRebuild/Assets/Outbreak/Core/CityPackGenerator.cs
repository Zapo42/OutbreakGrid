using System;
using System.Collections.Generic;
using System.Text;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    internal sealed class PackParcel
    {
        internal int X,Y,Column,Row,Lot,BoxX,BoxY;
        internal PackBlueprint Blueprint;
        internal PackProfile Profile;
        internal string Id { get { return "parcel-"+Row+"-"+Column+"-"+Lot; } }
        internal int SpawnX { get { return X+Blueprint.Anchors[0]; } }
        internal int SpawnY { get { return Y+Blueprint.Anchors[1]; } }
    }
    internal sealed class PackPlan
    {
        internal int[] Xs,Ys;
        internal int Road,Sidewalk,Width,Height,Columns,Rows,Density;
        internal uint Seed;
        internal CityPackTheme Theme;
        internal PackProfile[] Profiles;
        internal readonly List<PackParcel> Parcels=new List<PackParcel>();
        internal readonly List<int> Parks=new List<int>();
    }
    internal static class CityPackGenerator
    {
        // Planning uses its own stable streams. No geometry decision consumes the world's movement RNG.
        internal static uint Mix(uint seed,uint domain,int index)
        {
            uint value=seed^domain^unchecked((uint)(index+1)*0x9E3779B9u);value^=value>>16;value=unchecked(value*0x7FEB352Du);value^=value>>15;value=unchecked(value*0x846CA68Bu);value^=value>>16;return value;
        }
        private static int MaximumDimension(BuildingUse use)
        {
            int size=0;for(int i=((int)use-1)*8;i<(int)use*8;i++)size=Math.Max(size,Math.Max(CityPackData.Blueprints[i].Width,CityPackData.Blueprints[i].Height));return size;
        }
        internal static PackPlan Plan(uint seed,int columns,int rows,int density,CityPackTheme theme)
        {
            var p=new PackPlan{Seed=seed,Theme=theme,Columns=columns,Rows=rows,Density=density,Profiles=new PackProfile[columns*rows],Xs=new int[columns+1],Ys=new int[rows+1]};
            BuildingUse[] zones=CityGenerator.PlanZones(seed,columns,rows,CityZoning.NeighborhoodCenters);
            int[] widths=new int[columns],heights=new int[rows];int homes=0,jobs=0;
            for(int i=0;i<zones.Length;i++){
                var profile=CityPackData.Profiles[theme==CityPackTheme.Mixed?(int)(Mix(seed,0xD157u,i)%9):(int)theme-1];p.Profiles[i]=profile;p.Road=Math.Max(p.Road,profile.Road);p.Sidewalk=Math.Max(p.Sidewalk,profile.Sidewalk);
                if(zones[i]==BuildingUse.Home)homes+=density;else jobs++;
            }
            int minimumHomes=Math.Min(homes,jobs+1); // Keep at least one consumer where the chosen density can support one.
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){
                int index=row*columns+col;var f=p.Profiles[index];bool home=zones[index]==BuildingUse.Home;
                bool park=home && Mix(seed,0xFA42u,index)%1000<(uint)f.Parks && homes-density>=minimumHomes;
                if(park){homes-=density;p.Parks.Add(index);}
                int count=home?density:1,slotsX=count>1?2:1,slotsY=count>2?2:1;
                int maxSize=MaximumDimension(zones[index]),gap=Math.Max(1,f.Gap),margin=p.Road+2*p.Sidewalk+2*f.Setback;
                int variationX=(int)(Mix(seed,0xA113u,index)%(uint)(Math.Max(1,f.Width*f.Variation/1000)*2+1))-f.Width*f.Variation/1000;
                int variationY=(int)(Mix(seed,0xB113u,index)%(uint)(Math.Max(1,f.Height*f.Variation/1000)*2+1))-f.Height*f.Variation/1000;
                widths[col]=Math.Max(widths[col],Math.Max(f.Width+variationX,margin+slotsX*maxSize+(slotsX-1)*gap));heights[row]=Math.Max(heights[row],Math.Max(f.Height+variationY,margin+slotsY*maxSize+(slotsY-1)*gap));
            }
            p.Xs[0]=5;p.Ys[0]=5;for(int i=0;i<columns;i++)p.Xs[i+1]=p.Xs[i]+widths[i];for(int i=0;i<rows;i++)p.Ys[i+1]=p.Ys[i]+heights[i];p.Width=p.Xs[columns]+p.Road+p.Sidewalk+5;p.Height=p.Ys[rows]+p.Road+p.Sidewalk+5;
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){
                int index=row*columns+col;if(p.Parks.Contains(index))continue;var profile=p.Profiles[index];BuildingUse use=zones[index];int count=use==BuildingUse.Home?density:1;
                int west=p.Xs[col]+p.Road+p.Sidewalk+profile.Setback,east=p.Xs[col+1]-p.Sidewalk-profile.Setback,north=p.Ys[row]+p.Road+p.Sidewalk+profile.Setback,south=p.Ys[row+1]-p.Sidewalk-profile.Setback;
                for(int lot=0;lot<count;lot++){
                    int variant=Mix(seed,0xB100u,index*4+lot)%100<(uint)profile.Spacious?1:0,rotation=(int)(Mix(seed,0xB101u,index*4+lot)%4);
                    PackBlueprint b=CityPackData.Blueprints[((int)use-1)*8+variant*4+rotation];
                    int left=west,right=east,top=north,bottom=south,gap=Math.Max(1,profile.Gap);
                    if(count>1){int half=(east-west+1-gap)/2;if(lot%2==0)right=west+half-1;else left=west+half+gap;}
                    if(count>2){int half=(south-north+1-gap)/2;if(lot<2)bottom=north+half-1;else top=north+half+gap;}
                    int slackX=right-left-b.Width+1,slackY=bottom-top-b.Height+1;if(slackX<0 || slackY<0)throw new ArgumentException("City-pack lot cannot fit its complete template.");
                    int x=left+(int)(Mix(seed,0xB102u,index*4+lot)%(uint)(slackX+1)),y=top+(int)(Mix(seed,0xB103u,index*4+lot)%(uint)(slackY+1));
                    p.Parcels.Add(new PackParcel{X=x,Y=y,Column=col,Row=row,Lot=lot,Blueprint=b,Profile=profile});
                }
            }
            return p;
        }
        private static void Road(WorldMap map,int x,int y,int width,int height)
        {
            for(int cy=y;cy<y+height;cy++)for(int cx=x;cx<x+width;cx++)map.Set(cx,cy,Terrain.Road);
        }
        internal static WorldMap Build(PackPlan p)
        {
            var map=new WorldMap(p.Width,p.Height,"City pack: "+CityPackSetup.ThemeName(p.Theme)+" (seed "+p.Seed+")","city-pack-runtime-1");
            for(int x=0;x<p.Width;x++){map.Set(x,0,Terrain.Wall);map.Set(x,p.Height-1,Terrain.Wall);}for(int y=1;y<p.Height-1;y++){map.Set(0,y,Terrain.Wall);map.Set(p.Width-1,y,Terrain.Wall);}
            // A connected horizontal/outer backbone survives every loop/spur roll, including sparse suburbs.
            foreach(int y in p.Ys)Road(map,p.Xs[0],y,p.Xs[p.Columns]-p.Xs[0]+p.Road,p.Road);
            for(int col=0;col<=p.Columns;col++)for(int row=0;row<p.Rows;row++){
                var profile=p.Profiles[row*p.Columns+Math.Min(col,p.Columns-1)];uint roll=Mix(p.Seed,0x5700u,row*(p.Columns+1)+col);int length=p.Ys[row+1]-p.Ys[row]+p.Road;
                if(col==0 || col==p.Columns || roll%1000<(uint)profile.Loops)Road(map,p.Xs[col],p.Ys[row],p.Road,length);
                else if(Mix(p.Seed,0x5701u,row*(p.Columns+1)+col)%1000<(uint)profile.Spurs)Road(map,p.Xs[col],p.Ys[row],p.Road,length/2);
            }
            // Curbs follow the actual network, including the ends of short streets.
            var roads=new List<int>();for(int y=1;y<p.Height-1;y++)for(int x=1;x<p.Width-1;x++)if(map.Get(x,y)==Terrain.Road)roads.Add(y*p.Width+x);
            foreach(int key in roads){int x=key%p.Width,y=key/p.Width;for(int oy=-p.Sidewalk;oy<=p.Sidewalk;oy++)for(int ox=-p.Sidewalk;ox<=p.Sidewalk;ox++)if(map.Get(x+ox,y+oy)==Terrain.Grass)map.Set(x+ox,y+oy,Terrain.Sidewalk);} // Include corner cells so a cul-de-sac cap joins both curb strips cardinally.
            foreach(int x in p.Xs)foreach(int y in p.Ys){for(int k=0;k<p.Road;k++)foreach(int side in new[]{-1,p.Road}){if(map.Get(x+side,y+k)==Terrain.Road)map.Set(x+side,y+k,Terrain.Crosswalk);if(map.Get(x+k,y+side)==Terrain.Road)map.Set(x+k,y+side,Terrain.Crosswalk);}}
            var reserved=new HashSet<int>();var groups=new List<BuildingRoomsDocument>();
            foreach(var parcel in p.Parcels){var b=parcel.Blueprint;
                for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){map.Set(parcel.X+x,parcel.Y+y,Terrain.Floor);reserved.Add((parcel.Y+y)*p.Width+parcel.X+x);}
                PaintCells(map,parcel,b.Walls,Terrain.Wall);PaintCells(map,parcel,b.Doors,Terrain.DoorClosed);
                var members=new BuildingRoomDocument[b.RoomIds.Length];for(int i=0;i<members.Length;i++)members[i]=new BuildingRoomDocument{id=b.RoomIds[i],role=b.Roles[i],x=parcel.X+b.Anchors[i*2],y=parcel.Y+b.Anchors[i*2+1]};groups.Add(new BuildingRoomsDocument{id=parcel.Id,rooms=members});
                StampFixtures(map,parcel);
            }
            foreach(var parcel in p.Parcels){var b=parcel.Blueprint;for(int i=0;i<b.External.Length;i+=2){int x=b.External[i],y=b.External[i+1];int outsideX=parcel.X+x+(x==0?-1:x==b.Width-1?1:0),outsideY=parcel.Y+y+(y==0?-1:y==b.Height-1?1:0);Connect(map,p,reserved,outsideX,outsideY);}}
            foreach(int park in p.Parks){int col=park%p.Columns,row=park/p.Columns,cx=(p.Xs[col]+p.Xs[col+1])/2,cy=(p.Ys[row]+p.Ys[row+1])/2;Connect(map,p,reserved,cx,cy);for(int n=-3;n<=3;n++){map.Set(cx+n,cy,Terrain.Sidewalk);map.Set(cx,cy+n,Terrain.Sidewalk);}}
            map.SetBuildingRooms(groups.ToArray());
            MapDocument saved=map.Capture();var meta=new StringBuilder("{\"generator\":\""+CityPackData.Version+"\",\"source_sha256\":\""+CityPackData.SourceHash+"\",\"seed\":"+p.Seed+",\"columns\":"+p.Columns+",\"rows\":"+p.Rows+",\"homes_per_block\":"+p.Density+",\"theme\":\""+p.Theme+"\",\"attribution\":\"© OpenStreetMap contributors; see CityReferences/NOTICE.md\",\"parcels\":[");
            for(int i=0;i<p.Parcels.Count;i++){var b=p.Parcels[i];if(i>0)meta.Append(',');meta.Append("{\"id\":\"").Append(b.Id).Append("\",\"district\":\"").Append(b.Profile.Id).Append("\",\"template\":\"").Append(b.Blueprint.Id).Append("\",\"x\":").Append(b.X).Append(",\"y\":").Append(b.Y).Append('}');}meta.Append("],\"parks\":[");for(int i=0;i<p.Parks.Count;i++){if(i>0)meta.Append(',');meta.Append(p.Parks[i]);}meta.Append("]}");saved.authoringJson=meta.ToString();return WorldMap.Restore(saved);
        }
        private static void PaintCells(WorldMap map,PackParcel p,int[] cells,Terrain terrain){for(int i=0;i<cells.Length;i+=2)map.Set(p.X+cells[i],p.Y+cells[i+1],terrain);}
        private static void StampFixtures(WorldMap map,PackParcel p)
        {
            var b=p.Blueprint;PackFixture stock=null;foreach(var f in b.Fixtures)if(f.Role=="storage-shelf" && stock==null)stock=f;
            if(stock==null)throw new ArgumentException("City-pack building lacks its primary inventory.");p.BoxX=p.X+stock.Cells[0];p.BoxY=p.Y+stock.Cells[1];int food=b.Use==BuildingUse.DistributionCenter?40:b.Use==BuildingUse.Home?2:0;
            map.SetStorageBox(new StorageBoxDocument{x=p.BoxX,y=p.BoxY,capacity=b.Use==BuildingUse.DistributionCenter?60:12,foodReserve=b.Use==BuildingUse.Home?2:b.Use==BuildingUse.Store?4:0,items=food==0?new ItemStackDocument[0]:new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=food}}});
            int shelfFood=4;foreach(var f in b.Fixtures){
                if(f.Role=="sales-shelf")for(int i=0;i<f.Cells.Length;i+=2){int meals=shelfFood>0?1:0;shelfFood-=meals;map.SetShelf(p.X+f.Cells[i],p.Y+f.Cells[i+1],meals,true);}
                if(f.Role=="seating")for(int i=0;i<f.Cells.Length;i+=2)map.SetRestSpot(p.X+f.Cells[i],p.Y+f.Cells[i+1],true);
                if(f.Role=="order-computer")PaintCells(map,p,f.Cells,Terrain.Wall); // Same solid authoring placeholder as the supplied DC pack; no invented computer behavior.
            }
            if(b.Use==BuildingUse.Store)map.SetPhone(p.X+b.Phone[0],p.Y+b.Phone[1]);if(b.Use!=BuildingUse.Home)map.SetRestSpot(p.SpawnX,p.SpawnY,true);
        }
        private static void Connect(WorldMap map,PackPlan p,HashSet<int> reserved,int x,int y)
        {
            if(!map.Contains(x,y) || reserved.Contains(y*p.Width+x))throw new ArgumentException("A city-pack entrance has no exterior clearance.");
            int start=y*p.Width+x,goal=-1;var previous=new Dictionary<int,int>();var queue=new Queue<int>();previous[start]=-1;queue.Enqueue(start);int[] dx={0,1,0,-1},dy={-1,0,1,0};
            while(queue.Count>0){int key=queue.Dequeue(),cx=key%p.Width,cy=key/p.Width;if(map.Get(cx,cy)==Terrain.Sidewalk){goal=key;break;}
                for(int d=0;d<4;d++){int nx=cx+dx[d],ny=cy+dy[d],next=ny*p.Width+nx;if(!map.Contains(nx,ny) || reserved.Contains(next) || previous.ContainsKey(next))continue;Terrain t=map.Get(nx,ny);if(t!=Terrain.Grass && t!=Terrain.Sidewalk)continue;previous[next]=key;queue.Enqueue(next);}}
            if(goal<0)throw new ArgumentException("A city-pack entrance cannot reach a sidewalk.");for(int key=goal;key>=0;key=previous[key])if(map.Get(key%p.Width,key/p.Width)==Terrain.Grass)map.Set(key%p.Width,key/p.Width,Terrain.Sidewalk);
        }
    }
}
