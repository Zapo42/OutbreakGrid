using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum WeaponActionKind { Pickup, Swing }
    public struct WeaponAction
    {
        public int actorId,targetId,x,y;public long tick;public WeaponActionKind kind;public bool hit,broke;
    }
    public static class MeleeRules
    {
        public const int BatHits=12;
        public const int CooldownTicks=10, MaxPickups=4096, MaxBats=MaxPickups+Rules.DefaultCapacity;
        public static int HitsPerLevel(int skill){return 3+skill;} // Successful hits needed for the next level; higher levels take more practice.
        public static int Damage(ActorState actor){return 8+2*actor.strength+2*actor.bluntSkill;}
        public static int Accuracy(ActorState actor,ActorState target){return Math.Max(10,Math.Min(95,55+2*(actor.strength+actor.perception+actor.agility)+3*actor.bluntSkill-2*target.agility-actor.stress/5));}
    }
    public sealed partial class WorldMap
    {
        private readonly Dictionary<int,int> weapons=new Dictionary<int,int>();
        private readonly Dictionary<int,List<int>> weaponWear=new Dictionary<int,List<int>>();
        public bool HasWornWeapons { get { return weaponWear.Count!=0; } }
        private int[] CaptureWear(int key){List<int> values;return weaponWear.TryGetValue(key,out values)?values.ToArray():null;}
        internal int GroundBatCount { get { int total=0;foreach(int quantity in weapons.Values)total+=quantity;return total; } }
        public bool HasDroppedWeapons { get { if(weapons.Count>MeleeRules.MaxPickups)return true;foreach(KeyValuePair<int,int> pile in weapons)if(pile.Value>1 || Get(pile.Key%Width,pile.Key/Width)==Terrain.DoorOpen || Get(pile.Key%Width,pile.Key/Width)==Terrain.DoorClosed)return true;return false; } }
        public bool HasWeapons { get { return weapons.Count!=0; } }
        public WeaponPickupDocument WeaponAt(int x,int y){int quantity;return Contains(x,y) && weapons.TryGetValue(y*Width+x,out quantity)?new WeaponPickupDocument{x=x,y=y,weapon=MeleeWeapon.Bat,extraBats=quantity-1,wear=CaptureWear(y*Width+x)}:null;}
        public void SetWeapon(int x,int y,MeleeWeapon weapon)
        {
            if(weapon!=MeleeWeapon.Bat || !CanWalk(x,y) || Get(x,y)==Terrain.DoorOpen || Get(x,y)==Terrain.DoorClosed || (!weapons.ContainsKey(y*Width+x) && weapons.Count>=MeleeRules.MaxPickups))throw new ArgumentException("A bat needs a walkable, non-door cell and room in the pickup limit.");
            weapons[y*Width+x]=1;weaponWear.Remove(y*Width+x);
        }
        internal int TakeWeapon(int x,int y)
        {
            int key=y*Width+x,wear=0;List<int> values;if(weaponWear.TryGetValue(key,out values)){wear=values[0];values.RemoveAt(0);if(values.Count==0 || values[values.Count-1]==0)weaponWear.Remove(key);}
            if(weapons[key]>1)weapons[key]--;else weapons.Remove(key);return wear;
        }
        internal void DropBat(int x,int y,int wear=0)
        {
            int key=y*Width+x,quantity;weapons.TryGetValue(key,out quantity);if(wear<0 || wear>=MeleeRules.BatHits || !Contains(x,y) || (!CanWalk(x,y) && Get(x,y)!=Terrain.DoorClosed) || ShelfAt(x,y)!=null || GroundBatCount>=MeleeRules.MaxBats)throw new ArgumentException("Invalid dropped bat cell or supply bound.");weapons[key]=quantity+1;
            List<int> values;if(weaponWear.TryGetValue(key,out values)){values.Add(wear);values.Sort();}else if(wear!=0){values=new List<int>();for(int i=0;i<quantity;i++)values.Add(0);values.Add(wear);weaponWear[key]=values;}
        }
        internal WeaponPickupDocument[] CaptureWeapons()
        {
            if(weapons.Count==0)return null;List<int> keys=new List<int>(weapons.Keys);keys.Sort();WeaponPickupDocument[] result=new WeaponPickupDocument[keys.Count];for(int i=0;i<keys.Count;i++)result[i]=new WeaponPickupDocument{x=keys[i]%Width,y=keys[i]/Width,weapon=MeleeWeapon.Bat,extraBats=weapons[keys[i]]-1,wear=CaptureWear(keys[i])};return result;
        }
        private void RestoreWeapons(MapDocument source)
        {
            if(source.schemaVersion<4){if(source.weapons!=null)throw new ArgumentException("Weapon pickups require a version-four map.");return;}
            if(source.weapons==null || source.weapons.Length==0 || source.weapons.Length>(source.schemaVersion>=5?MeleeRules.MaxBats:MeleeRules.MaxPickups))throw new ArgumentException("Invalid weapon pickup collection.");
            foreach(WeaponPickupDocument pickup in source.weapons)
            {
                if(pickup==null || pickup.weapon!=MeleeWeapon.Bat || pickup.extraBats<0 || pickup.extraBats>=MeleeRules.MaxBats || WeaponAt(pickup.x,pickup.y)!=null || (source.schemaVersion<5 && pickup.extraBats!=0))throw new ArgumentException("Invalid or duplicate weapon pickup.");
                if(source.schemaVersion<5)SetWeapon(pickup.x,pickup.y,pickup.weapon);else {if(!Contains(pickup.x,pickup.y) || (!CanWalk(pickup.x,pickup.y) && Get(pickup.x,pickup.y)!=Terrain.DoorClosed) || ShelfAt(pickup.x,pickup.y)!=null || GroundBatCount+pickup.Quantity>MeleeRules.MaxBats)throw new ArgumentException("Invalid dropped weapon pile.");weapons[pickup.y*Width+pickup.x]=pickup.Quantity;}
                if(pickup.wear!=null)
                {
                    if(source.schemaVersion<6 || pickup.wear.Length!=pickup.Quantity)throw new ArgumentException("Invalid weapon condition collection.");int previous=-1;foreach(int value in pickup.wear){if(value<0 || value>=MeleeRules.BatHits || value<previous)throw new ArgumentException("Invalid weapon wear.");previous=value;}if(previous==0)throw new ArgumentException("Fresh weapon piles omit condition data.");weaponWear[pickup.y*Width+pickup.x]=new List<int>(pickup.wear);
                }
            }
        }
    }
    public sealed partial class World
    {
        private WeaponAction[] weaponActions;private int weaponActionCount;
        public int WeaponActionCount { get { return weaponActionCount; } }
        public WeaponAction WeaponActionAt(int index){if(index<0 || index>=weaponActionCount)throw new ArgumentOutOfRangeException("index");return weaponActions[index];}
        private void RecordWeaponAction(ActorState actor,int x,int y,WeaponActionKind kind,bool hit=false,int targetId=0,bool broke=false)
        {
            if(weaponActions==null)weaponActions=new WeaponAction[Capacity];
            weaponActions[weaponActionCount++]=new WeaponAction{actorId=actor.id,targetId=targetId,x=x,y=y,tick=Tick,kind=kind,hit=hit,broke=broke}; // One transient record per actor action, consumed without changing saved state.
        }
        public WeaponPickupDocument[] WeaponSites(){return map.CaptureWeapons()??new WeaponPickupDocument[0];}
        public bool TryPlaceBat(int x,int y)
        {
            if(!MeleeCombat || !map.CanWalk(x,y) || IsDoor(x,y) || map.WeaponAt(x,y)!=null || WeaponSites().Length>=MeleeRules.MaxPickups || TotalBatCount()>=MeleeRules.MaxBats)return false;
            map.SetWeapon(x,y,MeleeWeapon.Bat);return true;
        }
        private bool TryPickupBat(int index,ref ActorState actor,int threat)
        {
            if(actor.meleeWeapon!=MeleeWeapon.None || actor.turnAtTick!=0 || actor.freezeUntilTick>Tick || actor.reaction==HumanReaction.Flight || (threat>=0 && Math.Max(Math.Abs(actor.x-actors[threat].x),Math.Abs(actor.y-actors[threat].y))<=2))return false;
            WeaponPickupDocument best=null;int distance=2,key=int.MaxValue;
            for(int d=-1;d<8;d+=d<0?1:2)
            {
                int x=actor.x+(d<0?0:dx[d]),y=actor.y+(d<0?0:dy[d]);WeaponPickupDocument pickup=map.WeaponAt(x,y);if(pickup==null || !map.CanWalk(x,y) || !Visible(actor,new ActorState{x=x,y=y}))continue;int cost=d<0?0:1,k=Key(x,y);if(cost<distance || (cost==distance && k<key)){best=pickup;distance=cost;key=k;}
            }
            if(best==null)return false;actor.batWear=map.TakeWeapon(best.x,best.y);actor.meleeWeapon=best.weapon;RecordWeaponAction(actor,best.x,best.y,WeaponActionKind.Pickup);ClearShelterDeparture(ref actor);actors[index]=actor;return true; // One physical, finite pickup consumes the opportunity.
        }
        private bool TrySeekBat(ActorState actor,int threat,out int direction)
        {
            direction=8;
            // Equipment is optional: food and shelter decisions already ran. Do not leave an assigned refuge, ignore danger, or use hidden map stock.
            if(actor.meleeWeapon!=MeleeWeapon.None || actor.turnAtTick!=0 || actor.shelterId!=0 || actor.stress!=0 || actor.freezeUntilTick>Tick || actor.reaction==HumanReaction.Flight || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || threat>=0)return false;
            int bestDistance=7,bestKey=int.MaxValue;
            for(int y=Math.Max(0,actor.y-6);y<=Math.Min(Height-1,actor.y+6);y++)for(int x=Math.Max(0,actor.x-6);x<=Math.Min(Width-1,actor.x+6);x++)
            {
                if(!map.CanWalk(x,y) || Math.Abs(x-actor.x)+Math.Abs(y-actor.y)<=1 || !Visible(actor,new ActorState{x=x,y=y}) || map.WeaponAt(x,y)==null)continue;
                // Route to a cardinal pickup neighbor, never onto a closed door or through a solid corner. Six steps is the maximum detour.
                for(int d=0;d<8;d+=2){int gx=x+dx[d],gy=y+dy[d];if(!map.CanWalk(gx,gy))continue;int length,route=ShelterRoute(actor,gx,gy,0,out length,walkOnly:true);int key=Key(x,y);if(route>=0 && length>0 && (length<bestDistance || (length==bestDistance && key<bestKey))){bestDistance=length;bestKey=key;direction=route;}}
            }
            return bestKey!=int.MaxValue;
        }
        private int CombatBackstep(ActorState actor,int threatIndex)
        {
            ActorState closest=actors[threatIndex];int distance=Math.Max(Math.Abs(actor.x-closest.x),Math.Abs(actor.y-closest.y));if(distance>2)return 8; // Hold useful distance while the bat is recovering; do not charge with an unavailable swing.
            int threats=KnownShelterThreats(actor,threatIndex),before=int.MaxValue;for(int j=0;j<threats;j++)before=Math.Min(before,Math.Max(Math.Abs(actor.x-escapeThreats[j].x),Math.Abs(actor.y-escapeThreats[j].y)));
            int direction=8,best=before,alignment=int.MinValue;
            for(int d=0;d<8;d++)
            {
                if(!MayMove(actor,d))continue;int x=actor.x+dx[d],y=actor.y+dy[d];if(dx[d]!=0 && dy[d]!=0 && (IsDoor(actor.x,actor.y) || IsDoor(x,y)))continue;
                int clearance=int.MaxValue;bool safe=true;for(int j=0;j<threats;j++){ActorState zombie=escapeThreats[j];int previous=Math.Max(Math.Abs(actor.x-zombie.x),Math.Abs(actor.y-zombie.y)),next=Math.Max(Math.Abs(x-zombie.x),Math.Abs(y-zombie.y));if(next<previous || (x-zombie.x)*(x-zombie.x)+(y-zombie.y)*(y-zombie.y)<DistanceSquared(actor,zombie)){safe=false;break;}clearance=Math.Min(clearance,next);}if(!safe || clearance<=before)continue;
                int away=((actor.x-closest.x)*dx[d]+(actor.y-closest.y)*dy[d])*(dx[d]!=0 && dy[d]!=0?707:1000);if(clearance>best || (clearance==best && away>alignment)){direction=d;best=clearance;alignment=away;}
            }
            return direction; // Stable ties, visible threats only, one normal neighboring step and no random draw.
        }
        private void SwingBat(int humanIndex,int zombieIndex)
        {
            ActorState human=actors[humanIndex],zombie=actors[zombieIndex];if(human.nextMeleeTick>Tick || human.turnAtTick!=0 || human.meleeWeapon!=MeleeWeapon.Bat || zombie.species!=Species.Zombie || zombie.health==0 || Math.Max(Math.Abs(human.x-zombie.x),Math.Abs(human.y-zombie.y))>1 || !Visible(human,zombie))return;
            human.nextMeleeTick=Tick+MeleeRules.CooldownTicks;actors[humanIndex]=human;
            bool hit=reactionRandom.Below(100)<MeleeRules.Accuracy(human,zombie);RecordWeaponAction(human,zombie.x,zombie.y,WeaponActionKind.Swing,hit,zombie.id,WeaponDurability && hit && human.batWear==MeleeRules.BatHits-1);
            if(!hit)return; // A missed swing still consumes its action and cooldown.
            zombie.health=Math.Max(0,zombie.health-MeleeRules.Damage(human));
            if(CombatExperience && human.bluntSkill<10){human.bluntPractice++;if(human.bluntPractice>=MeleeRules.HitsPerLevel(human.bluntSkill)){human.bluntPractice=0;human.bluntSkill++;}actors[humanIndex]=human;} // Apply learning after damage: the next swing benefits, including when this hit was fatal.
            if(WeaponDurability){human.batWear++;if(human.batWear==MeleeRules.BatHits){human.meleeWeapon=MeleeWeapon.None;human.batWear=0;}actors[humanIndex]=human;} // The final hit deals full damage; retain swing recovery even when the bat breaks.
            if(zombie.health==0){ClearPursuit(ref zombie);ClearHumanReaction(ref zombie);ClearSearchHearing(ref zombie);actors[zombieIndex]=zombie;return;}
            zombie.staggerUntilTick=Tick+HumanRules.ShoveTicks;
            int x=zombie.x+Math.Sign(zombie.x-human.x),y=zombie.y+Math.Sign(zombie.y-human.y);
            if((x!=zombie.x || y!=zombie.y) && ClearStep(zombie.x,zombie.y,x,y) && occupancy.Allows(Key(x,y),Species.Zombie)){occupancy.Change(Key(zombie.x,zombie.y),Species.Zombie,-1);zombie.x=x;zombie.y=y;occupancy.Change(Key(x,y),Species.Zombie,1);}
            actors[zombieIndex]=zombie;
        }
        private int TotalBatCount(){int total=map.GroundBatCount;for(int i=0;i<count;i++)if(actors[i].meleeWeapon==MeleeWeapon.Bat)total++;return total;}
        private void DropZombieBat(ref ActorState actor)
        {
            if(actor.species!=Species.Zombie || actor.meleeWeapon!=MeleeWeapon.Bat)return;map.DropBat(actor.x,actor.y,actor.batWear);actor.meleeWeapon=MeleeWeapon.None;actor.batWear=0;actor.nextMeleeTick=0; // A pile can hold multiple finite bats, including at a doorway.
        }
        private void ValidateMelee()
        {
            if(TotalBatCount()>MeleeRules.MaxBats)throw new ArgumentException("Too many finite bats in this run.");
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.meleeWeapon<MeleeWeapon.None || actor.meleeWeapon>MeleeWeapon.Bat || actor.bluntSkill<0 || actor.bluntSkill>10 || (!MeleeCombat && (actor.meleeWeapon!=MeleeWeapon.None || actor.bluntSkill!=0 || actor.nextMeleeTick!=0 || actor.health==0)))throw new ArgumentException("Invalid melee equipment or skill.");
                if(actor.batWear<0 || actor.batWear>=MeleeRules.BatHits || (actor.batWear!=0 && (!WeaponDurability || actor.meleeWeapon!=MeleeWeapon.Bat)))throw new ArgumentException("Invalid equipped bat wear.");
                if(actor.bluntPractice<0 || actor.bluntPractice>=MeleeRules.HitsPerLevel(actor.bluntSkill) || (actor.bluntPractice!=0 && (!CombatExperience || actor.bluntSkill==10)))throw new ArgumentException("Invalid blunt practice progress.");
                if(actor.nextMeleeTick<0 || (actor.nextMeleeTick!=0 && (actor.species!=Species.Human || (actor.meleeWeapon!=MeleeWeapon.Bat && !WeaponDurability) || actor.nextMeleeTick<=Tick || actor.nextMeleeTick-Tick>MeleeRules.CooldownTicks)))throw new ArgumentException("Invalid melee cooldown.");
                if(actor.health==0 && (actor.species!=Species.Zombie || actor.targetId!=0 || actor.staggerUntilTick!=0 || actor.heardSearchUntilTick!=0 || actor.heardDoorUntilTick!=0 || actor.heardWarningUntilTick!=0))throw new ArgumentException("Defeated zombie retained active behavior.");
            }
        }
    }
}
