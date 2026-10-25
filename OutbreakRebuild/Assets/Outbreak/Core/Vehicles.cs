using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class VehicleRules { public const int Maximum=16,MoveTicks=3,RouteRadius=256,MaxRouteCells=16384; }
    public sealed partial class World
    {
        private readonly List<VehicleState> vehicles=new List<VehicleState>();private int nextVehicleId=1;
        private static readonly int[] carDX={1,0,-1,0},carDY={0,1,0,-1};
        public int VehicleCount { get { return vehicles.Count; } }
        public VehicleState VehicleAtIndex(int index){if(index<0 || index>=vehicles.Count)throw new ArgumentOutOfRangeException("index");return vehicles[index];}
        private bool VehicleAt(int x,int y,int exceptId=0){if(vehicles.Count==0)return false;foreach(VehicleState car in vehicles)if(car.id!=exceptId && car.x==x && car.y==y)return true;return false;}
        private bool BodyAt(int x,int y,int exceptActor=0){for(int i=0;i<count;i++)if(actors[i].id!=exceptActor && !ActorRiding(actors[i].id) && actors[i].x==x && actors[i].y==y)return true;return false;}
        private bool VehicleRoad(int x,int y){return map.Contains(x,y) && (TerrainRules.RoadSurface(map.Get(x,y)) || (ParkingTrafficEnabled && map.ParkingExitAt(x,y)>=0)) && map.CanWalk(x,y);}
        public bool TryPlaceVehicle(int x,int y,out int id)
        {
            id=0;if(!VehicleRoutesEnabled || vehicles.Count==VehicleRules.Maximum || nextVehicleId==int.MaxValue || !VehicleRoad(x,y) || VehicleAt(x,y) || BodyAt(x,y) || (map.ParkingExitAt(x,y)>=0 && ParkingClaimed(Key(x,y),0)))return false;
            int heading=0,mask=VehicleDirections(x,y);while((mask&(1<<heading))==0)heading++;
            id=nextVehicleId++;vehicles.Add(new VehicleState{id=id,x=x,y=y,heading=heading});return true;
        }
        private int VehicleRoute(VehicleState car,int goalX,int goalY)
        {
            if(ParkingTrafficEnabled && car.detourUntilTick>Tick){int detour=TrafficDetour(car,goalX,goalY);if(detour>=0)return detour;}
            if(CourierVehiclesEnabled && car.driverId!=0){int planned=AssignedVehicleRoadStep(car,goalX,goalY);if(planned!=-2)return planned;}
            int radius=DistrictRoutesEnabled?DistrictRouteRules.Radius:VehicleRules.RouteRadius,maxCells=DistrictRoutesEnabled?32768:VehicleRules.MaxRouteCells;
            if(!VehicleRoad(goalX,goalY) || Math.Max(Math.Abs(car.x-goalX),Math.Abs(car.y-goalY))>radius)return -1;
            int start=Key(car.x,car.y),goal=Key(goalX,goalY);var first=new Dictionary<int,int>{{start,4}};var queue=new Queue<int>();queue.Enqueue(start);
            int[] order={car.heading,(car.heading+1)%4,(car.heading+3)%4,(car.heading+2)%4};
            while(queue.Count>0)
            {
                int cell=queue.Dequeue();if(cell==goal)return first[cell];int x=cell%Width,y=cell/Width;
                foreach(int direction in order)
                {
                    int nx=x+carDX[direction],ny=y+carDY[direction];if(!VehicleLink(x,y,direction) || Math.Max(Math.Abs(nx-car.x),Math.Abs(ny-car.y))>radius || !VehicleRoad(nx,ny))continue;
                    int key=Key(nx,ny);if(first.ContainsKey(key))continue;if(first.Count==maxCells)return -1;
                    first.Add(key,cell==start?direction:first[cell]);queue.Enqueue(key);
                }
            }
            return -1;
        }
        public bool TrySetVehicleDestination(int index,int x,int y){return SetVehicleDestination(index,x,y,false);}
        private bool SetVehicleDestination(int index,int x,int y,bool driverCommand)
        {
            if(!VehicleRoutesEnabled || index<0 || index>=vehicles.Count)return false;VehicleState car=vehicles[index];if(car.driverId!=0 && !driverCommand)return false;
            if(!VehicleRoad(x,y) || (JunctionTrafficEnabled && JunctionCell(x,y)) || VehicleAt(x,y,car.id) || (map.ParkingExitAt(x,y)>=0 && ParkingClaimed(Key(x,y),car.id)) || VehicleRoute(car,x,y)<0)return false;
            ClearTrafficRecovery(ref car);car.nextRoutePlanTick=0;car.junctionWaitTick=0;car.hasDestination=true;car.destinationX=x;car.destinationY=y;car.waitReason=VehicleWaitReason.None;car.nextMoveTick=car.x==x && car.y==y?0:Tick+VehicleRules.MoveTicks;vehicles[index]=car;return true;
        }
        public bool TryStopVehicle(int index)
        {
            if(!VehicleRoutesEnabled || index<0 || index>=vehicles.Count)return false;VehicleState car=vehicles[index];ClearTrafficRecovery(ref car);if(car.driverId!=0)car.deliveriesPaused=true;car.exitAtTick=0;car.nextRoutePlanTick=0;car.junctionWaitTick=0;car.hasDestination=false;car.destinationX=0;car.destinationY=0;car.nextMoveTick=0;car.waitReason=VehicleWaitReason.None;vehicles[index]=car;return true;
        }
        private bool CrossingOccupied(int x,int y)
        {
            if(map.Get(x,y)!=Terrain.Crosswalk)return BodyAt(x,y);
            var seen=new HashSet<int>();var queue=new Queue<int>();int start=Key(x,y);seen.Add(start);queue.Enqueue(start);
            while(queue.Count>0)
            {
                int cell=queue.Dequeue(),cx=cell%Width,cy=cell/Width;if(BodyAt(cx,cy))return true;
                for(int d=0;d<4;d++){int nx=cx+carDX[d],ny=cy+carDY[d];if(!map.Contains(nx,ny) || map.Get(nx,ny)!=Terrain.Crosswalk)continue;int next=Key(nx,ny);if(!seen.Add(next))continue;if(seen.Count>64)return true;queue.Enqueue(next);}
            }
            return false;
        }
        private void StepVehicles()
        {
            PrepareJunctionRequests();
            VehicleState[] junctionOccupants=JunctionTrafficEnabled?vehicles.ToArray():null;
            for(int i=0;i<vehicles.Count;i++)
            {
                VehicleState car=vehicles[i];if(car.driverId!=0 && (!car.driverAboard || car.deliveriesPaused || !HealthyVehicleDriver(car)))continue;if(!car.hasDestination || car.nextMoveTick==0 || car.nextMoveTick>Tick)continue;
                car.nextMoveTick=Tick+VehicleRules.MoveTicks;int direction=VehicleRoute(car,car.destinationX,car.destinationY);
                if(direction<0){car.waitReason=VehicleWaitReason.NoRoute;vehicles[i]=car;continue;}
                int nx=car.x+carDX[direction],ny=car.y+carDY[direction];
                VehicleWaitReason junctionReason=JunctionWaitReason(car,nx,ny,junctionOccupants);
                if(junctionReason!=VehicleWaitReason.None){car.waitReason=junctionReason;RecordTrafficBlock(ref car);vehicles[i]=car;continue;}
                if(CrossingOccupied(nx,ny)){car.waitReason=VehicleWaitReason.Pedestrian;vehicles[i]=car;continue;}
                if(VehicleAt(nx,ny,car.id)){car.waitReason=VehicleWaitReason.Vehicle;RecordTrafficBlock(ref car);vehicles[i]=car;continue;}
                if(FollowingSegment(car.x,car.y,nx,ny,direction) && VehicleAt(nx+carDX[direction],ny+carDY[direction],car.id)){car.waitReason=VehicleWaitReason.FollowingDistance;RecordTrafficBlock(ref car);vehicles[i]=car;continue;}
                if(BayMergeBlocked(car,nx,ny)){car.waitReason=VehicleWaitReason.FollowingDistance;RecordTrafficBlock(ref car);vehicles[i]=car;continue;}
                car.blockedSinceTick=car.trafficRetryTick=0;car.junctionWaitTick=0;car.x=nx;car.y=ny;car.heading=direction;car.waitReason=VehicleWaitReason.None;
                if(car.x==car.destinationX && car.y==car.destinationY){car.nextMoveTick=0;ClearTrafficRecovery(ref car);}
                vehicles[i]=car;MoveVehicleDriver(car);
            }
        }
        public string VehicleStatus(int index)
        {
            VehicleState car=VehicleAtIndex(index);if(!car.hasDestination)return "Parked";if(car.x==car.destinationX && car.y==car.destinationY)return "Arrived";
            return car.waitReason==VehicleWaitReason.Intersection?"Waiting for intersection":car.waitReason==VehicleWaitReason.ExitBlocked?"Waiting for clear exit":car.waitReason==VehicleWaitReason.FollowingDistance?"Keeping following distance":car.waitReason==VehicleWaitReason.Pedestrian?"Yielding to pedestrian":car.waitReason==VehicleWaitReason.Vehicle?"Waiting for vehicle":car.waitReason==VehicleWaitReason.NoRoute?"Route unavailable":(car.detourUntilTick>Tick?"Taking traffic detour":"En route");
        }
        private VehicleState[] CaptureVehicles(){return vehicles.Count==0?null:vehicles.ToArray();}
        private void RestoreVehicles(VehicleState[] records,int next)
        {
            if(records==null){if(next!=0)throw new ArgumentException("Inactive vehicle IDs.");return;}
            if(!VehicleRoutesEnabled || records.Length<1 || records.Length>VehicleRules.Maximum || next<=0)throw new ArgumentException("Unsupported vehicle data.");
            int previous=0;
            foreach(VehicleState car in records)
            {
                if(car.id<=previous || car.id>=next || car.heading<0 || car.heading>3 || !VehicleRoad(car.x,car.y) || VehicleAt(car.x,car.y) || BodyAt(car.x,car.y,car.driverAboard?car.driverId:0) || car.waitReason<VehicleWaitReason.None || car.waitReason>VehicleWaitReason.ExitBlocked || (!LaneTrafficEnabled && car.waitReason==VehicleWaitReason.FollowingDistance))throw new ArgumentException("Invalid vehicle identity, placement or heading.");
                ValidateJunctionRequest(car);ValidateTrafficRecovery(car);
                if(!car.hasDestination){if(car.destinationX!=0 || car.destinationY!=0 || car.nextMoveTick!=0 || car.waitReason!=VehicleWaitReason.None)throw new ArgumentException("Parked vehicle retained a route.");}
                else
                {
                    if(!map.Contains(car.destinationX,car.destinationY) || (!TerrainRules.RoadSurface(map.Get(car.destinationX,car.destinationY)) && map.ParkingExitAt(car.destinationX,car.destinationY)<0))throw new ArgumentException("Invalid vehicle destination.");
                    bool arrived=car.x==car.destinationX && car.y==car.destinationY;
                    if(arrived?(car.nextMoveTick!=0 || car.waitReason!=VehicleWaitReason.None):(car.nextMoveTick<=Tick || car.nextMoveTick-Tick>VehicleRules.MoveTicks))throw new ArgumentException("Invalid vehicle movement clock.");
                }
                vehicles.Add(car);previous=car.id;
            }
            nextVehicleId=next;
        }
    }
}
