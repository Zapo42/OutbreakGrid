using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        public const int MaxParkingBays=512;
        private readonly SortedDictionary<int,int> parkingBays=new SortedDictionary<int,int>();
        public bool HasParkingBays { get { return parkingBays.Count>0; } }
        public int ParkingExitAt(int x,int y){int direction;return Contains(x,y) && parkingBays.TryGetValue(y*Width+x,out direction)?direction:-1;}
        public void SetParkingBay(int x,int y,int exitDirection)
        {
            int[] dx={1,0,-1,0},dy={0,1,0,-1};
            if(!Contains(x,y) || exitDirection<0 || exitDirection>3 || (Get(x,y)!=Terrain.Grass && Get(x,y)!=Terrain.Sidewalk) || !CanWalk(x,y) || !Contains(x+dx[exitDirection],y+dy[exitDirection]) || Get(x+dx[exitDirection],y+dy[exitDirection])!=Terrain.Road)throw new ArgumentException("A parking bay needs clear grass or sidewalk beside an ordinary road.");
            int key=y*Width+x;if(!parkingBays.ContainsKey(key) && parkingBays.Count==MaxParkingBays)throw new ArgumentException("Parking catalog limit reached.");
            int previous;if(parkingBays.TryGetValue(key,out previous) && previous==exitDirection)return;parkingBays[key]=exitDirection;VehicleRoadRevision++;
        }
        public ParkingBayDocument[] ParkingBaySites(){return CaptureParkingBays()??new ParkingBayDocument[0];}
        private ParkingBayDocument[] CaptureParkingBays()
        {
            if(parkingBays.Count==0)return null;var result=new ParkingBayDocument[parkingBays.Count];int i=0;foreach(var bay in parkingBays)result[i++]=new ParkingBayDocument{x=bay.Key%Width,y=bay.Key/Width,exitDirection=bay.Value};return result;
        }
        private void ParkingTerrainEdit(int x,int y,Terrain terrain)
        {
            if(parkingBays.Count==0)return;bool changed=false;
            if(terrain!=Terrain.Grass && terrain!=Terrain.Sidewalk)changed=parkingBays.Remove(y*Width+x);
            if(terrain!=Terrain.Road)for(int d=0;d<4;d++){int nx=x+(d==0?1:d==2?-1:0),ny=y+(d==1?1:d==3?-1:0),exit;if(Contains(nx,ny) && parkingBays.TryGetValue(ny*Width+nx,out exit) && exit==(d+2)%4)changed=parkingBays.Remove(ny*Width+nx) || changed;}
            if(changed)VehicleRoadRevision++;
        }
        private void RestoreParkingBays(MapDocument source)
        {
            if(source.parkingBays==null)return;if(source.schemaVersion<13 || source.parkingBays.Length<1 || source.parkingBays.Length>MaxParkingBays)throw new ArgumentException("Invalid parking catalog.");
            int previous=-1;foreach(var bay in source.parkingBays){if(bay==null || !Contains(bay.x,bay.y) || bay.y*Width+bay.x<=previous)throw new ArgumentException("Invalid or unordered parking bay.");SetParkingBay(bay.x,bay.y,bay.exitDirection);previous=bay.y*Width+bay.x;}
        }
    }
    public sealed partial class World
    {
        public ParkingBayDocument[] ParkingBays(){return map.ParkingBaySites();}
        public bool TryPlaceParkingBay(int x,int y)
        {
            if(!ParkingTrafficEnabled || !map.Contains(x,y) || map.ParkingExitAt(x,y)>=0 || !map.CanWalk(x,y) || VehicleAt(x,y) || BodyAt(x,y) || (map.Get(x,y)!=Terrain.Grass && map.Get(x,y)!=Terrain.Sidewalk) || map.HasParkingBays && map.ParkingBaySites().Length>=WorldMap.MaxParkingBays)return false;
            for(int d=0;d<4;d++){int nx=x+carDX[d],ny=y+carDY[d];if(!map.Contains(nx,ny) || map.Get(nx,ny)!=Terrain.Road || !map.CanWalk(nx,ny) || JunctionCell(nx,ny))continue;map.SetParkingBay(x,y,d);return true;}return false;
        }
        public bool TryPlaceParkingNearBox(int x,int y)
        {
            if(!ParkingTrafficEnabled || map.StorageBoxAt(x,y)==null)return false;Room room=FindRoom(HouseAt(x,y));if(room==null)return false;var choices=new List<int>();
            for(int by=Math.Max(0,y-CourierVehicleRules.ParkingRadius);by<=Math.Min(Height-1,y+CourierVehicleRules.ParkingRadius);by++)for(int bx=Math.Max(0,x-CourierVehicleRules.ParkingRadius);bx<=Math.Min(Width-1,x+CourierVehicleRules.ParkingRadius);bx++)if(map.Get(bx,by)==Terrain.Sidewalk || map.Get(bx,by)==Terrain.Grass)choices.Add(Key(bx,by));
            choices.Sort((a,b)=>{int c=ParkingDoorDistance(a,room).CompareTo(ParkingDoorDistance(b,room));return c==0?a.CompareTo(b):c;});foreach(int cell in choices)if(TryPlaceParkingBay(cell%Width,cell/Width))return true;return false;
        }
        private bool ParkingClaimed(int cell,int exceptId)
        {
            foreach(var car in vehicles)if(car.id!=exceptId && car.hasDestination && Key(car.destinationX,car.destinationY)==cell)return true;return false;
        }
        private bool BayMergeBlocked(VehicleState car,int nx,int ny)
        {
            if(map.ParkingExitAt(car.x,car.y)<0)return false;int mask=VehicleDirections(nx,ny);
            for(int d=0;d<4;d++)if((mask&(1<<d))!=0 && (VehicleAt(nx+carDX[d],ny+carDY[d],car.id) || VehicleAt(nx-carDX[d],ny-carDY[d],car.id)))return true;return false;
        }
    }
}
