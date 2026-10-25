using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        // Authored familiarity supplies a location, never a remote inventory observation.
        public int TryFamiliarizeResidentsWithShops()
        {
            if(!ShoppingContinuityEnabled)return 0;int updated=0;
            for(int i=0;i<count;i++){var actor=actors[i];Room home=FindRoom(actor.assignedHouseId);if(actor.species!=Species.Human || actor.health<=0 || home==null || actor.familiarStoreId!=0)continue;int nearest=0,best=int.MaxValue,origin=home.cells[0];
                foreach(int id in stores.Keys){Room shop=FindRoom(id);foreach(int door in shop.doors){int distance=Math.Abs(door%Width-origin%Width)+Math.Abs(door/Width-origin/Width);if(distance<best){best=distance;nearest=id;}}}
                if(nearest!=0){actor.familiarStoreId=nearest;actors[i]=actor;updated++;}
            }return updated;
        }
        private bool TryShoppingRest(int index,ref ActorState actor)
        {
            if(actor.shoppingStoreId==0 || HouseAt(actor.x,actor.y)==actor.assignedHouseId || IsDoor(actor.x,actor.y) || TerrainRules.RoadSurface(map.Get(actor.x,actor.y)))return false;
            if(EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy)actor.shoppingResting=true;
            if(!actor.shoppingResting)return false;
            if(EnergyRules.Remaining(actor)>=RestockRules.DepartureEnergy){actor.shoppingResting=false;actors[index]=actor;return false;}
            actor.energySpent=Math.Max(0,actor.energySpent-EnergyRules.RestGain/2);actor.hunger=Math.Min(100,actor.hunger+1);actors[index]=actor;return true;
        }
        public string ShoppingStatus(int index)
        {
            var actor=ActorAt(index);if(IsShopping(actor)){if(actor.shoppingResting)return "Resting during shopping trip; destination retained";if(actor.restockPhase==RestockPhase.Returning)return actor.shoppingEndReason==ShoppingEndReason.None?"Returning supplies home":"Returning home: "+ShoppingReason(actor.shoppingEndReason);return actor.shoppingShelf!=0?"Collecting from store shelf":actor.shoppingStoreId!=0?"Travelling to known store #"+actor.shoppingStoreId:"Looking for a known store";}
            return actor.shoppingEndReason==ShoppingEndReason.None?"No active shopping trip":ShoppingReason(actor.shoppingEndReason);
        }
        private static string ShoppingReason(ShoppingEndReason reason){switch(reason){case ShoppingEndReason.NoKnownStore:return "No reachable known store";case ShoppingEndReason.NoRoute:return "Route blocked or unavailable";case ShoppingEndReason.EmptyStores:return "Known stores empty or unavailable";case ShoppingEndReason.Danger:return "Interrupted by danger or unsafe condition";case ShoppingEndReason.Hunger:return "Interrupted by hunger without a personal meal";case ShoppingEndReason.TargetSatisfied:return "Home reserve already satisfied";case ShoppingEndReason.Expired:return "Shopping task timed out";default:return "Shopping";}}
        private void ValidateShoppingContinuity()
        {
            for(int i=0;i<count;i++){var actor=actors[i];bool data=actor.familiarStoreId!=0 || actor.shoppingResting || actor.shoppingEndReason!=ShoppingEndReason.None;
                if((data && (!ShoppingContinuityEnabled || actor.species!=Species.Human || actor.health<=0)) || actor.familiarStoreId<0 || (actor.familiarStoreId!=0 && !stores.ContainsKey(actor.familiarStoreId)) || (actor.shoppingResting && !IsShopping(actor)) || actor.shoppingEndReason<ShoppingEndReason.None || actor.shoppingEndReason>ShoppingEndReason.Expired)throw new ArgumentException("Invalid shopping continuity state.");
            }
        }
    }
}
