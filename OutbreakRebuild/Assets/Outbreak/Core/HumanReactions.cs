using System; // Validate portable reaction state and bound prototype tuning.
using Outbreak.Contracts; // Use saved actor values without engine objects.
namespace Outbreak.Core // Keep human decisions on the simulation clock.
{ // Begin reaction rules and implementation.
    public static class HumanRules // Publish provisional tuning separately from the retained courage formula.
    { // Begin explicit cell and tick units.
        public const int SupportRadius = 4, SupportTicks = 10, MinSupport = -3, MaxSupport = 6; // Sample visible nearby contributions every two simulation seconds and bound their total.
        public const int StressGain = 2, CloseStressGain = 4, Recovery = 2, MinFreezeTicks = 5, MaxFreezeTicks = 15, ShoveTicks = 3, ShoveCooldown = 10; // Prototype stress units per decision and clock-based defensive action timing.
        public static int Margin(ActorState actor) { return actor.bravery + actor.groupSupport - actor.stress / 20 - actor.threatPressure; } // Retain bravery plus support minus stress and pressure.
        public static StressLevel Level(int stress) { return stress <= 0 ? StressLevel.Normal : (StressLevel)Math.Min(5, 1 + (stress - 1) / 20); } // Map increasing stress to all six retained display labels.
    } // Finish prototype tuning.
    public sealed partial class World // Extend the existing authoritative encounter owner.
    { // Begin human response implementation.
        public bool HumanReactions { get; private set; } // Keep old saves on their original decisions.
        public bool CautiousMovement { get; private set; } // Keep prior reaction saves on their original pursuit and retreat choices.
        private RandomStream reactionRandom; // Isolate variable freeze durations from movement randomness.
        private static void ClearHumanReaction(ref ActorState actor) // Drop human-only state when infection changes species.
        { // Begin canonical conversion cleanup.
            actor.nextMeleeTick=0; ClearHunger(ref actor); ClearHumanWarnings(ref actor); ClearDoorHearing(ref actor); ClearShelter(ref actor); actor.rejectedShelterId = 0; actor.rejectedUntilTick = 0; ClearAvoidance(ref actor); actor.stress = 0; actor.groupSupport = 0; actor.threatPressure = 0; actor.reaction = HumanReaction.Calm; actor.freezeResolved = false; actor.freezeUntilTick = 0; actor.supportUntilTick = 0; actor.nextShoveTick = 0; actor.staggerUntilTick = 0; // Retain no stale human decisions on a zombie.
        } // Finish human-state cleanup.
        private void AdvanceReactionTimers(ref ActorState actor) // Complete deadlines even on ticks without an actor decision.
        { // Begin exact deadline handling.
            if (actor.staggerUntilTick != 0 && actor.staggerUntilTick <= Tick) actor.staggerUntilTick = 0; if (actor.nextShoveTick != 0 && actor.nextShoveTick <= Tick) actor.nextShoveTick = 0; // Release stagger and defensive cooldown at their exact clock boundary.
            if (actor.freezeUntilTick != 0 && actor.freezeUntilTick <= Tick) { actor.freezeUntilTick = 0; actor.freezeResolved = true; actor.reaction = HumanRules.Margin(actor) >= 1 ? HumanReaction.Fight : HumanReaction.Flight; } // A freeze resolves once rather than renewing indefinitely.
        } // Finish timer advancement.
        private void ValidateHumanReactions() // Reject malformed decisions before returning a restored candidate.
        { // Begin portable-state validation.
            for (int i = 0; i < count; i++) // Inspect every record while preserving the current live world.
            { // Begin one actor validation.
                ActorState a = actors[i]; bool hasHumanState = a.stress != 0 || a.groupSupport != 0 || a.threatPressure != 0 || a.reaction != HumanReaction.Calm || a.freezeResolved || a.freezeUntilTick != 0 || a.supportUntilTick != 0 || a.nextShoveTick != 0; // Identify state incompatible with old rules or zombie species.
                if ((!HumanReactions && (hasHumanState || a.staggerUntilTick != 0)) || (a.species == Species.Zombie && hasHumanState)) throw new ArgumentException("Reaction state is incompatible with these rules or species."); // Keep older releases and conversion state explicit.
                if (a.stress < 0 || a.stress > 100 || a.groupSupport < HumanRules.MinSupport || a.groupSupport > HumanRules.MaxSupport || a.threatPressure < 0 || a.threatPressure > 3 || a.reaction < HumanReaction.Calm || a.reaction > HumanReaction.Flight) throw new ArgumentException("Invalid human reaction values."); // Bound inputs before any future arithmetic.
                if (a.freezeUntilTick < 0 || (a.freezeUntilTick != 0 && (a.freezeUntilTick <= Tick || a.freezeUntilTick - Tick > HumanRules.MaxFreezeTicks || a.reaction != HumanReaction.Freeze || a.freezeResolved)) || (a.reaction == HumanReaction.Freeze && a.freezeUntilTick == 0)) throw new ArgumentException("Invalid freeze deadline."); // Require a future deadline for an unresolved freeze.
                if (a.supportUntilTick < 0 || a.supportUntilTick > Tick + HumanRules.SupportTicks || a.nextShoveTick < 0 || (a.nextShoveTick != 0 && (a.nextShoveTick <= Tick || a.nextShoveTick - Tick > HumanRules.ShoveCooldown))) throw new ArgumentException("Invalid support or shove timing."); // Support may be overdue until the next staggered observation opportunity.
                if (a.staggerUntilTick < 0 || (a.staggerUntilTick != 0 && (a.species != Species.Zombie || a.staggerUntilTick <= Tick || a.staggerUntilTick - Tick > HumanRules.ShoveTicks))) throw new ArgumentException("Invalid zombie stagger."); // Humans have no stagger mechanic in this slice.
            } // Finish actor validation.
        } // Finish reaction-state validation.
        private int ObserveSupport(ActorState observer) // Count only nearby humans whose cells the observer can actually see.
        { // Begin a bounded support observation.
            int total = 0; for (int i = 0; i < count; i++) { ActorState ally = actors[i]; if (ally.id == observer.id || ally.species != Species.Human) continue; int x = ally.x - observer.x, y = ally.y - observer.y; if (x * x + y * y <= HumanRules.SupportRadius * HumanRules.SupportRadius && Visible(observer, ally)) total += Math.Max(-1, Math.Min(3, ally.bravery)); } return Math.Max(HumanRules.MinSupport, Math.Min(HumanRules.MaxSupport, total)); // Brave allies contribute up to three; each coward subtracts at most one.
        } // Finish support observation.
        private int ObservePressure(ActorState observer, ActorState closest) // Use locally visible zombies rather than the total world population.
        { // Begin threat observation.
            int visible = 0; for (int i = 0; i < count; i++) { ActorState zombie = actors[i]; if (zombie.species != Species.Zombie || zombie.health==0) continue; int x = zombie.x - observer.x, y = zombie.y - observer.y; if (x * x + y * y <= EncounterRules.SightRadius * EncounterRules.SightRadius && Visible(observer, zombie)) visible++; } int distance = Math.Max(Math.Abs(closest.x - observer.x), Math.Abs(closest.y - observer.y)); return Math.Min(3, Math.Max(0, visible - 1) + (distance <= 2 ? 1 : 0)); // One distant zombie is less pressure than contact or a visible group.
        } // Finish threat observation.
        private int RetreatDirection(ActorState actor, ActorState threat) // Retain collision-safe neighboring retreat until shelter planning is implemented.
        { // Begin local escape selection.
            int direction = 8, alignment = int.MinValue, best = (actor.x - threat.x) * (actor.x - threat.x) + (actor.y - threat.y) * (actor.y - threat.y); // Never choose a step closer merely to keep moving.
            for (int d = 0; d < 8; d++) if (MayMove(actor, d)) { int x = actor.x + dx[d] - threat.x, y = actor.y + dy[d] - threat.y, score = x * x + y * y; int away = ((actor.x - threat.x) * dx[d] + (actor.y - threat.y) * dy[d]) * (dx[d] != 0 && dy[d] != 0 ? 707 : 1000); if ((!CautiousMovement && score > best) || (CautiousMovement && score > best && away > alignment)) { if (!CautiousMovement) best = score; alignment = away; direction = d; } } return direction; // Keep equal-distance decisions deterministic.
        } // Finish retreat selection.
        private void Shove(int humanIndex, int zombieIndex) // Give Fight an actual nonlethal defensive action rather than a display-only label.
        { // Begin a contact defense.
            ActorState human = actors[humanIndex], zombie = actors[zombieIndex]; if (human.nextShoveTick > Tick || zombie.staggerUntilTick > Tick) return; human.nextShoveTick = Tick + HumanRules.ShoveCooldown; zombie.staggerUntilTick = Tick + HumanRules.ShoveTicks; // A cooldown prevents each human from continuously renewing stagger.
            int nx = zombie.x + Math.Sign(zombie.x - human.x), ny = zombie.y + Math.Sign(zombie.y - human.y); // Push directly away from the human when a distinct neighboring cell exists.
            if ((nx != zombie.x || ny != zombie.y) && ClearStep(zombie.x, zombie.y, nx, ny) && occupancy.Allows(Key(nx, ny), Species.Zombie)) { occupancy.Change(Key(zombie.x, zombie.y), Species.Zombie, -1); zombie.x = nx; zombie.y = ny; occupancy.Change(Key(nx, ny), Species.Zombie, 1); } // A wall or full destination prevents displacement while retaining the brief stagger.
            actors[humanIndex] = human; actors[zombieIndex] = zombie; // Preserve both stable identities and the zombie's health.
        } // Finish nonlethal defense.
        private int ReactHuman(int index, int threatIndex) // Evaluate observation, stress, courage and the resulting action once per opportunity.
        { // Begin a complete human decision.
            ActorState actor = actors[index]; if (actor.supportUntilTick <= Tick) { actor.groupSupport = ObserveSupport(actor); actor.supportUntilTick = Tick + HumanRules.SupportTicks; } // Departures affect support only when the human next observes nearby allies.
            if(MeleeCombat && TryPickupBat(index,ref actor,threatIndex))return 8;
            if (ShelfMemoryEnabled) { ObserveShelves(ref actor); actors[index] = actor; } // Observe even while fed; retain personal knowledge for later trips.
            if (FoodSharing && TryShareFood(index, ref actor, threatIndex)) return 8; // A transfer consumes the donor action; the recipient eats on its own opportunity.
            int deliveryDirection; if (FoodDelivery && TryDeliverFood(index, ref actor, threatIndex, out deliveryDirection)) return deliveryDirection;
            int pantryDirection; if (PantryStorage && TryStoreFood(index, ref actor, threatIndex, out pantryDirection)) return pantryDirection; // Stock nearby shelter furniture using a normal action.
            if (ShelfReports && TryShareShelfReport(index, ref actor, threatIndex)) return 8; // A safe conversation is a separate action, after actual meal sharing.
            int foodDirection; if (HungerEnabled && TryForage(index, ref actor, threatIndex, out foodDirection)) return foodDirection;
            int shelterDirection; if (SheltersEnabled && TryShelter(index, ref actor, threatIndex, out shelterDirection)) return shelterDirection; // Retain a reachable room goal before falling back to local escape.
            if (InfectedEscape && actor.turnAtTick > Tick) return ReactInfected(index, actor, threatIndex); // Infection overrides fighting, freezing and wandering before any normal courage decision.
            if (threatIndex < 0) { actor.stress = Math.Max(0, actor.stress - RecoveryAfterDoorNoise(actor)); actor.threatPressure = 0; actor.reaction = actor.stress == 0 ? HumanReaction.Calm : HumanReaction.Aware; actor.freezeUntilTick = 0; if (actor.stress == 0) actor.freezeResolved = false; actors[index] = actor; return (HasRecentDoorNoise(actor) || HasRecentWarning(actor)) ? 8 : random.Below(9); } // Recover without sight of danger; this is not yet a shelter/hiding planner.
            ActorState threat = actors[threatIndex]; int distance = Math.Max(Math.Abs(threat.x - actor.x), Math.Abs(threat.y - actor.y)); actor.threatPressure = ObservePressure(actor, threat); actor.stress = Math.Min(100, actor.stress + (distance <= 2 ? HumanRules.CloseStressGain : HumanRules.StressGain)); // Seeing danger raises stress while close contact raises it faster.
            if (actor.freezeUntilTick > Tick) { actors[index] = actor; return 8; } // Respect the originally chosen freeze duration without rerolling it.
            int margin = HumanRules.Margin(actor); if (distance > 6 && actor.stress < 20) actor.reaction = HumanReaction.Aware; // Observe a distant threat before automatically running or charging it.
            else if (margin >= 1) actor.reaction = HumanReaction.Fight; else if (margin <= -2 || actor.freezeResolved) actor.reaction = HumanReaction.Flight; // Retain the agreed courage thresholds and avoid a second freeze loop.
            else { actor.reaction = HumanReaction.Freeze; actor.freezeUntilTick = Tick + HumanRules.MinFreezeTicks + reactionRandom.Below(HumanRules.MaxFreezeTicks - HumanRules.MinFreezeTicks + 1); } // Use a named saved stream for a variable one-to-three-second freeze.
            actors[index] = actor; if (actor.reaction == HumanReaction.Flight) return RetreatDirection(actor, threat); if (actor.reaction != HumanReaction.Fight) return 8; // Aware watches and Freeze stops; Flight retreats immediately.
            if (distance <= 1) { if(MeleeCombat && actor.meleeWeapon==MeleeWeapon.Bat)SwingBat(index,threatIndex);else Shove(index, threatIndex); return 8; } // Keep defensive contact available without requiring an approach.
            if (CautiousMovement && HumanRules.Level(actor.stress) <= StressLevel.Cautious) { actor.reaction = HumanReaction.Aware; actors[index] = actor; return distance <= 3 ? RetreatDirection(actor, threat) : 8; } // Wary humans watch or back away; support cannot make them charge a visible zombie.
            return Route(actor, threat); // Fight approaches visible danger and shoves on contact without inventing weapon damage.
        } // Finish human decisions.
    } // Finish the world extension.
} // Finish human reaction behavior.
