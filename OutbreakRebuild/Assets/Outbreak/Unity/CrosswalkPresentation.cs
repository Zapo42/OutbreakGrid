using System.Collections.Generic;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private readonly List<int> visibleLaneCells=new List<int>();
        private readonly List<int> visibleCrosswalkCells=new List<int>();
        private void RefreshCrosswalkCells(int left,int top,int width,int height)
        {
            visibleCrosswalkCells.Clear();visibleLaneCells.Clear();
            for(int y=top;y<top+height;y++)for(int x=left;x<left+width;x++)if(world.TerrainAt(x,y)==Terrain.Road && world.RoadLaneMaskAt(x,y)!=0)visibleLaneCells.Add(y*world.Width+x);
            for(int y=top;y<top+height;y++)for(int x=left;x<left+width;x++)if(world.TerrainAt(x,y)==Terrain.Crosswalk)visibleCrosswalkCells.Add(y*world.Width+x);
        }
        private void DrawLaneMarkings(Rect bounds,int left,int top,float cell)
        {
            GUI.color=new Color(0.78f,0.75f,0.57f);
            foreach(int key in visibleLaneCells)
            {
                int x=key%world.Width,y=key/world.Width,mask=world.RoadLaneMaskAt(x,y);
                float sx=bounds.x+(x-left)*cell,sy=bounds.y+(y-top)*cell;
                if(mask==4 && y+1<world.Height && world.TerrainAt(x,y+1)==Terrain.Road && world.RoadLaneMaskAt(x,y+1)==1 && x%2==0)
                    GUI.DrawTexture(new Rect(sx+cell*0.2f,sy+cell*0.95f,cell*0.6f,cell*0.1f),Texture2D.whiteTexture);
                if(mask==2 && x+1<world.Width && world.TerrainAt(x+1,y)==Terrain.Road && world.RoadLaneMaskAt(x+1,y)==8 && y%2==0)
                    GUI.DrawTexture(new Rect(sx+cell*0.95f,sy+cell*0.2f,cell*0.1f,cell*0.6f),Texture2D.whiteTexture);
                if((mask!=1 && mask!=2 && mask!=4 && mask!=8) || (x+y)%6!=0)continue;
                float dx=mask==1?1:mask==4?-1:0,dy=mask==2?1:mask==8?-1:0;
                // Rasterized arrow keeps rendering independent of simulation state and GUI rotation.
                for(int i=0;i<5;i++)
                {
                    float along=(i-2)*0.09f;
                    GUI.DrawTexture(new Rect(sx+(0.46f+dx*along)*cell,sy+(0.46f+dy*along)*cell,cell*0.08f,cell*0.08f),Texture2D.whiteTexture);
                }
                for(int side=-1;side<=1;side+=2)for(int i=1;i<=2;i++)
                    GUI.DrawTexture(new Rect(sx+(0.46f+dx*(0.18f-i*0.07f)-dy*side*i*0.07f)*cell,sy+(0.46f+dy*(0.18f-i*0.07f)+dx*side*i*0.07f)*cell,cell*0.08f,cell*0.08f),Texture2D.whiteTexture);
            }
            GUI.color=Color.white;
        }
        private bool CrosswalkAt(int x,int y){return x>=0 && y>=0 && x<world.Width && y<world.Height && world.TerrainAt(x,y)==Terrain.Crosswalk;}
        private void DrawCrosswalkMarkings(Rect bounds,int left,int top,float cell)
        {
            GUI.color=new Color(0.90f,0.88f,0.75f);
            foreach(int key in visibleCrosswalkCells)
            {
                int x=key%world.Width,y=key/world.Width;
                bool horizontal=CrosswalkAt(x-1,y) || CrosswalkAt(x+1,y);
                if(!horizontal && !CrosswalkAt(x,y-1) && !CrosswalkAt(x,y+1))horizontal=true;
                float screenX=bounds.x+(x-left)*cell,screenY=bounds.y+(y-top)*cell;
                for(int stripe=0;stripe<3;stripe++)
                {
                    float offset=0.18f+stripe*0.26f;
                    Rect paint=horizontal?new Rect(screenX+offset*cell,screenY+cell*0.12f,cell*0.12f,cell*0.76f):new Rect(screenX+cell*0.12f,screenY+offset*cell,cell*0.76f,cell*0.12f);
                    GUI.DrawTexture(paint,Texture2D.whiteTexture);
                }
            }
            GUI.color=Color.white;
        }
    }
}
