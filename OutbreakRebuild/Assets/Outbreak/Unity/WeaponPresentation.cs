using System;
using Outbreak.Core;
namespace Outbreak.UnityAdapter
{
    public struct WeaponPose
    {
        public double handX,handY,tipX,tipY;
        public bool swinging,pickingUp;
    }
    public struct WeaponPresentation
    {
        public const double SwingTicks=2,PickupTicks=3,HitFlashTicks=1.5;
        private double facingX,facingY;private bool hasAction;private WeaponAction action;
        public void Reset(){facingX=1;facingY=0;hasAction=false;action=new WeaponAction();}
        public void Move(int x,int y){if(x==0 && y==0)return;double length=Math.Sqrt(x*x+y*y);facingX=x/length;facingY=y/length;}
        public void Observe(WeaponAction next,int actorX,int actorY)
        {
            action=next;hasAction=true;if(next.kind==WeaponActionKind.Swing)Move(next.x-actorX,next.y-actorY);
        }
        public WeaponPose Pose(double actorX,double actorY,double tick,bool active=true)
        {
            double dx=facingX,dy=facingY;if(dx==0 && dy==0)dx=1;
            double direction=Math.Atan2(dy,dx),angle=direction-0.9,handX=actorX+0.5+dx*0.23,handY=actorY+0.5+dy*0.23;
            double elapsed=tick-action.tick;bool swing=active && hasAction && action.kind==WeaponActionKind.Swing && elapsed>=0 && elapsed<SwingTicks;
            bool pickup=active && hasAction && action.kind==WeaponActionKind.Pickup && elapsed>=0 && elapsed<PickupTicks;
            if(swing){double phase=elapsed/SwingTicks;angle=direction+(phase<0.7?-1.4+2.8*phase/0.7:1.4-2.3*(phase-0.7)/0.3);}
            if(pickup){double phase=elapsed/PickupTicks;phase=phase*phase*(3-2*phase);handX=(action.x+0.89)*(1-phase)+handX*phase;handY=(action.y+0.2)*(1-phase)+handY*phase;angle=Math.PI/2*(1-phase)+angle*phase;}
            return new WeaponPose{handX=handX,handY=handY,tipX=handX+Math.Cos(angle)*0.62,tipY=handY+Math.Sin(angle)*0.62,swinging=swing,pickingUp=pickup};
        }
    }
}
