using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool ReturnFromFoodDanger(int index,ref ActorState actor,int threat)
        {
            if(actor.homeShelterId==0)return false;
            Room home=FindRoom(actor.homeShelterId);
            bool returning=home!=null && actor.shelterId==home.id && actor.shelterState==ShelterState.Seeking;
            // A human still crossing out through its original doorway must finish the existing departure.
            if(returning && (!FoodRetreatFallback || actor.shelterExitPhase!=ShelterExitPhase.None))return true;
            if(!returning && (actor.shelterId!=0 || actor.hunger<HungerRules.SeekAt || CanRiskFoodTrip(actor,threat)))return false;
            int threats=KnownShelterThreats(actor,threat),goal,length;
            if(returning)
            {
                if(FoodReturnGoal(actor,home,threats,out goal,out length))return true;
                ClearShelter(ref actor);actors[index]=actor;
            }
            Room destination=home;
            if(!FoodReturnGoal(actor,home,threats,out goal,out length))
            {
                if(!FoodRetreatFallback)return false;
                destination=null;int best=int.MaxValue;
                // Keep the original home when usable; otherwise choose the shortest guarded local route.
                foreach(Room candidate in rooms)
                {
                    int candidateGoal,candidateLength;
                    if(candidate==home || !FoodReturnGoal(actor,candidate,threats,out candidateGoal,out candidateLength) || candidateLength>=best)continue;
                    destination=candidate;goal=candidateGoal;best=candidateLength;
                }
                if(destination==null)return false;
            }
            actor.homeShelterId=destination.id;actor.shelterId=destination.id;
            actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;
            ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        private bool FoodReturnGoal(ActorState actor,Room room,int threats,out int goal,out int length)
        {
            goal=-1;length=0;
            if(room==null || room.id==actor.rejectedShelterId || KnownRoomDanger(actor,room) || RoomAdmissions(room,actor.id)>=room.Capacity)return false;
            goal=PickShelterCell(room,actor);
            return goal>=0 && ShelterRoute(actor,goal%Width,goal/Width,threats,out length,foodRoute:true)>=0;
        }
    }
}
