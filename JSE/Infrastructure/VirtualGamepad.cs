using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace JSE.Infrastructure
{
    public sealed class VirtualGamepad : IDisposable
    {
        private ViGEmClient _client;
        private IXbox360Controller _controller;

        public bool IsConnected => _controller != null;

        public void Connect()
        {
            if (IsConnected) return;
            _client = new ViGEmClient();
            _controller = _client.CreateXbox360Controller();
            _controller.AutoSubmitReport = false;
            _controller.Connect();
        }

        public void Submit(GamepadState source, double correctedSteer)
        {
            if (_controller == null) return;
            _controller.SetAxisValue(Xbox360Axis.LeftThumbX, ToAxis(correctedSteer));
            _controller.SetAxisValue(Xbox360Axis.LeftThumbY, source.LeftThumbY);
            _controller.SetAxisValue(Xbox360Axis.RightThumbX, source.RightThumbX);
            _controller.SetAxisValue(Xbox360Axis.RightThumbY, source.RightThumbY);
            _controller.SetSliderValue(Xbox360Slider.LeftTrigger, source.LeftTrigger);
            _controller.SetSliderValue(Xbox360Slider.RightTrigger, source.RightTrigger);

            Set(Xbox360Button.Up, source.Has(GamepadButtons.DPadUp));
            Set(Xbox360Button.Down, source.Has(GamepadButtons.DPadDown));
            Set(Xbox360Button.Left, source.Has(GamepadButtons.DPadLeft));
            Set(Xbox360Button.Right, source.Has(GamepadButtons.DPadRight));
            Set(Xbox360Button.Start, source.Has(GamepadButtons.Start));
            Set(Xbox360Button.Back, source.Has(GamepadButtons.Back));
            Set(Xbox360Button.LeftThumb, source.Has(GamepadButtons.LeftThumb));
            Set(Xbox360Button.RightThumb, source.Has(GamepadButtons.RightThumb));
            Set(Xbox360Button.LeftShoulder, source.Has(GamepadButtons.LeftShoulder));
            Set(Xbox360Button.RightShoulder, source.Has(GamepadButtons.RightShoulder));
            Set(Xbox360Button.Guide, source.Has(GamepadButtons.Guide));
            Set(Xbox360Button.A, source.Has(GamepadButtons.A));
            Set(Xbox360Button.B, source.Has(GamepadButtons.B));
            Set(Xbox360Button.X, source.Has(GamepadButtons.X));
            Set(Xbox360Button.Y, source.Has(GamepadButtons.Y));
            _controller.SubmitReport();
        }

        private void Set(Xbox360Button button, bool pressed) => _controller.SetButtonState(button, pressed);
        private static short ToAxis(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            return value < 0 ? (short)(value * 32768.0) : (short)(value * 32767.0);
        }

        public void Dispose()
        {
            if (_controller != null)
            {
                try { _controller.Disconnect(); } catch { }
                _controller = null;
            }
            _client?.Dispose();
            _client = null;
        }
    }
}
