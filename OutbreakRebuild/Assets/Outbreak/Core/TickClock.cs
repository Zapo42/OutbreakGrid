using System; // Reject nonfinite frame inputs and unsupported speed settings.
namespace Outbreak.Core // Keep scheduling testable without running Unity.
{ // Begin fixed tick scheduling.
    public sealed class TickClock // Convert real frame time to a bounded number of exact logical ticks.
    { // Begin the scheduler.
        public const int MaxManualTicks=600; // One game hour, with the same authoritative tick path as ordinary playback.
        private double accumulated; // Keep unprocessed time as debt instead of silently skipping simulation ticks.
        public bool Paused { get; private set; } = true; public double Speed { get; private set; } = 1; // Start paused so fixtures can be inspected before running.
        public double DebtSeconds { get { return accumulated; } } // Make catch-up pressure visible to diagnostics.
        public double Alpha { get { return Paused ? 1 : Math.Min(1, accumulated * Rules.TicksPerSecond); } } // Freeze interpolation immediately when paused.
        public void SetPaused(bool value) { if (Paused != value) { Paused = value; accumulated = 0; } } // Remove stale fractional time at an explicit pause boundary.
        public void SetSpeed(double value) // Offer only the agreed half, normal, double, and triple rates.
        { // Begin speed validation.
            if (value != 0.5 && value != 1 && value != 2 && value != 3) throw new ArgumentOutOfRangeException("value"); Speed = value; // Keep all behavioral time in ticks rather than changing the tick size.
        } // Finish speed validation.
        public int Advance(double realSeconds, Action step, int budget = 10) // Bound work per rendered frame while retaining unfinished tick debt.
        { // Begin one frame scheduling pass.
            if (double.IsNaN(realSeconds) || double.IsInfinity(realSeconds) || realSeconds < 0 || budget < 1 || step == null) throw new ArgumentException("Invalid scheduler input."); // Refuse corrupt frame time before touching the accumulator.
            if (Paused) return 0; double addition = realSeconds * Speed; if (double.IsInfinity(addition) || addition > double.MaxValue - accumulated) throw new ArgumentException("Frame time exceeds the scheduler range."); accumulated += addition; int done = 0; // Exclude paused time and prevent an overflow from permanently corrupting tick debt.
            while (done < budget && accumulated + 1e-12 >= 1.0 / Rules.TicksPerSecond) { step(); accumulated = Math.Max(0, accumulated - 1.0 / Rules.TicksPerSecond); done++; } // Execute whole ticks and keep any remaining debt.
            return done; // Report actual work without implying a tick was skipped.
        } // Finish one frame scheduling pass.
        public void StepTicks(int ticks, Action step) // Offer exact bounded batches without changing playback speed.
        { // Begin manual stepping validation.
            if (!Paused || step == null) throw new InvalidOperationException("Pause before stepping."); if (ticks < 1 || ticks > MaxManualTicks) throw new ArgumentOutOfRangeException("ticks"); for (int i = 0; i < ticks; i++) step(); accumulated = 0; // Execute every requested tick and remain paused.
        } // Finish one manual stepping command.
        public void StepTen(Action step) { StepTicks(10, step); } // Preserve existing callers of the original ten-tick control.
    } // Finish the scheduler.
} // Finish fixed tick scheduling.
