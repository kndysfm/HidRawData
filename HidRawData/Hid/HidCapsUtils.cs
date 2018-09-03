using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Djlastnight.Win32.Win32Hid;
using System.Diagnostics;

namespace Djlastnight.Hid
{
    public static class HidCapsUtils
    {
        public static string GetName(this HIDP_BUTTON_CAPS caps)
        {
            var pageType = Utils.UsageType((UsagePage) caps.UsagePage);
            return Enum.GetName(pageType, caps.NotRange.Usage) + $"({pageType.Name})";
        }

        public static string GetName(this HIDP_VALUE_CAPS caps)
        {
            var pageType = Utils.UsageType((UsagePage)caps.UsagePage);
            return Enum.GetName(pageType, caps.NotRange.Usage) + $"({pageType.Name})";
        }

        public static bool HasLink(this HIDP_BUTTON_CAPS caps) => caps.LinkUsage != 0;

        public static int GetLinkIndex(this HIDP_BUTTON_CAPS caps) => caps.LinkCollection;

        public static string GetLinkName(this HIDP_BUTTON_CAPS caps)
        {
            if (!caps.HasLink()) return "";
            var pageType = Utils.UsageType((UsagePage)caps.LinkUsagePage);
            return Enum.GetName(pageType, caps.LinkUsage) + $"({pageType.Name})[{caps.LinkCollection}]";
        }

        public static bool HasLink(this HIDP_VALUE_CAPS caps) => caps.LinkUsage != 0;

        public static int GetLinkIndex(this HIDP_VALUE_CAPS caps) => caps.LinkCollection;

        public static string GetLinkName(this HIDP_VALUE_CAPS caps)
        {
            if (!caps.HasLink()) return "";
            var pageType = Utils.UsageType((UsagePage)caps.LinkUsagePage);
            return Enum.GetName(pageType, caps.LinkUsage) + $"({pageType.Name})[{caps.LinkCollection}]";
        }

        private static int getUnitExp(uint value)
        {
            Debug.Assert(value < 16);
            switch (value)
            {
                default:
                case 0x0: return +0;
                case 0x1: return +1;
                case 0x2: return +2;
                case 0x3: return +3;
                case 0x4: return +4;
                case 0x5: return +5;
                case 0x6: return +6;
                case 0x7: return +7;
                case 0x8: return -8;
                case 0x9: return -7;
                case 0xa: return -6;
                case 0xb: return -5;
                case 0xc: return -4;
                case 0xd: return -3;
                case 0xe: return -2;
                case 0xf: return -1;
            }
        }

        private static string getUnitWithPower(string unit, int pow)
        {
            var abs = Math.Abs(pow);
            if (pow > 0)
                if (abs > 1) return " " + unit + "^" + abs; else return " " + unit;
            if (pow < 0)
                if (abs > 1) return "/" + unit + "^" + abs; else return "/" + unit;
            return "";
        }

        private static Tuple<double,string> getUnitNibble(uint system, uint nibble, uint value, bool friendly)
        {
            Debug.Assert(system <= 4);
            Debug.Assert(nibble <= 7);

            var e = getUnitExp(value);
            if (e == 0) return new Tuple<double, string>(1.0, "");

            var u = "";
            var adj = 1.0; // convert into friendly units system
            
            switch (system)
            {
                default:
                case 0: break;
                case 1: // SI linear
                    if (friendly)
                    {
                        switch (nibble)
                        {
                            case 1: adj = Math.Pow(10, e); u = "mm"; break;
                            case 2: u = "g"; break;
                            case 3: adj = Math.Pow(1000, e); u = "ms"; break;
                            case 4: u = "C"; break;
                            case 5: u = "A"; break;
                            case 6: u = "cd"; break;
                        }
                    }
                    else
                    switch(nibble)
                    {
                        case 1: u = "cm"; break;
                        case 2: u = "g"; break;
                        case 3: u = "s"; break;
                        case 4: u = "C"; break;
                        case 5: u = "A"; break;
                        case 6: u = "cd"; break;
                    }
                    break;
                case 2: // SI roration
                    if (nibble == 1)
                    {
                        if (friendly)
                        {
                            adj = Math.Pow(180.0/Math.PI, e);
                            u = "degrees";
                        }
                        else u = "radian"; 
                    }
                    break;
                case 3: // English linear
                    if (friendly)
                    {
                        switch (nibble)
                        {
                            case 1: adj = Math.Pow(25.4, e); u = "mm"; break;
                            case 2: adj = Math.Pow(14.5939, e); u = "g"; break;
                            case 4: u = "F"; break; //TODO T[F] = 1.8*T[C] + 32 
                        }
                    }
                    else
                    switch (nibble)
                    {
                        case 1: u = "in"; break;
                        case 2: u = "slug"; break;
                        case 4: u = "F"; break;
                    }
                    break;
                case 4: // English rotation
                    if (nibble == 1) u = "degrees"; 
                    break;
            }

            return new Tuple<double,string>(adj, string.IsNullOrEmpty(u)? u: getUnitWithPower(u, e));
        }

        private static Tuple<float, string> getUnitTuple(uint unit, uint exp, bool friendly)
        {
            var s = "";
            var u = 1.0;
            var e = getUnitExp(exp);
            var system = (unit >> 0) & 0xf;
            var order = new uint[] { 2, 1, 3, 4, 5, 6, 7 };
            foreach (var i in order)
            {
                var n = getUnitNibble(system, i, (unit >> (int)(i * 4)) & 0xf, friendly);
                u *= n.Item1;
                s += n.Item2;
            }
            if (!string.IsNullOrEmpty(s))
            {
                if (s[0] == '/') s = "1" + s;
                else if (s[0] == ' ') s = s.Substring(1); 
            }
            return new Tuple<float, string>((float)(u * Math.Pow(10, e)), s);
        }

        public static bool HasUnit(this HIDP_VALUE_CAPS caps) => caps.Units != 0;

        public static string GetUnit(this HIDP_VALUE_CAPS caps, bool friendly = true)
        {
            return getUnitTuple(caps.Units, caps.UnitsExp, friendly).Item2;
        }

        public static float ConvertUnit(this HIDP_VALUE_CAPS caps, uint value_logical, bool friendly = true)
        {
            var t = getUnitTuple(caps.Units, caps.UnitsExp, friendly);
            return t.Item1 * ((float)caps.PhysicalMin + (caps.PhysicalMax - caps.PhysicalMin) * (value_logical - caps.LogicalMin) / (caps.LogicalMax - caps.LogicalMin));
        }
    }
}
