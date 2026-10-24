using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class FoodReturnRules
    {
        public const int MinimumReserve=20,MaximumReserve=40,Buffer=10;
        public static int ReserveForRoute(int steps){return Math.Min(MaximumReserve,Math.Max(MinimumReserve,steps*2+Buffer));}
    }
    public sealed partial class World
    {
        private bool ReturnFromFoodDanger(int index,ref ActorState actor,int threat)
        {
            if(actor.homeShelterId==0)return false;
            Room home=FindRoom(actor.homeShelterId);
            bool returning=home!=null && actor.shelterId==home.id && actor.shelterState==ShelterState.Seeking;
            // A human still crossing out through its original doorway must finish the existing departure.
            if(returning && (!FoodRetreatFallback || actor.shelterExitPhase!=ShelterExitPhase.None))return true;
            bool dangerReturn=actor.hunger>=HungerRules.SeekAt && !CanRiskFoodTrip(actor,threat);
            bool energyReturn=CanReturnForEnergy(actor);
            if(!returning && (actor.shelterId!=0 || (!dangerReturn && !energyReturn)))return false;
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
                if(destination==null)return false;length=best;
            }
            if(!returning && !dangerReturn && EnergyRules.Remaining(actor)>FoodReturnRules.ReserveForRoute(length))return false;
            actor.homeShelterId=destination.id;actor.shelterId=destination.id;
            actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;
            ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        private bool CanReturnForEnergy(ActorState actor)
        {
            if(!EnergyReturnEnabled || actor.species!=Species.Human || actor.turnAtTick!=0 || actor.homeShelterId==0 || EnergyRules.Remaining(actor)>FoodReturnRules.MaximumReserve || (actor.carriedFood==0 && actor.hunger>=FoodRestRules.UrgentHunger(actor)) || (actor.carriedFood>0 && actor.hunger>=HungerRules.SeekAt))return false;
            // Do not walk away from personally known food already within reach.
            if(actor.hunger>=HungerRules.SeekAt && actor.shelfMemory!=null)foreach(ShelfMemoryRecord shelf in actor.shelfMemory)if(shelf.food!=0 && !AvoidBlockedFood(actor,shelf.x,shelf.y) && Math.Abs(actor.x-shelf.x)+Math.Abs(actor.y-shelf.y)==1)return false;
            return true;
        }
        public bool ReturningWithLowEnergy(int index)
        {
            ActorState actor=ActorAt(index);Room home=FindRoom(actor.homeShelterId);
            return EnergyReturnEnabled && actor.species==Species.Human && actor.turnAtTick==0 && home!=null && actor.shelterId==home.id && actor.shelterState==ShelterState.Seeking && actor.shelterExitPhase==ShelterExitPhase.None && !home.Contains(Key(actor.x,actor.y)) && EnergyRules.Remaining(actor)<=FoodReturnRules.MaximumReserve;
        }
        private bool FoodReturnGoal(ActorState actor,Room room,int threats,out int goal,out int length)
        {
            goal=-1;length=0;
            if(room==null || room.id==actor.rejectedShelterId || KnownRoomDanger(actor,room) || RoomAdmissions(room,actor.id)>=room.Capacity)return false;
            goal=PickShelterCell(room,actor);
            if(goal>=0 && ShelterRoute(actor,goal%Width,goal/Width,threats,out length,foodRoute:true)>=0)return true;
            if(!OrderFulfillmentEnabled || actor.distributionCenterId==0)return false;
            int best=int.MaxValue;goal=-1;foreach(int cell in room.cells){int steps;if(!map.CanWalk(cell%Width,cell/Width))continue;if(ShelterRoute(actor,cell%Width,cell/Width,threats,out steps,foodRoute:true)>=0 && steps<best){best=steps;goal=cell;}}
            length=goal<0?0:best;return goal>=0;
        }
    }
}
