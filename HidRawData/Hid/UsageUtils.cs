using Djlastnight.Hid.UsageCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Djlastnight.Hid
{
    public static class UsageUtils
    {
        public static bool IsStylus(this Device d) => (UsagePage)d.Capabilities.UsagePage == Hid.UsagePage.Digitiser && (
                    (UsageCollection.Digitizer)d.Capabilities.Usage == Digitizer.Digitizer ||
                    (UsageCollection.Digitizer)d.Capabilities.Usage == Digitizer.Pen);


        public static bool IsFinger(this Device d) => (UsagePage)d.Capabilities.UsagePage == Hid.UsagePage.Digitiser && (
                    (UsageCollection.Digitizer)d.Capabilities.Usage == Digitizer.TouchScreen ||
                    (UsageCollection.Digitizer)d.Capabilities.Usage == Digitizer.TouchPad);

        public static bool IsGamePad(this Device d) => (UsagePage)d.Capabilities.UsagePage == Hid.UsagePage.GenericDesktopControls &&
            (UsageCollection.GenericDesktop)d.Capabilities.Usage == GenericDesktop.GamePad;

    }
}
