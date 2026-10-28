using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class ParkVisitRules
    {
        public const int MaximumKnown=8,StayTicks=120;
    }
    public sealed partial class World
    {
        private bool KnowsPark(ActorState actor,int cell)
        {
            return ParkVisitsEnabled && actor.knownParks!=null && Array.BinarySearch(actor.knownParks,cell+1)>=0;
        }
        public bool TryRememberPark(int index,int x,int y)
        {
            // This is an explicit familiarity/observation input, not a scan of hidden city metadata.
            if(!ParkVisitsEnabled || index<0 || index>=count || !LeisureOutdoor(x,y) || !map.CanWalk(x,y))return false;
            var actor=actors[index];if(actor.species!=Species.Human || actor.health<=0)return false;int cell=Key(x,y)+1;
            if(actor.knownParks!=null && Array.BinarySearch(actor.knownParks,cell)>=0)return true;
            int length=actor.knownParks==null?0:actor.knownParks.Length;if(length>=ParkVisitRules.MaximumKnown)return false;
            var known=new int[length+1];if(length>0)Array.Copy(actor.knownParks,known,length);known[length]=cell;Array.Sort(known);actor.knownParks=known;actors[index]=actor;return true;
        }
        public bool TryForgetPark(int index,int x,int y)
        {
            if(!ParkVisitsEnabled || index<0 || index>=count || !map.Contains(x,y))return false;var actor=actors[index];int cell=Key(x,y)+1;
            int at=actor.knownParks==null?-1:Array.BinarySearch(actor.knownParks,cell);if(at<0)return false;
            int length=actor.knownParks.Length-1;int[] known=length==0?null:new int[length];if(length>0){Array.Copy(actor.knownParks,0,known,0,at);Array.Copy(actor.knownParks,at+1,known,at,length-at);}actor.knownParks=known;
            // Losing a destination ends the optional outing; established shelter/home routines choose the safe return.
            if(actor.leisureParkVisit && actor.leisureX==x && actor.leisureY==y){EndLeisure(ref actor);ClearShelterDeparture(ref actor);}actors[index]=actor;return true;
        }
        private void ValidateParkVisits()
        {
            foreach(var actor in actors)
            {
                if(actor.knownParks!=null){if(!ParkVisitsEnabled || actor.species!=Species.Human || actor.health<=0 || actor.knownParks.Length<1 || actor.knownParks.Length>ParkVisitRules.MaximumKnown)throw new ArgumentException("Invalid park knowledge owner or count.");int previous=0;foreach(int cell in actor.knownParks){if(cell<=previous || cell>Width*Height)throw new ArgumentException("Invalid park knowledge location or order.");previous=cell;}}
                if(actor.leisureParkVisit && (!ParkVisitsEnabled || actor.leisurePhase==HumanLeisurePhase.None || !KnowsPark(actor,Key(actor.leisureX,actor.leisureY))))throw new ArgumentException("Invalid active park visit.");
            }
        }
    }
}
