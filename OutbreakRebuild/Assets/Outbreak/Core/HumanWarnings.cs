using System; // Reject invalid warning and cooldown records before publishing restored state.
using Outbreak.Contracts; // Separate heard speakers from personally observed zombie locations.
namespace Outbreak.Core // Deliver warnings from actual sight using the existing acoustic frontier.
{ // Begin nearby human warnings.
    public static class HumanWarningRules // Publish provisional communication tuning in logical ticks.
    { // Begin bounded warning constants.
        public const int MemoryTicks = 25, CooldownTicks = 50, StressGain = 8; // Remember a warning for five seconds; a witness warns at most once every ten seconds.
    } // Finish warning tuning.
    public sealed partial class World // Reuse the world clock, perception and sound geometry.
    { // Begin warning state and delivery.
        private bool HasRecentWarning(ActorState actor) { return HumanWarnings && actor.heardWarningUntilTick > Tick; } // Hearing is concern rather than a new visual zombie target.
        private static void ClearHeardWarning(ref ActorState actor) { actor.heardWarningX = 0; actor.heardWarningY = 0; actor.heardWarningUntilTick = 0; } // Canonicalize expired hearing.
        private static void ClearHumanWarnings(ref ActorState actor) { ClearHeardWarning(ref actor); actor.nextWarningTick = 0; } // Turning releases all human-only communication state.
        private void AdvanceWarningTimers(ref ActorState actor) // Expire memory and speaker cooldowns even between actor opportunities.
        { // Begin exact warning deadline handling.
            if (actor.heardWarningUntilTick != 0 && actor.heardWarningUntilTick <= Tick) ClearHeardWarning(ref actor); if (actor.nextWarningTick != 0 && actor.nextWarningTick <= Tick) actor.nextWarningTick = 0; // A future sight observation is still required before another warning.
        } // Finish warning deadlines.
        private void ValidateHumanWarnings() // Check source geometry, species and supported timing without requiring a surviving speaker.
        { // Begin restored warning validation.
            for (int i = 0; i < count; i++) { ActorState a = actors[i]; if (a.heardWarningUntilTick == 0) { if (a.heardWarningX != 0 || a.heardWarningY != 0) throw new ArgumentException("Inactive warning contains stale coordinates."); } else if (!HumanWarnings || (a.species != Species.Human && !ZombieWarningInvestigation) || a.heardWarningUntilTick <= Tick || a.heardWarningUntilTick - Tick > HumanWarningRules.MemoryTicks || !ValidObservedCell(a.heardWarningX, a.heardWarningY)) throw new ArgumentException("Invalid heard warning."); if (a.nextWarningTick != 0 && (!HumanWarnings || a.species != Species.Human || a.nextWarningTick <= Tick || a.nextWarningTick - Tick > HumanWarningRules.CooldownTicks)) throw new ArgumentException("Invalid warning cooldown."); } // The source cell may be a doorway that closed after the speaker moved away.
        } // Finish warning validation.
        private int InvestigateWarning(int index, ref ActorState actor) // Check only the heard speaker cell, without tracking a hidden human.
        { // Begin sound-only navigation.
            if (actor.x == actor.heardWarningX && actor.y == actor.heardWarningY) { ClearHeardWarning(ref actor); actors[index] = actor; return 8; } return Route(actor, new ActorState { x = actor.heardWarningX, y = actor.heardWarningY }, true); // Closed doors still require actual remembered human pursuit before an attack.
        } // Finish warning investigation.
        private void WarnNearbyHumans(int index) // Called only when this human has a currently visible zombie on its normal opportunity.
        { // Begin one real warning event.
            ActorState speaker = actors[index]; if (speaker.nextWarningTick > Tick) return; speaker.nextWarningTick = Tick + HumanWarningRules.CooldownTicks; actors[index] = speaker; PrepareSoundReach(speaker.x, speaker.y); int left = speaker.x - 8, top = speaker.y - 8; // A heard warning alone never invokes this method or reveals the seen zombie's cell.
            for (int i = 0; i < count; i++) { ActorState listener = actors[i]; if (listener.health==0 || listener.id == speaker.id || (listener.species == Species.Zombie && !ZombieWarningInvestigation)) continue; int lx = listener.x - left, ly = listener.y - top; if (lx < 0 || ly < 0 || lx >= 17 || ly >= 17 || soundDistance[ly * 17 + lx] < 0) continue; listener.heardWarningX = speaker.x; listener.heardWarningY = speaker.y; listener.heardWarningUntilTick = Tick + HumanWarningRules.MemoryTicks; if (listener.species == Species.Zombie) { actors[i] = listener; continue; } listener.stress = Math.Min(100, listener.stress + HumanWarningRules.StressGain); ClearShelterDeparture(ref listener); if (listener.reaction == HumanReaction.Calm || listener.reaction == HumanReaction.Aware) listener.reaction = HumanReaction.Aware; actors[i] = listener; } // Preserve infection, flight, fight, freeze and personal visual observations; zombies receive only the heard location and deadline, without a target identity.
        } // Finish warning delivery.
    } // Finish communication implementation.
} // Finish locally heard human warnings.
