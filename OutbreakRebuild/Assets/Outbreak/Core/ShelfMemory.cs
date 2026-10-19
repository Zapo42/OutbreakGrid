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
            if (actor.shelfMemory == null) return null; ShelfMemoryRecord best = new ShelfMemoryRecord(); int rank = -1, distance = int.MaxValue;
            foreach (ShelfMemoryRecord note in actor.shelfMemory) { if (note.food==0 || (insideRoom!=null && !insideRoom.Contains(Key(note.x,note.y)))) continue; int priority = note.food>0?1:0, squared=DistanceSquared(actor,new ActorState{x=note.x,y=note.y}); if (priority>rank || (priority==rank && squared<distance)) { best=note;rank=priority;distance=squared; } }
            return rank<0 ? null : new ShelfDocument{x=best.x,y=best.y}; // Prefer confirmed supplies over unknown shelves; never read hidden current stock.
        }
        private int RouteToRememberedShelf(ActorState actor, ShelfDocument target, int threats)
        {
            int x=actor.x+Math.Max(-12,Math.Min(12,target.x-actor.x)), y=actor.y+Math.Max(-12,Math.Min(12,target.y-actor.y)), best=DistanceSquared(actor,new ActorState{x=target.x,y=target.y}), direction=8;
            for (int d=0;d<8;d+=2) { int wx=x+dx[d],wy=y+dy[d];if(!ShelterWalkable(wx,wy))continue;int distance=DistanceSquared(new ActorState{x=wx,y=wy},new ActorState{x=target.x,y=target.y});if(distance>=best)continue;int length, candidate=ShelterRoute(actor,wx,wy,threats,out length);if(candidate>=0){best=distance;direction=candidate;} } return direction; // Bounded local waypoints allow a longer remembered trip without a world-sized path query.
        }
        private void ValidateShelfMemory()
        {
            for (int i=0;i<count;i++) { ActorState actor=actors[i]; if(actor.shelfMemory==null)continue;if(!ShelfMemoryEnabled || actor.species!=Species.Human || actor.shelfMemory.Length==0 || actor.shelfMemory.Length>ShelfMemoryRules.Limit)throw new ArgumentException("Unsupported shelf memory.");int previous=-1;foreach(ShelfMemoryRecord note in actor.shelfMemory){if(map.ShelfAt(note.x,note.y)==null || note.food < -1 || note.food>1000 || note.observedTick<0 || note.observedTick>Tick || Tick-note.observedTick>=ShelfMemoryRules.RememberTicks || Key(note.x,note.y)<=previous)throw new ArgumentException("Invalid personal shelf observation.");previous=Key(note.x,note.y);} }
        }
    }
}
