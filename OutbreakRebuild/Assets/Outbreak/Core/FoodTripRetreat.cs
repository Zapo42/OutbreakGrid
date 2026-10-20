using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool ReturnFromFoodDanger(int index,ref ActorState actor,int threat)
        {
            if(actor.homeShelterId==0)return false;Room home=FindRoom(actor.homeShelterId);if(home==null)return false;
            if(actor.shelterId==home.id && actor.shelterState==ShelterState.Seeking)return true; // Keep a return trip committed even if stress drops during the walk.
            if(actor.shelterId!=0 || actor.hunger<HungerRules.SeekAt || CanRiskFoodTrip(actor,threat) || home.id==actor.rejectedShelterId || KnownRoomDanger(actor,home) || RoomAdmissions(home,actor.id)>=home.Capacity)return false;
            int goal=PickShelterCell(home,actor),length;if(goal<0)return false;int threats=KnownShelterThreats(actor,threat);
            if(ShelterRoute(actor,goal%Width,goal/Width,threats,out length,foodRoute:true)<0)return false;
            actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;
            ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true; // Reuse saved home and shelter state; never invent supplies or a new unseen danger location.
        }
    }
}
