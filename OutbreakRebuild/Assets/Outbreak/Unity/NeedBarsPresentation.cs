using System;
using Outbreak.Contracts;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private bool showHealthBar,showHungerBar,showEnergyBar;
        private GUIStyle needLetterStyle;
        private void DrawNeedStat(string label,string name,bool enabled,Action toggle)
        {
            GUILayout.BeginHorizontal();GUILayout.Label(label);
            if(GUILayout.Button((enabled?"Hide ":"Show ")+name+" bar"))Queue(toggle);
            GUILayout.EndHorizontal();
        }
        private void DrawNpcBars(int index,Rect body,ActorState actor,Rect viewport,float cell)
        {
            if(index!=selected || actor.health<=0)return;
            bool food=showHungerBar && world.HungerEnabled && actor.species==Species.Human;
            bool energy=showEnergyBar && world.EnergyEnabled && actor.species==Species.Human;
            int rows=(showHealthBar?1:0)+(food?1:0)+(energy?1:0);if(rows==0)return;
            if(needLetterStyle==null){needLetterStyle=new GUIStyle{fontSize=9};needLetterStyle.normal.textColor=Color.white;}
            float height=Mathf.Clamp(cell*0.08f,2,4),width=Math.Max(16,body.width),rowHeight=11;
            float x=Math.Max(viewport.x+10,Math.Min(body.x,viewport.x+viewport.width-width));
            float y=Math.Max(viewport.y,body.y-rows*rowHeight-2);
            if(showHealthBar){float fraction=actor.health/100f;DrawNeedBar(x,y,width,height,"H",fraction,new Color(0.85f-0.35f*fraction,0.25f+0.4f*fraction,0.2f));y+=rowHeight;}
            if(food){DrawNeedBar(x,y,width,height,"F",(100-actor.hunger)/100f,new Color(0.90f,0.70f,0.25f));y+=rowHeight;}
            if(energy)DrawNeedBar(x,y,width,height,"E",EnergyRules.Remaining(actor)/100f,new Color(0.30f,0.70f,0.95f));
        }
        private void DrawNeedBar(float x,float y,float width,float height,string letter,float fraction,Color fill)
        {
            GUI.color=Color.white;GUI.Label(new Rect(x-10,y,10,11),letter,needLetterStyle);
            Rect bar=new Rect(x,y+4,width,height);GUI.color=new Color(0.1f,0.1f,0.12f);GUI.DrawTexture(bar,Texture2D.whiteTexture);
            if(fraction>0){GUI.color=fill;GUI.DrawTexture(new Rect(bar.x,bar.y,bar.width*fraction,bar.height),Texture2D.whiteTexture);}
        }
    }
}
