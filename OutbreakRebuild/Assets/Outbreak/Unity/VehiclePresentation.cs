using System;
using Outbreak.Contracts;
using Outbreak.Core;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private int selectedVehicle=-1,routingVehicle=-1;private bool vehicleBrush,parkingBrush;
        private VehicleHeadingPresentation[] vehicleHeading=new VehicleHeadingPresentation[VehicleRules.Maximum];
        private MovementPresentation[] vehicleMotion=new MovementPresentation[VehicleRules.Maximum];private int knownVehicles;
        private bool VehicleSelected { get { return selectedVehicle>=0 && selectedVehicle<world.VehicleCount && selected<0 && selectedStorageBox==0 && selectedShelf==0 && selectedPhone==0 && selectedBatCell<0; } }
        private static Color VehiclePaintColor(VehiclePaint paint)
        {
            switch(paint){case VehiclePaint.Red:return new Color(0.65f,0.24f,0.20f);case VehiclePaint.Green:return new Color(0.25f,0.48f,0.30f);case VehiclePaint.Sand:return new Color(0.67f,0.58f,0.38f);case VehiclePaint.Silver:return new Color(0.62f,0.65f,0.68f);case VehiclePaint.Charcoal:return new Color(0.25f,0.27f,0.29f);case VehiclePaint.White:return new Color(0.84f,0.84f,0.79f);case VehiclePaint.Yellow:return new Color(0.79f,0.65f,0.22f);default:return new Color(0.18f,0.42f,0.65f);}
        }
        private static void PrepareFasterVehicleClocks(SaveDocument saved)
        {
            if(saved.vehicles==null)return;for(int i=0;i<saved.vehicles.Length;i++){var car=saved.vehicles[i];if(car.nextMoveTick>saved.tick)car.nextMoveTick=saved.tick+Math.Min(2,car.nextMoveTick-saved.tick);if(car.exitAtTick>saved.tick)car.exitAtTick=saved.tick+Math.Min(2,car.exitAtTick-saved.tick);saved.vehicles[i]=car;}
        }
        private void SnapVehicleMotion()
        {
            knownVehicles=world.VehicleCount;for(int i=0;i<knownVehicles;i++){VehicleState car=world.VehicleAtIndex(i);vehicleMotion[i].Snap(car.x,car.y,world.Tick);vehicleHeading[i].Snap(car.heading,world.Tick);}
        }
        private void ObserveVehicleMotion()
        {
            if(knownVehicles!=world.VehicleCount){SnapVehicleMotion();return;}
            for(int i=0;i<knownVehicles;i++){VehicleState car=world.VehicleAtIndex(i);if(clock.Paused){vehicleMotion[i].Snap(car.x,car.y,world.Tick);vehicleHeading[i].Snap(car.heading,world.Tick);}else{vehicleMotion[i].Observe(car.x,car.y,world.Tick,interval:world.VehicleMoveTicks);vehicleHeading[i].Observe(car.heading,world.Tick,world.VehicleMoveTicks);}}
        }
        private void SelectVehicle(int index)
        {
            SetPaused(true);routingVehicle=-1;selectedVehicle=index;selected=-1;selectedStorageBox=0;selectedShelf=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;vehicleBrush=false;parkingBrush=false;sections["Selected item"]=true;sections["Vehicles"]=true;
            VehicleState car=world.VehicleAtIndex(index);centerX=car.x+0.5f;centerY=car.y+0.5f;textureX=-1;
        }
        private void BeginVehiclePlacement(int route=-1)
        {
            SetPaused(true);parkingBrush=false;routingVehicle=route;vehicleBrush=route<0;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;status=route<0?"Click a free road or crosswalk to place a vehicle.":"Click a reachable road or crosswalk for the vehicle destination.";
        }
        private void DrawVehiclePanel()
        {
            GUILayout.Label("Vehicles: "+world.VehicleCount+" / "+VehicleRules.Maximum);
            if(world.ParkingTrafficEnabled && GUILayout.Button("Place parking bay"))Queue(()=>{BeginVehiclePlacement();vehicleBrush=false;parkingBrush=true;status="Click clear grass or sidewalk beside an ordinary road, away from intersections, to mark an off-lane bay.";});
            if(world.ParkingTrafficEnabled && selectedStorageBox>0 && GUILayout.Button("Add parking near selected box")){int key=selectedStorageBox-1;Queue(()=>{SetPaused(true);status=world.TryPlaceParkingNearBox(key%world.Width,key/world.Width)?"Parking bay added near this box.":"No clear curb bay available near this box.";});}
            GUILayout.Label("Parking bays: "+world.ParkingBays().Length);
            if(GUILayout.Button("Place vehicle"))Queue(()=>BeginVehiclePlacement());
            for(int i=0;i<world.VehicleCount;i++){int index=i;VehicleState car=world.VehicleAtIndex(i);GUILayout.Label("Vehicle #"+car.id+" at "+car.x+", "+car.y+" | "+world.VehicleStatus(i));if(GUILayout.Button("Observe vehicle #"+car.id))Queue(()=>SelectVehicle(index));}
            if((parkingBrush || vehicleBrush || routingVehicle>=0) && GUILayout.Button("Cancel vehicle action"))Queue(()=>{parkingBrush=false;vehicleBrush=false;routingVehicle=-1;});
            GUILayout.Label("Marked roads use right-hand traffic. Straight sections keep one empty cell of following space.\nUnmarked roads retain free routing; marked junctions admit one car at a time when its exit is clear.");
        }
        private void DrawSelectedVehicle()
        {
            int index=selectedVehicle;VehicleState car=world.VehicleAtIndex(index);
            GUILayout.Label("VEHICLE #"+car.id+" at "+car.x+", "+car.y);GUILayout.Label("Status: "+world.VehicleStatus(index));GUILayout.Label("Vehicle color: "+car.paint);GUILayout.Label(world.VehicleVariationsEnabled?"Road pace: Faster":"Road pace: Legacy");if(world.VehicleVariationsEnabled && GUILayout.Button("Next vehicle color"))Queue(()=>{SetPaused(true);world.TrySetVehiclePaint(index,(VehiclePaint)(((int)car.paint+1)%8));});
            if(car.consumerDriver){GUILayout.Label("Shopper #"+car.driverId+(car.driverAboard?" (aboard)":" (on foot)"));GUILayout.Label(world.ConsumerVehicleStatus(index));GUILayout.Label("Vehicle groceries: "+car.cargoFood+" / "+ConsumerVehicleRules.CargoMeals);GUILayout.Label("Shopping cart: "+car.handFood+" / "+ConsumerVehicleRules.CartMeals);if(car.purchaseGoal>0)GUILayout.Label("Trip target: "+car.purchaseGoal+" meals");if(car.deliveriesPaused && GUILayout.Button("Resume vehicle shopping"))Queue(()=>{if(!world.TryResumeVehicleCourier(index))status="Assign a healthy replacement shopper before resuming.";});if(car.consumerPhase==ConsumerVehiclePhase.None && car.cargoFood==0 && !car.driverAboard && GUILayout.Button("Unassign shopper"))Queue(()=>world.TryUnassignVehicleCourier(index));}
            if(car.driverId!=0 && !car.consumerDriver){GUILayout.Label("Courier #"+car.driverId+(car.driverAboard?" (aboard)":" (on foot)"));GUILayout.Label(world.VehicleDeliveryStatus(index));GUILayout.Label("Cargo: "+car.cargoFood+" / "+CourierVehicleRules.CargoMeals+" | Shipment with courier: "+car.handFood);if(car.deliveriesPaused && GUILayout.Button("Resume courier deliveries"))Queue(()=>{if(!world.TryResumeVehicleCourier(index))status="Assign a healthy replacement courier before resuming deliveries.";});if(car.deliveryPhase==VehicleDeliveryPhase.None && !car.driverAboard && GUILayout.Button("Unassign courier"))Queue(()=>world.TryUnassignVehicleCourier(index));}
            if(world.CourierVehiclesEnabled && !car.driverAboard)for(int a=0;a<world.Count;a++){int actorIndex=a;var person=world.ActorAt(a);if(person.species==Species.Human && person.health>0 && person.turnAtTick==0 && person.distributionCenterId!=0 && world.AssignedVehicle(a)<0 && GUILayout.Button("Assign courier #"+person.id))Queue(()=>{if(!world.TryAssignVehicleCourier(index,actorIndex))status="Courier needs an idle assignment and a vehicle near the source crate.";});}
            if(world.ConsumerVehiclesEnabled && !car.driverAboard)for(int a=0;a<world.Count;a++){int actorIndex=a;var person=world.ActorAt(a);if(person.species==Species.Human && person.health>0 && person.turnAtTick==0 && person.assignedHouseId!=0 && person.clerkStoreId==0 && person.courierSourceBox==0 && world.AssignedVehicle(a)<0 && GUILayout.Button("Assign shopper #"+person.id))Queue(()=>{if(!world.TryAssignVehicleShopper(index,actorIndex))status="Shopper needs an idle assignment, an assigned home, and a vehicle near home.";});}
            if(car.shipmentDropped){GUILayout.Label("Dropped shipment: "+car.handFood+" meals at "+car.shipmentX+", "+car.shipmentY);if(GUILayout.Button("Observe dropped shipment"))Queue(()=>{centerX=car.shipmentX+0.5f;centerY=car.shipmentY+0.5f;textureX=-1;});}
            if(car.blockedSinceTick>0)GUILayout.Label("Blocked for "+(world.Tick-car.blockedSinceTick)+" ticks; bounded detour checks active.");
            if(car.detourUntilTick>world.Tick)GUILayout.Label("Following a traffic detour");
            if(car.hasDestination)GUILayout.Label("Destination: "+car.destinationX+", "+car.destinationY);
            if(car.driverId==0 && GUILayout.Button("Choose vehicle destination"))Queue(()=>BeginVehiclePlacement(index));
            if(GUILayout.Button("Stop vehicle"))Queue(()=>{SetPaused(true);world.TryStopVehicle(index);routingVehicle=-1;});
        }
        private void HandleVehiclePlacement(Event input,Rect visible,Rect bounds,int left,int top,float cell)
        {
            if(confirmReset || (!parkingBrush && !vehicleBrush && routingVehicle<0) || input.type!=EventType.MouseDown || input.button!=0 || !visible.Contains(input.mousePosition))return;
            int x=left+(int)((input.mousePosition.x-bounds.x)/cell),y=top+(int)((input.mousePosition.y-bounds.y)/cell),route=routingVehicle;
            Queue(()=>{
                if(parkingBrush){if(world.TryPlaceParkingBay(x,y)){parkingBrush=false;status="Parking bay marked. Couriers reserve available bays near their destination.";}else status="Choose clear grass or sidewalk beside a road outside an intersection.";}
                else if(route>=0){if(world.TrySetVehicleDestination(route,x,y)){routingVehicle=-1;status="Vehicle destination set. Run or step to drive.";}else status="Choose reachable road or crosswalk outside an intersection, clear of other vehicles.";}
                else{int id;if(world.TryPlaceVehicle(x,y,out id)){vehicleBrush=false;SelectVehicle(world.VehicleCount-1);status="Vehicle placed. Choose a road destination.";}else status="Vehicle needs a free road or crosswalk cell.";}
            });input.Use();
        }
        private void DrawParkingBays(Rect bounds,int left,int top,int width,int height,float cell)
        {
            foreach(var bay in world.ParkingBays()){if(bay.x<left || bay.y<top || bay.x>=left+width || bay.y>=top+height)continue;float x=bounds.x+(bay.x-left)*cell,y=bounds.y+(bay.y-top)*cell;GUI.color=new Color(0.32f,0.34f,0.36f);GUI.DrawTexture(new Rect(x+cell*0.05f,y+cell*0.05f,cell*0.9f,cell*0.9f),Texture2D.whiteTexture);GUI.color=new Color(0.88f,0.86f,0.68f);GUI.DrawTexture(new Rect(x+cell*0.12f,y+cell*0.12f,cell*0.08f,cell*0.76f),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(x+cell*0.8f,y+cell*0.12f,cell*0.08f,cell*0.76f),Texture2D.whiteTexture);}
            GUI.color=Color.white;
        }
        private void DrawShoppingCart(float x,float y,float cell)
        {
            GUI.color=new Color(0.68f,0.70f,0.68f);GUI.DrawTexture(new Rect(x,y,cell*0.38f,cell*0.26f),Texture2D.whiteTexture);GUI.color=new Color(0.66f,0.48f,0.24f);GUI.DrawTexture(new Rect(x+cell*0.05f,y+cell*0.04f,cell*0.28f,cell*0.14f),Texture2D.whiteTexture);GUI.color=new Color(0.12f,0.13f,0.14f);GUI.DrawTexture(new Rect(x+cell*0.04f,y+cell*0.26f,cell*0.08f,cell*0.08f),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(x+cell*0.26f,y+cell*0.26f,cell*0.08f,cell*0.08f),Texture2D.whiteTexture);GUI.color=Color.white;
        }
        private void DrawDroppedShipments(Rect bounds,int left,int top,int width,int height,float cell)
        {
            for(int index=0;index<world.VehicleCount;index++){var car=world.VehicleAtIndex(index);if(!car.shipmentDropped || car.shipmentX<left || car.shipmentY<top || car.shipmentX>=left+width || car.shipmentY>=top+height)continue;if(car.consumerDriver)DrawShoppingCart(bounds.x+(car.shipmentX-left+0.28f)*cell,bounds.y+(car.shipmentY-top+0.28f)*cell,cell);else{GUI.color=new Color(0.66f,0.48f,0.24f);GUI.DrawTexture(new Rect(bounds.x+(car.shipmentX-left+0.28f)*cell,bounds.y+(car.shipmentY-top+0.28f)*cell,cell*0.44f,cell*0.44f),Texture2D.whiteTexture);}}GUI.color=Color.white;
        }
        private void DrawVehicles(Rect bounds,int left,int top,int width,int height,float cell)
        {
            if(knownVehicles!=world.VehicleCount)SnapVehicleMotion();
            for(int i=0;i<world.VehicleCount;i++)
            {
                VehicleState car=world.VehicleAtIndex(i);double time=world.Tick+(clock.Paused?0:clock.Alpha);float x=clock.Paused?car.x:(float)vehicleMotion[i].X(time),y=clock.Paused?car.y:(float)vehicleMotion[i].Y(time);
                if(x<left || y<top || x>=left+width || y>=top+height)continue;
                const float w=0.86f,h=0.56f;
                float angle=clock.Paused?car.heading*90:(float)vehicleHeading[i].Angle(time);
                Rect body=new Rect(bounds.x+(x-left+(1-w)/2)*cell,bounds.y+(y-top+(1-h)/2)*cell,w*cell,h*cell);
                Matrix4x4 previousMatrix=GUI.matrix;float pivotX=body.x+body.width/2,pivotY=body.y+body.height/2;
                GUIUtility.RotateAroundPivot(angle,new Vector2(pivotX,pivotY));
                if(VehicleSelected && i==selectedVehicle){GUI.color=Color.yellow;GUI.DrawTexture(new Rect(body.x-cell*0.05f,body.y-cell*0.05f,body.width+cell*0.1f,body.height+cell*0.1f),Texture2D.whiteTexture);}
                GUI.color=VehiclePaintColor(car.paint);GUI.DrawTexture(body,Texture2D.whiteTexture);
                GUI.color=new Color(0.70f,0.84f,0.90f);GUI.DrawTexture(new Rect(body.x+body.width*0.3f,body.y+body.height*0.2f,body.width*0.4f,body.height*0.6f),Texture2D.whiteTexture);
                if(car.cargoFood>0){GUI.color=new Color(0.66f,0.48f,0.24f);GUI.DrawTexture(new Rect(body.x+body.width*0.06f,body.y+body.height*0.28f,body.width*0.18f,body.height*0.44f),Texture2D.whiteTexture);}
                GUI.color=new Color(0.10f,0.11f,0.12f);
                for(int wheel=0;wheel<2;wheel++)
                {
                    float offset=wheel==0?0.15f:0.7f;
                    GUI.DrawTexture(new Rect(body.x+body.width*offset,body.y-cell*0.04f,cell*0.15f,cell*0.10f),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(body.x+body.width*offset,body.y+body.height-cell*0.06f,cell*0.15f,cell*0.10f),Texture2D.whiteTexture);
                }
                GUI.matrix=previousMatrix;
                double radians=angle*Math.PI/180,mouseX=Event.current.mousePosition.x-pivotX,mouseY=Event.current.mousePosition.y-pivotY;
                Vector2 localMouse=new Vector2(pivotX+(float)(mouseX*Math.Cos(radians)+mouseY*Math.Sin(radians)),pivotY+(float)(-mouseX*Math.Sin(radians)+mouseY*Math.Cos(radians)));
                if(!confirmReset && !parkingBrush && !vehicleBrush && routingVehicle<0 && !placing.HasValue && shelfBrush<0 && !boxBrush && !restBrush && !phoneBrush && Event.current.type==EventType.MouseDown && Event.current.button==0 && body.Contains(localMouse)){int index=i;Queue(()=>SelectVehicle(index));Event.current.Use();}
            }
            GUI.color=Color.white;
        }
    }
}
