using System; // Reject malformed walk state before a loaded world replaces the running one.
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class HumanLeisureRules
    {
        public const int Radius=12,MinimumEnergy=80,MaxHunger=25,PauseTicks=20,MaxTripTicks=600,InitialDelayTicks=300,CooldownTicks=600,StaggerTicks=120;
    }
    public sealed partial class World
    {
        // Reuse a fixed local search window. Walks do not allocate per actor or query an entire city.
        private const int LeisureSide=HumanLeisureRules.Radius*2+1,LeisureCells=LeisureSide*LeisureSide;
        private int[] leisureQueue,leisureSeen,leisureFirst,leisureDistance;private int leisureStamp;
        private static void ClearLeisure(ref ActorState actor)
        {
            actor.leisureParkVisit=false;actor.leisurePhase=HumanLeisurePhase.None;actor.leisureDoor=actor.leisureHomeCell=actor.leisureX=actor.leisureY=0;actor.nextLeisureTick=actor.leisureUntilTick=0;
        }
        private void EndLeisure(ref ActorState actor)
        {
            ClearLeisure(ref actor);actor.nextLeisureTick=Tick+HumanLeisureRules.CooldownTicks+actor.id%HumanLeisureRules.StaggerTicks;
        }
        private bool LeisureSafe(ActorState actor,int threat,bool starting)
        {
            // Scheduled workers share consumer leisure off duty while retaining their permanent employment.
            return actor.species==Species.Human && actor.health==100 && actor.turnAtTick==0 && actor.grappledById==0 && actor.stress==0 && threat<0 && actor.freezeUntilTick<=Tick && !HasRecentWarning(actor) && !HasRecentDoorNoise(actor) && actor.assignedHouseId!=0 && EffectiveOccupationOf(actor)==HumanOccupation.Consumer && actor.workerCommuteId==0 && actor.restockPhase==RestockPhase.None && !IsShopping(actor) && actor.foodUntilTick==0 && actor.foodExploreRoomId==0 && actor.homeShelterId==0 && actor.hunger<(starting?HumanLeisureRules.MaxHunger:35) && EnergyRules.Remaining(actor)>=(starting?HumanLeisureRules.MinimumEnergy:40);
        }
        private void AdvanceLeisure(ref ActorState actor)
        {
            // Needs advance even while a grapple suppresses movement decisions; a bitten or injured walker stops immediately.
            if(actor.leisurePhase!=HumanLeisurePhase.None && !LeisureSafe(actor,-1,false))
            {
                if(actor.species!=Species.Human || actor.health<=0 || actor.assignedHouseId==0)ClearLeisure(ref actor);else EndLeisure(ref actor);
                ClearShelterDeparture(ref actor);
            }
        }
        private void InterruptLeisure(int index,ref ActorState actor,int threat)
        {
            if(actor.leisurePhase==HumanLeisurePhase.None)return;
            Room home=FindRoom(actor.assignedHouseId);
            if(home==null || !home.Contains(actor.leisureHomeCell-1) || !LeisureSafe(actor,threat,false) || actor.leisureUntilTick<=Tick && actor.leisurePhase!=HumanLeisurePhase.Pausing)
            {
                EndLeisure(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;
            }
        }
        public string LeisureStatus(int index)
        {
            ActorState actor=ActorAt(index);if(!LeisureEnabled)return "Leisure unavailable: older rules";
            if(actor.leisureParkVisit){switch(actor.leisurePhase){case HumanLeisurePhase.Outbound:return "Walking to known park";case HumanLeisurePhase.Pausing:return "Visiting park";case HumanLeisurePhase.Returning:return "Returning home from park";}}
            switch(actor.leisurePhase){case HumanLeisurePhase.Outbound:return "Taking a walk";case HumanLeisurePhase.Pausing:return "Pausing outside";case HumanLeisurePhase.Returning:return "Returning home";}
            return actor.species==Species.Human && actor.health>0 && actor.assignedHouseId!=0 && HouseAt(actor.x,actor.y)==actor.assignedHouseId && EffectiveOccupationOf(actor)==HumanOccupation.Consumer?"Free time: at home":"Waiting for free time";
        }
        private bool LeisureOutdoor(int x,int y)
        {
            if(!map.Contains(x,y) || HouseAt(x,y)!=0)return false;Terrain terrain=map.Get(x,y);
            return terrain>=Terrain.Grass && terrain<=Terrain.Sidewalk || terrain==Terrain.Crosswalk;
        }
        private bool LeisureCell(ActorState actor,Room home,int x,int y)
        {
            if(!map.Contains(x,y))return false;int key=Key(x,y);
            if(key==actor.leisureDoor-1)return IsDoor(x,y);
            return map.CanWalk(x,y) && (home.Contains(key) || LeisureOutdoor(x,y));
        }
        private int LeisureSearch(ActorState actor,Room home,bool chooseTarget,out int goal)
        {
            goal=-1;int door=actor.leisureDoor-1,cx=door%Width,cy=door/Width,r=HumanLeisureRules.Radius;
            if(Math.Abs(actor.x-cx)>r || Math.Abs(actor.y-cy)>r)return -1;
            if(leisureQueue==null){leisureQueue=new int[LeisureCells];leisureSeen=new int[LeisureCells];leisureFirst=new int[LeisureCells];leisureDistance=new int[LeisureCells];}
            if(leisureStamp==int.MaxValue){Array.Clear(leisureSeen,0,leisureSeen.Length);leisureStamp=0;}int stamp=++leisureStamp;
            int start=(actor.y-cy+r)*LeisureSide+actor.x-cx+r,head=0,tail=0,best=int.MaxValue;leisureQueue[tail++]=start;leisureSeen[start]=stamp;leisureFirst[start]=8;leisureDistance[start]=0;
            int target=actor.leisurePhase==HumanLeisurePhase.Returning?actor.leisureHomeCell-1:Key(actor.leisureX,actor.leisureY);
            while(head<tail)
            {
                int cell=leisureQueue[head++],x=cx+cell%LeisureSide-r,y=cy+cell/LeisureSide-r,key=Key(x,y),distance=leisureDistance[cell];
                if(!chooseTarget && key==target){goal=key;return leisureFirst[cell];}
                if(chooseTarget && LeisureOutdoor(x,y) && (KnowsPark(actor,key) || Math.Abs(x-cx)+Math.Abs(y-cy)>=4) && distance<=20)
                {
                    // Prefer a modest six-cell radius; stable identity/time-based ties vary destinations without changing simulation RNG.
                    int score=Math.Abs(Math.Abs(x-cx)+Math.Abs(y-cy)-6)*1024+(int)(((uint)x*73856093u^(uint)y*19349663u^(uint)actor.id*83492791u^(uint)Tick*2654435761u)%1024);
                    if(KnowsPark(actor,key))score=-32768+distance*1024; // Prefer a nearby reachable familiar venue without querying remote people or stock.
                    if(score<best){best=score;goal=key;}
                }
                for(int d=0;d<8;d+=2)
                {
                    int nx=x+dx[d],ny=y+dy[d];if(Math.Abs(nx-cx)>r || Math.Abs(ny-cy)>r || !LeisureCell(actor,home,nx,ny) || !occupancy.Allows(Key(nx,ny),Species.Human) || VehicleAt(nx,ny))continue;
                    int next=(ny-cy+r)*LeisureSide+nx-cx+r;if(leisureSeen[next]==stamp)continue;leisureSeen[next]=stamp;leisureDistance[next]=distance+1;leisureFirst[next]=cell==start?d:leisureFirst[cell];leisureQueue[tail++]=next;
                }
            }
            return chooseTarget && goal>=0?8:-1;
        }
        private bool StartLeisure(ref ActorState actor,Room home)
        {
            int best=int.MaxValue,door=-1;
            foreach(int key in home.doors)
            {
                int x=key%Width,y=key/Width,distance=Math.Abs(x-actor.x)+Math.Abs(y-actor.y);if(distance>=best || distance>HumanLeisureRules.Radius)continue;
                for(int d=0;d<8;d+=2)if(ExitGeometry(home,key+1,x+dx[d],y+dy[d]) && LeisureOutdoor(x+dx[d],y+dy[d])){best=distance;door=key;break;}
            }
            if(door<0)return false;
            actor.leisureDoor=door+1;actor.leisureHomeCell=Key(actor.x,actor.y)+1;int goal;
            if(LeisureSearch(actor,home,true,out goal)<0){EndLeisure(ref actor);return false;}
            actor.leisureParkVisit=KnowsPark(actor,goal);actor.leisureX=goal%Width;actor.leisureY=goal/Width;actor.leisurePhase=HumanLeisurePhase.Outbound;actor.leisureUntilTick=Tick+HumanLeisureRules.MaxTripTicks;actor.nextLeisureTick=0;
            ClearShelterDeparture(ref actor);actor.homeShelterId=0;actor.shelterId=home.id;actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;return true;
        }
        private bool TryLeisureRoutine(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;Room home=FindRoom(actor.assignedHouseId);
            if(actor.leisurePhase==HumanLeisurePhase.None)
            {
                if(home==null || !home.Contains(Key(actor.x,actor.y)) || !LeisureSafe(actor,threat,true))return false;
                if(actor.energySpent>0)return false; // Existing shelter rest replenishes movement energy before waiting or starting another walk.
                if(actor.nextLeisureTick==0){actor.nextLeisureTick=Tick+HumanLeisureRules.InitialDelayTicks+actor.id%HumanLeisureRules.StaggerTicks;actors[index]=actor;return true;}
                if(Tick<actor.nextLeisureTick)return true; // Stay home between deliberate walks rather than falling through to the old automatic departure.
                if(!StartLeisure(ref actor,home)){EndLeisure(ref actor);actors[index]=actor;return true;}
            }
            if(!LeisureSafe(actor,threat,false) || home==null){EndLeisure(ref actor);actors[index]=actor;return false;}
            if(actor.leisurePhase==HumanLeisurePhase.Outbound && actor.x==actor.leisureX && actor.y==actor.leisureY){actor.leisurePhase=HumanLeisurePhase.Pausing;actor.leisureUntilTick=Tick+(actor.leisureParkVisit?ParkVisitRules.StayTicks:HumanLeisureRules.PauseTicks);}
            if(actor.leisurePhase==HumanLeisurePhase.Pausing)
            {
                if(Tick<actor.leisureUntilTick){actors[index]=actor;return true;}
                actor.leisurePhase=HumanLeisurePhase.Returning;actor.leisureUntilTick=Tick+HumanLeisureRules.MaxTripTicks;
            }
            if(actor.leisurePhase==HumanLeisurePhase.Returning && Key(actor.x,actor.y)==actor.leisureHomeCell-1){CloseNearbyDoors(actor,home);EndLeisure(ref actor);actors[index]=actor;return true;}
            int goal;direction=LeisureSearch(actor,home,false,out goal);
            if(direction<0){EndLeisure(ref actor);actors[index]=actor;direction=8;return true;}
            if(direction<8)
            {
                int x=actor.x+dx[direction],y=actor.y+dy[direction];if(map.Get(x,y)==Terrain.DoorClosed){SetDoor(x,y,true);direction=8;}else if(!MayMove(actor,direction))direction=8;
            }
            actors[index]=actor;return true;
        }
        private void ValidateLeisure()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool active=actor.leisurePhase!=HumanLeisurePhase.None;bool any=active || actor.nextLeisureTick!=0 || actor.leisureUntilTick!=0 || actor.leisureDoor!=0 || actor.leisureHomeCell!=0 || actor.leisureX!=0 || actor.leisureY!=0;
                if(!any)continue;
                if(!LeisureEnabled || actor.species!=Species.Human || actor.health<=0 || actor.assignedHouseId==0 || actor.nextLeisureTick<0 || actor.nextLeisureTick>Tick+HumanLeisureRules.CooldownTicks+HumanLeisureRules.StaggerTicks || actor.leisurePhase<HumanLeisurePhase.None || actor.leisurePhase>HumanLeisurePhase.Returning)throw new ArgumentException("Invalid leisure identity or cooldown.");
                if(!active){if(actor.leisureUntilTick!=0 || actor.leisureDoor!=0 || actor.leisureHomeCell!=0 || actor.leisureX!=0 || actor.leisureY!=0)throw new ArgumentException("Inactive leisure contains stale geometry.");continue;}
                Room home=FindRoom(actor.assignedHouseId);int door=actor.leisureDoor-1;
                if(home==null || actor.nextLeisureTick!=0 || actor.leisureHomeCell<=0 || !home.Contains(actor.leisureHomeCell-1) || door<0 || Array.BinarySearch(home.doors,door)<0 || !LeisureOutdoor(actor.leisureX,actor.leisureY) || actor.leisureUntilTick<1 || actor.leisureUntilTick<Tick-HumanLeisureRules.MaxTripTicks || actor.leisureUntilTick>Tick+(actor.leisurePhase==HumanLeisurePhase.Pausing?(actor.leisureParkVisit?ParkVisitRules.StayTicks:HumanLeisureRules.PauseTicks):HumanLeisureRules.MaxTripTicks) || Math.Abs(actor.leisureX-door%Width)>HumanLeisureRules.Radius || Math.Abs(actor.leisureY-door/Width)>HumanLeisureRules.Radius || !LeisureCell(actor,home,actor.x,actor.y))throw new ArgumentException("Invalid leisure route or deadline.");
                if(actor.leisurePhase==HumanLeisurePhase.Pausing && (actor.x!=actor.leisureX || actor.y!=actor.leisureY))throw new ArgumentException("Leisure pause is not at its destination.");
            }
        }
    }
}
