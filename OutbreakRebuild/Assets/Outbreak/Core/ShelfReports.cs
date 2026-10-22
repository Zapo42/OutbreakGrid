using System; // Preserve deterministic age and adjacency checks.
using Outbreak.Contracts; // Copy one dated fact without exposing a mutable notebook.
namespace Outbreak.Core
{
    public static class ShelfReportRules { public const int RefreshTicks = 300; } // Repeating unchanged information requires a meaningfully fresher observation.
    public sealed partial class World
    {
        private bool UsefulShelfReport(ActorState receiver, ShelfMemoryRecord report)
        {
            if (receiver.shelfMemory == null) return true;
            foreach (ShelfMemoryRecord known in receiver.shelfMemory) if (known.x == report.x && known.y == report.y)
            {
                if (report.observedTick < known.observedTick) return false;
                return (report.observedTick == known.observedTick && known.food < 0) || (report.observedTick > known.observedTick && (report.food != known.food || report.observedTick - known.observedTick >= ShelfReportRules.RefreshTicks)); // Never overwrite fresher knowledge or repeat identical facts every action.
            }
            if (receiver.shelfMemory.Length < ShelfMemoryRules.Limit) return true;
            int worst = 0; for (int i=1;i<receiver.shelfMemory.Length;i++) if (LessUsefulShelf(receiver,receiver.shelfMemory[i],receiver.shelfMemory[worst])) worst=i;
            return LessUsefulShelf(receiver,receiver.shelfMemory[worst],report); // A full notebook must actually be able to retain the report.
        }
        private bool TryShareShelfReport(int index, ref ActorState speaker, int threatIndex)
        {
            if (speaker.turnAtTick!=0 || speaker.hunger>=HungerRules.SeekAt || speaker.shelfMemory==null || threatIndex>=0 || HasRecentWarning(speaker) || HasRecentDoorNoise(speaker) || speaker.freezeUntilTick>Tick || (speaker.reaction!=HumanReaction.Calm && speaker.reaction!=HumanReaction.Aware)) return false;
            int recipient=-1, need=-1, rank=-1; ShelfMemoryRecord chosen=new ShelfMemoryRecord();
            for (int i=0;i<count;i++)
            {
                ActorState other=actors[i]; if(i==index || other.health==0 || other.species!=Species.Human || other.turnAtTick!=0 || Math.Abs(other.x-speaker.x)+Math.Abs(other.y-speaker.y)>1 || !Visible(speaker,other) || HasRecentWarning(other) || HasRecentDoorNoise(other) || NearestVisible(other,Species.Zombie)>=0)continue;
                foreach (ShelfMemoryRecord note in speaker.shelfMemory)
                {
                    if(note.reportedBy!=0 || note.food<0 || !UsefulShelfReport(other,note))continue;int priority=note.food>0?1:0;
                    if(other.hunger>need || (other.hunger==need && (priority>rank || (priority==rank && note.observedTick>chosen.observedTick)))) {recipient=i;need=other.hunger;rank=priority;chosen=note;} // Hungry recipients, useful food, then freshest source; stable iteration resolves ties.
                }
            }
            if(recipient<0)return false;ActorState receiver=actors[recipient];chosen.reportedBy=speaker.id;RememberShelf(ref receiver,chosen);
            if(chosen.food==0 && receiver.foodUntilTick>Tick && receiver.foodX==chosen.x && receiver.foodY==chosen.y)ClearFoodMemory(ref receiver);
            actors[recipient]=receiver;actors[index]=speaker;return true; // Original observation time survives the conversation; hearsay cannot be relayed.
        }
    }
}
