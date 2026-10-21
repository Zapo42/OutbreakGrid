using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class SprintRules
    {
        public const int MinimumEnergy=30,ThreatDistance=6,ExtraPhase=2,ExtraStepCost=2;
    }
    public sealed partial class World
    {
        private bool[] sprintMoves;
        private bool CanEscapeSprint(ActorState actor,out int threat)
        {
            threat=-1;
            if(!SprintEnabled || actor.species!=Species.Human || actor.health<=0 || actor.reaction!=HumanReaction.Flight || EnergyRules.Remaining(actor)<SprintRules.MinimumEnergy || actor.freezeUntilTick>Tick || actor.staggerUntilTick>Tick || actor.bracedDoor!=0 || actor.nextShoveTick!=0 || actor.nextMeleeTick!=0 || IsDoor(actor.x,actor.y))return false;
            threat=NearestVisible(actor,Species.Zombie);
            return threat>=0 && Math.Max(Math.Abs(actor.x-actors[threat].x),Math.Abs(actor.y-actors[threat].y))<=SprintRules.ThreatDistance;
        }
        public bool EscapeSprintAvailable(int index){int threat;return CanEscapeSprint(ActorAt(index),out threat);}
        public bool SprintMovedThisTick(int index){ActorAt(index);return sprintMoves!=null && sprintMoves[index];}
        public int MovementBlendTicks(int index)
        {
            if(SprintMovedThisTick(index))return EncounterRules.MoveInterval-SprintRules.ExtraPhase;
            return EscapeSprintAvailable(index)?SprintRules.ExtraPhase:EncounterRules.MoveInterval;
        }
        private void TrySprintStep(int index,ActorState actor)
        {
            if(!SprintEnabled || (Tick%EncounterRules.MoveInterval-actor.id%EncounterRules.MoveInterval+EncounterRules.MoveInterval)%EncounterRules.MoveInterval!=SprintRules.ExtraPhase)return;
            int threat;if(!CanEscapeSprint(actor,out threat))return;
            int direction;Room room=FindRoom(actor.shelterId);
            if(room!=null){int length;direction=ShelterRoute(actor,actor.shelterX,actor.shelterY,KnownShelterThreats(actor,threat),out length);}
            else direction=actor.turnAtTick!=0?EscapeDirection(actor,actors[threat],true):RetreatDirection(actor,actors[threat]);
            if(direction<0 || direction==8 || !MayMove(actor,direction) || IsDoor(actor.x+dx[direction],actor.y+dy[direction]))return;
            ActorState next=actor;next.x+=dx[direction];next.y+=dy[direction];if(DistanceSquared(next,actors[threat])<DistanceSquared(actor,actors[threat]))return;
            MoveActor(index,actor,direction,true);if(SheltersEnabled)FinishShelterMove(index);
        }
    }
}
