using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class TrafficRecoveryRules { public const int WaitTicks=30,DetourTicks=120,Radius=64,MaxCells=4096; }
    public sealed partial class World
    {
        private static void ClearTrafficRecovery(ref VehicleState car){car.blockedSinceTick=car.trafficRetryTick=car.detourUntilTick=0;}
        private int TrafficDetour(VehicleState car,int gx,int gy)
        {
            int start=Key(car.x,car.y),goal=Key(gx,gy);if(start==goal)return 4;
            var queue=new Queue<int>();var first=new Dictionary<int,int>{{start,4}};queue.Enqueue(start);
            int[] order={car.heading,(car.heading+1)%4,(car.heading+3)%4,(car.heading+2)%4};
            while(queue.Count>0){int cell=queue.Dequeue(),x=cell%Width,y=cell/Width;
                foreach(int d in order){int nx=x+carDX[d],ny=y+carDY[d];if(!VehicleLink(x,y,d) || Math.Max(Math.Abs(nx-car.x),Math.Abs(ny-car.y))>TrafficRecoveryRules.Radius || VehicleAt(nx,ny,car.id))continue;
                    // Do not choose a first step that already violates the live following gap.
                    if(cell==start && FollowingSegment(x,y,nx,ny,d) && VehicleAt(nx+carDX[d],ny+carDY[d],car.id))continue;
                    int next=Key(nx,ny);if(first.ContainsKey(next))continue;int step=cell==start?d:first[cell];if(next==goal)return step;if(first.Count==TrafficRecoveryRules.MaxCells)return -1;first.Add(next,step);queue.Enqueue(next);
                }
            }
            return -1;
        }
        private void RecordTrafficBlock(ref VehicleState car)
        {
            if(!ParkingTrafficEnabled || (car.waitReason!=VehicleWaitReason.Vehicle && car.waitReason!=VehicleWaitReason.FollowingDistance && car.waitReason!=VehicleWaitReason.ExitBlocked))return;
            if(car.blockedSinceTick==0)car.blockedSinceTick=Tick;
            if(Tick-car.blockedSinceTick<TrafficRecoveryRules.WaitTicks || car.trafficRetryTick>Tick)return;
            car.trafficRetryTick=Tick+TrafficRecoveryRules.WaitTicks;
            if(TrafficDetour(car,car.destinationX,car.destinationY)>=0)car.detourUntilTick=Tick+TrafficRecoveryRules.DetourTicks;
        }
        private void ValidateTrafficRecovery(VehicleState car)
        {
            if(car.hasDestination && map.ParkingExitAt(car.destinationX,car.destinationY)>=0 && ParkingClaimed(Key(car.destinationX,car.destinationY),car.id))throw new ArgumentException("Parking destination claimed twice.");
            bool any=car.blockedSinceTick!=0 || car.trafficRetryTick!=0 || car.detourUntilTick!=0;
            if((!ParkingTrafficEnabled && any) || (!car.hasDestination && any) || car.blockedSinceTick<0 || car.blockedSinceTick>Tick || car.trafficRetryTick<0 || car.trafficRetryTick>Tick+TrafficRecoveryRules.WaitTicks || car.detourUntilTick<0 || car.detourUntilTick>Tick+TrafficRecoveryRules.DetourTicks)throw new ArgumentException("Invalid traffic recovery state.");
        }
    }
}
