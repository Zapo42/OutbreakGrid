using System.Collections.Generic;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    // Exact swatches from the approved pixel-scale sheet. This increment maps terrain, not every UI/object color.
    public static class TerrainPalette
    {
        private static Color Rgb(int hex) { return new Color(((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f); }
        public static readonly Color Ink=Rgb(0x101820),Structure=Rgb(0x26333b),Charcoal=Rgb(0x454c50),Stone=Rgb(0x80877a),Cream=Rgb(0xc6c7ae),
            Grass=Rgb(0x556b3c),Olive=Rgb(0x87944d),Sand=Rgb(0xba975f),Wood=Rgb(0x79583d),StoreBlue=Rgb(0x454f6b),WaterBlue=Rgb(0x7097ae),
            Sage=Rgb(0x56715e),Aqua=Rgb(0x78b7b0),Ochre=Rgb(0xc5a04a),Red=Rgb(0xad5348),Gold=Rgb(0xe0c87a);
    }
    public sealed partial class FoundationApp
    {
        private Dictionary<int,BuildingUse> buildingPalette=new Dictionary<int,BuildingUse>();
        private void RefreshBuildingPalette()
        {
            Dictionary<int,BuildingUse> next=world.BuildingUses();bool changed=next.Count!=buildingPalette.Count;
            if(!changed)foreach(var pair in next){BuildingUse old;if(!buildingPalette.TryGetValue(pair.Key,out old) || old!=pair.Value){changed=true;break;}}
            if(changed){buildingPalette=next;textureX=-1;}
        }
        private static string BuildingUseLabel(BuildingUse use)
        {
            return use==BuildingUse.DistributionCenter?"Distribution center":use==BuildingUse.Store?"Store":use==BuildingUse.Home?"Home / shelter":"Outdoor / unassigned";
        }
        private static Color BuildingFloorColor(BuildingUse use)
        {
            if(use==BuildingUse.DistributionCenter)return TerrainPalette.Sage;
            if(use==BuildingUse.Store)return TerrainPalette.StoreBlue;
            return TerrainPalette.Sand;
        }
        private static void IconPart(Rect area,float x,float y,float width,float height,Color color)
        {
            GUI.color=color;GUI.DrawTexture(new Rect(area.x+x*area.width,area.y+y*area.height,width*area.width,height*area.height),Texture2D.whiteTexture);
        }
        private static void DrawStorageIcon(Rect area,BuildingUse use,bool stocked,bool selectedBox)
        {
            if(selectedBox)IconPart(area,-0.05f,-0.05f,1.1f,1.1f,Color.yellow);
            Color dark=new Color(0.16f,0.19f,0.20f),wood=new Color(0.55f,0.39f,0.24f),metal=new Color(0.38f,0.57f,0.71f),crate=new Color(0.49f,0.55f,0.35f);
            IconPart(area,0,0,1,1,dark);
            if(use==BuildingUse.DistributionCenter)
            {
                IconPart(area,0.06f,0.06f,0.88f,0.88f,crate);
                for(int row=1;row<=3;row++)IconPart(area,0.08f,row*0.22f,0.84f,0.04f,dark);
                IconPart(area,0.13f,0.03f,0.12f,0.94f,wood);IconPart(area,0.75f,0.03f,0.12f,0.94f,wood);
            }
            else if(use==BuildingUse.Store)
            {
                IconPart(area,0.07f,0.06f,0.86f,0.87f,metal);IconPart(area,0.48f,0.07f,0.04f,0.83f,dark);
                IconPart(area,0.34f,0.39f,0.07f,0.21f,Color.yellow);IconPart(area,0.59f,0.39f,0.07f,0.21f,Color.yellow);
            }
            else
            {
                IconPart(area,0.07f,0.10f,0.86f,0.80f,wood);IconPart(area,0.04f,0.10f,0.92f,0.20f,new Color(0.70f,0.53f,0.33f));
                IconPart(area,0.07f,0.32f,0.86f,0.06f,dark);IconPart(area,0.43f,0.25f,0.14f,0.27f,new Color(0.84f,0.72f,0.40f));
            }
            IconPart(area,0.31f,0.78f,0.38f,0.09f,stocked?new Color(0.70f,0.85f,0.85f):dark);
        }
        private static void DrawPhoneIcon(Rect area,bool selectedPhoneBlock)
        {
            if(selectedPhoneBlock)IconPart(area,-0.05f,-0.05f,1.1f,1.1f,Color.yellow);
            Color body=new Color(0.18f,0.70f,0.75f),handset=new Color(0.09f,0.22f,0.25f);
            IconPart(area,0.13f,0.27f,0.74f,0.69f,body);
            IconPart(area,0.08f,0.05f,0.84f,0.16f,handset);IconPart(area,0.02f,0.12f,0.22f,0.32f,handset);IconPart(area,0.76f,0.12f,0.22f,0.32f,handset);
            IconPart(area,0.33f,0.33f,0.34f,0.12f,handset);
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)IconPart(area,0.31f+col*0.14f,0.53f+row*0.12f,0.09f,0.07f,Color.white);
        }
    }
}
