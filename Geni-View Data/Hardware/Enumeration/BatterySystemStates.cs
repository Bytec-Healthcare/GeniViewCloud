using System;
using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    [Flags]
    public enum BatterySystemStates
    {
        Unknown = 0,

        [Description("Battery 1 is Present")]
        BatteryOnePresent = 0x0001,
        [Description("Battery 2 is Present")]
        BatteryTwoPresent = 0x0002,
        [Description("Battery 1 is Charging")]
        BatteryOneCharging = 0x0010,
        [Description("Battery 2 is Charging")]
        BatteryTwoCharging = 0x0020,
        [Description("Battery 1 is Powering the System")]
        BatteryOnePoweringSystem = 0x0100,
        [Description("Battery 2 is Powering the System")]
        BatteryTwoPoweringSystem = 0x0200,

        // From BatterySystemStatesCont
        [Description("External Power is Connected")]
        ExternalPowerApplied = 0x10000,
        [Description("External Power has a Problem")]
        ExternalPowerNotGood = 0x20000
    }
}
