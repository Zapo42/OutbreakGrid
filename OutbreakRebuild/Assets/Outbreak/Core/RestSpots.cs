using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private readonly HashSet<int> restSpots=new HashSet<int>();
        public bool HasRestSpots { get { return restSpots.Count>0; } }
        public bool RestSpotAt(int x,int y){return Contains(x,y) && restSpots.Contains(y*Width+x);}
        public void SetRestSpot(int x,int y)
        {
            if(!Contains(x,y) || Get(x,y)!=Terrain.Floor || !CanWalk(x,y) || (restSpots.Count>=4096 && !RestSpotAt(x,y)))throw new ArgumentException("Rest spots need free floor cells.");restSpots.Add(y*Width+x);
        }
        internal RestSpotDocument[] CaptureRestSpots()
        {
            if(restSpots.Count==0)return null;List<int> keys=new List<int>(restSpots);keys.Sort();RestSpotDocument[] result=new RestSpotDocument[keys.Count];for(int i=0;i<keys.Count;i++)result[i]=new RestSpotDocument{x=keys[i]%Width,y=keys[i]/Width};return result;
        }
        private void RestoreRestSpots(MapDocument source)
        {
            if(source.restSpots==null)return;if(source.schemaVersion<7 || source.restSpots.Length>4096)throw new ArgumentException("Unsupported rest spots.");
            foreach(RestSpotDocument spot in source.restSpots){if(spot==null || RestSpotAt(spot.x,spot.y))throw new ArgumentException("Invalid or duplicate rest spot.");SetRestSpot(spot.x,spot.y);}
        }
        internal bool RemoveRestSpot(int x,int y){return Contains(x,y) && restSpots.Remove(y*Width+x);}
    }
    public sealed partial class World
    {
        public RestSpotDocument[] RestSpotSites(){return map.CaptureRestSpots()??new RestSpotDocument[0];}
        public bool RestSpotAt(int x,int y){return map.RestSpotAt(x,y);}
        public bool TryPlaceRestSpot(int x,int y)
        {
            if(!RestSpotsEnabled || !map.Contains(x,y) || !map.CanWalk(x,y) || map.Get(x,y)!=Terrain.Floor || map.RestSpotAt(x,y) || RestSpotSites().Length>=4096)return false;
            bool inside=false;foreach(Room room in rooms)if(room.Contains(Key(x,y))){inside=true;break;}if(!inside)return false;
            map.SetRestSpot(x,y);TerrainRevision++;return true;
        }
        public bool TryRemoveRestSpot(int x,int y){if(!RestSpotsEnabled || !map.RemoveRestSpot(x,y))return false;TerrainRevision++;return true;}
        private void PreferRestSpot(ref ActorState actor,Room room,int threat)
        {
            bool preparingFood=ShouldRestBeforeFood(actor,threat);
            if(!RestSpotsEnabled || actor.turnAtTick!=0 || (actor.hunger>=HungerRules.SeekAt && !preparingFood) || threat>=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor))return;
            bool assigned=map.RestSpotAt(actor.shelterX,actor.shelterY);
            if(actor.energySpent==0 && assigned){int normal=PickShelterCell(room,actor);if(normal>=0){actor.shelterX=normal%Width;actor.shelterY=normal/Width;}return;}
            if(EnergyRules.Remaining(actor)>EnergyRules.SeekRestAt && !assigned && !preparingFood)return;
            int best=-1,distance=int.MaxValue;foreach(RestSpotDocument spot in RestSpotSites())
            {
                int key=Key(spot.x,spot.y);if(!room.Contains(key) || !Visible(actor,new ActorState{x=spot.x,y=spot.y}))continue;bool occupied=false;
                for(int i=0;i<count;i++){ActorState other=actors[i];if(other.id!=actor.id && other.species==Species.Human && ((other.x==spot.x && other.y==spot.y) || (other.shelterId==room.id && other.shelterX==spot.x && other.shelterY==spot.y))){occupied=true;break;}}if(occupied)continue;
                int length;if(ShelterRoute(actor,spot.x,spot.y,0,out length,confinedRoom:room)<0)continue;if(length<distance || (length==distance && key<best)){best=key;distance=length;}
            }
            if(best<0 && assigned)best=PickShelterCell(room,actor);
            if(best>=0){actor.shelterX=best%Width;actor.shelterY=best/Width;}
        }
    }
}
