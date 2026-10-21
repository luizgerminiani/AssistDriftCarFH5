using System;

namespace JSE.Core
{
    public sealed class AssistSettings
    {
        public double GyroStrength { get; set; } = 1.500;
        public double CounterGain { get; set; } = 1.500;
        public double SlipDeadband { get; set; } = 0.025;
        public double InputSmoothing { get; set; } = 0.450;
        public double OutputSmoothing { get; set; } = 0.000;
        public double AssistFadeRange { get; set; } = 0.080;
        public double MaxSteerRate { get; set; } = 3.500;
        public double MinSpeedKmh { get; set; } = 20.0;

        public void Clamp()
        {
            GyroStrength = Between(GyroStrength, 0.000, 2.000);
            CounterGain = Between(CounterGain, 0.000, 2.000);
            SlipDeadband = Between(SlipDeadband, 0.010, 0.300);
            InputSmoothing = Between(InputSmoothing, 0.000, 0.950);
            OutputSmoothing = Between(OutputSmoothing, 0.000, 0.950);
            AssistFadeRange = Between(AssistFadeRange, 0.020, 0.200);
            MaxSteerRate = Between(MaxSteerRate, 0.500, 5.000);
            MinSpeedKmh = Between(MinSpeedKmh, 5.0, 80.0);
        }

        private static double Between(double value, double minimum, double maximum)
            => Math.Max(minimum, Math.Min(maximum, value));
    }
}
