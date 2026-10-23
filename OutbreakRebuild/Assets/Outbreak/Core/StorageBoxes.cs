using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class StorageRules
    {
        public const int DefaultCapacity=30,MaxCapacity=1000,MaxBoxes=4096;
        public static int Count(StorageBoxDocument box,InventoryItem item)
        {
            int total=0;if(box!=null && box.items!=null)foreach(ItemStackDocument stack in box.items)if(stack.item==item)total+=stack.quantity;return total;
        }
        public static int Used(StorageBoxDocument box)
        {
            int total=0;foreach(ItemStackDocument stack in box.items)total+=stack.quantity;return total;
        }
        internal static StorageBoxDocument Copy(StorageBoxDocument box)
        {
            ItemStackDocument[] items=new ItemStackDocument[box.items.Length];for(int i=0;i<items.Length;i++)items[i]=new ItemStackDocument{item=box.items[i].item,quantity=box.items[i].quantity};return new StorageBoxDocument{x=box.x,y=box.y,capacity=box.capacity,items=items};
        }
        internal static void ChangeFood(StorageBoxDocument box,int delta)
        {
            int food=Count(box,InventoryItem.Food)+delta;box.items=food==0?new ItemStackDocument[0]:new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=food}};
        }
    }
    public sealed partial class WorldMap
    {
        private readonly SortedDictionary<int,StorageBoxDocument> storageBoxes=new SortedDictionary<int,StorageBoxDocument>();
        public bool HasStorageBoxes { get { return storageBoxes.Count>0; } }
        internal IEnumerable<StorageBoxDocument> StorageRecords { get { return storageBoxes.Values; } }
        public StorageBoxDocument StorageBoxAt(int x,int y)
        {
            StorageBoxDocument box;return Contains(x,y) && storageBoxes.TryGetValue(y*Width+x,out box)?StorageRules.Copy(box):null;
        }
        public void SetStorageBox(StorageBoxDocument box)
        {
            if(box==null || !Contains(box.x,box.y) || !TerrainRules.Walkable(Get(box.x,box.y)) || Get(box.x,box.y)==Terrain.DoorOpen || shelves.ContainsKey(box.y*Width+box.x) || RestSpotAt(box.x,box.y) || WeaponAt(box.x,box.y)!=null || box.capacity<1 || box.capacity>StorageRules.MaxCapacity || box.items==null || box.items.Length>1 || (!storageBoxes.ContainsKey(box.y*Width+box.x) && storageBoxes.Count>=StorageRules.MaxBoxes))throw new ArgumentException("Invalid storage box placement, capacity or inventory.");
            int used=0;foreach(ItemStackDocument stack in box.items){if(stack==null || stack.item!=InventoryItem.Food || stack.quantity<1 || stack.quantity>StorageRules.MaxCapacity)throw new ArgumentException("Invalid storage item stack.");used+=stack.quantity;}
            if(used>box.capacity)throw new ArgumentException("Storage inventory exceeds capacity.");storageBoxes[box.y*Width+box.x]=StorageRules.Copy(box);
        }
        internal StorageBoxDocument[] CaptureStorageBoxes()
        {
            if(storageBoxes.Count==0)return null;List<StorageBoxDocument> result=new List<StorageBoxDocument>();foreach(StorageBoxDocument box in storageBoxes.Values)result.Add(StorageRules.Copy(box));return result.ToArray();
        }
        private void RestoreStorageBoxes(MapDocument source)
        {
            if(source.storageBoxes==null)return;if(source.schemaVersion<8 || source.storageBoxes.Length==0 || source.storageBoxes.Length>StorageRules.MaxBoxes)throw new ArgumentException("Unsupported storage boxes.");
            foreach(StorageBoxDocument box in source.storageBoxes){if(box==null || StorageBoxAt(box.x,box.y)!=null)throw new ArgumentException("Invalid or duplicate storage box.");SetStorageBox(box);}
        }
    }
    public sealed partial class World
    {
        public StorageBoxDocument[] StorageBoxSites(){return map.CaptureStorageBoxes()??new StorageBoxDocument[0];}
        public StorageBoxDocument StorageBoxAt(int x,int y){return map.StorageBoxAt(x,y);}
        public bool TryPlaceStorageBox(int x,int y)
        {
            if(!StorageEnabled || !map.Contains(x,y) || !map.CanWalk(x,y) || IsDoor(x,y) || map.RestSpotAt(x,y) || map.WeaponAt(x,y)!=null || StorageBoxSites().Length>=StorageRules.MaxBoxes)return false;
            for(int i=0;i<count;i++)if((actors[i].x==x && actors[i].y==y) || (actors[i].shelterId!=0 && actors[i].shelterX==x && actors[i].shelterY==y))return false;
            map.SetStorageBox(new StorageBoxDocument{x=x,y=y,capacity=StorageRules.DefaultCapacity,items=new ItemStackDocument[0]});TerrainRevision++;return true;
        }
        private bool StorageVisible(ActorState actor,StorageBoxDocument box)
        {
            return ShelfVisible(actor,new ShelfDocument{x=box.x,y=box.y}); // Observe the container from an accessible face, without seeing through walls.
        }
        private bool TryUseStorage(int index,ref ActorState actor,int threat,Room room,bool deposit,out int direction)
        {
            direction=8;if(!StorageEnabled || actor.turnAtTick!=0 || actor.freezeUntilTick>Tick || threat>=0 || HasRecentWarning(actor) || HasRecentDoorNoise(actor) || (actor.reaction!=HumanReaction.Calm && actor.reaction!=HumanReaction.Aware))return false;
            StorageBoxDocument chosen=null;int best=int.MaxValue,bestDirection=8;
            foreach(StorageBoxDocument box in map.StorageRecords)
            {
                if((room!=null && !room.Contains(Key(box.x,box.y))) || !StorageVisible(actor,box) || (deposit?StorageRules.Used(box)>=box.capacity:StorageRules.Count(box,InventoryItem.Food)==0))continue;
                if(Math.Abs(actor.x-box.x)+Math.Abs(actor.y-box.y)==1){if(best>0){chosen=box;best=0;bestDirection=8;}continue;}
                for(int d=0;d<8;d+=2)
                {
                    int x=box.x+dx[d],y=box.y+dy[d],length;if(!map.CanWalk(x,y) || (room!=null && !room.Contains(Key(x,y))))continue;
                    int route=ShelterRoute(actor,x,y,0,out length,room);if(route>=0 && length>0 && length<best){chosen=box;best=length;bestDirection=route;}
                }
            }
            if(chosen==null)return false;
            if(best>0){direction=bestDirection;int nx=actor.x+dx[direction],ny=actor.y+dy[direction];if(IsDoor(nx,ny) && map.Get(nx,ny)==Terrain.DoorClosed){SetDoor(nx,ny,true);direction=8;}if(deposit)ClearShelterDeparture(ref actor);actors[index]=actor;return true;}
            StorageBoxDocument update=StorageRules.Copy(chosen);int amount=deposit?Math.Min(actor.carriedFood-FoodReserve(actor),chosen.capacity-StorageRules.Used(chosen)):Math.Min(HungerRules.CarryLimit-actor.carriedFood,StorageRules.Count(chosen,InventoryItem.Food));if(amount<=0)return false;
            StorageRules.ChangeFood(update,deposit?amount:-amount);map.SetStorageBox(update);actor.carriedFood+=deposit?-amount:amount;ClearFoodMemory(ref actor);if(deposit)ClearShelterDeparture(ref actor);actors[index]=actor;return true; // Transfer one finite inventory into another, consuming the actor's ordinary action.
        }
    }
}
