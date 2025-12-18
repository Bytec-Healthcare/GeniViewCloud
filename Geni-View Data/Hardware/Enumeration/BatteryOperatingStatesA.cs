using System;
using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    [Flags]
    public enum BatteryOperatingStatesA
    {
        Unknown = 0x40000000,
        OK = 0,

        [Description("Charging Allowed")]
        ChargingAllowed = 0x0001,
        [Description("Full Charge Detected")]
        FullChargeDetected = 0x0002,
        [Description("Charging Not Allowed")]
        ChargingNotAllowd = 0x0004,
        [Description("Charge Inhibit")]
        ChargeInhibit = 0x0008,
        [Description("Fully Discharged")]
        FullyDischarged = 0x0010,
        [Description("High Battery Voltage")]
        HighBatteryVoltage = 0x0020,
        [Description("Over Temperature In Discharge")]
        OverTemperatureInDischarge = 0x0040,
        [Description("Over Temperature In Charge")]
        OverTemperatureInCharge = 0x0080,
        [Description("Terminate Discharge")]
        TerminateDischarge = 0x0800,
        [Description("Over Temperature")]
        OverTemperature = 0x1000,
        [Description("Terminate Charge")]
        TerminateCharge = 0x4000,
        [Description("Over Charged")]
        OverCharged = 0x8000
    }
}
