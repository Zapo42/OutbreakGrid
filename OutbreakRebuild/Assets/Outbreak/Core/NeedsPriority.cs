using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool SafeNeedsAction(ActorState actor,int threat)
        {
            // These priorities never take an action away from an observed threat, infection escape or unresolved grapple/freeze.
            return actor.species==Species.Human && actor.health>0 && actor.turnAtTick==0 && actor.grappledById==0 && actor.stress==0 && threat<0 && actor.freezeUntilTick<=Tick && !HasRecentWarning(actor) && !HasRecentDoorNoise(actor);
        }
        private bool TryPriorityMeal(int index,ref ActorState actor,int threat)
        {
            if(!NeedsPriorityEnabled || !SafeNeedsAction(actor,threat) || ActorRiding(actor.id) || actor.hunger<HungerRules.SeekAt || actor.carriedFood<=0)return false;
            if(FoodExploration && actor.foodExploreRoomId!=0)CancelFoodExploration(index,ref actor);
            actor.carriedFood--;actor.hunger=System.Math.Max(0,actor.hunger-HungerRules.MealRelief);ClearFoodMemory(ref actor);actors[index]=actor;
            return true; // Keep shipment claims, quantities, destinations and all remaining physical cargo intact.
        }
        private bool SafeNeedsHomeReturn(ActorState actor,int threat)
        {
            // Home is authored location knowledge. Neither home stock nor a food report is consulted to return after work.
            return NeedsPriorityEnabled && actor.assignedHouseId!=0 && OccupationOf(actor)!=HumanOccupation.Consumer && !WorkerClockOn(actor) && !FinishingWorkerLoad(actor) && SafeNeedsAction(actor,threat);
        }
        private bool TryHomeReturnRest(int index,ref ActorState actor)
        {
            if(EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy || IsDoor(actor.x,actor.y) || TerrainRules.RoadSurface(map.Get(actor.x,actor.y)))return false;
            // Short breaks use the existing shopping-break exchange rate, at a safe floor/sidewalk location; no free travel energy.
            actor.energySpent=System.Math.Max(0,actor.energySpent-EnergyRules.RestGain/2);actor.hunger=System.Math.Min(100,actor.hunger+1);actors[index]=actor;return true;
        }
    }
}
