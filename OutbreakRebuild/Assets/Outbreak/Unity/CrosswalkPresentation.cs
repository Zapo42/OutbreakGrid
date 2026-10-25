using System.Collections.Generic;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private readonly List<int> visibleCrosswalkCells=new List<int>();
        private void RefreshCrosswalkCells(int left,int top,int width,int height)
        {
            visibleCrosswalkCells.Clear();
            for(int y=top;y<top+height;y++)for(int x=left;x<left+width;x++)if(world.TerrainAt(x,y)==Terrain.Crosswalk)visibleCrosswalkCells.Add(y*world.Width+x);
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
