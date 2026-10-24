using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class CityRouteRules
    {
        public const int Radius=256, MaxVisited=16384;
    }
    public sealed partial class World
    {
        private struct CityFrontier : IComparable<CityFrontier>
        {
            public int key,steps,cost,estimate,first;
            public int CompareTo(CityFrontier other)
            {
                int c=(cost+estimate).CompareTo(other.cost+other.estimate);
                if(c==0)c=estimate.CompareTo(other.estimate);
                return c==0?key.CompareTo(other.key):c;
            }
        }
        // A* uses a bounded sparse frontier rather than allocating the full map.
        // Recompute from live geometry: saves need no stale route cache or random draws.
        private static int CityEstimate(int x,int y){x=Math.Abs(x);y=Math.Abs(y);return 10*Math.Max(x,y)+4*Math.Min(x,y);}
        private int LongRoute(ActorState actor,int goalX,int goalY,bool walkOnly,out int distance)
        {
            distance=0;
            if(!map.Contains(goalX,goalY) || Math.Max(Math.Abs(goalX-actor.x),Math.Abs(goalY-actor.y))>CityRouteRules.Radius || !ShelterWalkable(goalX,goalY))return -1;
            var frontier=new SortedSet<CityFrontier>();var best=new Dictionary<int,int>();
            int start=Key(actor.x,actor.y),goal=Key(goalX,goalY);
            frontier.Add(new CityFrontier{key=start,first=8,estimate=CityEstimate(goalX-actor.x,goalY-actor.y)});best[start]=0;
            while(frontier.Count>0)
            {
                CityFrontier current=frontier.Min;frontier.Remove(current);
                if(best[current.key]!=current.cost)continue;
                if(current.key==goal){distance=current.steps;return current.first;}
                int x=current.key%Width,y=current.key/Width;
                for(int d=0;d<8;d++)
                {
                    int nx=x+dx[d],ny=y+dy[d];
                    if(Math.Max(Math.Abs(nx-actor.x),Math.Abs(ny-actor.y))>CityRouteRules.Radius || !ShelterWalkable(nx,ny) || (walkOnly && !map.CanWalk(nx,ny)) || !RoutineDistributionStepAllowed(actor,nx,ny))continue;
                    int key=Key(nx,ny);if(!occupancy.Allows(key,Species.Human) || ZombieAt(nx,ny))continue;
                    if(nx!=x && ny!=y && (IsDoor(x,y) || IsDoor(nx,ny) || !map.CanWalk(nx,y) || !map.CanWalk(x,ny)))continue;
                    int next=current.cost+(nx!=x && ny!=y?14:10),old;
                    if(best.TryGetValue(key,out old) && old<=next)continue;
                    if(!best.ContainsKey(key) && best.Count==CityRouteRules.MaxVisited)return -1;
                    best[key]=next;
                    frontier.Add(new CityFrontier{key=key,steps=current.steps+1,cost=next,estimate=CityEstimate(goalX-nx,goalY-ny),first=current.key==start?d:current.first});
                }
            }
            return -1;
        }
    }
}
