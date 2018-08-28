using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Djlastnight.Hid.UsageCollection
{
    public enum Digitizer : ushort
    {
        Digitizer = 0x01,
        Pen = 0x02,
        LightPen = 0x03,
        TouchScreen = 0x04,
        TouchPad = 0x05,
        WhiteBoard = 0x06,
        CoordinateMeasuringMachine = 0x07,
        Digitizer3D = 0x08,
        StereoPlotter = 0x09,
        ArticulatedArmm = 0x0A,
        Armature = 0x0B,
        MultiplePointDigitizer = 0x0C,
        FreeSpaceWand = 0x0D,
    }
}
