using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum WeaponActionKind { Pickup, Swing }
    public struct WeaponAction
    {
        public int actorId,targetId,x,y;public long tick;public WeaponActionKind kind;public bool hit;
    }
    public static class MeleeRules
    {
        public const int CooldownTicks=10, MaxPickups=4096;
        public static int HitsPerLevel(int skill){return 3+skill;} // Successful hits needed for the next level; higher levels take more practice.
        public static int Damage(ActorState actor){return 8+2*actor.strength+2*actor.bluntSkill;}
        public static int Accuracy(ActorState actor,ActorState target){return Math.Max(10,Math.Min(95,55+2*(actor.strength+actor.perception+actor.agility)+3*actor.bluntSkill-2*target.agility-actor.stress/5));}
    }
    public sealed partial class WorldMap
    {
        private readonly Dictionary<int,MeleeWeapon> weapons=new Dictionary<int,MeleeWeapon>();
        public bool HasWeapons { get { return weapons.Count!=0; } }
        public WeaponPickupDocument WeaponAt(int x,int y){MeleeWeapon weapon;return Contains(x,y) && weapons.TryGetValue(y*Width+x,out weapon)?new WeaponPickupDocument{x=x,y=y,weapon=weapon}:null;}
        public void SetWeapon(int x,int y,MeleeWeapon weapon)
        {
            if(weapon!=MeleeWeapon.Bat || !CanWalk(x,y) || Get(x,y)==Terrain.DoorOpen || Get(x,y)==Terrain.DoorClosed || (!weapons.ContainsKey(y*Width+x) && weapons.Count>=MeleeRules.MaxPickups))throw new ArgumentException("A bat needs a walkable, non-door cell and room in the pickup limit.");
            weapons[y*Width+x]=weapon;
        }
        internal void TakeWeapon(int x,int y){weapons.Remove(y*Width+x);}
        internal WeaponPickupDocument[] CaptureWeapons()
        {
            if(weapons.Count==0)return null;List<int> keys=new List<int>(weapons.Keys);keys.Sort();WeaponPickupDocument[] result=new WeaponPickupDocument[keys.Count];for(int i=0;i<keys.Count;i++)result[i]=new WeaponPickupDocument{x=keys[i]%Width,y=keys[i]/Width,weapon=weapons[keys[i]]};return result;
        }
        private void RestoreWeapons(MapDocument source)
        {
            if(source.schemaVersion<4){if(source.weapons!=null)throw new ArgumentException("Weapon pickups require a version-four map.");return;}
            if(source.weapons==null || source.weapons.Length==0 || source.weapons.Length>MeleeRules.MaxPickups)throw new ArgumentException("Invalid weapon pickup collection.");
            foreach(WeaponPickupDocument pickup in source.weapons){if(pickup==null || WeaponAt(pickup.x,pickup.y)!=null)throw new ArgumentException("Invalid or duplicate weapon pickup.");SetWeapon(pickup.x,pickup.y,pickup.weapon);}
        }
    }
    public sealed partial class World
    {
        private WeaponAction[] weaponActions;private int weaponActionCount;
        public int WeaponActionCount { get { return weaponActionCount; } }
        public WeaponAction WeaponActionAt(int index){if(index<0 || index>=weaponActionCount)throw new ArgumentOutOfRangeException("index");return weaponActions[index];}
        private void RecordWeaponAction(ActorState actor,int x,int y,WeaponActionKind kind,bool hit=false,int targetId=0)
        {
            if(weaponActions==null)weaponActions=new WeaponAction[Capacity];
            weaponActions[weaponActionCount++]=new WeaponAction{actorId=actor.id,targetId=targetId,x=x,y=y,tick=Tick,kind=kind,hit=hit}; // One transient record per actor action, consumed without changing saved state.
        }
        public WeaponPickupDocument[] WeaponSites(){return map.CaptureWeapons()??new WeaponPickupDocument[0];}
        public bool TryPlaceBat(int x,int y)
        {
            if(!MeleeCombat || !map.CanWalk(x,y) || IsDoor(x,y) || map.WeaponAt(x,y)!=null || WeaponSites().Length>=MeleeRules.MaxPickups)return false;
            map.SetWeapon(x,y,MeleeWeapon.Bat);return true;
        }
        private bool TryPickupBat(int index,ref ActorState actor,int threat)
        {
            if(actor.meleeWeapon!=MeleeWeapon.None || actor.turnAtTick!=0 || actor.freezeUntilTick>Tick || actor.reaction==HumanReaction.Flight || (threat>=0 && Math.Max(Math.Abs(actor.x-actors[threat].x),Math.Abs(actor.y-actors[threat].y))<=2))return false;
            WeaponPickupDocument best=null;int distance=2,key=int.MaxValue;
            for(int d=-1;d<8;d+=d<0?1:2)
            {
                int x=actor.x+(d<0?0:dx[d]),y=actor.y+(d<0?0:dy[d]);WeaponPickupDocument pickup=map.WeaponAt(x,y);if(pickup==null || !Visible(actor,new ActorState{x=x,y=y}))continue;int cost=d<0?0:1,k=Key(x,y);if(cost<distance || (cost==distance && k<key)){best=pickup;distance=cost;key=k;}
            }
            if(best==null)return false;map.TakeWeapon(best.x,best.y);actor.meleeWeapon=best.weapon;RecordWeaponAction(actor,best.x,best.y,WeaponActionKind.Pickup);ClearShelterDeparture(ref actor);actors[index]=actor;return true; // One physical, finite pickup consumes the opportunity.
        }
        private bool TrySeekBat(ActorState actor,int threat,out int direction)
        {
            direction=8;
            // Equipment is optional: food and shelter decisions already ran. Do not leave an assigned refuge, ignore danger, or use hidden map stock.
            if(actor.meleeWeapon!=MeleeWeapon.None || actor.turnAtTick!=0 || actor.shelterId!=0 || actor.stress!=0 || actor.freezeUntilTick>Tick || actor.reaction==HumanReaction.Flight || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || threat>=0)return false;
            int bestDistance=7,bestKey=int.MaxValue;
            for(int y=Math.Max(0,actor.y-6);y<=Math.Min(Height-1,actor.y+6);y++)for(int x=Math.Max(0,actor.x-6);x<=Math.Min(Width-1,actor.x+6);x++)
            {
                if(Math.Abs(x-actor.x)+Math.Abs(y-actor.y)<=1 || !Visible(actor,new ActorState{x=x,y=y}) || map.WeaponAt(x,y)==null)continue;
                // Route to a cardinal pickup neighbor, never onto a closed door or through a solid corner. Six steps is the maximum detour.
                for(int d=0;d<8;d+=2){int gx=x+dx[d],gy=y+dy[d];if(!map.CanWalk(gx,gy))continue;int length,route=ShelterRoute(actor,gx,gy,0,out length,walkOnly:true);int key=Key(x,y);if(route>=0 && length>0 && (length<bestDistance || (length==bestDistance && key<bestKey))){bestDistance=length;bestKey=key;direction=route;}}
            }
            return bestKey!=int.MaxValue;
        }
        private void SwingBat(int humanIndex,int zombieIndex)
        {
            ActorState human=actors[humanIndex],zombie=actors[zombieIndex];if(human.nextMeleeTick>Tick || human.turnAtTick!=0 || human.meleeWeapon!=MeleeWeapon.Bat || zombie.species!=Species.Zombie || zombie.health==0 || Math.Max(Math.Abs(human.x-zombie.x),Math.Abs(human.y-zombie.y))>1 || !Visible(human,zombie))return;
            human.nextMeleeTick=Tick+MeleeRules.CooldownTicks;actors[humanIndex]=human;
            bool hit=reactionRandom.Below(100)<MeleeRules.Accuracy(human,zombie);RecordWeaponAction(human,zombie.x,zombie.y,WeaponActionKind.Swing,hit,zombie.id);
            if(!hit)return; // A missed swing still consumes its action and cooldown.
            zombie.health=Math.Max(0,zombie.health-MeleeRules.Damage(human));
            if(CombatExperience && human.bluntSkill<10){human.bluntPractice++;if(human.bluntPractice>=MeleeRules.HitsPerLevel(human.bluntSkill)){human.bluntPractice=0;human.bluntSkill++;}actors[humanIndex]=human;} // Apply learning after damage: the next swing benefits, including when this hit was fatal.
            if(zombie.health==0){ClearPursuit(ref zombie);ClearHumanReaction(ref zombie);ClearSearchHearing(ref zombie);actors[zombieIndex]=zombie;return;}
            zombie.staggerUntilTick=Tick+HumanRules.ShoveTicks;
            int x=zombie.x+Math.Sign(zombie.x-human.x),y=zombie.y+Math.Sign(zombie.y-human.y);
            if((x!=zombie.x || y!=zombie.y) && ClearStep(zombie.x,zombie.y,x,y) && occupancy.Allows(Key(x,y),Species.Zombie)){occupancy.Change(Key(zombie.x,zombie.y),Species.Zombie,-1);zombie.x=x;zombie.y=y;occupancy.Change(Key(x,y),Species.Zombie,1);}
            actors[zombieIndex]=zombie;
        }
        private void ValidateMelee()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.meleeWeapon<MeleeWeapon.None || actor.meleeWeapon>MeleeWeapon.Bat || actor.bluntSkill<0 || actor.bluntSkill>10 || (!MeleeCombat && (actor.meleeWeapon!=MeleeWeapon.None || actor.bluntSkill!=0 || actor.nextMeleeTick!=0 || actor.health==0)))throw new ArgumentException("Invalid melee equipment or skill.");
                if(actor.bluntPractice<0 || actor.bluntPractice>=MeleeRules.HitsPerLevel(actor.bluntSkill) || (actor.bluntPractice!=0 && (!CombatExperience || actor.bluntSkill==10)))throw new ArgumentException("Invalid blunt practice progress.");
                if(actor.nextMeleeTick<0 || (actor.nextMeleeTick!=0 && (actor.species!=Species.Human || actor.meleeWeapon!=MeleeWeapon.Bat || actor.nextMeleeTick<=Tick || actor.nextMeleeTick-Tick>MeleeRules.CooldownTicks)))throw new ArgumentException("Invalid melee cooldown.");
                if(actor.health==0 && (actor.species!=Species.Zombie || actor.targetId!=0 || actor.staggerUntilTick!=0 || actor.heardSearchUntilTick!=0 || actor.heardDoorUntilTick!=0 || actor.heardWarningUntilTick!=0))throw new ArgumentException("Defeated zombie retained active behavior.");
            }
        }
    }
}
