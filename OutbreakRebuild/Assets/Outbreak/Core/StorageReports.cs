using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StorageReportRules { public const int RefreshTicks=300; }
    public sealed partial class World
    {
        private bool UsefulStorageReport(ActorState receiver,StorageMemoryRecord report)
        {
            if(receiver.storageMemory==null)return true;
            foreach(StorageMemoryRecord known in receiver.storageMemory)if(known.x==report.x && known.y==report.y)
            {
                return report.observedTick>known.observedTick && (report.food!=known.food || report.space!=known.space || report.observedTick-known.observedTick>=StorageReportRules.RefreshTicks);
            }
            if(receiver.storageMemory.Length<StorageMemoryRules.Limit)return true;
            int worst=0;for(int i=1;i<receiver.storageMemory.Length;i++)if(LessUsefulStorage(receiver,receiver.storageMemory[i],receiver.storageMemory[worst]))worst=i;
            return LessUsefulStorage(receiver,receiver.storageMemory[worst],report);
        }
        private bool TryShareStorageReport(int index,ref ActorState speaker,int threat)
        {
            if(speaker.health<=0 || speaker.turnAtTick!=0 || speaker.hunger>=HungerRules.SeekAt || speaker.storageMemory==null || threat>=0 || HasRecentWarning(speaker) || HasRecentDoorNoise(speaker) || speaker.freezeUntilTick>Tick || (speaker.reaction!=HumanReaction.Calm && speaker.reaction!=HumanReaction.Aware))return false;
            int recipient=-1,need=-1,rank=-1;StorageMemoryRecord chosen=new StorageMemoryRecord();
            for(int i=0;i<count;i++)
            {
                ActorState other=actors[i];
                if(i==index || other.health<=0 || other.species!=Species.Human || other.turnAtTick!=0 || other.freezeUntilTick>Tick || Math.Abs(other.x-speaker.x)+Math.Abs(other.y-speaker.y)>1 || !Visible(speaker,other) || HasRecentWarning(other) || HasRecentDoorNoise(other) || NearestVisible(other,Species.Zombie)>=0)continue;
                foreach(StorageMemoryRecord note in speaker.storageMemory)
                {
                    if(note.reportedBy!=0 || !UsefulStorageReport(other,note))continue;
                    StorageBoxDocument box=map.StorageBoxAt(note.x,note.y);if(box==null || StorageVisible(other,box))continue;
                    int priority=note.food>0?1:0;
                    if(other.hunger>need || (other.hunger==need && (priority>rank || (priority==rank && note.observedTick>chosen.observedTick)))){recipient=i;need=other.hunger;rank=priority;chosen=note;}
                }
            }
            if(recipient<0)return false;
            ActorState receiver=actors[recipient];chosen.reportedBy=speaker.id;RememberStorage(ref receiver,chosen);actors[recipient]=receiver;actors[index]=speaker;return true; // The original observation age survives; conversation consumes this actor's action.
        }
    }
}
