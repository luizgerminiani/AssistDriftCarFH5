using System;

namespace JSE.Core
{
    public sealed class DriftAssist
    {
        private double _filteredInput;
        private double _filteredSlip;
        private double _output;

        public double LastCorrection { get; private set; }
        public double LastBlend { get; private set; }

        public double Update(double rawSteer, TelemetryFrame telemetry, AssistSettings settings, double elapsedSeconds, bool enabled)
        {
            settings.Clamp();
            elapsedSeconds = Math.Max(0.001, Math.Min(0.100, elapsedSeconds));
            _filteredInput += (rawSteer - _filteredInput) * (1.0 - settings.InputSmoothing);

            bool active = enabled && telemetry != null && telemetry.IsRaceOn &&
                          telemetry.SpeedKmh >= settings.MinSpeedKmh && telemetry.VelocityForward > 0.5f;
            if (!active)
            {
                LastCorrection = 0;
                LastBlend = 0;
                _filteredSlip = 0;
                _output = _filteredInput;
                return Clamp(_output);
            }

            // Body slip is substantially less noisy than an individual tire signal.
            _filteredSlip += (telemetry.BodySlipRadians - _filteredSlip) * 0.18;
            double slipMagnitude = Math.Abs(_filteredSlip);
            double fade = (slipMagnitude - settings.SlipDeadband) / settings.AssistFadeRange;
            LastBlend = SmoothStep(Clamp01(fade));

            double gyroCorrection = -telemetry.YawRate * settings.GyroStrength;
            double counterCorrection = -_filteredSlip * settings.CounterGain;
            LastCorrection = (gyroCorrection + counterCorrection) * LastBlend;

            double target = Clamp(_filteredInput + LastCorrection);
            double maxDelta = settings.MaxSteerRate * elapsedSeconds;
            target = _output + Math.Max(-maxDelta, Math.Min(maxDelta, target - _output));
            _output += (target - _output) * (1.0 - settings.OutputSmoothing);
            return Clamp(_output);
        }

        public void Reset()
        {
            _filteredInput = 0;
            _filteredSlip = 0;
            _output = 0;
            LastCorrection = 0;
            LastBlend = 0;
        }

        private static double SmoothStep(double value) => value * value * (3.0 - 2.0 * value);
        private static double Clamp01(double value) => Math.Max(0.0, Math.Min(1.0, value));
        private static double Clamp(double value) => Math.Max(-1.0, Math.Min(1.0, value));
    }
}
