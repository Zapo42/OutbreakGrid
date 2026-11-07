using System;
using Outbreak.Core;
using Outbreak.Contracts;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    // Furniture has a solid silhouette. Shared integer edges avoid fractional-rectangle seams at camera zooms.
    public static class HouseholdFurniturePresentation
    {
        public static string Mask(HouseholdFurnitureKind kind,int width=1)
        {
            switch(kind){
                case HouseholdFurnitureKind.KitchenTable:return ".WWWWW./WWWWWWW/WSSSSSW/WSSSSSW/WWWWWWW/.W...W./.W...W.";
                case HouseholdFurnitureKind.Chair:return ".WWWWW./.WWWWW./.WSSSW./.WSSSW./.WWWWW./.W...W./.......";
                case HouseholdFurnitureKind.Couch:return width==2?".UUUUUUUUUUUU./UUUUUUUUUUUUUU/UBBBBBUUBBBBBU/UBBBBBUUBBBBBU/UUUUUUUUUUUUUU/.KK........KK./..............":".UUUUU./UUUUUUU/UBBBBBU/UBBBBBU/UUUUUUU/.K...K./.......";
                case HouseholdFurnitureKind.ComfyChair:return ".UUUUU./UUUUUUU/UUBBBUU/UUBBBUU/UUUUUUU/.K...K./.......";
                case HouseholdFurnitureKind.Bed:return "WWWWWWW/WCCCCCW/WCCCCCW/WBBBBBW/WBBBBBW/WBBBBBW/WWWWWWW";
                case HouseholdFurnitureKind.Television:return "KKKKKKK/KAAAAAK/KAAAAAK/KAAAAAK/KKKKKKK/..KKK../.KKKKK.";
                case HouseholdFurnitureKind.Radio:return ".....K./....K../WWWWWWW/WKKKWCW/WKKKWDW/WWWWWWW/.K...K.";
                default:throw new ArgumentOutOfRangeException("kind");
            }
        }
        private static Color Ink(char ink,bool working)
        {
            switch(ink){case 'W':return TerrainPalette.Wood;case 'S':return TerrainPalette.Sand;case 'C':return TerrainPalette.Cream;case 'B':return TerrainPalette.WaterBlue;case 'U':return TerrainPalette.StoreBlue;case 'A':return working?TerrainPalette.Aqua:TerrainPalette.Charcoal;case 'D':return working?TerrainPalette.Gold:TerrainPalette.Charcoal;default:return TerrainPalette.Ink;}
        }
        public static void Draw(Rect bounds,HouseholdFurnitureDocument f){Draw(bounds,f,bounds);}
        public static void Draw(Rect bounds,HouseholdFurnitureDocument f,Rect clip)
        {
            Color old=GUI.color;
            if(bounds.height<7){GUI.color=TerrainPalette.Wood;Paint(Math.Floor(bounds.x+bounds.width*0.5),Math.Floor(bounds.y+bounds.height*0.5),Math.Floor(bounds.x+bounds.width*0.5)+1,Math.Floor(bounds.y+bounds.height*0.5)+1,clip);GUI.color=old;return;}
            string[] rows=Mask(f.kind,WorldMap.FurnitureWidth(f)).Split('/');int columns=rows[0].Length;
            for(int y=0;y<rows.Length;y++)for(int x=0;x<columns;){
                char ink=rows[y][x];int end=x+1;while(end<columns && rows[y][end]==ink)end++;
                if(ink!='.'){
                    GUI.color=Ink(ink,f.working);
                    // Adjacent runs share exactly the same rounded edge, including at non-integral cell sizes.
                    Paint(Math.Floor(bounds.x+x*bounds.width/columns+0.5),Math.Floor(bounds.y+y*bounds.height/rows.Length+0.5),Math.Floor(bounds.x+end*bounds.width/columns+0.5),Math.Floor(bounds.y+(y+1)*bounds.height/rows.Length+0.5),clip);
                }x=end;
            }GUI.color=old;
        }
        private static void Paint(double x1,double y1,double x2,double y2,Rect clip)
        {
            x1=Math.Max(x1,Math.Ceiling(clip.x));y1=Math.Max(y1,Math.Ceiling(clip.y));x2=Math.Min(x2,Math.Floor(clip.x+clip.width));y2=Math.Min(y2,Math.Floor(clip.y+clip.height));
            if(x2>x1 && y2>y1)GUI.DrawTexture(new Rect((float)x1,(float)y1,(float)(x2-x1),(float)(y2-y1)),Texture2D.whiteTexture);
        }
    }
    public sealed partial class FoundationApp
    {
        private void DrawHouseholdFurniture(Rect bounds,int left,int top,int width,int height,float cell)
        {
            if(!world.HouseholdActivitiesAvailable)return;
            Rect clip=new Rect(bounds.x,bounds.y,Math.Min(bounds.width,width*cell),Math.Min(bounds.height,height*cell));
            foreach(var f in world.HouseholdFurnitureSites()){
                int footprint=WorldMap.FurnitureWidth(f);
                // A couch can intersect the camera even when its left-hand anchor is outside the visible cells.
                if(f.x+footprint<=left || f.y<top || f.x>=left+width || f.y>=top+height)continue;
                HouseholdFurniturePresentation.Draw(new Rect(bounds.x+(f.x-left)*cell,bounds.y+(f.y-top)*cell,footprint*cell,cell),f,clip);
            }
        }
        private void DrawHouseholdPanel()
        {
            var actor=world.ActorAt(selected);GUILayout.Label(world.HouseholdActivityKnownBy(selected));GUILayout.Label("Snack: "+HouseholdRules.SnackRelief(actor.endurance)+" hunger relief. Table meal: 50. Both use 1 food.");GUILayout.Label("Chair / comfy chair / couch / bed energy: 3 / 6 / 6 / 12. Standing does not restore energy.");
            foreach(var f in world.HouseholdFurnitureSites()){
                if(world.HouseAt(f.x,f.y)!=actor.assignedHouseId)continue;
                if(f.kind==HouseholdFurnitureKind.Radio || f.kind==HouseholdFurnitureKind.Television){int x=f.x,y=f.y;bool next=!f.working;if(GUILayout.Button(f.kind+" "+(f.working?"on":"off")+" at "+x+","+y+" — toggle"))Queue(()=>{world.TrySetApplianceWorking(x,y,next);status=f.kind+" switched "+(next?"on":"off")+".";});}
                else GUILayout.Label((f.kind==HouseholdFurnitureKind.ComfyChair?"Comfy chair":f.kind.ToString())+" at "+f.x+","+f.y+(f.width==2?" — 2 seats":""));
            }
        }
    }
}
