using System;
using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    [Flags]
    public enum Batteries
    {
        Unknown = 0x40000000,
        None = 0,

        [Description("Battery 1")]
        BatteryOne = 0x01,
        [Description("Battery 2")]
        BatteryTwo = 0x02,
        [Description("Battery 3")]
        BatteryThree = 0x04,
        [Description("Battery 4")]
        BatteryFour = 0x08,
        [Description("Battery 5")]
        BatteryFive = 0x10,
    }
}
