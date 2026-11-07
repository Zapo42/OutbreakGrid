using System;
using Outbreak.Core;
using Outbreak.Contracts;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    // Five-cell symbols use existing reference inks. No object state or decisions live in the renderer.
    public static class HouseholdFurniturePresentation
    {
        public static string Mask(HouseholdFurnitureKind kind)
        {
            switch(kind){case HouseholdFurnitureKind.KitchenTable:return ".WWW./WWWWW/WWWWW/.W.W./.W.W.";case HouseholdFurnitureKind.Chair:return ".WWW./.W.W./.SSS./.W.W./.W.W.";case HouseholdFurnitureKind.Couch:return ".WWW./WWWWW/WSSSW/WWWWW/.K.K.";case HouseholdFurnitureKind.Bed:return "WWWWW/WCCCW/WBBBW/WBBBW/WWWWW";case HouseholdFurnitureKind.Television:return "KKKKK/KAAAK/KAAAK/KKKKK/.K.K.";default:return ".WWW./WKWSW/WKWSW/WWWWW/.W.W.";}
        }
        public static void Draw(Rect bounds,HouseholdFurnitureDocument f)
        {
            Color old=GUI.color;if(bounds.width<5){GUI.color=TerrainPalette.Wood;GUI.DrawTexture(new Rect(bounds.x+bounds.width*0.5f,bounds.y+bounds.height*0.5f,1,1),Texture2D.whiteTexture);GUI.color=old;return;}
            string[] rows=Mask(f.kind).Split('/');float pitch=bounds.width/5f;
            for(int y=0;y<5;y++)for(int x=0;x<5;x++){char ink=rows[y][x];if(ink=='.')continue;GUI.color=ink=='W'?TerrainPalette.Wood:ink=='S'?TerrainPalette.Sand:ink=='C'?TerrainPalette.Cream:ink=='B'?TerrainPalette.WaterBlue:ink=='A'?(f.working?TerrainPalette.Aqua:TerrainPalette.Charcoal):TerrainPalette.Ink;GUI.DrawTexture(new Rect(bounds.x+x*pitch,bounds.y+y*pitch,pitch,pitch),Texture2D.whiteTexture);}
            if(f.kind==HouseholdFurnitureKind.Radio && f.working){GUI.color=TerrainPalette.Gold;GUI.DrawTexture(new Rect(bounds.x+pitch*3,bounds.y+pitch*2,pitch,pitch),Texture2D.whiteTexture);}GUI.color=old;
        }
    }
    public sealed partial class FoundationApp
    {
        private void DrawHouseholdFurniture(Rect bounds,int left,int top,int width,int height,float cell)
        {
            if(!world.HouseholdActivitiesAvailable)return;foreach(var f in world.HouseholdFurnitureSites()){if(f.x<left || f.y<top || f.x>=left+width || f.y>=top+height)continue;HouseholdFurniturePresentation.Draw(new Rect(bounds.x+(f.x-left)*cell,bounds.y+(f.y-top)*cell,cell,cell),f);}
        }
        private void DrawHouseholdPanel()
        {
            var actor=world.ActorAt(selected);GUILayout.Label(world.HouseholdActivityKnownBy(selected));GUILayout.Label("Snack: "+HouseholdRules.SnackRelief(actor.endurance)+" hunger relief. Table meal: 50. Both use 1 food.");GUILayout.Label("Chair / couch / bed energy: 3 / 6 / 12. Standing does not restore energy.");
            foreach(var f in world.HouseholdFurnitureSites()){
                if(world.HouseAt(f.x,f.y)!=actor.assignedHouseId)continue;
                if(f.kind==HouseholdFurnitureKind.Radio || f.kind==HouseholdFurnitureKind.Television){int x=f.x,y=f.y;bool next=!f.working;if(GUILayout.Button(f.kind+" "+(f.working?"on":"off")+" at "+x+","+y+" — toggle"))Queue(()=>{world.TrySetApplianceWorking(x,y,next);status=f.kind+" switched "+(next?"on":"off")+".";});}
                else GUILayout.Label(f.kind+" at "+f.x+","+f.y);
            }
        }
    }
}
