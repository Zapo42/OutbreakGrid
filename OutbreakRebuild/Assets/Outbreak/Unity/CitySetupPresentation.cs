using System;
using System.Globalization;
using Outbreak.Core;
using Outbreak.Contracts;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private CityZoning cityZoning=CityZoning.Fixed;
        private CityStreetLayout cityStreetLayout=CityStreetLayout.Staggered;
        private int cityHomesPerBlock=2;
        private bool cityUsePacks=true;
        private CityPackTheme cityPackTheme=CityPackTheme.Mixed;
        private bool showCitySetup,cityStartPaused=true;
        private string citySeedInput="214",cityColumnsInput="4",cityRowsInput="2";
        private int packFoodCursor,packRestCursor;
        private void DrawCityPackStations()
        {
            var boxes=world.StorageBoxSites();var shelves=world.ShelfSites();var rest=Array.FindAll(world.RestSpotSites(),s=>!s.area);
            GUILayout.Label("Food stations: "+boxes.Length+" storage, "+shelves.Length+" shelves | Rest stations: "+rest.Length);
            if(boxes.Length+shelves.Length>0 && GUILayout.Button("Find food station"))Queue(()=>{
                int pick=packFoodCursor++%(boxes.Length+shelves.Length);
                if(pick<boxes.Length){var box=boxes[pick];FocusSupplyBox(box.y*world.Width+box.x+1);}
                else{var shelf=shelves[pick-boxes.Length];SetPaused(true);selected=-1;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;selectedShelf=shelf.y*world.Width+shelf.x+1;centerX=shelf.x+0.5f;centerY=shelf.y+0.5f;span=16;textureX=-1;sections["Selected item"]=true;}
            });
            if(rest.Length>0 && GUILayout.Button("Find rest station"))Queue(()=>{var spot=rest[packRestCursor++%rest.Length];SetPaused(true);centerX=spot.x+0.5f;centerY=spot.y+0.5f;span=16;textureX=-1;status="Rest station at "+spot.x+", "+spot.y+". Bed markers identify usable recovery places.";});
            if(GUILayout.Button("City overview"))Queue(()=>{centerX=world.Width/2f;centerY=world.Height/2f;span=Math.Max(world.Width,world.Height*Math.Max(64,Screen.width-300)/(float)Math.Max(64,Screen.height-184))+2;textureX=-1;});
        }
        private void DrawCitySetup()
        {
            GUI.color=new Color(0.07f,0.08f,0.1f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
            float width=Math.Min(560,Screen.width-24),height=Math.Min(650,Screen.height-24);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height),GUI.skin.box);menuScroll=GUILayout.BeginScrollView(menuScroll);
            GUILayout.Label("SET UP GENERATED CITY");GUILayout.Label(BuildLabel);
            if(GUILayout.Button("City source: "+(cityUsePacks?"City packs":"Classic")))Queue(()=>{cityUsePacks=!cityUsePacks;cityHomesPerBlock=cityUsePacks?2:1;});
            if(cityUsePacks){
                GUILayout.Label("City pack: "+CityPackSetup.ThemeName(cityPackTheme));
                string[] names={"Mixed","Dense urban","Urban apartments","Terrace residential","Garden suburb","Cul-de-sac suburb","Suburban grid","Business core","Business campus","Retail corridor"};
                for(int row=0;row<4;row++){GUILayout.BeginHorizontal();for(int col=0;col<3;col++){int index=row*3+col;if(index>=names.Length)break;CityPackTheme choice=(CityPackTheme)index;if(GUILayout.Button(names[index]))Queue(()=>cityPackTheme=choice);}GUILayout.EndHorizontal();}
            }
            if(GUILayout.Button("Small city (2 x 2 blocks)"))Queue(()=>{cityColumnsInput="2";cityRowsInput="2";});
            if(GUILayout.Button("Standard city (4 x 2 blocks)"))Queue(()=>{cityColumnsInput="4";cityRowsInput="2";});
            int largeRows=cityHomesPerBlock>=3?3:cityHomesPerBlock==2?5:6;
            if(GUILayout.Button("Large city (6 x "+largeRows+" blocks)"))Queue(()=>{cityColumnsInput="6";cityRowsInput=largeRows.ToString(CultureInfo.InvariantCulture);});
            if(GUILayout.Button("Expanded city (8 x 8 blocks)"))Queue(()=>{cityColumnsInput="8";cityRowsInput="8";});
            if(GUILayout.Button("Residential density: "+(cityHomesPerBlock==1?"One house per block":cityHomesPerBlock==2?"Two houses per block":cityHomesPerBlock==3?"Three houses per block":"Four houses per block")))Queue(()=>{cityHomesPerBlock=cityHomesPerBlock==4?1:cityHomesPerBlock+1;});
            if(!cityUsePacks && GUILayout.Button("Street layout: "+cityStreetLayout))Queue(()=>{cityStreetLayout=cityStreetLayout==CityStreetLayout.Staggered?CityStreetLayout.Grid:cityStreetLayout==CityStreetLayout.Grid?CityStreetLayout.Neighborhood:CityStreetLayout.Staggered;});
            if(!cityUsePacks && GUILayout.Button("Zoning: "+(cityZoning==CityZoning.Fixed?"Fixed layout":"Neighborhood centers")))Queue(()=>{cityZoning=cityZoning==CityZoning.Fixed?CityZoning.NeighborhoodCenters:CityZoning.Fixed;});
            GUILayout.Label("City seed (1 to 4294967295)");citySeedInput=GUILayout.TextField(citySeedInput,32);
            GUILayout.Label("Blocks across (2 to 10)");cityColumnsInput=GUILayout.TextField(cityColumnsInput,32);
            GUILayout.Label("Blocks down (2 to 10)");cityRowsInput=GUILayout.TextField(cityRowsInput,32);
            if(GUILayout.Button("Start city paused: "+(cityStartPaused?"Yes":"No")))Queue(()=>{cityStartPaused=!cityStartPaused;});
            uint seed=0;int columns=0,rows=0;
            bool parsed=uint.TryParse(citySeedInput,NumberStyles.None,CultureInfo.InvariantCulture,out seed) && int.TryParse(cityColumnsInput,NumberStyles.None,CultureInfo.InvariantCulture,out columns) && int.TryParse(cityRowsInput,NumberStyles.None,CultureInfo.InvariantCulture,out rows);
            string error=parsed?(cityUsePacks?CityPackSetup.ValidationError(seed,columns,rows,cityHomesPerBlock,cityPackTheme):CitySetup.ValidationError(seed,columns,rows,cityHomesPerBlock,cityStreetLayout,cityZoning)):"Enter whole numbers for the seed and both block counts.";
            if(error!=null)GUILayout.Label(error);
            else
            {
                CitySetupSummary summary=cityUsePacks?CityPackSetup.Describe(seed,columns,rows,cityHomesPerBlock,cityPackTheme):CitySetup.Describe(seed,columns,rows,cityHomesPerBlock,cityStreetLayout,cityZoning);
                GUILayout.Label("Map: "+summary.Width+" x "+summary.Height+" cells | "+summary.Buildings+" buildings");
                GUILayout.Label(summary.Homes+" homes | "+summary.Stores+(summary.Stores==1?" store":" stores")+" | 1 distribution center");
                GUILayout.Label(summary.Humans+" humans: 1 courier, "+summary.Stores+(summary.Stores==1?" clerk, ":" clerks, ")+summary.Consumers+(summary.Consumers==1?" consumer":" consumers")+" | "+summary.Meals+" starting meals");
            }
            GUILayout.Label("One resident per home; workers start at their separate workplaces.");
            GUILayout.Label(cityUsePacks?"Pack templates: compact/spacious houses, stores and distribution centers.":"Staggered streets use varied block sizes and offset side-road junctions.");
            if(cityUsePacks)GUILayout.Label("District references: © OpenStreetMap contributors. See CityReferences/NOTICE.md.");
            GUILayout.Label("Street-facing entrances; larger stores and distribution buildings.");
            GUILayout.Label("No starting zombies. Use Place zombie to introduce an outbreak.");
            GUI.enabled=error==null;
            if(GUILayout.Button("Start generated city"))
            {
                bool paused=cityStartPaused,usePacks=cityUsePacks;CityPackTheme theme=cityPackTheme;int density=cityHomesPerBlock;CityStreetLayout layout=cityStreetLayout;CityZoning zoning=cityZoning;
                Queue(()=>StartScenario(()=>{World candidate=usePacks?CityPackSetup.Create(seed,columns,rows,density,theme):CitySetup.Create(seed,columns,rows,density,layout,zoning);UseWorld(candidate);span=Math.Max(candidate.Width,candidate.Height);selected=0;
                    if(usePacks){for(int i=0;i<world.Count;i++)if(world.Occupation(i)==HumanOccupation.Consumer){selected=i;break;}span=32;centerX=world.ActorAt(selected).x+0.5f;centerY=world.ActorAt(selected).y+0.5f;sections["Generated city stations"]=true;packFoodCursor=0;packRestCursor=0;}showStores=true;showCenters=true;SetPaused(paused);status="Generated city ("+(usePacks?CityPackSetup.ThemeName(theme):layout.ToString())+"): seed "+seed+", "+columns+" x "+rows+" blocks. "+(paused?"Paused: Run or step to begin.":"Running.");}));
            }
            GUI.enabled=true;
            if(GUILayout.Button("Back to Main Menu"))Queue(()=>{showCitySetup=false;menuScroll=new Vector2();});
            GUILayout.Label(status);GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
