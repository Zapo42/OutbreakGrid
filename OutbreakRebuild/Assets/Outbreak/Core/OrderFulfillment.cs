using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private readonly SortedDictionary<int,SupplyOrderDocument> completedSupplyOrders=new SortedDictionary<int,SupplyOrderDocument>();
        private SupplyOrderDocument ActiveSupplyOrder(ActorState actor)
        {
            if(!OrderFulfillmentEnabled || actor.courierOrderId==0)return null;
            foreach(SupplyOrderDocument order in supplyOrders.Values)if(order.id==actor.courierOrderId)return order;return null;
        }
        public int SupplyOrderCourier(int orderId)
        {
            for(int i=0;i<count;i++)if(actors[i].species==Species.Human && actors[i].health>0 && actors[i].courierOrderId==orderId && actors[i].restockPhase!=RestockPhase.None)return actors[i].id;return 0;
        }
        private bool StartSupplyOrderTrip(int index,ref ActorState actor,int threat)
        {
            if(!OrderFulfillmentEnabled || actor.distributionCenterId==0 || !CourierWorkingNow(actor) || !SafeRestock(actor,threat) || EnergyRules.Remaining(actor)<RestockRules.DepartureEnergy || actor.carriedFood>FoodReserve(actor) || actor.foodExploreRoomId!=0)return false;
            Room current=FindRoom(actor.shelterId);if(current==null || !current.Contains(Key(actor.x,actor.y)))return false;
            int key=actor.courierSourceBox-1,food,space;StorageBoxDocument source=map.StorageBoxAt(key%Width,key/Width);
            if(source==null || source.blockDonations || (KnownStorageState(actor,source,out food,out space) && food>=0 && food<=EffectiveFoodReserve(source)))return false;
            SupplyOrderDocument chosen=null;
            foreach(SupplyOrderDocument order in supplyOrders.Values)
            {
                if(order.supplyBox==actor.courierSourceBox || SupplyOrderCourier(order.id)!=0 || RestockClaimed(order.supplyBox,actor.id))continue;
                int target=order.supplyBox-1,goal,length;Room room=FindRoom(order.storeId);StorageBoxDocument box=map.StorageBoxAt(target%Width,target/Width);
                if(box==null || (KnownStorageState(actor,box,out food,out space) && space==0) || !FoodReturnGoal(actor,room,KnownShelterThreats(actor,threat),out goal,out length))continue;
                if(chosen==null || order.createdTick<chosen.createdTick || (order.createdTick==chosen.createdTick && order.id<chosen.id))chosen=order;
            }
            if(chosen==null)return false;
            actor.courierOrderId=chosen.id;actor.courierReturningHome=false;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);
            actor.restockSourceBox=actor.courierSourceBox;actor.restockBox=chosen.supplyBox;actor.restockHomeId=chosen.storeId;actor.restockPhase=RestockPhase.Collecting;actor.restockUntilTick=Tick+CourierRules.TaskTicks;actors[index]=actor;return true;
        }
        private bool OrderCourierWaitingAtDoor(Room room,int door)
        {
            if(!OrderFulfillmentEnabled || room==null)return false;
            for(int i=0;i<count;i++){ActorState courier=actors[i];if(courier.health>0 && courier.species==Species.Human && courier.courierOrderId!=0 && courier.restockPhase==RestockPhase.Returning && courier.restockHomeId==room.id && !room.Contains(Key(courier.x,courier.y)) && Math.Abs(courier.x-door%Width)+Math.Abs(courier.y-door/Width)<=1)return true;}
            return false;
        }
        private void CreditSupplyDelivery(ActorState actor,int box,int amount)
        {
            SupplyOrderDocument order=ActiveSupplyOrder(actor);if(order==null || order.supplyBox!=box || actor.restockPhase!=RestockPhase.Returning || amount<=0)return;
            order.deliveredMeals+=amount;
            if(order.deliveredMeals==order.requestedMeals)
            {
                completedSupplyOrders.Add(order.id,CopyOrder(order));supplyOrders.Remove(order.storeId);
                if(completedSupplyOrders.Count>64){int oldest=0;foreach(int id in completedSupplyOrders.Keys){oldest=id;break;}completedSupplyOrders.Remove(oldest);}
            }
        }
        public SupplyOrderDocument[] CompletedSupplyOrders(){return CaptureCompletedSupplyOrders()??new SupplyOrderDocument[0];}
        private SupplyOrderDocument[] CaptureCompletedSupplyOrders()
        {
            if(completedSupplyOrders.Count==0)return null;SupplyOrderDocument[] result=new SupplyOrderDocument[completedSupplyOrders.Count];int i=0;foreach(SupplyOrderDocument order in completedSupplyOrders.Values)result[i++]=CopyOrder(order);return result;
        }
        private void RestoreCompletedSupplyOrders(SupplyOrderDocument[] records)
        {
            if(records==null)return;if(!OrderFulfillmentEnabled || records.Length<1 || records.Length>64)throw new ArgumentException("Invalid completed supply orders.");int previous=0;
            foreach(SupplyOrderDocument order in records)
            {
                bool caller=false;if(order!=null)for(int i=0;i<count;i++)if(actors[i].id==order.requestedBy)caller=true;
                int phone=order==null?-1:order.phone-1,box=order==null?-1:order.supplyBox-1;
                if(order==null || order.id<=previous || order.id>=nextSupplyOrderId || order.requestedMeals<1 || order.requestedMeals>StorageRules.MaxCapacity || order.deliveredMeals!=order.requestedMeals || !caller || phone<0 || box<0 || !map.PhoneAt(phone%Width,phone/Width) || !stores.ContainsKey(order.storeId) || !FindRoom(order.storeId).Contains(phone) || !FindRoom(order.storeId).Contains(box) || map.StorageBoxAt(box%Width,box/Width)==null || order.createdTick<0 || order.createdTick>Tick)throw new ArgumentException("Invalid completed supply order.");
                foreach(SupplyOrderDocument pending in supplyOrders.Values)if(pending.id==order.id)throw new ArgumentException("Duplicate supply-order identity.");completedSupplyOrders.Add(order.id,CopyOrder(order));previous=order.id;
            }
        }
        private void ValidateCourierOrders()
        {
            HashSet<int> claimed=new HashSet<int>();
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.courierOrderId==0)continue;SupplyOrderDocument order=ActiveSupplyOrder(actor);
                if(!OrderFulfillmentEnabled || order==null || !claimed.Add(actor.courierOrderId) || actor.species!=Species.Human || actor.health<=0 || actor.distributionCenterId==0 || actor.restockPhase==RestockPhase.None || actor.restockSourceBox!=actor.courierSourceBox || actor.restockBox!=order.supplyBox || actor.restockHomeId!=order.storeId)throw new ArgumentException("Invalid courier order claim.");
            }
        }
    }
}
