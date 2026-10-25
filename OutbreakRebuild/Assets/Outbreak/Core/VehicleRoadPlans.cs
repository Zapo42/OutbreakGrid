using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        internal long VehicleRoadRevision { get; private set; }
    }
    public sealed partial class World
    {
        // Shared derived distance fields avoid repeating full-road searches for every driving step.
        private readonly Dictionary<int,Dictionary<int,int>> vehicleRoadPlans=new Dictionary<int,Dictionary<int,int>>();
        private readonly Queue<int> vehiclePlanOrder=new Queue<int>();private long vehiclePlanRevision=-1;
        private int AssignedVehicleRoadStep(VehicleState car,int gx,int gy)
        {
            if(!VehicleRoad(gx,gy) || System.Math.Max(System.Math.Abs(car.x-gx),System.Math.Abs(car.y-gy))>DistrictRouteRules.Radius)return -1;
            if(vehiclePlanRevision!=map.VehicleRoadRevision){vehicleRoadPlans.Clear();vehiclePlanOrder.Clear();vehiclePlanRevision=map.VehicleRoadRevision;}
            int goal=Key(gx,gy),start=Key(car.x,car.y);Dictionary<int,int> steps;
            if(!vehicleRoadPlans.TryGetValue(goal,out steps)){
                steps=new Dictionary<int,int>{{goal,0}};var queue=new Queue<int>();queue.Enqueue(goal);
                while(queue.Count>0 && steps.Count<32768){int key=queue.Dequeue(),x=key%Width,y=key/Width;for(int d=0;d<4;d++){
                    int nx=x+carDX[d],ny=y+carDY[d];if(System.Math.Max(System.Math.Abs(nx-gx),System.Math.Abs(ny-gy))>DistrictRouteRules.Radius || !VehicleRoad(nx,ny) || (VehicleDirections(nx,ny)&(1<<((d+2)%4)))==0)continue;int next=Key(nx,ny);if(steps.ContainsKey(next))continue;if(steps.Count==32768)break;steps[next]=steps[key]+1;queue.Enqueue(next);
                }}
                if(vehicleRoadPlans.Count==8)vehicleRoadPlans.Remove(vehiclePlanOrder.Dequeue());vehicleRoadPlans.Add(goal,steps);vehiclePlanOrder.Enqueue(goal);
            }
            int remaining;if(!steps.TryGetValue(start,out remaining))return -2;if(remaining==0)return 4;
            int[] order={car.heading,(car.heading+1)%4,(car.heading+3)%4,(car.heading+2)%4};foreach(int d in order){int nx=car.x+carDX[d],ny=car.y+carDY[d],distance;if((VehicleDirections(car.x,car.y)&(1<<d))!=0 && map.Contains(nx,ny) && steps.TryGetValue(Key(nx,ny),out distance) && distance==remaining-1)return d;}return -2;
        }
    }
}
