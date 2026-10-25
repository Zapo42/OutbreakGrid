using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class DistrictRouteRules { public const int Radius=512,MaxSurfaceCells=65536,MaxPortals=8192,LocalRadius=32,LookAhead=12; }
    public sealed partial class WorldMap
    {
        internal long DistrictSurfaceRevision { get; private set; }
        private void DistrictSurfaceChanged(int x,int y){Terrain t=Get(x,y);if(t==Terrain.Sidewalk || t==Terrain.Crosswalk)DistrictSurfaceRevision++;if(TerrainRules.RoadSurface(t))VehicleRoadRevision++;}
        internal int[] DistrictSurfaceCells()
        {
            var result=new List<int>();foreach(var chunk in chunks){int cx=chunk.Key%columns,cy=chunk.Key/columns;for(int i=0;i<chunk.Value.Length;i++){
                Terrain t=(Terrain)chunk.Value[i];if(t!=Terrain.Sidewalk && t!=Terrain.Crosswalk)continue;int x=cx*Rules.ChunkSize+i%Rules.ChunkSize,y=cy*Rules.ChunkSize+i/Rules.ChunkSize;
                if(Contains(x,y) && CanWalk(x,y)){if(result.Count==DistrictRouteRules.MaxSurfaceCells)return null;result.Add(y*Width+x);}
            }}result.Sort();return result.ToArray();
        }
    }
    public sealed partial class World
    {
        private sealed class DistrictEdge { public int end;public int[] cells; }
        private HashSet<int> districtSurface,districtPortals;
        private Dictionary<int,List<DistrictEdge>> districtEdges;
        private bool districtGraphAttempted;private long districtGraphRevision=-1;
        private int DistrictDegree(int key)
        {
            int x=key%Width,y=key/Width,total=0;for(int d=0;d<4;d++){int nx=x+carDX[d],ny=y+carDY[d];if(map.Contains(nx,ny) && districtSurface.Contains(Key(nx,ny)))total++;}return total;
        }
        private List<DistrictEdge> BuildDistrictEdges(int start)
        {
            var result=new List<DistrictEdge>();int x=start%Width,y=start/Width;
            for(int d=0;d<4;d++){
                int nx=x+carDX[d],ny=y+carDY[d];if(!map.Contains(nx,ny) || !districtSurface.Contains(Key(nx,ny)))continue;
                int previous=start,current=Key(nx,ny);var cells=new List<int>();
                while(cells.Count<=DistrictRouteRules.MaxSurfaceCells){cells.Add(current);if(districtPortals.Contains(current))break;int next=-1,cx=current%Width,cy=current/Width;
                    for(int turn=0;turn<4;turn++){int tx=cx+carDX[turn],ty=cy+carDY[turn];if(map.Contains(tx,ty)){int key=Key(tx,ty);if(key!=previous && districtSurface.Contains(key)){next=key;break;}}}
                    if(next<0)break;previous=current;current=next;
                }
                if(cells.Count<=DistrictRouteRules.MaxSurfaceCells)result.Add(new DistrictEdge{end=current,cells=cells.ToArray()});
            }
            return result;
        }
        private bool EnsureDistrictGraph()
        {
            if(districtGraphRevision!=map.DistrictSurfaceRevision){districtGraphAttempted=false;districtEdges=null;districtSurface=null;districtPortals=null;districtGraphRevision=map.DistrictSurfaceRevision;}
            if(districtGraphAttempted)return districtEdges!=null;districtGraphAttempted=true;int[] cells=map.DistrictSurfaceCells();if(cells==null || cells.Length==0)return false;
            districtSurface=new HashSet<int>(cells);districtPortals=new HashSet<int>();foreach(int cell in cells)if(DistrictDegree(cell)!=2)districtPortals.Add(cell);
            // Give isolated closed loops a portal too; disconnected components never become imaginary links.
            var seen=new HashSet<int>();var queue=new Queue<int>();foreach(int seed in cells){if(!seen.Add(seed))continue;bool hasPortal=false;queue.Enqueue(seed);while(queue.Count>0){int cell=queue.Dequeue(),x=cell%Width,y=cell/Width;hasPortal|=districtPortals.Contains(cell);for(int d=0;d<4;d++){int nx=x+carDX[d],ny=y+carDY[d];if(map.Contains(nx,ny)){int key=Key(nx,ny);if(districtSurface.Contains(key) && seen.Add(key))queue.Enqueue(key);}}}if(!hasPortal)districtPortals.Add(seed);}
            if(districtPortals.Count>DistrictRouteRules.MaxPortals)return false;
            districtEdges=new Dictionary<int,List<DistrictEdge>>();var ordered=new List<int>(districtPortals);ordered.Sort();foreach(int portal in ordered)districtEdges.Add(portal,BuildDistrictEdges(portal));return true;
        }
        private int DistrictAnchor(int x,int y)
        {
            int key=Key(x,y);if(districtSurface.Contains(key))return key;Room room=FindRoom(HouseAt(x,y));int best=-1,score=int.MaxValue;
            if(room!=null){foreach(int door in room.doors)for(int d=0;d<4;d++){int nx=door%Width+carDX[d],ny=door/Width+carDY[d],next=Key(nx,ny);if(!map.Contains(nx,ny) || room.Contains(next) || !districtSurface.Contains(next))continue;int distance=Math.Abs(nx-x)+Math.Abs(ny-y);if(distance<score || (distance==score && next<best)){score=distance;best=next;}}return best;}
            for(int ny=Math.Max(0,y-16);ny<=Math.Min(Height-1,y+16);ny++)for(int nx=Math.Max(0,x-16);nx<=Math.Min(Width-1,x+16);nx++){int next=Key(nx,ny);if(!districtSurface.Contains(next))continue;int distance=(nx-x)*(nx-x)+(ny-y)*(ny-y);if(distance<score || (distance==score && next<best)){score=distance;best=next;}}return best;
        }
        private int DistrictMacroRoute(int start,int goal,out int distance)
        {
            distance=0;if(start==goal)return goal;cityFrontier.Clear();cityBest.Clear();cityBest[start]=0;PushCityFrontier(new CityFrontier{key=start,first=-1,estimate=10*(Math.Abs(start%Width-goal%Width)+Math.Abs(start/Width-goal/Width))});
            while(cityFrontier.Count>0){CityFrontier current=PopCityFrontier();if(cityBest[current.key]!=current.cost)continue;if(current.key==goal){distance=current.steps;return current.first;}
                List<DistrictEdge> edges;if(!districtEdges.TryGetValue(current.key,out edges))edges=BuildDistrictEdges(current.key);
                foreach(DistrictEdge edge in edges){int goalIndex=Array.IndexOf(edge.cells,goal),length=goalIndex>=0?goalIndex+1:edge.cells.Length,end=goalIndex>=0?goal:edge.end,cost=current.cost+length*10,old;if(cityBest.TryGetValue(end,out old) && old<=cost)continue;if(!cityBest.ContainsKey(end) && cityBest.Count==DistrictRouteRules.MaxPortals)return -1;cityBest[end]=cost;
                    PushCityFrontier(new CityFrontier{key=end,cost=cost,steps=current.steps+length,first=current.key==start?edge.cells[Math.Min(length,DistrictRouteRules.LookAhead)-1]:current.first,estimate=10*(Math.Abs(end%Width-goal%Width)+Math.Abs(end/Width-goal/Width))});
                }
            }return -1;
        }
        private int DistrictRoute(ActorState actor,int goalX,int goalY,bool walkOnly,out int distance)
        {
            distance=0;if(!map.Contains(goalX,goalY) || Math.Max(Math.Abs(goalX-actor.x),Math.Abs(goalY-actor.y))>DistrictRouteRules.Radius || !ShelterWalkable(goalX,goalY) || !RoutineDistributionStepAllowed(actor,goalX,goalY) || !EnsureDistrictGraph())return -1;
            int start=DistrictAnchor(actor.x,actor.y),goal=DistrictAnchor(goalX,goalY);if(start<0 || goal<0)return -1;
            int first=8,startLength=0,tailLength=0;if(start!=Key(actor.x,actor.y)){first=LongRoute(actor,start%Width,start/Width,walkOnly,out startLength,DistrictRouteRules.LocalRadius);if(first<0)return -1;}
            ActorState tail=actor;tail.x=goal%Width;tail.y=goal/Width;if(LongRoute(tail,goalX,goalY,walkOnly,out tailLength,DistrictRouteRules.LocalRadius)<0)return -1;
            int macroLength,waypoint=DistrictMacroRoute(start,goal,out macroLength);if(waypoint<0)return -1;distance=startLength+macroLength+tailLength;
            if(start!=Key(actor.x,actor.y))return first;
            int localLength;return LongRoute(actor,waypoint%Width,waypoint/Width,walkOnly,out localLength,DistrictRouteRules.LocalRadius);
        }
    }
}
