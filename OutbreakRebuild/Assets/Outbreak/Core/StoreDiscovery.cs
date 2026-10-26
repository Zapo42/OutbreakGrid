using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StoreDiscoveryRules
    {
        public const int MemoryLimit=8,MemoryTicks=3600,RetryTicks=300;
    }
    public sealed partial class World
    {
        private static void RememberStore(ref FoodRoomVisit[] notes,int id,long tick)
        {
            if(notes==null){notes=new[]{new FoodRoomVisit{roomId=id,observedTick=tick}};return;}
            for(int i=0;i<notes.Length;i++)if(notes[i].roomId==id){notes[i].observedTick=tick;return;}
            if(notes.Length<StoreDiscoveryRules.MemoryLimit)Array.Resize(ref notes,notes.Length+1);
            else{int oldest=0;for(int i=1;i<notes.Length;i++)if(notes[i].observedTick<notes[oldest].observedTick)oldest=i;notes[oldest]=new FoodRoomVisit{roomId=id,observedTick=tick};Array.Sort(notes,(a,b)=>a.roomId.CompareTo(b.roomId));return;}
            notes[notes.Length-1]=new FoodRoomVisit{roomId=id,observedTick=tick};Array.Sort(notes,(a,b)=>a.roomId.CompareTo(b.roomId));
        }
        private void ObserveStoreEntrances(ref ActorState actor)
        {
            foreach(int id in stores.Keys)if(SeenStorefront(actor,FindRoom(id)))RememberStore(ref actor.storeMemory,id,Tick);
        }
        private readonly Dictionary<int,int[]> storefrontViewpoints=new Dictionary<int,int[]>();
        private bool SeenStorefront(ActorState actor,Room room)
        {
            if(SeenFoodEntrance(actor,room))return true;
            if(!StorefrontDiscoveryEnabled || HouseAt(actor.x,actor.y)!=0)return false;
            int[] viewpoints;
            if(!storefrontViewpoints.TryGetValue(room.id,out viewpoints))
            {
                var cells=new HashSet<int>();
                foreach(int cell in room.cells)for(int d=0;d<8;d+=2)
                {
                    int wallX=cell%Width+dx[d],wallY=cell/Width+dy[d];
                    if(!map.Contains(wallX,wallY) || (map.Get(wallX,wallY)!=Terrain.Wall && map.Get(wallX,wallY)!=Terrain.Window))continue;
                    int x=wallX+dx[d],y=wallY+dy[d];if(map.Contains(x,y) && HouseAt(x,y)==0)cells.Add(Key(x,y));
                }
                viewpoints=new int[cells.Count];cells.CopyTo(viewpoints);Array.Sort(viewpoints);storefrontViewpoints.Add(room.id,viewpoints);
            }
            // Seeing the outside of a shop identifies it without revealing shelves or stock.
            foreach(int cell in viewpoints)
            {
                ActorState facade=new ActorState{x=cell%Width,y=cell/Width};
                if(DistanceSquared(actor,facade)<=EncounterRules.SightRadius*EncounterRules.SightRadius && map.CanWalk(facade.x,facade.y) && Visible(actor,facade))return true;
            }
            return false;
        }
        private void PruneStoreNotes(ref FoodRoomVisit[] notes,int duration)
        {
            if(notes==null)return;List<FoodRoomVisit> retained=null;
            for(int i=0;i<notes.Length;i++)if(Tick-notes[i].observedTick>=duration){retained=new List<FoodRoomVisit>();break;}
            if(retained==null)return;foreach(FoodRoomVisit note in notes)if(Tick-note.observedTick<duration)retained.Add(note);notes=retained.Count==0?null:retained.ToArray();
        }
        private void AdvanceStoreMemory(ref ActorState actor)
        {
            if(!StoreDiscoveryEnabled)return;PruneStoreNotes(ref actor.storeMemory,StoreDiscoveryRules.MemoryTicks);PruneStoreNotes(ref actor.storeFailures,StoreDiscoveryRules.RetryTicks);
        }
        private bool FailedStore(ActorState actor,int store,long newerObservation=-1)
        {
            if(!StoreDiscoveryEnabled || actor.storeFailures==null)return false;
            foreach(FoodRoomVisit note in actor.storeFailures)if(note.roomId==store && newerObservation<=note.observedTick)return true;return false;
        }
        private ShelfMemoryRecord? KnownShoppingShelf(ActorState actor,int room=0)
        {
            if(actor.shelfMemory==null)return null;ShelfMemoryRecord? chosen=null;int best=int.MaxValue,rank=-1;
            foreach(ShelfMemoryRecord note in actor.shelfMemory)
            {
                int id=HouseAt(note.x,note.y);if(note.food==0 || (room!=0 && id!=room) || !stores.ContainsKey(id) || FailedStore(actor,id,note.observedTick) || AvoidBlockedFood(actor,note.x,note.y))continue;
                int distance=FoodShelfRouteLength(actor,note),priority=note.food>0?1:0;if(distance==int.MaxValue)continue;
                if(priority>rank || (priority==rank && distance<best)){chosen=note;best=distance;rank=priority;}
            }
            return chosen;
        }
        private int KnownShoppingStore(ActorState actor)
        {
            if(actor.storeMemory==null && (!ShoppingContinuityEnabled || actor.familiarStoreId==0))return 0;int chosen=0,best=int.MaxValue;
            foreach(FoodRoomVisit note in actor.storeMemory??new FoodRoomVisit[0])
            {
                Room room=FindRoom(note.roomId);if(room==null || FailedStore(actor,room.id) || KnownRoomDanger(actor,room))continue;
                foreach(int door in room.doors)
                {
                    int length,route=ShelterRoute(actor,door%Width,door/Width,0,out length,foodRoute:true);
                    if(route>=0 && length<best){best=length;chosen=room.id;}
                }
            }
            if(ShoppingContinuityEnabled && actor.familiarStoreId!=0 && !FailedStore(actor,actor.familiarStoreId)){
                Room familiar=FindRoom(actor.familiarStoreId);if(familiar!=null && !KnownRoomDanger(actor,familiar))foreach(int door in familiar.doors){int length;if(ShelterRoute(actor,door%Width,door/Width,0,out length,foodRoute:true)>=0 && length<best){best=length;chosen=familiar.id;}}
            }
            if(ConsumerVehiclesEnabled && actor.additionalFamiliarStores!=null)foreach(int id in actor.additionalFamiliarStores){if(FailedStore(actor,id))continue;Room familiar=FindRoom(id);if(familiar==null || KnownRoomDanger(actor,familiar))continue;foreach(int door in familiar.doors){int length;if(ShelterRoute(actor,door%Width,door/Width,0,out length,foodRoute:true)>=0 && length<best){best=length;chosen=id;}}}
            return chosen;
        }
        private bool EndUnsuccessfulShopping(int index,ref ActorState actor,Room home)
        {
            if(ShoppingContinuityEnabled)actor.shoppingEndReason=actor.storeFailures!=null?ShoppingEndReason.EmptyStores:ShoppingEndReason.NoKnownStore;
            actor.shoppingRetryTick=Tick+StoreDiscoveryRules.RetryTicks;actor.restockPhase=RestockPhase.Returning;actor.homeShelterId=0;ClearFoodMemory(ref actor);ClearShelterDeparture(ref actor);actors[index]=actor;
            if(home.Contains(Key(actor.x,actor.y))){ClearRestocking(ref actor);actors[index]=actor;}
            return false;
        }
        private bool TryDiscoverShoppingStore(int index,ref ActorState actor,Room home,out int direction)
        {
            direction=8;
            Room store=FindRoom(actor.shoppingStoreId),current=FindRoom(actor.shelterId);
            if(store==null)
            {
                ShelfMemoryRecord? known=KnownShoppingShelf(actor);
                if(known.HasValue){actor.shoppingShelf=Key(known.Value.x,known.Value.y)+1;actor.shoppingStoreId=HouseAt(known.Value.x,known.Value.y);actor.shoppingSearchCell=0;actors[index]=actor;return true;}
                actor.shoppingStoreId=KnownShoppingStore(actor);actor.shoppingSearchCell=0;store=FindRoom(actor.shoppingStoreId);
                if(store==null)return EndUnsuccessfulShopping(index,ref actor,home);
            }
            if(current!=null && current.id!=store.id && (current.Contains(Key(actor.x,actor.y)) || actor.shelterExitPhase!=ShelterExitPhase.None)){actors[index]=actor;return TryShelter(index,ref actor,-1,out direction);}
            if(!store.Contains(Key(actor.x,actor.y)))
            {
                int best=int.MaxValue,goal=-1,route=-1;
                foreach(int door in store.doors)for(int d=0;d<8;d+=2)
                {
                    int x=door%Width+dx[d],y=door/Width+dy[d],length;if(!map.CanWalk(x,y) || !store.Contains(Key(x,y)))continue;
                    int candidate=ShelterRoute(actor,x,y,0,out length,foodRoute:true);if(candidate>=0 && length<best){best=length;goal=Key(x,y);route=candidate;}
                }
                if(goal<0){RememberStore(ref actor.storeFailures,store.id,Tick);actor.shoppingStoreId=0;actor.shoppingSearchCell=0;actors[index]=actor;return true;}
                actor.shelterId=store.id;actor.shelterX=goal%Width;actor.shelterY=goal/Width;actor.shelterState=ShelterState.Seeking;
                direction=route;int nx=actor.x+dx[route],ny=actor.y+dy[route];if(map.Get(nx,ny)==Terrain.DoorClosed){SetDoor(nx,ny,true);direction=8;}actors[index]=actor;return true;
            }
            ShelfMemoryRecord? shelf=KnownShoppingShelf(actor,store.id);
            if(shelf.HasValue){actor.shoppingShelf=Key(shelf.Value.x,shelf.Value.y)+1;actors[index]=actor;return true;}
            // Sweep floor viewpoints, skipping only cells that are currently visible. Contents remain observation-driven.
            while(actor.shoppingSearchCell<store.cells.Length)
            {
                int goal=store.cells[actor.shoppingSearchCell],length;ActorState viewpoint=new ActorState{x=goal%Width,y=goal/Width};
                if(!map.CanWalk(viewpoint.x,viewpoint.y) || (DistanceSquared(actor,viewpoint)<=(EncounterRules.SightRadius-2)*(EncounterRules.SightRadius-2) && Visible(actor,viewpoint))){actor.shoppingSearchCell++;continue;}
                int route=ShelterRoute(actor,viewpoint.x,viewpoint.y,0,out length,confinedRoom:store);
                if(route<0){actor.shoppingSearchCell++;continue;}
                actor.shelterId=store.id;actor.shelterX=viewpoint.x;actor.shelterY=viewpoint.y;actor.shelterState=ShelterState.Inside;direction=route;actors[index]=actor;return true;
            }
            RememberStore(ref actor.storeFailures,store.id,Tick);actor.shoppingShelf=0;actor.shoppingStoreId=0;actor.shoppingSearchCell=0;ClearFoodMemory(ref actor);actors[index]=actor;return true;
        }
        private void ValidateStoreDiscovery()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];bool any=actor.storeMemory!=null || actor.storeFailures!=null || actor.shoppingStoreId!=0 || actor.shoppingSearchCell!=0 || actor.shoppingRetryTick!=0;
                if(any && (!StoreDiscoveryEnabled || actor.species!=Species.Human || actor.health<=0))throw new ArgumentException("Unsupported store discovery state.");
                ValidateStoreNotes(actor.storeMemory,StoreDiscoveryRules.MemoryTicks);ValidateStoreNotes(actor.storeFailures,StoreDiscoveryRules.RetryTicks);
                if(actor.shoppingRetryTick!=0 && (actor.shoppingRetryTick<=Tick || actor.shoppingRetryTick-Tick>StoreDiscoveryRules.RetryTicks))throw new ArgumentException("Invalid shopping retry.");
                Room store=FindRoom(actor.shoppingStoreId);
                if(actor.shoppingStoreId!=0 && (!IsShopping(actor) || !stores.ContainsKey(actor.shoppingStoreId) || store==null))throw new ArgumentException("Invalid shopping store.");
                if(actor.shoppingSearchCell<0 || (store==null?actor.shoppingSearchCell!=0:actor.shoppingSearchCell>store.cells.Length))throw new ArgumentException("Invalid store search progress.");
                if(StoreDiscoveryEnabled && IsShopping(actor) && actor.shoppingShelf!=0 && actor.shoppingStoreId!=0 && actor.shoppingStoreId!=HouseAt((actor.shoppingShelf-1)%Width,(actor.shoppingShelf-1)/Width))throw new ArgumentException("Shopping shelf differs from store.");
            }
        }
        private void ValidateStoreNotes(FoodRoomVisit[] notes,int duration)
        {
            if(notes==null)return;if(notes.Length<1 || notes.Length>StoreDiscoveryRules.MemoryLimit)throw new ArgumentException("Invalid store notes.");int previous=0;
            foreach(FoodRoomVisit note in notes){if(note.roomId<=previous || !stores.ContainsKey(note.roomId) || note.observedTick<0 || note.observedTick>Tick || Tick-note.observedTick>=duration)throw new ArgumentException("Invalid remembered store.");previous=note.roomId;}
        }
    }
}
