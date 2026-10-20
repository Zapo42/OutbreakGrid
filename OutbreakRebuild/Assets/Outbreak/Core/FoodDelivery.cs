using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private bool TryDeliverFood(int index, ref ActorState donor, int threatIndex, out int direction)
        {
            direction=8;
            if(donor.turnAtTick!=0 || donor.hunger>=HungerRules.SeekAt || donor.carriedFood<=FoodReserve(donor) || donor.shelterState!=ShelterState.Inside || threatIndex>=0 || HasRecentWarning(donor) || HasRecentDoorNoise(donor) || donor.freezeUntilTick>Tick || (donor.reaction!=HumanReaction.Calm && donor.reaction!=HumanReaction.Aware))return false;
            Room room=FindRoom(donor.shelterId);if(room==null || !room.Contains(Key(donor.x,donor.y)))return false;
            int need=-1;
            for(int i=0;i<count;i++)
            {
                ActorState other=actors[i];
                if(i==index || other.species!=Species.Human || other.turnAtTick!=0 || other.hunger<HungerRules.SeekAt || other.carriedFood!=0 || other.shelterState!=ShelterState.Inside || other.shelterId!=room.id || !room.Contains(Key(other.x,other.y)) || !Visible(donor,other) || HasRecentWarning(other) || HasRecentDoorNoise(other) || NearestVisible(other,Species.Zombie)>=0 || other.freezeUntilTick>Tick || (other.reaction!=HumanReaction.Calm && other.reaction!=HumanReaction.Aware))continue;
                if(other.hunger<=need)continue; // Stable actor order breaks equal-need ties.
                int route=-1,best=int.MaxValue;
                for(int d=0;d<8;d+=2)
                {
                    int x=other.x+dx[d],y=other.y+dy[d],length;
                    if(!room.Contains(Key(x,y)) || !map.CanWalk(x,y))continue;
                    int candidate=ShelterRoute(donor,x,y,0,out length,room);
                    if(candidate>=0 && length>0 && length<best){route=candidate;best=length;}
                }
                if(route>=0){need=other.hunger;direction=route;}
            }
            if(need<0)return false;
            ClearShelterDeparture(ref donor);actors[index]=donor;return true; // Only approach here; the existing adjacent sharing action transfers one real meal later.
        }
    }
}
