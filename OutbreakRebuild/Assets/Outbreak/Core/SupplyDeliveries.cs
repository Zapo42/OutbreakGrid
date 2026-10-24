using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private Room DeliverySourceRoom(ActorState actor)
        {
            if(actor.restockSourceBox<=0)return null;foreach(Room room in rooms)if(room.Contains(actor.restockSourceBox-1))return room;return null;
        }
        private bool DeliverySourceDepartureAllowed(ActorState actor,Room room,int threat)
        {
            return SupplyDeliveriesEnabled && actor.restockSourceBox>0 && actor.restockPhase==RestockPhase.Returning && room.id!=actor.restockHomeId && room.Contains(actor.restockSourceBox-1) && SafeRestock(actor,threat) && EnergyRules.Remaining(actor)>RestockRules.ReturnEnergy;
        }
        private StorageBoxDocument ChooseDeliverySource(ActorState actor,Room home)
        {
            StorageBoxDocument best=null;int steps=int.MaxValue;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                int food,space;if((DeliveryControlsEnabled && box.blockDonations) || home.Contains(Key(box.x,box.y)) || !KnownStorageState(actor,box,out food,out space) || food<=EffectiveFoodReserve(box))continue;
                Room sourceRoom=null;foreach(Room room in rooms)if(room.Contains(Key(box.x,box.y))){sourceRoom=room;break;}
                if(sourceRoom==null || sourceRoom.id==actor.rejectedShelterId || KnownRoomDanger(actor,sourceRoom) || RoomAdmissions(sourceRoom,actor.id)>=sourceRoom.Capacity)continue;
                for(int d=0;d<8;d+=2)
                {
                    int x=box.x+dx[d],y=box.y+dy[d],length;if(!map.CanWalk(x,y) || !sourceRoom.Contains(Key(x,y)))continue;
                    if(ShelterRoute(actor,x,y,0,out length,foodRoute:true)>=0 && length<steps){best=box;steps=length;}
                }
            }
            return best; // Only personally known or reported surplus may start a delivery; stable cell order breaks route ties.
        }
        private bool TryCollectDeliveryFood(int index,ref ActorState actor,Room home,int threat,out int direction)
        {
            direction=8;int key=actor.restockSourceBox-1;StorageBoxDocument box=map.StorageBoxAt(key%Width,key/Width);Room room=DeliverySourceRoom(actor);
            if(box==null || (DeliveryControlsEnabled && box.blockDonations) || room==null || room.id==actor.rejectedShelterId || KnownRoomDanger(actor,room) || (!room.Contains(Key(actor.x,actor.y)) && RoomAdmissions(room,actor.id)>=room.Capacity))return false;
            int knownFood,knownSpace;if(KnownStorageState(actor,box,out knownFood,out knownSpace) && knownFood>=0 && knownFood<=EffectiveFoodReserve(box))return false;
            int best=int.MaxValue,goal=-1;
            for(int d=0;d<8;d+=2)
            {
                int x=box.x+dx[d],y=box.y+dy[d],length;if(!map.CanWalk(x,y) || !room.Contains(Key(x,y)))continue;
                int route=ShelterRoute(actor,x,y,KnownShelterThreats(actor,threat),out length,foodRoute:true);
                if(route>=0 && length<best){best=length;goal=Key(x,y);direction=route;}
            }
            if(goal<0)return false;
            actor.shelterId=room.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=room.Contains(Key(actor.x,actor.y))?ShelterState.Inside:ShelterState.Seeking;
            if(best>0)
            {
                int x=actor.x+dx[direction],y=actor.y+dy[direction];actors[index]=actor;
                if(map.Get(x,y)==Terrain.DoorClosed)
                {
                    SetDoor(x,y,true);direction=8;
                    int observed=NearestVisible(actor,Species.Zombie);
                    if(observed>=0){actor.stress=Math.Min(100,actor.stress+HumanRules.CloseStressGain);actor.threatPressure=ObservePressure(actor,actors[observed]);actor.reaction=HumanReaction.Flight;RejectShelter(ref actor,room.id);BeginRestockReturn(index,ref actor,home,observed);}
                }
                actors[index]=actor;return true;
            }
            int take=Math.Min(HungerRules.CarryLimit-actor.carriedFood,Math.Max(0,StorageRules.Count(box,InventoryItem.Food)-EffectiveFoodReserve(box)));
            if(take<=0){RememberStorage(ref actor,StorageObservation(box));actors[index]=actor;return false;}
            StorageBoxDocument updated=StorageRules.Copy(box);StorageRules.ChangeFood(updated,-take);map.SetStorageBox(updated);RememberStorage(ref actor,StorageObservation(updated));actor.carriedFood+=take;actor.restockPhase=RestockPhase.Returning;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true; // Recheck actual surplus for each pickup, protecting the donor even under concurrent visits.
        }
        private void FinishSupplyDoorMove(int index)
        {
            ActorState actor=actors[index];if(actor.restockPhase!=RestockPhase.Returning || actor.restockSourceBox==0)return;Room room=DeliverySourceRoom(actor);if(room==null || room.Contains(Key(actor.x,actor.y)))return;
            foreach(int key in room.doors){int x=key%Width,y=key/Width;if(Math.Abs(actor.x-x)+Math.Abs(actor.y-y)==1 && map.Get(x,y)==Terrain.DoorOpen && !DoorOccupied(x,y))SetDoor(x,y,false);}
        }
    }
}
