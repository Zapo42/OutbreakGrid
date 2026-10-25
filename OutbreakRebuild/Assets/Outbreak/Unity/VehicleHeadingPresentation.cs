using System;
namespace Outbreak.UnityAdapter
{
    // Visual heading only: simulation headings and road routes remain cardinal.
    public struct VehicleHeadingPresentation
    {
        private double from,to,start;private bool initialized;private int targetHeading;
        public void Snap(int heading,double tick){from=to=heading*90;targetHeading=heading;start=tick;initialized=true;}
        public void Observe(int heading,double tick)
        {
            if(!initialized){Snap(heading,tick);return;}if(heading==targetHeading)return;
            from=Angle(tick);double delta=(heading*90-from)%360;if(delta>180)delta-=360;if(delta< -180)delta+=360;
            to=from+delta;targetHeading=heading;start=tick;
        }
        public double Angle(double tick)
        {
            double t=Math.Max(0,Math.Min(1,(tick-start)/Outbreak.Core.VehicleRules.MoveTicks));t=t*t*(3-2*t);return from+(to-from)*t;
        }
    }
}
