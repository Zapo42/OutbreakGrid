using System; // Validate saved observations and isolate mutable array ownership.
using Outbreak.Contracts; // Persist only what each human personally observed.
namespace Outbreak.Core
{
    public static class ShelfMemoryRules
    {
        public const int Limit = 8, RememberTicks = 3600; // Eight locations, retained for six game hours unless observed again.
    }
    public sealed partial class World
    {
        private static ActorState DetachActor(ActorState actor)
        {
            if(actor.foodRoomVisits!=null){if(actor.foodRoomVisits.Length>FoodExplorationRules.HistoryLimit)throw new ArgumentException("Food room history exceeds its limit.");actor.foodRoomVisits=(FoodRoomVisit[])actor.foodRoomVisits.Clone();}
            if (actor.shelfMemory != null) { if (actor.shelfMemory.Length > ShelfMemoryRules.Limit) throw new ArgumentException("Shelf memory exceeds its limit."); actor.shelfMemory = (ShelfMemoryRecord[])actor.shelfMemory.Clone(); } return actor; // ActorAt, save capture and restore never retain caller-owned arrays.
        }
        private void PruneShelfMemory(ref ActorState actor)
        {
            if (actor.shelfMemory == null) return; int keep = 0; foreach (ShelfMemoryRecord note in actor.shelfMemory) if (Tick - note.observedTick < ShelfMemoryRules.RememberTicks) keep++;
            if (keep == actor.shelfMemory.Length) return; if (keep == 0) { actor.shelfMemory = null; return; } ShelfMemoryRecord[] retained = new ShelfMemoryRecord[keep]; int index = 0; foreach (ShelfMemoryRecord note in actor.shelfMemory) if (Tick - note.observedTick < ShelfMemoryRules.RememberTicks) retained[index++] = note; actor.shelfMemory = retained;
        }
        private int CompareShelfNotes(ShelfMemoryRecord a, ShelfMemoryRecord b) { return Key(a.x,a.y).CompareTo(Key(b.x,b.y)); }
        private bool LessUsefulShelf(ActorState actor, ShelfMemoryRecord a, ShelfMemoryRecord b)
        {
            if (a.observedTick != b.observedTick) return a.observedTick < b.observedTick; int da = DistanceSquared(actor,new ActorState{x=a.x,y=a.y}), db = DistanceSquared(actor,new ActorState{x=b.x,y=b.y}); return da != db ? da > db : Key(a.x,a.y) > Key(b.x,b.y); // Older first; current observations retain nearer shelves and stable coordinate ties.
        }
        private void RememberShelf(ref ActorState actor, ShelfMemoryRecord observation)
        {
            ShelfMemoryRecord[] notes = actor.shelfMemory;
            if (notes == null) { actor.shelfMemory = new[] { observation }; return; }
            for (int i = 0; i < notes.Length; i++) if (notes[i].x == observation.x && notes[i].y == observation.y) { notes[i] = observation; return; }
            if (notes.Length < ShelfMemoryRules.Limit) { Array.Resize(ref notes, notes.Length + 1); notes[notes.Length-1] = observation; }
            else { int worst = 0; for (int i=1;i<notes.Length;i++) if (LessUsefulShelf(actor,notes[i],notes[worst])) worst=i; if (!LessUsefulShelf(actor,notes[worst],observation)) return; notes[worst]=observation; }
            Array.Sort(notes, CompareShelfNotes); actor.shelfMemory = notes;
        }
        private void ObserveShelves(ref ActorState actor)
        {
            foreach (ShelfDocument shelf in map.ShelfRecords) if (ShelfVisible(actor,shelf)) RememberShelf(ref actor,new ShelfMemoryRecord{x=shelf.x,y=shelf.y,food=shelf.searched?shelf.food:-1,observedTick=Tick}); // Sight reveals only exposed contents; unopened shelves remain unknown.
        }
        private ShelfDocument ChooseRememberedShelf(ActorState actor, Room insideRoom)
        {
            if (actor.shelfMemory == null) return null;
            if(SearchCoordination && actor.foodUntilTick>Tick)foreach(ShelfMemoryRecord current in actor.shelfMemory)if(current.x==actor.foodX && current.y==actor.foodY && current.food!=0 && !AvoidBlockedFood(actor,current.x,current.y) && (insideRoom==null || insideRoom.Contains(Key(current.x,current.y))) && (actor.shelfSearchProgress>0 || (!VisibleShelfSearch(actor,current.x,current.y) && ReachableSearchAlternative(actor,current))))return new ShelfDocument{x=current.x,y=current.y}; // Keep a reachable chosen task during travel and search instead of switching when another shelf becomes free.
            ShelfMemoryRecord best = new ShelfMemoryRecord(); int rank = -1, distance = int.MaxValue;bool bestReachable=false;
            foreach(ShelfMemoryRecord note in actor.shelfMemory)
            {
                if(note.food==0 || AvoidBlockedFood(actor,note.x,note.y) || (insideRoom!=null && !insideRoom.Contains(Key(note.x,note.y))))continue;
                int priority=note.food>0?1:0,metric=DistanceSquared(actor,new ActorState{x=note.x,y=note.y});bool reachable=false;
                if(FoodRouteChoice){int route=FoodShelfRouteLength(actor,note);reachable=route<int.MaxValue;if(reachable)metric=route;}
                if(rank<0 || (reachable && !bestReachable) || (reachable==bestReachable && (priority>rank || (priority==rank && BetterFoodYield(actor,note,metric,best,distance,reachable))))){best=note;rank=priority;distance=metric;bestReachable=reachable;}
            }
            if(SearchCoordination && rank>=0 && VisibleShelfSearch(actor,best.x,best.y))
            {
                ShelfMemoryRecord alternative=new ShelfMemoryRecord();int alternativeRank=-1,alternativeDistance=int.MaxValue;
                foreach(ShelfMemoryRecord note in actor.shelfMemory)
                {
                    if(note.food==0 || AvoidBlockedFood(actor,note.x,note.y) || (insideRoom!=null && !insideRoom.Contains(Key(note.x,note.y))) || VisibleShelfSearch(actor,note.x,note.y) || (!FoodRouteChoice && !ReachableSearchAlternative(actor,note)))continue;
                    int priority=note.food>0?1:0,squared=FoodRouteChoice?FoodShelfRouteLength(actor,note):DistanceSquared(actor,new ActorState{x=note.x,y=note.y});if(squared==int.MaxValue)continue;
                    if(priority>alternativeRank || (priority==alternativeRank && BetterFoodYield(actor,note,squared,alternative,alternativeDistance,FoodRouteChoice))){alternative=note;alternativeRank=priority;alternativeDistance=squared;}
                }
                if(alternativeRank>=0)best=alternative; // With no usable alternative, retain the original choice rather than blocking access to the only supplies.
            }
            return rank<0 ? null : new ShelfDocument{x=best.x,y=best.y}; // Prefer confirmed supplies over unknown shelves; never read hidden current stock.
        }
        private bool BetterFoodYield(ActorState actor,ShelfMemoryRecord candidate,int route,ShelfMemoryRecord current,int currentRoute,bool reachable)
        {
            if(!FoodYieldChoice || !reachable || candidate.food<=0 || current.food<=0)return route<currentRoute;
            if((route==0)!=(currentRoute==0))return route==0; // Take an available adjacent meal before making a supply detour.
            int space=HungerRules.CarryLimit-actor.carriedFood;
            int value=route*3-2*(Math.Min(candidate.food,space)-1),currentValue=currentRoute*3-2*(Math.Min(current.food,space)-1);
            return value<currentValue || (value==currentValue && route<currentRoute); // At most one extra walking step for two additional portable meals; stable coordinate ties.
        }
        private int FoodShelfRouteLength(ActorState actor,ShelfMemoryRecord note)
        {
            int threats=KnownShelterThreats(actor,NearestVisible(actor,Species.Zombie)),best=int.MaxValue;
            for(int d=0;d<8;d+=2){int x=note.x+dx[d],y=note.y+dy[d],length;if(map.CanWalk(x,y) && ShelterRoute(actor,x,y,threats,out length,foodRoute:true)>=0 && length<best)best=length;}
            return best; // Compare bounded routes to a search position, without reading unopened stock.
        }
        private bool VisibleShelfSearch(ActorState observer,int x,int y)
        {
            for(int i=0;i<count;i++){ActorState other=actors[i];if(other.id!=observer.id && other.species==Species.Human && other.turnAtTick==0 && other.shelfSearchProgress>0 && other.foodUntilTick>Tick && other.foodX==x && other.foodY==y && DistanceSquared(observer,other)<=EncounterRules.SightRadius*EncounterRules.SightRadius && Visible(observer,other))return true;}
            return false; // Visible active work is the only coordination signal; hidden humans never reserve shelves globally.
        }
        private bool ReachableSearchAlternative(ActorState actor,ShelfMemoryRecord note)
        {
            int threats=KnownShelterThreats(actor,NearestVisible(actor,Species.Zombie));
            for(int d=0;d<8;d+=2){int x=note.x+dx[d],y=note.y+dy[d],length;if(map.CanWalk(x,y) && ShelterRoute(actor,x,y,threats,out length,foodRoute:true)>=0)return true;}
            return false; // Prefer alternatives with a complete local route; do not abandon reachable food for an inaccessible or distant remembered shelf.
        }
        private int RouteToRememberedShelf(ActorState actor, ShelfDocument target, int threats)
        {
            int x=actor.x+Math.Max(-12,Math.Min(12,target.x-actor.x)), y=actor.y+Math.Max(-12,Math.Min(12,target.y-actor.y)), best=DistanceSquared(actor,new ActorState{x=target.x,y=target.y}), direction=8;
            for (int d=0;d<8;d+=2) { int wx=x+dx[d],wy=y+dy[d];if(!ShelterWalkable(wx,wy))continue;int distance=DistanceSquared(new ActorState{x=wx,y=wy},new ActorState{x=target.x,y=target.y});if(distance>=best)continue;int length, candidate=ShelterRoute(actor,wx,wy,threats,out length,foodRoute:true);if(candidate>=0){best=distance;direction=candidate;} } return direction; // Bounded local waypoints allow a longer remembered trip without a world-sized path query.
        }
        private void ValidateShelfMemory()
        {
            for (int i=0;i<count;i++) { ActorState actor=actors[i]; if(actor.shelfMemory==null)continue;if(!ShelfMemoryEnabled || actor.species!=Species.Human || actor.shelfMemory.Length==0 || actor.shelfMemory.Length>ShelfMemoryRules.Limit)throw new ArgumentException("Unsupported shelf memory.");int previous=-1;foreach(ShelfMemoryRecord note in actor.shelfMemory){if(map.ShelfAt(note.x,note.y)==null || note.food < -1 || note.food>1000 || note.reportedBy<0 || (note.reportedBy>0 && (!ShelfReports || note.reportedBy==actor.id || note.reportedBy>=nextId)) || note.observedTick<0 || note.observedTick>Tick || Tick-note.observedTick>=ShelfMemoryRules.RememberTicks || Key(note.x,note.y)<=previous)throw new ArgumentException("Invalid personal shelf observation.");previous=Key(note.x,note.y);} }
        }
    }
}
