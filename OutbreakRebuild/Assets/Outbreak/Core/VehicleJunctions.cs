using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        // Adjacent marked cells with multiple exits form a junction. Unmarked roads keep legacy routing.
        private bool JunctionCell(int x,int y)
        {
            int mask=map.RoadLaneMaskAt(x,y);return mask!=0 && (mask&(mask-1))!=0;
        }
        private HashSet<int> JunctionCells(int x,int y)
        {
            var cells=new HashSet<int>();var queue=new Queue<int>();cells.Add(Key(x,y));queue.Enqueue(Key(x,y));
            while(queue.Count>0 && cells.Count<=64)
            {
                int key=queue.Dequeue(),cx=key%Width,cy=key/Width;
                for(int d=0;d<4;d++){int nx=cx+carDX[d],ny=cy+carDY[d];if(JunctionCell(nx,ny) && cells.Add(Key(nx,ny)))queue.Enqueue(Key(nx,ny));}
            }
            return cells;
        }
        private void PrepareJunctionRequests()
        {
            if(!JunctionTrafficEnabled)return;
            for(int i=0;i<vehicles.Count;i++)
            {
                VehicleState car=vehicles[i];if(!car.hasDestination || car.nextMoveTick==0 || car.nextMoveTick>Tick)continue;
                int direction=VehicleRoute(car,car.destinationX,car.destinationY);
                bool entering=direction>=0 && direction<4 && !JunctionCell(car.x,car.y) && JunctionCell(car.x+carDX[direction],car.y+carDY[direction]);
                car.junctionWaitTick=entering?(car.junctionWaitTick==0?Tick:car.junctionWaitTick):0;vehicles[i]=car;
            }
        }
        private VehicleWaitReason JunctionEntryClearance(VehicleState car,HashSet<int> cells)
        {
            if(cells.Count>64)return VehicleWaitReason.ExitBlocked;
            // Follow the same deterministic replanning used during motion, through to the first exit.
            for(int step=0;step<=64;step++)
            {
                int d=VehicleRoute(car,car.destinationX,car.destinationY);if(d<0 || d>3)return VehicleWaitReason.ExitBlocked;
                int nx=car.x+carDX[d],ny=car.y+carDY[d];
                if(CrossingOccupied(nx,ny))return VehicleWaitReason.Pedestrian;
                if(VehicleAt(nx,ny,car.id))return VehicleWaitReason.ExitBlocked;
                bool inside=cells.Contains(Key(nx,ny));
                if(!inside)
                {
                    if(FollowingSegment(nx,ny,nx+carDX[d],ny+carDY[d],d) && VehicleAt(nx+carDX[d],ny+carDY[d],car.id))return VehicleWaitReason.ExitBlocked;
                    return VehicleWaitReason.None;
                }
                car.x=nx;car.y=ny;car.heading=d;
            }
            return VehicleWaitReason.ExitBlocked;
        }
        private VehicleWaitReason JunctionWaitReason(VehicleState car,int nx,int ny,VehicleState[] occupants)
        {
            if(!JunctionTrafficEnabled || JunctionCell(car.x,car.y) || !JunctionCell(nx,ny))return VehicleWaitReason.None;
            HashSet<int> cells=JunctionCells(nx,ny);
            // Hold admission for the whole movement update so interpolated departures clear before entry.
            foreach(VehicleState other in occupants)if(other.id!=car.id && cells.Contains(Key(other.x,other.y)))return VehicleWaitReason.Intersection;
            foreach(VehicleState other in vehicles)if(other.id!=car.id && cells.Contains(Key(other.x,other.y)))return VehicleWaitReason.Intersection;
            VehicleWaitReason clearance=JunctionEntryClearance(car,cells);if(clearance!=VehicleWaitReason.None)return clearance;
            // Both requests are stamped before any movement. Earlier arrival wins; ID breaks simultaneous ties.
            foreach(VehicleState other in vehicles)
            {
                if(other.id==car.id || other.junctionWaitTick==0 || !other.hasDestination || JunctionCell(other.x,other.y))continue;
                if(other.junctionWaitTick>car.junctionWaitTick || (other.junctionWaitTick==car.junctionWaitTick && other.id>car.id))continue;
                int d=VehicleRoute(other,other.destinationX,other.destinationY);if(d<0 || d>3 || !cells.Contains(Key(other.x+carDX[d],other.y+carDY[d])))continue;
                if(JunctionEntryClearance(other,cells)==VehicleWaitReason.None)return VehicleWaitReason.Intersection;
            }
            return VehicleWaitReason.None;
        }
        private void ValidateJunctionRequest(VehicleState car)
        {
            bool junctionReason=car.waitReason==VehicleWaitReason.Intersection || car.waitReason==VehicleWaitReason.ExitBlocked;
            if(car.junctionWaitTick<0 || car.junctionWaitTick>Tick || (!JunctionTrafficEnabled && (car.junctionWaitTick!=0 || junctionReason)) || (car.junctionWaitTick!=0 && (!car.hasDestination || car.nextMoveTick==0)) || (junctionReason && car.junctionWaitTick==0))throw new ArgumentException("Invalid intersection waiting state.");
        }
    }
}
