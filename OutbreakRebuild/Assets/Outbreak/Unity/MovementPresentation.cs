using System; // Keep presentation timing testable without native Unity APIs.
namespace Outbreak.UnityAdapter // Keep visual motion separate from authoritative cell positions.
{ // Begin lightweight presentation state.
    public struct MovementPresentation // Retain one bounded visual segment per actor rather than a movement history.
    { // Begin a single safe cell transition.
        private double fromX, fromY, targetX, targetY, startTick; // Store only the latest rendered segment in simulation tick units.
        public void Snap(int x, int y, double tick) { fromX = targetX = x; fromY = targetY = y; startTick = tick; } // Synchronize exact positions on load, pause and manual stepping.
        public void Observe(int x, int y, double tick, bool safe = true) // Register a committed move without animating across unverified terrain.
        { // Begin one visual update.
            if (x == targetX && y == targetY) return; if (!safe || Math.Abs(x - targetX) > 1 || Math.Abs(y - targetY) > 1) { Snap(x, y, tick); return; } // Snap compound pushes and unsafe corners instead of drawing through walls.
            fromX = targetX; fromY = targetY; targetX = x; targetY = y; startTick = tick; // Finish any interrupted segment at its canonical endpoint before blending the new adjacent step.
        } // Finish segment registration.
        private double Fraction(double tick) { return Math.Max(0, Math.Min(1, (tick - startTick) / 5.0)); } // Blend over the full five-tick movement interval with steady velocity.
        public double X(double tick) { return fromX + (targetX - fromX) * Fraction(tick); } // Keep movement progressing between authoritative opportunities.
        public double Y(double tick) { return fromY + (targetY - fromY) * Fraction(tick); } // Use the same timing for both axes.
        public static double OffsetX(int id) { return (((id % 11) * 4) % 11 - 5) * 0.018; } // Give each body a stable small within-cell offset without consuming simulation randomness.
        public static double OffsetY(int id) { return (((id % 11) * 9) % 11 - 5) * 0.018; } // Keep the debug body's full footprint inside its current cell.
    } // Finish bounded visual state.
} // Finish presentation-only motion.
