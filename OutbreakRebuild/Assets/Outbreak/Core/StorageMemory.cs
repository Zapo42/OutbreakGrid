using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StorageMemoryRules
    {
        public const int Limit=8,RememberTicks=3600;
    }
    public sealed partial class World
    {
        private StorageMemoryRecord StorageObservation(StorageBoxDocument box)
        {
            return new StorageMemoryRecord{x=box.x,y=box.y,food=StorageRules.Count(box,InventoryItem.Food),space=box.capacity-StorageRules.Used(box),observedTick=Tick};
        }
        private bool LessUsefulStorage(ActorState actor,StorageMemoryRecord a,StorageMemoryRecord b)
        {
            if(a.observedTick!=b.observedTick)return a.observedTick<b.observedTick;
            int distance=DistanceSquared(actor,new ActorState{x=a.x,y=a.y}),other=DistanceSquared(actor,new ActorState{x=b.x,y=b.y});return distance!=other?distance>other:Key(a.x,a.y)>Key(b.x,b.y);
        }
        private void RememberStorage(ref ActorState actor,StorageMemoryRecord observation)
        {
            StorageMemoryRecord[] notes=actor.storageMemory;
            if(notes==null){actor.storageMemory=new[]{observation};return;}
            for(int i=0;i<notes.Length;i++)if(notes[i].x==observation.x && notes[i].y==observation.y){notes[i]=observation;return;}
            if(notes.Length<StorageMemoryRules.Limit){Array.Resize(ref notes,notes.Length+1);notes[notes.Length-1]=observation;}
            else
            {
                int worst=0;for(int i=1;i<notes.Length;i++)if(LessUsefulStorage(actor,notes[i],notes[worst]))worst=i;
                if(!LessUsefulStorage(actor,notes[worst],observation))return;
                notes[worst]=observation;
            }
            Array.Sort(notes,(a,b)=>Key(a.x,a.y).CompareTo(Key(b.x,b.y)));actor.storageMemory=notes;
        }
        private void ObserveStorageBoxes(ref ActorState actor)
        {
            foreach(StorageBoxDocument box in map.StorageRecords)if(StorageVisible(actor,box))RememberStorage(ref actor,StorageObservation(box));
        }
        private void PruneStorageMemory(ref ActorState actor)
        {
            if(actor.storageMemory==null)return;int keep=0;foreach(StorageMemoryRecord note in actor.storageMemory)if(Tick-note.observedTick<StorageMemoryRules.RememberTicks)keep++;
            if(keep==actor.storageMemory.Length)return;if(keep==0){actor.storageMemory=null;return;}
            StorageMemoryRecord[] retained=new StorageMemoryRecord[keep];int index=0;foreach(StorageMemoryRecord note in actor.storageMemory)if(Tick-note.observedTick<StorageMemoryRules.RememberTicks)retained[index++]=note;actor.storageMemory=retained;
        }
        private bool KnownStorageState(ActorState actor,StorageBoxDocument box,out int food,out int space)
        {
            food=-1;space=-1;
            if(StorageVisible(actor,box)){food=StorageRules.Count(box,InventoryItem.Food);space=box.capacity-StorageRules.Used(box);return true;}
            if(!BoxMemoryEnabled)return false;
            if(actor.storageMemory!=null)foreach(StorageMemoryRecord note in actor.storageMemory)if(note.x==box.x && note.y==box.y){food=note.food;space=note.space;return true;}
            // An assigned return trip retains its destination even if other observations displaced that note.
            return actor.restockPhase==RestockPhase.Returning && actor.restockBox==Key(box.x,box.y)+1;
        }
        private bool KnownStorageFull(ActorState actor,StorageBoxDocument box)
        {
            int food,space;return KnownStorageState(actor,box,out food,out space) && space==0;
        }
        private void ValidateStorageMemory()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.storageMemory==null)continue;
                if(!BoxMemoryEnabled || actor.species!=Species.Human || actor.health<=0 || actor.storageMemory.Length==0 || actor.storageMemory.Length>StorageMemoryRules.Limit)throw new ArgumentException("Unsupported storage memory.");
                int previous=-1;
                foreach(StorageMemoryRecord note in actor.storageMemory)
                {
                    StorageBoxDocument box=map.StorageBoxAt(note.x,note.y);
                    if(note.reportedBy<0 || (note.reportedBy>0 && (!BoxReportsEnabled || note.reportedBy==actor.id || note.reportedBy>=nextId)) || box==null || note.food<0 || note.food>box.capacity || note.space<0 || note.space>box.capacity || note.food+note.space>box.capacity || note.observedTick<0 || note.observedTick>Tick || Tick-note.observedTick>=StorageMemoryRules.RememberTicks || Key(note.x,note.y)<=previous)throw new ArgumentException("Invalid storage observation.");previous=Key(note.x,note.y);
                }
            }
        }
    }
}
