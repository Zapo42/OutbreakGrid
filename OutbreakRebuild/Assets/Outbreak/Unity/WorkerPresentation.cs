using System;
using Outbreak.Core;
using Outbreak.Contracts;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private string clerkStartInput="7",clerkHoursInput="12";
        private int clerkScheduleActor=-1,clerkScheduleStart=-1,clerkScheduleHours=-1;
        private void DrawOccupationOutline(int index,ActorState actor,Rect body,float cell)
        {
            if(actor.species!=Species.Human || actor.health<=0)return;
            // Employment identity remains visible while an off-duty worker temporarily follows consumer routines.
            HumanOccupation job=world.AssignedOccupation(index);bool active=world.WorkerOnDuty(index);
            Color upper=job==HumanOccupation.Consumer?Color.white:!active?new Color(0.55f,0.55f,0.55f):job==HumanOccupation.Courier?new Color(0.50f,0.32f,0.18f):new Color(0.95f,0.80f,0.20f);
            Color lower=job==HumanOccupation.Clerk && active?new Color(0.20f,0.45f,0.90f):upper;
            float edge=Mathf.Clamp(cell*0.035f,1,2);Rect area=new Rect(body.x-edge,body.y-edge,body.width+edge*2,body.height+edge*2);
            GUI.color=upper;GUI.DrawTexture(new Rect(area.x,area.y,area.width,edge),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(area.x,area.y,edge,area.height/2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(area.x+area.width-edge,area.y,edge,area.height/2),Texture2D.whiteTexture);
            GUI.color=lower;GUI.DrawTexture(new Rect(area.x,area.y+area.height/2,edge,area.height/2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(area.x+area.width-edge,area.y+area.height/2,edge,area.height/2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(area.x,area.y+area.height-edge,area.width,edge),Texture2D.whiteTexture);
        }
        private void SetClerkSchedule(int index,int start,int hours)
        {
            SetPaused(true);status=world.TrySetClerkSchedule(index,start,hours)?"Clerk schedule updated. Loaded shelf work finishes before returning home.":"Select a living clerk with an assigned home; start hour 0–23, shift length 1–23.";
        }
        private void DrawClerkScheduleControls()
        {
            if(!world.WorkerShiftsEnabled || selected<0 || selected>=world.Count)return;ActorState actor=world.ActorAt(selected);if(actor.species!=Species.Human || actor.health<=0 || actor.clerkStoreId==0)return;
            GUILayout.Label("Clerk shift: "+(actor.clerkShiftHours==0?"Always available":actor.clerkShiftStartHour.ToString("00")+":00 for "+actor.clerkShiftHours+" hours"));
            int index=selected;
            if(GUILayout.Button("Clerk always available"))Queue(()=>SetClerkSchedule(index,0,0));
            if(GUILayout.Button("Clerk day shift 07-19"))Queue(()=>SetClerkSchedule(index,7,12));
            if(GUILayout.Button("Clerk night shift 19-07"))Queue(()=>SetClerkSchedule(index,19,12));
            if(clerkScheduleActor!=actor.id || clerkScheduleStart!=actor.clerkShiftStartHour || clerkScheduleHours!=actor.clerkShiftHours){clerkScheduleActor=actor.id;clerkScheduleStart=actor.clerkShiftStartHour;clerkScheduleHours=actor.clerkShiftHours;clerkStartInput=actor.clerkShiftStartHour.ToString();clerkHoursInput=(actor.clerkShiftHours==0?12:actor.clerkShiftHours).ToString();}
            GUILayout.Label("Clerk start hour / hours per day");GUILayout.BeginHorizontal();clerkStartInput=GUILayout.TextField(clerkStartInput,2);clerkHoursInput=GUILayout.TextField(clerkHoursInput,2);GUILayout.EndHorizontal();
            int start,hours;bool valid=int.TryParse(clerkStartInput,out start) && int.TryParse(clerkHoursInput,out hours) && start>=0 && start<24 && hours>0 && hours<24 && actor.assignedHouseId!=0;
            bool enabled=GUI.enabled;GUI.enabled=enabled && valid;
            if(GUILayout.Button("Apply clerk shift")){int hour=int.Parse(clerkStartInput),duration=int.Parse(clerkHoursInput);Queue(()=>SetClerkSchedule(index,hour,duration));}GUI.enabled=enabled;
            if(actor.assignedHouseId==0)GUILayout.Label("Assign a home before scheduling this clerk.");
        }
    }
}
