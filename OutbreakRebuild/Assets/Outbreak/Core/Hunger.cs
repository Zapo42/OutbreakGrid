using System; // Validate needs, inventory and bounded sensory memory.
using Outbreak.Contracts; // Save only personal observations and carried food.
namespace Outbreak.Core
{
    public static class HungerRules
    {
        public const int GainTicks=30, SeekAt=40, MealRelief=50, CarryLimit=3, MemoryTicks=150; // One hunger point per three game minutes; three portable meals per human.
        public static int RiskBudget(ActorState a) { return Math.Max(0,Math.Min(80,a.hunger-SeekAt+a.bravery*8)); } // Increasing need raises accepted risk; bravery shifts the threshold.
    }
    public sealed partial class World
    {
        private static void ClearFoodMemory(ref ActorState a){a.foodX=0;a.foodY=0;a.foodUntilTick=0;a.shelfSearchProgress=0;}
        private static void ClearHunger(ref ActorState a){a.knownParks=null;ClearHappiness(ref a);ClearLeisure(ref a);a.additionalFamiliarStores=null;a.familiarStoreId=0;a.shoppingResting=false;a.shoppingEndReason=ShoppingEndReason.None;a.storeMemory=null;a.storeFailures=null;a.shoppingRetryTick=0;ClearClerkAssignment(ref a);ClearCourierAssignment(ref a);ClearBlockedFood(ref a);ClearFoodExploration(ref a);a.foodRoomVisits=null;a.shelfMemory=null;a.storageMemory=null;a.hunger=0;a.carriedFood=0;a.homeShelterId=0;ClearFoodMemory(ref a);}
        private void AdvanceHunger(ref ActorState a)
        {
            if(a.species!=Species.Human)return;if(LeisureEnabled)AdvanceLeisure(ref a);AdvanceStoreMemory(ref a);if(a.shoppingRetryTick!=0 && a.shoppingRetryTick<=Tick)a.shoppingRetryTick=0;if(BoxMemoryEnabled)PruneStorageMemory(ref a);if(RestockingEnabled && a.restockPhase!=RestockPhase.None && (a.turnAtTick!=0 || a.restockUntilTick<=Tick)){if(ShoppingContinuityEnabled && IsShopping(a) && a.restockUntilTick<=Tick)a.shoppingEndReason=ShoppingEndReason.Expired;ClearRestocking(ref a);}if(FoodExploration)AdvanceFoodExploration(ref a);if(a.blockedFoodUntilTick!=0 && a.blockedFoodUntilTick<=Tick)ClearBlockedFood(ref a);if(ShelfMemoryEnabled)PruneShelfMemory(ref a);if(Tick%HungerRules.GainTicks==0)a.hunger=Math.Min(100,a.hunger+1);if(a.foodUntilTick!=0 && a.foodUntilTick<=Tick)ClearFoodMemory(ref a);AdvanceDeprivation(ref a);
        }
        private void ValidateHunger()
        {
            for(int i=0;i<count;i++){ActorState a=actors[i];if(a.shelfSearchProgress<0 || a.shelfSearchProgress>2 || (a.shelfSearchProgress>0 && (!CarefulSearch || a.species!=Species.Human || a.foodUntilTick==0 || Math.Abs(a.x-a.foodX)+Math.Abs(a.y-a.foodY)!=1)))throw new ArgumentException("Invalid shelf search progress.");if(a.hunger<0 || a.hunger>100 || a.carriedFood<0 || a.carriedFood>HungerRules.CarryLimit || ((!HungerEnabled || a.species!=Species.Human) && (a.hunger!=0 || a.carriedFood!=0 || a.foodUntilTick!=0 || a.foodX!=0 || a.foodY!=0 || a.homeShelterId!=0)))throw new ArgumentException("Invalid hunger or inventory.");if(a.homeShelterId!=0 && FindRoom(a.homeShelterId)==null)throw new ArgumentException("Invalid food trip home.");if(a.foodUntilTick==0){if(a.foodX!=0 || a.foodY!=0)throw new ArgumentException("Inactive shelf memory contains coordinates.");}else if(a.foodUntilTick<=Tick || a.foodUntilTick-Tick>HungerRules.MemoryTicks || map.ShelfAt(a.foodX,a.foodY)==null)throw new ArgumentException("Invalid personally observed shelf memory.");}
        }
        private bool ShelfVisible(ActorState observer,ShelfDocument shelf){return ShelfVisible(observer,shelf.x,shelf.y);}
        private bool ShelfVisible(ActorState observer,int shelfX,int shelfY)
        {
            if((observer.x-shelfX)*(observer.x-shelfX)+(observer.y-shelfY)*(observer.y-shelfY)>EncounterRules.SightRadius*EncounterRules.SightRadius)return false;
            for(int d=0;d<8;d+=2){int x=shelfX+dx[d],y=shelfY+dy[d];if(map.CanWalk(x,y) && Visible(observer,new ActorState{x=x,y=y}))return true;}return false; // Observe a surface from an adjacent visible cell without tracing through solid furniture.
        }
        private bool KnownRoomSupplies(ActorState actor,Room room)
        {
            if(actor.carriedFood>0)return true;if(StorageEnabled)foreach(StorageBoxDocument box in map.StorageRecords){int food,space;if((!IsRestrictedSupplyBox(box) || StaffUsesSupply(actor,box)) && room.Contains(Key(box.x,box.y)) && KnownStorageState(actor,box,out food,out space) && food>0)return true;}if(ShelfMemoryEnabled){if(actor.shelfMemory!=null)foreach(ShelfMemoryRecord known in actor.shelfMemory)if(RoutineFoodSiteAllowed(actor,known.x,known.y) && known.food!=0 && !AvoidBlockedFood(actor,known.x,known.y) && room.Contains(Key(known.x,known.y)))return true;return false;}foreach(ShelfDocument s in map.ShelfRecords)if(RoutineFoodSiteAllowed(actor,s.x,s.y) && room.Contains(Key(s.x,s.y)) && (!s.searched || s.food>0) && ShelfVisible(actor,s))return true;return false; // Unsearched shelves are possibilities; contents are revealed only by searching.
        }
        public int FoodTripRisk(int actorIndex) { return PerceivedFoodRisk(actors[actorIndex],NearestVisible(actors[actorIndex],Species.Zombie)); }
        public int FoodEnergyRiskPenalty(int actorIndex)
        {
            ActorState actor=ActorAt(actorIndex);return EnergyFoodRiskPenalty(actor,ObservedFoodRisk(actor,NearestVisible(actor,Species.Zombie)));
        }
        private int EnergyFoodRiskPenalty(ActorState actor,int observedRisk)
        {
            // Low energy raises the cost of observed danger, without inventing danger on a clear route.
            return EnergyRiskEnabled && observedRisk>0 && observedRisk<1000 ? Math.Max(0,(FoodRestRules.DepartureEnergy-EnergyRules.Remaining(actor)+1)/2) : 0;
        }
        private int PerceivedFoodRisk(ActorState actor,int threat)
        {
            int observed=ObservedFoodRisk(actor,threat);return observed+EnergyFoodRiskPenalty(actor,observed);
        }
        private int ObservedFoodRisk(ActorState a,int threat)
        {
            int risk=a.stress+(HasRecentWarning(a)?10:0)+(HasRecentDoorNoise(a)?20:0);if(threat>=0){if(Math.Max(Math.Abs(a.x-actors[threat].x),Math.Abs(a.y-actors[threat].y))<=2)return 1000;risk+=30+ObservePressure(a,actors[threat])*10;}return risk;
        }
        private bool CanRiskFoodTrip(ActorState actor,int threat){return actor.turnAtTick==0 && actor.hunger>=HungerRules.SeekAt && PerceivedFoodRisk(actor,threat)<=HungerRules.RiskBudget(actor);}
        private bool HungryDepartureAllowed(ActorState actor,Room room,int threat){return RestockDepartureAllowed(actor,room,threat) || CanRiskFoodTrip(actor,threat) && !ShouldRestBeforeFood(actor,threat) && !KnownRoomSupplies(actor,room);}
        private bool TryForage(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(CarefulSearch && (actor.turnAtTick!=0 || actor.hunger<HungerRules.SeekAt || actor.carriedFood>0 || !CanRiskFoodTrip(actor,threat))){actor.shelfSearchProgress=0;actors[index]=actor;}if(actor.turnAtTick!=0)return false;
            if(FoodExploration && actor.foodExploreRoomId!=0 && (actor.turnAtTick!=0 || actor.hunger<HungerRules.SeekAt || actor.carriedFood>0 || !CanRiskFoodTrip(actor,threat)))CancelFoodExploration(index,ref actor);
            if(TryPriorityMeal(index,ref actor,threat))return true; // Returning to shelter must not preempt a safe meal already in hand.
            if(FoodTripRetreat && ReturnFromFoodDanger(index,ref actor,threat))return false;
            if(actor.carriedFood>0 && actor.homeShelterId!=0 && actor.hunger<HungerRules.SeekAt && actor.shelterId==0){Room home=FindRoom(actor.homeShelterId);if(home!=null && !KnownRoomDanger(actor,home) && RoomAdmissions(home,actor.id)<home.Capacity){int goal=PickShelterCell(home,actor);if(goal>=0){actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;}}else{actor.homeShelterId=0;actor.reaction=HumanReaction.Flight;}actors[index]=actor;return false;}
            if(actor.hunger<HungerRules.SeekAt)return false;
            if(threat>=0 && Math.Max(Math.Abs(actor.x-actors[threat].x),Math.Abs(actor.y-actors[threat].y))<=2)return false;
            if(actor.carriedFood>0){actor.carriedFood--;actor.hunger=Math.Max(0,actor.hunger-HungerRules.MealRelief);ClearFoodMemory(ref actor);actors[index]=actor;return true;} // Eating consumes one ordinary action and one carried meal.
            if(ShouldRestBeforeFood(actor,threat)){actor.shelfSearchProgress=0;ClearShelterDeparture(ref actor);actors[index]=actor;return false;}
            if(!CanRiskFoodTrip(actor,threat))return false;
            Room room=FindRoom(actor.shelterId);bool inside=room!=null && room.Contains(Key(actor.x,actor.y));if(!inside && actor.shelterExitPhase!=ShelterExitPhase.None)return false;
            if(StorageEnabled && TryUseStorage(index,ref actor,threat,inside?room:null,false,out direction))return true;
            return CollectShelfFood(index,ref actor,threat,room,out direction);
        }
        private bool CollectShelfFood(int index,ref ActorState actor,int threat,Room room,out int direction,bool restock=false)
        {
            direction=8;bool inside=room!=null && room.Contains(Key(actor.x,actor.y));
            ShelfDocument closest=null;int best=int.MaxValue;if(restock)closest=ChooseRestockShelf(actor,FindRoom(actor.restockHomeId));else if(ShelfMemoryEnabled)closest=ChooseRememberedShelf(actor,inside?room:null);else foreach(ShelfDocument s in map.ShelfRecords){if(!RoutineFoodSiteAllowed(actor,s.x,s.y) || (s.searched && s.food==0) || !ShelfVisible(actor,s) || (inside && !room.Contains(Key(s.x,s.y))))continue;int distance=DistanceSquared(actor,new ActorState{x=s.x,y=s.y});if(distance<best || (distance==best && Key(s.x,s.y)<Key(closest.x,closest.y))){closest=s;best=distance;}}
            if(FoodExploration)
            {
                if(closest==null){ClearFoodMemory(ref actor);if(!restock)ExploreFoodRoom(index,ref actor,threat,inside?room:null);actors[index]=actor;return false;}
                if(actor.foodExploreRoomId!=0 && inside){RememberFoodRoom(ref actor,room.id);ClearFoodExploration(ref actor);}
            }
            if(ShelfMemoryEnabled && closest==null)ClearFoodMemory(ref actor);if(closest!=null){if(actor.foodX!=closest.x || actor.foodY!=closest.y)actor.shelfSearchProgress=0;actor.foodX=closest.x;actor.foodY=closest.y;actor.foodUntilTick=Tick+HungerRules.MemoryTicks;}
            if(actor.foodUntilTick==0 || (inside && !room.Contains(Key(actor.foodX,actor.foodY)))){actors[index]=actor;return false;}
            ShelfDocument target=map.ShelfAt(actor.foodX,actor.foodY);if(Math.Abs(actor.x-target.x)+Math.Abs(actor.y-target.y)==1){if(CarefulSearch && !(QuickStockPickup && target.searched) && actor.shelfSearchProgress<2){actor.shelfSearchProgress++;actors[index]=actor;EmitSearchNoise(actor);return true;}int take=FoodPickupAmount(actor,target);actor.carriedFood+=take;map.SetShelf(target.x,target.y,target.food-take,true);if(ShelfMemoryEnabled)RememberShelf(ref actor,new ShelfMemoryRecord{x=target.x,y=target.y,food=target.food-take,observedTick=Tick});ClearFoodMemory(ref actor);actors[index]=actor;if(SearchNoise)EmitSearchNoise(actor);return true;} // Searching costs an action even when the shelf is empty; unopened contents never drive selection.
            actor.shelfSearchProgress=0;
            int threats=KnownShelterThreats(actor,threat),length,steps=int.MaxValue;for(int d=0;d<8;d+=2){int x=target.x+dx[d],y=target.y+dy[d];if(!map.CanWalk(x,y))continue;int candidate=ShelterRoute(actor,x,y,threats,out length,foodRoute:true);if(candidate>=0 && length<steps){steps=length;direction=candidate;}}
            if(ShelfMemoryEnabled && steps==int.MaxValue && Math.Max(Math.Abs(target.x-actor.x),Math.Abs(target.y-actor.y))>15)direction=RouteToRememberedShelf(actor,target,threats);if(direction==8){if(FoodRouteMemory){actor.blockedFoodX=target.x;actor.blockedFoodY=target.y;actor.blockedFoodUntilTick=Tick+FoodRouteMemoryRules.RetryTicks;ClearFoodMemory(ref actor);actors[index]=actor;return true;}actors[index]=actor;return false;}
            if(!inside && actor.shelterId!=0 && !(FoodExploration && actor.foodExploreRoomId==actor.shelterId && room!=null && room.Contains(Key(target.x,target.y))))ClearShelter(ref actor);int nx=actor.x+dx[direction],ny=actor.y+dy[direction];if(map.Get(nx,ny)==Terrain.DoorClosed){SetDoor(nx,ny,true);direction=8;}actors[index]=actor;return true;
        }
    }
}
