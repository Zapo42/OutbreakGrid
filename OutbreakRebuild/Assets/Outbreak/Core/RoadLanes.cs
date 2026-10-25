using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        public const int MaxLaneCells=65536;
        private readonly SortedDictionary<int,int> roadLanes=new SortedDictionary<int,int>();
        public bool HasRoadLanes { get { return roadLanes.Count>0; } }
        public int RoadLaneMaskAt(int x,int y){int mask;return Contains(x,y) && roadLanes.TryGetValue(y*Width+x,out mask)?mask:0;}
        // Bit 0 East, 1 South, 2 West, 3 North. Zero removes the marking.
        public void SetRoadLaneMask(int x,int y,int mask)
        {
            if(!Contains(x,y) || !TerrainRules.RoadSurface(Get(x,y)) || mask<0 || mask>15)throw new ArgumentException("Lane needs roadway and a cardinal direction mask.");
            int key=y*Width+x,old;if(roadLanes.TryGetValue(key,out old) && old==mask)return;if(mask==0){if(roadLanes.Remove(key))VehicleRoadRevision++;return;}
            if(!roadLanes.ContainsKey(key) && roadLanes.Count==MaxLaneCells)throw new ArgumentException("Lane catalog limit reached.");roadLanes[key]=mask;VehicleRoadRevision++;
        }
        private RoadLaneDocument[] CaptureRoadLanes()
        {
            if(roadLanes.Count==0)return null;var result=new RoadLaneDocument[roadLanes.Count];int i=0;foreach(var pair in roadLanes)result[i++]=new RoadLaneDocument{x=pair.Key%Width,y=pair.Key/Width,directions=pair.Value};return result;
        }
        private void RestoreRoadLanes(MapDocument source)
        {
            if(source.roadLanes==null)return;if(source.schemaVersion<12 || source.roadLanes.Length<1 || source.roadLanes.Length>MaxLaneCells)throw new ArgumentException("Invalid road lane catalog.");
            int previous=-1;foreach(RoadLaneDocument lane in source.roadLanes)
            {
                if(lane==null || !Contains(lane.x,lane.y) || lane.directions<1 || lane.directions>15 || lane.y*Width+lane.x<=previous)throw new ArgumentException("Invalid or unordered road lane.");
                SetRoadLaneMask(lane.x,lane.y,lane.directions);previous=lane.y*Width+lane.x;
            }
        }
    }
    public static partial class CityGenerator
    {
        internal static void PaintRoadLanes(WorldMap map,int[] xs,int[] ys)
        {
            var xLanes=new Dictionary<int,int>();var yLanes=new Dictionary<int,int>();
            foreach(int x in xs){xLanes[x]=0;xLanes[x+1]=1;}foreach(int y in ys){yLanes[y]=0;yLanes[y+1]=1;}
            int[] dx={1,0,-1,0},dy={0,1,0,-1};
            for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)
            {
                if(!TerrainRules.RoadSurface(map.Get(x,y)))continue;
                int laneX,laneY;bool vertical=xLanes.TryGetValue(x,out laneX),horizontal=yLanes.TryGetValue(y,out laneY);
                int mask=(vertical?(laneX==0?2:8):0)|(horizontal?(laneY==0?4:1):0),valid=0;
                for(int d=0;d<4;d++)if((mask&(1<<d))!=0 && map.Contains(x+dx[d],y+dy[d]) && TerrainRules.RoadSurface(map.Get(x+dx[d],y+dy[d])))valid|=1<<d;
                // Connect the two lanes at the end of a shortened side street or map-edge spur.
                if(valid==0)
                {
                    int turn=horizontal?(laneY==0?1:3):(laneX==0?0:2);
                    if(map.Contains(x+dx[turn],y+dy[turn]) && TerrainRules.RoadSurface(map.Get(x+dx[turn],y+dy[turn])))valid=1<<turn;
                }
                if(valid==0)throw new ArgumentException("Generated road lane has no legal exit.");map.SetRoadLaneMask(x,y,valid);
            }
        }
    }
    public sealed partial class World
    {
        public int RoadLaneMaskAt(int x,int y){return map.RoadLaneMaskAt(x,y);}
        private int VehicleDirections(int x,int y){int mask=LaneTrafficEnabled?map.RoadLaneMaskAt(x,y):0;return mask==0?15:mask;}
        private bool FollowingSegment(int x,int y,int nx,int ny,int direction)
        {
            if(!LaneTrafficEnabled)return false;int from=map.RoadLaneMaskAt(x,y),to=map.RoadLaneMaskAt(nx,ny),bit=1<<direction;
            return (from==0 || from==bit) && (to==0 || to==bit);
        }
    }
}
