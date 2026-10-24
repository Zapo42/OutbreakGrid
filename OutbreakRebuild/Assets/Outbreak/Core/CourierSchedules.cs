using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class CourierScheduleRules
    {
        public const int StartHour=7,HoursPerDay=24;
    }
    public sealed partial class World
    {
        public int CourierClockHour { get { return (int)((Tick/Rules.TicksPerGameHour+CourierScheduleRules.StartHour)%CourierScheduleRules.HoursPerDay); } }
        private static void ClearCourierSchedule(ref ActorState actor)
        {
            actor.courierShiftStartHour=0;actor.courierShiftHours=0;
        }
        private bool CourierWorkingNow(ActorState actor)
        {
            if(!CourierSchedulesEnabled || actor.courierShiftHours==0)return true;
            int elapsed=(CourierClockHour-actor.courierShiftStartHour+CourierScheduleRules.HoursPerDay)%CourierScheduleRules.HoursPerDay;
            return elapsed<actor.courierShiftHours;
        }
        public bool CourierOnDuty(int index)
        {
            ActorState actor=ActorAt(index);return actor.species==Species.Human && actor.health>0 && (actor.courierBoxes!=null || (OrderFulfillmentEnabled && actor.distributionCenterId!=0)) && CourierWorkingNow(actor);
        }
        public string CourierScheduleStatus(int index)
        {
            ActorState actor=ActorAt(index);if(actor.species!=Species.Human || actor.health<=0 || (actor.courierBoxes==null && !(OrderFulfillmentEnabled && actor.distributionCenterId!=0)))return "No courier route";
            if(CourierWorkingNow(actor))return "On duty";
            if(actor.restockPhase!=RestockPhase.None)return "Finishing current delivery";
            return HouseAt(actor.x,actor.y)==actor.assignedHouseId?"Off duty: home/rest":"Off duty: return home when safe";
        }
        public bool TrySetCourierSchedule(int index,int startHour,int workHours)
        {
            if(!CourierSchedulesEnabled || index<0 || index>=count || startHour<0 || startHour>23 || workHours<0 || workHours>23 || (workHours==0 && startHour!=0))return false;
            ActorState actor=actors[index];if(actor.species!=Species.Human || actor.health<=0 || actor.turnAtTick!=0 || (actor.courierBoxes==null && !(OrderFulfillmentEnabled && actor.distributionCenterId!=0)))return false;
            // Shift changes affect new jobs only; current cargo, destination, door progress and task timeout remain intact.
            actor.courierShiftStartHour=startHour;actor.courierShiftHours=workHours;actors[index]=actor;return true;
        }
        private void ValidateCourierSchedules()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.courierShiftStartHour==0 && actor.courierShiftHours==0)continue;
                if(!CourierSchedulesEnabled || actor.species!=Species.Human || actor.health<=0 || (actor.courierBoxes==null && !(OrderFulfillmentEnabled && actor.distributionCenterId!=0)) || actor.courierShiftStartHour<0 || actor.courierShiftStartHour>23 || actor.courierShiftHours<1 || actor.courierShiftHours>23)throw new ArgumentException("Invalid courier schedule.");
            }
        }
    }
}
