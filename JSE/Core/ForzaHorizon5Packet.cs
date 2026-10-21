using System;

namespace JSE.Core
{
    public static class ForzaHorizon5Packet
    {
        public const int SledLength = 232;
        public const int HorizonDashLength = 324;
        private const int HorizonDashBase = 244;

        public static bool TryParse(byte[] packet, out TelemetryFrame frame)
        {
            frame = null;
            if (packet == null || (packet.Length != SledLength && packet.Length != HorizonDashLength))
                return false;

            float velocityX = ReadSingle(packet, 32);
            float velocityZ = ReadSingle(packet, 40);
            float speedKmh = packet.Length == HorizonDashLength
                ? Math.Abs(ReadSingle(packet, HorizonDashBase + 12)) * 3.6f
                : (float)Math.Sqrt(velocityX * velocityX + velocityZ * velocityZ) * 3.6f;

            // Forza exports tire slip angles in degrees. The assist math uses radians.
            float frontSlipDegrees = (ReadSingle(packet, 164) + ReadSingle(packet, 168)) * 0.5f;

            frame = new TelemetryFrame
            {
                IsRaceOn = ReadInt32(packet, 0) != 0,
                SpeedKmh = speedKmh,
                YawRate = ReadSingle(packet, 48),
                BodySlipRadians = (float)Math.Atan2(velocityX, velocityZ),
                FrontSlipRadians = frontSlipDegrees * ((float)Math.PI / 180.0f),
                VelocityForward = velocityZ
            };
            return IsFinite(frame.SpeedKmh) && IsFinite(frame.YawRate) && IsFinite(frame.BodySlipRadians);
        }

        private static float ReadSingle(byte[] source, int offset) => BitConverter.ToSingle(source, offset);
        private static int ReadInt32(byte[] source, int offset) => BitConverter.ToInt32(source, offset);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
