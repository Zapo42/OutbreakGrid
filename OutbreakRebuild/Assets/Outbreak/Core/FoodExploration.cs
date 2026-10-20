using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class FoodExplorationRules
    {
        public const int HistoryLimit=8, RememberTicks=300, TaskTicks=150;
    }
    public sealed partial class World
    {
        private static void ClearFoodExploration(ref ActorState actor){actor.foodExploreRoomId=0;actor.foodExploreUntilTick=0;}
        private void RememberFoodRoom(ref ActorState actor,int roomId)
        {
            FoodRoomVisit note=new FoodRoomVisit{roomId=roomId,observedTick=Tick};FoodRoomVisit[] visits=actor.foodRoomVisits;
            if(visits==null){actor.foodRoomVisits=new[]{note};return;}
            for(int i=0;i<visits.Length;i++)if(visits[i].roomId==roomId){visits[i]=note;return;}
            if(visits.Length<FoodExplorationRules.HistoryLimit){Array.Resize(ref visits,visits.Length+1);visits[visits.Length-1]=note;}
            else{int oldest=0;for(int i=1;i<visits.Length;i++)if(visits[i].observedTick<visits[oldest].observedTick)oldest=i;visits[oldest]=note;}
            Array.Sort(visits,(a,b)=>a.roomId.CompareTo(b.roomId));actor.foodRoomVisits=visits;
        }
        private bool RecentlyInvestigated(ActorState actor,int roomId)
        {
            if(actor.foodRoomVisits!=null)foreach(FoodRoomVisit visit in actor.foodRoomVisits)if(visit.roomId==roomId)return true;return false;
        }
        private void AdvanceFoodExploration(ref ActorState actor)
        {
            if(actor.foodRoomVisits!=null)
            {
                int keep=0;foreach(FoodRoomVisit visit in actor.foodRoomVisits)if(Tick-visit.observedTick<FoodExplorationRules.RememberTicks)keep++;
                if(keep==0)actor.foodRoomVisits=null;
                else if(keep<actor.foodRoomVisits.Length){FoodRoomVisit[] retained=new FoodRoomVisit[keep];int i=0;foreach(FoodRoomVisit visit in actor.foodRoomVisits)if(Tick-visit.observedTick<FoodExplorationRules.RememberTicks)retained[i++]=visit;actor.foodRoomVisits=retained;}
            }
            if(actor.foodExploreRoomId!=0 && actor.foodExploreUntilTick<=Tick)AbandonFoodExploration(ref actor);
        }
        private void AbandonFoodExploration(ref ActorState actor)
        {
            int id=actor.foodExploreRoomId;if(id==0)return;RememberFoodRoom(ref actor,id);
            Room room=FindRoom(id);if(actor.shelterId==id && room!=null && !room.Contains(Key(actor.x,actor.y)))ClearShelter(ref actor);
            ClearFoodExploration(ref actor); // An occupant keeps its physical refuge; an outdoor explorer releases admission.
        }
        private void CancelFoodExploration(int index,ref ActorState actor){AbandonFoodExploration(ref actor);actors[index]=actor;}
        private bool SeenFoodEntrance(ActorState actor,Room room)
        {
            if(room.Contains(Key(actor.x,actor.y)))return true;
            foreach(int door in room.doors)
            {
                ActorState entrance=new ActorState{x=door%Width,y=door/Width};if(DistanceSquared(actor,entrance)>EncounterRules.SightRadius*EncounterRules.SightRadius)continue;
                // A closed door blocks rays. Observe its adjacent outer face instead of seeing through it.
                for(int d=0;d<8;d+=2){int x=entrance.x+dx[d],y=entrance.y+dy[d];if(map.CanWalk(x,y) && !room.Contains(Key(x,y)) && Visible(actor,new ActorState{x=x,y=y}))return true;}
            }
            return false;
        }
        private void ExploreFoodRoom(int index,ref ActorState actor,int threat,Room inside)
        {
            if(actor.shelterExitPhase!=ShelterExitPhase.None)return;int threats=KnownShelterThreats(actor,threat),length;
            if(actor.foodExploreRoomId!=0)
            {
                Room current=FindRoom(actor.foodExploreRoomId);
                if(current==null || current.id!=actor.shelterId){ClearFoodExploration(ref actor);return;}
                if(KnownRoomDanger(actor,current) || (inside==null && RoomAdmissions(current,actor.id)>=current.Capacity) || ShelterRoute(actor,actor.shelterX,actor.shelterY,threats,out length,foodRoute:true)<0){CancelFoodExploration(index,ref actor);return;}
                if(actor.x==actor.shelterX && actor.y==actor.shelterY){RememberFoodRoom(ref actor,current.id);ClearFoodExploration(ref actor);}
                return; // Ordinary shelter movement performs entry and closure, without starting a hungry departure mid-investigation.
            }
            if(actor.shelterId!=0 && inside==null)return;Room best=null;int bestGoal=-1,bestLength=int.MaxValue;
            foreach(Room room in rooms)
            {
                if((inside!=null && room!=inside) || RecentlyInvestigated(actor,room.id) || room.id==actor.rejectedShelterId || KnownRoomDanger(actor,room) || RoomAdmissions(room,actor.id)>=room.Capacity || !SeenFoodEntrance(actor,room))continue;
                int goal=PickShelterCell(room,actor);if(goal<0 || ShelterRoute(actor,goal%Width,goal/Width,threats,out length,foodRoute:true)<0 || length>=bestLength)continue;
                best=room;bestGoal=goal;bestLength=length;
            }
            if(best==null)return;
            actor.shelterId=best.id;actor.shelterX=bestGoal%Width;actor.shelterY=bestGoal/Width;actor.shelterState=best.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;
            actor.foodExploreRoomId=best.id;actor.foodExploreUntilTick=Tick+FoodExplorationRules.TaskTicks;ClearShelterDeparture(ref actor);actors[index]=actor;
        }
        private void ValidateFoodExploration()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];
                if(actor.foodExploreRoomId==0){if(actor.foodExploreUntilTick!=0)throw new ArgumentException("Inactive food exploration has a deadline.");}
                else if(!FoodExploration || actor.species!=Species.Human || actor.foodExploreRoomId!=actor.shelterId || FindRoom(actor.foodExploreRoomId)==null || actor.foodExploreUntilTick<=Tick || actor.foodExploreUntilTick-Tick>FoodExplorationRules.TaskTicks)throw new ArgumentException("Invalid active food exploration.");
                if(actor.foodRoomVisits==null)continue;
                if(!FoodExploration || actor.species!=Species.Human || actor.foodRoomVisits.Length==0 || actor.foodRoomVisits.Length>FoodExplorationRules.HistoryLimit)throw new ArgumentException("Invalid food room history.");
                int previous=0;foreach(FoodRoomVisit visit in actor.foodRoomVisits){if(visit.roomId<=previous || FindRoom(visit.roomId)==null || visit.observedTick<0 || visit.observedTick>Tick || Tick-visit.observedTick>=FoodExplorationRules.RememberTicks)throw new ArgumentException("Invalid food room visit.");previous=visit.roomId;}
            }
        }
    }
}
