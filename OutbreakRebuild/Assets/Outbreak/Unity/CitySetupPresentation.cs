using System;
using System.Globalization;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private bool showCitySetup,cityStartPaused=true;
        private string citySeedInput="214",cityColumnsInput="4",cityRowsInput="2";
        private void DrawCitySetup()
        {
            GUI.color=new Color(0.07f,0.08f,0.1f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
            float width=Math.Min(560,Screen.width-24),height=Math.Min(650,Screen.height-24);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height),GUI.skin.box);menuScroll=GUILayout.BeginScrollView(menuScroll);
            GUILayout.Label("SET UP GENERATED CITY");GUILayout.Label(BuildLabel);
            if(GUILayout.Button("Small city (2 x 2 blocks)"))Queue(()=>{cityColumnsInput="2";cityRowsInput="2";});
            if(GUILayout.Button("Standard city (4 x 2 blocks)"))Queue(()=>{cityColumnsInput="4";cityRowsInput="2";});
            if(GUILayout.Button("Large city (6 x 6 blocks)"))Queue(()=>{cityColumnsInput="6";cityRowsInput="6";});
            GUILayout.Label("City seed (1 to 4294967295)");citySeedInput=GUILayout.TextField(citySeedInput,32);
            GUILayout.Label("Blocks across (2 to 6)");cityColumnsInput=GUILayout.TextField(cityColumnsInput,32);
            GUILayout.Label("Blocks down (2 to 6)");cityRowsInput=GUILayout.TextField(cityRowsInput,32);
            if(GUILayout.Button("Start city paused: "+(cityStartPaused?"Yes":"No")))Queue(()=>{cityStartPaused=!cityStartPaused;});
            uint seed=0;int columns=0,rows=0;
            bool parsed=uint.TryParse(citySeedInput,NumberStyles.None,CultureInfo.InvariantCulture,out seed) && int.TryParse(cityColumnsInput,NumberStyles.None,CultureInfo.InvariantCulture,out columns) && int.TryParse(cityRowsInput,NumberStyles.None,CultureInfo.InvariantCulture,out rows);
            string error=parsed?CitySetup.ValidationError(seed,columns,rows):"Enter whole numbers for the seed and both block counts.";
            if(error!=null)GUILayout.Label(error);
            else
            {
                CitySetupSummary summary=CitySetup.Describe(seed,columns,rows);
                GUILayout.Label("Map: "+summary.Width+" x "+summary.Height+" cells | "+summary.Buildings+" buildings");
                GUILayout.Label(summary.Homes+" homes | "+summary.Stores+(summary.Stores==1?" store":" stores")+" | 1 distribution center");
                GUILayout.Label(summary.Humans+" humans: 1 courier, "+summary.Stores+(summary.Stores==1?" clerk, ":" clerks, ")+summary.Consumers+(summary.Consumers==1?" consumer":" consumers")+" | "+summary.Meals+" starting meals");
            }
            GUILayout.Label("One resident per home; workers start at their separate workplaces.");
            GUILayout.Label("Street-facing entrances; larger stores and distribution buildings.");
            GUILayout.Label("No starting zombies. Use Place zombie to introduce an outbreak.");
            GUI.enabled=error==null;
            if(GUILayout.Button("Start generated city"))
            {
                bool paused=cityStartPaused;
                Queue(()=>StartScenario(()=>{World candidate=CitySetup.Create(seed,columns,rows);UseWorld(candidate);span=Math.Max(candidate.Width,candidate.Height);selected=0;showStores=true;showCenters=true;SetPaused(paused);status="Generated city: seed "+seed+", "+columns+" x "+rows+" blocks. "+(paused?"Paused: Run or step to begin.":"Running.");}));
            }
            GUI.enabled=true;
            if(GUILayout.Button("Back to Main Menu"))Queue(()=>{showCitySetup=false;menuScroll=new Vector2();});
            GUILayout.Label(status);GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
