using Outbreak.Contracts; // Transfer actual carried meals rather than creating a hunger bonus.
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private int FoodReserve(ActorState actor)
        {
            return RecoveryRations && actor.species==Species.Human && actor.turnAtTick==0 && actor.health<ShelterRecoveryRules.ExtraReserveBelowHealth?ShelterRecoveryRules.RecoveryReserveMeals:1;
        }
        private bool TryShareFood(int index, ref ActorState donor, int threatIndex)
        {
            if (donor.turnAtTick != 0 || donor.hunger >= HungerRules.SeekAt || donor.carriedFood <= FoodReserve(donor) || threatIndex >= 0 || HasRecentWarning(donor) || HasRecentDoorNoise(donor) || donor.freezeUntilTick > Tick || (donor.reaction != HumanReaction.Calm && donor.reaction != HumanReaction.Aware)) return false; // Eat first when hungry, keep the personal reserve and let immediate survival take priority.
            int recipient = -1, hungriest = -1;
            for (int i = 0; i < count; i++)
            {
                ActorState other = actors[i]; if (i == index || other.species != Species.Human || other.turnAtTick != 0 || other.hunger < HungerRules.SeekAt || other.carriedFood != 0 || System.Math.Abs(other.x - donor.x) + System.Math.Abs(other.y - donor.y) > 1 || !Visible(donor, other) || HasRecentWarning(other) || HasRecentDoorNoise(other) || NearestVisible(other, Species.Zombie) >= 0) continue; // Nearby visible people can ask for help; hidden recipients never influence the choice.
                if (other.hunger > hungriest) { recipient = i; hungriest = other.hunger; } // Canonical actor order breaks equal-need ties by stable identity.
            }
            if (recipient < 0) return false;
            ActorState receiver = actors[recipient]; donor.carriedFood--; receiver.carriedFood++; ClearFoodMemory(ref receiver); ClearShelterDeparture(ref receiver); ClearShelterDeparture(ref donor); actors[recipient] = receiver; actors[index] = donor; return true; // One real meal moves between inventories; hunger changes only when the receiver later eats.
        }
    }
}
