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
        private static void ClearHunger(ref ActorState a){ClearBlockedFood(ref a);a.shelfMemory=null;a.hunger=0;a.carriedFood=0;a.homeShelterId=0;ClearFoodMemory(ref a);}
        private void AdvanceHunger(ref ActorState a)
        {
            if(a.species!=Species.Human)return;if(a.blockedFoodUntilTick!=0 && a.blockedFoodUntilTick<=Tick)ClearBlockedFood(ref a);if(ShelfMemoryEnabled)PruneShelfMemory(ref a);if(Tick%HungerRules.GainTicks==0)a.hunger=Math.Min(100,a.hunger+1);if(a.foodUntilTick!=0 && a.foodUntilTick<=Tick)ClearFoodMemory(ref a);
        }
        private void ValidateHunger()
        {
            for(int i=0;i<count;i++){ActorState a=actors[i];if(a.shelfSearchProgress<0 || a.shelfSearchProgress>2 || (a.shelfSearchProgress>0 && (!CarefulSearch || a.species!=Species.Human || a.foodUntilTick==0 || Math.Abs(a.x-a.foodX)+Math.Abs(a.y-a.foodY)!=1)))throw new ArgumentException("Invalid shelf search progress.");if(a.hunger<0 || a.hunger>100 || a.carriedFood<0 || a.carriedFood>HungerRules.CarryLimit || ((!HungerEnabled || a.species!=Species.Human) && (a.hunger!=0 || a.carriedFood!=0 || a.foodUntilTick!=0 || a.foodX!=0 || a.foodY!=0 || a.homeShelterId!=0)))throw new ArgumentException("Invalid hunger or inventory.");if(a.homeShelterId!=0 && FindRoom(a.homeShelterId)==null)throw new ArgumentException("Invalid food trip home.");if(a.foodUntilTick==0){if(a.foodX!=0 || a.foodY!=0)throw new ArgumentException("Inactive shelf memory contains coordinates.");}else if(a.foodUntilTick<=Tick || a.foodUntilTick-Tick>HungerRules.MemoryTicks || map.ShelfAt(a.foodX,a.foodY)==null)throw new ArgumentException("Invalid personally observed shelf memory.");}
        }
        private bool ShelfVisible(ActorState observer,ShelfDocument shelf)
        {
            if(DistanceSquared(observer,new ActorState{x=shelf.x,y=shelf.y})>EncounterRules.SightRadius*EncounterRules.SightRadius)return false;
            for(int d=0;d<8;d+=2){int x=shelf.x+dx[d],y=shelf.y+dy[d];if(map.CanWalk(x,y) && Visible(observer,new ActorState{x=x,y=y}))return true;}return false; // Observe a surface from an adjacent visible cell without tracing through solid furniture.
        }
        private bool KnownRoomSupplies(ActorState actor,Room room)
        {
            if(actor.carriedFood>0)return true;if(ShelfMemoryEnabled){if(actor.shelfMemory!=null)foreach(ShelfMemoryRecord known in actor.shelfMemory)if(known.food!=0 && !AvoidBlockedFood(actor,known.x,known.y) && room.Contains(Key(known.x,known.y)))return true;return false;}foreach(ShelfDocument s in map.ShelfRecords)if(room.Contains(Key(s.x,s.y)) && (!s.searched || s.food>0) && ShelfVisible(actor,s))return true;return false; // Unsearched shelves are possibilities; contents are revealed only by searching.
        }
        public int FoodTripRisk(int actorIndex) { return PerceivedFoodRisk(actors[actorIndex],NearestVisible(actors[actorIndex],Species.Zombie)); }
        private int PerceivedFoodRisk(ActorState a,int threat)
        {
            int risk=a.stress+(HasRecentWarning(a)?10:0)+(HasRecentDoorNoise(a)?20:0);if(threat>=0){if(Math.Max(Math.Abs(a.x-actors[threat].x),Math.Abs(a.y-actors[threat].y))<=2)return 1000;risk+=30+ObservePressure(a,actors[threat])*10;}return risk;
        }
        private bool CanRiskFoodTrip(ActorState actor,int threat){return actor.turnAtTick==0 && actor.hunger>=HungerRules.SeekAt && PerceivedFoodRisk(actor,threat)<=HungerRules.RiskBudget(actor);}
        private bool HungryDepartureAllowed(ActorState actor,Room room,int threat){return CanRiskFoodTrip(actor,threat) && !KnownRoomSupplies(actor,room);}
        private bool TryForage(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(CarefulSearch && (actor.turnAtTick!=0 || actor.hunger<HungerRules.SeekAt || actor.carriedFood>0 || !CanRiskFoodTrip(actor,threat))){actor.shelfSearchProgress=0;actors[index]=actor;}if(actor.turnAtTick!=0)return false;
            if(actor.carriedFood>0 && actor.homeShelterId!=0 && actor.hunger<HungerRules.SeekAt && actor.shelterId==0){Room home=FindRoom(actor.homeShelterId);if(home!=null && !KnownRoomDanger(actor,home) && RoomAdmissions(home,actor.id)<home.Capacity){int goal=PickShelterCell(home,actor);if(goal>=0){actor.shelterId=home.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;}}else{actor.homeShelterId=0;actor.reaction=HumanReaction.Flight;}actors[index]=actor;return false;}
            if(actor.hunger<HungerRules.SeekAt)return false;
            if(threat>=0 && Math.Max(Math.Abs(actor.x-actors[threat].x),Math.Abs(actor.y-actors[threat].y))<=2)return false;
            if(actor.carriedFood>0){actor.carriedFood--;actor.hunger=Math.Max(0,actor.hunger-HungerRules.MealRelief);ClearFoodMemory(ref actor);actors[index]=actor;return true;} // Eating consumes one ordinary action and one carried meal.
            if(!CanRiskFoodTrip(actor,threat))return false;
            Room room=FindRoom(actor.shelterId);bool inside=room!=null && room.Contains(Key(actor.x,actor.y));if(!inside && actor.shelterExitPhase!=ShelterExitPhase.None)return false;
            ShelfDocument closest=null;int best=int.MaxValue;if(ShelfMemoryEnabled)closest=ChooseRememberedShelf(actor,inside?room:null);else foreach(ShelfDocument s in map.ShelfRecords){if((s.searched && s.food==0) || !ShelfVisible(actor,s) || (inside && !room.Contains(Key(s.x,s.y))))continue;int distance=DistanceSquared(actor,new ActorState{x=s.x,y=s.y});if(distance<best || (distance==best && Key(s.x,s.y)<Key(closest.x,closest.y))){closest=s;best=distance;}}
            if(ShelfMemoryEnabled && closest==null)ClearFoodMemory(ref actor);if(closest!=null){if(actor.foodX!=closest.x || actor.foodY!=closest.y)actor.shelfSearchProgress=0;actor.foodX=closest.x;actor.foodY=closest.y;actor.foodUntilTick=Tick+HungerRules.MemoryTicks;}
            if(actor.foodUntilTick==0 || (inside && !room.Contains(Key(actor.foodX,actor.foodY)))){actors[index]=actor;return false;}
            ShelfDocument target=map.ShelfAt(actor.foodX,actor.foodY);if(Math.Abs(actor.x-target.x)+Math.Abs(actor.y-target.y)==1){if(CarefulSearch && !(QuickStockPickup && target.searched) && actor.shelfSearchProgress<2){actor.shelfSearchProgress++;actors[index]=actor;EmitSearchNoise(actor);return true;}int take=FoodPickupAmount(actor,target);actor.carriedFood+=take;map.SetShelf(target.x,target.y,target.food-take,true);if(ShelfMemoryEnabled)RememberShelf(ref actor,new ShelfMemoryRecord{x=target.x,y=target.y,food=target.food-take,observedTick=Tick});ClearFoodMemory(ref actor);actors[index]=actor;if(SearchNoise)EmitSearchNoise(actor);return true;} // Searching costs an action even when the shelf is empty; unopened contents never drive selection.
            actor.shelfSearchProgress=0;
            int threats=KnownShelterThreats(actor,threat),length,steps=int.MaxValue;for(int d=0;d<8;d+=2){int x=target.x+dx[d],y=target.y+dy[d];if(!map.CanWalk(x,y))continue;int candidate=ShelterRoute(actor,x,y,threats,out length);if(candidate>=0 && length<steps){steps=length;direction=candidate;}}
            if(ShelfMemoryEnabled && steps==int.MaxValue && Math.Max(Math.Abs(target.x-actor.x),Math.Abs(target.y-actor.y))>15)direction=RouteToRememberedShelf(actor,target,threats);if(direction==8){if(FoodRouteMemory){actor.blockedFoodX=target.x;actor.blockedFoodY=target.y;actor.blockedFoodUntilTick=Tick+FoodRouteMemoryRules.RetryTicks;ClearFoodMemory(ref actor);actors[index]=actor;return true;}actors[index]=actor;return false;}
            if(!inside && actor.shelterId!=0)ClearShelter(ref actor);int nx=actor.x+dx[direction],ny=actor.y+dy[direction];if(map.Get(nx,ny)==Terrain.DoorClosed){SetDoor(nx,ny,true);direction=8;}actors[index]=actor;return true;
        }
    }
}
