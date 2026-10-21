using System;
using JSE.Core;
using Xunit;

namespace JSE.Tests
{
    public sealed class AssistTests
    {
        [Fact]
        public void ParsesFh5HorizonDashPacket()
        {
            var packet = new byte[ForzaHorizon5Packet.HorizonDashLength];
            WriteInt(packet, 0, 1);
            WriteFloat(packet, 32, 5f);
            WriteFloat(packet, 40, 20f);
            WriteFloat(packet, 48, 0.25f);
            WriteFloat(packet, 164, 4f);
            WriteFloat(packet, 168, 6f);
            WriteFloat(packet, 256, 25f);

            Assert.True(ForzaHorizon5Packet.TryParse(packet, out TelemetryFrame frame));
            Assert.True(frame.IsRaceOn);
            Assert.Equal(90f, frame.SpeedKmh, 3);
            Assert.Equal(0.25f, frame.YawRate, 3);
            Assert.Equal((float)Math.Atan2(5, 20), frame.BodySlipRadians, 3);
            Assert.Equal(5f * (float)Math.PI / 180f, frame.FrontSlipRadians, 3);
        }

        [Fact]
        public void RejectsUnknownPacketSize()
            => Assert.False(ForzaHorizon5Packet.TryParse(new byte[100], out _));

        [Fact]
        public void PassesInputThroughBelowMinimumSpeed()
        {
            var settings = FastSettings();
            var frame = ActiveFrame();
            frame.SpeedKmh = 10;
            var assist = new DriftAssist();

            double output = assist.Update(0.4, frame, settings, 0.1, true);

            Assert.Equal(0.4, output, 6);
            Assert.Equal(0, assist.LastBlend);
        }

        [Fact]
        public void CounterSteersDuringForwardDrift()
        {
            var settings = FastSettings();
            var assist = new DriftAssist();

            double output = assist.Update(0.2, ActiveFrame(), settings, 0.1, true);

            Assert.True(assist.LastBlend > 0);
            Assert.True(output < 0.2);
        }

        [Fact]
        public void SettingsStayInsideRequestedLimits()
        {
            var settings = new AssistSettings
            {
                GyroStrength = -1, CounterGain = 5, SlipDeadband = 0,
                InputSmoothing = 2, OutputSmoothing = -2, AssistFadeRange = 1,
                MaxSteerRate = 0, MinSpeedKmh = 100
            };
            settings.Clamp();
            Assert.Equal(0, settings.GyroStrength);
            Assert.Equal(2, settings.CounterGain);
            Assert.Equal(0.010, settings.SlipDeadband, 3);
            Assert.Equal(0.950, settings.InputSmoothing, 3);
            Assert.Equal(0, settings.OutputSmoothing);
            Assert.Equal(0.200, settings.AssistFadeRange, 3);
            Assert.Equal(0.500, settings.MaxSteerRate, 3);
            Assert.Equal(80, settings.MinSpeedKmh);
        }

        private static AssistSettings FastSettings() => new AssistSettings
        {
            GyroStrength = 1.5, CounterGain = 1.5, SlipDeadband = 0.010,
            AssistFadeRange = 0.020, InputSmoothing = 0, OutputSmoothing = 0,
            MaxSteerRate = 5, MinSpeedKmh = 20
        };

        private static TelemetryFrame ActiveFrame() => new TelemetryFrame
        {
            IsRaceOn = true, SpeedKmh = 80, VelocityForward = 20,
            YawRate = 0.2f, BodySlipRadians = 0.2f
        };

        private static void WriteFloat(byte[] packet, int offset, float value)
            => Array.Copy(BitConverter.GetBytes(value), 0, packet, offset, 4);
        private static void WriteInt(byte[] packet, int offset, int value)
            => Array.Copy(BitConverter.GetBytes(value), 0, packet, offset, 4);
    }
}
