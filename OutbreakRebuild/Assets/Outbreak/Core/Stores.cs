using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StoreRules
    {
        public const int ShelfTarget=6,MaxShelfTarget=30;
    }
    public struct StoreOverview
    {
        public int StoreId,SupplyBox,ShelfTarget,CrateMeals,Shelves,EmptyShelves,ShelfMeals,Clerks;
    }
    public sealed partial class World
    {
        private readonly SortedDictionary<int,StoreDocument> stores=new SortedDictionary<int,StoreDocument>();
        private static StoreDocument CopyStore(StoreDocument store){return new StoreDocument{storeId=store.storeId,supplyBox=store.supplyBox,shelfTarget=store.shelfTarget};}
        public bool TryConfigureStore(int x,int y,int shelfTarget=StoreRules.ShelfTarget)
        {
            int id=HouseAt(x,y);if(!StoresEnabled || id==0 || IsDistributionSupplyBox(x,y) || map.StorageBoxAt(x,y)==null || shelfTarget<1 || shelfTarget>StoreRules.MaxShelfTarget)return false;
            StoreDocument previous;if(stores.TryGetValue(id,out previous) && previous.supplyBox!=Key(x,y)+1)InvalidateStoreOrder(id);
            stores[id]=new StoreDocument{storeId=id,supplyBox=Key(x,y)+1,shelfTarget=shelfTarget};return true;
        }
        public bool TryAssignClerk(int index,int storeId)
        {
            StoreDocument store;if(!StoresEnabled || !CanEditCourier(index) || !stores.TryGetValue(storeId,out store))return false;
            ActorState actor=actors[index];if(actor.courierSourceBox!=0 || actor.clerkShelf!=0 || HouseAt(actor.x,actor.y)!=storeId)return false;
            ResetClerkCall(ref actor);actor.clerkNeedsSupplies=false;actor.clerkStoreId=storeId;actor.clerkShelf=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;return true;
        }
        public bool TryUnassignClerk(int index)
        {
            if(!StoresEnabled || !CanEditCourier(index) || actors[index].clerkStoreId==0)return false;
            ActorState actor=actors[index];ClearClerkAssignment(ref actor);actors[index]=actor;return true;
        }
        private static void ClearClerkAssignment(ref ActorState actor){ResetClerkCall(ref actor);actor.clerkNeedsSupplies=false;actor.clerkStoreId=0;actor.clerkShelf=0;}
        private bool IsStoreSupplyBox(StorageBoxDocument box)
        {
            if(!StoresEnabled || stores.Count==0)return false;int key=Key(box.x,box.y)+1;foreach(StoreDocument store in stores.Values)if(store.supplyBox==key)return true;return false;
        }
        private bool ClerkUsesSupply(ActorState actor,StorageBoxDocument box)
        {
            StoreDocument store;return StoresEnabled && stores.TryGetValue(actor.clerkStoreId,out store) && store.supplyBox==Key(box.x,box.y)+1;
        }
        private int StoreFurnitureRoute(ActorState actor,Room room,int x,int y,out int goal,out int length)
        {
            int direction=-1;goal=-1;length=int.MaxValue;
            for(int d=0;d<8;d+=2)
            {
                int nx=x+dx[d],ny=y+dy[d],steps;if(!map.CanWalk(nx,ny) || !room.Contains(Key(nx,ny)))continue;
                int route=ShelterRoute(actor,nx,ny,0,out steps,confinedRoom:room);if(route>=0 && steps<length){direction=route;length=steps;goal=Key(nx,ny);}
            }
            return direction;
        }
        private bool TryClerkWork(int index,ref ActorState actor,int threat,out int direction)
        {
            direction=8;if(actor.clerkStoreId==0)return false;StoreDocument store;Room room=FindRoom(actor.clerkStoreId);
            if(!stores.TryGetValue(actor.clerkStoreId,out store) || room==null)return false;
            if(!SafeRestock(actor,threat) || !room.Contains(Key(actor.x,actor.y)) || EnergyRules.Remaining(actor)<=RestockRules.ReturnEnergy)
            {
                ResetClerkCall(ref actor);actor.clerkShelf=0;actors[index]=actor;return false;
            }
            if(SupplyRequestsEnabled){UpdateClerkSupplyKnowledge(ref actor,store);actors[index]=actor;if(actor.carriedFood<=FoodReserve(actor) && TryClerkPhoneCall(index,ref actor,store,room,out direction))return true;}
            ShelfDocument target=actor.clerkShelf==0?null:map.ShelfAt((actor.clerkShelf-1)%Width,(actor.clerkShelf-1)/Width);
            if(target!=null && ShelfVisible(actor,target) && target.food>=store.shelfTarget){actor.clerkShelf=0;target=null;}
            if(target==null)
            {
                int best=int.MaxValue,key=int.MaxValue;
                foreach(ShelfDocument shelf in map.ShelfRecords)
                {
                    if(!room.Contains(Key(shelf.x,shelf.y)))continue;
                    int knownFood=-1;if(ShelfVisible(actor,shelf))knownFood=shelf.food;else if(OrderFulfillmentEnabled && actor.shelfMemory!=null)foreach(ShelfMemoryRecord note in actor.shelfMemory)if(note.x==shelf.x && note.y==shelf.y){knownFood=note.food;break;}
                    if(knownFood<0 || knownFood>=store.shelfTarget)continue;
                    int goal,length;if(StoreFurnitureRoute(actor,room,shelf.x,shelf.y,out goal,out length)<0)continue;
                    int candidate=Key(shelf.x,shelf.y);if(length<best || (length==best && candidate<key)){target=shelf;best=length;key=candidate;}
                }
                if(target==null){actor.clerkShelf=0;actors[index]=actor;return SupplyRequestsEnabled && TryClerkPhoneCall(index,ref actor,store,room,out direction);}
                actor.clerkShelf=Key(target.x,target.y)+1;
            }
            bool stocking=actor.carriedFood>FoodReserve(actor);int x=stocking?target.x:(store.supplyBox-1)%Width,y=stocking?target.y:(store.supplyBox-1)/Width,face,steps;
            int routeDirection=StoreFurnitureRoute(actor,room,x,y,out face,out steps);
            if(routeDirection<0){actor.clerkShelf=0;actors[index]=actor;return false;}
            actor.shelterId=room.id;actor.shelterX=face%Width;actor.shelterY=face/Width;actor.shelterState=ShelterState.Inside;ClearShelterDeparture(ref actor);
            if(steps>0){direction=routeDirection;actors[index]=actor;return true;}
            if(stocking)
            {
                int amount=Math.Min(actor.carriedFood-FoodReserve(actor),store.shelfTarget-target.food);
                if(amount>0){map.SetShelf(target.x,target.y,target.food+amount,true);actor.carriedFood-=amount;RememberShelf(ref actor,new ShelfMemoryRecord{x=target.x,y=target.y,food=target.food+amount,observedTick=Tick});}
                actor.clerkShelf=0;actors[index]=actor;return amount>0;
            }
            StorageBoxDocument crate=map.StorageBoxAt(x,y);int take=Math.Min(HungerRules.CarryLimit-actor.carriedFood,StorageRules.Count(crate,InventoryItem.Food));
            if(take<=0){if(SupplyRequestsEnabled)actor.clerkNeedsSupplies=!supplyOrders.ContainsKey(store.storeId);RememberStorage(ref actor,StorageObservation(crate));actor.clerkShelf=0;actors[index]=actor;return false;}
            StorageRules.ChangeFood(crate,-take);if(SupplyRequestsEnabled && StorageRules.Count(crate,InventoryItem.Food)==0)actor.clerkNeedsSupplies=!supplyOrders.ContainsKey(store.storeId);map.SetStorageBox(crate);RememberStorage(ref actor,StorageObservation(crate));actor.carriedFood+=take;actors[index]=actor;return true;
        }
        public StoreOverview[] StoreSupplies()
        {
            StoreOverview[] result=new StoreOverview[stores.Count];int n=0;
            foreach(StoreDocument store in stores.Values)
            {
                Room room=FindRoom(store.storeId);StoreOverview row=new StoreOverview{StoreId=store.storeId,SupplyBox=store.supplyBox,ShelfTarget=store.shelfTarget,CrateMeals=StorageRules.Count(map.StorageBoxAt((store.supplyBox-1)%Width,(store.supplyBox-1)/Width),InventoryItem.Food)};
                foreach(ShelfDocument shelf in map.ShelfRecords)if(room.Contains(Key(shelf.x,shelf.y))){row.Shelves++;row.ShelfMeals+=shelf.food;if(shelf.food==0)row.EmptyShelves++;}
                for(int i=0;i<count;i++)if(actors[i].species==Species.Human && actors[i].health>0 && actors[i].clerkStoreId==room.id)row.Clerks++;
                result[n++]=row;
            }
            return result; // Player stock view only; clerk decisions use sight and physical crate visits.
        }
        private StoreDocument[] CaptureStores()
        {
            if(stores.Count==0)return null;StoreDocument[] result=new StoreDocument[stores.Count];int i=0;foreach(StoreDocument store in stores.Values)result[i++]=CopyStore(store);return result;
        }
        private void RestoreStores(StoreDocument[] records)
        {
            if(records==null)return;if(!StoresEnabled || records.Length<1 || records.Length>ShelterRules.MaxRooms)throw new ArgumentException("Invalid store records.");int previous=0;
            foreach(StoreDocument record in records)
            {
                Room room=record==null?null:FindRoom(record.storeId);int key=record==null?-1:record.supplyBox-1;
                if(record==null || record.storeId<=previous || room==null || key<0 || !room.Contains(key) || map.StorageBoxAt(key%Width,key/Width)==null || record.shelfTarget<1 || record.shelfTarget>StoreRules.MaxShelfTarget)throw new ArgumentException("Invalid store configuration.");
                stores.Add(record.storeId,CopyStore(record));previous=record.storeId;
            }
        }
        private void ValidateClerks()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.clerkStoreId==0){if(actor.clerkShelf!=0)throw new ArgumentException("Unassigned clerk has a shelf task.");continue;}
                if(!StoresEnabled || actor.species!=Species.Human || actor.health<=0 || !stores.ContainsKey(actor.clerkStoreId) || actor.courierSourceBox!=0 || actor.restockPhase!=RestockPhase.None)throw new ArgumentException("Invalid clerk assignment.");
                if(actor.clerkShelf!=0){int key=actor.clerkShelf-1;if(key<0 || !FindRoom(actor.clerkStoreId).Contains(key) || map.ShelfAt(key%Width,key/Width)==null)throw new ArgumentException("Invalid clerk shelf target.");}
            }
        }
    }
}
