using System;
using Outbreak.Contracts;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private int selectedVehicle=-1,routingVehicle=-1;private bool vehicleBrush;
        private MovementPresentation[] vehicleMotion=new MovementPresentation[VehicleRules.Maximum];private int knownVehicles;
        private bool VehicleSelected { get { return selectedVehicle>=0 && selectedVehicle<world.VehicleCount && selected<0 && selectedStorageBox==0 && selectedShelf==0 && selectedPhone==0 && selectedBatCell<0; } }
        private void SnapVehicleMotion()
        {
            knownVehicles=world.VehicleCount;for(int i=0;i<knownVehicles;i++){VehicleState car=world.VehicleAtIndex(i);vehicleMotion[i].Snap(car.x,car.y,world.Tick);}
        }
        private void ObserveVehicleMotion()
        {
            if(knownVehicles!=world.VehicleCount){SnapVehicleMotion();return;}
            for(int i=0;i<knownVehicles;i++){VehicleState car=world.VehicleAtIndex(i);if(clock.Paused)vehicleMotion[i].Snap(car.x,car.y,world.Tick);else vehicleMotion[i].Observe(car.x,car.y,world.Tick,interval:VehicleRules.MoveTicks);}
        }
        private void SelectVehicle(int index)
        {
            SetPaused(true);routingVehicle=-1;selectedVehicle=index;selected=-1;selectedStorageBox=0;selectedShelf=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;vehicleBrush=false;sections["Selected item"]=true;sections["Vehicles"]=true;
            VehicleState car=world.VehicleAtIndex(index);centerX=car.x+0.5f;centerY=car.y+0.5f;textureX=-1;
        }
        private void BeginVehiclePlacement(int route=-1)
        {
            SetPaused(true);routingVehicle=route;vehicleBrush=route<0;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;status=route<0?"Click a free road or crosswalk to place a vehicle.":"Click a reachable road or crosswalk for the vehicle destination.";
        }
        private void DrawVehiclePanel()
        {
            GUILayout.Label("Vehicles: "+world.VehicleCount+" / "+VehicleRules.Maximum);
            if(GUILayout.Button("Place vehicle"))Queue(()=>BeginVehiclePlacement());
            for(int i=0;i<world.VehicleCount;i++){int index=i;VehicleState car=world.VehicleAtIndex(i);GUILayout.Label("Vehicle #"+car.id+" at "+car.x+", "+car.y+" | "+world.VehicleStatus(i));if(GUILayout.Button("Observe vehicle #"+car.id))Queue(()=>SelectVehicle(index));}
            if((vehicleBrush || routingVehicle>=0) && GUILayout.Button("Cancel vehicle action"))Queue(()=>{vehicleBrush=false;routingVehicle=-1;});
            GUILayout.Label("Marked roads use right-hand traffic. Straight sections keep one empty cell of following space.\nUnmarked roads retain free routing; drivers and traffic lights come later.");
        }
        private void DrawSelectedVehicle()
        {
            int index=selectedVehicle;VehicleState car=world.VehicleAtIndex(index);
            GUILayout.Label("VEHICLE #"+car.id+" at "+car.x+", "+car.y);GUILayout.Label("Status: "+world.VehicleStatus(index));
            if(car.hasDestination)GUILayout.Label("Destination: "+car.destinationX+", "+car.destinationY);
            if(GUILayout.Button("Choose vehicle destination"))Queue(()=>BeginVehiclePlacement(index));
            if(GUILayout.Button("Stop vehicle"))Queue(()=>{SetPaused(true);world.TryStopVehicle(index);routingVehicle=-1;});
        }
        private void HandleVehiclePlacement(Event input,Rect visible,Rect bounds,int left,int top,float cell)
        {
            if(confirmReset || (!vehicleBrush && routingVehicle<0) || input.type!=EventType.MouseDown || input.button!=0 || !visible.Contains(input.mousePosition))return;
            int x=left+(int)((input.mousePosition.x-bounds.x)/cell),y=top+(int)((input.mousePosition.y-bounds.y)/cell),route=routingVehicle;
            Queue(()=>{
                if(route>=0){if(world.TrySetVehicleDestination(route,x,y)){routingVehicle=-1;status="Vehicle destination set. Run or step to drive.";}else status="Destination needs reachable road or crosswalk; another vehicle cannot occupy it.";}
                else{int id;if(world.TryPlaceVehicle(x,y,out id)){vehicleBrush=false;SelectVehicle(world.VehicleCount-1);status="Vehicle placed. Choose a road destination.";}else status="Vehicle needs a free road or crosswalk cell.";}
            });input.Use();
        }
        private void DrawVehicles(Rect bounds,int left,int top,int width,int height,float cell)
        {
            if(knownVehicles!=world.VehicleCount)SnapVehicleMotion();
            for(int i=0;i<world.VehicleCount;i++)
            {
                VehicleState car=world.VehicleAtIndex(i);double time=world.Tick+(clock.Paused?0:clock.Alpha);float x=clock.Paused?car.x:(float)vehicleMotion[i].X(time),y=clock.Paused?car.y:(float)vehicleMotion[i].Y(time);
                if(x<left || y<top || x>=left+width || y>=top+height)continue;
                bool horizontal=car.heading%2==0;float w=horizontal?0.86f:0.56f,h=horizontal?0.56f:0.86f;
                Rect body=new Rect(bounds.x+(x-left+(1-w)/2)*cell,bounds.y+(y-top+(1-h)/2)*cell,w*cell,h*cell);
                GUI.color=VehicleSelected && i==selectedVehicle?Color.yellow:new Color(0.18f,0.42f,0.65f);GUI.DrawTexture(body,Texture2D.whiteTexture);
                GUI.color=new Color(0.70f,0.84f,0.90f);GUI.DrawTexture(new Rect(body.x+body.width*0.3f,body.y+body.height*0.2f,body.width*0.4f,body.height*0.6f),Texture2D.whiteTexture);
                GUI.color=new Color(0.10f,0.11f,0.12f);
                for(int wheel=0;wheel<2;wheel++)
                {
                    float offset=wheel==0?0.15f:0.7f;
                    if(horizontal){GUI.DrawTexture(new Rect(body.x+body.width*offset,body.y-cell*0.04f,cell*0.15f,cell*0.10f),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(body.x+body.width*offset,body.y+body.height-cell*0.06f,cell*0.15f,cell*0.10f),Texture2D.whiteTexture);}
                    else{GUI.DrawTexture(new Rect(body.x-cell*0.04f,body.y+body.height*offset,cell*0.10f,cell*0.15f),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(body.x+body.width-cell*0.06f,body.y+body.height*offset,cell*0.10f,cell*0.15f),Texture2D.whiteTexture);}
                }
                if(!confirmReset && !vehicleBrush && routingVehicle<0 && !placing.HasValue && shelfBrush<0 && !boxBrush && !restBrush && !phoneBrush && Event.current.type==EventType.MouseDown && Event.current.button==0 && body.Contains(Event.current.mousePosition)){int index=i;Queue(()=>SelectVehicle(index));Event.current.Use();}
            }
            GUI.color=Color.white;
        }
    }
}
