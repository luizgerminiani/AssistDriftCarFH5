namespace JSE.Core
{
    public sealed class TelemetryFrame
    {
        public bool IsRaceOn { get; set; }
        public float SpeedKmh { get; set; }
        public float YawRate { get; set; }
        public float BodySlipRadians { get; set; }
        public float FrontSlipRadians { get; set; }
        public float VelocityForward { get; set; }
    }
}
