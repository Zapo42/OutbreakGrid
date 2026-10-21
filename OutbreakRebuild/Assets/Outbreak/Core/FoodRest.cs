using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class FoodRestRules
    {
        public const int DepartureEnergy=60;
        public static int UrgentHunger(ActorState actor){return Math.Max(55,Math.Min(85,70-actor.bravery*3));}
    }
    public sealed partial class World
    {
        // Use only the human's shelter and personal supply knowledge, never hidden stocks.
        private bool ShouldRestBeforeFood(ActorState actor,int threat)
        {
            if(!FoodRestEnabled || actor.species!=Species.Human || actor.turnAtTick!=0 || actor.health<=0 || actor.stress!=0 || threat>=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || actor.carriedFood>0 || actor.hunger<HungerRules.SeekAt || actor.hunger>=FoodRestRules.UrgentHunger(actor) || EnergyRules.Remaining(actor)>=FoodRestRules.DepartureEnergy || actor.shelterExitPhase!=ShelterExitPhase.None || actor.foodExploreRoomId!=0)return false;
            Room room=FindRoom(actor.shelterId);
            return room!=null && room.Contains(Key(actor.x,actor.y)) && !KnownRoomSupplies(actor,room);
        }
        public bool PreparingFoodTrip(int index){ActorState actor=ActorAt(index);return ShouldRestBeforeFood(actor,NearestVisible(actor,Species.Zombie));}
    }
}
