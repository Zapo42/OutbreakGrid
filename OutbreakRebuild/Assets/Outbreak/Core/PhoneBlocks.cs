using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public sealed partial class WorldMap
    {
        private readonly SortedDictionary<int,PhoneBlockDocument> phones=new SortedDictionary<int,PhoneBlockDocument>();
        public bool HasPhones { get { return phones.Count>0; } }
        public bool PhoneAt(int x,int y){return Contains(x,y) && phones.ContainsKey(y*Width+x);}
        public void SetPhone(int x,int y)
        {
            if(!Contains(x,y) || Get(x,y)!=Terrain.Floor || !CanWalk(x,y) || RestSpotAt(x,y) || WeaponAt(x,y)!=null || phones.Count>=4096)throw new ArgumentException("Phone needs a free indoor floor cell.");phones.Add(y*Width+x,new PhoneBlockDocument{x=x,y=y});
        }
        internal PhoneBlockDocument[] CapturePhones()
        {
            if(phones.Count==0)return null;PhoneBlockDocument[] result=new PhoneBlockDocument[phones.Count];int i=0;foreach(var pair in phones)result[i++]=new PhoneBlockDocument{x=pair.Value.x,y=pair.Value.y};return result;
        }
        private void RestorePhones(MapDocument source)
        {
            if(source.phoneBlocks==null)return;if(source.schemaVersion<11 || source.phoneBlocks.Length<1 || source.phoneBlocks.Length>4096)throw new ArgumentException("Invalid phone blocks.");foreach(PhoneBlockDocument phone in source.phoneBlocks){if(phone==null)throw new ArgumentException("Invalid phone.");SetPhone(phone.x,phone.y);}
        }
    }
    public sealed partial class World
    {
        public PhoneBlockDocument[] PhoneBlocks(){return map.CapturePhones()??new PhoneBlockDocument[0];}
        public bool TryPlacePhone(int x,int y)
        {
            if(!SupplyRequestsEnabled || HouseAt(x,y)==0 || !map.CanWalk(x,y) || map.RestSpotAt(x,y) || map.WeaponAt(x,y)!=null || map.HasPhones && PhoneBlocks().Length>=4096)return false;
            for(int i=0;i<count;i++)if((actors[i].x==x && actors[i].y==y) || (actors[i].shelterId!=0 && actors[i].shelterX==x && actors[i].shelterY==y))return false;
            map.SetPhone(x,y);TerrainRevision++;return true;
        }
    }
}
