using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class HouseholdRules
    {
        public static int SnackRelief(int endurance){return 1+4*Math.Max(0,Math.Min(10,endurance))/10;}
        public static int RestGain(HouseholdFurnitureKind kind){return kind==HouseholdFurnitureKind.Bed?12:(kind==HouseholdFurnitureKind.Couch || kind==HouseholdFurnitureKind.ComfyChair)?6:kind==HouseholdFurnitureKind.Chair?3:0;}
    }
    public sealed partial class World
    {
        private static void ClearHouseholdActivity(ref ActorState actor){actor.householdActivity=HouseholdActivity.None;actor.householdActivityTick=0;}
        private void RecordHouseholdActivity(ref ActorState actor,HouseholdActivity activity){actor.householdActivity=activity;actor.householdActivityTick=Tick;}
        private bool SeatPassableFor(int actorId,int x,int y){if(!HouseholdSeatAt(x,y))return true;for(int i=0;i<count;i++)if(actors[i].id!=actorId && actors[i].x==x && actors[i].y==y)return false;return true;}
        private bool SeatFreeFor(int observerId,int x,int y)
        {
            for(int i=0;i<count;i++){var a=actors[i];if(a.id==observerId)continue;if(a.x==x && a.y==y || a.species==Species.Human && a.health>0 && a.shelterX==x && a.shelterY==y && a.shelterId==HouseAt(x,y))return false;}return true;
        }
        private bool DiningSeat(int x,int y)
        {
            var seat=map.FurnitureRecord(x,y);if(seat==null || seat.kind!=HouseholdFurnitureKind.Chair)return false;for(int d=0;d<8;d+=2){var table=map.FurnitureRecord(x+dx[d],y+dy[d]);if(table!=null && table.kind==HouseholdFurnitureKind.KitchenTable)return true;}return false;
        }
        private bool FurnitureVisibleFrom(int x,int y,int fx,int fy)
        {
            // A solid object's visible surface is the ray endpoint, not a square the observer must walk through.
            int ax=Math.Abs(fx-x),ay=Math.Abs(fy-y),sx=Math.Sign(fx-x),sy=Math.Sign(fy-y),error=ax-ay;
            while(x!=fx || y!=fy){int twice=2*error,nx=x,ny=y;if(twice>-ay){error-=ay;nx+=sx;}if(twice<ax){error+=ax;ny+=sy;}
                if(nx==fx && ny==fy)return nx==x || ny==y || (map.CanWalk(nx,y) && map.CanWalk(x,ny) && !VehicleAt(nx,y) && !VehicleAt(x,ny));
                if(!ClearStep(x,y,nx,ny))return false;x=nx;y=ny;
            }return true;
        }
        private bool FixtureVisible(ActorState actor,int fx,int fy){int x=actor.x-fx,y=actor.y-fy;return x*x+y*y<=EncounterRules.SightRadius*EncounterRules.SightRadius && FurnitureVisibleFrom(actor.x,actor.y,fx,fy);}
        private bool TelevisionFromSeat(ActorState observer,int x,int y)
        {
            foreach(var f in map.HouseholdRecords){int ax=f.x-x,ay=f.y-y;if(f.kind==HouseholdFurnitureKind.Television && f.working && ax*ax+ay*ay<=36 && FixtureVisible(observer,f.x,f.y) && FurnitureVisibleFrom(x,y,f.x,f.y))return true;}return false;
        }
        private bool FurnitureResting(ActorState a){return HouseholdActivitiesAvailable && a.householdActivity==HouseholdActivity.Resting && Tick-a.householdActivityTick<=10 && HouseholdSeatAt(a.x,a.y);}
        public bool RestingAtFurniture(int index){return FurnitureResting(ActorAt(index));}
        public string HouseholdActivityKnownBy(int index)
        {
            var a=ActorAt(index);if(!HouseholdActivitiesAvailable)return "Household activities unavailable";if(a.householdActivity==HouseholdActivity.None || Tick-a.householdActivityTick>10)return "No furniture activity";
            switch(a.householdActivity){case HouseholdActivity.GoingToMeal:return "Going to a table chair";case HouseholdActivity.TableMeal:return "Eating a table meal";case HouseholdActivity.Snack:return "Eating a snack";case HouseholdActivity.GoingToRest:return "Going to a rest place";default:return "Resting at "+map.FurnitureRecord(a.x,a.y)?.kind;}
        }
        private int HouseholdMedia(ActorState actor)
        {
            if(!HouseholdActivitiesAvailable)return 0;int radio=0;foreach(var f in map.HouseholdRecords){int x=f.x-actor.x,y=f.y-actor.y;if(f.kind==HouseholdFurnitureKind.Radio && f.working && x*x+y*y<=16)radio=2;}
            return radio+(FurnitureResting(actor) && TelevisionFromSeat(actor,actor.x,actor.y)?2:0);
        }
        private int HouseholdParkBonus(ActorState actor)
        {
            if(!HouseholdActivitiesAvailable || HappinessRules.Preference(actor.charisma)<=0 || HouseAt(actor.x,actor.y)!=0 || actor.knownParks==null)return 0;
            foreach(int cell in actor.knownParks){int x=(cell-1)%Width-actor.x,y=(cell-1)/Width-actor.y;if(x*x+y*y<=9)return 1;}return 0;
        }
        private void EatCarriedFood(int index,ref ActorState actor)
        {
            bool table=HouseholdActivitiesAvailable && DiningSeat(actor.x,actor.y);int relief=HouseholdActivitiesAvailable && !table?HouseholdRules.SnackRelief(actor.endurance):HungerRules.MealRelief;
            actor.carriedFood--;actor.hunger=Math.Max(0,actor.hunger-relief);ClearFoodMemory(ref actor);if(HouseholdActivitiesAvailable)RecordHouseholdActivity(ref actor,table?HouseholdActivity.TableMeal:HouseholdActivity.Snack);actors[index]=actor;
        }
        private bool TryHouseholdRoutine(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(!HouseholdActivitiesAvailable)return false;ClearHouseholdActivity(ref actor);actors[index]=actor;
            if(!SafeNeedsAction(actor,threat) || AssignedVehicle(index)>=0 || EffectiveOccupationOf(actor)!=HumanOccupation.Consumer || actor.restockPhase!=RestockPhase.None || actor.leisurePhase!=HumanLeisurePhase.None || actor.shelterExitPhase!=ShelterExitPhase.None)return false;
            Room room=FindRoom(HouseAt(actor.x,actor.y));if(room==null)return false;
            bool meal=actor.hunger>=HungerRules.SeekAt && actor.carriedFood>0;
            if(meal && DiningSeat(actor.x,actor.y))return false; // The normal safe-meal action consumes the same single inventory item.
            var here=map.FurnitureRecord(actor.x,actor.y);bool seated=here!=null && WorldMap.IsSeat(here.kind);
            bool hungry=actor.hunger>=HungerRules.SeekAt;
            if(hungry && actor.carriedFood==0){ObserveStorageBoxes(ref actor);ObserveShelves(ref actor);actors[index]=actor;} // Visible food still takes priority over a recovery break.
            bool recoverHealth=actor.health<100 && !hungry;
            bool rest=!meal && (!hungry || !KnownRoomSupplies(actor,room)) && (EnergyRules.Remaining(actor)<=EnergyRules.SeekRestAt || seated && actor.energySpent>0 || recoverHealth || !hungry && seated && actor.happiness<0 && TelevisionFromSeat(actor,actor.x,actor.y));
            if(!meal && !rest)return false;
            HouseholdFurnitureDocument chosen=null;int chosenRoute=-1,best=int.MinValue,chosenX=0,chosenY=0;
            foreach(var f in map.HouseholdRecords)for(int offset=0;offset<WorldMap.FurnitureWidth(f);offset++){
                int seatX=f.x+offset,seatY=f.y;
                if(!room.Contains(Key(seatX,seatY)) || !WorldMap.IsSeat(f.kind) || meal && !DiningSeat(seatX,seatY) || !FixtureVisible(actor,seatX,seatY) || !SeatFreeFor(actor.id,seatX,seatY))continue;
                int length,route=ShelterRoute(actor,seatX,seatY,0,out length,confinedRoom:room);if(route<0)continue;
                // Each couch cushion is a real reachable seat; compare recovery including paid travel as before.
                int gain=HouseholdRules.RestGain(f.kind),recovery=(actor.energySpent+length+gain-1)/gain;
                int score=meal?-length:-(length+recovery)+(actor.happiness<0 && EnergyRules.Remaining(actor)>20 && TelevisionFromSeat(actor,seatX,seatY)?20:0);
                if(score>best){best=score;chosen=f;chosenX=seatX;chosenY=seatY;chosenRoute=route;}
            }
            if(chosen==null)return false;
            actor.shelterId=room.id;actor.shelterX=chosenX;actor.shelterY=chosenY;actor.shelterState=ShelterState.Inside;
            if(actor.x!=chosenX || actor.y!=chosenY){direction=chosenRoute;RecordHouseholdActivity(ref actor,meal?HouseholdActivity.GoingToMeal:HouseholdActivity.GoingToRest);actors[index]=actor;return true;}
            if(meal)return false;
            // Only actual use of a typed seat restores energy. A full-energy TV rest adds no extra hunger cost.
            bool recoveringNow=actor.energySpent>0 || recoverHealth;
            if(actor.energySpent>0)actor.energySpent=Math.Max(0,actor.energySpent-HouseholdRules.RestGain(chosen.kind));
            if(recoverHealth)actor.health=Math.Min(100,actor.health+ShelterRecoveryRules.HealPerRest);
            if(recoveringNow)actor.hunger=Math.Min(100,actor.hunger+ShelterRecoveryRules.HungerPerRest);
            RecordHouseholdActivity(ref actor,HouseholdActivity.Resting);actors[index]=actor;return true;
        }
        private void ValidateHouseholdActivities()
        {
            for(int i=0;i<count;i++){var a=actors[i];bool active=a.householdActivity!=HouseholdActivity.None || a.householdActivityTick!=0;
                if(a.householdActivity<HouseholdActivity.None || a.householdActivity>HouseholdActivity.Resting || active && (!DomesticLifeEnabled || !map.HouseholdLife || a.species!=Species.Human || a.health<=0 || a.householdActivity==HouseholdActivity.None || a.householdActivityTick<1 || a.householdActivityTick>Tick))throw new ArgumentException("Invalid household activity owner or clock.");
                if(HouseholdSeatAt(a.x,a.y))for(int j=0;j<i;j++)if(actors[j].x==a.x && actors[j].y==a.y)throw new ArgumentException("A furniture seat cannot hold multiple bodies.");
            }
        }
    }
}
