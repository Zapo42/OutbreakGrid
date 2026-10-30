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
        private bool ReleaseUnusableCourierClaim(int index,ref ActorState actor,int threat)
        {
            if(!NeedsPriorityEnabled || actor.courierOrderId==0 || actor.carriedFood>FoodReserve(actor))return false;
            // An exhausted/hungry failed pickup and a return with only personal reserve have nothing to deliver.
            // Release ownership, never credit or delete the outstanding request, and keep every physical meal.
            if(actor.restockPhase!=RestockPhase.Returning && (actor.restockPhase!=RestockPhase.Collecting || SafeRestock(actor,threat)))return false;
            ClearRestocking(ref actor);actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        private bool CourierPickupReady(ActorState actor,StorageBoxDocument source)
        {
            if(!NeedsPriorityEnabled || actor.carriedFood>0)return true;
            // The authored source location supplies a conservative approach-time hunger check, never live stock knowledge.
            int steps=System.Math.Max(0,System.Math.Abs(actor.x-source.x)+System.Math.Abs(actor.y-source.y)-1);
            int hungerUntilPickup=(steps*EncounterRules.MoveInterval+HungerRules.GainTicks-1)/HungerRules.GainTicks;
            return actor.hunger+hungerUntilPickup<HungerRules.SeekAt;
        }
        private bool TryCourierRouteRest(int index,ref ActorState actor,int threat)
        {
            // Reuse the established paid recovery exchange before exhausted walking consumes the delivery load.
            // No road/doorway stops, vehicle state changes, extra inventory or knowledge are introduced.
            if(!NeedsPriorityEnabled || actor.distributionCenterId==0 || ActorRiding(actor.id) || !SafeNeedsAction(actor,threat))return false;
            int remaining=EnergyRules.Remaining(actor),distance=int.MaxValue;
            if(actor.courierOrderId!=0 && actor.restockPhase==RestockPhase.Returning)
            {
                int goal=actor.restockBox-1;distance=System.Math.Max(0,System.Math.Abs(actor.x-goal%Width)+System.Math.Abs(actor.y-goal/Width)-1);
            }
            else
            {
                Room destination=FindRoom(actor.workerCommuteId);if(destination==null)return false;
                foreach(int cell in destination.cells)distance=System.Math.Min(distance,System.Math.Abs(actor.x-cell%Width)+System.Math.Abs(actor.y-cell/Width));
            }
            // Nearby unloading/arrival wins over a break. Reassess this lower-bound route cost after each paid step.
            return remaining<distance && TryHomeReturnRest(index,ref actor);
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
