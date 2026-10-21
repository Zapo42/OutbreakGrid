using System; // Keep presentation timing testable without native Unity APIs.
namespace Outbreak.UnityAdapter // Keep visual motion separate from authoritative cell positions.
{ // Begin lightweight presentation state.
    public struct ShovePresentation
    {
        private bool active;private double tick;private int dx,dy;
        public void Reset(){active=false;}
        public void Observe(int x,int y,double time){dx=x;dy=y;tick=time;active=true;}
        public double Strength(double time){double t=time-tick;if(!active || t<0 || t>=2.4)return 0;return t<0.6?t/0.6:t<1.2?1:(2.4-t)/1.2;}
        public double X(double time){return 0.2*dx*Strength(time);}
        public double Y(double time){return 0.2*dy*Strength(time);}
        public int DX { get { return dx; } } public int DY { get { return dy; } }
    }
    public struct MovementPresentation // Retain one bounded visual segment per actor rather than a movement history.
    { // Begin a single safe cell transition.
        private bool shove;private double contactX,contactY;
        private double fromX, fromY, targetX, targetY, startTick; // Store only the latest rendered segment in simulation tick units.
        public void Snap(int x, int y, double tick) { shove=false;fromX = targetX = x; fromY = targetY = y; startTick = tick; } // Synchronize exact positions on load, pause and manual stepping.
        public void Observe(int x, int y, double tick, bool safe = true) // Register a committed move without animating across unverified terrain.
        { // Begin one visual update.
            if (x == targetX && y == targetY) return; if (!safe || Math.Abs(x - targetX) > 1 || Math.Abs(y - targetY) > 1) { Snap(x, y, tick); return; } // Snap compound pushes and unsafe corners instead of drawing through walls.
            shove=false;fromX = targetX; fromY = targetY; targetX = x; targetY = y; startTick = tick; // Finish any interrupted segment at its canonical endpoint before blending the new adjacent step.
        } // Finish segment registration.
        public void Push(double x,double y,double touchX,double touchY,int endX,int endY,double tick)
        {
            shove=true;fromX=x;fromY=y;contactX=touchX;contactY=touchY;targetX=endX;targetY=endY;startTick=tick;
        }
        private double Pushed(double from,double contact,double target,double tick)
        {
            double elapsed=Math.Max(0,tick-startTick);if(elapsed<0.6)return from+(contact-from)*(elapsed/0.6);
            double fraction=Math.Min(1,(elapsed-0.6)/1.2);return contact+(target-contact)*fraction;
        }
        private double Fraction(double tick) { return Math.Max(0, Math.Min(1, (tick - startTick) / 5.0)); } // Blend over the full five-tick movement interval with steady velocity.
        public double X(double tick) { return shove?Pushed(fromX,contactX,targetX,tick):fromX + (targetX - fromX) * Fraction(tick); } // Keep movement progressing between authoritative opportunities.
        public double Y(double tick) { return shove?Pushed(fromY,contactY,targetY,tick):fromY + (targetY - fromY) * Fraction(tick); } // Use the same timing for both axes.
        public bool OverlapsCell(int x,int y,double tick,int id)
        {
            double left=X(tick)+OffsetX(id)+0.18,top=Y(tick)+OffsetY(id)+0.18;
            return left<x+1 && left+0.64>x && top<y+1 && top+0.64>y;
        }
        public static double OffsetX(int id) { return (((id % 11) * 4) % 11 - 5) * 0.018; } // Give each body a stable small within-cell offset without consuming simulation randomness.
        public static double OffsetY(int id) { return (((id % 11) * 9) % 11 - 5) * 0.018; } // Keep the debug body's full footprint inside its current cell.
    } // Finish bounded visual state.
} // Finish presentation-only motion.
