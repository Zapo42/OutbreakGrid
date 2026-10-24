using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class World
    {
        private readonly SortedDictionary<int,SupplyOrderDocument> supplyOrders=new SortedDictionary<int,SupplyOrderDocument>();
        private int nextSupplyOrderId=1;
        private static void ResetClerkCall(ref ActorState actor){actor.clerkPhone=0;actor.clerkCallProgress=0;}
        private static SupplyOrderDocument CopyOrder(SupplyOrderDocument order){return new SupplyOrderDocument{id=order.id,storeId=order.storeId,supplyBox=order.supplyBox,requestedMeals=order.requestedMeals,requestedBy=order.requestedBy,phone=order.phone,createdTick=order.createdTick,deliveredMeals=order.deliveredMeals};}
        public SupplyOrderDocument[] PendingSupplyOrders(){return CaptureSupplyOrders()??new SupplyOrderDocument[0];}
        private SupplyOrderDocument[] CaptureSupplyOrders()
        {
            if(supplyOrders.Count==0)return null;SupplyOrderDocument[] result=new SupplyOrderDocument[supplyOrders.Count];int i=0;foreach(var pair in supplyOrders)result[i++]=CopyOrder(pair.Value);return result;
        }
        private void InvalidateStoreOrder(int storeId)
        {
            SupplyOrderDocument cancelled;if(supplyOrders.TryGetValue(storeId,out cancelled))for(int i=0;i<count;i++)if(actors[i].courierOrderId==cancelled.id){ActorState courier=actors[i];ClearRestocking(ref courier);ClearFoodMemory(ref courier);ClearShelterDeparture(ref courier);actors[i]=courier;}
            supplyOrders.Remove(storeId);for(int i=0;i<count;i++)if(actors[i].clerkStoreId==storeId){ActorState actor=actors[i];actor.clerkNeedsSupplies=false;ResetClerkCall(ref actor);actors[i]=actor;}
        }
        private void UpdateClerkSupplyKnowledge(ref ActorState actor,StoreDocument store)
        {
            if(supplyOrders.ContainsKey(store.storeId)){actor.clerkNeedsSupplies=false;ResetClerkCall(ref actor);return;}
            StorageBoxDocument crate=map.StorageBoxAt((store.supplyBox-1)%Width,(store.supplyBox-1)/Width);int food,space;
            if(KnownStorageState(actor,crate,out food,out space) && food>=0)
            {
                actor.clerkNeedsSupplies=food==0;if(food>0)ResetClerkCall(ref actor);
            }
        }
        private bool TryClerkPhoneCall(int index,ref ActorState actor,StoreDocument store,Room room,out int direction)
        {
            direction=8;if(!actor.clerkNeedsSupplies || supplyOrders.ContainsKey(store.storeId))return false;
            int best=int.MaxValue,face=-1,key=-1,route=-1;
            foreach(PhoneBlockDocument phone in PhoneBlocks())
            {
                if(!room.Contains(Key(phone.x,phone.y)))continue;int candidateFace,length,candidate=StoreFurnitureRoute(actor,room,phone.x,phone.y,out candidateFace,out length);
                if(candidate>=0 && length<best){best=length;face=candidateFace;key=Key(phone.x,phone.y)+1;route=candidate;}
            }
            if(key<0){ResetClerkCall(ref actor);actors[index]=actor;return false;}
            if(actor.clerkPhone!=key){ResetClerkCall(ref actor);actor.clerkPhone=key;}
            actor.shelterId=room.id;actor.shelterX=face%Width;actor.shelterY=face/Width;actor.shelterState=ShelterState.Inside;ClearShelterDeparture(ref actor);
            if(best>0){actor.clerkCallProgress=0;direction=route;actors[index]=actor;return true;}
            if(actor.clerkCallProgress==0){actor.clerkCallProgress=1;actors[index]=actor;return true;}
            StorageBoxDocument crate=map.StorageBoxAt((store.supplyBox-1)%Width,(store.supplyBox-1)/Width);
            if(nextSupplyOrderId<int.MaxValue)supplyOrders.Add(store.storeId,new SupplyOrderDocument{id=nextSupplyOrderId++,storeId=store.storeId,supplyBox=store.supplyBox,requestedMeals=crate.capacity,requestedBy=actor.id,phone=key,createdTick=Tick});
            actor.clerkNeedsSupplies=false;ResetClerkCall(ref actor);actors[index]=actor;return true;
        }
        private void RestoreSupplyOrders(SupplyOrderDocument[] records,int next)
        {
            if(next<0 || (!SupplyRequestsEnabled && next!=0))throw new ArgumentException("Invalid supply-order counter.");nextSupplyOrderId=next==0?1:next;
            if(records==null)return;if(!SupplyRequestsEnabled || records.Length<1 || records.Length>ShelterRules.MaxRooms)throw new ArgumentException("Invalid supply orders.");int previous=0;HashSet<int> ids=new HashSet<int>();
            foreach(SupplyOrderDocument order in records)
            {
                StoreDocument store;bool caller=false;if(order!=null)for(int i=0;i<count;i++)if(actors[i].id==order.requestedBy)caller=true;
                int phone=order==null?-1:order.phone-1;
                if(order==null || order.storeId<=previous || !stores.TryGetValue(order.storeId,out store) || order.supplyBox!=store.supplyBox || order.id<1 || order.id>=nextSupplyOrderId || !ids.Add(order.id) || !caller || phone<0 || !map.PhoneAt(phone%Width,phone/Width) || !FindRoom(store.storeId).Contains(phone) || order.createdTick<0 || order.createdTick>Tick)throw new ArgumentException("Invalid supply order.");
                StorageBoxDocument box=map.StorageBoxAt((order.supplyBox-1)%Width,(order.supplyBox-1)/Width);if(order.requestedMeals<1 || order.requestedMeals>box.capacity || order.deliveredMeals<0 || order.deliveredMeals>=order.requestedMeals || (!OrderFulfillmentEnabled && order.deliveredMeals!=0))throw new ArgumentException("Invalid requested quantity.");supplyOrders.Add(order.storeId,CopyOrder(order));previous=order.storeId;
            }
        }
        private void ValidateClerkCalls()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(!actor.clerkNeedsSupplies && actor.clerkPhone==0 && actor.clerkCallProgress==0)continue;
                if(!SupplyRequestsEnabled || actor.clerkStoreId==0 || actor.clerkCallProgress<0 || actor.clerkCallProgress>1 || (!actor.clerkNeedsSupplies && actor.clerkPhone!=0) || (actor.clerkPhone==0 && actor.clerkCallProgress!=0))throw new ArgumentException("Invalid clerk phone task.");
                if(actor.clerkPhone!=0){int key=actor.clerkPhone-1;if(key<0 || !map.PhoneAt(key%Width,key/Width) || !FindRoom(actor.clerkStoreId).Contains(key) || (actor.clerkCallProgress!=0 && Math.Abs(actor.x-key%Width)+Math.Abs(actor.y-key/Width)!=1))throw new ArgumentException("Invalid clerk phone location.");}
            }
        }
    }
}
