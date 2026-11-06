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
        private bool HomeFoodWaitAllowed(ActorState actor)
        {
            if(!HomeShoppingCompletionEnabled || actor.restockPhase!=RestockPhase.None || actor.hunger<FoodRestRules.UrgentHunger(actor) || actor.carriedFood!=0 || actor.shoppingRetryTick<=Tick || actor.assignedHouseId==0 || actor.shelterId!=actor.assignedHouseId || HouseAt(actor.x,actor.y)!=actor.assignedHouseId || KnownShoppingShelf(actor).HasValue)return false;
            int boxes=0;Room home=FindRoom(actor.assignedHouseId);
            foreach(var box in map.StorageRecords){if(!home.Contains(Key(box.x,box.y)) || IsRestrictedSupplyBox(box))continue;boxes++;var note=HomeNote(actor,box);if(!HomeBoxChecked(actor,box) || !note.HasValue || note.Value.food!=0)return false;}
            foreach(var shelf in map.ShelfRecords)if(home.Contains(Key(shelf.x,shelf.y)))return false; // Unsearched home shelves still deserve investigation.
            if(actor.storageMemory!=null)foreach(var note in actor.storageMemory)if(note.food>0 && RoutineFoodSiteAllowed(actor,note.x,note.y))return false;
            return boxes>0; // Wait only on fresh personal empty-pantry observations, never unseen inventory.
        }
        private bool TryHomeFoodWait(int index,ref ActorState actor,int threat)
        {
            if(!HomeFoodWaitAllowed(actor) || !SafeNeedsAction(actor,threat) || IsDoor(actor.x,actor.y) || TerrainRules.RoadSurface(map.Get(actor.x,actor.y)))return false;
            // An existing failed-shop cooldown is a wait, not another sweep of a personally checked empty home.
            ClearFoodExploration(ref actor);ClearShelterDeparture(ref actor);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;actors[index]=actor;
            Room home=FindRoom(actor.assignedHouseId);
            if(EnergyRules.Remaining(actor)<FoodRestRules.DepartureEnergy && actor.nextShoveTick==0 && actor.nextMeleeTick==0 && TryRestInShelter(index,ref actor,home,threat))return true;
            CloseNearbyDoors(actor,home);return true;
        }
        // Use only the human's shelter and personal supply knowledge, never hidden stocks.
        private bool ShouldRestBeforeFood(ActorState actor,int threat)
        {
            if(!FoodRestEnabled || actor.species!=Species.Human || actor.turnAtTick!=0 || actor.health<=0 || actor.stress!=0 || threat>=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || actor.carriedFood>0 || actor.hunger<HungerRules.SeekAt || (actor.hunger>=FoodRestRules.UrgentHunger(actor) && !HomeFoodWaitAllowed(actor)) || EnergyRules.Remaining(actor)>=FoodRestRules.DepartureEnergy || actor.shelterExitPhase!=ShelterExitPhase.None || actor.foodExploreRoomId!=0)return false;
            Room room=FindRoom(actor.shelterId);
            return room!=null && room.Contains(Key(actor.x,actor.y)) && !KnownRoomSupplies(actor,room);
        }
        public bool PreparingFoodTrip(int index){ActorState actor=ActorAt(index);return ShouldRestBeforeFood(actor,NearestVisible(actor,Species.Zombie));}
    }
}
