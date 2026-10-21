using System;
using System.Runtime.InteropServices;

namespace JSE.Infrastructure
{
    [Flags]
    public enum GamepadButtons : ushort
    {
        DPadUp = 0x0001, DPadDown = 0x0002, DPadLeft = 0x0004, DPadRight = 0x0008,
        Start = 0x0010, Back = 0x0020, LeftThumb = 0x0040, RightThumb = 0x0080,
        LeftShoulder = 0x0100, RightShoulder = 0x0200, Guide = 0x0400,
        A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GamepadState
    {
        public GamepadButtons Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short LeftThumbX;
        public short LeftThumbY;
        public short RightThumbX;
        public short RightThumbY;

        public double NormalizedSteer => LeftThumbX < 0 ? LeftThumbX / 32768.0 : LeftThumbX / 32767.0;
        public bool Has(GamepadButtons button) => (Buttons & button) != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeXInputState
    {
        public uint PacketNumber;
        public GamepadState Gamepad;
    }

    public sealed class XInputReader
    {
        public int ControllerIndex { get; private set; } = -1;

        public bool FindController()
        {
            for (uint index = 0; index < 4; index++)
            {
                if (XInputGetState(index, out _) == 0)
                {
                    ControllerIndex = (int)index;
                    return true;
                }
            }
            ControllerIndex = -1;
            return false;
        }

        public bool TryRead(out GamepadState state)
        {
            state = default;
            if (ControllerIndex < 0 && !FindController()) return false;
            if (XInputGetState((uint)ControllerIndex, out NativeXInputState native) == 0)
            {
                state = native.Gamepad;
                return true;
            }
            ControllerIndex = -1;
            return false;
        }

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern uint XInputGetState(uint userIndex, out NativeXInputState state);
    }
}
